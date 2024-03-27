using Newtonsoft.Json;
using Relewise.Client.DataTypes;
using RelewiseTest.Utils;

namespace RelewiseTest.Parsers
{
    record DeserializedProduct
    {
        [JsonConstructor]
        public DeserializedProduct(
            string? productId,
            string? productName,
            string? shortDescription,
            string? brandName,
            string? salesPrice,
            string? listPrice,
            string? category,
            string? inStock,
            string? color)
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

        public string? ProductId { get; }
        public string? ProductName { get; }
        public string? Description { get; }
        public string? BrandName { get; }
        public string? SalesPrice { get; }
        public string? ListPrice { get; }
        public string? InStock { get; }
        public string? Color { get; }
        public string? CategoryPath { get; }
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
                    string? id = deserializedProduct.ProductId;
                    string? productName = deserializedProduct.ProductName;
                    string? description = deserializedProduct.Description;
                    string? brandName = deserializedProduct.BrandName;
                    string? salesPrice = deserializedProduct.SalesPrice;
                    string? listPrice = deserializedProduct.ListPrice;
                    string? inStock = deserializedProduct.InStock;
                    string? color = deserializedProduct.Color;
                    string? categoryPath = deserializedProduct.CategoryPath;

                    if (String.IsNullOrEmpty(id) ||
                        String.IsNullOrEmpty(productName) ||
                        String.IsNullOrEmpty(description) ||
                        String.IsNullOrEmpty(brandName) ||
                        String.IsNullOrEmpty(salesPrice) ||
                        String.IsNullOrEmpty(listPrice) ||
                        String.IsNullOrEmpty(inStock) ||
                        String.IsNullOrEmpty(color) ||
                        String.IsNullOrEmpty(categoryPath))
                    {
                        await warn($"Skipping product {id} with missing data");

                        continue;
                    }

                    string salesPriceCurrency = CurrencyUtil.ExtractCurrency(salesPrice);
                    string listPriceCurrency = CurrencyUtil.ExtractCurrency(listPrice);
                    string language = arguments.JobConfiguration["language"];
                    CategoryNameAndId[] categories = CategoryUtil.SplitCategories(categoryPath)
                        .Select(category => new CategoryNameAndId(category, new Multilingual(language, category)))
                        .ToArray();

                    Product product = new(id)
                    {
                        DisplayName = new Multilingual(new Language(language), productName),
                        Brand = new Brand("fake-brand-id") { DisplayName = brandName },
                        SalesPrice = new(salesPriceCurrency, CurrencyUtil.RemoveCurrency(salesPrice)),
                        ListPrice = new(listPriceCurrency, CurrencyUtil.RemoveCurrency(listPrice)),
                        CategoryPaths = [new(categories)],
                        Data = new Dictionary<string, DataValue?>() {
                            { "ShortDescription", new Multilingual(new Language(language), description) },
                            { "InStock", inStock == "in stock" },
                            { "Colors", new MultilingualCollection(language, [color]) },
                            { "PrimaryColor", new Multilingual(language, color) },
                            { "ImportedAt", importTimestamp }
                        }
                    };

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
