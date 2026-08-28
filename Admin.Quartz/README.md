# QrtzSimPropTrigger 字段配置表

适用于：`CalendarIntervalTrigger` / `DailyTimeIntervalTrigger` / `RecurrenceTrigger`


## 1. CalendarIntervalTrigger

| 字段 | 值示例 | 说明 |
| :--- | :--- | :--- |
| `StrProp1` | `"Month"` | 间隔单位：Second / Minute / Hour / Day / Week / Month / Year |
| `IntProp1` | `2` | 间隔数值（每 2 个月执行一次） |

> 其他字段保持 `NULL`。


## 2. DailyTimeIntervalTrigger

| 字段 | 值示例 | 说明 |
| :--- | :--- | :--- |
| `StrProp1` | `"08:00:00"` | 每日开始时间（HH:mm:ss） |
| `StrProp2` | `"18:00:00"` | 每日结束时间（HH:mm:ss） |
| `StrProp3` | `"[1,2,3,4,5]"` | 执行星期 JSON：1=周日, 2=周一, ..., 7=周六 |
| `IntProp1` | `3600` | 间隔秒数 |
| `BoolProp1` | `true` | 是否无限重复（可选，默认 false） |

> 其他字段保持 `NULL`。


## 3. RecurrenceTrigger

| 字段 | 值示例 | 说明 |
| :--- | :--- | :--- |
| `StrProp1` | `"FREQ=MONTHLY;BYDAY=2MO"` | RRULE 规则字符串 |

> 其他字段保持 `NULL`。


## 4. 汇总对照表

| 触发器类型 | StrProp1 | StrProp2 | StrProp3 | IntProp1 | BoolProp1 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **CalendarInterval** | IntervalUnit | NULL | NULL | Interval | NULL |
| **DailyTimeInterval** | StartTime | EndTime | DaysOfWeek | IntervalSeconds | IsRepeatForever |
| **Recurrence** | RRule | NULL | NULL | NULL | NULL |