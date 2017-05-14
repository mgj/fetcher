using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services.Tosser
{
    public interface ITosserService
    {
        FetcherWebResponse Toss(Uri url, HttpWebRequest request);
    }
}
