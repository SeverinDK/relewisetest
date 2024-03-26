using RelewiseTest.Parsers;

namespace Relewise.Parsers
{
    public enum ParserType
    {
        RawData,
        JSONData,
        GoogleShoppingFeed
    }

    public static class ProductParserFactory
    {
        public static IJob CreateParser(ParserType parserType)
        {
            return parserType switch
            {
                ParserType.RawData => new RawDataProductParser(),
                ParserType.JSONData => new JSONDataProductParser(),
                ParserType.GoogleShoppingFeed => new GoogleShoppingFeedDataProductParser(),
                _ => throw new ArgumentException("Unknown parser type", nameof(parserType)),
            };
        }
    }
}
