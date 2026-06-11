using System.Threading.Tasks;

namespace Client.Network
{
    /// <summary>
    /// gửi packet lên server
    /// </summary>
    public class PacketSender
    {
        private readonly ClientConnection _connection;

        public PacketSender(ClientConnection connection)
        {
            _connection = connection;
        }

        public async Task SendAsync(string packet)
        {
            await _connection.SendMessageAsync(packet);
        }

        public async Task LoginAsync(string username)
        {
            string packet = $"LOGIN|{username}";
            await SendAsync(packet);
        }

        public async Task SendChatAsync(string receiver, string message)
        {
            string packet = $"CHAT|{receiver}|{message}";
            await SendAsync(packet);
        }
    }
}