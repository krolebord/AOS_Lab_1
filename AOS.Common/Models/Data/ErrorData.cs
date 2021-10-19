using AOS.Common.DataSerialization;

namespace AOS.Common.Models.Data
{
    public class ErrorData
    {
        [SOrder(1)]
        public string Message { get; set; } = string.Empty;

        public ErrorData() {}

        public ErrorData(string message)
        {
            Message = message;
        }
    }
}
