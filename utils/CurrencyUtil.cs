using System.Globalization;
using System.Text.RegularExpressions;

namespace RelewiseTest.Utils
{
    public static class CurrencyUtil
    {
        private static readonly string currencySymbolPattern = @"\s*\p{Sc}\s*";
        private static readonly string currencyIsoPattern = @"\s*[A-Z]{3}\s*";

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
                return symbolMatch.Value.Trim();
            }

            Match isoMatch = Regex.Match(input, currencyIsoPattern);
            if (isoMatch.Success)
            {
                return isoMatch.Value.Trim();
            }

            return string.Empty;
        }
    }
}
