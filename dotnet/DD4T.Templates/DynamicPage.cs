using System;
using System.IO;
using System.Text.RegularExpressions;
using Tridion.ContentManager;
using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Builder;
using System.Xml.Serialization;
using System.Text;
using System.Xml;

namespace DD4T.Templates {

	[TcmTemplateTitle("Generate dynamic page")]
    [TcmDefaultTemplate]
	public partial class DynamicPage : BasePageTemplate {
        

		protected override void TransformPage(Dynamic.Page page) {
			// do nothing, this is the basic operation
		}
	}

}
