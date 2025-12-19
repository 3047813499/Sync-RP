using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. 注册数据库服务，指定使用 SQLite 及其文件名
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite("Data Source=workspace.db"));

builder.Services.AddSignalR();
builder.WebHost.UseUrls("http://0.0.0.0:5376");
var app = builder.Build();

// 2. 自动创建数据库文件（如果不存在的话）
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	// 确保数据库已创建
	db.Database.EnsureCreated();

	// 如果列配置表是空的，初始化默认三列
	if (!db.Columns.Any())
	{
		db.Columns.AddRange(
			new ColumnConfig { Id = "col-1", Title = "剧情/世界观", Order = 0 },
			new ColumnConfig { Id = "col-2", Title = "人物/状态", Order = 1 },
			new ColumnConfig { Id = "col-3", Title = "记录/备忘", Order = 2 }
		);
		db.SaveChanges();
	}
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<WorkspaceHub>("/workspaceHub");

app.Run();