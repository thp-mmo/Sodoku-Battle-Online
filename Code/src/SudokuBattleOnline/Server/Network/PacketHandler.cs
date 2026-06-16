using System;
using System.Threading.Tasks;
using SudokuBattleOnline.Shared.Packets;

namespace SudokuBattle.Server.Network
{
    /// <summary>
    /// Xử lý logic nghiệp vụ cho từng loại gói tin nhận được từ client.
    /// Mỗi phương thức Handle*Async tương ứng với một PacketType cụ thể.
    /// Tầng này nằm giữa PacketRouter (định tuyến) và Service (truy xuất dữ liệu).
    /// </summary>
    public class PacketHandler
    {
        private readonly SessionManager _sessionManager;

        public PacketHandler(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        // ═══════════════════════════════════════════════
        //  XÁC THỰC (Authentication)
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Xử lý yêu cầu đăng nhập từ client.
        /// - Kiểm tra username/password.
        /// - Ngăn đăng nhập trùng tài khoản đang online.
        /// - Gửi phản hồi thành công/thất bại.
        /// </summary>
        public async Task HandleLoginAsync(ClientSession session, LoginPacket packet)
        {
            Console.WriteLine($"[LOGIN] {session} yêu cầu đăng nhập: Username='{packet.Username}'");

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(packet.Username) || string.IsNullOrWhiteSpace(packet.Password))
            {
                await session.SendPacketAsync(new LoginPacket
                {
                    PacketType = "LOGIN_RESULT",
                    Success = false,
                    Message = "Username và Password không được để trống."
                });
                return;
            }

            string username = packet.Username.Trim();
            string password = packet.Password.Trim();

            // Kiểm tra tài khoản đã đang online chưa
            if (_sessionManager.IsUserOnline(username))
            {
                await session.SendPacketAsync(new LoginPacket
                {
                    PacketType = "LOGIN_RESULT",
                    Success = false,
                    Message = "Tài khoản này đang được đăng nhập ở nơi khác."
                });
                return;
            }

            // TODO: Thay thế bằng truy vấn Database thực tế (UserRepository)
            // Hiện tại dùng tài khoản test cứng để kiểm thử luồng mạng
            bool isValid = (username == "admin" && password == "123456")
                        || (username == "test" && password == "test");

            if (isValid)
            {
                session.Username = username;
                session.IsAuthenticated = true;

                await session.SendPacketAsync(new LoginPacket
                {
                    PacketType = "LOGIN_RESULT",
                    Username = username,
                    Success = true,
                    Message = $"Đăng nhập thành công! Chào mừng {username}."
                });
                Console.WriteLine($"[LOGIN] ✓ {session} đăng nhập thành công.");
            }
            else
            {
                await session.SendPacketAsync(new LoginPacket
                {
                    PacketType = "LOGIN_RESULT",
                    Success = false,
                    Message = "Sai tên đăng nhập hoặc mật khẩu."
                });
                Console.WriteLine($"[LOGIN] ✗ {session} đăng nhập thất bại.");
            }
        }

        /// <summary>
        /// Xử lý yêu cầu đăng ký tài khoản mới.
        /// </summary>
        public async Task HandleRegisterAsync(ClientSession session, RegisterPacket packet)
        {
            Console.WriteLine($"[REGISTER] {session} yêu cầu đăng ký: Username='{packet.Username}'");

            if (string.IsNullOrWhiteSpace(packet.Username) || string.IsNullOrWhiteSpace(packet.Password))
            {
                await session.SendPacketAsync(new RegisterPacket
                {
                    PacketType = "REGISTER_RESULT",
                    Success = false,
                    Message = "Username và Password không được để trống."
                });
                return;
            }

            if (packet.Password != packet.ConfirmPassword)
            {
                await session.SendPacketAsync(new RegisterPacket
                {
                    PacketType = "REGISTER_RESULT",
                    Success = false,
                    Message = "Mật khẩu xác nhận không khớp."
                });
                return;
            }

            // TODO: Lưu vào Database thực tế (UserRepository)
            // Kiểm tra trùng username, hash password, lưu vào DB

            await session.SendPacketAsync(new RegisterPacket
            {
                PacketType = "REGISTER_RESULT",
                Success = true,
                Message = "Đăng ký tài khoản thành công! Vui lòng đăng nhập."
            });
            Console.WriteLine($"[REGISTER] ✓ Đăng ký thành công cho '{packet.Username}'.");
        }

        // ═══════════════════════════════════════════════
        //  PHÒNG CHƠI (Room Management)
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Xử lý yêu cầu tạo phòng mới.
        /// </summary>
        public async Task HandleCreateRoomAsync(ClientSession session, CreateRoomPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[ROOM] {session} tạo phòng: '{packet.RoomName}'");

            // TODO: Tạo phòng qua RoomManager, lưu vào danh sách phòng
            await session.SendPacketAsync(new CreateRoomPacket
            {
                PacketType = "CREATE_ROOM_RESULT",
                RoomName = packet.RoomName,
                Success = true,
                Message = $"Đã tạo phòng '{packet.RoomName}' thành công."
            });
        }

        /// <summary>
        /// Xử lý yêu cầu tham gia phòng.
        /// </summary>
        public async Task HandleJoinRoomAsync(ClientSession session, JoinRoomPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[ROOM] {session} tham gia phòng: {packet.RoomId}");

            // TODO: Tìm phòng qua RoomManager, thêm người chơi
            await session.SendPacketAsync(new JoinRoomPacket
            {
                PacketType = "JOIN_ROOM_RESULT",
                RoomId = packet.RoomId,
                Success = true,
                Message = $"Đã tham gia phòng {packet.RoomId}."
            });
        }

        /// <summary>
        /// Xử lý yêu cầu rời phòng.
        /// </summary>
        public async Task HandleLeaveRoomAsync(ClientSession session, LeaveRoomPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[ROOM] {session} rời phòng: {packet.RoomId}");

            session.CurrentRoomId = null;

            // TODO: Gỡ người chơi khỏi phòng qua RoomManager
            await session.SendPacketAsync(new LeaveRoomPacket
            {
                PacketType = "LEAVE_ROOM_RESULT",
                RoomId = packet.RoomId,
                Success = true,
                Message = "Đã rời phòng."
            });
        }

        // ═══════════════════════════════════════════════
        //  TRẬN ĐẤU (Matchmaking & Gameplay)
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Xử lý yêu cầu tìm trận.
        /// </summary>
        public async Task HandleFindMatchAsync(ClientSession session, FindMatchPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[MATCH] {session} yêu cầu tìm trận.");

            // TODO: Đưa vào MatchmakingQueue, khi đủ 2 người -> tạo GameRoom
            // Khi tìm được, gửi MatchFoundPacket cho cả 2 client
            await session.SendPacketAsync(new FindMatchPacket
            {
                PacketType = "FIND_MATCH",
                Success = true,
                Message = "Đang tìm đối thủ... Vui lòng chờ."
            });
        }

        /// <summary>
        /// Xử lý cập nhật ô Sudoku từ người chơi trong trận đấu.
        /// </summary>
        public async Task HandleCellUpdateAsync(ClientSession session, CellUpdatePacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[GAME] {session} cập nhật ô [{packet.Row},{packet.Col}] = {packet.Value}");

            // TODO: Validate ô, cập nhật GameRoom, gửi progress cho đối thủ
            await Task.CompletedTask;
        }

        // ═══════════════════════════════════════════════
        //  CHAT
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Xử lý tin nhắn chat.
        /// </summary>
        public async Task HandleChatAsync(ClientSession session, ChatPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[CHAT] {session}: {packet.Content}");

            // TODO: Chuyển tiếp tin nhắn tới các thành viên cùng phòng
            await Task.CompletedTask;
        }

        // ═══════════════════════════════════════════════
        //  RANKING (Bảng xếp hạng)
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Xử lý yêu cầu lấy bảng xếp hạng.
        /// </summary>
        public async Task HandleRankingAsync(ClientSession session, RankingPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[RANKING] {session} yêu cầu bảng xếp hạng.");

            // TODO: Truy vấn Database lấy danh sách ranking
            await session.SendPacketAsync(new RankingPacket
            {
                Success = true,
                Message = "Lấy bảng xếp hạng thành công.",
                Rankings = new System.Collections.Generic.List<RankingEntry>()
                // Sẽ đổ dữ liệu thực khi có Database
            });
        }

        // ═══════════════════════════════════════════════
        //  HEARTBEAT (Ping/Pong)
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Phản hồi gói tin Ping từ client bằng Pong.
        /// Giúp duy trì kết nối và phát hiện client đã mất kết nối.
        /// </summary>
        public async Task HandlePingAsync(ClientSession session)
        {
            await session.SendPacketAsync(new BasePacket { PacketType = "PONG" });
        }

        // ═══════════════════════════════════════════════
        //  TIỆN ÍCH NỘI BỘ
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Kiểm tra xem session đã đăng nhập chưa.
        /// Nếu chưa, tự động gửi phản hồi lỗi và trả về false.
        /// Dùng ở đầu mỗi handler yêu cầu xác thực.
        /// </summary>
        private async Task<bool> RequireAuthAsync(ClientSession session)
        {
            if (!session.IsAuthenticated)
            {
                await session.SendPacketAsync(new BasePacket
                {
                    PacketType = "AUTH_REQUIRED",
                    Success = false,
                    Message = "Bạn cần đăng nhập trước khi thực hiện thao tác này."
                });
                Console.WriteLine($"[AUTH] {session} bị từ chối: chưa đăng nhập.");
                return false;
            }
            return true;
        }
    }
}
