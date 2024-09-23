using System;

namespace DomainManageTool.Models
{
    public class RecordDto
    {
        public string RecordId { get; set; }

        public string Value { get; set; }

        public string Status { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string Name { get; set; }

        public string Line { get; set; }

        /// <summary>
        /// 线路Id
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// 记录类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 权重
        /// </summary>
        public int? Weight { get; set; }

        public string Remark { get; set; }

        public ulong TTL { get; set; }

        /// <summary>
        /// MX值，只有MX记录有
        /// </summary>
        public int? MX { get; set; }

        /// <summary>
        /// 是否是默认的ns记录
        /// </summary>
        public bool DefaultNS { get; set; }

    }
}
