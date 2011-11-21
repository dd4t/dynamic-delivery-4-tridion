using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCM = Tridion.ContentManager.ContentManagement;
using Dynamic = DD4T.ContentModel;
using TComm = Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;

namespace DD4T.Templates.Base.Builder
{
    /// <summary>
    /// Class serves as reference point to all builders, allowing subclasses of the BuildManager to override
    /// specific points of implementation. In a way, this class provides a poor man's dependency injection.
    /// </summary>
    public class BuildManager
    {
        public BuildManager()
        {

		}

        public virtual Dynamic.Page BuildPage(TComm.Page tcmPage, Engine engine)
        {
            return PageBuilder.BuildPage(tcmPage, engine, this);
        }

        public virtual Dynamic.Page BuildPage(TComm.Page tcmPage, Engine engine, int linkLevels, bool resolveWidthAndHeight)
        {
            return PageBuilder.BuildPage(tcmPage, engine, this, linkLevels, resolveWidthAndHeight);
        }

        public virtual List<Dynamic.Category> BuildCategories(TComm.Page page)
        {
            return CategoriesBuilder.BuildCategories(page,this);
        }

        public virtual List<Dynamic.Category> BuildCategories(TCM.Component component)
        {
            return CategoriesBuilder.BuildCategories(component, this);
        }

        public virtual Dynamic.Component BuildComponent(TCM.Component tcmComponent)
        {
            return ComponentBuilder.BuildComponent(tcmComponent, this);
		}

        public virtual Dynamic.Component BuildComponent(TCM.Component tcmComponent, int linkLevels, bool resolveWidthAndHeight)
        {
            return ComponentBuilder.BuildComponent(tcmComponent, linkLevels, resolveWidthAndHeight, this);
        }

        public virtual Dynamic.ComponentPresentation BuildComponentPresentation(TComm.ComponentPresentation tcmComponentPresentation, Engine engine, int linkLevels, bool resolveWidthAndHeight)
        {
            return ComponentPresentationBuilder.BuildComponentPresentation(tcmComponentPresentation, engine, linkLevels, resolveWidthAndHeight, this);
        }

        public virtual Dynamic.ComponentTemplate BuildComponentTemplate(TComm.ComponentTemplate tcmComponentTemplate)
        {
            return ComponentTemplateBuilder.BuildComponentTemplate(tcmComponentTemplate, this);
        }

        public virtual Dynamic.Field BuildField(TCM.Fields.ItemField tcmItemField, int linkLevels, bool resolveWidthAndHeight)
        {
            return FieldBuilder.BuildField(tcmItemField, linkLevels, resolveWidthAndHeight, this);
        }

        public virtual Dynamic.FieldSet BuildFields(TCM.Fields.ItemFields tcmItemFields, int linkLevels, bool resolveWidthAndHeight)
        {
            return FieldsBuilder.BuildFields(tcmItemFields, linkLevels, resolveWidthAndHeight, this);
        }

        public Dynamic.Keyword BuildKeyword(TCM.Keyword keyword)
        {
            return KeywordBuilder.BuildKeyword(keyword);
        }

        public virtual Dynamic.OrganizationalItem BuildOrganizationalItem(TComm.StructureGroup tcmStructureGroup)
        {
            return OrganizationalItemBuilder.BuildOrganizationalItem(tcmStructureGroup);
		}

        public virtual Dynamic.OrganizationalItem BuildOrganizationalItem(TCM.Folder tcmFolder)
        {
            return OrganizationalItemBuilder.BuildOrganizationalItem(tcmFolder);
        }

        public virtual Dynamic.PageTemplate BuildPageTemplate(TComm.PageTemplate tcmPageTemplate)
        {
            return PageTemplateBuilder.BuildPageTemplate(tcmPageTemplate, this);
        }

        public virtual Dynamic.Publication BuildPublication(TCM.Repository tcmPublication)
        {
            return PublicationBuilder.BuildPublication(tcmPublication);
        }

        public virtual Dynamic.Schema BuildSchema(TCM.Schema tcmSchema)
        {
            return SchemaBuilder.BuildSchema(tcmSchema, this);
        }
        public virtual void AddXpathToFields(Dynamic.FieldSet fieldSet, string baseXpath)
        {
            FieldsBuilder.AddXpathToFields(fieldSet, baseXpath);
        }
    }
}
