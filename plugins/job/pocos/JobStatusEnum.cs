namespace job
{
    public enum JobStatusEnum
    {
        Delayed,
        Ready,
        Starting,
        TaskRunning,
        Running,
        Done,
        Error,
        Interrupting,
        Interrupted
    }
}
