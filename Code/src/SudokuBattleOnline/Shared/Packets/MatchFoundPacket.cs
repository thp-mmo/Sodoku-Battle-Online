namespace SudokuBattleOnline.Shared.Packets
{
    /// <summary>
    /// Gói tin Server gửi cho Client khi tìm được đối thủ.
    /// Chứa thông tin đối thủ và mã phòng đấu.
    /// </summary>
    public class MatchFoundPacket : BasePacket
    {
        /// <summary>Mã phòng đấu được tạo.</summary>
        public string RoomId { get; set; } = string.Empty;

        /// <summary>Tên đăng nhập của đối thủ.</summary>
        public string OpponentUsername { get; set; } = string.Empty;

        public MatchFoundPacket()
        {
            PacketType = "MATCH_FOUND";
        }
    }
}
