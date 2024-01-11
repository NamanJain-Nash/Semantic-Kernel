using Buisness_Logic;
using ChatAPI;
using Models;
using Services.IService;
using Services.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IChatLogic,ChatLogic>();
builder.Services.AddSingleton<ISearchService,SearchService>();
builder.Services.AddSingleton<IChatService,ChatService>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapPost("/api/chat",async (ChatHandler chat,ChatInput input)=>{
    return await chat.HandleChat(input);
});

app.Run();

