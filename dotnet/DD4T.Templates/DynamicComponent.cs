using System;
using System.IO;
using System.Text.RegularExpressions;
using Tridion.ContentManager;
using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;
using System.Xml.Serialization;
using System.Text;
using System.Xml;

namespace DD4T.Templates {

	[TcmTemplateTitle("Generate dynamic component")]    
	public partial class DynamicComponent : BaseComponentTemplate {

		protected override void TransformComponent(Dynamic.Component component) {
			// do nothing, this is the basic operation
		}
	}

}
