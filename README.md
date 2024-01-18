<img src="https://github.com/gordon-matt/elFinder.NetCore/blob/master/_Misc/Logo.png" alt="Logo" width="350" />

## Instructions

1. Install the NuGet package: https://www.nuget.org/packages/elFinder.NetCore/

2. Look at the [demo project](https://github.com/gordon-matt/elFinder.NetCore/tree/master/elFinder.NetCore.Web) for an example on how to integrate it into your own project.

As simple as that!

## Alternative Drivers

- [Azure Storage](https://github.com/fsmirne/elFinder.NetCore.AzureStorage)
- [Azure Blob Storage](https://github.com/brunomel/elFinder.NetCore.AzureBlobStorage)

## Credits

**elFinder.NetCore** is based on my [fork](https://github.com/gordon-matt/elFinder.Net) of [elFinder.Net](https://github.com/leniel/elFinder.Net) by Leniel Macaferi which itself was based on an [older version](https://github.com/EvgenNoskov/Elfinder.NET) by Yevhen Noskov, which as far as I can tell is the original.

Many thanks also to [Flavio Smirne](https://github.com/fsmirne) for the excellent contribution of an Azure Storage driver and the work done to allow such extensibility.

#### Jan/2024:
Migration to ASP.NET 8 Core and frontend libs upgrade by [@utilsites fork](https://github.com/utilsites/elFinder.NetCore/tree/upgrade-net8-and-clientlibs).
Used original? [elFinder project](https://github.com/Studio-42/elFinder) to update elFinder frontend client lib files. This project claims that old versions have serious problems, does the asp.net port server side still has some of the referred problems? At least the path traversal security problem was fixed using the code on this [pull request](https://github.com/gordon-matt/elFinder.NetCore/pull/73).
Also updated tinyMCE to last version.
