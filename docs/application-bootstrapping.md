# Application bootstrapping

On the top-most level an execution is a request to perform something,
that runs for some time and then returns either a value, or throws
an exception. This is true even if the execution takes a long time
during which it communicates with the sandbox.

An execution is specified by `execution parameters`,
which is a JSON object of some sort.

> JSON in this context is preferred to use `"camelCase"`
> for naming keys

An execution might then end up in two scenarios:

**Successfully finishes and returns a value:**

```json
{
    "result": "ok",
    "returned": null
}
```

> Something has to always be returned. When no return value needed,
> just return `null`.

**Fails with an exception:**

```json
{
    "result": "exception",
    "exception": {}
}
```

> The exception might be serialized in any way. It might be created
> artificially by some subsystem, but it has to follow the exception
> serialization guidelines - see the serializer for that.


### Execution parameters

Execution parameters take the following form:

```json
{
    "method": "facet-call",
    "methodParameters": {},
    "env": "..."
}
```

Execution parameters simply redirect to some method with method
parameters (JSON object) that performs some specific task.

> Method might be: `facet-call`, `migration`, `command`, `some-hook`

> Method names should use `kebab-case`


### Sandbox API and Service Container

Before the sandbox (or testing rig) starts the framework,
it should bind sandbox api methods. Services will typically try
to use the api.

If the service container is `null`, it will be setup with default
services. If some testing is needed, and the container is not null,
it will not be modified and will be used as is.

> Note that this static interface (sandbox api and service container)
> prevents the framework from being recursively bootstrapped. This is
> however a fair price to pay for the type decoupling from the sandbox.


### Entrypoint method

With all this in mind, we can look at the definition of the entrypoint:

```csharp
public static class Entrypoint
{
    public static string Start(
        string executionParameters,
        Type[] gameAssemblyTypes
    )
    { /* ... */ }
}
```

`executionParameters` have been explained, here they are passed as
plain string, to decouple JSON parsing libraries.

`gameAssemblyTypes` are needed to access user-defined code and invoke it.

Return type is string - again this is the above described JSON response
serialized into a string.
