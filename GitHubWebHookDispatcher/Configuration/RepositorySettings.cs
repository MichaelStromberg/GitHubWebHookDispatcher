using System.Collections.Generic;

namespace GitHubWebHookDispatcher.Configuration
{
    public class RepositorySettings
    {
        public List<ScriptRepositoryPair> Scripts { get; set; }
    }
}
