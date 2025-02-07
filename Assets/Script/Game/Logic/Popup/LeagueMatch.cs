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

public class LeagueMatch : IBaseUI
{
    const string SCROLL_ITEM_NAME = "LeagueMatchItem";

    private class LeagueMatchItem : IBase
    {
        public ulong ID {get; private set;} 
        public RectTransform Target  {get; private set;}
        public TMPro.TMP_Text Name {get; private set;}
        public Button Info {get; private set;}
        public Button Match {get; private set;}
        public TMPro.TMP_Text Ranking {get; private set;}
        public TMPro.TMP_Text GameText {get; private set;}
        public TMPro.TMP_Text ScoreText {get; private set;}
        public TMPro.TMP_Text SquadPowerText {get; private set;}
        public EmblemBake Emblem {get; private set;}
        public int Standing {get; private set;}
        public int SquadPower {get; private set;}
        public int Game {get; private set;}
        public int Win {get; private set;}
        public int Lose {get; private set;}
        public int GF {get; private set;}
        public int GA {get; private set;}
        public int PTS {get; private set;}

        public int Status {get; private set;}

        byte[] m_pEmblemInfo = SingleFunc.CreateRandomEmblem();
        string m_pToken = GameContext.getCtx().GetLocalizingText("LEAGUE_MATCH_TXT_RANKING_NUMBER");
        
        public LeagueMatchItem( RectTransform taget, JObject data)
        {
            Target = taget;
            Info = Target.GetComponent<Button>();
            ID = (ulong)data["match"];
            Target.gameObject.name = (string)data["club"];
            Name = Target.Find("name").GetComponent<TMPro.TMP_Text>();
            Name.SetText((string)data["name"]);
            Emblem = Target.Find("emblem").GetComponent<EmblemBake>();
            GameText = Target.Find("record/text").GetComponent<TMPro.TMP_Text>();
            Ranking = Target.Find("ranking/text").GetComponent<TMPro.TMP_Text>();
            ScoreText = Target.Find("score/text").GetComponent<TMPro.TMP_Text>();
            SquadPowerText = Target.Find("over").GetComponent<TMPro.TMP_Text>();
            SquadPower = (int)data["squadPower"];
            GameContext pGameContext = GameContext.getCtx();
            SquadPowerText.SetText(pGameContext.GetLocalizingText("LEAGUE_MATCH_TXT_OVERALL") + ": " + SquadPower.ToString());
            Match = Target.Find("match").GetComponent<Button>();
            Status = (int)data["status"];
            Match.gameObject.SetActive(Status == 0);
            ScoreText.transform.parent.gameObject.SetActive(Status != 0);
            if(!Match.gameObject.activeSelf)
            {
                ScoreText.SetText(Status == 10 ? string.Format("{0}:{1}",(string)data["homeGoals"],(string)data["awayGoals"]) : "0:2");
            }

            Game = (int)data["game"];
            Win = (int)data["win"];
            Lose = (int)data["lose"];
            GameText.SetText(string.Format(pGameContext.GetLocalizingText("LEAGUE_MATCH_TXT_LEAGUE_RECORD_DETAIL"),Game,Win,Game - (Win + Lose),Lose));

            GF = (int)data["gf"];
            GA = (int)data["ga"];
            PTS = (int)data["pts"];
            SetStanding((int)data["standing"]);

            if(data.ContainsKey("latestResult") && data["latestResult"].Type != JTokenType.Null)
            {
                JArray pList = JArray.Parse((string)data["latestResult"]);
                Transform tm = Target.Find("log/log");
                for(int t =0; t < tm.childCount; ++t)
                {
                    tm.GetChild(t).gameObject.SetActive(false);
                }
                Transform item = null;
                TMPro.TMP_Text text = null;
                for(int t =0; t < pList.Count; ++t)
                {
                    item = tm.GetChild(t);
                    item.gameObject.SetActive(true);
                    item.GetChild(0).gameObject.SetActive(false);
                    item.GetChild(1).gameObject.SetActive(false);
                    item.GetChild(2).gameObject.SetActive(false);
                    item.Find((string)pList[t]).gameObject.SetActive(true);
                    text = item.Find("text").GetComponent<TMPro.TMP_Text>();
                    if((int)pList[t] == 3)
                    {
                        text.SetText(pGameContext.GetLocalizingText("LEAGUE_TXT_LEAGUE_STANDING_WIN"));
                    }
                    else if((int)pList[t] == 1)
                    {
                        text.SetText(pGameContext.GetLocalizingText("LEAGUE_TXT_LEAGUE_STANDING_DRAW"));
                    }
                    else
                    {
                        text.SetText(pGameContext.GetLocalizingText("LEAGUE_TXT_LEAGUE_STANDING_LOSE"));
                    }
                }
            }
            
            if(data.ContainsKey("emblem") && data["emblem"].Type != JTokenType.Null)
            {
                m_pEmblemInfo = SingleFunc.GetMakeEmblemData((string)data["emblem"]);
            }
            Emblem.SetupEmblemData(m_pEmblemInfo);

            LocalizingText[] list = Target.GetComponentsInChildren<LocalizingText>(true);
            for(int i =0; i < list.Length; ++i)
            {
                list[i].UpdateLocalizing();
            }
        }

        public void SetStanding(int iStanding)
        {
            if(Standing == iStanding) return;

            Standing = iStanding;
            
            Ranking.SetText(string.Format(m_pToken,Standing));
        }

        public void Dispose()
        {
            
            Info = null;
            Match = null;
            ScoreText = null;
            SquadPowerText = null;
            Name = null;
            Ranking = null;
            Info = null;
            GameText = null;
            Emblem.Dispose();
            Emblem = null;
            m_pEmblemInfo = null;
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,Target);
            Target = null;
        }
    }

    ulong m_ulId = 0;
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;

    List<LeagueMatchItem> m_pLeagueMatchItemList = new List<LeagueMatchItem>();
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    public LeagueMatch(){}
    
    public void Dispose()
    {
        ClearScroll();
        m_pScrollRect = null;
        m_pMainScene = null;
        MainUI = null; 
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {        
        ALFUtils.Assert(pBaseScene != null, "LeagueMatch : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "LeagueMatch : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupData(JObject data)
    {
        if(data != null && data.ContainsKey("opps") && data["opps"].Type == JTokenType.Array)
        {
            ClearScroll();
            GameContext pGameContext = GameContext.getCtx();
            JArray pJArray = (JArray)data["opps"];
            JObject item = null;
            RectTransform pItem = null; 
            float h =0;
            Vector3 size;
            
            for(int i =0; i < pJArray.Count; ++i)
            {
                item = (JObject)pJArray[i];
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
                if(pItem)
                {
                    m_pLeagueMatchItemList.Add(new LeagueMatchItem( pItem, item));

                    pItem.SetParent(m_pScrollRect.content,false);
                    
                    pItem.localScale = Vector3.one;       
                    pItem.anchoredPosition = new Vector2(0,-h);
                    size = pItem.sizeDelta;
                    size.x =0;
                    pItem.sizeDelta = size;
                    h += pItem.rect.height;
                }   
            }

            size = m_pScrollRect.content.sizeDelta;
            size.y = h;
            m_pScrollRect.content.sizeDelta = size;
            LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
            m_pScrollRect.horizontalNormalizedPosition = 1;
            m_pScrollRect.enabled = true;
            LayoutManager.Instance.InteractableEnabledAll();
        }
    }

    void ClearScroll()
    {
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);

        for(int i =0; i < m_pLeagueMatchItemList.Count; ++i)
        {
            m_pLeagueMatchItemList[i].Dispose();
        }
        m_pLeagueMatchItemList.Clear();
        m_pScrollRect.horizontalNormalizedPosition = 0;
    }

    void ShowClubProfileOverview()
    {
        m_pMainScene.ShowUserProfile(m_ulId,0);
        m_ulId =0;
    }

    public void FailChallenge()
    {
        Close();
    }

    void TryLeague()
    {
        GameContext pGameContext = GameContext.getCtx();
        pGameContext.LeagueTodayMatch();
        Close();
        m_pMainScene.UpdateLeagueTodayCount();
        m_pMainScene.ShowMainUI(false);
        m_pMainScene.ShowMatchPopup(pGameContext.GetCurrentMatchType());
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        if(tm.gameObject == sender)
        {
            if(ulong.TryParse(tm.gameObject.name,out m_ulId))
            {
                m_pMainScene.RequestClubProfile(m_ulId,ShowClubProfileOverview);
            }
        }
        else
        {
            for(int i =0; i < m_pLeagueMatchItemList.Count; ++i)
            {
                if(m_pLeagueMatchItemList[i].Target == tm)
                {
                    GameContext pGameContext = GameContext.getCtx();
                    JObject data = new JObject();
                    ulong id = m_pLeagueMatchItemList[i].ID;
                    data["match"] = id;
                    if(pGameContext.GetLineupTotalHP() / 1800.0f != 1)
                    {
                        m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_LEAGUE_SQUAD_HP_LOW_TITLE"),pGameContext.GetLocalizingText("DIALOG_LEAGUE_SQUAD_HP_LOW_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,() =>{       
                            m_pMainScene.GetInstance<LeagueSchedule>().StartMatch(id);
                            m_pMainScene.RequestAfterCall(E_REQUEST_ID.league_try,data,TryLeague);
                        } );
                    }
                    else
                    {
                        m_pMainScene.GetInstance<LeagueSchedule>().StartMatch(id);
                        m_pMainScene.RequestAfterCall(E_REQUEST_ID.league_try,data,TryLeague);
                    }
                
                    return;
                }
            }
        }
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI,()=>{
            ClearScroll();
            LayoutManager.Instance.InteractableEnabledAll();
            MainUI.Find("root").localScale = Vector3.one;
            MainUI.gameObject.SetActive(false);
        });
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
    
}
