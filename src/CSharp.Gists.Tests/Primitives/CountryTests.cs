using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharp.Gists.Primitives;
using NUnit.Framework;

namespace CSharp.Gists.Tests.Primitives
{
    [TestFixture]
    public class CountryTests
    {
        [Test]
        public void CanCreateAsDefaultValue()
        {
            var country = default(Country);

            Assert.That(country.IsoCode, Is.EqualTo(""));
            Assert.That(country.NumericCode, Is.EqualTo(0));
            Assert.IsFalse(country.IsValid);
        }

        [Test]
        public void DefaultValueIsSameAsEmpty()
        {
            var defaultCountry = default(Country);

            Assert.That(defaultCountry.IsoCode, Is.EqualTo(Country.Empty.IsoCode));
            Assert.That(defaultCountry.NumericCode, Is.EqualTo(Country.Empty.NumericCode));
            Assert.That(defaultCountry.IsValid, Is.EqualTo(Country.Empty.IsValid));
        }

        [Test]
        public void CounriesAreEqualWhenThereIsoCodesAreEqual()
        {
            var country1 = Country.Create("RUS");
            var country2 = Country.Create("RUS");
            var country3 = Country.Create("USA");

            Assert.IsTrue(country1.Equals(country2));
            Assert.IsTrue(country1 == country2);

            Assert.IsFalse(country1.Equals(country3));
            Assert.IsFalse(country1 == country3);
            Assert.IsTrue(country1 != country3);
        }

        [TestCase("RUS", "RUS", 643, true)]
        [TestCase("UKR", "UKR", 804, true)]
        [TestCase("BLR", "BLR", 112, true)]
        [TestCase("USA", "USA", 840, true)]
        [TestCase("rus", "RUS", 643, true)]
        [TestCase("ukr", "UKR", 804, true)]
        [TestCase("blr", "BLR", 112, true)]
        [TestCase("usa", "USA", 840, true)]
        [TestCase(default(string), "", 0, false)]
        [TestCase("", "", 0, false)]
        [TestCase("XXX", "XXX", 0, false)]
        [TestCase("123", "123", 0, false)]
        [TestCase("NotACountryCode", "NotACountryCode", 0, false)]
        public void CanImpicitlyConvertFromString(string input, string expectedIsoCode, int expectedNumericCode, bool expectedIsValid)
        {
            Country c = input;
            Assert.That(c.IsoCode, Is.EqualTo(expectedIsoCode));
            Assert.That(c.NumericCode, Is.EqualTo(expectedNumericCode));
            Assert.That(c.IsValid, Is.EqualTo(expectedIsValid));
        }

        [Test, TestCaseSource(typeof(CountryTests), "CanImpicitlyConvertToStringTestCases")]
        public void CanImpicitlyConvertToString(Country country, string expectedIsoCode)
        {
            string str = country;
            Assert.AreEqual(str, expectedIsoCode);
        }

        protected static IEnumerable<TestCaseData> CanImpicitlyConvertToStringTestCases
        {
            get
            {
                return "RUS,UKR,BLR,USA".Split(',').Select(c => new TestCaseData(Country.Create(c), c).SetName(c)).ToArray();
            }
        }
    }
}