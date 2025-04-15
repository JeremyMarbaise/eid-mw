namespace auth_server.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class EidDateParser
    {
        private static readonly Dictionary<string, string> FrenchMonths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {"JAN", "01"}, {"FEV", "02"}, {"MARS", "03"}, {"AVR", "04"},
        {"MAI", "05"}, {"JUIN", "06"}, {"JUIL", "07"}, {"AOUT", "08"},
        {"SEPT", "09"}, {"OCT", "10"}, {"NOV", "11"}, {"DEC", "12"}
    };

        private static readonly Dictionary<string, string> DutchMonths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {"JAN", "01"}, {"FEB", "02"}, {"MAAR", "03"}, {"APR", "04"},
        {"MEI", "05"}, {"JUN", "06"}, {"JUL", "07"}, {"AUG", "08"},
        {"SEP", "09"}, {"OKT", "10"}, {"NOV", "11"}, {"DEC", "12"}
    };

        private static readonly Dictionary<string, string> GermanMonths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {"JAN", "01"}, {"FEB", "02"}, {"MÄR", "03"}, {"APR", "04"},
        {"MAI", "05"}, {"JUN", "06"}, {"JUL", "07"}, {"AUG", "08"},
        {"SEP", "09"}, {"OKT", "10"}, {"NOV", "11"}, {"DEZ", "12"}
    };

        public static string ConvertEidDateToIso8601(string eidDate)
        {
            Console.WriteLine(eidDate);
            if (string.IsNullOrWhiteSpace(eidDate))
                throw new ArgumentException("Date string cannot be null or empty");

            if (eidDate.Contains('.'))
            {
                return ParseDotSeparatedFormat(eidDate);
            }
            // Try French/Dutch format (DD mmmm YYYY)
            else if (eidDate.Contains(' '))
            {
                return ParseSpaceSeparatedFormat(eidDate);
            }
            // Try German format (DD.mmm.YYYY)
           
            else
            {
                throw new FormatException($"Unrecognized eID date format: {eidDate}");
            }
        }

        private static string ParseSpaceSeparatedFormat(string date)
        {
            var parts = date.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new FormatException($"Invalid space-separated date format: {date}");

            string day = parts[0].PadLeft(2, '0');
            string monthAbbr = parts[1].ToUpper();
            string year = parts[2];

            // Try French months first
            if (FrenchMonths.TryGetValue(monthAbbr, out string month))
            {
                return $"{year}-{month}-{day}";
            }
            // Then try Dutch months
            else if (DutchMonths.TryGetValue(monthAbbr, out month))
            {
                return $"{year}-{month}-{day}";
            }
            else
            {
                throw new FormatException($"Unrecognized month abbreviation: {monthAbbr}");
            }
        }

        private static string ParseDotSeparatedFormat(string date)
        {
            var parts = date.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new FormatException($"Invalid dot-separated date format: {date}");

            string day = parts[0].PadLeft(2, '0');
            string monthAbbr = parts[1].ToUpper();
            string year = parts[2];

            if (GermanMonths.TryGetValue(monthAbbr, out string month))
            {
                return $"{year}-{month}-{day}";
            }
            else
            {
                throw new FormatException($"Unrecognized German month abbreviation: {monthAbbr}");
            }
        }

        // Bonus: Validate the converted date
        public static bool IsValidDate(string isoDate)
        {
            return DateTime.TryParseExact(isoDate, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }
    }
}
