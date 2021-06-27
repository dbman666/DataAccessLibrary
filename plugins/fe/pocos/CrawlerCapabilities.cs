using System;

namespace fe
{
    [Flags]
    public enum CrawlerCapabilities // Encoded as an int in CrawlerInfo
    {
        CC_BEAUTIFYURI = 1,
        CC_PAUSEANDRESUME = 2,
        CC_DELETEFOLDER = 4,
        CC_DELETEDOCUMENT = 8,
        CC_REFRESHFOLDER = 16,
        CC_REFRESHDOCUMENT = 32,
        CC_LIVEMONITORING = 64,
        CC_PAUSERESUMEREFRESHSOURCE = 128,
        CC_INCREMENTALREFRESH = 256
    }
}
