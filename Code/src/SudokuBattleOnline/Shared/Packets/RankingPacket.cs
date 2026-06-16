using System.Collections.Generic;

namespace SudokuBattleOnline.Shared.Packets
{
    /// <summary>
    /// Gói tin bảng xếp hạng.
    /// - Client gửi lên Server: yêu cầu lấy danh sách xếp hạng.
    /// - Server gửi về Client: chứa danh sách người chơi đã sắp xếp theo điểm.
    /// </summary>
    public class RankingPacket : BasePacket
    {
        /// <summary>
        /// Danh sách người chơi trong bảng xếp hạng.
        /// </summary>
        public List<RankingEntry> Rankings { get; set; } = new List<RankingEntry>();

        public RankingPacket()
        {
            PacketType = "RANKING";
        }
    }

    /// <summary>
    /// Một dòng trong bảng xếp hạng.
    /// </summary>
    public class RankingEntry
    {
        public int Rank { get; set; }
        public string Username { get; set; } = string.Empty;
        public int RankPoint { get; set; }
        public int WinCount { get; set; }
        public int MatchCount { get; set; }
    }
}
