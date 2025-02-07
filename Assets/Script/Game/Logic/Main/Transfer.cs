using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using ALF.NETWORK;
using USERDATA;
using STATEDATA;
using Newtonsoft.Json.Linq;
using PLAYERNATIONALITY;
using CONSTVALUE;
using USERRANK;

public class Transfer : ITimer,IBaseNetwork,ISoketNetwork
{
    public const Byte E_STATUS_OFFER = 19;
    enum E_SORT_TOKEN : byte {contract,value,profile,quality,name,roles,age}
    enum E_TAB : byte { transfer_market = 0,recuiting,youth_promotion,scout,MAX}
    enum E_MARKET : byte { profile = 0,roles,age,value,deadline,MAX}
    enum E_RECUITING : byte { name = 0,roles,age,quality,value,MAX}
    enum E_SCROLL_TYPE : byte { bidding = 0,market,recuiting,scoutTickets,MAX}
    enum E_SCROLL_ITEM : byte { BiddingItem = 0,MarketItem,RecuitingItem,ScoutTicketItem,MAX}

    enum E_TEXT_NAME : byte { TRANSFER_SCOUT_TXT_POSITION_GENERAL = 0,TRANSFER_SCOUT_TXT_POSITION_DFGK,TRANSFER_SCOUT_TXT_POSITION_MF,TRANSFER_SCOUT_TXT_POSITION_FW,MAX}

    enum E_SCOUT_STAR_START_COUNT : byte { NovitiateScoutStar = 6,EliteScoutStar = 9, LegendaryScoutStar = 11,MAX}
    
    MainScene m_pMainScene = null;
    ScrollRect[] m_pScrollList = new ScrollRect[(int)E_SCROLL_TYPE.MAX];
    RectTransform[] m_pTabUIList = new RectTransform[(int)E_TAB.MAX];
    Transform[] m_pTabButtonList = new Transform[(int)E_TAB.MAX];
    Transform[] m_pMarketTabButtonList = new Transform[(int)E_MARKET.deadline];
    Transform[] m_pRecuitingTabButtonList = new Transform[(int)E_RECUITING.MAX];
    Transform[] m_pYouthSlotList = new Transform[4];
    Transform[] m_pScoutSlotList = new Transform[3];
    bool m_bRefreshUpdate = false;
    bool m_bFoldding = true;
    Button m_pRefreshTimerButton = null;
    TMPro.TMP_Text m_pRefreshText = null;
    TMPro.TMP_Text m_pSkipItmeCountText = null;
    
    TMPro.TMP_Text[] m_pRemainingTimeTexts = new TMPro.TMP_Text[2];
    List<SortPlayer> m_pSortPlayers = new List<SortPlayer>();
    List<SortPlayer> m_pBiddingSortPlayers = new List<SortPlayer>();

    E_TAB m_eMainTab = E_TAB.MAX;
    ulong m_iScoutPlayerID = 0;
    E_SORT_TOKEN m_eSortToken = E_SORT_TOKEN.name;
    Animation m_pScoutResult = null;
    Transform m_pScoutTickets = null;
    Transform m_pSkipButton = null;
    TMPro.TMP_Text m_pYouthNominationText = null;
    TMPro.TMP_Text m_pSkipItemText = null;
    bool m_bRequestAuctionGet = false;

    class SortPlayer : IBase
    {   
        public ulong Id {get; private set;}     
        public int Index {get; private set;}     
        public ulong Value {get; private set;}
        public string Name {get; private set;}
        public byte Age {get; private set;}
        public byte Position {get; private set;}
        public uint Quality {get; private set;}
        public bool Reward {get; set;}
        
        public RectTransform Target  {get; private set;}

        public SortPlayer(RectTransform target,ulong id, int index, string name,byte age, byte position, uint quality, ulong value)
        {
            Id = id;
            Index = index;
            Target = target;
            Name = name;
            Age = age;
            Position = position;
            Quality = quality;
            Value = value;
            Reward = false;
        }

        public void Dispose()
        {
            Target = null;
        }
    }

    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ 
        for ( int i = 0; i < m_pScrollList.Length; ++i) 
        {
            m_pScrollList[i].enabled = value;
        }
    }}

    public Transfer(){}
    
    public void Dispose()
    {
        int i =0;
        for(i =0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i] = null;
            m_pTabButtonList[i] = null;
        }

        for(i =0; i < m_pMarketTabButtonList.Length; ++i)
        {
            m_pMarketTabButtonList[i] = null;
            m_pRecuitingTabButtonList[i] = null;
        }

        for(i =0; i < m_pYouthSlotList.Length; ++i)
        {
            m_pYouthSlotList[i] = null;
        }
        for(i =0; i < m_pScrollList.Length; ++i)
        {
            m_pScrollList[i].onValueChanged.RemoveAllListeners();
            ClearScroll(m_pScrollList[i],((E_SCROLL_ITEM)i).ToString());        
            m_pScrollList[i] = null;
        }
        for(i =0; i < m_pScoutSlotList.Length; ++i)
        {
            m_pScoutSlotList[i] = null;
        }
        m_pScoutSlotList = null;

        for(i =0; i < m_pSortPlayers.Count; ++i)
        {
            m_pSortPlayers[i].Dispose();
            m_pSortPlayers[i] = null; 
        }

        for(i =0; i < m_pBiddingSortPlayers.Count; ++i)
        {
            m_pBiddingSortPlayers[i].Dispose();
            m_pBiddingSortPlayers[i] = null; 
        }

        m_pBiddingSortPlayers.Clear();
        m_pBiddingSortPlayers = null;

        m_pRefreshTimerButton = null;
        m_pRefreshText = null;
        m_pSkipItmeCountText = null;
        for(i =0; i < m_pRemainingTimeTexts.Length; ++i)
        {
            m_pRemainingTimeTexts[i] = null; 
        }
        m_pRemainingTimeTexts = null;

        m_pSortPlayers.Clear();
        m_pSortPlayers = null;
        m_pScrollList = null;
        m_pTabButtonList = null;
        m_pTabUIList = null;
        m_pMarketTabButtonList = null;
        m_pRecuitingTabButtonList = null;
        m_pYouthSlotList = null;
        m_pMainScene = null;
        m_pYouthNominationText = null;
        MainUI = null;
        m_pScoutResult.Stop();
        Transform tm = m_pScoutResult.transform.Find("root/roles/roles");
        for(i = 0; i < tm.childCount; ++i)
        {
            tm.GetChild(i).GetComponent<RawImage>().texture = null;
        }

        tm = m_pScoutResult.transform.Find("root/player/icon");
        for(i = 0; i < tm.childCount; ++i)
        {
            tm.GetChild(i).GetComponent<RawImage>().texture = null;
        }

        m_pScoutResult = null;
        m_pScoutTickets = null;
        m_pSkipButton = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Transfer : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Transfer : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        float w = 0;
        float wh = 0;

        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        float ax = 0;
        int iTabIndex = -1;

        RectTransform ui = MainUI.Find("back").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);
        ui = MainUI.Find("root/bg/tip").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);
        ui = MainUI.Find("root/scout/scoutResult").GetComponent<RectTransform>();
        m_pScoutResult = ui.Find("scoutResult").GetComponent<Animation>();
        SingleFunc.SetupLocalizingText(m_pScoutResult.transform);
        
        m_pSkipButton = m_pScoutResult.transform.Find("skip");
        m_pScoutResult.transform.Find("root").gameObject.SetActive(false); 
        ui.SetParent(MainUI.parent,true);
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);
        ui.gameObject.SetActive(false);
        
        ui = MainUI.Find("root/tabs").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("root/item").GetComponent<RectTransform>(),ButtonEventCall);

        w = (ui.rect.width / ui.childCount);
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        int n =0;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;
            m_pTabUIList[iTabIndex] = MainUI.Find($"root/{item.gameObject.name}").GetComponent<RectTransform>();
            LayoutManager.SetReciveUIButtonEvent(m_pTabUIList[iTabIndex],ButtonEventCall);
            m_pTabUIList[iTabIndex].gameObject.SetActive(false);

            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }

        Transform tm = m_pTabUIList[(int)E_TAB.transfer_market].Find("tabs");

        for(n =0; n < tm.childCount; ++n)
        {
            iTabIndex = (int)((E_MARKET)Enum.Parse(typeof(E_MARKET), tm.GetChild(n).gameObject.name));
            if(iTabIndex < m_pMarketTabButtonList.Length)
            {
                m_pMarketTabButtonList[iTabIndex] = tm.GetChild(n);
            }
        }
        
        tm = m_pTabUIList[(int)E_TAB.recuiting].Find("tabs");

        for(n =0; n < tm.childCount; ++n)
        {
            iTabIndex = (int)((E_RECUITING)Enum.Parse(typeof(E_RECUITING), tm.GetChild(n).gameObject.name));
            m_pRecuitingTabButtonList[iTabIndex] = tm.GetChild(n);
        }

        tm = m_pTabUIList[(int)E_TAB.youth_promotion];

        for(n =0; n < m_pYouthSlotList.Length; ++n)
        {
            m_pYouthSlotList[n] = tm.Find($"slot_{n}");
        }
    
        ScrollRect[] list = MainUI.GetComponentsInChildren<ScrollRect>(true);
        
        for(n = 0; n < list.Length; ++n)
        {
            iTabIndex = (int)((E_SCROLL_TYPE)Enum.Parse(typeof(E_SCROLL_TYPE), list[n].transform.parent.gameObject.name)); 
            m_pScrollList[iTabIndex] = list[n];
        }
        
        m_pScrollList[(int)E_SCROLL_TYPE.market].onValueChanged.AddListener( ScrollViewChangeValueEventCall);

        ui = m_pTabUIList[(int)E_TAB.scout].Find("slots").GetComponent<RectTransform>();
        m_pScoutSlotList[0] = ui.Find("slot_0");
        m_pScoutSlotList[1] = ui.Find("slot_1");
        m_pScoutSlotList[2] = ui.Find("slot_2");
        
        w = (ui.rect.height - 150) / m_pScoutSlotList.Length;
        wh = w * 0.5f;
        ax = ui.pivot.y * (ui.rect.height);
        n = m_pScoutSlotList.Length;
        while(n > 0)
        {
            --n;
            ui = m_pScoutSlotList[n].GetComponent<RectTransform>();
            pos = ui.localPosition;
            pos.y = wh + ((m_pScoutSlotList.Length - (n +1)) * (w + 55)) - ax;
            ui.localPosition = pos;
        }

        m_pScoutTickets = m_pTabUIList[(int)E_TAB.scout].Find("scoutTickets");
        tm = m_pTabUIList[0].Find("market/remaining/time");
        m_pRemainingTimeTexts[0] = tm.Find("m").GetComponent<TMPro.TMP_Text>();
        m_pRemainingTimeTexts[1] = tm.Find("s").GetComponent<TMPro.TMP_Text>();
        m_pRefreshTimerButton = MainUI.Find("root/item/refresh").GetComponent<Button>();
        m_pSkipItmeCountText = m_pRefreshTimerButton.transform.Find("on/ad/refresh/count").GetComponent<TMPro.TMP_Text>();
        m_pRefreshText = m_pRefreshTimerButton.transform.Find("title").GetComponent<TMPro.TMP_Text>();

        EnableAdRefreshTimer(E_TAB.recuiting,false);
        EnableAdRefreshTimer(E_TAB.youth_promotion,false);
        EnableRefreshTimer(E_TAB.recuiting,false);
        EnableRefreshTimer(E_TAB.youth_promotion,false);

        m_pYouthNominationText = MainUI.Find("root/item/nomination/text").GetComponent<TMPro.TMP_Text>();
        m_pSkipItemText =  MainUI.Find("root/item/skipItem/text").GetComponent<TMPro.TMP_Text>();
        MainUI.gameObject.SetActive(false);
    }

    public void DoExpire(int index)
    {
        E_REQUEST_ID eReq = E_REQUEST_ID.auction_get;
        E_TAB eTab = E_TAB.MAX;
        if(index == 0)
        {
            if(m_eMainTab == E_TAB.recuiting )
            {
                eReq = E_REQUEST_ID.recruit_get;
                eTab = E_TAB.recuiting;                    
            }
            else if(m_eMainTab == E_TAB.youth_promotion )
            {
                eTab = E_TAB.youth_promotion;
                eReq = E_REQUEST_ID.youth_get;
            }
        }
        else if(index == 1)
        {
            eTab = E_TAB.transfer_market;
            m_bRequestAuctionGet = true;
            eReq = E_REQUEST_ID.auction_get;
            Debug.Log("-------------------------------------------DoExpire");
        }

        SendRequest(eReq,eTab);
    }

    void SendRequest(E_REQUEST_ID eReq,E_TAB eTab)
    {
        Debug.Log("-------------------------------------------SendRequest");
        m_pMainScene.RequestAfterCall( eReq,null,()=>{
            if(eReq == E_REQUEST_ID.auction_get)
            {
                m_bRequestAuctionGet = false; 
            }
            
            if(eTab == E_TAB.MAX) return;
            
            if(MainUI != null && MainUI.gameObject.activeSelf )
            {
                ShowTabUI((byte)eTab);
            }
        });
    }
    public bool IsRequestAuctionGet()
    {
        return m_bRequestAuctionGet;
    }
    public RectTransform GetRecuitingItemUI(int index)
    {
        ScrollRect pScrollRect = m_pScrollList[(int)E_SCROLL_TYPE.recuiting];
        if(pScrollRect.content.childCount > 0)
        {
            return pScrollRect.content.GetChild(index)?.GetComponent<RectTransform>();
        }
        return null;
    }

    void EnableAdRefreshTimer(E_TAB eMain,bool enable)
    {
        if(eMain == E_TAB.recuiting || eMain == E_TAB.youth_promotion)
        {
            int index = (int)eMain;
            m_pRefreshTimerButton.transform.Find("on/normal").gameObject.SetActive(!enable);
            m_pRefreshTimerButton.transform.Find("on/ad").gameObject.SetActive(enable);
        }
    }

    void EnableRefreshTimer(E_TAB eMain,bool enable)
    {
        if(eMain == E_TAB.recuiting || eMain == E_TAB.youth_promotion)
        {
            int index = (int)eMain;
            m_pRefreshTimerButton.transform.Find("on").gameObject.SetActive(enable);
            m_pRefreshTimerButton.transform.Find("off").gameObject.SetActive(!enable);
            m_pRefreshTimerButton.enabled = enable;
        }
    }

    void SetItemBox()
    {
        if(m_eMainTab == E_TAB.transfer_market || m_eMainTab == E_TAB.scout)
        {
            m_pYouthNominationText.transform.parent.parent.gameObject.SetActive(false);
        }
        else
        {
            GameContext pGameContext = GameContext.getCtx();
            m_pYouthNominationText.transform.parent.parent.gameObject.SetActive(true);
            
            if(m_eMainTab == E_TAB.recuiting)
            {
                m_pSkipItemText.transform.parent.gameObject.SetActive(true);
                m_pYouthNominationText.transform.parent.gameObject.SetActive(false);
                RectTransform tm = m_pYouthNominationText.transform.parent.GetComponent<RectTransform>();
                Vector2 pos = tm.anchoredPosition;

                tm = m_pSkipItemText.transform.parent.GetComponent<RectTransform>();
                tm.anchoredPosition = pos;
            }
            else
            {
                m_pSkipItemText.transform.parent.gameObject.SetActive(false);
                m_pYouthNominationText.transform.parent.gameObject.SetActive(true);
                RectTransform tm = m_pYouthNominationText.transform.parent.GetComponent<RectTransform>();
                Vector2 pos = tm.anchoredPosition;
                pos.x += tm.rect.width + 20;
                tm = m_pSkipItemText.transform.parent.GetComponent<RectTransform>();
                tm.anchoredPosition = pos;
            }
            ulong itemCount = pGameContext.GetItemCountByNO(GameContext.AD_SKIP_ID);
            if(itemCount > 0)
            {
                m_pRefreshText.gameObject.SetActive(false);
                m_pRefreshTimerButton.transform.Find("on/ad/ad").gameObject.SetActive(false);
                m_pSkipItmeCountText.transform.parent.gameObject.SetActive(true);
                m_pSkipItmeCountText.SetText($"x{itemCount}");
            }
            else
            {
                m_pRefreshText.gameObject.SetActive(true);
                m_pRefreshTimerButton.transform.Find("on/ad/ad").gameObject.SetActive(true);
                m_pSkipItmeCountText.transform.parent.gameObject.SetActive(false);
            }

            UpdateSkipCount();
            UpdateYouthNominationCount();
        }
    }

    public void SetupTimer()
    {
        GameContext pGameContext = GameContext.getCtx();
        for(uint n = 0; n < 4; ++n)
        {
            if(pGameContext.GetAdRefreshTime(n) > 0)
            {
                m_bRefreshUpdate = true;
            }
        }
    }

    public void ShowTabUI(byte eTab)
    {
        int i = 0;
        for(i = 0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i].gameObject.SetActive(eTab == i);
            m_pTabButtonList[i].Find("on").gameObject.SetActive(eTab == i);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = eTab == i ? Color.white : GameContext.GRAY;
        }
        GameContext pGameContext = GameContext.getCtx();
        m_pTabButtonList[(int)E_TAB.youth_promotion].Find("lock").gameObject.SetActive(!pGameContext.IsLicenseContentsUnlock(E_CONST_TYPE.licenseContentsUnlock_3));

        if(m_eMainTab == E_TAB.transfer_market)
        {
            int index = (int)E_SCROLL_TYPE.bidding;
            ClearScroll(m_pScrollList[index],((E_SCROLL_ITEM)index).ToString());
            index = (int)E_SCROLL_TYPE.market;
            ClearScroll(m_pScrollList[index],((E_SCROLL_ITEM)index).ToString());
        }
        else if(m_eMainTab == E_TAB.recuiting)
        {
            int index = (int)E_SCROLL_TYPE.recuiting;
            ClearScroll(m_pScrollList[index],((E_SCROLL_ITEM)index).ToString());
        }
        else if(m_eMainTab == E_TAB.scout)
        {
            int index = (int)E_SCROLL_TYPE.scoutTickets;
            ClearScroll(m_pScrollList[index],((E_SCROLL_ITEM)index).ToString());
        }
        
        m_eMainTab = (E_TAB)eTab;
        SetItemBox();
        
        m_bRefreshUpdate = false;
        
        float tic = 0;
        if(m_eMainTab == E_TAB.transfer_market)
        {
            tic = pGameContext.GetExpireTimerByUI(this,1);
        }
        else
        {    
            tic = pGameContext.GetAdRefreshTime((uint)m_eMainTab);
        }

        m_bRefreshUpdate = tic > 0;

        ShowSubTabUI(m_eMainTab,0);

        if(m_eMainTab == E_TAB.transfer_market)
        {
            SetupScroll(E_SCROLL_TYPE.bidding);
            SetupScroll(E_SCROLL_TYPE.market);
            m_eSortToken = E_SORT_TOKEN.value;
            Sort(m_eSortToken,ref m_pBiddingSortPlayers, m_pScrollList[(int)E_SCROLL_TYPE.bidding],true);
            Sort(m_eSortToken,ref m_pSortPlayers,m_pScrollList[(int)E_SCROLL_TYPE.market],true);
        }
        else if(m_eMainTab == E_TAB.recuiting)
        {
            m_eSortToken = E_SORT_TOKEN.name;
            SetupScroll(E_SCROLL_TYPE.recuiting);
            Sort(m_eSortToken,ref m_pSortPlayers,m_pScrollList[(int)E_SCROLL_TYPE.recuiting],true);
        }
        else if(m_eMainTab == E_TAB.youth_promotion)
        {
            SetupYouthPlayer();
        }
        else if(m_eMainTab == E_TAB.scout)
        {
            UpdateScoutSlot();
            SetupScroll(E_SCROLL_TYPE.scoutTickets);
            UpdateScoutSlotData(E_TEXT_NAME.TRANSFER_SCOUT_TXT_POSITION_GENERAL);
            m_pScoutResult.Stop();
            m_pScoutResult.transform.parent.gameObject.SetActive(false);
            m_pScoutTickets.gameObject.SetActive(false);
            m_pScoutResult.gameObject.SetActive(false);
        }
        UpdateRefreshTimeButton(m_eMainTab,tic);
    }

    void UpdateScoutSlot()
    {
        int i =0;
        Transform tm = null;
        TMPro.TMP_Text text = null;
        Transform item = null;
        GameContext pGameContext = GameContext.getCtx(); 
        E_CONST_TYPE eConstType = E_CONST_TYPE.transferScoutNovitiateScoutStar0;
        E_SCOUT_STAR_START_COUNT[] eStarCount = new E_SCOUT_STAR_START_COUNT[]{E_SCOUT_STAR_START_COUNT.NovitiateScoutStar,E_SCOUT_STAR_START_COUNT.EliteScoutStar,E_SCOUT_STAR_START_COUNT.LegendaryScoutStar};
        
        int count = 0;
        UserRankList pUserRankList = pGameContext.GetFlatBufferData<UserRankList>(E_DATA_TYPE.UserRank);
        UserRankItem? pUserRankItem = pUserRankList.UserRankByKey(pGameContext.GetCurrentUserRank());
        int rank = pUserRankItem.Value.PlayerTierMin;

        for(int e = 0; e < m_pScoutSlotList.Length; ++e)
        {
            tm = m_pScoutSlotList[e];
            count = (int)eStarCount[e];
            for(i =0; i < 5; ++i)
            {
                item = tm.Find($"quality/tier_{i}");
                text = item.Find("percent").GetComponent<TMPro.TMP_Text>();
                text.SetText($"{(pGameContext.GetConstValue(eConstType) / 1000)}%");
                text = item.Find("no").GetComponent<TMPro.TMP_Text>();
                text.SetText((rank + count).ToString());
                ++eConstType;
                ++count;
            }
        }
    }

    void UpdateScoutSlotData(E_TEXT_NAME eType)
    {
        uint id = GameContext.SCOUT_TICKET_ID_START + (uint)eType;
        for(int i = 0; i < m_pScoutSlotList.Length; ++i)
        {
            UpdateScoutSlotItem(eType,m_pScoutSlotList[i],id);
            id +=10;
        } 
    }

    void UpdateScoutSlotItem(E_TEXT_NAME eType,Transform tm,uint id)
    {
        Button pButton = tm.Find("buy").GetComponent<Button>();
        RawImage icon = pButton.transform.Find("icon").GetComponent<RawImage>();
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",id.ToString());
        icon.texture = pSprite.texture;
        ulong count = GameContext.getCtx().GetItemCountByNO(id);
        pButton.enabled = count > 0;
        pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
        pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
        LocalizingText text = tm.Find("formation/text").GetComponent<LocalizingText>();
        text.Key = eType.ToString();
        text.UpdateLocalizing();
        icon.transform.Find("text").GetComponent<TMPro.TMP_Text>().SetText($"x{count}");
    }

    void ShowSubTabUI(E_TAB eMain, byte eTab)
    {
        if(eMain == E_TAB.transfer_market)
        {        
            Transform item = null;
            bool bActive = false;
            for(int i = 0; i < m_pMarketTabButtonList.Length; ++i)
            {
                bActive = eTab == i;
                m_pMarketTabButtonList[i].Find("on").gameObject.SetActive(bActive);
                m_pMarketTabButtonList[i].Find("title").GetComponent<Graphic>().color = bActive ? Color.white : GameContext.GRAY;
                item = m_pMarketTabButtonList[i].Find("mark");
                item.gameObject.SetActive(bActive);
                item.localScale = Vector3.one;
            }
        }
        else if(eMain == E_TAB.recuiting)
        {
            Transform item = null;
            bool bActive = false;
            for(int i = 0; i < m_pRecuitingTabButtonList.Length; ++i)
            {
                bActive = eTab == i;
                m_pRecuitingTabButtonList[i].Find("on").gameObject.SetActive(bActive);
                m_pRecuitingTabButtonList[i].Find("title").GetComponent<Graphic>().color = bActive ? Color.white : GameContext.GRAY;
                item = m_pRecuitingTabButtonList[i].Find("mark");
                item.gameObject.SetActive(bActive);
                item.localScale = Vector3.one;
            }
        }
    }

    void ClearScroll(ScrollRect pScroll,string itemName)
    {
        int i = 0;
        if(pScroll != null && !string.IsNullOrEmpty(itemName))
        {
            LayoutManager.SetReciveUIScollViewEvent(pScroll,null);
            i = pScroll.content.childCount;
            RawImage icon = null;
            Transform tm = null;
            RectTransform item = null;
            while(i > 0)
            {
                --i;
                item = pScroll.content.GetChild(i).GetComponent<RectTransform>();
                tm = item.Find("nation");
                if(tm != null)
                {
                   icon = tm.GetComponent<RawImage>();
                   icon.texture = null;
                }

                tm = item.Find("icon");
                if(tm != null)
                {
                   icon = tm.GetComponent<RawImage>();
                   icon.texture = null;
                }

                tm = item.Find("roles");
                if(tm != null)
                {
                    for(int n = 0; n < tm.childCount; ++n)
                    {
                        icon = tm.GetChild(n).GetComponent<RawImage>();
                        icon.texture = null;
                    }
                }

                LayoutManager.Instance.AddItem(itemName,item);
            }
        }
        
        for(i = 0; i < m_pSortPlayers.Count; ++i)
        {
            m_pSortPlayers[i].Dispose();
        }
        for(i = 0; i < m_pBiddingSortPlayers.Count; ++i)
        {
            m_pBiddingSortPlayers[i].Dispose();
        }
        m_pBiddingSortPlayers.Clear();
        m_pSortPlayers.Clear();
    }
    void UpdateYouthPlayerData(Transform item, PlayerT data)
    {
        GameContext pGameContext = GameContext.getCtx();
        Button pButton = item.Find("player").GetComponent<Button>();
        pButton.enabled = data.Status == 0;
        SingleFunc.SetupPlayerCard(data,pButton.transform,E_ALIGN.Left);
        
        TMPro.TMP_Text text = item.Find("name").GetComponent<TMPro.TMP_Text>();
        text.SetText($"{data.Forename} {data.Surname}");
        text = item.Find("age/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(pGameContext.GetPlayerAge(data).ToString());
        text = item.Find("nation/text").GetComponent<TMPro.TMP_Text>();

        NATION_CODE code = pGameContext.ConvertPlayerNationCodeByString(data.Nation,data.Id);
        PlayerNationalityItem? pPlayerNationalityItem = pGameContext.GetPlayerNationDataByCode(code);
        text.SetText(pGameContext.GetLocalizingText(pPlayerNationalityItem.Value.List(0).Value.NationName));
        text = item.Find("quality/text").GetComponent<TMPro.TMP_Text>();
        text.SetText($"{data.AbilityTier}/{data.PotentialTier}");

        pButton = item.Find("promote").GetComponent<Button>();
        pButton.enabled = data.Status == 0;

        text = item.Find("promote/nomination/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(data.RecmdLetter.ToString());
        text = item.Find("promote/money/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(ALFUtils.NumberToString(data.Price));
        
        if(pButton.enabled)
        {
            pButton.transform.Find("complete").gameObject.SetActive(false);
            pButton.transform.Find("nomination").gameObject.SetActive(true);
            pButton.transform.Find("money").gameObject.SetActive(true);

            pButton.enabled = pGameContext.GetYouthNominationCount() >= data.RecmdLetter && pGameContext.GetGameMoney() >= data.Price;
            pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
            pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
            
            text = item.Find("promote/nomination/text").GetComponent<TMPro.TMP_Text>();
            text = item.Find("promote/money/text").GetComponent<TMPro.TMP_Text>();
        }
        else
        {
            pButton.transform.Find("off").gameObject.SetActive(true);
            pButton.transform.Find("on").gameObject.SetActive(false);
            pButton.transform.Find("complete").gameObject.SetActive(true);
            pButton.transform.Find("nomination").gameObject.SetActive(false);
            pButton.transform.Find("money").gameObject.SetActive(false);
        }
    }

    void UpdateSkipCount()
    {
        m_pSkipItemText.SetText(GameContext.getCtx().GetItemCountByNO(GameContext.AD_SKIP_ID).ToString());
    }

    void UpdateYouthNominationCount()
    {
        m_pYouthNominationText.SetText(GameContext.getCtx().GetYouthNominationCount().ToString());
    }
    void SetupYouthPlayer()
    {
        GameContext pGameContext = GameContext.getCtx();
        uint count = pGameContext.GetYouthSlotCount();
                
        List<PlayerT> YouthPlayerList = pGameContext.GetYouthPromotionCachePlayerData();
        Transform tm = null;
        for(int i = 0; i < m_pYouthSlotList.Length; ++i)
        {
            if(i < count)
            {
                tm = m_pYouthSlotList[i].Find("info");
                m_pYouthSlotList[i].Find("empty").gameObject.SetActive(false);
                tm.gameObject.SetActive(true);
                
                UpdateYouthPlayerData(tm,YouthPlayerList[i]);
            }
            else
            {
                m_pYouthSlotList[i].Find("empty").gameObject.SetActive(true);
                m_pYouthSlotList[i].Find("info").gameObject.SetActive(false);
            }
        }
    }
    void UpdateScrollItemStatus(Transform item, PlayerT data)
    {
        Transform tm = item.Find("on");
        bool bEnable = data.Status == 0;
        tm.gameObject.SetActive(bEnable);
        tm = item.Find("off");
        tm.gameObject.SetActive(!bEnable);
        item.GetComponent<Button>().enabled = bEnable;
        tm = item.Find("type");
        if(tm != null)
        {
            tm.gameObject.SetActive(bEnable);
        }
    }

    void SetupScroll( E_SCROLL_TYPE eType)
    {
        GameContext pGameContext = GameContext.getCtx();
        ScrollRect pScroll = m_pScrollList[(int)eType];
        ALFUtils.Assert(pScroll != null, "Transfer : SetupScroll => pScroll = null !");
        string itemName = ((E_SCROLL_ITEM)eType).ToString();
        
        List<PlayerT> playerList = null; 
        if(eType == E_SCROLL_TYPE.bidding)
        {
            playerList = pGameContext.GetAuctionBiddingPlayerInfoList();
        }
        else if(eType == E_SCROLL_TYPE.market)
        {
            playerList = pGameContext.GetAuctionPlayerInfoList();
        }
        else if(eType == E_SCROLL_TYPE.recuiting)
        {
            playerList = pGameContext.GetRecuitingCachePlayerData();
        }

        Vector2 size;
        float h = 0;
        
        TMPro.TMP_Text text = null;
        List<string> locList= new List<string>();
        RectTransform pItem = null;
        RawImage icon = null;
        Sprite pSprite = null;

        int n = 0;
        byte age = 0;
       
        if(playerList != null)
        {
            for(int i =0; i < playerList.Count; ++i)
            {
                pItem = LayoutManager.Instance.GetItem<RectTransform>(itemName);
                
                age = pGameContext.GetPlayerAge(playerList[i]);
                if(eType == E_SCROLL_TYPE.bidding)
                {
                    m_pBiddingSortPlayers.Add(new SortPlayer(pItem,playerList[i].Id, i, $"{playerList[i].Forename[0]}. {playerList[i].Surname}",age,playerList[i].Position,playerList[i].AbilityWeightSum,playerList[i].Price));
                }
                else
                {
                    m_pSortPlayers.Add(new SortPlayer(pItem,playerList[i].Id, i, $"{playerList[i].Forename[0]}. {playerList[i].Surname}",age,playerList[i].Position,playerList[i].AbilityWeightSum,playerList[i].Price));
                }
                
                if(pItem)
                {
                    text = pItem.Find("name").GetComponent<TMPro.TMP_Text>();

                    if(eType == E_SCROLL_TYPE.bidding)
                    {
                        text.SetText(m_pBiddingSortPlayers[i].Name);

                        AuctionBiddingInfoT pAuctionBiddingInfo = pGameContext.GetAuctionBiddingInfoByIPlayerID(playerList[i].Id);
                        text = pItem.Find("time/text").GetComponent<TMPro.TMP_Text>();
                        text.transform.parent.gameObject.SetActive(pAuctionBiddingInfo.TExtend > 0);
                        if(pAuctionBiddingInfo.TExtend > 0)
                        {
                            pItem.Find("off").gameObject.SetActive(false);
                            pItem.Find("status").gameObject.SetActive(false);
                            text.SetText(ALFUtils.SecondToString((int)pAuctionBiddingInfo.TExtend,true,false,false));
                        }
                        else
                        {
                            pItem.Find("off").gameObject.SetActive(true);
                            text = pItem.Find("status").GetComponent<TMPro.TMP_Text>();
                            text.gameObject.SetActive(true);
                            text.SetText((pAuctionBiddingInfo.FinalGold == pAuctionBiddingInfo.Gold && pAuctionBiddingInfo.FinalToken == pAuctionBiddingInfo.Token) ? pGameContext.GetLocalizingText("TRANSFER_MARKET_TXT_WIN"):pGameContext.GetLocalizingText("TRANSFER_MARKET_TXT_LOST"));
                        }

                        text = pItem.Find("money/text").GetComponent<TMPro.TMP_Text>();
                        text.SetText(ALFUtils.NumberToString(pAuctionBiddingInfo.FinalGold));
                        text = pItem.Find("token/text").GetComponent<TMPro.TMP_Text>();
                        text.SetText(string.Format("{0:#,0}", pAuctionBiddingInfo.FinalToken));
                    }
                    else
                    {
                        text.SetText(m_pSortPlayers[i].Name);
                    }
                
                    icon = pItem.Find("nation").GetComponent<RawImage>();
                    pSprite = AFPool.GetItem<Sprite>("Texture",playerList[i].Nation);
                    icon.texture = pSprite.texture;

                    pItem.gameObject.name = playerList[i].Id.ToString();
                    
                    text = pItem.Find("age").GetComponent<TMPro.TMP_Text>();
                    text.SetText(age > 40 ? "40+":age.ToString());
                    
                    SingleFunc.SetupPlayerCard(playerList[i],pItem,E_ALIGN.Left,E_ALIGN.Left);
                    
                    if(eType == E_SCROLL_TYPE.recuiting)
                    {
                        text = pItem.Find("type/money/text").GetComponent<TMPro.TMP_Text>();
                        text.SetText(ALFUtils.NumberToString(playerList[i].Price));
                        if(playerList[i].Price > 0)
                        {
                            pItem.Find("type/free").gameObject.SetActive(false);
                            pItem.Find("type/money").gameObject.SetActive(true);
                        }
                        else
                        {
                            pItem.Find("type/free").gameObject.SetActive(true);
                            pItem.Find("type/money").gameObject.SetActive(false);
                        }
                        
                        UpdateScrollItemStatus(pItem,playerList[i]);
                        
                    }
                    else if(eType == E_SCROLL_TYPE.market)
                    {
                        text = pItem.Find("money/text").GetComponent<TMPro.TMP_Text>();
                        text.SetText(ALFUtils.NumberToString(pGameContext.GetAuctionInitialGoldByPlayerNo(playerList[i].Id)));
                    }                    
                    
                    pItem.SetParent(pScroll.content,false);
                    pItem.localScale = Vector3.one;
                    pItem.anchoredPosition = new Vector2(0,-h);
                
                    size = pItem.sizeDelta;
                    size.x = 0;
                    pItem.sizeDelta = size;
                    
                    h += pItem.rect.height;
                }
            }
        }
        
        if(eType == E_SCROLL_TYPE.scoutTickets)
        {
            List<ItemInfoT> pList = pGameContext.GetScoutTickets();
            for(n = 0; n < pList.Count; ++n)
            {
                pItem = LayoutManager.Instance.GetItem<RectTransform>(itemName);

                if(pItem)
                {
                    pItem.gameObject.name = pList[n].No.ToString();
                    icon = pItem.Find("icon").GetComponent<RawImage>();
                    pSprite = AFPool.GetItem<Sprite>("Texture",pItem.gameObject.name);
                    icon.texture = pSprite.texture;

                    text = pItem.Find("count").GetComponent<TMPro.TMP_Text>();
                    text.SetText(pList[n].Amount.ToString());
                    text = pItem.Find("name").GetComponent<TMPro.TMP_Text>();
                    text.SetText(pGameContext.GetLocalizingText($"item_name_{pList[n].No}"));

                    pItem.SetParent(pScroll.content,false);
                    
                    pItem.localScale = Vector3.one;       
                    pItem.anchoredPosition = new Vector2(0,-h);
                    
                    size = pItem.sizeDelta;
                    size.x = 0;
                    pItem.sizeDelta = size;
                    h += pItem.rect.height;
                }
            }
            
            pScroll.viewport.Find("text").gameObject.SetActive(pList.Count <= 0);
            size = pScroll.GetComponent<RectTransform>().sizeDelta;
            if(500 < h)
            {
                size.y = 750;
            }
            else
            {
                size.y = h + 250;
            }
            pScroll.GetComponent<RectTransform>().sizeDelta = size;
        }
        else if(eType != E_SCROLL_TYPE.recuiting)
        {
            if(eType == E_SCROLL_TYPE.bidding)
            {
                AuctionBiddingInfoT pAuctionBiddingInfo = null;
                for(n = 0; n < m_pBiddingSortPlayers.Count; ++n)
                {
                    pAuctionBiddingInfo = pGameContext.GetAuctionBiddingInfoByIPlayerID(m_pBiddingSortPlayers[n].Id);
                    if(pAuctionBiddingInfo != null)
                    {
                        m_pBiddingSortPlayers[n].Reward = pAuctionBiddingInfo.Reward;
                    }
                } 

                Transform tm = pScroll.transform.parent.Find("tap/mark");
                Vector3 scale = tm.localScale;
                scale.y = h == 0 ? -1 : 1;
                tm.localScale = scale;
            }
            else
            {
                ScrollRect my = m_pScrollList[(int)E_SCROLL_TYPE.bidding];
                // ShowMyBidding(my.content.childCount > 0);
                
                ShowMyBidding(!m_bFoldding);
            }
        }
        
        size = pScroll.content.sizeDelta;
        size.y = h;
        pScroll.content.sizeDelta = size;
        LayoutManager.SetReciveUIScollViewEvent(pScroll,ScrollViewItemButtonEventCall);
    }

    void AddBiddingScrollItem(AuctionBiddingInfoT pAuctionBiddingInfo)
    {
        GameContext pGameContext = GameContext.getCtx();
        ScrollRect pScroll = m_pScrollList[(int)E_SCROLL_TYPE.bidding];
        string itemName = E_SCROLL_ITEM.BiddingItem.ToString();
         
        RectTransform pItem = LayoutManager.Instance.GetItem<RectTransform>(itemName);
                
        if(pItem)
        {
            float h = pScroll.content.sizeDelta.y;
            byte age = pGameContext.GetPlayerAge(pAuctionBiddingInfo.Player);
            SortPlayer pSortPlayer = new SortPlayer(pItem,pAuctionBiddingInfo.Player.Id, pScroll.content.childCount, $"{pAuctionBiddingInfo.Player.Forename[0]}. {pAuctionBiddingInfo.Player.Surname}",age,pAuctionBiddingInfo.Player.Position,pAuctionBiddingInfo.Player.AbilityWeightSum,pAuctionBiddingInfo.Player.Price);
            m_pBiddingSortPlayers.Add(pSortPlayer);
            TMPro.TMP_Text text = pItem.Find("name").GetComponent<TMPro.TMP_Text>();
            text.SetText(m_pBiddingSortPlayers[m_pBiddingSortPlayers.Count -1].Name);

            text = pItem.Find("time/text").GetComponent<TMPro.TMP_Text>();
            text.transform.parent.gameObject.SetActive(pAuctionBiddingInfo.TExtend > 0);
            if(pAuctionBiddingInfo.TExtend > 0)
            {
                pSortPlayer.Reward = false;
                pItem.Find("off").gameObject.SetActive(false);
                pItem.Find("status").gameObject.SetActive(false);
                text.SetText(ALFUtils.SecondToString((int)pAuctionBiddingInfo.TExtend,true,false,false));
            }
            else
            {
                pSortPlayer.Reward = true;
                pItem.Find("off").gameObject.SetActive(true);
                text = pItem.Find("status").GetComponent<TMPro.TMP_Text>();
                text.gameObject.SetActive(true);
                text.SetText((pAuctionBiddingInfo.FinalGold == pAuctionBiddingInfo.Gold && pAuctionBiddingInfo.FinalToken == pAuctionBiddingInfo.Token) ? pGameContext.GetLocalizingText("TRANSFER_MARKET_TXT_WIN"):pGameContext.GetLocalizingText("TRANSFER_MARKET_TXT_LOST"));
            }
        
            RawImage icon = pItem.Find("nation").GetComponent<RawImage>();
            Sprite pSprite = AFPool.GetItem<Sprite>("Texture",pAuctionBiddingInfo.Player.Nation);
            icon.texture = pSprite.texture;

            pItem.gameObject.name = pAuctionBiddingInfo.Player.Id.ToString();
            text = pItem.Find("age").GetComponent<TMPro.TMP_Text>();
            text.SetText(age > 40 ? "40+":age.ToString());
            
            SingleFunc.SetupPlayerCard(pAuctionBiddingInfo.Player,pItem,E_ALIGN.Left,E_ALIGN.Left);
            
            text = pItem.Find("money/text").GetComponent<TMPro.TMP_Text>();
            text.SetText(ALFUtils.NumberToString(pAuctionBiddingInfo.FinalGold));
            text = pItem.Find("token/text").GetComponent<TMPro.TMP_Text>();
            text.SetText(string.Format("{0:#,0}", pAuctionBiddingInfo.FinalToken));
            
            pItem.SetParent(pScroll.content,false);
            pItem.localScale = Vector3.one;
            pItem.anchoredPosition = new Vector2(0,-h);
            Vector2 size = pItem.sizeDelta;
            size.x =0;
            pItem.sizeDelta = size;

            h += pItem.rect.height;

            size = pScroll.content.sizeDelta;
            size.y = h;
            pScroll.content.sizeDelta = size;
            LayoutManager.SetReciveUIScollViewEvent(pScroll,ScrollViewItemButtonEventCall);
        }
    }

    List<SortPlayer> SortList(E_SORT_TOKEN token, List<SortPlayer> list, bool bASC)
    {
        List<SortPlayer> temp = null;
        switch(token)
        {
            case E_SORT_TOKEN.contract:
            case E_SORT_TOKEN.value:
            {
                if(bASC)
                {
                    temp = list.OrderByDescending(x => x.Value).ToList();
                }
                else
                {
                    temp = list.OrderBy(x => x.Value).ToList();
                }
            }
            break;
            case E_SORT_TOKEN.profile:
            case E_SORT_TOKEN.quality:
            {
                if(bASC)
                {
                    temp = list.OrderByDescending(x => x.Quality).ToList();
                }
                else
                {
                    temp = list.OrderBy(x => x.Quality).ToList();
                }
            }
            break;
            case E_SORT_TOKEN.name:
            {
                if(bASC)
                {
                    temp = list.OrderByDescending(x => x.Name).ToList();
                }
                else
                {
                    temp = list.OrderBy(x => x.Name).ToList();
                }
            }
            break;
            case E_SORT_TOKEN.roles:
            {
                if(bASC)
                {
                    temp = list.OrderByDescending(x => x.Position).ToList();
                }
                else
                {
                    temp = list.OrderBy(x => x.Position).ToList();
                }
            }
            break;
            case E_SORT_TOKEN.age:
            {
                if(bASC)
                {
                    temp = list.OrderByDescending(x => x.Age).ToList();
                }
                else
                {
                    temp = list.OrderBy(x => x.Age).ToList();
                }                    
            }
            break;
        }

        return temp;
    }

    void Sort(E_SORT_TOKEN token, ref List<SortPlayer> list, ScrollRect root, bool bASC)
    {
        if(root.content.childCount > 1)
        {
            LayoutManager.SetReciveUIScollViewEvent(root,null);
            int i = 0;

            List<SortPlayer> sList = new List<SortPlayer>();
            List<SortPlayer> sList1 = new List<SortPlayer>();
            
            for( i =0; i < list.Count; ++i )
            {
                if(list[i].Reward)
                {
                    sList1.Add(list[i]);
                }
                else
                {
                    sList.Add(list[i]);
                }
            }

            list.Clear();
            sList = SortList(token, sList, bASC);
            for(i =0; i < sList.Count; ++i)
            {
                list.Add(sList[i]);
            }

            if(sList1.Count > 0)
            {
                sList1 = SortList(token, sList1, bASC);
                for(i =0; i < sList1.Count; ++i)
                {
                    list.Add(sList1[i]);
                }
            }

            float h = 0;
            for(i =0; i < list.Count; ++i)
            {
                list[i].Target.SetAsLastSibling();
                list[i].Target.anchoredPosition = new Vector2(0,-h);
                h += list[i].Target.rect.height;
            }
            LayoutManager.SetReciveUIScollViewEvent(root,ScrollViewItemButtonEventCall);
        }
    }

    void ShowMyBidding(bool bShow)
    {
        ScrollRect my = m_pScrollList[(int)E_SCROLL_TYPE.bidding];
        my.gameObject.SetActive(bShow);
         
        float limit_h = m_pTabUIList[(int)E_TAB.transfer_market].rect.height;
        
        if(!my.gameObject.activeSelf)
        {
            limit_h -= my.transform.parent.Find("tap").GetComponent<RectTransform>().rect.height;
            limit_h += my.transform.parent.GetComponent<RectTransform>().anchoredPosition.y;
        }
        else
        {
            limit_h += m_pTabUIList[(int)E_TAB.transfer_market].Find("bidding").GetComponent<RectTransform>().anchoredPosition.y;
            limit_h *= 0.5f;
        }

        RectTransform market = m_pScrollList[(int)E_SCROLL_TYPE.market].transform.parent.GetComponent<RectTransform>();
        
        Vector2 size = market.sizeDelta;
        size.y = limit_h;
        market.sizeDelta = size;
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(value.y >= 1)
        {
            Transform tm = m_pScrollList[(int)E_SCROLL_TYPE.market].transform.parent.Find("remaining");
            Vector3 pos = tm.position;
            pos.y = m_pScrollList[(int)E_SCROLL_TYPE.market].content.position.y;
            tm.position = pos;
        }
    }

    void RefreshRewardCallback(E_AD_STATUS eStatus, bool useSkipItem)//( GoogleMobileAds.Api.Reward reward)
    { 
        if(eStatus != E_AD_STATUS.RewardComplete) return;

        JObject pJObject = new JObject();
        pJObject["refresh"] = 20;
        pJObject["skip"]= useSkipItem ? 1:0;
        m_pMainScene.RequestAfterCall( m_eMainTab == E_TAB.recuiting ? E_REQUEST_ID.recruit_refresh: E_REQUEST_ID.youth_refresh,pJObject);
        EnableRefreshTimer(m_eMainTab,false);
        GameContext pGameContext = GameContext.getCtx();
        
        float tic = m_eMainTab == E_TAB.recuiting ? (float)pGameContext.GetConstValue(E_CONST_TYPE.transferRecruitRefreshAdsCooldown) : (float)pGameContext.GetYouthCooldownTime();
        pGameContext.SetAdRefreshTime((uint)m_eMainTab,tic);
        pGameContext.SetAdViewTime((uint)m_eMainTab);
        pGameContext.SaveUserData(true);
    }

    public void CurrentClearScroll()
    {
        for(int i =0; i < m_pScrollList.Length; ++i)
        {
            ClearScroll(m_pScrollList[i],((E_SCROLL_ITEM)i).ToString());
        }
    }

    public void Close()
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pScoutResult);
        for(int i =0; i < list.Count; ++i)
        {
            list[i].Exit(true);
        }

        m_eMainTab = E_TAB.MAX;
        m_pMainScene.HideMoveDilog(MainUI,Vector3.right);
        m_pScoutResult.transform.parent.gameObject.SetActive(false);
        m_pScoutResult.Stop();
        m_pScoutResult.gameObject.SetActive(false);
    }

    void Skip()
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pScoutResult);
        if(list.Count > 0)
        {
            if(list[0].StateData is ChangeStarStateData data)
            {
                int i = data.AnimationList.Count;
                while(i > 0)
                {
                    --i;
                    data.AnimationList[i].gameObject.SetActive(true);
                    data.AnimationList.RemoveAt(i);
                }
                
                Animation[] pAnimation = m_pScoutResult.transform.Find("root/tier").GetComponentsInChildren<Animation>(true);
                for(int n =0; n < pAnimation.Length; ++n)
                {
                    pAnimation[n]["star"].time = pAnimation[n]["star"].length;
                }
            }
        }
        
        if(m_pScoutResult.IsPlaying("scout_start"))
        {
            m_pScoutResult["scout_start"].time = m_pScoutResult["scout_start"].length;
        }
        
        if( !m_pScoutResult.IsPlaying("scout_end"))
        {
            m_pScoutResult.Play("scout_end");
        }
        m_pScoutResult["scout_end"].time = m_pScoutResult["scout_end"].length;
        ShowActiveStar();
        
        m_pSkipButton.gameObject.SetActive(false);
    }

    void ShowActiveStar()
    {
        Animation[] animationList = m_pScoutResult.transform.Find("root/tier").GetComponentsInChildren<Animation>(true);
                    
        for(int n =0; n < animationList.Length; ++n)
        {
            if(animationList[n].gameObject.activeSelf)
            {
                Graphic[] graphicList = animationList[n].GetComponentsInChildren<Graphic>(true);
                for(int i =0; i < graphicList.Length; ++i)
                {
                    graphicList[i].color = Color.white;
                }
            }
        }
    }

    public void UpdateTimer(float dt)
    {
        /**
        * 경매가 진행중에 에디터에서 브레이크 잡으면, 해당 시간만큼 오차가 발생한다. 서비스에서는 발생하지 않음.. 
        */
        if(!m_bRefreshUpdate) return;
        
        GameContext pGameContext = GameContext.getCtx();

        if(m_eMainTab != E_TAB.scout)
        {
            float tic = 0;
            
            if(m_eMainTab == E_TAB.transfer_market)
            {
                tic = pGameContext.GetExpireTimerByUI(this,1);
                UpdateRefreshTimeButton(m_eMainTab,tic);

                if(tic <= 0)
                {
                    m_bRefreshUpdate = false; 
                    m_pRemainingTimeTexts[0].SetText("00");
                    m_pRemainingTimeTexts[1].SetText("00");
                }
                else
                {
                    string[] strTimes = ALFUtils.SecondToString((int)tic,true,true,true).Split(':');
                    m_pRemainingTimeTexts[0].SetText(strTimes[strTimes.Length -2]);
                    m_pRemainingTimeTexts[1].SetText(strTimes[strTimes.Length -1]);

                    m_bRefreshUpdate = true; 
                    ScrollRect pScrollRect = m_pScrollList[(int)E_SCROLL_TYPE.bidding];
                    Transform tm = null;
                    AuctionBiddingInfoT pAuctionBiddingInfo = null;
                    TMPro.TMP_Text text = null;
                    bool bSort = false;
                    for(int i =0; i < m_pBiddingSortPlayers.Count; ++i)
                    {
                        if(m_pBiddingSortPlayers[i].Reward) continue;
                        
                        tm = m_pBiddingSortPlayers[i].Target;
                        text = tm.Find("time/text").GetComponent<TMPro.TMP_Text>();
                        
                        pAuctionBiddingInfo = pGameContext.GetAuctionBiddingInfoByIPlayerID(m_pBiddingSortPlayers[i].Id);
                        if( pAuctionBiddingInfo.TExtend > 0)
                        {
                            m_bRefreshUpdate = true;    
                            text.SetText(ALFUtils.SecondToString((int)pAuctionBiddingInfo.TExtend,true,true,false));
                        }
                        else
                        {
                            bSort = true;
                            m_pBiddingSortPlayers[i].Reward = true;
                            tm.Find("off").gameObject.SetActive(true);
                            text.transform.parent.gameObject.SetActive(false);
                            text = tm.Find("status").GetComponent<TMPro.TMP_Text>();
                            text.gameObject.SetActive(true);
                            text.SetText((pAuctionBiddingInfo.FinalGold == pAuctionBiddingInfo.Gold && pAuctionBiddingInfo.FinalToken == pAuctionBiddingInfo.Token) ? pGameContext.GetLocalizingText("TRANSFER_MARKET_TXT_WIN"):pGameContext.GetLocalizingText("TRANSFER_MARKET_TXT_LOST"));
                        }
                    }

                    if(bSort)
                    {
                        tm = m_pTabUIList[(int)E_TAB.transfer_market].Find($"tabs/{m_eSortToken}/mark");
                        Sort(m_eSortToken,ref m_pBiddingSortPlayers, m_pScrollList[(int)E_SCROLL_TYPE.bidding], tm.localScale.y > 0);
                    }
                }
            }
            else
            {
                tic = pGameContext.GetAdRefreshTime((uint)m_eMainTab);

                if(tic <= 0)
                {
                    m_bRefreshUpdate = false; 
                    UpdateRefreshTimeButton(m_eMainTab,tic);
                }
                else
                {
                    m_pRefreshText.SetText(ALFUtils.SecondToString((int)tic,true,true,true));
                }
            }
        }
    }
    void UpdateRefreshTimeButton(E_TAB eTyp,float tic)
    {
        if(eTyp == E_TAB.recuiting || eTyp == E_TAB.youth_promotion)
        {
            GameContext pGameContext = GameContext.getCtx();
            int iRefreshFreeCount = 0;
            int iCount = 0;
            if(eTyp == E_TAB.recuiting)
            {
                iRefreshFreeCount = pGameContext.GetRecruitRefreshFreeCachePlayerData();
                iCount = pGameContext.GetConstValue(E_CONST_TYPE.transferRecruitRefreshFreeChanceCount);
            }
            else
            {
                iRefreshFreeCount = pGameContext.GetYouthRefreshFreeCachePlayerData();
                iCount = pGameContext.GetConstValue(E_CONST_TYPE.transferYouthRefreshFreeChanceCount);
            }
            
            bool bActive = iRefreshFreeCount <= 0;

            EnableAdRefreshTimer(eTyp,bActive);
            m_pRefreshText.gameObject.SetActive(true);
            
            if(bActive)
            {   
                if(tic > 0)
                {
                    bActive = false;
                    m_bRefreshUpdate = true;
                    m_pRefreshText.SetText(ALFUtils.SecondToString((int)tic,true,true,true));
                }
                else
                {
                    m_pRefreshText.gameObject.SetActive( pGameContext.GetItemCountByNO(GameContext.AD_SKIP_ID) < 1);
                    m_pRefreshText.SetText(pGameContext.GetLocalizingText("TRANSFER_RECRUIT_BTN_CALL_UP_NOW"));
                }
            }
            else
            {
                m_pRefreshText.SetText(string.Format(pGameContext.GetLocalizingText("TRANSFER_RECRUIT_BTN_REFRESH_WITH_COUNT"),iRefreshFreeCount,iCount));
                bActive = true;
            }
            pGameContext.SaveUserData(true);
            EnableRefreshTimer(eTyp,bActive);
        }
        else if(eTyp == E_TAB.transfer_market)
        {
            if(tic <= 0)
            {
                m_pRemainingTimeTexts[0].SetText("00");
                m_pRemainingTimeTexts[1].SetText("00");
            }
            else
            {
                string[] strTimes = ALFUtils.SecondToString((int)tic,true,true,true).Split(':');
                m_pRemainingTimeTexts[0].SetText(strTimes[strTimes.Length -2]);
                m_pRemainingTimeTexts[1].SetText(strTimes[strTimes.Length -1]);
            }
        }
    }

    void UpdateScoutResult(ulong id)
    {
        GameContext pGameContext = GameContext.getCtx();
        PlayerT pPlayer = pGameContext.GetPlayerByID(id);
        Transform tm = m_pScoutResult.transform.Find("root/player");
        SingleFunc.SetupPlayerCard(pPlayer,tm);
        RectTransform tier = m_pScoutResult.transform.Find("root/tier").GetComponent<RectTransform>();
        SingleFunc.SetupQuality(pPlayer,tier,true);

        int n =0;
        Color color = Color.white;
        color.a = 0;
        for(n =0 ; n < tier.childCount; ++n)
        {
            tm = tier.GetChild(n);
            if(tm.gameObject.activeSelf)
            {
                if(tm.Find("h") != null)
                {
                    tm.Find("h").GetComponent<Graphic>().color = color;
                }
                if(tm.Find("on") != null)
                {
                    tm.Find("on").GetComponent<Graphic>().color = color;
                }
            }
        }
    
        tm = m_pScoutResult.transform.Find("root/name/nation");
        
        RawImage icon = tm.GetComponent<RawImage>();
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",pPlayer.Nation);
        icon.texture = pSprite.texture;
        TMPro.TMP_Text text = m_pScoutResult.transform.Find("root/name/text").GetComponent<TMPro.TMP_Text>();
        text.SetText($"{pPlayer.Forename} {pPlayer.Surname}");
        
        text = m_pScoutResult.transform.Find("root/age/text").GetComponent<TMPro.TMP_Text>();
        byte age= pGameContext.GetPlayerAge(pPlayer);
        text.SetText(age > 40 ? "40+":age.ToString());
        
        text = m_pScoutResult.transform.Find("root/value/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(ALFUtils.NumberToString(pPlayer.Price));
        
        tm = m_pScoutResult.transform.Find("root/roles/roles");
        
        
        for(n = 0; n < tm.childCount; ++n)
        {
            tm.GetChild(n).gameObject.SetActive(false);
        }
        
        List<string> locList= new List<string>();
        
        for(n = 0; n < pPlayer.PositionFamiliars.Count; ++n)
        {
            if(pPlayer.PositionFamiliars[n] >= 80)
            {
                locList.Add(pGameContext.GetDisplayLocationName(n));
            }
        }
        Transform item = null;
        for(n = 0; n < locList.Count; ++n)
        {
            if(tm.childCount > n)
            {
                item = tm.GetChild(n);
                icon = item.GetComponent<RawImage>();
                icon.gameObject.SetActive(true);
                pSprite = AFPool.GetItem<Sprite>("Texture",pGameContext.GetDisplayCardFormationByLocationName(locList[n]));
                icon.texture = pSprite.texture;
                text = icon.transform.Find("text").GetComponent<TMPro.TMP_Text>();
                text.SetText(locList[n]);
            }
        }
    }

    void ShowToastPlayerCard(PlayerT player)
    {
        RectTransform card = LayoutManager.Instance.GetItem<RectTransform>("PlayerCard");
        card.gameObject.name = "PlayerCard";
        SingleFunc.SetupPlayerCard(player,card,E_ALIGN.Left);
        Vector2 pos = card.anchoredPosition;
        pos.y = card.rect.height * 0.7f;
        card.anchoredPosition = pos;
        m_pMainScene.ShowToastMessage(string.Format(GameContext.getCtx().GetLocalizingText("ALERT_TXT_PLAYER_CONTRACT"),$"{player.Forename} {player.Surname}"),card);
    }

    public void SendTutorial()
    {
        if(m_eMainTab == E_TAB.recuiting)
        {
            List<PlayerT> pRecruitPlayerList = GameContext.getCtx().GetRecuitingCachePlayerData();
            PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
            pPlayerInfo.SetupPlayerInfoData(E_PLAYER_INFO_TYPE.recuiting,pRecruitPlayerList[m_pSortPlayers[1].Index]);
            pPlayerInfo.SetupQuickPlayerInfoData(null);
            pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.offer);
        }        
    }
    
    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        ulong id = 0;
        if(ulong.TryParse(tm.gameObject.name,out id))
        {   
            GameContext pGameContext = GameContext.getCtx();
            E_PLAYER_INFO_TYPE eType = E_PLAYER_INFO_TYPE.recuiting;
            PlayerT pPlayer = null;
            if(m_eMainTab == E_TAB.transfer_market)
            {
                eType = E_PLAYER_INFO_TYPE.auction;
                if(root == m_pScrollList[(int)E_SCROLL_TYPE.bidding])
                {
                    eType = E_PLAYER_INFO_TYPE.bidding;
                }
            }

            pPlayer = pGameContext.GetCachePlayerDataByNo(id,eType);

            if(pPlayer != null)
            {
                PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();        
                pPlayerInfo.SetupPlayerInfoData(eType,pPlayer);
                pPlayerInfo.SetupQuickPlayerInfoData(m_eMainTab == E_TAB.recuiting ? pGameContext.GetRecuitingCachePlayerData():null);
                pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.offer);
            }

        }
    }
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        GameContext pGameContext = GameContext.getCtx();

        switch(root.gameObject.name)
        {
            case "tip":
            {
                m_pMainScene.ShowGameTip("game_tip_transfer_title");
            }
            break;
            case "tabs":
            {
                bool bRequest = false;

                E_TAB eType = (E_TAB)Enum.Parse(typeof(E_TAB), sender.name);
                E_REQUEST_ID eReq = E_REQUEST_ID.auction_get;
                if(eType != E_TAB.scout)
                {
                    if(eType == E_TAB.recuiting )
                    {
                        eReq = E_REQUEST_ID.recruit_get;
                        bRequest = pGameContext.IsExpireRecuitingCachePlayerData();
                    }
                    else if(eType == E_TAB.youth_promotion)
                    {
                        if(sender.transform.Find("lock").gameObject.activeSelf)
                        {
                            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_LOCKED_TRANSFER_YOUTH"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                            return;
                        }
                        eReq = E_REQUEST_ID.youth_get;
                        bRequest = pGameContext.IsExpireYouthCachePlayerData();
                    }
                    else
                    {
                        m_bRequestAuctionGet = true;
                        bRequest = pGameContext.IsExpireAuctionInfoData();
                    }
                }
                
                if(!bRequest)
                {
                    ShowTabUI((byte)eType);
                }
                else
                {
                    SendRequest(eReq,eType);
                }
            }
            break;
            case "scoutResult":
            {
                switch(sender.name)
                {
                    case "skip":
                    {
                        Skip();
                    }
                    break;
                    case "confirm":
                    {
                        m_pScoutResult.transform.parent.gameObject.SetActive(false);
                        m_pScoutResult.gameObject.SetActive(false);
                    }
                    break;
                    case "info":
                    {
                        PlayerT pPlayer = pGameContext.GetPlayerByID(m_iScoutPlayerID);
                        if(pPlayer != null)
                        {
                            PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
                            pPlayerInfo.SetupPlayerInfoData(E_PLAYER_INFO_TYPE.my,pPlayer);
                            pPlayerInfo.SetupQuickPlayerInfoData(pGameContext.GetTotalPlayerList());
                            pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
                        }
                    }
                    break;
                }
            }
            break;
            case "scout":
            {
                switch(sender.name)
                {
                    case "left":
                    {
                        LocalizingText text = sender.transform.parent.Find("text").GetComponent<LocalizingText>();
                        
                        E_TEXT_NAME eType = (E_TEXT_NAME)Enum.Parse(typeof(E_TEXT_NAME), text.Key);
                        if(eType == E_TEXT_NAME.TRANSFER_SCOUT_TXT_POSITION_GENERAL)
                        {
                            eType = E_TEXT_NAME.TRANSFER_SCOUT_TXT_POSITION_FW;
                        }
                        else
                        {
                            --eType;
                        }
                        uint id = GameContext.SCOUT_TICKET_ID_START + (uint)eType;
                        for(int i = 0; i < m_pScoutSlotList.Length; ++i)
                        {
                            if(m_pScoutSlotList[i] == sender.transform.parent.parent)
                            {
                                break;
                            }
                            id +=10;
                        }
                        UpdateScoutSlotItem(eType,sender.transform.parent.parent,id);
                    }
                    break;
                    case "right":
                    {
                        LocalizingText text = sender.transform.parent.Find("text").GetComponent<LocalizingText>();
                        
                        E_TEXT_NAME eType = (E_TEXT_NAME)Enum.Parse(typeof(E_TEXT_NAME), text.Key);
                        if(eType == E_TEXT_NAME.TRANSFER_SCOUT_TXT_POSITION_FW)
                        {
                            eType = E_TEXT_NAME.TRANSFER_SCOUT_TXT_POSITION_GENERAL;
                        }
                        else
                        {
                            ++eType;
                        }
                        uint id = GameContext.SCOUT_TICKET_ID_START + (uint)eType;
                        for(int i = 0; i < m_pScoutSlotList.Length; ++i)
                        {
                            if(m_pScoutSlotList[i] == sender.transform.parent.parent)
                            {
                                break;
                            }
                            id +=10;
                        }
                        UpdateScoutSlotItem(eType,sender.transform.parent.parent,id);
                    }
                    break;
                    case "tickets":
                    {
                        m_pScoutTickets.gameObject.SetActive(!m_pScoutTickets.gameObject.activeSelf);
                    }
                    break;
                    case "buy":
                    {
                        m_iScoutPlayerID =0;
                        m_pScoutTickets.gameObject.SetActive(false);

                        if(!pGameContext.IsMaxPlayerCount())
                        {
                            int scout =0;
                            for(int i = 0; i < m_pScoutSlotList.Length; ++i)
                            {
                                if(m_pScoutSlotList[i] == sender.transform.parent)
                                {
                                    break;
                                }
                                scout += 10;
                            }
                            LocalizingText text = sender.transform.parent.Find("formation/text").GetComponent<LocalizingText>();
                            E_TEXT_NAME eType = (E_TEXT_NAME)Enum.Parse(typeof(E_TEXT_NAME), text.Key);
                            JObject pJObject = new JObject();
                            pJObject["no"] = GameContext.SCOUT_TICKET_ID_START + (uint)scout;
                            pJObject["scout"]=scout;
                            pJObject["position"]=(byte)eType;
                            m_pMainScene.RequestAfterCall(E_REQUEST_ID.scout_reward,pJObject);
                        }
                        else
                        {
                            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("EXPANDSQUAD_TXT_EXPAND_DESC"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                        }
                    }
                    break;
                }
            }
            break;
            case "transfer_market":
            {
                switch(sender.name)
                {
                    case "tap":
                    {
                        Transform tm = sender.transform.Find("mark");
                        Vector3 scale = tm.localScale;
                        scale.y *= -1;
                        tm.localScale = scale;
                        m_bFoldding = !m_bFoldding;
                        ShowMyBidding(!m_bFoldding);
                    }
                    break;
                    case "profile":
                    case "roles":
                    case "age":
                    case "value":
                    {
                        E_MARKET eSelectTab = (E_MARKET)Enum.Parse(typeof(E_MARKET), sender.name);
                        Transform tm = sender.transform.Find("mark");
                        m_eSortToken = (E_SORT_TOKEN)Enum.Parse(typeof(E_SORT_TOKEN), sender.name);
                        if(tm.gameObject.activeSelf)
                        {
                            Vector3 scale = tm.localScale;
                            scale.y *= -1;
                            tm.localScale = scale;
                            Sort(m_eSortToken,ref m_pBiddingSortPlayers, m_pScrollList[(int)E_SCROLL_TYPE.bidding], tm.localScale.y > 0);
                            Sort(m_eSortToken,ref m_pSortPlayers,m_pScrollList[(int)E_SCROLL_TYPE.market], tm.localScale.y > 0);
                        }
                        else
                        {
                            ShowSubTabUI(E_TAB.transfer_market,(byte)eSelectTab);
                            Sort(m_eSortToken,ref m_pBiddingSortPlayers,m_pScrollList[(int)E_SCROLL_TYPE.bidding], true);
                            Sort(m_eSortToken,ref m_pSortPlayers,m_pScrollList[(int)E_SCROLL_TYPE.market], true);
                        }
                    }
                    break;
                }
            }
            break;
            case "recuiting":
            {
                switch(sender.name)
                {
                    case "name":
                    case "roles":
                    case "age":
                    case "quality":
                    case "contract":
                    {
                        E_RECUITING eSelectTab = (E_RECUITING)Enum.Parse(typeof(E_RECUITING), sender.name);
                        m_eSortToken = (E_SORT_TOKEN)Enum.Parse(typeof(E_SORT_TOKEN), sender.name);
                        Transform tm = sender.transform.Find("mark");
                        if(tm.gameObject.activeSelf)
                        {
                            Vector3 scale = tm.localScale;
                            scale.y *= -1;
                            tm.localScale = scale;
                            Sort(m_eSortToken,ref m_pSortPlayers,m_pScrollList[2], tm.localScale.y > 0);
                        }
                        else
                        {
                            ShowSubTabUI(E_TAB.recuiting,(byte)eSelectTab);
                            Sort(m_eSortToken,ref m_pSortPlayers,m_pScrollList[2], true);
                        }
                    }
                    break;
                }
            }
            break;
            case "youth_promotion":
            {
                for(int n =0; n < m_pYouthSlotList.Length; ++n)
                {
                    if(m_pYouthSlotList[n].gameObject == sender.transform.parent.parent.gameObject)
                    {
                        PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
                        uint count = pGameContext.GetYouthSlotCount();
                        List<PlayerT> pYouthPlayerList = pGameContext.GetYouthPromotionCachePlayerData().ToList();
                        while(pYouthPlayerList.Count > count)
                        {
                            pYouthPlayerList.RemoveAt(pYouthPlayerList.Count -1);
                        }
                        
                        pPlayerInfo.SetupPlayerInfoData(E_PLAYER_INFO_TYPE.youth,pYouthPlayerList[n]);
                        pPlayerInfo.SetupQuickPlayerInfoData(pYouthPlayerList);
                
                        pPlayerInfo.ShowTabUI(sender.name == "player" ? E_PLAYER_INFO_TAB.information : E_PLAYER_INFO_TAB.offer);
                        break;
                    }
                }
            }
            break;
            case "item":
            {
                if(sender.name == "refresh")
                {
                    int count = 0;
                    E_REQUEST_ID eID = E_REQUEST_ID.recruit_refresh;
                    if(m_eMainTab == E_TAB.recuiting )
                    {
                        count = pGameContext.GetRecruitRefreshFreeCachePlayerData();
                    }
                    else if(m_eMainTab == E_TAB.youth_promotion)
                    {
                        eID = E_REQUEST_ID.youth_refresh;
                        count = pGameContext.GetYouthRefreshFreeCachePlayerData();
                    }
                    
                    if(count > 0)
                    {
                        JObject pJObject = new JObject();
                        pJObject["refresh"] = 10;
                        m_pMainScene.RequestAfterCall(eID,pJObject);
                    }
                    else
                    {
                        if(pGameContext.GetItemCountByNO(GameContext.AD_SKIP_ID) > 0)
                        {
                            RefreshRewardCallback(E_AD_STATUS.RewardComplete,true);
                        }
                        else
                        {
                            if(!pGameContext.ShowRewardVideo((E_AD_STATUS eStatus)=>{ RefreshRewardCallback(eStatus,false);} ))
                            {
                                m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_AD_NOT_PREPARED"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                            }
                        }
                    }
                }
                else
                {
                    m_pMainScene.ShowItemTipPopup(sender);
                }
            }
            break;
            default:
            {
                if(sender.name == "back")
                {
                    Close();
                    m_pMainScene.ResetMainButton();
                }
            }
            break;
        }
    }

    void SetupRefreshUI( E_TAB eType)
    {
        if(eType == E_TAB.recuiting)
        {
            RectTransform pItem = null;
            ScrollRect pScroll = m_pScrollList[(int)E_SCROLL_TYPE.recuiting];
            for(int i =0; i < pScroll.content.childCount; ++i)
            {
                pItem = pScroll.content.GetChild(i).GetComponent<RectTransform>();
                ALFUtils.FadeObject(pItem, -1);
            }
        }
        else if(eType == E_TAB.youth_promotion)
        {
            RectTransform pItem = null;
            GameContext pGameContext = GameContext.getCtx();
            uint count = pGameContext.GetYouthSlotCount();
                    
            for(int i = 0; i < m_pYouthSlotList.Length; ++i)
            {
                if(!m_pYouthSlotList[i].Find("empty").gameObject.activeSelf)
                {
                    pItem = m_pYouthSlotList[i].GetComponent<RectTransform>();
                    ALFUtils.FadeObject(pItem, -1);
                }
            }
        }
    }

    void enterRefreshScroll(IState state)
    {
        ShowTabUI((byte)m_eMainTab);
        SetupRefreshUI(m_eMainTab);
        LayoutManager.Instance.InteractableDisableAll();
        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
        ScrollMoveStateData pData = new ScrollMoveStateData();
        pData.ScrollSelectIndex = 0;

        if(m_eMainTab == E_TAB.recuiting)
        {
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            Transform tm = target.GetMainTarget<Transform>();
            pData.ScrollSelectNode = tm.GetChild(0);
            RectTransform item = pData.ScrollSelectNode.GetComponent<RectTransform>();
            Vector3 pos = item.anchoredPosition;
            pos.y -= item.rect.height;
            pData.Original = pos;
            pData.Distance = item.rect.height;
            pData.Direction = Vector2.up;
            condition.SetRemainTime(1 / (float)tm.childCount);
        }
        else
        {
            pData.ScrollSelectNode = m_pYouthSlotList[0];
            RectTransform item = pData.ScrollSelectNode.GetComponent<RectTransform>();
            Vector3 pos = item.anchoredPosition;
            pos.y -= item.rect.height;
            pData.Original = pos;
            pData.Distance = item.rect.height;
            pData.Direction = Vector2.up;

            condition.SetRemainTime(1.0f / m_pYouthSlotList.Length);
        }
        state.StateData = pData;
    }

    bool executeRefreshScroll(IState state,float dt,bool bEnd)
    {
        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();

        if( state.StateData is ScrollMoveStateData data)
        {
            int index = data.ScrollSelectIndex;
            RectTransform item = data.ScrollSelectNode.GetComponent<RectTransform>();
            ALFUtils.FadeObject(item , condition.GetTimePercent());

            item.anchoredPosition = data.Original + (data.Direction * data.Distance * condition.GetTimePercent());

            if(m_eMainTab == E_TAB.recuiting)
            {
                if(bEnd)
                {
                    BaseStateTarget target = state.GetTarget<BaseStateTarget>();
                    Transform tm = target.GetMainTarget<Transform>();
                
                    ++index;
                    if(index < tm.childCount)
                    {
                        condition.Reset();
                        condition.SetRemainTime(1 / (float)tm.childCount);
                        data.ScrollSelectNode = tm.GetChild(index);
                        item = data.ScrollSelectNode.GetComponent<RectTransform>();
                        Vector3 pos = item.anchoredPosition;
                        pos.y -= item.rect.height;
                        data.Original = pos;
                        data.ScrollSelectIndex = index;
                        bEnd = false;
                    }
                }
            }
            else if(m_eMainTab == E_TAB.youth_promotion)
            {
                if(bEnd)
                {
                    ++index;
                    while(index < m_pYouthSlotList.Length)
                    {
                        if(!m_pYouthSlotList[index].Find("empty").gameObject.activeSelf)
                        {
                            bEnd = false;
                            condition.Reset();
                            condition.SetRemainTime(1.0f / m_pYouthSlotList.Length);
                            data.ScrollSelectNode = m_pYouthSlotList[index];
                            item = data.ScrollSelectNode.GetComponent<RectTransform>();
                            Vector3 pos = item.anchoredPosition;
                            pos.y -= item.rect.height;
                            data.Original = pos;
                            data.ScrollSelectIndex = index;
                            break;
                        }
                        else
                        {
                            ++index;
                        }
                    }
                }
            }
        }
        
        return bEnd;
    }

    bool executeChangeStarCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is ChangeStarStateData data)
        {
            if(!data.CurrentAnimation.IsPlaying(data.CurrentAnimation.clip.name))
            {
                if(data.AnimationList.Count == 0)
                {
                    return true;
                }

                data.CurrentAnimation = data.AnimationList[0];
                data.CurrentAnimation.gameObject.SetActive(true);
                data.AnimationList.RemoveAt(0);
                
                if(data.AnimationList.Count == 0)
                {
                    data.CurrentAnimation.Play("scout_end");
                    ShowActiveStar();
                }
                else
                {
                    if(data.CurrentAnimation != m_pScoutResult)
                    {
                        if(data.CurrentAnimation["star"].speed == 2f)
                        {
                            SoundManager.Instance.PlaySFX("sfx_scout_star", 0.5f);
                        }
                        else
                        {
                            SoundManager.Instance.PlaySFX("sfx_scout_star2", 0.7f);
                        }
                    }
                    data.CurrentAnimation.Play();
                }
            }
        }

        return bEnd;
    }

    IState exitRefreshScroll(IState state)
    {
        LayoutManager.Instance.InteractableEnabledAll();
        return null;
    }

    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
        if(data == null) return;
        
        E_REQUEST_ID eReq = (E_REQUEST_ID)data.Id;
        switch(eReq)
        {
            case E_REQUEST_ID.auction_get:
            {
                float fTimeRefreshTime = (float)(ALF.NETWORK.NetworkManager.ConvertLocalGameTimeTick(data.Json["data"]["tExpire"].ToString()) - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond;
                Debug.Log($"E_REQUEST_ID.auction_get------------------fTimeRefreshTime:{fTimeRefreshTime}");
                string[] strTimes = ALFUtils.SecondToString((int)fTimeRefreshTime,true,true,true).Split(':');
                m_pRemainingTimeTexts[0].SetText(strTimes[strTimes.Length -2]);
                m_pRemainingTimeTexts[1].SetText(strTimes[strTimes.Length -1]);
                
                if(fTimeRefreshTime <= 0)
                {
                    m_bRequestAuctionGet = true;
                    /**
                    * 콜벡이 등록되어 있으면 해제한다.
                    */
                    m_pMainScene.RemoveRequestAfterCallback(eReq);
                    SendRequest(eReq,m_eMainTab == E_TAB.transfer_market ? m_eMainTab : E_TAB.MAX);
                    return;
                }
                GameContext.getCtx().AddExpireTimer(this,1,fTimeRefreshTime);
                if(!MainUI.gameObject.activeSelf) return;
                m_bRefreshUpdate = true;   
            }
            break;
            case E_REQUEST_ID.scout_reward:
            {
                if(!MainUI.gameObject.activeSelf) return;

                if(data.Json.ContainsKey("players"))
                {
                    GameContext pGameContext = GameContext.getCtx();
                    JArray pArray = (JArray)data.Json["players"];
                    JObject item = null;
                    
                    for(int i= 0; i < pArray.Count; ++i)
                    {
                        item = (JObject)pArray[i];
                        m_iScoutPlayerID = (ulong)item["id"];
                        UpdateScoutResult(m_iScoutPlayerID);
                    }
                    pGameContext.SendReqClubLicensePut(false);
                    
                    E_TEXT_NAME eTextName = E_TEXT_NAME.MAX;
                    LocalizingText text = null;
                    uint id = GameContext.SCOUT_TICKET_ID_START;
                    for(int i = 0; i < m_pScoutSlotList.Length; ++i)
                    {
                        text = m_pScoutSlotList[i].Find("formation/text").GetComponent<LocalizingText>();
                        eTextName = (E_TEXT_NAME)Enum.Parse(typeof(E_TEXT_NAME), text.Key);
                        UpdateScoutSlotItem(eTextName,m_pScoutSlotList[i],id + (uint)eTextName);
                        id +=10;
                    }

                    ClearScroll(m_pScrollList[(int)E_SCROLL_TYPE.scoutTickets],E_SCROLL_ITEM.ScoutTicketItem.ToString());
                    SetupScroll(E_SCROLL_TYPE.scoutTickets);
                }
                
                m_pScoutResult.transform.parent.gameObject.SetActive(true);
                m_pScoutResult.transform.parent.SetAsLastSibling();
                m_pScoutResult.gameObject.SetActive(true);
                m_pScoutResult.Play();
                
                ChangeStarStateData pChangeStarStateData = new ChangeStarStateData();
                Animation[] list = m_pScoutResult.transform.Find("root/tier").GetComponentsInChildren<Animation>(true);
                int count = 0;
                for(int n =0; n < list.Length; ++n)
                {
                    if(list[n].gameObject.activeSelf)
                    {
                        ++count;
                        list[n]["star"].speed = count > 4 ? 1f - (count - 4) * 0.1f : 2f;
                        pChangeStarStateData.AnimationList.Add(list[n]);
                        list[n].gameObject.SetActive(false);
                    }
                }

                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pScoutResult),-1, (uint)E_STATE_TYPE.Timer, null, executeChangeStarCallback);
                
                pChangeStarStateData.CurrentAnimation = m_pScoutResult;
                pChangeStarStateData.AnimationList.Add(m_pScoutResult);
                pBaseState.StateData = pChangeStarStateData;
                StateMachine.GetStateMachine().AddState(pBaseState);
            }
            break;
            case E_REQUEST_ID.recruit_refresh:
            case E_REQUEST_ID.youth_refresh:
            case E_REQUEST_ID.recruit_get:
            case E_REQUEST_ID.tutorial_getRecruit:
            case E_REQUEST_ID.youth_get:
            {
                GameContext.getCtx().AddExpireTimer(this,0,((float)(ALF.NETWORK.NetworkManager.ConvertLocalGameTimeTick(data.Json["tExpire"].ToString()) - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond));
                E_REQUEST_ID eID = (E_REQUEST_ID)data.Id;
                BaseStateTarget pBaseStateTarget = null;
                if(m_eMainTab == E_TAB.recuiting && (eID == E_REQUEST_ID.recruit_refresh || eID == E_REQUEST_ID.recruit_get || eID == E_REQUEST_ID.tutorial_getRecruit))
                {
                    pBaseStateTarget = new BaseStateTarget(m_pScrollList[(int)E_SCROLL_TYPE.recuiting].content);
                }
                else if(m_eMainTab == E_TAB.youth_promotion && (eID == E_REQUEST_ID.youth_refresh || eID == E_REQUEST_ID.youth_get ))
                {
                    pBaseStateTarget = new BaseStateTarget(m_pTabUIList[(int)E_TAB.youth_promotion]);
                }
                if(pBaseStateTarget != null)
                {
                    BaseState pBaseState = BaseState.GetInstance(pBaseStateTarget,-1f, (uint)E_STATE_TYPE.ShowDailog, enterRefreshScroll, executeRefreshScroll,exitRefreshScroll);
                    StateMachine.GetStateMachine().AddState(pBaseState);
                }
            }
            break;
            case E_REQUEST_ID.recruit_offer:
            case E_REQUEST_ID.tutorial_recruit:
            {
                if(data.Json.ContainsKey("oldId"))
                {
                    int n = 0;
                    ulong oldId = (ulong)data.Json["oldId"];

                    for( n =0; n < m_pSortPlayers.Count; ++n)
                    {
                        if(m_pSortPlayers[n].Id == oldId)
                        {
                            PlayerT pPlayer = GameContext.getCtx().GetCachePlayerDataByNo(oldId,E_PLAYER_INFO_TYPE.recuiting);
                            pPlayer.Status = 19;
                            ShowToastPlayerCard(pPlayer);
                            UpdateScrollItemStatus(m_pSortPlayers[n].Target,pPlayer);
                            return;
                        }
                    }
                }
            }
            break;
            case E_REQUEST_ID.youth_offer:
            {
                if(data.Json.ContainsKey("oldId"))
                {
                    UpdateYouthNominationCount();
                    int n = 0;
                    ulong oldId = (ulong)data.Json["oldId"];
                    List<PlayerT> pYouthPlayerList = GameContext.getCtx().GetYouthPromotionCachePlayerData();
                    for( n =0; n < pYouthPlayerList.Count; ++n)
                    {
                        if(pYouthPlayerList[n].Id == oldId)
                        {
                            pYouthPlayerList[n].Status = E_STATUS_OFFER;
                            ShowToastPlayerCard(pYouthPlayerList[n]);
                            UpdateYouthPlayerData(m_pYouthSlotList[n].Find("info"),pYouthPlayerList[n]);                            
                            return;
                        }
                    }
                }
            }
            break;
        }
    }

    public void SoketProcessor(NetworkData data)
    {
        E_SOCKET_ID eId = (E_SOCKET_ID)((uint)data.Json["msgId"]);

        GameContext pGameContext = GameContext.getCtx();
        if(eId == E_SOCKET_ID.auctionBidBroadcast)
        {
            bool bAdd = false;
            AuctionBiddingInfoT pAuctionBiddingInfo = pGameContext.UpdateBidBroadcast(data.Json,ref bAdd);
            
            if(pAuctionBiddingInfo == null)
            {
                if(data.Json.ContainsKey("payload") && data.Json["payload"].Type != JTokenType.Null)
                {
                    JObject payload = (JObject)data.Json["payload"];
                    uint auctionId = (uint)payload["auctionId"];
                    for(int i =0; i < m_pSortPlayers.Count; ++i)
                    {
                        if(m_pSortPlayers[i].Id == auctionId)
                        {
                            RectTransform item = m_pSortPlayers[i].Target;
                            TMPro.TMP_Text text = item.Find("money/text").GetComponent<TMPro.TMP_Text>();
                            text.SetText(string.Format("{0:#,0}", (ulong)payload["finalGold"]));
                            break;
                        }
                    }
                }
            }
            else
            {
                SoundManager.Instance.PlaySFX("sfx_auction_bid");
                if(bAdd)
                {
                    for(int i =0; i < m_pSortPlayers.Count; ++i)
                    {
                        if(m_pSortPlayers[i].Id == pAuctionBiddingInfo.Player.Id)
                        {
                            RectTransform item = m_pSortPlayers[i].Target;
                            float h = item.sizeDelta.y;
                            
                            item.Find("nation").GetComponent<RawImage>().texture = null;

                            Transform tm = item.Find("roles");
                            for(int n = 0; n < tm.childCount; ++n)
                            {
                                tm.GetChild(n).GetComponent<RawImage>().texture = null;
                            }

                            LayoutManager.Instance.AddItem(E_SCROLL_ITEM.MarketItem.ToString(),item);
                            Vector2 size = m_pScrollList[(int)E_SCROLL_TYPE.market].content.sizeDelta;
                            size.y -=h;
                            m_pScrollList[(int)E_SCROLL_TYPE.market].content.sizeDelta = size;

                            tm = m_pTabUIList[(int)E_TAB.transfer_market].Find($"tabs/{m_eSortToken}/mark");
                            m_pSortPlayers[i].Dispose();
                            m_pSortPlayers.RemoveAt(i);
                            AddBiddingScrollItem(pAuctionBiddingInfo);

                            Sort(m_eSortToken,ref m_pBiddingSortPlayers, m_pScrollList[(int)E_SCROLL_TYPE.bidding], tm.localScale.y > 0);
                            Sort(m_eSortToken,ref m_pSortPlayers,m_pScrollList[(int)E_SCROLL_TYPE.market], tm.localScale.y > 0);

                            if(m_bFoldding)
                            {
                                tm = m_pScrollList[(int)E_SCROLL_TYPE.bidding].transform.parent.Find("tap/mark");
                                Vector3 scale = tm.localScale;
                                scale.y = 1;
                                tm.localScale = scale;
                                m_bFoldding = false;
                                ShowMyBidding(true);
                            }

                            break;
                        }
                    }
                }
                else
                {
                    for(int i =0; i < m_pBiddingSortPlayers.Count; ++i)
                    {
                        if(m_pBiddingSortPlayers[i].Id == pAuctionBiddingInfo.Player.Id)
                        {
                            Transform tm = m_pScrollList[(int)E_SCROLL_TYPE.bidding].content.GetChild(i);
                            TMPro.TMP_Text text = tm.Find("money/text").GetComponent<TMPro.TMP_Text>();
                            text.SetText(ALFUtils.NumberToString(pAuctionBiddingInfo.FinalGold));
                            text = tm.Find("token/text").GetComponent<TMPro.TMP_Text>();
                            text.SetText(string.Format("{0:#,0}", pAuctionBiddingInfo.FinalToken));
                            break;
                        }
                    }
                }
            }
        }
        else if(eId == E_SOCKET_ID.auctionJoin)
        {
            if(data.Json.ContainsKey("payload") && data.Json["payload"].Type != JTokenType.Null)
            {
                JObject payload = (JObject)data.Json["payload"];
                uint auctionId = (uint)payload["auctionId"];
                AuctionBiddingInfoT pAuctionBiddingInfo = pGameContext.GetAuctionBiddingInfoByID(auctionId);
                if(pAuctionBiddingInfo != null)
                {
                    for(int i =0; i < m_pBiddingSortPlayers.Count; ++i)
                    {
                        if(m_pBiddingSortPlayers[i].Id == pAuctionBiddingInfo.Player.Id)
                        {
                            Transform tm = m_pScrollList[(int)E_SCROLL_TYPE.bidding].content.GetChild(i);
                            TMPro.TMP_Text text = tm.Find("money/text").GetComponent<TMPro.TMP_Text>();
                            text.SetText(ALFUtils.NumberToString(pAuctionBiddingInfo.FinalGold));
                            text = tm.Find("token/text").GetComponent<TMPro.TMP_Text>();
                            text.SetText(string.Format("{0:#,0}", pAuctionBiddingInfo.FinalToken));
                            break;
                        }
                    }
                }
            }
        }
    }
}
