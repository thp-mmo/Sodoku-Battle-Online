using System.Collections.Generic;

namespace SudokuBattleOnline.Shared.Packets
{
    public class BestScorePacket : BasePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<BestScoreItem> Scores { get; set; } = new List<BestScoreItem>();
    }

    public class BestScoreItem
    {
        public int Rank { get; set; }
        public string Username { get; set; } = "";
        public string Difficulty { get; set; } = "";
        public int BestScore { get; set; }
        public int BestTimeSeconds { get; set; }
        public string AchievedAt { get; set; } = "";
    }
}