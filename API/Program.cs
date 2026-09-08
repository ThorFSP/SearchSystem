using Shared;
using Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ISearchDatabase, DatabaseSqlite>(); 
builder.Services.AddScoped<IIndexDatabase, DatabaseSqlite>(); 
builder.Services.AddScoped<ISearchLogic>(services =>
	SearchLogicFactory.Create(services.GetRequiredService<ISearchDatabase>()));

// Kan i fremtiden tilføjes 
// builder.Services.AddScoped<ISearchDatabase, DatabasePostgres>();
// builder.Services.AddScoped<IIndexDatabase, DatabasePostgres>(); 


var app = builder.Build();
app.MapControllers();
app.Run();