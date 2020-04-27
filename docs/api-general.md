# Execution JSON API

**Request**

> Same as`executionParameters`

```json
{
    "method": "facet-call",
    "methodParameters": {},
    "env": "..."
}
```

`method` Name of the requested method

`methodParameters` Any JSON value, depends on the method requested

`env` text of the env configuration file

**Success**

```json
{
    "result": "ok",
    "returned": null,
    "special": {}
}
```

`result` Always `ok` when no exception has been thrown

`returned` Returned JSON value from the requested method

`special` Special values returned from the method (always a JSON object
of key-value pairs)

**Exception**

```json
{
    "result": "exception",
    "exception": {},
    "special": {}
}
```

`result` Always `exception` when and exception has been thrown

`exception` Serialized exception that has been thrown (depends on
the type of serialization)

`special` Special values returned from the method (always a JSON object
of key-value pairs)

> **IMPORTANT:** Special values are values, that are returned even if the
method fails with exception. One of such values is session id.

> Special values might not be present if the execution crashes really hard
(like inside the framework booting process).
