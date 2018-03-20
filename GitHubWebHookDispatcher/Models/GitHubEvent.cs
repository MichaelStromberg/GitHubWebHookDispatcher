namespace GitHubWebHookDispatcher.Models
{
    public class GitHubEvent
    {
        // ReSharper disable InconsistentNaming
        public string compare { get; set; }
        public string @ref { get; set; }
        // ReSharper restore InconsistentNaming
    }
}
