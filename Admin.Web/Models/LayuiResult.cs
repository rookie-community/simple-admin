namespace Admin.Web.Models;

/// <summary>
/// layui / layuiadmin 约定的统一 JSON 响应结构。
/// </summary>
public class LayuiResult
{
    public int Code { get; set; }
    public string Msg { get; set; } = string.Empty;
    public object? Data { get; set; }
    public long Count { get; set; }

    public static LayuiResult Ok(string msg = "操作成功")
    {
        return new LayuiResult { Code = 0, Msg = msg };
    }

    public static LayuiResult OkData(object? data, string msg = "")
    {
        return new LayuiResult { Code = 0, Msg = msg, Data = data };
    }

    /// <summary>表格分页数据：<c>{code, msg, count, data}</c>。</summary>
    public static LayuiResult OkTable(long count, object? data)
    {
        return new LayuiResult { Code = 0, Count = count, Data = data };
    }

    public static LayuiResult Error(string msg, int code = 1)
    {
        return new LayuiResult { Code = code, Msg = msg };
    }
}
