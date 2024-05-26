using Microsoft.AspNetCore.Mvc;
using GodOfGodField.Shared;
using System.Text.Json;
using System.Web;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.SignalR;
using GodOfGodField.Server.Hubs;
using Microsoft.Extensions.Primitives;

namespace GodOfGodField.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class HiddenGameController(IHubContext<HiddenGameHub> hubContext) : ControllerBase {
    IHubContext<HiddenGameHub> HubContext { get; } = hubContext;

    [HttpPost("newplayer")]
    public async Task<string> NewPlayer() {
        async Task debugAddItem(int modelId) {
            var payload = JsonSerializer.Serialize(new {
                action = new {
                    stringValue = "debugAddItem"
                },
                modelId = modelId
            });
            foreach (var connectionId in HiddenGameHub.Rooms[0].Clients) {
                await HubContext.Clients.Client(connectionId).SendAsync("EnqueueEvent", payload);
            }
        }
        await debugAddItem(70);
        await debugAddItem(169);
        return "OK";
    }

    [HttpPost("advanceturn")]
    public async Task<string> AdvanceTurn() {
        var payload = JsonSerializer.Serialize(new {
            action = new {
                stringValue = "advanceTurn"
            },
            playerId = new {
                integerValue = "1"
            }
        });
        foreach (var connectionId in HiddenGameHub.Rooms[0].Clients) {
            await HubContext.Clients.Client(connectionId).SendAsync("EnqueueEvent", payload);
        }
        return "OK";
    }
}
