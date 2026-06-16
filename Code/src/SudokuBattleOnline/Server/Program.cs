using System;
using System.Text;
using System.Threading.Tasks;
using SudokuBattle.Server.Network;

namespace SudokuBattle.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Đồng bộ định dạng hiển thị ký tự UTF-8 trên Console
            Console.OutputEncoding = Encoding.UTF8;

            // Khởi tạo TCP Server tại cổng 8888
            TcpServer server = new TcpServer(port: 8888);

            // Bắt sự kiện Ctrl+C để tắt Server an toàn
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Ngăn tắt app đột ngột
                Console.WriteLine("\n[SERVER] Nhận tín hiệu tắt (Ctrl+C). Đang dừng Server...");
                server.Stop();
            };

            // Kích hoạt Server chạy bất đồng bộ
            await server.StartAsync();
        }
    }
}