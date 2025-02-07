using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using FlatBuffers;
using ALF.LAYOUT;
using DATA;
using USERDATA;
using STATEDATA;
using MATCHTEAMDATA;
using STATISTICSDATA;
using Newtonsoft.Json.Linq;
using MATCHCOMMENTARY;

public class MatchDetailLog : IBaseUI
{
    enum E_TAB : byte { summary = 0,comparison,homePlayer,awayPlayer,MAX}
    enum E_SCROLL : byte { timeline = 0,comparison,log,MAX}
    enum E_LOG_TAB : byte { Summary = 0,Offensive,Defensive,MAX}
    
    readonly string[] LOG_TAB_KEY = new string[3]{"MATCHVIEW_PLAYER_TXT_SUMMARY","MATCHVIEW_PLAYER_TXT_OFFENSE","MATCHVIEW_PLAYER_TXT_DEFENSE"};
    readonly string[] SCROLL_ITEM_NAMES = new string[3]{"TimeLineItem","ComparisonItem","PlayerLogItem"};
    readonly string[] TEAM_STATISTICS_LIST = new string[]{"MATCHVIEW_COMPARISON_TXT_STAT_POSSESION_TOTAL","MATCHVIEW_COMPARISON_TXT_STAT_SHOOT","MATCHVIEW_COMPARISON_TXT_STAT_GK_GOOD_DEFENSE","MATCHVIEW_COMPARISON_TXT_STAT_SHOOT_ONTARGET","MATCHVIEW_COMPARISON_TXT_STAT_PASS_TRY", "MATCHVIEW_COMPARISON_TXT_STAT_PASS_SUCCESS","MATCHVIEW_COMPARISON_TXT_STAT_STEAL","MATCHVIEW_COMPARISON_TXT_STAT_INTERCEPT","MATCHVIEW_COMPARISON_TXT_STAT_FOUL", "MATCHVIEW_COMPARISON_TXT_STAT_OFFSIDE", "MATCHVIEW_COMPARISON_TXT_CORNERKICK"};
    readonly int[] TEAM_STATISTICS_INDEX_LIST = new int[]{-1,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_GK_GOOD_DEFENCE,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT_ONTARGET,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSTRY,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSSUCCESS,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_STEAL,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_INTERCEPT,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_FOUL,-1,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_CORNERKICK};

    EmblemBake[] m_pEmblemList = new EmblemBake[2];
    Color[] m_pEmblemColor = new Color[2];
    
    Transform[] m_pTabUIList = new Transform[(int)E_TAB.MAX];
    Transform[] m_pTabButtonList = new Transform[(int)E_TAB.MAX];

    float[] m_SummaryHome = new float[]{0,0,0,0,0,0};
    float[] m_SummaryAway = new float[]{0,0,0,0,0,0};

    TMPro.TMP_Text m_pScoreText = null;
    TMPro.TMP_Text m_pTimeText = null;
    TMPro.TMP_Text m_pHomeText = null;
    TMPro.TMP_Text m_pAwayText = null;

    RectTransform m_pInfoUI = null;
    MainScene m_pMainScene = null;
    RecordT m_pRecord = null;
    ScrollRect[] m_pScrollList = new ScrollRect[(int)E_SCROLL.MAX];

    E_TAB m_eCurrentTab = E_TAB.MAX;
    E_LOG_TAB m_eCurrentLogTab = E_LOG_TAB.Summary;

    public RectTransform MainUI { get; private set;}
    public bool Enable {
        set{
            for(int i = 0; i < m_pScrollList.Length; ++i)
            {
                m_pScrollList[i].enabled = value;
            }
        }
    }
    public MatchDetailLog(){}

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "MatchDetailLog : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "MatchDetailLog : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("back").GetComponent<RectTransform>(),this.ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("root/close").GetComponent<RectTransform>(),this.ButtonEventCall);

        m_pInfoUI = MainUI.Find("root/info").GetComponent<RectTransform>();
        int n =0;

        RectTransform ui = m_pInfoUI.Find("summary/summary/goals/home").GetComponent<RectTransform>();
        Vector2 size = ui.offsetMax;
        size.x = m_pInfoUI.rect.width * -0.7f;
        ui.offsetMax = size;
        
        ui = m_pInfoUI.Find("summary/summary/shots/home").GetComponent<RectTransform>();
        ui.offsetMax = size; 
        ui = m_pInfoUI.Find("summary/summary/possesions/home").GetComponent<RectTransform>();
        ui.offsetMax = size; 
        ui = m_pInfoUI.Find("summary/summary/teamRating/home").GetComponent<RectTransform>();
        ui.offsetMax = size; 

        ui = m_pInfoUI.Find("summary/summary/goals/away").GetComponent<RectTransform>();
        size = ui.offsetMin;
        size.x = m_pInfoUI.rect.width * 0.7f;
        ui.offsetMin = size; 
        ui = m_pInfoUI.Find("summary/summary/shots/away").GetComponent<RectTransform>();
        ui.offsetMin = size; 
        ui = m_pInfoUI.Find("summary/summary/possesions/away").GetComponent<RectTransform>();
        ui.offsetMin = size; 
        ui = m_pInfoUI.Find("summary/summary/teamRating/away").GetComponent<RectTransform>();
        ui.offsetMin = size; 
        
        RectTransform pTop = MainUI.Find("root/top").GetComponent<RectTransform>();

        m_pEmblemList[0] =  pTop.Find("view/homeEmblem").GetComponent<EmblemBake>();
        m_pEmblemList[1] =  pTop.Find("view/awayEmblem").GetComponent<EmblemBake>();

        m_pScoreText = pTop.Find("view/score/text").GetComponent<TMPro.TMP_Text>();
        m_pTimeText = pTop.Find("view/time/round").GetComponent<TMPro.TMP_Text>();
        m_pHomeText = pTop.Find("view/home").GetComponent<TMPro.TMP_Text>();
        m_pAwayText = pTop.Find("view/away").GetComponent<TMPro.TMP_Text>();

        LayoutManager.SetReciveUIButtonEvent(m_pInfoUI,this.ButtonEventCall);
        
        RectTransform item = null;

        Vector3 pos;
        ui = MainUI.Find("root/info/tabs").GetComponent<RectTransform>();

        float w = (ui.rect.width / ui.childCount);
        float wh = w * 0.5f;
        float ax = ui.pivot.x * ui.rect.width;
        int iTabIndex = -1;
        
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;
            m_pTabUIList[iTabIndex] = iTabIndex < (int)E_TAB.homePlayer ? m_pInfoUI.Find(item.gameObject.name) : m_pInfoUI.Find("logs");
            m_pTabUIList[iTabIndex].gameObject.SetActive(false);

            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }
        
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);

        ui = m_pInfoUI.Find("logs/tabs").GetComponent<RectTransform>();
        E_LOG_TAB eTag = E_LOG_TAB.Summary;
        RectTransform tm = null;
        while(eTag < E_LOG_TAB.MAX)
        {
            item = ui.Find(eTag.ToString()).GetComponent<RectTransform>();
            size = item.sizeDelta;

            w = (item.rect.width / item.childCount);
            wh = w * 0.5f;
            ax = item.pivot.x * item.rect.width;
            
            for(n =0; n < item.childCount; ++n)
            {
                tm = item.GetChild(n).GetComponent<RectTransform>();
                pos = tm.localPosition;
                size = tm.sizeDelta;
                size.x = w;
                pos.x = wh + (n * w) - ax;
                tm.localPosition = pos;
                tm.sizeDelta = size;
            }
            ++eTag;
        }
        
        m_pScrollList[0] = m_pInfoUI.Find("summary").GetComponentInChildren<ScrollRect>(true);
        m_pScrollList[1] = m_pInfoUI.Find("comparison").GetComponentInChildren<ScrollRect>(true);
        m_pScrollList[2] = m_pInfoUI.Find("logs").GetComponentInChildren<ScrollRect>(true);
        SetupScroll(E_SCROLL.comparison,TEAM_STATISTICS_INDEX_LIST.Length);

        Vector2 sizeDelta = m_pScrollList[0].content.sizeDelta;
        sizeDelta.y = 0;
        m_pScrollList[0].content.sizeDelta = sizeDelta;
        MainUI.gameObject.SetActive(false);
    }

    public void Dispose()
    {
        ClearScroll(E_SCROLL.timeline);
        ClearScroll(E_SCROLL.comparison);
        ClearScroll(E_SCROLL.log);
        int i =0;
        for( i =0; i < m_pScrollList.Length; ++i)
        {
            m_pScrollList[i] = null;
        }

        for( i =0; i < m_pEmblemList.Length; ++i)
        {
            m_pEmblemList[i].material = null;
            m_pEmblemList[i] = null;
        }
        m_pEmblemColor = null;
        m_pEmblemList = null;
        
        for( i =0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i] = null;
            m_pTabButtonList[i] = null;
        }
        m_pRecord = null;
        m_pTabUIList = null;
        m_pTabButtonList  = null;
        m_pInfoUI = null;
        m_pScoreText = null;
        m_pTimeText = null;
        m_pHomeText = null;
        m_pAwayText = null;
        m_pScrollList = null;
        m_pMainScene = null;
        MainUI = null; 
    }

    public void SetupClubName(string home,string away)
    {
        m_pHomeText.SetText(home);
        m_pAwayText.SetText(away);
    }

    public void SetupScore(string score)
    {
        string[] temp = score.Split(':');
        m_SummaryHome[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_GOAL] = float.Parse(temp[0]);
        m_SummaryAway[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_GOAL] = float.Parse(temp[1]);
        m_pScoreText.SetText(score);
    }

    public void SetupRecordData(RecordT data,EmblemBake pEmblemBake)
    {
        if(data.HomeId == 0 || data.HomeId == GameContext.getCtx().GetClubID())
        {
            m_pEmblemList[0].CopyPoint(m_pMainScene.GetMyEmblem());
            m_pEmblemList[1].CopyPoint(pEmblemBake);
        }
        else
        {
            m_pEmblemList[1].CopyPoint(m_pMainScene.GetMyEmblem());
            m_pEmblemList[0].CopyPoint(pEmblemBake);
        }
        
        m_pEmblemColor[0] = m_pEmblemList[0].material.GetColor("_Pattern1Color");
        m_pEmblemColor[1] = m_pEmblemList[1].material.GetColor("_Pattern1Color");

        MainUI.Find("root/top/homeBar").GetComponent<Graphic>().color = m_pEmblemColor[0];
        MainUI.Find("root/top/awayBar").GetComponent<Graphic>().color = m_pEmblemColor[1];

        m_pInfoUI.Find("summary/summary/goals/home/fill").GetComponent<Graphic>().color = m_pEmblemColor[0];
        m_pInfoUI.Find("summary/summary/goals/away/fill").GetComponent<Graphic>().color = m_pEmblemColor[1];

        m_pInfoUI.Find("summary/summary/shots/home/fill").GetComponent<Graphic>().color = m_pEmblemColor[0];
        m_pInfoUI.Find("summary/summary/shots/away/fill").GetComponent<Graphic>().color = m_pEmblemColor[1];

        m_pInfoUI.Find("summary/summary/possesions/home/fill").GetComponent<Graphic>().color = m_pEmblemColor[0];
        m_pInfoUI.Find("summary/summary/possesions/away/fill").GetComponent<Graphic>().color = m_pEmblemColor[1];

        m_pInfoUI.Find("summary/summary/teamRating/home/fill").GetComponent<Graphic>().color = m_pEmblemColor[0];
        m_pInfoUI.Find("summary/summary/teamRating/away/fill").GetComponent<Graphic>().color = m_pEmblemColor[1];

        ScrollRect pScroll = m_pScrollList[(int)E_TAB.comparison];
        Transform pItem = null;
        for(int i =0; i < pScroll.content.childCount; ++i)
        {
            pItem = pScroll.content.GetChild(i);
            if(pItem)
            {
                pItem.Find("gauge/home").GetComponent<Graphic>().color = m_pEmblemColor[0];
                pItem.Find("gauge/away").GetComponent<Graphic>().color = m_pEmblemColor[1];
            }
        }
        
        m_pRecord = data;
        if(m_pRecord != null && m_pRecord.TimeRecord != null)
        {
            GameContext pGameContext = GameContext.getCtx();
            ulong myID = pGameContext.GetClubID();
            if(m_pRecord.HomeId == myID || m_pRecord.HomeId == 0)
            {
                SetupClubName(pGameContext.GetClubName(),m_pRecord.AwayName);
            }
            else
            {
                SetupClubName(m_pRecord.AwayName,pGameContext.GetClubName());
            }

            for(int i = 0; i < m_pRecord.TimeRecord.Count; ++i)
            {
                AddTimeLineItem(m_pRecord.TimeRecord[i]);
            }
        }
        
        ShowTabUI(E_TAB.summary);
        m_pScoreText.SetText($"{m_SummaryHome[0]}:{m_SummaryAway[0]}");
    }

    public void SetupSummaryData(JObject data, EmblemBake pEmblemBake)
    {
        ClearScroll(E_SCROLL.timeline);
        if(data != null && data.ContainsKey("matchStats"))
        {
            JObject matchStats = (JObject)data["matchStats"];
            RecordT pRecord = null;

            if(matchStats.ContainsKey("timeLine") && matchStats["timeLine"].Type != JTokenType.Null)
            {
                pRecord = Newtonsoft.Json.JsonConvert.DeserializeObject<RecordT>((string)matchStats["timeLine"]);
                m_pTimeText.SetText("90:00");
                pRecord.AwayId = (ulong)matchStats["away"];
                pRecord.HomeId = (ulong)matchStats["home"];
            }
            else
            {
                pRecord = new RecordT();
                pRecord.Id = (ulong)matchStats["id"];
                pRecord.AwayId = (ulong)matchStats["away"];
                pRecord.HomeId = (ulong)matchStats["home"];
                m_pTimeText.SetText("기권");
            }
            if(pRecord != null)
            {
                SetupRecordData(pRecord,pEmblemBake);
                GameContext.getCtx().AddMatchLogData(pRecord);
            }
        }
    }

    void UpdateMatchStatisticsData()
    {
        if(m_pRecord.StatisticsRecord == null)
        {
            if(m_eCurrentTab == E_TAB.summary)
            {
                for(int i = 0; i < m_SummaryHome.Length; ++i )
                {
                    m_SummaryHome[i] = 0;
                    m_SummaryAway[i] = 0;
                }
                UpdateSummaryData();
            }    
            else if(m_eCurrentTab == E_TAB.comparison)
            {
                m_pScrollList[(int)E_SCROLL.comparison].gameObject.SetActive(false);
            }
            else
            {
                m_pScrollList[(int)E_SCROLL.log].gameObject.SetActive(false);
            }
            return;
        }
        MatchStatistics pMatchStatistics = MatchStatistics.GetRootAsMatchStatistics(new ByteBuffer(m_pRecord.StatisticsRecord.SerializeToBinary()));
        
        float home = (float)pMatchStatistics.StatisticsTeam(0).Value.PossessionTotal;
        float away = (float)pMatchStatistics.StatisticsTeam(1).Value.PossessionTotal;
        float total = Mathf.Max(home + away,1);
        m_SummaryHome[2] = home / total * 100.0f;
        m_SummaryAway[2] = away / total * 100.0f;
        
        if(m_eCurrentTab == E_TAB.summary)
        {
            m_SummaryHome[1] = pMatchStatistics.StatisticsTeam(0).Value.TotalShooting;
            m_SummaryAway[1] = pMatchStatistics.StatisticsTeam(1).Value.TotalShooting;
            
            m_SummaryHome[3] = pMatchStatistics.StatisticsTeam(0).Value.TeamRating / 10f;
            m_SummaryAway[3] = pMatchStatistics.StatisticsTeam(1).Value.TeamRating / 10f;

            m_SummaryHome[0] = (float)(pMatchStatistics.StatisticsTeam(0).Value.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_GOAL));
            m_SummaryAway[0] = (float)(pMatchStatistics.StatisticsTeam(1).Value.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_GOAL));

            m_SummaryHome[4] = (float)(pMatchStatistics.StatisticsTeam(0).Value.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_RED_CARD));
            m_SummaryAway[4] = (float)(pMatchStatistics.StatisticsTeam(1).Value.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_RED_CARD));

            m_SummaryHome[5] = (float)(pMatchStatistics.StatisticsTeam(0).Value.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_YELLO_CARD));
            m_SummaryAway[5] = (float)(pMatchStatistics.StatisticsTeam(1).Value.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_YELLO_CARD));

            UpdateSummaryData();
        }    
        else if(m_eCurrentTab == E_TAB.comparison)
        {
            m_pScrollList[(int)E_SCROLL.comparison].gameObject.SetActive(true);
            UpdateTeamStatisticsData(pMatchStatistics);
        }
        else
        {
            m_pScrollList[(int)E_SCROLL.log].gameObject.SetActive(true);
            if(m_eCurrentTab == E_TAB.homePlayer)
            {
                UpdatePlayerStatisticsData(pMatchStatistics,0);
            }
            else
            {
                UpdatePlayerStatisticsData(pMatchStatistics,1);
            }
        }
    }

    void ShowTabUI(E_TAB eTab)
    {
        m_eCurrentTab = eTab;
        int index = (int)m_eCurrentTab;
        int i = 0;
        for(i = 0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i].gameObject.SetActive(index ==i);
            m_pTabButtonList[i].Find("on").gameObject.SetActive(index ==i);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = index ==i ? Color.white : GameContext.GRAY;
        }
        
        if(m_eCurrentTab == E_TAB.homePlayer)
        {
            m_pTabUIList[2].gameObject.SetActive(true);
            if(m_pRecord.StatisticsRecord != null)
            {
                SetupScroll(E_SCROLL.log,m_pRecord.StatisticsRecord.StatisticsPlayers[0].Players.Count);
            }
        }
        else if(m_eCurrentTab == E_TAB.awayPlayer)
        {
            m_pTabUIList[2].gameObject.SetActive(true);
            if(m_pRecord.StatisticsRecord != null)
            {
                SetupScroll(E_SCROLL.log,m_pRecord.StatisticsRecord.StatisticsPlayers[1].Players.Count);
            }
        }

        ShowSubTap(E_LOG_TAB.Summary);
        UpdateMatchStatisticsData();
    }

    void ShowSubTap(E_LOG_TAB eTag)
    {
        if(m_eCurrentTab > E_TAB.comparison)
        {
            m_eCurrentLogTab = eTag;
            Transform tm = m_pInfoUI.Find("logs/tabs");
            TMPro.TMP_Text text = tm.Find("toggle/name").GetComponent<TMPro.TMP_Text>();
            text.SetText(GameContext.getCtx().GetLocalizingText(LOG_TAB_KEY[(int)m_eCurrentLogTab]));

            E_LOG_TAB e = E_LOG_TAB.Summary;
            
            while(e < E_LOG_TAB.MAX)
            {
                tm.Find(e.ToString()).gameObject.SetActive(e == m_eCurrentLogTab);
                ++e;
            }
            bool bHome = m_eCurrentTab == E_TAB.homePlayer;

            ScrollRect pScroll = m_pScrollList[(int)E_SCROLL.log];
            for(int i =0; i < pScroll.content.childCount; ++i)
            {
                tm = pScroll.content.GetChild(i);
                e = E_LOG_TAB.Summary;
                while(e < E_LOG_TAB.MAX)
                {
                    tm.Find(e.ToString()).gameObject.SetActive(e == m_eCurrentLogTab);
                    ++e;
                }   
            }
        }
    }
    
    void UpdateTeamStatisticsData(MatchStatistics pMatchStatistics)
    {
        ScrollRect pScroll = m_pScrollList[(int)E_SCROLL.comparison];
        Transform pItem = null;
        TMPro.TMP_Text text = null;
        Image pGauge = null;
        float total = 0;
        int h =0;
        int a =0;
        pItem = pScroll.content.GetChild(0);

        h = pMatchStatistics.StatisticsTeam(0).Value.PossessionTotal;
        a = pMatchStatistics.StatisticsTeam(1).Value.PossessionTotal;
        pGauge = pItem.Find("gauge/home").GetComponent<Image>();
        total = h + a;
        total = Mathf.Max(total,1);
        text = pItem.Find("home").GetComponent<TMPro.TMP_Text>();
        text.SetText(string.Format("{0:0.#}%",h / total* 100.0f));
        text = pItem.Find("away").GetComponent<TMPro.TMP_Text>();
        text.SetText(string.Format("{0:0.#}%",a / total* 100.0f));
        total = h / total;
        pGauge.fillAmount = h > 0 ? total : 0.5f;

        for(int i = 1; i < TEAM_STATISTICS_INDEX_LIST.Length -1; ++i)
        {
            h = (int)pMatchStatistics.StatisticsTeam(0).Value.Common(TEAM_STATISTICS_INDEX_LIST[i]);
            a = (int)pMatchStatistics.StatisticsTeam(1).Value.Common(TEAM_STATISTICS_INDEX_LIST[i]);
            pItem = pScroll.content.GetChild(i);
            text = pItem.Find("home").GetComponent<TMPro.TMP_Text>();
            text.SetText(h.ToString());
            text = pItem.Find("away").GetComponent<TMPro.TMP_Text>();
            text.SetText(a.ToString());
            pGauge = pItem.Find("gauge/home").GetComponent<Image>();
            
            if(h == 0 && a == 0)
            {
                total = 0.5f;
            }
            else
            {
                total = h + a;
                total = Mathf.Max(total,1);
                total = h / total;
            }
            pGauge.fillAmount = total;
        }

        pItem = pScroll.content.GetChild(TEAM_STATISTICS_INDEX_LIST.Length -1);

        h = pMatchStatistics.StatisticsTeam(0).Value.TotalOffside;
        a = pMatchStatistics.StatisticsTeam(1).Value.TotalOffside;
        pGauge = pItem.Find("gauge/home").GetComponent<Image>();
        text = pItem.Find("home").GetComponent<TMPro.TMP_Text>();
        text.SetText(h.ToString());
        text = pItem.Find("away").GetComponent<TMPro.TMP_Text>();
        text.SetText(a.ToString());

        if(h == 0 && a == 0)
        {
            total = 0.5f;
        }
        else
        {
            total = h + a;
            total = Mathf.Max(total,1);
            total = h / total;
        }

        pGauge.fillAmount = total;
    }

    void UpdatePlayerStatisticsData(MatchStatistics pMatchStatistics,int teamIndex)
    {
        ScrollRect pScroll = m_pScrollList[(int)E_SCROLL.log];
        Transform pItem = null;
        TMPro.TMP_Text text = null;
        StatisticsPlayers pStatisticsPlayers = pMatchStatistics.StatisticsPlayers(teamIndex).Value;
        PlayerStatistics pPlayerStatistics;
        float x =0;
        RectTransform ui = null;
        Vector2 offset;
        int index = 0;
        int value = 0;
        bool bRun = true;
        GameContext pGameContext = GameContext.getCtx();
        for(int i = 0 ; i < pStatisticsPlayers.PlayersLength; ++i)
        {
            bRun = true;
            pItem = pScroll.content.GetChild(i);
            pPlayerStatistics = pStatisticsPlayers.Players(i).Value;
            pItem.gameObject.name = pPlayerStatistics.PlayerId.ToString();

            pItem.Find("player/substitution").gameObject.SetActive(false);
            pItem.Find("player/card").gameObject.SetActive(false);
            pItem.Find("player/substitution/in").gameObject.SetActive(false);
            pItem.Find("player/substitution/out").gameObject.SetActive(false);
            pItem.Find("player/card/r").gameObject.SetActive(false);
            pItem.Find("player/card/y").gameObject.SetActive(false);
            x = 0;

            if(pPlayerStatistics.SubstitutionOutTotalGameSec > 0)
            {
                pItem.Find("player/substitution/out").gameObject.SetActive(true);
                ui = pItem.Find("player/substitution").GetComponent<RectTransform>();
                ui.gameObject.SetActive(true);
                x = ui.rect.width;
            }
            else 
            {
                if(pPlayerStatistics.SubstitutionInTotalGameSec > 0)
                {
                    pItem.Find("player/substitution/in").gameObject.SetActive(true);
                    ui = pItem.Find("player/substitution").GetComponent<RectTransform>();
                    ui.gameObject.SetActive(true);
                    x = ui.rect.width;
                }
            }

            if(pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_RED_CARD) > 0)
            {
                pItem.Find("player/card/r").gameObject.SetActive(true);
                ui = pItem.Find("player/card").GetComponent<RectTransform>();
                ui.gameObject.SetActive(true);
                offset = ui.offsetMin;
                offset.x = x;
                ui.offsetMin = offset;
                x += ui.rect.width;
            }
            else 
            {
                if(pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_YELLO_CARD) > 0)
                {
                    pItem.Find("player/card/y").gameObject.SetActive(true);
                    ui = pItem.Find("player/card").GetComponent<RectTransform>();
                    ui.gameObject.SetActive(true);
                    offset = ui.offsetMin;
                    offset.x = x;
                    ui.offsetMin = offset;
                    x += ui.rect.width;
                }
            }
            
            text = pItem.Find("player/name").GetComponent<TMPro.TMP_Text>();
            text.SetText(pPlayerStatistics.Name);

            pItem.Find("bg/DF").gameObject.SetActive(false);
            pItem.Find("bg/FW").gameObject.SetActive(false);
            pItem.Find("bg/GK").gameObject.SetActive(false);
            pItem.Find("bg/MF").gameObject.SetActive(false);
            pItem.Find("bg/S").gameObject.SetActive(false);
            
            text = pItem.Find("form").GetComponent<TMPro.TMP_Text>();
            text.SetText(pGameContext.GetDisplayLocationName(index));

            for(int n = 0; n < m_pRecord.LineUpRecord[teamIndex].List.Count; ++n)
            {
                if(m_pRecord.LineUpRecord[teamIndex].List[n] == i)
                {
                    if(n < 11)
                    {
                        index = pGameContext.ConvertPositionByTag((E_LOCATION)m_pRecord.LineUpRecord[teamIndex].Formation[n]);
                        text.SetText(pGameContext.GetDisplayLocationName(index));
                        pItem.Find($"bg/{pGameContext.GetDisplayCardFormationByLocationName(pGameContext.GetDisplayLocationName(m_pRecord.LineUpRecord[teamIndex].Formation[n]))}").gameObject.SetActive(true);
                    }
                    else
                    {
                        bRun = false;
                        text.SetText($"S{n - 10}");
                        pItem.Find("bg/S").gameObject.SetActive(true);
                    }
                    break;
                }
            }

            ui = pItem.Find(E_LOG_TAB.Summary.ToString()).GetComponent<RectTransform>();
            text = ui.Find("assist").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_ASSIST);
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());
            text = ui.Find("goal").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_GOAL);
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());
            text = ui.Find("foul").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_FOUL);            
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());
            
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_INJURY);
            text = ui.Find("hp/text").GetComponent<TMPro.TMP_Text>();
            text.SetText(pPlayerStatistics.Hp.ToString());
            
            RectTransform tm = text.GetComponent<RectTransform>();
            Vector2 pos = tm.anchoredPosition;
            if(value > 0)
            {    
                pos.x = -30;
                tm.anchoredPosition = pos;
                ui.Find("hp/injury").gameObject.SetActive(true);
            }
            else
            {
                pos.x = 0;
                tm.anchoredPosition = pos;
                ui.Find("hp/injury").gameObject.SetActive(false);
            }
            
            text = ui.Find("totalRunDistance").GetComponent<TMPro.TMP_Text>();
            if(pPlayerStatistics.TotalRunDistance > 0)
            {
                text.SetText(string.Format("{0:0.#}km",pPlayerStatistics.TotalRunDistance * (2f / 300f)));
            }
            else
            {
                text.SetText("-");
            }
            
            text = ui.Find("rating").GetComponent<TMPro.TMP_Text>();
            
            if(!bRun && pPlayerStatistics.TotalRunDistance == 0 )
            {
                text.SetText("-");
            }
            else
            {
                text.SetText(string.Format("{0:0.#}",pPlayerStatistics.Rating));
            }
            
            ui = pItem.Find(E_LOG_TAB.Offensive.ToString()).GetComponent<RectTransform>();
            text = ui.Find("shoot").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT);
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());

            text = ui.Find("shootOT").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT_ONTARGET);
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());

            text = ui.Find("passSuccess").GetComponent<TMPro.TMP_Text>();
            x = (float)pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSSUCCESS) / Mathf.Max((float)pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSTRY),1.0f) * 100.0f;
            text.color = x == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(string.Format("{0:0.0#}%",x));
            text = ui.Find("assist").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_ASSIST);
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());

            text = ui.Find("goal").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_GOAL);
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());
            
            ui = pItem.Find(E_LOG_TAB.Defensive.ToString()).GetComponent<RectTransform>();
            text = ui.Find("intercept").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_INTERCEPT);
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());
            text = ui.Find("tackle").GetComponent<TMPro.TMP_Text>();
            text.color = pPlayerStatistics.TackleTry == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(pPlayerStatistics.TackleTry.ToString());
            text = ui.Find("steal").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_STEAL);
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());
            text = ui.Find("foul").GetComponent<TMPro.TMP_Text>();
            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_FOUL) + pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_YELLO_CARD) +pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_RED_CARD);
            text.color = value == 0 ? GameContext.GRAY_W : GameContext.GRAY;
            text.SetText(value.ToString());
        }
    }

    void UpdateSummaryData()
    {
        TMPro.TMP_Text text = m_pInfoUI.Find("summary/summary/goals/home/no").GetComponent<TMPro.TMP_Text>();
        float total = Mathf.Max(m_SummaryHome[0] + m_SummaryAway[0],1);
        text.SetText(m_SummaryHome[0].ToString());
        Image fill = m_pInfoUI.Find("summary/summary/goals/home/fill").GetComponent<Image>();
        fill.fillAmount = m_SummaryHome[0] / total;
        text = m_pInfoUI.Find("summary/summary/goals/away/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_SummaryAway[0].ToString());
        fill = m_pInfoUI.Find("summary/summary/goals/away/fill").GetComponent<Image>();
        fill.fillAmount = m_SummaryAway[0] / total;

        total = Mathf.Max(m_SummaryHome[1] + m_SummaryAway[1],1);
        text = m_pInfoUI.Find("summary/summary/shots/home/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_SummaryHome[1].ToString());
        fill = m_pInfoUI.Find("summary/summary/shots/home/fill").GetComponent<Image>();
        fill.fillAmount = m_SummaryHome[1] / total;

        text = m_pInfoUI.Find("summary/summary/shots/away/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_SummaryAway[1].ToString());
        fill = m_pInfoUI.Find("summary/summary/shots/away/fill").GetComponent<Image>();
        fill.fillAmount = m_SummaryAway[1] / total;
        
        total = 100;
        text = m_pInfoUI.Find("summary/summary/possesions/home/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(string.Format("{0:0.#}%",m_SummaryHome[2]));
        fill = m_pInfoUI.Find("summary/summary/possesions/home/fill").GetComponent<Image>();
        fill.fillAmount = m_SummaryHome[2] / total;

        text = m_pInfoUI.Find("summary/summary/possesions/away/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(string.Format("{0:0.#}%",m_SummaryAway[2]));
        
        fill = m_pInfoUI.Find("summary/summary/possesions/away/fill").GetComponent<Image>();
        fill.fillAmount = m_SummaryAway[2] / total;
        
        text = m_pInfoUI.Find("summary/summary/teamRating/home/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(string.Format("{0:0.#}",m_SummaryHome[3]));
        fill = m_pInfoUI.Find("summary/summary/teamRating/home/fill").GetComponent<Image>();
        fill.fillAmount = m_SummaryHome[3] / total;

        text = m_pInfoUI.Find("summary/summary/teamRating/away/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(string.Format("{0:0.#}",m_SummaryAway[3]));
        fill = m_pInfoUI.Find("summary/summary/teamRating/away/fill").GetComponent<Image>();
        fill.fillAmount = m_SummaryAway[3] / total;
                
        text = m_pInfoUI.Find("summary/summary/wonring/home/r/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_SummaryHome[4].ToString());
        text = m_pInfoUI.Find("summary/summary/wonring/away/r/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_SummaryAway[4].ToString());
        text = m_pInfoUI.Find("summary/summary/wonring/home/y/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_SummaryHome[5].ToString());
        text = m_pInfoUI.Find("summary/summary/wonring/away/y/no").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_SummaryAway[5].ToString());
    }
    
    void ClearScroll(E_SCROLL eScroll)
    {
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollList[(int)eScroll],null);
        int i = m_pScrollList[(int)eScroll].content.childCount;
        while(i > 0)
        {
            --i;
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAMES[(int)eScroll],m_pScrollList[(int)eScroll].content.GetChild(i).GetComponent<RectTransform>());
        }

        Vector2 size = m_pScrollList[(int)eScroll].content.sizeDelta;
        size.y =0;
        m_pScrollList[(int)eScroll].content.sizeDelta = size;
        m_pScrollList[(int)eScroll].content.anchoredPosition = Vector2.zero;
    }
    void SetupScroll( E_SCROLL eType,int total)
    {
        ScrollRect pScroll = m_pScrollList[(int)eType];
        ALFUtils.Assert(pScroll != null, "MatchDetailLog : SetupScroll => pScroll = null !");
        string itemName = SCROLL_ITEM_NAMES[(int)eType];
        ClearScroll(eType);

        Vector2 size;
        float h = 0;
        float w = 0;
        float wh = 0;
        float ax = 0;
        Vector3 pos;
        TMPro.TMP_Text text = null;
        RectTransform pItem = null;
        GameContext pGameContext = GameContext.getCtx();
        for(int i =0; i < total; ++i)
        {
            pItem = LayoutManager.Instance.GetItem<RectTransform>(itemName);
    
            if(pItem)
            {
                if(eType == E_SCROLL.comparison)
                {
                    text = pItem.Find("title").GetComponent<TMPro.TMP_Text>();
                    text.SetText(pGameContext.GetLocalizingText(TEAM_STATISTICS_LIST[i]));
                }
                
                pItem.SetParent(pScroll.content,false);
                
                pItem.localScale = Vector3.one;
                pItem.anchoredPosition = new Vector2(0,-h);
                size = pItem.sizeDelta;
                h += size.y;
                size.x = pScroll.content.sizeDelta.x;
                pItem.sizeDelta = size;

                if(eType > E_SCROLL.comparison)
                {
                    E_LOG_TAB eTag = E_LOG_TAB.Summary;
                    RectTransform tm = null;
                    RectTransform ui = null;
                    while(eTag < E_LOG_TAB.MAX)
                    {
                        ui = pItem.Find(eTag.ToString()).GetComponent<RectTransform>();
                        size = ui.sizeDelta;

                        w = (ui.rect.width / ui.childCount);
                        wh = w * 0.5f;
                        ax = ui.pivot.x * ui.rect.width;
                        
                        for(int n =0; n < ui.childCount; ++n)
                        {
                            tm = ui.GetChild(n).GetComponent<RectTransform>();
                            pos = tm.localPosition;
                            size = tm.sizeDelta;
                            size.x = w;
                            pos.x = wh + (n * w) - ax;
                            tm.localPosition = pos;
                            tm.sizeDelta = size;
                        }
                        ui.gameObject.SetActive(false);
                        ++eTag;
                    }
                }
            }
        }

        size = pScroll.content.sizeDelta;
        size.y = h;
        pScroll.content.sizeDelta = size;
        LayoutManager.SetReciveUIScollViewEvent(pScroll,ScrollViewItemButtonEventCall);
    }

    void AddTimeLineItem( TimeLineT pTimeLine)
    {
        E_NOTICE eType = E_NOTICE.injury;

        if(pTimeLine.Goal != null)
        {
            eType = E_NOTICE.goal;
        }
        else if(pTimeLine.Substitution != null)
        {
            eType = E_NOTICE.substitution;
        }
        else if(pTimeLine.Card != null)
        {
            eType = E_NOTICE.card;
        }

        GameContext pGameContext = GameContext.getCtx();
        ScrollRect pScroll = m_pScrollList[(int)E_SCROLL.timeline];
        string itemName = SCROLL_ITEM_NAMES[(int)E_SCROLL.timeline];
                
        float h = pScroll.content.rect.height;
        
        RectTransform pItem = LayoutManager.Instance.GetItem<RectTransform>(itemName);
        pItem.Find("line").gameObject.SetActive(pScroll.content.childCount > 0);

        for(E_NOTICE i = 0; i < E_NOTICE.MAX; ++i)
        {
            pItem.Find(i.ToString()).gameObject.SetActive(i == eType);
        }

        TMPro.TMP_Text text = pItem.Find("text").GetComponent<TMPro.TMP_Text>();
        
        if(eType == E_NOTICE.goal)
        {
            text.SetText(pTimeLine.Goal.Minute.ToString());

            pItem.Find("goal/away").gameObject.SetActive(pTimeLine.Goal.TeamIndex == 1);
            pItem.Find("goal/home").gameObject.SetActive(pTimeLine.Goal.TeamIndex == 0);
            
            if(pTimeLine.Goal.TeamIndex == 0)
            {
                text = pItem.Find("goal/home").GetComponent<TMPro.TMP_Text>();
            }
            else
            {
                text = pItem.Find("goal/away").GetComponent<TMPro.TMP_Text>();
            }
            
            text.SetText(pTimeLine.Goal.PlayerGoalName);
            RectTransform tm = text.GetComponent<RectTransform>();
            text = text.transform.Find("assist").GetComponent<TMPro.TMP_Text>();
            
            Vector2 anchoredPosition = tm.anchoredPosition;

            if(string.IsNullOrEmpty(pTimeLine.Goal.PlayerAssistName))
            {
                text.gameObject.SetActive(false);
                anchoredPosition.y = 0;
            }
            else
            {
                text.gameObject.SetActive(true);
                text.SetText(pTimeLine.Goal.PlayerAssistName);
                anchoredPosition.y = tm.rect.height * 0.5f;
            }
            tm.anchoredPosition = anchoredPosition;
        }
        else if(eType == E_NOTICE.substitution)
        {
            text.SetText(pTimeLine.Substitution.Minute.ToString());
            text = pItem.Find("substitution/home").GetComponent<TMPro.TMP_Text>();
            text.SetText(pTimeLine.Substitution.PlayerInName);
            text.gameObject.SetActive(pTimeLine.Substitution.TeamIndex == 0);
            text = text.transform.Find("out").GetComponent<TMPro.TMP_Text>();
            text.SetText(pTimeLine.Substitution.PlayerOutName);
            text = pItem.Find("substitution/away").GetComponent<TMPro.TMP_Text>();
            text.gameObject.SetActive(pTimeLine.Substitution.TeamIndex == 1);
            text.SetText(pTimeLine.Substitution.PlayerInName);
            text = text.transform.Find("out").GetComponent<TMPro.TMP_Text>();
            text.SetText(pTimeLine.Substitution.PlayerOutName);
        }
        else if(eType == E_NOTICE.card)
        {
            text.SetText(pTimeLine.Card.Minute.ToString());
            
            text = pItem.Find("card/away").GetComponent<TMPro.TMP_Text>();
            text.SetText(pTimeLine.Card.PlayerName);
            text.gameObject.SetActive(pTimeLine.Card.TeamIndex == 1);
            text.transform.Find("icon/y").gameObject.SetActive(pTimeLine.Card.Yellow > 0);
            text.transform.Find("icon/r").gameObject.SetActive(pTimeLine.Card.Red > 0);
            
            text = pItem.Find("card/home").GetComponent<TMPro.TMP_Text>();
            text.SetText(pTimeLine.Card.PlayerName);
            text.gameObject.SetActive(pTimeLine.Card.TeamIndex == 0);
            text.transform.Find("icon/y").gameObject.SetActive(pTimeLine.Card.Yellow > 0);
            text.transform.Find("icon/r").gameObject.SetActive(pTimeLine.Card.Red > 0);
        }
        else if(eType == E_NOTICE.injury)
        {
            text.SetText(pTimeLine.Injury.Minute.ToString());
            
            text = pItem.Find("injury/away").GetComponent<TMPro.TMP_Text>();
            text.SetText(pTimeLine.Injury.PlayerName);
            text.gameObject.SetActive(pTimeLine.Injury.TeamIndex == 1);
            
            text = pItem.Find("injury/home").GetComponent<TMPro.TMP_Text>();
            text.SetText(pTimeLine.Injury.PlayerName);
            text.gameObject.SetActive(pTimeLine.Injury.TeamIndex == 0);
        }

        pItem.SetParent(pScroll.content,false);
                
        pItem.localScale = Vector3.one;
        pItem.anchoredPosition = new Vector2(0,h);

        h += pItem.sizeDelta.y;
        
        Vector2 size = pScroll.content.sizeDelta;
        size.y = h;
        
        pScroll.content.sizeDelta = size;
        
        if(h > pScroll.viewport.rect.height)
        {
            size = pScroll.content.anchoredPosition;
            size.y = pScroll.viewport.rect.height - pScroll.content.rect.height;
            pScroll.content.anchoredPosition = size;
            // pScroll.verticalNormalizedPosition = 1;
            Canvas.ForceUpdateCanvases();
        }
        // pScroll.content.localPosition = SingleFunc.GetSnapToPositionToBringChildIntoView(pScroll,pItem);
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        ulong id = 0;
        if(ulong.TryParse(tm.gameObject.name,out id))
        {
            // PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
            // E_PLAYER_INFO_TYPE eTeamType = m_eCurrentTab == E_TAB.homePlayer ? E_PLAYER_INFO_TYPE.my : E_PLAYER_INFO_TYPE.match;
            // pPlayerInfo.SetupPlayerInfoData(eTeamType,GetPlayerByID(eTeamType,id));
            // pPlayerInfo.SetupQuickPlayerInfoData(null);
            // pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
        }  
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI,()=>{
            m_pRecord = null;
            ClearScroll(E_SCROLL.timeline);
            ClearScroll(E_SCROLL.log);
            m_eCurrentTab = E_TAB.MAX;
            MainUI.gameObject.SetActive(false);
            MainUI.Find("root").localScale = Vector3.one;
            LayoutManager.Instance.InteractableEnabledAll();
        });
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(sender.name)
        {
            case "toggle":
            {
                ++m_eCurrentLogTab;
                if(m_eCurrentLogTab == E_LOG_TAB.MAX)
                {
                    m_eCurrentLogTab = E_LOG_TAB.Summary;
                }
                ShowSubTap(m_eCurrentLogTab);
                return;
            }
            case "summary":
            case "comparison":
            case "homePlayer":
            case "awayPlayer":
            {
                ShowTabUI((E_TAB)Enum.Parse(typeof(E_TAB), sender.name));
                return;
            }
        }

        Close();
    }
}
