using System;
using System.Linq;
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

public class MatchLog : IBaseUI
{
    const string SCROLL_ITEM_NAME = "MatchLogItem";

    private class LogMatchData : IBase
    {
        public ulong ID {get; private set;} 
        
        public ulong HomeId {get; private set;}
        public ulong AwayId {get; private set;}
        public bool League {get; private set;}
        public uint HomeGoals {get; private set;}
        public uint AwayGoals {get; private set;}
        public string Name {get; private set;}

        public uint  SquadPower {get; private set;}
        public uint  AwayTrophy {get; private set;}
        public int  StartT {get; private set;}
        

        byte[] m_pEmblemInfo = SingleFunc.CreateRandomEmblem();

        public LogMatchData( JObject data)
        {
            ID = (ulong)data["match"];
            League = (int)data["matchType"] > GameContext.LADDER_ID;

            GameContext pGameContext = GameContext.getCtx();
            
            if(data.ContainsKey("home"))
            {
                HomeId = (ulong)data["home"];
            }
            else
            {
                HomeId = pGameContext.GetClubID();
            }

            if(League)
            {
                SquadPower = (uint)data["squadPower"];
            }
            else
            {
                AwayTrophy = (uint)data["awayTrophy"];
            }

            AwayId = (ulong)data["away"];
            HomeGoals = (uint)data["homeGoals"];
            AwayGoals = (uint)data["awayGoals"];
            Name = (string)data["name"];
            
            StartT = (int)((NetworkManager.GetGameServerTime().Ticks - ALF.NETWORK.NetworkManager.ConvertLocalGameTimeTick(data["tStart"].ToString())) / (float)TimeSpan.TicksPerSecond);

            if(data.ContainsKey("emblem") && data["emblem"].Type != JTokenType.Null)
            {
                m_pEmblemInfo = SingleFunc.GetMakeEmblemData((string)data["emblem"]);
            }
        }

        public void Dispose()
        {
            m_pEmblemInfo = null;
        }

        public void SetupEmblemData(EmblemBake pEmblemBake )
        {
            if(pEmblemBake == null) return;
            pEmblemBake.SetupEmblemData(m_pEmblemInfo);
        }
    }

    private class MatchLogItem : IBase
    {
        public ulong ID {get; private set;} 
        public RectTransform Target  {get; private set;}
        public EmblemBake Emblem {get; private set;}
        
        public GameObject[] Wins {get; private set;}
        public GameObject[] Bgs {get; private set;}
        public TMPro.TMP_Text WinText {get; private set;}
        public TMPro.TMP_Text NameText {get; private set;}
        public TMPro.TMP_Text TimeText {get; private set;}
        public TMPro.TMP_Text OverText {get; private set;}
        public TMPro.TMP_Text TrophyText {get; private set;}
        public GameObject[] Status {get; private set;}
        public TMPro.TMP_Text[] ScoreTexts {get; private set;}

        public GameObject Detail {get; private set;}
        Button m_pDetailButton = null;
        Button m_pClickButton = null;
        
        public MatchLogItem( RectTransform taget)
        {
            Target = taget;

            WinText = taget.Find("win/text").GetComponent<TMPro.TMP_Text>();
            Wins = new GameObject[3];
            Wins[0] = taget.Find("win/win").gameObject;
            Wins[1] = taget.Find("win/draw").gameObject;
            Wins[2] = taget.Find("win/lose").gameObject;
            Bgs = new GameObject[3];
            Bgs[0] = taget.Find("bg/win").gameObject;
            Bgs[1] = taget.Find("bg/draw").gameObject;
            Bgs[2] = taget.Find("bg/lose").gameObject;
            Status = new GameObject[2];
            Status[0] = taget.Find("status/home").gameObject;
            Status[1] = taget.Find("status/away").gameObject;
            NameText = taget.Find("name").GetComponent<TMPro.TMP_Text>();
            TimeText = taget.Find("time").GetComponent<TMPro.TMP_Text>();
            OverText = taget.Find("over/text").GetComponent<TMPro.TMP_Text>();
            TrophyText = taget.Find("trophy/text").GetComponent<TMPro.TMP_Text>();

            ScoreTexts = new TMPro.TMP_Text[2];
            ScoreTexts[0] = taget.Find("score/h").GetComponent<TMPro.TMP_Text>();
            ScoreTexts[1] = taget.Find("score/a").GetComponent<TMPro.TMP_Text>();

            m_pDetailButton = taget.Find("detail").GetComponent<Button>();
            m_pClickButton = taget.Find("click").GetComponent<Button>();

            Emblem = taget.Find("emblem").GetComponent<EmblemBake>();
            Detail = m_pDetailButton.gameObject;
        }

        public void Dispose()
        {
            int i =0;
            for(i=0; i < Wins.Length; ++i)
            {
                Wins[i] = null;
                Bgs[i] = null;
            }
            Wins = null;
            Bgs = null;
            Detail = null;
            for(i=0; i < Status.Length; ++i)
            {
                Status[i] = null;
            }
            Status = null;
            WinText = null;
            NameText = null;
            TimeText = null;
            OverText = null;
            TrophyText = null;
            for(i=0; i < ScoreTexts.Length; ++i)
            {
                ScoreTexts[i] = null;
            }

            m_pDetailButton.onClick.RemoveAllListeners();
            m_pDetailButton = null;
            m_pClickButton.onClick.RemoveAllListeners();
            m_pClickButton = null;

            Emblem.Dispose();
            Emblem = null;
            
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,Target);
            Target = null;
        }

        public void UpdateInfo(LogMatchData pLogMatchData)
        {
            if(pLogMatchData == null) return;

            ID = pLogMatchData.ID;

            Status[0].transform.parent.gameObject.SetActive(pLogMatchData.League);
            OverText.transform.parent.gameObject.SetActive(pLogMatchData.League);
            TrophyText.transform.parent.gameObject.SetActive(!pLogMatchData.League);

            GameContext pGameContext = GameContext.getCtx();
            
            bool bHome = true;
            if(pLogMatchData.League)
            {
                bHome = pLogMatchData.HomeId == pGameContext.GetClubID();
                if(bHome)
                {
                    Status[0].SetActive(true);
                    Status[1].SetActive(false);
                }
                else
                {
                    Status[0].SetActive(false);
                    Status[1].SetActive(true);
                }
    
                OverText.SetText(string.Format("{0} : {1}",pGameContext.GetLocalizingText("CHALLENGESTAGE_MATCH_TXT_OVERALL"),pLogMatchData.SquadPower));
            }
            else
            {
                TrophyText.SetText(pLogMatchData.AwayTrophy.ToString());
            }

            string token = null;
            int index =0;
            
            if(bHome)
            {
                if(pLogMatchData.HomeGoals > pLogMatchData.AwayGoals)
                {
                    index =0;
                    token = "MATCHLOG_TXT_WIN";
                }
                else if(pLogMatchData.HomeGoals < pLogMatchData.AwayGoals)
                {
                    index =2;
                    token = "MATCHLOG_TXT_LOSE";
                }
                else
                {
                    index =1;
                    token = "MATCHLOG_TXT_DRAW";
                }
            }
            else
            {
                if(pLogMatchData.HomeGoals < pLogMatchData.AwayGoals)
                {
                    index =0;
                    token = "MATCHLOG_TXT_WIN";
                }
                else if(pLogMatchData.HomeGoals > pLogMatchData.AwayGoals)
                {
                    index =2;
                    token = "MATCHLOG_TXT_LOSE";
                }
                else
                {
                    index =1;
                    token = "MATCHLOG_TXT_DRAW";
                }
            }

            int i =0;
            for(i =0; i < Bgs.Length; ++i)
            {
                Bgs[i].SetActive(i == index);
                Wins[i].SetActive(i == index);
            }

            WinText.SetText(pGameContext.GetLocalizingText(token));
            ScoreTexts[0].SetText(pLogMatchData.HomeGoals.ToString());
            ScoreTexts[1].SetText(pLogMatchData.AwayGoals.ToString());
            NameText.SetText(pLogMatchData.Name);
            SingleFunc.UpdateTimeText(pLogMatchData.StartT,TimeText,2);
            pLogMatchData.SetupEmblemData(Emblem);
        }
    }

    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    List<LogMatchData> m_pMatchIdList = new List<LogMatchData>();
    List<MatchLogItem> m_pMatchLogItems = new List<MatchLogItem>();

    int m_iSelectIndex = 0;
    int m_iMatchType = 0;
    ulong m_ulId = 0;
    
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}

    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;


    public MatchLog(){}
    
    public void Dispose()
    {
        if(m_pScrollRect != null)
        {
            ClearScroll();
            m_pScrollRect.onValueChanged.RemoveAllListeners();
        }
        m_pMatchIdList = null;
        m_pMatchLogItems = null;
        m_pScrollRect = null;
        m_pMainScene = null;
        MainUI = null; 
        m_pMatchIdList = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {        
        ALFUtils.Assert(pBaseScene != null, "MatchLog : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "MatchLog : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        SetupScroll();
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupData(JObject data,E_REQUEST_ID eReq)
    {
        if(data == null ) return;

        JArray list = null;
        m_iMatchType = GameContext.LADDER_ID;
        if(eReq == E_REQUEST_ID.matchStats_list && data.ContainsKey("list"))
        {
            list = (JArray)data["list"];
        }
        else if(eReq == E_REQUEST_ID.challengeStage_getMatches && data.ContainsKey("matches"))
        {
            m_iMatchType = GameContext.CHALLENGE_ID;
            list = (JArray)data["matches"];
        }
        
        int i = 0;
        
        if(list != null)
        {
            ResetScroll();
            i = m_pMatchIdList.Count;
            while(i > 0)
            {
                --i;
                m_pMatchIdList[i].Dispose();
            }
            m_pMatchIdList.Clear();
            GameContext pGameContext = GameContext.getCtx();
            m_ulId = 0;

            JObject pObject = null;
            LogMatchData pLogMatchData = null;
            
            if(list.Count > 1)
            {
                list = new JArray(list.OrderByDescending(obj => ALF.NETWORK.NetworkManager.ConvertLocalGameTimeTick(data["tStart"].ToString())));
            }
            
            for(i =0; i < list.Count; ++i)
            {    
                pObject = (JObject)list[i];
                pLogMatchData = new LogMatchData(pObject);
                m_pMatchIdList.Add(pLogMatchData);
            }
        }

        m_iStartIndex = 0;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        MatchLogItem pMatchLogItem = null;

        for(i =0; i < m_pMatchLogItems.Count; ++i)
        {
            pMatchLogItem = m_pMatchLogItems[i];
            itemSize = pMatchLogItem.Target.rect.height;

            if(m_pMatchIdList.Count <= i)
            {
                pMatchLogItem.Target.gameObject.SetActive(false);
            }
            else
            {
                if(viewSize > -itemSize)
                {    
                    viewSize -= itemSize;
                    pMatchLogItem.Target.gameObject.SetActive(viewSize > -itemSize);
                }
            }

            if(m_pMatchIdList.Count > i)
            {
                pMatchLogItem.UpdateInfo(m_pMatchIdList[i]);
            }
        }

        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y = m_pMatchIdList.Count * itemSize;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    void SetupScroll()
    {
        GameContext pGameContext = GameContext.getCtx();
        Vector2 size;
        float h = 0;
        RectTransform pItem = null;
        
        m_iTotalScrollItems = 0;
        m_iStartIndex = 0;

        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        MatchLogItem pMatchLogItem = null;

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

                pMatchLogItem = new MatchLogItem(pItem);
                m_pMatchLogItems.Add(pMatchLogItem);
            }
        }

        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);

        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.content.anchoredPosition = Vector2.zero; 
    }


    void ClearScroll()
    {
        int i = m_pMatchLogItems.Count;
        while(i > 0)
        {
            --i;
            m_pMatchLogItems[i].Dispose(); 
        }
        
        m_pMatchLogItems.Clear();

        i = m_pMatchIdList.Count;
        while(i > 0)
        {
            --i;
            m_pMatchIdList[i].Dispose(); 
        }
        
        m_pMatchIdList.Clear();

        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;

        m_ulId =0;
    }
    
    void ResetScroll()
    {
        Vector2 pos;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        MatchLogItem pItem = null;
        for(int i = 0; i < m_pMatchLogItems.Count; ++i)
        {
            pItem = m_pMatchLogItems[i];
            itemSize = pItem.Target.rect.height;
            viewSize -= itemSize;
            pItem.Target.gameObject.SetActive(viewSize > -itemSize);

            pos = pItem.Target.anchoredPosition;            
            pos.y = -i * itemSize;
            pItem.Target.anchoredPosition = pos;
        }

        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
        m_pPrevDir.y = 1;
    }

    void ShowClubProfileOverview()
    {
        m_pMainScene.ShowUserProfile(m_ulId,0);
    }

    void ShowMatchDetailLog()
    {
        GameContext pGameContext = GameContext.getCtx();
        string pHomeName = null;
        string pAwayName = null;
        string pScore = $"{m_pMatchIdList[m_iSelectIndex].HomeGoals}:{m_pMatchIdList[m_iSelectIndex].AwayGoals}";

        if(m_pMatchIdList[m_iSelectIndex].HomeId == pGameContext.GetClubID())
        {
            pHomeName = pGameContext.GetClubName();
            pAwayName = m_pMatchIdList[m_iSelectIndex].Name;
        }
        else
        {
            pHomeName = m_pMatchIdList[m_iSelectIndex].Name;
            pAwayName = pGameContext.GetClubName();
        }
        m_pMainScene.ShowMatchDetailLogPopup(null,pHomeName,pAwayName,pScore);
        m_iSelectIndex = -1;
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        MatchLogItem pItem = null;
        if(index > iTarget)
        {
            pItem = m_pMatchLogItems[iTarget];
            m_pMatchLogItems[iTarget] = m_pMatchLogItems[index];
            i = iTarget +1;
            MatchLogItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pMatchLogItems[i];
                m_pMatchLogItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pMatchLogItems[index] = pItem;
            pItem = m_pMatchLogItems[iTarget];
        }
        else
        {
            pItem = m_pMatchLogItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pMatchLogItems[i -1] = m_pMatchLogItems[i];
                ++i;
            }

            m_pMatchLogItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex + iTarget + iCount;
        
        if(i < 0 || m_pMatchIdList.Count <= i) return;

        pItem.UpdateInfo(m_pMatchIdList[i]);
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_pMatchIdList.Count <= 0) return;

        if(m_iTotalScrollItems < m_pMatchIdList.Count && value.y != m_pPrevDir.y)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        m_iSelectIndex = 0;
        for(int i =0; i < m_pMatchLogItems.Count; ++i)
        {
            if(m_pMatchLogItems[i].Target == tm)
            {
                m_iSelectIndex = m_iStartIndex + i;
                GameContext pGameContext = GameContext.getCtx();
                if(m_pMatchLogItems[i].Detail == sender )
                {
                    m_pMainScene.SetAwayEmblem(m_pMatchLogItems[i].Emblem);
                
                    RecordT pRecord = pGameContext.GetMatchLogData(m_pMatchLogItems[i].ID);
                    if(pRecord == null)
                    {
                        JObject pJObject = new JObject();
                        pJObject["match"] = m_pMatchLogItems[i].ID;
                        pJObject["matchType"] = m_iMatchType;
                        m_pMainScene.RequestAfterCall(E_REQUEST_ID.matchStats_getSummary,pJObject,ShowMatchDetailLog);
                    }
                    else
                    {
                        string pHomeName = null;
                        string pAwayName = null;
                        
                        string pScore = $"{m_pMatchIdList[m_iSelectIndex].HomeGoals}:{m_pMatchIdList[m_iSelectIndex].AwayGoals}";
                        if(m_pMatchIdList[m_iSelectIndex].HomeId == pGameContext.GetClubID())
                        {
                            pHomeName = pGameContext.GetClubName();
                            pAwayName = m_pMatchIdList[m_iSelectIndex].Name;
                        }
                        else
                        {
                            pHomeName = m_pMatchIdList[m_iSelectIndex].Name;
                            pAwayName = pGameContext.GetClubName();
                        }
                        pRecord.AwayName = pAwayName;
                        pRecord.HomeName = pHomeName;
                        pRecord.Score = pScore;
                        m_pMainScene.ShowMatchDetailLogPopup(pRecord,null,null,pScore);
                    }
                }
                else
                {
                    m_ulId = m_pMatchIdList[m_iSelectIndex].HomeId == pGameContext.GetClubID() ? m_pMatchIdList[m_iSelectIndex].AwayId : m_pMatchIdList[m_iSelectIndex].HomeId;
                    m_pMainScene.RequestClubProfile(m_ulId,ShowClubProfileOverview);
                }
                
                return;
            }
        } 
    }

    public void Close()
    {
        Enable = false;
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
    
}
