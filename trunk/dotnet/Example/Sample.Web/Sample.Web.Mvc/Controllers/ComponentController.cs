namespace Sample.Web.Mvc.Controllers
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Collections.Generic;
    using DD4T.Mvc.Controllers;
    using DD4T.ContentModel.Factories;
    using DD4T.ContentModel.Exceptions;
    using DD4T.ContentModel;

    public class ComponentController : TridionControllerBase
    {
        [ImportingConstructor]
        public ComponentController()
        {
        }

        private IModelFactory ModelFactory { get; set; }

        protected override ViewResult GetView(IComponentPresentation componentPresentation)
        {
            // TODO: define field names in Web.config
            if (!componentPresentation.ComponentTemplate.MetadataFields.ContainsKey("view"))
            {
                throw new ConfigurationException("no view configured for component template " + componentPresentation.ComponentTemplate.Id);
            }

            string viewName = componentPresentation.ComponentTemplate.MetadataFields["view"].Value;
            
            object model;
            // todo: implement ModelFactory
            //if (ModelFactory.TryCreateModel(viewName, componentPresentation, out model))
            //    return View(viewName, model);
            //else
            //    return null;

            return View(viewName, componentPresentation.Component);

        }

        private object GetModel()
        {
            return this.RouteData.Values["model"];
        }

        public ActionResult ArticleSummary()
        {
            ViewBag.Column = this.RouteData.Values["column"];
            ViewBag.ShowFeatureItemImage = this.RouteData.Values["showFeature"];
            return View(GetModel());
        }

        public ActionResult Query()
        {
            List<IComponent> components = new List<IComponent>();
            IComponentPresentation cp = this.GetComponentPresentation();

            ExtendedQueryParameters eqp = new ExtendedQueryParameters();
            if (cp.Component.Fields.ContainsKey("Schema"))
            {
                string schemaName = cp.Component.Fields["Schema"].Value;
                eqp.QuerySchemas = new string[] { schemaName };
            }
            // todo: add 'last XXX days' field
            eqp.LastPublishedDate = DateTime.Now.AddMonths(-3); // search for everything in the last 3 months

            // run the query
            ViewBag.Results = ComponentFactory.FindComponents(eqp);
            return View(cp.Component);
        }

        /* examples of actions for special models
        #region ContentRating
        [HttpGet]
        public PartialViewResult ContentRating()
        {
            return PartialView(GetModel());
        }

        [HttpPost]
        public PartialViewResult ContentRating(ContentRating contentRating)
        {
            if (this.ModelState.IsValid)
            {
                using (var db = new ContentRatingContainer())
                {
                    User user = null;
                    Content content = null;
                    // Does the content with the right publicationdate exist in the database ?
                    if (!db.ContentSet.Any(c => c.TcmId == contentRating.TcmId && c.PublicationDate == contentRating.PublicationDate))
                    {
                        //There is no content with the right publicationdate.
                        content = db.ContentSet.Create();
                        content.TcmId = contentRating.TcmId;
                        content.PublicationDate = contentRating.PublicationDate;
                        db.ContentSet.Add(content);
                    }
                    else
                    {
                        foreach (var result in db.ContentSet.Where(c => c.TcmId == contentRating.TcmId && c.PublicationDate == contentRating.PublicationDate))
                        {
                            content = result;
                            break;
                        }
                    }

                    if (!db.UserSet.Any(u => u.UserCode == contentRating.UserId))
                    {
                        user = db.UserSet.Create();
                        user.UserCode = contentRating.UserId;
                        user.UserName = contentRating.UserName;
                        user.BankCode = contentRating.BankCode;
                        db.UserSet.Add(user);
                    }
                    else
                    {
                        foreach (var result in db.UserSet.Where(u => u.UserCode == contentRating.UserId))
                        {
                            user = result;
                        }
                    }

                    Rating rating = db.RatingSet.Create();
                    rating.User = user;
                    rating.Content = content;
                    rating.Value = contentRating.Rating;
                    rating.Comments = contentRating.Comments;
                    db.RatingSet.Add(rating);

                    //Save
                    db.SaveChanges();

                    CalculateAverageRating(contentRating, db);
                }
                return PartialView("_ThanksForRating", contentRating);
            }
            else
            {
                using (var db = new ContentRatingContainer())
                {
                    CalculateAverageRating(contentRating, db);
                }
                return PartialView("ContentRating", contentRating);
            }
        }

        private static void CalculateAverageRating(ContentRating contentRating, ContentRatingContainer db)
        {
            var ratings = db.RatingSet.Where(r => r.Content.TcmId == contentRating.TcmId && r.Content.PublicationDate == contentRating.PublicationDate);
            int count = ratings.Count();
            if (count > 0)
            {
                int totalRating = ratings.Sum(r => r.Value);
                contentRating.RatingAverage = ((double)totalRating / (double)count);
                contentRating.RatingCount = count;
            }
        }
        #endregion ContentRating

        #region Poll
        [HttpGet]
        public PartialViewResult Poll()
        {
            return PartialView(GetModel());
        }

        [HttpPost]
        public PartialViewResult Poll(Poll poll)
        {
            if (this.ModelState.IsValid)
            {
                using (var db = new PollModelContainer())
                {
                    // Does the poll exist in the database?
                    if (!db.PollSet.Any(p => p.PollId == poll.PollId))
                    {
                        // There is no poll existing in the database yet, and therefore no answers yet
                        PollAnswer answer = db.PollAnswerSet.Create();
                        answer.Description = poll.ChosenAnswer;
                        answer.Count = 1;
                        poll.Answers.Add(answer);

                        db.PollSet.Add(poll);
                    }
                    else
                    {
                        bool answerExists = false;
                        // The poll allready exist in the database?
                        foreach (var answer in db.PollAnswerSet.Where(a => a.Poll.PollId == poll.PollId && a.Description == poll.ChosenAnswer))
                        {
                            answerExists = true;
                            // Increment count corresponding the chosen answer
                            answer.Count++;
                        }
                        if (!answerExists)
                        {
                            foreach (var p in db.PollSet.Where(p => p.PollId == poll.PollId))
                            {
                                PollAnswer answer = db.PollAnswerSet.Create();
                                answer.Description = poll.ChosenAnswer;
                                answer.Count = 1;
                                p.Answers.Add(answer);
                            }
                        }
                    }
                    db.SaveChanges();

                    //set cookie
                    poll.HasVoted = true;
                    string cookieName = PollCookie.Name.ToSetting();
                    HttpCookie cookie = new HttpCookie(cookieName);
                    cookie[PollCookie.FieldId.ToSetting()] = poll.PollId.ToString();
                    cookie[PollCookie.FieldVote.ToSetting()] = poll.HasVoted.ToString();
                    cookie.Expires = DateTime.Now.AddMonths(1);
                    Response.Cookies.Add(cookie);

                    // Get the answer with the new count for diplay in averages
                    foreach (var answer in db.PollAnswerSet.Where(a => a.Poll.PollId == poll.PollId))
                    {
                        foreach (var result in poll.Answers.Where(a => a.Description == answer.Description))
                        {
                            result.Count = answer.Count;
                        }
                    }
                }
                return PartialView("_AveragePoll", poll);
            }

            return PartialView("_Poll", poll);
        }

        #endregion Poll
        public ActionResult XmlAction()
        {
            string requestUrl = ControllerContext.HttpContext.Request.Path;
            IPage page;
            if (!PageFactory.TryFindPage(requestUrl, out page))
            {
                //Iets mis
            }

            var model = ModelFactory.createHomepageRssFeed(page) as System.ServiceModel.Syndication.SyndicationFeed;          
            
            

            //test return rss-feed
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<rss version=\"2.0\">");
              sb.Append("<channel>");
                sb.Append("<title>nu.nl - Algemeen</title>");
                sb.Append("<copyright>Copyright (c) 2011, nu.nl</copyright>");
                      sb.Append("<link>http://www.nu.nl/algemeen/</link>");
                    sb.Append("<language>nl</language>");
                sb.Append("<description>nu.nl Rich Site Summary</description>");

                sb.Append("<pubDate>Fri, 20 May 2011 11:04:17 +0200</pubDate>");
                          sb.Append("<item>");
                    sb.Append("<title>Akkoord over geluidsnorm bij optredens</title>");
                              sb.Append("<link>http://www.nu.nl/muziek/2519754/akkoord-geluidsnorm-bij-optredens.html</link>");
                      sb.Append("<guid>http://www.nu.nl/muziek/2519754/index.html</guid>");
        
                    sb.Append("<description>LEIDEN - De brancheverenigingen van evenementen, festivals en poppodia hebben in samenspraak met de Nationale Hoorstichting een akkoord bereikt over een geluidsnorm bij concerten. Het aantal decibels mag tijdens een meting van een kwartier niet hoger zijn dan 103.</description>");

                            sb.Append("<pubDate>Fri, 20 May 2011 11:04:02 +0200</pubDate>");
                    sb.Append("<category>Algemeen</category>");
                            sb.Append("<enclosure url=\"http://media.nu.nl/m/m1fzhs9a52rl.jpg\" type=\"image/jpeg\" />      </item></channel>");

            using (System.IO.StringWriter output = new System.IO.StringWriter())
            {
                System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(output);
                new System.ServiceModel.Syndication.Rss20FeedFormatter(model).WriteTo(writer);

                return new ContentResult
                {
                    ContentType = "application/rss+xml",
                    ContentEncoding = System.Text.Encoding.UTF8,
                    Content = output.ToString()
                };
            } 

            
           
            
        }

         */

    }
}
