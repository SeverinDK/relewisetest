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

            double importTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            for (int i = 0; i < lines.Length; i++)
            {
                try {
                    string[] data = SplitAndTrimLine(lines[i]);
                    ValidateDataSize(data, 9);

                    string id = data[0];
                    string productName = data[1];
                    string brandName = data[2];
                    string salesPrice = data[3];
                    string listPrice = data[4];
                    string description = data[5];
                    string inStock = data[6];
                    string color = data[7];
                    string categoryPath = data[8];

                    ProductRecord productRecord = new(
                        id,
                        productName,
                        description,
                        brandName,
                        salesPrice,
                        listPrice,
                        categoryPath,
                        inStock,
                        color);

                    Product product = productRecord.MakeProduct(new Language(arguments.JobConfiguration["language"]), importTimestamp);

                    products.Add(product);

                    await info(ProductUtil.SerializeProductDetails(product));
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
