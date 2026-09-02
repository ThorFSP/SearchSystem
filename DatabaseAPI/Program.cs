using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ISearchDatabase, DatabaseSqlite>(); 
builder.Services.AddScoped<IIndexDatabase, DatabaseSqlite>(); 

// Kan i fremtiden tilføjes 
// builder.Services.AddScoped<ISearchDatabase, DatabasePostgres>();
// builder.Services.AddScoped<IIndexDatabase, DatabasePostgres>(); 


var app = builder.Build();
app.MapControllers();
app.Run();