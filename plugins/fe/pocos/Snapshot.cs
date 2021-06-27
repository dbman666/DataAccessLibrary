using System;

namespace fe
{
    public class Snapshot
    {
        public string id;
        public string key;
        public DateTime createdDate;
        public string snapshot;

        public override string ToString()
        {
            return $"Key {key} @ {createdDate} ({id})";
        }
    }
}