///   
/// Copyright 2011 Capgemini & SDL
///
///   Licensed under the Apache License, Version 2.0 (the "License");
///   you may not use this file except in compliance with the License.
///   You may obtain a copy of the License at
///
///       http://www.apache.org/licenses/LICENSE-2.0
///
///   Unless required by applicable law or agreed to in writing, software
///   distributed under the License is distributed on an "AS IS" BASIS,
///   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
///   See the License for the specific language governing permissions and
///   limitations under the License.

using System;
using System.IO;
using System.Text.RegularExpressions;
using Tridion.ContentManager;
using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = Tridion.Extensions.DynamicDelivery.ContentModel;
using System.Xml.Serialization;
using System.Text;
using System.Xml;

namespace Tridion.Extensions.DynamicDelivery.Templates {

	[TcmTemplateTitle("Generate dynamic component")]
	public partial class DynamicComponent : BaseComponentTemplate {

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
