using WebApi;
var builder = WebApplication.CreateBuilder(args);

// Register Web API services and infrastructure
builder.Services.AddWebApi(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline and middleware
app.UseApiSettings();

app.Run();


