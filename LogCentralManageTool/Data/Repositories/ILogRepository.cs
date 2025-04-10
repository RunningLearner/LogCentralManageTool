using LogCentralManageTool.Data.Entities;

namespace LogCentralManageTool.Data
{
    public interface ILogRepository
    {
        ILog GetLatestLog();
        IEnumerable<ILog> GetAllLogs();
    }
}
