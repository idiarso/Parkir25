using Microsoft.AspNetCore.SignalR;

namespace ParkIRC.Data.Hub
{
    public class ParkingHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task SendParkingUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveParkingUpdate", message);
        }
    }
}
