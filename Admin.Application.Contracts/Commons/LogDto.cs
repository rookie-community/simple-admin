using Microsoft.Extensions.Logging;
using System;
using Volo.Abp.Application.Dtos;

namespace Admin.Commons
{
    public class LogDto : EntityDto
    {
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public string Messages { get; set; } = null!;

        /// <summary>
        /// 日志类型
        /// </summary>
        public LogLevel Level { get; set; }
    }
}
