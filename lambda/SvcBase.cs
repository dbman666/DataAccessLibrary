using System;

namespace Coveo.Dal
{
    public class SvcBase
    {
        public SvcDefinition Definition { get; internal set; }
        public SvcLogger Logger { get; private set; }
        public SvcConfig Config { get; private set; }
        
        protected string _dateStr = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        
        public class ArgsBase
        {
            public string Type;
        }

        public virtual string Run()
        {
            throw new NotImplementedException("SvcBase.Run");
        }

        public SvcBase(SvcLogger logger, SvcConfig config)
        {
            Logger = logger;
            Config = config;
        }
        
        public void Try(Action fn, string message)
        {
            Logger.Log(message);
            try {
                fn();
            } catch (Exception exc) {
                Logger.Log($"Exception in {message} -> {exc.Message}");
                throw;
            }
        }
    }
}