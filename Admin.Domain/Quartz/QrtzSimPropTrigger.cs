using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Admin.Quartz
{
    /// <summary>
    /// 通用属性触发器子表
    /// </summary>
    /// <remarks>兼容：CalendarIntervalTrigger、DailyTimeIntervalTrigger</remarks>
    public class QrtzSimPropTrigger : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 所属触发器主表 Id（外键 → QrtzTrigger.Id）
        /// </summary>
        public Guid TriggerId { get; set; }

        /// <summary>字符串属性1</summary>
        public string StrProp1 { get; set; } = null!;

        /// <summary>字符串属性2</summary>
        public string StrProp2 { get; set; } = null!;

        /// <summary>字符串属性3</summary>
        public string StrProp3 { get; set; } = null!;

        /// <summary>整数属性1</summary>
        public int? IntProp1 { get; set; }

        /// <summary>整数属性2</summary>
        public int? IntProp2 { get; set; }

        /// <summary>长整数属性1</summary>
        public long? LongProp1 { get; set; }

        /// <summary>长整数属性2</summary>
        public long? LongProp2 { get; set; }

        /// <summary>小数属性1</summary>
        public decimal? DecProp1 { get; set; }

        /// <summary>小数属性2</summary>
        public decimal? DecProp2 { get; set; }

        /// <summary>布尔属性1</summary>
        public bool? BoolProp1 { get; set; }

        /// <summary>布尔属性2</summary>
        public bool? BoolProp2 { get; set; }

        /// <summary>导航属性：所属触发器</summary>
        public QrtzTrigger Trigger { get; set; } = null!;
    }
}