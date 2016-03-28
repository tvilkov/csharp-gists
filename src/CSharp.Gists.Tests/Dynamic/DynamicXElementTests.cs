using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using CSharp.Gists.Dynamic;
using NUnit.Framework;

namespace CSharp.Gists.Tests.Dynamic
{
    [TestFixture]
    public class DynamicXElementTests
    {
        [Test]
        public void Test()
        {
            var elem = XElement.Parse(@"<root>
<items>
    <item1 id='1'>Value1</item1>
    <item2 id='2'>1</item2>
</items>
</root>");
            dynamic reader = DynamicXElementReader.CreateInstance(elem);
            XElement elem1 = reader;
            XElement[] children = reader.items;
            foreach (XElement item in reader.items)
            {
                Console.WriteLine(item.Name);
            }
            
            string value = reader;
            string value1 = reader.items.item1;
            int value3 = reader.items.item2["id"];

            dynamic writer = DynamicXElementWriter.CreateInstance(new XElement("Root"));
            writer.Children["count"] = 2;
            writer.Children.Child = DateTime.Now;

            XElement resut = writer;

        }
    }
}