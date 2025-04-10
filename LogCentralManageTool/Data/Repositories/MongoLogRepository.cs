using LogCentralManageTool.Data.Entities;

namespace LogCentralManageTool.Data.Repositories;
public class MongoLogRepository : ILogRepository
{
    private readonly MongoLoggingDbContext _context;

    public MongoLogRepository(MongoLoggingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ILog GetLatestLog()
    {
        return _context.Logs.OrderByDescending(l => l.Timestamp).FirstOrDefault();
    }

    public IEnumerable<ILog> GetAllLogs()
    {
        return _context.Logs.ToList();
    }
}

