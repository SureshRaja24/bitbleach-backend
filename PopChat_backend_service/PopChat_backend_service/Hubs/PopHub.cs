using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace PopChat.Backend.Services.Hubs;

[Authorize]
public class PopHub : Hub
{
    // 1. Join Session - Now using the User's Identity from the JWT
    private static readonly ConcurrentDictionary<string, HashSet<string>> _roomUsers = new();

    public async Task JoinSession(string sessionId)
    {
        var users = _roomUsers.GetOrAdd(sessionId, _ => new HashSet<string>());

        lock (users)
        {
            if (users.Count >= 2)
            {
                // Notify only the person trying to join that the room is full
                Clients.Caller.SendAsync("RoomFull", sessionId);
                return;
            }
            users.Add(Context.ConnectionId);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        await Clients.OthersInGroup(sessionId).SendAsync("UserJoined", Context.User?.Identity?.Name ?? "Peer");
    }

   

    // 2. WebRTC Signal Relay
    public async Task SignalWebRTC(string sessionId, string signalData)
    {
        // Security: Ensure the signaling only stays within the specific room
        await Clients.OthersInGroup(sessionId).SendAsync("ReceiveSignal", signalData);
    }

    // --- Download Consent Logic ---

    // User A asks User B for permission
    public async Task RequestDownload(string sessionId)
    {
        var requester = Context.User?.Identity?.Name ?? "Someone";

        // We pass the requester's name so User B knows WHO is asking
        await Clients.OthersInGroup(sessionId).SendAsync("OnDownloadRequested", requester);
    }

    // User B responds
    public async Task RespondToDownload(string sessionId, bool isApproved)
    {
        // Send the result back to the other person in the room
        await Clients.OthersInGroup(sessionId).SendAsync("OnDownloadResponse", isApproved);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string userName = Context.User?.Identity?.Name ?? "Peer";

        foreach (var room in _roomUsers)
        {
            lock (room.Value)
            {
                if (room.Value.Contains(Context.ConnectionId))
                {
                    room.Value.Remove(Context.ConnectionId);

                    // Notify the other user in the room
                    Clients.Group(room.Key).SendAsync("PeerDisconnected", userName);

                    if (room.Value.Count == 0)
                    {
                        _roomUsers.TryRemove(room.Key, out _);
                    }
                }
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}