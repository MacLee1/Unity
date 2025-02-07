using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.NETWORK;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using BUSINESS;
using Newtonsoft.Json.Linq;
using TEXT;
// using UnityEngine.EventSystems;
using STATEDATA;

public class League :ITimer
{
    enum E_TEXT_TITLE : byte { CHALLENGESTAGE_MATCH_TITLE = 0,LEAGUE_TXT_LEAGUE_STANDING,LEAGUE_TXT_PLAYER_RANKING,LEAGUE_TXT_WINNER_HISTORY}
    
    const float HIDE_BUTTON_TIME = 5;

    enum E_SCROLL_TYPE : byte { challenge = 0,leagueRanking,leaguePlayer,leagueWin,MAX}
    enum E_SCROLL_ITEM : byte { ChallengeItem=0,LeagueRankingItem,RankingPlayerItem,LeagueWinItem}
    enum E_SUB_TAB : byte { goals = 1, assists =2,prossessionWon=10,saves = 13, MAX = 4}
    
    private class LeagueRankingItemData : IBase
    {
        public ulong ID {get; private set;} 

        public string Name {get; private set;}
        public uint Ranking {get; private set;}
        public byte[] Emblem {get; private set;}
        public int Standing {get; private set;}
        public int SquadPower {get; private set;}
        public int Game {get; private set;}
        public int Slot {get; private set;}
        public int Win {get; private set;}
        public int Lose {get; private set;}
        public int GF {get; private set;}
        public int GA {get; private set;}
        public int PTS {get; private set;}

        public LeagueRankingItemData( JObject data)
        {
            ID = (ulong)data["club"];
            Name = (string)data["name"];
            Standing = (int)data["standing"];
            Slot = (int)data["slot"];
            Game = (int)data["game"];
            Win = (int)data["win"];
            Lose = (int)data["lose"];
            GF = (int)data["gf"];
            GA = (int)data["ga"];
            PTS = (int)data["pts"];
            
            if(data.ContainsKey("emblem") && data["emblem"].Type != JTokenType.Null)
            {
                Emblem = SingleFunc.GetMakeEmblemData((string)data["emblem"]);
            }
            else
            {
                Emblem = SingleFunc.CreateRandomEmblem();
            }
        }

        public void Dispose()
        {
            Emblem = null;
        }
    }

    private class LeagueRankingItem : IBase
    {
        public ulong ID {get; private set;} 
        public RectTransform Target  {get; private set;}
        public TMPro.TMP_Text Name {get; private set;}
        // public Button Info {get; private set;}
        public TMPro.TMP_Text Ranking {get; private set;}
        public TMPro.TMP_Text GameText {get; private set;}
        public TMPro.TMP_Text WinText {get; private set;}
        public TMPro.TMP_Text LoseText {get; private set;}
        public TMPro.TMP_Text DrawText {get; private set;}
        public TMPro.TMP_Text GFText {get; private set;}
        public TMPro.TMP_Text GAText {get; private set;}
        public TMPro.TMP_Text PTSText {get; private set;}
        public EmblemBake Emblem {get; private set;}
        public GameObject Up {get; private set;}
        public GameObject Dn {get; private set;}
        public int Standing {get; private set;}
        Graphic m_pBG = null;
        GameObject[] m_pRankingMark = new GameObject[3];
        
        
        public LeagueRankingItem( RectTransform taget)
        {
            Target = taget;
            Name = Target.Find("name").GetComponent<TMPro.TMP_Text>();
            Emblem = Target.Find("emblem").GetComponent<EmblemBake>();
            GameText = Target.Find("p").GetComponent<TMPro.TMP_Text>();
            WinText = Target.Find("w").GetComponent<TMPro.TMP_Text>();
            LoseText = Target.Find("l").GetComponent<TMPro.TMP_Text>();
            DrawText = Target.Find("d").GetComponent<TMPro.TMP_Text>();
            GFText = Target.Find("gf").GetComponent<TMPro.TMP_Text>();
            GAText = Target.Find("ga").GetComponent<TMPro.TMP_Text>();
            PTSText = Target.Find("pts").GetComponent<TMPro.TMP_Text>();
            m_pBG = Target.Find("bg").GetComponent<Graphic>();
            Ranking = Target.Find("ranking/text").GetComponent<TMPro.TMP_Text>();
            for(int i =1; i < 4; ++i)
            {
                m_pRankingMark[i -1] = Ranking.transform.parent.Find(i.ToString()).gameObject;
            }

            Up = Target.Find("ranking/up").gameObject;
            Dn = Target.Find("ranking/dn").gameObject;
        }

        public void UpdateInfo(LeagueRankingItemData pLeagueRankingItemData,int iUpStanding,int iDnStanding)
        {
            if(pLeagueRankingItemData == null) return;
            
            ID = pLeagueRankingItemData.ID;
            Name.SetText(pLeagueRankingItemData.Name);
        
            Standing = pLeagueRankingItemData.Standing;

            for(int i = 0; i < m_pRankingMark.Length; ++i)
            {
                m_pRankingMark[i].SetActive(false);
            }
            
            Ranking.SetText(Standing.ToString());
            if(Standing < 4)
            {
                m_pRankingMark[Standing -1].SetActive(true);
            }
            Up.SetActive(Standing <= iUpStanding);
            Dn.SetActive(Standing >= iDnStanding);

            m_pBG.color = GameContext.getCtx().GetClubID() == ID ? new Color(0.6392157f,1,0.7047523f) : Color.white;

            // Slot = (int)data["slot"];
            GameText.SetText(pLeagueRankingItemData.Game.ToString());
            WinText.SetText(pLeagueRankingItemData.Win.ToString());
            LoseText.SetText(pLeagueRankingItemData.Lose.ToString());
            DrawText.SetText((pLeagueRankingItemData.Game - (pLeagueRankingItemData.Win + pLeagueRankingItemData.Lose)).ToString());
            GFText.SetText(pLeagueRankingItemData.GF.ToString());
            GAText.SetText(pLeagueRankingItemData.GA.ToString());
            PTSText.SetText(pLeagueRankingItemData.PTS.ToString());
            
            Emblem.SetupEmblemData(pLeagueRankingItemData.Emblem);
        }

        public void Dispose()
        {
            m_pBG = null;

            Up = null;
            Dn = null;
            Name = null;
            Ranking = null;
            // if(Info != null)
            // {
            //     Info.onClick.RemoveAllListeners();
            // }
            
            // Info = null;
            GameText = null;
            WinText = null;
            LoseText = null;
            DrawText = null;
            GFText = null;
            GAText = null;
            PTSText = null;
            Emblem.Dispose();
            Emblem = null;
            for(int i =0; i < m_pRankingMark.Length; ++i)
            {
                m_pRankingMark[i]= null;
            }
            m_pRankingMark = null;
            LayoutManager.Instance.AddItem(E_SCROLL_ITEM.LeagueRankingItem.ToString(),Target);
            Target = null;
        }
    }

    private class LeagueWinData : IBase
    {
        public uint SeasonNo {get; private set;}
        public int MatchType {get; private set;} 
        public ulong WinID {get; private set;} 
        public string WinName {get; private set;} 
        public string SecondName {get; private set;} 
        public ulong SecondID {get; private set;} 
        
        public byte[] WinEmblem {get; private set;}
        public byte[] SecondEmblem {get; private set;}

        public LeagueWinData(JObject winData,JObject secondData)
        {
            SeasonNo = (uint)winData["seasonNo"];
            MatchType = (int)winData["matchType"];
            int standing = (int)winData["standing"];
            WinID = (ulong)winData["club"];

            WinName = (string)winData["name"];
            
            if(winData.ContainsKey("emblem") && winData["emblem"].Type != JTokenType.Null)
            {
                WinEmblem = SingleFunc.GetMakeEmblemData((string)winData["emblem"]);
            }
            else
            {
                WinEmblem = SingleFunc.CreateRandomEmblem();
            }
            
            SecondID = 0;
            if(secondData != null)
            {
                SecondID = (ulong)secondData["club"];    
                SecondName = (string)secondData["name"];
                
                if(secondData.ContainsKey("emblem") && secondData["emblem"].Type != JTokenType.Null)
                {
                    SecondEmblem = SingleFunc.GetMakeEmblemData((string)secondData["emblem"]);
                }
                else
                {
                    SecondEmblem = SingleFunc.CreateRandomEmblem();
                }
            }
        }

        public void Dispose()
        {
            WinEmblem = null;
            SecondEmblem = null;
        }
    }

    private class LeagueWinItem : IBase
    {
        public int Index {get; private set;}
        public uint SeasonNo {get; private set;}
        public int MatchType {get; private set;} 
        public ulong WinID {get; private set;} 
        public ulong SecondID {get; private set;} 
        public RectTransform Target  {get; private set;}
        
        public Button Win {get; private set;}
        public Button Second {get; private set;}
        public TMPro.TMP_Text SeasonNoText {get; private set;}
        public TMPro.TMP_Text WinText {get; private set;}
        public TMPro.TMP_Text SecondText {get; private set;}
        
        public EmblemBake WinEmblem {get; private set;}
        public EmblemBake SecondEmblem {get; private set;}

        public LeagueWinItem(RectTransform taget)
        {
            Target = taget;
            SeasonNoText = Target.Find("season").GetComponent<TMPro.TMP_Text>();
            WinText = Target.Find("winnder/text").GetComponent<TMPro.TMP_Text>();
            WinEmblem = Target.Find("winnder/emblem").GetComponent<EmblemBake>();

            SecondText = Target.Find("runner/text").GetComponent<TMPro.TMP_Text>();
            SecondEmblem = Target.Find("runner/emblem").GetComponent<EmblemBake>();
            Win = Target.Find("winnder").GetComponent<Button>();
            Second = Target.Find("runner").GetComponent<Button>();
            
            float w = taget.rect.width;
            RectTransform tm = SeasonNoText.GetComponent<RectTransform>();
            
            w -= tm.rect.width;
            w *= 0.5f;
            Vector2 size = tm.sizeDelta;
            size.y = 0;
            size.x = tm.rect.width + 10;
            tm = Win.GetComponent<RectTransform>();
            tm.anchoredPosition = size;
            size = tm.sizeDelta;
            size.x = w;
            tm.sizeDelta = size;
            
            size = tm.anchoredPosition;
            size.x += w; 
            tm = Second.GetComponent<RectTransform>();
            tm.anchoredPosition = size;
            size = tm.sizeDelta;
            size.x = w;
            tm.sizeDelta = size;
        }

        public void Dispose()
        {
            Win.onClick.RemoveAllListeners();
            Win = null;
            Second.onClick.RemoveAllListeners();
            Second = null;
            SeasonNoText = null;
            WinText = null;
            SecondText = null;
        
            WinEmblem.Dispose();
            WinEmblem = null;
            SecondEmblem.Dispose();
            WinEmblem = null;
            LayoutManager.Instance.AddItem(E_SCROLL_ITEM.LeagueWinItem.ToString(),Target);
            Target = null;
        }

        public void UpdateInfo(int index,LeagueWinData pLeagueWinData)
        {
            Index = index;

            if(pLeagueWinData == null) return;

            SeasonNo = pLeagueWinData.SeasonNo;
            SeasonNoText.SetText(SeasonNo.ToString());

            MatchType = pLeagueWinData.MatchType;
            // int standing = (int)winData["standing"];
            WinID = pLeagueWinData.WinID;
            
            WinText.SetText(pLeagueWinData.WinName);
            WinEmblem.SetupEmblemData(pLeagueWinData.WinEmblem);
            
            if(SecondID != 0)
            {
                SecondText.SetText(pLeagueWinData.SecondName);
                SecondEmblem.SetupEmblemData(pLeagueWinData.SecondEmblem);
                Second.gameObject.SetActive(true);
            }
            else
            {
                Second.gameObject.SetActive(false);
            }
        }
    }
    
    bool m_bChangeBGM = true;
    ulong m_ulId = 0;
    E_SCROLL_TYPE m_eCurrentTab = E_SCROLL_TYPE.MAX;
    E_SUB_TAB m_eCurrentSubTab = E_SUB_TAB.goals;
    TMPro.TMP_Text m_pTitle = null;
    TMPro.TMP_Text m_pSeasonTitle = null;
    GameObject[] m_pSeasonTitleBg = new GameObject[5];
    GameObject[] m_pIconList = new GameObject[5];
    TMPro.TMP_Text m_pLeagueSubTitle = null;
    TMPro.TMP_Text m_pLeagueSeasonTitle = null;
    RectTransform m_pAgo = null;
    RectTransform m_pCurrent = null;
    TMPro.TMP_Text m_pExpireText = null;
    MainScene m_pMainScene = null;
    GameObject m_pLeagueRewardNode = null;
    GameObject m_pLeagueScheduleNode = null;
    GameObject m_pMyStandingNode = null;
    GameObject m_pLeagueMatchNode = null;
    GameObject m_pChallengeMatchNode = null;
    
    Image[] m_pMatchGauges = new Image[2];
    Button[] m_pRecoveryButtons = new Button[2];
    Animation[] m_pMatchEffects = new Animation[2];
    
    ScrollRect[] m_pScrollRects = new ScrollRect[(int)E_SCROLL_TYPE.MAX];
    RectTransform[] m_pTabUIList = new RectTransform[(int)E_SCROLL_TYPE.MAX];
    RectTransform[] m_pSubTabUIList = new RectTransform[14];
    RectTransform[] m_pSubHeaderUIList = new RectTransform[14];
    Button[] m_pLeagueList = new Button[5];
    GameObject m_pLeagueListNode = null;
    int m_iMyStanding = 0;

    RectTransform m_pMyStandingRect = null;

    int m_iCurrentLeague = GameContext.CHALLENGE_ID;
    uint m_iCurrentSeasonNo = 0;

    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ 
        for(int i =0; i < m_pScrollRects.Length; ++i)
        {
            m_pScrollRects[i].enabled = value;
        }
    }}

    List<ChallengeItemData> m_pChallengeDataList = new List<ChallengeItemData>();
    List<ChallengeItem> m_pChallengeItems = new List<ChallengeItem>();

    List<LeagueRankingItemData> m_pLeagueRankingDataList = new List<LeagueRankingItemData>();
    List<LeagueRankingItem> m_pLeagueRankingItems = new List<LeagueRankingItem>();

    JArray m_pLeagueRankingPlayerData = null;
    List<RankingPlayerItem> m_pLeaguePlayerRankingItems = new List<RankingPlayerItem>();

    List<LeagueWinData>  m_pLeagueWinDataList = new List<LeagueWinData>();
    List<LeagueWinItem>  m_pLeagueWinItems = new List<LeagueWinItem>();

    ChallengeItem m_pChallengeMe = null;

    
    Vector2[] m_pPrevDir = new Vector2[]{Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero};
    int[] m_iTotalScrollItems = new int[]{0,0,0,0};
    int[] m_iStartIndex = new int[]{0,0,0,0};
    int[] m_iTotalDatas = new int[]{0,0,0,0};
    
    public League(){}

    public void Dispose()
    {
        m_pLeagueRankingPlayerData = null;

        m_pChallengeMatchNode = null;
        m_pAgo = null;
        m_pCurrent = null;
        m_pLeagueSeasonTitle = null;
        m_pLeagueMatchNode = null;
        m_pLeagueRewardNode = null;
        m_pLeagueScheduleNode = null;
        m_pMyStandingNode = null;
        m_pMyStandingRect = null;
        m_pTitle = null;
        m_pSeasonTitle = null;
        m_pLeagueSubTitle = null;
        m_pExpireText = null;
        m_pMainScene = null;
        MainUI = null;
        int i =0;
        for(i =0; i < m_pMatchGauges.Length; ++i)
        {
            m_pMatchGauges[i] = null;
            m_pRecoveryButtons[i] = null;
            m_pMatchEffects[i] = null;
        }
        m_pMatchGauges = null;
        m_pRecoveryButtons = null;
        m_pMatchEffects = null;

        for(i =0; i < m_pSeasonTitleBg.Length; ++i)
        {
            m_pSeasonTitleBg[i] = null;
            m_pIconList[i] = null;
        }
        m_pSeasonTitleBg = null;
        m_pIconList  = null;

        for(i =0; i < m_pScrollRects.Length; ++i)
        {
            ClearScroll((E_SCROLL_TYPE)i);
            m_pScrollRects[i].onValueChanged.RemoveAllListeners();
            m_pScrollRects[i] = null;
        }
        for(i =0; i < m_pSubTabUIList.Length; ++i)
        {
            m_pSubTabUIList[i] = null;
            m_pSubHeaderUIList[i] = null;
        }
        m_pSubHeaderUIList = null;
        m_pSubTabUIList = null;

        for(i =0; i < m_pLeagueList.Length; ++i)
        {
            m_pLeagueList[i] = null;
        }

        m_pLeagueList = null;
        m_pLeagueListNode = null;
        m_pScrollRects = null;
        
        for(i =0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i] = null;
        }

        m_pTabUIList = null;
        
        m_pChallengeMe?.Dispose();
        m_pChallengeMe = null;
        m_pLeagueWinDataList = null;
        m_pChallengeDataList = null;
        m_pLeaguePlayerRankingItems = null;
        m_pLeagueRankingItems = null;
        m_pChallengeItems = null;
        m_pLeagueWinItems = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "League : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "League : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
       
        m_pTitle = MainUI.Find("root/title/title").GetComponent<TMPro.TMP_Text>();
        m_pSeasonTitle = m_pTitle.transform.parent.Find("season/text").GetComponent<TMPro.TMP_Text>();

        for(int i =0; i < m_pSeasonTitleBg.Length; ++i )
        {
            m_pSeasonTitleBg[i] = m_pSeasonTitle.transform.parent.Find((11+i).ToString()).gameObject;
            m_pIconList[i] = m_pTitle.transform.parent.Find($"icon/{11+i}").gameObject;
        }

        m_pLeagueSubTitle = MainUI.Find("root/league/title/text").GetComponent<TMPro.TMP_Text>();
        m_pLeagueRewardNode = m_pLeagueSubTitle.transform.parent.parent.Find("reward").gameObject;
        m_pLeagueScheduleNode = m_pLeagueRewardNode.transform.parent.Find("schedule").gameObject;
        m_pLeagueSeasonTitle = MainUI.Find("root/league/title/season/text").GetComponent<TMPro.TMP_Text>();
        m_pExpireText = MainUI.Find("root/title/time/text").GetComponent<TMPro.TMP_Text>();
        m_pAgo = m_pLeagueSeasonTitle.transform.parent.Find("ago").GetComponent<RectTransform>();
        m_pCurrent = m_pLeagueSeasonTitle.transform.parent.Find("current").GetComponent<RectTransform>();
        float w = 0;
        float wh = 0;
        
        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        float ax = 0;
        int iTabIndex = -1;
        int n =0;
        GameContext pGameContext = GameContext.getCtx();
        m_iCurrentSeasonNo = pGameContext.GetCurrentSeasonNo();
        m_pLeagueMatchNode = MainUI.Find("root/league/leagueMatch").gameObject;
        
        m_pLeagueListNode = MainUI.Find("root/list").gameObject;
        RectTransform ui = m_pLeagueListNode.transform.Find("bg").GetComponent<RectTransform>();
        string current = pGameContext.GetCurrentMatchType().ToString();
        for(int i =0;i < ui.childCount; ++i)
        {
            m_pLeagueList[i] = ui.GetChild(i).GetComponent<Button>();
            m_pLeagueList[i].transform.Find("text").gameObject.SetActive(m_pLeagueList[i].gameObject.name == current);
        }
        
        ui.transform.parent.gameObject.SetActive(false);
        
        ui = MainUI.Find("root/league").GetComponent<RectTransform>();
        m_pTabUIList[0] = MainUI.Find("root/challenge").GetComponent<RectTransform>();
        m_pTabUIList[0].gameObject.SetActive(false);

        iTabIndex = 1;
        for(E_SCROLL_TYPE e = E_SCROLL_TYPE.leagueRanking; e < E_SCROLL_TYPE.MAX; ++e)
        {
            item = ui.Find(e.ToString()).GetComponent<RectTransform>();
            m_pTabUIList[iTabIndex] = item;
            m_pTabUIList[iTabIndex].gameObject.SetActive(false);
            ++iTabIndex;
        }
        m_pMyStandingNode = m_pTabUIList[(int)E_SCROLL_TYPE.challenge].Find("me").gameObject;
        m_pChallengeMatchNode = m_pMyStandingNode.transform.Find("challengeMatch").gameObject;
        
        m_pMyStandingRect = m_pTabUIList[(int)E_SCROLL_TYPE.challenge].Find("list").GetComponent<RectTransform>();
        ui = m_pTabUIList[(int)E_SCROLL_TYPE.leaguePlayer].Find("tabs").GetComponent<RectTransform>();
        w = (ui.rect.width / ui.childCount);
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_SUB_TAB)Enum.Parse(typeof(E_SUB_TAB), item.gameObject.name));
            m_pSubHeaderUIList[iTabIndex] = m_pTabUIList[(int)E_SCROLL_TYPE.leaguePlayer].Find($"header/{item.gameObject.name}").GetComponent<RectTransform>();
            m_pSubTabUIList[iTabIndex] = item;

            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }

        ui = m_pTabUIList[(int)E_SCROLL_TYPE.leagueWin].Find("tabs").GetComponent<RectTransform>();
        w = (ui.rect.width / ui.childCount);
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }

        for(n =0; n < m_pTabUIList.Length; ++n)
        {
            m_pScrollRects[n] = m_pTabUIList[n].GetComponentInChildren<ScrollRect>(true);
            m_pScrollRects[n].onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        }

        MainUI.gameObject.SetActive(false);

        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        TimeSpan pTimeSpan = pGameContext.GetCurrentSeasonExpireTime() - NetworkManager.GetGameServerTime();
        float totalTime = (float)pTimeSpan.Ticks / (float)TimeSpan.TicksPerSecond;
        pGameContext.AddExpireTimer(this,0,totalTime);
        SingleFunc.UpdateTimeText((int)totalTime,m_pExpireText,0);

        m_pMatchEffects[0] = m_pChallengeMatchNode.transform.Find("challengeMatch/effect").GetComponent<Animation>();
        m_pMatchEffects[1] = m_pLeagueMatchNode.transform.Find("leagueMatch/effect").GetComponent<Animation>();
        
        m_pMatchGauges[0] = m_pChallengeMatchNode.transform.Find("gauge/fill").GetComponent<Image>();
        m_pMatchGauges[1] = m_pLeagueMatchNode.transform.Find("gauge/fill").GetComponent<Image>();

        m_pRecoveryButtons[0] = m_pChallengeMatchNode.transform.Find("recovery").GetComponent<Button>();
        m_pRecoveryButtons[1] = m_pLeagueMatchNode.transform.Find("recovery").GetComponent<Button>();

        SetupScroll(E_SCROLL_TYPE.challenge);
        SetupScroll(E_SCROLL_TYPE.leagueRanking);
        SetupScroll(E_SCROLL_TYPE.leaguePlayer);
        SetupScroll(E_SCROLL_TYPE.leagueWin);
    }

    public void UpdateMatchGauge()
    {
        GameContext pGameContext = GameContext.getCtx();

        float fillAmount = pGameContext.GetLineupTotalHP() / 1800.0f;

        Color color;
        if(fillAmount < 0.5f)
        {
            color = GameContext.HP_L;
        }
        else if(fillAmount < 0.7f)
        {
            color = GameContext.HP_LH;
        }
        else if(fillAmount < 0.9f)
        {
            color = GameContext.HP_H;
        }
        else
        {
            color = GameContext.HP_F;
        }
        for(int i =0; i < m_pMatchGauges.Length; ++i)
        {
            m_pMatchGauges[i].fillAmount = fillAmount;
            m_pMatchGauges[i].color = color;
        }
        
        if(fillAmount < 1f)
        {
            m_pChallengeMatchNode.transform.Find("icon").gameObject.SetActive(fillAmount < 0.8f);
            m_pLeagueMatchNode.transform.Find("icon").gameObject.SetActive(fillAmount < 0.8f);

            ulong hasCount = pGameContext.GetItemCountByNO(GameContext.STAMINA_DRINK);
            uint count = pGameContext.GetUseRecoverHPItemCount(pGameContext.GetRecoverHpPlayerIDs());

            for(int i =0; i < 2; ++i)
            {
                m_pRecoveryButtons[i].gameObject.SetActive(true);
                m_pMatchEffects[i].gameObject.SetActive(false);
                m_pMatchEffects[i].Stop();
                
                m_pRecoveryButtons[i].enabled = count <= hasCount;
                m_pRecoveryButtons[i].transform.Find("off").gameObject.SetActive(!m_pRecoveryButtons[i].enabled);
                m_pRecoveryButtons[i].transform.Find("on").gameObject.SetActive(m_pRecoveryButtons[i].enabled);
                m_pRecoveryButtons[i].transform.Find("count").GetComponent<TMPro.TMP_Text>().SetText($"<color={(m_pRecoveryButtons[i].enabled ? Color.white.ToHexString() : Color.red.ToHexString())}>{ALFUtils.NumberToString(count)}</color>/{ALFUtils.NumberToString(hasCount)}");
            }
        }
        else
        {
            for(int i =0; i < 2; ++i)
            {
                m_pRecoveryButtons[i].gameObject.SetActive(false);
                m_pMatchEffects[i].gameObject.SetActive(true);
                m_pMatchEffects[i].Play();
            }
            m_pChallengeMatchNode.transform.Find("icon").gameObject.SetActive(false);
            m_pLeagueMatchNode.transform.Find("icon").gameObject.SetActive(false);
        }
    }

    public int GetMyClubStandingByID()
    {
        return m_iMyStanding;
    }

    public void DoExpire(int index)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            GameContext pGameContext = GameContext.getCtx();
            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText(pGameContext.GetCurrentMatchType() == GameContext.CHALLENGE_ID ? "ALERT_TXT_LEAGUE_ENDED_CHALLENGE_USER": "ALERT_TXT_LEAGUE_ENDED_LEAGUE_USER"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
            
            Close();
        }
    }

    public void UpdateTimer(float dt)
    {
        GameContext pGameContext = GameContext.getCtx();
        float tic = pGameContext.GetExpireTimerByUI(this,0);
        if(tic <= 86400)
        {
            SingleFunc.UpdateTimeText((int)tic,m_pExpireText,0);
        }
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
            }
        }
    }

    void SetupScroll(E_SCROLL_TYPE eType)
    {
        RectTransform pItem = null;
        Vector2 size;
        ScrollRect pScrollRect = m_pScrollRects[(int)eType];
        E_SCROLL_ITEM eItemName = (E_SCROLL_ITEM)eType;
        
        m_iTotalScrollItems[(int)eType] = 0;
        m_iStartIndex[(int)eType] = 0;

        float viewSize = pScrollRect.viewport.rect.height;
        float itemSize = 0;
        float h = 0;
        
        if(eType == E_SCROLL_TYPE.challenge)
        {
            while(viewSize > -itemSize)
            {
                if(viewSize > 0)
                {
                    ++m_iTotalScrollItems[(int)eType];
                }
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(eItemName.ToString());
                    
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
                    pItem.gameObject.SetActive(false);
                    m_pChallengeItems.Add(new ChallengeItem(pItem,eItemName.ToString()));
                }
            }
        }
        else if(eType == E_SCROLL_TYPE.leagueRanking)
        {
            while(viewSize > -itemSize)
            {
                if(viewSize > 0)
                {
                    ++m_iTotalScrollItems[(int)eType];
                }
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(eItemName.ToString());
                    
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
                    pItem.gameObject.SetActive(false);
                    m_pLeagueRankingItems.Add(new LeagueRankingItem(pItem));
                }
            }
        }
        else if(eType == E_SCROLL_TYPE.leaguePlayer)
        {
            while(viewSize > -itemSize)
            {
                if(viewSize > 0)
                {
                    ++m_iTotalScrollItems[(int)eType];
                }
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(eItemName.ToString());
                    
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
                    pItem.gameObject.SetActive(false);
                    m_pLeaguePlayerRankingItems.Add(new RankingPlayerItem(pItem,eItemName.ToString()));
                }
            }
        }
        else if(eType == E_SCROLL_TYPE.leagueWin)
        {
            while(viewSize > -itemSize)
            {
                if(viewSize > 0)
                {
                    ++m_iTotalScrollItems[(int)eType];
                }
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(eItemName.ToString());
                    
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
                    pItem.gameObject.SetActive(false);
                    m_pLeagueWinItems.Add(new LeagueWinItem(pItem));
                }
            }
        }
        
        size = pScrollRect.content.sizeDelta;
        size.y = h;
        m_pPrevDir[(int)eType].x = 0;
        
        pScrollRect.content.sizeDelta = size;
        pScrollRect.content.anchoredPosition = Vector2.zero;
        LayoutManager.SetReciveUIScollViewEvent(pScrollRect,ScrollViewItemButtonEventCall);
    }

    void SetupLeagueUI()
    {
        ShowTabUI(E_SCROLL_TYPE.leagueRanking);
        GameContext pGameContext = GameContext.getCtx();

        m_pTabUIList[(int)E_SCROLL_TYPE.leaguePlayer].parent.gameObject.SetActive(true);
        
        m_pLeagueScheduleNode.SetActive(pGameContext.GetCurrentMatchType() == m_iCurrentLeague);
        m_pLeagueMatchNode.SetActive(m_pLeagueScheduleNode.activeSelf);
    }

    public void SetupLeagueTop100(JObject data)
    {
        if( MainUI == null || !MainUI.gameObject.activeSelf) return;

        m_iCurrentLeague = (int)data["matchType"];
        m_iCurrentSeasonNo = (uint)data["seasonNo"];

        GameContext pGameContext = GameContext.getCtx();

        if(m_iCurrentSeasonNo == pGameContext.GetCurrentSeasonNo())
        {
            m_pAgo.gameObject.SetActive(true);
            m_pCurrent.gameObject.SetActive(false);
        }
        else
        {
            m_pAgo.gameObject.SetActive(false);
            m_pCurrent.gameObject.SetActive(true);
        }

        uint count = m_iCurrentSeasonNo % 100;
        m_pLeagueSeasonTitle.SetText(string.Format(pGameContext.GetLocalizingText("SEASONPASS_TITLE"),(m_iCurrentSeasonNo - count)/100,count));
        
        m_pLeagueRankingPlayerData = null;
        ScrollRect pScrollRect = m_pScrollRects[(int)m_eCurrentTab];

        if(data.ContainsKey("players") && data["players"].Type != JTokenType.Null)
        {    
            pScrollRect.gameObject.SetActive(true);
            
            m_pLeagueRankingPlayerData = (JArray)data["players"];

            m_iTotalDatas[(int)m_eCurrentTab] = m_pLeagueRankingPlayerData.Count;
            m_iStartIndex[(int)m_eCurrentTab] = 0;
            float viewSize = pScrollRect.viewport.rect.height;
            float itemSize = 0;

            RankingPlayerItem pPlayerItem = null;
            
            for(int i =0; i < m_pLeaguePlayerRankingItems.Count; ++i)
            {
                pPlayerItem = m_pLeaguePlayerRankingItems[i];
                itemSize = pPlayerItem.Target.rect.height;

                if(m_pLeagueRankingPlayerData.Count <= i)
                {
                    pPlayerItem.Target.gameObject.SetActive(false);
                }
                else
                {
                    pPlayerItem.UpdateData((JObject)m_pLeagueRankingPlayerData[i],i,(int)m_eCurrentSubTab);
                
                    if(viewSize > -itemSize)
                    {    
                        viewSize -= itemSize;
                        pPlayerItem.Target.gameObject.SetActive(viewSize > -itemSize);
                    }
                }
            }

            Vector2 size = pScrollRect.content.sizeDelta;
            size.y = m_pLeagueRankingPlayerData.Count * itemSize;
            pScrollRect.content.sizeDelta = size;
            pScrollRect.verticalNormalizedPosition = 1;
            m_pPrevDir[(int)m_eCurrentTab].y = 1;
            pScrollRect.content.anchoredPosition = Vector2.zero;

            pScrollRect.enabled = true;
        }
        else
        {
            pScrollRect.gameObject.SetActive(false);            
        }
    }

    public void SetupLeagueWinList(JObject data)
    {
        if( MainUI == null || !MainUI.gameObject.activeSelf) return;
        
        GameContext pGameContext = GameContext.getCtx();
        m_iCurrentLeague = (int)data["matchType"];
        
        if(m_pLeagueScheduleNode.activeSelf)
        {
            m_pMainScene.GetInstance<LeagueSchedule>().SetupData(data);
        }
        
        ScrollRect pScrollRect = m_pScrollRects[(int)m_eCurrentTab];
        if(data.ContainsKey("leaders") && data["leaders"].Type != JTokenType.Null)
        {
            int i = m_pLeagueWinDataList.Count;
            while(i > 0)
            {
                --i;
                m_pLeagueWinDataList[i].Dispose();
            }
            m_pLeagueWinDataList.Clear();

            pScrollRect.gameObject.SetActive(true);
            
            Dictionary<uint, List<JObject>> list = new Dictionary<uint, List<JObject>>();
            List<JObject> jList = null;
            JArray pJArray = (JArray)data["leaders"];
            JObject item = null;
            JObject item2 = null;
            
            uint seasonNo = 0;
            
            for(i = 0; i < pJArray.Count; ++i)
            {
                item = (JObject)pJArray[i];
                seasonNo = (uint)item["seasonNo"];
                if(list.ContainsKey(seasonNo))
                {
                    jList = list[seasonNo];
                }
                else
                {
                    jList = new List<JObject>();
                    list.Add(seasonNo,jList);
                }
                JObject temp = null;
                for(int n =0; n < jList.Count; ++n)
                {
                    temp = (JObject)jList[n];
                    if((int)temp["standing"] > (int)item["standing"])
                    {
                        jList[n] = item;
                        item = temp;
                    }
                }
                jList.Add(item);   
            }

            var itr = list.GetEnumerator();
            while(itr.MoveNext())
            {
                item = itr.Current.Value[0];
                if(itr.Current.Value.Count >1)
                {
                    item2 = itr.Current.Value[1];
                }
                else
                {
                    item2 = null;
                }

                m_pLeagueWinDataList.Add(new LeagueWinData( item,item2));
            }
            m_iTotalDatas[(int)m_eCurrentTab] = list.Count;
            m_iStartIndex[(int)m_eCurrentTab] = 0;
            float viewSize = pScrollRect.viewport.rect.height;
            float itemSize = 0;

            LeagueWinItem pLeagueWinItem = null;
            for(i =0; i < m_pLeagueWinItems.Count; ++i)
            {
                pLeagueWinItem = m_pLeagueWinItems[i];
                itemSize = pLeagueWinItem.Target.rect.height;

                if(m_pLeagueWinDataList.Count <= i)
                {
                    pLeagueWinItem.Target.gameObject.SetActive(false);
                }
                else
                {
                    if(viewSize > -itemSize)
                    {    
                        viewSize -= itemSize;
                        pLeagueWinItem.Target.gameObject.SetActive(viewSize > -itemSize);
                    }
                }

                if(m_pLeagueWinDataList.Count > i)
                {
                    pLeagueWinItem.UpdateInfo(i,m_pLeagueWinDataList[i]);
                }
            }

            Vector2 size = pScrollRect.content.sizeDelta;
            size.y = m_pLeagueWinDataList.Count * itemSize;
            pScrollRect.content.sizeDelta = size;
            pScrollRect.verticalNormalizedPosition = 1;
            m_pPrevDir[(int)m_eCurrentTab].y = 1;
            pScrollRect.content.anchoredPosition = Vector2.zero;

            pScrollRect.enabled = true;
        }
        else
        {
            pScrollRect.gameObject.SetActive(false);
        }
    }
    public void SetupLeagueStandings(JObject data)
    {
        if( MainUI == null || !MainUI.gameObject.activeSelf) return;

        UpdateMatchGauge();
        m_iCurrentLeague = (int)data["matchType"];
        m_iCurrentSeasonNo = (uint)data["seasonNo"];

        SetupLeagueUI();
        int i =0;
        for(i =0; i < m_pSeasonTitleBg.Length; ++i)
        {
            m_pSeasonTitleBg[i]?.SetActive(false);
            m_pIconList[i]?.SetActive(false);
        }
        m_pSeasonTitleBg[m_iCurrentLeague - GameContext.CHALLENGE_ID].SetActive(true);
        m_pIconList[m_iCurrentLeague - GameContext.CHALLENGE_ID].SetActive(true);
   
        GameContext pGameContext = GameContext.getCtx();

        if(m_iCurrentSeasonNo == pGameContext.GetCurrentSeasonNo())
        {
            m_pAgo.gameObject.SetActive(true);
            m_pCurrent.gameObject.SetActive(false);
        }
        else
        {
            m_pAgo.gameObject.SetActive(false);
            m_pCurrent.gameObject.SetActive(true);
        }

        uint count = m_iCurrentSeasonNo % 100;
        m_pLeagueSeasonTitle.SetText(string.Format(pGameContext.GetLocalizingText("SEASONPASS_TITLE"),(m_iCurrentSeasonNo - count)/100,count));
        ScrollRect pScrollRect = m_pScrollRects[(int)m_eCurrentTab];
        
        if(data.ContainsKey("standings") && data["standings"].Type != JTokenType.Null)
        {
            i = m_pLeagueRankingDataList.Count;
            while(i > 0)
            {
                --i;
                m_pLeagueRankingDataList[i].Dispose();
            }
            m_pLeagueRankingDataList.Clear();

            pScrollRect.gameObject.SetActive(true);
            JArray pJArray = (JArray)data["standings"];
            JObject item = null;
            ulong myID = pGameContext.GetClubID();
            m_iTotalDatas[(int)m_eCurrentTab] = pJArray.Count;
            m_iStartIndex[(int)m_eCurrentTab] = 0;

            LeagueRankingItemData pLeagueRankingItemData = null;
            for( i =0; i < pJArray.Count; ++i)
            {
                item = (JObject)pJArray[i];
                pLeagueRankingItemData = new LeagueRankingItemData(item);
                m_pLeagueRankingDataList.Add(pLeagueRankingItemData);
                
                if(pLeagueRankingItemData.ID == myID)
                {
                    m_iMyStanding = pLeagueRankingItemData.Standing;
                }
            }
            
            float viewSize = pScrollRect.viewport.rect.height;
            float itemSize = 0;

            LeagueRankingItem pLeagueRankingItem = null;
            int iUpStanding = pGameContext.GetLeagueUpStanding(m_iCurrentLeague);
            int iDnStanding = pGameContext.GetLeagueDownStanding(m_iCurrentLeague);
            
            for(i =0; i < m_pLeagueRankingItems.Count; ++i)
            {
                pLeagueRankingItem = m_pLeagueRankingItems[i];
                itemSize = pLeagueRankingItem.Target.rect.height;

                if(m_pLeagueRankingDataList.Count <= i)
                {
                    pLeagueRankingItem.Target.gameObject.SetActive(false);
                }
                else
                {
                    if(viewSize > -itemSize)
                    {    
                        viewSize -= itemSize;
                        pLeagueRankingItem.Target.gameObject.SetActive(viewSize > -itemSize);
                    }
                }

                if(m_pLeagueRankingDataList.Count > i)
                {
                    pLeagueRankingItem.UpdateInfo(m_pLeagueRankingDataList[i],iUpStanding,iDnStanding);
                }
            }

            Vector2 size = pScrollRect.content.sizeDelta;
            size.y = m_pLeagueRankingDataList.Count * itemSize;
            pScrollRect.content.sizeDelta = size;
            pScrollRect.verticalNormalizedPosition = 1;
            m_pPrevDir[(int)m_eCurrentTab].y = 1;
            pScrollRect.content.anchoredPosition = Vector2.zero;

            pScrollRect.enabled = true;
        }
        else
        {
            pScrollRect.gameObject.SetActive(false);
        }
        
        if(m_pLeagueScheduleNode.activeSelf)
        {
            m_pMainScene.GetInstance<LeagueSchedule>().SetupData(data);
        }
    }

    public void SetupChallengeStandings(JObject data)
    {        
        if( MainUI == null || !MainUI.gameObject.activeSelf) return;

        UpdateMatchGauge();
        
        ShowTabUI(E_SCROLL_TYPE.challenge);
        int i =0;
        for(i =0; i < m_pSeasonTitleBg.Length; ++i)
        {
            m_pSeasonTitleBg[i]?.SetActive(false);
            m_pIconList[i]?.SetActive(false);
        }
        m_pSeasonTitleBg[0].SetActive(true);
        m_pIconList[0].SetActive(true);
        
        m_iCurrentLeague = GameContext.CHALLENGE_ID;
        m_pTabUIList[(int)E_SCROLL_TYPE.leaguePlayer].parent.gameObject.SetActive(false);

        GameContext pGameContext = GameContext.getCtx();
        string itemName = E_SCROLL_ITEM.ChallengeItem.ToString();
        RectTransform pItem = null; 
        Vector3 size;
        ScrollRect pScrollRect = m_pScrollRects[(int)m_eCurrentTab];        
        if(data.ContainsKey("standings") && data["standings"].Type != JTokenType.Null)
        {
            i = m_pChallengeDataList.Count;
            while(i > 0)
            {
                --i;
                m_pChallengeDataList[i].Dispose();
            }
            m_pChallengeDataList.Clear();

            pScrollRect.gameObject.SetActive(true);
            JArray pJArray = (JArray)data["standings"];
            m_iTotalDatas[(int)m_eCurrentTab] = pJArray.Count;
            m_iStartIndex[(int)m_eCurrentTab] = 0;
            
            for( i =0; i < pJArray.Count; ++i)
            {
                m_pChallengeDataList.Add(new ChallengeItemData((JObject)pJArray[i]));       
            }

            float viewSize = pScrollRect.viewport.rect.height;
            float itemSize = 0;
            ChallengeItem pChallengeItem = null;
            for(i =0; i < m_pChallengeItems.Count; ++i)
            {
                pChallengeItem = m_pChallengeItems[i];
                itemSize = pChallengeItem.Target.rect.height;

                if(m_pChallengeDataList.Count <= i)
                {
                    pChallengeItem.Target.gameObject.SetActive(false);
                }
                else
                {
                    if(viewSize > -itemSize)
                    {    
                        viewSize -= itemSize;
                        pChallengeItem.Target.gameObject.SetActive(viewSize > -itemSize);
                    }
                }

                if(m_pChallengeDataList.Count > i)
                {
                    pChallengeItem.UpdateInfo(m_pChallengeDataList[i]);
                }
            }

            size = pScrollRect.content.sizeDelta;
            size.y = m_pChallengeDataList.Count * itemSize;
            pScrollRect.content.sizeDelta = size;
            pScrollRect.verticalNormalizedPosition = 1;
            m_pPrevDir[(int)m_eCurrentTab].y = 1;
            pScrollRect.content.anchoredPosition = Vector2.zero;

            pScrollRect.enabled = true;
        }
        else
        {
            pScrollRect.gameObject.SetActive(false);
        }

        if( data.ContainsKey("myStanding") && data["myStanding"].Type != JTokenType.Null && (int)data["myStanding"] > 0)
        {
            m_pMyStandingNode.SetActive(true);
            pItem = LayoutManager.Instance.GetItem<RectTransform>(itemName);
            JObject item = new JObject();
            item["standing"] = (int)data["myStanding"];
            item["club"] = pGameContext.GetClubID();
            item["name"] = pGameContext.GetClubName();
            item["emblem"] = Newtonsoft.Json.JsonConvert.SerializeObject(pGameContext.GetEmblemInfo().ToList(), Newtonsoft.Json.Formatting.None,new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore});
            item["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
            item["userRank"] = pGameContext.GetCurrentUserRank();

            pItem.localScale = Vector3.one;       
            size = pItem.sizeDelta;
            size.x =0;
            pItem.sizeDelta = size;
            
            pItem.SetParent(m_pMyStandingNode.transform,false); 
            pItem.SetAsFirstSibling();
            size = pItem.offsetMin;
            size.x = 20;
            pItem.offsetMin = size;
            size = pItem.offsetMax;
            size.x = -20;
            pItem.offsetMax = size;
            size = pItem.anchoredPosition;
            size.y = -71;
            pItem.anchoredPosition = size;
            m_pChallengeMe = new ChallengeItem(pItem,itemName);
            m_pChallengeMe.UpdateInfo(new ChallengeItemData(item));
            m_iMyStanding = m_pChallengeMe.Standing;
        }
        else
        {
            m_pMyStandingNode.SetActive(false);
        }

        if(m_pMyStandingNode.activeSelf)
        {
            size = m_pMyStandingRect.offsetMin;
            size.y = 440;
            m_pMyStandingRect.offsetMin = size;
        }
        else
        {
            size = m_pMyStandingRect.offsetMin;
            size.y = 300;
            m_pMyStandingRect.offsetMin = size;
        }
    }

    void ClearScroll(E_SCROLL_TYPE eType)
    {
        if(eType == E_SCROLL_TYPE.MAX ) return;
        
        int i =0;
        if(E_SCROLL_TYPE.challenge == eType)
        {
            for(i =0; i < m_pChallengeDataList.Count; ++i)
            {
                m_pChallengeDataList[i].Dispose();
            }

            for(i =0; i < m_pChallengeItems.Count; ++i)
            {
                m_pChallengeItems[i].Dispose();
            }
            m_pChallengeDataList.Clear();
            m_pChallengeItems.Clear();

            if(m_pChallengeMe != null)
            {
                m_pChallengeMe.Dispose();
                m_pChallengeMe = null;
            }
        }
        else if(E_SCROLL_TYPE.leagueRanking == eType)
        {
            for(i =0; i < m_pLeagueRankingDataList.Count; ++i)
            {
                m_pLeagueRankingDataList[i].Dispose();
            }
            m_pLeagueRankingDataList.Clear();

            for(i =0; i < m_pLeagueRankingItems.Count; ++i)
            {
                m_pLeagueRankingItems[i].Dispose();
            }
            m_pLeagueRankingItems.Clear();
        }
        else if(E_SCROLL_TYPE.leagueWin == eType)
        {
            for(i =0; i < m_pLeagueWinDataList.Count; ++i)
            {
                m_pLeagueWinDataList[i].Dispose();
            }
            for(i =0; i < m_pLeagueWinItems.Count; ++i)
            {
                m_pLeagueWinItems[i].Dispose();
            }
            m_pLeagueWinItems.Clear();
            m_pLeagueWinDataList.Clear();
        }
        else if(E_SCROLL_TYPE.leaguePlayer == eType)
        {
            for(i =0; i < m_pLeaguePlayerRankingItems.Count; ++i)
            {
                m_pLeaguePlayerRankingItems[i].Dispose();
            }
            m_pLeaguePlayerRankingItems.Clear();
        }

        ScrollRect pScrollRect = m_pScrollRects[(int)eType];
        LayoutManager.SetReciveUIScollViewEvent(pScrollRect,null);
        
        pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = pScrollRect.content.sizeDelta;
        size.y =0;
        pScrollRect.content.sizeDelta = size;
    }

    void ResetScroll(E_SCROLL_TYPE eScrollType)
    {
        if(eScrollType == E_SCROLL_TYPE.MAX ) return;

        ScrollRect pScrollRect = m_pScrollRects[(int)eScrollType];
        Vector2 pos;
        float viewSize = pScrollRect.viewport.rect.height;
        float itemSize = 0;

        if(E_SCROLL_TYPE.challenge == eScrollType)
        {
            ChallengeItem pItem = null;
        
            for(int i = 0; i < m_pChallengeItems.Count; ++i)
            {
                pItem = m_pChallengeItems[i];
                itemSize = pItem.Target.rect.height;
                viewSize -= itemSize;
                pItem.Target.gameObject.SetActive(viewSize > -itemSize);

                pos = pItem.Target.anchoredPosition;            
                pos.y = -i * itemSize;
                pItem.Target.anchoredPosition = pos;
            }
        }
        else if(E_SCROLL_TYPE.leagueRanking == eScrollType)
        {
            LeagueRankingItem pItem = null;
        
            for(int i = 0; i < m_pLeagueRankingItems.Count; ++i)
            {
                pItem = m_pLeagueRankingItems[i];
                itemSize = pItem.Target.rect.height;
                viewSize -= itemSize;
                pItem.Target.gameObject.SetActive(viewSize > -itemSize);

                pos = pItem.Target.anchoredPosition;            
                pos.y = -i * itemSize;
                pItem.Target.anchoredPosition = pos;
            }
        }
        else if(E_SCROLL_TYPE.leagueWin == eScrollType)
        {
            LeagueWinItem pItem = null;
        
            for(int i = 0; i < m_pLeagueWinItems.Count; ++i)
            {
                pItem = m_pLeagueWinItems[i];
                itemSize = pItem.Target.rect.height;
                viewSize -= itemSize;
                pItem.Target.gameObject.SetActive(viewSize > -itemSize);

                pos = pItem.Target.anchoredPosition;            
                pos.y = -i * itemSize;
                pItem.Target.anchoredPosition = pos;
            }
        }
        else if(E_SCROLL_TYPE.leaguePlayer == eScrollType)
        {
            RankingPlayerItem pItem = null;
        
            for(int i = 0; i < m_pLeaguePlayerRankingItems.Count; ++i)
            {
                pItem = m_pLeaguePlayerRankingItems[i];
                itemSize = pItem.Target.rect.height;
                viewSize -= itemSize;
                pItem.Target.gameObject.SetActive(viewSize > -itemSize);

                pos = pItem.Target.anchoredPosition;            
                pos.y = -i * itemSize;
                pItem.Target.anchoredPosition = pos;
            }
        }

        pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir[(int)eScrollType].y = 1;
        
        pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex[(int)eScrollType] = 0;
    }

    void ShowTabUI(E_SCROLL_TYPE eTab)
    {
        ResetScroll(eTab);
        for(E_SCROLL_TYPE i = E_SCROLL_TYPE.challenge; i < E_SCROLL_TYPE.MAX; ++i)
        {
            m_pTabUIList[(int)i].gameObject.SetActive(eTab == i);
        }
        m_eCurrentTab = eTab;

        GameContext pGameContext = GameContext.getCtx();
        m_pLeagueSubTitle.SetText(pGameContext.GetLocalizingText(((E_TEXT_TITLE)m_eCurrentTab).ToString()));
        uint no = pGameContext.GetCurrentSeasonNo();
        if(no > 0)
        {
            uint count = no % 100;
            m_pSeasonTitle.SetText(string.Format(pGameContext.GetLocalizingText("SEASONPASS_TITLE"),(no - count)/100,count));
            m_pTitle.SetText(pGameContext.GetLocalizingText(((E_TEXT_NAME)(m_iCurrentLeague - GameContext.CHALLENGE_ID)).ToString()));
        }

        if(m_eCurrentTab == E_SCROLL_TYPE.leaguePlayer || m_eCurrentTab == E_SCROLL_TYPE.leagueRanking)
        {
            m_pLeagueSeasonTitle.transform.parent.gameObject.SetActive(true);
                    
            m_pAgo.gameObject.SetActive(true);
            m_pCurrent.gameObject.SetActive(true);
            if(m_eCurrentTab == E_SCROLL_TYPE.leaguePlayer)
            {
                ShowSubTabUI(m_eCurrentSubTab);
            }
        }
        else
        {
            m_pLeagueSeasonTitle.transform.parent.gameObject.SetActive(false);
        }
    }

    void ShowClubProfileOverview()
    {
        m_pMainScene.ShowUserProfile(m_ulId,0);
        m_ulId = 0;
    }

    void CallbackShowPlayerProfile()
    {
        m_ulId = 0;
        PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
        pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
    }

    void ScrollViewChangeLeagueWinData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        
        int i = 0;
        LeagueWinItem pItem = null;
        
        if(index > iTarget)
        {
            pItem = m_pLeagueWinItems[iTarget];
            m_pLeagueWinItems[iTarget] = m_pLeagueWinItems[index];
            i = iTarget +1;
            LeagueWinItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pLeagueWinItems[i];
                m_pLeagueWinItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pLeagueWinItems[index] = pItem;
            pItem = m_pLeagueWinItems[iTarget];
        }
        else
        {
            pItem = m_pLeagueWinItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pLeagueWinItems[i -1] = m_pLeagueWinItems[i];
                ++i;
            }

            m_pLeagueWinItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex[(int)m_eCurrentTab] + iTarget + iCount;

        if(i < 0 || m_iTotalDatas[(int)m_eCurrentTab] <= i) return;

        pItem.UpdateInfo(i,m_pLeagueWinDataList[i]);
    }

    void ScrollViewChangeLeaguePlayerRankingData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        
        int i = 0;
        RankingPlayerItem pItem = null;
        
        if(index > iTarget)
        {
            pItem = m_pLeaguePlayerRankingItems[iTarget];
            m_pLeaguePlayerRankingItems[iTarget] = m_pLeaguePlayerRankingItems[index];
            i = iTarget +1;
            RankingPlayerItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pLeaguePlayerRankingItems[i];
                m_pLeaguePlayerRankingItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pLeaguePlayerRankingItems[index] = pItem;
            pItem = m_pLeaguePlayerRankingItems[iTarget];
        }
        else
        {
            pItem = m_pLeaguePlayerRankingItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pLeaguePlayerRankingItems[i -1] = m_pLeaguePlayerRankingItems[i];
                ++i;
            }

            m_pLeaguePlayerRankingItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex[(int)m_eCurrentTab] + iTarget + iCount;

        if(i < 0 || m_iTotalDatas[(int)m_eCurrentTab] <= i) return;

        pItem.UpdateData((JObject)m_pLeagueRankingPlayerData[i],i,(int)m_eCurrentSubTab);
    }

    void ScrollViewChangeChallengeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        
        int i = 0;
        ChallengeItem pItem = null;
        
        if(index > iTarget)
        {
            pItem = m_pChallengeItems[iTarget];
            m_pChallengeItems[iTarget] = m_pChallengeItems[index];
            i = iTarget +1;
            ChallengeItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pChallengeItems[i];
                m_pChallengeItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pChallengeItems[index] = pItem;
            pItem = m_pChallengeItems[iTarget];
        }
        else
        {
            pItem = m_pChallengeItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pChallengeItems[i -1] = m_pChallengeItems[i];
                ++i;
            }

            m_pChallengeItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex[(int)m_eCurrentTab] + iTarget + iCount;

        if(i < 0 || m_iTotalDatas[(int)m_eCurrentTab] <= i) return;

        pItem.UpdateInfo(m_pChallengeDataList[i]);
    }

    void ScrollViewChangeLeagueRankingeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        
        int i = 0;
        LeagueRankingItem pItem = null;
        
        if(index > iTarget)
        {
            pItem = m_pLeagueRankingItems[iTarget];
            m_pLeagueRankingItems[iTarget] = m_pLeagueRankingItems[index];
            i = iTarget +1;
            LeagueRankingItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pLeagueRankingItems[i];
                m_pLeagueRankingItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pLeagueRankingItems[index] = pItem;
            pItem = m_pLeagueRankingItems[iTarget];
        }
        else
        {
            pItem = m_pLeagueRankingItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pLeagueRankingItems[i -1] = m_pLeagueRankingItems[i];
                ++i;
            }

            m_pLeagueRankingItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex[(int)m_eCurrentTab] + iTarget + iCount;

        if(i < 0 || m_iTotalDatas[(int)m_eCurrentTab] <= i) return;

        GameContext pGameContext = GameContext.getCtx();
        pItem.UpdateInfo(m_pLeagueRankingDataList[i],pGameContext.GetLeagueUpStanding(m_iCurrentLeague),pGameContext.GetLeagueDownStanding(m_iCurrentLeague));
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        int index = (int)m_eCurrentTab;
        
        if(m_iTotalScrollItems[index] < m_iTotalDatas[index] && value.y != m_pPrevDir[index].y)
        {
            System.Action<int,int,int> pCallback = null;
            if(m_eCurrentTab == E_SCROLL_TYPE.challenge)
            {
                pCallback = ScrollViewChangeChallengeData;
            }
            else if(m_eCurrentTab == E_SCROLL_TYPE.leagueRanking)
            {
                pCallback = ScrollViewChangeLeagueRankingeData;
            }
            else if(m_eCurrentTab == E_SCROLL_TYPE.leaguePlayer)
            {
                pCallback = ScrollViewChangeLeaguePlayerRankingData;
            }
            else if(m_eCurrentTab == E_SCROLL_TYPE.leagueWin)
            {
                pCallback = ScrollViewChangeLeagueWinData;
            }
            
            m_pScrollRects[index].ScrollViewChangeValue(value - m_pPrevDir[index],ref m_iStartIndex[index],pCallback);
            m_pPrevDir[index] = value;
        }
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        if(m_eCurrentTab == E_SCROLL_TYPE.challenge)
        {
            for(int i =0; i < m_pChallengeItems.Count; ++i)
            {
                if(m_pChallengeItems[i].Target == tm)
                {
                    m_ulId = m_pChallengeItems[i].ID;
                    m_pMainScene.RequestClubProfile(m_ulId,ShowClubProfileOverview);
                    return;
                }
            }
        }
        else if(m_eCurrentTab == E_SCROLL_TYPE.leagueRanking)
        {
            for(int i =0; i < m_pLeagueRankingItems.Count; ++i)
            {
                if(m_pLeagueRankingItems[i].Target == tm)
                {
                    m_ulId = m_pLeagueRankingItems[i].ID;
                    m_pMainScene.RequestClubProfile(m_ulId,ShowClubProfileOverview);
                    return;
                }
            }
        }
        else if(m_eCurrentTab == E_SCROLL_TYPE.leagueWin)
        {
            for(int i =0; i < m_pLeagueWinItems.Count; ++i)
            {
                if(m_pLeagueWinItems[i].Target == tm)
                {
                    if(m_pLeagueWinItems[i].Win.gameObject == sender)
                    {
                        m_ulId = m_pLeagueWinItems[i].WinID;
                    }
                    else
                    {
                        m_ulId = m_pLeagueWinItems[i].SecondID;
                    }
                    m_pMainScene.RequestClubProfile(m_ulId,ShowClubProfileOverview);
                    return;
                }
            }
        }
        else if(m_eCurrentTab == E_SCROLL_TYPE.leaguePlayer)
        {
            for(int i =0; i < m_pLeaguePlayerRankingItems.Count; ++i)
            {
                if(m_pLeaguePlayerRankingItems[i].Target == tm)
                {
                    m_ulId = m_pLeaguePlayerRankingItems[i].Id;
                    m_pMainScene.RequestPlayerProfile(m_ulId,CallbackShowPlayerProfile);
                    return;
                }
            }
        }
    }

    public void ChangeBGM(bool bChange)
    {
        m_bChangeBGM = bChange;
    }

    public void Close()
    {
        if(m_bChangeBGM)
        {
            SoundManager.Instance.ChangeBGM("bgm_main",true,0.4f);
        }
        
        if(m_pLeagueListNode.activeSelf)
        {
            m_pLeagueListNode.SetActive(false);
        }
        
        m_pMainScene.HideMoveDilog(MainUI,Vector3.up);
        m_bChangeBGM = true;
    }

    public void ShowLeagueList(int index)
    {
        m_iCurrentLeague = index == GameContext.LADDER_ID ? GameContext.CHALLENGE_ID : index;
        string strIndex = m_iCurrentLeague.ToString();
        Color color = index == GameContext.getCtx().GetCurrentMatchType() ? Color.white : GameContext.GRAY;
        Button item = null;
        for(int i =0; i < m_pLeagueList.Length; ++i)
        {
            item = m_pLeagueList[i];
            item.enabled = item.gameObject.name != strIndex;
            item.transform.Find("on").gameObject.SetActive(!item.enabled);
            item.transform.Find("text").GetComponent<Graphic>().color = color;
        }
        
        m_eCurrentTab = m_iCurrentLeague == GameContext.CHALLENGE_ID ? E_SCROLL_TYPE.challenge : E_SCROLL_TYPE.leagueRanking;
        for(int i =0; i < m_pScrollRects.Length; ++i)
        {
            m_pScrollRects[i].gameObject.SetActive((int)m_eCurrentTab == i);
        }
    }
    void SendReq()
    {
        for(int i =0; i < m_pScrollRects.Length; ++i)
        {
            m_pScrollRects[i].gameObject.SetActive(false);
            m_pScrollRects[i].enabled = false;
        }

        JObject jObject = null;
        E_REQUEST_ID eReq = E_REQUEST_ID.challengeStage_getStandings;

        if( m_eCurrentTab == E_SCROLL_TYPE.leagueRanking)
        {
            eReq = E_REQUEST_ID.league_getStandings;
            jObject = new JObject();
            jObject["matchType"] = m_iCurrentLeague;
            jObject["seasonNo"] = m_iCurrentSeasonNo;
        }
        else if( m_eCurrentTab == E_SCROLL_TYPE.leaguePlayer)
        {
            eReq = E_REQUEST_ID.playerStats_top100;
            jObject = new JObject();
            jObject["seasonNo"] = m_iCurrentSeasonNo;
            jObject["type"] = (byte)m_eCurrentSubTab;
            jObject["matchType"] = m_iCurrentLeague;
        }
        else if( m_eCurrentTab == E_SCROLL_TYPE.leagueWin)
        {
            eReq = E_REQUEST_ID.league_getLeaders;
            jObject = new JObject();
            jObject["matchType"] = m_iCurrentLeague;
        }

        m_pMainScene.RequestAfterCall(eReq,jObject);        
    }
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(sender.name)
        {
            case "recovery":
            {
                m_pMainScene.ShowHPRecoveryPopup();
            }
            break;
            case "close":
            case "back":
            {
                Close();
            }
            break;
            case "left":
            {
                --m_eCurrentTab;
                if(m_eCurrentTab == E_SCROLL_TYPE.challenge)
                {
                    m_eCurrentTab = E_SCROLL_TYPE.leagueWin;
                }
                ShowTabUI(m_eCurrentTab);
                SendReq();
            }
            break;
            case "right":
            {
                ++m_eCurrentTab;
                if(m_eCurrentTab == E_SCROLL_TYPE.MAX)
                {
                    m_eCurrentTab = E_SCROLL_TYPE.leagueRanking;
                }
                ShowTabUI(m_eCurrentTab);
                SendReq();
            }
            break;
            case "goals":
            case "assists":
            case "prossessionWon":
            case "saves":
            {
                m_eCurrentSubTab = (E_SUB_TAB)Enum.Parse(typeof(E_SUB_TAB), sender.name);
                ShowSubTabUI(m_eCurrentSubTab);
                SendReq();
            }
            break;
            case "11":
            case "12":
            case "13":
            case "14":
            case "15":
            {
                m_iCurrentSeasonNo = GameContext.getCtx().GetCurrentSeasonNo();
                ShowLeagueList(int.Parse(sender.name));
                m_pLeagueListNode.SetActive(false);
                m_pTabUIList[(int)E_SCROLL_TYPE.leaguePlayer].parent.gameObject.SetActive(m_eCurrentTab != E_SCROLL_TYPE.challenge);
                SendReq();
            }
            break;
            case "change":
            {
                m_pLeagueListNode.SetActive(true);
                ShowLeagueList(m_iCurrentLeague);
            }
            break;
            case "list":
            {
                m_pLeagueListNode.SetActive(false);
            }
            break;
            case "challengeMatch":
            {
                m_pMainScene.ShowChallengePopup();
            }
            break;
            case "leagueMatch":
            {
                m_pMainScene.ShowLeagueMatchPopup();
            }
            break;
            case "schedule":
            {
                m_pMainScene.ShowLeagueSchedulePopup();
            }
            break;
            case "reward":
            {
                m_pMainScene.ShowLeagueRankingRewardPopup(m_iCurrentLeague);
            }
            break;
            case "log":
            {
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.challengeStage_getMatches,null,()=>{ m_pMainScene.ShowMatchLogPopup();});
            }
            break;
            case "tip":
            {
                m_pMainScene.ShowGameTip(m_iCurrentLeague == GameContext.CHALLENGE_ID ? "game_tip_challengestage_title" : "game_tip_league_title");
            }
            break;
            case "ago":
            {
                sender.SetActive(false);
                m_pCurrent.gameObject.SetActive(true);
                m_iCurrentSeasonNo = GameContext.getCtx().GetPrevSeasonNo();
                SendReq();
            }
            break;
            case "current":
            {
                sender.SetActive(false);
                m_pAgo.gameObject.SetActive(true);
                m_iCurrentSeasonNo = GameContext.getCtx().GetCurrentSeasonNo();
                SendReq();
            }
            break;
        }
    }
}

