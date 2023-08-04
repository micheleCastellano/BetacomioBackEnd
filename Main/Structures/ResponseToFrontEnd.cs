namespace Main.Structures
{
    public class ResponseToFrontEnd
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public ResponseToFrontEnd(int statuscode, bool success, string message)
        {
            StatusCode = statuscode;
            Success = success;
            Message = message;
        }
    }
}
