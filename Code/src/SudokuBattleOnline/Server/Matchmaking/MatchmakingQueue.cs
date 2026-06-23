using System.Collections.Generic;
using System.Linq;
using SudokuBattle.Server.Network;

namespace SudokuBattle.Server.Matchmaking
{
    /// <summary>
    /// hàng đợi ghép người chơi
    /// </summary>
    public class MatchmakingQueue
    {
        private readonly List<ClientSession> _queue = new();
        private readonly object _lock = new();

        /// <summary>
        /// add người chơi vào queue nếu chưa có
        /// </summary>
        public void Enqueue(ClientSession session)
        {
            lock (_lock)
            {
                if (!_queue.Any(s => s.SessionId == session.SessionId))
                {
                    _queue.Add(session);
                }
            }
        }

        /// <summary>
        /// lấy người chơi ra khỏi queue
        /// </summary>
        public ClientSession? Dequeue()
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    var session = _queue[0];
                    _queue.RemoveAt(0);
                    return session;
                }
                return null;
            }
        }

        /// <summary>
        /// remove người chơi khỏi queue khi họ hủy tìm trận hoặc ngắt kết nối
        /// </summary>
        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                _queue.RemoveAll(s => s.SessionId == session.SessionId);
            }
        }

        /// <summary>
        /// số lượng người chơi đang chờ ghép
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }
    }
}
