Unisave Framework
=================

Framework serves multiple purposes:

- library: contains code required on both the server and the client
    - serializer, facet, entity, exceptions, facades
- framework: contains code the user-defined server code talks to
    - database, logging, session
- runtime: contains code to bootstrap execution and communicate with the sandbox
    - entrypoint, sandbox api, service container


## Execution JSON API Reference

- [Execution JSON API](docs/api-general.md)
- [`facet-call` method](docs/api-facet-call.md)


## Sandbox API Reference

- [`session` channel](docs/channel-session.md)


## Documentation

- [Application bootstrapping](docs/application-bootstrapping.md)


## Dependency management via Paket

I decided to use [Paket](https://github.com/fsprojects/Paket) instead of NuGet for dependency management, because it can exclude transitive dependencies (used for JWT) and reference GitHub projects (used for LightJson).

The paket tool is used via the `dotnet` command, but the Unisave Framework is in fact built in Mono MSBuild. The `dotnet` command is therefore only used to run Paket and nothing else.

Paket was installed using commands:

```
dotnet new tool-manifest
dotnet tool install paket
dotnet tool restore
```

I guess the last command `dotnet tool restore` installs tools based on the config at `.config/dotnet-tools.json`, should the repository be restored on a new machine. (I guess that right!)

I initialized Paket files via `dotnet paket init`.

I filled out the `paket.dependencies` file and the `paket.references` for the framework project.

I installed the packages by:

```
dotnet paket install
```

...

To restore dependencies after cloning the repo, run:

```
dotnet tool restore
dotnet paket restore
```
