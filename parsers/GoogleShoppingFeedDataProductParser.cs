using System.Xml.Linq;
using Relewise.Client.DataTypes;
using RelewiseTest.Utils;

namespace RelewiseTest.Parsers
{
    public class GoogleShoppingFeedDataProductParser : IJob
    {
        public async Task<string> Execute(
            JobArguments arguments,
            Func<string, Task> info,
            Func<string, Task> warn,
            CancellationToken token)
        {
            await info("Starting Google Shopping Feed data parsing...");

            using HttpClient client = new();
            try {
                string response = await client.GetStringAsync(arguments.JobConfiguration["url"], token);
                token.ThrowIfCancellationRequested();

                List<Product> products = await ParseData(response, arguments, info, warn);

                await info("Finished Google Shopping Feed data parsing!");

                return products.Count.ToString();
            } catch (OperationCanceledException e) {
                await warn($"Operation was cancelled\nMessage :{e.Message}");

                return e.Message;
            } catch (Exception e) {
                await warn($"Exception Caught!\nMessage :{e.Message}");

                return e.Message;
            }
        }

        private async static Task<List<Product>> ParseData(string xml, JobArguments arguments, Func<string, Task> info, Func<string, Task> warn)
        {
            XNamespace gNamespace = "http://base.google.com/ns/1.0";
            IEnumerable<XElement>? items = XDocument.Parse(xml)?.Root?.Element("channel")?.Elements("item") ?? throw new FormatException("Invalid XML data");

	        double importTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();


            IEnumerable<Task<Product?>> tasks = items.Select(async item => {
                try {
                    string? id = item.Element(gNamespace + "id")?.Value;
                    string? brand = item.Element(gNamespace + "brand")?.Value;
                    string? title = item.Element("title")?.Value;
                    string? description = item.Element("description")?.Value;
                    string? salePrice = item.Element(gNamespace + "sale_price")?.Value;
                    string? listPrice = item.Element(gNamespace + "price")?.Value;
                    string? availability = item.Element(gNamespace + "availability")?.Value;
                    string? color = item.Element(gNamespace + "color")?.Value;
                    string? categoryPath = item.Element(gNamespace + "product_type")?.Value;

                    ProductRecord productRecord = new(
                        id,
                        title,
                        description,
                        brand,
                        salePrice,
                        listPrice,
                        categoryPath,
                        availability,
                        color);

                    Product product = productRecord.MakeProduct(new Language(arguments.JobConfiguration["language"]), importTimestamp);

                    await info(ProductUtil.SerializeProductDetails(product));

                    return product;
                } catch (Exception e) {
                    await warn($"Error parsing product {item.Element(gNamespace + "id")?.Value}: {e.Message}");

                    return null;
                }
            });

            Product?[] products = await Task.WhenAll(tasks);
            return products.Where(product => product != null).ToList()!;
        }
    }
}
