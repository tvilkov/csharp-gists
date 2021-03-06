﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CSharp.Gists.Dynamic;
using NUnit.Framework;

namespace CSharp.Gists.Tests.Dynamic
{
    public class DynamicXElementTests
    {
        [TestFixture(Category = "Reader", Description = "Dynamic XElementReader tests")]
        public class ReaderTests
        {
            [SetUp]
            public void Setup()
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            }

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
            public void EqualsAndGetHashCodeMethodsTest()
            {
                var xelement1 = new XElement("element", "value");
                var xelement2 = new XElement("element", "value");

                var dyn1 = xelement1.AsDynamic();
                var dyn2 = xelement2.AsDynamic();

                Assert.That(xelement1.Equals(xelement2), Is.EqualTo(dyn1.Equals(dyn2)));
                Assert.AreEqual(xelement1.GetHashCode(), dyn1.GetHashCode());
                Assert.AreEqual(xelement2.GetHashCode(), dyn2.GetHashCode());
            }

            [Test]
            public void ToStringMethodTest()
            {
                var xelement = XElement.Parse("<root><item>1</item><item>2</item></root>");
                var dyn = xelement.AsDynamic();

                Assert.AreEqual(xelement.ToString(), dyn.ToString());
            }

            [Test]
            public void GetDynamicMemberNamesMethodTests()
            {
                var xelement = XElement.Parse("<root><item1>1</item1><item2>2</item2><item3>3</item3></root>");
                var dyn = xelement.AsDynamic();
                IEnumerable<string> memberNames = dyn.GetDynamicMemberNames();
                CollectionAssert.AreEqual(memberNames, new[] { "item1", "item2", "item3" });
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
                var elem = XElement.Parse(string.Format("<root>{0}</root>", 123.1234567));
                var dyn = DynamicXElementReader.CreateInstance(elem);
                double casted = dyn;

                Assert.AreEqual(123.1234567, casted);
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
                // ReSharper disable UnusedVariable
                var ex = Assert.Throws<InvalidOperationException>(() => { var notUsed = dyn.NotExists; });
                // ReSharper restore UnusedVariable
                Assert.IsNotNull(ex);
                Assert.AreEqual("Element 'NotExists' not found among subelements of 'root'", ex.Message);
            }

            [Test]
            public void CanNavigateToChildElementByIndex()
            {
                var item1 = new XElement("item", "item1");
                var item2 = new XElement("item", "item2");
                var elem = new XElement("root", item1, item2);

                var dyn = DynamicXElementReader.CreateInstance(elem);

                XElement dynItem1 = dyn.item[0];
                XElement dynItem2 = dyn.item[1];

                Assert.AreEqual(item1, dynItem1);
                Assert.AreEqual(item2, dynItem2);
            }

            [Test]
            public void NavigationToNotExistingChildElementByIndexShouldFail()
            {
                var elem = new XElement("root", new XElement("item", "item1"));

                var dyn = DynamicXElementReader.CreateInstance(elem);

                // ReSharper disable UnusedVariable
                var ex = Assert.Throws<ArgumentOutOfRangeException>(() => { var notUsed = dyn.item[3]; });
                // ReSharper restore UnusedVariable

                Assert.IsNotNull(ex);
            }

            [Test]
            public void CanReadAttributes()
            {
                var elem = XElement.Parse(string.Format("<root str='123' int='123' double='{0}'></root>", 123.1234567));
                var dyn = DynamicXElementReader.CreateInstance(elem);

                string strVal = dyn["str"];
                int intVal = dyn["int"];
                double doubleVal = dyn["double"];

                Assert.AreEqual("123", strVal);
                Assert.AreEqual(123, intVal);
                Assert.AreEqual(123.1234567, doubleVal);
            }

            [Test]
            public void ReadComplexXmlTest()
            {
                const string xml = @"
<breakfast>
	<food id='food-1'>
		<name>Belgian Waffles</name>
		<price>5,95</price>
		<calories>650</calories>
        <category ref='desert' />
	</food>
	<food id='food-2'>
		<name>Strawberry Belgian Waffles</name>
		<price>7,95</price>
		<calories>900</calories>
        <category ref='desert' />
	</food>
	<food id='food-3'>
		<name>Berry-Berry Belgian Waffles</name>
		<price>8,95</price>
		<calories>900</calories>
        <category ref='desert' />
	</food>
</breakfast>
";
                var xelement = XElement.Parse(xml);
                var dyn = xelement.AsDynamic();

                Assert.That((string)dyn.food[0]["id"], Is.EqualTo("food-1"));
                Assert.That((string)dyn.food[0].name, Is.EqualTo("Belgian Waffles"));
                Assert.That((double)dyn.food[0].price, Is.EqualTo(5.95));
                Assert.That((int)dyn.food[0].calories, Is.EqualTo(650));
                Assert.That((string)dyn.food[0].category["ref"], Is.EqualTo("desert"));

                Assert.That((string)dyn.food[1]["id"], Is.EqualTo("food-2"));
                Assert.That((string)dyn.food[1].name, Is.EqualTo("Strawberry Belgian Waffles"));
                Assert.That((double)dyn.food[1].price, Is.EqualTo(7.95));
                Assert.That((int)dyn.food[1].calories, Is.EqualTo(900));
                Assert.That((string)dyn.food[1].category["ref"], Is.EqualTo("desert"));

                Assert.That((string)dyn.food[2]["id"], Is.EqualTo("food-3"));
                Assert.That((string)dyn.food[2].name, Is.EqualTo("Berry-Berry Belgian Waffles"));
                Assert.That((double)dyn.food[2].price, Is.EqualTo(8.95));
                Assert.That((int)dyn.food[2].calories, Is.EqualTo(900));
                Assert.That((string)dyn.food[2].category["ref"], Is.EqualTo("desert"));
            }
        }

        [TestFixture(Category = "Writer", Description = "Dynamic XElementWriter tests")]
        public class WriterTests
        {
            [SetUp]
            public void Setup()
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            }

            [Test]
            public void CanCreateDynamicWriter()
            {
                var elem = new XElement("Root");
                dynamic dyn = DynamicXElementWriter.CreateInstance(elem);

                Assert.IsNotNull(dyn);
            }

            [Test]
            public void CreateFromNullXElementShouldFail()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => DynamicXElementWriter.CreateInstance(default(XElement)));
                Assert.IsNotNull(ex);
                Assert.AreEqual("element", ex.ParamName);
            }

            [Test]
            public void CanChangeExistingElement()
            {
                var xelem = new XElement("root", "value");
                var writer = xelem.AsDynamicWriter();

                // Using writer = "2" would be better but unfortunatelly we can't do it
                writer.SetValue("newValue");

                Assert.AreEqual("newValue", xelem.Value);
            }

            [Test]
            public void CanChangeExistingSubElement()
            {
                var subItem = new XElement("item", "value");
                var xelem = new XElement("root", subItem);
                var writer = xelem.AsDynamicWriter();

                // Using writer.item = "2" would be better but unfortunatelly we can't do it
                writer.item.SetValue("newValue");

                Assert.That(subItem.Value, Is.EqualTo("newValue"));
            }

            [Test]
            public void CanInsertNewSubElement()
            {
                var xelem = new XElement("root");
                var writer = xelem.AsDynamicWriter();
                writer.item = "newValue";

                var insertedElem = xelem.Element("item");

                Assert.IsNotNull(insertedElem);
                Assert.AreEqual("newValue", insertedElem.Value);
            }

            [Test]
            public void CanInsertSeveralNewSubElements()
            {
                var xelem = new XElement("root");
                var writer = xelem.AsDynamicWriter();
                writer.item[0] = "item0";
                writer.item[1] = "item1";
                writer.item[2] = "item2";

                var insertedElem1 = xelem.Elements("item").First();
                var insertedElem2 = xelem.Elements("item").Skip(1).First();
                var insertedElem3 = xelem.Elements("item").Skip(2).First();

                Assert.AreEqual("item0", insertedElem1.Value);
                Assert.AreEqual("item1", insertedElem2.Value);
                Assert.AreEqual("item2", insertedElem3.Value);
            }

            [Test]
            public void CanInsertChainOfNewSubElements()
            {
                var xelem = new XElement("root");
                var writer = xelem.AsDynamicWriter();
                writer.item.subItemLevel1.subItemLevel2.subItemLevel3 = "newValue";

                var insertedElem = xelem.Element("item")
                                        .Element("subItemLevel1")
                                        .Element("subItemLevel2")
                                        .Element("subItemLevel3");

                Assert.IsNotNull(insertedElem);
                Assert.AreEqual("newValue", insertedElem.Value);
            }

            [Test]
            public void CanInsertSeveralSubElementsUsingIndexer()
            {
                var xelem = new XElement("root");
                var writer = xelem.AsDynamicWriter();
                writer.item[0].subItem = "item0";
                writer.item[1].subItem = "item1";

                var insertedElem1 = xelem.Elements("item").First().Element("subItem");
                var insertedElem2 = xelem.Elements("item").Skip(1).First().Element("subItem");

                Assert.IsNotNull(insertedElem1);
                Assert.IsNotNull(insertedElem2);
                Assert.AreEqual("item0", insertedElem1.Value);
                Assert.AreEqual("item1", insertedElem2.Value);
            }

            [Test]
            public void CanChangeExistingAttribute()
            {
                var xelem = new XElement("root", new XAttribute("id", "value"));
                var writer = xelem.AsDynamicWriter();
                writer["id"] = "newValue";

                Assert.AreEqual("newValue", xelem.Attribute("id").Value);
            }

            [Test]
            public void CanInsertNewAttribute()
            {
                var xelem = new XElement("root");
                var writer = xelem.AsDynamicWriter();
                writer["id"] = "newValue";
                writer["int"] = 123;
                writer["double"] = 123.1234567;

                Assert.AreEqual("newValue", xelem.Attribute("id").Value);
                Assert.AreEqual(123, (int)xelem.Attribute("int"));
                Assert.AreEqual(123.1234567, (double)xelem.Attribute("double"));
            }
        }
    }
}