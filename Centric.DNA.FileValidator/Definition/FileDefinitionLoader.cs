using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Centric.DNA.File.Definition
{
    public class FileDefinitionLoader
    {

      public static FileDefinition LoadFromXmlFile(String XmlFilePath)
      {
        return LoadFromXmlFile(XmlFilePath, false);
      }

      public static FileDefinition LoadFromXmlFile(String XmlFilePath, bool LoadOnlyFile)
      {

          XmlDocument XmlDoc = new XmlDocument();
          XmlDoc.Load(XmlFilePath);

          return LoadFromXmlDocument(XmlDoc);

      }

      public static FileDefinition LoadFromXmlString(String XmlString)
      {
        return LoadFromXmlString(XmlString, false);
      }

      public static FileDefinition LoadFromXmlString(String XmlString, bool LoadOnlyFile)
      {
          XmlDocument XmlDoc = new XmlDocument();
          XmlDoc.LoadXml(XmlString);

          return LoadFromXmlDocument(XmlDoc);

      }

      private static FileDefinition LoadFromXmlDocument(XmlDocument XmlDoc)
      {
        return LoadFromXmlDocument(XmlDoc, false);
      }
      

      private static FileDefinition LoadFromXmlDocument(XmlDocument XmlDoc, bool LoadOnlyFile)
      {

          FileDefinition fd = new FileDefinition();
          XmlNode FileNode = XmlDoc.SelectSingleNode("file");

          PopulateFileDefinition(FileNode, fd, LoadOnlyFile);

          return fd;

      }

      private static void PopulateFileDefinition(XmlNode FileNode, FileDefinition fd, bool LoadOnlyFile)
      {

          fd.Label = AttributeValue(FileNode, "label");

          // set the column delimiter. requires special handling for TAB
          string ColumnDelimiter = AttributeValue(FileNode, "column-delimiter", "\t");

          if(ColumnDelimiter.Equals("\\t"))
          {
            ColumnDelimiter = "\t";
          }

          fd.ColumnDelimiter = Char.Parse(ColumnDelimiter);

          fd.CompressWhitespace = bool.Parse(AttributeValue(FileNode, "compress-whitespace", "false"));
          fd.IgnoreEmptyRows = bool.Parse(AttributeValue(FileNode, "ignore-empty-rows", "true"));
          fd.TrimSpaces = bool.Parse(AttributeValue(FileNode, "trim-spaces", "true"));
          fd.TrimSpaces = bool.Parse(AttributeValue(FileNode, "strip-quotes", "true"));
          fd.SkipRows = int.Parse(AttributeValue(FileNode, "skip-rows", "0"));

          // get the encoding text and lookup the encoding object
          string EncodingText = AttributeValue(FileNode, "encoding", "ISO-8859-1");
          fd.Encoding = System.Text.Encoding.GetEncoding(EncodingText);         

          if (!LoadOnlyFile)
          {
            foreach (XmlNode RowNode in FileNode.SelectNodes("row"))
            {
              RowDefinition rd = new RowDefinition(fd);
              PopulateRowDefinition(RowNode, rd);
              fd.RowDefinitions.Add(rd);
            }
          }
      }

      private static void PopulateRowDefinition(XmlNode RowNode, RowDefinition rd)
      {
          rd.Label = AttributeValue(RowNode, "label");

          rd.DispositionColumnLabel = AttributeValue(RowNode, "disposition-column");
          rd.DispositionColumnValue = AttributeValue(RowNode, "disposition-column-value");
            
          int ColumnPosition = 0;

          foreach (XmlNode ColumnNode in RowNode.SelectNodes("column"))
          {

            ColumnPosition++;
              ColumnDefinition cd = new ColumnDefinition(rd);
              cd.Position = ColumnPosition;

              PopulateColumnDefinition(ColumnNode, cd);
              rd.ColumnDefinitions.Add(cd);
          }

      }

      private static void PopulateColumnDefinition(XmlNode ColumnNode, ColumnDefinition cd)
      {
          cd.Label = AttributeValue(ColumnNode, "label");

          cd.DataType = AttributeValue(ColumnNode, "data-type", "\r\n");
            
          cd.Required = bool.Parse(AttributeValue(ColumnNode, "required", "false"));
          cd.Truncate = bool.Parse(AttributeValue(ColumnNode, "truncate", "false"));
          cd.MaxLength = int.Parse(AttributeValue(ColumnNode, "max-length", "0"));
          cd.RegexPattern = AttributeValue(ColumnNode, "regex-pattern");

          cd.DomainList = AttributeValue(ColumnNode, "domain-list");
          cd.DomainCompliant = bool.Parse(AttributeValue(ColumnNode, "domain-compliant", "true"));
           

          // add domain values
          if(cd.DomainList!=null)
          {

            string DomainListXpath = "file/domain-list[@label=\"" + cd.DomainList + "\"]/item[@value]";
            string DomainValue = null;

            foreach (XmlNode dv in ColumnNode.OwnerDocument.SelectNodes(DomainListXpath))
            {

              DomainValue = AttributeValue(dv, "value");

              if (DomainValue != null && DomainValue.Length > 0)
                {
                  cd.DomainValues.Add(DomainValue);
                }
            }
          }

      }

      private static string AttributeValue(XmlNode Node, string AttributeName, string DefaultValue = null)
      {
          XmlAttribute Attribute = Node.Attributes[AttributeName];

          if (Attribute== null || Attribute.Value.Trim().Length == 0)
          {
              return DefaultValue;

          } else 
          {
              return Attribute.Value;
          }
      }
   }
}
