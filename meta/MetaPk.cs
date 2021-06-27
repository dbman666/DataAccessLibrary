namespace Coveo.Dal
{
    public class MetaPk
    {
        public MetaField Field { get; }
        public bool PkIsDependent { get; private set; }

        public MetaPk(MetaField p_Field, bool p_PkIsDependent)
        {
            Field = p_Field;
            PkIsDependent = p_PkIsDependent;
            // LimitHistory has a date. SourceExtension.Id is an int.
            Field._flags |= FieldFlags.IsPk;
            //    if (!(field.FieldType is StringType)) throw new DalException($"MetaPk.ctor: Expected pk {field.MetaClass.TypeName}.{field.Name} to be a string.");
        }
    }
}