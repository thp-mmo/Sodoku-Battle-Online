using System;
using System.Threading;
using System.Threading.Tasks;
using SudokuBattle.Server.Network;
using SudokuBattleOnline.Shared.Packets;

namespace SudokuBattle.Server.Matchmaking
{
    /// <summary>
    /// quản lý cái loop ghép người chơi trên server này, lần lượt lấy cố định 2 người chơi từ hàng đợi và tạo trận
    /// </summary>
    public class MatchmakingManager
    {
        private readonly MatchmakingQueue _queue;
        private bool _isRunning;

        public MatchmakingManager(MatchmakingQueue queue)
        {
            _queue = queue;
        }

        /// <summary>
        /// bắt đầu sẽ gọi MatchmakingLoopAsync để xử lý ghép cặp liên tục
        /// </summary>
        public void Start()
        {
            _isRunning = true;
            Task.Run(MatchmakingLoopAsync);
            Console.WriteLine("[MATCHMAKING] Đã khởi động trình quản lý ghép cặp.");
        }

        /// <summary>
        /// dừng loop ghép
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
        }

        private async Task MatchmakingLoopAsync()
        {
            while (_isRunning)
            {
                try
                {
                    // loop cho tới khi có 2 người
                    if (_queue.Count >= 2)
                    {
                        var player1 = _queue.Dequeue();
                        var player2 = _queue.Dequeue();

                        if (player1 != null && player2 != null)
                        {
                            bool p1Connected = player1.IsConnected;
                            bool p2Connected = player2.IsConnected;

                            if (p1Connected && p2Connected)
                            {
                                // ghép thành công và tạo 1 mã phòng tạm thời dài 8 ký tự
                                string roomId = Guid.NewGuid().ToString("N")[..8];
                                player1.CurrentRoomId = roomId;
                                player2.CurrentRoomId = roomId;

                                Console.WriteLine($"[MATCHMAKING] Đã ghép cặp '{player1.Username}' và '{player2.Username}' vào phòng {roomId}");

                                // gửi thông báo ghép thành công cho player 1
                                await player1.SendPacketAsync(new MatchFoundPacket
                                {
                                    RoomId = roomId,
                                    OpponentUsername = player2.Username ?? "Unknown",
                                    Success = true,
                                    Message = "Đã tìm thấy đối thủ!"
                                });

                                // gửi thông báo ghép thành công cho player 2
                                await player2.SendPacketAsync(new MatchFoundPacket
                                {
                                    RoomId = roomId,
                                    OpponentUsername = player1.Username ?? "Unknown",
                                    Success = true,
                                    Message = "Đã tìm thấy đối thủ!"
                                });
                            }
                            else
                            {
                                // nếu một người mất kết nối thì cho thằng kia về hàng chờ
                                if (p1Connected) _queue.Enqueue(player1);
                                if (p2Connected) _queue.Enqueue(player2);
                            }
                        }
                    }

                    // delay 1 giây để tránh cpu chạy 100%
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MATCHMAKING LỖI] {ex.Message}");
                }
            }
        }
    }
}
