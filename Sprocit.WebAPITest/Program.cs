using SharedTestingModels;
using System.Data.SqlClient;
using Sprocit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(_ => (new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString")!)).Sprocit<IMySprocitTest>());

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/testsprocit", (float rating, IMySprocitTest sprocit) => sprocit.MoviesRatings(rating))
.WithOpenApi();

app.MapGet("/testsprocit2", (float rating) =>
{
    var sprocit = (new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString")!)
    .Sprocit<IMySprocitTest>());
    var movies = sprocit.MoviesRatings(rating);
    return movies;
})
.WithOpenApi();

app.Run();
