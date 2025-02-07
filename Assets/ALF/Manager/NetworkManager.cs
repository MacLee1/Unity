#if USE_NETWORK

using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;

using System.Security.Cryptography;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using WebSocketSharp;
using ALF;
using ALF.MACHINE;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;

namespace ALF.NETWORK
{
    public interface IBaseNetwork
    {
        void NetworkProcessor(NetworkData data,bool bSuccess);
    }

    public interface ISoketNetwork
    {
        void SoketProcessor(NetworkData data);
    }

    public class NetworkData : IBase
    {
        public uint Id  {get;}
        public string Text { get; private set;}
        public byte[] Data {get; private set;}
        public JObject Json {get; private set;}
        public string ErrorCode  {get; private set;}

        public NetworkData(uint id, string errorCode,string text ,byte[] data)
        {
            Id = id;
            if(string.IsNullOrEmpty(text) || text[0] != '{')
            {
                errorCode = "NETWORK_TIME_OUT";
                text = "{}";
            }
            Text = text;
            
            try {
                /**
                * JObject에서 자료형( Date ) 사용하지 않음, 국가마다 다르게 파싱하는 경우 발생  
                */
                
                Json = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(Text, new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});

                if(Json.ContainsKey("msgId"))
                {
                    Id = (uint)Json["msgId"];
                }
            }
            catch (System.Exception e) {
				UnityEngine.Debug.LogException(e);
			}
            
            Data = data;
            ErrorCode = errorCode;
        }

        public void Dispose(){ Text = null; Data = null; ErrorCode = null; Json = null;}
    }

    public class NetworkManager : IBase
    {
        const uint NETWORK_STATE_TYPE = 100000;
        const uint WEBSOKET_STATE_TYPE = 100001;
        const uint NETWORK_REFRESH_STATE_TYPE = 100002;
        const uint WEBSOKET_PING = 100003;
        // const float WEBSOKET_PING_TIME = 60;
        const float NETWORK_TIME_OUT = 60;
        enum E_REQUEST_DATA_TYPE : byte { Json = 0,Binary,MAX }
        const string LOGIN = "/account/login";

        static string assetsBundlesURL = "http://localhost:80";
        public static string AssetsBundlesURL 
        {
            get
            {
                return assetsBundlesURL;
            }

            set { assetsBundlesURL = value;}
        }

        protected class BaseWebSoket : IBase
        {
            const uint RetryCount = 5;
            WebSocket m_pWebSocket = null;
            uint m_iRetryCount = 0;
            string m_pUrl = null;
            string m_pPort = null;
            
            public BaseWebSoket(string url, string port)
            {
                m_pUrl = url;
                m_pPort = port;
                ALFUtils.Assert(!string.IsNullOrEmpty(m_pUrl), "BaseWebSoket  m_pUrl == null !!!");
                ALFUtils.Assert(!string.IsNullOrEmpty(m_pPort), "BaseWebSoket  m_pPort == null !!!");
            }

            public void Dispose()
            {
                CloseSoket();
            }

            public void CloseSoket()
            {
                if(m_pWebSocket == null) return;
                
                m_pWebSocket.OnOpen -= SoketOpenCallback;
                m_pWebSocket.OnMessage -= SoketMessageCallback;
                m_pWebSocket.OnError -= SoketErrorCallback;
                m_pWebSocket.OnClose -= SoketCloseCallback;
                
                if(m_pWebSocket.IsAlive)
                {
                    m_pWebSocket.Close();
                }

                m_pWebSocket = null;
            }

            bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                // Debug.Log(sender);
                // Debug.Log(certificate.ToString());
                // Debug.Log(chain);
                // Debug.Log(sslPolicyErrors);
                return true; // If the server certificate is valid.
            }

            void SoketOpenCallback(object sender,EventArgs e)
            {
                m_iRetryCount =0;
                if(sender is WebSocket pWebSocket)
                {
                    // if(data.DataType == E_REQUEST_DATA_TYPE.Json)
                    //     {
                    //         netData = new NetworkData(data.Id, data.Request.result == UnityWebRequest.Result.Success ? null : data.Request.result.ToString(),data.Request.downloadHandler.text, null);
                    //     }
                    //     else
                    //     {
                    //         netData = new NetworkData(data.Id,data.Request.result == UnityWebRequest.Result.Success ? null : data.Request.result.ToString(),"{}",data.Request.downloadHandler.data);
                    //     }
                    JObject data = new JObject();
                    data["errorCode"] = 0;
                    data["errorMsg"] = "open";
                    NetworkManager.Instance.PushSoketMessage(new NetworkData(100000,null,data.ToString(),null));
                }
            }

            void SoketMessageCallback(object sender,MessageEventArgs e)
            {
                if(sender is WebSocket pWebSocket)
                {
                    if (e.IsText) 
                    {
                        NetworkManager.Instance.PushSoketMessage(new NetworkData(100000,null,e.Data,null));
                    }
                    // else if (e.IsBinary) 
                    // {
                    // }
                }
            }

            void SoketCloseCallback(object sender,CloseEventArgs e)
            {
                if(sender is WebSocket pWebSocket)
                {
                    JObject data = new JObject();
                    data["errorCode"] = e.Code;//1006
                    data["errorMsg"] = e.Reason;//"An exception has occurred while connecting.
                    NetworkManager.Instance.PushSoketMessage(new NetworkData(100000,"SoketClose",data.ToString(),null));
                }
            }

            void SoketErrorCallback(object sender,ErrorEventArgs e)
            {
                if(sender is WebSocket pWebSocket)
                {
                    JObject data = new JObject();
                    data["errorCode"] = 100000;
                    data["errorMsg"] = e.Message;
                    NetworkManager.Instance.PushSoketMessage(new NetworkData(100000,"SoketError",data.ToString(),null));
                }
            }

            public void SendMessage( JObject message)
            {
                if(m_pWebSocket == null || !m_pWebSocket.IsAlive)
                {
                    m_iRetryCount =0;
                    Connet();
                }
                
                if(m_pWebSocket != null)
                {
                    m_pWebSocket.Send(message.ToString());
                }
            }

            public void Connet()
            {
                CloseSoket();
                JObject data = new JObject();
                ++m_iRetryCount;
                if(m_iRetryCount >= RetryCount)
                {
                    data["errorCode"] = 400000;
                    data["errorMsg"] = "RetryCount max!";
                    NetworkManager.Instance.PushSoketMessage(new NetworkData(100000,"SoketClose",data.ToString(),null));
                    return;
                }
                
                m_pWebSocket = new WebSocket($"{m_pUrl}:{m_pPort}");
                m_pWebSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                m_pWebSocket.SslConfiguration.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
                m_pWebSocket.OnOpen += SoketOpenCallback;
                m_pWebSocket.OnMessage += SoketMessageCallback;
                m_pWebSocket.OnError += SoketErrorCallback;
                m_pWebSocket.OnClose += SoketCloseCallback;
                m_pWebSocket.Connect();
                data["type"] = "auth";
                data["accessToken"] = NetworkManager.GetServerAccessToken();

                m_pWebSocket.Send(data.ToString());
            }

            public bool Ping()
            {
                if(m_pWebSocket != null && m_pWebSocket.IsAlive)
                {
                    return m_pWebSocket.Ping("{}");
                }
                return false;
            }
            public bool IsSocketAlive()
            {
                if(m_pWebSocket != null)
                {
                    return m_pWebSocket.IsAlive;
                }
                return false;
            }
        }
        
        public class CustomCertificateHandler : CertificateHandler
        {
            public CustomCertificateHandler(string url)
            {
            }
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
        // class CertPublicKey : CertificateHandler
        // {
        //     public string PUB_KEY;

        //     // Encoded RSAPublicKey
        //     protected override bool ValidateCertificate(byte[] certificateData)
        //     {
        //         X509Certificate2 certificate = new X509Certificate2(certificateData);
        //         string pk = certificate.GetPublicKeyString();

        //         if (pk.ToLower().Equals(PUB_KEY.ToLower()))
        //             return true;
        //         else
        //             return false;
        //     }
        // }

    //     private void Start()
    //     {
    //         //인증에 필요한 PublicKey 생성
    //         TextAsset tx = Resources.Load<TextAsset>("certificateFile");
    //         byte[] by = Encoding.UTF8.GetBytes(tx.ToString());

    //         X509Certificate2 certificate = new X509Certificate2(by);
    //         pubkey = certificate.GetPublicKeyString();

    //         StartCoroutine(POST("https://url", data));
    //     }

    //     private IEnumerator Post(string url, byte[] data)
    //     {
    //         //byte로 데이터를 전송 해야하는데 UnityWebRequest.POST는 string만 가능 하여 Put으로 넣은뒤 POST로 변경 해서 전송

    //         UnityWebRequest request = UnityWebRequest.Put(url, data);
    //         {
    //             request.method = "POST";
    //             request.certificateHandler = new CertPublicKey{ PUB_KEY = pubkey };
    //             //request.SetRequestHeader("Content-Type", "application/json"); //json전송이 필요하다면

    //             yield return request.SendWebRequest();

    //             if (request.isNetworkError)
    //             {
    //                 Debug.Log(request.error + " / " + request.responseCode);
    //             }
    //             else
    //             {
    //                 Debug.Log(request.downloadHandler.text);
    //             }
    //         }
    //     }
    // }

        class NetworkStateData : IStateData
        {
            public bool IsShowLoading {get; protected set;}
            public float ShowWaitTime {get; set;}
            public uint Id {get;protected set;}
            public UnityWebRequest Request {get; protected set;}
            public E_REQUEST_DATA_TYPE DataType {get;protected set;} 
            public NetworkStateData()
            {
            }
            public NetworkStateData(uint id, UnityWebRequest request,E_REQUEST_DATA_TYPE dataType, bool bShowLoading = false)
            {
                IsShowLoading = bShowLoading;
                ShowWaitTime = 1.5f;
                Id = id;
                Request = request;
                DataType = dataType;
            }
            public void Dispose()
            {
                Request?.Dispose();
                Request = null;
            }
        }

        class RefreshStateData : NetworkStateData
        {
            public BaseState NetworkState {get; set; }
            public RefreshStateData(BaseState state, UnityWebRequest request,E_REQUEST_DATA_TYPE dataType, bool bShowLoading = false)
            {
                IsShowLoading = bShowLoading;
                ShowWaitTime = 1.5f;
                Request = request;
                DataType = dataType;
                NetworkState = state;
            }
            public new void Dispose()
            {
                if(NetworkState != null)
                {
                    StateCache.Instance.AddState(NetworkState);
                }
                Request?.Dispose();
                Request = null;
                NetworkState = null;
            }
        }

        static NetworkManager instance = null;
        static System.DateTime sRecvServerUtc = new System.DateTime();
        // static TimeSpan sServerTimeOffset = new TimeSpan(0);
		static float sSinceTimeAtRecvSvTime = 0;
        static bool bRefreshServerData = false;
        // static System.TimeSpan sServerTimeSpan = System.TimeSpan.Zero;
        Queue<NetworkData> m_pSoketMessageList = new Queue<NetworkData>();
        StateMachine m_pStateMachine = null;
        Animation m_pWaitAnimation = null;
        static string sLogServerUrl = "";
        static string sGameServerUrl = "";
        static string sWebSoketServerUrl = "";
        static string sWebSoketPort = "";
        static string sServerAccessToken = null;
        static string sLastLoginData = null;

        System.DateTime m_pExpireServerAccessTokenTime = new System.DateTime();
        
        IBaseScene m_pAdapterScene = null;
        BaseWebSoket m_pBaseWebSoket = null;

        public static NetworkManager Instance
        {
            get {return instance;}
        }

        public static bool InitInstance()
        {
            if(instance == null)
            {
                instance = new NetworkManager();
                
                return true;
            }

            return false;
        }
        public static void ShowWaitMark(bool bActive)
        {
            if( instance != null )
            {
                if(bActive != instance.m_pWaitAnimation.gameObject.activeSelf)
                {
                    instance.m_pWaitAnimation.gameObject.SetActive(bActive);
                    if(bActive && !instance.m_pWaitAnimation.IsPlaying("loading"))
                    {
                        instance.m_pWaitAnimation.Play();
                    }
                }   
            }
        }

        public static void EnableWaitMark(bool bEnable)
        {
            if( instance != null && instance.m_pWaitAnimation.gameObject.activeSelf)
            {
                instance.m_pWaitAnimation.transform.Find("root").gameObject.SetActive(bEnable);
                if(bEnable)
                {
                    if(!instance.m_pWaitAnimation.IsPlaying("loading"))
                    {
                        instance.m_pWaitAnimation.Play();
                    }
                    ALFUtils.FadeObject(instance.m_pWaitAnimation.transform.Find("back"), 1);
                }
                else
                {
                    instance.m_pWaitAnimation.Stop();
                    ALFUtils.FadeObject(instance.m_pWaitAnimation.transform.Find("back"), -1);
                }
            }
        }
        public static void SetWaitMark(Animation pAnimation)
        {
            if(instance == null || pAnimation == null)
            {
                return;
            }

            pAnimation.gameObject.SetActive(false);
            instance.m_pWaitAnimation = pAnimation;
        }

        public static void ClearServerUrl()
        {
            sWebSoketServerUrl = "";
            sWebSoketPort  = "";
            sGameServerUrl = "";
            sLastLoginData = null;
            SetExpireServerAccessTokenTime(new System.DateTime());
        }

        public static void SetWebSoketServerUrl(string url)
        {
            if(string.IsNullOrEmpty(url))
            {
                sWebSoketServerUrl = "";
                sWebSoketPort = "";
                return;
            }

            sWebSoketServerUrl = url.Substring(0,url.LastIndexOf(':'));
            sWebSoketPort = url.Substring(url.LastIndexOf(':')+1);
        }

        public static void SetLogServerUrl(string url)
        {
            sLogServerUrl = url;
        }

        public static void SetGameServerUrl(string url)
        {
            sGameServerUrl = url;
        }

        public static bool IsUseNetwork()
        {
            if(instance != null )
            {
                return instance.m_pStateMachine.IsCurrentTargetStates(Director.Runner,(uint)NETWORK_STATE_TYPE);
            }

            return false;
        }

        public static bool IsLastLogin()
        {
            if(string.IsNullOrEmpty(sServerAccessToken) )
            {
                return false;
            }

            return true;
        }

        public static void ClearServerAccessToken()
        {
            sServerAccessToken = null;
        }

        public static void SetServerAccessToken(string accessToken)
        {
            sServerAccessToken = accessToken;

            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),-1, WEBSOKET_STATE_TYPE, null, instance.executeSoketCallback);
            instance.m_pStateMachine.AddState(pBaseState);
            NetworkManager.ConnectSoketServer();
        }

        public static string GetServerAccessToken()
        {
            return sServerAccessToken;
        }

        public static void SetExpireServerAccessTokenTime(System.DateTime expireTime)
        {
            if(instance == null )
            {
                return;
            }
            // instance.m_pExpireServerAccessTokenTime = expireTime.Add(sServerTimeOffset);
            instance.m_pExpireServerAccessTokenTime = expireTime;
        }

        public static long ConvertLocalGameTimeTick(string strTime)
        {
            // return (System.DateTime.Parse(strTime) - sServerTimeOffset).Ticks;             
            return System.DateTime.Parse(strTime).Ticks;
        }

        public static void SetGameServerTime(System.DateTime serverTime)
        {
            sRecvServerUtc = serverTime;
            // sServerTimeOffset = DateTime.Now - sRecvServerUtc;
            sSinceTimeAtRecvSvTime = Time.realtimeSinceStartup;
            SetupRefreshTime();
        }
        public static System.DateTime GetGameServerTime()
        {
            // return sRecvServerUtc + System.TimeSpan.FromSeconds(Time.realtimeSinceStartup - sSinceTimeAtRecvSvTime) - sServerTimeOffset;
            return sRecvServerUtc + System.TimeSpan.FromSeconds(Time.realtimeSinceStartup - sSinceTimeAtRecvSvTime);
        }

        public static void RefreshServerDataComplete()
        {
            bRefreshServerData = false;
        }

        public static void SetupRefreshTime()
        {
            if(bRefreshServerData) return;

            List<BaseState> list = instance.m_pStateMachine.GetCurrentTargetStates<BaseState>(NETWORK_REFRESH_STATE_TYPE);
            for(int i =0; i < list.Count; ++i)
            {
                list[i].SetExitCallback(null);
                list[i].Exit(true);
            }
            
            instance.m_pStateMachine.AddState(BaseState.GetInstance(new BaseStateTarget(Director.Runner),GetGameServerTimeOfDayTime(), NETWORK_REFRESH_STATE_TYPE, null, null,instance.exitRefreshTimeCallback,-1));
        }

        public static float GetGameServerTimeOfDayTime()
        {
            DateTime serverT = GetGameServerTime();
            TimeSpan pTimeSpan = serverT.ToUniversalTime() - serverT.AddHours(-9);

            long timeOfDay = TimeSpan.TicksPerDay - (serverT.TimeOfDay.Ticks + pTimeSpan.Ticks);
            if(timeOfDay <= 0)
            {
                return 0;
            }
            
            return (float)(timeOfDay / (float)TimeSpan.TicksPerSecond);
        }

        public static void SetAdapterScene( IBaseScene pScene )
        {
            if(instance != null)
            {
                instance.m_pAdapterScene = pScene;
            }
        }

        public static IBaseScene GetAdapterScene()
        {
            if(instance != null )
            {
                return instance.m_pAdapterScene;
            }
            return null;
        }

        public static void SetLastLoginData( string data)
        {
            if(string.IsNullOrEmpty(data))
            {
                return;
            }

            sLastLoginData = data;
        }

        public static void ConnectSoketServer()
        {
            if(instance != null)
            {
                if(instance.m_pBaseWebSoket == null)
                {
                    instance.m_pBaseWebSoket = new BaseWebSoket(sWebSoketServerUrl,sWebSoketPort /*, params string[] protocols */);
                }
                
                instance.m_pBaseWebSoket.Connet();
                SetupPing();
            }
        }

        static void SetupPing()
        {
            if(instance != null)
            {
                List<BaseState> list = instance.m_pStateMachine.GetCurrentTargetStates<BaseState>(WEBSOKET_PING);
                int i = list.Count;
                while(i > 0)
                {
                    --i;
                    list[i].Exit(true);
                }
                list.Clear();

                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),NETWORK_TIME_OUT * 10, WEBSOKET_PING, null, instance.executeSoketPingCallback);
                instance.m_pStateMachine.AddState(pBaseState);
            }
        }

        public static void CloseSoket()
        {
            if(instance != null && instance.m_pBaseWebSoket != null)
            {
                instance.m_pBaseWebSoket.CloseSoket();
            }
        }

        public static void SendMessage( JObject message)
        {
            if(instance != null )
            {
                instance.m_pBaseWebSoket.SendMessage(message);
            }
        }

        public static bool IsSocketAlive()
        {
            if(instance != null && instance.m_pBaseWebSoket != null)
            {
                return instance.m_pBaseWebSoket.IsSocketAlive();
            }

            return false;
        }

        protected NetworkManager()
        {
            m_pStateMachine = StateMachine.Create();
            Director.Instance.AddSchedule(m_pStateMachine);
        }

        public void Dispose()
        {
            m_pStateMachine?.Dispose();
            m_pWaitAnimation = null;
            m_pStateMachine = null;
            m_pAdapterScene = null;
            m_pBaseWebSoket?.CloseSoket();
        }

        public void PushSoketMessage(NetworkData data)
        {
            if(data != null)
            {
                m_pSoketMessageList.Enqueue(data);
            }
        }

        void enterNetworkCallback(IState state)
        {
            if (state.StateData is NetworkStateData data)
            {
                if(data.IsShowLoading)
                {
                    LayoutManager.Instance.InteractableDisableAll();
                    m_pWaitAnimation.gameObject.SetActive(true);
                    m_pWaitAnimation.transform.Find("root").gameObject.SetActive(true);
                    ALFUtils.FadeObject(m_pWaitAnimation.transform.Find("back"), 1);
                    m_pWaitAnimation.Play();
                    data.ShowWaitTime = 0;
                }
                data.Request.SendWebRequest();
            }
        }

        IState exitRefreshTimeCallback(IState state)
        {
            bRefreshServerData = true;
            m_pAdapterScene?.RefreshServerData();
            return null;
        }

        bool executeRefreshCallback(IState state,float dt,bool bEnd)
        {
            if (state.StateData is RefreshStateData data)
            {
                if(data.ShowWaitTime > 0)
                {
                    data.ShowWaitTime = data.ShowWaitTime - dt;
                    if(data.ShowWaitTime < 0)
                    {
                        m_pWaitAnimation.gameObject.SetActive(true);
                        m_pWaitAnimation.transform.Find("root").gameObject.SetActive(true);
                        ALFUtils.FadeObject(m_pWaitAnimation.transform.Find("back"), 1);
                        m_pWaitAnimation.Play();
                        data.ShowWaitTime = 0;
                    }
                }

                bEnd = data.Request.result != UnityWebRequest.Result.InProgress;

                if(bEnd)
                {
                    bool bOk = false;
                    if(data.Request.result == UnityWebRequest.Result.Success )
                    {
                        JObject jdata = null; 
                        if(data.DataType == E_REQUEST_DATA_TYPE.Json)
                        {
                            /**
                            * JObject에서 자료형( Date ) 사용하지 않음, 국가마다 다르게 파싱하는 경우 발생  
                            */
                            jdata = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(data.Request.downloadHandler.text, new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});

                            if(jdata.ContainsKey("errorCode"))
                            {
                                bOk = (int)jdata["errorCode"] == 0;
                                if(bOk)
                                {
                                    if(jdata.ContainsKey("tServer"))
                                    {
                                        System.DateTime pTime;
                                        if(System.DateTime.TryParse((string)jdata["tServer"],out pTime))
                                        {
                                            NetworkManager.SetGameServerTime(pTime);
                                        }
                                        else
                                        {
                                            ALFUtils.Assert( false, $"NetworkManager executeRefreshCallback System.DateTime.TryParse error strServerTime = {(string)jdata["tServer"]}");
                                        }

                                        if(jdata.ContainsKey("tExpire"))
                                        {
                                            if(System.DateTime.TryParse((string)jdata["tExpire"],out pTime))
                                            {
                                                NetworkManager.SetExpireServerAccessTokenTime(pTime);
                                            }
                                            else
                                            {
                                                ALFUtils.Assert( false, $"NetworkManager executeRefreshCallback System.DateTime.TryParse error strExpire = {(string)jdata["tExpire"]}");
                                            }
                                        }
                                    }

                                    if(jdata.ContainsKey("accessToken"))
                                    {
                                        NetworkManager.SetServerAccessToken((string)jdata["accessToken"]);
                                        if(data.NetworkState.StateData is NetworkStateData _data)
                                        {
                                            _data.Request.SetRequestHeader("auth", sServerAccessToken);
                                            m_pStateMachine.AddState(data.NetworkState);
                                        }
                                    }
                                    data.NetworkState = null;
                                }
                            }    
                        }
                    }

                    if(!bOk)
                    {
                        if(data.IsShowLoading)
                        {
                            m_pWaitAnimation.Stop();
                            m_pWaitAnimation.gameObject.SetActive(false);
                            LayoutManager.Instance.InteractableEnabledAll();
                        }
                        m_pAdapterScene.NetworkProcessor(new NetworkData(data.Id,data.Request.result.ToString(),null,null),false);
                        data.NetworkState = null;
                    }
                }
            }

            return bEnd;
        }
        
        bool executeNetworkCallback(IState state,float dt,bool bEnd)
        {
            if (state.StateData is NetworkStateData data)
            {
                if(data.ShowWaitTime > 0)
                {
                    data.ShowWaitTime = data.ShowWaitTime - dt;
                    if(data.ShowWaitTime < 0)
                    {
                        m_pWaitAnimation.gameObject.SetActive(true);
                        m_pWaitAnimation.Play();
                        data.ShowWaitTime = 0;
                    }
                }
                
                if(bEnd)
                {
                    if(data.IsShowLoading)
                    {
                        m_pWaitAnimation.Stop();
                        m_pWaitAnimation.gameObject.SetActive(false);
                        LayoutManager.Instance.InteractableEnabledAll();
                    }

                    m_pAdapterScene.NetworkProcessor(new NetworkData(data.Id,"NETWORK_TIME_OUT","{}",null),false);
                }
                else
                {
                    bEnd = data.Request.result != UnityWebRequest.Result.InProgress;

                    if(bEnd)
                    {
                        if(data.IsShowLoading)
                        {
                            m_pWaitAnimation.Stop();
                            m_pWaitAnimation.gameObject.SetActive(false);
                            LayoutManager.Instance.InteractableEnabledAll();
                        }

                        NetworkData netData = null;

                        if(data.Request.downloadHandler != null)
                        {
                            if(data.DataType == E_REQUEST_DATA_TYPE.Json)
                            {
                                netData = new NetworkData(data.Id, data.Request.result == UnityWebRequest.Result.Success ? null : data.Request.result.ToString(),data.Request.downloadHandler.text, null);
                            }
                            else
                            {
                                netData = new NetworkData(data.Id,data.Request.result == UnityWebRequest.Result.Success ? null : data.Request.result.ToString(),"{}",data.Request.downloadHandler.data);
                            }
                        }
                        else
                        {
                            netData = new NetworkData(data.Id,data.Request.result.ToString(),"{}",null);
                        }
                        m_pAdapterScene.NetworkProcessor(netData,data.Request.result == UnityWebRequest.Result.Success);
                    }
                }
            }

            return bEnd;
        }

        bool executeSoketCallback(IState state,float dt,bool bEnd)
        {
            NetworkData pData = null;
            while(m_pSoketMessageList.Count > 0)
            {
                pData = m_pSoketMessageList.Dequeue();
                m_pAdapterScene?.SoketProcessor(pData);
            }
            
            return false;
        }

        bool executeSoketPingCallback(IState state,float dt,bool bEnd)
        {
            if(bEnd)
            {
                TimeOutCondition pTimeOutCondition = state.GetCondition<TimeOutCondition>(); 
                pTimeOutCondition.Reset();
                pTimeOutCondition.SetRemainTime(NETWORK_TIME_OUT * 10);
                if(m_pBaseWebSoket.IsSocketAlive())
                {
                    if(!m_pBaseWebSoket.Ping())
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        static public void SendLog(Newtonsoft.Json.Linq.JObject data)
        {
            if(string.IsNullOrEmpty(sLogServerUrl) || data == null || !data.HasValues ) return;

            string url = $"{sLogServerUrl}/admin/clientReport";
            UnityWebRequest req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            req.SetRequestHeader("Content-Type", "application/json");
            req.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(data.ToString()));
            req.uploadHandler.contentType = "application/json";
			req.downloadHandler = new DownloadHandlerBuffer();
            req.certificateHandler = new CustomCertificateHandler(url);
            req.SendWebRequest();
        }

        public void SendRequest(uint id, string api,bool bShowLoading = false,bool bPost = true, JObject header = null, JObject body = null)
        {
            ALFUtils.Assert(string.IsNullOrEmpty(api) == false, $"NetworkManager : SendRequest id :{id} api = null !!" );

            Debug.Log($"===========> NetworkManager : SendRequest id :{id} api: {sGameServerUrl}{api} header:{(header == null ? 0 : header.ToString())} body:{(body == null ? 0 : body.ToString())} bPost:{bPost} bShowLoading:{bShowLoading} <===========" );
            // if (Application.internetReachability == NetworkReachability.NotReachable)
            // {
            //     if(m_pAdapterScene != null)
            //     {
            //         JObject pJObject = new JObject();
            //         pJObject["msgId"] = id;
            //         m_pAdapterScene.NetworkProcessor(new NetworkData(id,"NotReachable",pJObject.ToString(),null),false);
            //     }
            //     return;
            // }
            // else
            // {
            //     // StartCoroutine(DoPing()); //It could be a network connection but not internet access so you have to ping your host/server to be sure.
            // }
            
            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),NETWORK_TIME_OUT, NETWORK_STATE_TYPE, this.enterNetworkCallback, this.executeNetworkCallback);
            pBaseState.StateData = new NetworkStateData(id,CreateUnityWebRequest(bPost,$"{sGameServerUrl}{api}",header,body),E_REQUEST_DATA_TYPE.Json,bShowLoading);
            
            if(!string.IsNullOrEmpty(sGameServerUrl) && m_pExpireServerAccessTokenTime.Ticks > 0 && !string.IsNullOrEmpty(sLastLoginData) && m_pExpireServerAccessTokenTime <= NetworkManager.GetGameServerTime() )
            {
                BaseState pState = pBaseState;
                pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),-1, NETWORK_STATE_TYPE, this.enterNetworkCallback, this.executeRefreshCallback,null,-1);
                pBaseState.StateData = new RefreshStateData(pState,CreateUnityWebRequest(true,$"{sGameServerUrl}{LOGIN}",null,Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(sLastLoginData, new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None})),E_REQUEST_DATA_TYPE.Json,true);
            }
            m_pStateMachine.AddState(pBaseState);
        }

        UnityWebRequest CreateUnityWebRequest(bool bPost, string serverUrl, JObject header = null, JObject body = null)
		{
            UnityWebRequest req = new UnityWebRequest(serverUrl, bPost ? UnityWebRequest.kHttpVerbPOST : UnityWebRequest.kHttpVerbGET);

            if(bPost)
            {
                req.SetRequestHeader("Content-Type", "application/json");

                if(!string.IsNullOrEmpty(sServerAccessToken))
                {
                    req.SetRequestHeader("auth",sServerAccessToken);
                }
            }

            if(body != null)
            {
                req.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(body.ToString()));
                req.uploadHandler.contentType = "application/json";
            }

            if(header != null)
            {
                var itr = header.GetEnumerator();

                while(itr.MoveNext())
                {
                    req.SetRequestHeader(itr.Current.Key, itr.Current.Value.ToString());
                }
            }
			
			req.downloadHandler = new DownloadHandlerBuffer();
            
			req.certificateHandler = new CustomCertificateHandler(serverUrl);
			return req;
		}
    }
}
#endif