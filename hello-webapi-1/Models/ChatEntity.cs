using System;

namespace hello_webapi.Models
{
    public class ChatEntity
    {
        /// <summary>
        /// 消息发送者
        /// </summary>
        /// <value></value>
        public string Sender { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        /// <value></value>
        public DateTime SendTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 消息内容
        /// </summary>
        /// <value></value>
        public string Message { get; set; }

        /// <summary>
        /// 重写ToString()方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{SendTime} - {Sender} : {Message}";
        }
    }
}