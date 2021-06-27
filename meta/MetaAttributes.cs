using System;

namespace Coveo.Dal
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class EnumAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CssColorAttribute : Attribute
    {
        public string Color;

        public CssColorAttribute(string p_Color)
        {
            Color = p_Color;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PkAttribute : Attribute
    {
        public bool IsDependent;

        public PkAttribute(bool p_IsDependent = false)
        {
            IsDependent = p_IsDependent;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ComputedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DontKeepTypeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EdgeAttribute : Attribute
    {
        public string FkFieldName;

        public EdgeAttribute()
        {
        }

        public EdgeAttribute(string p_FkFieldName)
        {
            FkFieldName = p_FkFieldName;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class IsNotUsefulAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DomainAttribute : Attribute
    {
        public Type Type;
        
        public DomainAttribute(Type p_Type)
        {
            Type = p_Type;
        }
    }
    public class IntAttribute : DomainAttribute
    {
        public IntAttribute() : base(typeof(int))
        {
        }
    }

    public class StringAttribute : DomainAttribute
    {
        public StringAttribute() : base(typeof(string))
        {
        }
    }

    public class JsonAttribute : StringAttribute
    {
    }

    public class TableAttribute : Attribute
    {
        public string Name;
        public bool UseSelectStar = true;
        
        public TableAttribute(string p_Name)
        {
            Name = p_Name;
        }
    }
}