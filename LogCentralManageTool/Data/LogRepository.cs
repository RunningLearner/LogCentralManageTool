using LogCentralManageTool.Data.Entities;

namespace LogCentralManageTool.Data
{
    public class LogRepository
    {
        private readonly MySQLLoggingDbContext _context;

        public LogRepository(MySQLLoggingDbContext context)
        {
            _context = context;
        }

        public LogMySQL GetLatestLog()
        {
            return _context.Set<LogMySQL>()
                           .OrderByDescending(l => l.Timestamp)
                           .FirstOrDefault();
        }
    }
}
