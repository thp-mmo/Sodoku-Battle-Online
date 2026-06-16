namespace SudokuBattleOnline.Shared.Packets
{
    public class CellUpdatePacket : BasePacket
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Value { get; set; }

        public CellUpdatePacket()
        {
            PacketType = "CELL_UPDATE";
        }
    }
}
