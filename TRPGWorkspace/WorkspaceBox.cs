public class WorkspaceBox
{
	// 格子的唯一 ID
	public string Id { get; set; }
	// 属于哪一列 (col-1, col-2, col-3)
	public string ColumnId { get; set; }
	// 格子里的文字内容
	public string Content { get; set; } = "";
}