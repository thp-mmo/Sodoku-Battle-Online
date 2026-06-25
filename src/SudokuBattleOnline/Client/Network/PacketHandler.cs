namespace Client.Network
{
    /// <summary>
    /// điều phối packet nhận được
    /// </summary>
    public class PacketHandler
    {
        private readonly PacketReceiver _receiver;

        public PacketHandler(PacketReceiver receiver)
        {
            _receiver = receiver;
        }

        public void Handle(string packet)
        {
            _receiver.Process(packet);
        }
    }
}