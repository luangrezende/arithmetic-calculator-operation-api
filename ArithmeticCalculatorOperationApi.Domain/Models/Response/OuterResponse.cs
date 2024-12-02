namespace ArithmeticCalculatorOperationApi.Domain.Models.Response
{
    public class OuterResponse
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }
        public bool IsBase64Encoded { get; set; }
    }

}
