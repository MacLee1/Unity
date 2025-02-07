using System;
using UnityEngine;
using UnityEngine.UI;
using ALF;

// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.LOGIN;
// using ALF.MACHINE;
using ALF.SOUND;
using ALF.NETWORK;
using DATA;
using Newtonsoft.Json.Linq;
// using UnityEngine.EventSystems;
using STATEDATA;

#if !USE_HIVE
#if UNITY_IOS || UNITY_IPHONE
using AppleAuth;
#endif
#endif

public class Login : IBaseUI
{
    //E_SOCIAL_PROVIDER
    IntroScene m_pScene = null;
    public RectTransform MainUI { get; private set; }
    Button[] m_pLoginButtonList = new Button[(int)LoginManager.E_SOCIAL_PROVIDER.MAX];
    public Login(){}
    
    public void Dispose()
    {
        m_pScene = null;
        MainUI = null;
        for(int i =0; i < m_pLoginButtonList.Length; ++i)
        {
            m_pLoginButtonList[i] = null;
        }
        m_pLoginButtonList = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Login : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Login : targetUI is null!!");

        m_pScene = (IntroScene)pBaseScene;
        MainUI = pMainUI;

        Transform tm = MainUI.Find("root/list");
        Transform item = null;
        for(LoginManager.E_SOCIAL_PROVIDER e = LoginManager.E_SOCIAL_PROVIDER.guest; e < LoginManager.E_SOCIAL_PROVIDER.MAX; ++e)
        {
            item = tm.Find(e.ToString());
            if(item != null)
            {
                m_pLoginButtonList[(int)e] =item.GetComponent<Button>();
            }
        }
    
#if UNITY_ANDROID
        m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.google].transform.Find("ios").gameObject.SetActive(false);
        m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.google].transform.Find("android").gameObject.SetActive(true);
#else
        m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.google].transform.Find("ios").gameObject.SetActive(true);
        m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.google].transform.Find("android").gameObject.SetActive(false);
#endif

// m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.apple].gameObject.SetActive(AppleAuthManager.IsCurrentPlatformSupported);

#if UNITY_IOS || UNITY_IPHONE
#if USE_HIVE
    
        GameContext pGameContext = GameContext.getCtx();
        int apple = (pGameContext.ProviderTypeList >> 16) & 255;
        int google = (pGameContext.ProviderTypeList >> 8) & 255;
        int facebook = (pGameContext.ProviderTypeList >> 0) & 255;

        m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.google].gameObject.SetActive( google > 0);
        m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.facebook].gameObject.SetActive( facebook > 0);
        m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.apple].gameObject.SetActive( apple > 0);        
#else
            m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.apple].gameObject.SetActive(AppleAuthManager.IsCurrentPlatformSupported);
#endif
#else
            m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.apple].gameObject.SetActive(false);
#endif

        #if UNITY_EDITOR
            m_pLoginButtonList[(int)LoginManager.E_SOCIAL_PROVIDER.google].gameObject.SetActive(false);
        #endif

        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);

    }

    void ShowWaitUI()
    {
        Animation pAnimation = LayoutManager.Instance.FindUIFormRoot<Animation>("UI_Wait");
        pAnimation.gameObject.SetActive(true);
        pAnimation.Play();
    }

    void LoginSuccess(int iResult)
    {
#if USE_HIVE
        NetworkManager.ShowWaitMark(false);
        hive.ResultAPI.Code eCode = (hive.ResultAPI.Code)iResult;
        // if(eCode == hive.ResultAPI.Code.AuthV4ConflictPlayer)
        // {
        //     // string current = eCurrent.ToString();
        //     // current = char.ToUpper(current[0]) + current.Substring(1);
        //     // m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_ACOUNTCHANGE_TITLE"),string.Format(pGameContext.GetLocalizingText("DIALOG_ACOUNTCHANGE_TXT"),current,sender.name) ,pGameContext.GetLocalizingText("DIALOG_ACOUNTCHANGE_BTN_LOGOUT"),pGameContext.GetLocalizingText("DIALOG_ACOUNTCHANGE_BTN_CANCEL"),false,ConnectSns);
        //     m_pScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("MSG_TXT_TRY_AGAIN"),null);
        // }
        // else 
        if(eCode != hive.ResultAPI.Code.Success && eCode != hive.ResultAPI.Code.AuthV4AlreadyAuthorized )
        {
            m_pScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("MSG_TXT_TRY_AGAIN"),null);
            return;
        }

#endif
             
        Close();
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        LoginManager.E_SOCIAL_PROVIDER eType = (LoginManager.E_SOCIAL_PROVIDER)Enum.Parse(typeof(LoginManager.E_SOCIAL_PROVIDER), sender.name);
        
        GameContext pGameContext = GameContext.getCtx();
        pGameContext.SendAdjustEvent("e84usn",true,true,-1);
        if(eType == LoginManager.E_SOCIAL_PROVIDER.guest)
        {
            m_pScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_GUESTLOGIN_TITLE"),pGameContext.GetLocalizingText("DIALOG_GUESTLOGIN_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,()=>{
                LoginManager.Instance.Login((uint)E_REQUEST_ID.account_login, pGameContext.GetNetworkAPI(E_REQUEST_ID.account_login), eType,LoginSuccess);
            });
        }
        else
        {
            LoginManager.Instance.Login((uint)E_REQUEST_ID.account_login, pGameContext.GetNetworkAPI(E_REQUEST_ID.account_login), eType, LoginSuccess);
        }
    }    
}
