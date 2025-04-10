using LogCentralManageTool.Data.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogCentralManageTool.Data.Repositories;
public class MySQLLogRepository : ILogRepository
{
    private readonly MySQLLoggingDbContext _context;

    public MySQLLogRepository(MySQLLoggingDbContext context)
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
