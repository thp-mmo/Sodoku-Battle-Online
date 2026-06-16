using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SudokuBattle.Server.Network
{
    /// <summary>
    /// Quản lý tập trung tất cả các phiên kết nối (ClientSession) đang hoạt động.
    /// Sử dụng ConcurrentDictionary để đảm bảo an toàn luồng (thread-safe)
    /// khi nhiều client kết nối/ngắt kết nối đồng thời.
    /// </summary>
    public class SessionManager
    {
        // Dictionary: SessionId -> ClientSession
        private readonly ConcurrentDictionary<string, ClientSession> _sessions = new();

        /// <summary>
        /// Tổng số phiên đang kết nối.
        /// </summary>
        public int OnlineCount => _sessions.Count;

        // ─── Thêm / Xóa ───

        /// <summary>
        /// Đăng ký một phiên kết nối mới vào hệ thống quản lý.
        /// </summary>
        public void AddSession(ClientSession session)
        {
            _sessions.TryAdd(session.SessionId, session);
            Console.WriteLine($"[SESSION] Thêm {session} | Tổng trực tuyến: {OnlineCount}");
        }

        /// <summary>
        /// Gỡ bỏ một phiên kết nối khỏi hệ thống quản lý và đóng kết nối TCP.
        /// </summary>
        public void RemoveSession(ClientSession session)
        {
            if (_sessions.TryRemove(session.SessionId, out _))
            {
                session.Disconnect();
                Console.WriteLine($"[SESSION] Gỡ {session} | Tổng trực tuyến: {OnlineCount}");
            }
        }

        // ─── Truy vấn ───

        /// <summary>
        /// Tìm một phiên kết nối theo SessionId.
        /// </summary>
        public ClientSession? GetSessionById(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }

        /// <summary>
        /// Tìm phiên kết nối theo tên đăng nhập (Username).
        /// Trả về null nếu không có người chơi nào đang đăng nhập với tên này.
        /// </summary>
        public ClientSession? GetSessionByUsername(string username)
        {
            return _sessions.Values.FirstOrDefault(
                s => s.IsAuthenticated &&
                     string.Equals(s.Username, username, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Lấy danh sách tất cả các phiên đang kết nối (snapshot).
        /// </summary>
        public List<ClientSession> GetAllSessions()
        {
            return _sessions.Values.ToList();
        }

        /// <summary>
        /// Lấy danh sách tất cả các phiên đã đăng nhập thành công.
        /// </summary>
        public List<ClientSession> GetAuthenticatedSessions()
        {
            return _sessions.Values
                .Where(s => s.IsAuthenticated)
                .ToList();
        }

        /// <summary>
        /// Kiểm tra xem một username đã có người đăng nhập hay chưa.
        /// Dùng để ngăn đăng nhập trùng tài khoản.
        /// </summary>
        public bool IsUserOnline(string username)
        {
            return _sessions.Values.Any(
                s => s.IsAuthenticated &&
                     string.Equals(s.Username, username, StringComparison.OrdinalIgnoreCase)
            );
        }

        // ─── Dọn dẹp ───

        /// <summary>
        /// Ngắt kết nối và gỡ bỏ tất cả các phiên. Dùng khi shutdown server.
        /// </summary>
        public void DisconnectAll()
        {
            foreach (var session in _sessions.Values)
            {
                session.Disconnect();
            }
            _sessions.Clear();
            Console.WriteLine("[SESSION] Đã ngắt kết nối tất cả phiên.");
        }
    }
}
