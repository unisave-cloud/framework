# NuGet and Paket Dependency Management

I use both NuGet and Paket to manage dependencies. In theory, one should use only one, but:

- NuGet cannot download files from the internet.
- Paket does not add `<PackageReference />` to csproj files (although it should! Bug? Or should at least add something equivalent) and (thus) does not play nice with Rider's package window.

So I ended up with a solution where:

- Regular NuGet packages are managed via NuGet
- Files and git repositories are managed via Paket


## Understanding NuGet

NuGet is used by `dotnet` CLI and Rider transparently. All you need to have a package "installed" is to have a `<PackageReference />` in your csproj.

Example:

```xml
<PackageReference Include="Owin" Version="1.0.0" />
<PackageReference Include="Microsoft.Owin" Version="4.2.2" />
```

Rider (and `dotnet`) will download and reference these automatically. I think it usually downloads NuGet packages into the `packages/` folder, but since Paket clears this folder whenever `dotnet paket install` is called, it instead installs NuGet packages globally into `~/.nuget`. (or something like that - it just works)


## Understanding Paket

You can get access to the Paket CLI by adding it to the `.config/dotnet-tools.json` file, running `dotnet tool restore` and then using it like:

```bash
dotnet paket [args...]
```

Paket can be used to manage even NuGet packages, but it didn't work for me somehow.

How Paket is [supposed to work](https://fsprojects.github.io/Paket/learn-how-to-use-paket.html):

1. You define the `paket.dependencies` file in your solution where you list all the packages and files all your projects depend on.
2. You define `paket.references` file for each of your csproj projects that list a subset of the `paket.dependencies` to be actually referenced in this project.
3. You run `paket install` to:
    - read the `paket.dependencies` and download/upgrade all dependencies
    - read all `paket.references` files and add references to corresponding csproj files

Then when you ONLY want to download dependencies, without any csproj modifications, you run `paket restore`.

The issue I had was that `paket install` DID NOT add `<PackageReference />` links to my csproj files. So I started using NuGet for these regular packages.

However for files, github repos and other non-standard things, Paket works just fine. When you run `paket install`, it adds file references as expected:

```xml
<Compile Include="..\paket-files\grumpydev\TinyIoC\src\TinyIoC\TinyIoC.cs">
    <Paket>True</Paket>
    <Link>Foundation/TinyIoC.cs</Link>
</Compile>

or

<Reference Include="UnisaveJWT">
    <HintPath>..\paket-files\github.com\UnisaveJWT.dll</HintPath>
    <Private>True</Private>
    <Paket>True</Paket>
</Reference>
```

Paket also creates the file `.paket/Paket.Restore.targets` which is automatically generated with `paket install` and added to all csproj files like this:

```xml
<Import Project="..\.paket\Paket.Restore.targets" />
```

This file binds the standard `dotnet restore`, which downloads NuGet packages, with the `paket restore`, so that Paket dependencies are restored as part of the typical .NET SDK workflow.

However this does not work for me, because one of my dependencies is the `LightJson` project, which I reference as a GitHub repository and it is being referenced from the solution file, as well as most csproj files like any other csproj project. This causes the `dotnet restore` to fail after cloning, as the solution file references a project that does not exist (has not been downloaded yet). So I have to do the manual `paket restore` anyways.

> **Note:** The LightJson project must be referenced by the solution file, otherwise Rider complains about a broken project reference.
