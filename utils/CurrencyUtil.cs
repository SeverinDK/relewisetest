using System.Globalization;
using System.Text.RegularExpressions;

namespace RelewiseTest.Utils
{
    public class CurrencyUtil
    {
        private static readonly string currencySymbolPattern = @"[$€£¥₹]";
        private static readonly string currencyIsoPattern = @"\b[A-Z]{3}\b";

        public static decimal RemoveCurrency(string input)
        {
            string cleanedInput = Regex.Replace(input, currencySymbolPattern, "");
            cleanedInput = Regex.Replace(cleanedInput, currencyIsoPattern, "");

            if (decimal.TryParse(cleanedInput.Trim(), NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            else
            {
                throw new FormatException($"Invalid price format: {cleanedInput}");
            }
        }

        public static string ExtractCurrency(string input)
        {
            Match symbolMatch = Regex.Match(input, currencySymbolPattern);
            if (symbolMatch.Success)
            {
                return symbolMatch.Value;
            }

            Match isoMatch = Regex.Match(input, currencyIsoPattern);
            if (isoMatch.Success)
            {
                return isoMatch.Value;
            }

            return string.Empty;
        }
    }
}
