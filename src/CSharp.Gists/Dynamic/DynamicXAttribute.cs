using System;
using System.Dynamic;
using System.Globalization;
using System.Xml.Linq;

namespace CSharp.Gists.Dynamic
{
    internal class DynamicXAttribute : DynamicObject
    {
        private readonly XAttribute m_Attribute;

        private DynamicXAttribute(XAttribute attribute)
        {
            if (attribute == null) throw new ArgumentNullException("attribute");
            
            m_Attribute = attribute;
        }

        public static dynamic CreateInstance(XAttribute attribute)
        {
            if (attribute == null) throw new ArgumentNullException("attribute");

            return new DynamicXAttribute(attribute);
        }

        public override string ToString()
        {
            return m_Attribute.ToString();
        }

        public override int GetHashCode()
        {
            return m_Attribute.GetHashCode();
        }

        protected bool Equals(DynamicXAttribute other)
        {
            return !ReferenceEquals(null, other) && Equals(m_Attribute, other.m_Attribute);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DynamicXAttribute);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.ReturnType == typeof(XAttribute))
            {
                result = m_Attribute;
                return true;
            }

            // TimeSpan can't be handled with Convert.ChangeType, threfore we parse it manually
            if (binder.ReturnType == typeof(TimeSpan))
            {
                result = TimeSpan.Parse(m_Attribute.Value);
                return true;
            }

            result = Convert.ChangeType(m_Attribute.Value, binder.ReturnType);
            return true;
        }

        public string Value
        {
            get { return m_Attribute.Value; }
        }
    }
}