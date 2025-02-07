using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALF;
using System;

// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
// using DATA;
// using UnityEngine.EventSystems;
using STATEDATA;


public class WebDlg : IBaseUI
{
    TMPro.TMP_Text m_pTitleText = null;
    WebViewObject m_pWebViewObject = null;
    RectTransform m_pWeb = null;
    public RectTransform MainUI { get; private set;}
    public WebDlg(){}
    
    public void Dispose()
    {
        if(MainUI.gameObject.activeInHierarchy)
        {
            MainUI.gameObject.SetActive(false);
        }
        MainUI = null;
        m_pWeb = null;
        m_pTitleText = null;
        m_pWebViewObject = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pMainUI != null, "WebDlg : targetUI is null!!");

        MainUI = pMainUI;
        MainUI.gameObject.SetActive(false);
        m_pWeb = MainUI.Find("root/web").GetComponent<RectTransform>();
        m_pTitleText = MainUI.Find("root/title").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void Setup(string title,string url)
    {
        if(m_pWebViewObject != null)
        {
            UnityEngine.GameObject.Destroy(m_pWebViewObject);
        }
        m_pTitleText.SetText(title);
        // string url = "http://pasta.service.s3.amazonaws.com/PrivacyPolicy_eng.htm";
        m_pWebViewObject = m_pWeb.gameObject.AddComponent<WebViewObject>();
        m_pWebViewObject.Init(
                // cb: (msg) =>
                // {
                //     // Debug.Log(string.Format("CallFromJS[{0}]", msg));
                // },
                // err: (msg) =>
                // {
                //     // Debug.Log(string.Format("CallOnError[{0}]", msg));
                // },
                // httpErr: (msg) =>
                // {
                //     Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
                // },
                // started: (msg) =>
                // {
                //     Debug.Log(string.Format("CallOnStarted[{0}]", msg));
                // },
                // hooked: (msg) =>
                // {
                //     Debug.Log(string.Format("CallOnHooked[{0}]", msg));
                // },
                ld: (msg) =>
                {
                    m_pWebViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
    #if UNITY_IPHONE || UNITY_IOS
                },
                enableWKWebView :true
    #else
                }
    #endif
            );

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        m_pWebViewObject.bitmapRefreshCycle = 1;
#endif
        Vector3 pos = m_pWeb.TransformPoint(new Vector3(m_pWeb.rect.width,0,0));
        Vector3 pos1 = m_pWeb.TransformPoint(new Vector3(m_pWeb.rect.width,m_pWeb.rect.height * -1,0));
        m_pWebViewObject.SetMargins((int)m_pWeb.transform.position.x, (int)(Screen.height - m_pWeb.transform.position.y),(int)(Screen.width - pos.x),(int)pos1.y);
    
        m_pWebViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
        m_pWebViewObject.SetVisibility(true);

        if (url.StartsWith("http")) 
        {
            m_pWebViewObject.LoadURL(url.Replace(" ", "%20"));
        }                     
    }

    public void Close()
    {
        if(m_pWebViewObject != null)
        {
            UnityEngine.GameObject.Destroy(m_pWebViewObject);
        }
        m_pWebViewObject = null;
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
    
}
