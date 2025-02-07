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

public class MailMessage : IBaseUI
{
    const string SCROLL_ITEM_NAME = "MailRewardItem";
    enum E_MAIL_STATUS : byte { Unread=0,Read_Reward=4,Read_NoReward=5,Delete=9}
    MainScene m_pMainScene = null;

    ScrollRect m_pMessage = null;
    TMPro.TMP_Text m_pMessageText = null;
    TMPro.TMP_Text m_pMessageTitle = null;
    
    ScrollRect m_pRewardScrollRect = null;

    MailInfoT m_pMailInfo = null;

    public RectTransform MainUI { get; private set;}

    public bool Enable { set{ if (m_pRewardScrollRect != null) m_pRewardScrollRect.enabled = value; if (m_pMessage != null) m_pMessage.enabled = value;}}

    public MailMessage(){}
    
    public void Dispose()
    {
        if(m_pRewardScrollRect != null)
        {
            ClearRewardScroll();
        }
        m_pMessage = null;
        m_pMessageText = null;
        m_pMessageTitle = null;
        m_pRewardScrollRect = null;
        m_pMainScene = null;
        MainUI = null;
        m_pMailInfo = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "MailMessage : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "MailMessage : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_pMessage = MainUI.Find("root/info/scroll").GetComponent<ScrollRect>();
        m_pMessageText = m_pMessage.content.GetComponent<TMPro.TMP_Text>();
        m_pMessageTitle = MainUI.Find("root/title").GetComponent<TMPro.TMP_Text>();
        m_pRewardScrollRect = MainUI.Find("root/info/rewards").GetComponent<ScrollRect>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupData(ulong id,byte type)
    {
        ClearRewardScroll();
        GameContext pGameContext = GameContext.getCtx();
        m_pMailInfo = pGameContext.GetMailInfoById(id,type);
        if(m_pMailInfo == null) return;
        
        m_pMessageTitle.SetText(pGameContext.GetLocalizingText(m_pMailInfo.Title));
        string content = pGameContext.GetLocalizingText(m_pMailInfo.Content);

        if(!string.IsNullOrEmpty(m_pMailInfo.Data))
        {
            try
            {
                JObject data = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(m_pMailInfo.Data, new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
                if(data != null && data.Count > 0)
                {
                    object[] list = new object[data.Count];
                    if(m_pMailInfo.Content == "MAIL_SEASON_TXT_LEAGUE_STANDING_REWARD")
                    {
                        if(data.ContainsKey("0") && data.ContainsKey("1") && data.ContainsKey("2"))
                        {
                            list[0] = (string)data["0"];
                            int iLeagueID = (int)data["1"];
                            if(iLeagueID == GameContext.LEAGUE1_ID)
                            {
                                list[1] = pGameContext.GetLocalizingText("LEAGUE_TITLE_CONFERENCE_2");
                            }
                            else if(iLeagueID == GameContext.LEAGUE2_ID)
                            {
                                list[1] = pGameContext.GetLocalizingText("LEAGUE_TITLE_CONFERENCE_1");
                            }
                            else if(iLeagueID == GameContext.LEAGUE3_ID)
                            {
                                list[1] = pGameContext.GetLocalizingText("LEAGUE_TITLE_CHAMPION");
                            }
                            else if(iLeagueID == GameContext.LEAGUE4_ID)
                            {
                                list[1] = pGameContext.GetLocalizingText("LEAGUE_TITLE_ULTIMATE_CHAMPION");
                            }
                            else
                            {
                                list[1] = pGameContext.GetLocalizingText("CHALLENGESTAGE_TITLE_CHALLENGE_STAGE");
                            }

                            list[2] = (string)data["2"];
                        }
                    }
                    else
                    {
                        for(int i = 0; i < data.Count; ++i)
                        {
                            if(data.ContainsKey(i.ToString()))
                            {
                                list[i] = (string)data[i.ToString()];
                            }
                        }
                    }

                    content = string.Format(content,list);
                }
            }
            catch
            {
                Debug.Log($"-------------------:{m_pMailInfo.Data}");
            }
        }

        m_pMessageText.SetText(content);

        Vector2 size = m_pMessage.content.sizeDelta;
        size.y = m_pMessageText.preferredHeight;
        m_pMessage.content.sizeDelta = size;
        m_pMessage.verticalNormalizedPosition =0;
        RectTransform tm = m_pMessage.GetComponent<RectTransform>();
        Vector2 offsetMin = tm.offsetMin;
        
        if(m_pMailInfo.Rewards.Count > 0)
        {
            offsetMin.y = m_pRewardScrollRect.viewport.rect.height;
            SetupRewardScroll();
        }
        else
        {
            offsetMin.y = 0;
        }

        tm.offsetMin = offsetMin;
    }

    void SetupRewardScroll()
    {
        RectTransform pItem = null;
        Vector2 size;
        float w = 0;
        int i =0;
        E_MAIL_STATUS eStatus = eStatus = (E_MAIL_STATUS)m_pMailInfo.Status;
        uint itemId = 0;
        float width = 0;
        RawImage icon = null;
        for(i =0; i < m_pMailInfo.Rewards.Count; ++i)
        {
            itemId = m_pMailInfo.Rewards[i] == GameContext.FREE_CASH_ID ? GameContext.CASH_ID : m_pMailInfo.Rewards[i];
            pItem = SingleFunc.GetRewardIcon(m_pRewardScrollRect.content,SCROLL_ITEM_NAME,itemId,m_pMailInfo.Amounts[i]);
    
            if(pItem)
            {
                icon = pItem.Find("icon").GetComponent<RawImage>();
                icon.color = eStatus > E_MAIL_STATUS.Read_Reward ? Color.gray : Color.white;

                pItem.anchoredPosition = new Vector2(w,0);
                size = pItem.sizeDelta;
                width = pItem.rect.width;
                w += size.x + 20;
            }
        }

        if(m_pRewardScrollRect.viewport.rect.width > w)
        {
            if(m_pRewardScrollRect.content.childCount > 1)
            {
                w = width;

                if(m_pRewardScrollRect.content.childCount %2 == 0)
                {
                    w -= width* 0.5f;
                }
            
                for(i =0; i < m_pRewardScrollRect.content.childCount; ++i)
                {
                    pItem = m_pRewardScrollRect.content.GetChild(i).GetComponent<RectTransform>();
                    size = pItem.anchoredPosition;
                    size.x -= w;
                    pItem.anchoredPosition = size;
                }    
            }

            size = m_pRewardScrollRect.content.sizeDelta;
            size.x = m_pRewardScrollRect.viewport.rect.width;
            m_pRewardScrollRect.content.sizeDelta = size;
        }
        else
        {
            size = m_pRewardScrollRect.content.sizeDelta;
            size.x = w;
            m_pRewardScrollRect.content.sizeDelta = size;
        }

        m_pRewardScrollRect.content.anchoredPosition = Vector2.zero;
    }

    void ClearRewardScroll()
    {
        int i = m_pRewardScrollRect.content.childCount;
        RectTransform pItem = null;
        Vector2 anchor = new Vector2(0.5f,0.5f);
        while(i > 0)
        {
            --i;
            pItem = m_pRewardScrollRect.content.GetChild(i).GetComponent<RectTransform>();
            pItem.Find("icon").GetComponent<RawImage>().color = Color.white;
            SingleFunc.AddRewardIcon(pItem,SCROLL_ITEM_NAME);
        }
        m_pRewardScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = m_pRewardScrollRect.content.sizeDelta;
        size.x =0;
        m_pRewardScrollRect.content.sizeDelta = size;
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
