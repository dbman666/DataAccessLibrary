namespace fe
{
    public enum SourceTaskDtype
    {
        CheckProvisioningTask,
        CreateSourceTask,
        UpdateSourceTask,
        DeleteSourceTask,
        RebuildSourceTask,
        FullRefreshSourceTask,
        IncrementalRefreshSourceTask,
        CancelRefreshTask,
        PauseSourceTask,
        ResumeSourceTask,
        ResumeAllSourcesTask,
        ChangePushSourceStateTask
    }
}
