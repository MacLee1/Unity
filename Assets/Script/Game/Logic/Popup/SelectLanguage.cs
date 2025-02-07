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

public class SelectLanguage : IBaseUI
{
    MainScene m_pMainScene = null;
    Transform[] m_pTabButtonList = null;
    public RectTransform MainUI { get; private set;}

    // public bool Enable { set{ if (m_pMessage != null) m_pMessage.enabled = value;}}

    public SelectLanguage(){}
    
    public void Dispose()
    {
        int i =0;
        
        for(i =0; i < m_pTabButtonList.Length; ++i)
        {
            m_pTabButtonList[i] = null;
        }
        m_pTabButtonList = null;
        m_pMainScene = null;
        MainUI = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "SelectLanguage : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "SelectLanguage : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        RectTransform item = null;
        GameContext pGameContext = GameContext.getCtx();
        RectTransform ui = MainUI.Find("root/bg/lang").GetComponent<RectTransform>();
        m_pTabButtonList = new Transform[ui.childCount +1];
        int n =0;
        int iTabIndex = 0;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_DATA_TYPE)Enum.Parse(typeof(E_DATA_TYPE), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;
        }
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupData()
    {
        E_DATA_TYPE eType = GameContext.getCtx().GetCurrentLangauge();
        int i = m_pTabButtonList.Length;
        while(i > 1)
        {
            --i;
            m_pTabButtonList[i].Find("on").gameObject.SetActive(i == (int)eType);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = i == (int)eType ? Color.white : GameContext.GRAY;
            if(AFPool.GetItem<TextAsset>("Bin",((E_DATA_TYPE)i).ToString()) == null)
            {
                m_pTabButtonList[i].gameObject.SetActive(false);
            }
        }
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "close")
        {
            Close();
            return;
        }

        E_DATA_TYPE eType = E_DATA_TYPE.en_US;
        if(!Enum.TryParse(sender.name,out eType))
        {
            eType = E_DATA_TYPE.en_US;
        }

        GameContext pGameContext = GameContext.getCtx();
        if(eType != pGameContext.GetCurrentLangauge())
        {
            pGameContext.SetCurrentLangauge((byte)eType);
            pGameContext.SaveUserData(true);
            m_pMainScene.RunIntroSecne(E_LOGIN_TYPE.direct);
        }
    }
}
