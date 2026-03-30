namespace SV22T1020590.Admin
{
    public class ApiResult
    {
        public ApiResult(int code, string message)
        {
            Code = code;
            Message = message;
        }
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
