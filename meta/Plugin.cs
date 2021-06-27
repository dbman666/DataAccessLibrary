using System;

namespace Coveo.Dal
{
    public abstract class Plugin
    {
        public Meta Meta { get; }
        public Type ClrType { get; }
        public string Name { get; }
        public Type[] DependsOn { get; }
        public Type[] TablesToLoad { get; }

        protected Plugin(Meta p_Meta, string p_Name, Type[] p_DependsOn, Type[] p_TablesToLoad)
        {
            Meta = p_Meta;
            ClrType = GetType();
            Name = p_Name;
            DependsOn = p_DependsOn;
            TablesToLoad = p_TablesToLoad;
            Meta.Add(this);
        }

        public void Load(IDataProxy p_DataProxy, params Table[] p_Tables)
        {
            foreach (var table in p_Tables)
                table.Load(p_DataProxy);
        }

        public void Load(Repo p_Repo, IDataProxy p_DataProxy, params Type[] p_Types)
        {
            var nb = p_Types.Length;
            var tables = new Table[nb];
            for (var i = 0; i < nb; ++i)
                tables[i] = p_Repo.GetTable(p_Types[i]) ?? throw new DalException($"No table for type '{p_Types[i].Name}'.");
            Load(p_DataProxy, tables);
        }

        public void LoadAll(Repo p_Repo, IDataProxy p_DataProxy)
        {
            Load(p_Repo, p_DataProxy, TablesToLoad);
        }
    }
}