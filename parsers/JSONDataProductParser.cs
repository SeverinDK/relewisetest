using Newtonsoft.Json;
using Relewise.Client.DataTypes;
using RelewiseTest.Utils;

namespace RelewiseTest.Parsers
{
    class ProductDeserializationDTO(string productId, string productName, string brandName, string salesPrice, string listPrice, string category)
    {
        public string ProductId { get; } = productId;
        public string ProductName { get; } = productName;
        public string BrandName { get; } = brandName;
        public string SalesPrice { get; } = salesPrice;
        public string ListPrice { get; } = listPrice;
        public string CategoryPath { get; } = category;
    }

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
            List<ProductDeserializationDTO> productDeserializationDTOs = JsonConvert.DeserializeObject<List<ProductDeserializationDTO>>(json) ?? throw new FormatException("Invalid JSON data");

            List<Product> products = [];

            foreach (ProductDeserializationDTO deserializedProduct in productDeserializationDTOs)
            {
                try {
                    string salesPriceCurrency = CurrencyUtil.ExtractCurrency(deserializedProduct.SalesPrice);
                    string listPriceCurrency = CurrencyUtil.ExtractCurrency(deserializedProduct.ListPrice);
                    string language = arguments.JobConfiguration["language"];
                    CategoryNameAndId[] categories = CategoryUtil.SplitCategories(deserializedProduct.CategoryPath).Select(category => new CategoryNameAndId(category, new Multilingual(language, category))).ToArray();

                    Product product = new(deserializedProduct.ProductId)
                    {
                        DisplayName = new Multilingual(new Language(language), deserializedProduct.ProductName),
                        Brand = new Brand("fake-brand-id") { DisplayName = deserializedProduct.BrandName },
                        SalesPrice = new(salesPriceCurrency, CurrencyUtil.RemoveCurrency(deserializedProduct.SalesPrice)),
                        ListPrice = new(listPriceCurrency, CurrencyUtil.RemoveCurrency(deserializedProduct.ListPrice)),
                        CategoryPaths = [new(categories)]
                    };

                    products.Add(product);

                    await info($"Parsed product: Id: {product.Id}, Product: {product.DisplayName}, Brand: {product.Brand.DisplayName}, SalePrice: {product.SalesPrice}, ListPrice: {product.ListPrice}, CategoryPath: {product.CategoryPaths[0]}\n");
                } catch (Exception e) {
                    await warn($"Error parsing product {deserializedProduct.ProductId}: {e.Message}");

                    continue;
                }
            }

            return products;
        }
    }
}
