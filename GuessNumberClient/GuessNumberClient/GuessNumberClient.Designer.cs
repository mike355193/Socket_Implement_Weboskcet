namespace GuessNumberClient
{
    partial class GuessNumberClient
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.BtnStart = new System.Windows.Forms.Button();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.lbStatus = new System.Windows.Forms.Label();
            this.BtnRestart = new System.Windows.Forms.Button();
            this.BtnLogout = new System.Windows.Forms.Button();
            this.textBoxInfo = new System.Windows.Forms.TextBox();
            this.textBoxGaming = new System.Windows.Forms.TextBox();
            this.lbGameStatus = new System.Windows.Forms.Label();
            this.lbInfo = new System.Windows.Forms.Label();
            this.BtnSendGuess = new System.Windows.Forms.Button();
            this.textBoxGuess = new System.Windows.Forms.TextBox();
            this.textBoxPlayer = new System.Windows.Forms.TextBox();
            this.BtnConnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnStart
            // 
            this.BtnStart.Enabled = false;
            this.BtnStart.Location = new System.Drawing.Point(387, 13);
            this.BtnStart.Name = "BtnStart";
            this.BtnStart.Size = new System.Drawing.Size(122, 23);
            this.BtnStart.TabIndex = 0;
            this.BtnStart.Text = "登入遊戲並等待開始";
            this.BtnStart.UseVisualStyleBackColor = true;
            this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Font = new System.Drawing.Font("宋体", 9F);
            this.textBoxStatus.Location = new System.Drawing.Point(13, 30);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxStatus.Size = new System.Drawing.Size(165, 379);
            this.textBoxStatus.TabIndex = 1;
            // 
            // lbStatus
            // 
            this.lbStatus.AutoSize = true;
            this.lbStatus.Location = new System.Drawing.Point(11, 15);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(65, 12);
            this.lbStatus.TabIndex = 2;
            this.lbStatus.Text = "連線狀態：";
            // 
            // BtnRestart
            // 
            this.BtnRestart.Enabled = false;
            this.BtnRestart.Location = new System.Drawing.Point(527, 13);
            this.BtnRestart.Name = "BtnRestart";
            this.BtnRestart.Size = new System.Drawing.Size(122, 23);
            this.BtnRestart.TabIndex = 3;
            this.BtnRestart.Text = "再玩一次";
            this.BtnRestart.UseVisualStyleBackColor = true;
            this.BtnRestart.Click += new System.EventHandler(this.BtnRestart_Click);
            // 
            // BtnLogout
            // 
            this.BtnLogout.Enabled = false;
            this.BtnLogout.Location = new System.Drawing.Point(665, 12);
            this.BtnLogout.Name = "BtnLogout";
            this.BtnLogout.Size = new System.Drawing.Size(122, 23);
            this.BtnLogout.TabIndex = 4;
            this.BtnLogout.Text = "不玩了，登出";
            this.BtnLogout.UseVisualStyleBackColor = true;
            this.BtnLogout.Click += new System.EventHandler(this.BtnLogout_Click);
            // 
            // textBoxInfo
            // 
            this.textBoxInfo.Location = new System.Drawing.Point(201, 71);
            this.textBoxInfo.Multiline = true;
            this.textBoxInfo.Name = "textBoxInfo";
            this.textBoxInfo.ReadOnly = true;
            this.textBoxInfo.Size = new System.Drawing.Size(359, 328);
            this.textBoxInfo.TabIndex = 5;
            // 
            // textBoxGaming
            // 
            this.textBoxGaming.Location = new System.Drawing.Point(583, 71);
            this.textBoxGaming.Multiline = true;
            this.textBoxGaming.Name = "textBoxGaming";
            this.textBoxGaming.ReadOnly = true;
            this.textBoxGaming.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxGaming.Size = new System.Drawing.Size(205, 367);
            this.textBoxGaming.TabIndex = 6;
            // 
            // lbGameStatus
            // 
            this.lbGameStatus.AutoSize = true;
            this.lbGameStatus.Location = new System.Drawing.Point(581, 56);
            this.lbGameStatus.Name = "lbGameStatus";
            this.lbGameStatus.Size = new System.Drawing.Size(65, 12);
            this.lbGameStatus.TabIndex = 7;
            this.lbGameStatus.Text = "遊戲狀態：";
            // 
            // lbInfo
            // 
            this.lbInfo.AutoSize = true;
            this.lbInfo.Location = new System.Drawing.Point(199, 56);
            this.lbInfo.Name = "lbInfo";
            this.lbInfo.Size = new System.Drawing.Size(65, 12);
            this.lbInfo.TabIndex = 8;
            this.lbInfo.Text = "遊戲說明：";
            // 
            // BtnSendGuess
            // 
            this.BtnSendGuess.Enabled = false;
            this.BtnSendGuess.Location = new System.Drawing.Point(486, 405);
            this.BtnSendGuess.Name = "BtnSendGuess";
            this.BtnSendGuess.Size = new System.Drawing.Size(75, 23);
            this.BtnSendGuess.TabIndex = 9;
            this.BtnSendGuess.Text = "猜數字！";
            this.BtnSendGuess.UseVisualStyleBackColor = true;
            this.BtnSendGuess.Click += new System.EventHandler(this.BtnSendGuess_Click);
            // 
            // textBoxGuess
            // 
            this.textBoxGuess.Enabled = false;
            this.textBoxGuess.Location = new System.Drawing.Point(293, 407);
            this.textBoxGuess.Name = "textBoxGuess";
            this.textBoxGuess.Size = new System.Drawing.Size(187, 21);
            this.textBoxGuess.TabIndex = 10;
            // 
            // textBoxPlayer
            // 
            this.textBoxPlayer.Enabled = false;
            this.textBoxPlayer.Location = new System.Drawing.Point(201, 15);
            this.textBoxPlayer.Name = "textBoxPlayer";
            this.textBoxPlayer.Size = new System.Drawing.Size(180, 21);
            this.textBoxPlayer.TabIndex = 11;
            this.textBoxPlayer.Text = "請輸入玩家名稱";
            // 
            // BtnConnect
            // 
            this.BtnConnect.Location = new System.Drawing.Point(30, 415);
            this.BtnConnect.Name = "BtnConnect";
            this.BtnConnect.Size = new System.Drawing.Size(122, 23);
            this.BtnConnect.TabIndex = 12;
            this.BtnConnect.Text = "連上Server";
            this.BtnConnect.UseVisualStyleBackColor = true;
            this.BtnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // GuessNumberClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BtnConnect);
            this.Controls.Add(this.textBoxPlayer);
            this.Controls.Add(this.textBoxGuess);
            this.Controls.Add(this.BtnSendGuess);
            this.Controls.Add(this.lbInfo);
            this.Controls.Add(this.lbGameStatus);
            this.Controls.Add(this.textBoxGaming);
            this.Controls.Add(this.textBoxInfo);
            this.Controls.Add(this.BtnLogout);
            this.Controls.Add(this.BtnRestart);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.BtnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "GuessNumberClient";
            this.Text = "GuessNumberClient";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnStart;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Button BtnRestart;
        private System.Windows.Forms.Button BtnLogout;
        private System.Windows.Forms.TextBox textBoxInfo;
        private System.Windows.Forms.TextBox textBoxGaming;
        private System.Windows.Forms.Label lbGameStatus;
        private System.Windows.Forms.Label lbInfo;
        private System.Windows.Forms.Button BtnSendGuess;
        private System.Windows.Forms.TextBox textBoxGuess;
        private System.Windows.Forms.TextBox textBoxPlayer;
        private System.Windows.Forms.Button BtnConnect;
    }
}

