using Shared;
using Api;

var builder = WebApplication.CreateBuilder(args);

var shardPaths = builder.Configuration
	.GetSection("Search:ShardDatabasePaths")
	.GetChildren()
	.Select(section => section.Value)
	.Where(path => !string.IsNullOrWhiteSpace(path))
	.Cast<string>()
	.ToArray();

if (shardPaths.Length == 0)
	shardPaths = new[] { Paths.SQLITE_DATABASE };

builder.Services.AddControllers();
builder.Services.AddScoped<IReadOnlyList<ISearchDatabase>>(_ =>
	shardPaths.Select(path => (ISearchDatabase)new DatabaseSqlite(path)).ToArray());
builder.Services.AddScoped<IIndexDatabase, DatabaseSqlite>(); 
builder.Services.AddScoped<ISearchLogic>(services =>
	SearchLogicFactory.Create(services.GetRequiredService<IReadOnlyList<ISearchDatabase>>()));

// Kan i fremtiden tilføjes 
// builder.Services.AddScoped<ISearchDatabase, DatabasePostgres>();
// builder.Services.AddScoped<IIndexDatabase, DatabasePostgres>(); 


var app = builder.Build();
app.MapControllers();
app.Run();