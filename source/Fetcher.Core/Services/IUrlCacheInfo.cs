using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public interface IUrlCacheInfo
    {
        string Response { get; set; }

        string Url { get; set; }

        DateTimeOffset Created { get; set; }

        DateTimeOffset LastAccessed { get; set; }

        DateTimeOffset LastUpdated { get; set; }
    }
}
