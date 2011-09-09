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
using System.Collections.Generic;
using System.Text;
using Dynamic = Tridion.Extensions.DynamicDelivery.ContentModel;
using TCM = Tridion.ContentManager.ContentManagement;

namespace Tridion.Extensions.DynamicDelivery.Templates.Builder {
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
                s.RootElement = tcmSchema.RootElementName;
            }
            // note that non-webschemas and multimedia schema's lack a root element. In order not
            // to break deserialization, setting it to undefined will do
            else
            {
                s.RootElement = "undefined";
            }

			return s;
		}
	}
}
