using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace CSharp.Gists.Dynamic
{
    internal class DynamicXElementReader : DynamicXElementBase
    {
        private DynamicXElementReader(XElement element)
            : base(element)
        {
        }

        public static dynamic CreateInstance(XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            return new DynamicXElementReader(element);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            if (string.IsNullOrWhiteSpace(binder.Name)) throw new ArgumentException(@"binder.name must be non empty string", "binder");

            var child = InnerElement.Elements(binder.Name).FirstOrDefault();
            if (child != null)
            {
                result = CreateInstance(child);
                return true;
            }

            throw new InvalidOperationException(string.Format("Element '{0}' not found among subelements of '{1}'", binder.Name, InnerElement.Name));
        }

        public dynamic this[XName name]
        {
            get
            {
                if (name == null) throw new ArgumentNullException("name");

                var attribute = InnerElement.Attribute(name);

                if (attribute == null)
                    throw new InvalidOperationException(string.Format("Attribute '{0}' not found in element '{1}'", name, InnerElement.Name));

                return DynamicXAttribute.CreateInstance(attribute);
            }
        }

        public dynamic this[int index]
        {
            get
            {
                if (index < 0 || index >= InnerElement.Elements().Count())
                    throw new IndexOutOfRangeException("Index out of range");

                var child = InnerElement.Elements().ElementAt(index);
                Debug.Assert(child != null);
                return CreateInstance(child);
            }
        }
    }
}