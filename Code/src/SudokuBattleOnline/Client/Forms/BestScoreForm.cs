using System;
using System.Drawing;
using System.Windows.Forms;
using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;

namespace SudokuBattleOnline.Forms
{
    public class BestScoreForm : Form
    {
        private ListView listView;
        private Label lblStatus;

        public BestScoreForm()
        {
            Text = "Best Score";
            BackColor = Color.White;

            Label lblTitle = new Label();
            lblTitle.Text = "BEST SCORE - SINGLE PLAYER";
            lblTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(30, 25);

            lblStatus = new Label();
            lblStatus.Text = "Đang tải dữ liệu...";
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(30, 60);

            listView = new ListView();
            listView.Location = new Point(30, 90);
            listView.Size = new Size(650, 400);
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;

            listView.Columns.Add("Rank", 60);
            listView.Columns.Add("Username", 120);
            listView.Columns.Add("Difficulty", 100);
            listView.Columns.Add("Best Score", 100);
            listView.Columns.Add("Best Time", 100);
            listView.Columns.Add("Achieved At", 160);

            Controls.Add(lblTitle);
            Controls.Add(lblStatus);
            Controls.Add(listView);

            Load += BestScoreForm_Load;
        }

        private async void BestScoreForm_Load(object sender, EventArgs e)
        {
            try
            {
                var request = new BestScorePacket
                {
                    PacketType = "BEST_SCORE_REQUEST"
                };

                BestScorePacket? response =
                    await AppSession.SendAndWaitAsync<BestScorePacket>(
                        request,
                        "BEST_SCORE_RESULT"
                    );

                listView.Items.Clear();

                if (response == null)
                {
                    lblStatus.Text = "Không nhận được phản hồi từ Server.";
                    return;
                }

                if (!response.Success)
                {
                    lblStatus.Text = response.Message;
                    return;
                }

                if (response.Scores.Count == 0)
                {
                    lblStatus.Text = "Chưa có dữ liệu Best Score.";
                    return;
                }

                lblStatus.Text = $"Tìm thấy {response.Scores.Count} kết quả.";

                foreach (var score in response.Scores)
                {
                    ListViewItem item = new ListViewItem(score.Rank.ToString());
                    item.SubItems.Add(score.Username);
                    item.SubItems.Add(score.Difficulty);
                    item.SubItems.Add(score.BestScore.ToString());
                    item.SubItems.Add(score.BestTimeSeconds + "s");
                    item.SubItems.Add(score.AchievedAt);

                    listView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải Best Score: " + ex.Message;
            }
        }
    }
}