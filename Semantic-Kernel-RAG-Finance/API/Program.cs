using Azure;
using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Models.Chat;
using Services;
using Services.IService;
using Services.Service;
using API.Modules;
using Buisness_Logic.Interfaces;
using Buisness_Logic;
using Services.Services;
using Services.IServices;

var builder = WebApplication.CreateBuilder(args);

// Add Services.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
//Loggics
builder.Services.AddSingleton<IChatLogic,ChatLogic>();
builder.Services.AddSingleton<IDocumentLogic,DocumentLogic>();
builder.Services.AddSingleton<ISummarizationLogic,SummarizationLogic>();
builder.Services.AddSingleton<ISummarizationBasedEmbeddingLogic,SummarizationBasedEmbeddingLogic>();
//Services
builder.Services.AddSingleton<ISearchService, SearchEmbeddingsService>();
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<ILoadMemoryService,LoadMemoryService>();
builder.Services.AddSingleton<ISumarizationLLMService, SumarizationLLMService>();
//Modules
builder.Services.AddTransient<DocumentHandler>();
builder.Services.AddTransient<ChatHandler>();
builder.Services.AddTransient<SummarizationHandler>();
builder.Services.AddTransient<SummarizationBasedEmbeddingHandler>();


var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// Get token
app.MapGet("antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    var xsrfToken = tokens.RequestToken!;
    return TypedResults.Content(xsrfToken, "text/plain");
});
// File Embeddings Endpoint
app.MapPost("api/file", async (IFormFileCollection files,string collection,DocumentHandler handler) => {
    if (files == null)
    {
        return Results.BadRequest("Invalid input");
    }
    try
    {
        var result = await handler.DocumentToRag(files,collection);
        return TypedResults.Ok(result);
    }
    catch (Exception ex)
    {
        // Log the exception
        return Results.BadRequest(ex.Message);
    }
});

//Chatting Endpoint
app.MapPost("/api/chat", async (ChatInput chatInput, ChatHandler chatHandler) =>
{
    if (chatInput == null)
    {
        return Results.BadRequest("Invalid input");
    }

    try
    {
        var result = await chatHandler.HandleChat(chatInput);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        // Log the exception
        return Results.BadRequest("Invalid input");
    }
});
app.MapPost("api/summarize", async (IFormFileCollection files, string name, SummarizationHandler handler) => {
    if (files == null)
    {
        return Results.BadRequest("Invalid input");
    }
    try
    {
        var result = await handler.DocumentToSummarization(files,name);
        return TypedResults.Ok(result);
    }
    catch (Exception ex)
    {
        // Log the exception
        return Results.BadRequest(ex.Message);
    }
});
// File Embeddings Endpoint with summarization
app.MapPost("api/summarization/file", async (IFormFileCollection files, string collection, SummarizationBasedEmbeddingHandler handler) => {
    if (files == null)
    {
        return Results.BadRequest("Invalid input");
    }
    try
    {
        var result = await handler.DocumentToRag(files, collection);
        return TypedResults.Ok(result);
    }
    catch (Exception ex)
    {
        // Log the exception
        return Results.BadRequest(ex.Message);
    }
});
app.Run();



