using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using ALF;
using ALF.NETWORK;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using UnityEngine.UI;
using DATA;
using STATEDATA;
using Newtonsoft.Json.Linq;
using ALF.LOGIN;
// using Firebase.Analytics;

#if USE_HIVE
using hive;
#else
#if UNITY_IOS
// using Unity.Advertisement.IosSupport.Components;
using Unity.Advertisement.IosSupport;
#endif

#if UNITY_ANDROID
// using Google.Play.Review;
#endif
#endif
// using System;
// using UnityEngine.Events;

// using UnityEngine.Networking;

// using System.Net;
// using System.Net.Mail;
// using System.Net.Security;
// using System.Security.Cryptography.X509Certificates;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;

public class IntroScene : IBaseScene
{
    E_LOGIN_TYPE m_eLoginType = E_LOGIN_TYPE.auto;
    public bool Transition {get; set;}
    Dictionary<string,IBaseUI> m_pUIClassList = new Dictionary<string,IBaseUI>();

    TMPro.TMP_Text m_pVersionText = null;
    Image m_pInitGauge = null;
    TMPro.TMP_Text m_pInitMessageText = null;

    Dictionary<string,List<IBaseUI>> m_pMutiUIList = new Dictionary<string,List<IBaseUI>>();
    protected IntroScene(){}
    bool m_bMaintenanceCheck = false;
    
    public static IntroScene Create(E_LOGIN_TYPE eLoginType)
    {
        IntroScene pIntroScene = new IntroScene();
        /**
        *  eLoginType: 최초 실행시 해당 값으로 로그인 처리( 기본 : auto )
        */
        pIntroScene.m_eLoginType = eLoginType;
        return pIntroScene;
    }

    public void OnExit()
    {
        Director.SetApplicationFocus(null);
        Director.SetApplicationPause(null);
        Director.SetApplicationQuit(null);
    }

    public void Dispose()
    {
        m_pInitGauge = null;
        m_pInitMessageText = null;
        m_pVersionText = null;
        var itr = m_pUIClassList.GetEnumerator();

        while(itr.MoveNext())
        {
            LayoutManager.Instance.AddItem(itr.Current.Key,itr.Current.Value.MainUI);
            itr.Current.Value.Dispose();
        }

        var it = m_pMutiUIList.GetEnumerator();
            
        while(it.MoveNext())
        {
            for(int i =0; i < it.Current.Value.Count; ++i)
            {
                if(it.Current.Value[i].MainUI != null)
                {
                    it.Current.Value[i].MainUI.gameObject.SetActive(false);
                }
                LayoutManager.Instance.AddItem(it.Current.Key,it.Current.Value[i].MainUI);
                it.Current.Value[i].Dispose();
                it.Current.Value[i] = null;
            }
            it.Current.Value.Clear();
        }
        m_pMutiUIList.Clear();
        m_pMutiUIList = null;
    
        m_pUIClassList.Clear();
        m_pUIClassList = null;
    }

    void OnApplicationFocus(bool hasFocus)
    {
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if(pauseStatus)
        {
            StateMachine.PauseSchedulerAndActions();
            SoundManager.Instance.Pause();
        }
        else
        {
            StateMachine.ResumeSchedulerAndActions();
            SoundManager.Instance.Resume();
        }
    }

    void OnApplicationQuit()
    {
        StateMachine.PauseSchedulerAndActions();
        StateMachine.UnscheduleUpdate();
        Director.Instance.Dispose();
    }

    public void OnEnter()
    {
        LayoutManager.Instance.InteractableDisableAll();
        Director.SetApplicationFocus(OnApplicationFocus);
        Director.SetApplicationPause(OnApplicationPause);
        Director.SetApplicationQuit(OnApplicationQuit);

        GameContext pGameContext = GameContext.getCtx();
        
        RectTransform mainUI = LayoutManager.Instance.GetItem<RectTransform>("IntroUI");
        mainUI.gameObject.name = "IntroUI";
        LayoutManager.Instance.SetMainUI(mainUI);
        SingleFunc.SetupLocalizingText(mainUI);
        m_pVersionText = mainUI.Find("ver").GetComponent<TMPro.TMP_Text>();
        m_pInitGauge = mainUI.Find("gauge/fill").GetComponent<Image>();
        m_pInitGauge.fillAmount = 0;
        m_pInitMessageText = mainUI.Find("gauge/msg").GetComponent<TMPro.TMP_Text>();
        
        SetupUI();
        
        /**
        *  메인 업데이트 시작
        */
        StateMachine.ScheduleUpdate(true);
#if !UNITY_EDITOR && UNITY_ANDROID
        /**
        *  안드로이드 벡버튼 처리
        */
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),-1, (uint)E_STATE_TYPE.Timer, null, this.executeUpdateCallback,null,-1);
        StateMachine.GetStateMachine().AddState(pBaseState);
#endif

    }

    public void OnTransitionEnter()
    {
        StateMachine.ResumeSchedulerAndActions();
        RectTransform main = LayoutManager.Instance.GetMainUI();
        LayoutManager.SetReciveUIButtonEvent(main,ButtonEventCall); 
        m_bMaintenanceCheck = false;
        GetServerInfo();
        StateMachine.GetStateMachine().AddState(BaseState.GetInstance(new BaseStateTarget(main),-1, (uint)E_STATE_TYPE.Timer, null, executeCheckCallback,null));

#if !USE_HIVE
#if UNITY_IOS
        // check with iOS to see if the user has accepted or declined tracking
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
#endif
#endif
    }

    public void RefreshServerData()
    {
        
    }

    public T GetInstance<T>() where T : IBaseUI, new()
    {
        string key = typeof(T).Name;
     
        if(m_pUIClassList.ContainsKey(key))
        {
            if(key == "Message" || key == "Confirm")
            {
                if(m_pUIClassList[key].MainUI.gameObject.activeSelf)
                {
                    if(m_pMutiUIList.ContainsKey(key))
                    {
                        for(int i =0; i < m_pMutiUIList[key].Count; ++i)
                        {
                            if(!m_pMutiUIList[key][i].MainUI.gameObject.activeSelf)
                            {
                                return (T)m_pMutiUIList[key][i];
                            }
                        }
                    }
                    else
                    {
                        m_pMutiUIList[key] = new List<IBaseUI>();
                    }
                    
                    T temp1 = new T();

                    RectTransform tmepUI = LayoutManager.Instance.FindUI<RectTransform>(key);
                    tmepUI.localScale =  Vector2.one;
                    tmepUI.SetParent(LayoutManager.Instance.GetMainCanvas(),false);
                    tmepUI.SetAsLastSibling();

                    temp1.OnInit(this,tmepUI);
                    temp1.MainUI.gameObject.name = key;
                    SingleFunc.SetupLocalizingText(temp1.MainUI);
                    m_pMutiUIList[key].Add(temp1);
                    return temp1;
                }
            }

            return (T)m_pUIClassList[key];
        }
        
        T temp = new T();

        if(key == "Message" || key == "Confirm")
        {
            temp.OnInit(this,LayoutManager.Instance.FindUIFormRoot<RectTransform>(key));
        }
        else
        {
            temp.OnInit(this,LayoutManager.Instance.FindUI<RectTransform>(key));
        }
        
        m_pUIClassList[key] = temp;
        temp.MainUI.gameObject.name = key;
        SingleFunc.SetupLocalizingText(temp.MainUI);
        return temp;
    }
    public bool IsShowInstance<T>() where T : IBaseUI
    {
        string key = typeof(T).Name;
     
        if(m_pUIClassList.ContainsKey(key))
        {
            if(m_pMutiUIList.ContainsKey(key))
            {
                for(int i =0; i < m_pMutiUIList[key].Count; ++i)
                {
                    if(m_pMutiUIList[key][i].MainUI.gameObject.activeSelf)
                    {
                        return true;
                    }
                }
            }
            
            return m_pUIClassList[key].MainUI.gameObject.activeSelf;
        }
        
        return false;
    }

    void SetupUI()
    {
        GameContext gtx = GameContext.getCtx();
        LayoutManager layoutManager = LayoutManager.Instance;
        RectTransform  ui = layoutManager.FindUI<RectTransform>("title");
        ui.gameObject.SetActive(false);
        ui.Find("Play").gameObject.SetActive(false);
    }
#if USE_HIVE

    void OnPromotionViewCB(hive.ResultAPI pResult, hive.PromotionEventType ePromotionEventType)
    {
        NetworkManager.ShowWaitMark(false);
        Transform tm = LayoutManager.Instance.FindUI<Transform>("title/Play");
        tm.gameObject.SetActive(true);
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(tm),2f, (uint)E_STATE_TYPE.Loop, null, this.executeStartUICallback,null);
        LoopStateData pStateData = new LoopStateData();
        pStateData.Pos = tm.localPosition;
        pBaseState.StateData = pStateData;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    void LoginSuccess(int iResult)
    {
        NetworkManager.ShowWaitMark(false);
        ResultAPI.Code eCode = (ResultAPI.Code)iResult;
        if(eCode == ResultAPI.Code.AuthV4ConflictPlayer)
        {
            ShowMessagePopup(GameContext.getCtx().GetLocalizingText("MSG_TXT_TRY_AGAIN"),ShowLogin);
        }
        else if(eCode != ResultAPI.Code.Success && eCode != ResultAPI.Code.AuthV4AlreadyAuthorized )
        {
            ShowLogin();    
        }        
    }
#endif

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if (sender.name == "title")
        {
            sender.SetActive(false);

            List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(sender.transform.Find("Play"),(uint)E_STATE_TYPE.Loop);
            for(int i = 0; i < list.Count; ++i)
            {
                list[i].Exit(true);
            }

            GameContext pGameContext = GameContext.getCtx();
            if(NetworkManager.IsLastLogin())
            {
                CheckRecordMatchData();
            }
            else
            {
#if USE_HIVE
                LoginManager.Instance.ShowSignInHive((uint)E_REQUEST_ID.account_login,pGameContext.GetNetworkAPI(E_REQUEST_ID.account_login),(LoginManager.E_SOCIAL_PROVIDER)pGameContext.GetLastLoginSocialProvider(),LoginSuccess);
#else
                LoginManager.Instance.Login((uint)E_REQUEST_ID.account_login, pGameContext.GetNetworkAPI(E_REQUEST_ID.account_login), (LoginManager.E_SOCIAL_PROVIDER)pGameContext.GetLastLoginSocialProvider(),null);
#endif
            }
            
            return;
        }
    }

    IBaseUI GetInstanceByUI(RectTransform ui)
    {
        var itr = m_pUIClassList.GetEnumerator();
        while(itr.MoveNext())
        {
            if(itr.Current.Value.MainUI == ui)
            {
                return itr.Current.Value;
            }
        }

        var it = m_pMutiUIList.GetEnumerator();
        while(it.MoveNext())
        {
            for(int i =0; i < it.Current.Value.Count; ++i)
            {
                if(it.Current.Value[i].MainUI == ui)
                {
                    return it.Current.Value[i];
                }
            }
        }

        return null;
    }

    bool executeUpdateCallback(IState state,float dt,bool bEnd)
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            if(NetworkManager.IsUseNetwork()) return false;

            RectTransform dlg = LayoutManager.Instance.GetLastPopup();

            if(dlg == null || dlg.gameObject.name == "Login")
            {
                GameContext pGameContext = GameContext.getCtx();
#if USE_HIVE
                pGameContext.HiveExit();
#else
                ShowConfirmPopup( pGameContext.GetLocalizingText("DIALOG_GAMEEXIT_TITLE"),pGameContext.GetLocalizingText("DIALOG_GAMEEXIT_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,() =>{
                    Application.Quit();
                });
#endif
            }
            else
            {
                IBaseUI pIBaseUI = GetInstanceByUI(dlg);
                if(pIBaseUI != null && pIBaseUI.MainUI.gameObject.name != "Download")
                {
                    pIBaseUI?.Close();
                }
            }
        }
        
        return false;
    }

    bool executeCheckCallback(IState state,float dt,bool bEnd)
    {
        GameContext pGameContext = GameContext.getCtx();
        if(m_bMaintenanceCheck)
        {
            NetworkManager.ShowWaitMark(false);
            if(!pGameContext.IsProductInfoList())
            {
                pGameContext.LoadProductInfo();
            }
            NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.ServerInfo, pGameContext.GetNetworkAPI(E_REQUEST_ID.ServerInfo),true,false);
            return true;
        }
        
        return false;
    }

    /**
    *  게임 서버 정보를 받는다.
    * 앱업데이트, 패치 버전, 점검
    * NetworkProcessor에서 처리
    */
    void GetServerInfo()
    {
#if !USE_HIVE
        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.ServerInfo, GameContext.getCtx().GetNetworkAPI(E_REQUEST_ID.ServerInfo),true,false);
    }
#else
        NetworkManager.ShowWaitMark(true);
        AuthV4.checkMaintenance(true, Maintenance);
    }

    void Maintenance(ResultAPI pResult, List<AuthV4.AuthV4MaintenanceInfo> pMaintenanceInfoList)
    {
        switch (pResult.code)
        {
            case ResultAPI.Code.Success:
            {
                m_bMaintenanceCheck = true;
                return;
            }
            case ResultAPI.Code.AuthV4MaintenanceActionDefault_DoExit:
            case ResultAPI.Code.AuthV4MaintenanceActionExit_DoExit:
            case ResultAPI.Code.AuthV4MaintenanceTimeover_DoExit:
            {
                GameContext pGameContext = GameContext.getCtx();
                ShowMessagePopup( pGameContext.GetLocalizingText("MSG_TXT_TRY_AGAIN"),() =>{
                   GetServerInfo();
                });
            }
                break;
            case ResultAPI.Code.AuthV4MaintenanceActionOpenURL_DoExit:
            {
                if(!string.IsNullOrEmpty(pMaintenanceInfoList[0].url))
                {
                    Application.OpenURL(pMaintenanceInfoList[0].url);
                }
            }
                break;
        }
        NetworkManager.ShowWaitMark(false);
    }

    void SendHomeGet()
    {
        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.home_get, GameContext.getCtx().GetNetworkAPI(E_REQUEST_ID.home_get),true,true);
    }

#endif

    /**
    * 로딩 게이지
    */
    public void ShowInitInfo(bool bShow)
    {
        m_pInitGauge.transform.parent.gameObject.SetActive(bShow);
    }

    public void SetInitInfo(float percent,string message)
    {
        m_pInitGauge.fillAmount = percent;
        m_pInitMessageText.SetText(message);
    }
        
    void UserGameDataLoad()
    {
        GameContext.getCtx().GetReqSettingData();
    }
    bool executeStartUICallback(IState state,float dt,bool bEnd)
    {
        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
        
        if (state.StateData is LoopStateData data)
        {            
            Transform target = state.GetTarget<BaseStateTarget>().GetMainTarget<Transform>();
            if(bEnd)
            {
                float time = condition.GetDurationTime();
                condition.Reset();
                condition.SetRemainTime(time);
            }

            float value = Mathf.Sin(condition.GetTimePercent() * Mathf.PI * 2 ) * 15;
            Vector3 newPos = data.Pos;
            newPos.y = value + data.Pos.y;
            target.localPosition = newPos;
        }

        return false;
    }
    
    void RunMainScene(bool bCreateClub)
    {
        LayoutManager.Instance.InteractableDisableAll();
        Director.Instance.RunWithScene(MainScene.Create(bCreateClub),0.7f,true);
    }
    void ShowMainUI(bool bShow)
    {
        RectTransform ui = LayoutManager.Instance.FindUI<RectTransform>("title");
        ui.gameObject.SetActive(bShow);
    }
    /**
    *  로그인 선택 팝업
    */
    void ShowLogin()
    {
        SingleFunc.ShowAnimationDailog(GetInstance<Login>().MainUI,null);
    }
    /**
    *  게임 데이터 패치 팝업
    */
    public Download ShowDownloadPopup( long size ,IEnumerable list, System.Action actionOk = null)
    {
        Download pUI = GetInstance<Download>();
        pUI.SetData(size,list, actionOk);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public Confirm ShowConfirmPopup(string title,string message,string okText,string cancelText = null,bool bClose = false,System.Action actionOk = null,System.Action actionCancel = null)
    {
        Confirm pUI = GetInstance<Confirm>();

        pUI.SetText(title,message,okText,cancelText);
        pUI.ShowCloseButton(bClose);
        pUI.SetOKAction(actionOk);
        pUI.SetCancelAction(actionCancel);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }
    
    public Message ShowMessagePopup(string message,System.Action action = null)
    {
        Message pMessage = GetInstance<Message>();
        pMessage.SetMessage(message,GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"),action);
        SingleFunc.ShowAnimationDailog(pMessage.MainUI,null);
        return pMessage;
    }

    void CatalogsLoadComplete()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(m_eLoginType == E_LOGIN_TYPE.direct )
        {
            LoginManager.Instance.ShowSignInHive((uint)E_REQUEST_ID.account_login,GameContext.getCtx().GetNetworkAPI(E_REQUEST_ID.account_login),(LoginManager.E_SOCIAL_PROVIDER)pGameContext.GetLastLoginSocialProvider(),LoginSuccess);
        }
        else
        {
            bool isLastLogin = pGameContext.IsLastLogin();
            if(isLastLogin)
            {
                ShowMainUI(true);
                if(Application.isEditor)
                {
                    OnPromotionViewCB(new hive.ResultAPI(),hive.PromotionEventType.OPEN);
                }
                else
                {
                    NetworkManager.ShowWaitMark(true);
                    pGameContext.ShowHivePromotionByPromotionType(hive.PromotionType.BANNER,false,OnPromotionViewCB);
                }
            }
            else
            {
        //         if(!Application.isEditor)
        //         {
        // #if USE_HIVE && UNITY_ANDROID
        //             if(m_eLoginType == E_LOGIN_TYPE.auto )
        //             {
        //                 LoginManager.Instance.ShowSignInHive((uint)E_REQUEST_ID.account_login,GameContext.getCtx().GetNetworkAPI(E_REQUEST_ID.account_login),LoginManager.E_SOCIAL_PROVIDER.google,LoginSuccess);
        //                 return;   
        //             }
        // #endif
        //         }
                
                ShowLogin();
            }
        }
    }

    void CheckCatalogs(List<IResourceLocator> pUpdateList,AsyncOperationStatus eStatus)
    {
        GameContext pGameContext = GameContext.getCtx();
        if(eStatus == AsyncOperationStatus.Failed)
        {
            ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TXT_TRY_AGAIN"),pGameContext.GetLocalizingText("MSG_TXT_RESOURCE_DOWNLOAD_FAIL"),pGameContext.GetLocalizingText("MSG_TXT_TRY_AGAIN"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,()=>{Director.StartCoroutine(SingleFunc.CheckCatalogs(CheckCatalogs));},() =>{ Application.Quit();});
            return;
        }
        List<object> list = new List<object>();

        if(pUpdateList == null)
        {
            list.Add("Bundle");
        }
        else
        {
            for(int i =0; i < pUpdateList.Count; ++i)
            {
                foreach(var k in pUpdateList[i].Keys)
                {
                    list.Add(k);
                }
            }
            Caching.ClearCache();
        }

        Addressables.GetDownloadSizeAsync((IEnumerable)list).Completed += (AsyncOperationHandle<long> sizeHandle)=>{
            if(sizeHandle.Status == AsyncOperationStatus.Failed)
            {
                ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TXT_TRY_AGAIN"),pGameContext.GetLocalizingText("MSG_TXT_RESOURCE_DOWNLOAD_FAIL"),pGameContext.GetLocalizingText("MSG_TXT_TRY_AGAIN"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,()=>{Director.StartCoroutine(SingleFunc.CheckCatalogs(CheckCatalogs));},() =>{ Application.Quit();});
            }
			else
			{
				if (sizeHandle.Result > 0) 
	            {
	                ShowDownloadPopup(sizeHandle.Result,(IEnumerable)list, CatalogsLoadComplete);
	            }
	            else
	            {
	                pGameContext.BundleUpdate(CatalogsLoadComplete);
	            }
			}
            Addressables.Release(sizeHandle);
        };
    }
    void CheckRecordMatchData()
    {
        GameContext pGameContext = GameContext.getCtx();
        JObject pRecordMatchData = pGameContext.GetRecordMatchData();
        if(pRecordMatchData != null)
        {
            E_REQUEST_ID req = E_REQUEST_ID.ladder_clear;

            if(pRecordMatchData.ContainsKey("matchType"))
            {
                int matchType = (int)pRecordMatchData["matchType"];
                if(matchType == GameContext.CHALLENGE_ID)
                {
                    req = E_REQUEST_ID.challengeStage_clear;
                }
                else if(matchType == GameContext.LADDER_ID)
                {
                    req = E_REQUEST_ID.ladder_clear;
                }
                else
                {
                    req = E_REQUEST_ID.league_clear;
                }
            }

            NetworkManager.Instance.SendRequest((uint)req, pGameContext.GetNetworkAPI(req),true,true,null,pRecordMatchData);
        }
        else
        {
            UserGameDataLoad();
        }
    }
    /**
    *  네이버 라운지(한국), 페이스북 페이지(나머지 국가)
    */
    void ShowSupportPage(string msg)
    {
        GameContext pGameContext = GameContext.getCtx();

        bool bKr = pGameContext.GetCurrentLangauge() == E_DATA_TYPE.ko_KR;
                    
        ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TITLE_NOTICE"),pGameContext.GetLocalizingErrorMsg(msg),pGameContext.GetLocalizingText(bKr ? "MAIN_SUBMENU_BTN_NAVER_LOUNGE" :"MAIN_SUBMENU_BTN_FACEBOOK_FORUM" ),null,false,()=>{
            string token = bKr ? "300114" : "300115";
#if FTM_LIVE
            token = pGameContext.GetConstValue( bKr ? CONSTVALUE.E_CONST_TYPE.NAVER_ROUNGE : CONSTVALUE.E_CONST_TYPE.FACEBOOK_FANPAGE).ToString();
#endif
            pGameContext.ShowHiveCustomView(token,true,(hive.ResultAPI result, hive.PromotionEventType promotionEventType)=>{
                NetworkManager.ShowWaitMark(false);
                if(result.isSuccess())
                {
                    switch(promotionEventType)
                    {
                        case hive.PromotionEventType.OPEN:   // 프로모션 뷰 열림
                            break;
                        case hive.PromotionEventType.CLOSE:   // 프로모션 뷰 닫힘
                        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.ServerInfo, pGameContext.GetNetworkAPI(E_REQUEST_ID.ServerInfo),true,false);
                            break;
                    }
                }
            } );
        });
    }

    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
        if(data == null) return;
        E_REQUEST_ID eID = (E_REQUEST_ID)data.Id;
        GameContext pGameContext = GameContext.getCtx();
        
        Debug.Log( $"E_REQUEST_ID:  {eID}\nJson:{(data.Json == null ? 0 : data.Json.ToString())}\n");
        
        /**
        *  네트워크 에러 처리 
        */
        if( !string.IsNullOrEmpty(data.ErrorCode))
        {
            if(eID == E_REQUEST_ID.ServerInfo)
            {
                ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(data.ErrorCode),()=>{
                    NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.ServerInfo, pGameContext.GetNetworkAPI(E_REQUEST_ID.ServerInfo),true,false);
                });
            }
            else if(eID == E_REQUEST_ID.account_login)
            {
                ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(data.ErrorCode),()=>{
                    bool isLastLogin = pGameContext.IsLastLogin();
                    if(isLastLogin)
                    {
                        OnPromotionViewCB(new hive.ResultAPI(),hive.PromotionEventType.OPEN);
                    }
                    else
                    {
                        ShowLogin();
                    }
                    ShowMainUI(isLastLogin);
                });
            }
            else
            {
                ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(data.ErrorCode));
            }
            
            data.Dispose();
            return;
        }

        JObject networkData = data.Json;
        if(eID == E_REQUEST_ID.ServerInfo)
        {      
            if(networkData.ContainsKey("service"))
            {
                string msg = (string)(networkData["service"]["status"]);
                if(msg != "0")
                {
                    ShowSupportPage(msg);
                    data.Dispose();
                    return;
                }
            }

            if(networkData.ContainsKey("version"))
            {
                string service = null;
            
                JObject pItem = null;
                JArray jArray = (JArray)networkData["version"];
                int i =0;
                SingleFunc.RemoveSendLog();
                
                string store = "google";
#if UNITY_IOS
                store = "apple";
#elif ONESTORE
                store = "onestore";
#endif
                string appBuildVer = ALFUtils.GetVersionCode();
                int iVer = int.Parse(appBuildVer);
                for(i =0; i < jArray.Count; ++i)
                {
                    pItem = (JObject)jArray[i];
                    if((string)pItem["store"] == store )
                    {
                        if(iVer < (int)pItem["id"] && (string)pItem["service"] != "Review")
                        {
                            service = null;
                            break;
                        }

                        if(appBuildVer == (string)pItem["id"])
                        {
                            service = (string)pItem["service"];
                            Configuration.setServerId(service);
                        }
                    }
                }

                if(string.IsNullOrEmpty(service))
                {
                    pItem = (JObject)networkData["store"];
#if UNITY_IOS
                    store = (string)pItem["apple"];
#elif ONESTORE
                    store = (string)pItem["onestore"];
#else
                    store = (string)pItem["google"];
#endif              
                    ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_UPDATENOTICE_TITLE"),pGameContext.GetLocalizingText("DIALOG_UPDATENOTICE_TXT"),pGameContext.GetLocalizingText("DIALOG_UPDATENOTICE_BTN_STORE"),null,false,()=>{Application.OpenURL(store);},null);
                    data.Dispose();
                    return;
                }

                string cdn = (string)networkData["cdn"];
#if UNITY_IOS
                NetworkManager.AssetsBundlesURL = $"{cdn}/iOS/{appBuildVer}/";
#elif UNITY_ANDROID
                NetworkManager.AssetsBundlesURL = $"{cdn}/Android/{appBuildVer}/";
#endif
                ALFUtils.Assert(networkData.ContainsKey("server"),"server is null !!");

                jArray = (JArray)networkData["server"];
                for(i =0; i < jArray.Count; ++i)
                {
                    pItem = (JObject)jArray[i];

                    if((string)pItem["service"] == service)
                    {
                        NetworkManager.SetGameServerUrl((string)pItem["url"]);
                        NetworkManager.SetWebSoketServerUrl((string)pItem["chat"]);
                        break;
                    }
                }
#if !FTM_LIVE
                m_pVersionText.SetText($"{store}:{service}:{appBuildVer}");
#endif
                Director.StartCoroutine(SingleFunc.CheckCatalogs(CheckCatalogs));
            }
        }
        else
        {            
            int errorCode = -1;
            if(networkData.ContainsKey("errorCode"))
            {
                errorCode =(int)networkData["errorCode"];
                
                if(errorCode == 0)
                {
                    /**
                    *  게임 서버 requset 성공
                    */
                    bool bRunMainScene = false;
                    if(eID == E_REQUEST_ID.account_login)
                    {
                        /**
                        *  로그인 성공 이후 클라이언트 정보에 따른 게임서버 데이터 요청 처리 순서
                        *  클라에 이전 경기 기록이 있으면 
                        */  
                        pGameContext.SetUserData(networkData);
                        CheckRecordMatchData();
                    }
                    else if(eID == E_REQUEST_ID.ladder_clear || eID == E_REQUEST_ID.challengeStage_clear || eID == E_REQUEST_ID.league_clear)
                    {
                        pGameContext.DeleteRecordMatchData();
                        UserGameDataLoad();
                    }
                    else if(eID == E_REQUEST_ID.settings_get)
                    {
                        pGameContext.SetSettingData(networkData);
                        SendHomeGet();
                    }
                    else if(eID == E_REQUEST_ID.home_get)
                    {
                        pGameContext.SetGameData(networkData);
                        
                        if(pGameContext.IsAttendReward())
                        {
                            pGameContext.SendReqAttendReward();
                        }
                        else
                        {
                            pGameContext.SendReqLeagueOpps();
                        }
                    }
                    else 
                    {
                        pGameContext.UpdateGameData(eID,networkData,null);
                        
                        switch(eID)
                        {
                            case E_REQUEST_ID.attend_reward:
                            {
                                pGameContext.SendReqLeagueOpps();
                            }
                            break;
                            case E_REQUEST_ID.league_getTodayFixture:
                            {
                                pGameContext.SendReqClubLicensePut(true);
                            }
                            break;
                            case E_REQUEST_ID.clubLicense_put:
                            {
                                JObject pJObject = new JObject();
                                pJObject["type"] = 0;
                                NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.tactics_get, pGameContext.GetNetworkAPI(E_REQUEST_ID.tactics_get),true,true,null,pJObject);
                            }
                            break;
                            case E_REQUEST_ID.tactics_get:
                            {
                                if(pGameContext.GetTutorial() < 30)
                                {
                                    pGameContext.DefaultSuggestionPlayerData();
                                    List<ulong> lineup = pGameContext.GetLineupPlayerIdListByIndex(pGameContext.GetActiveLineUpType());
                                    JArray jArray = new JArray();
                                    for(int i = 0; i < lineup.Count; ++i)
                                    {
                                        jArray.Add(lineup[i]);
                                    }
                                    JObject pJObject = new JObject();
                                    pJObject["type"] = pGameContext.GetActiveLineUpType();
                                    pJObject["data"] = jArray.ToString(Newtonsoft.Json.Formatting.None);
                                    pJObject["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
                                    pJObject["totalValue"] = pGameContext.GetTotalPlayerValue(0);
                                    pJObject["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(null,true);
                                    pJObject["avgAge"] = pGameContext.GetPlayerAvgAge(null,true);
                                    pJObject["playerCount"] = pGameContext.GetTotalPlayerCount();
                                    NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.lineup_put, pGameContext.GetNetworkAPI(E_REQUEST_ID.lineup_put),false,true,null,pJObject);
                                }

                                if(pGameContext.IsGetPrevSeasonReward())
                                {
                                    NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.ladder_rewardStanding, pGameContext.GetNetworkAPI(E_REQUEST_ID.ladder_rewardStanding),true,true,null,null);
                                }
                                else
                                {
                                    bRunMainScene = true;
                                }
                            }
                            break;
                            case E_REQUEST_ID.ladder_rewardStanding: 
                            {
                                NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.ladder_rewardUserRank, pGameContext.GetNetworkAPI(E_REQUEST_ID.ladder_rewardUserRank),true,true,null,null);
                            }
                            break;
                            case E_REQUEST_ID.ladder_rewardUserRank:
                            {
                                bRunMainScene = true;
                            }
                            break;
                        }
                    }
                    
                    data.Dispose();
                    
                    if(bRunMainScene)
                    {
                        LayoutManager.Instance.InteractableDisableAll();
                        pGameContext.AddHiveNews();
                        RunMainScene(false);
                    }
                    return;
                }   
            }
            
            /**
            *  게임 서버 requset 실패
            */
            if(eID == E_REQUEST_ID.home_get && errorCode == 10002) // 구단 생성
            {
                LayoutManager.Instance.InteractableDisableAll();
                pGameContext.DeleteCashData();
                RunMainScene(true);
            }
            else
            {
                string msg =(string)networkData["errorCode"];
                
#if !FTM_LIVE
                if(networkData.ContainsKey("errorMsg"))
                {
                    msg = (string)networkData["errorMsg"];
                }
#endif                
                if(E_REQUEST_ID.ladder_clear == eID || E_REQUEST_ID.challengeStage_clear == eID || E_REQUEST_ID.league_clear == eID)
                {
                    ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(msg),()=>{
                        pGameContext.DeleteRecordMatchData();
                        UserGameDataLoad();
                    });
                }
                else
                {
                    if(errorCode != 1836)
                    {
                        /**
                        * //10	점검 중 // 20	CS 처리 중 // 25	지정된 시간까지 블럭 // 29	영구 정지// 39	유저 탈퇴
                        */
                        if(errorCode == 10)
                        {
                            ShowSupportPage(errorCode.ToString());
                        }
                        else if(errorCode == 20 || errorCode == 25 || errorCode == 29 || errorCode == 39 )
                        {
                            Message pUI = ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(errorCode.ToString()));
                            pUI.MainUI.Find("root/confirm").gameObject.SetActive(false);
                        }
                        else
                        {
                            if(E_REQUEST_ID.ladder_rewardStanding == eID)
                            {
                                NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.ladder_rewardUserRank, pGameContext.GetNetworkAPI(E_REQUEST_ID.ladder_rewardUserRank),true,true,null,null);
                            }
                            else if(E_REQUEST_ID.attend_reward == eID)
                            {
                                pGameContext.SendReqLeagueOpps();
                            }
                            else if(E_REQUEST_ID.ladder_rewardUserRank == eID)
                            {
                                data.Dispose();
                                LayoutManager.Instance.InteractableDisableAll();
                                pGameContext.AddHiveNews();
                                RunMainScene(false);
                                return;
                            }
                            else
                            {
                                ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(msg),()=>{
                                    if((errorCode > 200 && errorCode < 304) || E_REQUEST_ID.home_get == eID )
                                    {
                                        NetworkManager.ClearServerAccessToken();
                                        /**
                                        * 기존에 네트워크 Refresh 로직때문에 해당 시간 초기화..
                                        */
                                        NetworkManager.SetExpireServerAccessTokenTime(new System.DateTime());
                                        ShowMainUI(true);
        #if USE_HIVE
                                        OnPromotionViewCB(new hive.ResultAPI(), hive.PromotionEventType.OPEN);
        #endif
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }
        data.Dispose();
    }

    public void SoketProcessor(NetworkData data)
    {
        if(data == null) return;
        // Debug.Log(recv);
        data.Dispose();
    }
}