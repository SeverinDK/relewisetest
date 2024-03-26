using RelewiseTest.Parsers;

namespace RelewiseTest
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            string rawProductData = await ParseRawData();
            string JSONProductData = await ParseJSONData();
            string googleShoppingFeedData = await ParseGoogleShoppingFeedXML();

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"RawData product count: {rawProductData}");
            Console.WriteLine("----------------------------------------\n");

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"JSON product count: {JSONProductData}");
            Console.WriteLine("----------------------------------------\n");

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"GoogleShoppingFeed XML product count: {googleShoppingFeedData}");
            Console.WriteLine("----------------------------------------\n");
        }

        private static async Task<string> ParseRawData()
        {
            RawDataProductParser parser = new();

            return await parser.Execute(
                new JobArguments(
                    Guid.NewGuid(),
                    "NO-NEED-4-API-KEY",
                    new Dictionary<string, string>{
                        { "url", "https://cdn.relewise.com/academy/productdata/raw" },
                        { "language", "EN" }
                    }),
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

        private static async Task<string> ParseJSONData()
        {
            JSONDataProductParser job = new();

            return await job.Execute(
                new JobArguments(
                    Guid.NewGuid(),
                    "NO-NEED-4-API-KEY",
                    new Dictionary<string, string>{
                        { "url", "https://cdn.relewise.com/academy/productdata/customjsonfeed" },
                        { "language", "EN" }
                    }),
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

        private static async Task<string> ParseGoogleShoppingFeedXML()
        {
            GoogleShoppingFeedDataProductParser job = new();

            return await job.Execute(
                new JobArguments(
                    Guid.NewGuid(),
                    "NO-NEED-4-API-KEY",
                    new Dictionary<string, string>{
                        { "url", "https://cdn.relewise.com/academy/productdata/googleshoppingfeed" },
                        { "language", "EN" }
                    }),
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
    }
}
