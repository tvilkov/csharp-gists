using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace CSharp.Gists.Dynamic
{
    internal class DynamicXElementWriter : DynamicXElementBase
    {
        private DynamicXElementWriter(XElement element)
            : base(element)
        {
        }

        public static dynamic CreateInstance(XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            return new DynamicXElementWriter(element);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            if (string.IsNullOrWhiteSpace(binder.Name)) throw new ArgumentException(@"binder.name must be non empty string", "binder");

            var child = InnerElement.Elements(binder.Name).FirstOrDefault();
            if (child == null)
            {
                child = new XElement(binder.Name);
                InnerElement.Add(child);
            }
            result = CreateInstance(child);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            if (value == null) throw new ArgumentNullException("value");
            if (string.IsNullOrWhiteSpace(binder.Name)) throw new ArgumentException(@"binder.name must be non empty string", "binder");

            if (binder.Name == InnerElement.Name)
                InnerElement.SetValue(value);
            else
                InnerElement.SetElementValue(binder.Name, value);

            return true;
        }

        public void SetValue(object value)
        {
            if (value == null) throw new ArgumentNullException("value");

            InnerElement.SetValue(value);
        }

        /// <summary>
        /// Changing curent XElement's attribute value
        /// </summary>
        public void SetAttributeValue(XName name, object value)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");

            InnerElement.SetAttributeValue(name, value);
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
            set
            {
                SetAttributeValue(name, value);
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
            set
            {
                var element = this[index];
                element.SetValue(value);
            }
        }
    }
}