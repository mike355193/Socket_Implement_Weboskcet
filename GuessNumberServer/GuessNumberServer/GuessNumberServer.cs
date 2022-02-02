using System;
using System.Threading;
using System.Windows.Forms;

namespace GuessNumberServer
{
    public partial class GuessNumberServer : Form
    {
        GameServer m_Server = new GameServer();
        //委派Handler
        delegate void UpdateHandler(TextBox tb, string text);
        delegate void SetAnswerHandler(Control tb, string text);
        bool m_bStart = false;
        ManualResetEvent m_eventCloseUpdate;
        Thread m_tdUpdateTextBox;

        public GuessNumberServer()
        {
            InitializeComponent();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (true == m_bStart)
            {
                m_bStart = false;
                m_Server.Stop();
                m_eventCloseUpdate.Set();
            }
            base.OnFormClosing(e);
            // Code
        }

        private void BtnInitServer_Click(object sender, EventArgs e)
        {
            //ParseRealData("{\"Type\":0, \"Player\":\"Player1\"}");
            //ParseRealData("{\"Type\":1, \"Player\":\"Player1\", \"Guess\":\"123\"}");
            //ParseRealData("{\"Type\":2, \"Player\":\"Player1\"}");
            //return;
            m_Server.Start();
            textBoxStatus.Clear();
            m_bStart = true;
            m_eventCloseUpdate = new ManualResetEvent(false);
            m_eventCloseUpdate.Reset();
            m_tdUpdateTextBox = new Thread(UpdateTextBoxProc);
            m_tdUpdateTextBox.IsBackground = true;
            m_tdUpdateTextBox.Start();
            btnInitServer.Enabled = false;
            btnSendTest.Enabled = true;
            btnSendBC.Enabled = true;
            btnDisconnect.Enabled = true;
        }
        private void BtnSendTest_Click(object sender, EventArgs e)
        {
            m_Server.SendTest();
        }

        private void BtnSendBC_Click(object sender, EventArgs e)
        {
            m_Server.SendTestBC();
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            if (true == m_bStart)
            {
                m_bStart = false;
                m_Server.Stop();
                m_eventCloseUpdate.Set();
            }
            btnInitServer.Enabled = true;
            btnSendTest.Enabled = false;
            btnSendBC.Enabled = false;
            btnDisconnect.Enabled = false;
        }

        private void UpdateTextBoxProc()
        {
            string sPopResp = "";
            while(true)
            {
                if(false == m_bStart)
                {
                    break;
                }
                if (m_eventCloseUpdate.WaitOne(0))
                {
                    break;
                }
                sPopResp = m_Server.PopRespQueue();
                if("NO_DATA" == sPopResp)
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
            if(true == tb.InvokeRequired)
            {
                UpdateHandler uh = new UpdateHandler(UpdateTextBox);
                tb.Invoke(uh, tb, sPopResp);
            }
            else
            {
                tb.AppendText(sPopResp + "\r\n");
            }
        }

        private void SetText(Control obj, string sSetString)
        {
            if (true == obj.InvokeRequired)
            {
                SetAnswerHandler uh = new SetAnswerHandler(SetText);
                obj.Invoke(uh, obj, sSetString);
            }
            else
            {
                obj.Text = sSetString;
            }
        }

        class Data
        {
            public Data()
            {
                m_iType = -1;
                m_sPlayer = "";
                m_sGuess = "";
            }
            public int m_iType;
            public string m_sPlayer;
            public string m_sGuess;
        }

        private void ParseRealData(string sRealData)
        {
            if (true == sRealData.Contains("Status"))
            {
                UpdateTextBox(textBoxStatus, sRealData);
            }
            else if (true == sRealData.Contains("Gaming"))
            {
                if (true == sRealData.Contains("亂數答案： "))
                {
                    int iPos = sRealData.IndexOf("亂數答案： ");

                    string sSetAnswer = "Answer： " + 
                        sRealData.Substring(sRealData.IndexOf("亂數答案： ")+ "亂數答案： ".Length, 1) + " " +
                        sRealData.Substring(sRealData.IndexOf("亂數答案： ") + "亂數答案： ".Length+1, 1) + " " +
                        sRealData.Substring(sRealData.IndexOf("亂數答案： ") + "亂數答案： ".Length+2, 1);
                    SetText(lbQuestion, sSetAnswer);
                }
                UpdateTextBox(textBoxGaming, sRealData);
            }
            
        }

        //private void ParseRealData(string sRealData)
        //{
        //    const string sNewLine = "\r\n";
        //    Data dataPop = new Data();
        //    ParseJsonData(sRealData, ref dataPop);
        //    string sShowString = "";
        //    switch(dataPop.m_iType)
        //    {
        //        case 0:
        //            sShowString = "玩家： " + dataPop.m_sPlayer + " 登入" + sNewLine;
        //            UpdateTextBox(textBoxStatus, sShowString);
        //            break;
        //        case 1:
        //            sShowString = "玩家： " + dataPop.m_sPlayer + " 猜測數字： " + dataPop.m_sGuess + "結果： " + "XAXB " + sNewLine;
        //            UpdateTextBox(textBoxGaming, sShowString);
        //            break;
        //        case 2:
        //            sShowString = "玩家： " + dataPop.m_sPlayer + " 登出" + sNewLine;
        //            UpdateTextBox(textBoxStatus, sShowString);
        //            break;
        //        default:    //HandShaking
        //            UpdateTextBox(textBoxStatus, sRealData);
        //            break;
        //    }
        //}

        //private void ParseJsonData(string sJson, ref Data dataPop)
        //{
        //    string sTempJson = sJson;
        //    int iPos = 0;

        //    if(true == sJson.Contains("Type"))
        //    {
        //        iPos = sTempJson.IndexOf("\"Type\":") + "\"Type\":".Length;
        //        sTempJson = sTempJson.Substring(iPos, sTempJson.Length - iPos);
        //        //dataPop.m_iType = Int32.Parse(sJson.Substring(sJson.IndexOf("Type\":") + "Type\":".Length, 1));
        //        dataPop.m_iType = Int32.Parse(sTempJson.Substring(0, 1));
        //    }
        //    else
        //    {
        //        return;
        //    }

        //    switch (dataPop.m_iType)
        //    {
        //        case 0:
        //        case 2:
        //            iPos = sTempJson.IndexOf("Player\":\"") + "Player\":\"".Length;
        //            sTempJson = sTempJson.Substring(iPos, sTempJson.Length - iPos);
        //            dataPop.m_sPlayer = sTempJson.Substring(0, sTempJson.IndexOf("\"}"));
        //            break;
        //        case 1:
        //            iPos = sTempJson.IndexOf("Player\":\"") + "Player\":\"".Length;
        //            sTempJson = sTempJson.Substring(iPos, sTempJson.Length - iPos);
        //            dataPop.m_sPlayer = sTempJson.Substring(0, sTempJson.IndexOf("\","));

        //            iPos = sTempJson.IndexOf("Guess\":\"") + "Guess\":\"".Length;
        //            sTempJson = sTempJson.Substring(iPos, sTempJson.Length - iPos);
        //            dataPop.m_sGuess = sTempJson.Substring(0, sTempJson.IndexOf("\"}"));
        //            break;
        //        //case 2:
        //        //    break;
        //        default:
        //            return;
        //    }
        //}
    }
}
