Fetcher
==========

[![Build status](https://ci.appveyor.com/api/projects/status/iysnpswp82ogp4vb?svg=true)](https://ci.appveyor.com/project/mgj/fetcher)

| Plugin          | NuGet version                                                                                                                                                              |
| --------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Fetcher        | [![NuGet](https://img.shields.io/nuget/v/artm.fetcher.svg)](https://www.nuget.org/packages/artm.fetcher/)             |

Example
==========
> IFetcherRepositoryStoragePathService path = new FetcherRepositoryStoragePathService();
> IFetcherRepositoryService repository = new FetcherRepositoryService(path);
> IFetcherWebService web = new FetcherWebService();
> Mvx.LazyConstructAndRegisterSingleton<IFetcherService>(() => new FetcherService(web, repository));


License
=======

- **Fetcher** is licensed under [Apache 2.0][apache]

[apache]: https://www.apache.org/licenses/LICENSE-2.0.html