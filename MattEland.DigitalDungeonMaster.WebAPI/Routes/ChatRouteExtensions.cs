using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class ChatRouteExtensions
{
    public static void AddChatEndpoints(this WebApplication app)
    {
        app.MapPost("/chat", async ([FromBody] ChatRequest request,
                [FromServices] ChatService chat,
                [FromServices] AppUser user) =>
            {
                ChatResult result = await chat.ChatAsync(request);

                return Results.Ok(result);
            })
            .WithName("Chat")
            .WithDescription("Send a chat message to the server and gets back a response")
            .WithOpenApi()
            .RequireAuthorization();
    }
}