using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TCM = Tridion.ContentManager.ContentManagement;

namespace DD4T.Templates.Base.Builder
{
	public class SchemaBuilder {
        public static Dynamic.Schema BuildSchema(TCM.Schema tcmSchema, BuildManager manager)
        {
			if (tcmSchema == null) {
				return null;
			}
			Dynamic.Schema s = new Dynamic.Schema();
			s.Title = tcmSchema.Title;
			s.Id = tcmSchema.Id.ToString();
            s.Folder = manager.BuildOrganizationalItem((TCM.Folder)tcmSchema.OrganizationalItem);
            s.Publication = manager.BuildPublication(tcmSchema.ContextRepository);

            if (!String.IsNullOrEmpty(tcmSchema.RootElementName))
            {
                s.RootElementName = tcmSchema.RootElementName;
            }
            // note that non-webschemas and multimedia schema's lack a root element. In order not
            // to break deserialization, setting it to undefined will do
            else
            {
                s.RootElementName = "undefined";
            }

			return s;
		}
	}
}
