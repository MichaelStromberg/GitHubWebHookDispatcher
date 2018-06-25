namespace GitHubWebHook
{
    public class AnalysisConfiguration
    {
        public readonly string ScriptPath;
        public readonly string Branch;
        public readonly string ShortBranch;
        public readonly string Key;

        public AnalysisConfiguration(string scriptPath, string branch, string shortBranch, string key)
        {
            ScriptPath  = scriptPath;
            Branch      = branch;
            ShortBranch = shortBranch;
            Key         = key;
        }
    }
}
