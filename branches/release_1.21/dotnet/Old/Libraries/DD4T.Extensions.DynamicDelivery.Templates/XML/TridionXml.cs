using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DD4T.Extensions.DynamicDelivery.Templates
{
   public class TridionXml : XmlDocument
   {
      private XmlNamespaceManager mNamespaceManager;

      /// <summary>
      /// Initializes a new instance of the <see cref="TridionXml"/> class.
      /// </summary>
      public TridionXml()
      {
         mNamespaceManager = new XmlNamespaceManager(new NameTable());
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="TridionXml"/> class using source as the starting point.
      /// </summary>
      /// <param name="source">The XmlDocument to start from.</param>
      public TridionXml(XmlDocument source)
      {
         mNamespaceManager = new XmlNamespaceManager(new NameTable());
         this.LoadXml(source.OuterXml);
      }

      public TridionXml(XmlElement source)
      {
         mNamespaceManager = new XmlNamespaceManager(new NameTable());
         this.LoadXml(source.OuterXml);
      }

      ///// <summary>
      ///// Loads a Tridion Item from its URI into a TridionXml object.
      ///// </summary>
      ///// <param name="tcmUri">The Tridion URI</param>
      //public TridionXml(string tcmUri)
      //{
      //    Tridion.ContentManager.Interop.msxml4.DOMDocument domDoc = new Tridion.ContentManager.Interop.msxml4.DOMDocument();
      //    domDoc.load(tcmUri);
      //    this.LoadXml(domDoc.xml);
      //}

      /// <summary>
      /// Gets or sets the namespace manager.
      /// </summary>
      /// <value>The namespace manager.</value>
      public XmlNamespaceManager NamespaceManager
      {
         get { return mNamespaceManager; }
         set { mNamespaceManager = value; }
      }

      /// <summary>
      /// Loads the XML document from the specified string.
      /// </summary>
      /// <param name="xml">String containing the XML document to load.</param>
      /// <exception cref="T:System.Xml.XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
      public override void LoadXml(string xml)
      {
         base.LoadXml(xml);
         mNamespaceManager.AddNamespace("tcm", "http://www.tridion.com/ContentManager/5.0");
         mNamespaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
         mNamespaceManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
      }

      /// <summary>
      /// Gets the root element.
      /// </summary>
      /// <returns></returns>
      public XmlNode GetRootElement()
      {
         string xPathQuery = @"/tcm:*/tcm:Data/tcm:Content/Content:*";
         XmlNode xmlNode = null;
         xmlNode = base.SelectSingleNode(xPathQuery, NamespaceManager);
         if (xmlNode == null)
         {
         }
         return xmlNode;
      }

      /// <summary>
      /// Gets the name of the root element.
      /// </summary>
      /// <returns></returns>
      public string GetRootElementName()
      {
         XmlNode xmlNode = GetRootElement();
         return xmlNode.Name;
      }

      /// <summary>
      /// Gets the node value.
      /// </summary>
      /// <param name="node">The node.</param>
      /// <returns></returns>
      public string GetNodeValue(XmlNode node)
      {
         string output = null;
         string xPathQuery = "text()";
         XmlNode xmlNode = null;
         xmlNode = node.SelectSingleNode(xPathQuery, NamespaceManager);
         if (xmlNode != null)
         {
            output = xmlNode.Value;
         }
         return output;
      }

      /// <summary>
      /// Gets the node value.
      /// </summary>
      /// <param name="node">The node.</param>
      /// <param name="fieldName">Name of the field.</param>
      /// <returns></returns>
      public string GetNodeValue(XmlNode node, string fieldName)
      {
         string output = null;
         string xPathQuery = @"Content:" + fieldName + "/text()";
         XmlNode xmlNode = null;
         xmlNode = node.SelectSingleNode(xPathQuery, NamespaceManager);
         if (xmlNode != null)
         {
            output = xmlNode.Value;
         }
         return output;
      }

      /// <summary>
      /// Gets the content node value.
      /// </summary>
      /// <param name="rootName">Name of the content root.</param>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="deep">if set to <c>true</c> [deep].</param>
      /// <returns></returns>
      public string GetContentNodeValue(string rootName, string fieldName, bool deep)
      {
         string output = null;
         string deepPath = string.Empty;
         if (deep)
         {
            deepPath = "/";
         }
         string xPathQuery = @"/tcm:*/tcm:Data/tcm:Content/Content:" + rootName + deepPath + "/Content:" + fieldName + "/text()";
         XmlNode xmlNode = null;
         xmlNode = base.SelectSingleNode(xPathQuery, NamespaceManager);
         if (xmlNode != null)
         {
            output = xmlNode.Value;
         }
         return output;
      }

      /// <summary>
      /// Gets the metadata node value.
      /// </summary>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="deep">if set to <c>true</c> [deep].</param>
      /// <returns></returns>
      public string GetPublicationMetadataNodeValue(string fieldName, bool deep)
      {
         string output = null;
         string deepPath = string.Empty;
         if (deep)
         {
            deepPath = "/";
         }
         string xPathQuery = @"/tcm:*/tcm:Data/tcm:Metadata/Metadata:Metadata" + deepPath + "/Metadata:" + fieldName + "/text()";
         XmlNode xmlNode = null;
         xmlNode = base.SelectSingleNode(xPathQuery, NamespaceManager);
         if (xmlNode != null)
         {
            output = xmlNode.Value;
         }
         return output;
      }

      /// <summary>
      /// Gets the metadata node value.
      /// </summary>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="deep">if set to <c>true</c> [deep].</param>
      /// <returns></returns>
      public string GetPageMetadataNodeValue(string fieldName, bool deep)
      {
         string output = null;
         string deepPath = string.Empty;
         if (deep)
         {
            deepPath = "/";
         }
         string xPathQuery = @"/tcm:*/tcm:Data/tcm:Metadata/Metadata:Metadata" + deepPath + "/Metadata:" + fieldName + "/text()";
         XmlNode xmlNode = null;
         xmlNode = base.SelectSingleNode(xPathQuery, NamespaceManager);
         if (xmlNode != null)
         {
            output = xmlNode.Value;
         }
         return output;
      }

      /// <summary>
      /// Gets the metadata node value.
      /// </summary>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="deep">if set to <c>true</c> [deep].</param>
      /// <returns></returns>
      public string GetMetadataNodeValue(string fieldName, bool deep)
      {
         string output = null;
         string deepPath = string.Empty;
         if (deep)
         {
            deepPath = "/";
         }
         string xPathQuery = @"/tcm:*/tcm:Data/tcm:Metadata/Metadata:Metadata" + deepPath + "/Metadata:" + fieldName + "/text()";
         XmlNode xmlNode = null;
         xmlNode = base.SelectSingleNode(xPathQuery, NamespaceManager);
         if (xmlNode != null)
         {
            output = xmlNode.Value;
         }
         return output;
      }

      /// <summary>
      /// Gets the node.
      /// </summary>
      /// <param name="node">The node.</param>
      /// <param name="fieldName">Name of the field.</param>
      /// <returns></returns>
      public XmlNode GetNode(XmlNode node, string fieldName)
      {
         string xPathQuery = @"Content:" + fieldName;
         XmlNode xmlNode = null;
         xmlNode = node.SelectSingleNode(xPathQuery, NamespaceManager);

         return xmlNode;
      }

      /// <summary>
      /// Gets the node.
      /// </summary>
      /// <param name="rootName">Name of the root.</param>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="deep">if set to <c>true</c> [deep].</param>
      /// <returns></returns>
      public XmlNode GetContentNode(string rootName, string fieldName, bool deep)
      {
         string deepPath = string.Empty;
         if (deep)
         {
            deepPath = "/";
         }
         string xPathQuery = @"/tcm:*/tcm:Data/tcm:Content/Content:" + rootName + deepPath + "/Content:" + fieldName;
         XmlNode xmlNode = null;
         xmlNode = base.SelectSingleNode(xPathQuery, NamespaceManager);

         return xmlNode;
      }

      /// <summary>
      /// Gets the metadata node.
      /// </summary>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="deep">if set to <c>true</c> [deep].</param>
      /// <returns></returns>
      public XmlNode GetMetadataNode(string fieldName, bool deep)
      {
         string deepPath = string.Empty;
         if (deep)
         {
            deepPath = "/";
         }
         string xPathQuery = @"/tcm:*/tcm:Data/tcm:Metadata/Metadata:Metadata" + deepPath + "/Metadata:" + fieldName;
         XmlNode xmlNode = null;
         xmlNode = base.SelectSingleNode(xPathQuery, NamespaceManager);

         return xmlNode;
      }

      /// <summary>
      /// Gets the nodes.
      /// </summary>
      /// <param name="node">The node.</param>
      /// <param name="fieldName">Name of the field.</param>
      /// <returns></returns>
      public XmlNodeList GetNodes(XmlNode node, string fieldName)
      {
         string xPathQuery = @"Content:" + fieldName;
         XmlNodeList xmlNodeList = null;
         xmlNodeList = node.SelectNodes(xPathQuery, NamespaceManager);

         return xmlNodeList;
      }

      /// <summary>
      /// Gets the nodes.
      /// </summary>
      /// <param name="rootName">Name of the root.</param>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="deep">if set to <c>true</c> [deep].</param>
      /// <returns></returns>
      public XmlNodeList GetContentNodes(string rootName, string fieldName, bool deep)
      {
         string deepPath = string.Empty;
         if (deep)
         {
            deepPath = "/";
         }
         string xPathQuery = @"/tcm:*/tcm:Data/tcm:Content/Content:" + rootName + deepPath + "/Content:" + fieldName;
         XmlNodeList xmlNodeList = null;
         xmlNodeList = base.SelectNodes(xPathQuery, NamespaceManager);

         return xmlNodeList;
      }

      /// <summary>
      /// Gets the metadata nodes.
      /// </summary>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="deep">if set to <c>true</c> [deep].</param>
      /// <returns></returns>
      public XmlNodeList GetMetadataNodes(string fieldName, bool deep)
      {
         string deepPath = string.Empty;
         if (deep)
         {
            deepPath = "/";
         }
         string xPathQuery = @"/tcm:*/tcm:Data/tcm:Metadata/Metadata:Metadata" + deepPath + "/Metadata:" + fieldName;
         XmlNodeList xmlNodeList = null;
         xmlNodeList = base.SelectNodes(xPathQuery, NamespaceManager);

         return xmlNodeList;
      }

      /// <summary>
      /// Gets the attribute value.
      /// </summary>
      /// <param name="rootName">Name of the root.</param>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="attr">The attr.</param>
      /// <returns></returns>
      public string GetContentAttributeValue(string rootName, string fieldName, string attr)
      {
         string output = null;
         string xPathQuery = @"tcm:*/tcm:Data/tcm:Content/Content:" + rootName + "/Content:" + fieldName;
         XmlNode xmlNode = null;
         xmlNode = base.SelectSingleNode(xPathQuery, NamespaceManager);

         if (xmlNode != null)
         {
            output = GetAttributeValue(xmlNode, attr);
         }

         return output;
      }

      /// <summary>
      /// Gets the metadata attribute value.
      /// </summary>
      /// <param name="fieldName">Name of the field.</param>
      /// <param name="attr">The attr.</param>
      /// <returns></returns>
      public string GetMetadataAttributeValue(string fieldName, string attr)
      {
         string output = null;
         string xPathQuery = @"tcm:Component/tcm:Data/tcm:Metadata/Metadata:Metadata" + "/Metadata:" + fieldName;
         XmlNode xmlNode = null;
         xmlNode = base.SelectSingleNode(xPathQuery, NamespaceManager);

         if (xmlNode != null)
         {
            output = GetAttributeValue(xmlNode, attr);
         }

         return output;
      }

      /// <summary>
      /// Gets the attribute value.
      /// </summary>
      /// <param name="node">The node.</param>
      /// <param name="attr">The attr.</param>
      /// <returns></returns>
      public string GetAttributeValue(XmlNode node, string attr)
      {
         string output = null;
         XmlAttributeCollection attrColl = node.Attributes;

         for (int i = 0; i < attrColl.Count; i++)
         {
            if (attrColl.Item(i).Name.Equals(attr))
            {
               output = attrColl.Item(i).Value;
            }
         }
         return output;
      }

      /// <summary>
      /// Adds (if it exists) the Tridion Content namespace to the namespacemanager, using the Prefix "Content"
      /// </summary>
      public void AddContentNamespace()
      {
         AddNamespace("Content");
      }

      /// <summary>
      /// Adds (if it exists) the Tridion Metadata namespace to the namespacemanager, using the Prefix "Metadata"
      /// </summary>
      public void AddMetadataNamespace()
      {
         AddNamespace("Metadata");
      }

      /// <summary>
      /// Queries the base XML and adds Content or Metadata namespaces to the Namespace manager
      /// </summary>
      /// <param name="contentOrMetadata">"Content" or "Metadata"</param>
      private void AddNamespace(string contentOrMetadata)
      {
         string xpathQuery = string.Format("/tcm:*/tcm:Data/tcm:{0}/*[1]", contentOrMetadata);
         XmlNode contentNode = base.SelectSingleNode(xpathQuery, NamespaceManager);
         if (contentNode != null)
         {
            if (contentNode.NamespaceURI != null)
               NamespaceManager.AddNamespace(contentOrMetadata, contentNode.NamespaceURI);
         }
      }


      /// <summary>
      /// Checks if a given TridionXml object has a metadata node
      /// </summary>
      public bool HasMetadata
      {
         get
         {
            XmlNode metadataNode = base.SelectSingleNode("/tcm:*/tcm:Data/tcm:Metadata", mNamespaceManager);
            if (metadataNode != null)
            {
               if (base.SelectSingleNode("/tcm:*/tcm:Data/tcm:Metadata", mNamespaceManager).HasChildNodes)
                  return true;
            }
            return false;
         }
      }

      /// <summary>
      /// Checks if a given TridionXml object has a Content node
      /// </summary>
      public bool HasContent
      {
         get
         {
            XmlNode contentNode = base.SelectSingleNode("/tcm:*/tcm:Data/tcm:Content", mNamespaceManager);
            if (contentNode != null)
               return true;
            else
               return false;
         }
      }

      /// <summary>
      /// Returns an item's ID
      /// </summary>
      public string Id
      {
         get
         {
            return GetId();
         }


      }

      /// <summary>
      /// Reads an Item's ID
      /// </summary>
      /// <returns>The Item's ID</returns>
      private string GetId()
      {

         return base.SelectSingleNode("/tcm:*/@ID", NamespaceManager).Value;

      }
   }

}
