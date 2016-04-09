using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CSharp.Gists.Dynamic
{
    internal class DynamicXElementBase : DynamicObject
    {
        protected readonly XElement InnerElement;

        protected DynamicXElementBase(XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            InnerElement = element;
        }

        public sealed override bool Equals(object obj)
        {
            var comparand = obj as DynamicXElementBase;
            return !ReferenceEquals(comparand, null) && InnerElement == comparand.InnerElement;
        }

        public override int GetHashCode()
        {
            return InnerElement.GetHashCode();
        }

        public override string ToString()
        {
            return InnerElement.ToString();
        }

        public sealed override IEnumerable<string> GetDynamicMemberNames()
        {
            return InnerElement.Elements()
                               .Select(x => x.Name.LocalName)
                               .Distinct()
                               .OrderBy(x => x);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder == null) throw new ArgumentNullException("binder");

            if (binder.ReturnType == typeof(XElement))
            {
                result = InnerElement;
                return true;
            }

            if (binder.ReturnType.IsAssignableFrom(typeof(IEnumerable<XElement>)) || binder.ReturnType == typeof(XElement[]))
            {
                result = InnerElement.Elements().ToArray();
                return true;
            }

            // TimeSpan can't be handled with Convert.ChangeType, threfore we parse it manually
            if (binder.ReturnType == typeof(TimeSpan))
            {
                result = TimeSpan.Parse(InnerElement.Value);
                return true;
            }

            result = Convert.ChangeType(InnerElement.Value, binder.ReturnType);
            return true;
        }

        public XElement XElement
        {
            get { return InnerElement; }
        }
    }
}