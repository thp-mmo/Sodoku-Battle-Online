using System;

namespace Client.Network
{
    /// <summary>
    /// nhận packet từ server
    /// </summary>
    public class PacketReceiver
    {
        public void Process(string packet)
        {
            string[] parts = packet.Split('|');

            if (parts.Length == 0)
                return;

            switch (parts[0])
            {
                case "CHAT":
                    Console.WriteLine(
                        $"[{parts[1]}]: {parts[2]}"
                    );
                    break;

                case "LOGIN_OK":
                    Console.WriteLine("Đăng nhập thành công");
                    break;

                default:
                    Console.WriteLine(packet);
                    break;
            }
        }
    }
}