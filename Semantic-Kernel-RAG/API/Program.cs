using Azure;
using Domain;
using Domain.Interfaces;
using ChatAPI;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Models.Chat;
using Services;
using Services.IService;
using Services.Service;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add Services.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IChatLogic,ChatLogic>();
builder.Services.AddSingleton<ISearchService,SearchEmbeddingsService>();
builder.Services.AddSingleton<IChatService,ChatService>();
builder.Services.AddSingleton<IDocumentLogic,DocumentLogic>();
builder.Services.AddSingleton<ILoadMemoryService,LoadMemoryService>();
builder.Services.AddTransient<DocumentHandler>();
builder.Services.AddTransient<ChatHandler>();


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
app.Run();



