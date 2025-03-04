namespace Unisave.HttpClient
{
    /// <summary>
    /// Wraps a request and the corresponding response object pair
    /// </summary>
    public struct RequestResponsePair
    {
        public Request Request { get; }
        public Response Response { get; }
            
        public RequestResponsePair(Request request, Response response)
        {
            Request = request;
            Response = response;
        }
    }
}