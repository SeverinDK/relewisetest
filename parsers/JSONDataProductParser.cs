using Newtonsoft.Json;
using Relewise.Client.DataTypes;
using RelewiseTest.Utils;

namespace RelewiseTest.Parsers
{
    public class JSONDataProductParser : IJob
    {
        public async Task<string> Execute(
            JobArguments arguments,
            Func<string, Task> info,
            Func<string, Task> warn,
            CancellationToken token)
        {
            await info("Starting JSON data parsing...");

            using HttpClient client = new();
            try {
                string response = await client.GetStringAsync(arguments.JobConfiguration["url"], token);
                token.ThrowIfCancellationRequested();

                List<Product> products = await ParseData(response, arguments, info, warn);

                await info("Finished JSON data parsing!");

                return products.Count.ToString();
            } catch (InvalidOperationException e) {
                await warn($"Product is missing data. Skipping...\n");

                return e.Message;
            } catch (OperationCanceledException e) {
                await warn($"Operation was cancelled\nMessage :{e.Message}");

                return e.Message;
            } catch (Exception e) {
                await warn($"Exception Caught!\nMessage :{e.Message}");

                return e.Message;
            }
        }

        private async static Task<List<Product>> ParseData(string json, JobArguments arguments, Func<string, Task> info, Func<string, Task> warn)
        {
            List<ProductRecord> deserializedProducts = JsonConvert.DeserializeObject<List<ProductRecord>>(json) ?? throw new FormatException("Invalid JSON data");

            double importTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            List<Product> products = [];

            foreach (ProductRecord deserializedProduct in deserializedProducts)
            {
                try {
                    Product product = deserializedProduct.MakeProduct(new Language(arguments.JobConfiguration["language"]), importTimestamp);

                    products.Add(product);

                    await info(ProductUtil.SerializeProductDetails(product));
                } catch (Exception e) {
                    await warn($"Error parsing product {deserializedProduct.ProductId}: {e.Message}");

                    continue;
                }
            }

            return products;
        }
    }
}
