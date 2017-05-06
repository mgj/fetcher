﻿using artm.Fetcher.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherRepositoryService
    {
        IUrlCacheInfo GetEntryForUrl(Uri url);

        void UpdateUrl(Uri uri, IUrlCacheInfo hero, string response);

        IUrlCacheInfo InsertUrl(Uri uri, string response);

        IUrlCacheInfo PreloadUrl(Uri uri, string response);
    }
}