using UnityEngine;
using UnityEngine.UI;
using ALF;
using System.Collections.Generic;
using ALF.NETWORK;
using ALF.LAYOUT;
using ALF.SOUND;
using USERDATA;
using DATA;
using LADDERSTANDINGREWARD;

public class RankingReward : ITimer
{
    const string SCROLL_ITEM_NAME = "RankingRewardItem";
    
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;

    float m_fTime = 0;
    float m_fUpdateTime = 0;
    TMPro.TMP_Text m_pSeasonExpireText = null;
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}

    List<RewardRankingItem> m_pRewardRankingItems = new List<RewardRankingItem>();

    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;
    int m_iDataCount = 0;

    public RankingReward(){}
    
    public void Dispose()
    {
        if(m_pScrollRect != null)
        {
            ClearScroll();
            m_pScrollRect.onValueChanged.RemoveAllListeners();
        }
        m_pScrollRect = null;
        m_pRewardRankingItems = null;
        m_pMainScene = null;
        MainUI = null;   
        m_pSeasonExpireText = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "RankingReward : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "RankingReward : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;   
        m_pSeasonExpireText = MainUI.Find("root/info/time/text").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        SetupScroll();
        
        MainUI.gameObject.SetActive(false);
    }

    public void DoExpire(int index)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            GameContext pGameContext = GameContext.getCtx();
            m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_SEASON_ALEADY_ENDED"),null);
            Enable =false;
            SingleFunc.HideAnimationDailog(MainUI);
        }
    }

    public void SetupExpire()
    {
        ResetScroll();
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.GetExpireTimerByUI(this,0) <= -1)
        {
            float fTime = pGameContext.GetCurrentSeasonExpireRemainTime();
            pGameContext.AddExpireTimer(this,0,fTime);
            SingleFunc.UpdateTimeText((int)fTime,m_pSeasonExpireText,0);
        }
    }

    public void UpdateTimer(float dt)
    {
        GameContext pGameContext = GameContext.getCtx();
        float tic = pGameContext.GetExpireTimerByUI(this,0);
        if(tic <= 86400)
        {
            SingleFunc.UpdateTimeText((int)tic,m_pSeasonExpireText,0);
        }
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        RewardRankingItem pItem = null;
        if(index > iTarget)
        {
            pItem = m_pRewardRankingItems[iTarget];
            m_pRewardRankingItems[iTarget] = m_pRewardRankingItems[index];
            i = iTarget +1;
            RewardRankingItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pRewardRankingItems[i];
                m_pRewardRankingItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pRewardRankingItems[index] = pItem;
            pItem = m_pRewardRankingItems[iTarget];
        }
        else
        {
            pItem = m_pRewardRankingItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pRewardRankingItems[i -1] = m_pRewardRankingItems[i];
                ++i;
            }

            m_pRewardRankingItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex + iTarget + iCount;
        
        if(i < 0 || m_iDataCount <= i) return;

        pItem.UpdateInfo(i,GameContext.LADDER_ID);
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_iDataCount <= 0) return;

        if(m_iTotalScrollItems < m_iDataCount && value.y != m_pPrevDir.y)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }

    public void Close()
    {
        Enable =false;
        GameContext.getCtx().RemoveExpireTimerByUI(this);
        SingleFunc.HideAnimationDailog(MainUI);
    } 

    void SetupScroll()
    {
        GameContext pGameContext = GameContext.getCtx();
        LadderStandingRewardList pLadderStandingRewardList = pGameContext.GetFlatBufferData<LadderStandingRewardList>(E_DATA_TYPE.LadderStandingReward);
        m_iDataCount = pLadderStandingRewardList.LadderStandingRewardLength;
        Vector2 size;
        float h = 0;
        RectTransform pItem = null;
        
        m_iTotalScrollItems = 0;
        m_iStartIndex = 0;

        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        
        while(viewSize > -itemSize)
        {
            if(viewSize > 0)
            {
                ++m_iTotalScrollItems;
            }
            
            pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
                
            if(pItem)
            {
                pItem.SetParent(m_pScrollRect.content,false);
                pItem.anchoredPosition = new Vector2(0,-h);
                pItem.localScale = Vector3.one;
                size = pItem.sizeDelta;
                size.x = 0;
                pItem.sizeDelta = size;
                itemSize = pItem.rect.height;
                h += itemSize;
                viewSize -= itemSize;
                m_pRewardRankingItems.Add(new RewardRankingItem(pItem));
            }
        }

        size = m_pScrollRect.content.sizeDelta;
        size.y = m_iDataCount * itemSize;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.content.anchoredPosition = Vector2.zero; 
    }
    void ClearScroll()
    {
        int i = m_pRewardRankingItems.Count;
        while(i > 0)
        {
            --i;
            m_pRewardRankingItems[i].Dispose(); 
        }
        
        m_pRewardRankingItems.Clear();

        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }

    void ResetScroll()
    {
        Vector2 pos;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        RewardRankingItem pItem = null;
        for(int i = 0; i < m_pRewardRankingItems.Count; ++i)
        {
            pItem = m_pRewardRankingItems[i];
            itemSize = pItem.Target.rect.height;
            viewSize -= itemSize;
            pItem.Target.gameObject.SetActive(viewSize > -itemSize);

            pos = pItem.Target.anchoredPosition;            
            pos.y = -i * itemSize;
            pItem.Target.anchoredPosition = pos;
            pItem.UpdateInfo(i,GameContext.LADDER_ID);
        }

        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
        m_pPrevDir.y = 1;
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }
    
}
