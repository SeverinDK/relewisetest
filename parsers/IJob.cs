namespace RelewiseTest.Parsers
{
    public interface IJob
    {
        Task<string> Execute(
            JobArguments arguments,
            Func<string, Task> info,
            Func<string, Task> warn,
            CancellationToken token);
    }

    public class JobArguments
    {
        public JobArguments(
            Guid datasetId,
            string apiKey,
            IReadOnlyDictionary<string, string> jobConfiguration)
        {
            DatasetId = datasetId;
            ApiKey = apiKey;
            JobConfiguration = jobConfiguration;
        }

        public Guid DatasetId { get; }
        public string ApiKey { get; }

        public IReadOnlyDictionary<string, string> JobConfiguration { get; }
    }
}
