# Facet calling

Facet methods are endpoints that can be invoked by clients on demand
as remote procedure calls.

This is what a facet call request looks like:

**Request:**

```json
{
    "facetName": "SomeFacet",
    "methodName": "MyMethod",
    "arguments": ["first", "second", 42],
    "sessionId": "123456789"
}
```

`facetName` and `methodName` specify what method to call.

`arguments` are serialized arguments passed to the method.

`sessionId` keeps track of a client session. First request has this
value equal to `null` and then it obtains a new session id in the response.
Sessions are (just like on the web) needed for authentication.

Then the response looks like this:

**Response:**

Response is exactly the serialized returned value from the facet method.

**Special:**

```json
{
    "sessionId": "123456789"
}
```

`sessionId` what session id should the client remember and send
with the next request.
