using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class WorkspaceHub : Hub
{
	private readonly AppDbContext _db;

	// 通过构造函数注入数据库上下文
	public WorkspaceHub(AppDbContext db)
	{
		_db = db;
	}

	// 当用户打开网页连接成功时自动调用
	public override async Task OnConnectedAsync()
	{
		var allBoxes = await _db.Boxes.ToListAsync();
		// 1. 获取所有存好的标题
		var allTitles = await _db.ColumnTitles.ToListAsync();

		// 2. 同时发送格子和标题给刚登录的人
		await Clients.Caller.SendAsync("LoadExistingBoxes", allBoxes, allTitles);

		await base.OnConnectedAsync();
	}

	public async Task RequestAddBox(string columnId, string boxId)
	{
		// 1. 保存到数据库
		_db.Boxes.Add(new WorkspaceBox { Id = boxId, ColumnId = columnId });
		await _db.SaveChangesAsync();

		// 2. 广播给所有人
		await Clients.All.SendAsync("OnBoxAdded", columnId, boxId);
	}

	public async Task SendTextUpdate(string boxId, string content)
	{
		// 1. 更新数据库
		var box = await _db.Boxes.FindAsync(boxId);
		if (box != null)
		{
			box.Content = content;
			await _db.SaveChangesAsync();
		}

		// 2. 同步给其他人
		await Clients.Others.SendAsync("OnTextUpdated", boxId, content);
	}

	// 一键清空所有格子
	public async Task ClearAllBoxes()
	{
		// 1. 从数据库中删除所有记录
		_db.Boxes.RemoveRange(_db.Boxes);
		await _db.SaveChangesAsync();

		// 2. 广播给所有人，执行前端的清空动作
		await Clients.All.SendAsync("OnWorkspaceCleared");
	}

	// 删除单个格子 (可选，建议加上)
	public async Task RequestDeleteBox(string boxId)
	{
		var box = await _db.Boxes.FindAsync(boxId);
		if (box != null)
		{
			_db.Boxes.Remove(box);
			await _db.SaveChangesAsync();
			// 通知所有人移除这个格子的 UI
			await Clients.All.SendAsync("OnBoxDeleted", boxId);
		}
	}
	// 用于同步列标题的改变
	public async Task UpdateColumnTitle(string columnId, string newTitle)
	{
		// 1. 查找并更新数据库中的标题
		var titleRecord = await _db.ColumnTitles.FindAsync(columnId);
		if (titleRecord != null)
		{
			titleRecord.Title = newTitle;
			await _db.SaveChangesAsync();
		}

		// 2. 广播给其他人实时显示
		await Clients.Others.SendAsync("OnTitleUpdated", columnId, newTitle);
	}
}


