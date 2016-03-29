using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using CSharp.Gists.Dynamic;
using NUnit.Framework;

namespace CSharp.Gists.Tests.Dynamic
{
    public class DynamicXElementTests
    {
        [TestFixture(Category = "Reader", Description = "Dynamic XElement reader tests")]
        public class ReaderTests
        {
            [Test]
            public void CanCreateDynamicReader()
            {
                var elem = new XElement("Root");
                dynamic dyn = DynamicXElementReader.CreateInstance(elem);

                Assert.IsNotNull(dyn);
                Assert.IsNotNullOrEmpty(dyn.ToString());
            }

            [Test]
            public void CreateFromNullXElementShouldFail()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => DynamicXElementReader.CreateInstance(default(XElement)));
                Assert.IsNotNull(ex);
                Assert.AreEqual("element", ex.ParamName);
            }

            [Test]
            public void CanCastToXElement()
            {
                var elem = XElement.Parse("<root>123</root>");
                var dyn = DynamicXElementReader.CreateInstance(elem);
                XElement casted = dyn;

                Assert.AreEqual(elem, casted);
            }

            [Test]
            public void CanCastToString()
            {
                var elem = XElement.Parse("<root>123</root>");
                var dyn = DynamicXElementReader.CreateInstance(elem);
                string casted = dyn;

                Assert.AreEqual("123", casted);
            }

            [Test]
            public void CanCastToInt()
            {
                var elem = XElement.Parse("<root>123</root>");
                var dyn = DynamicXElementReader.CreateInstance(elem);
                int casted = dyn;

                Assert.AreEqual(123, casted);
            }

            [Test]
            public void CanCastToDouble()
            {
                var elem = XElement.Parse("<root>123.123</root>");
                var dyn = DynamicXElementReader.CreateInstance(elem);
                double casted = dyn;

                Assert.AreEqual(123.123, casted);
            }

            [Test]
            public void CanCastToTimeSpan()
            {
                var time = TimeSpan.FromMinutes(120.5);
                var elem = XElement.Parse(string.Format("<root>{0}</root>", time));
                var dyn = DynamicXElementReader.CreateInstance(elem);
                TimeSpan casted = dyn;

                Assert.AreEqual(time, casted);
            }

            [Test]
            public void CanNavigateToChildElement()
            {
                var elem = XElement.Parse("<root><item><subitem>123</subitem></item></root>");
                var dyn = DynamicXElementReader.CreateInstance(elem);
                XElement child = dyn.item.subitem;

                Assert.AreEqual("subitem", child.Name.LocalName);
                Assert.AreEqual("123", child.Value);
            }

            [Test]
            public void NavigationToAbsentElementShouldFail()
            {
                var elem = XElement.Parse("<root>123</root>");
                var dyn = DynamicXElementReader.CreateInstance(elem);
                var ex = Assert.Throws<InvalidOperationException>(() => { var notUsed = dyn.NotExists; });
                Assert.IsNotNull(ex);
                Assert.AreEqual("Element 'NotExists' not found among subelements of 'root'", ex.Message);
            }

            [Test]
            public void CanReadAttributes()
            {
                var elem = XElement.Parse("<root str='123' int='123' double='123.123'></root>");
                var dyn = DynamicXElementReader.CreateInstance(elem);

                string strVal = dyn["str"];
                int intVal = dyn["int"];
                double doubleVal = dyn["double"];

                Assert.AreEqual("123", strVal);
                Assert.AreEqual(123, intVal);
                Assert.AreEqual(123.123, doubleVal);
            }
        }
    }
}