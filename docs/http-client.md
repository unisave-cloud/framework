# HTTP Client

This documentation page describes the inner structure of the `HttpClient` component (or rather the `Unisave.HttpClient` namespace).

> Why build a wrapper around the .NET HttpClient?
> - To create stable abstraction that game developers can rely on, even if the underlying implementation changes (which has happend during mono2coreclr migration)
> - To add JSON support which is compatible with the Unisave JSON serializer
> - To provide nice fluent API inspired by Laravel
> - To allow for testing and faking HTTP requests without the use of mocking frameworks

To learn about the component from the user's perspective, read the corresponding Unisave documentation page: https://unisave.cloud/docs/http-client

> **Note:** The design of this component was inspired by the [Laravel's HTTP client component](https://laravel.com/docs/12.x/http-client).

<img src="http-client-code-diagram.svg" />
<!-- https://drive.google.com/file/d/1NlkHlixIv0MHTAOq79-tz5-BN9tAYmQw/view?usp=drive_link -->


## Common usage

The primary entrypoint into the component is the `Unisave.HttpClient.IHttp` interface. It defines the public API of the whole framework component. The `Unisave.Facades.Http` facade exposes the same API via static methods.

The interface (and therefore also the facade) is implemented by the class `Client`. It primarily forwards the public API calls into two classes: `Factory` and `PendingRequest`.

The `PendingRequest` class is a fluent API request builder that is returned by the public API after almost every call. It allows the user to specify additional request parameters and then send the defined HTTP request. After the request is sent, a `Response` object is returned for the user to consume. For testing purposes, also a `Request` object is created. Both the `Request` and `Response` objects are just wrappers around the corresponding .NET classes `System.Net.Http.HttpRequestMessage` and `System.Net.Http.HttpResponseMessage`.

The `Factory` service is responsible for creating `PendingRequest` instances. It injects the underlying `System.Net.Http.HttpClient` service into it and also defines callbacks to be executed before the HTTP request is actually sent. These callbacks are primarily used for testing and thus most of the testing logic is implemented inside the `Factory` service.

Lastly, the whole component is registered into the framework's service container in the `HttpClientBootstrapper`.


## Testing

The component contains extensive support for testing and mocking HTTP calls. This testing is intended for test, where the backend code is executed locally on the developer's machine (not in the cloud).

These testing features are documented from the user's perspective in the unisave documentation: https://unisave.cloud/docs/http-client#testing

The testing logic consists of two parts:

- Faking responses
- Recording traffic (request, response pairs)

Both of these features are implemented in the `Factory` service, by utilizing the `PendingRequest`'s request interceptor callback.

Recording is enabled the moment the first faking callback is registered. It cannot be turned off (which would not make sense in the testing scenario). User can then query the recorded requests by making assertion API calls (such as `Http.AssertSent(...)`).

Responses are faked by registering callbacks (called `stubCallbacks`) and these callbacks are then responsible for creating the stub response. The callback can choose to produce a stub response or let the request continue into the next callback, or ultimately let it be executed as a real HTTP request.

This callback registration has many helper method variants that let the user specify a sub-set of URL addresses, or generate default `200 OK` response, etc. But under the hood, only these simple callback functions are registered. This faking is provided via the `Http.Fake(...)` public API method.


## Unit Tests

This is an overview of unit tests for this component:

- `PendingRequestTest` Test the central component (`PendingRequest`) that makes the actual HTTP calls. It tests that it behaves in the way that's described in the user documentation. It works by mocking the `HttpMessageHandler` wich is an internal component of the .NET `HttpClient` service. It just records the HTTP request and generates a fake `200 OK` response. This means we really test the construction of the HTTP request, not parsing of the response - for that, create a `ResponseTest` fixture. This test is primarily for the low-level HTTP request parameters, such as HTTP method, query string, headers, content and content type. Higher-level APIs that are built on top of this are tested separately (such as auth headers, etc.).
- `AuthorizationTest` Tests the higher-level API for sending the `Authorization` header with the request.
- `FakingTest` Tasks that the HTTP client can be faked and start recording requests. Another words, that the HTTP client can be tested.
