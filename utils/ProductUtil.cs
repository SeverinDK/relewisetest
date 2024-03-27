using Newtonsoft.Json;
using Relewise.Client.DataTypes;

namespace RelewiseTest.Utils
{
    public static class ProductUtil
    {
        public static Product MakeProduct(ProductRecord productRecord, Language language, double importTimestamp)
        {
            if (productRecord.IsValid() == false)
            {
                throw new InvalidOperationException("Product data is missing");
            }

            string salesPriceCurrency = CurrencyUtil.ExtractCurrency(productRecord.SalesPrice!);
            string listPriceCurrency = CurrencyUtil.ExtractCurrency(productRecord.ListPrice!);

            CategoryNameAndId[] categories = CategoryUtil.SplitCategories(productRecord.CategoryPath!)
                .Select(category => new CategoryNameAndId(category, new Multilingual(language, category)))
                .ToArray();

            return new Product(productRecord.ProductId!)
            {
                DisplayName = new Multilingual(language, productRecord.ProductName),
                Brand = new Brand("fake-brand-id") { DisplayName = productRecord.BrandName },
                SalesPrice = new(salesPriceCurrency, CurrencyUtil.RemoveCurrency(productRecord.SalesPrice!)),
                ListPrice = new(listPriceCurrency, CurrencyUtil.RemoveCurrency(productRecord.ListPrice!)),
                CategoryPaths = [new(categories)],
                Data = new Dictionary<string, DataValue?>() {
                    { "ShortDescription", new Multilingual(language, productRecord.Description) },
                    { "InStock", productRecord.InStock == "in stock" },
                    { "Colors", new MultilingualCollection(language, [productRecord.Color]) },
                    { "PrimaryColor", new Multilingual(language, productRecord.Color) },
                    { "ImportedAt", importTimestamp }
                }
            };
        }

        public static string SerializeProductDetails(Product product)
        {
            var description = product.Data?.TryGetValue("ShortDescription", out var descValue) == true ? descValue!.ToString() : "N/A";
            var colors = product.Data?.TryGetValue("Colors", out var colorsValue) == true ? colorsValue!.ToString() : "N/A";
            var primaryColor = product.Data?.TryGetValue("PrimaryColor", out var primaryColorValue) == true ? primaryColorValue!.ToString() : "N/A";
            var inStock = product.Data?.TryGetValue("InStock", out var inStockValue) == true ? inStockValue!.ToString() : "N/A";
            var importedAt = product.Data?.TryGetValue("ImportedAt", out var importedAtValue) == true ? importedAtValue!.ToString() : "N/A";

            var categoryPaths = product.CategoryPaths != null
                ? string.Join(" | ", product.CategoryPaths.Select(cp => cp.ToString()))
                : "Unknown";

            return $@"Parsed product:
                - Id: {product.Id},
                - DisplayName: {product.DisplayName}
                - ShortDescription: {description}
                - Brand: {product.Brand?.DisplayName ?? "Unknown"}
                - SalePrice: {product.SalesPrice}
                - ListPrice: {product.ListPrice}
                - Colors: {colors}
                - PrimaryColor: {primaryColor}
                - InStock: {inStock}
                - CategoryPaths: {categoryPaths}
                - ImportedAt: {importedAt}";
        }
    }

    public record ProductRecord
    {
        [JsonConstructor]
        public ProductRecord(
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

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(ProductId) &&
                !String.IsNullOrEmpty(ProductName) &&
                !String.IsNullOrEmpty(Description) &&
                !String.IsNullOrEmpty(BrandName) &&
                !String.IsNullOrEmpty(SalesPrice) &&
                !String.IsNullOrEmpty(ListPrice) &&
                !String.IsNullOrEmpty(InStock) &&
                !String.IsNullOrEmpty(Color) &&
                !String.IsNullOrEmpty(CategoryPath);
        }
    }
}
