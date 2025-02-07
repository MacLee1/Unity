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
using GAMETIP;

public class Setting : IBaseUI
{
    enum E_PUSH :byte {Game,Local,Night,MAX};

    MainScene m_pMainScene = null;
    Transform[] m_pTogleButtonList = new Transform[5];
    // ScrollRect m_pMessage = null;
    TMPro.TMP_Text m_pLangaugeText = null;
    TMPro.TMP_Text m_pCustomerText = null;
    RawImage m_pAccountIcon = null;
    GameObject m_pGuestNode = null;
    GameObject m_pAccountNode = null;

    bool[] m_bPushList = new bool[(int)E_PUSH.MAX]{false,false,false};
    public RectTransform MainUI { get; private set;}

    // public bool Enable { set{ if (m_pMessage != null) m_pMessage.enabled = value;}}

    public Setting(){}
    
    public void Dispose()
    {
        int i =0;
        
        for(i =0; i < m_pTogleButtonList.Length; ++i)
        {
            m_pTogleButtonList[i] = null;
        }
        m_pTogleButtonList = null;
        m_pMainScene = null;
        m_pLangaugeText = null;

        m_pCustomerText = null;
        m_pAccountIcon.texture = null;
        m_pAccountIcon = null;
        m_pGuestNode = null;
        m_pAccountNode = null;
        
        MainUI = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Setting : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Setting : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        RectTransform ui = MainUI.Find("root/bg/sound").GetComponent<RectTransform>();
        
        m_pTogleButtonList[0] = ui.Find("push");
        m_pTogleButtonList[1] = ui.Find("localPush");
        m_pTogleButtonList[2] = ui.Find("night");
        m_pTogleButtonList[3] = ui.Find("music");
        m_pTogleButtonList[4] = ui.Find("sound");

        ui = MainUI.Find("root/bg/account/account").GetComponent<RectTransform>();
        m_pAccountNode = ui.Find("sns").gameObject;
        m_pAccountIcon = m_pAccountNode.GetComponent<RawImage>();
        m_pGuestNode = ui.Find("guest").gameObject;
    
#if FTM_LIVE && UNITY_IOS
        MainUI.Find("root/bg/support/coupon").gameObject.SetActive(Application.isEditor);
#endif

        m_pCustomerText = MainUI.Find("root/bg/customer/text").GetComponent<TMPro.TMP_Text>();
        m_pLangaugeText = MainUI.Find("root/bg/account/lang/title").GetComponent<TMPro.TMP_Text>();
        TMPro.TMP_Text pVersionText = MainUI.Find("root/bg/ver/text").GetComponent<TMPro.TMP_Text>();
        pVersionText.SetText($"{Application.version}_{GameContext.PatchVersion}");
        
        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    void UpdateLanguage()
    {
        GameContext pGameContext = GameContext.getCtx();
        string token = "ENG";
        switch(pGameContext.GetCurrentLangauge())
        {
            case E_DATA_TYPE.ko_KR:
            {
                token = "KOR";
            }
            break;
            case E_DATA_TYPE.zh_CN:
            {
                token = "CHN";
            }
            break;
            case E_DATA_TYPE.zh_TW:
            {
                token = "TWN";
            }
            break;
        }

        m_pLangaugeText.SetText(pGameContext.GetLocalizingText(token));        
    }

    void UpdateSNS()
    {
        ALF.LOGIN.LoginManager.E_SOCIAL_PROVIDER eProvider = ALF.LOGIN.LoginManager.GetCurrentSocialProvider();
        if(eProvider == ALF.LOGIN.LoginManager.E_SOCIAL_PROVIDER.guest)
        {
            m_pGuestNode.SetActive(true);
            m_pAccountNode.SetActive(false);
        }
        else
        {
            string token = eProvider.ToString();
            // token = char.ToUpper(token[0]) + token.Substring(1);
            m_pAccountNode.SetActive(true);
            m_pGuestNode.SetActive(false);
            if(eProvider == ALF.LOGIN.LoginManager.E_SOCIAL_PROVIDER.google)
            {
#if UNITY_ANDROID
                token += "Play";
#endif
            }
            
            m_pAccountIcon.texture = ALF.AFPool.GetItem<Sprite>("Texture",token.ToString()).texture;
        }
    }

    public void SetupData()
    {
        GameContext pGameContext = GameContext.getCtx();

        UpdateLanguage();
        
        m_bPushList[(int)E_PUSH.Game] = pGameContext.GetPush();
        m_bPushList[(int)E_PUSH.Local] = pGameContext.GetLocalPush();
        m_bPushList[(int)E_PUSH.Night] = pGameContext.GetNightPush();

        m_pTogleButtonList[(int)E_PUSH.Game].Find("on").gameObject.SetActive(m_bPushList[(int)E_PUSH.Game]);
        m_pTogleButtonList[(int)E_PUSH.Game].Find("off").gameObject.SetActive(!m_bPushList[(int)E_PUSH.Game]);
        m_pTogleButtonList[(int)E_PUSH.Local].Find("on").gameObject.SetActive(m_bPushList[(int)E_PUSH.Local]);
        m_pTogleButtonList[(int)E_PUSH.Local].Find("off").gameObject.SetActive(!m_bPushList[(int)E_PUSH.Local]);
        m_pTogleButtonList[(int)E_PUSH.Night].Find("on").gameObject.SetActive(m_bPushList[(int)E_PUSH.Night]);
        m_pTogleButtonList[(int)E_PUSH.Night].Find("off").gameObject.SetActive(!m_bPushList[(int)E_PUSH.Night]);

        m_pTogleButtonList[3].Find("on").gameObject.SetActive(!pGameContext.GetMuteBgm());
        m_pTogleButtonList[3].Find("off").gameObject.SetActive(pGameContext.GetMuteBgm());
        m_pTogleButtonList[4].Find("on").gameObject.SetActive(!pGameContext.GetMuteSfx());
        m_pTogleButtonList[4].Find("off").gameObject.SetActive(pGameContext.GetMuteSfx());

        if(m_bPushList[(int)E_PUSH.Game])
        {
            ALFUtils.FadeObject(m_pTogleButtonList[(int)E_PUSH.Night],1);
        }
        else
        {
            ALFUtils.FadeObject(m_pTogleButtonList[(int)E_PUSH.Night],-0.7f);
        }
        
        UpdateSNS();

        m_pCustomerText.SetText(pGameContext.GetUserCustomerNo().ToString());
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void SetNightPushInfo(bool bPush)
    {
#if USE_HIVE
        if(Application.isEditor)
        {
            OnRemotePushCB(new hive.ResultAPI(), new hive.RemotePush(m_bPushList[(int)E_PUSH.Game],bPush));
        }
        else
        {
            hive.RemotePush pRemotePush = new hive.RemotePush();
            pRemotePush.isAgreeNight = bPush;
            pRemotePush.isAgreeNotice = m_bPushList[(int)E_PUSH.Game];
            hive.Push.setRemotePush(pRemotePush, OnRemotePushCB);
        }
#else
        m_bPushList[(int)E_PUSH.Night] = bPush;
        GameContext.getCtx().SetNightPush(bPush);
        m_pTogleButtonList[(int)E_PUSH.Night].Find("on").gameObject.SetActive(bPush);
        m_pTogleButtonList[(int)E_PUSH.Night].Find("off").gameObject.SetActive(!bPush);
#endif
    }

    void SetLocalPushInfo(bool bPush)
    {
        m_bPushList[(int)E_PUSH.Local] = bPush;
        GameContext pGameContext = GameContext.getCtx();
        pGameContext.SetLocalPush(bPush);
        
        m_pMainScene.ShowToastMessage(string.Format(pGameContext.GetLocalizingText(bPush ? "ALERT_TXT_NOTIFICATION_ON" :"ALERT_TXT_NOTIFICATION_OFF"),DateTime.Now.ToString("yyyy-MM-dd")),null);
        m_pTogleButtonList[(int)E_PUSH.Local].Find("on").gameObject.SetActive(bPush);
        m_pTogleButtonList[(int)E_PUSH.Local].Find("off").gameObject.SetActive(!bPush);    
    }

    void SetGamePushInfo(bool bPush)
    {
#if USE_HIVE
        if(Application.isEditor)
        {
            OnRemotePushCB(new hive.ResultAPI(), new hive.RemotePush(bPush,!bPush ? false : m_bPushList[(int)E_PUSH.Night]));
        }
        else
        {
            hive.RemotePush pRemotePush = new hive.RemotePush();
            pRemotePush.isAgreeNotice = bPush;
            pRemotePush.isAgreeNight = !bPush ? false : m_bPushList[(int)E_PUSH.Night];

            hive.Push.setRemotePush(pRemotePush, OnRemotePushCB);
        }
#else
        m_bPushList[(int)E_PUSH.Game] = bPush;
        GameContext.getCtx().SetPush(bPush);
        m_pTogleButtonList[(int)E_PUSH.Game].Find("on").gameObject.SetActive(bPush);
        m_pTogleButtonList[(int)E_PUSH.Game].Find("off").gameObject.SetActive(!bPush);
#endif
    }

#if USE_HIVE
    
    void OnRemotePushCB(hive.ResultAPI pResult, hive.RemotePush pRemotePush)
    {
        string pNotice = null;
        GameContext pGameContext = GameContext.getCtx();
        if (pResult.isSuccess())
        {
            if (m_bPushList[(int)E_PUSH.Game] != pRemotePush.isAgreeNotice)
            {
                m_bPushList[(int)E_PUSH.Game] = pRemotePush.isAgreeNotice;

                pNotice = string.Format(pGameContext.GetLocalizingText(pRemotePush.isAgreeNotice ? "ALERT_TXT_NOTIFICATION_ON" : "ALERT_TXT_NOTIFICATION_OFF"),DateTime.Now.ToString("yyyy-MM-dd"));
                pGameContext.SetPush(pRemotePush.isAgreeNotice);
            }

            if (m_bPushList[(int)E_PUSH.Night] != pRemotePush.isAgreeNight)
            {
                m_bPushList[(int)E_PUSH.Night] = pRemotePush.isAgreeNight;
                pNotice = string.Format(pGameContext.GetLocalizingText(pRemotePush.isAgreeNight ? "ALERT_TXT_NOTIFICATION_ON" : "ALERT_TXT_NOTIFICATION_OFF"),DateTime.Now.ToString("yyyy-MM-dd"));
                pGameContext.SetNightPush(pRemotePush.isAgreeNight);
            }
        }
        else
        {
            m_bPushList[(int)E_PUSH.Game] = pRemotePush.isAgreeNotice;
            m_bPushList[(int)E_PUSH.Night] = pRemotePush.isAgreeNight;
            pNotice = string.Format(pGameContext.GetLocalizingText("ALERT_TXT_NOTIFICATION_OFF"),DateTime.Now.ToString("yyyy-MM-dd"));
        }

        m_pTogleButtonList[(int)E_PUSH.Game].Find("on").gameObject.SetActive(pRemotePush.isAgreeNotice);
        m_pTogleButtonList[(int)E_PUSH.Game].Find("off").gameObject.SetActive(!pRemotePush.isAgreeNotice);

        m_pTogleButtonList[(int)E_PUSH.Night].Find("on").gameObject.SetActive(pRemotePush.isAgreeNight);
        m_pTogleButtonList[(int)E_PUSH.Night].Find("off").gameObject.SetActive(!pRemotePush.isAgreeNight);

        if(pRemotePush.isAgreeNotice)
        {
            ALFUtils.FadeObject(m_pTogleButtonList[(int)E_PUSH.Night],1);
        }
        else
        {
            ALFUtils.FadeObject(m_pTogleButtonList[(int)E_PUSH.Night],-0.7f);
        }

        m_pMainScene.ShowToastMessage(pNotice,null);
    }
#endif

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        GameContext pGameContext = GameContext.getCtx();
        switch(sender.name)
        {
            case "back":
            {
                Close();
            }
            break;
            case "lang":
            {
                m_pMainScene.ShowSelectLanguagePopup();
            }
            break;
            case "music":
            {
                bool bOn = !pGameContext.GetMuteBgm();
                pGameContext.SetMuteBgm(bOn); 
                m_pTogleButtonList[3].Find("on").gameObject.SetActive(!bOn);
                m_pTogleButtonList[3].Find("off").gameObject.SetActive(bOn);
            }
            break;
            case "sound":
            {
                bool bOn = !pGameContext.GetMuteSfx();
                pGameContext.SetMuteSfx(bOn); 
                m_pTogleButtonList[4].Find("on").gameObject.SetActive(!bOn);
                m_pTogleButtonList[4].Find("off").gameObject.SetActive(bOn);
            }
            break;
            case "push":
            {
                SetGamePushInfo(!pGameContext.GetPush());
            }
            break;
            case "localPush":
            {
                SetLocalPushInfo(!pGameContext.GetLocalPush());
            }
            break;
            case "night":
            {
                if(pGameContext.GetPush())
                {
                    SetNightPushInfo(!pGameContext.GetNightPush());
                }
            }
            break;
            case "account":
            {
                m_pMainScene.ShowSelectLoginPopup();    
            }
            break;
            case "copy":
            {
                GUIUtility.systemCopyBuffer = m_pCustomerText.text;
                m_pMainScene.ShowToastMessage( pGameContext.GetLocalizingText("ALERT_TXT_COPY_CUSTOMER_NO"),null);
            }
            break;
            case "coupon":
            {   
                if(Application.isEditor)
                {
                    m_pMainScene.ShowWebDlgPopup(pGameContext.GetLocalizingText("SETTINGS_TXT_TERMS_OF_SERVICE"),"https://coupon.withhive.com/934");
                }
                else
                {
#if FTM_LIVE
                    pGameContext.ShowHiveCustomView(pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.HIVE_COUPON).ToString(),true,null);
#else
                    pGameContext.ShowHiveCustomView("300116",true,null);
#endif
                    
                }
            }
            break;
            case "support":
            {
#if USE_HIVE
                NetworkManager.ShowWaitMark(true);
                hive.AuthV4.showInquiry(op=> {
                    NetworkManager.ShowWaitMark(false);
                    if(op.isSuccess())
                    {
                        if(Application.isEditor)
                        {
                            m_pMainScene.ShowWebDlgPopup(pGameContext.GetLocalizingText("SETTINGS_TXT_TERMS_OF_SERVICE"),"https://sandbox-customer-m.withhive.com/faq/934");
                            return;
                        }
                    }
                });
#endif
            }
            break;
            case "terms":
            {
#if USE_HIVE
                NetworkManager.ShowWaitMark(true);
                hive.AuthV4.showTerms(op=> {
                    NetworkManager.ShowWaitMark(false);
                    if(op.isSuccess())
                    {
                        if(Application.isEditor)
                        {
                            m_pMainScene.ShowWebDlgPopup(pGameContext.GetLocalizingText("SETTINGS_TXT_TERMS_OF_SERVICE"),"http://terms.withhive.com/terms/policy/view/M3");
                            return;
                        }
                    }
                });
#endif
            }
            break;
        }

        // m_pMainScene.ShowMessagePopup("미구현!!",GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
    }
}
