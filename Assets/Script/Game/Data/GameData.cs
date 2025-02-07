using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ALF.LAYOUT;
using ALF;
using System.Runtime.InteropServices;
using System;
using USERDATA;
using MATCHTEAMDATA;
using LEAGUESTANDINGREWARDDATA;
using LADDERSTANDINGREWARD;
using Newtonsoft.Json.Linq;

namespace DATA
{
   
    public enum E_SHOP_ITEM : uint { ShopLimitedItem = 1000,ShopMonthlyItem = 1100,ShopSpecialItem =1200, ShopFreeItem = 2000, ShopCurrencyItem = 3000, ShopMoneyItem = 3100,ShopSpotsItem =3200,ShopManageItem = 3300, ShopSkipItem =4000,ShopAdSkipItem = 4100,MAX = 10}
    public enum E_LOGIN_TYPE : byte { show = 0, auto, direct = 2}
    public enum E_NOTICE_NODE : byte { menu = 0,trophy, clubLicense, seasonPass,fastReward, MAX,NONE}
    public enum E_AD_STATUS : byte { None = 0,RewardLoad,RewardShow,RewardFail,RewardComplete,RewardSkip,RewardClose}

    public enum E_SOKET : byte { auth=0,join,leave,chat,auctionJoin,auctionLeave,auctionBid}
    public enum E_ALIGN : byte { Left=0,Center,Right}
    public enum E_PLAYER_INFO_TAB : byte { information = 0,record,offer,MAX}
    public enum E_PLAYER_INFO_TYPE : byte { my = 0,away,bidding,auction,match,recuiting,youth};
    public enum E_DIRECTION : byte { Up = 0,Down = 1,Left = 2,Right = 3,LeftUp = 4,RightUp = 5,LeftDown = 6,RightDown = 7,None = 8,};
    public enum E_DATA_TYPE : byte { None = 0,en_US,ko_KR,zh_CN,zh_TW,fr_FR,de_DE,es_ES,pt_BR,in_ID, AdReward,Business,ConstValue,PlayerPotentialWeightSum,PlayerAbilityWeightConversion,TrainingCost,PositionFamilar,PlayerNationality,UserRank,MatchCommentary,UserData,LadderStandingReward,LadderSeasonNo,RewardSet,AttendReward,LadderReward,Pass,PassMission,ClubNationality,Shop,ShopProduct,GameTip,ClubLicenseMission,MileageMission,AchievementMission,EventCondition,QuestMission,Push,LeagueStandingRewardData,ItemData,Fword,TimeSale };

    public enum E_TEXT_NAME : byte { CHALLENGESTAGE_TITLE_CHALLENGE_STAGE = 0,LEAGUE_TITLE_CONFERENCE_2,LEAGUE_TITLE_CONFERENCE_1,LEAGUE_TITLE_CHAMPION,LEAGUE_TITLE_ULTIMATE_CHAMPION,MAX}

    public enum E_REQUEST_ID : uint { 
        ServerInfo = 0,
        account_login = 20100,
        home_get = 20200,
        home_reset= 20201,
        user_getProfile = 20301,
        item_get = 20500,
        item_use = 20501,
        mail_get = 20600,
        mail_read = 20602,
        mail_reward = 20601,
        mail_delete = 20603,
        mileage_get = 20800,
        mileage_reward = 20801,
        quest_get = 21100,
        quest_reward = 21101,
        achievement_get = 21200,
        achievement_reward = 21201,
        pass_get = 21400,
        pass_reward = 21401,
        attend_get = 21600,
        attend_reward = 21601,
        shop_get = 21700,
        shop_buy = 21701,
        shop_rewardFlatRate = 21702,
        timeSale_get = 21800,//	타임세일 기록 획득		
        timeSale_put = 21801,//	타임세일 기록 추가	no: 타임세일 no	"no: 타임세일 no tExpire: 만료시간"
        iap_purchase = 22000,
        iap_reserve = 22001,
        coupon_reward = 22100,
        settings_get = 22400,
        settings_put = 22401,
        club_create = 25001,
        club_changeEmblem = 25002,
        club_changeProfile = 25003,
        club_profile = 25004,
        club_top100 = 25005,
        club_upgradeCapacity = 25006,
        player_get = 25100,
        player_abilityUp = 25101,
        player_positionFamiliarUp = 25102,
        player_release = 25103,
        player_profile = 25105,
        player_changeProfile = 25106,
        player_recoverHP = 25110,
        playerStats_seasonStats = 25150,
        playerStats_top100 = 25151,
        business_get = 25200,
        business_levelUp = 25201,
        business_reward = 25202,
        business_rewardTraining = 25203,
        tactics_get = 25400,
        tactics_put = 25401,
        tactics_load = 25402,
        lineup_get = 25500,
        lineup_put = 25501,
        // lineup_changeTactics = 25502,
        recruit_get = 25600,
        recruit_refresh = 25601,
        recruit_offer = 25602,
        youth_get = 25700,
        youth_refresh = 25701,
        youth_offer = 25702,
        scout_reward = 25801,
        auction_get = 26000,
        auction_register = 26001,
        auction_cancel = 26002,
        auction_withdraw = 26003,
        auction_refund = 26004,
        auction_reward = 26005,
        auction_trade = 26006,
        auction_status = 26007,

        ladder_try = 26101,
        ladder_clear = 26102,
        ladder_rewardStanding = 26103,
        ladder_rewardUserRank = 26104,
        ladder_skipMatch = 26105,
        matchStats_list = 26200,
        matchStats_getSummary = 26201,
        matchStats_getPlayers = 26202,
        fastReward_reward = 26301,
        adReward_get = 26400,
        adReward_reward = 26401,

        challengeStage_getStandings = 26500,
        challengeStage_searchOpps = 26501,
        challengeStage_try = 26502,
        challengeStage_clear = 26503,
        challengeStage_skipMatch = 26504,
        challengeStage_getMatches = 26505,

        league_getStandings = 26600,
        league_try = 26602,
        league_clear = 26603,
        league_skipMatch = 26604,
        league_getLeaders = 26605,
        league_getTodayFixture = 26606,
        league_getHistory = 26607,

        tutorial_put = 26900,
        tutorial_business = 26901,
        tutorial_recruit = 26902,
        tutorial_getRecruit = 26903,
        clubLicense_get = 27000,	
        clubLicense_reward = 27001,
        clubLicense_put = 27002,

        auctionBid = uint.MaxValue -2,
        review_popup = uint.MaxValue -1,
#if USE_HIVE
        hive_news = uint.MaxValue,
#endif

    }

    public enum E_SOCKET_ID : uint {

        // * 아래  socket 통신용이다.	
        auth = 40002,
        join = 40100,
        leave = 40101,
        chat = 40102,
        chatBroadcast = 40103,

        auctionJoin = 40200,
        auctionLeave = 40201,
        auctionBid = 40202,
        auctionBidBroadcast = 40203,
        
    }

    public enum E_TRAINING_TYPE : byte { attacking = 0, defending, physicality,mentality,goalkeeping,MAX}
    public enum E_BUILDING:byte { office, ticketbox, parkingArea, giftShop, vendingMachine, droneCamera, youthClub, adBoard, clubCafe, shuttleBus, medicalCenter, rooftopAdBoard, footballPark, statue, hallOfFame,trainingGround,ClubHouse,Stadium,MAX } 
    public enum E_PLAYER_CREATE_TYPE:byte{ initialize = 0,market,recruit,youth,MAX}
    public enum E_NOTICE : byte { substitution = 0,goal,card,injury, MAX}
    public enum E_TEAM_TACTICS : byte
    {
        TEAM_TACTICS_DEFENCELINE = 0
        , TEAM_TACTICS_START = TEAM_TACTICS_DEFENCELINE				// 수비라인
        , TEAM_TACTICS_PASS_DIRECTION_WIDE		// 패스방향 (높을수록 측면)
        , TEAM_TACTICS_OFFSIDE_TRAP				// 오프사이드 트랩 빈도
        , TEAM_TACTICS_WASTE_TIME				// 시간보내기 (높을수록 데드볼에서 시간 낭비)
        , TEAM_TACTICS_WIDTH					// 좌우 폭 (높을수록 넓게)
        , TEAM_TACTICS_TEMPO					// 공격템포
        , TEAM_TACTICS_END
    };
    //////////////////////////////////////////////////////////////////////////
    // 선수전술
    //////////////////////////////////////////////////////////////////////////
    public enum E_PLAYER_TACTICS  : byte
    {
        PLAYER_TACTICS_PASS_LENGTH = 0
        , PLAYER_TACTICS_START = PLAYER_TACTICS_PASS_LENGTH			// 패스유형 (높을수록 긴 다이렉트 패스)
        , PLAYER_TACTICS_DRIBBLE				// 드리블 빈도
        , PLAYER_TACTICS_LONGSHOT				// 중거리슛 빈도
        , PLAYER_TACTICS_THROUGH_PASS			// 쓰루패스 빈도
        , PLAYER_TACTICS_OVERLAPPING			// 오버래핑 빈도 및 깊이
        , PLAYER_TACTICS_CROSS					// 크로스 빈도 (높을수록 얼리크로스 자주)
        , PLAYER_TACTICS_PRESSURE_RANGE			// 압박범위
        , PLAYER_TACTICS_DEFENCE_TYPE_MARKING	// 수비형태(낮으면 지역방어, 높을수록 대인마크 위주)
        , PLAYER_TACTICS_MARKING_DISTANCE		// 근접마킹(높을수록 가까이서 마킹)
        , PLAYER_TACTICS_END
    };
    public enum E_APP_MATCH_TYPE  : byte
    {
        APP_MATCH_TYPE_NONE                    = 0
        , APP_MATCH_TYPE_INTERNAL            = 1    // 경기가 내부에 있어서 막 조작이 가능
        , APP_MATCH_TYPE_REPLAY_FILE_READ    = 2    // 이미 끝난 경기의 리플레이. 조작불가
        , APP_MATCH_TYPE_GIVEN                = 3 // 경기가 외부에 있음. 일부 조작 가능.
        , APP_MATCH_TYPE_CLEAR                = 10 // 경기 정상 종료
        , APP_MATCH_TYPE_GIVEUP                = 90 // 경기 포기
        , APP_MATCH_TYPE_SHUTDOWN                = 99 // 경기 비정상 종료
        
    };

    public enum E_COMMON_STATISTICS : byte
    {
        COMMON_STATISTICS_GOAL	= 0				// 득점
        , COMMON_STATISTICS_ASSIST				// 도움
        , COMMON_STATISTICS_SHOOT				// 슈팅
        , COMMON_STATISTICS_SHOOT_ONTARGET		// 유효슈팅
        , COMMON_STATISTICS_PASSTRY				// 패스시도
        , COMMON_STATISTICS_PASSSUCCESS			// 패스성공
        , COMMON_STATISTICS_INTERCEPT			// 인터셉트
        , COMMON_STATISTICS_GK_GOOD_DEFENCE		// GK 선방
        , COMMON_STATISTICS_HEADER				// 헤딩			?? 지금 안가고 있는듯?
        , COMMON_STATISTICS_STEAL				// 스틸
        , COMMON_STATISTICS_CORNERKICK			// 코너킥
        , COMMON_STATISTICS_FREEKICK			// 프리킥
        , COMMON_STATISTICS_PENALTYKICK			// 페널티킥
        , COMMON_STATISTICS_PENALTYKICK_GOAL	// 페널티킥 성공(골)
        , COMMON_STATISTICS_OWN_GOAL			// 자살골
        , COMMON_STATISTICS_INJURY				// 부상
        , COMMON_STATISTICS_FOUL				// 반칙
        , COMMON_STATISTICS_YELLO_CARD			// 경고
        , COMMON_STATISTICS_RED_CARD			// 퇴장
        , COMMON_STATISTICS_END
    };


    public enum E_LOCATION  : sbyte
    {
        LOC_NONE = -1
        , LOC_GK = 0
        , LOC_START = LOC_GK
        , LOC_DL,  LOC_DCL,  LOC_DCC,  LOC_DCR,  LOC_DR	
        , LOC_DML, LOC_DMCL, LOC_DMCC, LOC_DMCR, LOC_DMR
        , LOC_ML,  LOC_MCL,  LOC_MCC,  LOC_MCR,  LOC_MR
        , LOC_AML, LOC_AMCL, LOC_AMCC, LOC_AMCR, LOC_AMR
        , LOC_FL,  LOC_FCL,  LOC_FCC,  LOC_FCR,  LOC_FR
        , LOC_END

        // 후보
        , SUB_START = 101

    };

    public enum E_ABILINDEX : byte
    {	
        AB_ATT_PASS		= 0
        ,AB_START = AB_ATT_PASS
        ,AB_ATT_CROSS		,AB_ATT_SHOOT	   ,AB_ATT_OFFTHEBALL		,AB_ATT_DRIBBLE		,AB_ATT_TOUCH
        ,AB_DEF_PRESSURE	,AB_DEF_MARKING	   ,AB_DEF_POSITIONING		,AB_DEF_TACKLE		,AB_DEF_INTERCEPT	,AB_DEF_CONCENTRATION
        ,AB_PHY_SPEED		,AB_PHY_ACCEL	   ,AB_PHY_AGILITY			,AB_PHY_BALANCE		,AB_PHY_STAMINA		,AB_PHY_JUMP
        ,AB_MEN_CREATIVITY  ,AB_MEN_VISION	   ,AB_MEN_DETERMINATION	,AB_MEN_DECISION	,AB_MEN_AGGRESSIVE	,AB_MEN_INFLUENCE
        ,AB_GK_REFLEX		,AB_GK_COMMAND	   ,AB_GK_GOALKICK			,AB_GK_ONEONONE		,AB_GK_HANDLING		,AB_GK_AERIALABILITY
        ,AB_END
    };

    public enum E_TALENT : byte
    {
        TALENT_TECHNICAL = 0
        , TALENT_START = TALENT_TECHNICAL	, TALENT_PHYSICAL		, TALENT_MENTAL		, TALENT_HARDWORKING
        , TALENT_INJURYPRONE					, TALENT_GENIUS							
        , TALENT_END
    };

    public enum E_COMMAND : byte
    {
        COMMAND_NONE        = 0,
    
        COMMAND_GAME_KICKOFF,    // 킥오프라는 데드볼 상황을 별도로 설정. 데드볼에서 빠져 나오는 규칙과 동일하기때문. 골 이후의 킥오프는 그냥 GOAL 상태임
        COMMAND_GAME_PLAYING,
        COMMAND_GAME_HALFTIME,
        COMMAND_GAME_GAMEOVER,
        
        COMMAND_GAME_TIME,
        COMMAND_GAME_SCORE,
        COMMAND_GAME_POSSESSION_TOTAL,
        COMMAND_GAME_BALL_CHANGE,
        
        COMMAND_SUBSTITUTION_WAIT,
        COMMAND_TATICS_WAIT,
        
        COMMAND_TIMELINE_GOAL,
        COMMAND_TIMELINE_SUBSTITUTION,
        COMMAND_TIMELINE_REDCARD,
        COMMAND_TIMELINE_YELLOWCARD,
        COMMAND_TIMELINE_INJURY,
        
        
        COMMAND_BROAD_CAST_TEXT,
        
        COMMAND_SOUND_HEADING,
        COMMAND_SOUND_PASS,
        COMMAND_SOUND_SHOT,
        
    };

    public enum E_BROADCAST_TEXT_EVENT_TYPE: byte
	{
		NONE = 0
		, SHOOT
		, SHOOT_OUT
		, PASS
		, LONGPASS
		, CROSS
		, DRIBBLE
		, TOUCH
		, CLEAR
		, BLOCK
		, TACKLE
		, INTERCEPT
		, GK_HOLD
		, GK_PUNCH
		, GK_GOOD_DEFENCE
		, SHOOT_ON_TARGET
		, GOAL
		, ASSIST
		, SIDE_OUT
		, GOAL_OUT
		, CORNER_OUT
		, FOUL
		, YELLOW_CARD
		, RED_CARD
		, FREEKICK_DIRECT_READY
		, PENALTYKICK_READY
		, THROW_IN
		, FREEKICK_DIRECT
		, FREEKICK_INDIRECT
		, PENALTYKICK
		, CORNERKICK
		, GOALKICK
		, INJURY
		, SUBSTITUTION_READY
		, SUBSTITUTION
		, KICK_OFF_FIRST_HALF
		, KICK_OFF_SECOND_HALF
		, HALFTIME
		, GAMEOVER
		, SCORE_FIRST_BLOOD
		, SCORE_TIE

		, SCORE_CHASE
		, SCORE_TIE_BREAKER
		, SCORE_TURN_AROUND

		// 추가 이벤트, 2013-08-22 신대리버전
		, OFFSIDE

		, ENTER_EXTRATIME
		, START_EXTRATIME_FIRST
		, START_EXTRATIME_SECOND
		, ENTER_PENALTIES
		, GAMEOVER_EXTRATIME
		, GAMEOVER_PENALTIES
		, HALFTIME_EXTRATIME

		, PENALTYKICK_GOAL	
		, PENALTY_SHOOTOUT_TEAM	// 선축팀
		, PENALTY_SHOOTOUT_READY
		, PENALTY_SHOOTOUT_GOAL
		, PENALTY_SHOOTOUT_MISS
		, PENALTY_SHOOTOUT_GK

		, HIT_CROSSBAR
		, GK_CLEAR
		, FORFEITED				// 몰수패
		, FANBUFF					// 팬버프
		, END

		// 추가방법: (디파인에서 우측 키워드만 찾으면 다 나옴. 예: GAMEOVER_EXTRATIME)
		// 1. 이곳에 디파인
		// 2. 클래스 작성 (디파인으로 찾아보면나옴)
		// 3. 클래스를 
	};

    public enum E_HISTORY : byte { matchPlayed =0,winStreak,bigWin,goals,mostGoals,mostAssists,transferSpending,transferIncome,netSpend,MAX}
    
    public class RewardRankingItem : IBase
    {
        public uint ID {get; private set;} 
        public RectTransform Target  {get; private set;}
        
        public GameObject[] Wins {get; private set;}
        public TMPro.TMP_Text NumText {get; private set;}

        public RectTransform Rewards  {get; private set;}

        public RewardRankingItem( RectTransform taget)
        {
            Target = taget;

            NumText = taget.Find("4/text").GetComponent<TMPro.TMP_Text>();
            Wins = new GameObject[4];
            Wins[0] = taget.Find("1").gameObject;
            Wins[1] = taget.Find("2").gameObject;
            Wins[2] = taget.Find("3").gameObject;
            Wins[3] = taget.Find("4").gameObject;

            Rewards = taget.Find("rewards").GetComponent<RectTransform>();
        }

        public void Dispose()
        {
            int i =0;
            for(i=0; i < Wins.Length; ++i)
            {
                Wins[i] = null;
            }
            NumText = null;
            Wins = null;
            
            RemoveRewards();
            Rewards = null;
            LayoutManager.Instance.AddItem("RankingRewardItem",Target);
            Target = null;
        }
        
        void RemoveRewards()
        {
            int i = Rewards.childCount;
            RectTransform item = null;
            while(i > 0)
            {
                --i;
                item = Rewards.GetChild(i).GetComponent<RectTransform>();
                 item.localScale = Vector3.one;
                SingleFunc.AddRewardIcon(item,"RewardItem");
            }
        }

        public void UpdateInfo(int index,int iMatchTyp)
        {
            if(index < 0) return;

            ID = (uint)index;
            
            RectTransform pReward = null;
            Vector2 size;
            float w = 0;
            Vector2 anchor = new Vector2(0, 0.5f);
            
            for(int i =0; i < Wins.Length -1; ++i)
            {
                Wins[i].SetActive(i == index);
            }

            Wins[3].SetActive(index > 2);
            RemoveRewards();

            GameContext pGameContext = GameContext.getCtx();
            if(iMatchTyp == GameContext.LADDER_ID)
            {
                w = 30;
                LadderStandingRewardList pLadderStandingRewardList = pGameContext.GetFlatBufferData<LadderStandingRewardList>(E_DATA_TYPE.LadderStandingReward);
                LadderStandingRewardItem? pLadderStandingRewardItem = pLadderStandingRewardList.LadderStandingReward(index);
                if(pLadderStandingRewardItem.Value.Type == 0)
                {
                    NumText.SetText($"{pLadderStandingRewardItem.Value.Top} - {pLadderStandingRewardItem.Value.Bottom}");
                }
                else
                {
                    NumText.SetText(string.Format(pGameContext.GetLocalizingText("RANKINGREWARD_TXT_RANKING_PERCENT"),pLadderStandingRewardItem.Value.Bottom) );
                }
                
                uint id = pLadderStandingRewardItem.Value.Reward1 == GameContext.FREE_CASH_ID ? GameContext.CASH_ID : pLadderStandingRewardItem.Value.Reward1;
                if(id > 0)
                {
                    pReward = SingleFunc.GetRewardIcon(Rewards,"RewardItem",id,pLadderStandingRewardItem.Value.RewardAmount1);
                    pReward.localScale = Vector3.one * 0.6f;
                    pReward.anchoredPosition = new Vector2(w,0);
                    size = pReward.sizeDelta;
                    w += size.x* 0.6f + 20;
                }
                id = pLadderStandingRewardItem.Value.Reward2 == GameContext.FREE_CASH_ID ? GameContext.CASH_ID : pLadderStandingRewardItem.Value.Reward2;
                if(id > 0)
                {
                    pReward = SingleFunc.GetRewardIcon(Rewards,"RewardItem",id,pLadderStandingRewardItem.Value.RewardAmount2);
                    pReward.localScale = Vector3.one * 0.6f;
                    pReward.anchoredPosition = new Vector2(w,0);
                    size = pReward.sizeDelta;
                    w += size.x* 0.6f + 20;
                }
                id = pLadderStandingRewardItem.Value.Reward3 == GameContext.FREE_CASH_ID ? GameContext.CASH_ID : pLadderStandingRewardItem.Value.Reward3;
                if(id > 0)
                {
                    pReward = SingleFunc.GetRewardIcon(Rewards,"RewardItem",id,pLadderStandingRewardItem.Value.RewardAmount3);
                    pReward.localScale = Vector3.one * 0.6f;
                    pReward.anchoredPosition = new Vector2(w,0);
                    size = pReward.sizeDelta;
                    w += size.x* 0.6f + 20;
                }
            }
            else
            {
                LeagueStandingRewardList pLeagueStandingRewardList = pGameContext.GetFlatBufferData<LeagueStandingRewardList>(E_DATA_TYPE.LeagueStandingRewardData);
                LeagueStandingItem? pLeagueStandingItem = pLeagueStandingRewardList.LeagueStandingRewardByKey((byte)iMatchTyp);
                LeagueStandingRewardItem? pLeagueStandingRewardItem = pLeagueStandingItem.Value.List(index);

                NumText.SetText(pLeagueStandingRewardItem.Value.Top.ToString());
                
                if(pLeagueStandingRewardItem.Value.Reward1 > 0)
                {
                    pReward = SingleFunc.GetRewardIcon(Rewards,"RewardItem",pLeagueStandingRewardItem.Value.Reward1,pLeagueStandingRewardItem.Value.RewardAmount1);
                    pReward.localScale = Vector3.one * 0.6f;
                    pReward.anchoredPosition = new Vector2(w,0);
                    size = pReward.sizeDelta;
                    w = ( size.x * 0.6f * 0.5f) + 5f;
                }

                if(pLeagueStandingRewardItem.Value.Reward2 > 0)
                {
                    pReward.anchoredPosition = new Vector2(-w,0);

                    pReward = SingleFunc.GetRewardIcon(Rewards,"RewardItem",pLeagueStandingRewardItem.Value.Reward2,pLeagueStandingRewardItem.Value.RewardAmount2);
                    pReward.localScale = Vector3.one * 0.6f;
                    pReward.anchoredPosition = new Vector2(w,0);
                    size = pReward.sizeDelta;
                    
                    w += size.x * 0.6f + 20;
                }
            }
        }
    }
    
    public class NationItem : IBase
    {
        public string ID {get; private set;} 
        public RectTransform Target  {get; private set;}
        public TMPro.TMP_Text NameText {get; private set;}
        
        public RawImage Icon {get; private set;}
        public Button Info {get; private set;}
        public GameObject On {get; private set;}
        string m_strItemName = null;
        
        public NationItem( RectTransform taget,string pItemName)
        {
            m_strItemName = pItemName;
            Target = taget;
            Info = taget.GetComponent<Button>();
            On = taget.Find("on").gameObject;
            NameText = taget.Find("text").GetComponent<TMPro.TMP_Text>();
            Icon = taget.Find("nation").GetComponent<RawImage>();
            
            Target.localScale = Vector2.one;
        }

        public void UpdateNationItem(string pNationCode, string current)
        {
            string[] token = pNationCode.Split(':');
            ID = token[1];
            NameText.SetText(token[0]);

            On.SetActive(current == ID);

            Sprite pSprite = AFPool.GetItem<Sprite>("Texture",ID);
            Icon.texture = pSprite.texture;
        }

        public void Dispose()
        {
            Info.onClick.RemoveAllListeners();
            Info = null;
            On = null;
            NameText = null;
            Icon.texture = null;

            LayoutManager.Instance.AddItem(m_strItemName,Target);
            m_strItemName = null;
            Target = null;
        }
    }
    
    public class RankingPlayerItem : IBase
    {
        public ulong Id {get; private set;}

        public TMPro.TMP_Text PlayerName {get; private set;}
        public TMPro.TMP_Text ClubName {get; private set;}
        public TMPro.TMP_Text NoText {get; private set;}
        
        public TMPro.TMP_Text WinText {get; private set;}
        public TMPro.TMP_Text OverallText {get; private set;}
        public GameObject[] Ranking {get; private set;}
        public GameObject[] SubHeader {get; private set;}
        TMPro.TMP_Text[] m_pSubText = null;

        string m_pItemName = null;

        public RectTransform Target  {get; private set;}
        
        Button m_pButton = null;

        public RankingPlayerItem(RectTransform target,string pItemName)
        {
            m_pItemName = pItemName;
            Target = target;
            m_pButton = target.GetComponent<Button>();
            
            PlayerName = target.Find("player").GetComponent<TMPro.TMP_Text>();
            ClubName = target.Find("club").GetComponent<TMPro.TMP_Text>();
            NoText = target.Find("ranking/text").GetComponent<TMPro.TMP_Text>();
            Ranking = new GameObject[3];
            Transform tm = target.Find("ranking");
            Ranking[0]= tm.Find("1").gameObject;
            Ranking[1]= tm.Find("2").gameObject;
            Ranking[2]= tm.Find("3").gameObject;

            SubHeader = new GameObject[14];
            m_pSubText = new TMPro.TMP_Text[13];

            SubHeader[1] = target.Find("goals").gameObject;
            SubHeader[2] = target.Find("assists").gameObject;
            SubHeader[10] = target.Find("prossessionWon").gameObject;
            SubHeader[13] = target.Find("saves").gameObject;
            
            int index = 0;
            for(int i =0; i < SubHeader.Length; ++i)
            {
                if(SubHeader[i] != null)
                {
                    TMPro.TMP_Text[] list = SubHeader[i].GetComponentsInChildren<TMPro.TMP_Text>(true);
                    for(int n =0; n < list.Length; ++n)
                    {
                        m_pSubText[index] = list[n];
                        ++index;
                    }
                }
            }
        }

        public void Dispose()
        {
            int i =0;
            for(i =0; i < Ranking.Length; ++i)
            {
                Ranking[i] = null;
            }

            for(i =0; i < SubHeader.Length; ++i)
            {
                SubHeader[i] = null;
            }
            SubHeader = null;

            for(i =0; i < m_pSubText.Length; ++i)
            {
                m_pSubText[i] = null;
            }
            m_pSubText = null;
            m_pButton.onClick.RemoveAllListeners();
            m_pButton = null;
            
            PlayerName = null;
            ClubName = null;
            NoText = null;
            Ranking = null;

            LayoutManager.Instance.AddItem(m_pItemName,Target);
            m_pItemName = null;
            Target = null;
        }

        public void UpdateData(JObject pObject,int count,int eTab)
        {
            if(pObject == null) return;
            
            Id = (ulong)pObject["player"];

            for(int i =0; i < Ranking.Length; ++i)
            {
                Ranking[i].SetActive(count == i);
            }

            NoText.SetText((count+1).ToString());
            NoText.gameObject.SetActive(count > 2);

            PlayerName.SetText(string.Format("{0}.{1}",((string)pObject["forename"])[0],(string)pObject["surname"]));
            ClubName.SetText((string)pObject["clubName"]);
            
            for(int i =0; i < SubHeader.Length; ++i)
            {
                if(SubHeader[i] != null)
                {
                    SubHeader[i].SetActive((int)eTab == i);
                }
            }

            int value = 0;
            if(pObject.ContainsKey("games") && pObject["games"].Type != JTokenType.Null)
            {
                value = (int)pObject["games"];
            }

            if(eTab == 1)
            {
                m_pSubText[0].SetText(value > 0 ? value.ToString(): "-");
                int goals = 0;
                if(pObject.ContainsKey("goals") && pObject["goals"].Type != JTokenType.Null)
                {
                    goals = (int)pObject["goals"];
                }
                m_pSubText[2].SetText(string.Format("{0:0.0#}", (float)goals/(float)value));
            }
            else if(eTab == 2)
            {
                m_pSubText[3].SetText(value > 0 ? value.ToString(): "-");
                int assists =0;
                if(pObject.ContainsKey("assists") && pObject["assists"].Type != JTokenType.Null)
                {
                    assists = (int)pObject["assists"];
                }
                m_pSubText[4].SetText(assists > 0 ? assists.ToString(): "-");
                m_pSubText[5].SetText(string.Format("{0:0.0#}", (float)assists/(float)value));
            }
            else if(eTab == 10)
            {
                
                m_pSubText[6].SetText(value > 0 ? value.ToString(): "-");
                int tackles =0;
                int interceptions =0;
                if(pObject.ContainsKey("tackles") && pObject["tackles"].Type != JTokenType.Null)
                {
                    tackles = (int)pObject["tackles"];
                }
                if(pObject.ContainsKey("interceptions") && pObject["interceptions"].Type != JTokenType.Null)
                {
                    interceptions = (int)pObject["interceptions"];
                }

                value = tackles + interceptions;
                m_pSubText[7].SetText(value > 0 ? value.ToString(): "-");
                m_pSubText[8].SetText(tackles > 0 ? tackles.ToString(): "-");
                m_pSubText[9].SetText(interceptions > 0 ? interceptions.ToString(): "-");
            }
            else if(eTab == 13)
            {
                int saves= 0;
                if(pObject.ContainsKey("saves") && pObject["saves"].Type != JTokenType.Null)
                {
                    saves = (int)pObject["saves"];
                }
                m_pSubText[10].SetText(value > 0 ? value.ToString(): "-");
                m_pSubText[11].SetText(saves > 0 ? saves.ToString(): "-");                
                m_pSubText[12].SetText(string.Format("{0:0.0#}", (float)saves/(float)value));
            }
        }
    }
    
    public class ChallengeItemData : IBase
    {
        public ulong ID {get; private set;} 
        public string Name {get; private set;}
        public uint SquadPower {get; private set;}
        public byte Rank {get; private set;}
        public uint Standing {get; private set;}
        public byte[] Emblem  {get; private set;}
        
        public ChallengeItemData( JObject data)
        {
            ID = (ulong)data["club"];
            Name = (string)data["name"];
            SquadPower = (uint)data["squadPower"];
            
            Standing = (uint)data["standing"];
            
            if(data.ContainsKey("emblem") && data["emblem"].Type != JTokenType.Null)
            {
                Emblem = SingleFunc.GetMakeEmblemData((string)data["emblem"]);
            }
            else
            {
                Emblem = SingleFunc.CreateRandomEmblem();
            }

            Rank = 0;
            if(data.ContainsKey("userRank") && data["userRank"].Type != JTokenType.Null)
            {
                Rank = (byte)data["userRank"];
            }
        }

        public void Dispose()
        {
            Emblem = null;
        }
    }

    
    public class ChallengeItem : IBase
    {
        public ulong ID {get; private set;} 
        public RectTransform Target  {get; private set;}
        public TMPro.TMP_Text Name {get; private set;}
        public Button Info {get; private set;}
        public TMPro.TMP_Text SquadPower {get; private set;}
        public TMPro.TMP_Text Ranking {get; private set;}
        public RawImage Rank {get; private set;}

        public int Standing {get; private set;}

        public EmblemBake Emblem  {get; private set;}

        string m_strItemName = null;
        
        public ChallengeItem( RectTransform taget, string itemName)
        {
            m_strItemName = itemName;
            Target = taget;
            
            Info = Target.GetComponent<Button>();
            Target.gameObject.name = ID.ToString();
            Name = Target.Find("name").GetComponent<TMPro.TMP_Text>();
            SquadPower = Target.Find("over/text").GetComponent<TMPro.TMP_Text>();
            
            Ranking = Target.Find("ranking").GetComponent<TMPro.TMP_Text>();
            Emblem = Target.Find("emblem").GetComponent<EmblemBake>();
            Rank = Target.Find("rank").GetComponent<RawImage>();
        }

        public void Dispose()
        {
            Name = null;
            SquadPower = null;   
            Ranking = null;
            Rank = null;
            Emblem.Dispose();
            Emblem = null;

            Info.onClick.RemoveAllListeners();
            Info = null;
            LayoutManager.Instance.AddItem(m_strItemName,Target);
            m_strItemName = null;
            SingleFunc.ClearRankIcon(Rank);
            Target = null;
        }

        public void UpdateInfo(ChallengeItemData pChallengeItemData)
        {
            if(pChallengeItemData == null) return;

            ID = pChallengeItemData.ID;
            Name.SetText(pChallengeItemData.Name);
            SquadPower.SetText(GameContext.getCtx().GetLocalizingText("CHALLENGESTAGE_MATCH_TXT_OVERALL") + ": " + pChallengeItemData.SquadPower.ToString());

            Standing = (int)pChallengeItemData.Standing;
            Ranking.SetText(Standing.ToString());

            Emblem.SetupEmblemData(pChallengeItemData.Emblem);
            SingleFunc.SetupRankIcon(Rank,pChallengeItemData.Rank);
        }
    }

    public class ClubHistory : IBase
    {
        public ulong Club { get; set; }
        public uint SeasonEntered { get; set; }
        public uint TrophyHigh { get; set; }
        public uint StandingHigh { get; set; }
        
        public uint Game { get; set; }
        public uint Win { get; set; }
        public uint Lose { get; set; }
        public uint Goals { get; set; }
        public uint Assists { get; set; }
        public uint CleanSheet { get; set; }
        public uint WinStreak { get; set; }
        public uint BigWinHome { get; set; }
        public uint BigWinAway { get; set; }
        public string BigWinMatch { get; set; }
        public uint MostGoals { get; set; }
        public string MostGoalsPlayerName { get; set; }
        public uint MostAssists { get; set; }
        public string MostAssistsPlayerName { get; set; }
        public long TransferSpending { get; set; }
        public long TransferIncome { get; set; }
        public long NetSpend { get; set; }

        public static ClubHistory Create(JObject data)
        {
            ClubHistory pClubHistory = new ClubHistory();
            pClubHistory.Club = (ulong)data["club"];
            pClubHistory.SeasonEntered = data.ContainsKey("seasonEntered") ? (uint)data["seasonEntered"] : 0;
            pClubHistory.TrophyHigh = data.ContainsKey("trophyHigh") ? (uint)data["trophyHigh"] : 0;
            pClubHistory.StandingHigh = data.ContainsKey("standingHigh") ? (uint)data["standingHigh"] : 0;
            pClubHistory.Game = data.ContainsKey("game") ? (uint)data["game"] : 0;
            pClubHistory.Win = data.ContainsKey("win") ? (uint)data["win"] : 0;
            pClubHistory.Lose = data.ContainsKey("lose") ? (uint)data["lose"] : 0;
            pClubHistory.Goals = data.ContainsKey("goals") ? (uint)data["goals"] : 0;
            pClubHistory.Assists = (data.ContainsKey("assists") && data["assists"].Type != JTokenType.Null )? (uint)data["assists"] : 0;
            pClubHistory.CleanSheet = data.ContainsKey("cleanSheet") ? (uint)data["cleanSheet"] : 0;
            pClubHistory.WinStreak = data.ContainsKey("winStreak") ? (uint)data["winStreak"] : 0;
            pClubHistory.BigWinHome = data.ContainsKey("bigWinHome") ? (uint)data["bigWinHome"] : 0;
            pClubHistory.BigWinAway = data.ContainsKey("bigWinAway") ? (uint)data["bigWinAway"] : 0;
            pClubHistory.BigWinMatch = (data.ContainsKey("bigWinMatch") && data["bigWinMatch"].Type != JTokenType.Null) ? (string)data["bigWinMatch"] : "";
            pClubHistory.MostGoals = data.ContainsKey("mostGoals") ? (uint)data["mostGoals"] : 0;
            pClubHistory.MostGoalsPlayerName = (data.ContainsKey("mostGoalsPlayerName") && data["mostGoalsPlayerName"].Type != JTokenType.Null) ? (string)data["mostGoalsPlayerName"] : "";
            pClubHistory.MostAssists = data.ContainsKey("mostAssists") ? (uint)data["mostAssists"] : 0;
            pClubHistory.MostAssistsPlayerName =(data.ContainsKey("mostAssistsPlayerName")&& data["mostAssistsPlayerName"].Type != JTokenType.Null)? (string)data["mostAssistsPlayerName"] : "";
            pClubHistory.TransferSpending = (data.ContainsKey("transferSpending") && data["transferSpending"].Type != JTokenType.Null)? (long)data["transferSpending"] : 0;
            pClubHistory.TransferIncome = (data.ContainsKey("transferIncome") && data["transferIncome"].Type != JTokenType.Null)? (long)data["transferIncome"] : 0;
            pClubHistory.NetSpend = (data.ContainsKey("netSpend") && data["netSpend"].Type != JTokenType.Null)? (long)data["netSpend"] : 0;
            
            return pClubHistory;
        }
        protected ClubHistory(){}

        public void Dispose()
        {

        }
    }

    public class ClubSeasonStats : IBase
    {
        public ulong Club { get; set; }
        public uint SeasonNo { get; set; }
        public uint MatchType { get; set; }
        public uint Rank { get; set; }
        public uint Trophy { get; set; }
        public uint TrophyHigh { get; set; }
        public uint Standing { get; set; }
        public uint StandingHigh { get; set; }
        public uint SquadPower { get; set; }
        
        public uint Game { get; set; }
        public uint Win { get; set; }
        public uint Lose { get; set; }
        public uint Goals { get; set; }
        public uint Assists { get; set; }
        public uint CleanSheet { get; set; }
        public uint WinStreak { get; set; }
        public uint BigWinHome { get; set; }
        public uint BigWinAway { get; set; }
        public string BigWinMatch { get; set; }
        public uint MostGoals { get; set; }
        public string MostGoalsPlayerName { get; set; }
        public uint MostAssists { get; set; }
        public string MostAssistsPlayerName { get; set; }
        
        public long TransferSpending { get; set; }
        public long TransferIncome { get; set; }
        public long NetSpend { get; set; }
            
        public static ClubSeasonStats Create(JObject data)
        {
            ClubSeasonStats pClubSeasonStats = new ClubSeasonStats();
            pClubSeasonStats.Club = (ulong)data["club"];
            pClubSeasonStats.SeasonNo = (uint)data["seasonNo"];
            pClubSeasonStats.MatchType = (uint)data["matchType"];
            pClubSeasonStats.SquadPower = data.ContainsKey("squadPower") ? (uint)data["squadPower"] : 0;
            pClubSeasonStats.Rank = (uint)data["userRank"];

            uint trophy = 0;
            if(data.ContainsKey("trophy") && data["trophy"].Type != JTokenType.Null)
            {
                if(!uint.TryParse((string)data["trophy"],out trophy))
                {
                    trophy = 0;
                }
            }
            pClubSeasonStats.Trophy = trophy;
            pClubSeasonStats.TrophyHigh = (uint)data["trophyHigh"];
            pClubSeasonStats.Standing = (uint)data["standing"];
            pClubSeasonStats.StandingHigh = (uint)data["standingHigh"];
            pClubSeasonStats.Game = (uint)data["game"];
            pClubSeasonStats.Win = (uint)data["win"];
            pClubSeasonStats.Lose = (uint)data["lose"];
            pClubSeasonStats.Goals = (uint)data["goals"];
            pClubSeasonStats.Assists = (uint)data["assists"];
            pClubSeasonStats.CleanSheet = (uint)data["cleanSheet"];
            pClubSeasonStats.WinStreak = (uint)data["winStreak"];
            pClubSeasonStats.BigWinHome = (uint)data["bigWinHome"];
            pClubSeasonStats.BigWinAway = (uint)data["bigWinAway"];
            pClubSeasonStats.BigWinMatch = (string)data["bigWinMatch"];
            pClubSeasonStats.MostGoals = (uint)data["mostGoals"];
            pClubSeasonStats.MostGoalsPlayerName = (data.ContainsKey("mostGoalsPlayerName") && data["mostGoalsPlayerName"].Type != JTokenType.Null) ? (string)data["mostGoalsPlayerName"] : "";
            pClubSeasonStats.MostAssists = (uint)data["mostAssists"];
            pClubSeasonStats.MostAssistsPlayerName =(data.ContainsKey("mostAssistsPlayerName")&& data["mostAssistsPlayerName"].Type != JTokenType.Null)? (string)data["mostAssistsPlayerName"] : "";
            pClubSeasonStats.TransferSpending = (data.ContainsKey("transferSpending") && data["transferSpending"].Type != JTokenType.Null)? (long)data["transferSpending"] : 0;
            pClubSeasonStats.TransferIncome = (data.ContainsKey("transferIncome") && data["transferIncome"].Type != JTokenType.Null)? (long)data["transferIncome"] : 0;
            pClubSeasonStats.NetSpend = (data.ContainsKey("netSpend") && data["netSpend"].Type != JTokenType.Null)? (long)data["netSpend"] : 0;
            
            return pClubSeasonStats;
        }
        protected ClubSeasonStats(){}
        public void Dispose()
        {
            BigWinMatch = null;
        }
    }
    
    public class ClubData : IBase
    {
        public uint SeasonNo { get; set; }
        public byte UserRank { get; set; }
        public uint Trophy { get; set; }
        public uint Standing { get; set; }
        ClubInfoT m_pClubInfo = null;
        ClubHistory m_pHistory = null;
        List<ClubSeasonStats> m_pLatestSeasonStats = null;
        List<ulong> m_pLineup = null;
        List<byte> m_pFormation = null;
        public uint PlayerCount { get; set; }
        public uint SquadPower { get; set; }
        public ulong PlayerTotalValue { get; set; }
        public float AvgAge { get; set; }
        
        List<PlayerT> m_pPlayers = null;
        List<ulong> m_pCorePlayers = null;
        
        public static ClubData CreateOnlyPlayer(JObject data)
        {
            ClubData pClubData = new ClubData();

            int i = 0;
            E_ABILINDEX n =E_ABILINDEX.AB_ATT_PASS;
            JArray pArray = (JArray)data["players"];
            JObject item = null;
            PlayerT pPlayer = null;
            pClubData.m_pPlayers = new List<PlayerT>();

            for(i= 0; i < pArray.Count; ++i)
            {
                item = (JObject)pArray[i];
                pPlayer = new PlayerT();
                pPlayer.Id = (ulong)item["id"];
                pPlayer.PositionFamiliars = new List<byte>();
                pPlayer.Ability = new List<PlayerAbilityT>();
                
                for(n =E_ABILINDEX.AB_ATT_PASS; n < E_ABILINDEX.AB_END; ++n)
                {
                    pPlayer.Ability.Add(new PlayerAbilityT());
                }
                pClubData.m_pPlayers.Add(pPlayer);
                GameContext.getCtx().UpdatePlayer(pPlayer,item);
            }

            return pClubData;
        }

        public static ClubData CreateHistory(JObject data)
        {
            ClubData pClubData = new ClubData();
            pClubData.m_pHistory = ClubHistory.Create(data);
            
            return pClubData;
        }        
        
        public static ClubData Create(JObject data)
        {
            ClubData pClubData = new ClubData();

            pClubData.SeasonNo = (uint)data["seasonNo"];
            pClubData.UserRank = (byte)data["userRank"];
            uint amount =0;
            if(data.ContainsKey("trophy") && data["trophy"].Type != JTokenType.Null )
            {
                if(!uint.TryParse((string)data["trophy"],out amount))
                {
                    amount =0;
                }
            }
            
            pClubData.Trophy = (uint)amount;
            pClubData.Standing = (uint)data["standing"];
            
            pClubData.SquadPower = data.ContainsKey("squadPower") ? (uint)data["squadPower"] : 0;
            pClubData.PlayerCount = (uint)data["playerCount"];
            pClubData.PlayerTotalValue = (ulong)data["totalValue"];
            pClubData.AvgAge = (float)data["avgAge"] / 10;

            pClubData.m_pHistory = ClubHistory.Create((JObject)data["history"]);
            
            pClubData.m_pClubInfo = new ClubInfoT();
            JObject club = (JObject)data["club"];
            pClubData.m_pClubInfo.Id = (ulong)club["id"];
            pClubData.m_pClubInfo.ClubName = club["name"].ToString();
            pClubData.m_pClubInfo.Nation = club["nation"].ToString();
            pClubData.m_pClubInfo.Age = (byte)club["age"];
            pClubData.m_pClubInfo.Gender = (byte)club["gender"];
            
            pClubData.m_pClubInfo.TCreate = ALF.NETWORK.NetworkManager.ConvertLocalGameTimeTick(club["tCreate"].ToString());
            
            if(club.ContainsKey("emblem") && club["emblem"].Type != JTokenType.Null)
            {
                pClubData.m_pClubInfo.Emblem = SingleFunc.GetMakeEmblemData((string)club["emblem"]).ToList();
            }
            else
            {
                pClubData.m_pClubInfo.Emblem = SingleFunc.CreateRandomEmblem().ToList();
            }
            
            int i = 0;
            E_ABILINDEX n = E_ABILINDEX.AB_ATT_PASS;
            JArray pArray = (JArray)data["players"];
            JObject item = null;
            PlayerT pPlayer = null;
            pClubData.m_pPlayers = new List<PlayerT>();

            for(i= 0; i < pArray.Count; ++i)
            {
                item = (JObject)pArray[i];
                pPlayer = new PlayerT();
                pPlayer.Id = (ulong)item["id"];
                pPlayer.PositionFamiliars = new List<byte>();
                pPlayer.Ability = new List<PlayerAbilityT>();
                
                for(n = E_ABILINDEX.AB_ATT_PASS; n < E_ABILINDEX.AB_END; ++n)
                {
                    pPlayer.Ability.Add(new PlayerAbilityT());
                }
                pClubData.m_pPlayers.Add(pPlayer);
                GameContext.getCtx().UpdatePlayer(pPlayer,item);
            }

            pClubData.m_pLatestSeasonStats = new List<ClubSeasonStats>();
            if(data.ContainsKey("latestSeasonStats"))
            {
                pArray = (JArray)data["latestSeasonStats"];
                for(i= 0; i < pArray.Count; ++i)
                {
                    item = (JObject)pArray[i];
                    pClubData.m_pLatestSeasonStats.Add(ClubSeasonStats.Create(item));
                }
            }
            
            pClubData.m_pCorePlayers = new List<ulong>();
            if(data.ContainsKey("corePlayers"))
            {
                pArray = (JArray)data["corePlayers"];
                for(i= 0; i < pArray.Count; ++i)
                {
                    pClubData.m_pCorePlayers.Add((ulong)pArray[i]);
                }
            }

            pClubData.m_pFormation = Newtonsoft.Json.JsonConvert.DeserializeObject<List<byte>>(data["formation"].ToString());
            
            pClubData.m_pLineup = new List<ulong>();

            pArray = (JArray)data["lineup"];
            for(i= 0; i < pArray.Count; ++i)
            {
                pClubData.m_pLineup.Add((ulong)pArray[i]);
            }

            return pClubData;
        }
        protected ClubData(){}

        public void UpdateHistory(JObject data)
        {
            m_pHistory = ClubHistory.Create(data);
        }

        public uint GetThisSeasonLose()
        {
            for(int i =0; i < m_pLatestSeasonStats.Count; ++i)
            {
                if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                {
                    return m_pLatestSeasonStats[i].Lose;
                }
            }
            return 0;
        }
        public uint GetThisSeasonWin()
        {
            for(int i =0; i < m_pLatestSeasonStats.Count; ++i)
            {
                if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                {
                    return m_pLatestSeasonStats[i].Win;
                }
            }
            return 0;
        }
        public uint GetThisSeasonDraw()
        {
            for(int i =0; i < m_pLatestSeasonStats.Count; ++i)
            {
                if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                {
                    return m_pLatestSeasonStats[i].Game - (m_pLatestSeasonStats[i].Win + m_pLatestSeasonStats[i].Lose);
                }
            }
            return 0;
        }
        public uint GetLineupOverall()
        {
            uint total =0;

            PlayerT pPlayer = null;
            for(int i =0; i < m_pLineup.Count; ++i)
            {
                pPlayer = GetPlayerByID(m_pLineup[i]);
                for( int t =0; t < pPlayer.Ability.Count; ++t)
                {
                    if(pPlayer.Ability[t].Current > 0)
                    {
                        total += (uint)pPlayer.Ability[t].Current;
                    }
                }
            }

            
            return total;
        }
        public uint GetTotalWin()
        {
            return m_pHistory.Win;
        }

        public uint GetTotalLose()
        {
            return m_pHistory.Lose;
        }

        public uint GetHistoryWinStreak()
        {
            return m_pHistory.WinStreak;
        }

        public uint GetSeasonEntered()
        {
            return m_pHistory.SeasonEntered;
        }

        public uint GetSeasonStandingHigh()
        {
            uint total = 0;
            for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
            {
                if(m_pLatestSeasonStats[i].StandingHigh > total)
                {
                    total = m_pLatestSeasonStats[i].StandingHigh;
                }
            }

            return total;
        }

        public uint GetSeasonTrophyHigh()
        {
            uint total = 0;
            for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
            {
                if(m_pLatestSeasonStats[i].TrophyHigh > total)
                {
                    total = m_pLatestSeasonStats[i].TrophyHigh;
                }
            }

            return total;
        }

        public ClubSeasonStats GetLatestSeasonStats(int index)
        {
            if(index > -1 && m_pLatestSeasonStats.Count >index)
            {
                return m_pLatestSeasonStats[index];
            }

            return null;
        }

        public uint GetGamesStats(bool bSeason)
        {
            uint total = 0;
            if(bSeason)
            {
                for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                    {
                        total = m_pLatestSeasonStats[i].Game;
                        break;
                    }
                }
            }
            else
            {
                total = m_pHistory.Game;
            }

            return total;
        }

        public uint GetWinStreakStats(bool bSeason)
        {
            uint total = 0;
            if(bSeason)
            {
                for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                    {
                        total = m_pLatestSeasonStats[i].WinStreak;
                        break;
                    }
                }
            }
            else
            {
                total = m_pHistory.WinStreak;
            }

            return total;
        }

        public string GetBigWinStats(bool bSeason)
        {
            if(bSeason)
            {
                for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                    {
                        return m_pLatestSeasonStats[i].BigWinMatch;
                    }
                }
            }
            else
            {
                return m_pHistory.BigWinMatch;
            }
            
            return "";
        }

        public uint GetGoalsStats(bool bSeason)
        {
            uint total = 0;
            if(bSeason)
            {
                for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                    {
                        total = m_pLatestSeasonStats[i].Goals;
                        break;
                    }
                }
            }
            else
            {
                total = m_pHistory.Goals;
            }
            
            return total;
        }

        public string GetMostGoalsStats(bool bSeason)
        {
            if(bSeason)
            {
                for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                    {
                        return m_pLatestSeasonStats[i].MostGoalsPlayerName;
                    }
                }
            }
            else
            {
                return m_pHistory.MostGoalsPlayerName;
            }
            return "-";
        }

        public string GetMostAssistsStats(bool bSeason)
        {
            if(bSeason)
            {
                for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                    {
                        return m_pLatestSeasonStats[i].MostAssistsPlayerName;
                    }
                }
            }
            else
            {
                return m_pHistory.MostAssistsPlayerName;
            }
            
            return "-";
        }

        public long GetTransferSpendingStats(bool bSeason)
        {
            long total = 0;
            if(bSeason)
            {
                for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                    {
                        total = m_pLatestSeasonStats[i].TransferSpending;
                        break;
                    }
                }
            }
            else
            {
                total = m_pHistory.TransferSpending;
            }
            
            return total;
        }

        public long GetTransferIncomeStats(bool bSeason)
        {
            long total = 0;
            if(bSeason)
            {
                for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                    {
                        total = m_pLatestSeasonStats[i].TransferIncome;
                        break;
                    }
                }
            }
            else
            {
                total = m_pHistory.TransferIncome;
            }
            
            return total;
        }

        public long GetNetSpendStats(bool bSeason)
        {
            long total = 0;
            if(bSeason)
            {
                for(int i = 0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    if(m_pLatestSeasonStats[i].SeasonNo == SeasonNo)
                    {
                        total = m_pLatestSeasonStats[i].NetSpend;
                        break;
                    }
                }
            }
            else
            {
                total = m_pHistory.NetSpend;
            }
            
            return total;
        }

        public uint GetTotalTrophyHigh()
        {
            return m_pHistory.TrophyHigh;
        }

        public uint GetTotalStandingHigh()
        {
            return m_pHistory.StandingHigh;
        }

        public long GetClubCreateTime()
        {
            return m_pClubInfo.TCreate;
        }

        public uint GetStanding()
        {
            return Standing;
        }

        public uint GetSquadPower()
        {
            return SquadPower;
        }
        public float GetAvgAge()
        {
            return AvgAge;
        }

        public ulong GetPlayerTotalValue()
        {
            return PlayerTotalValue;
        }
        
        public uint GetPlayerCount()
        {
            return PlayerCount;
        }

    
        public byte GetRank()
        {
            return UserRank;
        }

        public uint GetTrophy()
        {
            return Trophy;
        }
        public List<ulong> GetCorePlayers()
        {
            return m_pCorePlayers;
        }    
        public List<byte> GetFormationList()
        {
            return m_pFormation;
        }

        public List<ulong> GetLineupList()
        {
            return m_pLineup;
        }

        public List<ulong> GetPlayerByIDList()
        {
            List<ulong> list = new List<ulong>();
            for(int i =0; i < m_pPlayers.Count; ++i)
            {
                list.Add(m_pPlayers[i].Id);
            } 
            return list;
        }
        public PlayerT GetPlayerByID(ulong id)
        {
            for(int i =0; i < m_pPlayers.Count; ++i)
            {
                if(m_pPlayers[i].Id == id)
                {
                    return m_pPlayers[i];
                }
            } 
            return null;
        }

        public List<PlayerT> GetTotalPlayer()
        {
            return m_pPlayers;
        }

        public byte[] GetEmblemData()
        {
            if(m_pClubInfo == null) return null;
            return m_pClubInfo.Emblem.ToArray();
        }

        public void SetNation(string nation)
        {
            if(m_pClubInfo == null) return;
            m_pClubInfo.Nation = nation;
        }

        public void SetClubName(string name)
        {
            if(m_pClubInfo == null) return;
            m_pClubInfo.ClubName = name;
        }

        public string GetNation()
        {
            if(m_pClubInfo == null) return null;
            return m_pClubInfo.Nation;
        }

        public string GetClubName()
        {
            if(m_pClubInfo == null) return null;
            return m_pClubInfo.ClubName;
        }

        public ulong GetID()
        {
            if(m_pClubInfo == null) return 0;
            return m_pClubInfo.Id;
        }

        public void Dispose()
        {
            m_pClubInfo = null;
            m_pHistory?.Dispose();
            m_pHistory = null;
            if(m_pLatestSeasonStats != null)
            {
                for(int i =0; i < m_pLatestSeasonStats.Count; ++i)
                {
                    m_pLatestSeasonStats[i].Dispose();
                    m_pLatestSeasonStats[i] = null;
                }
                m_pLatestSeasonStats.Clear();
                m_pLatestSeasonStats = null;
            }
            
            m_pLineup?.Clear();
            m_pLineup = null;
            m_pFormation?.Clear();
            m_pFormation = null;
            m_pPlayers?.Clear();
            m_pPlayers = null;
            m_pCorePlayers?.Clear();
            m_pCorePlayers = null;
        }
    }
    
    public class MatchData : IBase
    {
        ClubInfoT m_pClubInfo = null;
        public ulong Match { get; set; }
        public int MatchType { get; set; }
        List<PlayerT> m_pPlayers = null;
        TacticsT m_pTactics = null;
        LineupPlayerT m_pLineup = null;
        byte m_eRank = 0;
        uint m_iTrophy = 0;
        int m_iStanding = 0;

        public static MatchData Create(JObject data)
        {
            MatchData pMatchData = new MatchData();
            pMatchData.m_pClubInfo = new ClubInfoT();
            pMatchData.MatchType = (int)data["matchType"];
            pMatchData.m_eRank = (data.ContainsKey("userRank") && data["userRank"].Type != JTokenType.Null) ? (byte)data["userRank"] : (byte)0;
            uint amount = 0;
            if((data.ContainsKey("trophy") && data["trophy"].Type != JTokenType.Null))
            {
                if(!uint.TryParse((string)data["trophy"], out amount))
                {
                    amount = 0;
                }
            }
            JObject club = (JObject)data["club"];
            pMatchData.m_iTrophy = amount;
            pMatchData.m_pClubInfo.ClubName = club["name"].ToString();
            pMatchData.m_pClubInfo.Nation = ALFUtils.ConvertThreeLetterNameToTwoLetterName(club["nation"].ToString());
            pMatchData.m_pClubInfo.Age = (byte)club["age"];
            pMatchData.m_pClubInfo.Gender = (byte)club["gender"];
            pMatchData.m_pClubInfo.Id = (ulong)club["id"];

            if(club.ContainsKey("emblem") && club["emblem"].Type != JTokenType.Null)
            {
                pMatchData.m_pClubInfo.Emblem = SingleFunc.GetMakeEmblemData((string)(club["emblem"])).ToList();
            }
            else
            {
                pMatchData.m_pClubInfo.Emblem = SingleFunc.CreateRandomEmblem().ToList();
            }
            
            int i = 0;
            E_ABILINDEX n = E_ABILINDEX.AB_ATT_PASS;
            JArray pArray = (JArray)data["players"];
            JObject item = null;
            PlayerT pPlayer = null;
            pMatchData.m_pPlayers = new List<PlayerT>();
            GameContext pGameContext = GameContext.getCtx();
            for(i= 0; i < pArray.Count; ++i)
            {
                item = (JObject)pArray[i];
                pPlayer = new PlayerT();
                pPlayer.Id = (ulong)item["id"];
                pPlayer.PositionFamiliars = new List<byte>();
                pPlayer.Ability = new List<PlayerAbilityT>();
                
                for(n = E_ABILINDEX.AB_ATT_PASS; n < E_ABILINDEX.AB_END; ++n)
                {
                    pPlayer.Ability.Add(new PlayerAbilityT());
                }
                pMatchData.m_pPlayers.Add(pPlayer);
                pGameContext.UpdatePlayer(pPlayer,item);
            }

            pMatchData.Match = (ulong)data["match"];
            pArray = (JArray)data["tactics"];
            item = (JObject)pArray[0];

            pMatchData.m_pTactics = Newtonsoft.Json.JsonConvert.DeserializeObject<TacticsT>(item["data"].ToString());
            if(item.ContainsKey("formation") && !string.IsNullOrEmpty(item["formation"].ToString()))
            {
                pMatchData.m_pTactics.Formation = Newtonsoft.Json.JsonConvert.DeserializeObject<List<byte>>(item["formation"].ToString());
            }            
            
            pArray = (JArray)data["lineup"];
            pMatchData.m_pLineup = new LineupPlayerT();
            pMatchData.m_pLineup.Type = 1;
            pMatchData.m_pLineup.Data = new List<ulong>();

            for(i =0; i < pArray.Count; ++i)
            {
                pMatchData.m_pLineup.Data.Add((ulong)pArray[i]);
            }

            return pMatchData;
        }
        protected MatchData(){}


        public byte GetRank()
        {
            return m_eRank;
        }

        public uint GetTrophy()
        {
            return m_iTrophy;
        }

        public int GetStanding()
        {
            return m_iStanding;
        }

        public void SetStanding(int iStanding)
        {
            m_iStanding = iStanding;
        }

        public List<byte> GetFormationList()
        {
            return m_pTactics.Formation;
        }

        public List<PlayerT> GetTotalPlayerList()
        {
            return m_pPlayers;
        }

        public PlayerT GetPlayerByID(ulong id)
        {
            for(int i =0; i < m_pPlayers.Count; ++i)
            {
                if(m_pPlayers[i].Id == id)
                {
                    return m_pPlayers[i];
                }
            } 
            return null;
        }


        public uint GetTotalPlayerAbility()
        {
            uint total =0;
            PlayerT pPlayer = null;
            
            for(int n =0; n < 11; ++n)
            {
                pPlayer = m_pPlayers[n];
                total += pPlayer.AbilityWeightSum;
            }
            
            return total;
        }
        public byte[] GetEmblemData()
        {
            return m_pClubInfo.Emblem.ToArray();
        }

        public string GetNation()
        {
            return m_pClubInfo.Nation;
        }

        public string GetClubName()
        {
            return m_pClubInfo.ClubName;
        }

        public ulong GetID()
        {
            return m_pClubInfo.Id;
        }

        public TeamDataT MakeTeamData(float fNerf)
        {
            TeamDataT pTeamData = new TeamDataT();
            pTeamData.PlayerData = new List<PlayerDataT>();
            
            pTeamData.Tactics = m_pTactics;
            pTeamData.TeamName = m_pClubInfo.ClubName;
            pTeamData.TeamId = m_pClubInfo.Id;
            pTeamData.TeamColor = new List<ushort>(){0,0};
            pTeamData.LineUp = new List<int>();
            pTeamData.TeamWorkTotal = 100;
            PlayerDataT pPlayerData = null;
            int n = 0;
            int i = 0;
            int count = (int)E_TALENT.TALENT_END;
            GameContext pGameContext = GameContext.getCtx();
            PlayerT pPlayer = null;
            int temp =0;
            float vNerf = 0;
            for(i = 0; i < m_pLineup.Data.Count; ++i)
            {
                pPlayerData = new PlayerDataT();
                pPlayerData.Ability = new List<byte>();
                pPlayer = GetPlayerByID(m_pLineup.Data[i]);
                for(n = 0; n < pPlayer.Ability.Count; ++n )
                {
                    vNerf = (pPlayer.Ability[n].Origin + pPlayer.Ability[n].Changed) * fNerf;
                    temp = Mathf.RoundToInt(vNerf);
                    pPlayerData.Ability.Add(temp > 0 ? (byte)temp : (byte) 0);
                }
                
                pPlayerData.PosAbil = new List<float>();
                for(n = 0; n < pPlayer.PositionFamiliars.Count; ++n )
                {
                    pPlayerData.PosAbil.Add(pPlayer.PositionFamiliars[n]);
                }
                pPlayerData.Talent = new List<byte>();
                for(n = 0; n < count; ++n )
                {
                    pPlayerData.Talent.Add(50); // 매치엔진에서 기본값으로 설정
                }
                
                pPlayerData.PlayerName = $"{pPlayer.Forename[0]}. {pPlayer.Surname}";
                pPlayerData.ExperienceLevel = 5;  // 매치엔진에서 기본값으로 설정
                pPlayerData.ExperienceExp = 50;  // 매치엔진에서 기본값으로 설정
                pPlayerData.Hp = pPlayer.Hp;
                pPlayerData.Age = pGameContext.GetPlayerAge(pPlayer);
                pPlayerData.IsValidSlot = true;
                pPlayerData.PlayerId = pPlayer.Id;
                pPlayerData.Injury = new InjuryResultT();
                pPlayerData.PlayerSkill = new List<PlayerSkillT>(){new PlayerSkillT(),new PlayerSkillT()};
                pTeamData.PlayerData.Add(pPlayerData);
                pTeamData.LineUp.Add(i);
            }

            return pTeamData;
        }

        public void Dispose()
        {
            m_pClubInfo = null;
            m_pPlayers.Clear();
            m_pTactics = null;
            m_pLineup = null;
        }
    }
  }