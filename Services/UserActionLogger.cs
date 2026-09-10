using NLog;

namespace BlogApp.Services
{

    public class UserActionLogger
    {
        private static readonly Logger Logger = LogManager.GetLogger("UserActions");

        public void LogAction(string userName, string action, string? details = null)
        {
            var message = $"[{userName}] {action}";
            if (!string.IsNullOrEmpty(details))
                message += $" | {details}";

            Logger.Info(message);
        }
    }
}