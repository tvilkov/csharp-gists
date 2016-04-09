using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
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
    }
}