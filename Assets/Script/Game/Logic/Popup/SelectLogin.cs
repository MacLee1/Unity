using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;

using ALF.NETWORK;
using ALF.LAYOUT;
using ALF.SOUND;
using USERDATA;
using DATA;
using Newtonsoft.Json.Linq;
using ALF.LOGIN;

#if USE_HIVE
using hive;
#endif


public class SelectLogin : IBaseUI
{
    MainScene m_pMainScene = null;
    Button m_pFacebook = null;
    Button m_pApple = null;
    Button m_pGoogle = null;
    Button m_pLogout = null;
    Button m_pSignout = null;

    GameObject m_pGoogleOn = null;
    GameObject m_pFacebookOn = null;
    GameObject m_pAppleOn = null;
    TMPro.TMP_Text m_pUIDText = null;
    public RectTransform MainUI { get; private set;}

    LoginManager.E_SOCIAL_PROVIDER m_eSelectProvider = LoginManager.E_SOCIAL_PROVIDER.MAX;

    // public bool Enable { set{ if (m_pMessage != null) m_pMessage.enabled = value;}}
// guest =0,facebook,google,apple, dev, gamecenter,hive

    public SelectLogin(){}
    
    public void Dispose()
    {
        m_pFacebook = null;
        m_pApple = null;
        m_pGoogle = null;
        m_pLogout = null;
        m_pSignout = null;
        m_pMainScene = null;
        m_pUIDText = null;
        MainUI = null;
        m_pGoogleOn = null;
        m_pFacebookOn = null;
        m_pAppleOn = null;

    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "SelectLogin : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "SelectLogin : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        GameContext pGameContext = GameContext.getCtx();
        Transform ui = MainUI.Find("root/bg/sns");
        
        m_pFacebook = ui.Find("Facebook").GetComponent<Button>();
        m_pFacebookOn = m_pFacebook.transform.Find("on").gameObject;
        
        m_pApple = ui.Find("Apple").GetComponent<Button>();
        m_pAppleOn = m_pApple.transform.Find("on").gameObject;
        
        m_pGoogle = ui.Find("Google").GetComponent<Button>();
        m_pGoogleOn = m_pGoogle.transform.Find("on").gameObject;
#if UNITY_ANDROID
        m_pGoogle.transform.Find("icon/ios").gameObject.SetActive(false);
        m_pGoogle.transform.Find("icon/android").gameObject.SetActive(true);
#else
        m_pGoogle.transform.Find("icon/ios").gameObject.SetActive(true);
        m_pGoogle.transform.Find("icon/android").gameObject.SetActive(false);
#endif
        
        m_pLogout = ui.Find("logout").GetComponent<Button>();
        m_pSignout = MainUI.Find("root/bg/list/signout").GetComponent<Button>();
        
        m_pUIDText = MainUI.Find("root/bg/list/uid").GetComponent<TMPro.TMP_Text>();

        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupData()
    {
        GameContext pGameContext = GameContext.getCtx();
        
        LoginManager.E_SOCIAL_PROVIDER eCurrent = (LoginManager.E_SOCIAL_PROVIDER)pGameContext.GetLastLoginSocialProvider();
        m_pLogout.gameObject.SetActive(!(eCurrent == LoginManager.E_SOCIAL_PROVIDER.guest));
        
#if UNITY_IOS
        m_pApple.gameObject.SetActive(true);
        m_pSignout.gameObject.SetActive(m_pLogout.gameObject.activeSelf);
#else
        m_pApple.gameObject.SetActive(false);
        m_pSignout.gameObject.SetActive(false);
#endif

#if USE_HIVE
        m_pFacebookOn.SetActive(false);
        m_pAppleOn.SetActive(false);
        m_pGoogleOn.SetActive(false);
        m_pGoogle.enabled = true;
        m_pFacebook.enabled = true;
        m_pApple.enabled = true;
        
        int apple = (pGameContext.ProviderTypeList >> 16) & 255;
        int google = (pGameContext.ProviderTypeList >> 8) & 255;
        int facebook = (pGameContext.ProviderTypeList >> 0) & 255;

        m_pGoogle.gameObject.SetActive( google > 0);
        m_pFacebook.gameObject.SetActive( facebook > 0);
        m_pApple.gameObject.SetActive( apple > 0);

        AuthV4.PlayerInfo pPlayerInfo = AuthV4.getPlayerInfo();
        
        var itr = pPlayerInfo.providerInfoData.GetEnumerator();
        while(itr.MoveNext())
        {
            if(itr.Current.Key == AuthV4.ProviderType.SIGNIN_APPLE)
            {
                m_pAppleOn.SetActive(true);
                m_pApple.enabled = false;    
            }
            else if(itr.Current.Key == AuthV4.ProviderType.GOOGLE)
            {
                m_pGoogleOn.SetActive(true);
                m_pGoogle.enabled = false;
            }
            else if(itr.Current.Key == AuthV4.ProviderType.FACEBOOK)
            {
                m_pFacebookOn.SetActive(true);
                m_pFacebook.enabled = false;
            }
        }
#endif        

        m_pUIDText.SetText($"ID : {pGameContext.GetUserCustomerNo()}" );
    }

    void ConnectSuccess(int iResult)
    {
        NetworkManager.ShowWaitMark(false);
#if USE_HIVE
        ResultAPI.Code eCode = (ResultAPI.Code)iResult;
        switch(eCode)
        {
            case ResultAPI.Code.Success:
            {                
                LoginManager.UpdateCurrentSocialProvider(m_eSelectProvider);
                // PlayerPrefs.SetString("uuid",m_strCurrentSocialID);
                Close();
            }
            return;
            case ResultAPI.Code.AuthV4PlayerChange:
            {
                Change();
            }
            return;
            default:
            {
                m_pMainScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingErrorMsg(eCode.ToString()),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
            }
                // WaitingMgr.Instance.Hide();
                // OneButtonMgr.Instance.Show(SystemMgr.Instance.Quit, _Result.code.ToString(), _Result.errorMessage, "end");
                // 기타 예외 상황 처리
            break;

        }
#endif      
        m_eSelectProvider = LoginManager.E_SOCIAL_PROVIDER.MAX;  
    }

    void ConnectSns()
    {
        NetworkManager.ShowWaitMark(true);
        LoginManager.Instance.Connect(m_eSelectProvider, ConnectSuccess);
    }

    void Change()
    {
        GameContext.getCtx().Logout();
        PlayerPrefs.SetInt("LLSP",(int)m_eSelectProvider);
        m_eSelectProvider = LoginManager.E_SOCIAL_PROVIDER.MAX;
        m_pMainScene.RunIntroSecne(E_LOGIN_TYPE.direct);
    }

    void LogOutSuccess(int iResult)
    {
        GameContext.getCtx().Logout();
        SoundManager.Instance.StopBGM();
        m_eSelectProvider = LoginManager.E_SOCIAL_PROVIDER.MAX;
        m_pMainScene.RunIntroSecne(E_LOGIN_TYPE.show);
    }

    void LogOut()
    {
        LoginManager.Instance.LogOut(LogOutSuccess);
    }

    void SignOut()
    {
        LoginManager.Instance.PlayerDelete(LogOutSuccess);
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch (sender.name)
        {
            case "logout":
            {
                GameContext pGameContext = GameContext.getCtx();
                m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_LOGOUT_TITLE"),pGameContext.GetLocalizingText("DIALOG_LOGOUT_TXT"),pGameContext.GetLocalizingText("DIALOG_LOGOUT_BTN_LOGOUT"),pGameContext.GetLocalizingText("DIALOG_LOGOUT_BTN_CANCEL"),false,LogOut);
            }
            return;
            case "Apple":
            case "Google":
            case "Facebook":
            {
                m_eSelectProvider = (LoginManager.E_SOCIAL_PROVIDER)Enum.Parse(typeof(LoginManager.E_SOCIAL_PROVIDER),sender.name.ToLower());
                GameContext pGameContext = GameContext.getCtx();
                LoginManager.E_SOCIAL_PROVIDER eCurrent = (LoginManager.E_SOCIAL_PROVIDER)pGameContext.GetLastLoginSocialProvider();
                if(eCurrent == LoginManager.E_SOCIAL_PROVIDER.guest)
                {
                    string current = eCurrent.ToString();
                    current = char.ToUpper(current[0]) + current.Substring(1);
                    m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_ACOUNTCHANGE_TITLE"),string.Format(pGameContext.GetLocalizingText("DIALOG_ACOUNTCHANGE_TXT"),current,sender.name) ,pGameContext.GetLocalizingText("DIALOG_ACOUNTCHANGE_BTN_LOGOUT"),pGameContext.GetLocalizingText("DIALOG_ACOUNTCHANGE_BTN_CANCEL"),false,ConnectSns);
                }
                else
                {
                    ConnectSns();
                }
            }
            return;
            case "signout":
            {
                GameContext pGameContext = GameContext.getCtx();
                m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_SIGN_OUT_TITLE"),pGameContext.GetLocalizingText("DIALOG_SIGN_OUT_TXT"),pGameContext.GetLocalizingText("DIALOG_SIGN_OUT_BTN_SIGN_OUT"),pGameContext.GetLocalizingText("DIALOG_SIGN_OUT_BTN_CANCEL"),false,SignOut);
            }
            return;

        }
        
        Close();
    }
}
