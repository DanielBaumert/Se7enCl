namespace Se7enCl
{
    public enum CommandExecutionStatus : uint
    {
        Complete = 0x0,
        Running = 0x1,
        Submitted = 0x2,
        Queued = 0x3
    }
}