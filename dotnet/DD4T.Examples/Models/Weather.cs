using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using DD4T.ContentModel;

namespace DD4T.Examples.Models
{
    public class Weather
    {
        public string Temperature { get; set; }
        public string Conditions { get; set; }
        public string Location { get; set; }

        public Weather(IComponentPresentation cp)
        {
            //string feedurl = "http://weather.yahooapis.com/forecastrss?w=615702&u=c";
            Location = cp.Component.Fields["Location"].Value;
            using (XmlReader reader = XmlReader.Create(cp.Component.Fields["Url"].Value))
            {
                XmlDocument feed = new XmlDocument();
                feed.Load(reader);

                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(feed.NameTable);
                namespaceManager.AddNamespace("yweather", "http://xml.weather.yahoo.com/ns/rss/1.0");
                XmlElement condition = (XmlElement)feed.SelectSingleNode("//yweather:condition", namespaceManager);
                Temperature = condition.GetAttribute("temp");
                Conditions = condition.GetAttribute("text");
            }
        }
    }
}