namespace RelewiseTest.Utils
{
    public static class CategoryUtil
    {
        public static string[] SplitCategories(string input)
        {
            string normalizedInput = input.Replace(" > ", ">").Replace("> ", ">").Replace(" >", ">");

            return normalizedInput.Split('>').Select(c => c.Trim()).ToArray();
        }
    }
}
