# Product Data Parser

This project demonstrates the parsing of product data in various formats, including raw data, JSON, and Google Shopping Feed XML. It showcases the use of a factory pattern to abstract the creation of different types of parsers.

## Prerequisites

Before you begin, ensure you have the following installed on your system:
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

These tools are required to build and run the application in a Docker container, making it easy to set up and execute regardless of your development environment.

## Running the Application

To run the application, follow these steps:

1. Open a terminal.
2. Navigate to the root directory of this project where the `docker-compose.yml` file is located.
3. Run the following command to build and start the application:

`docker-compose up --build`

This command builds the Docker image for the application and starts a container. It will execute the application within the container, and you will see the output for each parser directly in your terminal.

The application is configured to automatically parse product data using different parsers and output the results, demonstrating the product count for each data format.

## Understanding the Output

As the application runs, it outputs detailed information for each product parsed from the data sources. For each product, you will see an informational message in the terminal with key details about the product. Here are some examples of what this output looks like:

```
...

INFO: Parsed product:
                    - Id: 94,
                    - Product: en: eBook Reader
                    - Description: Multilingual: en: E-Ink Display, Built-in Light
                    - Brand: Kindle
                    - SalePrice: USD: 99,99
                    - ListPrice: USD: 119,99,
                    - Colors: MultilingualCollection: en: White
                    - PrimaryColor: Multilingual: en: White
                    - InStock: Boolean: False
                    - CategoryPath: en: Electronics/en: Computers/en: eBook Readers
                    - ImportedAt: Double: 1711468616392
INFO: Parsed product:
                    - Id: 95,
                    - Product: en: Wireless Gaming Mouse
                    - Description: Multilingual: en: RGB Optical Gaming Mouse
                    - Brand: Logitech
                    - SalePrice: USD: 69,99
                    - ListPrice: USD: 79,99,
                    - Colors: MultilingualCollection: en: Black
                    - PrimaryColor: Multilingual: en: Black
                    - InStock: Boolean: False
                    - CategoryPath: en: Electronics/en: Computer Accessories
                    - ImportedAt: Double: 1711468616392
INFO: Parsed product:
                    - Id: 100,
                    - Product: en: Bluetooth Headset
                    - Description: Multilingual: en: Wireless Noise Cancelling Headset
                    - Brand: Plantronics
                    - SalePrice: USD: 79,99
                    - ListPrice: USD: 89,99,
                    - Colors: MultilingualCollection: en: Black
                    - PrimaryColor: Multilingual: en: Black
                    - InStock: Boolean: False
                    - CategoryPath: en: Electronics/en: Audio/en: Headphones
                    - ImportedAt: Double: 1711468616392

...
```

Each `INFO` message contains details about a single product, including its ID, name, description, brand, sales price, list price, colors, primary color, stock, category path, and import timestamp. This detailed output provides a clear insight into the parsing process and the data being processed.

After all products have been parsed, the application will print the results for each parser in the following format to summarize the total number of products parsed:

`[Parser Type] product count: [Count]`

This output provides a clear understanding of how many products were parsed using each type of parser.

## Project Structure

- `Program.cs`: The main entry point of the application.
- `Dockerfile` and `docker-compose.yml`: Configuration files for Docker.
- `Parsers/IJob.cs`: Interface for product parsers.
- `Parsers/ProductParserFactory.cs`: Factory class for creating parser instances.
- `Parsers/RawDataProductParser.cs`, `JSONDataProductParser.cs`, `GoogleShoppingFeedDataProductParser.cs`: Parser implementations.
- `Utils/CategoryUtil.cs`: Utility class for processing category strings. It provides functionality to normalize and split category strings based on a specified delimiter. This is useful for parsing hierarchical category data that may come in various string formats.
- `Utils/CurrencyUtil.cs`: Utility class for handling currency data within strings. It includes methods to remove currency symbols or ISO currency codes from price strings and convert them to a numeric format, as well as to extract the currency symbol or code from a given string. This class is essential for processing product prices that are presented in a string format with varying currency indicators.

Each utility class serves a specific function that aids in the preprocessing or handling of product data:

### Utils Folder

- `CategoryUtil.cs`
  - `SplitCategories(string input)`: Normalizes and splits a given category string into an array of categories. It ensures that the category delimiter `>` is consistently formatted and removes unnecessary spaces, making it easier to work with hierarchical category data.

- `CurrencyUtil.cs`
  - `RemoveCurrency(string input)`: Removes currency symbols or ISO currency codes from a given string, leaving only the numeric part. This method is crucial for converting price information into a decimal format that can be used for numerical operations.
  - `ExtractCurrency(string input)`: Extracts the currency symbol or ISO currency code from a given string, allowing for the identification of the currency without altering the original price string. This method is useful for scenarios where currency differentiation is needed without modifying the price data.

- `ProductUtil.cs`
  - `MakeProduct(ProductRecord productRecord, Language language, double importTimestamp)`: Creates a `Product` object from a `ProductRecord`, using specified `Language` and `importTimestamp`. It validates the `ProductRecord` for completeness, extracts and processes currency information with `CurrencyUtil`, splits category paths with `CategoryUtil`, and compiles all data into a new `Product` instance. Throws `InvalidOperationException` if the product data is missing.
  - `SerializeProductDetails(Product product)`: Serializes the details of a `Product` into a human-readable string format. It extracts information such as product ID, display name, description, brand, pricing, colors, stock status, category paths, and import timestamp. This method is useful for logging, debugging, or displaying product information in a concise format.

These utility classes enhance the application's ability to process and analyze product data by providing specialized string manipulation capabilities.
