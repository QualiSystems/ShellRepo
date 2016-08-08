namespace ShellRepo.Engine
{
    public interface IWebErrorLogger
    {
        void LogError(string message);
    }

    public class WebErrorLogger : IWebErrorLogger
    {
        public void LogError(string message)
        {
            new LogEvent(message).Raise();
        }
    }
}