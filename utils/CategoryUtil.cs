namespace RelewiseTest.Utils
{
    public class CategoryUtil
    {
        public static string[] SplitCategories(string input)
        {
            string normalizedInput = input.Replace(" > ", ">").Replace("> ", ">").Replace(" >", ">");

            return normalizedInput.Split('>').Select(c => c.Trim()).ToArray();
        }
    }
}
