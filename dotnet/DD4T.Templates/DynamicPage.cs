using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Base;

namespace DD4T.Templates {

	[TcmTemplateTitle("Generate dynamic page")]
    [TcmTemplateParameterSchema("resource:DD4T.Templates.Resources.Schemas.Dynamic Delivery Parameters.xsd")]
    [TcmDefaultTemplate]
	public partial class DynamicPage : BasePageTemplate {
        

		protected override void TransformPage(Dynamic.Page page) {
			// do nothing, this is the basic operation
		}
	}

}
