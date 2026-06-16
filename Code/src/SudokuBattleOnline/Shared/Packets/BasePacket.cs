namespace SudokuBattleOnline.Shared.Packets
{
    /// <summary>
    /// Lớp gói tin gốc - tất cả các Packet đều kế thừa từ đây.
    /// Chứa PacketType để định tuyến và Success/Message để phản hồi kết quả.
    /// </summary>
    public class BasePacket
    {
        /// <summary>
        /// Loại gói tin (ví dụ: "LOGIN", "REGISTER", "LOGIN_RESULT"...).
        /// Dùng để PacketRouter định tuyến tới handler phù hợp.
        /// </summary>
        public string PacketType { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái phản hồi: true = thành công, false = thất bại.
        /// Chỉ có ý nghĩa khi gói tin được gửi từ Server về Client (phản hồi).
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Thông điệp mô tả kết quả xử lý.
        /// Ví dụ: "Đăng nhập thành công", "Sai mật khẩu"...
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}