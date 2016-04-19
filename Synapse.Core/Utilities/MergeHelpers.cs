﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.XmlDiffPatch;

namespace Synapse.Core.Utilities
{
	public class MergeHelpers
	{
		public static void MergeXml(ref XmlDocument source, XmlDocument patch)
		{
			Stack<XmlNodeList> lists = new Stack<XmlNodeList>();
			lists.Push( patch.ChildNodes );

			while( lists.Count > 0 )
			{
				XmlNodeList list = lists.Pop();
				foreach( XmlNode node in list )
				{
					string xpath = FindXPath( node );
					XmlNode src = source.SelectSingleNode( xpath );
					if( src != null && src.Value != node.Value )
					{
						if( src.NodeType == XmlNodeType.Element )
						{
							src.InnerText = node.InnerText;
						}
						else
						{
							src.Value = node.Value;
						}
					}
					if( node.Attributes != null )
					{
						foreach( XmlNode attr in node.Attributes )
						{
							string xpathAttr = FindXPath( attr );
							XmlNode srcAttr = source.SelectSingleNode( xpathAttr );
							if( srcAttr != null && srcAttr.Value != attr.Value )
							{
								srcAttr.Value = attr.Value;
							}
						}
					}
					if( node.ChildNodes.Count > 0 )
					{
						lists.Push( node.ChildNodes );
					}
				}
			}
		}
		static string FindXPath(XmlNode node)
		{
			StringBuilder builder = new StringBuilder();
			while( node != null )
			{
				switch( node.NodeType )
				{
					case XmlNodeType.Attribute:
					{
						builder.Insert( 0, "/@" + node.Name );
						node = ((XmlAttribute)node).OwnerElement;
						break;
					}
					case XmlNodeType.Element:
					{
						int index = FindElementIndex( (XmlElement)node );
						builder.Insert( 0, "/" + node.Name + "[" + index + "]" );
						node = node.ParentNode;
						break;
					}
					case XmlNodeType.Document:
					{
						return builder.ToString();
					}
					case XmlNodeType.Text:
					{
						node = node.ParentNode;
						break;
					}
					//default:
					//{
					//	throw new ArgumentException( "Only elements and attributes are supported" );
					//}
				}
			}
			throw new ArgumentException( "Node was not in a document" );
		}

		static int FindElementIndex(XmlElement element)
		{
			XmlNode parentNode = element.ParentNode;
			if( parentNode is XmlDocument )
			{
				return 1;
			}
			XmlElement parent = (XmlElement)parentNode;
			int index = 1;
			foreach( XmlNode candidate in parent.ChildNodes )
			{
				if( candidate is XmlElement && candidate.Name == element.Name )
				{
					if( candidate == element )
					{
						return index;
					}
					index++;
				}
			}
			throw new ArgumentException( "Couldn't find element within parent" );
		}


		public static void MergeXml(ref XmlNode sourceNode, XmlNode changedNode)
		{
			XmlDiff xmldiff = new XmlDiff( XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreXmlDecl |
				XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreNamespaces | XmlDiffOptions.IgnorePrefixes );

			MemoryStream ms = new MemoryStream();
			XmlTextWriter diffgramWriter = new XmlTextWriter( ms, Encoding.Default );
			bool match = xmldiff.Compare( sourceNode, changedNode, diffgramWriter );
			ms.Position = 0;

			using( XmlTextReader diffgramReader = new XmlTextReader( ms ) )
			{
				XmlPatch xmlpatch = new XmlPatch();
				xmlpatch.Patch( ref sourceNode, diffgramReader );
			}

			diffgramWriter.Close();
		}

		//reference: https://msdn.microsoft.com/en-us/library/aa302295.aspx
		//source:	 https://msdn.microsoft.com/en-us/library/aa302294.aspx
		public void GenerateDiffGram(string originalFile, string finalFile, XmlWriter diffgramWriter)
		{
			XmlDiff xmldiff = new XmlDiff( XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreXmlDecl |
				XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreNamespaces | XmlDiffOptions.IgnorePrefixes );

			bool bIdentical = xmldiff.Compare( originalFile, finalFile, false, diffgramWriter );
			diffgramWriter.Close();
		}

		public void PatchUp(string originalFile, String diffgramFile, String outputFile)
		{
			XmlDocument sourceDoc = new XmlDocument( new NameTable() );
			sourceDoc.Load( originalFile );

			using( XmlTextReader diffgramReader = new XmlTextReader( diffgramFile ) )
			{
				XmlPatch xmlpatch = new XmlPatch();
				xmlpatch.Patch( sourceDoc, diffgramReader );

				using( XmlTextWriter output = new XmlTextWriter( outputFile, Encoding.Unicode ) )
				{
					sourceDoc.Save( output );
				}
			}
		}
	}
}