using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
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
using USERDATA;
using STATEDATA;
using Newtonsoft.Json.Linq;
using MATCHTEAMDATA;
using STATISTICSDATA;
using USERRANK;
using CONSTVALUE;

public class MainScene : IBaseScene
{
    enum E_CLICK_TYPE :byte { None = 0,Start,Move,End };
    enum E_CURTAIN_STEP: byte { FadeOut = 0,FadeIn };

    const byte TODAY_MATCH_MAX = 100;
    const int CONST_TOP_HEIGHT = 140;
    
    EmblemBake m_pAwayEmblem = null;
    EmblemBake m_pEmblem = null;
    TMPro.TMP_Text m_pUserName = null;
    TMPro.TMP_Text m_pUserRank = null;
    RawImage m_pUserRankIcon = null;
    RawImage m_pUserRankNumIcon = null;
    TMPro.TMP_Text m_pUserGameMoney = null;
    TMPro.TMP_Text m_pChatNoti = null;
    TMPro.TMP_Text m_pUserToken = null;
    TMPro.TMP_Text m_pUnreadMailCount = null;
    Image m_pMatchGauge = null;
    Animation m_pMatchEffect = null;
    Transform m_pMenu = null;
    RectTransform m_pAdGroup = null;
    Animation[] m_pNotiIconList = new Animation[(int)E_NOTICE_NODE.MAX];
    Transform m_pMenuBack = null;
    RectTransform m_pMainButtons = null;
    Animation m_pTimeSale = null;
    TMPro.TMP_Text m_pTimeSaleText = null;
    Button m_pMatchButton = null;
    Button m_pRecoveryButton = null;
    byte m_iCurrentUserRank = 0;
    Dictionary<string,IBaseUI> m_pUIClassList = null;
    Dictionary<string,List<IBaseUI>> m_pMutiUIList = new Dictionary<string,List<IBaseUI>>();
    long m_lSuspendTime = 0;
    Vector3 deltaPosition;
    bool m_bCreateClub = false;
    
    int bgmID = -1;

    Dictionary<E_REQUEST_ID,Action> m_pRequestAfterCallList = new Dictionary<E_REQUEST_ID, Action>();

    E_CLICK_TYPE eClickType = E_CLICK_TYPE.None;
    float dtime = 0;
    
    public bool Transition {get; set;}
    protected MainScene(bool bCreateClub)
    {
        m_bCreateClub = bCreateClub;
        m_pUIClassList = new Dictionary<string, IBaseUI>();
    }
    public static MainScene Create( bool bCreateClub)
    {
        return new MainScene(bCreateClub);
    }

    public PositionTraining ShowPositionTrainingPopup()
    {
        PositionTraining pUI = GetInstance<PositionTraining>();
        pUI.MainUI.SetAsLastSibling();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
    
        return pUI;
    }

    public MailMessage ShowMailMessagePopup(ulong id, byte type)
    {
        MailMessage pUI = GetInstance<MailMessage>();

        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        pUI.SetupData(id,type);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.Enable = true;
        });
        return pUI;
    }

    public MailBox ShowMailBoxPopup()
    {   
        MailBox pUI = GetInstance<MailBox>();
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        pUI.SetupData();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.Enable = true;
        });
        return pUI;
    }

    public AttendReward ShowAttendRewardPopup()
    {
        AttendReward pUI = GetInstance<AttendReward>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupRewardAnimation();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,pUI.ShowReward);
        return pUI;
    }
    public QuickLineUp ShowQuickLineUpPopup()
    {
        QuickLineUp pUI = GetInstance<QuickLineUp>();
        pUI.MainUI.SetAsLastSibling();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public Tip ShowGameTip(string id)
    {
        Tip pUI = GetInstance<Tip>();
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        pUI.SetupData(id);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.Enable = true;
        });
        return pUI;
    }

    public MatchHelp ShowMatchHelpPopup()
    {
        MatchHelp pUI = GetInstance<MatchHelp>();
        pUI.MainUI.SetAsLastSibling();
        pUI.MainUI.gameObject.SetActive(true);
        LayoutManager.Instance.ShowPopup(pUI.MainUI);

        pUI.PlayNextStep();
        return pUI;
    }

    public PlayerInfo ShowPlayerInfoPopup()
    {
        PlayerInfo pUI = GetInstance<PlayerInfo>();
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.MoveQuickPlayer();
            pUI.Enable = true;
        });
        return pUI;
    }

    public League ShowLeaguePopup()
    {
        League pUI = null;

        long timeOfDay = TimeSpan.TicksPerDay - NetworkManager.GetGameServerTime().TimeOfDay.Ticks - (TimeSpan.TicksPerHour * 9);
        if(timeOfDay < 0)
        {
            timeOfDay += TimeSpan.TicksPerDay;
        }
        
        if(timeOfDay < TimeSpan.TicksPerDay - TimeSpan.TicksPerHour)
        {
            pUI = GetInstance<League>();
            pUI.MainUI.SetAsLastSibling();
            pUI.Enable = false;
            pUI.ShowLeagueList(GameContext.getCtx().GetCurrentMatchType());
            SoundManager.Instance.ChangeBGM("bgm_league",true,0.4f);
            ShowMoveDilog(pUI.MainUI,Vector3.down);
        }
        else
        {
            ShowMessagePopup("리그 정산중..",GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
        }

        return pUI;
    }

    public ManagementPlan ShowManagementPlanPopup(SHOPPRODUCT.ShopProduct? pSProduct)
    {
        ManagementPlan pUI = GetInstance<ManagementPlan>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupData(pSProduct);

        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }
    public SelectLogin ShowSelectLoginPopup()
    {
        SelectLogin pUI = GetInstance<SelectLogin>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupData();

        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }
    public SelectLanguage ShowSelectLanguagePopup()
    {
        SelectLanguage pUI = GetInstance<SelectLanguage>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupData();

        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public SelectFormation ShowSelectFormationPopup()
    {
        SelectFormation pUI = GetInstance<SelectFormation>();
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.Enable = true;
        });
        return pUI;
    }

    public LoadTactics ShowLoadTacticsPopup()
    {
        LoadTactics pUI = GetInstance<LoadTactics>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupTacticsInfo();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public SaveTactics ShowSaveTacticsPopup()
    {
        SaveTactics pUI = GetInstance<SaveTactics>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupTacticsInfo();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public Match ShowMatchPopup(int iMatchType)
    {
        if(m_pMenu.gameObject.activeSelf)
        {
            ShowMenuPopup(false);
        }
        if(iMatchType == GameContext.LADDER_ID)
        {
            GetInstance<MatchView>().AddMatchCount();
        }

        Match pUI = GetInstance<Match>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupMatchInfo(m_pEmblem,iMatchType);
        ShowMoveDilog(pUI.MainUI,Vector3.down);
        return pUI;
    }

    public void UpdateMatchData(TacticsT pTempTactics, LineupPlayerT pLineupPlayer)
    {
        if(IsShowInstance<Match>())
        {
            GetInstance<Match>().UpdateTacticsData(pTempTactics,pLineupPlayer);
            return;
        }
    }

    public void UpdateMatchTacticsData(TacticsT pTempTactics, ulong[] playerList,LineupPlayerT pLineupPlayer)
    {
        GetInstance<MatchView>().UpdateTacticsData(pTempTactics,playerList,pLineupPlayer);
    }

    public TrophyReward ShowTrophyRewardPopup()
    {
        TrophyReward pUI = GetInstance<TrophyReward>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupMileageMission();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.Enable = true;
        });
        return pUI;
    }

    public void ShowMenuPopup(bool bShow)
    {
        m_pMenu.gameObject.SetActive(bShow);
        m_pMenuBack.gameObject.SetActive(bShow);
    }

    public Rookie ShowRookiePopup()
    {
        Rookie pUI = GetInstance<Rookie>();
        
        pUI.MainUI.SetAsLastSibling();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {LayoutManager.Instance.InteractableEnabledAll();});
        return pUI;
    }

    public ClubLicense ShowClubLicensePopup()
    {
        ClubLicense pUI = GetInstance<ClubLicense>();
        
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupLicenseData();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.Enable = true;
        });
        return pUI;
    }

    public QuestMission ShowQuestMissionPopup()
    {
        QuestMission pUI = GetInstance<QuestMission>();
        
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupQuestData();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.Enable = true;
        });
        return pUI;
    }

    public RankingReward ShowRankingRewardPopup()
    {
        RankingReward pUI = GetInstance<RankingReward>();
        
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupExpire();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Enable = true;
            LayoutManager.Instance.InteractableEnabledAll();
        });
        return pUI;
    }

    public MatchDetailLog ShowMatchDetailLogPopup(RecordT pRecord, string homeName, string awayName, string pScore)
    {
        MatchDetailLog pUI = GetInstance<MatchDetailLog>();

        pUI.MainUI.SetAsLastSibling();
        if(pRecord != null)
        {
            pUI.SetupRecordData(pRecord,m_pAwayEmblem);
            m_pAwayEmblem = null;
        }

        if(awayName != null && homeName != null)
        {
            pUI.SetupClubName(homeName,awayName);
        }

        if(pScore != null)
        {
            pUI.SetupScore(pScore);
        }
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Enable = true;
            LayoutManager.Instance.InteractableEnabledAll();
        });
        return pUI;
    }
    public MatchLog ShowMatchLogPopup()
    {
        MatchLog pUI = GetInstance<MatchLog>();
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Enable = true;
            LayoutManager.Instance.InteractableEnabledAll();
        });
        return pUI;
    }

    public Ranking ShowRankingPopup()
    {
        Ranking pUI = GetInstance<Ranking>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupExpire();
        if(!pUI.MainUI.gameObject.activeSelf)
        {
            pUI.SetCurrentSeasinNo();
            pUI.Enable = false;
            SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
                LayoutManager.Instance.InteractableEnabledAll();
                pUI.Enable = true;
            });
        }
        return pUI;
    }
    
    public SquadExpansion ShowSquadExpansionPopup()
    {
        SquadExpansion pUI = GetInstance<SquadExpansion>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupSquadExpansionData();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }
    
    public EditEmblem ShowEditEmblemPopup()
    {
        EditEmblem pUI = GetInstance<EditEmblem>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupEmblemInfo(GameContext.getCtx().GetEmblemInfo());
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Enable = true;
            LayoutManager.Instance.InteractableEnabledAll();
        });
        return pUI;
    }

    public HPRecovery ShowHPRecoveryPopup()
    {
        HPRecovery pUI = GetInstance<HPRecovery>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupData();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public PremiumSeasonPass ShowPremiumSeasonPassPopup()
    {
        PremiumSeasonPass pUI = GetInstance<PremiumSeasonPass>();
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        pUI.SetupData();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => { 
            pUI.Enable = true;
            GameContext pGameContext = GameContext.getCtx();
            if(pGameContext.IsRestorePurchase())
            {
                ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TITLE_IAPRESTORE"),pGameContext.GetLocalizingText("MSG_TXT_IAPRESTORE"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),null,false,()=>{
    #if USE_HIVE
                    NetworkManager.ShowWaitMark(true);
                    LayoutManager.Instance.InteractableDisableAll(null,true);
                    SingleFunc.IAPV4Restore(OnIAPV4RestoreCB);
    #endif
                });
            }
            else
            {
                LayoutManager.Instance.InteractableEnabledAll();
            }
        });

        return pUI;
    }
    
    public Challenge ShowChallengePopup()
    {
        Challenge pUI = GetInstance<Challenge>();
        pUI.HideScroll();
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            RequestAfterCall(E_REQUEST_ID.challengeStage_searchOpps,null);
        });

        return pUI;
    }
    
    public LeagueMatch ShowLeagueMatchPopup()
    {
        LeagueMatch pUI = GetInstance<LeagueMatch>();
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            JObject data = new JObject();
            data["matchType"] = GameContext.getCtx().GetCurrentMatchType();
            RequestAfterCall(E_REQUEST_ID.league_getTodayFixture,data);
        });

        return pUI;
    }
    
    public LeagueSchedule ShowLeagueSchedulePopup()
    {
        LeagueSchedule pUI = GetInstance<LeagueSchedule>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupToday();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);

        return pUI;
    }
    
    public LeagueRankingReward ShowLeagueRankingRewardPopup(int id)
    {
        LeagueRankingReward pUI = GetInstance<LeagueRankingReward>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupExpire();
        pUI.Enable = false;
        pUI.ShowTabUI(id);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Enable = true;
            LayoutManager.Instance.InteractableEnabledAll();
        });

        return pUI;
    }
    
    public Reward ShowRewardPopup(JObject data)
    {
        Reward pUI = GetInstance<Reward>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupRewardItems(data);
        SoundManager.Instance.PlaySFX("sfx_reward");
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.ShowTab();
            pUI.ShowRewardItems();
        });

        return pUI;
    }

    public Profile ShowUserProfile(ulong userId,byte index)
    {
        E_PLAYER_INFO_TYPE eTeamType = E_PLAYER_INFO_TYPE.my;
        EmblemBake pEmblemBake = m_pEmblem;
        if(userId != GameContext.getCtx().GetClubID())
        {
            eTeamType = E_PLAYER_INFO_TYPE.away;
            pEmblemBake = null;
        }
        
        Profile pUI = GetInstance<Profile>();

        pUI.MainUI.SetAsLastSibling();
        pUI.SetupData(eTeamType,pEmblemBake);
        pUI.ShowTabUI(index);
        pUI.SetupTapUI(false);
        
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public SeasonPass ShowSeasonPass()
    {
        SeasonPass pUI = GetInstance<SeasonPass>();

        pUI.MainUI.SetAsLastSibling();
        pUI.SetupData();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Enable = true;
             LayoutManager.Instance.InteractableEnabledAll();
         });
        return pUI;
    }

    public void SetAdNotice(uint no,bool bShow)
    {
        Transform tm = m_pAdGroup.Find(no.ToString());
        if(tm == null) return;
        RectTransform notice = tm.GetComponent<RectTransform>();
        bool bMove = false;
        if(notice.gameObject.activeSelf != bShow)
        {
            bMove = true;
            notice.gameObject.SetActive(bShow);
            if(bShow)
            {
                notice.GetComponent<Animation>().Play("show");
            }
        }
        
        if(bMove)
        {
            float h = 0;
            Vector2 pos;
            for(int i =0; i < m_pAdGroup.childCount; ++i)
            {
                notice = m_pAdGroup.GetChild(i).GetComponent<RectTransform>();
                if(notice.gameObject.activeSelf)
                {
                    pos = notice.anchoredPosition;
                    pos.y = h;
                    notice.anchoredPosition = pos;
                    h -= notice.rect.height + 20;
                }
            }
        }
    }
    public MonthlyPackage ShowMonthlyPackagePopup(SHOPPRODUCT.ShopProduct? data)
    {
        MonthlyPackage pUI = GetInstance<MonthlyPackage>();
        pUI.SetupData(data);
        pUI.MainUI.SetAsLastSibling();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public ShopPackage ShowShopPackagePopup(SHOPPRODUCT.ShopProduct? data)
    {
        ShopPackage pUI = GetInstance<ShopPackage>();
        pUI.SetupScroll(data);
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => { pUI.Enable = true;LayoutManager.Instance.InteractableEnabledAll();});
        return pUI;
    }

    public ItemTip ShowItemTipPopup(GameObject obj)
    {
        uint id = 0;
        if(uint.TryParse(obj.name,out id))
        {
            ItemTip pUI = GetInstance<ItemTip>();
            pUI.SetupData(obj.GetComponent<RectTransform>(), id);
            pUI.MainUI.SetAsLastSibling();
            SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
            return pUI;
        }
        return null;
    }

    public AD ShowADPopup(uint no)
    {
        AD pUI = GetInstance<AD>();
        pUI.SetupRewardData(no);
        pUI.MainUI.SetAsLastSibling();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public ScoutReward ShowScoutRewardPopup(JObject data)
    {
        ScoutReward pUI = GetInstance<ScoutReward>();
        pUI.SetupScoutData(data);
        pUI.MainUI.SetAsLastSibling();
       
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Play();
            LayoutManager.Instance.InteractableEnabledAll();
        });
        
        return pUI;
    }

    public CreateClub ShowCreateClub()
    {
        CreateClub pUI = GetInstance<CreateClub>();
        pUI.Enable = false;
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupData();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Enable = true;
            LayoutManager.Instance.InteractableEnabledAll();
        });
        return pUI;
    }

    public Research ShowResearch()
    {
        Research pUI = GetInstance<Research>();

        pUI.MainUI.SetAsLastSibling();
        pUI.SetupResearch();
        
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => { LayoutManager.Instance.InteractableEnabledAll(); pUI.SetFocus();});
        return pUI;
    }

    public EditProfile ShowEditProfilePopup()
    {
        EditProfile pUI = GetInstance<EditProfile>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupProfileData(m_pEmblem);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public PlayerEdit ShowPlayerEditPopup(PlayerT pPlayer)
    {
        PlayerEdit pUI = GetInstance<PlayerEdit>();
        pUI.Enable = false;
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupPlayerInfoData(pPlayer);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Enable = true;
            LayoutManager.Instance.InteractableEnabledAll();
        });
        return pUI;
    }
    
    public PlayerComparison ShowPlayerComparisonPopup(PlayerT pPlayer,byte[] pEmblemData)
    {
        PlayerComparison pUI = GetInstance<PlayerComparison>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupPlayerInfoData(pPlayer,pEmblemData);
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.Enable = true;
        });
        return pUI;
    }

    public TrophyRewardInfo ShowTrophyRewardInfoPopup(uint group,uint id)
    {
        TrophyRewardInfo pUI = GetInstance<TrophyRewardInfo>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupMileageMissionReward(group,id);
        pUI.Enable = false;
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            LayoutManager.Instance.InteractableEnabledAll();
            pUI.Enable = true;
        });
        return pUI;
    }

    public Setting ShowSettingPopup()
    {
        Setting pUI = GetInstance<Setting>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupData();
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public TimeSale ShowTimeSaledPopup()
    {
        TimeSale pUI = GetInstance<TimeSale>();
        pUI.MainUI.SetAsLastSibling();
        pUI.Enable = false;
        pUI.SetupData(0);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,() => {
            pUI.Enable = true; 
            GameContext pGameContext = GameContext.getCtx();
            if(pGameContext.IsRestorePurchase())
            {
                ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TITLE_IAPRESTORE"),pGameContext.GetLocalizingText("MSG_TXT_IAPRESTORE"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),null,false,()=>{
    #if USE_HIVE
                    NetworkManager.ShowWaitMark(true);
                    LayoutManager.Instance.InteractableDisableAll(null,true);
                    SingleFunc.IAPV4Restore(OnIAPV4RestoreCB);
    #endif
                });
            }
            else
            {
                LayoutManager.Instance.InteractableEnabledAll();
            }
        });
        return pUI;
    }

    public FastReward ShowFastRewardPopup()
    {
        FastReward pUI = GetInstance<FastReward>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupRewardData(false);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public Message ShowMessagePopup(string message,string okText,Action pAction = null)
    {
        Message pUI = GetInstance<Message>();

        pUI.SetMessage(message,okText,pAction);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public RectTransform ShowToastMessage(string message,Transform addItem)
    {
        RectTransform pToastMessage = LayoutManager.Instance.FindUIFormRoot<RectTransform>("ToastMessage");
        TMPro.TMP_Text pText = pToastMessage.GetComponentInChildren<TMPro.TMP_Text>();
        pText.SetText(message);
        RectTransform tm = null;
        for(int i =0; i < pToastMessage.childCount; ++i)
        {
            tm = pToastMessage.GetChild(i).GetComponent<RectTransform>();
            if(tm.gameObject.name != "bg")
            {
                LayoutManager.Instance.AddItem(tm.gameObject.name,tm);
            }
        }
        
        if(addItem != null)
        {
            addItem.SetParent(pToastMessage,false);
        }
        
        ShowDilog(pToastMessage,true);
        return pToastMessage;
    }

    public WebDlg ShowWebDlgPopup(string title,string url)
    {
        WebDlg pUI = GetInstance<WebDlg>();

        pUI.Setup(title,url);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    public Confirm ShowConfirmPopup(string title,string message,string okText,string cancelText = null,bool bClose = false,System.Action actionOk = null,System.Action actionCancel = null)
    {
        Confirm pUI = GetInstance<Confirm>();

        pUI.SetText(title,message,okText,cancelText);
        pUI.ShowCloseButton(bClose);
        pUI.SetOKAction(actionOk);
        pUI.SetCancelAction(actionCancel);
        SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
        return pUI;
    }

    void OnApplicationFocus(bool hasFocus)
    {

    }

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log($"----------OnApplicationPause:{pauseStatus}");
        GameContext pGameContext = GameContext.getCtx();
        if(pauseStatus)
        {
            /**
            * 게임 서버에 로그인된 상태 체크
            */
            if(pGameContext.IsLoadGameData())
            {
                /**
                * 진행중인 옥션정보처리를 멈춤/ 동기화를 위해서..
                * 진행중인 옥션방을 떠난다. 서버 부하 때문...
                */
                pGameContext.PauseCurrentAuction(0);
                if(NetworkManager.IsSocketAlive())
                {
                    pGameContext.LeaveCurrentAuction();

                    if(IsShowInstance<PlayerInfo>())
                    {
                        GetInstance<PlayerInfo>().CurrentLeaveAuctionRoom();
                    }
                }
                /**
                * 매치가 진행중이면, 현재 매치 기록을 로컬에 저장..
                * 비정상 종료에 따른 매치 경기 기록 서버 전송을 위해서...
                */
                if(IsMatch())
                {
                    GetInstance<MatchView>().SaveRecordMatchData();
                }
            }
            /**
            * 로컬시간을 저장. 서스펜드모드에서 돌아오면 해당 시간을 다시 계산을 위해서..
            */
            m_lSuspendTime = DateTime.Now.Ticks;
            // PlayerPrefs.SetString("st" ,DateTime.Now.Ticks.ToString());
            StateMachine.PauseSchedulerAndActions();
            SoundManager.Instance.Pause();
            /**
            * 로컬 게임 알림 등록
            */
            SingleFunc.SendLocalNotification();
            pGameContext.SaveUserData(true);
        }
        else
        {
            /**
            * 로컬 게임 알림 등록 취소
            */
            SingleFunc.CancelLocalNotification();
            if(pGameContext.IsLoadGameData())
            {
                /**
                * websocket 상태 체크후 재연경
                */
                if(!NetworkManager.IsSocketAlive())
                {
                   NetworkManager.ConnectSoketServer();
                }

                /**
                * 각 타이머 지연시간 계산및 적용
                */
                TimeUpdate((float)(DateTime.Now.Ticks - m_lSuspendTime) / (float)TimeSpan.TicksPerSecond);
                m_lSuspendTime =0;
                /**
                * TimeUpdate 에서 진행중인 옥션과, 옥션 리스트 갱신 처리를 했다면, 
                * Transfer에서 RequestAuctionGet를 요청했다면, 응답후 내부적으로 다시 동기화를 진행한다.
                */
                if(!GetInstance<Transfer>().IsRequestAuctionGet())
                {
                    /**
                    * 서스펜드전에 진행중인 옥션 동기화 작업을 진행해야 한다.
                    * 입찰에 참여하지 않고 있는 옥션에서 시간이 연장이 되어도 참여 할수 없기 때문에 해당 UI를 내부에서(TimeUpdate) 닫는다.
                    */
                    pGameContext.ConnectCurrentAuction();
                }

                if(IsShowInstance<PlayerInfo>())
                {
                    GetInstance<PlayerInfo>().CurrentJoinAuctionRoom();
                }

                NetworkManager.SetupRefreshTime();
            }

            StateMachine.ResumeSchedulerAndActions();
            SoundManager.Instance.Resume();
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("----------OnApplicationQuit");
        GameContext pGameContext = GameContext.getCtx();
        if(NetworkManager.IsSocketAlive())
        {
            pGameContext.PauseCurrentAuction(0);
            pGameContext.LeaveCurrentAuction();
        }

        if(IsMatch())
        {
            GetInstance<MatchView>().SaveRecordMatchData();
        }

        pGameContext.SaveUserData(true);
        StateMachine.PauseSchedulerAndActions();
        StateMachine.UnscheduleUpdate();
        SingleFunc.SendLocalNotification();
        Director.Instance.Dispose();
    }

    public void OnEnter()
    {
        Director.SetApplicationFocus(OnApplicationFocus);
        Director.SetApplicationPause(OnApplicationPause);
        Director.SetApplicationQuit(OnApplicationQuit);
        
        LayoutManager layoutManager = LayoutManager.Instance;
        RectTransform ui = layoutManager.GetItem<RectTransform>("MainUI");
        ui.gameObject.name = "MainUI";
        layoutManager.SetMainUI(ui);
        ui.Find("bg").gameObject.SetActive(false);
        if(GameContext.getCtx().RefreshSelfAreaRect())
        {
            RefreshRect();
        }

        StateMachine.ScheduleUpdate(true);
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),-1, (uint)E_STATE_TYPE.Timer, null, executeUpdateCallback,null,-1);
        StateMachine.GetStateMachine().AddState(pBaseState);
        
        m_pMatchButton = layoutManager.FindUI<Button>("MenuBottom/match/bg");
        m_pMatchButton.enabled = false;
        m_pMatchButton.transform.parent.Find("icon").gameObject.SetActive(false);
        m_pMatchButton.transform.parent.SetParent(ui);
        
        SingleFunc.SetupLocalizingText(ui);
        
        m_iCurrentUserRank = GameContext.getCtx().GetCurrentUserRank();
        SetupUI(false);

        GetInstance<Management>();
        GetInstance<Squad>();
        GetInstance<Store>();
        GetInstance<TacticsFormation>();
        GetInstance<Transfer>();
        GetInstance<MatchView>();
        GetInstance<Chat>();
        
        if(!ActorManager.IsBaseResource("CompleteBusiness"))
        {
            ActorManager.AddBaseResource(new CompleteEffectActor(this,AFPool.GetItem<RectTransform>("Object","CompleteBusiness"),"CompleteBusiness"));
        }

        if(!ActorManager.IsBaseResource("GaugeEffect"))
        {
            ActorManager.AddBaseResource(new CompleteEffectActor(this,AFPool.GetItem<RectTransform>("Object","GaugeEffect"),"GaugeEffect"));
        }
        
        ui.gameObject.SetActive(false);

        StateMachine.ResumeSchedulerAndActions();
    }
    public void OnTransitionEnter()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(!m_bCreateClub)
        {
            if(pGameContext.IsCurrentAuction())
            {
                RequestAfterCall(E_REQUEST_ID.auction_get,null);
            }
            UpdateMatchGauge();
            ShowStartPopup();
        }
        else
        {
            ShowMatchButton(false);
        }
        GetInstance<Ground>().PlayGroundObject();
        m_bCreateClub = false;
        LayoutManager pLayoutManager = LayoutManager.Instance;
        pLayoutManager.GetMainUI().gameObject.SetActive(true);
        LayoutManager.SetReciveUIButtonEvent(pLayoutManager.FindUI<RectTransform>("MenuTop"),ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(pLayoutManager.FindUI<RectTransform>("MenuBottom"),ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(m_pMatchButton.transform.parent.GetComponent<RectTransform>(),ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(pLayoutManager.FindUI<RectTransform>("SubMenu"),ButtonEventCall);
        SoundManager.Instance.ChangeBGM("bgm_main",true,0.4f);
        if(!pGameContext.IsRewardVideoLoaded() && !pGameContext.IsRewardVideoLoading())
        {
            pGameContext.LoadRewardVideo();
        }
    }
    public void OnExit()
    {
        // superBlurFast.enabled = false;
        Director.SetApplicationFocus(null);
        Director.SetApplicationPause(null);
        Director.SetApplicationQuit(null);
    }

    void CallHomeResetRequeset()
    {
        RequestAfterCall(E_REQUEST_ID.home_reset,null,()=> {
            GameContext pGameContext = GameContext.getCtx();
            pGameContext.SendReqAttendReward();
            pGameContext.SendReqClubLicensePut(false);
            int matchType = pGameContext.GetCurrentMatchType();
            if(pGameContext.IsLeagueOpen() && matchType > GameContext.CHALLENGE_ID)
            {
                JObject pJObject = new JObject();
                pJObject["matchType"] = matchType;
                RequestAfterCall(E_REQUEST_ID.league_getTodayFixture,pJObject);
            } 
            NetworkManager.RefreshServerDataComplete();
        });
    }

    public void RefreshServerData()
    {
        GameContext pGameContext = GameContext.getCtx();
        ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_TOKEN_EXPIRED"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),CallHomeResetRequeset);
    }

    public T GetInstance<T>() where T : IBaseUI, new()
    {
        string key = typeof(T).Name;
     
        if(m_pUIClassList.ContainsKey(key))
        {
            if(key == "Message" || key == "Confirm")
            {
                if(m_pUIClassList[key].MainUI.gameObject.activeSelf)
                {
                    if(m_pMutiUIList.ContainsKey(key))
                    {
                        for(int i =0; i < m_pMutiUIList[key].Count; ++i)
                        {
                            if(!m_pMutiUIList[key][i].MainUI.gameObject.activeSelf)
                            {
                                return (T)m_pMutiUIList[key][i];
                            }
                        }
                    }
                    else
                    {
                        m_pMutiUIList[key] = new List<IBaseUI>();
                    }
                    
                    T temp1 = new T();

                    RectTransform tmepUI = LayoutManager.Instance.FindUI<RectTransform>(key);
                    tmepUI.localScale =  Vector2.one;
                    tmepUI.SetParent(LayoutManager.Instance.GetMainCanvas(),false);
                    tmepUI.SetAsLastSibling();

                    temp1.OnInit(this,tmepUI);
                    temp1.MainUI.gameObject.name = key;
                    SingleFunc.SetupLocalizingText(temp1.MainUI);
                    m_pMutiUIList[key].Add(temp1);
                    return temp1;
                }
            }

            return (T)m_pUIClassList[key];
        }
        
        T temp = new T();
        if(key == "Message" || key == "Confirm")
        {
            temp.OnInit(this,LayoutManager.Instance.FindUIFormRoot<RectTransform>(key));
        }
        else
        {
            temp.OnInit(this,LayoutManager.Instance.FindUI<RectTransform>(key));
        }
        temp.MainUI.gameObject.name = key;
        m_pUIClassList[key] = temp;
        SingleFunc.SetupLocalizingText(temp.MainUI);
        return temp;
    }

    public bool IsShowInstance<T>() where T : IBaseUI
    {
        string key = typeof(T).Name;
     
        if(m_pUIClassList.ContainsKey(key))
        {
            if(m_pMutiUIList.ContainsKey(key))
            {
                for(int i =0; i < m_pMutiUIList[key].Count; ++i)
                {
                    if(m_pMutiUIList[key][i].MainUI.gameObject.activeSelf)
                    {
                        return true;
                    }
                }
            }
            
            return m_pUIClassList[key].MainUI.gameObject.activeSelf;
        }
        
        return false;
    }

    public void AllInstanceClose(bool bAll)
    {
        if(GameContext.getCtx().GetTutorial() < 44) return;
        var itr = m_pUIClassList.GetEnumerator();
        GameObject obj = null;

        while(itr.MoveNext())
        {
            obj = itr.Current.Value.MainUI.gameObject;
            if(obj.activeSelf)
            {
                if(bAll || obj.name != "MatchView" || obj.name != "Match" )
                {
                    itr.Current.Value.Close();
                }
            }
        }
        obj = null;
    }

    public IBaseUI GetInstanceByUI(RectTransform ui)
    {
        var itr = m_pUIClassList.GetEnumerator();
        while(itr.MoveNext())
        {
            if(itr.Current.Value.MainUI == ui)
            {
                return itr.Current.Value;
            }
        }

        var it = m_pMutiUIList.GetEnumerator();
        while(it.MoveNext())
        {
            for(int i =0; i < it.Current.Value.Count; ++i)
            {
                if(it.Current.Value[i].MainUI == ui)
                {
                    return it.Current.Value[i];
                }
            }
        }

        return null;
    }

    public void Dispose()
    {
        m_pTimeSale = null;
        m_pTimeSaleText = null;
        m_pAwayEmblem = null;
        m_pAdGroup = null;
        m_pUserName = null;
        m_pChatNoti = null;
        m_pUserRank = null;
        m_pUserRankIcon.texture = null;
        m_pUserRankNumIcon.texture = null;
        m_pUserRankIcon = null;
        m_pUserRankNumIcon = null;
        m_pUserGameMoney = null;
        m_pUserToken = null;
        m_pMatchGauge = null;
        m_pMatchEffect = null;
        m_pMenu = null;
        int i =0;
        for(i =0; i < m_pNotiIconList.Length; ++i)
        {
            m_pNotiIconList[i] = null;
        }
        m_pNotiIconList = null;
        m_pUnreadMailCount = null;
        m_pEmblem?.Dispose();
        m_pEmblem = null;
        m_pMainButtons = null;
        m_pRecoveryButton = null;
        var itr = m_pUIClassList.GetEnumerator();
        LayoutManager pLayoutManager = LayoutManager.Instance;
        while(itr.MoveNext())
        {
            pLayoutManager.AddItem(itr.Current.Key,itr.Current.Value.MainUI);
            itr.Current.Value.Dispose();
        }

        var it = m_pMutiUIList.GetEnumerator();
            
        while(it.MoveNext())
        {
            for(i =0; i < it.Current.Value.Count; ++i)
            {
                if(it.Current.Value[i].MainUI != null)
                {
                    it.Current.Value[i].MainUI.gameObject.SetActive(false);
                }
                pLayoutManager.AddItem(it.Current.Key,it.Current.Value[i].MainUI);
                it.Current.Value[i].Dispose();
                it.Current.Value[i] = null;
            }
            it.Current.Value.Clear();
        }
        m_pMutiUIList.Clear();
        m_pMutiUIList = null;

        m_pUIClassList.Clear();
        m_pUIClassList = null;
        

        // LayoutManager.Instance.RemoveMainUI();
    }

    public void ShowTimeSale(bool bActive, int time)
    {
        if(m_pTimeSale.gameObject.activeSelf != bActive)
        {
            m_pTimeSale.gameObject.SetActive(bActive);
            if(bActive)
            {
                m_pTimeSale.Play();
            }
        }
        
        if(bActive)
        {
            SingleFunc.UpdateTimeText(time,m_pTimeSaleText,0);
        }
    }
    
    public void ShowStartPopup()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.GetTutorial() >= 44)
        {
            JObject data = pGameContext.GetResponData();
            if(data != null)
            {
                E_REQUEST_ID eID = (E_REQUEST_ID)((uint)data["msgId"]);
                RectTransform pTarget = null;
                Action pAction = null;
                switch(eID)
                {
                    case E_REQUEST_ID.timeSale_put:
                    {
                        TimeSale pUI = GetInstance<TimeSale>();
                        pTarget = pUI.MainUI;
                        pUI.SetupData((uint)data["no"]);
                        pAction = () => { pUI.Enable = true; LayoutManager.Instance.InteractableEnabledAll();};
                    }
                    break;
                    case E_REQUEST_ID.review_popup:
                    {
                        pGameContext.ShowNativeReview();
                    }
                    break;
#if USE_HIVE
                    case E_REQUEST_ID.hive_news:
                    {
                        if(!Application.isEditor)
                        {
                            NetworkManager.ShowWaitMark(true);
                            pGameContext.ShowHivePromotionByPromotionType(hive.PromotionType.NEWS,false,OnPromotionViewCB);
                        }
                    }
                    break;
#endif
                    case E_REQUEST_ID.attend_reward:
                    case E_REQUEST_ID.mileage_reward:
                    {
                        if(data.ContainsKey("LicenseComplete"))
                        {
                            ClubLicenseComplete pUI = GetInstance<ClubLicenseComplete>();
                            pTarget = pUI.MainUI;
                            pUI.SetupData(data);
                            SoundManager.Instance.PlaySFX("sfx_license_complete");
                            pAction = () => { pUI.PlayEffect(); };
                        }
                        else if(data.ContainsKey("ContentsUnlock"))
                        {
                            ContentsUnlock pUI = GetInstance<ContentsUnlock>();
                            pTarget = pUI.MainUI;
                            pUI.SetupData(data);
                            pAction = () => { pUI.PlayEffect(); };
                        }
                        else
                        {
                            Reward pUI = GetInstance<Reward>();
                            pTarget = pUI.MainUI;
                            pUI.SetupRewardItems(data);
                            pAction = () => { 
                                LayoutManager.Instance.InteractableEnabledAll();
                                pUI.ShowTab();
                                pUI.ShowRewardItems(); 
                            };
                        }   
                    }
                    break;
                    case E_REQUEST_ID.attend_get:
                    {
                        AttendReward pUI = GetInstance<AttendReward>();
                        pTarget = pUI.MainUI;
                        pUI.UpdateData(data);
                        pAction = () => { pUI.ShowReward();};
                    }
                    break;
                    case E_REQUEST_ID.ladder_rewardStanding:
                    {
                        SeasonRankingReward pUI = GetInstance<SeasonRankingReward>();
                        pTarget = pUI.MainUI;
                        pUI.SetupData(data);
                    }
                    break;
                    case E_REQUEST_ID.ladder_rewardUserRank:
                    {
                        SeasonReward pUI = GetInstance<SeasonReward>();
                        pTarget = pUI.MainUI;
                        pUI.SetupData(data);
                    }
                    break;
                    case E_REQUEST_ID.auction_trade:
                    {
                        Conference pUI = GetInstance<Conference>();
                        pTarget = pUI.MainUI;
                        pUI.SetupData(data);
                        pAction = () => { pUI.Show(); LayoutManager.Instance.InteractableEnabledAll();};
                    }
                    break;
                    case E_REQUEST_ID.auction_refund:
                    {
                        pTarget = ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_TRANSFER_AUCTION_BID_FAIL"),null);
                        pAction = () => { LayoutManager.Instance.InteractableEnabledAll();};
                    }
                    break;
                }
                if(pTarget != null)
                {
                    pTarget.SetAsLastSibling();
                    if(eID != E_REQUEST_ID.auction_refund) 
                    {
                        SingleFunc.ShowAnimationDailog(pTarget,pAction,eID == E_REQUEST_ID.timeSale_put ? "TimeSale_open":"Popup_open");
                    }
                    
                    BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pTarget),-1, (uint)E_STATE_TYPE.ShowStartPopup, null, this.executeStartPopupCallback,null);
                    StateMachine.GetStateMachine().AddState(pBaseState);
                    return;
                }
            }
            
            GetInstance<Chat>().JoinChatServer();
        }
    }

    public void CloseSoket()
    {
        if(Application.internetReachability != NetworkReachability.NotReachable && NetworkManager.IsSocketAlive())
        {
            GetInstance<Chat>().LeaveRoom(true);
            GameContext.getCtx().LeaveCurrentAuction();
        }
        
        NetworkManager.CloseSoket();
    }

    public void SharePlayerInfoChatUI(ulong playerID)
    {
        GetInstance<Chat>().SharePlayerInfo(playerID);
    }

    public void UpdateTopChatMsg(string msg)
    {        
        m_pChatNoti.SetText(msg);
        if(m_pChatNoti.rectTransform.rect.width < m_pChatNoti.preferredWidth)
        {
            while(msg.Length > 0)
            {
                msg = msg.Substring(0,msg.Length - 2);
                m_pChatNoti.SetText(msg);
                if(m_pChatNoti.rectTransform.rect.width >= m_pChatNoti.preferredWidth)
                {
                    break;
                }
            }
        }
    }
    public void RunIntroSecne(E_LOGIN_TYPE eLoginType)
    {
        GameContext pGameContext = GameContext.getCtx();
        pGameContext.ClearAllExpireTime();
        pGameContext.ResetDeviceOrientation();
        CloseSoket();
        NetworkManager.ClearServerUrl();
        if(eLoginType != E_LOGIN_TYPE.direct )
        {
            NetworkManager.ClearServerAccessToken();
        }
        
        Director.Instance.RunWithScene(IntroScene.Create(eLoginType),0);
    }

    IEnumerator coShowInterstitial()
    {
        bool isOK = false;
#if DEBUG_CHEAT
        if(!GameContext.getCtx().AD)
        {
            isOK = true;
        }
        else
#endif
        while(!isOK)
        {
            yield return null;
        }
    }

    void RefreshRect()
    {
        RectTransform tm = LayoutManager.Instance.GetMainUI();
        Vector2 size = tm.offsetMax;
        tm.Find("bg").gameObject.SetActive(true);
        
        float H = size.y == CONST_TOP_HEIGHT ? 0: -100;
        size.y = H;
        tm.offsetMax = size;
    }

    void UpdateLeagueTodayInfo()
    {
        GameContext pGameContext = GameContext.getCtx();
        Transform tm = LayoutManager.Instance.FindUI<Transform>("SubMenu");
        // if(pGameContext.IsLicenseContentsUnlock(E_CONST_TYPE.licenseContentsUnlock_5) && pGameContext.IsLeagueOpen())
        if(pGameContext.IsJoinLeague())
        {
            tm.Find("league").gameObject.SetActive(true);
            UpdateLeagueText(tm.Find("league/name/text").GetComponent<LocalizingText>());
            UpdateLeagueTodayCount();
        }
        else
        {
            tm.Find("league").gameObject.SetActive(false);
        }   
    }

    void SetupUI(bool bBGM = true)
    {
        // adRewardAdCollect = 1;
        // superBlurFast.enabled = false;
        LayoutManager layoutManager = LayoutManager.Instance;
        
        Ground pUI = GetInstance<Ground>();
        pUI.MainUI.SetAsFirstSibling();

        m_pEmblem = layoutManager.FindUI<EmblemBake>("MenuTop/userProfile/emblem");
        m_pUserName = layoutManager.FindUI<TMPro.TMP_Text>("MenuTop/userName");
        m_pUserRank = layoutManager.FindUI<TMPro.TMP_Text>("MenuTop/userRank/userRank");
        m_pUserRankIcon = m_pUserRank.transform.parent.Find("icon").GetComponent<RawImage>();
        m_pUserRankNumIcon= m_pUserRankIcon.transform.Find("num").GetComponent<RawImage>();
        m_pUserGameMoney = layoutManager.FindUI<TMPro.TMP_Text>("MenuTop/gameMoney/text");
        m_pUserToken = layoutManager.FindUI<TMPro.TMP_Text>("MenuTop/token/text");
        Transform tm = layoutManager.FindUI<Transform>("SubMenu");
        m_pMenu = tm.Find("menu/box");
        m_pAdGroup = tm.Find("ad").GetComponent<RectTransform>();
        m_pMenu.gameObject.SetActive(false);
        m_pTimeSale = tm.Find("timeSale").GetComponent<Animation>();
        m_pTimeSaleText = m_pTimeSale.transform.Find("timer/text").GetComponent<TMPro.TMP_Text>();
        m_pTimeSale.gameObject.SetActive(false);

        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.GetCurrentLangauge() == E_DATA_TYPE.ko_KR)
        {
            m_pMenu.Find("forum/facebook").gameObject.SetActive(false);
            m_pMenu.Find("forum/naver").gameObject.SetActive(true);
        }
        else
        {
            m_pMenu.Find("forum/facebook").gameObject.SetActive(true);
            m_pMenu.Find("forum/naver").gameObject.SetActive(false);
        }
        
        for(E_NOTICE_NODE e = E_NOTICE_NODE.menu; e < E_NOTICE_NODE.MAX; ++e)
        {
            m_pNotiIconList[(int)e] = tm.Find($"{e}/noti").GetComponent<Animation>();
        }
        
        UpdateLeagueTodayInfo();

        m_pMenuBack = tm.Find("back");
        m_pMenuBack.gameObject.SetActive(false);
        m_pChatNoti = tm.Find("chat/text").GetComponent<TMPro.TMP_Text>();
        m_pUnreadMailCount = m_pMenu.Find("mailbox/count/text").GetComponent<TMPro.TMP_Text>();
        m_pMainButtons = layoutManager.FindUI<RectTransform>("MenuBottom/buttons");

        float w = (m_pMainButtons.rect.width / m_pMainButtons.childCount);
        float wh = w * 0.5f;
        
        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        float ax = m_pMainButtons.pivot.x * m_pMainButtons.rect.width;
        int i =0;
        for(i =0; i < m_pMainButtons.childCount; ++i)
        {
            item = m_pMainButtons.GetChild(i).GetComponent<RectTransform>();
            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (i * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
            item.Find("on").gameObject.SetActive(false);
        }

        for(i =0; i < m_pAdGroup.childCount; ++i)
        {
            m_pAdGroup.GetChild(i).gameObject.SetActive(false);
        }
        
        m_pMatchGauge = m_pMatchButton.transform.parent.Find("gauge/fill").GetComponent<Image>();
        m_pMatchEffect = m_pMatchButton.transform.Find("effect").GetComponent<Animation>();
        m_pRecoveryButton = m_pMatchButton.transform.parent.Find("recovery").GetComponent<Button>();
        m_pRecoveryButton.gameObject.SetActive(false);
        bool bActive = false;
        if(m_bCreateClub)
        {
            // ShowResearch();
            pGameContext.CreateDummyUser();
            ShowCreateClub();
        }
        else
        {
            UpdateTopUI();

            if(!ShowTutorial())
            {
                bActive = true;
                SetupTimer();
            }
            pGameContext.UpdateNoticeIcon(E_REQUEST_ID.home_get,this);
        }

        ShowMainUI(bActive);
        ShowTopSubUI(bActive);
        ActiveMatch(bActive);
    }
    public void ShowMatchButton(bool bShow)
    {
        m_pMatchButton.transform.parent.gameObject.SetActive(bShow);
    }

    public void ShowMainUI(bool bShow)
    {
        m_pMainButtons.transform.parent.gameObject.SetActive(bShow);
        m_pUserName.transform.parent.gameObject.SetActive(bShow);
    }

    public void ShowTopSubUI(bool bShow)
    {
        m_pMenuBack.parent.gameObject.SetActive(bShow);
    }

    public void EnableTutorial(byte eStep)
    {
        LayoutManager.Instance.InteractableDisableAll();
        if(eStep == 5 || eStep == 6)
        {
            Management pManagement = GetInstance<Management>();
            pManagement.Enable = false;
            Button ui = pManagement.GetBusinessItemUI(0,"btn_up").GetComponent<Button>();
            ui.interactable = true;
        }
    }

    public void StartMatch(MatchTeamDataT pMatchTeamData, EmblemBake[] pEmblemBakeList,int iMatchType)
    {
        ShowBackground(false);
        MatchView pUI = GetInstance<MatchView>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetupLineup(pMatchTeamData,pEmblemBakeList,iMatchType);
        ShowMoveDilog(pUI.MainUI,Vector3.left);
    }

    public void UpdateLeagueTodayCount()
    {
        GameContext pGameContext = GameContext.getCtx();
        int id = pGameContext.GetCurrentMatchType();
        if(id == GameContext.LADDER_ID)
        {
            return;
        }

        Transform tm = m_pAdGroup.parent.Find("league");
        TMPro.TMP_Text text = tm.Find("count/text").GetComponent<TMPro.TMP_Text>();
        
        if(id == GameContext.CHALLENGE_ID)
        {
            text.SetText($"{pGameContext.GetCurrentChallengeTicket()}/{pGameContext.GetConstValue(E_CONST_TYPE.challengeStageMatchChanceMax)}");
        }
        else
        {
            text.SetText($"{pGameContext.GetLeagueTodayMatchCount()}/{pGameContext.GetLeagueTodayMatchMax()}");
        }
    }

    void UpdateLeagueText(LocalizingText pLocalizingText)
    {
        if(pLocalizingText != null)
        {
            pLocalizingText.Key = GameContext.getCtx().GetCurrentMatchType() > GameContext.CHALLENGE_ID ? "MAIN_BTN_LEAGUE" : "MAIN_BTN_CHALLENGE";
            pLocalizingText.UpdateLocalizing();
        }
    }

    IState exitDilogCallbackForTutorialStart(IState state)
    {
        exitDilogMoveCallback(state);
        
        Tutorial pUI = GetInstance<Tutorial>();
        pUI.MainUI.SetAsLastSibling();
        pUI.SetTutorialStep(GameContext.getCtx().GetTutorial());
        SingleFunc.ShowAnimationDailog(pUI.MainUI,pUI.PlayTutorialStep);
        return null;
    }

    public void SetupNoticeIcon(E_NOTICE_NODE eNode,bool bShow)
    {
        if(eNode == E_NOTICE_NODE.NONE)
        {
            for(E_NOTICE_NODE i = E_NOTICE_NODE.menu; i < E_NOTICE_NODE.MAX; ++i)
            {
                m_pNotiIconList[(int)i].gameObject.SetActive(bShow);
            }
        }
        else if(eNode < E_NOTICE_NODE.MAX )
        {
            m_pNotiIconList[(int)eNode].gameObject.SetActive(bShow);
            if(bShow)
            {
                m_pNotiIconList[(int)eNode].Play();
            }
        }
    }

    bool ShowTutorial()
    {
        byte eTutorialStep = GameContext.getCtx().GetTutorial();
        eTutorialStep = 29;
        if(eTutorialStep < 44)
        {
            if(eTutorialStep >= 4 && eTutorialStep < 7)
            {
                Management pUI = GetInstance<Management>();
                if( !pUI.MainUI.gameObject.activeSelf)
                {
                    pUI.MainUI.SetAsLastSibling();
                    pUI.ShowTabUI(0);
                    pUI.Enable = false;
                    if(eTutorialStep == 6)
                    {
                        ScrollRect pScroll = pUI.MainUI.GetComponentInChildren<ScrollRect>();
                        pScroll.content.GetChild(0).Find("muti_up").gameObject.SetActive(true);
                    }
                    ShowMoveDilog(pUI.MainUI,Vector3.left,false,exitDilogCallbackForTutorialStart);
                    return eTutorialStep < 5;
                }
            }
            else if(eTutorialStep >= 11 && eTutorialStep < 13)
            {
                if(eTutorialStep == 12)
                {
                    GameContext.getCtx().SetTutorial(11);
                }
                Transfer pUI = GetInstance<Transfer>();
                if( !pUI.MainUI.gameObject.activeSelf)
                {
                    if(GameContext.getCtx().IsExpireRecuitingCachePlayerData())
                    {
                        RequestAfterCall(E_REQUEST_ID.tutorial_getRecruit,null,()=>{
                            pUI.MainUI.SetAsLastSibling();
                            pUI.SetupTimer();
                            pUI.ShowTabUI(1);
                            m_pMainButtons.parent.SetAsLastSibling();
                            ShowMoveDilog(pUI.MainUI,Vector3.left,false,exitDilogCallbackForTutorialStart);
                        });
                    }
                    else
                    {
                        pUI.MainUI.SetAsLastSibling();
                        pUI.SetupTimer();
                        pUI.ShowTabUI(1);
                        m_pMainButtons.parent.SetAsLastSibling();
                        ShowMoveDilog(pUI.MainUI,Vector3.left,false,exitDilogCallbackForTutorialStart);
                    }
                    
                    return false;
                }
            }
            else if(eTutorialStep >= 16 && eTutorialStep < 21)
            {
                eTutorialStep = 21;
            }
            else if(eTutorialStep >= 21 && eTutorialStep < 24)
            {
                eTutorialStep = 24;
            }
            // else if(eTutorialStep >= 24 && eTutorialStep < 45)
            // {
            //     eTutorialStep = 44;
            // }
            
            Tutorial pTutorial = GetInstance<Tutorial>();
            pTutorial.MainUI.SetAsLastSibling();
            pTutorial.SetTutorialStep(eTutorialStep);
            SingleFunc.ShowAnimationDailog(pTutorial.MainUI,pTutorial.PlayTutorialStep);
            
            return eTutorialStep < 5;
        }

        return false;
    }

    public RectTransform GetMatchButton()
    {
        return m_pMatchButton.transform.parent.GetComponent<RectTransform>();
    }
    
    public bool IsMatch()
    {
        return IsShowInstance<Match>() || IsShowInstance<MatchView>();
    }
    
    public void ActiveMatch(bool bActive)
    {
        m_pMatchButton.enabled = bActive;
        ShowMatchButton(bActive);
    }

    public void FocusBuildingObject(uint id)
    {
        GetInstance<Ground>().FocusBuildingObject(id);
    }

    public void UpdateBuildingTimer(bool bBusiness, E_BUILDING eType)
    {
        GetInstance<Ground>().UpdateBuildingTimer(bBusiness,eType);
    }

    public EmblemBake GetMyEmblem()
    {
        return m_pEmblem;
    }

    public Material GetMyEmblemMaterial()
    {
        return m_pEmblem.material;
    }

    public void UpdateScrollLineUpPlayer()
    {
        GetInstance<Squad>().UpdateScrollLineUpPlayer();
    }

    public void UpdateBuilding(bool bBusiness, E_BUILDING eType)
    {
        GetInstance<Ground>().UpdateBuilding(bBusiness,eType);
    }

    void UpdateRank()
    {
        GameContext pGameContext = GameContext.getCtx();
        byte rank = pGameContext.GetCurrentUserRank();
        if(m_iCurrentUserRank != rank)
        {
            GetInstance<Squad>().UpdateAllPlayerData();
            GetInstance<TacticsFormation>().UpdateAllPlayerData();
        }

        m_iCurrentUserRank = rank;
        SingleFunc.SetupRankIcon(m_pUserRankIcon,rank);

        UserRankList pUserRankList = pGameContext.GetFlatBufferData<UserRankList>(E_DATA_TYPE.UserRank);
        UserRankItem? pUserRankItem = pUserRankList.UserRankByKey(rank);
        m_pUserRank.SetText(pGameContext.GetLocalizingText(pUserRankItem.Value.Name));
    }

    public void UpdateTopEmblem()
    {
        m_pEmblem.SetupEmblemData(GameContext.getCtx().GetEmblemInfo());
        
        // GameObject mainUI = LayoutManager.Instance.GetMainUI().gameObject;
        // mainUI.SetActive(false);
        // mainUI.SetActive(true);
        if(IsShowInstance<Profile>())
        {
            Profile pProfile = GetInstance<Profile>();
            pProfile.MainUI.gameObject.SetActive(false);
            pProfile.MainUI.gameObject.SetActive(true);
        }

        if(IsShowInstance<EditProfile>())
        {
            EditProfile pEditProfile = GetInstance<EditProfile>();
            pEditProfile.MainUI.gameObject.SetActive(false);
            pEditProfile.MainUI.gameObject.SetActive(true);
        }
    }

    public void UpdateTopClubName()
    {
        m_pUserName.SetText(GameContext.getCtx().GetClubName());
    }
    public void UpdateTopUI()
    {
        UpdateTopClubName();
        UpdateTopEmblem();
        UpdateRank();
        UpdateGameMaterials();
        UpdateUnreadMailCount();
    }

    public void UpdateGameMaterials()
    {
        GameContext gtx = GameContext.getCtx();
        m_pUserGameMoney.SetText(ALFUtils.NumberToString(gtx.GetGameMoney()));
        m_pUserToken.SetText(string.Format("{0:#,0}", gtx.GetTotalCash()));
    }

    public void UpdateUnreadMailCount()
    {
        GameContext gtx = GameContext.getCtx();
        uint count = gtx.GetUnreadMailCount();
        
        if(count > 0)
        {
            m_pUnreadMailCount.transform.parent.gameObject.SetActive(true);
            m_pUnreadMailCount.SetText(count.ToString());
            m_pUnreadMailCount.ForceMeshUpdate(true,true);
            RectTransform tm = m_pUnreadMailCount.transform.parent.GetComponent<RectTransform>();
            Vector2 size = tm.sizeDelta;
            size.x = m_pUnreadMailCount.textBounds.size.x + 40;
            tm.sizeDelta = size;
        }
        else
        {
            m_pUnreadMailCount.transform.parent.gameObject.SetActive(false);
        }
    }

    public void RequestAfterCall(E_REQUEST_ID id, JObject data, System.Action endCallback = null,bool bLoading = true)
    {
        if(endCallback != null)
        {
			if(m_pRequestAfterCallList.ContainsKey(id))
            {
            	m_pRequestAfterCallList[id] = endCallback;
            }
            else
            {
                m_pRequestAfterCallList.Add(id,endCallback);
            }
        }
        
        NetworkManager.Instance.SendRequest((uint)id, GameContext.getCtx().GetNetworkAPI(id),bLoading,true,null,data);
        if(id == E_REQUEST_ID.business_levelUp || id == E_REQUEST_ID.player_abilityUp || id == E_REQUEST_ID.player_positionFamiliarUp || id == E_REQUEST_ID.business_reward || id == E_REQUEST_ID.business_rewardTraining || id == E_REQUEST_ID.tutorial_business )
        {
            NetworkManager.EnableWaitMark(false);
        }
    }

    public void RemoveRequestAfterCallback(E_REQUEST_ID id)
    {
        if(m_pRequestAfterCallList.ContainsKey(id))
        {
            m_pRequestAfterCallList.Remove(id);
        }
    }
    
    public void ShowDilog(RectTransform dlg,bool bHide = false)
    {
        if(dlg == null) return;
        BaseState pBaseState = null;
        if(bHide)
        {
            pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),0.3f, (uint)E_STATE_TYPE.ShowDailog, this.enterDilogCallback, this.executeDilogCallback, this.exitHideCallback );
        }
        else
        {
            pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),0.3f, (uint)E_STATE_TYPE.ShowDailog, this.enterDilogCallback, this.executeDilogCallback,this.exitDilogCallback );
        }
        
        DilogMoveStateData data = new DilogMoveStateData();
        data.FadeDelta = 1 / 0.3f;
        data.Out = false;
        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    public void HideDilog(RectTransform dlg,System.Func<IState,IState> _exitCallback = null)
    {
        if(dlg == null) return;
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),0.3f, (uint)E_STATE_TYPE.HideDailog, this.enterDilogCallback, this.executeDilogCallback, _exitCallback == null ? this.exitDilogCallback :_exitCallback);
        DilogMoveStateData data = new DilogMoveStateData();
        data.FadeDelta = 1 / 0.3f;
        data.Out = true;
        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    public void ShowMoveDilog(RectTransform dlg,Vector3 dir, bool bAni = true,System.Func<IState,IState> _exitCallback = null)
    {
        SingleFunc.ShowMoveDilog(dir,dlg,this.enterDilogMoveCallback, this.executeDilogMoveCallback, _exitCallback == null ? this.exitDilogMoveCallback :_exitCallback,bAni);
        // superBlurFast.enabled = true;
    }

    public void HideMoveDilog(RectTransform dlg,Vector3 dir, bool bAni = true,System.Func<IState,IState> _exitCallback = null)
    {
        SingleFunc.HideMoveDilog(dir, dlg,this.enterDilogMoveCallback, this.executeDilogMoveCallback, _exitCallback == null ? this.exitDilogMoveCallback :_exitCallback,bAni);
    }
   
    bool SetBottomTab( Transform select)
    {
        if(m_pMenu.gameObject.activeSelf)
        {
            ShowMenuPopup(false);
        }
        
        Transform on = null;
        RectTransform dlg = null;
        
        int i =0;
        Transform tm = null;
        for(i =0; i < m_pMainButtons.childCount; ++i)
        {
            tm = m_pMainButtons.GetChild(i);
            on = tm.Find("on");
            if(on.gameObject.activeSelf)
            {
                switch(on.parent.gameObject.name)
                {
                    case "Management":
                    {
                        GetInstance<Management>().Close();
                    }
                    break;
                    case "Squad":
                    {
                        GetInstance<Squad>().Close();
                    }
                    break;
                    case "TacticsFormation":
                    {
                        GetInstance<TacticsFormation>().Close();
                    }
                    break;
                    case "Transfer":
                    {
                        GetInstance<Transfer>().Close();
                    }
                    break;
                    case "Store":
                    {
                        GetInstance<Store>().Close();
                    }
                    break;
                }
                
                dlg = LayoutManager.Instance.FindUI<RectTransform>(on.parent.gameObject.name);

                break;
            }
        }
        
        on = select.Find("on");
        
        if(on != null)
        {
            select.Find("icon").gameObject.SetActive(false);
            if(on.gameObject.activeSelf == false)
            {
                if(dlg != null)
                {
                    tm.Find("on").gameObject.SetActive(false);
                    tm.Find("icon").gameObject.SetActive(true);
                    tm.Find("title").GetComponent<TMPro.TMP_Text>().color = new Color(0.2784314f,0.3372549f,0.5333334f);
                }

                // ShowMatchButton(false);
                on.gameObject.SetActive(true);
                select.Find("title").GetComponent<TMPro.TMP_Text>().color = Color.white;
                return true;
            }
            else
            {
                if(dlg != null)
                {
                    tm.Find("on").gameObject.SetActive(false);
                    tm.Find("icon").gameObject.SetActive(true);
                    tm.Find("title").GetComponent<TMPro.TMP_Text>().color = new Color(0.2784314f,0.3372549f,0.5333334f);
                }
                
                // ShowMatchButton(true);
                if((!IsShowInstance<Match>() && (!IsShowInstance<MatchView>()|| GetInstance<MatchView>().IsFinishMatchGame())))
                {
                    ActiveMatch(true);
                }
            }
        }
        
        return false;
    }

    public void TutorialMatch()
    {
        ButtonEventCall(null,m_pMatchButton.gameObject);
    }

    /// <summary>
    /// 버튼 이벤트 코드
    /// </summary>
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender == m_pMatchButton.gameObject)
        {
            GameContext pGameContext = GameContext.getCtx();
            if(TODAY_MATCH_MAX <= pGameContext.GetTodayMatchCount())
            {
                ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_MATCHREWARDLIMIT_TITLE"),pGameContext.GetLocalizingText("DIALOG_MATCHREWARDLIMIT_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,() =>{
                    RequestAfterCall(E_REQUEST_ID.ladder_try,null,()=>{ GetInstance<Ground>().FocusStadium(true); });
                } );
            }
            else
            {
                RequestAfterCall(E_REQUEST_ID.ladder_try,null,()=>{ GetInstance<Ground>().FocusStadium(true);});
            }

            return;
        }
        else if(sender.name == "recovery")
        {
            ShowHPRecoveryPopup();
            return;
        }

        switch(root.gameObject.name)
        {
            case "MenuTop":
            {   
                switch(sender.name)
                {
                    case "gameMoney":
                    {
                        if(IsShowInstance<Store>())
                        {
                            GetInstance<Store>().SetScroll(3,"ShopManageItem");
                        }
                        else
                        {
                            RequestAfterCall(E_REQUEST_ID.shop_get,null,()=>{ 
                                SetBottomTab(m_pMainButtons.GetChild(4)); 
                                ShowStore(3,"ShopManageItem");
                                GameContext pGameContext = GameContext.getCtx();
                                if(pGameContext.IsRestorePurchase())
                                {
                                    ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TITLE_IAPRESTORE"),pGameContext.GetLocalizingText("MSG_TXT_IAPRESTORE"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),null,false,()=>{
#if USE_HIVE
                                        NetworkManager.ShowWaitMark(true);
                                        LayoutManager.Instance.InteractableDisableAll(null,true);
                                        SingleFunc.IAPV4Restore(OnIAPV4RestoreCB);
#endif
                                    });
                                }
                            });
                        }
                    }
                    break;
                    case "token":
                    {
                        if(IsShowInstance<Store>())
                        {
                            GetInstance<Store>().SetScroll(3,"ShopCurrencyItem");
                        }
                        else
                        {
                            RequestAfterCall(E_REQUEST_ID.shop_get,null,()=>{ 
                                SetBottomTab(m_pMainButtons.GetChild(4)); 
                                ShowStore(3,"ShopCurrencyItem");
                                GameContext pGameContext = GameContext.getCtx();
                                if(pGameContext.IsRestorePurchase())
                                {
                                    ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TITLE_IAPRESTORE"),pGameContext.GetLocalizingText("MSG_TXT_IAPRESTORE"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),null,false,()=>{
#if USE_HIVE
                                        NetworkManager.ShowWaitMark(true);
                                        LayoutManager.Instance.InteractableDisableAll(null,true);
                                        SingleFunc.IAPV4Restore(OnIAPV4RestoreCB);
#endif
                                    });
                                }
                            });
                        } 
                    }
                    break;
                    case "userProfile":
                    {
                        RequestClubProfile(GameContext.getCtx().GetClubID(),()=>{ShowUserProfile(GameContext.getCtx().GetClubID(),0);});
                    }
                    break;
                }
            }
            break;
            case "SubMenu":
            {   
                switch(sender.name)
                {
                    case "chat":
                    {
                        SetBottomTab(sender.transform);
                        ShowChat();
                    }
                    break;
                    case "menu":
                    {
                        ShowMenuPopup(!m_pMenu.gameObject.activeSelf);
                    }
                    break;
                    case "mailbox":
                    {
                        if(m_pMenu.gameObject.activeSelf)
                        {
                            ShowMenuPopup(false);
                        }
#if !FTM_LIVE
                        // GameContext.getCtx().TestCode(2);
                        // if(!StateMachine.GetStateMachine().IsCurrentTargetStates((uint)E_STATE_TYPE.ShowStartPopup)) 
                        // {
                        //     ShowStartPopup();
                        // }
                        // return;
#endif
                        RequestAfterCall(E_REQUEST_ID.mail_get,null,()=>{ShowMailBoxPopup();});
                    }
                    break;
                    case "clubLicense":
                    {          
                        if(GameContext.getCtx().IsAllClearClubLicenses())
                        {
                            ShowQuestMissionPopup();
                        }
                        else
                        {
                            ShowClubLicensePopup();
                        }
                    }
                    break;
                    case "ranking":
                    {
                        uint no = GameContext.getCtx().GetCurrentSeasonNo();
                        Ranking pRanking = GetInstance<Ranking>();
                        JObject data = pRanking.GetCacheRankingData(no,0);
                        if(data != null)
                        {
                            pRanking.NetworkDataParse(E_REQUEST_ID.club_top100,data);
                            ShowRankingPopup();
                        }
                        else
                        {
                            // seasonNo: 시즌 no
                            // matchType: 시즌 종류 
                            // - 1: ladder
                            // - 10: league
                            data = new JObject();
                            data["seasonNo"] = no;
                            data["matchType"] = GameContext.LADDER_ID;
                            RequestAfterCall(E_REQUEST_ID.club_top100,data,()=>{ShowRankingPopup();});
                        }
                    }
                    break;
                    case "trophy":
                    {
                        ShowTrophyRewardPopup();
                    }
                    break;
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    {
                        ShowADPopup(uint.Parse(sender.name));
                    }
                    break;
                    case "matchLog":
                    {
                        if(m_pMenu.gameObject.activeSelf)
                        {
                            ShowMenuPopup(false);
                        }
                        
                        JObject data = new JObject();
                        data["matchType"] = GameContext.LADDER_ID;
                        RequestAfterCall(E_REQUEST_ID.matchStats_list,data,()=>{ ShowMatchLogPopup();});
                    }
                    break;
                    case "fastReward":
                    {        
                        ShowFastRewardPopup();
                    }
                    break;
                    case "timeSale":
                    {
                        if(m_pMenu.gameObject.activeSelf)
                        {
                            ShowMenuPopup(false);
                        }
                        ShowTimeSaledPopup();
                    }
                    break;
                    case "league":
                    {
                        if(m_pMenu.gameObject.activeSelf)
                        {
                            ShowMenuPopup(false);
                        }

                        ShowLeaguePopup();
                    }
                    break;
                    case "attendance":
                    {
                        if(m_pMenu.gameObject.activeSelf)
                        {
                            ShowMenuPopup(false);
                        }
                
                        ShowAttendRewardPopup();
                    }
                    break;
                    case "seasonPass":
                    {
                        ShowSeasonPass();
                    }
                    break;
                    case "setting":
                    {
                        if(m_pMenu.gameObject.activeSelf)
                        {
                            ShowMenuPopup(false);
                        } 
                        ShowSettingPopup();
                    }
                    break;
                    case "news":
                    {
                        if(m_pMenu.gameObject.activeSelf)
                        {
                            ShowMenuPopup(false);
                        }
                        if(Application.isEditor)
                        {
                            ShowMessagePopup("미지원!!",GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
                        }
                        else
                        {
                            NetworkManager.ShowWaitMark(true);
                            GameContext.getCtx().ShowHivePromotionByPromotionType(hive.PromotionType.NEWS,true,OnPromotionViewCB);
                        }
                    }
                    break;
                    case "forum":
                    {
                        if(m_pMenu.gameObject.activeSelf)
                        {
                            ShowMenuPopup(false);
                        }
                        GameContext pGameContext = GameContext.getCtx();
                        bool bKr = pGameContext.GetCurrentLangauge() == E_DATA_TYPE.ko_KR;
#if FTM_LIVE
                        pGameContext.ShowHiveCustomView(pGameContext.GetConstValue( bKr ? E_CONST_TYPE.NAVER_ROUNGE : E_CONST_TYPE.FACEBOOK_FANPAGE).ToString(),true,null);
#else
                        pGameContext.ShowHiveCustomView(bKr ? "300114" : "300115",true,null);
#endif
                    }
                    break;
                    case "predictionEvent":
                    {
                        ShowMessagePopup(string.Format("{0}: 미구현!!",sender.name),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
                    }
                    break;
                    case "back":
                    {
                        ShowMenuPopup(false);
                    }
                    break;
                }
            }
            break;
            case "MenuBottom":
            {   
                if(sender.gameObject == m_pRecoveryButton)
                {
                    ShowHPRecoveryPopup();
                }
                else
                {
                    if(SetBottomTab(sender.transform))
                    {
                        sender.GetComponent<Button>().enabled = false;
                        switch(sender.name)
                        {
                            case "Management":
                            {
                                ShowManagement();
                            }
                            break;
                            case "Squad":
                            {
                                ShowSquad();
                            }
                            break;
                            case "TacticsFormation":
                            {
                                ShowTacticsFormation();
                            }
                            break;
                            case "Transfer":
                            {
                                if(GameContext.getCtx().IsExpireRecuitingCachePlayerData())
                                {
                                    RequestAfterCall(E_REQUEST_ID.recruit_get,null,()=>{ ShowTransfer(1);});
                                }
                                else
                                {
                                    ShowTransfer(1);
                                }
                            }
                            break;
                            case "Store":
                            {
                                RequestAfterCall(E_REQUEST_ID.shop_get,null,()=>{ 
                                    ShowStore(1);
                                    GameContext pGameContext = GameContext.getCtx();
                                    if(pGameContext.IsRestorePurchase())
                                    {
                                        ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TITLE_IAPRESTORE"),pGameContext.GetLocalizingText("MSG_TXT_IAPRESTORE"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),null,false,()=>{
#if USE_HIVE
                                            NetworkManager.ShowWaitMark(true);
                                            LayoutManager.Instance.InteractableDisableAll(null,true);
                                            SingleFunc.IAPV4Restore(OnIAPV4RestoreCB);
#endif
                                        });
                                    }
                                });
                            }
                            break;
                        }
                    }
                }
            }
            break;
        }
    }

    public void ShowBackground(bool bShow)
    {
        Ground pGround = GetInstance<Ground>();
        if(bShow)
        {
            pGround.PlayGroundObject();
        }
        else
        {
            pGround.StopPlayGroundObject();
        }
        ShowMatchButton(bShow);
        pGround.MainUI.gameObject.SetActive(bShow);
        ShowMainUI(bShow);
    }

    public TacticsFormation ShowTacticsFormation(TeamDataT pTeamData = null)
    {
        TacticsFormation pTacticsFormation = GetInstance<TacticsFormation>();
        pTacticsFormation.SetupView(IsShowInstance<MatchView>());
        pTacticsFormation.MainUI.SetAsLastSibling();
        pTacticsFormation.SetupData(pTeamData);
        pTacticsFormation.ShowTabUI(0); 
        ShowMoveDilog(pTacticsFormation.MainUI,Vector3.left);
        m_pUserName.transform.parent.gameObject.SetActive(false);
        m_pMainButtons.parent.SetAsLastSibling();
        return pTacticsFormation;
    }

    public Transfer ShowTransfer(byte eTab)
    {
        Transfer pTransfer = GetInstance<Transfer>();
        pTransfer.MainUI.SetAsLastSibling();
        GameContext.getCtx().AddExpireTimer(pTransfer,0,GameContext.getCtx().GetExpireCachePlayerData(true));
        pTransfer.SetupTimer();
        pTransfer.ShowTabUI(eTab);
        ShowMoveDilog(pTransfer.MainUI,Vector3.left);
        m_pMainButtons.parent.SetAsLastSibling();
        m_pUserName.transform.parent.SetAsLastSibling();
        m_pUserName.transform.parent.gameObject.SetActive(true);
        return pTransfer;
    }

    public Squad ShowSquad()
    {
        Squad pSquad = GetInstance<Squad>();
        pSquad.MainUI.SetAsLastSibling();
        pSquad.ShowTabUI(0);
        ShowMoveDilog(pSquad.MainUI,Vector3.left);
        m_pMainButtons.parent.SetAsLastSibling();
        m_pUserName.transform.parent.SetAsLastSibling();
        m_pUserName.transform.parent.gameObject.SetActive(true);
        return pSquad;
    }
    public Management ShowManagement()
    {
        Management pManagement = GetInstance<Management>();
        pManagement.MainUI.SetAsLastSibling();
        m_pMainButtons.parent.SetAsLastSibling();
        m_pUserName.transform.parent.SetAsLastSibling();
        m_pUserName.transform.parent.gameObject.SetActive(true);
        pManagement.ShowTabUI(0);
        pManagement.Enable = false;
        ShowMoveDilog(pManagement.MainUI,Vector3.left);
        return pManagement;
    }

    public Store ShowStore(byte type,string name = null)
    {
        if(!GameContext.getCtx().IsProductInfoList())
        {
            SingleFunc.MarketConnect();
        }

        Store pStore = GetInstance<Store>();
        pStore.MainUI.SetAsLastSibling();
        m_pMainButtons.parent.SetAsLastSibling();
        m_pUserName.transform.parent.SetAsLastSibling();
        m_pUserName.transform.parent.gameObject.SetActive(true);
        pStore.SetupScroll(true);
        pStore.SetScroll(type,name);
        pStore.Enable = false;
        ShowMoveDilog(pStore.MainUI,Vector3.left);
        return pStore;
    }

    public Chat ShowChat()
    {
        Chat pChat = GetInstance<Chat>();
        pChat.MainUI.SetAsLastSibling();
        pChat.JoinRoom();
        // m_pMainButtons.parent.SetAsLastSibling();
        // userName.transform.parent.SetAsLastSibling();
        m_pUserName.transform.parent.gameObject.SetActive(true);
        pChat.Enable = false;
        ShowMoveDilog(pChat.MainUI,Vector3.up);
        return pChat;
    }
    public void SetupTimer()
    {
        GetInstance<Ground>().SetupTimer();
    }

    public void UpdateTrainingTimer(float time)
    {
        Management pManagement = GetInstance<Management>();
        if(pManagement.MainUI.gameObject.activeSelf)
        {
            pManagement.UpdateTrainingTimer(time);
        }
    }
    public void RequestPlayerProfile(ulong id, System.Action endCallback = null )
    {
        JObject pJObject = new JObject();
        pJObject["player"] = id;
        RequestAfterCall(E_REQUEST_ID.player_profile,pJObject,endCallback);
    }
    public void RequestClubProfile(ulong id, System.Action endCallback = null )
    {
        JObject pJObject = new JObject();
        pJObject["no"] = id;
        RequestAfterCall(E_REQUEST_ID.club_profile,pJObject,endCallback);
    }

    public void ResetMainButton()
    {
        Transform tm = null;
        for(int i =0; i < m_pMainButtons.childCount; ++i)
        {
            tm = m_pMainButtons.GetChild(i);
            tm.Find("on").gameObject.SetActive(false);
            tm.Find("icon").gameObject.SetActive(true);
            tm.Find("title").GetComponent<TMPro.TMP_Text>().color = GameContext.GRAY;
        }
    }

    public void SetMainButton(int index)
    {
        Transform tm = null;
        for(int i =0; i < m_pMainButtons.childCount; ++i)
        {
            tm = m_pMainButtons.GetChild(i);
            tm.Find("on").gameObject.SetActive(index == i);
            tm.Find("icon").gameObject.SetActive(!(index == i));
            tm.Find("title").GetComponent<TMPro.TMP_Text>().color = index == i ? Color.white : GameContext.GRAY;
        }
    }

    bool executeStartPopupCallback(IState state,float dt,bool bEnd)
    {
        BaseStateTarget target = state.GetTarget<BaseStateTarget>();
        RectTransform tm = target.GetMainTarget<RectTransform>();
        
        if(!tm.gameObject.activeSelf)
        {
            ShowStartPopup();
            bEnd = true;
        }

        return bEnd;
    }

    void enterDilogCallback(IState state)
    {
        if( state.StateData is DilogMoveStateData data)
        { 
            LayoutManager.Instance.InteractableDisableAll();
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            RectTransform tm = target.GetMainTarget<RectTransform>();
            
            if(data.Out)
            {
                ALFUtils.FadeObject(tm,1);
                tm.offsetMin = new Vector2(0,tm.offsetMin.y);
                tm.offsetMax = new Vector2(0,tm.offsetMax.y);
            }
            else
            {
                ALFUtils.FadeObject(tm,-1);
                tm.gameObject.SetActive(true);
                tm.offsetMin = new Vector2(LayoutManager.Width,tm.offsetMin.y);
                tm.offsetMax = new Vector2(LayoutManager.Width,tm.offsetMax.y);
            }
        }
    }
    bool executeDilogCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            if(data.FadeDelta > 0)
            {
                BaseStateTarget target = state.GetTarget<BaseStateTarget>();
                RectTransform tm = target.GetMainTarget<RectTransform>();
                TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                
                float p = 0;
                if(data.Out)
                {
                    ALFUtils.FadeObject(tm,-(data.FadeDelta * dt));
                    p = LayoutManager.Width * ALFUtils.EaseIn(condition.GetTimePercent(),2);
                }
                else
                {
                    ALFUtils.FadeObject(tm,data.FadeDelta * dt);
                    p = LayoutManager.Width * (1.0f - ALFUtils.EaseIn(condition.GetTimePercent(),2));
                }

                tm.offsetMin = new Vector2(p,tm.offsetMin.y);
                tm.offsetMax = new Vector2(p,tm.offsetMax.y);

                if(bEnd && !data.Out)
                {
                    data.FadeDelta =0;
                    return false;
                }
            }
            else
            {
                data.FadeDelta -= dt;
                if(data.FadeDelta > -2)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        return bEnd;
    }

    IState exitHideCallback(IState state)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            HideDilog(target.GetMainTarget<RectTransform>());
        }

        return null;
    }
    IState exitDilogCallback(IState state)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            LayoutManager.Instance.InteractableEnabledAll();

            if(data.Out)
            {
                BaseStateTarget target = state.GetTarget<BaseStateTarget>();
                RectTransform tm = target.GetMainTarget<RectTransform>();
                tm.gameObject.SetActive(false);
            }
        }

        return null;
    }

    void enterDilogMoveCallback(IState state)
    {
        if( state.StateData is DilogMoveStateData data)
        { 
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            Transform tm = target.GetMainTarget<Transform>();
            RectTransform ui = tm.Find("root").GetComponent<RectTransform>();
            LayoutManager.Instance.InteractableDisableAll();

            if(tm.gameObject.name == "Squad")
            {
                GetInstance<Squad>().Enable = false;
            }
            else if(tm.gameObject.name == "TacticsFormation")
            {
                GetInstance<TacticsFormation>().Enable = false;
            }
            
            if(data.Out)
            {
                ui.offsetMin = new Vector2(0,ui.offsetMin.y);
                ui.offsetMax = new Vector2(0,ui.offsetMax.y);
            }
            else
            {
                ALFUtils.FadeObject(tm.Find("back"),-1);
                tm.gameObject.SetActive(true);
                
                Vector2 offsetMin = ui.offsetMin;
                Vector2 offsetMax = ui.offsetMax;

                if(data.Direction.x > 0)
                {
                    offsetMin.x = LayoutManager.Width;
                }
                else if(data.Direction.x < 0)
                {
                    offsetMin.x = LayoutManager.Width * -1;
                }

                if(data.Direction.y > 0)
                {
                    offsetMin.y = LayoutManager.Height;
                }
                else if(data.Direction.y < 0)
                {
                    offsetMin.y = LayoutManager.Height * -1;
                }
                offsetMax.y = offsetMin.y;
                
                ui.offsetMin = offsetMin;
                ui.offsetMax = offsetMax;
            }
        }
    }
    bool executeDilogMoveCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            Transform tm = target.GetMainTarget<Transform>();
            TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
            RectTransform ui = tm.Find("root").GetComponent<RectTransform>();
            
            Vector2 offsetMin = ui.offsetMin;
            Vector2 offsetMax = ui.offsetMax;
            float p = ALFUtils.EaseIn(condition.GetTimePercent(),2);
            if(!data.Out)
            {
                p = 1.0f - p;
            }

            if(data.Direction.x > 0)
            {
                offsetMin.x = LayoutManager.Width * p;
            }
            else if(data.Direction.x < 0)
            {
                offsetMin.x = LayoutManager.Width * -p;
            }

            if(data.Direction.y > 0)
            {
                offsetMin.y = LayoutManager.Height * p;
            }
            else if(data.Direction.y < 0)
            {
                offsetMin.y = LayoutManager.Height * -p;
            }

            offsetMax.x = offsetMin.x;
            offsetMax.y = offsetMin.y;
            ALFUtils.FadeObject(tm.Find("back"),data.Out ? -(data.FadeDelta * dt):data.FadeDelta * dt);
            ui.offsetMin = offsetMin;
            ui.offsetMax = offsetMax;
        }
        return bEnd;
    }
    IState exitDilogMoveCallback(IState state)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            LayoutManager.Instance.InteractableEnabledAll();
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            RectTransform tm = target.GetMainTarget<RectTransform>();
            
            bool bEnable = false;
            
            if(data.Out)
            {
                switch(tm.gameObject.name)
                {
                    case "Management":
                    {
                        bEnable = true;
                    }
                    break;
                    case "Squad":
                    {
                        bEnable = true;
                    }
                    break;
                    case "TacticsFormation":
                    {
                        GetInstance<TacticsFormation>().CurrentClearScroll();
                        bEnable = true;
                    }
                    break;
                    case "Transfer":
                    {
                        GetInstance<Transfer>().CurrentClearScroll();
                        bEnable = true;
                    }
                    break;
                    case "MatchView":
                    {
                        GetInstance<MatchView>().ResetRewardUI();
                        if(!StateMachine.GetStateMachine().IsCurrentTargetStates((uint)E_STATE_TYPE.ShowStartPopup)) 
                        {
                            ShowStartPopup();
                        }
                        
                        bEnable = true;
                    }
                    break;
                    case "Store":
                    {
                        bEnable = true;
                        GetInstance<Store>().ClearScroll();
                    }
                    break;
                    case "Match":
                    {
                        GetInstance<Match>().ClearData();
                    }
                    break;
                }
                
                tm.gameObject.SetActive(false);
            }
            else
            {
                switch(tm.gameObject.name)
                {
                    case "Management":
                    {
                        GetInstance<Management>().Enable = true;
                        bEnable = true;
                    }
                    break;
                    case "Squad":
                    {
                        GetInstance<Squad>().Enable = true;
                        bEnable = true;
                    }
                    break;
                    case "TacticsFormation":
                    {
                        GetInstance<TacticsFormation>().Enable = true;
                        bEnable = true;
                    }
                    break;
                    case "Transfer":
                    {
                        GetInstance<Transfer>().Enable = true;
                        bEnable = true;
                    }
                    break;
                    case "MatchView":
                    {
                        GetInstance<MatchView>().PlayLineupAnimation(true);
                        bEnable = true;
                    }
                    break;
                    case "Store":
                    {
                        bEnable = true;
                        GetInstance<Store>().Enable = true;
                    }
                    break;
                    case "Chat":
                    {
                        // bEnable = true;
                        GetInstance<Chat>().Enable = true;
                    }
                    break;
                    case "Match":
                    {
                        GetInstance<Match>().Play();
                    }
                    break;
                    case "League":
                    {
                        int iMatchType = GameContext.getCtx().GetCurrentMatchType();
                        if(iMatchType == GameContext.LADDER_ID || iMatchType == GameContext.CHALLENGE_ID)
                        {
                            RequestAfterCall(E_REQUEST_ID.challengeStage_getStandings,null);
                        }
                        else
                        {
                            JObject jObject = new JObject();
                            jObject["matchType"] = iMatchType;
                            jObject["seasonNo"] = GameContext.getCtx().GetCurrentSeasonNo();
                            RequestAfterCall(E_REQUEST_ID.league_getStandings,jObject);
                        }
                    }
                    break;
                }
            }

            if(bEnable)
            {
                for(int i =0; i < m_pMainButtons.childCount; ++i)
                {
                    m_pMainButtons.GetChild(i).GetComponent<Button>().enabled = true;
                }
            }
        }

        return null;
    }
    
    bool executeShakeCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is ShakeStateData data)
        {
            data.shakeTime -= dt;
            Camera.main.transform.position = UnityEngine.Random.insideUnitSphere * data.shakeTime + data.cameraPos;
        }

        return bEnd;
    }    

    void TimeUpdate(float dt)
    {
        GameContext.getCtx().UpdateTimer(dt,this);
        GetInstance<Chat>().UpdateTimer(dt);
        
        if(IsShowInstance<PlayerInfo>()) GetInstance<PlayerInfo>().UpdateTimer(dt);
        if(IsShowInstance<Transfer>()) GetInstance<Transfer>().UpdateTimer(dt);
        if(IsShowInstance<Store>()) GetInstance<Store>().UpdateTimer(dt);
        if(IsShowInstance<SeasonPass>()) GetInstance<SeasonPass>().UpdateTimer(dt);
        if(IsShowInstance<PremiumSeasonPass>()) GetInstance<PremiumSeasonPass>().UpdateTimer(dt);
        if(IsShowInstance<Ranking>()) GetInstance<Ranking>().UpdateTimer(dt);
        if(IsShowInstance<FastReward>()) GetInstance<FastReward>().UpdateTimer(dt);
        if(IsShowInstance<QuestMission>()) GetInstance<QuestMission>().UpdateTimer(dt);
        if(IsShowInstance<League>()) GetInstance<League>().UpdateTimer(dt);
        if(IsShowInstance<Challenge>()) GetInstance<Challenge>().UpdateTimer(dt);
        if(IsShowInstance<TrophyReward>()) GetInstance<TrophyReward>().UpdateTimer(dt);
        if(IsShowInstance<RankingReward>()) GetInstance<RankingReward>().UpdateTimer(dt);
        if(IsShowInstance<LeagueRankingReward>()) GetInstance<LeagueRankingReward>().UpdateTimer(dt);
        if(IsShowInstance<MonthlyPackage>()) GetInstance<MonthlyPackage>().UpdateTimer(dt);
        if(IsShowInstance<TimeSale>()) GetInstance<TimeSale>().UpdateTimer(dt);
        if(IsShowInstance<ShopPackage>()) GetInstance<ShopPackage>().UpdateTimer(dt);
    }

    public void UpdateMatchGauge()
    {
        GameContext pGameContext = GameContext.getCtx();
        m_pMatchGauge.fillAmount = pGameContext.GetLineupTotalHP() / 1800.0f;
        
        if(m_pMatchGauge.fillAmount < 0.5f)
        {
            m_pMatchGauge.color = GameContext.HP_L;
        }
        else if(m_pMatchGauge.fillAmount < 0.7f)
        {
            m_pMatchGauge.color = GameContext.HP_LH;
        }
        else if(m_pMatchGauge.fillAmount < 0.9f)
        {
            m_pMatchGauge.color = GameContext.HP_H;
        }
        else
        {
            m_pMatchGauge.color = GameContext.HP_F;
        }
        
        if(m_pMatchGauge.fillAmount < 1f)
        {
            m_pRecoveryButton.gameObject.SetActive(true);
            m_pMatchEffect.gameObject.SetActive(false);
            m_pMatchEffect.Stop();
            m_pMatchButton.transform.parent.Find("icon").gameObject.SetActive(m_pMatchGauge.fillAmount < 0.8f);
            ulong hasCount = pGameContext.GetItemCountByNO(GameContext.STAMINA_DRINK);
            uint count = pGameContext.GetUseRecoverHPItemCount(pGameContext.GetRecoverHpPlayerIDs());
            m_pRecoveryButton.enabled = count <= hasCount;
            m_pRecoveryButton.transform.Find("off").gameObject.SetActive(!m_pRecoveryButton.enabled);
            m_pRecoveryButton.transform.Find("on").gameObject.SetActive(m_pRecoveryButton.enabled);
            m_pRecoveryButton.transform.Find("count").GetComponent<TMPro.TMP_Text>().SetText($"<color={(m_pRecoveryButton.enabled ? Color.white.ToHexString() : Color.red.ToHexString())}>{ALFUtils.NumberToString(count)}</color>/{ALFUtils.NumberToString(hasCount)}");
        }
        else
        {
            m_pRecoveryButton.gameObject.SetActive(false);
            m_pMatchEffect.gameObject.SetActive(true);
            m_pMatchEffect.Play();
            m_pMatchButton.transform.parent.Find("icon").gameObject.SetActive(false);
        }

        if(IsShowInstance<PlayerInfo>())
        {
            GetInstance<PlayerInfo>().UpdatePlayerHP();
        }

        if(IsShowInstance<QuickLineUp>())
        {
            GetInstance<QuickLineUp>().UpdatePlayerHP();
        }

        if(IsShowInstance<TacticsFormation>())
        {
            GetInstance<TacticsFormation>().UpdatePlayerHP();
        }

        if(IsShowInstance<Squad>())
        {
            GetInstance<Squad>().UpdatePlayerHP(false);
        }

        if(IsShowInstance<League>())
        {
            GetInstance<League>().UpdateMatchGauge();
        }
    }

    bool executeUpdateCallback(IState state,float dt,bool bEnd)
    {
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.IsLoadGameData())
        {
            TimeUpdate(dt);
        }
        
        if(pGameContext.RefreshSelfAreaRect())
        {
            RefreshRect();
        }
#if UNITY_EDITOR || UNITY_ANDROID
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            if(NetworkManager.IsUseNetwork()) return false;
            
            if(pGameContext.GetTutorial() < 44)
            {
                if(IsShowInstance<Tutorial>())
                {
                    GetInstance<Tutorial>().ShowSkipPopup();
                }
            }
            else
            {
                RectTransform dlg = LayoutManager.Instance.GetLastPopup();
                
                if(dlg == null)
                {
#if USE_HIVE
                    pGameContext.HiveExit();
#else
                    ShowConfirmPopup( pGameContext.GetLocalizingText("DIALOG_GAMEEXIT_TITLE"),pGameContext.GetLocalizingText("DIALOG_GAMEEXIT_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,() =>{
                        Application.Quit();
                    });
#endif
                }
                else
                {
                    if(dlg.GetComponent<Animation>() != null)
                    {
                        IBaseUI pIBaseUI = GetInstanceByUI(dlg);
                        
                        if(IsShowInstance<Match>())
                        {
                            GetInstance<Match>().CloseShowGiveup();
                        }
                        pIBaseUI?.Close();
                    }
                    else
                    {
                        Transform tm = m_pMainButtons.Find(dlg.gameObject.name);
                        if(tm != null)
                        {
                            SetBottomTab(tm);
                        }
                        else
                        {
                            if(dlg.gameObject.name == "Chat")
                            {
                                GetInstance<Chat>().Close();
                            }
                            else if(dlg.gameObject.name == "Match")
                            {
                                GetInstance<Match>().ShowGiveupPopup();
                            }
                            else if(dlg.gameObject.name == "MatchView" || dlg.gameObject.name == "away")
                            {
                                GetInstance<MatchView>().BackButton();
                            }
                        }
                    }
                }
            }
        }
#endif
        return false;
    }

    public void SetAwayEmblem(EmblemBake pEmblemBake)
    {
        m_pAwayEmblem = pEmblemBake;
    }
    
    void UserGameDataLoad()
    {
        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.home_get, GameContext.getCtx().GetNetworkAPI(E_REQUEST_ID.home_get),true,true);
    }

    public void UpdateSquadCount()
    {
        Squad pSquad = GetInstance<Squad>();
        if(pSquad.MainUI.gameObject.activeSelf)
        {
            GameContext pGameContext = GameContext.getCtx();
            pSquad.UpdateSquadCount(pGameContext.GetTotalPlayerCount());
        }
    }
    public JObject MakeTacticsData(int selectIndex)
    {
        return GetInstance<TacticsFormation>().MakeTacticsData(selectIndex);
    }

    public void UpdateFormationFromPresetData(int selectIndex, int index)
    {
        GetInstance<TacticsFormation>().UpdateFormationFromPresetData(selectIndex,index);
    }

    public void CheckShowStartPopup()
    {
        if(!IsShowInstance<MatchView>())
        {
            if(!StateMachine.GetStateMachine().IsCurrentTargetStates((uint)E_STATE_TYPE.ShowStartPopup)) 
            {
                ShowStartPopup();
            }
        }
    }

    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
        if(data == null) return;
        E_REQUEST_ID eID = (E_REQUEST_ID)data.Id;
        Debug.Log( $"E_REQUEST_ID:  {eID}\nJson:{(data.Json == null ? 0 : data.Json.ToString())}\n");
        
        string msg = null;
        
        if(!string.IsNullOrEmpty(data.ErrorCode))
        {
#if FTM_LIVE
            msg = data.ErrorCode;
#else
            msg = string.Format("eID:{0}\n\n{1}",eID,data.ErrorCode);
#endif
        }
        
        if(data.Json.ContainsKey("errorCode") && (int)data.Json["errorCode"] != 0)
        {
            // errorNo =(int)networkData["errorCode"];
            // msgId =(int)networkData["msgId"];
            // msg = string.Format("{2}\n code:{0}\n\n{1}}",msg,data.Json.ContainsKey("errorCode") ? (string)data.Json["errorCode"]: " errorCode null" , data.Json.ContainsKey("errorMsg") ? (string)data.Json["errorMsg"]: "errorMsg null"); 
            msg = data.Json.ToString();
        }
        
        GameContext pGameContext = GameContext.getCtx();

        if(eID == E_REQUEST_ID.iap_reserve)
        {
            if(IsShowInstance<PremiumSeasonPass>())
            {
                GetInstance<PremiumSeasonPass>().Purchase(string.IsNullOrEmpty(msg));
            }
            else if(IsShowInstance<TimeSale>())
            {
                GetInstance<TimeSale>().Purchase(string.IsNullOrEmpty(msg));
            }
            else
            {
                GetInstance<Store>().NetworkProcessor(data,bSuccess);
            }

            if(!string.IsNullOrEmpty(msg))
            {
                ShowMessagePopup(GameContext.getCtx().GetLocalizingErrorMsg(msg),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
            }
        }
        else
        {
            if(string.IsNullOrEmpty(msg))
            {
                if(E_REQUEST_ID.home_get == eID)
                {
                    pGameContext.SetGameData(data.Json);

                    pGameContext.SendReqClubLicensePut(false);
                    JObject pJObject = new JObject();
                    pJObject["type"] = 0;
                    RequestAfterCall(E_REQUEST_ID.tactics_get,pJObject);
                    m_pEmblem.gameObject.SetActive(true);
                    
                    UpdateTopUI();
                    pGameContext.UpdateNoticeIcon(eID,this);
                    if(!ShowTutorial())
                    {
                        SetupTimer();
                    }
                }
                else if(eID == E_REQUEST_ID.settings_get)
                {
                    if(data.Json.ContainsKey("data") && data.Json["data"].Type != JTokenType.Null)
                    {
                        pGameContext.SetSettingData((JObject)data.Json["data"]);
                    }
                }
                else if(E_REQUEST_ID.club_create == eID)
                {
                    UserGameDataLoad();
                }
                else if(E_REQUEST_ID.account_login == eID)
                {
                    // GameContext.getCtx().SetUserData(data.Json);
                    // // {"errorCode":0,"msgId":20004,"loginType":1,"customerNo":"34449701914","accessToken":"npl2itxky2ea4q5","tExpire":1641460401}
                    // // loginType :0 -> 최초 유저 등록 , 1 -> 일반로그인 , 2 -> 로그인 갱신 
                    // //tServer -> 서버 시간 
                    // UserGameDataLoad();
                }
                else
                {
                    msg = pGameContext.UpdateGameData(eID,data.Json,this);

                    UpdateGameMaterials();

                    if(eID == E_REQUEST_ID.home_reset)
                    {
                        ResetMainButton();
                        AllInstanceClose(false);
                    }
                    
                    if(IsShowInstance<PlayerInfo>())
                    {
                        GetInstance<PlayerInfo>().NetworkProcessor(data,true);
                    }
                    GetInstance<MatchView>().NetworkProcessor(data,true);
                    GetInstance<Ground>().NetworkProcessor(data,true);
                    GetInstance<Management>().NetworkProcessor(data,true);
                    GetInstance<Squad>().NetworkProcessor(data,true);
                    GetInstance<TacticsFormation>().NetworkProcessor(data,true);
                    GetInstance<Transfer>().NetworkProcessor(data,true);
                    GetInstance<Store>().NetworkProcessor(data,true);

                    if(string.IsNullOrEmpty(msg))
                    {
                        switch(eID)
                        {
                            case E_REQUEST_ID.auction_withdraw:
                            case E_REQUEST_ID.auction_reward:
                            {
                                bool bMailBox = IsShowInstance<MailBox>();
                                Action pCallback = null;
                                if(!bMailBox)
                                {
                                    pCallback = ()=>{NetworkManager.EnableWaitMark(false);};
                                }

                                RequestAfterCall(E_REQUEST_ID.mail_get,null,pCallback, bMailBox);
                            }
                            break;
                            case E_REQUEST_ID.tutorial_recruit:
                            case E_REQUEST_ID.recruit_offer:
                            case E_REQUEST_ID.player_abilityUp:
                            case E_REQUEST_ID.scout_reward:
                            case E_REQUEST_ID.youth_offer:
                            {
                                pGameContext.SendReqClubLicensePut(false);
                            }
                            break;
                            case E_REQUEST_ID.mail_delete:
                            case E_REQUEST_ID.mail_get:
                            case E_REQUEST_ID.mail_read:
                            {
                                UpdateUnreadMailCount();
                                if(IsShowInstance<MailBox>())
                                {
                                    GetInstance<MailBox>().SetupData();
                                }
                            }
                            break;
                            case E_REQUEST_ID.attend_reward:
                            {
                                if(pGameContext.IsGetPrevSeasonReward())
                                {
                                    RequestAfterCall(E_REQUEST_ID.ladder_rewardStanding,null,null);
                                }
                                else
                                {
                                    if(!IsShowInstance<Match>())
                                    {
                                        CheckShowStartPopup();
                                    }
                                }
                            }
                            break;
                            case E_REQUEST_ID.ladder_rewardStanding:
                            {
                                RequestAfterCall(E_REQUEST_ID.ladder_rewardUserRank,null,null);
                            }
                            break;
                            case E_REQUEST_ID.ladder_rewardUserRank:
                            {
                                if(!IsShowInstance<Match>())
                                {
                                    CheckShowStartPopup();
                                }
                            }
                            break;
                            case E_REQUEST_ID.league_getHistory:
                            {
                                GetInstance<Profile>().SetupLeagueHistoryData(data.Json);
                            }
                            break;
                            case E_REQUEST_ID.league_getLeaders:
                            {
                                GetInstance<League>().SetupLeagueWinList(data.Json);
                            }
                            break;
                            case E_REQUEST_ID.league_getTodayFixture:
                            {
                                UpdateLeagueTodayInfo();
                                if(IsShowInstance<LeagueMatch>())
                                {
                                    GetInstance<LeagueMatch>().SetupData(data.Json);
                                }
                            }
                            break;            
                            case E_REQUEST_ID.challengeStage_searchOpps:
                            {
                                GetInstance<Challenge>().SetupData(data.Json);
                            }
                            break;
                            case E_REQUEST_ID.league_getStandings:
                            {
                                GetInstance<League>().SetupLeagueStandings(data.Json);
                            }
                            break;
                            case E_REQUEST_ID.challengeStage_getStandings:
                            {
                                GetInstance<League>().SetupChallengeStandings(data.Json);
                            }
                            break;
                            case E_REQUEST_ID.auction_trade:
                            case E_REQUEST_ID.auction_refund:
                            {
                                if(!IsShowInstance<PlayerInfo>() || !GetInstance<PlayerInfo>().IsCurrentBidding())
                                {
                                    CheckShowStartPopup();   
                                }
                            }
                            break;
                            case E_REQUEST_ID.tactics_get:
                            {
                                if(pGameContext.GetTutorial() < 30)
                                {
                                    GetInstance<Squad>().SetupPlayerData();
                                    GetInstance<TacticsFormation>().SetupPlayerData();
                                    
                                    pGameContext.DefaultSuggestionPlayerData();
                                    List<ulong> lineup = pGameContext.GetLineupPlayerIdListByIndex(pGameContext.GetActiveLineUpType());
                                    JArray jArray = new JArray();
                                    for(int i = 0; i < lineup.Count; ++i)
                                    {
                                        jArray.Add(lineup[i]);
                                    }
                                    
                                    JObject pJObject = new JObject();
                                    pJObject["type"] = pGameContext.GetActiveLineUpType();
                                    pJObject["data"] =jArray.ToString(Newtonsoft.Json.Formatting.None);

                                    pJObject["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
                                    pJObject["totalValue"] = pGameContext.GetTotalPlayerValue(0);
                                    pJObject["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(null,true);
                                    pJObject["avgAge"] = pGameContext.GetPlayerAvgAge(null,true);
                                    pJObject["playerCount"] = pGameContext.GetTotalPlayerCount();
                                    RequestAfterCall(E_REQUEST_ID.lineup_put,pJObject);
                                }
                            }
                            break;
                            case E_REQUEST_ID.adReward_reward:
                            {
                                if(data.Json.ContainsKey("adRewards"))
                                {
                                    JArray pArray = (JArray)data.Json["adRewards"];
                                    for(int i =0; i < pArray.Count; ++i)
                                    {
                                        SetAdNotice((uint)pArray[i]["no"],false);
                                    }
                                }

                                if(data.Json.ContainsKey("rewards"))
                                {
                                    ShowRewardPopup(data.Json);
                                }
                                else if(data.Json.ContainsKey("players"))
                                {
                                    ShowScoutRewardPopup(data.Json);
                                    pGameContext.SendReqClubLicensePut(false);
                                }
                            }
                            break;
                            case E_REQUEST_ID.tutorial_put:
                            {
                                GetInstance<Tutorial>().NextStep();
                            }
                            break;
                            case E_REQUEST_ID.player_recoverHP:
                            case E_REQUEST_ID.ladder_clear:
                            case E_REQUEST_ID.challengeStage_clear:
                            case E_REQUEST_ID.league_clear:
                            {
                                
                                if(eID == E_REQUEST_ID.ladder_clear)
                                {
                                    UpdateRank();
                                }

                                if(eID != E_REQUEST_ID.player_recoverHP)
                                {
                                    pGameContext.DeleteRecordMatchData();    
                                }
                                else
                                {
                                    SoundManager.Instance.PlaySFX("sfx_hp_recovery");
                                }
                                
                                UpdateMatchGauge();
                            }
                            break;
                            case E_REQUEST_ID.player_profile:
                            {
                                GetInstance<PlayerInfo>().NetworkProcessor(data,true);
                            }
                            break;
                            case E_REQUEST_ID.club_changeEmblem:
                            {
                                EditEmblem pUI = GetInstance<EditEmblem>();
                                pGameContext.SetEmblem(pUI.GetEmblemInfoData());
                                UpdateTopEmblem();
                                if(IsShowInstance<EditProfile>())
                                {
                                    GetInstance<EditProfile>().CheckEnableConfirm();
                                }
                            }
                            break;
                            case E_REQUEST_ID.matchStats_getSummary:
                            {
                                GetInstance<MatchDetailLog>().SetupSummaryData(data.Json,m_pAwayEmblem);
                                m_pAwayEmblem = null;
                            }
                            break;
                            case E_REQUEST_ID.matchStats_list:
                            case E_REQUEST_ID.challengeStage_getMatches:
                            {    
                                GetInstance<MatchLog>().SetupData(data.Json,eID);
                            }
                            break;
                            case E_REQUEST_ID.iap_purchase:
                            {
                                UpdateMatchGauge();
                                if(IsShowInstance<Reward>())
                                {
                                    Reward pUI = GetInstance<Reward>();
                                    pUI.SetupRewardItems(data.Json);
                                    SoundManager.Instance.PlaySFX("sfx_reward");
                                    pUI.ShowTab();
                                    pUI.ShowRewardItems();
                                }
                                else
                                {
                                    ShowRewardPopup(data.Json);
                                }
                                
                                if(pGameContext.IsRestorePurchase())
                                {
                                    if(pGameContext.RestorePurchase())
                                    {
                                        NetworkManager.ShowWaitMark(false);
                                        LayoutManager.Instance.InteractableEnabledAll(null,true);
                                    }
                                }
                                else
                                {
                                    LayoutManager.Instance.InteractableEnabledAll(null,true);
                                    NetworkManager.ShowWaitMark(false);

                                    if(IsShowInstance<TimeSale>())
                                    {
                                        GetInstance<TimeSale>().SetupData(0);
                                    }
                                    if(IsShowInstance<ShopPackage>())
                                    {
                                        GetInstance<ShopPackage>().UpdateProductMileageData(true);
                                    }

                                    if(IsShowInstance<MonthlyPackage>())
                                    {
                                        GetInstance<MonthlyPackage>().Close();
                                    }

                                    if(data.Json.ContainsKey("pass") && data.Json["pass"].Type != JTokenType.Null)
                                    {
                                        if(IsShowInstance<PremiumSeasonPass>())
                                        {
                                            GetInstance<PremiumSeasonPass>().PurchaseComplete();
                                        }

                                        if(IsShowInstance<SeasonPass>())
                                        {
                                            GetInstance<SeasonPass>().SetupData();
                                        }
                                    }
                                }
                            }
                            break;
                            case E_REQUEST_ID.mileage_reward:
                            {
                                if(IsShowInstance<ClubLicense>())
                                {
                                    if(data.Json.ContainsKey("mileages"))
                                    {
                                        JArray pArray = (JArray)data.Json["mileages"];
                                        for(int i = 0; i < pArray.Count; ++i)
                                        {
                                            pGameContext.IsTotalLicenseComplete((JObject)pArray[i]);
                                        }
                                        UpdateLeagueTodayInfo();
                                    }
                                }

                                if(IsShowInstance<QuestMission>())
                                {
                                    if(data.Json.ContainsKey("mileages"))
                                    {
                                        JArray pArray = (JArray)data.Json["mileages"];
                                        for(int i = 0; i < pArray.Count; ++i)
                                        {
                                            pGameContext.IsTotalQuestMissionComplete((JObject)pArray[i]);
                                        } 
                                    }
                                }

                                CheckShowStartPopup();
                            }
                            break;
                            case E_REQUEST_ID.achievement_reward:
                            case E_REQUEST_ID.fastReward_reward:
                            case E_REQUEST_ID.pass_reward:
                            case E_REQUEST_ID.clubLicense_reward:
                            case E_REQUEST_ID.quest_reward:
                            case E_REQUEST_ID.mail_reward:
                            case E_REQUEST_ID.shop_buy:
                            {
                                ShowRewardPopup(data.Json);

                                if(eID == E_REQUEST_ID.achievement_reward)
                                {
                                    if(data.Json.ContainsKey("achievements"))
                                    {
                                        string[] list = new string[]{null,"kkzufm","idx8j0","mpyvld","zclw34","gwjabq","ybwh2l","k87876","i06pbs","novdx4","l333n9","exlkzc","xwvg3i","ok5ka3","qtqiwt","er03si","2xoeo0","iw73oo","rpwo92","eqk1nq","ni2gkm","p8p79r","7i8t5e","6eppct","u8jppf","ave32m","g0eo16","zcs59l","k7yuv6","6wu20l","hxp21a","vlqknl","g0w5db","vyw9t6","ltn5es","eoj3ws","pe5xkn","dieclsv","4etjyh","lfa28j"};
                                        JArray pJArray = (JArray)data.Json["achievements"];
                                        JObject pJObject = null;
                                        int index = 0;
                                        for(int i = 0; i < pJArray.Count; ++i)
                                        {
                                            pJObject = (JObject)pJArray[i];
                                            index = (int)pJObject["level"];
                                            if(index > -1 && index < list.Length)
                                            {
                                                pGameContext.SendAdjustEvent(list[index],true,true,-1);
                                            }
                                        }
                                    }
                                }
                                else if(eID == E_REQUEST_ID.mail_reward)
                                {
                                    UpdateUnreadMailCount();
                                }
                                else if(eID == E_REQUEST_ID.shop_buy)
                                {
                                    UpdateMatchGauge();
                                }
                                else if(eID == E_REQUEST_ID.fastReward_reward)
                                {
                                    if(IsShowInstance<FastReward>())
                                    {
                                        GetInstance<FastReward>().SetupRewardData(true);
                                    }
                                }
                                else if(eID == E_REQUEST_ID.pass_reward)
                                {
                                    if(IsShowInstance<SeasonPass>())
                                    {
                                        GetInstance<SeasonPass>().UpdateScroll();
                                    }
                                }
                            }
                            break;
                            case E_REQUEST_ID.club_top100:
                            {
                                GetInstance<Ranking>().NetworkDataParse(eID,data.Json);
                            }
                            break;
                            case E_REQUEST_ID.playerStats_top100:
                            {
                                if(IsShowInstance<Ranking>())
                                {
                                    GetInstance<Ranking>().NetworkDataParse(eID,data.Json);
                                }
                                
                                if(IsShowInstance<League>())
                                {
                                    GetInstance<League>().SetupLeagueTop100(data.Json);
                                }    
                            }
                            break;
                        }
                    }
                    else
                    {
                        LayoutManager.Instance.InteractableEnabledAll(null,true);
                        ShowMessagePopup(msg,pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                        if(m_pRequestAfterCallList.ContainsKey(eID))
                        {
                            m_pRequestAfterCallList.Remove(eID);
                        }
                    }
                }
                            
                if(m_pRequestAfterCallList.ContainsKey(eID))
                {
                    m_pRequestAfterCallList[eID]();
                }
            }
            else
            {
                if(IsMatch())
                {
                    GetInstance<MatchView>().SaveRecordMatchData();
                }

                if(data.Json.ContainsKey("errorCode"))
                {
                    int errorCode = (int)data.Json["errorCode"];
                    
                    if(eID == E_REQUEST_ID.tutorial_put && errorCode == 5303)
                    {
                        pGameContext.SetTutorial(32);
                        ShowMessagePopup( pGameContext.GetLocalizingErrorMsg((string)data.Json["errorCode"]),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),() =>{
                            GetInstance<Tutorial>().NextStep();
                        });
                    }
                    else if(eID == E_REQUEST_ID.auction_cancel && errorCode == 1632)
                    {
                        if(IsShowInstance<PlayerInfo>())
                        {
                            ShowMessagePopup( pGameContext.GetLocalizingText("MSG_TXT_PLAYER_SELL_MARKET_ALREADY_SOLD"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),() =>{
                                PlayerInfo pPlayerInfo = GetInstance<PlayerInfo>();
                                uint id = pGameContext.GetAuctionSellInfoIDByPlayerID(pPlayerInfo.GetPlayerID());
                                pPlayerInfo.Close();
                                JObject pJObject = new JObject();
                                JArray pJArray = new JArray();
                                pJArray.Add(id);
                                pJObject["auctionIds"] = pJArray;
                                
                                RequestAfterCall(E_REQUEST_ID.auction_reward,pJObject);
                            });
                        }
                    }
                    else
                    {
                        if(eID == E_REQUEST_ID.challengeStage_try)
                        {
                            if(IsShowInstance<Challenge>())
                            {
                                ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(errorCode.ToString()),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),GetInstance<Challenge>().FailChallenge);
                            }
                        }
                        else if(eID == E_REQUEST_ID.iap_purchase )
                        {
                            LayoutManager.Instance.InteractableEnabledAll(null,true);
                            NetworkManager.ShowWaitMark(false);

                            if(errorCode == 1910 || errorCode == 1911 )
                            {
                                ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(errorCode.ToString()),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"));
                                pGameContext.CurrentTransactionFinish();
                            }

                            pGameContext.ClearCurrentPurchaseInfo();
                        }
                        else if(eID == E_REQUEST_ID.club_create)
                        {
                            if(m_pRequestAfterCallList.ContainsKey(eID))
                            {
                                m_pRequestAfterCallList[eID]();
                            }
                            ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(errorCode.ToString()),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),() =>{
                                ShowCreateClub();
                            } );
                        }
                        else
                        {
                            if(errorCode != 1836)
                            {
                                ShowMessagePopup( pGameContext.GetLocalizingErrorMsg(errorCode.ToString()),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),() =>{
                                    /**
                                    * //10	점검 중 // 20	CS 처리 중 // 25	지정된 시간까지 블럭 // 29	영구 정지// 39	유저 탈퇴
                                    */
                                    if(eID == E_REQUEST_ID.ladder_clear || eID == E_REQUEST_ID.challengeStage_clear  || eID == E_REQUEST_ID.league_clear)
                                    {
                                        pGameContext.DeleteRecordMatchData();
                                    }
                                    else if(errorCode == 10 || errorCode == 20 || errorCode == 25 || errorCode == 29 || errorCode == 39 || (errorCode > 200 && errorCode < 304 ) )
                                    {
                                        RunIntroSecne(E_LOGIN_TYPE.auto);
                                    }
                                });
                            }
                        }
                    }  
                }
                else
                {
                    ShowMessagePopup(msg,pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),() =>{ RunIntroSecne(E_LOGIN_TYPE.auto);} );
                }
                
                switch(eID)
                {
                    case E_REQUEST_ID.recruit_refresh:
                    case E_REQUEST_ID.youth_refresh:
                    case E_REQUEST_ID.recruit_get:
                    case E_REQUEST_ID.tutorial_getRecruit:
                    case E_REQUEST_ID.youth_get:
                    {
                        pGameContext.ClearCachePlayerData();
                    }
                    break;
                }

                GetInstance<Ground>().NetworkProcessor(data,false);
            }
        }
        
        if(m_pRequestAfterCallList.ContainsKey(eID))
        {
            m_pRequestAfterCallList.Remove(eID);
        }

        data.Dispose();        
    }

    public void SoketProcessor(NetworkData data)
    {
        if(data == null) return;
        
        Debug.Log( $"SoketProcessor ------------->:\n Json:{(data.Json == null ? 0:data.Json.ToString())}\n");
        
        string msg = null;

        GameContext pGameContext = GameContext.getCtx();
        
        if(!string.IsNullOrEmpty(data.ErrorCode))
        {
            msg = data.ErrorCode;
        }
        
        if(data.Json.ContainsKey("errorCode") )
        {
            if((int)data.Json["errorCode"] != 0)
            {
                GetInstance<Chat>().SetJoin(false);
                msg = (string)data.Json["errorCode"];
            }
            else
            {   
                if(data.Json.ContainsKey("errorMsg"))
                {
                    string errorMsg = (string)data.Json["errorMsg"];
                    if(errorMsg ==  "open")
                    {
                        GetInstance<Chat>().JoinChatServer();
                        pGameContext.ConnectCurrentAuction();
                    }
                }
            }
        }
        
        if(string.IsNullOrEmpty(msg))
        {
            if(data.Json.ContainsKey("msgId") && data.Json["msgId"].Type != JTokenType.Null)
            {
                E_SOCKET_ID eID = (E_SOCKET_ID)((uint)data.Json["msgId"]);
                switch(eID)
                {
                    case E_SOCKET_ID.leave:
                    {
                        GetInstance<Chat>().SetJoin(false);
                    }
                    break;
                    case E_SOCKET_ID.join:
                    {
                        GetInstance<Chat>().SetJoin(true);
                    }
                    break;
                    case E_SOCKET_ID.chatBroadcast:
                    {
                        if(data.Json.ContainsKey("payload") && data.Json["payload"].Type != JTokenType.Null)
                        {
                            GetInstance<Chat>().SoketProcessor(data);
                        }
                    }
                    break;
                    case E_SOCKET_ID.auctionJoin:
                    case E_SOCKET_ID.auctionBid:
                    case E_SOCKET_ID.auctionBidBroadcast:
                    {
                        JObject payload = null;
                        if(data.Json.ContainsKey("payload") && data.Json["payload"].Type != JTokenType.Null)
                        {
                            payload = (JObject)data.Json["payload"];
                            if(eID == E_SOCKET_ID.auctionBid)
                            {
                                pGameContext.UpdateGameData(E_REQUEST_ID.auctionBid,payload,null);
                                UpdateGameMaterials();
                            }
                            else
                            {
                                pGameContext.UpdateCurrentAuctionInfo(eID,payload);
                            }
                        }

                        if(IsShowInstance<Transfer>())
                        {
                            GetInstance<Transfer>().SoketProcessor(data);
                        }
                        
                        if(IsShowInstance<PlayerInfo>())
                        {
                            GetInstance<PlayerInfo>().SoketProcessor(data);
                        }
                    }
                    break;
                }
            }
        }
        else
        {
            if(IsMatch())
            {
                GetInstance<MatchView>().SaveRecordMatchData();
            }
            if(data.ErrorCode == "SoketClose")
            {
                if(Application.internetReachability == NetworkReachability.NotReachable)
                {
                    ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_NETWORK_ERROR"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),() =>{RunIntroSecne(E_LOGIN_TYPE.auto); } );
                }
                else
                {
                    if((int)data.Json["errorCode"] == 400000)
                    {
                        ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_NETWORK_ERROR"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),()=>{
                            if(IsShowInstance<PlayerInfo>())
                            {
                                GetInstance<PlayerInfo>().Close();
                            }
                        } );
                    }
                    else
                    {
                        NetworkManager.ConnectSoketServer();   
                    }
                }
            }
            else
            {
                if(data.Json.ContainsKey("msgId") && data.Json["msgId"].Type != JTokenType.Null)
                {
                    E_SOCKET_ID eID = (E_SOCKET_ID)((uint)data.Json["msgId"]);
                    if(eID == E_SOCKET_ID.auctionJoin && (int)data.Json["errorCode"] == 1671)
                    {
                        if(data.Json.ContainsKey("payload") && data.Json["payload"].Type != JTokenType.Null)
                        {
                            /**
                            * 비딩중인 옥션이면 해딩 에러는 옥션id가 잘못된 경우나 시간이 종료된 경우,
                            * 비딩중인 옥션은 서버에 다시 요청해서 결과를 받는다, 요청은 다음주기로 넘김...동기화 이슈를 막기위해서...
                            * 판매 옥션이면 내부에서 보상처리 요청을 한다.
                            */
                            uint auctionId = pGameContext.JoinAuctionFail((JObject)data.Json["payload"]);
                            if(auctionId > 0)
                            {
                                Debug.Log($"--------------E_SOCKET_ID.auctionJoin:{auctionId}");
                                if(IsShowInstance<PlayerInfo>())
                                {
                                    GetInstance<PlayerInfo>().IsCloseAuctionId(auctionId);
                                }
                                
                                ShowMessagePopup(pGameContext.GetLocalizingErrorMsg("1671"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM") );
                            }
                        }
                    }
                }
                else
                {
                    ShowMessagePopup(pGameContext.GetLocalizingErrorMsg(msg),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM") );
                }
            }
        }
        data.Dispose();        
    }

    #if USE_HIVE

    void OnIAPV4RestoreCB(hive.ResultAPI result, List<hive.IAPV4.IAPV4Receipt> iapv4ReceiptList)
    {
        GameContext pGameContext = GameContext.getCtx();
        if (result.isSuccess()) 
        {
            foreach(hive.IAPV4.IAPV4Receipt receipt in iapv4ReceiptList) 
            {
                pGameContext.AddPurchaseReceipt(receipt);
            }
        } 
        else if (result.errorCode != hive.ResultAPI.ErrorCode.NOT_OWNED)
        {
            NetworkManager.ShowWaitMark(false);
            LayoutManager.Instance.InteractableEnabledAll(null,true);
            ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TITLE_NOTICE"),pGameContext.GetLocalizingErrorMsg(result.errorCode.ToString()),pGameContext.GetLocalizingText("MSG_BTN_OKAY"),null,false);
            return;
        }
        
        if(pGameContext.RestorePurchase())
        {
            NetworkManager.ShowWaitMark(false);
            LayoutManager.Instance.InteractableEnabledAll(null,true);
        }
    }

    void OnPromotionViewCB(hive.ResultAPI pResult, hive.PromotionEventType ePromotionEventType)
    {
        NetworkManager.ShowWaitMark(false);
        
//         switch (ePromotionEventType)
//         {
// // // OPEN					///< \~korean 프로모션 뷰 창이 열렸을 때	\~english  When the Promotion View window opens.
// // // , CLOSE					///< \~korean 프로모션 뷰 창이 닫혔을 때	\~english  When the Promotion View window is closed.
// // // , START_PLAYBACK		///< \~korean 영상 재생이 시작되었을 때	\~english  When Playback starts.
// // // , FINISH_PLAYBACK		///< \~korean 영상 재생이 완료되었을 때	\~english  When Playback is finished.
// // // , EXIT					///< \~korean 종료(더 많은 게임 보기) 팝업에서 종료하기를 선택했을 때	\~english  When you choose to quit from the Quit (see more games) popup.
// // // , GOBACK				///
//             // case hive.PromotionEventType.OPEN:
//             // {
//             //     NetworkManager.ShowWaitMark(false);
//             // }
//             // break;
// //             case hive.PromotionEventType.START_PLAYBACK:
// //             case hive.PromotionEventType.FINISH_PLAYBACK:
// //             {
// //             }
// //                 break;
// //             default:
// //                 // NetworkMgr.Instance.Login();
// //                 break;
//         }
    }
#endif
}