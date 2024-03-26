namespace RelewiseTest.Exceptions
{
    public class DataSizeValidationException(int actualSize, int expectedSize) : Exception($"Invalid data size: {actualSize}, expected: {expectedSize}.")
    {
        public int ActualSize { get; } = actualSize;
        public int ExpectedSize { get; } = expectedSize;
    }
}
