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
public class SeasonRankingReward : IBaseUI
{
    const string REWARD_ITEM_NAME = "RewardItem";
    MainScene m_pMainScene = null;
    TMPro.TMP_Text m_pCurrentRanking = null;
    TMPro.TMP_Text m_pRanking = null;
    TMPro.TMP_Text m_pTrophy = null;
    RawImage m_pRankIcon = null;
    Transform m_pRewards = null;

    public RectTransform MainUI { get; private set;}
    public SeasonRankingReward(){}
    
    public void Dispose()
    {
        MainUI = null;
        m_pRanking = null;
        m_pCurrentRanking = null;
        m_pTrophy = null;
        
        ClreaReward();
        SingleFunc.ClearRankIcon(m_pRankIcon);
        m_pRankIcon = null;
        m_pRewards = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "SeasonRankingReward : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "SeasonRankingReward : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pRanking = MainUI.Find("root/info/ranking/text").GetComponent<TMPro.TMP_Text>();
        m_pTrophy = MainUI.Find("root/info/trophy/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentRanking = MainUI.Find("root/text").GetComponent<TMPro.TMP_Text>();
        m_pRankIcon = MainUI.Find("root/info/rank").GetComponent<RawImage>();
        m_pRewards = MainUI.Find("root/info/rewards");
        
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    void ClreaReward()
    {
        RectTransform item = null;
        int i = m_pRewards.childCount;
        while(i > 0)
        {
            --i;
            item = m_pRewards.GetChild(i).GetComponent<RectTransform>();
            SingleFunc.AddRewardIcon(item,REWARD_ITEM_NAME);
        }
    }

    public void SetupData(JObject data)
    {
        ClreaReward();
        GameContext pGameContext = GameContext.getCtx();
        JArray pArray = (JArray)data["rewards"];
        JObject pItem = null;
        RectTransform pReward = null;
        int i =0;
        float width = 0;
        float w = 0;
        uint itemId = 0;
        Vector2 pos;
        for(i =0; i < pArray.Count; ++i)
        {
            pItem = (JObject)pArray[i];
            itemId = (uint)pItem["no"];
            if(itemId == 0) continue;

            pReward = SingleFunc.GetRewardIcon(m_pRewards,REWARD_ITEM_NAME,itemId,(ulong)pItem["amount"]);
            
            width = pReward.rect.width;
            pos = pReward.anchoredPosition;
            pos.x = w;
            pReward.anchoredPosition = pos;
            w += width + 20;
        }        

        w = (w * -0.5f) + (width * 0.5f);
        
        for(i = 0; i < m_pRewards.childCount; ++i)
        {
            pReward = m_pRewards.GetChild(i).GetComponent<RectTransform>();
            pos = pReward.anchoredPosition;
            pos.x += w;
            pReward.anchoredPosition = pos;
        }
        itemId = (uint)data["prevSeasonNo"];
        m_pCurrentRanking.SetText(string.Format(pGameContext.GetLocalizingText("SEASONREWARD_RANKING_TXT_ENDED_SEASON_RANKING"),itemId / 100,itemId % 100));
        m_pRanking.SetText((string)data["prevSeasonStanding"]);
        
        m_pTrophy.SetText((string)data["prevSeasonTrophy"]);
        i = (int)data["prevSeasonUserRank"];
        SingleFunc.SetupRankIcon(m_pRankIcon,pGameContext.GetCurrentUserRank());
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
