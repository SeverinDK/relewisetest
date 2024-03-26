using Newtonsoft.Json;
using Relewise.Client.DataTypes;
using RelewiseTest.Utils;

namespace RelewiseTest.Parsers
{
    record DeserializedProduct
    {
        [JsonConstructor]
        public DeserializedProduct(string productId, string productName, string shortDescription, string brandName, string salesPrice, string listPrice, string category, string inStock, string color)
        {
            ProductId = productId;
            ProductName = productName;
            Description = shortDescription;
            BrandName = brandName;
            SalesPrice = salesPrice;
            ListPrice = listPrice;
            CategoryPath = category;
            InStock = inStock;
            Color = color;
        }

        public string ProductId { get; }
        public string ProductName { get; }
        public string Description { get; }
        public string BrandName { get; }
        public string SalesPrice { get; }
        public string ListPrice { get; }
        public string InStock { get; }
        public string Color { get; }
        public string CategoryPath { get; }
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
            List<DeserializedProduct> deserializedProducts = JsonConvert.DeserializeObject<List<DeserializedProduct>>(json) ?? throw new FormatException("Invalid JSON data");

            double importTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            List<Product> products = [];

            foreach (DeserializedProduct deserializedProduct in deserializedProducts)
            {
                try {
                    string salesPriceCurrency = CurrencyUtil.ExtractCurrency(deserializedProduct.SalesPrice);
                    string listPriceCurrency = CurrencyUtil.ExtractCurrency(deserializedProduct.ListPrice);
                    string language = arguments.JobConfiguration["language"];
                    CategoryNameAndId[] categories = CategoryUtil.SplitCategories(deserializedProduct.CategoryPath)
                        .Select(category => new CategoryNameAndId(category, new Multilingual(language, category)))
                        .ToArray();

                    Product product = new(deserializedProduct.ProductId)
                    {
                        DisplayName = new Multilingual(new Language(language), deserializedProduct.ProductName),
                        Brand = new Brand("fake-brand-id") { DisplayName = deserializedProduct.BrandName },
                        SalesPrice = new(salesPriceCurrency, CurrencyUtil.RemoveCurrency(deserializedProduct.SalesPrice)),
                        ListPrice = new(listPriceCurrency, CurrencyUtil.RemoveCurrency(deserializedProduct.ListPrice)),
                        CategoryPaths = [new(categories)],
                        Data = new Dictionary<string, DataValue?>() {
                            { "Description", new Multilingual(new Language(language), deserializedProduct.Description) },
                            { "InStock", deserializedProduct.InStock == "in stock" },
                            { "Colors", new MultilingualCollection(language, [deserializedProduct.Color]) },
                            { "PrimaryColor", new Multilingual(language, deserializedProduct.Color) },
                            { "ImportedAt", importTimestamp }
                        }
                    };

                    products.Add(product);

                    await info($@"Parsed product:
                        - Id: {product.Id},
                        - Product: {product.DisplayName}
                        - Description: {product.Data["Description"]}
                        - Brand: {product.Brand.DisplayName}
                        - SalePrice: {product.SalesPrice}
                        - ListPrice: {product.ListPrice}
                        - Colors: {product.Data["Colors"]},
                        - PrimaryColor: {product.Data["PrimaryColor"]}
                        - InStock: {product.Data["InStock"]}
                        - CategoryPath: {product.CategoryPaths[0]}
                        - ImportedAt: {product.Data["ImportedAt"]}");
                } catch (Exception e) {
                    await warn($"Error parsing product {deserializedProduct.ProductId}: {e.Message}");

                    continue;
                }
            }

            return products;
        }
    }
}
