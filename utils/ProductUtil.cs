using Relewise.Client.DataTypes;

namespace RelewiseTest.Utils
{
    public static class ProductUtil
    {
        public static string Dump(Product product)
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
}
