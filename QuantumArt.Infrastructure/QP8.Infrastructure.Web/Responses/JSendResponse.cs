﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QP8.Infrastructure.Web.Enums;

namespace QP8.Infrastructure.Web.Responses
{
    public class JSendResponse : JSendResponse<dynamic>
    {
    }

    public class JSendResponse<T>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public JSendStatus Status { get; set; }

        public T Data { get; set; }

        public string Message { get; set; }

        public int Code { get; set; }
    }
}
