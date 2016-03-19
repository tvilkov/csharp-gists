using System;
using System.Globalization;

namespace CSharp.Gists.Primivitives
{
    internal static class CountryFormat
    {
        internal abstract class KnownFormats
        {
            public const char ISO = 'i';
            public const char NUMERIC = 'd';
            public const char NAME = 's';
        }

        public static string ToString(Country country, string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "i";
            }

            if (format.Length == 1)
            {
                switch (format[0])
                {
                    case KnownFormats.ISO:
                        return country.IsoCode;
                    case KnownFormats.NUMERIC:
                        return country.NumericCode.ToString(formatProvider);
                    case KnownFormats.NAME:
                        {
                            var ci = formatProvider as CultureInfo;
                            return CountryStrings.ResourceManager.GetString(country.IsoCode, ci) ?? country.IsoCode;
                        }
                }
            }
            throw new FormatException(string.Format("Bad format string '{0}'", format));
        }
    }
}