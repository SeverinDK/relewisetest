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

            IEnumerable<Task<Product?>> tasks = items.Select(async product => {
                string? id = product.Element(gNamespace + "id")?.Value;
                string? brand = product.Element(gNamespace + "brand")?.Value;
                string? title = product.Element("title")?.Value;
                string? salePrice = product.Element(gNamespace + "sale_price")?.Value;
                string? listPrice = product.Element(gNamespace + "price")?.Value;
                string? categoryPath = product.Element(gNamespace + "product_type")?.Value;

                if (String.IsNullOrEmpty(id) ||
                    String.IsNullOrEmpty(brand) ||
                    String.IsNullOrEmpty(title) ||
                    String.IsNullOrEmpty(salePrice) ||
                    String.IsNullOrEmpty(listPrice) ||
                    String.IsNullOrEmpty(categoryPath))
                {
                    await warn($"Skipping product {id} with missing data");

                    return null;
                }

                string salesPriceCurrency = CurrencyUtil.ExtractCurrency(salePrice);
                string listPriceCurrency = CurrencyUtil.ExtractCurrency(listPrice);
                string language = arguments.JobConfiguration["language"];
                CategoryNameAndId[] categories = CategoryUtil.SplitCategories(categoryPath)
                    .Select(category => new CategoryNameAndId(category, new Multilingual(language, category)))
                    .ToArray();

                Product newProduct = new(id)
                {
                    DisplayName = new Multilingual(new Language(language), title),
                    Brand = new Brand("fake-brand-id") { DisplayName = brand },
                    SalesPrice = new MultiCurrency(salesPriceCurrency, CurrencyUtil.RemoveCurrency(salePrice)),
                    ListPrice = new MultiCurrency(listPriceCurrency, CurrencyUtil.RemoveCurrency(listPrice)),
                    CategoryPaths = [new(categories)]
                };

                await info($"Parsed: Id: {newProduct.Id}, Product: {newProduct.DisplayName}, Brand: {newProduct.Brand.DisplayName}, SalesPrice: {newProduct.SalesPrice}, ListPrice: {newProduct.ListPrice}, CategoryPath: {newProduct.CategoryPaths[0]}\n");

                return newProduct;
            });

            Product?[] products = await Task.WhenAll(tasks);
            return products.Where(product => product != null).ToList()!;
        }
    }
}
