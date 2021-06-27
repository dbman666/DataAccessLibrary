using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coveo.Dal
{
    public static class SvcManager
    {
        public static Dictionary<string, SvcDefinition> _services = new Dictionary<string, SvcDefinition>(StringComparer.InvariantCultureIgnoreCase);

        public static void Register(SvcDefinition definition)
        {
            _services[definition.Name] = definition;
        }

        public static SvcDefinition Find(string name)
        {
            return _services.TryGetValue(name, out var def) ? def : null;
        }

        public static string ParseAndExecute(Stream stream, SvcLogger logger, SvcConfig config)
        {
            var str = new StreamReader(stream).ReadToEnd();
            if (string.IsNullOrWhiteSpace(str))
                return null;
            var svc = JsonConvert.DeserializeObject<SvcBase>(str, new SvcArgsConverter(logger, config));
            return svc?.Run();
        }

        public static void ForceSvcRegistrations()
        {
            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
                if (type.IsSubclassOf(typeof(SvcBase)))
                    RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }

    public class SvcDefinition
    {
        public string Name;
        public Type TypeService;
        public Type TypeConfig;
        public Type TypeArgs;
    }
    
    public class SvcArgsConverter : JsonConverter
    {
        private SvcLogger _logger;
        private SvcConfig _config;

        public SvcArgsConverter(SvcLogger logger, SvcConfig config)
        {
            _logger = logger;
            _config = config;
        }
        
        public override bool CanConvert(Type objectType) => typeof(SvcBase).IsAssignableFrom(objectType);

#pragma warning disable 8632
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, JsonSerializer serializer)
#pragma warning restore 8632
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var root = JObject.Load(reader);
            var strType = root.TryGetValue("type", StringComparison.OrdinalIgnoreCase, out var token) ? token.Value<string>() : throw new Exception("No 'type' was specified. Don't know what to execute.");
            var svcDef = SvcManager.Find(strType);
            if (svcDef == null)
                throw new Exception($"Invalid type '{strType}'.");
            var args = (SvcBase.ArgsBase)root.ToObject(svcDef.TypeArgs);
            var ctor = svcDef.TypeService.GetConstructor(new[] {typeof(SvcLogger), svcDef.TypeConfig, svcDef.TypeArgs});
            if (ctor == null)
                throw new Exception($"Expected to find '{svcDef.TypeService.Name}(Logger, Config, {svcDef.TypeArgs.Name})'.");
            var svc = ctor.Invoke(new object[] {_logger, _config, args});
            if (svc is SvcBase svcBase)
                svcBase.Definition = svcDef;
            return svc;
        }

        public override bool CanWrite => false;
    }
}