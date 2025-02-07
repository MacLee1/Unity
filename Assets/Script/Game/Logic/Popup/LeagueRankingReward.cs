using UnityEngine;
using UnityEngine.UI;
using ALF;
using System.Collections.Generic;
using ALF.NETWORK;
using ALF.LAYOUT;
using ALF.SOUND;
using USERDATA;
using DATA;
using LEAGUESTANDINGREWARDDATA;

public class LeagueRankingReward : ITimer
{
    const string SCROLL_ITEM_NAME = "RankingRewardItem";
    const string REWARD_ITEM_NAME = "RewardItem";
    MainScene m_pMainScene = null;
    
    ScrollRect m_pScrollRect = null;

    float m_fTime = 0;
    float m_fUpdateTime = 0;
    TMPro.TMP_Text m_pSeasonExpireText = null;
    Button[] m_pTabUIList = new Button[4];
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}

    List<RewardRankingItem> m_pRewardRankingItems = new List<RewardRankingItem>();
    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;
    int m_iDataCount = 0;
    int m_iSelectID = 0;

    public LeagueRankingReward(){}
    
    public void Dispose()
    {
        for(int i =0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i] = null;
        }

        if(m_pScrollRect != null)
        {
            ClearScroll();
            m_pScrollRect.onValueChanged.RemoveAllListeners();
        }
        m_pScrollRect = null;
        m_pRewardRankingItems = null;

        m_pTabUIList = null;
        
        m_pMainScene = null;
        MainUI = null;   
        m_pSeasonExpireText = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "LeagueRankingReward : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "LeagueRankingReward : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;   
        m_pSeasonExpireText = MainUI.Find("root/time/text").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);

        RectTransform item = null;
        RectTransform ui = MainUI.Find("root/tabs").GetComponent<RectTransform>();

        for(int n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            m_pTabUIList[n] = item.GetComponent<Button>();
        }

        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        SetupScroll();
        
        MainUI.gameObject.SetActive(false);
    }

    public void DoExpire(int index)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            GameContext pGameContext = GameContext.getCtx();
            m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("ALERT_TXT_SEASON_ALEADY_ENDED"),null);
            Enable =false;
            SingleFunc.HideAnimationDailog(MainUI);
        }
    }

    public void SetupExpire()
    {
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

        pItem.UpdateInfo(i,m_iSelectID);
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
        size.y = h;
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

    void ResetScroll(int id)
    {
        m_iSelectID = id;
        Vector2 pos;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        GameContext pGameContext = GameContext.getCtx();
        LeagueStandingRewardList pLeagueStandingRewardList = pGameContext.GetFlatBufferData<LeagueStandingRewardList>(E_DATA_TYPE.LeagueStandingRewardData);
        LeagueStandingItem? pLeagueStandingItem = pLeagueStandingRewardList.LeagueStandingRewardByKey((byte)m_iSelectID);
        m_iDataCount = pLeagueStandingItem.Value.ListLength;

        RewardRankingItem pItem = null;
        for(int i = 0; i < m_pRewardRankingItems.Count; ++i)
        {
            pItem = m_pRewardRankingItems[i];
            itemSize = pItem.Target.rect.height;
            if(i < m_iDataCount)
            {
                viewSize -= itemSize;
                pItem.Target.gameObject.SetActive(viewSize > -itemSize);
                pItem.UpdateInfo(i,m_iSelectID);
            }
            else
            {
                pItem.Target.gameObject.SetActive(false);
            }

            pos = pItem.Target.anchoredPosition;            
            pos.y = -i * itemSize;
            pItem.Target.anchoredPosition = pos;
        }

        pos = m_pScrollRect.content.sizeDelta;
        pos.y = itemSize * m_iDataCount;
        m_pScrollRect.content.sizeDelta = pos;
        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
        m_pPrevDir.y = 1;
    }

    public void ShowTabUI(int ID)
    {
        int index = ID - GameContext.LEAGUE1_ID; 
        for(int i =0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i].enabled = i != index;
            m_pTabUIList[i].transform.Find("on").gameObject.SetActive(!m_pTabUIList[i].enabled);
            
            ResetScroll(ID);
        }
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "close")
        {
            SingleFunc.HideAnimationDailog(MainUI);
            return;
        }

        int id = 0;
        if(int.TryParse(sender.name,out id))
        {
            ShowTabUI(id);
        }
    }   
}
