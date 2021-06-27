using System;
using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("IndexStatus")]
    public class IndexStatus
    {
        [IndexId] [Pk] public string Id;
        public DateTime Ts;
        public string RowState;
        public uint PendingPreTransactions;
        public uint PendingTransactions;
        public ulong DiskSpaceUsed;
        public ulong RemainingDiskSpace;
        public MemoryBreakdown TotalMemoryUsed;
        public uint DocumentCount;
        public ulong DocumentTotalSize;
        public uint PendingDocsToAdd;
        public uint PendingDocsToUpdate;
        public uint PendingDocsToDelete;
        public uint VisibilityDelay;
        public uint RealtimePendingPreTransactions;
        public uint RealtimePendingTransactions;
        public ulong RealtimeDiskSpaceUsed;
        public MemoryBreakdown RealtimeTotalMemoryUsed;
        public uint RealtimeDocumentCount;
        public ulong RealtimeDocumentTotalSize;
        public uint RealtimePendingDocsToAdd;
        public uint RealtimePendingDocsToUpdate;
        public uint RealtimePendingDocsToDelete;
        public uint RealtimeVisibilityDelay;
        public uint FragmentationLevel;
        public DateTime? LastCommit;
        public List<IndexSourceStatus> Sources;
        public List<IndexSliceStatus> Slices;
        public ulong ResidentSetSize;
        public ulong VirtualMemorySize;
        public ulong PeakResidentSetSize;
        public ulong PeakVirtualMemorySize;
        public ulong TotalPhysicalMemory;
        public ulong TotalDiskSpace;
        public ulong TotalOcrPages;
        public double DocumentsFragmentation;
        
        // Es
        public ShardsInfo Shards;
        public int Segments;
        public int Replicas;
        public int IndexTime;
        public int FlushTime;
        public DateTime LastMessageProcessed;
        public DateTime LastDocumentInserted;
        public string ErrorCode;
        public string ErrorMessage;

        [Edge_Index_IndexStatus(FkFieldName = "Id")] public Index Index;
        
        public static IndexSourceStatus ComputeAvg(List<IndexSourceStatus> statuses)
        {
            if (statuses == null || statuses.Count == 0)
                return null;
            var ret = new IndexSourceStatus();
            var nb = (uint)statuses.Count;
            foreach (var status in statuses) {
                ret.DocumentCount += status.DocumentCount;
                ret.DocumentTotalSize += status.DocumentTotalSize;
                ret.PendingDocsToAdd += status.PendingDocsToAdd;
                ret.PendingDocsToUpdate += status.PendingDocsToUpdate;
                ret.PendingDocsToDelete += status.PendingDocsToDelete;
            }
            ret.DocumentCount = ret.DocumentCount / nb;
            ret.DocumentTotalSize = ret.DocumentTotalSize / nb;
            ret.PendingDocsToAdd = ret.PendingDocsToAdd / nb;
            ret.PendingDocsToUpdate = ret.PendingDocsToUpdate / nb;
            ret.PendingDocsToDelete = ret.PendingDocsToDelete / nb;
            return ret;
        }
    }
    
    public class MemoryBreakdown
    {
        public string TupleType;
        public ulong FacetLookupCache;
        public ulong ExpressionCache;
        public ulong DocumentsCache;
        public ulong TransactionWriter;
        public ulong TransactionOptimizer;
        public ulong TransactionReader;
        public LexiconMemoryBreakdown Lexicon;
        public ulong AuthorizationManager;
        public ulong IndexedDocuments;
        public ulong DocumentsRatings;
        public ulong Collections;
        public ulong FileSecurity;
        public ulong Ranking;
        public ulong Total;
    }
    
    public class LexiconMemoryBreakdown
    {
        public string TupleType;
        public ulong BTreeCaches;
        public ulong FacetsCache;
        public ulong TermsCache;
        public ulong TermIDsCache;
        public ulong SortCacheNumFields;
        public ulong SortCacheStringFields;
        public ulong SortStringTable;
        public ulong EvaluatorLongFields;
        public ulong EvaluatorLong64Fields;
        public ulong EvaluatorDateFields;
        public ulong EvaluatorDoubleFields;
        public ulong WordCorrectorLexicon;
        public ulong Facets;
        public ulong StemExpansionMap;
        public ulong Total;
    }
    
    public class IndexSourceStatus
    {
        public uint CollectionId;
        public uint SourceId;
        public uint DocumentCount;
        public ulong DocumentTotalSize;
        public uint PendingDocsToAdd;
        public uint PendingDocsToUpdate;
        public uint PendingDocsToDelete;
    }
    
    public class IndexSliceStatus
    {
        public string TupleType;
        public uint SliceId;
        public DateTime LastTransactionsApplication;
        public uint VisibilityDelay;
        public double DocumentsFragmentation;
    }

    public class ShardsInfo
    {
        public int Total;
        public int Successful;
        public int Failed;
    }
}