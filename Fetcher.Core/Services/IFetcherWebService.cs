﻿using artm.Fetcher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherWebService
    {
        FetcherWebResponse DoPlatformWebRequest(Uri uri);
    }
}