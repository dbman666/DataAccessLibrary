namespace Coveo.Dal
{
    public class KeyValue
    {
        [Pk] public string Key;
        public object Value;

        public KeyValue(string p_Key, object p_Value)
        {
            Key = p_Key;
            Value = p_Value;
        }
    }
}