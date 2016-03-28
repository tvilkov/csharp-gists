using System;
using System.Xml.Linq;

namespace CSharp.Gists.Dynamic
{
    public static class DynamicXElementExtensions
    {
        public static dynamic AsDynamic(this XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            return DynamicXElementReader.CreateInstance(element);
        }

        public static dynamic AsDynamic(this XDocument document)
        {
            if (document == null) throw new ArgumentNullException("document");

            return DynamicXElementReader.CreateInstance(document.Root);
        }

        public static dynamic AsDynamicWriter(this XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            return DynamicXElementWriter.CreateInstance(element);
        }

        public static dynamic AsDynamicWriter(this XDocument document)
        {
            if (document == null) throw new ArgumentNullException("document");

            return DynamicXElementWriter.CreateInstance(document.Root);
        }

    }
}