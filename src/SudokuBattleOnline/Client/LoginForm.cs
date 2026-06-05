using System;
using System.Drawing;
using System.Windows.Forms;
using Shared.Models; // Đảm bảo đã tham chiếu tới thư mục Models chứa Board.cs và Cell.cs

namespace Client
{
    public partial class LoginForm : Form
    {
        // Đổi từ 'const' thành biến thường để có thể thay đổi khi kéo giãn màn hình
        private int cellSize = 45;
        private const int MinCellSize = 25;  // Kích thước ô tối thiểu để không bị lỗi giao diện
        private const int BlockGap = 5;
        private const int MarginSize = 15;
        private const int SidebarWidth = 160; // Khoảng trống cố định bên phải dành cho Timer và các chức năng khác

        private TextBox[,] uiCells = new TextBox[9, 9];
        private Panel[,] uiBlocks = new Panel[3, 3]; // Mảng quản lý 9 khối lớn 3x3 để dịch chuyển vị trí
        private Board gameBoard;

        private Label lblTimer;       // Label dùng để hiển thị thời gian số
        private int totalSeconds = 600; // 10p chơi game

        // SỬA LỖI 1: Chỉ định rõ System.Windows.Forms.Timer để không bị trùng lặp với System.Threading hay System.Timers
        private System.Windows.Forms.Timer gameTimer;
        private int secondsElapsed = 0; // Biến đếm tổng số giây đã trôi qua

        private bool isInitializing = true; // Cờ chặn sự kiện resize chạy bậy khi chưa load xong Form

        public LoginForm()
        {
            InitializeComponent();

            // Đăng ký sự kiện Load bàn cờ khi Form mở lên
            this.Load += LoginForm_Load;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Khởi tạo thực thể bàn cờ từ Shared Project
            gameBoard = new Board();

            // Setup dữ liệu giả lập để test giao diện
            SetupDemoData();

            // Cấu hình kích thước cửa sổ game và cho phép phóng to
            SetupFormWindow();

            // Tự động vẽ 81 ô lên màn hình
            BuildSudokuUI();

            // Đã khởi tạo xong, bật cờ và bắt đầu lắng nghe sự kiện thay đổi kích thước (Resize)
            isInitializing = false;
            this.Resize += LoginForm_Resize;
        }

        private void SetupDemoData()
        {
            // Gán thử vài ô cố định (đề bài) xem hiển thị đúng không
            gameBoard.Cells[0, 0].Value = 5; gameBoard.Cells[0, 0].IsFixed = true;
            gameBoard.Cells[0, 1].Value = 3; gameBoard.Cells[0, 1].IsFixed = true;
            gameBoard.Cells[1, 1].Value = 6; gameBoard.Cells[1, 1].IsFixed = true;
            gameBoard.Cells[4, 4].Value = 9; gameBoard.Cells[4, 4].IsFixed = false;
        }

        private void SetupFormWindow()
        {
            // Tính toán kích thước bàn cờ mặc định ban đầu
            int boardWidth = (cellSize * 9) + (BlockGap * 2) + (MarginSize * 2);
            int boardHeight = boardWidth + 40;

            // Kích thước chiều rộng của Form sẽ bằng chiều rộng bàn cờ + khoảng trống Sidebar bên phải
            this.Size = new Size(boardWidth + SidebarWidth, boardHeight);
            this.BackColor = Color.FromArgb(50, 50, 50); // Nền xám đậm tạo đường viền lớn rõ ràng
            this.Text = "Sudoku Battle Online";

            // Cho phép kéo giãn (Sizable) và hiện nút Phóng to (Maximize)
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;

            // Đặt kích thước giới hạn nhỏ nhất (được nới rộng chiều rộng để chứa thanh thời gian)
            this.MinimumSize = new Size(520, 450);

            // 1. Tạo và cấu hình Label hiển thị thời gian (Đã chỉnh sửa để đóng khung)
            lblTimer = new Label();
            lblTimer.Text = "Thời gian:\n00:00";
            lblTimer.Font = new Font("Segoe UI", 14, FontStyle.Bold);

            // --- ĐOẠN MỚI THÊM: TẠO KHUNG NỀN TRẮNG CHỮ ĐEN ---
            lblTimer.BackColor = Color.White;          // Nền trắng
            lblTimer.ForeColor = Color.Black;          // Chữ đen
            lblTimer.BorderStyle = BorderStyle.FixedSingle; // Đóng khung viền đơn mảnh
            lblTimer.Padding = new Padding(10, 8, 10, 8);   // Tạo khoảng trống đệm giúp hộp đẹp hơn
            // --------------------------------------------------

            lblTimer.AutoSize = true;

            this.Controls.Add(lblTimer);

            // 2. Khởi tạo Timer (SỬA LỖI 1: Khởi tạo tường minh bằng từ khóa đầy đủ)
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 1000; // 1000 mili-giây = 1 giây kích hoạt 1 lần
            gameTimer.Tick += GameTimer_Tick; // Đăng ký sự kiện chạy mỗi giây
            gameTimer.Start(); // Chạy đồng hồ luôn khi vào game
        }

        /// <summary>
        /// Hàm cốt lõi: Tự động tính toán lại vị trí, kích thước bàn cờ và font chữ theo size của Form
        /// </summary>
        private void UpdateLayoutPositions()
        {
            if (lblTimer == null) return;

            // Chiều rộng khả dụng của bàn cờ phải trừ thêm khoảng trống SidebarWidth bên phải
            int availableWidth = this.ClientSize.Width - (MarginSize * 2) - (BlockGap * 2) - SidebarWidth;
            int availableHeight = this.ClientSize.Height - (MarginSize * 2) - (BlockGap * 2);

            // Vì Sudoku bắt buộc phải là hình vuông, ta lấy cạnh ngắn hơn làm chuẩn để tính toán
            int shortestEdge = Math.Min(availableWidth, availableHeight);

            // Chia đều cho 9 để ra kích thước mới của 1 ô (Cell)
            int calculatedCellSize = shortestEdge / 9;

            // Nếu thu quá nhỏ thì giữ nguyên ở mức MinCellSize nhằm tránh lỗi giao diện
            if (calculatedCellSize < MinCellSize)
                calculatedCellSize = MinCellSize;

            cellSize = calculatedCellSize;

            // Tính toán tổng kích thước mới của toàn bộ bàn cờ sau khi co giãn
            int totalBoardWidth = (cellSize * 9) + (BlockGap * 2);
            int totalBoardHeight = totalBoardWidth;

            // Tính toán vị trí bàn cờ tập trung ở nửa phần bên trái của màn hình (chừa lại Sidebar)
            int startX = MarginSize + (availableWidth - totalBoardWidth) / 2;
            int startY = MarginSize + (availableHeight - totalBoardHeight) / 2;

            // Cập nhật vị trí cho thanh hiển thị Thời gian luôn nằm ở thanh Sidebar bên phải, cách bàn cờ 20px
            int timerX = this.ClientSize.Width - SidebarWidth + 20;
            lblTimer.Location = new Point(timerX, startY + 10);

            // Duyệt qua và cập nhật lại vị trí/kích thước cho từng Block và từng Ô TextBox con
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    Panel blockPanel = uiBlocks[blockRow, blockCol];
                    if (blockPanel == null) continue;

                    // 1. Cập nhật kích thước và vị trí mới cho khối 3x3
                    blockPanel.Size = new Size(cellSize * 3, cellSize * 3);
                    int posX = startX + blockCol * (cellSize * 3 + BlockGap);
                    int posY = startY + blockRow * (cellSize * 3 + BlockGap);
                    blockPanel.Location = new Point(posX, posY);

                    // 2. Cập nhật kích thước, vị trí và cỡ chữ cho 9 ô TextBox bên trong khối đó
                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int actualRow = blockRow * 3 + r;
                            int actualCol = blockCol * 3 + c;
                            TextBox txtCell = uiCells[actualRow, actualCol];

                            if (txtCell != null)
                            {
                                txtCell.Size = new Size(cellSize, cellSize);
                                txtCell.Location = new Point(c * cellSize, r * cellSize);

                                // Tính toán lại kích thước chữ (Font Size) tỉ lệ thuận theo kích thước ô (khoảng 38%)
                                float newFontSize = Math.Max(10, cellSize * 0.38f);
                                txtCell.Font = new Font("Segoe UI", newFontSize, FontStyle.Bold);
                            }
                        }
                    }
                }
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // 1. Kiểm tra nếu thời gian vẫn còn (> 0)
            if (totalSeconds > 0)
            {
                // Giảm đi 1 giây mỗi khi hàm này được kích hoạt (mỗi 1000ms)
                totalSeconds--;

                // 2. Tính toán số phút và số giây còn lại từ tổng số giây
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                // 3. Cập nhật lên giao diện (lblTimer)
                lblTimer.Text = $"Thời gian:\n{minutes:D2}:{seconds:D2}";
            }
            else
            {
                // 4. Xử lý khi hết thời gian
                gameTimer.Stop(); // Dừng đồng hồ lại không cho chạy nữa

                // Hiển thị thông báo thua cuộc hoặc kết thúc game
                MessageBox.Show("Hết giờ rồi! Bạn đã thua cuộc.", "Kết thúc", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BuildSudokuUI()
        {
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    // Tạo panel cho khối lớn 3x3
                    Panel blockPanel = new Panel();
                    blockPanel.BorderStyle = BorderStyle.FixedSingle;
                    blockPanel.BackColor = Color.White;

                    // Lưu panel vào mảng quản lý để cập nhật lại vị trí khi Resize
                    uiBlocks[blockRow, blockCol] = blockPanel;

                    // Tạo 9 ô con trong khối lớn
                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int actualRow = blockRow * 3 + r;
                            int actualCol = blockCol * 3 + c;
                            Cell cellLogic = gameBoard.Cells[actualRow, actualCol];

                            TextBox txtCell = new TextBox();
                            txtCell.Multiline = true;
                            txtCell.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                            txtCell.TextAlign = HorizontalAlignment.Center;
                            txtCell.MaxLength = 1;

                            if (cellLogic.Value > 0)
                            {
                                txtCell.Text = cellLogic.Value.ToString();
                            }

                            if (cellLogic.IsFixed)
                            {
                                txtCell.ReadOnly = true;
                                txtCell.BackColor = Color.FromArgb(230, 230, 230);
                                txtCell.ForeColor = Color.Black;
                            }
                            else
                            {
                                txtCell.BackColor = Color.White;
                                txtCell.ForeColor = Color.RoyalBlue;

                                txtCell.KeyPress += TxtCell_KeyPress;
                                txtCell.TextChanged += (s, ev) => TxtCell_TextChanged(s, ev, cellLogic);
                            }

                            uiCells[actualRow, actualCol] = txtCell;
                            blockPanel.Controls.Add(txtCell);
                        }
                    }
                    this.Controls.Add(blockPanel);
                }
            }

            // Gọi hàm tính toán kích thước và vị trí lần đầu tiên khi dựng UI
            UpdateLayoutPositions();
        }

        // Sự kiện kích hoạt liên tục khi người dùng cầm chuột kéo giãn cửa sổ hoặc bấm Toàn màn hình
        private void LoginForm_Resize(object sender, EventArgs e)
        {
            if (isInitializing) return;

            // Tạm dừng vẽ giao diện trong tích tắc để mượt hơn, tránh hiện tượng nhấp nháy màn hình (Flicker)
            this.SuspendLayout();

            UpdateLayoutPositions();

            // Tiếp tục vẽ lại giao diện sau khi đã tính toán xong vị trí mới
            this.ResumeLayout();
        }

        private void TxtCell_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) || e.KeyChar == '0')
            {
                if (e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                }
            }
        }

        private void TxtCell_TextChanged(object sender, EventArgs e, Cell cellLogic)
        {
            TextBox txt = (TextBox)sender;
            if (int.TryParse(txt.Text, out int num))
            {
                cellLogic.Value = num;
            }
            else
            {
                cellLogic.Value = 0;
            }
        }
    }
}