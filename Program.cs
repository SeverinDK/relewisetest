using Relewise.Parsers;
using RelewiseTest.Parsers;

namespace RelewiseTest
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            string rawData = await ParseRawData();
            string jsonData = await ParseCustomJSONFeedData();
            string googleShoppingFeedData = await ParseGoogleShoppingFeedData();

            PrintResult(ParserType.RawData, rawData);
            PrintResult(ParserType.JSONData, jsonData);
            PrintResult(ParserType.GoogleShoppingFeed, googleShoppingFeedData);
        }

        private static async Task<string> ParseRawData()
        {
            string url = GetEnvironmentVariable("PRODUCT_DATA_RAW");
            string key = GetEnvironmentVariable("PRODUCT_DATA_RAW_KEY");
            Dictionary<string, string> jobConfiguration = new()
            {
                { "url", url },
                { "language", "en" }
            };

            return await ParseData(ParserType.RawData, jobConfiguration, key);
        }

        private async static Task<string> ParseCustomJSONFeedData()
        {
            string url = GetEnvironmentVariable("PRODUCT_DATA_CUSTON_JSON_FEED");
            string key = GetEnvironmentVariable("PRODUCT_DATA_CUSTON_JSON_FEED_KEY");
            Dictionary<string, string> jobConfiguration = new()
            {
                { "url", url },
                { "language", "en" }
            };

            return await ParseData(ParserType.JSONData, jobConfiguration, key);
        }

        private async static Task<string> ParseGoogleShoppingFeedData()
        {
            string url = GetEnvironmentVariable("PRODUCT_DATA_GOOGLE_SHOPPING_FEED");
            string key = GetEnvironmentVariable("PRODUCT_DATA_GOOGLE_SHOPPING_FEED_KEY");
            Dictionary<string, string> jobConfiguration = new()
            {
                { "url", url },
                { "language", "en" }
            };

            return await ParseData(ParserType.GoogleShoppingFeed, jobConfiguration, key);
        }

        private static string GetEnvironmentVariable(string name)
        {
            string? value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception($"{name} environment variable is not set.");
            }

            return value;
        }

        private static async Task<string> ParseData(ParserType parserType, Dictionary<string, string> jobConfiguration, string apiKey)
        {
            IJob parser = ProductParserFactory.CreateParser(parserType);

            string parserTypeName = parserType.ToString();
            JobArguments jobArguments = new(
                Guid.NewGuid(),
                apiKey,
                jobConfiguration);

            return await parser.Execute(
                jobArguments,
                message =>
                {
                    Console.WriteLine($"INFO: {message}");
                    return Task.CompletedTask;
                },
                message =>
                {
                    Console.WriteLine($"WARNING: {message}");
                    return Task.CompletedTask;
                },
                CancellationToken.None);
        }

        private static void PrintResult(ParserType type, string count)
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"{type} product count: {count}");
            Console.WriteLine("----------------------------------------\n");
        }
    }
}
