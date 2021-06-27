using System;

namespace Coveo.Dal
{
    public delegate object ConvFn(object src, Type dstType, IConvContext context);

    public interface IConvContext
    {
        Repo Repo { get; }
        object Context { get; set; } // I want a JsonParser (coz it's the only need I have for now) to be instantiated only once for one mapping session, for all fields.
    }

    public static class TypeConverter
    {
        public static ConvFn FindConvFn(Type dstType, Type srcType)
        {
            if (dstType.IsEnum)
                return ConvertToEnum;
            if (dstType == typeof(DateTime))
                return ConvertToDateTime;
            if (srcType == typeof(string) && dstType.IsClass) // Let's guess this is Json. We should pass the MetaField, or the MetaField's MetaType.
                return ConvertFromJson;
            return ChangeType;
        }
        
        public static object ConvertToEnum(object src, Type dstType, IConvContext context)
        {
            if (src is string str) {
                // Either this, or look up the MetaEnum in the Meta, and call its EnumFromName overload or something.
                if (str.IndexOf('-') != -1)
                    str = str.Replace('-', '_');
                if (str.IndexOf(' ') != -1)
                    str = str.Replace(' ', '_');
                return Enum.Parse(dstType, str, true);
            }
            // This works even if src is not in the enum values. On the other hand, I think we have no situation where we want to use an int directly.
            return Enum.ToObject(dstType, src);
        }

        public static object ConvertToDateTime(object src, Type dstType, IConvContext context)
        {
            if (src is long l)
                return CmfUtil.Epoch2DateTime(l);
            if (src is int i)
                return CmfUtil.Epoch2DateTime(i);
            if (src is double d)
                return CmfUtil.Epoch2DateTime((long)d);
            if (src is string s)
                return DateTime.Parse(s);
            throw new DalException($"Can't convert {src} to epoch.");
        }

        public static object ChangeType(object src, Type dstType, IConvContext context)
        {
            return Convert.ChangeType(src, dstType);
        }

        public static object ConvertFromJson(object src, Type dstType, IConvContext context)
        {
            var str = (string)src;
            if (str == null || str == "{}" || str == "[]")
                return null;
            if (context.Context == null)
                context.Context = new JsonParserPoco<object>(context.Repo, dstType);
            return ((JsonParserPoco<object>)context.Context).Parse(str);
        }
    }
}