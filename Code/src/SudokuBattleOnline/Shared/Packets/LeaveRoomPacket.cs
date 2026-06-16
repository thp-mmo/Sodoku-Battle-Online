namespace SudokuBattleOnline.Shared.Packets
{
    public class LeaveRoomPacket : BasePacket
    {
        public string RoomId { get; set; } = string.Empty;

        public LeaveRoomPacket()
        {
            PacketType = "LEAVE_ROOM";
        }
    }
}
