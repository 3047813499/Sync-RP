using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


public class WorkspaceHub : Hub
{
	private readonly AppDbContext _db;
	public WorkspaceHub(AppDbContext db) => _db = db;

	// 1. 用户连接时加载所有数据
	public override async Task OnConnectedAsync()
	{
		var columns = await _db.Columns.OrderBy(c => c.Order).ToListAsync();
		var boxes = await _db.Boxes.ToListAsync();
		await Clients.Caller.SendAsync("LoadWorkspace", columns, boxes);
		await base.OnConnectedAsync();
	}



	// 2. 增加格子 (修复点：添加后通知所有人更新布局)
	public async Task RequestAddBox(string columnId, string boxId)
	{
		var newBox = new WorkspaceBox { Id = boxId, ColumnId = columnId, Content = "" };
		_db.Boxes.Add(newBox);
		await _db.SaveChangesAsync();

		// 核心修改：让所有人重新加载布局，确保格子出现在正确的列
		await Clients.All.SendAsync("OnLayoutUpdated");
	}

	// 3. 更新文字内容
	public async Task SendTextUpdate(string boxId, string content)
	{
		var box = await _db.Boxes.FindAsync(boxId);
		if (box != null)
		{
			box.Content = content;
			await _db.SaveChangesAsync();
			await Clients.Others.SendAsync("OnTextUpdated", boxId, content);
		}
	}

	// 4. 删除格子
	public async Task RequestDeleteBox(string boxId)
	{
		var box = await _db.Boxes.FindAsync(boxId);
		if (box != null)
		{
			_db.Boxes.Remove(box);
			await _db.SaveChangesAsync();
			await Clients.All.SendAsync("OnLayoutUpdated");
		}
	}

	// 5. 增加一列
	public async Task AddColumn()
	{
		var count = await _db.Columns.CountAsync();
		var newId = "col_" + DateTime.Now.Ticks;
		var newCol = new ColumnConfig { Id = newId, Title = "新竖列", Order = count };
		_db.Columns.Add(newCol);
		await _db.SaveChangesAsync();
		await Clients.All.SendAsync("OnLayoutUpdated");
	}

	// 6. 减少最后一列
	public async Task RemoveLastColumn()
	{
		var columns = await _db.Columns.OrderBy(c => c.Order).ToListAsync();
		if (columns.Count <= 1) return;

		var lastCol = columns.Last();
		var associatedBoxes = _db.Boxes.Where(b => b.ColumnId == lastCol.Id);
		_db.Boxes.RemoveRange(associatedBoxes);
		_db.Columns.Remove(lastCol);

		await _db.SaveChangesAsync();
		await Clients.All.SendAsync("OnLayoutUpdated");
	}

	// 7. 更新标题
	public async Task UpdateColumnTitle(string columnId, string newTitle)
	{
		var col = await _db.Columns.FindAsync(columnId);
		if (col != null)
		{
			col.Title = newTitle;
			await _db.SaveChangesAsync();
			await Clients.Others.SendAsync("OnTitleUpdated", columnId, newTitle);
		}
	}

	// 8. 清空数据
	public async Task ClearAllBoxes()
	{
		_db.Boxes.RemoveRange(_db.Boxes);
		await _db.SaveChangesAsync();
		await Clients.All.SendAsync("OnLayoutUpdated");
	}

	// v1.3: 取消单个格子增加，改为同步添加一行
	// v1.3.2: 一次性添加 10 行
	public async Task RequestAddRow()
	{
		// 1. 获取当前所有列
		var columns = await _db.Columns.OrderBy(c => c.Order).ToListAsync();

		if (columns.Count == 0) return;

		// 2. 外层循环 10 次（生成 10 行）
		for (int i = 0; i < 10; i++)
		{
			// 内层循环遍历每一列
			foreach (var col in columns)
			{
				var newBox = new WorkspaceBox
				{
					Id = Guid.NewGuid().ToString(),
					ColumnId = col.Id,
					Content = ""
				};
				_db.Boxes.Add(newBox);
			}
		}

		// 3. 统一保存并通知
		await _db.SaveChangesAsync();
		await Clients.All.SendAsync("OnLayoutUpdated");
	}


}