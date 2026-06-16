namespace SudokuBattleOnline.Shared.Packets
{
    public class JoinRoomPacket : BasePacket
    {
        public string RoomId { get; set; } = string.Empty;

        public JoinRoomPacket()
        {
            PacketType = "JOIN_ROOM";
        }
    }
}
