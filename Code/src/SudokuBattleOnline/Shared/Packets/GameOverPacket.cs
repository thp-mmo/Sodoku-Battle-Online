namespace SudokuBattleOnline.Shared.Packets
{
    /// <summary>
    /// Gói tin Server gửi cho cả 2 Client khi trận đấu kết thúc.
    /// Chứa kết quả trận đấu, người thắng, và thống kê.
    /// </summary>
    public class GameOverPacket : BasePacket
    {
        /// <summary>Mã phòng đấu.</summary>
        public string RoomId { get; set; } = string.Empty;

        /// <summary>Tên đăng nhập người chiến thắng (rỗng nếu hòa).</summary>
        public string WinnerUsername { get; set; } = string.Empty;

        /// <summary>Lý do kết thúc trận đấu.</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>Tiến độ hoàn thành của người chơi 1 (0-100%).</summary>
        public int Player1Progress { get; set; }

        /// <summary>Tiến độ hoàn thành của người chơi 2 (0-100%).</summary>
        public int Player2Progress { get; set; }

        public GameOverPacket()
        {
            PacketType = "GAME_OVER";
        }
    }
}
