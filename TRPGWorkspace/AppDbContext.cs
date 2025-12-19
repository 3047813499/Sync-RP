using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<WorkspaceBox> Boxes { get; set; }
	// 新增：存放列标题的表
	public DbSet<ColumnTitle> ColumnTitles { get; set; }
}
