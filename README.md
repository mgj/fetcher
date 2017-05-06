Fetcher
==========

[![Build status](https://ci.appveyor.com/api/projects/status/iysnpswp82ogp4vb?svg=true)](https://ci.appveyor.com/project/mgj/fetcher)

| Plugin          | NuGet version                                                                                                                                                              |
| --------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Fetcher        | [![NuGet](https://img.shields.io/nuget/v/artm.fetcher.svg)](https://www.nuget.org/packages/artm.fetcher/)             |

Example
==========
```
IFetcherRepositoryStoragePathService path = new FetcherRepositoryStoragePathService();
IFetcherRepositoryService repository = new FetcherRepositoryService(path);
IFetcherWebService web = new FetcherWebService();

// Primary interface you should use
IFetcherService fetcher = new FetcherService(web, repository);

var url = new System.Uri("https://www.google.com");

// Cold start: You can ship with preloaded data, and thus avoid
// an initial requirement for an active internet connection
fetcher.Preload(url, "<html>Hello world!</html>");

// Try to fetch the url from the network (preloaded data is considered 
// old and invalidated), but return the preloaded data if there's no connection
IUrlCacheInfo response = await fetcher.Fetch(url); 
```

License
=======

- **Fetcher** is licensed under [Apache 2.0][apache]

[apache]: https://www.apache.org/licenses/LICENSE-2.0.html