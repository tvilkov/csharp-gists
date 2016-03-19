using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace CSharp.Gists.Primivitives
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{DebuggerDisplay}")]
    public struct Country : IEquatable<Country>, IComparable<Country>, IFormattable, ISerializable
    {
        private string m_IsoCode;
        private readonly bool m_isValid;
        private readonly int m_NumericCode;
        private const string ISO_CODE_FIELD = "isoCode";

        private static readonly Dictionary<string, int> m_IsoToNumericCodeMap
            = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                //TODO[tv]: Add all other countries
                { "RUS", 643 }, { "UKR", 804 }, { "BLR", 112 }, { "USA", 840 }
            };

        private static readonly Dictionary<int, string> m_NumericToIsoCodeMap
            = m_IsoToNumericCodeMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static readonly Country Empty = default(Country);

        #region Construction

        private Country(string isoCode)
            : this()
        {
            if (isoCode == null) throw new ArgumentNullException("isoCode");

            m_isValid = IsValidIsoCode(isoCode);
            m_IsoCode = m_isValid ? isoCode.ToUpperInvariant() : isoCode;
            m_NumericCode = m_isValid ? m_IsoToNumericCodeMap[m_IsoCode] : 0;
        }
        #endregion

        #region Serialization

        private Country(SerializationInfo information, StreamingContext context)
            : this(information.GetString(ISO_CODE_FIELD) ?? string.Empty)
        {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(ISO_CODE_FIELD, IsoCode);
        }

        #endregion

        public string IsoCode
        {
            [DebuggerStepThrough]
            get { return m_IsoCode ?? (m_IsoCode = ""); }
        }

        public int NumericCode
        {
            [DebuggerStepThrough]
            get { return m_NumericCode; }
        }

        public bool IsValid
        {
            [DebuggerStepThrough]
            get { return m_isValid; }
        }

        #region Equality, comparison, hash

        public override bool Equals(object obj)
        {
            if (!(obj is Country)) return false;
            return Equals((Country)obj);
        }

        public bool Equals(Country other)
        {
            return string.Equals(IsoCode, other.IsoCode, StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(Country other)
        {
            return String.Compare(IsoCode, other.IsoCode, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return IsoCode.GetHashCode();
        }

        #endregion Equality and hash

        #region Operator overloads

        public static bool operator ==(Country left, Country right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Country left, Country right)
        {
            return !(left == right);
        }

        public static implicit operator string(Country c)
        {
            return c.IsoCode;
        }

        public static implicit operator Country(string str)
        {
            return new Country(str ?? string.Empty);
        }

        #endregion Operator overloads

        #region Formatting

        [DebuggerStepThrough]
        public override string ToString()
        {
            return m_IsoCode;
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentUICulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return CountryFormat.ToString(this, format, formatProvider);
        }

        #endregion Formatting

        #region Factory methods

        public static Country Create(string countryCode)
        {
            if (!IsValidIsoCode(countryCode))
                throw new ArgumentException(string.Format("Value '{0}' is not a valid 3-letter country ISO code", "countryCode"));

            return new Country(countryCode);
        }

        public static bool IsValidIsoCode(string countryCode)
        {
            return !string.IsNullOrWhiteSpace(countryCode) && m_IsoToNumericCodeMap.ContainsKey(countryCode);
        }

        public static Country Create(int countryNumericCode)
        {
            if (!IsValidNumericCode(countryNumericCode))
                throw new ArgumentException(string.Format("Value '{0}' is not a valid numeric ISO country code", "countryNumericCode"));

            return new Country(m_NumericToIsoCodeMap[countryNumericCode]);
        }

        public static bool IsValidNumericCode(int countryNumericCode)
        {
            return countryNumericCode > 0 && m_NumericToIsoCodeMap.ContainsKey(countryNumericCode);
        }

        #endregion Factory methods

        #region Well known countries

        //TODO[tv]: add all known countries here and to resources
        public static readonly Country Russia = new Country("RUS");
        public static readonly Country Ukraine = new Country("UKR");
        public static readonly Country Belarusia = new Country("BLR");
        public static readonly Country USA = new Country("USA");

        #endregion Well known countries

        // ReSharper disable UnusedMember.Local
        private string DebuggerDisplay
        {
            get { return IsValid ? IsoCode : IsoCode + " [invalid]"; }
        }
        // ReSharper restore UnusedMember.Local
    }
}
