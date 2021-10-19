using AOS.Common.DataSerialization;
using AOS.Common.DataSerialization.Objects;
using AOS.Common.Models.Data;
using OneOf;

namespace AOS.Common.Models.Responses
{
    public class Response<T>
    {
        [SOrder(1)]
        public byte RawStatus { get; set; }

        [SOrder(2)]
        public SObject RawData { get; set; } = default!;

        public ResponseStatus Status
        {
            get => (ResponseStatus)RawStatus;
            set => RawStatus = (byte)value;
        }

        public OneOf<T, ErrorData>? GetBody()
        {
            switch (Status)
            {
                case ResponseStatus.Error:
                {
                    var error = SConverter.ConvertFromSObject<T>(RawData);
                    if (error == null)
                    {
                        return null;
                    }

                    return error;
                }
                case ResponseStatus.Success:
                {
                    var success = SConverter.ConvertFromSObject<T>(RawData);
                    if (success == null)
                    {
                        return null;
                    }

                    return success;
                }
                default:
                    return null;
            }
        }

        public static Response<ErrorData> CreateError(ErrorData data)
        {
            return new Response<ErrorData>
            {
                Status = ResponseStatus.Error,
                RawData = SConverter.ConvertToSObject(data)
            };
        }

        public static Response<T> Create(T data)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Success,
                RawData = SConverter.ConvertToSObject(data!)
            };
        }
    }
}
