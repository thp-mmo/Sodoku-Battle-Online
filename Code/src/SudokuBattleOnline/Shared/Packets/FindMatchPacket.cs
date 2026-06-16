namespace SudokuBattleOnline.Shared.Packets
{
    public class FindMatchPacket : BasePacket
    {
        public FindMatchPacket()
        {
            PacketType = "FIND_MATCH";
        }
    }
}
