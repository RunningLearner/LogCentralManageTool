using LogCentralManageTool.Data.Entities;

namespace LogCentralManageTool.Data
{
    public class LogRepository
    {
        private readonly LoggingDbContext _context;

        public LogRepository(LoggingDbContext context)
        {
            _context = context;
        }

        public Log GetLatestLog()
        {
            return _context.Set<Log>()
                           .OrderByDescending(l => l.Timestamp)
                           .FirstOrDefault();
        }
    }
}
