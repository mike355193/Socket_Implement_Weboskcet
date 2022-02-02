using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Collections;

class ClientSocketObj
{
    public ClientSocketObj()
    {

    }
    public Socket m_sock;
    public ManualResetEvent m_eventClose;
    public Thread m_tdSockSAccept;
}

namespace GuessNumberServer
{
    class GameServer
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>

        static bool m_bInit = false;
        static Socket m_sockServer;
        static ClientSocketObj[] m_sockClients;
        static int m_iSockIndex = 0;
        static Socket m_sockListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //static string m_sLocalIP = "192.168.1.201";
        static string m_sLocalIP = "127.0.0.1";
        static int m_iPort = 4443;
        static IPEndPoint m_localEndPoint = new IPEndPoint(IPAddress.Parse(m_sLocalIP), m_iPort);
        static Queue<string> m_queRespString = new Queue<string>();
        private static object m_lockQueue = new object();

        static int m_iMaxPlayer = 2;
        static int m_iLoginCount = 0;
        static PlayerData[] m_Players = new PlayerData[m_iMaxPlayer];
        static string sAnswer = "";
        static int[] m_iAnswerArr = new int[3] { -1, -1, -1 };
        static int[] m_iGuessArr = new int[3] { -1, -1, -1 };
        static int m_iGuessRound = 1;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GuessNumberServer());
        }

        public void Start()
        {
            Listen();
            m_Players[0] = new PlayerData();
            m_Players[1] = new PlayerData();
            m_bInit = true;
        }

        public void SendTest()
        {
            for (int iSockIndex = 0; iSockIndex <= m_sockClients.Length - 1; iSockIndex++)
            {
                if (null != m_sockClients[iSockIndex] && null != m_sockClients[iSockIndex].m_sock)
                {
                    if (true == m_sockClients[iSockIndex].m_sock.Connected)
                    {
                        try
                        {
                            SockSendBySocketIndex(iSockIndex, "[Status]ServerTest", 0x01);
                            break;
                        }
                        catch (SocketException e)   //該Client被斷線
                        {
                            string sErr = e.ToString();
                            CloseClient(iSockIndex);
                        }
                    }
                }
            }
        }

        public void SendTestBC()
        {
            SockBroadcast("[Status]ServerTestBC");
        }

        //從RespQueue取出一個字串
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
            m_bInit = false;
            for (int iSockIndex = 0; iSockIndex < m_sockClients.Length; iSockIndex++)
            {
                if (null != m_sockClients[iSockIndex])
                {
                    CloseClient(iSockIndex);
                }
            }
            if (null != m_sockServer)
            {
                m_sockServer.Close();
                m_sockServer = null;
            }
        }

        //----------------------------------------------------------------------------------

        //Server開始Listen
        private static void Listen()
        {
            if(null != m_sockServer)
            {
                m_sockServer.Close();
                m_sockServer = null;
            }
            m_sockServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_sockServer.Bind(m_localEndPoint);
                m_sockServer.Listen(2);

                //Array.Resize(ref m_sockClients, 1);
                //m_sockClients[0] = new ClientSocketObj();
                ServerWaitAccept();
            }
            catch(SocketException e)
            {
                MessageBox.Show("[Status]ServerInitfail,m_localEndPoint is listening.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(System.Environment.ExitCode);   //強制關閉
                m_queRespString.Enqueue("[Status]ServerInitfail,m_localEndPoint is listening.");
            }
        }

        //Server等待有Client來連線
        private static void ServerWaitAccept()
        {
            // 判斷目前是否有空的 Socket 可以提供給Client端連線
            if (null == m_sockClients)
            {
                Array.Resize(ref m_sockClients, 1);
                m_sockClients[m_iSockIndex] = new ClientSocketObj();
            }
            else
            {
                bool bFlagFinded = false;
                for (int i = 0; i < m_sockClients.Length; i++)
                {
                    // m_sockObjs[i] 若不為 null 表示已被實作過, 判斷是否有 Client 端連線
                    if (null != m_sockClients[i])
                    {
                        if (null != m_sockClients[i].m_sock)
                        {
                            // 如果目前第 i 個 Socket 若沒有人連線, 便可提供給下一個 Client 進行連線
                            if (m_sockClients[i].m_sock.Connected == false)
                            {
                                m_iSockIndex = i;
                                bFlagFinded = true;
                                break;
                            }
                        }
                        else
                        {
                            m_sockClients[i].m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            m_iSockIndex = i;
                            bFlagFinded = true;
                            break;
                        }
                    }
                }

                // 如果 FlagFinded 為 false 表示目前並沒有多餘的 Socket 可供 Client 連線
                if (false == bFlagFinded)
                {
                    // 增加 Socket 的數目以供下一個 Client 端進行連線
                    Array.Resize(ref m_sockClients, m_sockClients.Length + 1);
                    m_iSockIndex = m_sockClients.Length - 1;
                    m_sockClients[m_iSockIndex] = new ClientSocketObj();
                }
            }



            // 以下兩行為多執行緒的寫法, 因為接下來 Server 端的部份要使用 Accept() 讓 Cleint 進行連線;
            // 該執行緒有需要時再產生即可, 因此定義為區域性的 Thread. 命名為 SckSAcceptTd;
            // 在 new Thread( ) 裡為要多執行緒去執行的函數. 這裡命名為 SckSAcceptProc;
            m_sockClients[m_iSockIndex].m_eventClose = new ManualResetEvent(false);
            m_sockClients[m_iSockIndex].m_eventClose.Reset();
            m_sockClients[m_iSockIndex].m_tdSockSAccept = new Thread(SockSAcceptProc);
            m_sockClients[m_iSockIndex].m_tdSockSAccept.IsBackground = true;
            m_sockClients[m_iSockIndex].m_tdSockSAccept.Start();  // 開始執行 SckSAcceptTd 這個執行緒

            // 這裡要點出 SckSacceptTd 這個執行緒會在 Start( ) 之後開始執行 SckSAcceptProc 裡的程式碼, 同時主程式的執行緒也會繼續往下執行各做各的. 
            // 主程式不用等到 SckSAcceptProc 的程式碼執行完便會繼續往下執行.
        }

        //Server的廣播方法
        private static void SockBroadcast(string sSendMessage)
        {
            for (int iSockIndex = 0; iSockIndex <= m_sockClients.Length -1; iSockIndex++)
            {
                if (null != m_sockClients[iSockIndex] && null != m_sockClients[iSockIndex].m_sock)
                {
                    if (true == m_sockClients[iSockIndex].m_sock.Connected)
                    {
                        try
                        {
                            SockSendBySocketIndex(iSockIndex, sSendMessage, 0x01);
                        }
                        catch (SocketException e)   //該Client被斷線
                        {
                            string sErr = e.ToString();
                            CloseClient(iSockIndex);
                        }
                    }
                }
            }
        }

        private static void SockSendBySocketIndex(int iSockIndex, string sSendMessage, byte byType)
        {
            if(null == m_sockClients[iSockIndex].m_sock)
            {
                return;
            }
            byte[] bySendHeader = new byte[2];
            byte[] byShakingString = System.Text.Encoding.UTF8.GetBytes(sSendMessage);
            byte[] byRealSend = new byte[2 + byShakingString.Length];
            bySendHeader[0] = byType;                               //0x00 HandShaking, 0x01 一般字串
            bySendHeader[1] = Convert.ToByte(sSendMessage.Length);  //用一個byte存資料真實長度
            Array.Copy(bySendHeader, 0, byRealSend, 0, bySendHeader.Length);
            Array.Copy(byShakingString, 0, byRealSend, 2, byShakingString.Length);
            m_sockClients[iSockIndex].m_sock.Send(byRealSend);
        }

        // 接收來自Client的連線與Client傳來的資料
        private static void SockSAcceptProc()
        {
            int iThisClientIndex = 0;
            // 這裡加入 try 是因為 SckSs[0] 若被 Close 的話, SckSs[0].Accept() 會產生錯誤
            try
            {
                m_sockClients[m_iSockIndex].m_sock = m_sockServer.Accept();  // 等待Client 端連線
                if (m_sockClients[m_iSockIndex].m_eventClose.WaitOne(0))
                {
                    return;
                }

                // 能來這表示有 Client 連上線. 記錄該 Client 對應的 SckCIndex
                iThisClientIndex = m_iSockIndex;
                Socket sockThisClient = m_sockClients[iThisClientIndex].m_sock;
                // 再產生另一個執行緒等待下一個 Client 連線
                ServerWaitAccept();

                int iRecvLen;
                byte[] pbyClientData = new byte[512];

                /* Do Handshaking */
                iRecvLen = sockThisClient.Receive(pbyClientData);
                if (0 < iRecvLen)
                {
                    string sHeaderResponse = (System.Text.Encoding.UTF8.GetString(pbyClientData)).Substring(0, iRecvLen);
                    lock (m_lockQueue)
                    {
                        m_queRespString.Enqueue(sHeaderResponse);
                    }
                    string sKey = sHeaderResponse.Replace("ey:", "`")
                                    .Split('`')[1]                     // dGhlIHNhbXBsZSBub25jZQ== \r\n .......
                                    .Replace("\r", "").Split('\n')[0]  // dGhlIHNhbXBsZSBub25jZQ==
                                    .Trim();
                    string sAcceptKey = AcceptKey(ref sKey);
                    string sNewLine = "\r\n";
                    string sResp = "HTTP/1.1 101 Switching Protocols" + sNewLine
                                    + "Upgrade: websocket" + sNewLine
                                    + "Connection: Upgrade" + sNewLine
                                    + "Sec-WebSocket-Accept: " + sAcceptKey + sNewLine + sNewLine
                                    //+ "Sec-WebSocket-Protocol: chat, superchat" + sNewLine
                                    //+ "Sec-WebSocket-Version: 13" + sNewLine
                                    ;
                    SockSendBySocketIndex(iThisClientIndex, sResp, 0x00);
                }
                else
                {
                    sockThisClient.Close();
                }
                //-----

                //Receive loop
                while (true)
                {
                    if (m_sockClients[iThisClientIndex].m_eventClose.WaitOne(0))
                    {
                        break;
                    }
                    // 程式會被 hand 在此, 等待接收來自 Client 端傳來的資料                    
                    iRecvLen = sockThisClient.Receive(pbyClientData); // wait for client to send a message
                    if (0 < iRecvLen)
                    {
                        string sContent = Encoding.UTF8.GetString(pbyClientData);
                        if (true == sContent.Contains("CloseThisClient"))
                        {
                            CloseClient(iThisClientIndex);
                        }
                        else
                        {
                            ParseRecvBuffer(pbyClientData, iThisClientIndex);
                        }
                    }
                }
            }
            catch (SocketException e)   //該Client被斷線
            {
                string sErr = e.ToString();
                CloseClient(iThisClientIndex);
            }
        }

        private static void CloseClient(int iClientIndex)
        {
            try
            {
                if (null != m_sockClients[iClientIndex].m_eventClose)
                {
                    m_sockClients[iClientIndex].m_eventClose.Set();
                }
                //如果還沒Accept到Client，建立一個Client以跳脫AceeptLoop
                if (null == m_sockClients[iClientIndex].m_sock)
                {
                    Socket sockTempClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        sockTempClient.Connect(m_localEndPoint);
                        if (true == sockTempClient.Connected)
                        {
                            sockTempClient.Close();
                        }
                    }
                    catch (SocketException e)
                    {
                        e.ToString();
                    }
                }
                //Thread IsBackground = true; ==>程式關閉自動關閉Thread
                if (null != m_sockClients[iClientIndex].m_sock)
                {
                    m_sockClients[iClientIndex].m_sock.Close();
                    m_sockClients[iClientIndex].m_sock = null;
                }
            }
            catch (SocketException e)
            {
                // 這裡出錯, 主要是出在 SckSs[Scki] 出問題, 自己加判斷吧~
                string sErr = e.ToString();
                int i = 0;
            }
        }

        private static string AcceptKey(ref string sKey)
        {
            const string sWebsocketGUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            string sLongKey = sKey + sWebsocketGUID;
            SHA1 sha1 = SHA1CryptoServiceProvider.Create();
            byte[] pbyHashBytes = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sLongKey));
            return Convert.ToBase64String(pbyHashBytes);
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

        //根據WebSocket協定，解析封包
        private static void ParseRecvBuffer(byte[] pbyRecvBuffer, int iClientIndex)
        {
            string sLook = Encoding.UTF8.GetString(pbyRecvBuffer);
            // 判斷是否為最後一個Frame(第一個bit為FIN若為1代表此Frame為最後一個Frame)，超過一個Frame暫不處理
            if (!((pbyRecvBuffer[0] & 0x80) == 0x80))
            {
                Console.WriteLine("Exceed 1 Frame. Not Handle");
                return;
            }
            // 是否包含Mask(第一個bit為1代表有Mask)，沒有Mask則不處理。且根據協定需對該連線斷線
            if (!((pbyRecvBuffer[1] & 0x80) == 0x80))
            {
                Console.WriteLine("Exception: No Mask");
                CloseClient(iClientIndex);
                return;
            }
            // 資料長度 = dataBuffer[1] - 127
            int iPayloadLen = pbyRecvBuffer[1] & 0x7F;
            Byte[] pbyMasks = new Byte[4];
            Byte[] pbyPayloadData = FilterPayloadData(ref pbyRecvBuffer, ref iPayloadLen, ref pbyMasks);
            // 使用WebSocket Protocol中的公式解析資料
            for (int j = 0; j < iPayloadLen; j++)
                pbyPayloadData[j] = (Byte)(pbyPayloadData[j] ^ pbyMasks[j % 4]);

            // 解析出的資料
            string sContent = Encoding.UTF8.GetString(pbyPayloadData);
            lock (m_lockQueue)
            {
                ParseRealData(sContent, iClientIndex);
            }
        }

        //根據WebSocket協定，解析封包內的Masking Data
        private static Byte[] FilterPayloadData(ref byte[] pbyDataBuffer, ref int iPayloadLen, ref Byte[] pbyMasks)
        {
            Byte[] pbyPayloadData;
            switch (iPayloadLen)
            {
                // 包含16 bit Extend Payload Length
                case 126:
                    Array.Copy(pbyDataBuffer, 4, pbyMasks, 0, 4);
                    iPayloadLen = (UInt16)(pbyDataBuffer[2] << 8 | pbyDataBuffer[3]);
                    pbyPayloadData = new Byte[iPayloadLen];
                    Array.Copy(pbyDataBuffer, 8, pbyPayloadData, 0, iPayloadLen);
                    break;
                // 包含 64 bit Extend Payload Length
                case 127:
                    Array.Copy(pbyDataBuffer, 10, pbyMasks, 0, 4);
                    Byte[] pbyuInt64Bytes = new Byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        pbyuInt64Bytes[i] = pbyDataBuffer[9 - i];
                    }
                    UInt64 iLen = BitConverter.ToUInt64(pbyuInt64Bytes, 0);

                    pbyPayloadData = new Byte[iLen];
                    for (UInt64 i = 0; i < iLen; i++)
                        pbyPayloadData[i] = pbyDataBuffer[i + 14];
                    break;
                // 沒有 Extend Payload Length
                default:
                    Array.Copy(pbyDataBuffer, 2, pbyMasks, 0, 4);
                    pbyPayloadData = new Byte[iPayloadLen];
                    Array.Copy(pbyDataBuffer, 6, pbyPayloadData, 0, iPayloadLen);
                    break;
            }
            return pbyPayloadData;
        }

        class PlayerData
        {
            public string m_sPlayer = "";
            public bool m_bWantPlay = false;
            public bool m_bFirst = false;
            public bool m_bHasGuess = false;
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

        private static void ParseRealData(string sRealData, int iClientIndex)
        {
            const string sNewLine = "\r\n";
            Data dataPop = new Data();
            ParseJsonData(sRealData, ref dataPop);
            string sShowString = "";

            switch (dataPop.m_iType)
            {
                case 0:
                    ParseLoginPacket(dataPop, iClientIndex);
                    break;
                case 1:
                    ParseGuessPacket(dataPop, iClientIndex);
                    break;
                case 2:
                    ParseLogoutPacket(dataPop, iClientIndex);
                    break;
                case 3:
                    ParseRestartPacket(dataPop, iClientIndex);
                    break;
                default:    //HandShaking
                    sShowString = "[Status]" + sRealData + sNewLine;
                    lock (m_lockQueue)
                    {
                        m_queRespString.Enqueue(sShowString);
                    }
                    break;
            }
        }

        private static bool CheckStartGame()
        {
            int iWantPlay = 0;
            for (int i = 0; i < m_Players.Length; i++)
            {
                if (true == m_Players[i].m_bWantPlay)
                {
                    iWantPlay++;
                }
            }
            if (iWantPlay != m_iMaxPlayer)
            {
                return false;
            }

            m_iGuessRound = 1;
            string sShowString = "";
            const string sNewLine = "\r\n";
            sShowString = "[Gaming]遊戲開始！亂數答案： " + NewGame() + sNewLine;
            lock (m_lockQueue)  //只顯示在Server，並沒有要送至Client
            {
                m_queRespString.Enqueue(sShowString);
            }
            //廣播至Client告知遊戲開始
            sShowString = "[Gaming]遊戲開始！第 [" + m_iGuessRound.ToString() + "] 輪" + sNewLine; //"開始""輪" 使所有Client端 輸入Guess方塊Enable、Guess按鈕Enable

            for (int i = 0; i < m_Players.Length; i++)
            {
                if(true == m_Players[i].m_bFirst)
                {
                    sShowString += m_Players[i].m_sPlayer + "先" + sNewLine;
                }
            }
            lock (m_lockQueue)
            {
                m_queRespString.Enqueue(sShowString);
            }
            SockBroadcast(sShowString);
            return true;
        }

        private static void ParseLoginPacket(Data dataPop, int iClientIndex)
        {
            bool bFinded = false;
            string sShowString = "";
            const string sNewLine = "\r\n";
            //int iFindIndex = -1;
            int iSetIndex = -1;
            bool bHasAssignFirst = false;
            if (m_iLoginCount != m_iMaxPlayer)
            {
                //檢查名稱並且給予空的索引值
                for (int i = 0; i < m_Players.Length; i++)
                {
                    if (m_Players[i].m_sPlayer == dataPop.m_sPlayer)
                    {
                        sShowString = "[Status]玩家： " + dataPop.m_sPlayer + " 登入失敗：重複名稱" + sNewLine;
                        lock (m_lockQueue)
                        {
                            m_queRespString.Enqueue(sShowString);
                        }
                        SockSendBySocketIndex(iClientIndex, sShowString, 0x01);
                        bFinded = true;
                        break;
                    }
                    else if (m_Players[i].m_sPlayer == "")
                    {
                        iSetIndex = i;
                    }
                }
                if (false == bFinded)
                {
                    m_Players[iSetIndex].m_bWantPlay = true;
                    m_Players[iSetIndex].m_sPlayer = dataPop.m_sPlayer;
                    //在還沒有人指定為優先猜的時候，指定此人為優先猜
                    for (int i = 0; i < m_Players.Length; i++)
                    {
                        if (true == m_Players[i].m_bFirst)
                        {
                            bHasAssignFirst = true;
                        }
                    }
                    if (false == bHasAssignFirst) 
                    {
                        m_Players[iSetIndex].m_bFirst = true;
                        bHasAssignFirst = true;
                    }
                    sShowString = "[Status]玩家： " + dataPop.m_sPlayer + " 登入成功" + sNewLine;  //"登入成功"使該Client端 登入按鈕Disable，登入按鈕Enable
                    lock (m_lockQueue)
                    {
                        m_queRespString.Enqueue(sShowString);
                    }
                    SockSendBySocketIndex(iClientIndex, sShowString, 0x01);
                    m_iLoginCount++;

                    //檢查如果有兩名玩家且都想玩，即開局
                    CheckStartGame();
                }
            }
            else //已有兩人登入
            {
                sShowString = "[Status]玩家： " + dataPop.m_sPlayer + " 登入失敗：已經有兩人登入" + sNewLine;
                lock (m_lockQueue)
                {
                    m_queRespString.Enqueue(sShowString);
                }
                SockSendBySocketIndex(iClientIndex, sShowString, 0x01);
            }
        }

        private static void ParseGuessPacket(Data dataPop, int iClientIndex)
        {
            bool bFinded = false;
            string sShowString = "";
            const string sNewLine = "\r\n";
            int iFindIndex = -1;
            //得知是誰猜的
            for (int i = 0; i < m_Players.Length; i++)
            {
                if (m_Players[i].m_sPlayer == dataPop.m_sPlayer)
                {
                    bFinded = true;
                    iFindIndex = i;
                    break;
                }
            }
            //做檢查運算，廣播結果，並把某人猜的flag設為true
            if (true == bFinded)
            {
                m_Players[iFindIndex].m_bHasGuess = true;
                sShowString = "[Gaming]玩家： " + dataPop.m_sPlayer + " 猜測數字： " + dataPop.m_sGuess + "結果： " + CheckGuess(dataPop.m_sGuess) + sNewLine;
                lock (m_lockQueue)
                {
                    m_queRespString.Enqueue(sShowString);
                }
                SockBroadcast(sShowString);
                if (true == sShowString.Contains("3A0B"))   //代表結束比賽，輪數回歸1，"結束比賽"使所有Client端無法再SendGuess
                {
                    sShowString = "[Gaming]在第 [" + m_iGuessRound.ToString() + "] 輪，結束比賽！"+ sNewLine+ "勝者：" + dataPop.m_sPlayer; //"結束比賽" 使所有Client端 輸入Guess方塊Disable、Guess按鈕Disable、再玩一次按鈕Enable
                    lock (m_lockQueue)
                    {
                        m_queRespString.Enqueue(sShowString);
                    }
                    SockBroadcast(sShowString);

                    //將兩名玩家想玩狀態設為false、優先猜的狀態設為false、猜的狀態設為false
                    for (int i = 0; i < m_Players.Length; i++)
                    {
                        m_Players[i].m_bWantPlay = false;
                        m_Players[i].m_bFirst = false;
                        m_Players[i].m_bHasGuess = false;
                    }
                    return;
                }
            }
            //如果兩名玩家都猜了，才可以繼續遊戲
            int iGuessCount = 0;
            for (int i = 0; i < m_Players.Length; i++)
            {
                if (true == m_Players[i].m_bHasGuess)
                {
                    iGuessCount++;
                }
            }
            if (iGuessCount == m_iMaxPlayer)
            {
                m_iGuessRound++;
                //廣播下一輪
                sShowString = "[Gaming]遊戲繼續！第 [" + m_iGuessRound.ToString() + "] 輪" + sNewLine;

                for (int i = 0; i < m_Players.Length; i++)
                {
                    if (true == m_Players[i].m_bFirst)
                    {
                        sShowString += m_Players[i].m_sPlayer + "先" + sNewLine; //"先""m_Players[i].m_sPlayer" 使所有Client端判斷該ID是否為自己，是→ 輸入Guess方塊Enable、Guess按鈕Enable
                    }
                }
                lock (m_lockQueue)
                {
                    m_queRespString.Enqueue(sShowString);
                }
                SockBroadcast(sShowString);
                m_iGuessRound++;

                //將兩名玩家猜的狀態變更為false
                for (int i = 0; i < m_Players.Length; i++)
                {
                    m_Players[i].m_bHasGuess = false;
                }
            }
            else
            {
                for (int i = 0; i < m_Players.Length; i++)
                {
                    if (false == m_Players[i].m_bFirst)
                    {
                        sShowString = "[Gaming]換" + m_Players[i].m_sPlayer + "猜" + sNewLine; //"換""m_Players[i].m_sPlayer" 使所有Client端判斷該ID是否為自己，是→ 輸入Guess方塊Enable、Guess按鈕Enable
                    }
                }
                lock (m_lockQueue)
                {
                    m_queRespString.Enqueue(sShowString);
                }
                SockBroadcast(sShowString);
            }
        }

        private static void ParseLogoutPacket(Data dataPop, int iClientIndex)
        {
            bool bFinded = false;
            string sShowString = "";
            const string sNewLine = "\r\n";
            //確認玩家是否存在，初始化該物件，並且都設置不想玩狀態
            for (int i = 0; i < m_Players.Length; i++)
            {
                if (m_Players[i].m_sPlayer == dataPop.m_sPlayer)
                {
                    m_Players[i].m_sPlayer = "";
                    m_Players[i].m_bFirst = false;
                    m_Players[i].m_bFirst = false;
                    m_Players[i].m_bHasGuess = false;
                    m_iLoginCount--;
                    bFinded = true;
                }
                m_Players[i].m_bWantPlay = false;
            }
            if (true == bFinded)
            {
                sShowString = "[Gaming]玩家： " + dataPop.m_sPlayer + " 登出了遊戲，結束比賽。" + sNewLine;   //"結束比賽"使所有Client端無法再SendGuess
                lock (m_lockQueue)
                {
                    m_queRespString.Enqueue(sShowString);
                }
                SockBroadcast(sShowString);

                sShowString = "[Status]玩家： " + dataPop.m_sPlayer + " 登出成功" + sNewLine;  //"登出成功" 使該Client 登入按鈕Enable、登出按鈕Disable
                lock (m_lockQueue)
                {
                    m_queRespString.Enqueue(sShowString);
                }
                SockSendBySocketIndex(iClientIndex, sShowString, 0x01);
            }
            else
            {
                sShowString = "[Status]玩家： " + dataPop.m_sPlayer + " 不存在，登出失敗" + sNewLine;
                lock (m_lockQueue)
                {
                    m_queRespString.Enqueue(sShowString);
                }
                SockSendBySocketIndex(iClientIndex, sShowString, 0x01);
            }
        }

        private static void ParseRestartPacket(Data dataPop, int iClientIndex)
        {
            bool bFinded = false;
            string sShowString = "";
            const string sNewLine = "\r\n";
            int iFindIndex = -1;
            bool bHasAssignFirst = false;
            bool bStartGame = false;
            //確認玩家是否存在，找到該索引值
            for (int i = 0; i < m_Players.Length; i++)
            {
                if (m_Players[i].m_sPlayer == dataPop.m_sPlayer)
                {
                    bFinded = true;
                    iFindIndex = i;
                    break;
                }
            }
            if (true == bFinded)
            {
                //在還沒有人指定為優先猜的時候，指定此人為優先猜
                for (int i = 0; i < m_Players.Length; i++)
                {
                    if (true == m_Players[i].m_bFirst)
                    {
                        bHasAssignFirst = true;
                    }
                }
                if (false == bHasAssignFirst)
                {
                    m_Players[iFindIndex].m_bFirst = true;
                    bHasAssignFirst = true;
                }
                m_Players[iFindIndex].m_bWantPlay = true;
                //檢查如果有兩名玩家且都想玩，即開局
                bStartGame = CheckStartGame();
            }
            if (false == bStartGame)
            {
                sShowString = "[Status]玩家： " + dataPop.m_sPlayer + " 想要在玩一次" + sNewLine;
                lock (m_lockQueue)
                {
                    m_queRespString.Enqueue(sShowString);
                }
                SockBroadcast(sShowString);
            }
        }

        //0:Login               {"Type":0, "Player":"Player1"}
        //1:Guess               {"Type":1, "Player":"Player1", "Guess":"123"}
        //2:Logout              {"Type":2, "Player":"Player1"}
        //3:Restart             {"Type":3, "Player":"Player1"}

        private static void ParseJsonData(string sJson, ref Data dataPop)
        {
            string sTempJson = sJson;
            int iPos = 0;

            if (true == sJson.Contains("Type"))
            {
                iPos = sTempJson.IndexOf("\"Type\":") + "\"Type\":".Length;
                sTempJson = sTempJson.Substring(iPos, sTempJson.Length - iPos);
                //dataPop.m_iType = Int32.Parse(sJson.Substring(sJson.IndexOf("Type\":") + "Type\":".Length, 1));
                dataPop.m_iType = Int32.Parse(sTempJson.Substring(0, 1));
            }
            else
            {
                return;
            }

            switch (dataPop.m_iType)
            {
                case 0:
                case 2:
                case 3:
                    iPos = sTempJson.IndexOf("Player\":\"") + "Player\":\"".Length;
                    sTempJson = sTempJson.Substring(iPos, sTempJson.Length - iPos);
                    dataPop.m_sPlayer = sTempJson.Substring(0, sTempJson.IndexOf("\"}"));
                    break;
                case 1:
                    iPos = sTempJson.IndexOf("Player\":\"") + "Player\":\"".Length;
                    sTempJson = sTempJson.Substring(iPos, sTempJson.Length - iPos);
                    dataPop.m_sPlayer = sTempJson.Substring(0, sTempJson.IndexOf("\","));

                    iPos = sTempJson.IndexOf("Guess\":\"") + "Guess\":\"".Length;
                    sTempJson = sTempJson.Substring(iPos, sTempJson.Length - iPos);
                    dataPop.m_sGuess = sTempJson.Substring(0, sTempJson.IndexOf("\"}"));
                    break;
                //case 2:
                //    break;
                default:
                    return;
            }
        }

        //產生答案亂數且不能重複的三位數
        private static string NewGame()
        {
            Random crandom = new Random();
            m_iAnswerArr[0] = crandom.Next(10);
            do
            {
                m_iAnswerArr[1] = crandom.Next(10);
            } while (m_iAnswerArr[0] == m_iAnswerArr[1]);
            do
            {
                m_iAnswerArr[2] = crandom.Next(10);
            } while (m_iAnswerArr[0] == m_iAnswerArr[1] || m_iAnswerArr[0] == m_iAnswerArr[2] || m_iAnswerArr[1] == m_iAnswerArr[2]);
            sAnswer = m_iAnswerArr[0].ToString() + m_iAnswerArr[1].ToString() + m_iAnswerArr[2].ToString();
            return sAnswer;
        }

        private static string CheckGuess(string sGuess)
        {
            if(sGuess == sAnswer)
            {
                return "3A0B";
            }
            int iAcount = 0;
            int iBcount = 0;

            
            m_iGuessArr[0] = Int32.Parse(sGuess.Substring(0, 1));
            m_iGuessArr[1] = Int32.Parse(sGuess.Substring(1, 1));
            m_iGuessArr[2] = Int32.Parse(sGuess.Substring(2, 1));


            for (int k = 0; k < 3; k++)
            {
                for (int l = 0; l < 3; l++)
                {
                    if (m_iGuessArr[k] == m_iAnswerArr[l])
                    {
                        if (k == l) { iAcount++; }
                        else if (k != l) { iBcount++; }
                    }
                }
            }

            return iAcount.ToString() + "A" + iBcount.ToString() + "B";
        }
    }
}
