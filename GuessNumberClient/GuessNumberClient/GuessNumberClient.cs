using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuessNumberClient
{
    public partial class GuessNumberClient : Form
    {
        static PlayerClient m_Client= new PlayerClient();
        //委派Handler
        delegate void UpdateTextBoxHandler(TextBox tb, string sPopResp);
        delegate void EnableObjectHandler(Control obj, bool bAble);
        delegate void ClearTextBoxHandler(TextBox tb);
        bool m_bInitClient = false;
        Thread m_tdUpdateTextBox;
        static ManualResetEvent m_eventCloseUpdate;
        bool m_bHasLogin = false;

        public GuessNumberClient()
        {
            const string sNewLine = "\r\n";
            string sInfo = "↑↑↑輸入你的遊戲名稱並且登入↑↑↑" + sNewLine + sNewLine + sNewLine +
                            "需有兩名玩家使得開始遊戲。" + sNewLine + sNewLine +
                            "開始遊戲後，系統將隨機產生出三個數字不重複為答案。" + sNewLine + sNewLine +
                            "玩家照順序(依進房順序)的開始猜測數字。" + sNewLine + sNewLine +
                            "數字正確、位置正確為A；數字正確、位置錯誤為B" + sNewLine + sNewLine +
                            "==========================================================" + sNewLine + sNewLine +
                            "範例：        假設答案為258↓" + sNewLine + sNewLine +
                            "你猜了 123，[1]數字正確、位置錯誤→結果輸出為0A1B" + sNewLine + sNewLine +
                            "你猜了 214，[1]數字正確、位置正確→結果輸出為1A0B" + sNewLine + sNewLine +
                            "你猜了 280，[1]數字正確、位置正確；[1]數字正確、位置錯誤→結果輸出為1A1B" + sNewLine + sNewLine +
                            "你猜了 285，[1]數字正確、位置正確；[2]數字正確、位置錯誤→結果輸出為1A2B" + sNewLine + sNewLine +
                            "祝遊戲愉快！           輸入三個數字↓↓↓輸入後送出↓↓↓"
                            ;
            InitializeComponent();
            UpdateTextBox(textBoxInfo, sInfo);

            m_eventCloseUpdate = new ManualResetEvent(false);
            m_tdUpdateTextBox = new Thread(UpdateTextBoxProc);
            m_tdUpdateTextBox.IsBackground = true;
            m_tdUpdateTextBox.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(true == m_bInitClient)
            {
                //關閉前先登出
                BtnLogout_Click(this, null);
                m_bInitClient = false;
                m_Client.Stop();
                m_eventCloseUpdate.Set();
                m_tdUpdateTextBox.Abort();
            }
            base.OnFormClosing(e);
            // Code
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (true == m_Client.Start())
            {
                EnableObject(BtnConnect, false);
            }
            else
            {
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if(false == m_bInitClient)
            {
                return;
            }
            string sPlayerName = textBoxPlayer.Text;
            if(0 == sPlayerName.Length)
            {
                return;
            }
            m_Client.SendLogin(sPlayerName);
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            if (false == m_bInitClient)
            {
                return;
            }
            string sPlayerName = textBoxPlayer.Text;
            if (0 == sPlayerName.Length)
            {
                return;
            }
            m_Client.SendRestart(sPlayerName);
            EnableObject(BtnRestart, false);
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (false == m_bInitClient || false == m_bHasLogin)
            {
                return;
            }

            string sPlayerName = textBoxPlayer.Text;
            if (0 == sPlayerName.Length)
            {
                return;
            }
            m_Client.SendLogout(sPlayerName);
        }

        private void BtnSendGuess_Click(object sender, EventArgs e)
        {
            string sGuessString = textBoxGuess.Text;
            if(3 != sGuessString.Length)
            {
                UpdateTextBox(textBoxGaming, "僅能輸入三個數字！");
                return;
            }
            try
            {
                int iGuessString = Int32.Parse(sGuessString);
            }
            catch
            {
                UpdateTextBox(textBoxGaming, "請輸入純數字三個！");
                return;
            }
            int iNum1 = Int32.Parse(sGuessString.Substring(0, 1));
            int iNum2 = Int32.Parse(sGuessString.Substring(1, 1));
            int iNum3 = Int32.Parse(sGuessString.Substring(2, 1));

            if(iNum1 == iNum2 || iNum1 == iNum3 || iNum2 == iNum3)
            {
                UpdateTextBox(textBoxGaming, "輸入數字不得重複！");
                return;
            }

            m_Client.SendGuess(sGuessString);
            EnableObject(textBoxGuess, false);
            EnableObject(BtnSendGuess, false);
        }

        private void UpdateTextBoxProc()
        {
            string sPopResp = "";
            while (true)
            {
                if (m_eventCloseUpdate.WaitOne(0))
                {
                    break;
                }
                sPopResp = m_Client.PopRespQueue();
                if ("NO_DATA" == sPopResp)
                {
                    Thread.Sleep(1);
                    continue;
                }
                else
                {
                    //如何從Thread回到MainThread改值，委派
                    ParseRealData(sPopResp);
                }
                Thread.Sleep(1);
            }
        }

        private void UpdateTextBox(TextBox tb, string sPopResp)
        {
            if (true == tb.InvokeRequired)
            {
                UpdateTextBoxHandler uh = new UpdateTextBoxHandler(UpdateTextBox);
                tb.Invoke(uh, tb, sPopResp);
            }
            else
            {
                tb.AppendText(sPopResp + "\r\n");
            }
        }

        private void ClearTextBox(TextBox tb)
        {
            if (true == tb.InvokeRequired)
            {
                ClearTextBoxHandler ch = new ClearTextBoxHandler(ClearTextBox);
                tb.Invoke(ch, tb);
            }
            else
            {
                tb.Clear();
            }
        }

        private void EnableObject(Control obj, bool bAble)
        {
            if (true == obj.InvokeRequired)
            {
                EnableObjectHandler eo = new EnableObjectHandler(EnableObject);
                obj.Invoke(eo, obj, bAble);
            }
            else
            {
                obj.Enabled = bAble;
            }
        }

        private void ParseRealData(string sRealData)
        {
            if (true == sRealData.Contains("Status"))
            {
                if(true == sRealData.Contains("登入成功"))
                {
                    EnableObject(BtnStart, false);
                    EnableObject(textBoxPlayer, false);
                    EnableObject(BtnLogout, true);
                    m_bHasLogin = true;
                }
                if (true == sRealData.Contains("登出成功"))
                {
                    EnableObject(BtnStart, true);
                    EnableObject(textBoxPlayer, true);
                    EnableObject(BtnLogout, false);
                    EnableObject(BtnRestart, false);
                    m_bHasLogin = false;
                }
                if (true == sRealData.Contains("遊戲主機成功"))
                {
                    ClearTextBox(textBoxStatus);
                    m_bInitClient = true;
                    EnableObject(BtnStart, true);
                    EnableObject(textBoxPlayer, true);
                    //if()
                    //{
                    //    EnableObject(BtnRestart, true);
                    //}
                }
                if (true == sRealData.Contains("遊戲主機斷線"))
                {
                    m_bInitClient = false;
                    EnableObject(textBoxPlayer, false);
                    EnableObject(BtnStart, false);
                    EnableObject(BtnRestart, false);
                    EnableObject(BtnLogout, false);
                    EnableObject(textBoxGuess, false);
                    EnableObject(BtnSendGuess, false);
                }
                UpdateTextBox(textBoxStatus, sRealData);
            }
            else if(true == sRealData.Contains("Gaming"))
            {
                if (true == sRealData.Contains("結束比賽"))
                {
                    EnableObject(textBoxGuess, false);
                    EnableObject(BtnSendGuess, false);
                    if (true == m_bHasLogin)
                    {
                        EnableObject(BtnRestart, true);
                    }
                }
                if ((true == sRealData.Contains("開始") || true == sRealData.Contains("繼續")) && true == sRealData.Contains("輪"))
                {
                    EnableObject(BtnRestart, false);
                    if ((true == sRealData.Contains("開始")))
                    {
                        ClearTextBox(textBoxGaming);
                    }
                }
                if (true == sRealData.Contains("先") || true == sRealData.Contains("換")) //判斷是否可以進行猜測
                {
                    if(true == sRealData.Contains(textBoxPlayer.Text))
                    {
                        EnableObject(textBoxGuess, true);
                        EnableObject(BtnSendGuess, true);
                    }
                }
                UpdateTextBox(textBoxGaming, sRealData);
            }
            else
            {

            }
        }
    }
}
