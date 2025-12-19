public class WorkspaceBox
{
	// 格子的唯一 ID
	public string Id { get; set; }
	// 属于哪一列 (col-1, col-2, col-3)
	public string ColumnId { get; set; }
	// 格子里的文字内容
	public string Content { get; set; } = "";
}

public class ColumnConfig
{
	public string Id { get; set; }    // 列的唯一标识，如 "col-1"
	public string Title { get; set; } // 列标题
	public int Order { get; set; }    // 排列顺序
}