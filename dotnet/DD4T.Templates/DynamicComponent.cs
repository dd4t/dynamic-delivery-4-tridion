using TCM = Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Base;

namespace DD4T.Templates {

	[TcmTemplateTitle("Generate dynamic component")]
    [TcmTemplateParameterSchema("resource:DD4T.Templates.Resources.Schemas.Dynamic Delivery Parameters.xsd")]
    public partial class DynamicComponent : BaseComponentTemplate
    {

		protected override void TransformComponent(Dynamic.Component component) {
			// do nothing, this is the basic operation
		}
	}

}
