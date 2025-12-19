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
	db.Database.EnsureCreated();
	if (!db.ColumnTitles.Any())
	{
		db.ColumnTitles.AddRange(
			new ColumnTitle { Id = "col-1", Title = "1" },
			new ColumnTitle { Id = "col-2", Title = "2" },
			new ColumnTitle { Id = "col-3", Title = "3" }
		);
		db.SaveChanges();
	}
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<WorkspaceHub>("/workspaceHub");

app.Run();