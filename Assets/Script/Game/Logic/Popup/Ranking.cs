using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using USERDATA;
using ALF.NETWORK;
// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
using Newtonsoft.Json.Linq;
using ALF.SOUND;
using DATA;
// using UnityEngine.EventSystems;
using STATEDATA;


public class Ranking : ITimer
{
    readonly string[] ScrollItemNames = new string[] { "RankingClubItem","RankingPlayerItem"};
    enum E_TAB : byte { clubs = 0, players,MAX};
    enum E_SUB_TAB : byte { goals = 1, assists =2,prossessionWon=10,saves = 13, MAX = 4}

    
    private class CacheRankingData : IBase
    {
        public uint SeasonNo = 0;
        public float ExpireTime = -1;
        public byte Type = 0;
        public JObject Data = null;
        
        public CacheRankingData(uint no, byte type, float expire, JObject data)
        {
            SeasonNo = no;
            ExpireTime = expire;
            Type = type;
            Data = data;
        }

        public void Dispose()
        {
            Data = null;
        }
    }

    class RankingClubItem : IBase
    {
        public ulong Id {get; private set;}

        public TMPro.TMP_Text ClubName {get; private set;}
        public TMPro.TMP_Text NoText {get; private set;}
        public TMPro.TMP_Text TrophyText {get; private set;}
        public TMPro.TMP_Text WinText {get; private set;}
        public TMPro.TMP_Text OverallText {get; private set;}
        public Graphic BG {get; private set;}
        
        public GameObject[] Ranking {get; private set;}

        string m_pItemName = null;

        public RectTransform Target  {get; private set;}
        
        Button m_pButton = null;

        public RankingClubItem(RectTransform target,string pItemName)
        {
            m_pItemName = pItemName;
            Target = target;
            m_pButton = target.GetComponent<Button>();
            
            ClubName = target.Find("club").GetComponent<TMPro.TMP_Text>();
            NoText = target.Find("ranking/text").GetComponent<TMPro.TMP_Text>();
            Ranking = new GameObject[3];
            Transform tm = target.Find("ranking");
            Ranking[0]= tm.Find("1").gameObject;
            Ranking[1]= tm.Find("2").gameObject;
            Ranking[2]= tm.Find("3").gameObject;
            BG =  target.Find("bg").GetComponent<Graphic>();
            TrophyText = target.Find("trophy/text").GetComponent<TMPro.TMP_Text>();
            WinText = target.Find("win").GetComponent<TMPro.TMP_Text>();
            OverallText = target.Find("overall").GetComponent<TMPro.TMP_Text>();
        }

        public void Dispose()
        {
            for(int i =0; i < Ranking.Length; ++i)
            {
                Ranking[i] = null;
            }

            m_pButton.onClick.RemoveAllListeners();
            m_pButton = null;
            
            ClubName = null;
            NoText = null;
            Ranking = null;
            BG = null;
            TrophyText = null;
            WinText = null;
            OverallText = null;

            LayoutManager.Instance.AddItem(m_pItemName,Target);
            Target = null;
        }

        public void UpdateData(JObject pObject,int count)
        {
            if(pObject == null) return;
            
            Id = (ulong)pObject["club"];
            
            BG.color = GameContext.getCtx().GetClubID() == Id ? new Color(0.6392157f,0.8431373f,1) : Color.white;
            for(int i =0; i < Ranking.Length; ++i)
            {
                Ranking[i].SetActive(count == i);
            }
            NoText.SetText((count+1).ToString());
            NoText.gameObject.SetActive(count > 2);
            
            TrophyText.SetText((string)pObject["trophy"]);
            WinText.SetText((string)pObject["win"]);
            OverallText.SetText((string)pObject["squadPower"]);
            ClubName.SetText((string)pObject["name"]);
        }
    }

    // const uint START_SEASON_NO = 202208;
    
    E_TAB m_eCurrentTab = E_TAB.MAX;
    uint m_iSeasonNoForClub = 0;
    uint m_iSeasonNoForPlayer = 0;
    E_SUB_TAB m_eCurrentSubTab = E_SUB_TAB.MAX;
    ulong m_ulSendNo = 0;
    MainScene m_pMainScene = null;
    ScrollRect[] m_pScrollRects = new ScrollRect[(int)E_TAB.MAX];
    Transform[] m_pTabButtonList = new Transform[(int)E_TAB.MAX];
    RectTransform[] m_pTabUIList = new RectTransform[(int)E_TAB.MAX];
    RectTransform[] m_pSubTabUIList = new RectTransform[14];
    RectTransform[] m_pSubHeaderUIList = new RectTransform[14];
    RectTransform m_pReward = null;
    RectTransform m_pAgo = null;
    TMPro.TMP_Text m_pSeasonTitle = null;

    List<CacheRankingData> m_pCacheRankingDatas = new List<CacheRankingData>();
    
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRects != null) m_pScrollRects[0].enabled = value; m_pScrollRects[1].enabled = value;}}

    JArray m_pRankingClubData = null;
    JArray m_pRankingPlayerData = null;

    List<RankingClubItem> m_pRankingClubItems = new List<RankingClubItem>();
    List<RankingPlayerItem> m_pRankingPlayerItems = new List<RankingPlayerItem>();

    Vector2 m_pPrevDir = Vector2.zero;
    int[] m_iTotalScrollItems = new int[(int)E_TAB.MAX];
    int m_iStartIndex = 0;


    public Ranking(){}
    
    public void Dispose()
    {
        m_pRankingClubData = null;
        m_pRankingPlayerData = null;
        m_pMainScene = null;
        MainUI = null;
        m_pReward = null;
        m_pAgo = null;
        m_pSeasonTitle = null;
        int i =0;
        for(i =0; i < m_pScrollRects.Length; ++i)
        {
            ClearScroll((E_TAB)i);
            m_pScrollRects[i].onValueChanged.RemoveAllListeners();
            m_pScrollRects[i] = null;
        }
        m_pScrollRects = null;

        for(i =0; i < m_pTabButtonList.Length; ++i)
        {
            m_pTabButtonList[i] = null;
            m_pTabUIList[i] = null;
        }

        for(i =0; i < m_pCacheRankingDatas.Count; ++i)
        {
            m_pCacheRankingDatas[i]?.Dispose();            
            m_pCacheRankingDatas[i] = null;
        }
        m_pCacheRankingDatas.Clear();
        for(i =0; i < m_pSubTabUIList.Length; ++i)
        {
            m_pSubTabUIList[i] = null;
            m_pSubHeaderUIList[i] = null;
        }
        m_pSubHeaderUIList = null;
        m_pSubTabUIList = null;
        m_pTabButtonList = null;
        m_pTabUIList = null;
        m_pCacheRankingDatas = null;
        m_pRankingClubItems = null;
        m_pRankingPlayerItems = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Ranking : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Ranking : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
    
        float w = 0;
        float wh = 0;
        
        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        float ax = 0;
        int iTabIndex = -1;
        RectTransform ui = MainUI.Find("root/tabs").GetComponent<RectTransform>();
        w = (ui.rect.width / ui.childCount);
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        int n =0;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabUIList[iTabIndex] = MainUI.Find($"root/{item.gameObject.name}").GetComponent<RectTransform>();
            m_pScrollRects[iTabIndex] = m_pTabUIList[iTabIndex].GetComponentInChildren<ScrollRect>(true);
            m_pScrollRects[iTabIndex].onValueChanged.AddListener( ScrollViewChangeValueEventCall);

            m_pTabButtonList[iTabIndex] = item;

            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }

        m_pReward = MainUI.Find("root/reward").GetComponent<RectTransform>();
        m_pAgo = MainUI.Find("root/ago").GetComponent<RectTransform>();

        GameObject go = m_pAgo.Find("ago").gameObject;
        go.SetActive(true);
        go = m_pAgo.Find("current").gameObject;
        go.SetActive(false);

        m_pSeasonTitle = MainUI.Find("root/text").GetComponent<TMPro.TMP_Text>();

        ui = m_pTabUIList[(int)E_TAB.players].Find("tabs").GetComponent<RectTransform>();
        w = (ui.rect.width / ui.childCount);
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        n =0;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_SUB_TAB)Enum.Parse(typeof(E_SUB_TAB), item.gameObject.name));
            m_pSubHeaderUIList[iTabIndex] = m_pTabUIList[(int)E_TAB.players].Find($"header/{item.gameObject.name}").GetComponent<RectTransform>();
            m_pSubTabUIList[iTabIndex] = item;

            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }

        MainUI.gameObject.SetActive(false);
        
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        SetupScroll(E_TAB.clubs);
        SetupScroll(E_TAB.players);
    }

    public void DoExpire(int index)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            if(!m_pMainScene.IsShowInstance<RankingReward>())
            {
                GameContext pGameContext = GameContext.getCtx();        
                m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_SEASON_ALEADY_ENDED"),null);
            }
        
            Close();
        }
    }
    
    void ShowTabUI(E_TAB eTab)
    {
        int i = 0;
        int index = (int)eTab;
        for(i = 0; i < m_pTabButtonList.Length; ++i)
        {
            m_pTabUIList[i].gameObject.SetActive(index == i);
            m_pTabButtonList[i].Find("on").gameObject.SetActive(index == i);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = index == i ? Color.white : GameContext.GRAY;
        }
        GameContext pGameContext = GameContext.getCtx();
        string strSeasonNo = null;
        uint currentNo = pGameContext.GetCurrentSeasonNo();
        if(eTab == E_TAB.players)
        {
            strSeasonNo = m_iSeasonNoForPlayer.ToString();
            ShowSubTabUI(m_eCurrentSubTab);

            if(m_iSeasonNoForPlayer < currentNo)
            {
                m_pAgo.gameObject.SetActive(true);
                ShowRankingInfo(true);
            }
            else if(m_iSeasonNoForPlayer == currentNo)
            {
                m_pAgo.gameObject.SetActive(pGameContext.GetPrevSeasonNo() < currentNo);
                ShowRankingInfo(false);
            }
            else
            {
                ALFUtils.Assert(false,"m_iSeasonNoForPlayer > currentNo !!!");
            }
            
            m_pReward.gameObject.SetActive(false);
        }
        else
        {
            strSeasonNo = m_iSeasonNoForClub.ToString();
            if(m_iSeasonNoForClub < currentNo)
            {
                m_pReward.gameObject.SetActive(false);
                m_pAgo.gameObject.SetActive(true);
                ShowRankingInfo(true);
            }
            else if(m_iSeasonNoForClub == currentNo)
            {
                m_pReward.gameObject.SetActive(true);
                m_pAgo.gameObject.SetActive(pGameContext.GetPrevSeasonNo() < currentNo);
                ShowRankingInfo(false);
            }
            else
            {
                ALFUtils.Assert(false,"m_iSeasonNoForClub > currentNo !!!");
            }
        }
        m_pSeasonTitle.SetText(string.Format(pGameContext.GetLocalizingText("RANKING_TXT_SEASON_NAME"), strSeasonNo.Substring(0,4),strSeasonNo[4] != '0'? strSeasonNo.Substring(4,2) : strSeasonNo.Substring(5,1)) );
    }

    void ShowSubTabUI(E_SUB_TAB eTab)
    {
        int i = 0;
        int index = (int)eTab;
        // m_eCurrentSubTab = eTab;
        for(i = 0; i < m_pSubTabUIList.Length; ++i)
        {
            if(m_pSubHeaderUIList[i] != null)
            {
                m_pSubHeaderUIList[i].gameObject.SetActive(index == i);
                m_pSubTabUIList[i].Find("on").gameObject.SetActive(index == i);
                // m_pSubTabUIList[i].Find("title").GetComponent<Graphic>().color = index == i ? Color.white : GameContext.GRAY_W;
            }
        }
    }
    public void SetupExpire()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.GetExpireTimerByUI(this,0) <= -1)
        {
            float fTime = pGameContext.GetCurrentSeasonExpireRemainTime();
            pGameContext.AddExpireTimer(this,0,fTime);
        }
    }

      void SetupScroll(E_TAB eTab)
    {
        ClearScroll(eTab);
        
        string strItem = ScrollItemNames[(int)eTab];
        ScrollRect pScrollRect = m_pScrollRects[(int)eTab];
        m_iTotalScrollItems[(int)eTab] = 0;

        float viewSize = pScrollRect.viewport.rect.height;
        float itemSize = 0;

        float h = 0;
        RectTransform pItem = null;
        Vector2 size;

        if(eTab == E_TAB.clubs)
        {
            RankingClubItem pClubItem = null;
            while(viewSize > -itemSize)
            {
                if(viewSize > 0)
                {
                    m_iTotalScrollItems[(int)eTab] += 1;
                }
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(strItem);
                    
                if(pItem)
                {
                    pClubItem = new RankingClubItem (pItem,strItem);
                    m_pRankingClubItems.Add(pClubItem);
                    
                    pItem.SetParent(pScrollRect.content,false);
                    pItem.localScale = Vector3.one;       
                    pItem.anchoredPosition = new Vector2(0,-h);
                    size = pItem.sizeDelta;
                    size.x = 0;
                    pItem.sizeDelta = size;
                    
                    itemSize = pItem.rect.height;
                    h += itemSize;
                    viewSize -= itemSize;
                    pItem.gameObject.SetActive(viewSize > -itemSize);
                }
            }
        }
        else
        {
            RankingPlayerItem pPlayerItem = null;

            while(viewSize > -itemSize)
            {
                if(viewSize > 0)
                {
                    m_iTotalScrollItems[(int)eTab] += 1;
                }
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(strItem);
                    
                if(pItem)
                {
                    pPlayerItem = new RankingPlayerItem (pItem,strItem);
                    m_pRankingPlayerItems.Add(pPlayerItem);
                    
                    pItem.SetParent(pScrollRect.content,false);
                    pItem.localScale = Vector3.one;       
                    pItem.anchoredPosition = new Vector2(0,-h);
                    size = pItem.sizeDelta;
                    size.x = 0;
                    pItem.sizeDelta = size;
                    itemSize = pItem.rect.height;
                    h += itemSize;
                    viewSize -= itemSize;
                    pItem.gameObject.SetActive(viewSize > -itemSize);
                }
            }
        }

        size = pScrollRect.content.sizeDelta;
        size.y = h;
        pScrollRect.content.sizeDelta = size;
        pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        pScrollRect.content.anchoredPosition = Vector2.zero;
        LayoutManager.SetReciveUIScollViewEvent(pScrollRect,ScrollViewItemButtonEventCall);
    }

    void SetupPlayerScroll()
    {
        if(m_pRankingPlayerData ==  null) return;
        m_eCurrentTab = E_TAB.players;
        ResetScroll(m_eCurrentTab);
        
        ScrollRect pScrollRect = m_pScrollRects[(int)m_eCurrentTab];

        float viewSize = pScrollRect.viewport.rect.height;
        float itemSize = 0;

        RankingPlayerItem pPlayerItem = null;
        JObject pObject = null;
        
        for(int i =0; i < m_pRankingPlayerItems.Count; ++i)
        {
            pPlayerItem = m_pRankingPlayerItems[i];
            itemSize = pPlayerItem.Target.rect.height;

            if(m_pRankingPlayerData.Count <= i)
            {
                pPlayerItem.Target.gameObject.SetActive(false);
            }
            else
            {
                pObject = (JObject)m_pRankingPlayerData[i];
                pPlayerItem.UpdateData(pObject,i,(int)m_eCurrentSubTab);
            
                if(viewSize > -itemSize)
                {    
                    viewSize -= itemSize;
                    pPlayerItem.Target.gameObject.SetActive(viewSize > -itemSize);
                }
            }
        }

        Vector2 size = pScrollRect.content.sizeDelta;
        size.y = m_pRankingPlayerData.Count * itemSize;
        pScrollRect.content.sizeDelta = size;
        pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    void SetupMyClubRanking(JObject pData)
    {
        GameContext pGameContext = GameContext.getCtx();
        Transform tm = m_pTabUIList[(int)E_TAB.clubs].Find("me");
        TMPro.TMP_Text text = tm.Find("club").GetComponent<TMPro.TMP_Text>();
        text.SetText(pGameContext.GetClubName());

        if(pData.ContainsKey("myStanding") && pData["myStanding"].Type != JTokenType.Null)
        {
            text = tm.Find("overall").GetComponent<TMPro.TMP_Text>();
            text.SetText((string)pData["mySquadPower"]);
            text.SetText((uint)pData["mySquadPower"] > 0 ? (string)pData["mySquadPower"] : pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType()).ToString());

            text = tm.Find("ranking").GetComponent<TMPro.TMP_Text>();
            text.SetText((uint)pData["myStanding"] > 0 ? (string)pData["myStanding"] : "-");
            text = tm.Find("trophy/text").GetComponent<TMPro.TMP_Text>();
            text.SetText((string)pData["myTrophy"]);
            text = tm.Find("win").GetComponent<TMPro.TMP_Text>();
            text.SetText((string)pData["myWin"]);
        }        
    }

    void SetupClubScroll()
    {
        if(m_pRankingClubData == null) return;

        m_eCurrentTab = E_TAB.clubs;
        ResetScroll(m_eCurrentTab);

        ScrollRect pScrollRect = m_pScrollRects[(int)m_eCurrentTab];

        float viewSize = pScrollRect.viewport.rect.height;
        float itemSize = 0;
        
        RankingClubItem pClubItem = null;
        JObject pObject = null;

        for(int i =0; i < m_pRankingClubItems.Count; ++i)
        {
            pClubItem = m_pRankingClubItems[i];
            itemSize = pClubItem.Target.rect.height;

            if(m_pRankingClubData.Count <= i)
            {
                pClubItem.Target.gameObject.SetActive(false);
            }
            else
            {
                pObject = (JObject)m_pRankingClubData[i];
                pClubItem.UpdateData(pObject,i);
            
                if(viewSize > -itemSize)
                {    
                    viewSize -= itemSize;
                    pClubItem.Target.gameObject.SetActive(viewSize > -itemSize);
                }
            }
        }

        Vector2 size = pScrollRect.content.sizeDelta;
        size.y = m_pRankingClubData.Count * itemSize;
        pScrollRect.content.sizeDelta = size;
        pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    void ClearScroll(E_TAB eTab)
    {
        if(eTab == E_TAB.MAX ) return;

        int i = 0;

        if(eTab == E_TAB.clubs)
        {
            i = m_pRankingClubItems.Count;
                
            while(i > 0)
            {
                --i;
                m_pRankingClubItems[i].Dispose();
            }
            m_pRankingClubItems.Clear();
        }
        else
        {
            i = m_pRankingPlayerItems.Count;
                
            while(i > 0)
            {
                --i;
                m_pRankingPlayerItems[i].Dispose();
            }
            m_pRankingPlayerItems.Clear();
        }

        ScrollRect pScrollRect = m_pScrollRects[(int)eTab];
        LayoutManager.SetReciveUIScollViewEvent(pScrollRect,null);

        pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = pScrollRect.content.sizeDelta;
        size.y =0;
        pScrollRect.content.sizeDelta = size;
    }

    void ResetScroll(E_TAB eTab)
    {
        if(eTab == E_TAB.MAX ) return;
        
        Vector2 pos;
        ScrollRect pScrollRect = m_pScrollRects[(int)eTab];

        float viewSize = pScrollRect.viewport.rect.height;
        float itemSize = 0;
        
        if(eTab == E_TAB.clubs)
        {
            RankingClubItem pItem = null;
            for(int i = 0; i < m_pRankingClubItems.Count; ++i)
            {
                pItem = m_pRankingClubItems[i];
                itemSize = pItem.Target.rect.height;
                viewSize -= itemSize;
                pItem.Target.gameObject.SetActive(viewSize > -itemSize);
                pos = pItem.Target.anchoredPosition;            
                pos.y = -i * itemSize;
                pItem.Target.anchoredPosition = pos;
            }
        }
        else
        {
            RankingPlayerItem pItem = null;
            for(int i = 0; i < m_pRankingPlayerItems.Count; ++i)
            {
                pItem = m_pRankingPlayerItems[i];
                itemSize = pItem.Target.rect.height;
                viewSize -= itemSize;
                pItem.Target.gameObject.SetActive(viewSize > -itemSize);
                pos = pItem.Target.anchoredPosition;            
                pos.y = -i * itemSize;
                pItem.Target.anchoredPosition = pos;
            }
        }
        
        pScrollRect = m_pScrollRects[(int)eTab];
        pScrollRect.verticalNormalizedPosition = 1;
        pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
        m_pPrevDir.y = 1;
    }

    void ShowRankingInfo(bool bCurrent)
    {
        GameObject go = m_pAgo.Find("ago").gameObject;
        go.SetActive(!bCurrent);
        go = m_pAgo.Find("current").gameObject;
        go.SetActive(bCurrent);
    }

    void ShowPlayerProfile(ulong id)
    {
        PlayerT pPlayer = GameContext.getCtx().GetPlayerByID(id);
        if(pPlayer != null)
        {
            PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
            pPlayerInfo.SetupPlayerInfoData(E_PLAYER_INFO_TYPE.my,pPlayer);
            pPlayerInfo.SetupQuickPlayerInfoData(GameContext.getCtx().GetTotalPlayerList());
            pPlayerInfo.MoveQuickPlayer();
            pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
            return;
        }
        
        m_pMainScene.RequestPlayerProfile(id,CallbackShowPlayerProfile);
    }

    void CallbackShowPlayerProfile()
    {
        PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
        pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
    }

    void ShowClubProfileOverview()
    {
        m_pMainScene.ShowUserProfile(m_ulSendNo,0);
        m_ulSendNo = 0;
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;

        if(m_eCurrentTab == E_TAB.clubs)
        {            
            RankingClubItem pItem = null;
            if(index > iTarget)
            {
                pItem = m_pRankingClubItems[iTarget];
                m_pRankingClubItems[iTarget] = m_pRankingClubItems[index];
                i = iTarget +1;
                RankingClubItem pTemp = null;
                while(i < index)
                {
                    pTemp = m_pRankingClubItems[i];
                    m_pRankingClubItems[i] = pItem;
                    pItem = pTemp;
                    ++i;
                }
                m_pRankingClubItems[index] = pItem;
                pItem = m_pRankingClubItems[iTarget];
            }
            else
            {
                pItem = m_pRankingClubItems[index];
                i = index +1;
                while(i <= iTarget)
                {
                    m_pRankingClubItems[i -1] = m_pRankingClubItems[i];
                    ++i;
                }

                m_pRankingClubItems[iTarget] = pItem;
            }
            
            i = m_iStartIndex + iTarget + iCount;

            if(i < 0 || m_pRankingClubData.Count <= i) return;

            pItem.UpdateData((JObject)m_pRankingClubData[i],i);
        }
        else
        {
            RankingPlayerItem pItem = null;
            if(index > iTarget)
            {
                pItem = m_pRankingPlayerItems[iTarget];
                m_pRankingPlayerItems[iTarget] = m_pRankingPlayerItems[index];
                i = iTarget +1;
                RankingPlayerItem pTemp = null;
                while(i < index)
                {
                    pTemp = m_pRankingPlayerItems[i];
                    m_pRankingPlayerItems[i] = pItem;
                    pItem = pTemp;
                    ++i;
                }
                m_pRankingPlayerItems[index] = pItem;
                pItem = m_pRankingPlayerItems[iTarget];
            }
            else
            {
                pItem = m_pRankingPlayerItems[index];
                i = index +1;
                while(i <= iTarget)
                {
                    m_pRankingPlayerItems[i -1] = m_pRankingPlayerItems[i];
                    ++i;
                }

                m_pRankingPlayerItems[iTarget] = pItem;
            }
            
            i = m_iStartIndex + iTarget + iCount;

            if(i < 0 || m_pRankingPlayerData.Count <= i) return;
            pItem.UpdateData((JObject)m_pRankingPlayerData[i],i,(int)m_eCurrentSubTab);
        }
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_eCurrentTab == E_TAB.MAX) return;
        
        ScrollRect pScrollRect = m_pScrollRects[(int)m_eCurrentTab];
        int count = 0;
        if(m_eCurrentTab == E_TAB.clubs)
        {
            if(m_pRankingClubData == null) return;
            count = m_pRankingClubData.Count;
        }
        else
        {
            if(m_pRankingPlayerData == null) return;
            count = m_pRankingPlayerData.Count;
        }

        if(m_iTotalScrollItems[(int)m_eCurrentTab] < count && value.y != m_pPrevDir.y)
        {
            pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        if(m_pScrollRects[(int)m_eCurrentTab] != root) return;
        
        if(m_eCurrentTab == E_TAB.clubs)
        {
            for(int i =0; i < m_pRankingClubItems.Count; ++i)
            {
                if(m_pRankingClubItems[i].Target == tm)
                {
                    m_ulSendNo = m_pRankingClubItems[i].Id;
                    m_pMainScene.RequestClubProfile(m_pRankingClubItems[i].Id,ShowClubProfileOverview);
                    return;
                }
            }
        }
        else
        {
            for(int i =0; i < m_pRankingPlayerItems.Count; ++i)
            {
                if(m_pRankingPlayerItems[i].Target == tm)
                {
                    ShowPlayerProfile(m_pRankingPlayerItems[i].Id);
                    return;
                }
            }
        }
    }

    void AddCacheRankingDatas(uint no, byte type,float expire,JObject data)
    {
        for(int i =0; i < m_pCacheRankingDatas.Count; ++i)
        {
            if(m_pCacheRankingDatas[i].SeasonNo == no && m_pCacheRankingDatas[i].Type == type)
            {
                m_pCacheRankingDatas[i].ExpireTime = expire;

                if(!m_pCacheRankingDatas[i].Data.Equals(data))
                {
                    m_pCacheRankingDatas[i].Dispose();
                    m_pCacheRankingDatas[i].Data = data;
                }
                
                return;
            }
        }

        m_pCacheRankingDatas.Add(new CacheRankingData(no,type,expire,data));
    }

    public void UpdateTimer(float dt)
    {
        int i = m_pCacheRankingDatas.Count;
        while(i > 0)
        {
            --i;
            m_pCacheRankingDatas[i].ExpireTime -= dt;
            if(m_pCacheRankingDatas[i].ExpireTime <= 0)
            {
                m_pCacheRankingDatas[i].Dispose();
                m_pCacheRankingDatas.RemoveAt(i);
            }
        }
    }

    public void SetCurrentSeasinNo()
    {
        m_iSeasonNoForClub = GameContext.getCtx().GetCurrentSeasonNo();
        m_iSeasonNoForPlayer = m_iSeasonNoForClub;
    }
    public void Close()
    {
        Enable = false;
        m_ulSendNo = 0;
        SingleFunc.HideAnimationDailog(MainUI);
    }

    public JObject GetCacheRankingData(uint no, byte type)
    {
        for(int i =0; i < m_pCacheRankingDatas.Count; ++i)
        {
            if(m_pCacheRankingDatas[i].SeasonNo == no && m_pCacheRankingDatas[i].Type == type && m_pCacheRankingDatas[i].ExpireTime > 0)
            {
                return m_pCacheRankingDatas[i].Data;
            }
        }

        return null;
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(sender.name)
        {
            case "close":
            case "back":
            {
                Close();
            }
            break;
            case "ago":
            {
                uint id = GameContext.getCtx().GetCurrentSeasonNo();               
                if(m_eCurrentTab == E_TAB.clubs)
                {   
                    if(m_iSeasonNoForClub == id)
                    {
                        id = GameContext.getCtx().GetPrevSeasonNo();
                    }

                    JObject data = GetCacheRankingData(id,(byte)E_TAB.clubs);
                    if(data != null)
                    {
                        NetworkDataParse(E_REQUEST_ID.club_top100,data);
                    }
                    else
                    {
                        JObject pJObject = new JObject();
                        pJObject["seasonNo"] = id;
                        pJObject["matchType"] = GameContext.LADDER_ID;
                        m_pMainScene.RequestAfterCall(E_REQUEST_ID.club_top100,pJObject);
                    }
                }
                else
                {
                    if(m_iSeasonNoForPlayer == id)
                    {
                        id = GameContext.getCtx().GetPrevSeasonNo();
                    }

                    JObject data = GetCacheRankingData(id,(byte)m_eCurrentSubTab);
                    if(data != null)
                    {
                        NetworkDataParse(E_REQUEST_ID.playerStats_top100,data);
                    }
                    else
                    {
                        JObject pJObject = new JObject();
                        pJObject["seasonNo"] = id;
                        pJObject["matchType"] = GameContext.LADDER_ID;
                        pJObject["type"]=(int)m_eCurrentSubTab;
                        m_pMainScene.RequestAfterCall(E_REQUEST_ID.playerStats_top100,pJObject);
                    }
                }
            }
            break;
            case "reward":
            {
                m_pMainScene.ShowRankingRewardPopup();
            }
            break;
            case "clubs":
            {
                m_eCurrentTab = E_TAB.clubs;
                JObject data = GetCacheRankingData(m_iSeasonNoForClub,(byte)E_TAB.clubs);
                if(data != null)
                {
                    NetworkDataParse(E_REQUEST_ID.club_top100,data);
                }
                else
                {
                    JObject pJObject = new JObject();
                    pJObject["seasonNo"] = m_iSeasonNoForClub;
                    pJObject["matchType"] = GameContext.LADDER_ID;
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.club_top100,pJObject);
                }
            }
            break;
            case "players":
            {
                m_eCurrentTab = E_TAB.players;
                m_eCurrentSubTab = E_SUB_TAB.goals;
                JObject data = GetCacheRankingData(m_iSeasonNoForPlayer,(byte)m_eCurrentSubTab);
                if(data != null)
                {
                    NetworkDataParse(E_REQUEST_ID.playerStats_top100,data);
                }
                else
                {
                    JObject pJObject = new JObject();
                    pJObject["seasonNo"] = m_iSeasonNoForPlayer;
                    pJObject["matchType"] = GameContext.LADDER_ID;
                    pJObject["type"] = (int)m_eCurrentSubTab;
                    
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.playerStats_top100,pJObject);
                }
            }
            break;
            case "goals":
            case "assists":
            case "prossessionWon":
            case "saves":
            {
                m_eCurrentSubTab = (E_SUB_TAB)Enum.Parse(typeof(E_SUB_TAB), sender.name);
                JObject data = GetCacheRankingData(m_iSeasonNoForPlayer,(byte)m_eCurrentSubTab);
                if(data != null)
                {
                    NetworkDataParse(E_REQUEST_ID.playerStats_top100,data);
                }
                else
                {
                    JObject pJObject = new JObject();
                    pJObject["seasonNo"] = m_iSeasonNoForPlayer;
                    pJObject["matchType"] = GameContext.LADDER_ID;
                    pJObject["type"]=(int)m_eCurrentSubTab;
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.playerStats_top100,pJObject);
                }
            }
            break;
        } 
    }
    
    public void  NetworkDataParse(E_REQUEST_ID id,JObject data)
    {
        uint seasonNo = 0;
        byte eType = 0;
        if(E_REQUEST_ID.playerStats_top100 == id)
        {
            if((int)data["matchType"] != GameContext.LADDER_ID) return;
            
            m_pRankingPlayerData = null;

            m_iSeasonNoForPlayer = (uint)data["seasonNo"];
            seasonNo = m_iSeasonNoForPlayer;
            eType = (byte)m_eCurrentSubTab;
            if(data.ContainsKey("players") && data["players"].Type != JTokenType.Null)
            {
                m_pRankingPlayerData = (JArray)data["players"];
                SetupPlayerScroll();
            }
            
            ShowTabUI(E_TAB.players);
        }
        else if(E_REQUEST_ID.club_top100 == id)
        {
            if((int)data["matchType"] != GameContext.LADDER_ID) return;

            m_pRankingClubData = null;
            m_iSeasonNoForClub = (uint)data["seasonNo"];
            seasonNo = m_iSeasonNoForClub;
            eType = (byte)E_TAB.clubs;
            if(data.ContainsKey("standings") && data["standings"].Type != JTokenType.Null)
            {
                m_pRankingClubData = (JArray)data["standings"];
                SetupClubScroll();
            }

            SetupMyClubRanking(data);
            ShowTabUI(E_TAB.clubs);
        }
        if(seasonNo > 0)
        {
            // 2031-01-01 이면 영구 기록 
            AddCacheRankingDatas(seasonNo, eType,(float)(NetworkManager.ConvertLocalGameTimeTick(data["tExpire"].ToString()) - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond,data);
        }
    }
}
