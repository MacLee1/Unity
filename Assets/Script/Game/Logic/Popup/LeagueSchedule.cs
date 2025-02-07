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
using STATISTICSDATA;
using Newtonsoft.Json.Linq;
using LADDERSEASONNO;

public class LeagueSchedule : IBaseUI
{
    ulong m_ulId = 0;

    enum E_TAB : byte { day1 = 1, day2 =2,day3,day4,day5,day6,day7, MAX}

    private class LeagueMatchRecord : IBase
    {
        public byte Slot {get; private set;}

        public E_TAB Day {get; private set;}
        public long StartTime {get; private set;} 
        public ulong ID {get; private set;} 
        public int Status {get; private set;}
        public uint SeasonNo {get; private set;}
        public byte MatchType {get; private set;}
        public byte HomeGoals {get; private set;}
        public byte AwayGoals {get; private set;}
        public ulong AwayID {get; private set;}
        public ulong HomeID {get; private set;}
        public string Name {get; private set;}
        public int SquadPower {get; private set;}

        byte[] m_pEmblemInfo = SingleFunc.CreateRandomEmblem();
        
        public LeagueMatchRecord(JObject data)
        {
            Day = (E_TAB)((byte)data["round"]);
            ID = (ulong)data["match"];   
            SquadPower = (int)data["squadPower"];
            Status = (int)data["status"];
            SeasonNo = (uint)data["seasonNo"];
            MatchType = (byte)data["matchType"];
            
            StartTime = ALF.NETWORK.NetworkManager.ConvertLocalGameTimeTick(data["tStart"].ToString());
            
            AwayID = (ulong)data["away"];
            HomeID = (ulong)data["home"];
            HomeGoals = (byte)data["homeGoals"];
            AwayGoals = (byte)data["awayGoals"];
            
            Name = (string)data["name"];

            if(data.ContainsKey("emblem") && data["emblem"].Type != JTokenType.Null)
            {
                m_pEmblemInfo = SingleFunc.GetMakeEmblemData((string)data["emblem"]);
            }
        }
        public byte[] GetEmblemInfo()
        {
            return m_pEmblemInfo;
        }
        public void StartMatch()
        {
            Status = 1;
        }
        public void Dispose()
        {
            m_pEmblemInfo = null;
        }
    }

    private class LeagueScheduleItem : IBase
    {
        public ulong ID {get; private set;} 
        public byte MatchType {get; private set;} 
        public ulong Club {get; private set;} 
        public Transform Target  {get; private set;}
        public TMPro.TMP_Text Name {get; private set;}
        public Button Info {get; private set;}
        public Button Log {get; private set;}
        public Button Match {get; private set;}
        public TMPro.TMP_Text ScoreText {get; private set;}
        public GameObject Wait {get; private set;}
        public EmblemBake Emblem {get; private set;}
        
        byte[] m_scores = new byte[2];
        public LeagueScheduleItem( Transform taget)
        {
            Target = taget;
            Name = Target.Find("name").GetComponent<TMPro.TMP_Text>();            
            ScoreText = Target.Find("score/text").GetComponent<TMPro.TMP_Text>();
            Emblem = Target.Find("emblem").GetComponent<EmblemBake>(); 
            Log = Target.Find("log").GetComponent<Button>();
            Transform tm = Target.Find("match");
            if(tm != null)
            {
                Match = tm.GetComponent<Button>();
            }
            else
            {
                Match = null;
            }

            tm = Target.Find("wait");
            if(tm != null)
            {
                Wait = tm.gameObject;
            }
            else
            {
                Wait = null;
            }

            
            Info = Target.GetComponent<Button>(); 
        }

        public void SetupData(LeagueMatchRecord data,E_TAB eDay)
        {
            MatchType = data.MatchType;
            ID = data.ID;
            bool bMy = data.HomeID == GameContext.getCtx().GetClubID();
            if(bMy)
            {
                Club = data.AwayID;
            }
            else
            {
                Club = data.HomeID;
            }

            Name.SetText(data.Name);
            m_scores[0]= 0;
            m_scores[1]= 0;
            if(data.Status == 0)
            {
                ScoreText.SetText("VS");
                if(Match != null)
                {
                    Match.gameObject.SetActive(true);
                    Match.enabled = eDay == data.Day;
                    Match.transform.Find("on").gameObject.SetActive(Match.enabled);
                    Match.transform.Find("off").gameObject.SetActive(!Match.enabled);
                }

                if(Wait != null)
                {
                    Wait.SetActive(true);
                }
                
                Log.gameObject.SetActive(false);
            }
            else
            {   
                Log.gameObject.SetActive(true);

                if(Match != null)
                {
                    Match.gameObject.SetActive(false);
                }

                if(Wait != null)
                {
                    Wait.SetActive(false);
                }
                m_scores[0]= data.HomeGoals;
                m_scores[1]= data.AwayGoals;

                byte homeGoals = data.HomeGoals;
                byte awayGoals = data.AwayGoals;
                
                if(!bMy)
                {
                    homeGoals = data.AwayGoals;
                    awayGoals = data.HomeGoals;
                }
                
                ScoreText.SetText( $"{homeGoals}:{awayGoals}");
            }

            Emblem.Dispose();
            Emblem.SetupEmblemData(data.GetEmblemInfo());
        }

        public byte GetScore(int index)
        {
            return m_scores[index];
        }

        public void Dispose()
        {
            m_scores = null;
            Info = null;
            Log = null;
            Match = null;
            Wait = null;
            Name = null;
            ScoreText = null;
            Target = null;
            Emblem.Dispose();
            Emblem = null;
        }
    }

    Button[] m_pTabUIList = new Button[(int)E_TAB.MAX];
    LeagueScheduleItem[] m_pHomeList = new LeagueScheduleItem[5];
    LeagueScheduleItem[] m_pAwayList = new LeagueScheduleItem[5];

    Dictionary<E_TAB,List<LeagueMatchRecord>>  m_pLeagueMatchHomeRecordList = new Dictionary<E_TAB,List<LeagueMatchRecord>>();
    Dictionary<E_TAB,List<LeagueMatchRecord>>  m_pLeagueMatchAwayRecordList = new Dictionary<E_TAB,List<LeagueMatchRecord>>();
    
    E_TAB m_eCurrentDay = E_TAB.day1;
    E_TAB m_eTab = E_TAB.day1;
    MainScene m_pMainScene = null;
    string m_pAwayName = null;
    string m_pHomeName = null;
    string m_pScore = null;
    public RectTransform MainUI { get; private set;}

    public LeagueSchedule(){}
    
    public void Dispose()
    {
        ClearData();
        m_pLeagueMatchHomeRecordList = null;
        m_pLeagueMatchAwayRecordList = null;
        
        int i =0;
        for(i =0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i] = null;
        }

        for(i =0; i < m_pHomeList.Length; ++i)
        {
            m_pHomeList[i].Dispose();
            m_pAwayList[i].Dispose();
            m_pHomeList[i] = null;
            m_pAwayList[i] = null;
        }

        m_pHomeList = null;
        m_pAwayList = null;
        m_pTabUIList = null;
        m_pMainScene = null;
        MainUI = null;
    }
    
    void ClearData()
    {
        var itr = m_pLeagueMatchAwayRecordList.GetEnumerator();
        int i =0;
        while(itr.MoveNext())
        {
            for(i =0; i < itr.Current.Value.Count; ++i)
            {
                itr.Current.Value[i].Dispose();
            }
            itr.Current.Value.Clear();
        }
        m_pLeagueMatchAwayRecordList.Clear();

        itr = m_pLeagueMatchHomeRecordList.GetEnumerator();
        while(itr.MoveNext())
        {
            for(i =0; i < itr.Current.Value.Count; ++i)
            {
                itr.Current.Value[i].Dispose();
            }
            itr.Current.Value.Clear();
        }
        m_pLeagueMatchHomeRecordList.Clear();
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {        
        ALFUtils.Assert(pBaseScene != null, "LeagueSchedule : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "LeagueSchedule : targetUI is null!!");
        
        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        
        RectTransform ui = MainUI.Find("root/bg/tabs").GetComponent<RectTransform>();
        float w = (ui.rect.width / ui.childCount);
        float wh = w * 0.5f;
        float ax = ui.pivot.x * ui.rect.width;
        
        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        int iTabIndex = -1;
        int n =0;
        
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabUIList[iTabIndex] = item.GetComponent<Button>();
            item.Find("on").gameObject.SetActive(false);
            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);

        ui = MainUI.Find("root/bg/home").GetComponent<RectTransform>();
        for(n = 1; n < 6; ++n)
        {
            m_pHomeList[n-1] = new LeagueScheduleItem(ui.Find(n.ToString()));
            m_pHomeList[n-1].Target.gameObject.SetActive(false);
        }
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);

        ui = MainUI.Find("root/bg/away").GetComponent<RectTransform>();
        for(n = 1; n < 6; ++n)
        {
            m_pAwayList[n-1] = new LeagueScheduleItem(ui.Find(n.ToString()));
            m_pAwayList[n-1].Target.gameObject.SetActive(false);
        }
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);        
        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("root/bg/close").GetComponent<RectTransform>(),ButtonEventCall);

        MainUI.gameObject.SetActive(false);
    }

    public void SetupData(JObject data)
    {
        ClearData();
        JArray pJArray = null;
        JObject item = null;
        GameContext pGameContext = GameContext.getCtx();
        ulong myID = pGameContext.GetClubID();

        if(data.ContainsKey("matches") && data["matches"].Type != JTokenType.Null)
        {
            pJArray = (JArray)data["matches"];
            E_TAB eDay = E_TAB.day1;
            List<LeagueMatchRecord> list = null;
            int iFailCount = 0;
            for(int i =0; i < pJArray.Count; ++i)
            {
                item = (JObject)pJArray[i];
                eDay = (E_TAB)(byte)item["round"];

                if((ulong)item["home"] == myID)
                {
                    if((int)item["status"] == 1)
                    {
                        item["status"] = 99;
                        JObject pObject = pGameContext.MakeMatchStatsData(E_APP_MATCH_TYPE.APP_MATCH_TYPE_GIVEUP,null,pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType()));
                        pObject["match"] = (ulong)item["match"];
                        pObject["matchType"] = (int)item["matchType"];
                        m_pMainScene.RequestAfterCall(E_REQUEST_ID.league_clear,pObject);
                        ++iFailCount;
                    }

                    if(m_pLeagueMatchHomeRecordList.ContainsKey(eDay))
                    {
                        list = m_pLeagueMatchHomeRecordList[eDay];
                    }
                    else
                    {
                        list = new List<LeagueMatchRecord>();
                        m_pLeagueMatchHomeRecordList.Add(eDay,list);
                    }

                    list.Add(new LeagueMatchRecord(item));
                }
                else
                {
                    if(m_pLeagueMatchAwayRecordList.ContainsKey(eDay))
                    {
                        list = m_pLeagueMatchAwayRecordList[eDay];
                    }
                    else
                    {
                        list = new List<LeagueMatchRecord>();
                        m_pLeagueMatchAwayRecordList.Add(eDay,list);
                    }

                    list.Add(new LeagueMatchRecord(item));
                }
            }
        
            if(iFailCount > 0)
            {
                pJArray = (JArray)data["standings"];
                for(int i =0; i < pJArray.Count; ++i)
                {
                    item = (JObject)pJArray[i];
                    if((ulong)item["club"] == myID)
                    {
                        item["game"] = (int)item["game"] + iFailCount;
                        item["lose"] = (int)item["lose"] + iFailCount;
                        break;
                    }
                }
            }
        }

        SetToday();
    }

    void SetToday()
    {
        m_eCurrentDay = E_TAB.day1;
        GameContext pGameContext = GameContext.getCtx();
        LadderSeasonNoList pLadderSeasonNoList = pGameContext.GetFlatBufferData<LadderSeasonNoList>(E_DATA_TYPE.LadderSeasonNo);
        LadderSeasonNoItem? pLadderSeasonNoItem = pLadderSeasonNoList.LadderSeasonNoByKey(pGameContext.GetCurrentSeasonNo());
        if(pLadderSeasonNoItem != null)
        {
            DateTime pSeason = DateTime.Parse(pLadderSeasonNoItem.Value.TStart);
            TimeSpan pTimeSpan = NetworkManager.GetGameServerTime() - pSeason;
            if(pTimeSpan.TotalDays < 7)
            {
                m_eCurrentDay += (byte)pTimeSpan.TotalDays;
            }
        }
    }

    public void SetupToday()
    {
        SetToday();
        ShowTabUI(m_eCurrentDay);
    }

    void ShowTabUI(E_TAB eTab)
    {
        int i = 0;
        
        for(i = 0; i < m_pAwayList.Length; ++i)
        {
            m_pAwayList[i].Target.gameObject.SetActive(false);
            m_pHomeList[i].Target.gameObject.SetActive(false);
        }

        int index = (int)eTab;
        m_eTab = eTab;
        for(i = 0; i < m_pTabUIList.Length; ++i)
        {
            if(m_pTabUIList[i] != null)
            {
                m_pTabUIList[i].enabled = index != i;
                m_pTabUIList[i].transform.Find("on").gameObject.SetActive(!m_pTabUIList[i].enabled);
                // m_pTabUIList[i].transform.Find("title").GetComponent<Graphic>().color = !m_pTabUIList[i].enabled ? Color.white : GameContext.GRAY;
            }
        }
        
        List<LeagueMatchRecord> list = m_pLeagueMatchHomeRecordList[m_eTab];
        for(i =0; i <list.Count; ++i)
        {
            m_pHomeList[i].Target.gameObject.SetActive(true);
            m_pHomeList[i].SetupData(list[i],m_eCurrentDay);
        }

        list = m_pLeagueMatchAwayRecordList[m_eTab];
        for(i =0; i <list.Count; ++i)
        {
            m_pAwayList[i].Target.gameObject.SetActive(true);
            m_pAwayList[i].SetupData(list[i],m_eCurrentDay);
        }
    }

    void ShowClubProfileOverview()
    {
        m_pMainScene.ShowUserProfile(m_ulId,0);
        m_ulId = 0;
    }

    public void FailChallenge()
    {
        Close();
    }

    void TryChallengeStage()
    {
        Close();
        m_pMainScene.ShowMatchPopup(GameContext.CHALLENGE_ID);
    }

    void ShowMatchDetailLog()
    {
        m_pMainScene.ShowMatchDetailLogPopup(null,m_pHomeName,m_pAwayName,m_pScore);
        m_pAwayName = null;
        m_pHomeName = null;
        m_pScore = null;
    }
    void TryLeague()
    {
        GameContext.getCtx().LeagueTodayMatch();
        m_pMainScene.UpdateLeagueTodayCount();
        Close();
        m_pMainScene.ShowMatchPopup(GameContext.getCtx().GetCurrentMatchType());
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI,()=>{
            LayoutManager.Instance.InteractableEnabledAll();
            MainUI.Find("root").localScale = Vector3.one;
            MainUI.gameObject.SetActive(false);
        });
    }

    public void StartMatch(ulong id)
    {
        List<LeagueMatchRecord> list = m_pLeagueMatchHomeRecordList[m_eCurrentDay];
        for(int i =0; i < list.Count; ++i)
        {
            if(list[i].ID == id)
            {
                list[i].StartMatch();
                break;
            }
        }
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(root.gameObject.name == "tabs")
        {
            ShowTabUI((E_TAB)Enum.Parse(typeof(E_TAB), sender.name));
        }
        else if(root.gameObject.name == "close")
        {
            Close();
        }
        else 
        {
            LeagueScheduleItem pLeagueScheduleItem = null;

            if(root.gameObject.name == "home")
            {   
                for(int i =0; i < m_pHomeList.Length; ++i)
                {
                    if(m_pHomeList[i].Info.gameObject == sender || m_pHomeList[i].Match.gameObject == sender || m_pHomeList[i].Log.gameObject == sender)
                    {
                        pLeagueScheduleItem = m_pHomeList[i];
                        break;
                    }
                }
            }
            else if(root.gameObject.name == "away")
            {
                for(int i =0; i < m_pAwayList.Length; ++i)
                {
                    if(m_pAwayList[i].Info.gameObject == sender || m_pAwayList[i].Log.gameObject == sender)
                    {
                        pLeagueScheduleItem = m_pAwayList[i];
                        break;
                    }
                }
            }

            if(pLeagueScheduleItem != null)
            {
                if(sender.name == "match")
                {
                    GameContext pGameContext = GameContext.getCtx();
                    ulong id = pLeagueScheduleItem.ID;
                    m_pMainScene.ShowConfirmPopup( pGameContext.GetLocalizingText("DIALOG_LEAGUE_QUICK_MATCH_TITLE"),string.Format(pGameContext.GetLocalizingText("DIALOG_LEAGUE_QUICK_MATCH_TXT"),pLeagueScheduleItem.Name.text),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,() =>{
                        JObject data = new JObject();
                        data["match"] = id;
                        StartMatch(id);
                        m_pMainScene.RequestAfterCall(E_REQUEST_ID.league_try,data,TryLeague);
                    });
                }
                else if(sender.name == "log")
                {
                    if(root.gameObject.name == "home")
                    {
                        m_pAwayName = pLeagueScheduleItem.Name.text;
                        m_pHomeName = GameContext.getCtx().GetClubName();
                    }
                    else
                    {
                        m_pHomeName = pLeagueScheduleItem.Name.text;
                        m_pAwayName = GameContext.getCtx().GetClubName();
                    }
                    
                    m_pScore = $"{pLeagueScheduleItem.GetScore(0)}:{pLeagueScheduleItem.GetScore(1)}"; 

                    JObject pJObject = new JObject();
                    pJObject["match"] = pLeagueScheduleItem.ID;
                    pJObject["matchType"] = pLeagueScheduleItem.MatchType;
                    m_pMainScene.SetAwayEmblem(pLeagueScheduleItem.Emblem);

                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.matchStats_getSummary,pJObject,ShowMatchDetailLog);
                }
                else
                {
                    m_ulId = pLeagueScheduleItem.Club;
                    m_pMainScene.RequestClubProfile(m_ulId,ShowClubProfileOverview);
                }
            }
        }
    }
    
}
