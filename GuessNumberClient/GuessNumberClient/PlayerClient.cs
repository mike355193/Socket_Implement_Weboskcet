using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuessNumberClient
{
    class PlayerClient
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>

        static bool m_bInit = false;
        static Socket m_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static readonly string m_sConnectIP = "127.0.0.1";
        //static string m_sConnectIP = "192.168.1.201";
        static int m_iPort = 4443;
        static readonly IPEndPoint m_connectEndPoint = new IPEndPoint(IPAddress.Parse(m_sConnectIP), m_iPort);
        static bool m_bDoReconnect = false;
        static Queue<string> m_queRespString = new Queue<string>();
        static string sPlayerName = "";
        static Thread m_tdSockReceive;
        static Thread m_tdSockReconnect;
        static ManualResetEvent m_eventCloseReceive;
        static ManualResetEvent m_eventReceivePause;
        static ManualResetEvent m_eventCloseReconnect;
        private static object m_lockQueue = new object();
        private static object m_lockReconnect = new object();
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GuessNumberClient());
        }

        public bool Start()
        {
            if (true == InitClient())
            {
                return true;
            }
            return false;
        }

        //0:Login               {"Type":0, "Player":"Player1"}
        //1:Guess               {"Type":1, "Player":"Player1", "Guess":"123"}
        //2:Logout              {"Type":2, "Player":"Player1"}
        //3:Restart             {"Type":3, "Player":"Player1"}

        //登入封包格式
        public void SendLogin(string sSetPlayerName)
        {
            sPlayerName = sSetPlayerName;
            string sSend = "{" + "\"Type\":0," +
                                 "\"Player\":\"" + sPlayerName + "\"" +
                           "}"
                           ;
            WebSockSend(sSend);
        }

        //猜數字封包格式
        public void SendGuess(string sGuess)
        {
            if("" == sPlayerName)
            {
                return;
            }
            string sSend = "{" + "\"Type\":1," +
                                 "\"Player\":\"" + sPlayerName + "\"," +
                                 "\"Guess\":\"" + sGuess + "\"" +
                           "}"
                           ;
            WebSockSend(sSend);
        }

        //登出封包格式
        public void SendLogout(string sSetPlayerName)
        {
            string sSend = "{" + "\"Type\":2," +
                                 "\"Player\":\"" + sPlayerName + "\"" +
                           "}"
                           ;
            WebSockSend(sSend);
            sPlayerName = "";
        }

        public void SendRestart(string sSetPlayerName)
        {
            string sSend = "{" + "\"Type\":3," +
                                 "\"Player\":\"" + sPlayerName + "\"" +
                           "}"
                           ;
            WebSockSend(sSend);
        }

        public string PopRespQueue()
        {
            lock (m_lockQueue)
            {
                if (0 < m_queRespString.Count())
                {
                    string sPopString = m_queRespString.Peek();
                    m_queRespString.Dequeue();
                    return sPopString;
                }
                return "NO_DATA";
            }
        }

        public void Stop()
        {
            SockSend("CloseThisClient");
            m_bInit = false;
            m_eventReceivePause.Set();
            m_eventCloseReceive.Set();
            m_eventCloseReconnect.Set();
            m_tdSockReconnect.Abort();
            m_tdSockReceive.Abort();
            m_client.Close();
        }

        //----------------------------------------------------------------------------------

        private static bool InitClient()
        {
            try
            {
                m_queRespString.Enqueue("[Status]嘗試連線遊戲主機…\r\n");
                m_client.Connect(m_connectEndPoint);
                SendHandShake();

                m_bInit = true;

                m_eventCloseReceive = new ManualResetEvent(false);
                m_eventReceivePause = new ManualResetEvent(false);
                m_tdSockReceive = new Thread(SockReceiveProc);
                m_tdSockReceive.IsBackground = true;
                m_tdSockReceive.Start();

                m_eventCloseReconnect = new ManualResetEvent(false);
                m_tdSockReconnect = new Thread(SockReconnetProc);
                m_tdSockReceive.IsBackground = true;
                m_tdSockReconnect.Start();


                m_queRespString.Enqueue("[Status]連線遊戲主機成功！可以登入。\r\n");
                //開始ReveiveThread
                m_eventReceivePause.Set();

                return true;
            }
            catch (SocketException e)
            {
                m_queRespString.Enqueue("[Status]遊戲主機連線失敗，原因： " + e.ToString() + " \r\n");
                return false;
            }
        }

        private static void SockReceiveProc()
        {
            try
            {
                int iRecvLen = 0;
                byte[] pbyRecvBuffer = new byte[1024];

                //WebSockSend("Test");
                while (true == m_bInit)
                {
                    m_eventReceivePause.WaitOne(Timeout.Infinite);
                    if (m_eventCloseReceive.WaitOne(0))
                    {
                        break;
                    }
                    // 程式會被 hand 在此, 等待接收來自 Server 端傳來的資料
                    try
                    {
                        iRecvLen = m_client.Receive(pbyRecvBuffer); // wait for server to send a message
                        if (0 < iRecvLen)
                        {
                            DispatchMessage(pbyRecvBuffer, iRecvLen);
                        }
                    }
                    catch (SocketException e)
                    {
                        string sErr = e.ToString();
                        int iErrCode = 0;
                        Win32Exception w32ex = e as Win32Exception;
                        if (null == w32ex)
                        {
                            w32ex = e.InnerException as Win32Exception;
                        }
                        if (null != w32ex)
                        {
                            iErrCode = w32ex.ErrorCode;
                        }
                        if (10054 == iErrCode || 10053 == iErrCode)  //Server 關閉
                        {
                            m_bDoReconnect = true;
                            m_queRespString.Enqueue("[Status]遊戲主機斷線，原因： " + e.ToString() + " \r\n");
                            m_queRespString.Enqueue("[Gaming]遊戲主機斷線，原因： " + e.ToString() + " \r\n");
                        }
                        sErr = e.ToString();
                        //暫停ReceiveThread
                        m_eventReceivePause.Reset();
                    }
                }
            }
            catch(Exception e)
            {
                string sErr = e.ToString();
                m_bDoReconnect = true;
                m_queRespString.Enqueue("[Status]遊戲主機斷線，原因： " + e.ToString() + " \r\n");
                m_queRespString.Enqueue("[Gaming]遊戲主機斷線，原因： " + e.ToString() + " \r\n");
                //暫停ReceiveThread
                m_eventReceivePause.Reset();
            }
        }

        private static void SockReconnetProc()
        {
            while (true == m_bInit)
            {
                if (m_eventCloseReconnect.WaitOne(0))
                {
                    break;
                }
                if (true == m_bDoReconnect)
                {
                    try
                    {
                        m_queRespString.Enqueue("[Status]嘗試重連遊戲主機…\r\n");
                        m_client.Close();
                        m_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        m_client.Connect(m_connectEndPoint);
                        SendHandShake();
                        m_bDoReconnect = false;
                        m_queRespString.Enqueue("[Status]重連遊戲主機成功！請重新登入。\r\n");
                        //開始ReceiveThread
                        m_eventReceivePause.Set();
                    }
                    catch (SocketException e)
                    {
                        m_queRespString.Enqueue("[Status]遊戲主機重連失敗，原因： " + e.ToString() + " \r\n");
                    }
                }
                
                Thread.Sleep(5000);
            }
        }

        //
        //  0                   1                   2                   3
        //  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        // +-+-+-+-+-------+-+-------------+-------------------------------+
        // |F|R|R|R| opcode|M| Payload len |    Extended payload length    |
        // |I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
        // |N|V|V|V|       |S|             |   (if payload len==126/127)   |
        // | |1|2|3|       |K|             |                               |
        // +-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
        // |     Extended payload length continued, if payload len == 127  |
        // + - - - - - - - - - - - - - - - +-------------------------------+
        // |                               |Masking-key, if MASK set to 1  |
        // +-------------------------------+-------------------------------+
        // | Masking-key (continued)       |          Payload Data         |
        // +-------------------------------- - - - - - - - - - - - - - - - +
        // :                     Payload Data continued ...                :
        // + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
        // |                     Payload Data continued ...                |
        // +---------------------------------------------------------------+

        private static void WebSockSend(string sSendString)
        {
            //if (null != m_client && true == m_client.Connected)
            //{
            //    return;
            //}
            byte[] byWebSocketHeader = new byte[512];
            int iPos = 0;
            //組WebSocketHeader

            BitArray bitArray = new BitArray(8);

            //byWebSocketHeader[0] = 0x81;
            bitArray[0] = true;    //FIN
            bitArray[1] = false;   //RSV1
            bitArray[2] = false;   //RSV2
            bitArray[3] = false;   //RSV3
            bitArray[4] = false;   //opcode
            bitArray[5] = false;
            bitArray[6] = false;
            bitArray[7] = true;

            byte[] byByte = new byte[1];
            bitArray.CopyTo(byByte, 0);
            byWebSocketHeader[iPos++] = byByte[0];


            //Payload len
            byte[] bySendString = System.Text.Encoding.UTF8.GetBytes(sSendString);
            int iSendLen = bySendString.Length;
            byte byPayLoad = Convert.ToByte(iSendLen);
            //MASK
            byPayLoad |= 0x80;      //true
            byWebSocketHeader[iPos++] = byPayLoad;

            //Extended payload if payload len ==126/127
            //Extended payload if payload len ==127

            //Masking-key, if MASK set to 1     client to server must set MASK to 1
            Byte[] masks = new Byte[4];
            masks[0] = 0x01;
            masks[1] = 0x01;
            masks[2] = 0x01;
            masks[3] = 0x01;
            byWebSocketHeader[iPos++] = masks[0];
            byWebSocketHeader[iPos++] = masks[1];
            byWebSocketHeader[iPos++] = masks[2];
            byWebSocketHeader[iPos++] = masks[3];

            //Payload Data
            Byte[] byMaskSendString = new Byte[bySendString.Length];
            for (int i = 0; i < iSendLen; i++)
            {
                byMaskSendString[i] = (Byte)(bySendString[i] ^ masks[i % 4]);
            }

            //真正要送的Data = WebSocket Header + Masked Payload;
            byte[] byRealSend = new byte[512];
            //byte[] byRealSend = new byte[iPos + sSendString.Length + 1];
            Array.Copy(byWebSocketHeader, 0, byRealSend, 0, byWebSocketHeader.Length);
            Array.Copy(byMaskSendString, 0, byRealSend, iPos, byMaskSendString.Length);

            SockSend(byRealSend);
        }

        private static void SockSend(string sSendString)
        {
            try
            {
                m_client.Send(Encoding.UTF8.GetBytes(sSendString));
            }
            catch (SocketException e)
            {
                string sErr = e.ToString();
                m_bDoReconnect = true;
                m_queRespString.Enqueue("[Status]遊戲主機斷線，原因： " + e.ToString() + " \r\n");
                m_queRespString.Enqueue("[Gaming]遊戲主機斷線，原因： " + e.ToString() + " \r\n");
            }
        }

        private static void SockSend(byte[] bySendString)
        {
            try
            {
                m_client.Send(bySendString);
            }
            catch
            {

            }
        }

        private static void SendHandShake()
        {
            string sHandShakeString = "";
            string sNewLine = "\r\n";
            sHandShakeString = "GET / HTTP/1.1" + sNewLine
                                + "Host: " + m_sConnectIP + ":" + m_iPort.ToString() + sNewLine
                                + "Connection: Upgrade" + sNewLine
                                + "Pragma: no-cache" + sNewLine
                                + "Cache-Control: no-cache" + sNewLine
                                + "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.110 Safari/537.36" + sNewLine
                                + "Upgrade: websocket" + sNewLine
                                + "Origin: " + "http://localhost:33021" + sNewLine
                                + "Sec-WebSocket-Version: 13" + sNewLine
                                + "Accept - Encoding: gzip, deflate, br" + sNewLine
                                + "Accept - Language: zh-CN,zh; q=0.9" + sNewLine
                                + "Sec-WebSocket-Key: 9LEOCcXSLQoZngPbQnApJA ==" + sNewLine
                                + "Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits" + sNewLine + sNewLine
                                //+ "Connection" + sNewLine;
                                ;

            byte[] sSendBuf = Encoding.UTF8.GetBytes(sHandShakeString);
            m_client.BeginSend(sSendBuf, 0, sSendBuf.Length, SocketFlags.None, null, null);
            //m_client.Send(Encoding.UTF8.GetBytes(sHandShakeString));
        }

        private static void DispatchMessage(byte[] byBuffer, int iRecvLen)
        {
            int iRealDataLen = byBuffer[1];
            string sMessage = (System.Text.Encoding.UTF8.GetString(byBuffer)).Substring(2, iRealDataLen);
            if (0x00 == byBuffer[0])   //第一包Resp該為HandShaking
            {                
                if (true == sMessage.Contains("HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\n"))  /* Accept Handshaking */
                {
                    ////CHUN
                    //WebSockSend("Test");
                    lock (m_lockQueue)
                    {
                        m_queRespString.Enqueue(sMessage);
                    }
                }
                else
                {
                    //Shaking fail;
                    m_client.Close();
                }
            }
            else    //0x01一般字串
            {
                lock (m_lockQueue)
                {
                    m_queRespString.Enqueue(sMessage);
                }
            }
        }
    }
}
