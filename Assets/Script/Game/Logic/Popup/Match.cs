using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;

using ALF.STATE;
// using ALF.CONDITION;
using STATEDATA;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using Newtonsoft.Json.Linq;
using USERRANK;
using MATCHTEAMDATA;

public class Match : IBaseUI
{
    MainScene m_pMainScene = null;    
    public RectTransform MainUI { get; private set;}

    EmblemBake[] m_pEmblemList = new EmblemBake[2];
    TMPro.TMP_Text[] m_pHomeInfos = new TMPro.TMP_Text[5];
    TMPro.TMP_Text[] m_pAwayInfos = new TMPro.TMP_Text[5];
    TMPro.TMP_Text m_pRankTitle = null;
    GameObject m_pGiveup = null;
    TMPro.TMP_Text m_pTitle = null;

    bool m_bGiveup = false;
    bool m_bShowGiveup = false;
    ulong m_ulSendCustomerNo = 0;

    MatchTeamDataT m_pMatchTeamData = null;
    int m_iMatchType = 1;
    public Match(){}
    
    public void Dispose()
    {
        m_pGiveup = null;
        m_pRankTitle = null;
        m_pMainScene = null;
        m_pTitle = null;
        MainUI = null;
        int i =0;
        for(i =0; i < m_pHomeInfos.Length; ++i)
        {
            m_pHomeInfos[i] = null;
            m_pAwayInfos[i] = null;
        }

        for(i =0; i < m_pEmblemList.Length; ++i)
        {
            m_pEmblemList[i].material = null;
            m_pEmblemList[i] = null;
        }

        m_pEmblemList = null;
        m_pHomeInfos = null;
        m_pAwayInfos = null;
        
        m_pMatchTeamData = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Match : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Match : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        Transform tm = MainUI.Find("root/home");
        m_pEmblemList[0] = tm.Find("home").GetComponent<EmblemBake>();
        m_pHomeInfos[0] = tm.Find("name").GetComponent<TMPro.TMP_Text>();
        m_pHomeInfos[1] = tm.Find("trophy/text").GetComponent<TMPro.TMP_Text>();
        m_pHomeInfos[2] = tm.Find("overall/text").GetComponent<TMPro.TMP_Text>();
        m_pHomeInfos[3] = tm.Find("formation/text").GetComponent<TMPro.TMP_Text>();
        m_pHomeInfos[4] = tm.Find("rank/text").GetComponent<TMPro.TMP_Text>();

        tm = MainUI.Find("root/away");
        m_pEmblemList[1] = tm.Find("away").GetComponent<EmblemBake>();
        m_pAwayInfos[0] = tm.Find("name").GetComponent<TMPro.TMP_Text>();
        m_pAwayInfos[1] = tm.Find("trophy/text").GetComponent<TMPro.TMP_Text>();
        m_pAwayInfos[2] = tm.Find("overall/text").GetComponent<TMPro.TMP_Text>();
        m_pAwayInfos[3] = tm.Find("formation/text").GetComponent<TMPro.TMP_Text>();
        m_pAwayInfos[4] = tm.Find("rank/text").GetComponent<TMPro.TMP_Text>();
        m_pRankTitle = MainUI.Find("root/bg/rank").GetComponent<TMPro.TMP_Text>();
        m_pTitle = MainUI.Find("root/bg/title").GetComponent<TMPro.TMP_Text>();
        m_pGiveup = MainUI.Find("root/buttons/giveup").gameObject;
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void SetupMatchInfo( EmblemBake pEmblemBake, int iMatchType)
    {
        m_bGiveup = false;
        m_bShowGiveup = false;
        m_ulSendCustomerNo = 0;
        if(pEmblemBake != null)
        {
            m_pEmblemList[0].CopyPoint(pEmblemBake);
        }
        
        m_iMatchType = iMatchType;
        GameContext pGameContext = GameContext.getCtx();
        
        if(m_iMatchType == GameContext.LADDER_ID)
        {
            m_pTitle.gameObject.SetActive(false);
            m_pRankTitle.SetText(pGameContext.GetLocalizingText("MATCH_TXT_RANK"));

            m_pHomeInfos[1].transform.parent.gameObject.SetActive(true);
            m_pAwayInfos[1].transform.parent.gameObject.SetActive(true);
            m_pHomeInfos[1].SetText(pGameContext.GetCurrentUserTrophy().ToString());
            m_pAwayInfos[1].SetText(pGameContext.GetMatchAwayTrophy().ToString());

            UserRankList pUserRankList = pGameContext.GetFlatBufferData<UserRankList>(E_DATA_TYPE.UserRank);
            UserRankItem? pUserRankItem = pUserRankList.UserRankByKey(pGameContext.GetCurrentUserRank());
            m_pHomeInfos[4].SetText(pGameContext.GetLocalizingText(pUserRankItem.Value.Name));
            
            pUserRankItem = pUserRankList.UserRankByKey(pGameContext.GetMatchAwayRank());
            m_pAwayInfos[4].SetText(pGameContext.GetLocalizingText(pUserRankItem.Value.Name));
            m_pGiveup.SetActive(true);
        }
        else
        {
            m_pGiveup.SetActive(false);
            m_pTitle.gameObject.SetActive(true);
            m_pHomeInfos[1].transform.parent.gameObject.SetActive(false);
            m_pAwayInfos[1].transform.parent.gameObject.SetActive(false);
            
            m_pRankTitle.SetText(pGameContext.GetLocalizingText("LEAGUE_TXT_LEAGUE_STANDING_RANKING"));
            m_pAwayInfos[4].SetText(pGameContext.GetMatchAwayStanding().ToString());
            string token = "";
            if(m_iMatchType == GameContext.CHALLENGE_ID)
            {
                token = "CHALLENGESTAGE_TITLE_CHALLENGE_STAGE";
            }
            else if(m_iMatchType == GameContext.LEAGUE1_ID)
            {
                token = "LEAGUE_TITLE_CONFERENCE_2";
            }
            else if(m_iMatchType == GameContext.LEAGUE2_ID)
            {
                token = "LEAGUE_TITLE_CONFERENCE_1";
            }
            else if(m_iMatchType == GameContext.LEAGUE3_ID)
            {
                token = "LEAGUE_TITLE_CHAMPION";
            }
            else if(m_iMatchType == GameContext.LEAGUE4_ID)
            {
                token = "LEAGUE_TITLE_ULTIMATE_CHAMPION";
            }
            
            m_pTitle.SetText(pGameContext.GetLocalizingText(token));

            if(m_pMainScene.IsShowInstance<League>())
            {
                m_pHomeInfos[4].SetText(m_pMainScene.GetInstance<League>().GetMyClubStandingByID().ToString());
            }
        }
        
        m_pEmblemList[1].SetupEmblemData(pGameContext.GetMatchTeamEmblemData());

        m_pMatchTeamData = new MatchTeamDataT();
        m_pMatchTeamData.TeamData = new List<TeamDataT>();
        m_pMatchTeamData.TeamData.Add(pGameContext.MakeTeamData(pGameContext.GetActiveTacticsType(),pGameContext.GetActiveLineUpType()));
        m_pMatchTeamData.TeamData.Add(pGameContext.MakeMatchTeamData(m_iMatchType));

        m_pHomeInfos[0].SetText(pGameContext.GetClubName());
        m_pHomeInfos[2].SetText(pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType()).ToString());
        m_pHomeInfos[3].SetText(TacticsFormation.UpdateFormationText(m_pMatchTeamData.TeamData[0].Tactics.Formation));
        m_pAwayInfos[0].SetText(pGameContext.GetMatchAwayClubName());
        m_pAwayInfos[2].SetText(pGameContext.GetMatchTeamTotalPlayerAbility().ToString());
        m_pAwayInfos[3].SetText(TacticsFormation.UpdateFormationText(m_pMatchTeamData.TeamData[1].Tactics.Formation));
        Transform root = MainUI.Find("root");
        for(int i =0; i < root.childCount; ++i)
        {
            root.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void UpdateTacticsData(TacticsT pTempTactics, LineupPlayerT pLineupPlayer)
    {
        SingleFunc.UpdateTacticsData(pTempTactics,pLineupPlayer,0,ref m_pMatchTeamData);
        m_pHomeInfos[3].SetText(TacticsFormation.UpdateFormationText(m_pMatchTeamData.TeamData[0].Tactics.Formation));
        
        GameContext pGameContext = GameContext.getCtx();
        uint total =0;

        USERDATA.PlayerT pPlayer = null;
        for(int n =0; n < GameContext.CONST_NUMSTARTING; ++n)
        {
            pPlayer = pGameContext.GetPlayerByID(pLineupPlayer.Data[n]);
            total += pPlayer.AbilityWeightSum;
        }
        
        m_pHomeInfos[2].SetText(total.ToString());
    }

    void ViewClubProfileFormation()
    {
        m_pMainScene.RequestClubProfile(m_ulSendCustomerNo,ShowClubProfileFormation);
    }

    void ShowClubProfileFormation()
    {
        m_pMainScene.ShowUserProfile(m_ulSendCustomerNo,1);
    }

    void ViewClubProfileOverview()
    {
        m_pMainScene.RequestClubProfile(m_ulSendCustomerNo,ShowClubProfileOverview);
    }

    void ShowClubProfileOverview()
    {
        m_pMainScene.ShowUserProfile(m_ulSendCustomerNo,0);
    }

    public void Play()
    {
        Transform root = MainUI.Find("root");
        for(int i =0; i < root.childCount; ++i)
        {
            root.GetChild(i).gameObject.SetActive(false);
        }
        root.GetComponent<Animation>().Play();
    }
    public void ClearData()
    {
        m_pEmblemList[0].material = null;
        if(m_bGiveup)
        {
            m_pEmblemList[1].Dispose();
            if(!StateMachine.GetStateMachine().IsCurrentTargetStates((uint)E_STATE_TYPE.ShowStartPopup)) 
            {
                m_pMainScene.ShowStartPopup();
            }
        }
        m_bGiveup = false;
        m_bShowGiveup = false;
        m_pEmblemList[1].material = null;
    }

    public void Close()
    {
        m_pMainScene.HideMoveDilog(MainUI,Vector3.up);
    }   

    public TeamDataT GetMyTeamData()
    {
        if(m_pMatchTeamData != null)
        {
            return m_pMatchTeamData.TeamData[0];
        }

        return null;
    }
    public void CloseShowGiveup()
    {
        if(m_bShowGiveup)
        {
            Giveup();
        }
    }
    
    void Giveup()
    {
        GameContext pGameContext = GameContext.getCtx();
        pGameContext.SendAdjustEvent("88s2qy",false,false,-1);
        m_bGiveup = true;
        m_bShowGiveup = false;
        m_pMainScene.GetInstance<Ground>().FocusStadium(false);
        
        m_pMainScene.RequestAfterCall(E_REQUEST_ID.ladder_clear,pGameContext.MakeMatchStatsData(E_APP_MATCH_TYPE.APP_MATCH_TYPE_GIVEUP));
        m_pMainScene.GetInstance<MatchView>().ResetMatchCount();
        Close();
    }


    public void StartTutorial()
    {
        ButtonEventCall(null,MainUI.Find("root/buttons/kickoff").gameObject);
    }
    public RectTransform GetTutorialButton()
    {
        return MainUI.Find("root/buttons/kickoff").GetComponent<RectTransform>();
    }

    public void ShowGiveupPopup()
    {
        m_bShowGiveup = true;
        GameContext pGameContext = GameContext.getCtx();
        m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_WITHDRAW_TITLE"),pGameContext.GetLocalizingText("DIALOG_WITHDRAW_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,Giveup,()=>{m_bShowGiveup = false;});
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(sender.name)
        {
            case "giveup":
            {
                ShowGiveupPopup();
                return;
            }
            case "tatics":
            {
                m_pMainScene.ShowTacticsFormation(m_pMatchTeamData.TeamData[0]);
                m_pMainScene.SetMainButton(2);
                return;
            }
            case "info":
            {
                m_ulSendCustomerNo = GameContext.getCtx().GetMatchAwayID();
                ViewClubProfileFormation();
                return;
            }
            case "home":
            {
                m_ulSendCustomerNo = GameContext.getCtx().GetClubID();
                ViewClubProfileOverview();
                return;
            }
            case "away":
            {
                m_ulSendCustomerNo = GameContext.getCtx().GetMatchAwayID();
                ViewClubProfileOverview();
                return;
            }
            case "tip":
            {
                m_pMainScene.ShowGameTip("game_tip_match_title");
                return;
            }
            case "kickoff":
            {
                GameContext.getCtx().FadeCrowdSFX();
                if(m_pMainScene.IsShowInstance<League>())
                {
                    SoundManager.Instance.ChangeBGMVolume(0,0.5f);
                    League pLeague = m_pMainScene.GetInstance<League>();
                    pLeague.ChangeBGM(false);
                    m_pMainScene.GetInstance<League>().Close();
                }
                m_pMainScene.StartMatch(m_pMatchTeamData, m_pEmblemList,m_iMatchType);
                m_pMatchTeamData = null;
                GameContext.getCtx().SendAdjustEvent("2vd0pu",false,false,-1);
            }
            break;    
        }
        
        Close();
    }
    
}
