using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace hello_webapi.Models
{
    [Serializable]
    public class EventData<TData>
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        /// <value></value>
        [JsonConverter(typeof(StringEnumConverter))]
        public Events Event { get; set; }

        /// <summary>
        /// 事件参数
        /// </summary>
        /// <value></value>
        public TData Data { get; set; }
    }
}