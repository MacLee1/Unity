using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;

using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
using TRAININGCOST;
// using UnityEngine.EventSystems;
using STATEDATA;
using Newtonsoft.Json.Linq;
using PLAYERNATIONALITY;
using CONSTVALUE;
using PLAYERABILITYWEIGHTCONVERSION;
using ALF.NETWORK;
using UnityEngine.UI.Extensions;
using PLAYERPOTENTIALWEIGHTSUM;

public class PlayerInfo : ITimer, IBaseNetwork,ISoketNetwork
{
    const float HIDE_BUTTON_TIME = 5;
    const byte AUCTION_REGISTER_TIER = 5;
    readonly string[] SCROLL_ITEM_NAME = new string[]{"RecordItem","LeagueRecordItem","QuickPlayerItem"};

    enum E_EMOJI{ NONE=0,smile=10,thinking=20,sad=30,angry=40 }
    enum E_TAB{ ladder,league,quick }

    class QuickPlayerItem : IBase
    {
        public ulong Id {get; private set;}
        
        public TMPro.TMP_Text PlayerName {get; private set;}
        public TMPro.TMP_Text FormationText {get; private set;}

        public RawImage Icon  {get; private set;}
        public Transform Quality  {get; private set;}
        
        public GameObject On {get; private set;}

        string m_pItemName = null;

        public RectTransform Target  {get; private set;}
        
        Button m_pButton = null;

        public QuickPlayerItem(RectTransform target,string pItemName)
        {
            m_pItemName = pItemName;
            Target = target;
            Quality = target.Find("quality");
            m_pButton = target.GetComponent<Button>();
            Icon = target.Find("roles").GetComponent<RawImage>();
            PlayerName = target.Find("name").GetComponent<TMPro.TMP_Text>();
            FormationText = target.Find("roles/text").GetComponent<TMPro.TMP_Text>();
            
            On = target.Find("on").gameObject;
        }

        public void Dispose()
        {
            PlayerName = null;
            FormationText = null;

            Quality = null;
            m_pButton.onClick.RemoveAllListeners();
            m_pButton = null;
            Icon.texture = null;
            Icon = null;
            On = null;
            LayoutManager.Instance.AddItem(m_pItemName,Target);
            Target = null;
        }

        public void UpdatePlayerName(JObject data)
        {
            if(data == null) return;
            
            PlayerName.SetText(data["surname"].ToString());
        }

        public void UpdatePlayerInfo(PlayerT pPlayer)
        {
            if(pPlayer == null) return;
            Id = pPlayer.Id;
            PlayerName.SetText(pPlayer.Surname);
            SingleFunc.SetupQuality(pPlayer,Quality);

            GameContext pGameContext = GameContext.getCtx();
            On.SetActive(false);
            
            FormationText.SetText(pGameContext.GetDisplayLocationName(pPlayer.Position));

            Sprite pSprite = AFPool.GetItem<Sprite>("Texture",pGameContext.GetDisplayCardFormationByLocationName(FormationText.text));
            Icon.texture = pSprite.texture;
        }
    }

    private class BiddingNotice : IBase
    {
        RectTransform MainUI = null;
        public ulong ID {get; set;}
        TMPro.TMP_Text Name = null;
        TMPro.TMP_Text MoneyText = null;
        TMPro.TMP_Text CashText = null;
        public ulong Money {get; private set; }
        public uint Cash {get; private set;}

        BaseState m_pBaseState = null;
        Transform[] Emoji = new Transform[4];

        public void Show(bool bAni)
        {
            MainUI.gameObject.SetActive(true);
            if(bAni)
            {
                ALFUtils.FadeObject(MainUI,-1);
                m_pBaseState = BaseState.GetInstance(new BaseStateTarget(MainUI),1f, (uint)E_STATE_TYPE.ShowNoticePopup, null, execute,exit);
                StateMachine.GetStateMachine().AddState(m_pBaseState);
            }
            else
            {
                ALFUtils.FadeObject(MainUI,1);
                Vector2 pos = MainUI.anchoredPosition;
                pos.y = 40;
                MainUI.anchoredPosition = pos;
            }
        }
        bool execute(IState state,float dt,bool bEnd)
        {
            TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
            ALFUtils.FadeObject(MainUI,condition.GetTimePercent());
            Vector2 pos = MainUI.anchoredPosition;
            pos.y = 40 * condition.GetTimePercent();
            MainUI.anchoredPosition = pos;

            return bEnd;
        }

        IState exit(IState state)
        {
            m_pBaseState = null;
            return null;
        }
        
        public void Hide()
        {
            ID = 0;
            Money = 0;
            Cash = 0;
            MainUI.gameObject.SetActive(false);
            m_pBaseState?.Exit(true);
            m_pBaseState = null;
        }

        public void Setup( ulong id, string name, ulong money, uint cash, E_EMOJI eType)
        {
            ID = id;
            Money = money;
            Cash = cash;
            Name.SetText(name);
            MoneyText.SetText(ALFUtils.NumberToString(money));
            CashText.SetText(string.Format("{0:#,0}", cash));
            
            Emoji[0].transform.parent.gameObject.SetActive(eType != E_EMOJI.NONE);

            if(eType != E_EMOJI.NONE)
            {
                int index =(int)eType;
                index = (int)(index * 0.1f) -1;
                for(int i =0; i < Emoji.Length; ++i)
                {
                    Emoji[i].gameObject.SetActive(i == index);
                }
            }
            MainUI.anchoredPosition = Vector2.zero;
        }
       
        public BiddingNotice(RectTransform tm)
        {
            MainUI = tm;
            ID = 0;
            Money = 0;
            Cash = 0;
            Name = tm.Find("name").GetComponent<TMPro.TMP_Text>();
            MoneyText = tm.Find("money/text").GetComponent<TMPro.TMP_Text>();
            CashText = tm.Find("token/text").GetComponent<TMPro.TMP_Text>();
            
            Transform m = MainUI.Find("emoji");
            E_EMOJI[] list = new E_EMOJI[]{E_EMOJI.smile,E_EMOJI.thinking,E_EMOJI.sad,E_EMOJI.angry};
            for(int i =0; i < list.Length; ++i)
            {
                Emoji[i] = m.Find(list[i].ToString());
                Emoji[i].gameObject.SetActive(false);
            }

            m.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            m_pBaseState?.Exit(true);
            m_pBaseState = null;
            MainUI = null;
            Name = null;
            MoneyText = null;
            CashText = null;

            for(int i =0; i < Emoji.Length; ++i)
            {
                Emoji[i]= null;
            }
            Emoji = null;
        }
    }

    private class PlayerSeasonStats
    {
        [Newtonsoft.Json.JsonProperty("seasonNo")]
        public uint seasonNo = 0;

        [Newtonsoft.Json.JsonProperty("matchType")]
        public byte matchType = 0;

        [Newtonsoft.Json.JsonProperty("games")]
        public uint games = 0;

        [Newtonsoft.Json.JsonProperty("goals")]
        public uint goals = 0;
        [Newtonsoft.Json.JsonProperty("assists")]
        public uint assists = 0;
        [Newtonsoft.Json.JsonProperty("rating")]
        public uint rating = 0;

        public PlayerSeasonStats(){}
    }
    
    private class RecordLogItem : IBase
    {
        public RectTransform Target  {get; private set;}
        
        public TMPro.TMP_Text SeasonText {get; private set;}
        public TMPro.TMP_Text LeagueText {get; private set;}
        public TMPro.TMP_Text PlayedText {get; private set;}
        public TMPro.TMP_Text GoalsText {get; private set;}
        public TMPro.TMP_Text AssistsText {get; private set;}
        public TMPro.TMP_Text AvRatingText {get; private set;}

        string m_strItemName = null;
        public RecordLogItem( RectTransform taget,string pItemName)
        {
            Target = taget;
            m_strItemName = pItemName;
            SeasonText = taget.Find("season").GetComponent<TMPro.TMP_Text>();
            if(taget.Find("league") != null)
            {
                LeagueText = taget.Find("league").GetComponent<TMPro.TMP_Text>();
            }
            else
            {
                LeagueText = null;
            }

            PlayedText = taget.Find("played").GetComponent<TMPro.TMP_Text>();
            GoalsText = taget.Find("goals").GetComponent<TMPro.TMP_Text>();
            AssistsText = taget.Find("assists").GetComponent<TMPro.TMP_Text>();
            AvRatingText = taget.Find("avRating").GetComponent<TMPro.TMP_Text>();
        }

        public void Dispose()
        {
            SeasonText = null;
            LeagueText = null;
            PlayedText = null;
            GoalsText = null;
            AssistsText = null;
            AvRatingText = null;
            
            LayoutManager.Instance.AddItem(m_strItemName,Target);
            Target = null;
        }

        public void UpdateInfo(PlayerSeasonStats pPlayerSeasonStats)
        {
            if(pPlayerSeasonStats == null) return;

            GameContext pGameContext = GameContext.getCtx();
            
            SeasonText.SetText(pPlayerSeasonStats.seasonNo.ToString());
            PlayedText.SetText( pPlayerSeasonStats.games > 0 ? pPlayerSeasonStats.games.ToString():"-");
            GoalsText.SetText(pPlayerSeasonStats.goals > 0 ? pPlayerSeasonStats.goals.ToString():"-");            
            AssistsText.SetText(pPlayerSeasonStats.assists > 0 ? pPlayerSeasonStats.assists.ToString() : "-");
            AvRatingText.SetText(string.Format("{0:0.#}",pPlayerSeasonStats.rating / 10f ));

            if(pPlayerSeasonStats.matchType == (byte)GameContext.LADDER_ID)
            {
                if(LeagueText != null)
                {
                    LeagueText.gameObject.SetActive(false);
                }
            }
            else
            {
                E_TEXT_NAME eTxt = (E_TEXT_NAME)(pPlayerSeasonStats.matchType - GameContext.CHALLENGE_ID);
                if(LeagueText != null)
                {
                    LeagueText.SetText(pGameContext.GetLocalizingText(eTxt.ToString()));
                    LeagueText.gameObject.SetActive(true);
                }
            }
        }
    }
    readonly string[] AbilityNodeNameList = new string[]{"passing","crossing","shooting","offtheball","dribbling","touch","pressing","marking","positioning","tackle","intercept","concentate","speed","accelate","agility","balance","stamina","jump","creativity","vision","determination","decisions","aggression","influence","reflex","command","goalkick","oneonone","handling","aerialability"};    
    MainScene m_pMainScene = null;
    ScrollRect[] m_pScrollRect = new ScrollRect[3];
    Button[] m_pScrollTap = new Button[2];
    System.Action m_pEndCallback = null;
    TMPro.TMP_Text m_PlayerName = null;
    Transform m_QualityGroup = null;
    Image m_QualityGauge = null;
    Transform m_pPlayerCard = null;
    TMPro.TMP_Text m_Age = null;
    TMPro.TMP_Text m_Value = null;
    TMPro.TMP_Text m_Nation = null;
    TMPro.TMP_Text m_Condition = null;
    TMPro.TMP_Text m_Quality = null;
    TMPro.TMP_Text m_Height = null;
    Transform[] m_BoardPosition = new Transform[(int)E_LOCATION.LOC_END];
    GameObject m_pTraining = null;
    GameObject m_pCompare = null;
    Transform m_pTrainingPoint = null;
    GameObject m_pCloseGameObject = null;
    List<Transform> m_pMutiLevelupButtonList = new List<Transform>();
    TMPro.TMP_Text[] m_pTrainingPoints = new TMPro.TMP_Text[(int)E_TRAINING_TYPE.MAX];
    E_TRAINING_TYPE m_eCurrentLevelUp = E_TRAINING_TYPE.MAX;
    PlayerT m_pPlayerData = null;
    List<PlayerT> m_pPlayerDataList = new List<PlayerT>();
    E_PLAYER_INFO_TYPE m_eTeamType = E_PLAYER_INFO_TYPE.my;
    RectTransform[] m_pTabUIList = new RectTransform[(int)E_PLAYER_INFO_TAB.MAX];
    Transform[] m_pTabButtonList = new Transform[(int)E_PLAYER_INFO_TAB.MAX];
    List<int> m_pAbilityValue = new List<int>();
    List<PlayerSeasonStats> m_pPlayerSeasonStatsList = new List<PlayerSeasonStats>();
    E_PLAYER_INFO_TAB m_eCurrentTap = E_PLAYER_INFO_TAB.MAX;
    byte m_eMatchType = 0;

    BiddingMsgT m_pLastBiddingMsg = null;
    BiddingNotice[] m_pBiddingNotice = new BiddingNotice[4];
    Transform m_pLastBiddingPlayer = null;
    // Transform m_pEmptyPlayer = null;
    Button m_pBiddingButton = null;
    TMPro.TMP_Text[] m_pNextBidding = new TMPro.TMP_Text[2];
    TMPro.TMP_Text[] m_pRemainingTimeTexts = new TMPro.TMP_Text[2];
    Button[] m_EmojiIcon = new Button[4];
    EmblemBake m_pEmblemBake = null;
    TMPro.TMP_Text[] m_pCurrentBidding = new TMPro.TMP_Text[2];
    TMPro.TMP_Text m_pCurrentTotalCash = null;
    TMPro.TMP_Text m_pCurrentTotalMoney = null;
    TMPro.TMP_Text m_pCurrentName = null;
    TMPro.TMP_Text m_pBiddingTitle = null;
    TMPro.TMP_Text m_pClubName = null;
    Animation m_pCongratsFX = null;
    Image m_pShareTime = null;
    Button m_pShare = null;
    
    ulong m_ulValue = 0;
    ulong m_ulSellValue = 0;
    ulong m_ulValueMin = 0;
    ulong m_ulValueMax = 0;

    int[] m_iCurrentStarList = null;
    Dictionary<E_TRAINING_TYPE,int> m_iAmountList = new Dictionary<E_TRAINING_TYPE,int>();
    uint m_iCurrentAuctionId = 0;
    float m_fCurrentAuctionRemainingTime = 0;
    bool m_bJoinAuction= false;
    uint m_currentAbilityTier = 0;
    bool m_bRefreshUpdate = false;
    bool m_bBidding = false;
    bool m_bFinishAuction = false;
    
    uint m_iMyBiddingCash = 0;
    ulong m_iMyBiddingMoney = 0;
    E_EMOJI m_eSelectEmoji = E_EMOJI.NONE;
   
    public RectTransform MainUI { get; private set;}

    public bool Enable 
    {
        set{        
            if (m_pScrollRect != null) 
            {
                for(int i =0; i < m_pScrollRect.Length; ++i)
                {
                    m_pScrollRect[i].enabled = value;
                }
            }
        }
    }

    List<QuickPlayerItem> m_pQuickPlayerItems = new List<QuickPlayerItem>();
    int m_iCurrentSelectPlayerIndex = -1;
    QuickPlayerItem m_pSelectQuickPlayer = null;

    List<RecordLogItem> m_pLadderRecordLogItems = new List<RecordLogItem>();
    List<RecordLogItem> m_pLeagueRecordLogItems = new List<RecordLogItem>();

    Vector2[] m_pPrevDir = new Vector2[]{Vector2.zero,Vector2.zero,Vector2.zero};
    int[] m_iTotalScrollItems = new int[]{0,0,0};
    int[] m_iStartIndex = new int[]{0,0,0};


    public PlayerInfo(){}
    
    public void Dispose()
    {
        m_pSelectQuickPlayer = null;
        ClearTimerState();
        m_pCongratsFX = null;
        m_pMainScene = null;
        MainUI = null;
        m_pEndCallback = null;
        m_pLastBiddingPlayer = null;
        m_pCurrentTotalCash = null;
        m_pCurrentTotalMoney = null;
        m_pLastBiddingMsg = null;
        int i =0;
        for(i =0; i < m_pScrollRect.Length; ++i)
        {
            ClearScroll((E_TAB)i);
            m_pScrollRect[i].onValueChanged.RemoveAllListeners();
            m_pScrollRect[i] = null;
        }
        m_pQuickPlayerItems = null;
        m_pLadderRecordLogItems = null;
        m_pLeagueRecordLogItems = null;
        m_pScrollRect = null;
        for(i =0; i < m_pScrollTap.Length; ++i)
        {
            m_pScrollTap[i] = null;
        }
        m_pScrollTap = null;
        for(i =0; i < m_pTabButtonList.Length; ++i)
        {
            m_pTabButtonList[i] = null;
            m_pTabUIList[i] = null;
        }
        for(i =0; i < m_pTrainingPoints.Length; ++i)
        {
            m_pTrainingPoints[i] = null;
        }

        for(i =0; i < m_BoardPosition.Length; ++i)
        {
            m_BoardPosition[i] = null;
        }

        for(i =0; i < m_EmojiIcon.Length; ++i)
        {
            m_EmojiIcon[i] = null;
        }
        m_EmojiIcon = null;
        for(i =0; i < m_pBiddingNotice.Length; ++i)
        {
            m_pBiddingNotice[i]?.Dispose();
            m_pBiddingNotice[i]= null;
        }
        m_pBiddingNotice = null;
        
        m_pEmblemBake?.Dispose();
        m_pEmblemBake = null;
        m_pCurrentName = null;
        m_pBiddingTitle = null;
        m_pClubName = null;

        for(i = 0; i < m_pCurrentBidding.Length; ++i)
        {
            m_pCurrentBidding[i] = null;
        }
        m_pCurrentBidding = null;

        for(i = 0; i < m_pMutiLevelupButtonList.Count; ++i)
        {
            m_pMutiLevelupButtonList[i] = null;
        }

        for(i = 0; i < m_pNextBidding.Length; ++i)
        {
            m_pNextBidding[i] = null;
            m_pRemainingTimeTexts[i] = null;
        }
        m_pNextBidding = null;
        m_pBiddingButton = null;
        m_pRemainingTimeTexts = null;
        m_pMutiLevelupButtonList = null;
        m_pPlayerDataList.Clear();
        m_pPlayerDataList = null;
        m_BoardPosition = null;
        m_pTrainingPoints = null;
        m_pTabButtonList = null;
        m_pTabUIList = null;
        m_pPlayerCard = null;
        m_QualityGroup = null;
        m_QualityGauge = null;
 
        m_PlayerName = null;
        m_Age = null;
        m_Value = null;
        m_Nation = null;
        m_Condition = null;
        m_Quality = null;
        m_Height = null;
        
        m_pTraining = null;
        m_pCompare = null;
        m_pTrainingPoint = null;
        m_pPlayerData = null;
        m_pCloseGameObject = null;
        m_pShareTime = null;
        m_pShare = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "PlayerInfo : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "PlayerInfo : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        float w = 0;
        float wh = 0;
        
        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        float ax = 0;

        RectTransform ui = MainUI.Find("back").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);
        ui = MainUI.Find("root/bg/tabs").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);
        w = (ui.rect.width / ui.childCount);
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        int iTabIndex = -1;
        int n =0;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_PLAYER_INFO_TAB)Enum.Parse(typeof(E_PLAYER_INFO_TAB), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;
            m_pTabUIList[iTabIndex] = MainUI.Find($"root/bg/{item.gameObject.name}").GetComponent<RectTransform>();
            m_pTabUIList[iTabIndex].gameObject.SetActive(false);
            LayoutManager.SetReciveUIButtonEvent(m_pTabUIList[iTabIndex],this.ButtonEventCall);
            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
            item.Find("on").gameObject.SetActive(false);
        }
        m_pTrainingPoint = MainUI.Find("root/training");
        Transform tm = MainUI.Find("root/bg/playerInfo");
        LayoutManager.SetReciveUIButtonEvent(tm.GetComponent<RectTransform>(),this.ButtonEventCall);
        m_pPlayerCard = tm.Find("player");
        m_QualityGroup = m_pPlayerCard.Find("quality");
        
        m_Age = tm.Find("info/age/text").GetComponent<TMPro.TMP_Text>();
        m_Value = tm.Find("info/value/text").GetComponent<TMPro.TMP_Text>();
        m_Nation = tm.Find("info/nation/text").GetComponent<TMPro.TMP_Text>();
        m_Condition = tm.Find("info/condition/text").GetComponent<TMPro.TMP_Text>();
        m_Height = tm.Find("info/height/text").GetComponent<TMPro.TMP_Text>();
        m_Quality = tm.Find("info/quality/text").GetComponent<TMPro.TMP_Text>();
        m_QualityGauge = tm.Find("info/quality/gauge").GetComponent<Image>();
        m_pTraining = tm.Find("training").gameObject;
        m_pCompare = tm.Find("compare").gameObject;
        m_pCongratsFX = MainUI.Find("FX_congrats").GetComponent<Animation>();
        
        for(n=0; n < (int)E_ABILINDEX.AB_END; ++n)
        {
            m_pAbilityValue.Add(0);
        }
        
        tm = tm.Find("board");

        for(n =0; n < tm.childCount; ++n)
        {
            iTabIndex = (int)((E_LOCATION)Enum.Parse(typeof(E_LOCATION), tm.GetChild(n).gameObject.name));
            m_BoardPosition[iTabIndex] = tm.GetChild(n);
        }
        
        LayoutManager.SetReciveUIButtonEvent(tm.GetComponent<RectTransform>(),this.ButtonEventCall);
        
        m_PlayerName = MainUI.Find("root/bg/title").GetComponent<TMPro.TMP_Text>();

        for(n =0; n < m_pTrainingPoints.Length; ++n)
        {
            m_pTrainingPoints[n] = m_pTrainingPoint.Find($"{((E_TRAINING_TYPE)n).ToString()}/text").GetComponent<TMPro.TMP_Text>();
        }

        MainUI.gameObject.SetActive(false);
        
        m_pScrollRect[0] = MainUI.Find("root/bg/record/ladder/scroll").GetComponent<ScrollRect>();
        m_pScrollRect[1] = MainUI.Find("root/bg/record/league/scroll").GetComponent<ScrollRect>();
        m_pScrollRect[2] = MainUI.Find("root/quick").GetComponent<ScrollRect>();
        
        m_pScrollRect[0].onValueChanged.AddListener( RecordScrollViewChangeValueEventCall);
        m_pScrollRect[1].onValueChanged.AddListener( RecordScrollViewChangeValueEventCall);
        m_pScrollRect[2].onValueChanged.AddListener( ScrollViewChangeValueEventCall);

        m_pScrollTap[0] = MainUI.Find("root/bg/record/top/ladder").GetComponent<Button>();
        m_pScrollTap[1] = MainUI.Find("root/bg/record/top/league").GetComponent<Button>();

        tm = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer];
        
        m_pBiddingNotice[0] = new BiddingNotice(tm.Find("bidding/notice_0/root").GetComponent<RectTransform>());
        m_pBiddingNotice[1] = new BiddingNotice(tm.Find("bidding/notice_1/root").GetComponent<RectTransform>());
        m_pBiddingNotice[2] = new BiddingNotice(tm.Find("bidding/notice_2/root").GetComponent<RectTransform>());
        m_pBiddingNotice[3] = new BiddingNotice(tm.Find("bidding/my/root").GetComponent<RectTransform>());
        m_pLastBiddingPlayer = tm.Find("bidding/player");

        m_pCurrentTotalCash = tm.Find("bidding/tap/token/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentTotalMoney = tm.Find("bidding/tap/money/text").GetComponent<TMPro.TMP_Text>();

        m_pRemainingTimeTexts[0] = tm.Find("bidding/remaining/m").GetComponent<TMPro.TMP_Text>();
        m_pRemainingTimeTexts[1] = tm.Find("bidding/remaining/s").GetComponent<TMPro.TMP_Text>();
        
        m_pBiddingButton = tm.Find("bidding/bid/bid").GetComponent<Button>();
        m_pNextBidding[0] = tm.Find("bidding/bid/next/token/text").GetComponent<TMPro.TMP_Text>();
        m_pNextBidding[1] = tm.Find("bidding/bid/next/money/text").GetComponent<TMPro.TMP_Text>();

        m_EmojiIcon[0] = tm.Find($"bidding/bid/emoji/{E_EMOJI.smile}").GetComponent<Button>();
        m_EmojiIcon[1] = tm.Find($"bidding/bid/emoji/{E_EMOJI.thinking}").GetComponent<Button>();
        m_EmojiIcon[2] = tm.Find($"bidding/bid/emoji/{E_EMOJI.sad}").GetComponent<Button>();
        m_EmojiIcon[3] = tm.Find($"bidding/bid/emoji/{E_EMOJI.angry}").GetComponent<Button>();
    
        m_pEmblemBake = m_pLastBiddingPlayer.Find("emblem").GetComponent<EmblemBake>();
        m_pCurrentName = m_pLastBiddingPlayer.Find("name").GetComponent<TMPro.TMP_Text>();
        m_pBiddingTitle = m_pLastBiddingPlayer.Find("title").GetComponent<TMPro.TMP_Text>();
        m_pClubName = m_pLastBiddingPlayer.Find("text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentBidding[0] = m_pLastBiddingPlayer.Find("box/token/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentBidding[1] = m_pLastBiddingPlayer.Find("box/money/text").GetComponent<TMPro.TMP_Text>();

        m_pShare = m_pPlayerCard.parent.Find("share").GetComponent<Button>();
        m_pShareTime = m_pShare.transform.Find("fill").GetComponent<Image>();
        m_pShareTime.gameObject.SetActive(false);
        m_pCloseGameObject = MainUI.Find("root/bg/close").gameObject;
        LayoutManager.SetReciveUIButtonEvent(m_pCloseGameObject.GetComponent<RectTransform>(),this.ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("root/bg/tip").GetComponent<RectTransform>(),this.ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(m_pTrainingPoint.GetComponent<RectTransform>(),this.ButtonEventCall);
        SetupScroll(E_TAB.quick);
        SetupScroll(E_TAB.ladder);
        SetupScroll(E_TAB.league);
    }

    public void DoExpire(int index)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            if(index == 0)
            {
                m_pShareTime.gameObject.SetActive(false);
                m_pShare.enabled = true;
            }
        }
    }

    void ShowBiddingNotice(BiddingMsgT pBiddingMsg, bool bAni)
    {
        GameContext pGameContext = GameContext.getCtx();
        BiddingNotice pBiddingNotice = null;
        if(pBiddingMsg.Id == pGameContext.GetClubID())
        {
            pBiddingNotice = m_pBiddingNotice[3];
            pBiddingNotice.Hide();
        }
        else
        {
            BiddingNotice pBiddingNotice1 = null;
            int visibleCount = 0;
            for(int i =0; i < m_pBiddingNotice.Length -1; ++i)
            {
                if(m_pBiddingNotice[i].ID > 0)
                {
                    ++visibleCount;
                }

                if(m_pBiddingNotice[i].ID == pBiddingMsg.Id)
                {
                    pBiddingNotice = m_pBiddingNotice[i];
                }
                else if(m_pBiddingNotice[i].ID > 0 && m_pBiddingNotice[i].ID != pBiddingMsg.Id)
                {
                    pBiddingNotice1 = m_pBiddingNotice[i];
                }
            }
            
            if(visibleCount == 2 && pBiddingNotice == null && pBiddingNotice1 != null)
            {
                for(int i =0; i < m_pBiddingNotice.Length -1; ++i)
                {
                    if(m_pBiddingNotice[i].ID > 0 && m_pBiddingNotice[i].Money < pBiddingNotice1.Money && m_pBiddingNotice[i].Cash < pBiddingNotice1.Cash)
                    {
                        pBiddingNotice1 = m_pBiddingNotice[i];
                    }
                }
            }

            for(int i =0; i < m_pBiddingNotice.Length -1; ++i)
            {
                if(m_pBiddingNotice[i].ID == 0)
                {
                    pBiddingNotice?.Hide();
                    pBiddingNotice1?.Hide();
                    pBiddingNotice = m_pBiddingNotice[i];
                    break;
                }
            }
        }

        pBiddingNotice.Setup( pBiddingMsg.Id, pBiddingMsg.Name, pBiddingMsg.Gold, pBiddingMsg.Token, (E_EMOJI)pBiddingMsg.Emotion);
        pBiddingNotice.Show(bAni);
    }

    void SetupScroll(E_TAB eType)
    {
        RectTransform pItem = null;
        Vector2 size;
        ScrollRect pScrollRect = m_pScrollRect[(int)eType];
        string strItemName = SCROLL_ITEM_NAME[(int)eType];
        m_iTotalScrollItems[(int)eType] = 0;
        m_iStartIndex[(int)eType] = 0;

        if(eType == E_TAB.quick )
        {
            float viewSize = pScrollRect.viewport.rect.width;
            float itemSize = 0;

            float w = 0;
            
            QuickPlayerItem pPlayerItem = null;

            while(viewSize > -itemSize)
            {
                if(viewSize > 0)
                {
                    ++m_iTotalScrollItems[(int)eType];
                }
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(strItemName);
                    
                if(pItem)
                {
                    pPlayerItem = new QuickPlayerItem (pItem,strItemName);
                    m_pQuickPlayerItems.Add(pPlayerItem);
                    
                    pItem.SetParent(pScrollRect.content,false);
                    pItem.localScale = Vector3.one;       
                    pItem.anchoredPosition = new Vector2(w,0);
                    itemSize = pItem.rect.width;
                    w += itemSize;
                    viewSize -= itemSize;
                    pItem.gameObject.SetActive(viewSize > -itemSize);
                }
            }

            size = pScrollRect.content.sizeDelta;
            size.x = w;
            pScrollRect.horizontalNormalizedPosition = 0;
            m_pPrevDir[(int)eType].x = 0;
        }
        else
        {
            float viewSize = pScrollRect.viewport.rect.height;
            float itemSize = 0;
            float h = 0;
            List<RecordLogItem> list = null;
            if(eType == E_TAB.ladder)
            {
                list = m_pLadderRecordLogItems;
            }
            else
            {
                list = m_pLeagueRecordLogItems;
            }

            RecordLogItem pRecordLogItem = null;

            while(viewSize > -itemSize)
            {
                if(viewSize > 0)
                {
                    ++m_iTotalScrollItems[(int)eType];
                }
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(strItemName);
                    
                if(pItem)
                {
                    pItem.SetParent(pScrollRect.content,false);
                    pItem.anchoredPosition = new Vector2(0,-h);
                    pItem.localScale = Vector3.one;
                    size = pItem.sizeDelta;
                    size.x = 0;
                    pItem.sizeDelta = size;
                    itemSize = pItem.rect.height;
                    h += itemSize;
                    viewSize -= itemSize;

                    pRecordLogItem = new RecordLogItem(pItem,strItemName);
                    list.Add(pRecordLogItem);
                }
            }

            size = pScrollRect.content.sizeDelta;
            size.y = h;
            m_pPrevDir[(int)eType].x = 0;
        }
        
        pScrollRect.content.sizeDelta = size;
        pScrollRect.content.anchoredPosition = Vector2.zero;
        LayoutManager.SetReciveUIScollViewEvent(pScrollRect,ScrollViewItemButtonEventCall);
    }

    void ClearScroll(E_TAB eScrollType)
    {
        int i = 0;
        ScrollRect pScrollRect = m_pScrollRect[(int)eScrollType];

        if(eScrollType == E_TAB.quick)
        {
            i = m_pQuickPlayerItems.Count;
                
            while(i > 0)
            {
                --i;
                m_pQuickPlayerItems[i].Dispose();
            }

            m_pQuickPlayerItems.Clear();
        }
        else if(eScrollType == E_TAB.ladder)
        {
            i = m_pLadderRecordLogItems.Count;
                
            while(i > 0)
            {
                --i;
                m_pLadderRecordLogItems[i].Dispose();
            }

            m_pLadderRecordLogItems.Clear();
        }
        else if(eScrollType == E_TAB.league)
        {
            i = m_pLeagueRecordLogItems.Count;
                
            while(i > 0)
            {
                --i;
                m_pLeagueRecordLogItems[i].Dispose();
            }

            m_pLeagueRecordLogItems.Clear();
        }
    
        LayoutManager.SetReciveUIScollViewEvent(pScrollRect,null);

        Vector2 pos = pScrollRect.content.anchoredPosition;
        Vector2 size = pScrollRect.content.sizeDelta;
        if(eScrollType == E_TAB.quick)
        {
            size.x =0;
            pos.x =0;
        }
        else
        {
            size.y =0;
            pos.y =0;
        }
        
        pScrollRect.content.sizeDelta = size;
        pScrollRect.content.anchoredPosition = pos;
    }
    void ResetScroll(E_TAB eScrollType)
    {
        ScrollRect pScrollRect = m_pScrollRect[(int)eScrollType];
        Vector2 pos;
        float viewSize = 0;
        float itemSize = 0;

        if(eScrollType == E_TAB.quick)
        {   
            viewSize = pScrollRect.viewport.rect.width;
            QuickPlayerItem pPlayerItem = null;
            for(int i = 0; i < m_pQuickPlayerItems.Count; ++i)
            {
                pPlayerItem = m_pQuickPlayerItems[i];
                itemSize = pPlayerItem.Target.rect.width;
                viewSize -= itemSize;
                pPlayerItem.Target.gameObject.SetActive(viewSize > -itemSize);

                pos = pPlayerItem.Target.anchoredPosition;            
                pos.x = i * itemSize;
                pPlayerItem.Target.anchoredPosition = pos;
            }

            pScrollRect.horizontalNormalizedPosition = 0;
            m_pPrevDir[(int)eScrollType].x = 0;
        }
        else
        {
            viewSize = pScrollRect.viewport.rect.height;
            List<RecordLogItem> list = null; 
            RecordLogItem pItem = null;
            if(eScrollType == E_TAB.ladder)
            {
                list = m_pLadderRecordLogItems;
            }
            else
            {
                list = m_pLeagueRecordLogItems;
            }

            for(int i = 0; i < list.Count; ++i)
            {
                pItem = list[i];
                itemSize = pItem.Target.rect.height;
                viewSize -= itemSize;
                pItem.Target.gameObject.SetActive(viewSize > -itemSize);

                pos = pItem.Target.anchoredPosition;            
                pos.y = -i * itemSize;
                pItem.Target.anchoredPosition = pos;
            }

            pScrollRect.verticalNormalizedPosition = 1;
            m_pPrevDir[(int)eScrollType].y = 1;
        }
        
        pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex[(int)eScrollType] = 0;
    }

    void SetupScrollTapButton()
    {
        bool bOn = m_eMatchType == GameContext.LEAGUE_ID;
        m_pScrollTap[0].transform.Find("on").gameObject.SetActive(!bOn);
        m_pScrollTap[0].transform.Find("off").gameObject.SetActive(bOn);
        m_pScrollTap[1].transform.Find("off").gameObject.SetActive(!bOn);
        m_pScrollTap[1].transform.Find("on").gameObject.SetActive(bOn);
    }

    void SetupRecordData()
    {
        RectTransform tm = m_pScrollTap[1].GetComponent<RectTransform>();
        float x = tm.anchoredPosition.x;
        tm = m_pScrollTap[0].GetComponent<RectTransform>();
        Vector2 pos = tm.anchoredPosition;
        if(GameContext.getCtx().IsLeagueOpen())
        {
            pos.x = -x;
            m_pScrollTap[1].gameObject.SetActive(true);
        }
        else
        {
            m_pScrollTap[1].gameObject.SetActive(false);
            pos.x = 0;
        }

        tm.anchoredPosition = pos;

        E_TAB eType = m_eMatchType == GameContext.LEAGUE_ID ? E_TAB.league : E_TAB.ladder;
        ResetScroll(eType);
        m_iStartIndex[(int)eType] = 0;
        ScrollRect pScrollRect = m_pScrollRect[(int)eType];

        m_pScrollRect[0].transform.parent.gameObject.SetActive(false);
        m_pScrollRect[1].transform.parent.gameObject.SetActive(false);
        
        pScrollRect.transform.parent.gameObject.SetActive(true);

        float viewSize = pScrollRect.viewport.rect.height;
        float itemSize = 0;
        List<RecordLogItem> list = null; 
        RecordLogItem pItem = null;
        if(eType == E_TAB.ladder)
        {
            list = m_pLadderRecordLogItems;
        }
        else
        {
            list = m_pLeagueRecordLogItems;
        }

        for(int i =0; i < list.Count; ++i)
        {
            pItem = list[i];
            itemSize = pItem.Target.rect.height;

            if(list.Count <= i)
            {
                pItem.Target.gameObject.SetActive(false);
            }
            else
            {
                if(viewSize > -itemSize)
                {    
                    viewSize -= itemSize;
                    pItem.Target.gameObject.SetActive(viewSize > -itemSize);
                }
            }

            if(m_pPlayerSeasonStatsList.Count > i)
            {
                pItem.UpdateInfo(m_pPlayerSeasonStatsList[i]);
            }
            else
            {
                pItem.Target.gameObject.SetActive(false);
            }
        }

        uint totalGame = 0;
        uint totalGoal = 0;
        uint totalAssist = 0;
        uint totalRating = 0;
        uint totalCount = 0;

        uint totalGame1 = 0;
        uint totalGoal1 = 0;
        uint totalAssist1 = 0;
        uint totalRating1 = 0;
        uint totalCount1 = 0;

        TMPro.TMP_Text text = null;
        
        for(int i =0; i < m_pPlayerSeasonStatsList.Count; ++i)
        {
            if(m_pPlayerSeasonStatsList[i].matchType == (byte)GameContext.LADDER_ID)
            {
                ++totalCount;
                totalGame += m_pPlayerSeasonStatsList[i].games;
                totalGoal += m_pPlayerSeasonStatsList[i].goals;
                totalAssist += m_pPlayerSeasonStatsList[i].assists;
                totalRating += m_pPlayerSeasonStatsList[i].rating;
            }
            else
            {
                ++totalCount1;
                totalGame1 += m_pPlayerSeasonStatsList[i].games;
                totalGoal1 += m_pPlayerSeasonStatsList[i].goals;
                totalAssist1 += m_pPlayerSeasonStatsList[i].assists;
                totalRating1 += m_pPlayerSeasonStatsList[i].rating;
            }
        }
        
        if(eType == E_TAB.ladder )
        {
            tm = pScrollRect.transform.parent.Find("total").GetComponent<RectTransform>();
        }
        else
        {
            tm = pScrollRect.transform.parent.Find("leagueTotal").GetComponent<RectTransform>();

            text = tm.Find("played").GetComponent<TMPro.TMP_Text>();
            text.SetText( totalGame1 > 0 ? totalGame1.ToString():"-");
            text = tm.Find("goals").GetComponent<TMPro.TMP_Text>();
            text.SetText(totalGoal1 > 0 ? totalGoal1.ToString():"-");
            text = tm.Find("assists").GetComponent<TMPro.TMP_Text>();
            text.SetText(totalAssist1 > 0 ? totalAssist1.ToString() : "-");
            text = tm.Find("avRating").GetComponent<TMPro.TMP_Text>();
            text.SetText( totalCount1 > 0 ? string.Format("{0:0.#}",totalRating1 / totalCount1 / 10f) : "-");
            
            tm = pScrollRect.transform.parent.Find("challengeTotal").GetComponent<RectTransform>();
        }

        text = tm.Find("played").GetComponent<TMPro.TMP_Text>();
        text.SetText( totalGame > 0 ? totalGame.ToString():"-");
        text = tm.Find("goals").GetComponent<TMPro.TMP_Text>();
        text.SetText(totalGoal > 0 ? totalGoal.ToString():"-");
        text = tm.Find("assists").GetComponent<TMPro.TMP_Text>();
        text.SetText(totalAssist > 0 ? totalAssist.ToString() : "-");
        text = tm.Find("avRating").GetComponent<TMPro.TMP_Text>();
        text.SetText( totalCount > 0 ? string.Format("{0:0.#}",totalRating / totalCount / 10f) : "-");

        Vector2 size = pScrollRect.content.sizeDelta;
        size.y = m_pPlayerSeasonStatsList.Count * itemSize;
        pScrollRect.content.sizeDelta = size;
        pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir[(int)eType].y = 1;
        pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    public void SetupQuickPlayerInfoData(List<PlayerT> pPlayerList)
    {
        ResetScroll(E_TAB.quick);
        
        m_pSelectQuickPlayer = null;

        if(pPlayerList != null)
        {
            m_pPlayerDataList = pPlayerList.ToList();
        }
        
        m_iStartIndex[2] = 0;

        ScrollRect pScrollRect = m_pScrollRect[(int)E_TAB.quick];

        if(m_pPlayerDataList == null || m_pPlayerDataList.Count == 0)
        {
            pScrollRect.gameObject.SetActive(false);
            return;
        }
        
        pScrollRect.gameObject.SetActive(true);

        GameContext pGameContext = GameContext.getCtx();

        float viewSize = pScrollRect.viewport.rect.width;
        float itemSize = 0;
        
        QuickPlayerItem pPlayerItem = null;

        for(int i =0; i < m_pQuickPlayerItems.Count; ++i)
        {
            pPlayerItem = m_pQuickPlayerItems[i];
            itemSize = pPlayerItem.Target.rect.width;

            if(m_pPlayerDataList.Count <= i)
            {
                pPlayerItem.Target.gameObject.SetActive(false);
            }
            else
            {
                pPlayerItem.UpdatePlayerInfo(m_pPlayerDataList[i]);
            
                if(viewSize > -itemSize)
                {    
                    viewSize -= itemSize;
                    pPlayerItem.Target.gameObject.SetActive(viewSize > -itemSize);
                }
            }
        }

        Vector2 size = pScrollRect.content.sizeDelta;
        size.x = m_pPlayerDataList.Count * itemSize;
        pScrollRect.content.sizeDelta = size;
        pScrollRect.horizontalNormalizedPosition = 0;
        m_pPrevDir[2].x = 0;
        pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    public void MoveQuickPlayer()
    {
        if(m_pPlayerDataList == null || m_pPlayerDataList.Count == 0) return;
        
        int index =0;
        PlayerT pPlayer = ALFUtils.BinarySearch<PlayerT>(m_pPlayerDataList,(PlayerT d)=> { return d.Id.CompareTo(m_pPlayerData.Id);},ref index);
        ScrollRect pScrollRect = m_pScrollRect[(int)E_TAB.quick];

        m_iStartIndex[2] = index;

        Vector2 pos = pScrollRect.content.anchoredPosition;
        pos.x = m_pQuickPlayerItems[0].Target.rect.width * m_iStartIndex[2];
        pScrollRect.content.anchoredPosition = pos * -1;
        m_iCurrentSelectPlayerIndex = m_iStartIndex[2];
        index =0;
        for(int i = 0; i < m_pQuickPlayerItems.Count; ++i)
        {
            m_pQuickPlayerItems[i].Target.anchoredPosition = m_pQuickPlayerItems[i].Target.anchoredPosition + pos;
            index = m_iStartIndex[2] +i;
            if( index > -1 && index < m_pPlayerDataList.Count)
            {
                m_pQuickPlayerItems[i].UpdatePlayerInfo(m_pPlayerDataList[index]);
            }
        }
        m_pQuickPlayerItems[0].On.SetActive(true);
        m_pSelectQuickPlayer = m_pQuickPlayerItems[0];
    }

    public bool IsCurrentBidding()
    {
        return m_bBidding && !m_bFinishAuction;
    }

    public void SetupPlayerInfoData(E_PLAYER_INFO_TYPE eTeam, PlayerT pPlayer, System.Action pEndCallback = null)
    {
        m_pPlayerData = pPlayer;
        m_iCurrentAuctionId = GameContext.getCtx().GetAuctionIdByPlayerNo(eTeam,m_pPlayerData.Id);
        m_bJoinAuction = false;
        m_bBidding = false;
        m_iMyBiddingCash = 0;
        m_iMyBiddingMoney = 0;
        m_eTeamType = eTeam;
        m_pEndCallback = pEndCallback;
        m_currentAbilityTier = m_pPlayerData.AbilityTier;
        m_fCurrentAuctionRemainingTime = 0;
        m_eSelectEmoji = E_EMOJI.NONE;
        for(int i =0; i < m_pBiddingNotice.Length -1; ++i)
        {
            m_pBiddingNotice[i].Hide();
        }
        SetupPlayerInformation();
        SetupPlayerAbilityInformation(false);
        UpdateTrainingPoint();
    }

    public void UpdatePlayerHP()
    {
        
        if(m_pPlayerData.Hp < 50)
        {
            m_Condition.color = GameContext.HP_L;
        }
        else if(m_pPlayerData.Hp < 70)
        {
            m_Condition.color = GameContext.HP_LH;
        }
        else if(m_pPlayerData.Hp < 90)
        {
            m_Condition.color = GameContext.HP_H;
        }
        else
        {
            m_Condition.color = GameContext.HP_F;
        }
        
        m_Condition.SetText($"{m_pPlayerData.Hp}%");
    }

    bool ChangeStarAnimation(int[] list)
    {
        ChangeStarStateData data = null;
        Transform tm = m_pPlayerCard.Find("quality");
        Transform item = null;
        
        for(int i =0; i < m_iCurrentStarList.Length; ++i)
        {
            item = tm.GetChild(i);
            if(m_iCurrentStarList[i] != list[i])
            {
                if(data == null)
                {
                    data = new ChangeStarStateData();
                }
                item.Find("on").gameObject.SetActive(false);
                if(item.Find("h") != null)
                {
                    item.Find("h").gameObject.SetActive(false);
                }
                
                if(list[i] == 1)
                {
                    data.AnimationList.Add(item.Find("h").GetComponent<Animation>());
                }
                else if(list[i] == 2)
                {
                    data.AnimationList.Add(item.Find("on").GetComponent<Animation>());
                }

                if(m_iCurrentStarList[i] == 1)
                {
                    item.Find("h").gameObject.SetActive(true);
                }

                m_iCurrentStarList[i] = list[i];
            }
        }
        
        if(data != null)
        {
            LayoutManager.Instance.InteractableDisableAll(null,true);
            Animation pAnimation = m_pPlayerCard.GetComponent<Animation>();
            pAnimation.Play();
            
            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pAnimation),-1, (uint)E_STATE_TYPE.Timer, null, executeChangeStarCallback,exitChangeStarCallback);
            data.CurrentAnimation = pAnimation;
            pBaseState.StateData = data;
            StateMachine.GetStateMachine().AddState(pBaseState);
            pAnimation = m_pPlayerCard.Find("GlintEffect").GetComponent<Animation>();
            pAnimation.gameObject.SetActive(true);
            pAnimation.Play();                        

            return true;
        }

        return false;
    }

    int[] CheckChangeStar()
    {
        Transform tm = m_pPlayerCard.Find("quality");
        int[] list = new int[7]{0,0,0,0,0,0,0};
        if(tm != null)
        {
            Transform item = null;
            for(int i =0; i < tm.childCount; ++i)
            {
                item = tm.GetChild(i);
                if(item.gameObject.activeSelf)
                {
                    if(item.gameObject.name == "h_star" && item.Find("on").gameObject.activeSelf)
                    {
                        list[i]= 2;
                    }
                    else if(item.Find("h") && item.Find("h").gameObject.activeSelf)
                    {
                        list[i]= 1;
                    }
                    else if(item.Find("on").gameObject.activeSelf)
                    {
                        list[i]= 2;
                    }
                }
                else
                {
                    list[i]= 0;
                }
            }
        }
        
        return list;
    }

    void UpdatePlayerInformation()
    {
        GameContext pGameContext = GameContext.getCtx();
        SingleFunc.SetupPlayerCard(m_pPlayerData,m_pPlayerCard,E_ALIGN.Left,E_ALIGN.Left);
        
        m_ulValue = m_pPlayerData.Price;

        m_ulValueMin = (ulong)(m_ulValue * (float)(pGameContext.GetConstValue(E_CONST_TYPE.playerAuctionStartPriceRateMin) / 10000f));
        m_ulValueMax = (ulong)(m_ulValue * (float)(pGameContext.GetConstValue(E_CONST_TYPE.playerAuctionStartPriceRateMax) / 10000f));
        m_ulSellValue = m_ulValue;
        m_Value.SetText(ALFUtils.NumberToString(m_ulValue));

        UpdatePlayerHP();

        m_Quality.SetText($"{m_pPlayerData.AbilityTier}/{m_pPlayerData.PotentialTier}");
        
        PlayerPotentialWeightSumList pPlayerPotentialWeightSumList = pGameContext.GetFlatBufferData<PlayerPotentialWeightSumList>(E_DATA_TYPE.PlayerPotentialWeightSum); 
        PlayerPotentialWeightSumItem? pPlayerPotentialWeightSumItem = pPlayerPotentialWeightSumList.PlayerPotentialWeightSumByKey(m_pPlayerData.AbilityTier);
        float fCurrent = m_pPlayerData.AbilityWeightSum - pPlayerPotentialWeightSumItem.Value.Min;
        float fMax = pPlayerPotentialWeightSumItem.Value.Max - pPlayerPotentialWeightSumItem.Value.Min;
        
        m_QualityGauge.fillAmount = m_pPlayerData.AbilityTier == m_pPlayerData.PotentialTier ? 1 : fCurrent / fMax;

        UpdatePositionFamiliar();
    }
    void SetupPlayerInformation()
    {
        if(m_pMainScene.IsMatch())
        {
            m_pShare.gameObject.SetActive(false);
            m_pTraining.SetActive(false);
            m_pCompare.SetActive(false);
            m_pPlayerCard.parent.Find("edit").gameObject.SetActive(false);
        }
        else
        {
            m_pShare.gameObject.SetActive(m_eTeamType == E_PLAYER_INFO_TYPE.my);
            m_pTraining.SetActive(m_eTeamType == E_PLAYER_INFO_TYPE.my && m_pPlayerData.Status == 0);
            m_pCompare.SetActive(true);
            m_pPlayerCard.parent.Find("edit").gameObject.SetActive(m_eTeamType == E_PLAYER_INFO_TYPE.my && m_pPlayerData.Status == 0);
        }
        
        m_PlayerName.SetText($"{m_pPlayerData.Forename} {m_pPlayerData.Surname}");
        GameContext pGameContext = GameContext.getCtx();
        byte age = pGameContext.GetPlayerAge(m_pPlayerData);
        m_Age.SetText(age > 40 ? "40+":age.ToString());
        
        NATION_CODE code = pGameContext.ConvertPlayerNationCodeByString(m_pPlayerData.Nation,m_pPlayerData.Id);
        PlayerNationalityItem? pPlayerNationalityItem = pGameContext.GetPlayerNationDataByCode(code);
        m_Nation.SetText(pGameContext.GetLocalizingText(pPlayerNationalityItem.Value.List(0).Value.NationName));
        m_Height.SetText($"{m_pPlayerData.Height} CM");

        UpdatePlayerInformation();
    }

    void UpdatePositionFamiliar()
    {
        int index =0;
        byte value = 0;
        GameContext pGameContext = GameContext.getCtx();
        for(int n =0; n < m_BoardPosition.Length; ++n)
        {
            if(m_BoardPosition[n] != null)
            {
                m_BoardPosition[n].Find("c1").gameObject.SetActive(false);
                m_BoardPosition[n].Find("c2").gameObject.SetActive(false);
                m_BoardPosition[n].Find("c3").gameObject.SetActive(false);
                m_BoardPosition[n].Find("c4").gameObject.SetActive(false);

                index = pGameContext.ConvertPositionByTag((E_LOCATION)n);
                value = m_pPlayerData.PositionFamiliars[index];
                if(value > 0)
                {
                    if(value >= 90)
                    {
                        m_BoardPosition[n].Find("c4").gameObject.SetActive(true);
                    }
                    else if(value >= 60)
                    {
                        m_BoardPosition[n].Find("c3").gameObject.SetActive(true);
                    }
                    else if(value >= 30)
                    {
                        m_BoardPosition[n].Find("c2").gameObject.SetActive(true);
                    }
                    else
                    {
                        m_BoardPosition[n].Find("c1").gameObject.SetActive(true);
                    }
                }
            }
        }
    }
    
    uint UpdateTotalAbility(E_TRAINING_TYPE eType)
    {
        int i = (int)E_ABILINDEX.AB_ATT_PASS + ((int)eType * 6);
        int end = i + 6;
        
        Transform group = m_pTabUIList[(int)E_PLAYER_INFO_TAB.information].Find(eType.ToString());
        
        TMPro.TMP_Text text = group.Find("top/lv").GetComponent<TMPro.TMP_Text>();
        uint total = 0;
        while(i < end)
        {
            if(m_pPlayerData.Ability[i].Current > 0)
            {
                total += (uint)m_pPlayerData.Ability[i].Current;
            }
            
            ++i;
        }
        text.SetText(((int)(total / 6)).ToString());
        return total;
    }

    void UpdateTrainingCost(E_TRAINING_TYPE eType,bool bMuti)
    {
        Transform group = m_pTabUIList[(int)E_PLAYER_INFO_TAB.information].Find(eType.ToString());
        uint total = UpdateTotalAbility(eType);
        
        GameContext pGameContext = GameContext.getCtx();
        TrainingCostList pTrainingCostList = pGameContext.GetFlatBufferData<TrainingCostList>(E_DATA_TYPE.TrainingCost);
        TrainingCostItem? pTrainingCostItem = pTrainingCostList.TrainingCostByKey(total);
        if(pTrainingCostItem != null)
        {
            Button pButton = group.Find("training").GetComponent<Button>();
            pButton.gameObject.SetActive(true);
            TMPro.TMP_Text text = pButton.transform.Find("cost").GetComponent<TMPro.TMP_Text>();
            text.SetText(ALFUtils.NumberToString(pTrainingCostItem.Value.Point));
            bool bActive = false;
            if(pGameContext.IsPlayerAbilityUp(m_pPlayerData,eType))
            {
                bActive = pGameContext.GetTrainingPointByType(eType) >= pTrainingCostItem.Value.Point;
            }
            pButton.enabled = bActive;
            pButton.transform.Find("on").gameObject.SetActive(bActive);
            pButton.transform.Find("off").gameObject.SetActive(!bActive);
            if(bMuti && bActive && m_eCurrentLevelUp == eType)
            {
                uint totalCost = 0;

                int iAmount = GetTrainingLevelCount(total,eType,ref totalCost);
                if(!m_iAmountList.ContainsKey(eType))
                {
                    m_iAmountList.Add(eType,iAmount);
                }
                else
                {
                    m_iAmountList[eType]=iAmount;
                }

                Transform mutiTraining = group.Find("mutiTraining");
                List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(mutiTraining);
                if(iAmount > 1)
                {
                    mutiTraining.GetComponent<Animation>().Stop();
                    mutiTraining.Find("eff").GetComponent<Graphic>().color = Color.white;
                    mutiTraining.Find("eff").gameObject.SetActive(false);
                    mutiTraining.gameObject.SetActive(true);
                    if(list.Count == 0)
                    {
                        m_pMutiLevelupButtonList.Add(mutiTraining);
                        
                        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(mutiTraining),HIDE_BUTTON_TIME, (uint)E_STATE_TYPE.Timer, null, this.executeLevelUpButtonCallback,this.exitLevelUpButtonCallback);
                        StateMachine.GetStateMachine().AddState(pBaseState);
                    }
                    else
                    {
                        TimeOutCondition condition = null;
                        for(int n = 0; n < list.Count; ++n)
                        {
                            condition = list[n].GetCondition<TimeOutCondition>();
                            ALFUtils.FadeObject(mutiTraining,1);
                            condition.Reset();
                            condition.SetRemainTime(HIDE_BUTTON_TIME);
                        }
                    }
                    text = mutiTraining.Find("cost").GetComponent<TMPro.TMP_Text>();
                    text.SetText(ALFUtils.NumberToString(totalCost));
                    
                    text = mutiTraining.Find("lv").GetComponent<TMPro.TMP_Text>();
                    text.SetText( pGameContext.GetLocalizingText("PLAYERINFO_INFO_BTN_LEVEL_UP") + $" x{iAmount}");
                }
                else
                {
                    for(int n = 0; n < list.Count; ++n)
                    {
                        list[n].Exit(true);
                    }
                }
            }
        }
        else
        {
            group.Find("training").gameObject.SetActive(false);
        }
    }

     public void UpdateTimer(float dt)
    {
        if(m_bRefreshUpdate && m_iCurrentAuctionId > 0 && m_bJoinAuction)
        {
            m_fCurrentAuctionRemainingTime -= dt;

            if(m_fCurrentAuctionRemainingTime <= 0)
            {
                Debug.Log("PlayerInfo-------------------------------------------UpdateTimer");
                m_bRefreshUpdate = false;
                m_fCurrentAuctionRemainingTime = 0;
                m_pRemainingTimeTexts[0].SetText("");
                m_pRemainingTimeTexts[1].SetText("");
                m_pRemainingTimeTexts[0].color = Color.white;
                m_pRemainingTimeTexts[1].color = Color.white;
                /**
                * 옥션에 입찰하지 않았다면, CloseAuction 호출 
                * 입찰을 하면 GameContext.UpdateTimer 에서 미리 처리를 진행함..
                */
                if(GameContext.getCtx().GetAuctionBiddingInfoByID(m_iCurrentAuctionId) == null)
                {
                    CloseAuction(m_iCurrentAuctionId);
                }
            }
            else
            {
                string[] strTimes = ALFUtils.SecondToString((int)m_fCurrentAuctionRemainingTime,true,true,true).Split(':');
                m_pRemainingTimeTexts[0].SetText(strTimes[strTimes.Length -2]);
                m_pRemainingTimeTexts[1].SetText(strTimes[strTimes.Length -1]);
                if(m_fCurrentAuctionRemainingTime < 10)
                {
                    m_pRemainingTimeTexts[0].color = Color.red;
                    m_pRemainingTimeTexts[1].color = Color.red;
                }
            }
        }
    
        if(m_pShareTime.gameObject.activeSelf)
        {
            float tic = GameContext.getCtx().GetExpireTimerByUI(this,0);
            if(tic > 0)
            {
                m_pShareTime.fillAmount = tic / 10f;
            }
        }
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
                data.CurrentAnimation.Play();
                data.AnimationList.RemoveAt(0);
            }
        }

        return bEnd;
    }
    IState exitChangeStarCallback(IState state)
    {
        LayoutManager.Instance.InteractableEnabledAll(null,true);
        BaseStateTarget target = state.GetTarget<BaseStateTarget>();
        Animation pAnimation = target.GetMainTarget<Animation>();
        pAnimation.Play("player_hide");
        pAnimation = m_pPlayerCard.Find("GlintEffect").GetComponent<Animation>();
        pAnimation.Stop();
        pAnimation.gameObject.SetActive(false);

        SetupPlayerAbilityInformation(true);
        UpdatePlayerInformation();
        UpdateTrainingPoint();
        m_eCurrentLevelUp = E_TRAINING_TYPE.MAX;
        return null;
    }

    bool executeLevelUpButtonCallback(IState state,float dt,bool bEnd)
    {
        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
        BaseStateTarget target = state.GetTarget<BaseStateTarget>();
        Transform tm = target.GetMainTarget<Transform>();
        if(condition.GetRemainTime() <= 1)
        {
            ALFUtils.FadeObject(tm,-dt);
        }
        
        return bEnd;
    }
    IState exitLevelUpButtonCallback(IState state)
    {
        BaseStateTarget target = state.GetTarget<BaseStateTarget>();
        Transform tm = target.GetMainTarget<Transform>();
        ALFUtils.FadeObject(tm,1);
        tm.gameObject.SetActive(false);
        for(int i = 0; i < m_pMutiLevelupButtonList.Count; ++i)
        {
            if(m_pMutiLevelupButtonList[i] == tm)
            {
                m_pMutiLevelupButtonList.RemoveAt(i);
            }
        }
        return null;
    }

    void SteupBiddingButton(ulong cash, ulong money)
    {
        GameContext pGameContext = GameContext.getCtx();
        ulong totalCash = pGameContext.GetTotalCash();
        ulong totalMoney = pGameContext.GetGameMoney();
        m_pBiddingButton.enabled = (cash - m_iMyBiddingCash <= totalCash) && (money - m_iMyBiddingMoney <= totalMoney);
        m_pBiddingButton.transform.Find("on").gameObject.SetActive(m_pBiddingButton.enabled);
        m_pBiddingButton.transform.Find("off").gameObject.SetActive(!m_pBiddingButton.enabled);
    }

    int GetTrainingLevelCount(uint total,E_TRAINING_TYPE eType,ref uint totalCost)
    {
        E_ABILINDEX i = (E_ABILINDEX)((int)E_ABILINDEX.AB_ATT_PASS + ((int)eType * 6));
        E_ABILINDEX end = i + 6;

        GameContext pGameContext = GameContext.getCtx();
        
        TrainingCostList pTrainingCostList = pGameContext.GetFlatBufferData<TrainingCostList>(E_DATA_TYPE.TrainingCost);
        TrainingCostItem? pTrainingCostItem = null;
        
        PlayerAbilityWeightConversionList pPlayerAbilityWeightConversionList = pGameContext.GetFlatBufferData<PlayerAbilityWeightConversionList>(E_DATA_TYPE.PlayerAbilityWeightConversion);
        PlayerAbilityWeightConversionItem? pPlayerAbilityWeightConversionItem = null;
        
        ulong totalPoint = pGameContext.GetTrainingPointByType(eType);
        int count = 0;
        uint totals = 0;
        uint totalAbility = 0;
        E_ABILINDEX n = E_ABILINDEX.AB_ATT_PASS;
        while(n < E_ABILINDEX.AB_END)
        {
            if(n < i || n >= end)
            {
                totals = 0;
                for(count =0; count < 6; ++count)
                {
                    if(m_pPlayerData.Ability[(int)n].Current > 0)
                    {
                        totals += (uint)m_pPlayerData.Ability[(int)n].Current;
                    }
                    
                    ++n;
                }
                
                if(totals > 0)
                {
                    pPlayerAbilityWeightConversionItem = pPlayerAbilityWeightConversionList.PlayerAbilityWeightConversionByKey(totals);
                    totalAbility += pPlayerAbilityWeightConversionItem.Value.Weight;
                }
            }
            else
            {
                ++n;
            }
        }
        
        count = 0;        
        totals = total;
        pPlayerAbilityWeightConversionItem = pPlayerAbilityWeightConversionList.PlayerAbilityWeightConversionByKey(totals);
        uint lastTotalAbility = totalAbility + pPlayerAbilityWeightConversionItem.Value.Weight;
        
        while(m_pPlayerData.Potential > lastTotalAbility )
        {
            pTrainingCostItem = pTrainingCostList.TrainingCostByKey((uint)totals);
            
            if(totalPoint >= pTrainingCostItem.Value.Point)
            {
                totalPoint -= pTrainingCostItem.Value.Point; 
                totalCost += pTrainingCostItem.Value.Point;
                ++count;
            }
            else
            {
                break;
            }

            if(count == 10)
            {
                break;
            }

            ++totals;
            if(totals >= 600)
            {
                break;
            }

            pPlayerAbilityWeightConversionItem = pPlayerAbilityWeightConversionList.PlayerAbilityWeightConversionByKey(totals);
            lastTotalAbility = totalAbility + pPlayerAbilityWeightConversionItem.Value.Weight;
        }

        return count;
    }

    void PlaytAbilityUpEffect()
    {
        CompleteEffectActor pEffectActor = null;
        Transform tm = null;
        E_TRAINING_TYPE eType = E_TRAINING_TYPE.attacking;
        int i =0;
        for(eType = E_TRAINING_TYPE.attacking; eType < E_TRAINING_TYPE.MAX; ++eType)
        {
            tm = m_pTabUIList[(int)E_PLAYER_INFO_TAB.information].Find(eType.ToString());
            while( i < ((int)eType * 6) + 6)
            {
                if(m_pAbilityValue[i] != m_pPlayerData.Ability[i].Current)
                {
                    pEffectActor = ActorManager.Instance.GetActor<CompleteEffectActor>("GaugeEffect"); 
                    pEffectActor.MainUI.SetParent(tm.Find(AbilityNodeNameList[i]).Find("gauge"),false);
                    pEffectActor.ChangeAnimation("eff");
                }
                ++i;
            }
        }
    }

    void SetupPlayerAbilityInformation(bool bMuti)
    {
        E_ABILINDEX eStart = E_ABILINDEX.AB_ATT_PASS;
        E_TRAINING_TYPE eType = E_TRAINING_TYPE.attacking;
        E_ABILINDEX eEnd = E_ABILINDEX.AB_ATT_PASS;
        Transform tm = null;
        bool bActive = (m_eTeamType == E_PLAYER_INFO_TYPE.my && m_pPlayerData.Status == 0) ? !m_pMainScene.IsMatch() : false;
        
        for(eType = E_TRAINING_TYPE.attacking; eType < E_TRAINING_TYPE.MAX; ++eType)
        {
            tm = m_pTabUIList[(int)E_PLAYER_INFO_TAB.information].Find(eType.ToString());
            tm.gameObject.SetActive(true);

            if((E_LOCATION)m_pPlayerData.Position == E_LOCATION.LOC_GK )
            {
                if(eType == E_TRAINING_TYPE.defending) continue;
            }
            else 
            {
                if(eType == E_TRAINING_TYPE.goalkeeping) continue;
            }
            
            tm.Find("mutiTraining").gameObject.SetActive(false);
            if(bActive)
            {
                UpdateTrainingCost(eType,bMuti);
            }
            else
            {
                tm.Find("training").gameObject.SetActive(false);
                UpdateTotalAbility(eType);
            }

            eStart = (E_ABILINDEX)((int)E_ABILINDEX.AB_ATT_PASS + ((int)eType * 6));
            eEnd = eStart + 6;
            
            while(eStart < eEnd)
            {
                m_pAbilityValue[(int)eStart] = m_pPlayerData.Ability[(int)eStart].Current;
                UpdatePlayerAbility(eStart,tm.Find(AbilityNodeNameList[(int)eStart]));
                ++eStart;
            }
        }
        
        if((E_LOCATION)m_pPlayerData.Position == E_LOCATION.LOC_GK)
        {
            RectTransform attacking = m_pTabUIList[(int)E_PLAYER_INFO_TAB.information].Find("attacking").GetComponent<RectTransform>();
            RectTransform defending = m_pTabUIList[(int)E_PLAYER_INFO_TAB.information].Find("defending").GetComponent<RectTransform>();
            attacking.anchoredPosition = defending.anchoredPosition;
            defending.gameObject.SetActive(false);
        }
        else
        {
            RectTransform goalkeeping = m_pTabUIList[(int)E_PLAYER_INFO_TAB.information].Find("goalkeeping").GetComponent<RectTransform>();
            RectTransform attacking = m_pTabUIList[(int)E_PLAYER_INFO_TAB.information].Find("attacking").GetComponent<RectTransform>();
            attacking.anchoredPosition = goalkeeping.anchoredPosition;
            goalkeeping.gameObject.SetActive(false);
        }
    }

    void UpdatePlayerAbility(E_ABILINDEX index, Transform gauge)
    {
        RectTransform item = gauge.Find("gauge").GetComponent<RectTransform>();
        
        float w = m_pPlayerData.Ability[(int)index].Current * 0.01f;
        float w1 = m_pPlayerData.Ability[(int)index].Origin * 0.01f;

        TMPro.TMP_Text text = gauge.Find("text").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_pPlayerData.Ability[(int)index].Current.ToString());
        if(w >= 0.9f)
        {
            text.color = Color.red;
        }
        else if(w >= 0.8f)
        {
            text.color = new Color(1,0.5333334f,0);
        }
        else
        {
            text.color = GameContext.GRAY;
        }

        item.Find("over").GetComponent<Image>().fillAmount = w;
        item.Find("origin").GetComponent<Image>().fillAmount = w1;
        if(w1 < w)
        {
            w = w1; 
        }

        item.Find("current").GetComponent<Image>().fillAmount = w;
        
        w1 = m_pPlayerData.Ability[(int)index].Current - m_pPlayerData.Ability[(int)index].Origin;
        text = gauge.Find("text_a").GetComponent<TMPro.TMP_Text>();
        if(w1 > 0)
        {
            text.SetText($"(+{(int)w1})");
            text.color = new Color(0,0.6156863f,0);
        }
        else if(w1 < 0)
        {
            text.SetText($"({(int)w1})");
            text.color = Color.red;
        }
        
        text.gameObject.SetActive(w1 != 0);
    }
    
    public void ShowTabUI(E_PLAYER_INFO_TAB eTab)
    {
        if(m_pMainScene.IsMatch() && eTab == E_PLAYER_INFO_TAB.offer)
        {
            m_pMainScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("MSG_TXT_DURING_MATCH"),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
            return;
        }

        int i = 0;
        int temp = (int)eTab;
        m_eCurrentTap = eTab;
        for(i = 0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i].gameObject.SetActive(temp == i);
            m_pTabButtonList[i].Find("on").gameObject.SetActive(temp == i);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = temp == i ? Color.white : GameContext.GRAY;
        }
        
        if(eTab == E_PLAYER_INFO_TAB.information)
        {
            UpdateTrainingPoint();
        }
        else if(eTab == E_PLAYER_INFO_TAB.record)
        {
            SetupRecordData();
        }
        else if(eTab == E_PLAYER_INFO_TAB.offer)
        {
            SetupOffer();
        }
        ClearTimerState();
    }

    void Bidding()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(!m_bBidding)
        {
            if(pGameContext.IsMaxPlayerCount())
            {
                m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_TRANSFER_MARKET_NOT_ENOUGH_CAPACITY"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                return;
            }
        }
        
        m_bBidding = true;
        JObject pJObject = new JObject();
        pJObject["type"] = E_SOKET.auctionBid.ToString();
        pJObject["auctionId"]= m_iCurrentAuctionId;
        JObject pJObject1 = new JObject();
        pJObject1["Id"] = pGameContext.GetClubID();
        pJObject1["Name"] = pGameContext.GetClubName();
        JArray pJArray = new JArray();
        byte[] emblemInfo = pGameContext.GetEmblemInfo();
        for(int i =0; i < emblemInfo.Length; ++i)
        {
            pJArray.Add(emblemInfo[i]);
        }
        pJObject1["Emblem"] = pJArray;
        pJObject1["Emotion"] = (byte)m_eSelectEmoji;
        pJObject1["Time"]=NetworkManager.GetGameServerTime().Ticks;

        pJObject["msg"] =pJObject1;
        SoketTime = Time.realtimeSinceStartup;
        NetworkManager.SendMessage(pJObject);
    }

    void RefreshRewardCallback(E_AD_STATUS eStatus, bool useSkipItem)//( GoogleMobileAds.Api.Reward reward)
    { 
        if(eStatus != E_AD_STATUS.RewardComplete) return;

        GameContext pGameContext = GameContext.getCtx();
        
        JObject pJObject = new JObject();
        pJObject["id"] = m_pPlayerData.Id;
        pJObject["skip"] = useSkipItem ? 1 : 0;
        
        pJObject["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
        pJObject["totalValue"] = pGameContext.GetTotalPlayerValue((long)SingleFunc.GetPlayerValue(m_pPlayerData));
        pJObject["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(m_pPlayerData,true);
        pJObject["avgAge"] = pGameContext.GetPlayerAvgAge(m_pPlayerData,true);
        pJObject["playerCount"] = pGameContext.GetTotalPlayerCount()+1;
        m_pMainScene.RequestAfterCall(E_REQUEST_ID.recruit_offer,pJObject,Close);
    }

    public void CurrentJoinAuctionRoom()
    {
        if(m_bFinishAuction) return;

        if(m_bJoinAuction)
        {
            AuctionBiddingInfoT pAuctionBiddingInfo = GameContext.getCtx().GetAuctionBiddingInfoByID(m_iCurrentAuctionId);
            if(pAuctionBiddingInfo == null)
            {
                JoinAuctionRoom();
            }
        }
    }

    public void CurrentLeaveAuctionRoom()
    {
        if(m_bJoinAuction)
        {
            AuctionBiddingInfoT pAuctionBiddingInfo = GameContext.getCtx().GetAuctionBiddingInfoByID(m_iCurrentAuctionId);
            if(pAuctionBiddingInfo == null)
            {
                LeaveAuctionRoom();
            }
        }
    }

    void JoinAuctionRoom()
    {
        JObject pJObject = new JObject();
        pJObject["type"] = E_SOKET.auctionJoin.ToString();
        pJObject["auctionId"]= m_iCurrentAuctionId;
        SoketTime = Time.realtimeSinceStartup;
        NetworkManager.SendMessage(pJObject);
    }

    void LeaveAuctionRoom()
    {
        JObject pJObject = new JObject();
        pJObject["type"] = E_SOKET.auctionLeave.ToString();
        pJObject["auctionId"]= m_iCurrentAuctionId;
        SoketTime = Time.realtimeSinceStartup;
        NetworkManager.SendMessage(pJObject);
    }

    void ClearTimerState()
    {
        int i = m_pMutiLevelupButtonList.Count;
        List<BaseState> list = null;
        while(i > 0)
        {
            --i;
            list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pMutiLevelupButtonList[i]);
            for(int n = 0; n < list.Count; ++n)
            {
                list[n].Exit(true);
            }
        }
        m_pMutiLevelupButtonList.Clear();
    }

    void SetupOffer()
    {
        GameContext pGameContext = GameContext.getCtx();
        int index = (int)E_PLAYER_INFO_TAB.offer;
        for(int i =0; i < m_pTabUIList[index].childCount; ++i)
        {
            m_pTabUIList[index].GetChild(i).gameObject.SetActive(false);
        }

        if(m_eTeamType == E_PLAYER_INFO_TYPE.my)
        {
            m_pTabUIList[index].Find("release").gameObject.SetActive(true);
            USERRANK.UserRankList pUserRankList = pGameContext.GetFlatBufferData<USERRANK.UserRankList>( E_DATA_TYPE.UserRank);
            USERRANK.UserRankItem? pUserRankItem = pUserRankList.UserRankByKey(1);
            if(pUserRankItem != null)
            {
                m_pTabUIList[index].Find("sell_at_transfer_market").gameObject.SetActive(m_pPlayerData.PotentialTier >= pUserRankItem.Value.PlayerTierMin + AUCTION_REGISTER_TIER);
            }
            else
            {
                m_pTabUIList[index].Find("sell_at_transfer_market").gameObject.SetActive(false);
            }

            
            UpdateOfferData();
        }
        else if(m_eTeamType == E_PLAYER_INFO_TYPE.bidding || m_eTeamType == E_PLAYER_INFO_TYPE.auction)
        {
            if(!m_bJoinAuction)
            {
                bool bSend = true;
                if(m_eTeamType == E_PLAYER_INFO_TYPE.bidding )
                {
                    AuctionBiddingInfoT pAuctionBiddingInfo = pGameContext.GetAuctionBiddingInfoByID(m_iCurrentAuctionId);
                    if(pAuctionBiddingInfo != null && pAuctionBiddingInfo.TExtend <= 0)
                    {
                        bSend = false;
                        if(!string.IsNullOrEmpty(pAuctionBiddingInfo.Msg))
                        {
                            m_pLastBiddingMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<BiddingMsgT>(pAuctionBiddingInfo.Msg);
                        }
                        
                        SetupAuctionLog(false);
                    }
                }
                
                if(bSend)
                {
                    JoinAuctionRoom();   
                }
            }
            else
            {
                m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("bidding").gameObject.SetActive(true);
            }
        }
        else if(m_eTeamType == E_PLAYER_INFO_TYPE.recuiting)
        {
            Transform tm = m_pTabUIList[index].Find("contact");
            tm.gameObject.SetActive(true);
            tm.Find("token").gameObject.SetActive(false);
            Button btn = tm.Find("sign").GetComponent<Button>();
            if(m_pPlayerData.Price > 0)
            {    
                tm.Find("money").gameObject.SetActive(true);
                btn.transform.Find("icon").gameObject.SetActive(false);
                TMPro.TMP_Text text = tm.Find("money/text").GetComponent<TMPro.TMP_Text>();
                text.SetText(ALFUtils.NumberToString(m_pPlayerData.Price));
                tm.Find("skip").gameObject.SetActive(false);
            }
            else
            {
                tm.Find("money").gameObject.SetActive(false);
                
                ulong count = pGameContext.GetItemCountByNO(GameContext.AD_SKIP_ID);
                if(count > 0)
                {
                    tm.Find("skip").gameObject.SetActive(true);
                    TMPro.TMP_Text text = tm.Find("skip/text").GetComponent<TMPro.TMP_Text>();
                    text.SetText($"x{count}");
                }
                else
                {
                    tm.Find("skip").gameObject.SetActive(false);
                    btn.transform.Find("icon").gameObject.SetActive(true);
                }
            }            
            
            bool bEnable = pGameContext.GetGameMoney() >= m_pPlayerData.Price;
            btn.transform.Find("on").gameObject.SetActive(bEnable);
            btn.transform.Find("off").gameObject.SetActive(!bEnable);
            btn.enabled = bEnable;
        }
        else if(m_eTeamType == E_PLAYER_INFO_TYPE.youth)
        {
            Transform tm = m_pTabUIList[index].Find("contact");
            tm.gameObject.SetActive(true);
            Button btn = tm.Find("sign").GetComponent<Button>();
            btn.transform.Find("icon").gameObject.SetActive(false);
            TMPro.TMP_Text text = tm.Find("money/text").GetComponent<TMPro.TMP_Text>();
            text.SetText(ALFUtils.NumberToString(m_pPlayerData.Price));
            tm.Find("money").gameObject.SetActive(true);
            tm.Find("token").gameObject.SetActive(true);
            tm.Find("skip").gameObject.SetActive(false);
            text = tm.Find("token/text").GetComponent<TMPro.TMP_Text>();
            text.SetText(m_pPlayerData.RecmdLetter.ToString());
            
            bool bEnable = pGameContext.GetGameMoney() >= m_pPlayerData.Price && pGameContext.GetYouthNominationCount() >= m_pPlayerData.RecmdLetter;
            btn.transform.Find("on").gameObject.SetActive(bEnable);
            btn.transform.Find("off").gameObject.SetActive(!bEnable);
            btn.enabled = bEnable;
        }
    }

    public PlayerT GetPlayerData()
    {
        return m_pPlayerData;
    }

    public ulong GetPlayerID()
    {
        if(m_pPlayerData == null)
        {
            return 0;
        }

        return m_pPlayerData.Id;
    }

    void UpdateOfferData()
    {
        GameContext pGameContext = GameContext.getCtx();
        bool isTrade = m_pPlayerData.Status != 0;
        bool isLineup = pGameContext.IsLineupPlayer(pGameContext.GetActiveLineUpType(),m_pPlayerData.Id) != 0;
        bool isNoTrade = pGameContext.IsTempLineupPlayer(m_pPlayerData.Id);

        // if(m_pPlayerData.Status == 10)//0: 일반, 10: 이적시장에 등록됨(10번대는 trade계열), 20: injury(20번대는 부상계열), 99: release(계약해지)
        Button pButton = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("release/release").GetComponent<Button>();
        pButton.enabled = !(isNoTrade || isTrade || isLineup);

        pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
        pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
        TMPro.TMP_Text text = pButton.transform.Find("title").GetComponent<TMPro.TMP_Text>();
        
        pButton = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("sell_at_transfer_market/btn/sell").GetComponent<Button>();
        pButton.gameObject.SetActive( !isTrade);
        pButton.enabled = !(isNoTrade || isLineup);
        pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
        pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
        text = pButton.transform.Find("title").GetComponent<TMPro.TMP_Text>();
        
        pButton = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("sell_at_transfer_market/sell/minus").GetComponent<Button>();
        pButton.enabled = !(isNoTrade || isLineup);
        pButton.GetComponent<Graphic>().color = pButton.enabled ? Color.white : GameContext.GRAY_W;
        pButton = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("sell_at_transfer_market/sell/plus").GetComponent<Button>();
        pButton.enabled = !(isNoTrade || isLineup);
        pButton.GetComponent<Graphic>().color = pButton.enabled ? Color.white : GameContext.GRAY_W;
        
        pButton.transform.parent.gameObject.SetActive(m_pPlayerData.Status != 10);
        pButton = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("sell_at_transfer_market/btn/cancel").GetComponent<Button>();
        pButton.gameObject.gameObject.SetActive( isTrade);
        pButton.enabled = !(isLineup);
        pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
        pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
        text = pButton.transform.Find("title").GetComponent<TMPro.TMP_Text>();
        
        text = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("release/comment").GetComponent<TMPro.TMP_Text>();
        text.SetText(pGameContext.GetLocalizingText("PLAYERINFO_OFFER_TXT_REALEASE_DESC"));
        text = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("release/cash/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(ALFUtils.NumberToString((ulong)(m_pPlayerData.Price * (pGameContext.GetConstValue(E_CONST_TYPE.playerReleasePriceRate) / 10000.0f))));
        
        text = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("sell_at_transfer_market/comment").GetComponent<TMPro.TMP_Text>();
        text.SetText(pGameContext.GetLocalizingText(m_pPlayerData.Status != 10 ? "PLAYERINFO_OFFER_TXT_SELL_MARKET_DESC":"PLAYERINFO_OFFER_TXT_SELL_MARKET_WATING_DESC"));
        
        text = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer].Find("sell_at_transfer_market/sell/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(ALFUtils.NumberToString(m_ulValue));
    }

    byte[] GetEmblemData()
    {
        if(m_eTeamType == E_PLAYER_INFO_TYPE.my)
        {
            return GameContext.getCtx().GetEmblemInfo();
        }
        else if(m_eTeamType == E_PLAYER_INFO_TYPE.away)
        {
            return GameContext.getCtx().GetClubEmblemDataInClubProfile(m_eTeamType);
        }
        else if(m_eTeamType == E_PLAYER_INFO_TYPE.match)
        {
            return GameContext.getCtx().GetMatchTeamEmblemData();
        }

        return null;
    }
    
    void UpdateTrainingPoint()
    {
        bool bActive = !m_pMainScene.IsMatch();
        
        if(bActive)
        {
            bActive = m_eTeamType == E_PLAYER_INFO_TYPE.my && m_pPlayerData.Status == 0;
            if(bActive)
            {
                GameContext pGameContext = GameContext.getCtx();
                for(int i =0; i < m_pTrainingPoints.Length; ++i)
                {
                    m_pTrainingPoints[i].SetText(ALFUtils.NumberToString(pGameContext.GetTrainingPointByType((E_TRAINING_TYPE)i)));
                }
            }
        }
        m_pTrainingPoint.gameObject.SetActive(bActive);
    }
    
    void RequestPlayerSeasonStats(byte matchType)
    {
        if(m_pPlayerSeasonStatsList.Count > 0 && m_pPlayerSeasonStatsList[0].matchType == matchType)
        {
            ShowTabUI(E_PLAYER_INFO_TAB.record);
        }
        else
        {
            m_pPlayerSeasonStatsList.Clear();
            if(m_pPlayerData.Club >= GameContext.VIRTUAL_ID)
            {
                JObject pJObject = new JObject();
                pJObject["player"] = m_pPlayerData.Id;
                pJObject["matchType"] = matchType;
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.playerStats_seasonStats,pJObject);
            }
            else
            {
                ShowTabUI(E_PLAYER_INFO_TAB.record);
            }
        }
    }
    
    public void Close()
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pCongratsFX,(uint)E_STATE_TYPE.Timer);
        m_pCongratsFX.gameObject.SetActive(false);
        for(int i=0; i < list.Count; ++i)
        {
            list[i].Exit(true);
        }

        Enable = false;
        m_bRefreshUpdate = false;
        m_iCurrentAuctionId = 0;
        m_bJoinAuction = false;
        ClearTimerState();
        m_pPlayerDataList.Clear();
        m_pPlayerSeasonStatsList.Clear();
        m_eCurrentLevelUp = E_TRAINING_TYPE.MAX;
        SingleFunc.HideAnimationDailog(MainUI,()=>{

            Button[] pButton= m_pTabUIList[(int)E_PLAYER_INFO_TAB.information].GetComponentsInChildren<Button>(true);
            for(int n = 0; n < pButton.Length; ++n)
            {
                pButton[n].GetComponent<Animation>().Stop();
            }
            m_pEmblemBake?.Dispose();
            MainUI.gameObject.SetActive(false);
            MainUI.Find("root").localScale = Vector3.one;
            LayoutManager.Instance.InteractableEnabledAll();
            if(m_pEndCallback != null)
            {
                m_pEndCallback();
            }
            m_pEndCallback = null;
        });
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        QuickPlayerItem pPlayerItem = null;
        if(index > iTarget)
        {
            pPlayerItem = m_pQuickPlayerItems[iTarget];
            m_pQuickPlayerItems[iTarget] = m_pQuickPlayerItems[index];
            i = iTarget +1;
            QuickPlayerItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pQuickPlayerItems[i];
                m_pQuickPlayerItems[i] = pPlayerItem;
                pPlayerItem = pTemp;
                ++i;
            }
            m_pQuickPlayerItems[index] = pPlayerItem;
            pPlayerItem = m_pQuickPlayerItems[iTarget];
        }
        else
        {
            pPlayerItem = m_pQuickPlayerItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pQuickPlayerItems[i -1] = m_pQuickPlayerItems[i];
                ++i;
            }

            m_pQuickPlayerItems[iTarget] = pPlayerItem;
        }
        
        i = m_iStartIndex[2] + iTarget + iCount;

        if(i < 0 || m_pPlayerDataList.Count <= i) return;

        pPlayerItem.UpdatePlayerInfo(m_pPlayerDataList[i]);
        if(m_iCurrentSelectPlayerIndex > 0 )
        {
            if(m_iCurrentSelectPlayerIndex == i)
            {
                m_pSelectQuickPlayer = pPlayerItem;
                m_pSelectQuickPlayer.On.SetActive(true);
            }
            else
            {
                pPlayerItem.On.SetActive(false);
            }
        }
    }

    void RecordScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        
        int eType = m_eMatchType == (byte)GameContext.LADDER_ID ? 0 : 1;
        int i = 0;
        List<RecordLogItem> list = null; 
        RecordLogItem pItem = null;
        if(eType == 0)
        {
            list = m_pLadderRecordLogItems;
        }
        else
        {
            list = m_pLeagueRecordLogItems;
        }

        if(index > iTarget)
        {
            pItem = list[iTarget];
            list[iTarget] = list[index];
            i = iTarget +1;
            RecordLogItem pTemp = null;
            while(i < index)
            {
                pTemp = list[i];
                list[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            list[index] = pItem;
            pItem = list[iTarget];
        }
        else
        {
            pItem = list[index];
            i = index +1;
            while(i <= iTarget)
            {
                list[i -1] = list[i];
                ++i;
            }

            list[iTarget] = pItem;
        }
        
        i = m_iStartIndex[2] + iTarget + iCount;

        if(i < 0 || m_pPlayerSeasonStatsList.Count <= i) return;

        pItem.UpdateInfo(m_pPlayerSeasonStatsList[i]);
    }

    void RecordScrollViewChangeValueEventCall( Vector2 value)
    {
        int index = m_eMatchType == (byte)GameContext.LADDER_ID ? 0 : 1;

        if(m_iTotalScrollItems[index] < m_pPlayerSeasonStatsList.Count && value.y != m_pPrevDir[index].y)
        {
            m_pScrollRect[index].ScrollViewChangeValue(value - m_pPrevDir[index],ref m_iStartIndex[index],RecordScrollViewChangeData);
            m_pPrevDir[index] = value;
        }
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_iTotalScrollItems[2] < m_pPlayerDataList.Count && value.x != m_pPrevDir[2].x)
        {
            m_pScrollRect[2].ScrollViewChangeValue(value - m_pPrevDir[2],ref m_iStartIndex[2],ScrollViewChangeData);
            m_pPrevDir[2] = value;
        }
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        if(m_pScrollRect[(int)E_TAB.quick] == root)
        {
            for(int i = 0; i < m_pQuickPlayerItems.Count; ++i)
            {
                if(m_pQuickPlayerItems[i].Target == tm)
                {
                    if(m_pQuickPlayerItems[i].Id == m_pPlayerData.Id) return;

                    if(m_pSelectQuickPlayer.On.activeSelf)
                    {
                        m_pSelectQuickPlayer.On.SetActive(false);
                    }
                    
                    m_pSelectQuickPlayer = m_pQuickPlayerItems[i];
                    m_pSelectQuickPlayer.On.SetActive(true);
                    m_iCurrentSelectPlayerIndex = m_iStartIndex[(int)E_TAB.quick] + i;

                    PlayerT pPlayer = m_pPlayerDataList[m_iCurrentSelectPlayerIndex];
                    
                    SetupPlayerInfoData(m_eTeamType, pPlayer);
                    if(m_eCurrentTap == E_PLAYER_INFO_TAB.record)
                    {
                        m_pPlayerSeasonStatsList.Clear();
                        RequestPlayerSeasonStats(m_eMatchType);
                    }
                    else if(m_eCurrentTap == E_PLAYER_INFO_TAB.offer)
                    {
                        SetupOffer();
                    }
                    return;
                }
            }
        }
    }

    void PlayerRelease()
    {
        GameContext pGameContext = GameContext.getCtx();
        long value = (long)m_pPlayerData.Price * -1;
        JObject pJObject = new JObject();
        pJObject["player"] = m_pPlayerData.Id;
        pJObject["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
        pJObject["totalValue"] = pGameContext.GetTotalPlayerValue(value);
        pJObject["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(m_pPlayerData, false);
        pJObject["avgAge"] = pGameContext.GetPlayerAvgAge(m_pPlayerData, false);
        pJObject["playerCount"] = pGameContext.GetTotalPlayerCount()-1;
        m_pMainScene.RequestAfterCall(E_REQUEST_ID.player_release,pJObject);
    }

    void ChangeCancelButton()
    {
        m_pMainScene.RequestAfterCall( E_REQUEST_ID.auction_status,null);
        Close();
    }
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(root.gameObject.name)
        {
            case "tip":
            {
                m_pMainScene.ShowGameTip("game_tip_playerinfo_title");
            }
            break;
            case "playerInfo":
            {
                if(sender == m_pTraining)
                {
                    m_pMainScene.ShowPositionTrainingPopup().SetPlayerData(m_pPlayerData);
                }
                else if(sender == m_pCompare)
                {
                    m_pMainScene.ShowPlayerComparisonPopup(m_pPlayerData,GetEmblemData());
                }
                else if(sender.name == "edit")
                {
                    m_pMainScene.ShowPlayerEditPopup(m_pPlayerData);
                }
                else if(sender.name == "share")
                {
                    GameContext pGameContext = GameContext.getCtx();
                    m_pMainScene.ShowConfirmPopup( pGameContext.GetLocalizingText("DIALOG_PLAYERPROFILESHARE_TITLE"),pGameContext.GetLocalizingText("DIALOG_PLAYERPROFILESHARE_TXT"),pGameContext.GetLocalizingText("DIALOG_PLAYERPROFILESHARE_BTN_SHARE"),pGameContext.GetLocalizingText("DIALOG_PLAYERPROFILESHARE_BTN_CANCEL"),false,() =>{
                        m_pShare.enabled = false;
                        pGameContext.AddExpireTimer(this,0,10);
                        m_pShareTime.gameObject.SetActive(true);
                        m_pShareTime.fillAmount = 1;
                        m_pMainScene.SharePlayerInfoChatUI(m_pPlayerData.Id);
                    });
                }
                
            }
            break;
            case "tabs":
            {
                if(sender.name == "record" && m_eTeamType < E_PLAYER_INFO_TYPE.match)
                {
                    m_eMatchType = (byte)GameContext.LADDER_ID;
                    if(m_pPlayerSeasonStatsList.Count > 0)
                    {
                        m_eMatchType = m_pPlayerSeasonStatsList[0].matchType > GameContext.LADDER_ID ? (byte)GameContext.LEAGUE_ID : (byte)GameContext.LADDER_ID;
                    }
                    RequestPlayerSeasonStats(m_eMatchType);
                }
                else
                {
                    ShowTabUI((E_PLAYER_INFO_TAB)Enum.Parse(typeof(E_PLAYER_INFO_TAB), sender.name));
                }
                SetupScrollTapButton();
            }
            break;
            case "information":
            {
                Animation pAnimation = sender.GetComponent<Animation>();
                pAnimation.Stop();
                sender.transform.Find("eff").GetComponent<Graphic>().color = Color.white;
                sender.transform.Find("eff").gameObject.SetActive(false);
                pAnimation.Play();
                Transform tm = sender.transform.parent;

                m_eCurrentLevelUp = (E_TRAINING_TYPE)Enum.Parse(typeof(E_TRAINING_TYPE), tm.gameObject.name);
                GameContext pGameContext = GameContext.getCtx();
                JObject pJObject = new JObject();
                pJObject["player"] = m_pPlayerData.Id;
                pJObject["category"] = (byte)m_eCurrentLevelUp;
                
                int iAmount = 1;
                if(sender.name == "mutiTraining")
                {
                    if(m_iAmountList.ContainsKey(m_eCurrentLevelUp))
                    {
                        iAmount = m_iAmountList[m_eCurrentLevelUp];
                    }
                }
                
                pJObject["amount"] = iAmount;
                pJObject["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
                pJObject["totalValue"] = pGameContext.GetTotalPlayerValue(0);
                pJObject["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(null,true);
                pJObject["avgAge"] = pGameContext.GetPlayerAvgAge(null,true);
                pJObject["playerCount"] = pGameContext.GetTotalPlayerCount();

                m_pMainScene.RequestAfterCall(E_REQUEST_ID.player_abilityUp,pJObject);
                if(sender.name == "mutiTraining")
                {
                    if(m_iAmountList.ContainsKey(m_eCurrentLevelUp))
                    {
                        m_iAmountList.Remove(m_eCurrentLevelUp);
                    }
                }
            }
            break;
            case "training":
            {
                m_pMainScene.ShowItemTipPopup(sender);
            }
            break;
            case "record":
            {
                byte eType = sender.name == "ladder" ? (byte)GameContext.LADDER_ID : (byte)GameContext.LEAGUE_ID;
                if(eType == m_eMatchType) return;

                m_eMatchType = eType;
                if(m_eTeamType < E_PLAYER_INFO_TYPE.match)
                {
                    m_pPlayerSeasonStatsList.Clear();
                    RequestPlayerSeasonStats(m_eMatchType);
                }
                
                SetupScrollTapButton();
            }
            break;
            case "offer":
            {
                switch(sender.name)
                {
                    case "angry":
                    case "sad":
                    case "smile":
                    case "thinking":
                    {
                        E_EMOJI eType = (E_EMOJI)Enum.Parse(typeof(E_EMOJI),sender.name);
                        int index = 0;
                        if(m_eSelectEmoji != E_EMOJI.NONE)
                        {
                            index = (int)((int)m_eSelectEmoji * 0.1f) -1;
                            m_EmojiIcon[index].transform.Find("off").gameObject.SetActive(true);
                            m_EmojiIcon[index].transform.Find("on").gameObject.SetActive(false);
                        }

                        if(m_eSelectEmoji != eType)
                        {
                            m_eSelectEmoji = eType;
                            index = (int)((int)m_eSelectEmoji * 0.1f) -1;
                            m_EmojiIcon[index].transform.Find("off").gameObject.SetActive(false);
                            m_EmojiIcon[index].transform.Find("on").gameObject.SetActive(true);
                        }
                        else
                        {
                            m_eSelectEmoji = E_EMOJI.NONE;
                        }
                    }
                    break;
                    case "bid":
                    {
                        Bidding();
                    }
                    break;
                    case "sell":
                    {
                        if(m_eTeamType == E_PLAYER_INFO_TYPE.my)
                        {
                            JObject pJObject = new JObject();
                            pJObject["player"] = m_pPlayerData.Id;
                            pJObject["price"] = m_ulSellValue;
                            m_pMainScene.RequestAfterCall(E_REQUEST_ID.auction_register,pJObject,ChangeCancelButton);
                        }
                    }
                    break;
                    case "cancel":
                    {
                        if(m_eTeamType == E_PLAYER_INFO_TYPE.my)
                        {
                            GameContext pGameContext = GameContext.getCtx();
                            uint id = pGameContext.GetAuctionSellInfoIDByPlayerID(m_pPlayerData.Id);
                            if(id > 0)
                            {
                                JObject pJObject = new JObject();
                                pJObject["auctionId"] = id;
                                m_pMainScene.RequestAfterCall(E_REQUEST_ID.auction_cancel,pJObject);
                            }
                        }
                    }
                    break;
                    case "sign":
                    {
                        GameContext pGameContext = GameContext.getCtx();
                        
                        if(!pGameContext.IsMaxPlayerCount())
                        {
                            if(m_eTeamType == E_PLAYER_INFO_TYPE.recuiting)
                            {
                                if(m_pPlayerData.Price > 0)
                                {
                                    if(pGameContext.GetGameMoney() >= m_pPlayerData.Price )
                                    {
                                        JObject pJObject = new JObject();
                                        pJObject["id"] = m_pPlayerData.Id;
                                        pJObject["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
                                        pJObject["totalValue"] = pGameContext.GetTotalPlayerValue((long)SingleFunc.GetPlayerValue(m_pPlayerData));
                                        pJObject["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(m_pPlayerData,true);
                                        pJObject["avgAge"] = pGameContext.GetPlayerAvgAge(m_pPlayerData,true);
                                        pJObject["playerCount"] = pGameContext.GetTotalPlayerCount()+1;
                                        m_pMainScene.RequestAfterCall(E_REQUEST_ID.recruit_offer,pJObject,Close);
                                    }
                                }
                                else
                                {
                                    if(!pGameContext.ShowRewardVideo((E_AD_STATUS eStatus)=>{ RefreshRewardCallback(eStatus,false);} ))
                                    {
                                        m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_AD_NOT_PREPARED"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                                    }
                                }
                            }
                            else if(m_eTeamType == E_PLAYER_INFO_TYPE.youth)
                            {
                                bool bEnable = pGameContext.GetGameMoney() >= m_pPlayerData.Price && pGameContext.GetYouthNominationCount() >= m_pPlayerData.RecmdLetter;
                                if(bEnable)
                                {
                                    long value = (long)SingleFunc.GetPlayerValue(m_pPlayerData);
                                    JObject pJObject = new JObject();
                                    pJObject["id"] = m_pPlayerData.Id;
                                    pJObject["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
                                    pJObject["totalValue"] = pGameContext.GetTotalPlayerValue(value);
                                    pJObject["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(m_pPlayerData,true);
                                    pJObject["avgAge"] = pGameContext.GetPlayerAvgAge(m_pPlayerData,true);
                                    pJObject["playerCount"] = pGameContext.GetTotalPlayerCount()+1;
                                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.youth_offer,pJObject,Close);
                                }
                            }
                        }
                        else
                        {
                            m_pMainScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("EXPANDSQUAD_TXT_EXPAND_DESC"),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
                        }
                    }
                    break;
                    case "skip":
                    {
                        GameContext pGameContext = GameContext.getCtx();
                        
                        if(!pGameContext.IsMaxPlayerCount())
                        {
                            if(pGameContext.GetItemCountByNO(GameContext.AD_SKIP_ID) > 0)
                            {
                                RefreshRewardCallback(E_AD_STATUS.RewardComplete,true);
                            }
                        }
                        else
                        {
                            m_pMainScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("EXPANDSQUAD_TXT_EXPAND_DESC"),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
                        }
                    }
                    break;
                    case "plus":
                    {
                        if(m_ulSellValue >= m_ulValueMax) return;
                        
                        TMPro.TMP_Text text = sender.transform.parent.Find("text").GetComponent<TMPro.TMP_Text>();
                        
                        m_ulSellValue += (ulong)(m_ulValue * (float)(GameContext.getCtx().GetConstValue(E_CONST_TYPE.playerAuctionStartPriceInterval) / 10000f));
                        if(m_ulSellValue > m_ulValueMax)
                        {
                            m_ulSellValue = m_ulValueMax;
                        }
                        
                        text.SetText(ALFUtils.NumberToString(m_ulSellValue));
                    }
                    break;
                    case "minus":
                    {
                        if(m_ulSellValue <= m_ulValueMin) return;

                        TMPro.TMP_Text text = sender.transform.parent.Find("text").GetComponent<TMPro.TMP_Text>();
                        m_ulSellValue -= (ulong)(m_ulValue * 0.05f);
                        if(m_ulSellValue < m_ulValueMin)
                        {
                            m_ulSellValue = m_ulValueMin;
                        }
                        text.SetText(ALFUtils.NumberToString(m_ulSellValue));
                    }
                    break;
                    case "release":
                    {
                        GameContext pGameContext = GameContext.getCtx();
                        m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_RELEASE_TITLE"),pGameContext.GetLocalizingText("DIALOG_RELEASE_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,PlayerRelease);
                    }
                    break;
                }
            }
            break;
            default:
            {
                if(sender.name == "close" || sender.name == "back")
                {
                    Close();

                    if(m_iCurrentAuctionId > 0 && m_bJoinAuction && !m_bBidding)
                    {
                        LeaveAuctionRoom();
                    }
                }
            }
            break;
        }
    }
    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
        if( data == null ) return;

        if(bSuccess)
        {
            E_REQUEST_ID eID = (E_REQUEST_ID)data.Id;

            switch(eID)
            {
                case E_REQUEST_ID.auction_reward:
                case E_REQUEST_ID.auction_trade:
                case E_REQUEST_ID.auction_cancel:
                case E_REQUEST_ID.auction_withdraw:
                {
                    if(!MainUI.gameObject.activeInHierarchy) return;

                    if(m_eTeamType == E_PLAYER_INFO_TYPE.my)
                    {
                        GameContext pGameContext = GameContext.getCtx();
                        m_pPlayerDataList = pGameContext.GetTotalPlayerList().ToList();

                        if(eID == E_REQUEST_ID.auction_cancel)
                        {
                            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_AUCTION_CANCELED"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                            UpdateOfferData();
                        }
                    }
                }
                break;
                case E_REQUEST_ID.playerStats_seasonStats:
                {
                    if(!MainUI.gameObject.activeInHierarchy) return;

                    m_pPlayerSeasonStatsList.Clear();
                    if(data.Json.ContainsKey("records") && data.Json["records"].Type != JTokenType.Null)
                    {
                        JArray pArray = (JArray)data.Json["records"];
                        for(int i = 0; i < pArray.Count; ++i)
                        {
                            m_pPlayerSeasonStatsList.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerSeasonStats>(pArray[i].ToString()));
                            m_eMatchType = m_pPlayerSeasonStatsList[0].matchType > GameContext.LADDER_ID ? (byte)GameContext.LEAGUE_ID : (byte)GameContext.LADDER_ID;
                        }
                    }

                    ShowTabUI(E_PLAYER_INFO_TAB.record);
                }
                break;
                case E_REQUEST_ID.player_positionFamiliarUp:
                {
                    if(!MainUI.gameObject.activeInHierarchy) return;

                    UpdatePlayerInformation();
                    SoundManager.Instance.PlaySFX("sfx_player_training");
                }
                break;
                case E_REQUEST_ID.player_abilityUp:
                {
                    if(!MainUI.gameObject.activeInHierarchy) return;

                    GameContext pGameContext = GameContext.getCtx();

                    m_iCurrentStarList = CheckChangeStar();
                    PlaytAbilityUpEffect();
                    SingleFunc.SetupQuality(m_pPlayerData,m_QualityGroup,false,E_ALIGN.Left);
                    bool bAni = ChangeStarAnimation(CheckChangeStar());
                    if(!bAni)
                    {
                        SetupPlayerAbilityInformation(true);
                        UpdatePlayerInformation();
                        UpdateTrainingPoint();
                        m_eCurrentLevelUp = E_TRAINING_TYPE.MAX;
                    }

                    if(m_pPlayerData.AbilityTier == m_pPlayerData.PotentialTier)
                    {
                        m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("ALERT_TXT_PLAYER_ABILITY_MAX"),null);
                        ParticleSystem pParticleSystem = m_pPlayerCard.GetComponentInChildren<ParticleSystem>();
                        pParticleSystem.Play(true);
                        UIParticleSystem[] pList = pParticleSystem.GetComponentsInChildren<UIParticleSystem>(true);
                        for(int i =0; i < pList.Length; ++i)
                        {
                            pList[i].StartParticleEmission();
                        }

                        if(bAni)
                        {
                            bAni = false;
                        }
                        else
                        {
                            LayoutManager.Instance.InteractableDisableAll(null,true);
                            Animation pAnimation = m_pPlayerCard.GetComponent<Animation>();
                            pAnimation.Play();
                            
                            StateMachine.GetStateMachine().AddState(BaseState.GetInstance(new BaseStateTarget(pAnimation),pAnimation["player_scale"].length +2, (uint)E_STATE_TYPE.Timer, null, null,exitChangeStarCallback));
                            pAnimation = m_pPlayerCard.Find("GlintEffect").GetComponent<Animation>();
                            pAnimation.gameObject.SetActive(true);
                            pAnimation.Play();
                        }
                    }
                    
                    if(bAni)
                    {
                        m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_PLAYER_ABILITY_UP"),null);
                    }
                    
                    SoundManager.Instance.PlaySFX("sfx_player_training");
                    
                    if(m_pSelectQuickPlayer != null)
                    {
                        JArray pArray = (JArray)data.Json["players"];
                        GameContext.getCtx().UpdatePlayer(m_pPlayerDataList[m_iCurrentSelectPlayerIndex], (JObject)pArray[0]);
                        
                        for(int i = 0; i < m_pQuickPlayerItems.Count; ++i)
                        {
                            if(m_pQuickPlayerItems[i].Id == m_pPlayerDataList[m_iCurrentSelectPlayerIndex].Id)
                            {
                                m_pQuickPlayerItems[i].UpdatePlayerInfo(m_pPlayerDataList[m_iCurrentSelectPlayerIndex]);
                                return;
                            }
                        }
                    }
                }
                break;
                case E_REQUEST_ID.player_release:
                {
                    if(!MainUI.gameObject.activeInHierarchy) return;

                    Close();
                }
                break;
                case E_REQUEST_ID.player_changeProfile:
                {
                    if(!MainUI.gameObject.activeInHierarchy) return;

                    m_PlayerName.SetText($"{m_pPlayerData.Forename} {m_pPlayerData.Surname}");
                    GameContext pGameContext = GameContext.getCtx();
                    
                    NATION_CODE code = (NATION_CODE)Enum.Parse(typeof(NATION_CODE),m_pPlayerData.Nation);
                    PlayerNationalityItem? pPlayerNationalityItem = pGameContext.GetPlayerNationDataByCode(code);
                    m_Nation.SetText(pGameContext.GetLocalizingText(pPlayerNationalityItem.Value.List(0).Value.NationName));
                    SingleFunc.SetupPlayerCard(m_pPlayerData,m_pPlayerCard,E_ALIGN.Left);
                    if(m_pPlayerDataList != null)
                    {
                        JArray pArray = (JArray)data.Json["players"];
                        JObject pItem = null;
                        ulong no = 0;

                        PlayerT pPlayer = null;
                        for(int i =0; i < pArray.Count; ++i)
                        {
                            pItem = (JObject)pArray[i];
                            no = (ulong)pItem["id"];

                            int index =0;
                            pPlayer = ALFUtils.BinarySearch<PlayerT>(m_pPlayerDataList,(PlayerT d)=> { return d.Id.CompareTo(m_pPlayerData.Id);},ref index);
                            pGameContext.UpdatePlayer(pPlayer, pItem);
                            
                            for(int n = 0; n < m_pQuickPlayerItems.Count; ++n)
                            {
                                if(m_pQuickPlayerItems[n].Id == no)
                                {
                                    m_pQuickPlayerItems[n].UpdatePlayerName(pItem);
                                    break;
                                }
                            }
                        }
                    }
                }
                break;
                case E_REQUEST_ID.player_profile:
                {
                    ALFUtils.Assert((data.Json.ContainsKey("players") && data.Json["players"].Type != JTokenType.Null), "NetworkProcessor players = null!!");

                    JArray pArray = (JArray)data.Json["players"];
                    ALFUtils.Assert(pArray.Count == 1, "pArray.Count != 1");
                    
                    ulong no = (ulong)pArray[0]["id"];
                    GameContext pGameContext = GameContext.getCtx();
                    int index = 0;
                    E_PLAYER_INFO_TYPE eType = ALFUtils.BinarySearch<PlayerT>(pGameContext.GetTotalPlayerList(),(PlayerT d)=> { return d.Id.CompareTo(no);},ref index) == null ? E_PLAYER_INFO_TYPE.away : E_PLAYER_INFO_TYPE.my;

                    PlayerT pPlayer = new PlayerT();
                    pPlayer.PositionFamiliars = new List<byte>();
                    pPlayer.Ability = new List<PlayerAbilityT>();
                    
                    for(int n =0; n < (int)E_ABILINDEX.AB_END; ++n)
                    {
                        pPlayer.Ability.Add(new PlayerAbilityT());
                    }
                    pPlayer.Id = no;
                    pGameContext.UpdatePlayer(pPlayer,(JObject)pArray[0]);
                    SetupPlayerInfoData(eType, pPlayer);
                    if(eType == E_PLAYER_INFO_TYPE.my)
                    {
                        SetupQuickPlayerInfoData(pGameContext.GetTotalPlayerList());
                        MoveQuickPlayer();
                    }
                    else
                    {
                        SetupQuickPlayerInfoData(null);
                    }
                }
                break;
            }
        }
    }

    IState exitFX(IState state)
    {
        JObject pJObject = new JObject();
        JArray pJArray = new JArray();
        pJArray.Add(m_iCurrentAuctionId);
        pJObject["auctionIds"] = pJArray;
        m_pMainScene.RequestAfterCall(E_REQUEST_ID.auction_trade,pJObject);
        return null;
    }

    public void IsCloseAuctionId(uint id)
    {
        if(m_iCurrentAuctionId == id)
        {
            Close();
        }
    }

    public bool CloseAuction(uint id)
    {
        /**
        * 현재 진행중인 경매를 체크한다.
        */
        if(m_iCurrentAuctionId != id) return false;
        if(m_bFinishAuction) return false;

        int i =0;
        for(i= 0; i < m_EmojiIcon.Length; ++i)
        {
            m_EmojiIcon[i].enabled = false;
            m_EmojiIcon[i].transform.Find("on").gameObject.SetActive(false);
            m_EmojiIcon[i].transform.Find("off").gameObject.SetActive(true);
        }

        m_pBiddingButton.enabled =false;
        m_pBiddingButton.transform.Find("on").gameObject.SetActive(false);
        m_pBiddingButton.transform.Find("off").gameObject.SetActive(true);
        bool bSend = false;
        m_bFinishAuction = true;
        JObject pJObject = new JObject();
        pJObject["type"] = E_SOKET.auctionLeave.ToString();
        pJObject["auctionId"] = m_iCurrentAuctionId;
        SoketTime = Time.realtimeSinceStartup;
        NetworkManager.SendMessage(pJObject);

        if(m_pLastBiddingPlayer.gameObject.activeSelf)
        {
            if(m_bBidding)
            {
                bSend = true;
                if(m_iMyBiddingCash == m_pLastBiddingMsg.Token && m_iMyBiddingMoney == m_pLastBiddingMsg.Gold)
                {
                    if(m_eCurrentTap == E_PLAYER_INFO_TAB.offer)
                    {
                        m_pCongratsFX.gameObject.SetActive(true);
                        ParticleSystem pParticleSystem = m_pCongratsFX.GetComponent<ParticleSystem>();
                        pParticleSystem.Play(true);
                        UIParticleSystem[] list = m_pCongratsFX.GetComponentsInChildren<UIParticleSystem>(true);
                        for(i =0; i < list.Length; ++i)
                        {
                            list[i].StartParticleEmission();
                        }
                        m_pCongratsFX.Play();
                        BaseState m_pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pCongratsFX),pParticleSystem.main.duration, (uint)E_STATE_TYPE.Timer, null, null,exitFX);
                        StateMachine.GetStateMachine().AddState(m_pBaseState);
                    }
                    else
                    {
                        exitFX(null);
                    }
                }
                else
                {
                    pJObject = new JObject();
                    JArray pJArray = new JArray();
                    pJArray.Add(m_iCurrentAuctionId);
                    pJObject["auctionIds"] = pJArray;
                    NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.auction_refund, GameContext.getCtx().GetNetworkAPI(E_REQUEST_ID.auction_refund),false,true,null,pJObject);
                    m_pCongratsFX.gameObject.SetActive(false);
                }
            }
        }
        
        SetupAuctionLog(false);
        return bSend;
    }

    void BidBroadcast(JObject data)
    {
        if(data.ContainsKey("payload") && data["payload"].Type != JTokenType.Null)
        {
            JObject payload = (JObject)data["payload"];
            m_pLastBiddingMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<BiddingMsgT>(payload["data"].ToString());
            m_eSelectEmoji = E_EMOJI.NONE;
            int i =0;            
            for(i =0; i < m_EmojiIcon.Length; ++i)
            {
                m_EmojiIcon[i].enabled = true;
                m_EmojiIcon[i].transform.Find("on").gameObject.SetActive(false);
                m_EmojiIcon[i].transform.Find("off").gameObject.SetActive(true);
            }
            
            GameContext pGameContext = GameContext.getCtx();
            m_pLastBiddingPlayer.gameObject.SetActive(true);
            
            ulong money =(ulong)payload["finalGold"];
            uint cash = (uint)payload["finalToken"];
            if(m_pLastBiddingMsg.Id == pGameContext.GetClubID())
            {
               m_iMyBiddingCash = cash;
               m_iMyBiddingMoney = money;
            }
            
            m_pLastBiddingMsg.Gold = money;
            m_pLastBiddingMsg.Token = cash;

            m_pCurrentBidding[0].SetText(string.Format("{0:#,0}", cash));
            m_pCurrentBidding[1].SetText(ALFUtils.NumberToString(money));

            m_pEmblemBake.SetupEmblemData(m_pLastBiddingMsg.Emblem.ToArray());
            m_pBiddingTitle.SetText(pGameContext.GetLocalizingText("PLAYERINFO_OFFER_TXT_TOP_BIDDER"));
            m_pCurrentName.SetText(m_pLastBiddingMsg.Name);

            ShowBiddingNotice(m_pLastBiddingMsg,true);

            money += (ulong)(money * 0.01f);
            cash += 1;
            m_pNextBidding[0].SetText(string.Format("{0:#,0}", cash));
            m_pNextBidding[1].SetText(ALFUtils.NumberToString(money));

            m_pCurrentTotalCash.SetText(string.Format("{0:#,0}", pGameContext.GetTotalCash()));
            m_pCurrentTotalMoney.SetText(ALFUtils.NumberToString(pGameContext.GetGameMoney()));
            
            

            m_fCurrentAuctionRemainingTime = (float)(NetworkManager.ConvertLocalGameTimeTick(payload["tExtend"].ToString()) - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond;
            m_bRefreshUpdate = m_fCurrentAuctionRemainingTime > 0;
            Debug.Log($"m_fCurrentAuctionRemainingTime------------------------------------:{m_fCurrentAuctionRemainingTime}");
            if(m_fCurrentAuctionRemainingTime < 10)
            {
                string[] strTimes = ALFUtils.SecondToString((int)m_fCurrentAuctionRemainingTime,true,true,true).Split(':');
                m_pRemainingTimeTexts[0].SetText(strTimes[strTimes.Length -2]);
                m_pRemainingTimeTexts[1].SetText(strTimes[strTimes.Length -1]);
            }
            SteupBiddingButton(cash, money);
        }
    }

    void SetupAuctionLog(bool bShow)
    {
        m_eSelectEmoji = E_EMOJI.NONE;
        m_bJoinAuction = true;
        Transform tm = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer];
        tm = tm.Find("bidding");
        tm.gameObject.SetActive(true);

        GameContext pGameContext = GameContext.getCtx();
        
        int i =0;
        for(i =0; i < m_pBiddingNotice.Length; ++i)
        {
            m_pBiddingNotice[i].Hide();
        }
        
        if(m_pLastBiddingMsg == null)
        {
            m_pLastBiddingPlayer.gameObject.SetActive(false);
        }
        else
        {
            m_pLastBiddingPlayer.gameObject.SetActive(true);
            m_pClubName.gameObject.SetActive(true);
            m_pEmblemBake.SetupEmblemData(m_pLastBiddingMsg.Emblem.ToArray());
            m_pBiddingTitle.SetText(pGameContext.GetLocalizingText("PLAYERINFO_OFFER_TXT_FINALLY_MADE"));
            m_pClubName.SetText(string.Format(pGameContext.GetLocalizingText("PLAYERINFO_OFFER_TXT_SIGN_TEAM"),m_pLastBiddingMsg.Name));
            m_pCurrentName.SetText($"{m_pPlayerData.Forename} {m_pPlayerData.Surname}");
            m_pLastBiddingPlayer.Find("box").gameObject.SetActive(bShow);
        }

        m_pNextBidding[0].SetText("");
        m_pNextBidding[1].SetText("");

        m_pBiddingButton.enabled = false;
        m_pBiddingButton.transform.Find("on").gameObject.SetActive(m_pBiddingButton.enabled);
        m_pBiddingButton.transform.Find("off").gameObject.SetActive(!m_pBiddingButton.enabled);

        m_pCurrentTotalCash.SetText(string.Format("{0:#,0}", pGameContext.GetTotalCash()));
        m_pRemainingTimeTexts[0].SetText("");
        m_pRemainingTimeTexts[1].SetText("");
    }

    void AuctionJoin(JObject data)
    { 
        m_iMyBiddingCash = 0;
        m_iMyBiddingMoney = 0;
        m_pLastBiddingMsg = null;
        
        m_pEmblemBake.gameObject.SetActive(true);

        m_eSelectEmoji = E_EMOJI.NONE;
        m_bJoinAuction = true;
        m_bFinishAuction = false;
        JObject payload = (JObject)data["payload"];
        Transform tm = m_pTabUIList[(int)E_PLAYER_INFO_TAB.offer];
        tm = tm.Find("bidding");
        tm.gameObject.SetActive(true);

        GameContext pGameContext = GameContext.getCtx();
        AuctionBiddingInfoT pAuctionBiddingInfo = pGameContext.GetAuctionBiddingInfoByID(m_iCurrentAuctionId);

        if(pAuctionBiddingInfo != null)
        {
            m_iMyBiddingCash = pAuctionBiddingInfo.Token;
            m_iMyBiddingMoney = pAuctionBiddingInfo.Gold;
            m_bBidding = m_iMyBiddingCash > 0 && m_iMyBiddingMoney > 0;
        }

        JArray pArray = (JArray)payload["bids"];
        
        int i =0;
        for(i =0; i < m_pBiddingNotice.Length; ++i)
        {
            m_pBiddingNotice[i].Hide();
        }

        for(i =0; i < m_EmojiIcon.Length; ++i)
        {
            m_EmojiIcon[i].enabled = true;
            m_EmojiIcon[i].transform.Find("on").gameObject.SetActive(false);
            m_EmojiIcon[i].transform.Find("off").gameObject.SetActive(true);
        }
        
        ulong money =0;
        uint cash =0;
        m_pLastBiddingPlayer.Find("box").gameObject.SetActive(true);
        
        if(pArray.Count > 0)
        {
            List<BiddingMsgT> list = new List<BiddingMsgT>();
            JObject item = null;
            m_pLastBiddingPlayer.gameObject.SetActive(true);
            int index = 0;
            for(i =0; i < pArray.Count; ++i)
            {
                item = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>((string)pArray[i], new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
                m_pLastBiddingMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<BiddingMsgT>(item["data"].ToString());
                m_pLastBiddingMsg.Gold = (ulong)item["gold"];
                m_pLastBiddingMsg.Token = (uint)item["token"];
                if(money < m_pLastBiddingMsg.Gold)
                {
                    money = m_pLastBiddingMsg.Gold;
                    index = i;
                }
                
                bool bAdd = true;
                for(int n = 0; n < list.Count; ++n)
                {
                    if(list[n].Id == m_pLastBiddingMsg.Id )
                    {
                        bAdd = false;
                        if(list[n].Gold < m_pLastBiddingMsg.Gold && list[n].Token < m_pLastBiddingMsg.Token)
                        {
                            list[n] = m_pLastBiddingMsg;
                            break;
                        }
                    }   
                }

                if(bAdd)
                {
                    list.Add(m_pLastBiddingMsg);
                }
            }

            for(i =0; i < list.Count; ++i)
            {
                ShowBiddingNotice(list[i],false);
            }
            
            item = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>((string)pArray[index], new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
            
            m_pCurrentBidding[0].SetText(string.Format("{0:#,0}", (uint)item["token"]));
            m_pCurrentBidding[1].SetText(ALFUtils.NumberToString((ulong)item["gold"]));
            
            m_pLastBiddingMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<BiddingMsgT>(item["data"].ToString());
            m_pLastBiddingMsg.Gold = (ulong)item["gold"];
            m_pLastBiddingMsg.Token = (uint)item["token"];
            m_pEmblemBake.SetupEmblemData(m_pLastBiddingMsg.Emblem.ToArray());
            m_pBiddingTitle.SetText(pGameContext.GetLocalizingText("PLAYERINFO_OFFER_TXT_TOP_BIDDER"));
            m_pCurrentName.SetText(m_pLastBiddingMsg.Name);
        }
        else
        {
            m_pLastBiddingPlayer.gameObject.SetActive(false);
            money = pGameContext.GetAuctionInitialGoldById(m_eTeamType, m_iCurrentAuctionId);
            m_pBiddingTitle.SetText("");
            m_pCurrentName.SetText("");
        }

        if(m_pLastBiddingMsg != null)
        {
            cash = m_pLastBiddingMsg.Token;
            money += (ulong)(m_pLastBiddingMsg.Gold * 0.01f);
        }
        
        m_pClubName.gameObject.SetActive(false);
        cash +=1;

        m_pNextBidding[0].SetText(string.Format("{0:#,0}", cash));
        m_pNextBidding[1].SetText(ALFUtils.NumberToString(money));

        SteupBiddingButton(cash,money);

        m_pCurrentTotalCash.SetText(string.Format("{0:#,0}", pGameContext.GetTotalCash()));
        m_pCurrentTotalMoney.SetText(ALFUtils.NumberToString(pGameContext.GetGameMoney()));
        
        m_fCurrentAuctionRemainingTime = (float)(NetworkManager.ConvertLocalGameTimeTick(payload["tExtend"].ToString()) - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond;
        m_bRefreshUpdate = m_fCurrentAuctionRemainingTime > 0;

        string[] strTimes = ALFUtils.SecondToString((int)m_fCurrentAuctionRemainingTime,true,true,true).Split(':');
        m_pRemainingTimeTexts[0].SetText(strTimes[strTimes.Length -2]);
        m_pRemainingTimeTexts[1].SetText(strTimes[strTimes.Length -1]);

        if(m_fCurrentAuctionRemainingTime < 10)
        {
            m_pRemainingTimeTexts[0].color = Color.red;
            m_pRemainingTimeTexts[1].color = Color.red;
        }
        else
        {
            m_pRemainingTimeTexts[0].color = Color.white;
            m_pRemainingTimeTexts[1].color = Color.white;
        }
    }

    float SoketTime = 0;
    float SoketProcessorTime = 0;

    public void SoketProcessor(NetworkData data)
    {
        E_SOCKET_ID eID = (E_SOCKET_ID)((uint)data.Json["msgId"]);
        float temp = Time.realtimeSinceStartup;
        float time = temp - SoketTime;
        Debug.Log($"{eID}:--------------SoketTime--------:{time}");
        SoketTime = temp;
        SoketProcessorTime = temp;

        switch(eID)
        {
            case E_SOCKET_ID.auctionJoin:
            {        
                if(data.Json.ContainsKey("payload") && data.Json["payload"].Type != JTokenType.Null)
                {
                    JObject pJObject = (JObject)data.Json["payload"]; 
                    if(m_iCurrentAuctionId == (uint)pJObject["auctionId"])
                    {
                        AuctionJoin(data.Json);
                    }
                }
            }
            break;
            case E_SOCKET_ID.auctionBidBroadcast:
            {
                GameContext pGameContext = GameContext.getCtx();
                AuctionBiddingInfoT pAuctionBiddingInfo = null;
                if(data.Json.ContainsKey("payload") && data.Json["payload"].Type != JTokenType.Null)
                {
                    JObject pJObject = (JObject)data.Json["payload"];
                    pAuctionBiddingInfo = pGameContext.GetAuctionBiddingInfoByID((uint)pJObject["auctionId"]);
                    if(pAuctionBiddingInfo != null)
                    {
                        pAuctionBiddingInfo.Update = pAuctionBiddingInfo.TExtend > 0;
                        if(pAuctionBiddingInfo.AuctionId != m_iCurrentAuctionId)
                        {
                            return;
                        }
                    }
                }
                BidBroadcast(data.Json);
            }
            break;   
        }

        temp = Time.realtimeSinceStartup;
        time = temp - SoketProcessorTime;
        Debug.Log($"{eID}:--------------SoketProcessorTime--------:{time}");
    }
}
