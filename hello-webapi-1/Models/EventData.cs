using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Serialization;

namespace hello_webapi.Models
{
    [Serializable]
    public class EventData<TData>
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        /// <value></value>
        public Events Event { get; set; }

        /// <summary>
        /// 事件参数
        /// </summary>
        /// <value></value>
        public TData Data { get; set; }
    }
}