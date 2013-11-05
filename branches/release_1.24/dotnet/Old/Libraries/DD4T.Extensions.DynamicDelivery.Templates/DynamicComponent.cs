using System;
using System.IO;
using System.Text.RegularExpressions;
using Tridion.ContentManager;
using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.Extensions.DynamicDelivery.ContentModel;
using System.Xml.Serialization;
using System.Text;
using System.Xml;

namespace DD4T.Extensions.DynamicDelivery.Templates
{

	[TcmTemplateTitle("Generate dynamic component")]    
	public partial class DynamicComponent : ComponentTemplateBase {

		protected override void TransformComponent(Dynamic.Component component) {
			// do nothing, this is the basic operation
		}
	}

	public class XmlTextWriterFormattedNoDeclaration : XmlTextWriter {
		public XmlTextWriterFormattedNoDeclaration(System.IO.TextWriter w) : base(w) { 
			Formatting = System.Xml.Formatting.Indented; 
		}
		public XmlTextWriterFormattedNoDeclaration(System.IO.MemoryStream ms, Encoding enc)
			: base(ms, enc) {
			Formatting = System.Xml.Formatting.Indented;
		}
		public override void WriteStartDocument() { } // suppress
	}
}
