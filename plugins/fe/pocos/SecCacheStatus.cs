using System;
using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("SecurityCacheInformationStatus")]
    public class SecCacheStatus
    {
        [Pk] public string Id;
        public DateTime Ts;
        public string RowState;
        public uint EntitiesCount;
        public uint EntitiesInErrorCount;
        public List<EntitiesCountPerStatePerProvider> EntitiesCountPerStatePerProvider;
        public uint PermissionModelsCount;
        public List<PermissionModelsCountForState> PermissionModelsCountPerState;
        public SecurityCachePerfMetrics PerfMetrics;

        [Computed] public NbEntitiesPerState NbEntitiesPerState;
        [Computed] public NbPermsPerState NbPermsPerState;

        [Edge_SecCache_SecCacheStatus(FkFieldName = "Id")] public SecurityCacheEntry SecCache;

        public void PostLoad()
        {
            // Numbers are a bit screwed because of dangling secprovs (remaining in the seccache, but not in the platform anymore).
            
            if (EntitiesCountPerStatePerProvider == null) {
                EntitiesCountPerStatePerProvider = new List<EntitiesCountPerStatePerProvider>();
            } else {
                foreach (var ec in EntitiesCountPerStatePerProvider)
                    ec.Transform();
                NbEntitiesPerState = ComputeSum(EntitiesCountPerStatePerProvider);
            }
            if (PermissionModelsCountPerState != null) {
                NbPermsPerState = new NbPermsPerState();
                var nbp = NbPermsPerState;
                foreach (var pc in PermissionModelsCountPerState) {
                    switch (pc.State) {
                    case PermissionModelState.Unknown:
                        nbp.Unknown += pc.Count;
                        nbp.Total += pc.Count;
                        break;
                    case PermissionModelState.Valid:
                        nbp.Valid += pc.Count;
                        nbp.Total += pc.Count;
                        break;
                    case PermissionModelState.Pending:
                        nbp.Pending += pc.Count;
                        nbp.Total += pc.Count;
                        break;
                    case PermissionModelState.Incomplete:
                        nbp.Incomplete += pc.Count;
                        nbp.Total += pc.Count;
                        break;
                    case PermissionModelState.InError:
                        nbp.InError += pc.Count;
                        nbp.Total += pc.Count;
                        break;
                    case PermissionModelState.Warning:
                        nbp.Warning += pc.Count;
                        nbp.Total += pc.Count;
                        break;
                    case PermissionModelState.Disabled:
                        nbp.Disabled += pc.Count;
                        nbp.Total += pc.Count;
                        break;
                    }
                }
                PermissionModelsCountPerState = null;
            }
        }
        
        public static NbEntitiesPerState ComputeSum(List<EntitiesCountPerStatePerProvider> statuses)
        {
            if (statuses == null || statuses.Count == 0)
                return null;
            var ret = new NbEntitiesPerState();
            foreach (var status in statuses) {
                var nbe = status.NbEntitiesPerState;
                ret.Total += nbe.Total;
                ret.Unknown += nbe.Unknown;
                ret.UpToDate += nbe.UpToDate;
                ret.NotUpdated += nbe.NotUpdated;
                ret.OutOfDate += nbe.OutOfDate;
                ret.InError += nbe.InError;
                ret.Disabled += nbe.Disabled;
            }
            return ret;
        }

        public static NbEntitiesPerState ComputeAvg(List<EntitiesCountPerStatePerProvider> statuses)
        {
            var ret = ComputeSum(statuses);
            if (ret != null) {
                var nb = (uint)statuses.Count;
                ret.Total = ret.Total / nb;
                ret.Unknown = ret.Unknown / nb;
                ret.UpToDate = ret.UpToDate / nb;
                ret.NotUpdated = ret.NotUpdated / nb;
                ret.OutOfDate = ret.OutOfDate / nb;
                ret.InError = ret.InError / nb;
                ret.Disabled = ret.Disabled / nb;
            }
            return ret;
        }
    }

    public class NbEntitiesPerState
    {
        public uint Total;
        public uint Unknown;
        public uint UpToDate;
        public uint NotUpdated;
        public uint OutOfDate;
        public uint InError;
        public uint Disabled;
    }

    public class NbPermsPerState
    {
        public uint Total;
        public uint Unknown;
        public uint Valid;
        public uint Pending;
        public uint Incomplete;
        public uint InError;
        public uint Warning;
        public uint Disabled;
    }

    public class EntitiesCountPerStatePerProvider
    {
        public string TupleType;
        public string ProviderName;
        public List<EntitiesCountForState> EntitiesCountPerState;
        [Computed] public NbEntitiesPerState NbEntitiesPerState;

        public void Transform()
        {
            if (EntitiesCountPerState != null) {
                NbEntitiesPerState = new NbEntitiesPerState();
                var nbe = NbEntitiesPerState;
                foreach (var ec in EntitiesCountPerState) {
                    switch (ec.State) {
                    case EntityState.Unknown:
                        nbe.Unknown += ec.Count;
                        nbe.Total += ec.Count;
                        break;
                    case EntityState.UpToDate:
                        nbe.UpToDate += ec.Count;
                        nbe.Total += ec.Count;
                        break;
                    case EntityState.NotUpdated:
                        nbe.NotUpdated += ec.Count;
                        nbe.Total += ec.Count;
                        break;
                    case EntityState.OutOfDate:
                        nbe.OutOfDate += ec.Count;
                        nbe.Total += ec.Count;
                        break;
                    case EntityState.InError:
                        nbe.InError += ec.Count;
                        nbe.Total += ec.Count;
                        break;
                    case EntityState.Disabled:
                        nbe.Disabled += ec.Count;
                        nbe.Total += ec.Count;
                        break;
                    }
                }
                EntitiesCountPerState = null;
            }
        }
    }

    public enum PermissionModelState
    {
        __default__ = 0,
        Unknown,
        Valid,
        Pending,
        Incomplete,
        InError,
        Warning,
        Disabled
    }

    public class PermissionModelsCountForState
    {
        public string TupleType;
        public PermissionModelState State;
        public uint Count;
    }

    public enum EntityState
    {
        __default__ = 0,
        Unknown,
        UpToDate,
        NotUpdated,
        OutOfDate,
        InError,
        Disabled
    }

    public class EntitiesCountForState
    {
        public string TupleType;
        public EntityState State;
        public uint Count;
    }

    public class SecurityCacheOperationStatus
    {
        [OperationId] public string OperationId;
        public int NumberOfCreatedJobs;
        public int NumberOfSuccessfulJobs;
        public int NumberOfFailedJobs;
        public int NumberOfProcessedSyncs;
        public int NumberOfSkippedJobs;
        public int NumberOfProcessedJobs;
        public int NumberOfCreatedSyncs;
        public string Id;
        public int Ts;
    }

    public class SecurityCachePerfMetrics
    {
        public uint GetParentEntitiesCallCount;
        public double GetParentEntitiesCallDurationAvg;
        public double GetParentEntitiesCallDurationPct;
        public uint CompletedPermissionModelsUpdateCount;
        public uint UpdatedPermissionModelsCount;
        public uint PermissionModelsUpdateNumberOfCores;
        public double GetParentEntitiesCallDurationMax;
        public double GetParentEntitiesCallNbParentsAvg;
        public double GetParentEntitiesCallNbParentsMax;
        public double GetParentEntitiesCallNbParentsPct;
    }
}