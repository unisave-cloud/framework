# `session` sandbox API channel


## `[100]` Load session data

**Request:**

```json
{
    "sessionId": "123456789"
}
```

`sessionId` ID of the session to load. If no such session exists,
empty object should be responded with

**Response:**

```json
{
    "sessionData": { ... }
}
```

`sessionData` JSON object containing the session data


## `[200]` Store session data

**Request:**

```json
{
    "sessionId": "123456789",
    "lifetime": 3600,
    "sessionData": { ... }
}
```

`sessionId` ID of the stored session
`lifetime` lifetime for the session in seconds
`sessionData` the actual data as a JSON object

**Response:**

```json
{
}
```

Empty JSON object
