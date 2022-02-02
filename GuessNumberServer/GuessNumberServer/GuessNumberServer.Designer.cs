namespace GuessNumberServer
{
    partial class GuessNumberServer
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
            this.btnInitServer = new System.Windows.Forms.Button();
            this.btnSendTest = new System.Windows.Forms.Button();
            this.btnSendBC = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.lbServerStatus = new System.Windows.Forms.Label();
            this.lbQuestion = new System.Windows.Forms.Label();
            this.textBoxGaming = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnInitServer
            // 
            this.btnInitServer.Location = new System.Drawing.Point(30, 34);
            this.btnInitServer.Name = "btnInitServer";
            this.btnInitServer.Size = new System.Drawing.Size(75, 23);
            this.btnInitServer.TabIndex = 0;
            this.btnInitServer.Text = "InitServer";
            this.btnInitServer.UseVisualStyleBackColor = true;
            this.btnInitServer.Click += new System.EventHandler(this.BtnInitServer_Click);
            // 
            // btnSendTest
            // 
            this.btnSendTest.Enabled = false;
            this.btnSendTest.Location = new System.Drawing.Point(126, 34);
            this.btnSendTest.Name = "btnSendTest";
            this.btnSendTest.Size = new System.Drawing.Size(75, 23);
            this.btnSendTest.TabIndex = 1;
            this.btnSendTest.Text = "SendText";
            this.btnSendTest.UseVisualStyleBackColor = true;
            this.btnSendTest.Click += new System.EventHandler(this.BtnSendTest_Click);
            // 
            // btnSendBC
            // 
            this.btnSendBC.Enabled = false;
            this.btnSendBC.Location = new System.Drawing.Point(225, 34);
            this.btnSendBC.Name = "btnSendBC";
            this.btnSendBC.Size = new System.Drawing.Size(75, 23);
            this.btnSendBC.TabIndex = 3;
            this.btnSendBC.Text = "SendBroadcast";
            this.btnSendBC.UseVisualStyleBackColor = true;
            this.btnSendBC.Click += new System.EventHandler(this.BtnSendBC_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Location = new System.Drawing.Point(326, 33);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect.TabIndex = 5;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.BtnDisconnect_Click);
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(30, 97);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxStatus.Size = new System.Drawing.Size(372, 295);
            this.textBoxStatus.TabIndex = 4;
            // 
            // lbServerStatus
            // 
            this.lbServerStatus.AutoSize = true;
            this.lbServerStatus.Location = new System.Drawing.Point(30, 79);
            this.lbServerStatus.Name = "lbServerStatus";
            this.lbServerStatus.Size = new System.Drawing.Size(77, 12);
            this.lbServerStatus.TabIndex = 6;
            this.lbServerStatus.Text = "ServerStatus";
            // 
            // lbQuestion
            // 
            this.lbQuestion.AutoSize = true;
            this.lbQuestion.Location = new System.Drawing.Point(434, 37);
            this.lbQuestion.Name = "lbQuestion";
            this.lbQuestion.Size = new System.Drawing.Size(83, 12);
            this.lbQuestion.TabIndex = 7;
            this.lbQuestion.Text = "Answer: _ _ _";
            // 
            // textBoxGaming
            // 
            this.textBoxGaming.Location = new System.Drawing.Point(436, 67);
            this.textBoxGaming.Multiline = true;
            this.textBoxGaming.Name = "textBoxGaming";
            this.textBoxGaming.ReadOnly = true;
            this.textBoxGaming.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxGaming.Size = new System.Drawing.Size(340, 325);
            this.textBoxGaming.TabIndex = 8;
            this.textBoxGaming.Text = "有兩人登入使得開始遊戲，這裡顯示兩名玩家的猜測狀態";
            // 
            // GuessNumberServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBoxGaming);
            this.Controls.Add(this.lbQuestion);
            this.Controls.Add(this.lbServerStatus);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.btnSendBC);
            this.Controls.Add(this.btnSendTest);
            this.Controls.Add(this.btnInitServer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "GuessNumberServer";
            this.Text = "GuessNumberServer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnInitServer;
        private System.Windows.Forms.Button btnSendTest;
        private System.Windows.Forms.Button btnSendBC;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Label lbServerStatus;
        private System.Windows.Forms.Label lbQuestion;
        private System.Windows.Forms.TextBox textBoxGaming;
    }
}

