using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Coveo.Dal
{
    public class UniqueNames
    {
        private ConcurrentDictionary<string, int> _namesMap;
        private List<string> _names = new List<string>();
        private int _next;
        private long _nbFinds;
        private object _lock = new Object();

        public int Count => _namesMap.Count;
        public long NbFinds => _nbFinds;
        public string this[int p_Idx] => _names[p_Idx];

        public UniqueNames(bool p_CaseInsensitive)
        {
            _namesMap = p_CaseInsensitive ? new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase) : new ConcurrentDictionary<string, int>();
        }

        public bool Contains(string p_Name)
        {
            return _namesMap.TryGetValue(p_Name, out int _);
        }

        public int Find(string p_Name, bool p_CreateIfNotFound = false)
        {
            Interlocked.Increment(ref _nbFinds);
            if (!_namesMap.TryGetValue(p_Name, out int ret)) {
                if (p_CreateIfNotFound) {
                    lock (_lock) {
                        _names.Add(p_Name);
                        return _namesMap[p_Name] = Interlocked.Increment(ref _next) - 1;
                    }
                }
                return -1;
            }
            return ret;
        }
    }
}