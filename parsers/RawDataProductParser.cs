using Relewise.Client.DataTypes;
using RelewiseTest.Exceptions;
using RelewiseTest.Utils;

namespace RelewiseTest.Parsers
{
    public class RawDataProductParser : IJob
    {
        public async Task<string> Execute(
            JobArguments arguments,
            Func<string, Task> info,
            Func<string, Task> warn,
            CancellationToken token)
        {
            await info("Starting raw data parsing...");

            using HttpClient client = new();
            try {
                string response = await client.GetStringAsync(arguments.JobConfiguration["url"], token);
                token.ThrowIfCancellationRequested();

                List<Product> products = await ParseData(response, arguments, info, warn);

                await info("Finished raw data parsing!");

                return products.Count.ToString();
            } catch (OperationCanceledException e) {
                await warn($"Operation was cancelled\nMessage :{e.Message}");

                return e.Message;
            } catch (Exception e) {
                await warn($"Exception Caught!\nMessage :{e.Message}");

                return e.Message;
            }
        }

        private async static Task<List<Product>> ParseData(string rawData, JobArguments arguments, Func<string, Task> info, Func<string, Task> warn)
        {
            List<Product> products = [];

            // Split the raw data into lines and skip the first two lines (header)
            string[] lines = rawData.Split(Environment.NewLine).Skip(2).ToArray();

            for (int i = 0; i < lines.Length; i++)
            {
                try {
                    string[] data = SplitAndTrimLine(lines[i]);
                    ValidateDataSize(data, 9);

                    string id = data[0];
                    string productName = data[1];
                    string brandName = data[5];
                    string salesPrice = data[3];
                    string listPrice = data[4];

                    if (String.IsNullOrEmpty(id) ||
                        String.IsNullOrEmpty(productName) ||
                        String.IsNullOrEmpty(brandName) ||
                        String.IsNullOrEmpty(salesPrice) ||
                        String.IsNullOrEmpty(listPrice))
                    {
                        await warn($"Skipping product {id} with missing data");

                        continue;
                    }

                    string salesPriceCurrency = CurrencyUtil.ExtractCurrency(salesPrice);
                    string listPriceCurrency = CurrencyUtil.ExtractCurrency(listPrice);
                    string language = arguments.JobConfiguration["language"];
                    CategoryNameAndId[] categories = CategoryUtil.SplitCategories(data[8])
                        .Select(category => new CategoryNameAndId(category, new Multilingual(language, category)))
                        .ToArray();

                    Product product = new(id)
                    {
                        DisplayName = new Multilingual(new Language(language), productName),
                        Brand = new Brand("fake-brand-id") { DisplayName = brandName },
                        SalesPrice = new(salesPriceCurrency, CurrencyUtil.RemoveCurrency(salesPrice)),
                        ListPrice = new(listPriceCurrency, CurrencyUtil.RemoveCurrency(listPrice)),
                        CategoryPaths = [new(categories)]
                    };

                    products.Add(product);

                    await info($"Parsed product: Id: {product.Id}, Product: {product.DisplayName}, Brand: {product.Brand.DisplayName}, SalePrice: {product.SalesPrice}, ListPrice: {product.ListPrice}, CategoryPath: {product.CategoryPaths[0]}\n");
                } catch (Exception e) {
                    await warn($"Error parsing product on line {i}: {e.Message}");
                    continue;
                }
            }

            return products;
        }

        private static string[] SplitAndTrimLine(string line, char delimiter = '|')
        {
            return line.Trim().TrimStart(delimiter).TrimEnd(delimiter).Split(delimiter)
                    .Select(column => column.Trim()).ToArray();
        }

        private static void ValidateDataSize(string[] data, int expectedSize)
        {
            if (data.Length != expectedSize) {
                throw new DataSizeValidationException(data.Length, expectedSize);
            }
        }
    }
}
