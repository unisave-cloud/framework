# Unisave Framework

Unisave Framework is a library that encapsulates the Unisave user's custom backend code and provides a standardized interface for that code to interact with other Unisave services and the outside world.


## Documentation

- [HTTP Client](docs/http-client.md)
- [NuGet and Paket Dependency Management](docs/nuget-and-paket-dependency-management.md)


## After Cloning

The project uses `dotnet` CLI for building, NuGet for the management of regular NuGet packages and [Paket](https://github.com/fsprojects/Paket) for the management of dependencies that are outside of the NuGet repository (LightJson, TinyIoC, UnisaveJWT).

After cloning do:

1. `dotnet tool restore` to install the paket tool (reads the `.config/dotnet-tools.json` file)
2. `dotnet paket restore` to install all paket-managed dependencies

Now Rider should be able to compile all projects. You can verify by running:

```bash
dotnet build
```

If Rider complains about not finding .NET Framework 4.7.2, try to close and open the solution again. If it underlines a file as erroneous, open that file and there's a chance that the error disappears.

Run Unit tests by right-clicking the solution and clicking `Run Unit Tests`. If that option is not available, open a some unit test, right click that test's method definition and click `Run Unit Test` - this runs that one test and also wakes up Rider to register all other tests (at least in one csproj, do the same in the other one). Then you can run tests directly by right-clicking the solution or from the test runner window.
