using System;
using Amazon.Lambda.Core;

namespace Coveo.Dal
{
    public class SvcLogger
    {
        public string LogPrefix { get; set; } = "";

        private ILambdaContext _context;
        private int _i;

        public SvcLogger(ILambdaContext context)
        {
            _context = context;
        }
        
        public void Log(string msg)
        {
            var line = $"{LogPrefix.Replace("$I$", _i.ToString("D4"))}{msg}";
            ++_i;
            if (_context == null) {
                Console.WriteLine(line);
            } else {
                _context.Logger.LogLine($"{{\"@timestamp\":\"{DateTime.UtcNow.ToIsoWithMillis()}\", \"message\":\"{line}\"}}");
            }
        }

        public void Log(Exception exc) => Log("Exception: " + exc.Message);
    }

    public class LogPrefix : IDisposable
    {
        private SvcLogger _logger;
        private string _oldPrefix;
        
        public LogPrefix(SvcLogger logger, string newPrefix)
        {
            _logger = logger;
            _oldPrefix = _logger.LogPrefix;
            _logger.LogPrefix = newPrefix;
        }

        public void Dispose()
        {
            _logger.LogPrefix = _oldPrefix;
        }
    }
}
