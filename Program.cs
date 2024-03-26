using Relewise.Parsers;
using RelewiseTest.Parsers;

namespace RelewiseTest
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            string rawData = await ParseData(ParserType.RawData, new Dictionary<string, string>
            {
                { "url", "https://cdn.relewise.com/academy/productdata/raw" },
                { "language", "en" }
            }, "NO-NEED-4-API-KEY");

            string jsonData = await ParseData(ParserType.JSONData, new Dictionary<string, string>
            {
                { "url", "https://cdn.relewise.com/academy/productdata/customjsonfeed" },
                { "language", "en" }
            }, "NO-NEED-4-API-KEY");

            string googleShoppingFeedData = await ParseData(ParserType.GoogleShoppingFeed, new Dictionary<string, string>
            {
                { "url", "https://cdn.relewise.com/academy/productdata/googleshoppingfeed" },
                { "language", "en" }
            }, "NO-NEED-4-API-KEY");

            PrintResult("RawData", rawData);
            PrintResult("JSONData", jsonData);
            PrintResult("GoogleShoppingFeed", googleShoppingFeedData);
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

        private static void PrintResult(string dataType, string data)
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"{dataType} product count: {data}");
            Console.WriteLine("----------------------------------------\n");
        }
    }
}
