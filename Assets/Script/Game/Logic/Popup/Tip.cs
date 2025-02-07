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

public class Tip : IBaseUI
{
    MainScene m_pMainScene = null;
    ScrollRect m_pMessage = null;
    TMPro.TMP_Text m_pMessageText = null;
    TMPro.TMP_Text m_pMessageTitle = null;
    

    public RectTransform MainUI { get; private set;}

    public bool Enable { set{ if (m_pMessage != null) m_pMessage.enabled = value;}}

    public Tip(){}
    
    public void Dispose()
    {
        m_pMessage = null;
        m_pMessageText = null;
        m_pMessageTitle = null;
        m_pMainScene = null;
        MainUI = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Tip : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Tip : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_pMessage = MainUI.Find("root/bg/scroll").GetComponent<ScrollRect>();
        m_pMessageText = m_pMessage.content.GetComponent<TMPro.TMP_Text>();
        m_pMessageTitle = MainUI.Find("root/bg/title").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupData(string id)
    {
        GameContext pGameContext = GameContext.getCtx();
        GameTipList pGameTipList = pGameContext.GetFlatBufferData<GameTipList>(E_DATA_TYPE.GameTip);
        GameTipItem? pGameTipItem = pGameTipList.GameTipByKey(id);
        if(pGameTipItem != null)
        {
            m_pMessageTitle.SetText(pGameContext.GetLocalizingText(pGameTipItem.Value.Title));
            TipGroupItem? pTipGroupItem = null;
            TipItem? pTipItem = null;
            string str = "";
            for(int i =0; i < pGameTipItem.Value.ListLength; ++i)
            {
                pTipGroupItem = pGameTipItem.Value.List(i);
                str = string.Format("{0}<size=35>{1}\n\n</size>",str,pGameContext.GetLocalizingText(pTipGroupItem.Value.SubTitle));

                for(int n =0; n < pTipGroupItem.Value.ListLength; ++n)
                {
                    pTipItem = pTipGroupItem.Value.List(n);
                    str = string.Format("{0}<size=30>{1}\n</size>",str,pGameContext.GetLocalizingText(pTipItem.Value.Text));
                }

                str = string.Format("{0}\n",str);
            }

            m_pMessageText.SetText(str);
        }

        Vector2 size = m_pMessage.content.sizeDelta;
        size.y = m_pMessageText.preferredHeight;
        m_pMessage.content.sizeDelta = size;
        m_pMessage.verticalNormalizedPosition = 1;
    }


    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
}
