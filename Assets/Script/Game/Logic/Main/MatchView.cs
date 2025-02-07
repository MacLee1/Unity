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
using UnityEngine.EventSystems;
using STATEDATA;
using MATCHTEAMDATA;
using STATISTICSDATA;
using Newtonsoft.Json.Linq;
using MATCHCOMMENTARY;
using ADREWARD;
using ALF.NETWORK;

public class MatchView : IBaseUI,IBaseNetwork
{
    const int WIN_ID = 3;
    const int DRAW_ID = 1;
    
    enum E_TAB : byte { summary = 0,comparison,homePlayer,awayPlayer,MAX}
    enum E_SCROLL : byte { timeline = 0,comparison,log,MAX}
    
    enum E_LOG_TAB : byte { Summary = 0,Offensive,Defensive,MAX}

    enum E_AD : byte { None = 0,Match3=101,WinStreak=103,ReverseMatch=102,Lose=104}
    
    const string NOTICE_ITEM_NAME = "NoticeItem";
    const string REWARD_ITEM_NAME = "RewardItem";

    readonly string[] LOG_TAB_KEY = new string[3]{"MATCHVIEW_PLAYER_TXT_SUMMARY","MATCHVIEW_PLAYER_TXT_OFFENSE","MATCHVIEW_PLAYER_TXT_DEFENSE"};

    readonly string[] SCROLL_ITEM_NAMES = new string[3]{"TimeLineItem","ComparisonItem","PlayerLogItem"};
    readonly string[] TEAM_STATISTICS_LIST = new string[]{"MATCHVIEW_COMPARISON_TXT_STAT_POSSESION_TOTAL","MATCHVIEW_COMPARISON_TXT_STAT_SHOOT","MATCHVIEW_COMPARISON_TXT_STAT_GK_GOOD_DEFENSE","MATCHVIEW_COMPARISON_TXT_STAT_SHOOT_ONTARGET","MATCHVIEW_COMPARISON_TXT_STAT_PASS_TRY", "MATCHVIEW_COMPARISON_TXT_STAT_PASS_SUCCESS","MATCHVIEW_COMPARISON_TXT_STAT_STEAL","MATCHVIEW_COMPARISON_TXT_STAT_INTERCEPT","MATCHVIEW_COMPARISON_TXT_STAT_FOUL", "MATCHVIEW_COMPARISON_TXT_STAT_OFFSIDE", "MATCHVIEW_COMPARISON_TXT_CORNERKICK"};
    readonly int[] TEAM_STATISTICS_INDEX_LIST = new int[]{-1,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_GK_GOOD_DEFENCE,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT_ONTARGET,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSTRY,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSSUCCESS,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_STEAL,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_INTERCEPT,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_FOUL,-1,(int)E_COMMON_STATISTICS.COMMON_STATISTICS_CORNERKICK};

    EmblemBake[] m_pEmblemList = new EmblemBake[4];
    Color[] m_pEmblemColor = new Color[2];
    RecordT m_pRecord = null;
    
    Queue<BaseState> m_pNoticeStateList = new Queue<BaseState>();
    List<string> m_pBroadCastTextList = new List<string>();
    int m_iRankUpSFX = -1;
    bool m_bFullView = false;
    int m_iDribbleSFX = -1;
    int m_offset_H = 0;
    byte m_eChnageSubstitutionCount = 0;
    int m_iMatchType = 1;
    int m_iRankUp = 0;
    byte m_eCurrentRank = 1;
    bool m_bFinish = false;
    bool m_bSendClearData = false;
    bool m_bKickOff = false;
    Transform m_pBall = null;
    Transform m_pBallShadow = null;
    Transform m_pPauseText = null;
    Transform m_pLastBallChangePlayer = null;
    int m_iLastBallTeam = -1;
    Transform[] m_pHomePlayerList = new Transform[11];
    GameObject m_pLastSelect = null;
    Transform[] m_pAwayPlayerList = new Transform[11];

    int[,] m_pTeamDataLineUp = null;

    Transform[] m_pTabUIList = new Transform[(int)E_TAB.MAX];
    Transform[] m_pTabButtonList = new Transform[(int)E_TAB.MAX];
    TMPro.TMP_Text m_pBroadCastText = null;
    TMPro.TMP_Text m_pScoreText = null;
    TMPro.TMP_Text m_pTimerText = null;
    RectTransform m_pTop = null;
    RectTransform m_pViewNotice = null;
    RectTransform m_pViewSetting = null;
    RectTransform m_pFieldUI = null;
    RectTransform m_pResult = null;
    Animation m_pRankUp = null;
    Animation m_pViewRefresh = null;
    TMPro.TMP_Text m_pHomeText = null;
    TMPro.TMP_Text m_pAwayText = null;

    MatchTeamDataT m_pMatchTeamData = null;
    ALF.MatchEngine m_pMatchEngine = null;

    Button m_pTaticsButton = null;

    RectTransform m_pLineupUI = null;
    RectTransform m_pInfoUI = null;
    RectTransform m_pViewUI = null;    
    MainScene m_pMainScene = null;
    ScrollRect[] m_pScrollList = new ScrollRect[(int)E_SCROLL.MAX];
    BaseState m_pMatchUpdateState = null;
    E_TAB m_eCurrentTab = E_TAB.MAX;
    E_LOG_TAB m_eCurrentLogTab = E_LOG_TAB.Summary;
    Image m_pHomeBar = null;
    RectTransform m_pHomeBarRoot = null;
    TMPro.TMP_Text[] m_pPowerPercent = new TMPro.TMP_Text[2];
    string[] m_pPreScore = new string[2];
    bool m_bReverseMatch = false;
    int m_iMatchCount = 0;
    int m_iLoseCount = 0;
    List<E_AD> m_eAdTypeList = new List<E_AD>();
    
    float[] m_fPossessionTotals = new float[]{0,0};
    float[] m_SummaryHome = new float[]{0,0,0,0,0,0};
    float[] m_SummaryAway = new float[]{0,0,0,0,0,0};
    float m_fTotalGameTime = 0;

    public RectTransform MainUI { get; private set;}
    public MatchView(){}

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "MatchView : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "MatchView : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        GameContext pGameContext = GameContext.getCtx();

        m_pTeamDataLineUp = new int[2,GameContext.CONST_NUMPLAYER];

        m_pLineupUI = MainUI.Find("root/lineups").GetComponent<RectTransform>();
        m_pInfoUI = MainUI.Find("root/info").GetComponent<RectTransform>();
        m_pViewUI = MainUI.Find("root/ground").GetComponent<RectTransform>();
        m_pPauseText = m_pViewUI.Find("pause");
        m_pFieldUI = m_pViewUI.Find("feild").GetComponent<RectTransform>();
        int n =0;

        RectTransform ui = m_pFieldUI.Find("panel").GetComponent<RectTransform>();
        for(n =0; n < 11; ++n)
        {
            m_pHomePlayerList[n] = ui.Find($"h_{n}");
            m_pAwayPlayerList[n] = ui.Find($"a_{n}");
        }
        m_pBall = ui.Find("ball");
        m_pBallShadow = ui.Find("ballShadow");
        m_pResult = MainUI.Find("root/result").GetComponent<RectTransform>();
        m_pRankUp = MainUI.Find("root/rankUp").GetComponent<Animation>();
        
        ui = m_pInfoUI.Find("summary/summary/goals/home").GetComponent<RectTransform>();
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
        
        m_pViewNotice = m_pViewUI.Find("notice").GetComponent<RectTransform>();
        m_pViewSetting = m_pViewUI.Find("setting").GetComponent<RectTransform>();
        m_pViewRefresh = m_pViewUI.Find("refresh").GetComponent<Animation>();
#if FTM_LIVE
        m_pViewSetting.Find("x10").gameObject.SetActive(false);
#endif
    
        m_pTop = MainUI.Find("root/top").GetComponent<RectTransform>();

        m_pEmblemList[0] =  m_pTop.Find("view/homeEmblem").GetComponent<EmblemBake>();
        m_pEmblemList[1] =  m_pTop.Find("view/awayEmblem").GetComponent<EmblemBake>();
        m_pEmblemList[2] =  m_pLineupUI.Find("home/top/emblem").GetComponent<EmblemBake>();
        m_pEmblemList[3] =  m_pLineupUI.Find("away/top/emblem").GetComponent<EmblemBake>();

        m_pScoreText = m_pTop.Find("view/score/text").GetComponent<TMPro.TMP_Text>();
        m_pTimerText = m_pTop.Find("view/time/round").GetComponent<TMPro.TMP_Text>();
        m_pHomeText = m_pTop.Find("view/home").GetComponent<TMPro.TMP_Text>();
        m_pAwayText = m_pTop.Find("view/away").GetComponent<TMPro.TMP_Text>();

        LayoutManager.SetReciveUIButtonEvent(m_pInfoUI,this.ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(m_pViewUI,this.ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(m_pLineupUI,this.ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(m_pResult,this.ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(m_pRankUp.GetComponent<RectTransform>(),this.ButtonEventCall);
        
        m_pHomeBarRoot = MainUI.Find("root/bar").GetComponent<RectTransform>(); 
        m_pHomeBar = m_pHomeBarRoot.Find("view/home").GetComponent<Image>();
        m_pPowerPercent[0] = m_pHomeBarRoot.Find("view/home_percent").GetComponent<TMPro.TMP_Text>();
        m_pPowerPercent[1] = m_pHomeBarRoot.Find("view/away_percent").GetComponent<TMPro.TMP_Text>();
        m_pBroadCastText = m_pHomeBarRoot.Find("view/broadCast").GetComponent<TMPro.TMP_Text>();
        // m_pBroadCastText.outlineWidth = 0.2f;
        // m_pBroadCastText.outlineColor = new Color32(0,0,0,255);
        
        // m_pPowerPercent[0].outlineWidth = 0.2f;
        // m_pPowerPercent[1].outlineWidth = 0.2f;
        // m_pPowerPercent[0].outlineColor = m_pBroadCastText.outlineColor;
        // m_pPowerPercent[1].outlineColor = m_pBroadCastText.outlineColor;

        ui = m_pInfoUI.Find("command").GetComponent<RectTransform>();
        float w = ui.rect.width;

        RectTransform item = ui.Find("tatics").GetComponent<RectTransform>();
        m_pTaticsButton = item.GetComponent<Button>();

        float xx = item.rect.width + item.anchoredPosition.x;;
        w -= xx;
        item = ui.Find("finish").GetComponent<RectTransform>();
        w -= item.rect.width - item.anchoredPosition.x;

        Vector3 pos;
        
        w = w / (ui.childCount -2);
        float wh = w * 0.5f;
        float ax = ui.pivot.x * ui.rect.width;
        int iTabIndex = -1;
        
        for(n =1; n < ui.childCount -1; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;
            m_pTabUIList[iTabIndex] = iTabIndex < (int)E_TAB.homePlayer ? m_pInfoUI.Find(item.gameObject.name) : m_pInfoUI.Find("logs");
            m_pTabUIList[iTabIndex].gameObject.SetActive(false);

            pos = item.localPosition;
            size = item.sizeDelta;
            pos.x = xx + wh + ((n-1) * w) - ax;
            item.localPosition = pos;
        }

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
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>((uint)E_STATE_TYPE.ShowDailog);
        int i =0;
        for(i =0; i < list.Count; ++i)
        {
            list[i].SetExitCallback(null);
            list[i].Exit(true);
        }
        list.Clear();
        m_pBroadCastTextList.Clear();
        ClearScroll(E_SCROLL.timeline);
        ClearScroll(E_SCROLL.comparison);
        ClearScroll(E_SCROLL.log);
        
        if(m_pRecord != null)
        {
            m_pRecord.TimeRecord?.Clear();
            m_pRecord.LineUpRecord?.Clear();
            m_pRecord.StatisticsRecord = null;
            m_pRecord.TimeRecord = null;
            m_pRecord.LineUpRecord = null;
        }
        m_pRecord = null;
        m_pTeamDataLineUp = null;        
        
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

        while(m_pNoticeStateList.Count > 0)
        {
            StateMachine.GetStateMachine().RemoveState(m_pNoticeStateList.Dequeue());
        }

        m_pNoticeStateList.Clear();
        
        for( i =0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i] = null;
            m_pTabButtonList[i] = null;
        }

        for( i =0; i < m_pHomePlayerList.Length; ++i)
        {
            m_pHomePlayerList[i] = null;
            m_pAwayPlayerList[i] = null;
        }
        m_pTaticsButton = null;
        m_pNoticeStateList = null;        
        m_pLastBallChangePlayer = null;
        m_pMatchTeamData = null;
        m_pHomePlayerList = null;
        m_pLastSelect = null;
        m_pAwayPlayerList = null;
        m_pResult = null;
        m_pRankUp = null;
        m_pBall = null;
        m_pBallShadow = null;
        m_pBroadCastText = null;
        m_pFieldUI = null;
        m_pTabUIList = null;
        m_pTabButtonList  = null;
        m_pTop = null;
        m_pLineupUI = null;
        m_pInfoUI = null;
        m_pViewUI = null;
        m_pScoreText = null;
        m_pTimerText = null;
        m_pViewNotice = null;
        m_pViewSetting = null;
        m_pViewRefresh = null;
        m_pHomeText = null;
        m_pAwayText = null;
        m_pScrollList = null;
        m_pMainScene = null;
        MainUI = null;
        m_pHomeBarRoot = null;
        m_pHomeBar = null;

        m_pMatchEngine?.Dispose();
        m_pMatchEngine = null;
        
        if(m_pMatchUpdateState != null)
        {
            StateMachine.GetStateMachine().RemoveState(m_pMatchUpdateState);
        }
        m_pMatchUpdateState = null;
    }

    public byte GetSubstitutionCunt()
    {
        return m_eChnageSubstitutionCount;
    }

    // public bool IsSubstitution() 
    // {
    //     return m_eChnageSubstitutionCount < 3;
    // }

    // public void ChagneSubstitutionList(ulong[] _inList,ulong[] _outList)
    // {
    //     byte count = 0;
    //     for(int i =0; i < _inList.Length; ++i)
    //     {
    //         if(_inList[i] > 0)
    //         {
    //             ++count;
    //         }
    //     }

    //     if(m_eChnageSubstitutionCount + count < 4)
    //     {

    //     }
    // }

    void EnableFinishButton(bool bEnable)
    {
        m_pInfoUI.Find("command/finish/finish").GetComponent<Button>().enabled = bEnable;
        m_pInfoUI.Find("command/finish/finish/on").gameObject.SetActive(bEnable);
        m_pInfoUI.Find("command/finish/finish/off").gameObject.SetActive(!bEnable);
        m_pInfoUI.Find("command/finish/finish/count").gameObject.SetActive(bEnable);

        m_pInfoUI.Find("command/finish/finish/on").GetComponent<Graphic>().color = bEnable ? Color.white:GameContext.GRAY;
    }

    void ResetTopUI()
    {
        m_fTotalGameTime = 0;
        m_pScoreText.SetText("0:0");
        m_pTimerText.SetText(GameContext.getCtx().GetLocalizingText("MATCHVIEW_TXT_1ST_HALF"));
    }

    public RectTransform GetHelpTarget(byte eStep)
    {
        if(eStep == 1)
        {
            return m_pViewSetting;
        }
        
        if(eStep == 2)
        {
            return m_pTaticsButton.GetComponent<RectTransform>();
        }

        if(eStep == 3)
        {
            return m_pTaticsButton.transform.parent.Find("awayPlayer").GetComponent<RectTransform>();
        }

        if(eStep == 4)
        {
            return m_pTaticsButton.transform.parent.Find("finish").GetComponent<RectTransform>();
        }

        return null;
    }

    void SetupClubName(string home,string away)
    {
        m_pHomeText.SetText(home);
        m_pAwayText.SetText(away);
    }
    void UpdateViewMode()
    {
        m_pViewSetting.Find("view/1").gameObject.SetActive(false);
        m_pViewSetting.Find("view/2").gameObject.SetActive(false);
        m_pViewSetting.Find("view/3").gameObject.SetActive(false);
        byte eViewMode = GameContext.getCtx().MatchViewMode();
        if(eViewMode == 1)
        {
            m_pViewSetting.Find("view/1").gameObject.SetActive(true);
            for(int i =0; i < m_pHomePlayerList.Length; ++i)
            {
                m_pHomePlayerList[i].Find("name").gameObject.SetActive(false);
                m_pAwayPlayerList[i].Find("name").gameObject.SetActive(false);
            }
        }
        else if(eViewMode == 2)
        {
            m_pViewSetting.Find("view/2").gameObject.SetActive(true);
            for(int i =0; i < m_pHomePlayerList.Length; ++i)
            {
                m_pHomePlayerList[i].Find("name").gameObject.SetActive(true);
                m_pAwayPlayerList[i].Find("name").gameObject.SetActive(false);
            }
        }
        else
        {
            m_pViewSetting.Find("view/3").gameObject.SetActive(true);
            for(int i =0; i < m_pHomePlayerList.Length; ++i)
            {
                m_pHomePlayerList[i].Find("name").gameObject.SetActive(true);
                m_pAwayPlayerList[i].Find("name").gameObject.SetActive(true);
            }
        }
    }

    void UpdateViewSpeed(byte eViewSpeed)
    {
        for(int i =1; i < 4; ++i)
        {
            m_pViewSetting.Find($"speed/{i}").gameObject.SetActive(eViewSpeed == i);
        }
    }

    void UpdateFullView()
    {
        m_pViewSetting.Find("full/on").gameObject.SetActive(m_bFullView);
        m_pViewSetting.Find("full/off").gameObject.SetActive(!m_bFullView); 
        m_pViewSetting.Find("help").gameObject.SetActive(!m_bFullView); 
        m_pInfoUI.gameObject.SetActive(!m_bFullView);

        Vector2 offset = Vector2.one;
        float H = 0;
        if(m_bFullView)
        {
            m_pViewUI.Rotate(new Vector3(0,0,-90));
            offset.x = 0.5f;
            offset.y = 0.5f;

            m_pViewUI.anchorMin = offset;
            m_pViewUI.anchorMax = offset;
            offset.x = 0;
            offset.y = 0;
            m_pViewUI.anchoredPosition = Vector2.zero;

            offset.x = MainUI.rect.height;
            offset.y = MainUI.rect.width - 85;
            m_pViewUI.sizeDelta = offset;

            offset.x = 1;
            offset.y = 0;
            m_pTop.anchorMin = offset;
            offset.y = 0f;
            m_pTop.pivot = offset;

            offset.x = 124;
            H = (m_pViewUI.sizeDelta.y -offset.x) / m_pFieldUI.rect.height;
            offset.y = 0;
            m_pTop.sizeDelta = offset;
            
            RectTransform view = m_pTop.Find("bar").GetComponent<RectTransform>();
            view.Rotate(new Vector3(0,0,-90));
            offset = view.sizeDelta;
            offset.x = m_pTop.rect.height;
            view.sizeDelta = offset;

            view = m_pTop.Find("view").GetComponent<RectTransform>();
            view.Rotate(new Vector3(0,0,-90));

            offset = view.anchoredPosition;
            offset.y = m_pTop.rect.height * 0.5f;
            view.anchoredPosition = offset;

            m_pHomeBarRoot.anchorMin = Vector2.zero;
            offset.x =0;
            offset.y =1;
            m_pHomeBarRoot.anchorMax = offset;   

            offset.x =0f;
            offset.y = 1;
            m_pHomeBarRoot.pivot = offset;
            view = m_pHomeBarRoot.Find("view").GetComponent<RectTransform>();
            view.Rotate(new Vector3(0,0,-90));
            offset.x = 85;
            offset.y = 0;
            m_pHomeBarRoot.sizeDelta = offset;
            m_pHomeBarRoot.anchoredPosition = Vector2.zero;
            offset.x = m_pHomeBarRoot.rect.height -40;
            offset.y = m_pHomeBarRoot.rect.width;
            view.sizeDelta = offset;
        }
        else
        {
            m_pViewUI.rotation = new Quaternion(0,0,0,1);

            offset.x = 0.5f;
            offset.y = 1;

            m_pViewUI.anchorMin = offset;
            m_pViewUI.anchorMax = offset;
            offset.x = 0;
            offset.y = -487- m_offset_H;
            m_pViewUI.anchoredPosition = offset;
            
            offset.x = 1060;
            offset.y = 706;
            m_pViewUI.sizeDelta = offset;

            offset.x = 0;
            offset.y = 1;
            m_pTop.anchorMin = offset;
            offset.x = 0.5f;
            offset.y = 1;
            m_pTop.pivot = offset;

            offset.x = 0;
            offset.y = 124 + m_offset_H;
            H = m_pViewUI.sizeDelta.y / m_pFieldUI.rect.height;
            m_pTop.sizeDelta = offset;

            RectTransform view = m_pTop.Find("bar").GetComponent<RectTransform>();
            
            offset = view.sizeDelta;
            offset.x = m_pTop.rect.width;
            view.sizeDelta = offset;
            
            view.rotation = new Quaternion(0,0,0,1);
            
            view = m_pTop.Find("view").GetComponent<RectTransform>();
            view.rotation = new Quaternion(0,0,0,1);

            offset = view.anchoredPosition;
            offset.y = 50;
            view.anchoredPosition = offset;

            offset.x =0;
            offset.y =1;
            m_pHomeBarRoot.anchorMin = offset;
            m_pHomeBarRoot.anchorMax = Vector2.one;

            offset.x =0.5f;
            offset.y = 1;
            m_pHomeBarRoot.pivot = offset;
            view = m_pHomeBarRoot.Find("view").GetComponent<RectTransform>();
            view.rotation = new Quaternion(0,0,0,1);

            offset.x = 0;
            offset.y = 85;
            m_pHomeBarRoot.sizeDelta = offset;
            offset.y = -837 - m_offset_H;
            m_pHomeBarRoot.anchoredPosition = offset;
            offset.x = m_pHomeBarRoot.rect.width -40;
            offset.y = m_pHomeBarRoot.rect.height;
            view.sizeDelta = offset;
        }

        m_pTop.anchoredPosition = Vector2.zero;

        Vector3 scale = m_pFieldUI.localScale;
        scale.x = H;
        scale.y = scale.x;
        m_pFieldUI.localScale = scale;
    }
    
    void ResetGround()
    {
        m_pLastBallChangePlayer = null;
        m_iLastBallTeam = -1;
        m_pLastSelect = null;
        m_fTotalGameTime = 0;
        m_bFullView = false;
        m_pBroadCastTextList.Clear();
        m_pBroadCastText.gameObject.SetActive(false);
        m_pPauseText.gameObject.SetActive(false);
        m_pViewSetting.gameObject.SetActive(false);
        m_pResult.gameObject.SetActive(false);
        m_pRankUp.gameObject.SetActive(false);
        
        m_pTaticsButton.enabled = true;
        m_pTaticsButton.transform.Find("on").gameObject.SetActive(true);
        m_pTaticsButton.transform.Find("off").gameObject.SetActive(false);
        UpdateViewSpeed(GameContext.getCtx().MatchViewSpeed());
        UpdateViewMode();
        UpdateFullView();

        ALFUtils.FadeObject(m_pViewSetting,-1);
        
        int i =0;
        for(i =0; i < m_SummaryHome.Length; ++i)
        {
            m_SummaryHome[i] =0;
            m_SummaryAway[i] = 0;
        }

        for(i =0; i < m_pViewNotice.childCount; ++i)
        {
            LayoutManager.Instance.AddItem(NOTICE_ITEM_NAME,m_pViewNotice.GetChild(i).GetComponent<RectTransform>());
        }

        UpdateSummaryData();
        m_fPossessionTotals[0] =0;
        m_fPossessionTotals[1] =0;
        PossessionTotal(null);

        m_pBall.localPosition = Vector3.zero;
        m_pBallShadow.localPosition = Vector3.zero;

        for(i =0; i< m_pHomePlayerList.Length; ++i)
        {
            m_pHomePlayerList[i].localPosition = Vector3.zero;
            m_pHomePlayerList[i].GetComponent<Graphic>().color = i == 0 ? Color.yellow : m_pEmblemColor[0];
            
            m_pHomePlayerList[i].Find("sel").gameObject.SetActive(false);
            m_pAwayPlayerList[i].Find("sel").gameObject.SetActive(false);

            m_pAwayPlayerList[i].localPosition = Vector3.zero;
            m_pAwayPlayerList[i].GetComponent<Graphic>().color = i == 0 ? Color.green : m_pEmblemColor[1];
        }

        // m_pPowerPercent[0].outlineColor = GameContext.GRAY_W;
        // m_pPowerPercent[0].color = GameContext.GRAY;
        // m_pPowerPercent[1].outlineColor = GameContext.GRAY_W;
        // m_pPowerPercent[1].color = GameContext.GRAY;
        // m_pBroadCastText.outlineColor = GameContext.GRAY_W;
        // m_pBroadCastText.color = GameContext.GRAY;

        for(i =0; i< m_pPowerPercent.Length; ++i)
        {
            m_pPowerPercent[i].SetText("");
        }
    }

    bool executeMatchUpdateCallback(IState state,float dt,bool bEnd)
    {
        if(m_pMatchEngine != null)
        {
            float t = dt * (float)GameContext.getCtx().MatchViewSpeed();
            m_pMatchEngine.Update(t);
                        
            if(m_bKickOff && !m_bFinish)
            {
                UpdateMatchStatisticsData();
            }

            if(m_pMatchUpdateState == null) // game over
            {
                m_pTaticsButton.enabled = false;
                m_pTaticsButton.transform.Find("on").gameObject.SetActive(false);
                m_pTaticsButton.transform.Find("off").gameObject.SetActive(true);
                EnableFinishButton(false);
                UpdateMatchStatisticsData();
                PossessionTotal(null);
                UpdateSummaryData();
            }
        }

        return bEnd;
    }

    void ShowTabUI(E_TAB eTab)
    {
        if(m_eCurrentTab == eTab) return;

        m_eCurrentTab = eTab;
        int i = 0;
        int index = (int)eTab;
        for(i = 0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i].gameObject.SetActive(i == index);
            m_pTabButtonList[i].Find("off").gameObject.SetActive(!(i == index));
            m_pTabButtonList[i].Find("on").gameObject.SetActive(i == index);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = i == index ? Color.white : GameContext.GRAY;
        }
        
        if(m_eCurrentTab > E_TAB.summary)
        {
            if(m_eCurrentTab == E_TAB.homePlayer)
            {
                m_pTabUIList[2].gameObject.SetActive(true);
                SetupScroll(E_SCROLL.log,m_pRecord.StatisticsRecord.StatisticsPlayers[0].Players.Count);
            }
            else if(m_eCurrentTab == E_TAB.awayPlayer)
            {
                m_pTabUIList[2].gameObject.SetActive(true);
                SetupScroll(E_SCROLL.log,m_pRecord.StatisticsRecord.StatisticsPlayers[1].Players.Count);
            }
            
            ShowSubTap(E_LOG_TAB.Summary);
            UpdateMatchStatisticsData();
        }
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

            text = m_pInfoUI.Find("logs/title").GetComponent<TMPro.TMP_Text>();
            text.SetText(GameContext.getCtx().GetLocalizingText( bHome ? "MATCHVIEW_PLAYER_TXT_HOME_TEAM_PLAYER":"MATCHVIEW_PLAYER_TXT_AWAY_TEAM_PLAYER"));

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

    public void TutorailStep(byte eStep)
    {
        if(eStep == 35)
        {
            PlayLineupAnimation(false);
        }
        else if(eStep == 36)
        {
            ButtonEventCall(m_pLineupUI,m_pLineupUI.Find("play").gameObject);
            GameContext.getCtx().SendAdjustEvent("xu45nt",false,false,-1);
        }
    }

    void StartMatch()
    {
        m_pLineupUI.gameObject.SetActive(false);
        m_pTop.gameObject.SetActive(true);
        m_pInfoUI.gameObject.SetActive(true);
        m_pViewUI.gameObject.SetActive(true);
        m_pHomeBarRoot.gameObject.SetActive(true);
        ShowTabUI(E_TAB.summary);
        m_pMatchEngine.StartMatch();
        m_pMatchUpdateState = BaseState.GetInstance(new BaseStateTarget(this.MainUI),-1, (uint)E_STATE_TYPE.MatchUpdate, null,this.executeMatchUpdateCallback, null,-1);
        StateMachine.GetStateMachine().AddState(m_pMatchUpdateState);
        // if(GameContext.getCtx().GetTutorial() == 32)
        // {
        //     m_pMainScene.ShowMatchHelpPopup();
        // }
    }
    public bool IsPauseMatchButton()
    {
        return GameContext.getCtx().MatchViewSpeed() == 3;
    }
    public void ResumeMatch()
    {
        if(m_pMatchUpdateState != null)
        {
            if(!IsPauseMatchButton())
            {
                m_pMatchUpdateState.Paused = false;
            }
        }
    }

    public void SetViewSpeed(byte step,bool bSave)
    {
        byte eViewSpeed = GameContext.getCtx().MatchViewSpeed();
        if(eViewSpeed == step) return;
        ResetTimer(true);
        eViewSpeed = step;
        #if FTM_LIVE
        if(eViewSpeed > 3)
        #else
        if(eViewSpeed == 4 || eViewSpeed > 10)
        #endif
        {
            eViewSpeed =1;
        }
        if(bSave)
        {
            GameContext.getCtx().SetMatchViewSpeed(eViewSpeed);
        }
        
        UpdateViewSpeed(eViewSpeed);
        
        if(m_pMatchUpdateState != null)
        {            
            if(eViewSpeed == 3)
            {
                m_pMatchUpdateState.Paused = true;
                m_pPauseText.gameObject.SetActive(true);
            }
            else
            {
                List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pBroadCastText,(uint)E_STATE_TYPE.Timer);
                TimeOutCondition condition = null;
                for(int i =0; i < list.Count; ++i)
                {
                    condition = list[i].GetCondition<TimeOutCondition>();
                    condition.UpdateScale = (float)eViewSpeed;
                }

                list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>((uint)E_STATE_TYPE.ShowNoticePopup);
                for(int i =0; i < list.Count; ++i)
                {
                    condition = list[i].GetCondition<TimeOutCondition>();
                    condition.UpdateScale = (float)eViewSpeed;
                }

                m_pMatchUpdateState.Paused = false;
                m_pPauseText.gameObject.SetActive(false);
            }
        }
    }

    public void PauseMatch()
    {
        if(m_pMatchUpdateState != null)
        {
            m_pMatchUpdateState.Paused = true;
        }
    }
    public bool IsFinishMatchGame()
    {
        return m_pMatchUpdateState == null;
    }

    public void AddMatchCount()
    {
        ++m_iMatchCount;
    }

    public void ResetMatchCount()
    {
        m_iMatchCount = 0;
    }

    public bool IsCurrentActivePlayer(ulong id)
    {
        int index = 0;
        for(int i =0; i < GameContext.CONST_NUMSTARTING; ++i)
        {
            index = m_pMatchTeamData.TeamData[0].LineUp[i];
            if(m_pMatchTeamData.TeamData[0].PlayerData[index].PlayerId == id)
            {
                return true;
            }
        }
        
        return false;
    }

    public void TransformSetPosition(int iTeamIndex, int thisHandle, Vector3 position)
    {
        Transform tm = null;
        if( iTeamIndex == -1 && thisHandle == -1)
        {
            tm = m_pBall;
        }
        else if( iTeamIndex == -2 && thisHandle == -2)
        {
            tm = m_pBallShadow;
        }
        else if( iTeamIndex == 0 )
        {
            tm = m_pHomePlayerList[thisHandle];
        }
        else if( iTeamIndex == 1 )
        {
            tm = m_pAwayPlayerList[thisHandle];
        }

        Vector3 p = position;
        p.z = 0;
        tm.localPosition = p;

        if( iTeamIndex > -1) return;
        
        p = Vector3.one;
        
        if( thisHandle == -1)
        {
            p *= Mathf.Min(3,1 +(position.z  * 0.05f));
        }
        else
        {
            p *= Mathf.Max(0.4f,1 +(position.z  * -0.05f));
        }
        
        tm.localScale = p;   
    }

    public bool IsCurrentActivePlayerByIndex(int index, ulong id)
    {
        if(GameContext.CONST_NUMSTARTING <= index) return false;

        index = m_pMatchTeamData.TeamData[0].LineUp[index];
        return m_pMatchTeamData.TeamData[0].PlayerData[index].PlayerId == id;
    }

    public void SetupLineup( MatchTeamDataT pMatchTeamData ,EmblemBake[] pEmblemBakeList,int iMatchType)
    {
        GameContext pGameContext = GameContext.getCtx();
        byte eViewSpeed = pGameContext.MatchViewSpeed();
        if(eViewSpeed == 3)
        {
            eViewSpeed =1;
            pGameContext.SetMatchViewSpeed(eViewSpeed);
        }

        if(m_pMatchUpdateState != null)
        {
            m_pMatchUpdateState.Exit(true);
            m_pMatchUpdateState = null;
        }

        m_pPreScore[0]="0";
        m_pPreScore[1]="0";
        m_bReverseMatch = false;
        m_eAdTypeList.Clear();
        m_iMatchType = iMatchType;
        m_eCurrentRank = m_iMatchType == GameContext.LADDER_ID ? pGameContext.GetCurrentUserRank() : (byte)0;
        
        m_pLineupUI.Find("play").gameObject.SetActive(false);
        m_pLineupUI.Find("next").gameObject.SetActive(false);
    
        m_pRecord = new RecordT();
        m_pRecord.TimeRecord = new List<TimeLineT>();
        m_pRecord.LineUpRecord = new List<LineUpT>();
        m_pRecord.HomeId = pGameContext.GetClubID();

        m_pRecord.HomeName = pGameContext.GetClubName();
        m_pRecord.AwayName = pMatchTeamData.TeamData[1].TeamName;
        m_pRecord.Id = pGameContext.GetCurrentMatchID();
        m_pRecord.AwayId = pMatchTeamData.TeamData[1].TeamId;

        m_iRankUp = 0;
        m_bSendClearData = false;
        m_pEmblemList[0].CopyPoint( pEmblemBakeList[0]);
        m_pEmblemList[1].CopyPoint( pEmblemBakeList[1]);
        m_pEmblemList[2].CopyPoint( pEmblemBakeList[0]);
        m_pEmblemList[3].CopyPoint( pEmblemBakeList[1]);

        m_pEmblemColor[0] = m_pEmblemList[0].material.GetColor("_Pattern1Color");
        m_pEmblemColor[1] = m_pEmblemList[1].material.GetColor("_Pattern1Color");
        bool bSame = Vector3.Distance(new Vector3((int)(m_pEmblemColor[0].r * 255),(int)(m_pEmblemColor[0].g * 255),(int)(m_pEmblemColor[0].b * 255)),new Vector3((int)(m_pEmblemColor[1].r * 255),(int)(m_pEmblemColor[1].g * 255),(int)(m_pEmblemColor[1].b * 255))) < 15;
        if(bSame)
        {
            m_pEmblemColor[0].r += 0.15f;
            m_pEmblemColor[0].g += 0.15f;
            m_pEmblemColor[0].b += 0.15f;
            m_pEmblemColor[1].r -= 0.15f;
            m_pEmblemColor[1].g -= 0.15f;
            m_pEmblemColor[1].b -= 0.15f;
        }

        m_pHomeBar.color = m_pEmblemColor[0];
        m_pHomeBar.transform.parent.Find("away").GetComponent<Graphic>().color = m_pEmblemColor[1];

        MainUI.Find("root/top/bar/homeBar").GetComponent<Graphic>().color = m_pEmblemColor[0];
        MainUI.Find("root/top/bar/awayBar").GetComponent<Graphic>().color = m_pEmblemColor[1];

        m_pInfoUI.Find("summary/summary/goals/home/fill").GetComponent<Graphic>().color = m_pEmblemColor[0];
        m_pInfoUI.Find("summary/summary/goals/away/fill").GetComponent<Graphic>().color = m_pEmblemColor[1];

        m_pInfoUI.Find("summary/summary/shots/home/fill").GetComponent<Graphic>().color = m_pEmblemColor[0];
        m_pInfoUI.Find("summary/summary/shots/away/fill").GetComponent<Graphic>().color = m_pEmblemColor[1];

        m_pInfoUI.Find("summary/summary/possesions/home/fill").GetComponent<Graphic>().color = m_pEmblemColor[0];
        m_pInfoUI.Find("summary/summary/possesions/away/fill").GetComponent<Graphic>().color = m_pEmblemColor[1];

        m_pInfoUI.Find("summary/summary/teamRating/home/fill").GetComponent<Graphic>().color = m_pEmblemColor[0];
        m_pInfoUI.Find("summary/summary/teamRating/away/fill").GetComponent<Graphic>().color = m_pEmblemColor[1];

        m_bFinish = false;
        m_bKickOff = false;
        m_eChnageSubstitutionCount = 0;
        m_pMatchTeamData = pMatchTeamData;
        
        ClearScroll(E_SCROLL.timeline);

        m_pLineupUI.gameObject.SetActive(true);
        m_pTop.gameObject.SetActive(false);
        m_pViewUI.gameObject.SetActive(false);

        UpdateLineupUI();

        int i =0;
        int n = 0;
        
        LineUpT pLineUp = null;

        for(i =0; i < m_pMatchTeamData.TeamData.Count; ++i)
        {
            pLineUp = new LineUpT();
            pLineUp.List = new List<int>();
            pLineUp.Formation = new List<byte>();
            for(n = 0; n < m_pMatchTeamData.TeamData[i].LineUp.Count; ++n)
            {
                m_pTeamDataLineUp[i,n] = m_pMatchTeamData.TeamData[i].LineUp[n];
                pLineUp.List.Add(m_pMatchTeamData.TeamData[i].LineUp[n]);
            }
            for(n = 0; n < m_pMatchTeamData.TeamData[i].Tactics.Formation.Count; ++n)
            {
                pLineUp.Formation.Add(m_pMatchTeamData.TeamData[i].Tactics.Formation[n]);
            }
            m_pRecord.LineUpRecord.Add(pLineUp);
        }

        ScrollRect pScroll = m_pScrollList[(int)E_TAB.comparison];
        Transform pItem = null;
        for(i =0; i < pScroll.content.childCount; ++i)
        {
            pItem = pScroll.content.GetChild(i);
            if(pItem)
            {
                pItem.Find("gauge/home").GetComponent<Graphic>().color = m_pEmblemColor[0];
                pItem.Find("gauge/away").GetComponent<Graphic>().color = m_pEmblemColor[1];
            }
        }
        
        Button pButton =m_pInfoUI.Find("command/finish/exit").GetComponent<Button>();
        pButton.enabled = true;
        pButton.gameObject.SetActive(false);
        pButton =m_pInfoUI.Find("command/finish/finish").GetComponent<Button>();
        pButton.gameObject.SetActive(true);
        pButton.enabled = true;
        UpdateMatchSkipItem(false);

        ResetTopUI();
        ResetGround();
        m_pInfoUI.gameObject.SetActive(false);
        m_pHomeBarRoot.gameObject.SetActive(false);
        m_pViewRefresh.gameObject.SetActive(false);

        SetupClubName(pMatchTeamData.TeamData[0].TeamName,pMatchTeamData.TeamData[1].TeamName);
        
        m_pMatchEngine?.Dispose();
        m_pMatchEngine = null;
                
        m_pMatchEngine = new ALF.MatchEngine((int)m_pFieldUI.rect.width,(int)m_pFieldUI.rect.height,27,24,708,461);
        #if UNITY_EDITOR
        // m_pMatchEngine.SetDebugMode(Application.platform != RuntimePlatform.WindowsEditor);
        m_pMatchEngine.SetDebugMode(false);
        #endif
        
        MatchCommentaryList pMatchCommentaryList = pGameContext.GetMatchCommentaryData();
        MatchCommentaryItem? pMatchCommentaryItem = null;
        CommentaryItem? pCommentaryItem = null;
        E_BROADCAST_TEXT_EVENT_TYPE eEvent = E_BROADCAST_TEXT_EVENT_TYPE.NONE;
        for(i =0; i < pMatchCommentaryList.MatchCommentaryLength; ++i)
        {
            pMatchCommentaryItem = pMatchCommentaryList.MatchCommentary(i);
            if(pMatchCommentaryItem != null)
            {
                for(n =0; n < pMatchCommentaryItem.Value.ListLength; ++n)
                {
                    pCommentaryItem = pMatchCommentaryItem.Value.List(n);
                    if(pCommentaryItem != null && pCommentaryItem.Value.IsUsable)
                    {
                        eEvent =(E_BROADCAST_TEXT_EVENT_TYPE)Enum.Parse(typeof(E_BROADCAST_TEXT_EVENT_TYPE), pMatchCommentaryItem.Value.CommentaryType);
                        m_pMatchEngine.AddBroadCastTextData(eEvent,pCommentaryItem.Value.No, pCommentaryItem.Value.Token);
                    }
                }
            }
        }
        
        m_pMatchEngine.SetMatchTeamData(m_pMatchTeamData.SerializeToBinary());
        
    }

    void UpdateMatchSkipItem(bool bSkip)
    {
        ulong itemCount = GameContext.getCtx().GetItemCountByNO(GameContext.MATCH_SKIP_ID);
        TMPro.TMP_Text text = m_pInfoUI.Find("command/finish/finish/count").GetComponent<TMPro.TMP_Text>();
        text.SetText(itemCount > 0 ? itemCount.ToString() : "");
        if(!bSkip)
        {
            EnableFinishButton(true);
        }
    }
    public void HalfTime()
    {
        m_pTimerText.SetText(GameContext.getCtx().GetLocalizingText("MATCHVIEW_TXT_2ND_HALF"));
        m_bKickOff = false;
    }
    public void KickOff()
    {
        if(!m_bFinish)
        {
            SoundManager.Instance.PlaySFX("sfx_match_whistle_start");
        }        
        m_bKickOff = true;
    }

    public void PlaySFX(string file)
    {
        if(!m_bFinish)
        {
            SoundManager.Instance.PlaySFX(file);
        }
    }

    public void PlayDribbleSFX(bool bPlay)
    {
        if(!m_bFinish)
        {
            if(bPlay)
            {
                if(!SoundManager.Instance.IsPlaySFX(m_iDribbleSFX))
                {
                    m_iDribbleSFX = SoundManager.Instance.PlaySFX("sfx_match_ball_dribble",0,true);
                }
            }
            else
            {
                SoundManager.Instance.StopSFX(m_iDribbleSFX);
            }
        }
    }

    public void SetScore(string score)
    {
        if(string.IsNullOrEmpty(score)) return;

        if(m_iMatchType == GameContext.LADDER_ID)
        {
            string[] token = score.Split(':');
        
            if(string.Compare(token[0] ,token[1]) == 0)
            {
                m_bReverseMatch = (string.Compare(m_pPreScore[0] ,m_pPreScore[1]) == -1);
            }
            else
            {
                if(string.Compare(token[0] ,token[1]) == -1)
                {
                    m_bReverseMatch = false;
                }

                m_pPreScore[0]=token[0];
                m_pPreScore[1]=token[1];
            }
        }

        m_pScoreText.SetText(score);
        m_pRecord.Score = score;
    }
    public void SetPlayTime(string time)
    {
        if(m_bKickOff)
        {
            float fT = float.Parse(time.Split(':')[0]);
            
            if(fT < 80)
            {
                if(fT >= 80 && m_fTotalGameTime != 80)
                {
                    m_pInfoUI.Find("command/finish/exit").gameObject.SetActive(true);
                    m_pInfoUI.Find("command/finish/finish").gameObject.SetActive(false);
                    m_fTotalGameTime = 80;
                }
                else
                {
                    m_fTotalGameTime = fT;
                }
            }
            m_pTimerText.SetText(time);
        }
    }

    public void PlayLineupAnimation(bool bHome)
    {
        Animation pAnimation = m_pLineupUI.Find("feild").GetComponent<Animation>();
        pAnimation.gameObject.SetActive(true);
        float fDuration = 0;
        if(bHome)
        {
            pAnimation = m_pLineupUI.Find("home").GetComponent<Animation>();
            
            m_pLineupUI.Find("play").gameObject.SetActive(false);
            m_pLineupUI.Find("next").gameObject.SetActive(false);
            fDuration = pAnimation["feildShow"].length;
        }
        else
        {
            fDuration = pAnimation["feild"].length;
            m_pLineupUI.Find("home").gameObject.SetActive(false);
        }

        pAnimation.gameObject.SetActive(true);
        pAnimation.Play();

        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pAnimation),fDuration, (uint)E_STATE_TYPE.ShowDailog, null, null,this.exitLineupDilogCallback);
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    void UpdateLineupUI()
    {
        Transform tm = m_pLineupUI.Find("home");
        tm.gameObject.SetActive(false);
        RectTransform home = tm.Find("root").GetComponent<RectTransform>();
        home.offsetMin = Vector2.zero;
        home.offsetMax = Vector2.zero;

        TMPro.TMP_Text text = tm.Find("top/name").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_pMatchTeamData.TeamData[0].TeamName);

        tm = m_pLineupUI.Find("away");
        tm.gameObject.SetActive(false);
        RectTransform away = tm.Find("root").GetComponent<RectTransform>();
        away.offsetMin = Vector2.zero;
        away.offsetMax = Vector2.zero;

        text = tm.Find("top/name").GetComponent<TMPro.TMP_Text>();
        text.SetText(m_pMatchTeamData.TeamData[1].TeamName);

        PlayerT pPlayer = null;
        int i =0;
        int index = 0;
        string formation = null;
        Vector3 pos;
        Color color = Color.white; 
        bool bSame = Vector3.Distance(new Vector3((int)(m_pEmblemColor[0].r * 255),(int)(m_pEmblemColor[0].g * 255),(int)(m_pEmblemColor[0].b * 255)),new Vector3(255,255,255)) < 15;
        bool bSame1 = Vector3.Distance(new Vector3((int)(m_pEmblemColor[1].r * 255),(int)(m_pEmblemColor[1].g * 255),(int)(m_pEmblemColor[1].b * 255)),new Vector3(255,255,255)) < 15;
        
        for(i =0; i < m_pMatchTeamData.TeamData[0].Tactics.Formation.Count; ++i)
        {
            formation = ((E_LOCATION)m_pMatchTeamData.TeamData[0].Tactics.Formation[i]).ToString();
            tm = home.Find(formation);
            pos = tm.localPosition;
            tm = home.Find($"player_{i}");
            tm.localPosition = pos;

            tm.Find("name").GetComponent<Graphic>().color = m_pEmblemColor[0];
            text = tm.Find("name/text").GetComponent<TMPro.TMP_Text>();
            text.color = bSame ? GameContext.GRAY : Color.white;
            index = m_pMatchTeamData.TeamData[0].LineUp[i];
            formation = m_pMatchTeamData.TeamData[0].PlayerData[index].PlayerName;
            text.SetText(formation);
            text = m_pHomePlayerList[i].Find("name/text").GetComponent<TMPro.TMP_Text>();
            text.SetText(formation);

            pPlayer = GetPlayerByID(E_PLAYER_INFO_TYPE.my, m_pMatchTeamData.TeamData[0].PlayerData[index].PlayerId);

            SingleFunc.SetupPlayerCard( pPlayer,tm,E_ALIGN.Center);

            formation = ((E_LOCATION)m_pMatchTeamData.TeamData[1].Tactics.Formation[i]).ToString();
            tm = away.Find(formation);
            pos = tm.localPosition;
            tm = away.Find($"player_{i}");
            tm.localPosition = pos;
            
            tm.Find("name").GetComponent<Graphic>().color = m_pEmblemColor[1];
            text = tm.Find("name/text").GetComponent<TMPro.TMP_Text>();
            text.color = bSame1 ? GameContext.GRAY : Color.white;
            index = m_pMatchTeamData.TeamData[1].LineUp[i];
            formation = m_pMatchTeamData.TeamData[1].PlayerData[index].PlayerName;
            text.SetText(formation);
            text = m_pAwayPlayerList[i].Find("name/text").GetComponent<TMPro.TMP_Text>();
            text.SetText(formation);

            pPlayer = GetPlayerByID(E_PLAYER_INFO_TYPE.match, m_pMatchTeamData.TeamData[1].PlayerData[index].PlayerId);
            SingleFunc.SetupPlayerCard( pPlayer,tm,E_ALIGN.Center);
        }
    }

    public void BallChangePlayer(string strData)
    {
        if(m_bFinish ) return;

        JObject pData = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(strData, new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
        
        if(m_pLastBallChangePlayer != null)
        {
            byte eViewMode = GameContext.getCtx().MatchViewMode();
            if( eViewMode == 1)
            {
                m_pLastBallChangePlayer.gameObject.SetActive(false);
            }
            else if(m_iLastBallTeam == 1 && eViewMode == 2)
            {
                m_pLastBallChangePlayer.gameObject.SetActive(false);
            }
        }

        if(m_pLastSelect != null)
        {
            m_pLastSelect.SetActive(false);
        }

        m_iLastBallTeam = (int)pData["team_index"];
        int iPlayerIndex = (int)pData["player_index"];//{"team_index":1, "player_index":9 }        

        if(m_iLastBallTeam == 0)
        {
            m_pLastSelect = m_pHomePlayerList[iPlayerIndex].Find("sel").gameObject;
            m_pLastBallChangePlayer = m_pHomePlayerList[iPlayerIndex].Find("name");
        }
        else if(m_iLastBallTeam == 1)
        {
            m_pLastSelect = m_pAwayPlayerList[iPlayerIndex].Find("sel").gameObject;
            m_pLastBallChangePlayer = m_pAwayPlayerList[iPlayerIndex].Find("name");
        }
        else
        {
            m_pLastSelect = null;
            m_pLastBallChangePlayer = null;
            m_iLastBallTeam = -1;
            return;
        }

        m_pLastBallChangePlayer.parent.SetAsLastSibling();
        m_pBallShadow.SetAsLastSibling();
        m_pBall.SetAsLastSibling();

        m_pLastSelect.SetActive(true);
        
        m_pLastBallChangePlayer.gameObject.SetActive(true);
    }

    public void PlayBroadCastText(string strData)
    {
        if(m_bFinish) return;

        m_pBroadCastTextList.Add(strData);
        
        if(!StateMachine.GetStateMachine().IsCurrentTargetStates(m_pBroadCastText,(uint)E_STATE_TYPE.Timer))
        {
            byte eViewSpeed = GameContext.getCtx().MatchViewSpeed();
            float fSpeed = eViewSpeed;
            if(eViewSpeed == 3)
            {
                fSpeed = 2;
            }
            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pBroadCastText),2, (uint)E_STATE_TYPE.Timer, this.enterBroadCastTextCallback, null, this.exitBroadCastTextCallback);
            TimeOutCondition condition = pBaseState.GetCondition<TimeOutCondition>();
            condition.UpdateScale = fSpeed;
            
            StateMachine.GetStateMachine().AddState(pBaseState);
        }
    }
    public void PlayGameOver()
    {
        if(m_pMatchUpdateState != null)
        {
            m_pMatchUpdateState.Exit(true);
        }
        m_pMatchUpdateState = null;

        if(m_pLastBallChangePlayer != null)
        {
            m_pLastBallChangePlayer.gameObject.SetActive(false);
        }

        m_pLastBallChangePlayer = null;

        if(m_bFinish)
        {
            Animation pAnimation = LayoutManager.Instance.FindUIFormRoot<Animation>("UI_Wait");
            pAnimation.Stop();
            pAnimation.gameObject.SetActive(false);
        }
        else
        {
            SoundManager.Instance.PlaySFX("sfx_match_whistle_end");
        }
        
        if(m_pRecord != null)
        {
            m_pRecord.Score = m_pScoreText.text;
            if(m_pMatchEngine != null)
            {
                m_pRecord.StatisticsRecord = m_pMatchEngine.GetMatchStatisticsData().UnPack();
            }
            
            int n = 0;
            for(int i =0; i < m_pMatchTeamData.TeamData.Count; ++i)
            {
                for(n = 0; n < m_pMatchTeamData.TeamData[i].LineUp.Count; ++n)
                {
                    m_pRecord.LineUpRecord[i].List[n] = m_pMatchTeamData.TeamData[i].LineUp[n];
                }
                for(n = 0; n < m_pMatchTeamData.TeamData[i].Tactics.Formation.Count; ++n)
                {
                    m_pRecord.LineUpRecord[i].Formation[n] = m_pMatchTeamData.TeamData[i].Tactics.Formation[n];
                }
            }

        }
        GameContext pGameContext = GameContext.getCtx();
        E_REQUEST_ID eReq = E_REQUEST_ID.league_clear;
        if(m_iMatchType == GameContext.LADDER_ID)
        {
            eReq = E_REQUEST_ID.ladder_clear;
            pGameContext.UpdateLineupPlayerHP(m_pRecord);
        }
        else if(m_iMatchType == GameContext.CHALLENGE_ID)
        {
            eReq = E_REQUEST_ID.challengeStage_clear;
        }
        
        m_pMainScene.RequestAfterCall(eReq,pGameContext.MakeMatchStatsData(E_APP_MATCH_TYPE.APP_MATCH_TYPE_CLEAR,m_pRecord,pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType())));
        if(!pGameContext.IsAdjustEvent("ad0n5r"))
        {
            pGameContext.SendAdjustEvent("zdbmjo",true,true,-1);
        }
    }
    public void PlayWaitView(bool bShow)
    {
        TMPro.TMP_Text text = m_pViewRefresh.transform.Find("text").GetComponent<TMPro.TMP_Text>();
        if(bShow)
        {
            m_pViewRefresh.Play("wait");
            text.SetText(GameContext.getCtx().GetLocalizingText("MATCHVIEW_TXT_TACTICS_ADJUSTMENT_WAITING"));
        }
        else
        {
            m_pViewRefresh.Play("complete");
            text.SetText(GameContext.getCtx().GetLocalizingText("MATCHVIEW_TXT_TACTICS_ADJUSTMENT_APPLY"));
        }
    }
    public void PossessionTotal(string data)
    {
        if( m_pBroadCastTextList.Count == 0)
        {
            if(!string.IsNullOrEmpty(data))
            {
                JObject pData = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(data, new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
                m_fPossessionTotals[(int)pData["team_index"]] = (int)pData["possession"];
            }

            float total = Mathf.Max(m_fPossessionTotals[0] + m_fPossessionTotals[1],1);
            if(m_fPossessionTotals[0] <= 0 && m_fPossessionTotals[1] <= 0)
            {
                m_pHomeBar.fillAmount = 0.5f;
            }
            else
            {
                m_pHomeBar.fillAmount = m_fPossessionTotals[0] / total;
            }

            m_pPowerPercent[0].SetText(string.Format("{0:0.#}%",m_fPossessionTotals[0] / total * 100f ));
            m_pPowerPercent[1].SetText(string.Format("{0:0.#}%",m_fPossessionTotals[1] / total * 100f));
        }
    }

    public List<ulong> GetCurrentPlayerInjuryInfo()
    {
        List<ulong> info = new List<ulong>();
        ushort value = 0;
        for(int i =0; i < m_pRecord.StatisticsRecord.StatisticsPlayers[0].Players.Count; ++i)
        {
            value = m_pRecord.StatisticsRecord.StatisticsPlayers[0].Players[i].Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_INJURY];
            if(value > 0)
            {
                info.Add(m_pRecord.StatisticsRecord.StatisticsPlayers[0].Players[i].PlayerId);
            }
        }
        return info;
    }

    public Dictionary<int,List<ulong>> GetCurrentPlayerYRCardInfo()
    {
        Dictionary<int,List<ulong>> info = new Dictionary<int,List<ulong>>();
        info.Add(0,new List<ulong>());
        info.Add(1,new List<ulong>());
        ushort value = 0;
        ulong playerId = 0;
        List<STATISTICSDATA.PlayerStatisticsT> pStatisticsPlayersList = m_pRecord.StatisticsRecord.StatisticsPlayers[0].Players;
        for(int i =0; i < pStatisticsPlayersList.Count; ++i)
        {
            value = pStatisticsPlayersList[i].Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_RED_CARD];
            // if(i == 10)
            // {
            //     value = 1;
            // }
            playerId = pStatisticsPlayersList[i].PlayerId;
            if(value > 0)
            {
                if(!info[0].Contains(playerId))
                {
                    info[0].Add(playerId);
                }
            }
            else
            {
                value = pStatisticsPlayersList[i].Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_YELLO_CARD];
                // if(i == 4)
                // {
                //     value = 1;
                // }

                if(value > 1)
                {
                    if(!info[0].Contains(playerId))
                    {
                        info[0].Add(playerId);
                    }
                }
                else if(value == 1)
                {
                    if(!info[1].Contains(playerId))
                    {
                        info[1].Add(playerId);
                    }
                }
            }
        }
        return info;
    }
    
    void UpdateMatchStatisticsData()
    {
        if(m_pMatchEngine == null) return;

        MatchStatistics pMatchStatistics = m_pMatchEngine.GetMatchStatisticsData();
        UpdateHomePlayerHpData(pMatchStatistics);
        
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

            UpdateSummaryData();
        }    
        else if(m_eCurrentTab == E_TAB.comparison)
        {
            UpdateTeamStatisticsData(pMatchStatistics);
        }
        else
        {
            if(m_eCurrentTab == E_TAB.homePlayer)
            {
                UpdatePlayerStatisticsData(pMatchStatistics,0);
            }
            else
            {
                UpdatePlayerStatisticsData(pMatchStatistics,1);
            }
        }
        if(m_pRecord != null)
        {
            m_pRecord.StatisticsRecord = pMatchStatistics.UnPack();
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

        for(int i = 1; i < TEAM_STATISTICS_INDEX_LIST.Length; ++i)
        {
            if(TEAM_STATISTICS_INDEX_LIST[i] > -1)
            {
                h = (int)pMatchStatistics.StatisticsTeam(0).Value.Common(TEAM_STATISTICS_INDEX_LIST[i]);
                a = (int)pMatchStatistics.StatisticsTeam(1).Value.Common(TEAM_STATISTICS_INDEX_LIST[i]);
            }
            else
            {
                if(TEAM_STATISTICS_LIST[i] == "MATCHVIEW_COMPARISON_TXT_STAT_POSSESION_TOTAL")
                {
                    h = pMatchStatistics.StatisticsTeam(0).Value.PossessionTotal;
                    a = pMatchStatistics.StatisticsTeam(1).Value.PossessionTotal;
                }
                else
                {
                    h = pMatchStatistics.StatisticsTeam(0).Value.TotalOffside;
                    a = pMatchStatistics.StatisticsTeam(1).Value.TotalOffside;
                }
            }
            
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
    }

    public bool CheckChangeTacticsData( TacticsT data)
    {
        if(m_pMatchTeamData != null)
        {
            int i =0;
            for(i =0; i < data.Formation.Count; ++i)
            {
                if(m_pMatchTeamData.TeamData[0].Tactics.Formation[i] != data.Formation[i])
                {
                    return true;
                }
            }

            for(i =0; i < data.TeamTactics.Count; ++i)
            {
                if(m_pMatchTeamData.TeamData[0].Tactics.TeamTactics[i] != data.TeamTactics[i])
                {
                    return true;
                }
            }
            
            int n =0;
            for(i =0; i < data.PlayerTactics.Count; ++i)
            {
                for( n =0; n < m_pMatchTeamData.TeamData[0].Tactics.PlayerTactics[i].Tactics.Count; ++n)
                {
                    if(m_pMatchTeamData.TeamData[0].Tactics.PlayerTactics[i].Tactics[n] != data.PlayerTactics[i].Tactics[n])
                    {
                        return true;
                    }
                }
            }            
        }

        return false;
    }

    public bool CheckChangeLineupData(List<ulong> data)
    {
        if(m_pMatchTeamData != null)
        {
            int index = 0;
            for(int i =0; i < data.Count; ++i)
            {
                index = m_pMatchTeamData.TeamData[0].LineUp[i];   
                if(m_pMatchTeamData.TeamData[0].PlayerData[index].PlayerId != data[i])
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CheckChangeFormationData( TacticsT data)
    {
        if(m_pMatchTeamData != null)
        {
            int i =0;
            for(i =0; i < data.Formation.Count; ++i)
            {
                if(m_pMatchTeamData.TeamData[0].Tactics.Formation[i] != data.Formation[i])
                {
                    return true;
                }
            }
        }

        return false;
    }

    public TeamDataT GetMyTeamData()
    {
        if(m_pMatchTeamData != null)
        {
            return m_pMatchTeamData.TeamData[0];
        }

        return null;
    }

    PlayerDataT GetPlayerData(int teamIndex, ulong id)
    {
        for(int i =0; i < m_pMatchTeamData.TeamData[teamIndex].PlayerData.Count; ++i)
        {
            if(m_pMatchTeamData.TeamData[teamIndex].PlayerData[i].PlayerId == id)
            {
                return m_pMatchTeamData.TeamData[teamIndex].PlayerData[i];
            }
        }
        return null;   
    }

    void UpdateHomePlayerHpData(MatchStatistics pMatchStatistics)
    {
        StatisticsPlayers pStatisticsPlayers = pMatchStatistics.StatisticsPlayers(0).Value;
        PlayerStatistics pPlayerStatistics;
        PlayerT pPlayerData = null;
        GameContext pGameContext = GameContext.getCtx();
        for(int i = 0 ; i < pStatisticsPlayers.PlayersLength; ++i)
        {
            pPlayerStatistics = pStatisticsPlayers.Players(i).Value;
            pPlayerData = pGameContext.GetPlayerByID(pPlayerStatistics.PlayerId);
            if(pPlayerData != null)
            {
                pPlayerData.Hp = pPlayerStatistics.Hp;
            }
        }
    }

    public RectTransform GetTutorialStepUI(byte eStep)
    {
        if(eStep == 35)
        {
            return m_pLineupUI;
        }
        else if(eStep == 36)
        {
            return m_pLineupUI.Find("play").GetComponent<RectTransform>();
        }
        else if(eStep == 39)
        {
            return m_pInfoUI.Find("command/finish/finish").GetComponent<RectTransform>();
        }
        else if(eStep == 40 || eStep == 41 || eStep == 42)
        {
            return m_pInfoUI.Find("command/finish/exit").GetComponent<RectTransform>();
        }
        
        return null;
    }

    public bool CheckTutorialStep(byte eStep)
    {
        if(eStep ==34)
        {
            return m_pLineupUI.Find("next").gameObject.activeSelf;
        }
        else if(eStep ==35)
        {
            return m_pLineupUI.Find("play").gameObject.activeSelf;
        }
        else if(eStep ==36)
        {
            return !StateMachine.GetStateMachine().IsCurrentTargetStates(m_pViewSetting,(uint)E_STATE_TYPE.ShowDailog);
        }

        return false;
    }

    void UpdatePlayerStatisticsData(MatchStatistics pMatchStatistics,int teamIndex)
    {
        ScrollRect pScroll = m_pScrollList[(int)E_SCROLL.log];
        Transform pItem = null;
        TMPro.TMP_Text text = null;
        PlayerDataT pPlayerData = null;
        StatisticsPlayers pStatisticsPlayers = pMatchStatistics.StatisticsPlayers(teamIndex).Value;
        PlayerStatistics pPlayerStatistics;
        float x =0;
        RectTransform ui = null;
        Vector2 offset;
        int index = 0;
        int value = 0;
        bool bRun = true;
        GameContext pGameContext = GameContext.getCtx();
        Vector2 pos;
        
        for(int n = 0; n < GameContext.CONST_NUMPLAYER; ++n)
        {
            bRun = true;
            pItem = pScroll.content.GetChild(n);
            
            index = m_pTeamDataLineUp[teamIndex,n];
            
            pPlayerStatistics = pStatisticsPlayers.Players(index).Value;
            pPlayerData = GetPlayerData(teamIndex,pPlayerStatistics.PlayerId);
            pItem.gameObject.name = $"{pPlayerStatistics.PlayerId}";

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

            value = pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_YELLO_CARD);

            if(pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_RED_CARD) > 0 || value > 0)
            {
                ui = pItem.Find("player/card").GetComponent<RectTransform>();
                ui.gameObject.SetActive(true);
                offset = ui.offsetMin;
                offset.x = x;
                ui.offsetMin = offset;
                x += ui.rect.width;

                RectTransform redRect = pItem.Find("player/card/r").GetComponent<RectTransform>();
                redRect.gameObject.SetActive(pPlayerStatistics.Common((int)E_COMMON_STATISTICS.COMMON_STATISTICS_RED_CARD) > 0);
                pItem.Find("player/card/y").gameObject.SetActive(value > 0);
                
                pos = redRect.anchoredPosition;
                pos.x = value > 1 ? 5 : 0;
                pos.y = value > 1 ? -5 : 0;
                redRect.anchoredPosition = pos;

                if(value > 1)
                {
                    redRect.gameObject.SetActive(true);
                }
            }
            
            text = pItem.Find("player/name").GetComponent<TMPro.TMP_Text>();
            text.SetText(pPlayerData.PlayerName);

            pItem.Find("bg/DF").gameObject.SetActive(false);
            pItem.Find("bg/FW").gameObject.SetActive(false);
            pItem.Find("bg/GK").gameObject.SetActive(false);
            pItem.Find("bg/MF").gameObject.SetActive(false);
            pItem.Find("bg/S").gameObject.SetActive(false);

            text = pItem.Find("form").GetComponent<TMPro.TMP_Text>();

            if(n < GameContext.CONST_NUMSTARTING)
            {
                index = pGameContext.ConvertPositionByTag((E_LOCATION)m_pMatchTeamData.TeamData[teamIndex].Tactics.Formation[n]);
                string token = pGameContext.GetDisplayLocationName(index);
                text.SetText(token); 
                pItem.Find($"bg/{pGameContext.GetDisplayCardFormationByLocationName(token)}").gameObject.SetActive(true);
            }
            else
            {
                bRun = false;
                text.SetText($"S{n - 10}");
                pItem.Find("bg/S").gameObject.SetActive(true);
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
            pos = tm.anchoredPosition;
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
        
        total = 10;
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
    PlayerT GetPlayerByID(E_PLAYER_INFO_TYPE eTeamType,ulong id)
    {
        if(eTeamType == E_PLAYER_INFO_TYPE.away)
        {
            return GameContext.getCtx().GetAwayClubPlayerByID(id);
        }
        else if(eTeamType == E_PLAYER_INFO_TYPE.match)
        {
            return GameContext.getCtx().GetMatchAwayPlayerByID(id);
        }
        else
        {
            return GameContext.getCtx().GetPlayerByID(id);
        }
    }
    
    public void PlayNoticePopup(E_COMMAND eType,string data)
    {
        JObject pJObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(data, new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
        
        switch(eType)
        {
            case E_COMMAND.COMMAND_TIMELINE_INJURY:
            {
                AddTimeLineItem(E_NOTICE.injury,pJObject);
            }
            return;
            case E_COMMAND.COMMAND_TIMELINE_GOAL:
            {
                if(!m_bFinish)
                {
                    SoundManager.Instance.PlaySFX("sfx_match_goal");
                }
                AddTimeLineItem(E_NOTICE.goal,pJObject);
                if((int)pJObject["team_index"] == 0)
                {   
                    m_SummaryHome[0] += 1;
                }
                else
                {
                    m_SummaryAway[0] += 1;
                }
                UpdateSummaryData();
            }
            break;
            case E_COMMAND.COMMAND_TIMELINE_SUBSTITUTION:
            {
                AddTimeLineItem(E_NOTICE.substitution,pJObject);
            }
                break;
            case E_COMMAND.COMMAND_TIMELINE_REDCARD:
            case E_COMMAND.COMMAND_TIMELINE_YELLOWCARD:
            {
                if(!m_bFinish)
                {
                    SoundManager.Instance.PlaySFX("sfx_match_card");
                }

                AddTimeLineItem(E_NOTICE.card,pJObject);
                byte red = (byte)pJObject["red"];
                byte yellow = (byte)pJObject["yellow"];

                if((int)pJObject["team_index"] == 0)
                {   
                    m_SummaryHome[4] += red;
                    m_SummaryHome[5] += yellow > 0 ? 1 : 0;
                }
                else
                {
                    m_SummaryAway[4] += red;
                    m_SummaryAway[5] += yellow > 0 ? 1 : 0;
                }
                UpdateSummaryData();
            }
            break;
            default: return;
        }   

        if(!m_bFinish)
        {
            GameContext pGameContext = GameContext.getCtx();
            RectTransform dlg = LayoutManager.Instance.GetItem<RectTransform>(NOTICE_ITEM_NAME);
            dlg.SetParent(m_pViewNotice,false);
            dlg.localScale = Vector3.one;
            dlg.gameObject.SetActive(false);
            E_PLAYER_INFO_TYPE eTeamType = E_PLAYER_INFO_TYPE.my;
            switch(eType)
            {
                case E_COMMAND.COMMAND_TIMELINE_GOAL:
                {
                    int teamIndex = (int)pJObject["team_index"];
                    ulong playerGoalId = (ulong)pJObject["player_goal_id"];
                    ulong playerAssistId = (ulong)pJObject["player_assist_id"];
                    int minute = (int)pJObject["minute"];

                    List<PlayerDataT> players = m_pMatchTeamData.TeamData[teamIndex].PlayerData;
                    PlayerDataT goalPlayer = null;
                    PlayerDataT assistPlayer = null;
                    
                    for(int i =0; i < players.Count; ++i)
                    {
                        if(players[i].PlayerId == playerGoalId)
                        {
                            goalPlayer = players[i];
                        }
                        else if(players[i].PlayerId == playerAssistId)
                        {
                            assistPlayer = players[i];
                        }
                    }

                    Transform tm = dlg.Find("top");
                    tm.GetComponent<Graphic>().color = m_pEmblemColor[teamIndex];
                    if(teamIndex == 1)
                    {   
                        eTeamType = E_PLAYER_INFO_TYPE.match;
                    }

                    for(int i =0; i < tm.childCount; ++i)
                    {
                        tm.GetChild(i).gameObject.SetActive(false);
                    }

                    tm.Find("goal").gameObject.SetActive(true);
                    
                    SingleFunc.SetupPlayerCard(GetPlayerByID(eTeamType,playerGoalId),dlg.Find("player"),E_ALIGN.Left);
                    
                    dlg.Find("substitution").gameObject.SetActive(false);
                    dlg.Find("card").gameObject.SetActive(false);
                    tm = dlg.Find("goal");
                    tm.gameObject.SetActive(true);

                    TMPro.TMP_Text text = tm.Find("text").GetComponent<TMPro.TMP_Text>();
                    text.SetText(goalPlayer.PlayerName);
                    tm.Find("assist").gameObject.SetActive(assistPlayer != null);
                    if(assistPlayer != null)
                    {
                        text = tm.Find("assist/text").GetComponent<TMPro.TMP_Text>();
                        text.SetText(assistPlayer.PlayerName);
                    }

                    text = dlg.Find("text").GetComponent<TMPro.TMP_Text>();
                    text.SetText(string.Format("{0} ({1}')",pGameContext.GetLocalizingText("MATCHVIEW_TXT_EVENT_GOAL"),minute));
                }
                break;
                case E_COMMAND.COMMAND_TIMELINE_SUBSTITUTION:
                {
                    m_pMainScene.GetInstance<TacticsFormation>().OnChangeSubstitution(false);

                    int teamIndex = (int)pJObject["team_index"];
                    ulong player_in_id = (ulong)pJObject["player_in_id"];
                    ulong player_out_id = (ulong)pJObject["player_out_id"];
                    int minute = (int)pJObject["minute"];
                    Transform[] pPlayerList = m_pHomePlayerList;
                    List<PlayerDataT> players = m_pMatchTeamData.TeamData[teamIndex].PlayerData;
                    PlayerDataT pOutPlayer = null;
                    PlayerDataT pInPlayer = null;
                    int outIndex = 0;
                    int inIndex = 0;
                    int i =0;
                    for(i =0; i < players.Count; ++i)
                    {
                        if(players[i].PlayerId == player_out_id)
                        {
                            pOutPlayer = players[i];
                            outIndex = i;
                        }
                        if(players[i].PlayerId == player_in_id)
                        {
                            pInPlayer = players[i];
                            inIndex = i;
                        }
                        if(pInPlayer != null && pOutPlayer != null)
                        {
                            break;
                        }
                    }
                    
                    if(teamIndex == 1)
                    {
                        eTeamType = E_PLAYER_INFO_TYPE.match;
                        pPlayerList = m_pAwayPlayerList;
                    }

                    for(i =0; i < m_pMatchTeamData.TeamData[teamIndex].LineUp.Count; ++i)
                    {
                        if(m_pMatchTeamData.TeamData[teamIndex].LineUp[i] == inIndex)
                        {
                            if(i >= GameContext.CONST_NUMSTARTING)
                            {
                                Debug.LogError($"i:{i}  outIndex:{outIndex} inIndex:{inIndex}");
                                break;
                            }
                            pPlayerList[i].Find("name/text").GetComponent<TMPro.TMP_Text>().SetText(m_pMatchTeamData.TeamData[teamIndex].PlayerData[inIndex].PlayerName);
                        }
                    }
                    
                    Transform tm = dlg.Find("top");
                    tm.GetComponent<Graphic>().color = m_pEmblemColor[teamIndex];
                    for(i =0; i < tm.childCount; ++i)
                    {
                        tm.GetChild(i).gameObject.SetActive(false);
                    }

                    tm.Find("substitution").gameObject.SetActive(true);
                    
                    SingleFunc.SetupPlayerCard(GetPlayerByID(eTeamType,player_in_id),dlg.Find("player"),E_ALIGN.Left);
                    
                    dlg.Find("goal").gameObject.SetActive(false);
                    dlg.Find("card").gameObject.SetActive(false);
                    tm = dlg.Find("substitution");
                    tm.gameObject.SetActive(true);

                    TMPro.TMP_Text text = tm.Find("text").GetComponent<TMPro.TMP_Text>();
                    text.SetText(pInPlayer.PlayerName);

                    text = tm.Find("changeOut/text").GetComponent<TMPro.TMP_Text>();
                    text.SetText(pOutPlayer.PlayerName);

                    text = dlg.Find("text").GetComponent<TMPro.TMP_Text>();
                    text.SetText(string.Format("{0} ({1}')",pGameContext.GetLocalizingText("MATCHVIEW_TXT_EVENT_SUBSTITUTION"),minute));
                }
                    break;
                case E_COMMAND.COMMAND_TIMELINE_REDCARD:
                case E_COMMAND.COMMAND_TIMELINE_YELLOWCARD:
                {
                    int teamIndex = (int)pJObject["team_index"];
                    ulong player_id = (ulong)pJObject["player_id"];
                    int minute = (int)pJObject["minute"];
                    byte yellow = (byte)pJObject["yellow"];
                    byte red = (byte)pJObject["red"];

                    List<PlayerDataT> players = m_pMatchTeamData.TeamData[teamIndex].PlayerData;
                    PlayerDataT pPlayer = null;
                    
                    for(int i =0; i < players.Count; ++i)
                    {
                        if(players[i].PlayerId == player_id)
                        {
                            pPlayer = players[i];
                            break;
                        }
                    }

                    if(teamIndex == 1)
                    {
                        eTeamType = E_PLAYER_INFO_TYPE.match;
                    }
                    Transform tm = dlg.Find("top");
                    tm.GetComponent<Graphic>().color = m_pEmblemColor[teamIndex];
                    for(int i =0; i < tm.childCount; ++i)
                    {
                        tm.GetChild(i).gameObject.SetActive(false);
                    }

                    tm.Find("card").gameObject.SetActive(true);
                    
                    SingleFunc.SetupPlayerCard(GetPlayerByID(eTeamType,player_id),dlg.Find("player"),E_ALIGN.Left);
                    
                    dlg.Find("goal").gameObject.SetActive(false);
                    dlg.Find("substitution").gameObject.SetActive(false);

                    TMPro.TMP_Text text = dlg.Find("card").GetComponent<TMPro.TMP_Text>();
                    text.gameObject.SetActive(true);
                    text.SetText(pPlayer.PlayerName);

                    RectTransform redRect = text.transform.Find("r").GetComponent<RectTransform>();

                    text.transform.Find("y").gameObject.SetActive(yellow > 0);
                    redRect.gameObject.SetActive(red > 0);
                    
                    Vector2 pos = redRect.anchoredPosition;
                    pos.x = yellow > 1 ? 5 : 0;
                    pos.y = yellow > 1 ? -5 : 0;
                    redRect.anchoredPosition = pos;
                    
                    text = dlg.Find("text").GetComponent<TMPro.TMP_Text>();
                    text.SetText(string.Format("{0} ({1}')", (red > 0 || yellow > 1) ? pGameContext.GetLocalizingText("MATCHVIEW_TXT_EVENT_RED_CARD") : pGameContext.GetLocalizingText("MATCHVIEW_TXT_EVENT_YELLOW_CARD") ,minute));
                }
                    break;
                default:
                {
                    LayoutManager.Instance.AddItem(NOTICE_ITEM_NAME,dlg);
                }
                return;
            }

            byte eViewSpeed = pGameContext.MatchViewSpeed();
    
            float fSpeed = eViewSpeed;
            if(eViewSpeed == 3)
            {
                fSpeed = 2;
            }

            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),0.3f, (uint)E_STATE_TYPE.ShowNoticePopup, this.enterDilogMoveCallback, this.executeDilogMoveCallback, this.exitDilogMoveCallback);
            DilogMoveStateData _data = new DilogMoveStateData();
            _data.FadeDelta = 2f;
            _data.Out = false;
            pBaseState.StateData = _data;
            TimeOutCondition condition = pBaseState.GetCondition<TimeOutCondition>();
            condition.UpdateScale = fSpeed;
            
            if(StateMachine.GetStateMachine().IsCurrentTargetStates((uint)E_STATE_TYPE.ShowNoticePopup))
            {
                m_pNoticeStateList.Enqueue(pBaseState);
            }
            else
            {
                StateMachine.GetStateMachine().AddState(pBaseState);
            }
        }
    }

    void enterBroadCastTextCallback(IState state)
    {
        JObject pData = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(m_pBroadCastTextList[0], new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
        m_pBroadCastText.gameObject.SetActive(true);

        m_pPowerPercent[0].gameObject.SetActive(false);
        m_pPowerPercent[1].gameObject.SetActive(false);
        int teamIndex = 255;
        int playerIndex = 255;
        int no = 0;
        
        if(pData.ContainsKey("team1"))
        {
            teamIndex = (int)pData["team1"];
        }
        if(teamIndex == 255 && pData.ContainsKey("team2"))
        {
            teamIndex = (int)pData["team2"];
        }

        if(pData.ContainsKey("player1"))
        {
            playerIndex = (int)pData["player1"];
        }
        if(playerIndex == 255 && pData.ContainsKey("player2"))
        {
            playerIndex = (int)pData["player2"];
        }

        if(pData.ContainsKey("no"))
        {
            no = (int)pData["no"];
        }
        
        if(teamIndex == 0)
        {
            m_pHomeBar.fillAmount = 1;
        }
        else
        {
            m_pHomeBar.fillAmount = 0;
        }

        if(no > 0)
        {
            MatchCommentaryList pMatchCommentaryList = GameContext.getCtx().GetMatchCommentaryData();
            MatchCommentaryItem? pMatchCommentaryItem = null;
            CommentaryItem? pCommentaryItem = null;
            for(int i =0; i < pMatchCommentaryList.MatchCommentaryLength; ++i)
            {
                pMatchCommentaryItem = pMatchCommentaryList.MatchCommentary(i);
                if(pMatchCommentaryItem != null )
                {
                    for(int n =0; n < pMatchCommentaryItem.Value.ListLength; ++n)
                    {
                        pCommentaryItem = pMatchCommentaryItem.Value.List(n);
                        if(pCommentaryItem != null && pCommentaryItem.Value.No == no)
                        {
                            string token = GameContext.getCtx().GetLocalizingText(pCommentaryItem.Value.Text);
                            if(token.Contains("#1") && teamIndex != 255 && playerIndex != 255)
                            {
                                token = token.Replace("#1",m_pMatchTeamData.TeamData[teamIndex].PlayerData[playerIndex].PlayerName);
                            }

                            if(token.Contains("#5") && teamIndex != 255)
                            {
                                token = token.Replace("#5",m_pMatchTeamData.TeamData[teamIndex].TeamName);
                            }

                            m_pBroadCastText.SetText(token);
                            return;
                        }
                    }
                }
            }
        }
        
        m_pBroadCastText.SetText("");
    }
    IState exitBroadCastTextCallback(IState state)
    {
        BaseState pBaseState = null;
        if( m_pBroadCastTextList.Count > 0)
        {
            m_pBroadCastTextList.RemoveAt(0);
            if( m_pBroadCastTextList.Count > 0)
            {
                byte eViewSpeed = GameContext.getCtx().MatchViewSpeed();
                float fSpeed = eViewSpeed;
                if(eViewSpeed == 3)
                {
                    fSpeed = 2;
                }
                
                pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pBroadCastText),2, (uint)E_STATE_TYPE.Timer, this.enterBroadCastTextCallback, null, this.exitBroadCastTextCallback);
                TimeOutCondition condition = pBaseState.GetCondition<TimeOutCondition>();
                condition.UpdateScale = fSpeed;
            }
        }

        if( m_pBroadCastTextList.Count == 0)
        {
            m_pBroadCastText.gameObject.SetActive(false);
            m_pPowerPercent[0].gameObject.SetActive(true);
            m_pPowerPercent[1].gameObject.SetActive(true);
            PossessionTotal(null);
        }
        return pBaseState;
    }

    void enterDilogMoveCallback(IState state)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            RectTransform target = state.GetTarget<BaseStateTarget>().GetMainTarget<RectTransform>();

            if(data.Out)
            {
                target.offsetMin = new Vector2(0,target.offsetMin.y);
                target.offsetMax = new Vector2(0,target.offsetMax.y);
            }
            else
            {
                target.gameObject.SetActive(true);
                target.offsetMin = new Vector2(LayoutManager.Width,target.offsetMin.y);
                target.offsetMax = new Vector2(LayoutManager.Width,target.offsetMax.y);
            }
        }
    }
    bool executeDilogMoveCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
            if(data.FadeDelta >= 0)
            {
                float p = 0;
                if(data.Out)
                {
                    p = LayoutManager.Width * ALFUtils.EaseIn(condition.GetTimePercent(),2);
                }
                else
                {
                    p = LayoutManager.Width * (1.0f - ALFUtils.EaseIn(condition.GetTimePercent(),2));
                }
                
                RectTransform target = state.GetTarget<BaseStateTarget>().GetMainTarget<RectTransform>();
                target.offsetMin = new Vector2(p,target.offsetMin.y);
                target.offsetMax = new Vector2(p,target.offsetMax.y);

                if(bEnd && data.FadeDelta > 0)
                {
                    condition.Reset();
                    condition.SetRemainTime(data.FadeDelta);
                    data.FadeDelta *= -1;
                    bEnd = false;
                }
            }
            else if( data.FadeDelta < 0)
            {
                data.FadeDelta += dt;
                if(data.FadeDelta > 0)
                {
                    data.FadeDelta =0;
                }
            }

            if(bEnd && !data.Out)
            {
                BaseStateTarget target = state.GetTarget<BaseStateTarget>();
                if(m_pResult == target.GetMainTarget<RectTransform>())
                {
                    ShowRewardItems();
                }
            }
        }
        
        return bEnd;
    }
    IState exitDilogMoveCallback(IState state)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            RectTransform dlg = state.GetTarget<BaseStateTarget>().GetMainTarget<RectTransform>();
            if(data.Out)
            {
                LayoutManager.Instance.AddItem(NOTICE_ITEM_NAME,dlg);

                if(m_pNoticeStateList.Count > 0)
                {
                    StateMachine.GetStateMachine().AddState(m_pNoticeStateList.Dequeue());
                }
            }
            else
            {
                 byte eViewSpeed = GameContext.getCtx().MatchViewSpeed();
                float fSpeed = eViewSpeed;
                if(eViewSpeed == 3)
                {
                    fSpeed = 2;
                }

                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),0.3f, (uint)E_STATE_TYPE.ShowDailog, this.enterDilogMoveCallback, this.executeDilogMoveCallback, this.exitDilogMoveCallback);
                DilogMoveStateData _data = new DilogMoveStateData();
                _data.FadeDelta = 0f;
                _data.Out = true;
                pBaseState.StateData = _data;
                TimeOutCondition condition = pBaseState.GetCondition<TimeOutCondition>();
                condition.UpdateScale = fSpeed;
                StateMachine.GetStateMachine().AddState(pBaseState);
            }
        }

        return null;
    }

    IState exitTimerCallback(IState state)
    {
        Transform dlg = state.GetTarget<BaseStateTarget>().GetMainTarget<Transform>();
        DilogMoveStateData data = new DilogMoveStateData();
        data.FadeDelta = 1 / 0.3f;
        data.Out = true;
        ALFUtils.FadeObject(dlg,1);
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),0.3f, (uint)E_STATE_TYPE.ShowDailog, null, this.executeFadeDilogCallback,this.exitFadeDilogCallback);
        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);
        dlg = dlg.parent;
        if(dlg != null)
        {
            dlg.GetComponent<Button>().enabled = false;
        }
        return null;
    }

    bool executeFadeDilogCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
            RectTransform target = state.GetTarget<BaseStateTarget>().GetMainTarget<RectTransform>();
            if(data.Out)
            {
                ALFUtils.FadeObject(target,-(data.FadeDelta * dt));
            }
            else
            {
                ALFUtils.FadeObject(target,data.FadeDelta * dt);
            }
        }

        return bEnd;
    }

    IState exitFadeDilogCallback(IState state)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            Transform target = state.GetTarget<BaseStateTarget>().GetMainTarget<Transform>();
            if(data.Out)
            {
                target.gameObject.SetActive(false);
            }
            else
            {
                StateMachine.GetStateMachine().AddState(BaseState.GetInstance(new BaseStateTarget(target),3, (uint)E_STATE_TYPE.Timer, null, null, this.exitTimerCallback));
            }
            
            target = target.parent;
            if(target != null)
            {
                target.GetComponent<Button>().enabled = true;
            }
        }

        return null;
    }

    IState exitLineupDilogCallback(IState state)
    {
        Animation target = state.GetTarget<BaseStateTarget>().GetMainTarget<Animation>();
        if(target.gameObject.name == "away")
        {
            m_pLineupUI.Find("play").gameObject.SetActive(true);
        }
        else if(target.gameObject.name == "feild")
        {
            target = m_pLineupUI.Find("away").GetComponent<Animation>();
            target.gameObject.SetActive(true);
            target.Play();

            return BaseState.GetInstance(new BaseStateTarget(target),target["feildShow"].length, (uint)E_STATE_TYPE.ShowDailog, null, null,this.exitLineupDilogCallback);
        }
        else
        {
            m_pLineupUI.Find("next").gameObject.SetActive(true);    
        }
        
        return null;
    }

    
    void ShowRewardItems()
    {
        Animation pAnimation = null;
        BaseState pBaseState = null;
        float fDelay = 0f;
        Transform ui = m_pResult.Find("root/rewards");

        for(int i =0; i < ui.childCount; ++i)
        {
            pAnimation = ui.GetChild(i).GetComponent<Animation>();
            pBaseState = BaseState.GetInstance(new BaseStateTarget(pAnimation),fDelay, (uint)E_STATE_TYPE.Shake, null, null, exitCallback);
            StateMachine.GetStateMachine().AddState(pBaseState);
            fDelay += 0.2f;
        }
    }

    IState exitCallback(IState state)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>((uint)E_STATE_TYPE.Shake);
            if(list.Count <= 1)
            {
                LayoutManager.Instance.InteractableEnabledAll();
            }
            
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            Animation pAnimation = target.GetMainTarget<Animation>();
            pAnimation.gameObject.SetActive(true);
            pAnimation.Play("blink_target");
        }
        
        return null;
    }
    
    void ResetTimer(bool bAdd)
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pViewSetting,(uint)E_STATE_TYPE.Timer);
        for(int i =0; i < list.Count; ++i)
        {
            StateMachine.GetStateMachine().RemoveState(list[i]);
        }
        if(bAdd)
        {
            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pViewSetting),3, (uint)E_STATE_TYPE.Timer, null, null, this.exitTimerCallback);
            StateMachine.GetStateMachine().AddState(pBaseState);
        }
    }

    public void FinishGame()
    {
        UpdateMatchSkipItem(true);
        m_pMatchUpdateState.Paused = false;
        m_pPauseText.gameObject.SetActive(false);
        m_bFinish = true;
        Animation pAnimation = LayoutManager.Instance.FindUIFormRoot<Animation>("UI_Wait");
        pAnimation.gameObject.SetActive(true);
        pAnimation.Play();
        m_pMatchEngine.FinishMatch();
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
        ALFUtils.Assert(pScroll != null, "MatchView : SetupScroll => pScroll = null !");
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

    void AddTimeLineItem(E_NOTICE eType,JObject data)
    {
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
        int m = (int)data["minute"];
        text.SetText(m.ToString());

        m = (int)data["team_index"];
        TimeLineT pTimeLine = null;
        
        if(eType == E_NOTICE.goal)
        {
            pTimeLine = new TimeLineT();
            pTimeLine.Goal = new GoalRecordT();
            
            ulong playerGoalId = (ulong)data["player_goal_id"];
            ulong playerAssistId = (ulong)data["player_assist_id"];
            
            pTimeLine.Goal.TeamIndex = (byte)m;
            pTimeLine.Goal.Minute = (ushort)data["minute"];
            pTimeLine.Goal.Second = (ushort)data["second"];
            pTimeLine.Goal.IsOwnGoal = (bool)data["is_own_goal"];
            
            List<PlayerDataT> players = m_pMatchTeamData.TeamData[m].PlayerData;
            PlayerDataT goalPlayer = null;
            PlayerDataT assistPlayer = null;
            ulong playerId = 0;
            for(int i =0; i < players.Count; ++i)
            {
                playerId = players[i].PlayerId;
                if(playerId == playerGoalId)
                {
                    goalPlayer = players[i];
                }
				if(playerId == playerAssistId)
                {
                    assistPlayer = players[i];
                }

                if(goalPlayer != null && assistPlayer != null)
                {
                    break;
                }
            }
            pItem.Find("goal/away").gameObject.SetActive(m == 1);
            pItem.Find("goal/home").gameObject.SetActive(m == 0);
            
            if(m == 0)
            {
                text = pItem.Find("goal/home").GetComponent<TMPro.TMP_Text>();
            }
            else
            {
                text = pItem.Find("goal/away").GetComponent<TMPro.TMP_Text>();
            }

            pTimeLine.Goal.PlayerGoalName = goalPlayer != null ? goalPlayer.PlayerName : "";
            text.SetText(pTimeLine.Goal.PlayerGoalName);
            RectTransform tm = text.GetComponent<RectTransform>();
            Vector2 anchoredPosition = tm.anchoredPosition;
            text = text.transform.Find("assist").GetComponent<TMPro.TMP_Text>();
            text.gameObject.SetActive(assistPlayer != null);
            if(assistPlayer != null)
            {
                pTimeLine.Goal.PlayerAssistName = assistPlayer.PlayerName;
                text.SetText(assistPlayer.PlayerName);
                anchoredPosition.y = tm.rect.height * 0.5f;
            }
            else
            {
                anchoredPosition.y = 0;
            }

            if(goalPlayer == null)
            {
                m = m == 0 ? 1: 0;
                players = m_pMatchTeamData.TeamData[m].PlayerData;
                for(int i =0; i < players.Count; ++i)
                {
                    playerId = players[i].PlayerId;
                    if(playerId == playerGoalId)
                    {
                        goalPlayer = players[i];
                        break;
                    }
                }

                if(goalPlayer != null)
                {
                    JObject pJObject = new JObject();
                    pJObject["matchId"] = pGameContext.GetCurrentMatchID();
                    pJObject["playerGoalId"] = playerGoalId;
                    pJObject["teamIndex"] = m;
                    SingleFunc.SendLog(pJObject.ToString());
                }
            }
        }
        else if(eType == E_NOTICE.substitution)
        {
            pTimeLine = new TimeLineT();
            pTimeLine.Substitution = new SubstitutionRecordT();
            
            ulong player_in_id = (ulong)data["player_in_id"];
            ulong player_out_id = (ulong)data["player_out_id"];
         
            pTimeLine.Substitution.TeamIndex = (byte)m;
            pTimeLine.Substitution.Minute = (ushort)data["minute"];
            pTimeLine.Substitution.Second = (ushort)data["second"];
            
            List<PlayerDataT> players = m_pMatchTeamData.TeamData[m].PlayerData;
            PlayerDataT pOutPlayer = null;
            PlayerDataT pInPlayer = null;
            int i =0;
            for(i =0; i < players.Count; ++i)
            {
                if(players[i].PlayerId == player_out_id)
                {
                    pOutPlayer = players[i];
                }
                else if(players[i].PlayerId == player_in_id)
                {
                    pInPlayer = players[i];
                }
                
                if(pInPlayer != null && pOutPlayer != null)
                {
                    break;
                }
            }

            for(i =0; i < m_pMatchTeamData.TeamData[m].LineUp.Count; ++i)
            {
                m_pTeamDataLineUp[m,i] = m_pMatchTeamData.TeamData[m].LineUp[i];
            }
            
            text = pItem.Find("substitution/home").GetComponent<TMPro.TMP_Text>();
            pTimeLine.Substitution.PlayerInName = pInPlayer.PlayerName;
            pTimeLine.Substitution.PlayerOutName= pOutPlayer.PlayerName;

            text.SetText(pInPlayer.PlayerName);
            text.gameObject.SetActive(m == 0);
            text = text.transform.Find("out").GetComponent<TMPro.TMP_Text>();
            text.SetText(pOutPlayer.PlayerName);
            text = pItem.Find("substitution/away").GetComponent<TMPro.TMP_Text>();
            text.gameObject.SetActive(m == 1);
            text.SetText(pInPlayer.PlayerName);
            text = text.transform.Find("out").GetComponent<TMPro.TMP_Text>();
            text.SetText(pOutPlayer.PlayerName);
        }
        else if(eType == E_NOTICE.card)
        {
            pTimeLine = new TimeLineT();
            pTimeLine.Card = new CardRecordT();
            
            ulong player_id = (ulong)data["player_id"];
            byte yellow = (byte)data["yellow"];
            byte red = (byte)data["red"];

            pTimeLine.Card.Yellow = yellow;
            pTimeLine.Card.Red = red;
            pTimeLine.Card.TeamIndex = (byte)m;
            pTimeLine.Card.Minute = (ushort)data["minute"];
            
            List<PlayerDataT> players = m_pMatchTeamData.TeamData[m].PlayerData;
            PlayerDataT pPlayer = null;
            
            for(int i =0; i < players.Count; ++i)
            {
                if(players[i].PlayerId == player_id)
                {
                    pPlayer = players[i];
                    break;
                }
            }
            
            pTimeLine.Card.PlayerName = pPlayer.PlayerName;
            
            if(m == 0)
            {
                text = pItem.Find("card/away").GetComponent<TMPro.TMP_Text>();
                text.gameObject.SetActive(false);
                text = pItem.Find("card/home").GetComponent<TMPro.TMP_Text>();
                text.gameObject.SetActive(true);
            }
            else
            {
                text = pItem.Find("card/home").GetComponent<TMPro.TMP_Text>();
                text.gameObject.SetActive(false);
                text = pItem.Find("card/away").GetComponent<TMPro.TMP_Text>();
                text.gameObject.SetActive(true);
            }
            
            text.SetText(pPlayer.PlayerName);
            RectTransform redRect = text.transform.Find("icon/r").GetComponent<RectTransform>();
            
            text.transform.Find("icon/y").gameObject.SetActive(yellow > 0);
            redRect.gameObject.SetActive((red > 0 || yellow > 1));
            Vector2 pos = redRect.anchoredPosition;
            pos.x = yellow > 1 ? 5 : 0;
            pos.y = yellow > 1 ? -5 : 0;
            redRect.anchoredPosition = pos;
        }
        else if(eType == E_NOTICE.injury)
        {
            pTimeLine = new TimeLineT();
            pTimeLine.Injury = new InjuryRecordT();
            
            ulong player_id = (ulong)data["player_id"];
            
            pTimeLine.Injury.TeamIndex = (byte)m;
            pTimeLine.Injury.Minute = (ushort)data["minute"];
            
            List<PlayerDataT> players = m_pMatchTeamData.TeamData[m].PlayerData;
            PlayerDataT pPlayer = null;
            
            for(int i =0; i < players.Count; ++i)
            {
                if(players[i].PlayerId == player_id)
                {
                    pPlayer = players[i];
                    break;
                }
            }
            
            pTimeLine.Injury.PlayerName = pPlayer.PlayerName;
            
            if(m == 0)
            {
                text = pItem.Find("injury/away").GetComponent<TMPro.TMP_Text>();
                text.gameObject.SetActive(false);
                text = pItem.Find("injury/home").GetComponent<TMPro.TMP_Text>();
                text.gameObject.SetActive(true);
            }
            else
            {
                text = pItem.Find("injury/home").GetComponent<TMPro.TMP_Text>();
                text.gameObject.SetActive(false);
                text = pItem.Find("injury/away").GetComponent<TMPro.TMP_Text>();
                text.gameObject.SetActive(true);
            }
            
            text.SetText(pPlayer.PlayerName);
        }

        if(pTimeLine != null)
        {
            m_pRecord.TimeRecord.Add(pTimeLine);
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
            Canvas.ForceUpdateCanvases();
            pScroll.verticalNormalizedPosition = 1;
        }
    }

    bool CheckTacticsData(TacticsT pTempTactics)
    {
        int i =0;
        int n =0;
        for(i =0; i < m_pMatchTeamData.TeamData[0].Tactics.Formation.Count; ++i)
        {
            if(m_pMatchTeamData.TeamData[0].Tactics.Formation[i] != pTempTactics.Formation[i])
            {
                return true;
            }
        }
        for(i =0; i < m_pMatchTeamData.TeamData[0].Tactics.TeamTactics.Count; ++i)
        {
            if(m_pMatchTeamData.TeamData[0].Tactics.TeamTactics[i] != pTempTactics.TeamTactics[i])
            {
                return true;
            }
        }
        
        for(i =0; i < m_pMatchTeamData.TeamData[0].Tactics.PlayerTactics.Count; ++i)
        {
            for(n =0; n < pTempTactics.PlayerTactics[i].Tactics.Count; ++n)
            {
                if(pTempTactics.PlayerTactics[i].Tactics[n] != m_pMatchTeamData.TeamData[0].Tactics.PlayerTactics[i].Tactics[n])
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void UpdateTacticsData(TacticsT pTempTactics, ulong[] pSubstPlayer,LineupPlayerT pLineupPlayer)
    {
        if(m_pMatchEngine != null && m_pMatchTeamData != null)
        {
            TeamDataT pTeamData = new TeamDataT();
            TacticsT pTactics = new TacticsT();
            MATCHTEAMDATA.TeamDataT pOriginTeamData = m_pMatchTeamData.TeamData[0];
            SingleFunc.CloneTacticsDataByIndex( pOriginTeamData.Tactics ,ref pTactics);
            pTeamData.Tactics = pTactics;
            pTeamData.LineUp = new List<int>();

            int i =0;
            bool bAdd = false;
            
            for(i =0; i < pOriginTeamData.LineUp.Count; ++i)
            {
                pTeamData.LineUp.Add(pOriginTeamData.LineUp[i]);
            }

            SubstPairT pSubstPair = null;

            List<byte> pOriginData = pOriginTeamData.Tactics.Formation;
            for(i =0; i < pTempTactics.Formation.Count; ++i)
            {
                if(pOriginData[i] != pTempTactics.Formation[i])
                {
                    bAdd = true;
                    pOriginData[i] = pTempTactics.Formation[i];
                }
            }
            
            if(bAdd)
            {
                pTeamData.SubstPair = new List<SubstPairT>();
                            
                for(i =0; i < pTempTactics.Formation.Count; ++i)
                {
                    pSubstPair = new SubstPairT();
                    pSubstPair.Loc = pTempTactics.Formation[i];
                    pSubstPair.PlayerOrgIndex = (byte)pOriginTeamData.LineUp[i];
                    pTeamData.SubstPair.Add(pSubstPair);
                }
            }

            for(i =0; i < GameContext.CONST_NUMSTARTING; ++i)
            {
                if(pLineupPlayer.Data[i] != pOriginTeamData.PlayerData[pOriginTeamData.LineUp[i]].PlayerId)
                {
                    bAdd = true;
                    for(int t =0; t < pOriginTeamData.PlayerData.Count; ++t)
                    {
                        if(pOriginTeamData.PlayerData[t].PlayerId == pLineupPlayer.Data[i])
                        {
                            pTeamData.LineUp[i] = t;
                            break;
                        }
                    }
                }
            }

            pOriginData = pOriginTeamData.Tactics.TeamTactics;
            for(i =0; i < pOriginData.Count; ++i)
            {
                if(pOriginData[i] != pTempTactics.TeamTactics[i])
                {
                    bAdd = true;
                    pOriginData[i] = pTempTactics.TeamTactics[i];
                }
            }

            int n =0;
            List<PlayerTacticsT> list = pOriginTeamData.Tactics.PlayerTactics;
            List<byte> tempList= null;
            for(i =0; i < list.Count; ++i)
            {
                pOriginData = list[i].Tactics;
                tempList = pTempTactics.PlayerTactics[i].Tactics;
                for(n =0; n < tempList.Count; ++n)
                {
                    if(tempList[n] != pOriginData[n])
                    {
                        bAdd = true;
                        pOriginData[n] = tempList[n];
                    }
                }
            }
            
            if(pSubstPlayer != null)
            {
                m_pViewRefresh.gameObject.SetActive(true);
                bool bSubstitution = false;
                if(pTeamData.SubstPair == null)
                {
                    pTeamData.SubstPair = new List<SubstPairT>();
                    for(i =0; i < pTempTactics.Formation.Count; ++i)
                    {
                        pSubstPair = new SubstPairT();
                        pSubstPair.Loc = pTempTactics.Formation[i];
                        pSubstPair.PlayerOrgIndex = (byte)pOriginTeamData.LineUp[i];
                        pTeamData.SubstPair.Add(pSubstPair);
                    }
                }

                if(pOriginTeamData.Subst == null)
                {
                    pOriginTeamData.Subst = new List<int>();
                }
                
                for( i =0; i < pSubstPlayer.Length; ++i)
                {
                    if(pSubstPlayer[i] != 0)
                    {
                        bAdd = true;
                        bSubstitution = true;
                        ++m_eChnageSubstitutionCount;
                        int index = pOriginTeamData.LineUp[i];

                        for( n =0; n < pOriginTeamData.PlayerData.Count; ++n)
                        {
                            if(pOriginTeamData.PlayerData[n].PlayerId == pSubstPlayer[i])
                            {
                                pOriginTeamData.Subst.Add(index);
                                pTeamData.SubstPair[i].PlayerOrgIndex = (byte)n;                                
                                pOriginTeamData.LineUp[i] = n;
                                pOriginTeamData.LineUp[n] = index;
                                break;
                            }
                        }
                    }
                }
                
                if(bSubstitution)
                {
                    m_pMainScene.GetInstance<TacticsFormation>().OnChangeSubstitution(true);
                }
            }
            
            if(bAdd)
            {
                m_pMatchEngine.SetChangeTacticsData(0,pTeamData.SerializeToBinary());
            }
        }
    }

    public void Close()
    {
        SoundManager.Instance.FadeOutFX(m_iRankUpSFX);
        
        m_pMainScene.GetInstance<Ground>().FocusStadium(false);
        m_pMainScene.ShowBackground(true);
        m_pMainScene.ResetMainButton();
        m_pMainScene.UpdateGameMaterials();
        m_pMainScene.HideMoveDilog(MainUI,Vector3.right);
        if(m_iMatchType != GameContext.LADDER_ID)
        {
            m_pMainScene.ShowLeaguePopup();
        }
    }

    public void ResetRewardUI()
    {
        m_iRankUpSFX = -1;
        Transform ui = m_pResult.Find("root/rewards");
        RectTransform item = null;
        RawImage icon = null;
        int i = ui.childCount;
        
        while(i > 0)
        {
            --i;
            item = ui.GetChild(i).GetComponent<RectTransform>();
            SingleFunc.AddRewardIcon(item,REWARD_ITEM_NAME);
        }

        icon = m_pRankUp.transform.Find("root/rank").GetComponent<RawImage>();
        SingleFunc.ClearRankIcon(icon);
        icon = m_pResult.Find("root/info/total/box/rank").GetComponent<RawImage>();
        SingleFunc.ClearRankIcon(icon);
        
        m_pRankUp.transform.Find("root/title/up").gameObject.SetActive(false);
        m_pRankUp.transform.Find("root/title/dn").gameObject.SetActive(false);

        ParticleSystem pParticleSystem = m_pRankUp.transform.Find("root/rankUp/FX_2").GetComponent<ParticleSystem>();
        pParticleSystem.time = 0;
        pParticleSystem.Stop(true);
        UnityEngine.UI.Extensions.UIParticleSystem[] list = pParticleSystem.GetComponentsInChildren<UnityEngine.UI.Extensions.UIParticleSystem>(true);
        for(i =0; i < list.Length; ++i)
        {
            list[i].StopParticleEmission();
        }

        m_pEmblemList[1].Dispose();
        
        for( i =0; i < m_pEmblemList.Length; ++i)
        {
            m_pEmblemList[i].material = null;
        }
        m_pRecord = null;
    }

    void SetupRewardData(JObject rewardData)
    {
        int winStreak = 0;
        m_iRankUp = 0;

        RectTransform ui = m_pResult.Find("root/rewards").GetComponent<RectTransform>();
        TMPro.TMP_Text text = null;
        GameContext pGameContext = GameContext.getCtx();
        AdRewardList pAdRewardList = pGameContext.GetFlatBufferData<AdRewardList>(E_DATA_TYPE.AdReward);
        AdRewardItem? pAdRewardItem = null;
        
        if(m_iMatchType == GameContext.LADDER_ID)
        {
            if(m_iMatchCount == 3)
            {
                pAdRewardItem = pAdRewardList.AdRewardByKey((uint)E_AD.Match3);
                AdRewardDataT pAdRewardData = pGameContext.GetAdRewardDataByID((uint)E_AD.Match3);
                if(pAdRewardData.Amount < pAdRewardItem.Value.MaxAmount)
                {
                    m_eAdTypeList.Add(E_AD.Match3);
                }
                m_iMatchCount =0;
            }
        }

        if(rewardData.ContainsKey("rewards"))
        {
            JArray pArray = (JArray)rewardData["rewards"];
            JObject pData= null;
            uint no = 0;
            
            int i  =0;
            Vector2 anchor = new Vector2(0, 0.5f);
            int index = -1;
            Dictionary<uint,ulong> rewardList = new Dictionary<uint, ulong>();
            for(i =0; i < pArray.Count; ++i)
            {
                pData = (JObject)pArray[i];
                no = (uint)pData["no"];
                
                if(no == GameContext.TROPHY_ITEM_ID)
                {                    
                    text = m_pResult.Find("root/info/win/box/count").GetComponent<TMPro.TMP_Text>();
                    
                    winStreak = (int)pData["amount"] - winStreak;
                    if(winStreak > 0)
                    {
                        text.SetText($"+{winStreak}");
                    }
                    else
                    {
                        text.SetText(winStreak.ToString());
                    }
                }
                else if(no == GameContext.RANK_ITEM_ID)
                {
                    m_iRankUp = (int)pData["amount"];
                }
                else
                {
                    ++index;
                    if((ulong)pData["amount"] > 0)
                    {
                        if(rewardList.ContainsKey(no))
                        {
                            rewardList[no] += (ulong)pData["amount"];
                        }
                        else
                        {
                            rewardList.Add(no,(ulong)pData["amount"]);
                        } 
                    }
                }
            }

            RectTransform item = null;
            
            var itr = rewardList.GetEnumerator();
            
            while(itr.MoveNext())
            {
                item = SingleFunc.GetRewardIcon(ui,REWARD_ITEM_NAME,itr.Current.Key,itr.Current.Value);
                item.Find("eff").gameObject.SetActive(true);
                item.Find("eff").GetComponent<Graphic>().color = Color.white;
                item.gameObject.SetActive(false);
            }
        }
        
        int win = (int)rewardData["win"];

        if(m_iMatchType == GameContext.LADDER_ID)
        {
            m_pResult.Find("root/info").gameObject.SetActive(true);
            Transform tm = m_pResult.Find("root/bg");
            for(int n =0; n < tm.childCount; ++n)
            {
                tm.GetChild(n).gameObject.SetActive(false);
            }

            text = m_pResult.Find("root/info/win/title").GetComponent<TMPro.TMP_Text>();

            m_pResult.Find("root/info/winningStreak").gameObject.SetActive(win == WIN_ID);
            
            if (win == WIN_ID)
            {
                m_iLoseCount = 0;
                tm.Find("win").gameObject.SetActive(true);
                text.SetText(pGameContext.GetLocalizingText("MATCHVIEW_RESULT_TXT_WIN"));
                text = m_pResult.Find("root/info/win/box/count").GetComponent<TMPro.TMP_Text>();
                text.SetText($"+{pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.ladderRewardTrophyIncreaseWin)}");

                winStreak = (int)rewardData["winStreak"];
                ui = m_pResult.Find("root/info/winningStreak").GetComponent<RectTransform>();
                if(winStreak > 1)
                {
                    if(winStreak == 2)
                    {
                        pAdRewardItem = pAdRewardList.AdRewardByKey((uint)E_AD.WinStreak);
                        AdRewardDataT pAdRewardData = pGameContext.GetAdRewardDataByID((uint)E_AD.WinStreak);
                        if(pAdRewardData.Amount < pAdRewardItem.Value.MaxAmount)
                        {
                            m_eAdTypeList.Add(E_AD.WinStreak);
                        }
                    }

                    if(m_iMatchType == GameContext.LADDER_ID && m_bReverseMatch)
                    {
                        pAdRewardItem = pAdRewardList.AdRewardByKey((uint)E_AD.ReverseMatch);
                        AdRewardDataT pAdRewardData = pGameContext.GetAdRewardDataByID((uint)E_AD.ReverseMatch);
                        if(pAdRewardData.Amount < pAdRewardItem.Value.MaxAmount)
                        {
                            m_eAdTypeList.Add(E_AD.ReverseMatch);
                        }
                    }
                    
                    winStreak -= 1;
                    ui.gameObject.SetActive(true);
                    text = ui.Find("box/count").GetComponent<TMPro.TMP_Text>();
                    winStreak = winStreak > 5 ? 5 : winStreak;
                    text.SetText($"+{winStreak}");
                }
                else
                {
                    winStreak = 0;
                    ui.gameObject.SetActive(false);
                }

            }
            else if (win == DRAW_ID)
            {
                m_iLoseCount = 0;
                tm.Find("draw").gameObject.SetActive(true);
                text.SetText(pGameContext.GetLocalizingText("MATCHVIEW_RESULT_TXT_DRAW"));
                text = m_pResult.Find("root/info/win/box/count").GetComponent<TMPro.TMP_Text>();
                text.SetText($"+{pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.ladderRewardTrophyIncreaseDraw)}");
            }
            else
            {
                tm.Find("lose").gameObject.SetActive(true);
                ++m_iLoseCount;
                if(m_iLoseCount == 3)
                {
                    pAdRewardItem = pAdRewardList.AdRewardByKey((uint)E_AD.Lose);
                    AdRewardDataT pAdRewardData = pGameContext.GetAdRewardDataByID((uint)E_AD.Lose);
                    if(pAdRewardData.Amount < pAdRewardItem.Value.MaxAmount)
                    {
                        m_eAdTypeList.Add(E_AD.Lose);
                    }
                }
                text.SetText(pGameContext.GetLocalizingText("MATCHVIEW_RESULT_TXT_LOSE"));
                text = m_pResult.Find("root/info/win/box/count").GetComponent<TMPro.TMP_Text>();
                text.SetText($"-{pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.ladderRewardTrophyDecreaseLose)}");
            }

            text = m_pResult.Find("root/info/total/box/count").GetComponent<TMPro.TMP_Text>();
            text.SetText(pGameContext.GetCurrentUserTrophy().ToString());

            Image gauge = text.transform.parent.Find("gauge").GetComponent<Image>();
            RawImage icon = text.transform.parent.Find("rank").GetComponent<RawImage>();
            
            byte rank = pGameContext.GetCurrentUserRank();
            SingleFunc.SetupRankIcon(icon,rank);

            USERRANK.UserRankList pUserRankList = pGameContext.GetFlatBufferData<USERRANK.UserRankList>(E_DATA_TYPE.UserRank);
            USERRANK.UserRankItem? pUserRankItem = pUserRankList.UserRankByKey(rank);
            
            gauge.fillAmount = (pGameContext.GetCurrentUserTrophy() - pUserRankItem.Value.TrophyMin) / (float)(pUserRankItem.Value.TrophyMax - pUserRankItem.Value.TrophyMin);
                    
            m_pRankUp.transform.Find("root").GetComponent<Button>().enabled = true;
            
            m_pRankUp.transform.Find("root/title").gameObject.SetActive(true);
            
            m_pRankUp.transform.Find("root/rank").gameObject.SetActive(true);
            m_pRankUp.transform.Find("root/info").gameObject.SetActive(true);
            m_pRankUp.transform.Find("root/text").gameObject.SetActive(true);

            icon = m_pRankUp.transform.Find("root/rank").GetComponent<RawImage>();
            SingleFunc.SetupRankIcon(icon,rank);

            text = m_pRankUp.transform.Find("root/info/ago").GetComponent<TMPro.TMP_Text>();
            text.SetText(pGameContext.GetLocalizingText($"rank_name_{rank - m_iRankUp}"));
            text = m_pRankUp.transform.Find("root/info/current").GetComponent<TMPro.TMP_Text>();
            text.SetText(rank.ToString());
            text.SetText(pGameContext.GetLocalizingText($"rank_name_{rank}"));

            if(m_iRankUp > 0)
            {
                if( rank == 2 && pGameContext.IsReview((byte)(rank-1)))
                {
                    pGameContext.AddReviewInfo();
                }
                else if( rank == 4 && pGameContext.IsReview((byte)(rank-1)))
                {
                    pGameContext.AddReviewInfo();
                }

                m_pRankUp.transform.Find("root/title/up").gameObject.SetActive(true);
                m_pRankUp.transform.Find("root/title/dn").gameObject.SetActive(false);
                m_pRankUp.transform.Find("root/info/up").gameObject.SetActive(true);
                m_pRankUp.transform.Find("root/info/dn").gameObject.SetActive(false);
                m_pRankUp.transform.Find("root/rankDown").gameObject.SetActive(false);
                m_pRankUp.transform.Find("root/rankUp").gameObject.SetActive(true);
                LocalizingText lt = m_pRankUp.transform.Find("root/title").GetComponent<LocalizingText>();
                lt.Key = "MATCHVIEW_RESULT_TITLE_RANK_UP";
                lt.IsLocalizing = true;
                lt.UpdateLocalizing();
            }
            else if(m_iRankUp < 0)
            {
                m_pRankUp.transform.Find("root/title/up").gameObject.SetActive(false);
                m_pRankUp.transform.Find("root/title/dn").gameObject.SetActive(true);
                m_pRankUp.transform.Find("root/info/up").gameObject.SetActive(false);
                m_pRankUp.transform.Find("root/info/dn").gameObject.SetActive(true);
                m_pRankUp.transform.Find("root/rankUp").gameObject.SetActive(false);
                m_pRankUp.transform.Find("root/rankDown").gameObject.SetActive(true);
                LocalizingText lt = m_pRankUp.transform.Find("root/title").GetComponent<LocalizingText>();
                lt.Key = "MATCHVIEW_RESULT_TITLE_RANK_DOWN";
                lt.IsLocalizing = true;
                lt.UpdateLocalizing();
            }
        }
        else
        {
            m_pResult.Find("root/info").gameObject.SetActive(false);            
        }
    }

    public void SaveRecordMatchData()
    {
        if(m_bSendClearData) return;

        E_APP_MATCH_TYPE eType = E_APP_MATCH_TYPE.APP_MATCH_TYPE_SHUTDOWN;
        if(m_pMatchEngine != null && m_pRecord != null && m_pMatchUpdateState != null)
        {
            m_pRecord.StatisticsRecord = m_pMatchEngine.GetMatchStatisticsData().UnPack();
            int n = 0;
            for(int i =0; i < m_pMatchTeamData.TeamData.Count; ++i)
            {
                for(n = 0; n < m_pMatchTeamData.TeamData[i].LineUp.Count; ++n)
                {
                    m_pRecord.LineUpRecord[i].List[n] = m_pMatchTeamData.TeamData[i].LineUp[n];
                }
                for(n = 0; n < m_pMatchTeamData.TeamData[i].Tactics.Formation.Count; ++n)
                {
                    m_pRecord.LineUpRecord[i].Formation[n] = m_pMatchTeamData.TeamData[i].Tactics.Formation[n];
                }
            }
           
            if(m_bFinish)
            {
                eType = E_APP_MATCH_TYPE.APP_MATCH_TYPE_CLEAR;
            }
        }

        GameContext.getCtx().SaveRecordMatchData(m_pRecord,eType,m_iMatchType);
    }

    public List<PlayerT> GetTotalPlayerList(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.match)
        {
            return GameContext.getCtx().GetMatchAwayTotalPlayerList();
        }
        else if(eType == E_PLAYER_INFO_TYPE.away)
        {
            return GameContext.getCtx().GetMatchAwayTotalPlayerList();
        }
        else
        {
            return GameContext.getCtx().GetTotalPlayerList();
        }
    }
    
    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        ulong id = 0;
        if(ulong.TryParse(tm.gameObject.name,out id))
        {
            PauseMatch();
            PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
            E_PLAYER_INFO_TYPE eTeamType = m_eCurrentTab == E_TAB.homePlayer ? E_PLAYER_INFO_TYPE.my : E_PLAYER_INFO_TYPE.match;
            pPlayerInfo.SetupPlayerInfoData(eTeamType,GetPlayerByID(eTeamType,id),ResumeMatch);
            pPlayerInfo.SetupQuickPlayerInfoData(GetTotalPlayerList(eTeamType));
            pPlayerInfo.MoveQuickPlayer();
            pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
        }  
    }

    public void BackButton()
    {
        if(m_pLineupUI.gameObject.activeSelf)
        {
            if(!StateMachine.GetStateMachine().IsCurrentTargetStates(m_pLineupUI.Find("away").GetComponent<RectTransform>()))
            {
                ButtonEventCall(m_pLineupUI,null);
            }
        }
        else
        {
            if(!m_bFinish)
            {
                SetViewSpeed( IsPauseMatchButton() == true ? (byte)1 : (byte)3,true);
            }
            else
            {
                Animation pAnimation = LayoutManager.Instance.FindUIFormRoot<Animation>("UI_Wait");
                if(!pAnimation.gameObject.activeSelf)
                {
                    Button exit = m_pInfoUI.Find("command/finish/exit").GetComponent<Button>();
                    
                    if(exit.gameObject.activeSelf && exit.enabled)
                    {
                        ButtonEventCall(m_pInfoUI,exit.gameObject);
                    }
                    else
                    {
                        if(!StateMachine.GetStateMachine().IsCurrentTargetStates(m_pResult,(uint)E_STATE_TYPE.ShowDailog))
                        {
                            ButtonEventCall(m_pResult,null);
                        }
                    }
                }
            }
        }
    }

    void BroadCastTextClear()
    {
        m_pBroadCastTextList.Clear();
        List<BaseState> statelist = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pBroadCastText,(uint)E_STATE_TYPE.Timer);

        for(int i =0; i < statelist.Count; ++i)
        {
            statelist[i].Exit(true);
        }
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(root == null || sender == null) return;
        
        if(root == m_pInfoUI)
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
                }
                break;
                case "tatics":
                {
                    if(m_bFinish)
                    {
                        return;
                    }
                    
                    if(m_pMainScene.GetInstance<TacticsFormation>().IsChangeSubstitution())
                    {
                        m_pMainScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("MSG_TXT_MATCH_TACTICS_ADJUSTMENT_WAITING"),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
                    }
                    else
                    {
                        PauseMatch();
                        m_pMainScene.ShowTacticsFormation(m_pMatchTeamData.TeamData[0]);
                        m_pMainScene.SetMainButton(2);
                    }
                }
                break;
                case "exit": 
                {
                    sender.GetComponent<Button>().enabled = false;
                    
                    LayoutManager.Instance.InteractableDisableAll();
                    BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pResult),0.3f, (uint)E_STATE_TYPE.ShowDailog, this.enterDilogMoveCallback, this.executeDilogMoveCallback);
                    DilogMoveStateData data = new DilogMoveStateData();
                    data.FadeDelta = 0f;
                    data.Out = false;
                    pBaseState.StateData = data;
                    StateMachine.GetStateMachine().AddState(pBaseState);
                }
                break;
                case "finish": 
                {
                    if(m_pMatchUpdateState != null && m_fTotalGameTime < 80)
                    {
                        GameContext pGameContext = GameContext.getCtx();
                        ulong itemCount = pGameContext.GetItemCountByNO(GameContext.MATCH_SKIP_ID);
                        if(itemCount <= 0)
                        {
                            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("ALERT_TXT_MATCH_NO_MATCH_SKIP_TICKET"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                            return;
                        }

                        BroadCastTextClear();
                        Button pButton = sender.GetComponent<Button>();   
                        m_pMatchUpdateState.Paused = true;
                        m_pPauseText.gameObject.SetActive(true);
                        E_REQUEST_ID eReq = E_REQUEST_ID.league_skipMatch;
                        if(m_iMatchType == GameContext.LADDER_ID)
                        {
                            eReq = E_REQUEST_ID.ladder_skipMatch;
                        }
                        else if(m_iMatchType == GameContext.CHALLENGE_ID)
                        {
                            eReq = E_REQUEST_ID.challengeStage_skipMatch;
                        }

                        m_pMainScene.ShowConfirmPopup( pGameContext.GetLocalizingText("DIALOG_MATCHSKIP_TITLE"),string.Format(pGameContext.GetLocalizingText("DIALOG_MATCHSKIP_TXT"),itemCount),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,() =>{
                            SetViewSpeed(3,false);
                            pButton.enabled = false;
                            JObject data = new JObject();
                            data["match"] = pGameContext.GetCurrentMatchID();
                            m_pMainScene.RequestAfterCall(eReq,data,FinishGame);
                            pGameContext.SendAdjustEvent("f26584",false,false,-1);
                            pGameContext.SendAdjustEvent("ad0n5r",true,true,-1);
                        },() =>{
                            if(!IsPauseMatchButton())
                            {
                                m_pMatchUpdateState.Paused = false;
                                m_pPauseText.gameObject.SetActive(false);
                            }
                        } );
                    }
                }   
                break;
                case "summary":
                case "comparison":
                case "homePlayer":
                case "awayPlayer":
                {
                    ShowTabUI((E_TAB)Enum.Parse(typeof(E_TAB), sender.name));
                }
                break;
            }
        }
        else if(root == m_pLineupUI)
        {
            sender.SetActive(false);
            if(sender.name == "play")
            {
                LayoutManager.Instance.InteractableEnabledAll();
                StartMatch();
                DilogMoveStateData data = new DilogMoveStateData();
                data.FadeDelta = 1 / 0.3f;
                data.Out = m_pViewSetting.gameObject.activeSelf;
                m_pViewSetting.gameObject.SetActive(true);
                ALFUtils.FadeObject(m_pViewSetting,data.Out ? 1 : -1);
                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pViewSetting),0.3f, (uint)E_STATE_TYPE.ShowDailog, null, this.executeFadeDilogCallback,this.exitFadeDilogCallback);
                pBaseState.StateData = data;
                StateMachine.GetStateMachine().AddState(pBaseState);
            }
            else
            {
                PlayLineupAnimation(false);
            }
        }
        else if(root == m_pViewUI)
        {
            switch(sender.name)
            {
                case "ground":
                {
                    ResetTimer(false);
                    DilogMoveStateData data = new DilogMoveStateData();
                    data.FadeDelta = 1 / 0.3f;
                    data.Out = m_pViewSetting.gameObject.activeSelf;
                    sender.GetComponent<Button>().enabled = false;
                    m_pViewSetting.gameObject.SetActive(true);
                    ALFUtils.FadeObject(m_pViewSetting,data.Out ? 1 : -1);
                    BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pViewSetting),0.3f, (uint)E_STATE_TYPE.ShowDailog, null, this.executeFadeDilogCallback,this.exitFadeDilogCallback);
                    pBaseState.StateData = data;
                    StateMachine.GetStateMachine().AddState(pBaseState);
                }
                break;
                case "help":
                {
                    PauseMatch();
                    m_pMainScene.ShowMatchHelpPopup();
                }
                break;
                case "full":
                {
                    ResetTimer(true);
                    m_bFullView = !m_bFullView;
                    UpdateFullView();
                }
                break;
                case "view":
                {
                    ResetTimer(true);
                    byte eViewMode = GameContext.getCtx().MatchViewMode();
                    ++eViewMode;
                    if(eViewMode > 3)
                    {
                        eViewMode =1;
                    }
                    GameContext.getCtx().SetMatchViewMode(eViewMode);
                    UpdateViewMode();
                }
                break;
                case "speed":
                {
                    SetViewSpeed((byte)(GameContext.getCtx().MatchViewSpeed() +1),true);
                }
                break;
                #if !FTM_LIVE
                case "x10":
                {
                    SetViewSpeed(10,true);
                }
                break;
                #endif
            }
        }
        else if(root == m_pResult)
        {
            BroadCastTextClear();
            if(m_eAdTypeList.Count > 0)
            {
                AdRewardList pAdRewardList = GameContext.getCtx().GetFlatBufferData<AdRewardList>(E_DATA_TYPE.AdReward);  
                AdRewardItem? pAdRewardItem = pAdRewardList.AdRewardByKey((uint)m_eAdTypeList[0]);
                AdRewardItem? pTem = null;
                for(int i =1; i < m_eAdTypeList.Count; ++i)
                {
                    pTem = pAdRewardList.AdRewardByKey((uint)m_eAdTypeList[i]);
                    if( pAdRewardItem.Value.Order > pTem.Value.Order)
                    {
                        pAdRewardItem = pTem;
                    }
                }

                m_pMainScene.ShowADPopup(pAdRewardItem.Value.No);
                m_eAdTypeList.Clear();
            }
            else
            {
                m_pMatchEngine?.Dispose();
                m_pMatchEngine = null;
                m_pMatchTeamData = null;
                ClearScroll(E_SCROLL.timeline);
                ClearScroll(E_SCROLL.log);
                
                m_pResult.gameObject.SetActive(false);

                if(m_iRankUp != 0)
                {
                    m_pRankUp.gameObject.SetActive(true);

                    m_pRankUp.transform.Find("root/rankNotice").gameObject.SetActive(false);
                    if(m_iRankUp > 0)
                    {
                        ParticleSystem pParticleSystem = m_pRankUp.transform.Find("root/rankUp/FX_2").GetComponent<ParticleSystem>();
                        pParticleSystem.time = 0;
                        pParticleSystem.Play(true);
                        UnityEngine.UI.Extensions.UIParticleSystem[] list = pParticleSystem.GetComponentsInChildren<UnityEngine.UI.Extensions.UIParticleSystem>(true);
                        for(int i =0; i < list.Length; ++i)
                        {
                            list[i].StartParticleEmission();
                        }
                        m_pRankUp.Play("rankUp");
                        m_iRankUpSFX = SoundManager.Instance.PlaySFX("sfx_rankup");
                    }
                }
                else
                {
                    Close();
                }
            }
        }
        else if(root == m_pRankUp.transform)
        {
            GameContext pGameContext = GameContext.getCtx();
            if(sender.name == "confirm")
            {
                if(m_iRankUp > 0 && m_iMatchType == GameContext.LADDER_ID && m_eCurrentRank == pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.beginnerBuffUserRank))
                {
                    m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TITLE_BEGINNER_BUFF_END"),pGameContext.GetLocalizingText("MSG_TXT_BEGINNER_BUFF_END"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"),null,false,Close);
                }
                else
                {
                    Close();
                }
            }
            else
            {
                if(m_pRankUp.IsPlaying("rankUp"))
                {
                    m_pRankUp["rankUp"].time = m_pRankUp["rankUp"].length;
                    if(m_iRankUp > 0 && GameContext.getCtx().GetCurrentUserRank() == 2)
                    {
                        m_pRankUp.Play("rankNotice");
                    }
                }
                else
                {
                    if(m_iRankUp > 0 && GameContext.getCtx().GetCurrentUserRank() == 2)
                    {
                        m_pRankUp.Play("rankNotice");
                    }
                    else
                    {
                        if(m_iRankUp > 0 && m_iMatchType == GameContext.LADDER_ID && m_eCurrentRank == pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.beginnerBuffUserRank))
                        {
                            m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TITLE_BEGINNER_BUFF_END"),pGameContext.GetLocalizingText("MSG_TXT_BEGINNER_BUFF_END"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"),null,false,Close);
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
            }
        }
    }
    
    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
        if( data == null) return;
        E_REQUEST_ID eID = (E_REQUEST_ID)data.Id;

        if(eID == E_REQUEST_ID.ladder_try || eID == E_REQUEST_ID.challengeStage_try|| eID == E_REQUEST_ID.league_try)
        {
            SaveRecordMatchData();
        }
        else if(eID == E_REQUEST_ID.ladder_clear || eID == E_REQUEST_ID.challengeStage_clear|| eID == E_REQUEST_ID.league_clear)
        {
            m_bSendClearData = true;
            if(m_pRecord != null && m_pRecord.StatisticsRecord != null)
            {
                m_pMainScene.GetInstance<TacticsFormation>().OnChangeSubstitution(false);
                GameContext pGameContext = GameContext.getCtx();
                pGameContext.AddMatchLogData(m_pRecord);
                pGameContext.SetPlayerHP(m_pRecord.StatisticsRecord.StatisticsPlayers[0]);
            }
            
            if(!MainUI.gameObject.activeInHierarchy) return;

            SetupRewardData(data.Json);
            m_pInfoUI.Find("command/finish/finish").gameObject.SetActive(false);
            m_pInfoUI.Find("command/finish/exit").gameObject.SetActive(true);
        }
    }

}
