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


## Documentation

- [Application bootstrapping](docs/application-bootstrapping.md)
