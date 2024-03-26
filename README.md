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

INFO: Parsed: Id: 93, Product: en: Wireless Surround Sound System, Brand: Sonos, SalesPrice: USD: 699,99, ListPrice: USD: 799,99, CategoryPath: en: Electronics/en: Audio/en: Speakers

INFO: Parsed: Id: 94, Product: en: eBook Reader, Brand: Kindle, SalesPrice: USD: 99,99, ListPrice: USD: 119,99, CategoryPath: en: Electronics/en: Computers/en: eBook Readers

INFO: Parsed: Id: 95, Product: en: Wireless Gaming Mouse, Brand: Logitech, SalesPrice: USD: 69,99, ListPrice: USD: 79,99, CategoryPath: en: Electronics/en: Computer Accessories

INFO: Parsed: Id: 100, Product: en: Bluetooth Headset, Brand: Plantronics, SalesPrice: USD: 79,99, ListPrice: USD: 89,99, CategoryPath: en: Electronics/en: Audio/en: Headphones

...
```

Each `INFO` message contains details about a single product, including its ID, name, brand, sales price, list price, and category path. This detailed output provides a clear insight into the parsing process and the data being processed.

After all products have been parsed, the application will print the results for each parser in the following format to summarize the total number of products parsed:

`[Parser Type] product count: [Count]`

This output provides a clear understanding of how many products were parsed using each type of parser.

## Project Structure

- `Program.cs`: The main entry point of the application.
- `IJob.cs`: Interface for product parsers.
- `ProductParserFactory.cs`: Factory class for creating parser instances.
- `RawDataProductParser.cs`, `JSONDataProductParser.cs`, `GoogleShoppingFeedDataProductParser.cs`: Parser implementations.
- `Dockerfile` and `docker-compose.yml`: Configuration files for Docker.
- `Utils/CategoryUtil.cs`: Utility class for processing category strings. It provides functionality to normalize and split category strings based on a specified delimiter. This is useful for parsing hierarchical category data that may come in various string formats.
- `Utils/CurrencyUtil.cs`: Utility class for handling currency data within strings. It includes methods to remove currency symbols or ISO currency codes from price strings and convert them to a numeric format, as well as to extract the currency symbol or code from a given string. This class is essential for processing product prices that are presented in a string format with varying currency indicators.

Each utility class serves a specific function that aids in the preprocessing or handling of product data:

### Utils Folder

- `CategoryUtil.cs`
  - `SplitCategories(string input)`: Normalizes and splits a given category string into an array of categories. It ensures that the category delimiter `>` is consistently formatted and removes unnecessary spaces, making it easier to work with hierarchical category data.

- `CurrencyUtil.cs`
  - `RemoveCurrency(string input)`: Removes currency symbols or ISO currency codes from a given string, leaving only the numeric part. This method is crucial for converting price information into a decimal format that can be used for numerical operations.
  - `ExtractCurrency(string input)`: Extracts the currency symbol or ISO currency code from a given string, allowing for the identification of the currency without altering the original price string. This method is useful for scenarios where currency differentiation is needed without modifying the price data.

These utility classes enhance the application's ability to process and analyze product data by providing specialized string manipulation capabilities.