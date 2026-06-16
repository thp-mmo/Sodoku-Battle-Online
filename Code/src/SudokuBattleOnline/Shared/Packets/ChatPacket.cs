namespace SudokuBattleOnline.Shared.Packets
{
    public class ChatPacket : BasePacket
    {
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;

        public ChatPacket()
        {
            PacketType = "CHAT";
        }
    }
}
