using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using ALF;
using ALF.NETWORK;
using Newtonsoft.Json.Linq;
using FlatBuffers;
using DATA;
using USERDATA;
using CONSTVALUE;
using ADREWARD;
using BUSINESS;
using PLAYERNATIONALITY;
using PLAYERPOTENTIALWEIGHTSUM;
using TEXT;
using USERRANK;
using MATCHTEAMDATA;
using TRAININGCOST;
using POSITIONFAMILAR;
using PLAYERABILITYWEIGHTCONVERSION;
using MATCHCOMMENTARY;
using STATISTICSDATA;
using LADDERSTANDINGREWARD;
using LADDERSEASONNO;
using REWARDSET;
using MILEAGEMISSION;
using ATTENDREWARD;
using LADDERREWARD;
using PASS;
using PASSMISSION;
using CLUBNATIONALITY;
using SHOP;
using FWORD;
using PUSH;
using EVENTCONDITION;
using SHOPPRODUCT;
using GAMETIP;
using CLUBLICENSEMISSION;
using ACHIEVEMENTMISSION;
using QUESTMISSION;
using System.Globalization;
using ALF.SOUND;
using ITEM;
using LEAGUESTANDINGREWARDDATA;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using ALF.STATE;
using ALF.MACHINE;
using STATEDATA;
using TIMESALE;

// using Firebase.Analytics;

public class GameContext : IBase
{
    public static string PatchVersion = ALFUtils.GetVersionCode();
    public readonly static int CHALLENGE_ID = 11;
    public readonly static int LADDER_ID = 1;
    public readonly static int LEAGUE_ID = 10;
    public readonly static int LEAGUE1_ID = 12;
    public readonly static int LEAGUE2_ID = 13;
    public readonly static int LEAGUE3_ID = 14;
    public readonly static int LEAGUE4_ID = 15;
    public readonly static uint VIRTUAL_ID = 10000;
    public readonly static uint FAST_REWARD_ID = 70;
    public readonly static uint ACHIEVEMENT_ID = 1;
    public readonly static uint QUSET_MISSION_ID = 1;
    public readonly static uint REWARD_SET = 21;
    public readonly static uint REWARD_NORMAL = 1;
    public readonly static uint NOMINATION_ID = 1101;
    public readonly static uint SCOUT_TICKET_ID_START = 1200;
    public readonly static uint SCOUT_TICKET_ID_END = 1223;
    public readonly static uint FREE_CASH_ID = 1;
    public readonly static uint CASH_ID = 2;
    public readonly static uint MATCH_SKIP_ID = 1100;
    public readonly static uint PRODUCT_CASH_ID = 9;
    public readonly static uint GAME_MONEY_ID = 10; 
    public readonly static uint CHALLENGE_TICKET_ID = 80;
    public readonly static uint PLAYET_N_ABILITY_TIER_ID = 2202;
    public readonly static uint RANK_ITEM_ID = 31; 
    public readonly static uint TROPHY_ITEM_ID = 30; 
    public readonly static uint CLUB_LICENSE_START_ID = 10010;
    public readonly static uint POSITON_POINT_ID = 40; 
    public readonly static uint STAMINA_DRINK = 1102; 
    public readonly static uint AD_SKIP_ID = 1103; 
    public readonly static uint QUEST_MISSION_MILEAGE_ID = 80; 
    public readonly static Color GRAY = new Color(0.4666667f,0.4666667f,0.4666667f);
    public readonly static Color GRAY_W = new Color(0.6431373f,0.6509804f,0.654902f);
    public readonly static Color GREEN = new Color(0.2337576f,0.4811321f,0.3072352f);
    public readonly static Color BULE = new Color(0.2039216f,0.5294118f,0.6f);
    public readonly static Color YELLOW = new Color(0.6470588f,0.482353f,0.2039216f);
    public readonly static Color RED = new Color(0.5943396f,0.1597988f,0.1597988f);
    public readonly static Color HP_F = new Color(0.3843137f,0.854902f,0.5137255f);
    public readonly static Color HP_H = new Color(0.9811321f,0.7515077f,0.0509078f);
    public readonly static Color HP_LH = new Color(0.9803922f,0.4075524f,0.05098038f);
    public readonly static Color HP_L = new Color(1,0.4431373f,0.3960784f);
    readonly byte MILEAGE_MATCH = 50;
    public readonly static byte CONST_NUMSTARTING = 11;
    public readonly static byte CONST_NUMBENCH = 7;
    public readonly static byte CONST_NUMPLAYER = 18;

    readonly int[] const_displayLocationConvertList = new int[]{0, 1,  2,  2,  2,  3, 4, 5, 5, 5, 6, 7,  8,  8,  8,  9, 10, 11, 11, 11, 12, 10,  14,  14,  14,  12};
    readonly string[] const_displayLocationList = new string[]{"GK", "LB", "CB", "RB", "LWB", "CDM", "RWB", "LM", "CM", "RM","LWF", "CAM","RWF","LWF","CF","RWF"};
    readonly string[] const_displayBackCardFormationList = new string[]{"GK", "DF", "DF", "DF", "DF", "MF", "DF", "MF", "MF", "MF", "FW", "MF", "FW", "FW",  "FW", "FW"};
    readonly string[] const_adjsutEventPass = new string[]{"ll3z36","4zt87z","hutmpv","pginoi","5bx621","w4nyyl","hyar14","8s193a","sthzow","4h7hct","4b3uuq","aho3v9","w9pgad","tjex8f","1oce6r","xwhrtc","1ivbr2","je4h9r","uagyq7","p6i819"};


    private const string BIN = "Bin";
    static GameContext gCtx = null;
    UserDataT m_pUserData = null;
    ClubInfoT m_pClubInfo = null;
    GameInfoT m_pGameInfo = null;
    MatchData m_pMatchData = null;
    LineupPlayerT m_pCacheLineupPlayerData = null;
    Dictionary<int,LineupPlayerT> m_pLineupPlayerList = new Dictionary<int, LineupPlayerT>();
    Dictionary<int,TacticsT> m_pTacticsList = new Dictionary<int,TacticsT>();
    Dictionary<uint,ItemInfoT> m_pInventoryInfoList = new Dictionary<uint, ItemInfoT>();
    MailBoxInfoT m_pMailBoxInfo = new MailBoxInfoT();
    Dictionary<uint,MileageT> m_pMileagesInfo = new Dictionary<uint,MileageT>();
    Dictionary<uint,ProductFlatRateT> m_pProductFlatRateInfo = new Dictionary<uint,ProductFlatRateT>();
    Dictionary<uint,ProductMileageT> m_pProductMileagesInfo = new Dictionary<uint,ProductMileageT>();
    Dictionary<uint,TimeSaleT> m_pProductTimeSaleInfo = new Dictionary<uint,TimeSaleT>();
    
    PassT m_pPassInfo = new PassT();
    AdRewardInfoT m_pAdRewardInfo = null;
    List<AttendInfoT> m_pAttendInfo = new List<AttendInfoT>();
    STATISTICSDATA.MatchLogT m_pMatchLogRecords = null;
    Queue<JObject> m_pResponData = new Queue<JObject>();
    ClubData m_pAwayClubData = null;
    ClubData m_pClubData = null;
    CachePlayerDataT m_pCachePlayerData = null;
    Dictionary<E_DATA_TYPE,IFlatbufferObject> dataList = new Dictionary<E_DATA_TYPE,IFlatbufferObject>();
    Dictionary<E_CONST_TYPE,int> constValueList = new Dictionary<E_CONST_TYPE,int>();
    DeviceOrientation m_eDeviceOrientation = DeviceOrientation.Unknown;
    Dictionary<uint,ulong> m_pLastMatchRewardList = new Dictionary<uint,ulong>();
    Dictionary<uint,AchievementT> m_pAchievementList = new Dictionary<uint,AchievementT>();
    Dictionary<uint,List<ClubLicensesT>> m_pClubLicensesList = new Dictionary<uint,List<ClubLicensesT>>();
    List<QuestT> m_pQuestList = new List<QuestT>();
    
    Dictionary<ITimer,List<float>> m_pExpireTimeList = new Dictionary<ITimer,List<float>>();
    
    List<E_CONST_TYPE> m_pLicenseContentsUnlockIDList = new List<E_CONST_TYPE>();

    AuctionInfoT m_pAuctionInfoData = null;
    List<AuctionBiddingInfoT> m_pAuctionBiddingInfoData = null;
    List<AuctionSellInfoT> m_pAuctionSellInfoData = null;

    List<string> m_pClubNationCodeList = new List<string>();
    List<string> m_pPlayerNationCodeList = new List<string>();

    RestorePurchaseT m_pCurrentPurchaseInfo = null;
    RestoreInfoT m_pRestoreInfoList = new RestoreInfoT();
    bool m_bTransactionFinish = false;

    System.Action<E_AD_STATUS> m_pRewardADCallback = null;

#if USE_HIVE

    public string HiveRewardVideoUnitId
    {
        get
        {
            if(hive.ZoneType.REAL == hive.Configuration.getZone())
            {
#if UNITY_ANDROID
#if ONESTORE
                return "1c293526-8bb8-42e4-85bb-dad34de76183";
#else
                return "50b4011f-a5a4-448e-80d4-73e8ef7fac6a";
#endif
#elif UNITY_IOS
                return "1ac4ec3d-0c0a-43b0-9f02-e80fe6c8456e";
#else
                return "NotSuport";
#endif
            }
			else
            {
#if UNITY_ANDROID
                return "7d9a2c9e-5755-4022-85f1-6d4fc79e4418";
#elif UNITY_IOS
                return "29e1ef67-98d2-47b3-9fa2-9192327dd75d";
#else
                return "NotSuport";
#endif
            }
		}
    }

    hive.HIVEAdKit.RewardVideo m_pRewardVideoAd = null;
    public bool IsInitializeHive = false;
    
    public int ProviderTypeList = 0;
    Dictionary<string,hive.IAPV4.IAPV4Product> m_pProductInfoList = new Dictionary<string, hive.IAPV4.IAPV4Product>();

    public void ShowHiveCustomView(string pContentsKey, bool bView, hive.Promotion.onPromotionView listener)
    {
        if (Application.isEditor)
        {
            IBaseScene pScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
            if(pScene != null)
            {
                Message pUI = pScene.GetInstance<Message>();
                pUI.SetMessage("미지원!!",GetLocalizingText("MSG_BTN_OKAY"),null);
                SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
            }
        }
        else
        {
            NetworkManager.ShowWaitMark(true);
            hive.Promotion.showCustomContents(bView ? hive.PromotionCustomType.VIEW : hive.PromotionCustomType.BOARD, pContentsKey, listener == null ? (hive.ResultAPI result, hive.PromotionEventType promotionEventType)=>{
                
                NetworkManager.ShowWaitMark(false);
                // if(result.isSuccess())
                // {
                // switch(promotionEventType){
                //     case OPEN:   // 프로모션 뷰 열림
                //         break;
                //     case CLOSE:   // 프로모션 뷰 닫힘
                //         break;
                // }
            } : listener);
        }

    }

    public void ShowHivePromotionByPromotionType(hive.PromotionType eType,bool isForced, hive.Promotion.onPromotionView listener)
    {
        hive.Promotion.showPromotion(eType, isForced, listener);
    }

    void OnAdLoadedCB()
    {
        Debug.Log("---------------------OnAdLoadedCB");
        // _LoadedFlag = true;
    }

    void OnAdOpeningCB()
    {
        SoundManager.Instance.StopBGM(false);
        Debug.Log("---------------------OnAdOpeningCB");
        if(m_pRewardADCallback != null)
        {
            m_pRewardADCallback(E_AD_STATUS.RewardShow);
        }
    }

    void OnAdClosedCB()
    {
        SoundManager.Instance.ChangeBGM("bgm_main",true,0.4f);
        if(m_pRewardADCallback != null)
        {
            m_pRewardADCallback(E_AD_STATUS.RewardClose);
            m_pRewardADCallback = null;
        }
        LoadRewardVideo();
    }

    void OnAdFailedCB()
    {
        Debug.Log("---------------------OnAdFailedCB");
        if(m_pRewardADCallback != null)
        {
            m_pRewardADCallback(E_AD_STATUS.RewardFail);
            m_pRewardADCallback = null;
        }
    }
    void OnAdSkip()
    {
        Debug.Log("---------------------OnAdSkip");
    }

    void OnAdRewardCB()
    {
        Debug.Log("---------------------OnAdRewardCB");
        if(m_pRewardADCallback != null)
        {
            m_pRewardADCallback(E_AD_STATUS.RewardComplete);
            m_pRewardADCallback = null;
        }
    }

    void RewardVideoInitialize()
    {
        hive.EventHandlers pEventHandlers = new hive.EventHandlers.Builder()
                                     .OnAdLoaded(OnAdLoadedCB)
                                     .OnAdFailed(OnAdFailedCB)
                                     .OnAdOpening(OnAdOpeningCB)
                                     .OnAdReward(OnAdRewardCB)
                                     .OnAdClosed(OnAdClosedCB)
                                    //  OnAdClick
                                     .OnAdSkip(OnAdSkip)
                                     .Build();
        
        m_pRewardVideoAd = hive.HIVEAdKit.RewardVideo.Initialize(HiveRewardVideoUnitId, pEventHandlers);
    }
    public void InitializeWithADOPConsent()
    {
        hive.HIVEAdKit.InitializeWithShowADOPConsent(false, null);
        
        GameObject bidmadManager = new GameObject("BidmadManager");
        bidmadManager.AddComponent<BidmadManager>();
        bidmadManager.transform.SetParent(ALF.LAYOUT.LayoutManager.Instance.GetMainCanvas());
    }

    bool executeRewardVideoLoadCallback(IState state,float dt,bool bEnd)
    {
        if(m_pRewardVideoAd == null)
        {
            return true;
        }

        return IsRewardVideoLoaded();
    }

    public void AddPurchaseReceipt(hive.IAPV4.IAPV4Receipt pReceipt)
    {
        if(pReceipt != null)
        {
            for(int i =0; i < m_pRestoreInfoList.List.Count; ++i)
            {
                if(m_pRestoreInfoList.List[i].Receipt == pReceipt.bypassInfo)
                {
                    return;
                }
            } 

            RestorePurchaseT pRestorePurchase = new RestorePurchaseT();
            pRestorePurchase.Receipt = pReceipt.bypassInfo;
            pRestorePurchase.Currency = pReceipt.product.currency;
            pRestorePurchase.CustomerNo = m_pUserData.CustomerNo;
            pRestorePurchase.Sku = pReceipt.product.marketPid;;
            pRestorePurchase.TimeSale = 0;
         
#if UNITY_IOS
            pRestorePurchase.Store = (byte)ALF.LOGIN.LoginManager.E_STORE.apple;
#elif ONESTORE
            pRestorePurchase.Store = (byte)ALF.LOGIN.LoginManager.E_STORE.onestore;
#elif UNITY_ANDROID
            pRestorePurchase.Store = (byte)ALF.LOGIN.LoginManager.E_STORE.google;
#endif
            m_pRestoreInfoList.List.Add(pRestorePurchase);
        }
    }

    void OnIAPV4TransactionFinishCB(hive.ResultAPI pResult, string marketPid)
    {
        IBaseScene pBaseScene =Director.Instance.GetActiveBaseScene<IBaseScene>();
        if(pBaseScene != null && pBaseScene is MainScene)
        {
            ALF.LAYOUT.LayoutManager.Instance.InteractableEnabledAll(null,true);
        }
        
        if (pResult.isSuccess()) 
        {
            m_bTransactionFinish = false;
        }
        else
        {
            IBaseScene pScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
            if(pScene != null)
            {
                Message pMessage = pScene.GetInstance<Message>();
                pMessage.SetMessage(GetLocalizingText("MSG_TXT_TRY_AGAIN"),GetLocalizingText("MSG_BTN_OKAY"));
                SingleFunc.ShowAnimationDailog(pMessage.MainUI,null);
            }
            // if(pIntroScene)
            // Error Handling
        }
    }

    public void HiveExit()
    {
        hive.Promotion.showExit(OnHiveExit);
    }

    void OnHiveExit(hive.ResultAPI pResult, hive.PromotionEventType ePromotionEventType)
    {
        NetworkManager.ShowWaitMark(false);
//        switch (ePromotionEventType)
//        {
// // OPEN					///< \~korean 프로모션 뷰 창이 열렸을 때	\~english  When the Promotion View window opens.
// // , CLOSE					///< \~korean 프로모션 뷰 창이 닫혔을 때	\~english  When the Promotion View window is closed.
// // , START_PLAYBACK		///< \~korean 영상 재생이 시작되었을 때	\~english  When Playback starts.
// // , FINISH_PLAYBACK		///< \~korean 영상 재생이 완료되었을 때	\~english  When Playback is finished.
// // , EXIT					///< \~korean 종료(더 많은 게임 보기) 팝업에서 종료하기를 선택했을 때	\~english  When you choose to quit from the Quit (see more games) popup.
// // , GOBACK				///
//            case hive.PromotionEventType.OPEN:
//            {
//                NetworkManager.ShowWaitMark(false);
//            }
//            break;
//             case hive.PromotionEventType.START_PLAYBACK:
//             case hive.PromotionEventType.FINISH_PLAYBACK:
//             {
//             }
//                 break;
//             default:
//                 // NetworkMgr.Instance.Login();
//                 break;
 //       }
    }

    public void ShowReview(hive.Promotion.onPromotionView listener)
    {
        if(Application.isEditor)
        {
            if(listener != null)
            {
                listener(new hive.ResultAPI(), hive.PromotionEventType.OPEN);
            }
        }
        else
        {
            hive.Promotion.showReview(listener);
            // // Hive 리뷰 팝업 호출
            // hive.Promotion.showReview((hive.ResultAPI result, hive.PromotionEventType promotionEventType)=>{
            //     // Hive 리뷰 팝업 결과 콜백 함수
            
            //     if (result.isSuccess()) {
            //         // API 호출 성공
            //     }
            
            // });
        } 
    }

    public void AddHiveNews()
    {
        JObject pJObject = new JObject();
        pJObject["msgId"] = (uint)E_REQUEST_ID.hive_news;
        m_pResponData.Enqueue(pJObject);
    }

    void OnIAPV4ProductInfo(hive.ResultAPI result, List<hive.IAPV4.IAPV4Product> iapV4ProductList, int balance)
    {
        m_pProductInfoList.Clear();
        for(int i =0; i < iapV4ProductList.Count; ++i)
        {
            if(!m_pProductInfoList.ContainsKey(iapV4ProductList[i].marketPid))
            {
                m_pProductInfoList.Add(iapV4ProductList[i].marketPid, iapV4ProductList[i]);
            }
        }
    }

    public void LoadProductInfo()
    {
        hive.IAPV4.getProductInfo(OnIAPV4ProductInfo);
    }

    public bool IsTransactionFinish()
    {
        return m_bTransactionFinish;
    }

    public void Purchase(string pProductID)
    {
        string msg = null;
        if(m_bTransactionFinish)
        {
            msg = "MSG_TXT_TRY_AGAIN";
        }
        else if(m_pProductInfoList.ContainsKey(pProductID))
        {
            m_bTransactionFinish = true;
            m_pCurrentPurchaseInfo = new RestorePurchaseT();
            m_pCurrentPurchaseInfo.Sku = pProductID;
            ALF.LAYOUT.LayoutManager.Instance.InteractableDisableAll();
            NetworkManager.ShowWaitMark(true);
            hive.IAPV4.purchase(pProductID,m_pUserData.CustomerNo.ToString(),OnIAPV4PurchaseCB);
            return;
        }
        else
        {
            msg = "IAPV4FailMarketConnect";
        }
        
        IBaseScene pScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
        if(pScene != null)
        {
            Message pMessage = pScene.GetInstance<Message>();
            pMessage.SetMessage(GetLocalizingErrorMsg(msg),GetLocalizingText("MSG_BTN_OKAY"));
            SingleFunc.ShowAnimationDailog(pMessage.MainUI,null);
        }
    }

    void OnIAPV4PurchaseCB(hive.ResultAPI pResult, hive.IAPV4.IAPV4Receipt pReceipt)
    {
        if (pResult.isSuccess())
        {
            ALF.LAYOUT.LayoutManager.Instance.InteractableDisableAll(null,true);
            if(m_pCurrentPurchaseInfo == null )
            {
                m_pCurrentPurchaseInfo = new RestorePurchaseT();
            }
            
            m_pCurrentPurchaseInfo.Currency = pReceipt.product.currency;
            m_pCurrentPurchaseInfo.CustomerNo = m_pUserData.CustomerNo;
            m_pCurrentPurchaseInfo.Sku = pReceipt.product.marketPid;
            m_pCurrentPurchaseInfo.Receipt = pReceipt.bypassInfo;
            m_pCurrentPurchaseInfo.TimeSale = 0;

            SHOPPRODUCT.ShopProductList pShopProductList = GetFlatBufferData<SHOPPRODUCT.ShopProductList>(E_DATA_TYPE.ShopProduct);
            SHOPPRODUCT.ShopList? pShopList = null;
            SHOPPRODUCT.ShopProduct? pItem = null;

            for(int i =0; i < pShopProductList.ShopProductLength; ++i)
            {
                pShopList = pShopProductList.ShopProduct(i);
                for(int n =0; n < pShopList.Value.ListLength; ++n)
                {
                    pItem = pShopList.Value.List(n);
                    if(pItem.Value.Sku == m_pCurrentPurchaseInfo.Sku)
                    {
                        SendAdjustEvent(pItem.Value.Analytics,false, false,pItem.Value.CostAmount);
                        break;
                    }
                }
            }

            IBaseScene pScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
            if(pScene != null)
            {
                if(pScene.IsShowInstance<TimeSale>())
                {
                    m_pCurrentPurchaseInfo.TimeSale = pScene.GetInstance<TimeSale>().GetTimeSaleNo();
                }
            }
         
#if UNITY_IOS
            m_pCurrentPurchaseInfo.Store = (byte)ALF.LOGIN.LoginManager.E_STORE.apple;
#elif ONESTORE
            m_pCurrentPurchaseInfo.Store = (byte)ALF.LOGIN.LoginManager.E_STORE.onestore;
#elif UNITY_ANDROID
            m_pCurrentPurchaseInfo.Store = (byte)ALF.LOGIN.LoginManager.E_STORE.google;
#endif
            m_pRestoreInfoList.List.Add(m_pCurrentPurchaseInfo);
            SavePurchaseReceiptData();

            JObject pJObject = new JObject();
            pJObject["sku"] = m_pCurrentPurchaseInfo.Sku;
            pJObject["receipt"] = m_pCurrentPurchaseInfo.Receipt;
            pJObject["currency"] = m_pCurrentPurchaseInfo.Currency;
            if(Application.isEditor)
            {
                pJObject["store"] = "hive";
            }   
            else
            {
                pJObject["store"] = ((ALF.LOGIN.LoginManager.E_STORE)m_pCurrentPurchaseInfo.Store).ToString();
            }
            
            pJObject["timeSale"] = m_pCurrentPurchaseInfo.TimeSale;
            
            SendAdjustEvent("pzbi0l",true,false,-1);
            SendAdjustEvent("s0h8iq",false,true,-1);
            NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.iap_purchase, GetNetworkAPI(E_REQUEST_ID.iap_purchase),true,true,null,pJObject);
        }
        else
        {
            NetworkManager.ShowWaitMark(false);
            IBaseScene pScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
            if(pScene != null)
            {
                Message pMessage = pScene.GetInstance<Message>();
                pMessage.SetMessage(GetLocalizingErrorMsg(pResult.code.ToString()),GetLocalizingText("MSG_BTN_OKAY"),(hive.ResultAPI.Code.IAPV4NeedRestore == pResult.code ? CurrentTransactionFinish : null ));
                SingleFunc.ShowAnimationDailog(pMessage.MainUI,null);
            }
            m_bTransactionFinish = false;
        }
    }

#endif

    int m_iCrowdSFX = -1;
    
    public static void InitCtx()
    {
        if(gCtx == null)
        {
            gCtx = new GameContext();
            gCtx.Init();
        }
    }

    public static void GameDataLoad(System.Action action)
    {
        if(Director.Runner != null)
        {
            Director.StartCoroutine(gCtx.coGameDataLoad(action));
        }
    }

    public static GameContext getCtx()
    {
        return gCtx;
    }
    
	protected GameContext(){}

    public bool IsRestorePurchase()
    {
        if(m_pRestoreInfoList.List == null) return false;

        return m_pRestoreInfoList.List.Count > 0;
    }

    public bool RestorePurchase()
    {
        CurrentTransactionFinish();

        if(m_pRestoreInfoList.List == null) return true;
        
        if(m_pRestoreInfoList.List.Count > 0)
        {
            int index = m_pRestoreInfoList.List.Count -1;
            m_pCurrentPurchaseInfo = m_pRestoreInfoList.List[index];
            JObject pJObject = new JObject();
            pJObject["sku"] = m_pCurrentPurchaseInfo.Sku;
            pJObject["receipt"] = m_pCurrentPurchaseInfo.Receipt;
            pJObject["currency"] = m_pCurrentPurchaseInfo.Currency;
            pJObject["store"] = ((ALF.LOGIN.LoginManager.E_STORE)m_pCurrentPurchaseInfo.Store).ToString();    
            pJObject["timeSale"] = m_pCurrentPurchaseInfo.TimeSale;
        
            NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.iap_purchase, GetNetworkAPI(E_REQUEST_ID.iap_purchase),true,true,null,pJObject);
        }
        return m_pRestoreInfoList.List.Count <= 0;
    }

    public void LoadRewardVideo()
    {
#if USE_HIVE
        RewardVideoInitialize();
        hive.HIVEAdKit.RewardVideo.LoadAd(m_pRewardVideoAd, "Unity-RewardVideo-Load-AdPlacementInfo");
        StateMachine.GetStateMachine().AddState(BaseState.GetInstance(new BaseStateTarget(Director.Runner),-1, (uint)E_STATE_TYPE.AD, null, executeRewardVideoLoadCallback,null,-1));
#endif
    }

    public bool ShowRewardVideo(System.Action<E_AD_STATUS> pRewardCallback )
    {
        if(IsRewardVideoLoaded())
        {
            if(Application.isEditor)
            {
                pRewardCallback(E_AD_STATUS.RewardComplete);
            }
            else
            {
                m_pRewardADCallback = pRewardCallback;
#if USE_HIVE
                hive.HIVEAdKit.RewardVideo.Show(m_pRewardVideoAd, "Unity-RewardVideo-Show-AdPlacementInfo");
#endif
            }
            return true;
        }

        if(!IsRewardVideoLoading())
        {
            LoadRewardVideo();
        }

        return false;
    }

    public bool IsRewardVideoLoading()
    {
        return StateMachine.GetStateMachine().IsCurrentTargetStates(Director.Runner,(uint)E_STATE_TYPE.AD);
    }

    public bool IsRewardVideoLoaded()
    {
        if(Application.isEditor) return true;
#if USE_HIVE
        if(m_pRewardVideoAd != null)
        {
            return hive.HIVEAdKit.RewardVideo.IsLoaded(m_pRewardVideoAd);
        }
#endif
        return false;
    }

    public void ShowNativeReview()
    {
        if (Application.isEditor)
        {
            IBaseScene pScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
            if(pScene != null)
            {
                Message pUI = pScene.GetInstance<Message>();
                pUI.SetMessage("NativeReview 미지원!!",GetLocalizingText("MSG_BTN_OKAY"),null);
                SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
            }
        }
#if USE_HIVE
        else
        {
#if ONESTORE
            NetworkManager.ShowWaitMark(true);
            ShowReview((hive.ResultAPI result, hive.PromotionEventType promotionEventType)=>{
                // Hive 리뷰 팝업 결과 콜백 함수
                NetworkManager.ShowWaitMark(false);
                // if (result.isSuccess()) {
                //     // API 호출 성공
                // }
            });
#else
            hive.Promotion.showNativeReview();
#endif
        }    
#endif  
    }
    
    void RemovePurchaseReceipt(RestorePurchaseT pRestorePurchase)
    {
        if(pRestorePurchase != null)
        {
            for(int i =0; i < m_pRestoreInfoList.List.Count; ++i)
            {
                if(m_pRestoreInfoList.List[i] == pRestorePurchase)
                {
                    m_pRestoreInfoList.List.RemoveAt(i);
                    return;
                }
            }
        }
    }
    
    public void CurrentTransactionFinish()
    {
        if(m_pCurrentPurchaseInfo != null)
        {
            TransactionFinish(m_pCurrentPurchaseInfo.Sku);
            RemovePurchaseReceipt(m_pCurrentPurchaseInfo);
            SavePurchaseReceiptData();
            m_pCurrentPurchaseInfo = null;
        }
    }

    void TransactionFinish(string pProductID)
    {
#if USE_HIVE
        hive.IAPV4.transactionFinish(pProductID, OnIAPV4TransactionFinishCB);
#endif
    }

    public bool IsProductInfoList()
    {
#if USE_HIVE
        return m_pProductInfoList.Count > 0;
#endif
        return false;
    }

    public void UpdateCurrencyPrice(TMPro.TMP_Text pText, string sku,string price)
    {
        if(pText == null) return;
#if USE_HIVE
        if(m_pProductInfoList.ContainsKey(sku))
        {
            pText.SetText(m_pProductInfoList[sku].displayPrice);
            return;
        }
#endif  

        pText.SetText(price);
    }

    public void UpdateCurrencyBonusPrice(TMPro.TMP_Text pText, string sku, float fBonus)
    {
        if(pText == null) return;
#if USE_HIVE
        if(m_pProductInfoList.ContainsKey(sku))
        {
            pText.SetText(( m_pProductInfoList[sku].price * (1.0f + fBonus)).ToString());
            return;
        }
#endif  

        pText.SetText("");
    }
    

    public bool RefreshSelfAreaRect()
    {
        if(ALFUtils.IsNotchDevice())
        {
            if( Input.deviceOrientation != DeviceOrientation.Unknown && m_eDeviceOrientation != Input.deviceOrientation)
            {
                m_eDeviceOrientation = Input.deviceOrientation;

                if(m_eDeviceOrientation != DeviceOrientation.LandscapeLeft && m_eDeviceOrientation != DeviceOrientation.LandscapeRight)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void PlayCrowdSFX()
    {
        m_iCrowdSFX = SoundManager.Instance.PlaySFX("sfx_match_crowd");
    }

    public void FadeCrowdSFX()
    {
        SoundManager.Instance.FadeOutFX(m_iCrowdSFX);
        m_iCrowdSFX = -1;
    }

    public int IsSendLocalNotification(PushItem? pPushItem)
    {
        if(pPushItem != null)
        {
            switch(pPushItem.Value.Id)
            {
                case "seasonStart":
                {
                    return (int)GetCurrentSeasonExpireRemainTime();
                }
                case "recruitRefreshAd":
                {
                    return (int)GetAdRefreshTime(1);
                }
                case "youthRefresh":
                {
                    return (int)GetAdRefreshTime(2);
                }
                default:
                {
                    return (int)pPushItem.Value.TimeGap;
                }
            }
        }
        
        return -1;
    }

    public string GetNetworkAPI(E_REQUEST_ID id)
    {
        if(id == E_REQUEST_ID.ServerInfo)
        {
#if FTM_LIVE
            return "https://config-ftm.s3.ap-northeast-2.amazonaws.com/ftm_service_config.json";
#else
            return "https://ftp.ncucu.com:26943/FTM_DEV/service_config.json";
#endif
        }
        
        string[] list = id.ToString().Split('_');
        string api = $"/{list[0]}";
        for(int i = 1; i < list.Length; ++i)
        {
            api += $"/{list[i]}";
        }

        return api;
    }


    void AddFlatBufferData(E_DATA_TYPE type, IFlatbufferObject pData)
    {
        if(pData == null) return;

        dataList[type] = pData;
    }

    public T GetFlatBufferData<T>(E_DATA_TYPE type) where T : IFlatbufferObject
    {
        if(dataList.ContainsKey(type)) 
        {
            return (T)dataList[type];
        }
        
        return default(T);
    }

    public List<MailInfoT> GetMails()
    {
        return m_pMailBoxInfo.Mails;
    }

    public MailInfoT GetMailInfoById(ulong id, byte type)
    {
        for(int i =0; i < m_pMailBoxInfo.Mails.Count; ++i)
        {
            if(m_pMailBoxInfo.Mails[i].Id == id && m_pMailBoxInfo.Mails[i].Type == type)
            {
                return m_pMailBoxInfo.Mails[i];
            }
        }

        return null;
    }

    public void AddReviewInfo()
    {
        JObject pJObject = new JObject();
        pJObject["msgId"] = (uint)E_REQUEST_ID.review_popup;
        m_pResponData.Enqueue(pJObject);
        AddReviewCount(true);
    }

    public JObject GetRecordMatchData()
    {
        string md = PlayerPrefs.GetString("md");
        
        if (string.IsNullOrEmpty(md))
        {
            return null;
        }

        try
        {
            RecordMatchData pRecordMatchData = RecordMatchData.GetRootAsRecordMatchData(new ByteBuffer(Convert.FromBase64String(md)));
            E_APP_MATCH_TYPE eType = (E_APP_MATCH_TYPE)pRecordMatchData.Status;
            
            JObject pObject = null;
            if(pRecordMatchData.MatchData != null)
            {
                pObject = MakeMatchStatsData(eType,pRecordMatchData.MatchData.Value.UnPack(),pRecordMatchData.HomePower);
            }
            else
            {
                pObject = new JObject();
            }
            
            if(pObject != null)
            {
                pObject["match"] = pRecordMatchData.Match;
                pObject["matchType"] = pRecordMatchData.MatchType;
                if(eType == E_APP_MATCH_TYPE.APP_MATCH_TYPE_CLEAR)
                {
                    pObject["away"]["name"] = pRecordMatchData.Away;
                    pObject["home"]["name"] = pRecordMatchData.Home;
                }
            }
            
            return pObject;

        }
        catch{
            
        }

        return null;
    }

    public bool IsTempLineupPlayer(ulong id)
    {
        if(m_pCacheLineupPlayerData != null)
        {
            for(int i =0; i < m_pCacheLineupPlayerData.Data.Count; ++i)
            {
                if(m_pCacheLineupPlayerData.Data[i] == id)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ClearCacheLineupPlayerData()
    {
        m_pCacheLineupPlayerData = null;
    }

    public void SetCacheLineupPlayerData(LineupPlayerT tempLineupPlayer)
    {
        m_pCacheLineupPlayerData = tempLineupPlayer;
    }

    public void DeleteRecordMatchData()
    {
        PlayerPrefs.DeleteKey("md");
        PlayerPrefs.Save();
    }

    public void SaveRecordMatchData(RecordT pRecord,E_APP_MATCH_TYPE eType,int iMatchType)
    {
        RecordMatchDataT pRecordMatchData = new RecordMatchDataT();
        pRecordMatchData.Match = m_pMatchData.Match;
        pRecordMatchData.MatchType = iMatchType;
        pRecordMatchData.Status = (byte)eType;
        pRecordMatchData.Home = GetClubName();
        pRecordMatchData.Away = m_pMatchData.GetClubName();
        pRecordMatchData.HomePower = GetTotalPlayerAbility(GetActiveLineUpType());
        pRecordMatchData.MatchData = pRecord;

        PlayerPrefs.SetString("md" ,Convert.ToBase64String(pRecordMatchData.SerializeToBinary()));
        PlayerPrefs.Save();
    }

    void LoadCacheAdRewardData()
    {
        if(m_pAdRewardInfo != null)
        {
            m_pAdRewardInfo.List.Clear();
            m_pAdRewardInfo.List = null;
            m_pAdRewardInfo = null;
        }
        
        string ar = PlayerPrefs.GetString($"{m_pClubInfo.Id}:ar");
        
        if (!string.IsNullOrEmpty(ar))
        {
            try
            {
                AdRewardInfo pAdRewardInfo = AdRewardInfo.GetRootAsAdRewardInfo(new ByteBuffer(Convert.FromBase64String(ar)));
                m_pAdRewardInfo = pAdRewardInfo.UnPack();
            }
            catch{
                PlayerPrefs.DeleteKey($"{m_pClubInfo.Id}:ar");
            }

            return;
        }

        m_pAdRewardInfo = new AdRewardInfoT();
        m_pAdRewardInfo.List = new List<AdRewardDataT>();
    }

    void LoadCachePlayerData()
    {
        if(m_pCachePlayerData != null)
        {
            m_pCachePlayerData.Recuiting.Clear();
            m_pCachePlayerData.YouthPromotion.Clear();
            m_pCachePlayerData = null;
        }
        
        string cp = PlayerPrefs.GetString($"{m_pClubInfo.Id}:cp");
        
        if (!string.IsNullOrEmpty(cp))
        {
            try
            {
                CachePlayerData pCachePlayerData = CachePlayerData.GetRootAsCachePlayerData(new ByteBuffer(Convert.FromBase64String(cp)));
                m_pCachePlayerData = pCachePlayerData.UnPack();
                return;
            }
            catch{
                PlayerPrefs.DeleteKey($"{m_pClubInfo.Id}:cp");
            }
        }


        m_pCachePlayerData = new CachePlayerDataT();
        m_pCachePlayerData.Recuiting = new List<PlayerT>();
        m_pCachePlayerData.YouthPromotion = new List<PlayerT>();
        m_pCachePlayerData.RecruitRefreshFree = GetConstValue(E_CONST_TYPE.transferRecruitRefreshFreeChanceCount);
        m_pCachePlayerData.YouthRefreshFree = GetConstValue(E_CONST_TYPE.transferYouthRefreshFreeChanceCount);
    }

    public void ClearCachePlayerData()
    {
        PlayerPrefs.DeleteKey($"{m_pClubInfo.Id}:cp");
        LoadCachePlayerData();
    }

    public void MakeCachePlayerList(List<PlayerT> pPlayerList,JArray pArray)
    {
        if(pPlayerList != null && pArray != null)
        {
            pPlayerList.Clear();
        
            int i = 0;
            int n = 0;
            JObject item = null;
            PlayerT pPlayer = null;
            for(i= 0; i < pArray.Count; ++i)
            {
                item = (JObject)pArray[i];
                pPlayer = new PlayerT();
                pPlayer.PositionFamiliars = new List<byte>();
                pPlayer.Ability = new List<PlayerAbilityT>();
                
                for(n =0; n < (int)E_ABILINDEX.AB_END; ++n)
                {
                    pPlayer.Ability.Add(new PlayerAbilityT());
                }
                
                UpdatePlayer(pPlayer,item);
                pPlayerList.Add(pPlayer);
            }
        }
    }
    public void ResetDeviceOrientation()
    {
        m_eDeviceOrientation = DeviceOrientation.Unknown;
    }
    public void ClearAllExpireTime()
    {
        if(m_pAuctionInfoData != null)
        {
            m_pAuctionInfoData.List.Clear();
            m_pAuctionInfoData.TExpire = 0;
        }

        List<ITimer> list = m_pExpireTimeList.Keys.ToList();
        int i = list.Count;
        while(i > 0)
        {
            --i;
            m_pExpireTimeList[list[i]].Clear();
            m_pExpireTimeList.Remove(list[i]);
        }
        m_pExpireTimeList.Clear();
    }

    public bool IsExpireAuctionInfoData()
    {
        if(m_pAuctionInfoData != null)
        {
            return NetworkManager.GetGameServerTime().Ticks >= m_pAuctionInfoData.TExpire;
        }

        return true;
    }

    public bool IsExpireRecuitingCachePlayerData()
    {
        if(m_pCachePlayerData != null)
        {
            return NetworkManager.GetGameServerTime().Ticks >= m_pCachePlayerData.TRecruitExpire;
        }
        return true;
    }

    public bool IsExpireYouthCachePlayerData()
    {
        if(m_pCachePlayerData != null)
        {
            return NetworkManager.GetGameServerTime().Ticks >= m_pCachePlayerData.TYouthExpire;
        }
        return true;
    }

    public float GetExpireCachePlayerData(bool bRecruit)
    {
        if(m_pCachePlayerData != null)
        {
            long time = bRecruit ? m_pCachePlayerData.TRecruitExpire : m_pCachePlayerData.TYouthExpire;
            return (float)(time - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond;
        }
        return -1;
    }

    public ulong GetAuctionInitialGoldById(E_PLAYER_INFO_TYPE eType,uint id)
    {
        if(eType == E_PLAYER_INFO_TYPE.auction)
        {
            for(int i =0; i < m_pAuctionInfoData.List.Count; ++i)
            {
                if(m_pAuctionInfoData.List[i].AuctionId == id)
                {
                    return m_pAuctionInfoData.List[i].InitialGold;
                }
            }
        }
        else if(eType == E_PLAYER_INFO_TYPE.bidding)
        {
            for(int i =0; i < m_pAuctionBiddingInfoData.Count; ++i)
            {
                if(m_pAuctionBiddingInfoData[i].Player.Id == id)
                {
                    return m_pAuctionBiddingInfoData[i].FinalGold;
                }
            }
        }

        return 0;
    }

    public ulong GetAuctionInitialGoldByPlayerNo(ulong id)
    {
        for(int i =0; i < m_pAuctionInfoData.List.Count; ++i)
        {
            if(m_pAuctionInfoData.List[i].Player.Id == id)
            {
                return m_pAuctionInfoData.List[i].InitialGold;
            }
        }

        return 0;
    }
    
    public uint GetAuctionIdByPlayerNo(E_PLAYER_INFO_TYPE eType,ulong id)
    {
        if( eType == E_PLAYER_INFO_TYPE.auction)
        {
            for(int i =0; i < m_pAuctionInfoData.List.Count; ++i)
            {
                if(m_pAuctionInfoData.List[i].Player.Id == id)
                {
                    return m_pAuctionInfoData.List[i].AuctionId;
                }
            }
        }
        else if( eType == E_PLAYER_INFO_TYPE.bidding)
        {
            for(int i =0; i < m_pAuctionBiddingInfoData.Count; ++i)
            {
                if(m_pAuctionBiddingInfoData[i].Player.Id == id)
                {
                    return m_pAuctionBiddingInfoData[i].AuctionId;
                }
            }
        }

        return 0;
    }

    public PlayerT GetCachePlayerDataByNo(ulong id,E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.bidding)
        {
            for(int i =0; i < m_pAuctionBiddingInfoData.Count; ++i)
            {
                if(m_pAuctionBiddingInfoData[i].Player.Id == id)
                {
                    return m_pAuctionBiddingInfoData[i].Player;
                }
            }
        }
        else if(eType == E_PLAYER_INFO_TYPE.recuiting)
        {
            for(int i =0; i < m_pCachePlayerData.Recuiting.Count; ++i)
            {
                if(m_pCachePlayerData.Recuiting[i].Id == id)
                {
                    return m_pCachePlayerData.Recuiting[i];
                }
            }
        }
        else if(eType == E_PLAYER_INFO_TYPE.auction)
        {
            for(int i =0; i < m_pAuctionInfoData.List.Count; ++i)
            {
                if(m_pAuctionInfoData.List[i].Player.Id == id)
                {
                    return m_pAuctionInfoData.List[i].Player;
                }
            }
        }
        
        return null;
    }

    public List<PlayerT> GetRecuitingCachePlayerData()
    {
        return m_pCachePlayerData.Recuiting;
    }

    public int GetRecuitingCachePlayerDataCount()
    {
        if(m_pCachePlayerData.Recuiting != null)
        {
            return m_pCachePlayerData.Recuiting.Count;
        }
        return 0;
    }

    public List<PlayerT> GetYouthPromotionCachePlayerData()
    {
        return m_pCachePlayerData.YouthPromotion;
    }

    public int GetYouthRefreshFreeCachePlayerData()
    {
        return m_pCachePlayerData.YouthRefreshFree;
    }
    public int GetRecruitRefreshFreeCachePlayerData()
    {
        return m_pCachePlayerData.RecruitRefreshFree;
    }

    public void SaveAdRewardData()
    {
        PlayerPrefs.SetString($"{m_pClubInfo.Id}:ar" ,Convert.ToBase64String(m_pAdRewardInfo.SerializeToBinary()));
        PlayerPrefs.Save();
    }

    public void SaveCachePlayerData(E_REQUEST_ID eID,JObject data)
    {
        switch(eID)
        {
            case E_REQUEST_ID.tutorial_getRecruit:
            case E_REQUEST_ID.recruit_get:
            case E_REQUEST_ID.recruit_refresh:
            {
                m_pCachePlayerData.TRecruitExpire = NetworkManager.ConvertLocalGameTimeTick((string)data["tExpire"]);
                m_pCachePlayerData.RecruitRefreshFree = (int)data["free"];
                if(data.ContainsKey("recruitPlayers"))
                {
                    MakeCachePlayerList(m_pCachePlayerData.Recuiting,(JArray)data["recruitPlayers"]);
                }
            }
            break;
            case E_REQUEST_ID.youth_get:
            case E_REQUEST_ID.youth_refresh:
            {
                m_pCachePlayerData.TYouthExpire = NetworkManager.ConvertLocalGameTimeTick((string)data["tExpire"]);
                m_pCachePlayerData.YouthRefreshFree = (int)data["free"];
                
                if(data.ContainsKey("youthPlayers"))
                {
                    MakeCachePlayerList(m_pCachePlayerData.YouthPromotion,(JArray)data["youthPlayers"]);
                }
            }
            break;
            case E_REQUEST_ID.recruit_offer:
            {
                if(data.ContainsKey("oldId"))
                {
                    ulong oldId = (ulong)data["oldId"];
                    for( int n = 0; n < m_pCachePlayerData.Recuiting.Count; ++n)
                    {
                        if(m_pCachePlayerData.Recuiting[n].Id == oldId)
                        {
                            m_pCachePlayerData.Recuiting[n].Status = Transfer.E_STATUS_OFFER;
                            break;
                        }
                    }
                }
            }
            break;
            case E_REQUEST_ID.youth_offer:
            {
                if(data.ContainsKey("oldId"))
                {
                    ulong oldId = (ulong)data["oldId"];
                    for( int n = 0; n < m_pCachePlayerData.YouthPromotion.Count; ++n)
                    {
                        if(m_pCachePlayerData.YouthPromotion[n].Id == oldId)
                        {
                            m_pCachePlayerData.YouthPromotion[n].Status = Transfer.E_STATUS_OFFER;
                            break;
                        }
                    }
                }
            }
            break;
            default: return;
        }
        PlayerPrefs.SetString($"{m_pClubInfo.Id}:cp" ,Convert.ToBase64String(m_pCachePlayerData.SerializeToBinary()));
        PlayerPrefs.Save();
    }
    public void DeleteCashData()
    {
        int LLSP = PlayerPrefs.GetInt("LLSP");
        string uuid = PlayerPrefs.GetString("uuid");
        float volumeBGM = PlayerPrefs.GetFloat("BGMV", 1);
        float volumeSFX = PlayerPrefs.GetFloat("SFXV", 1);  
        
        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetInt("LLSP",LLSP);
        PlayerPrefs.SetString("uuid",uuid);
        PlayerPrefs.SetFloat("BGMV", volumeBGM);
        PlayerPrefs.SetFloat("SFXV", volumeSFX);
        PlayerPrefs.Save();
    }
    public ulong GetCurrentMatchID()
    {
        if(m_pMatchData != null)
        {
            return m_pMatchData.Match;
        }
        return 0;
    }

    public JObject MakeMatchStatsData(E_APP_MATCH_TYPE eMatchType)
    {        
        if(eMatchType == E_APP_MATCH_TYPE.APP_MATCH_TYPE_NONE) return null;
        JObject pObject = new JObject();
        if(m_pMatchData != null)
        {
            pObject["match"] = m_pMatchData.Match;
        }
        
        pObject["status"] = (byte)eMatchType;
        pObject["homePower"] = GetTotalPlayerAbility(GetActiveLineUpType());
        return pObject;
    }

    public JObject MakeMatchStatsData(E_APP_MATCH_TYPE eMatchType, RecordT pRecord,uint homePower)
    { 
        if(eMatchType == E_APP_MATCH_TYPE.APP_MATCH_TYPE_NONE) return null;
        JObject pObject = new JObject();
        if(m_pMatchData != null)
        {
            pObject["match"] = m_pMatchData.Match;
        }

        pObject["status"] = (byte)eMatchType;
        pObject["homePower"] = homePower;

        if(eMatchType == E_APP_MATCH_TYPE.APP_MATCH_TYPE_CLEAR)
        {
            JObject pHome = new JObject();
            if(m_pClubInfo != null)
            {
                pHome["name"] = GetClubName();
            }
            
            pObject["timeLine"] = Newtonsoft.Json.JsonConvert.SerializeObject(pRecord, Newtonsoft.Json.Formatting.None,new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore});
            
            pHome["match"] = MakeTeamStatsData(pRecord.StatisticsRecord.StatisticsTeam[0]);
            pHome["players"] = MakePlayersMatchStatsData(pRecord.StatisticsRecord.StatisticsPlayers[0]);
            pObject["home"] = pHome;

            JObject pAway = new JObject();
            if(m_pMatchData != null)
            {
                pAway["name"] = m_pMatchData.GetClubName();
            }
            
            pAway["match"] = MakeTeamStatsData(pRecord.StatisticsRecord.StatisticsTeam[1]);
            pAway["players"] = MakePlayersMatchStatsData(pRecord.StatisticsRecord.StatisticsPlayers[1]);
            pObject["away"] = pAway;
        }
        else if(eMatchType == E_APP_MATCH_TYPE.APP_MATCH_TYPE_SHUTDOWN)
        {
            JObject pHome = new JObject();
            JArray pJArray = new JArray();
            PlayerStatisticsT pPlayerStatistics = null;
            JObject pItem = null;
            int value = 0;
            int count = pRecord.StatisticsRecord.StatisticsPlayers[0].Players.Count;
            for(int i =0; i < count; ++i)
            {
                pItem = new JObject();
                pPlayerStatistics = pRecord.StatisticsRecord.StatisticsPlayers[0].Players[i];
                pItem["player"] = pPlayerStatistics.PlayerId;

                value = pPlayerStatistics.Hp - 30;
                if(value < 1)
                {
                    value = 1;
                }
                
                pItem["hp"] = value;
                pJArray.Add(pObject);
            }
            
            pHome["players"] = pJArray;
            pObject["home"] = pHome;
        }
        
        return pObject;
    }

    JObject MakeTeamStatsData(TeamStatisticsT pTeamStatistics)
    {
        JObject pMatchStats = new JObject();
        int value = pTeamStatistics.TeamRating;
        if(value > 0)
        {
            pMatchStats["rating"] = value;
        }

        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_GOAL];
        pMatchStats["goals"] = value;
        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_ASSIST];
        if(value > 0)
        {
            pMatchStats["assists"] = value;
        }
        value = pTeamStatistics.PossessionTotal;
        if(value > 0)
        {
            pMatchStats["posession"] = value;
        }

        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT];
        if(value > 0)
        {
            pMatchStats["shots"] = value;
        }
        
        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT_ONTARGET];
        if(value > 0)
        {
            pMatchStats["shotsOnTarget"] = value;
        }

        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSSUCCESS];
        if(value > 0)
        {
            pMatchStats["passes"] = value;
        }

        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSTRY];
        if(value > 0)
        {
            pMatchStats["passesAttempted"] = value;
        }

        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_STEAL];
        if(value > 0)
        {
            pMatchStats["tackles"] = value;
        }

        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_INTERCEPT];
        if(value > 0)
        {
            pMatchStats["interceptions"] = value;
        }
        
        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_GK_GOOD_DEFENCE];
        if(value > 0)
        {
            pMatchStats["saves"] = value;
        }
        value = pTeamStatistics.TotalOffside;
        if(value > 0)
        {
            pMatchStats["offsides"] = value;
        }
        
        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_CORNERKICK];
        if(value > 0)
        {
            pMatchStats["corners"] = value;
        }
        
        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_FOUL];
        if(value > 0)
        {
            pMatchStats["fouls"] = value;
        }
        
        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_YELLO_CARD];
        if(value > 0)
        {
            pMatchStats["yellowCards"] = value;
        }

        value = pTeamStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_RED_CARD];
        if(value > 0)
        {
            pMatchStats["redCards"] = value;
        }
    
        return pMatchStats;
    }

    JArray MakePlayersMatchStatsData( StatisticsPlayersT pStatisticsPlayers)
    {
        JArray pJArray = new JArray();
        PlayerStatisticsT pPlayerStatistics = null;
        int value = 0;
        JObject pObject = null;
        for(int i =0; i < pStatisticsPlayers.Players.Count; ++i)
        {
            pPlayerStatistics = pStatisticsPlayers.Players[i];
            if(pPlayerStatistics.TotalRunDistance > 0)
            {
                pObject = new JObject();
                pObject["hp"] = pPlayerStatistics.Hp < 1 ? 1 : pPlayerStatistics.Hp;
                pObject["name"] = pPlayerStatistics.Name;
                pObject["player"] = pPlayerStatistics.PlayerId;
                pObject["totalRunDistance"] = (int)pPlayerStatistics.TotalRunDistance;

                value = (int)(pPlayerStatistics.Rating * 10);
                if(value > 0)
                {
                    pObject["rating"] = value;
                }

                value = (int)(pPlayerStatistics.SubstitutionInTotalGameSec);
                if(value > 0)
                {
                    pObject["tSubstitutionIn"] = value;
                }

                value = (int)(pPlayerStatistics.SubstitutionOutTotalGameSec);
                if(value > 0)
                {
                    pObject["tSubstitutionOut"] = value;
                }

                value = (int)(pPlayerStatistics.TackleTry);
                if(value > 0)
                {
                    pObject["tackleTry"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_GOAL];
                if(value > 0)
                {
                    pObject["goals"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_ASSIST];
                if(value > 0)
                {
                    pObject["assists"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT_ONTARGET];
                if(value > 0)
                {
                    pObject["shotsOnTarget"] = value;
                }
                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_SHOOT];
                if(value > 0)
                {
                    pObject["shots"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSSUCCESS];
                if(value > 0)
                {
                    pObject["passes"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_PASSTRY];
                if(value > 0)
                {
                    pObject["passesAttempted"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_STEAL];
                if(value > 0)
                {
                    pObject["tackles"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_INTERCEPT];
                if(value > 0)
                {
                    pObject["interceptions"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_GK_GOOD_DEFENCE];
                if(value > 0)
                {
                    pObject["saves"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_FOUL];
                if(value > 0)
                {
                    pObject["fouls"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_YELLO_CARD];
                if(value > 0)
                {
                    pObject["yellowCards"] = value;
                }

                value = pPlayerStatistics.Common[(int)E_COMMON_STATISTICS.COMMON_STATISTICS_RED_CARD];
                if(value > 0)
                {
                    pObject["redCards"] = value;
                }
                
                pJArray.Add(pObject);
            }
        }
        
        return pJArray;
    }

    public AuctionBiddingInfoT UpdateBidBroadcast(JObject data,ref bool bAdd)
    {
        bAdd = false;
        if(data.ContainsKey("payload") && data["payload"].Type != JTokenType.Null)
        {
            JObject payload = (JObject)data["payload"];
            uint auctionId = (uint)payload["auctionId"];
            AuctionBiddingInfoT pAuctionBiddingInfo = GetAuctionBiddingInfoByID(auctionId);
            JObject pData = null;
            if(payload.ContainsKey("data") && payload["data"].Type != JTokenType.Null)
            {
                pData = (JObject)payload["data"];
            }

            if(pAuctionBiddingInfo == null && (ulong)pData["Id"] == GetClubID())
            {
                for(int i =0; i < m_pAuctionInfoData.List.Count; ++i)
                {
                    if(m_pAuctionInfoData.List[i].AuctionId == auctionId)
                    {
                        pAuctionBiddingInfo = new AuctionBiddingInfoT();
                        pAuctionBiddingInfo.AuctionId = auctionId;
                        pAuctionBiddingInfo.Player = m_pAuctionInfoData.List[i].Player;
                        pAuctionBiddingInfo.Reward = false;
                        m_pAuctionInfoData.List.RemoveAt(i);
                        m_pAuctionBiddingInfoData.Add(pAuctionBiddingInfo);
                        bAdd = true;
                        break;
                    }
                } 
            }
            
            if(pAuctionBiddingInfo != null)
            {
                ulong money =(ulong)payload["finalGold"];
                uint cash = (uint)payload["finalToken"];
                
                pAuctionBiddingInfo.TExtend = (float)(NetworkManager.ConvertLocalGameTimeTick((string)payload["tExtend"]) - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond;
                pAuctionBiddingInfo.FinalGold = money;
                pAuctionBiddingInfo.FinalToken = cash;
                if(pData != null)
                {
                    if((ulong)pData["Id"] == GetClubID())
                    {
                        pAuctionBiddingInfo.Gold = money;
                        pAuctionBiddingInfo.Token = cash; 
                    }
                    pAuctionBiddingInfo.Msg = payload["data"].ToString();
                }
            }

            return pAuctionBiddingInfo;
        }

        return null;
    }

    public uint JoinAuctionFail(JObject payload)
    {
        if(payload != null)
        {
            if(payload.ContainsKey("tServer") && payload["tServer"].Type != JTokenType.Null)
            {
                NetworkManager.SetGameServerTime(DateTime.Parse((string)payload["tServer"]));
            }

            if(payload.ContainsKey("auctionId") && payload["auctionId"].Type != JTokenType.Null)
            {
                uint auctionId = (uint)payload["auctionId"];
                AuctionSellInfoT pAuctionSellInfo = GetAuctionSellInfoByID(auctionId);
                if(pAuctionSellInfo != null)
                {
                    pAuctionSellInfo.End = true;
                    pAuctionSellInfo.TExtend = 0;
                    pAuctionSellInfo.Join = true;
                    pAuctionSellInfo.Update = true;
                }
                else
                {
                    AuctionBiddingInfoT pAuctionBiddingInfo = GetAuctionBiddingInfoByID(auctionId);
                    if(pAuctionBiddingInfo != null)
                    {
                        pAuctionBiddingInfo.Update = true;
                        pAuctionBiddingInfo.TExtend = 0;
                    }

                    return auctionId;
                }
            }
        }
        return 0;
    }

    public void UpdateCurrentAuctionInfo(E_SOCKET_ID eId,JObject payload)
    {
        if(payload == null) return;

        if(payload.ContainsKey("tServer") && payload["tServer"].Type != JTokenType.Null)
        {
            NetworkManager.SetGameServerTime(DateTime.Parse((string)payload["tServer"]));
        }

        if(payload.ContainsKey("auctionId") && payload["auctionId"].Type != JTokenType.Null)
        {
            uint auctionId = (uint)payload["auctionId"];
            AuctionSellInfoT pAuctionSellInfo = GetAuctionSellInfoByID(auctionId);
            if(pAuctionSellInfo != null)
            {
                pAuctionSellInfo.Update = true;

                if(payload.ContainsKey("tExtend") && payload["tExtend"].Type != JTokenType.Null)
                {
                    pAuctionSellInfo.TExtend = (float)(NetworkManager.ConvertLocalGameTimeTick((string)payload["tExtend"]) - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond;
                    Debug.Log($"------------------fTimeRefreshTime:{pAuctionSellInfo.TExtend}");
                }
                
                ulong finalGold =0;
                uint finalToken =0;

                if(E_SOCKET_ID.auctionJoin == eId)
                {
                    if(payload.ContainsKey("bids") && payload["bids"].Type != JTokenType.Null)
                    {
                        SingleFunc.OutputPaylodData((JArray)payload["bids"], ref finalGold,ref finalToken);
                    }
                }
                else
                {
                    if(payload.ContainsKey("finalGold") && payload["finalGold"].Type != JTokenType.Null)
                    {
                        finalGold = (ulong)payload["finalGold"];
                    }
                }

                pAuctionSellInfo.FinalGold = finalGold;
            }
            else
            {
                AuctionBiddingInfoT pAuctionBiddingInfo = GetAuctionBiddingInfoByID(auctionId);
                if(pAuctionBiddingInfo != null)
                {
                    pAuctionBiddingInfo.Update = true;
                    
                    if(payload.ContainsKey("tExtend") && payload["tExtend"].Type != JTokenType.Null)
                    {
                        pAuctionBiddingInfo.TExtend = (float)(NetworkManager.ConvertLocalGameTimeTick((string)payload["tExtend"]) - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond;
                    }

                    ulong finalGold =0;
                    uint finalToken =0;

                    if(E_SOCKET_ID.auctionJoin == eId)
                    {
                        if(payload.ContainsKey("bids") && payload["bids"].Type != JTokenType.Null)
                        {
                            SingleFunc.OutputPaylodData((JArray)payload["bids"], ref finalGold,ref finalToken);
                        }
                    }
                    else
                    {
                        if(payload.ContainsKey("finalGold") && payload["finalGold"].Type != JTokenType.Null)
                        {
                            finalGold = (ulong)payload["finalGold"];
                        }

                        if(payload.ContainsKey("finalToken") && payload["finalToken"].Type != JTokenType.Null)
                        {
                            finalToken = (uint)payload["finalToken"];
                        }
                    }

                    pAuctionBiddingInfo.FinalGold = finalGold;
                    pAuctionBiddingInfo.FinalToken = finalToken;
                }
            }
        }
    }

    public void LeaveCurrentAuction()
    {
        int i = 0;
        JObject pJObject = null;
        if(m_pAuctionBiddingInfoData != null)
        {
            i = m_pAuctionBiddingInfoData.Count;
            while(i > 0)
            {
                --i;
                if(!m_pAuctionBiddingInfoData[i].Reward && m_pAuctionBiddingInfoData[i].TExtend > 0)
                {
                    m_pAuctionBiddingInfoData[i].Update = false;
                    pJObject = new JObject();
                    pJObject["type"]= E_SOKET.auctionLeave.ToString();
                    pJObject["auctionId"] = m_pAuctionBiddingInfoData[i].AuctionId;
                    NetworkManager.SendMessage(pJObject);
                }
            }
        }
        if(m_pAuctionSellInfoData != null)
        {
            i = m_pAuctionSellInfoData.Count;
            while(i > 0)
            {
                --i;
                if(!m_pAuctionSellInfoData[i].Reward && m_pAuctionSellInfoData[i].Join)
                {
                    if( m_pAuctionSellInfoData[i].TExtend <= 0)
                    {
                        m_pAuctionSellInfoData[i].Join = false;
                    }

                    m_pAuctionSellInfoData[i].Update = false;
                    pJObject = new JObject();
                    pJObject["type"]= E_SOKET.auctionLeave.ToString();
                    pJObject["auctionId"] = m_pAuctionSellInfoData[i].AuctionId;
                    NetworkManager.SendMessage(pJObject);
                }
            }
        }
    }

    public void PauseCurrentAuction(uint ignoreAuctionId)
    {
        int i = 0;
        if(m_pAuctionBiddingInfoData != null)
        {
            i = m_pAuctionBiddingInfoData.Count;
        
            while(i > 0)
            {
                --i;
                if(ignoreAuctionId == m_pAuctionBiddingInfoData[i].AuctionId) continue;
                m_pAuctionBiddingInfoData[i].Update = false;
            }
        }

        if(m_pAuctionSellInfoData != null)
        {
            i = m_pAuctionSellInfoData.Count;
        
            while(i > 0)
            {
                --i;
                if(ignoreAuctionId == m_pAuctionSellInfoData[i].AuctionId) continue;
                m_pAuctionSellInfoData[i].Update = false;
            }
        }
    }

    public bool IsCurrentAuction()
    {
        return (m_pAuctionBiddingInfoData != null && m_pAuctionBiddingInfoData.Count > 0) || (m_pAuctionSellInfoData != null || m_pAuctionSellInfoData.Count > 0);
    }

    public void ConnectCurrentAuction()
    {
        ConnectAuctionSell();
        ConnectAuctionBid();
    }

    void ConnectAuctionBid()
    {
        if(!NetworkManager.IsSocketAlive()) return;
        int i = 0;
        JObject pJObject = null;
        if(m_pAuctionBiddingInfoData != null)
        {
            i = m_pAuctionBiddingInfoData.Count;
        
            while(i > 0)
            {
                --i;
                if(!m_pAuctionBiddingInfoData[i].Reward)
                {
                    m_pAuctionBiddingInfoData[i].Update = false;
                    pJObject = new JObject();
                    pJObject["type"]= E_SOKET.auctionJoin.ToString();
                    pJObject["auctionId"] = m_pAuctionBiddingInfoData[i].AuctionId;
                    NetworkManager.SendMessage(pJObject);
                }
            }
        }
    }

    void ConnectAuctionSell()
    {
        if(!NetworkManager.IsSocketAlive()) return;
        int i = 0;
        JObject pJObject = null;
        
        if(m_pAuctionSellInfoData != null)
        {
            i = m_pAuctionSellInfoData.Count;
        
            while(i > 0)
            {
                --i;
                if(m_pAuctionSellInfoData[i].End) continue;

                if(m_pAuctionSellInfoData[i].Join || (!m_pAuctionSellInfoData[i].Reward && m_pAuctionSellInfoData[i].TExtend < 10))
                {
                    m_pAuctionSellInfoData[i].Join = true;
                    m_pAuctionSellInfoData[i].Update = false;
                    pJObject = new JObject();
                    pJObject["type"]= E_SOKET.auctionJoin.ToString();
                    pJObject["auctionId"] = m_pAuctionSellInfoData[i].AuctionId;
                    Debug.Log($"ConnectAuctionSell-----------m_pAuctionSellInfoData[i].Join:{m_pAuctionSellInfoData[i].Join} m_pAuctionSellInfoData[i].TExtend:{m_pAuctionSellInfoData[i].TExtend}");
                    NetworkManager.SendMessage(pJObject);
                }
            }
        }
    }

    void SendMiscarriedAuctionSell()
    {
        JObject pJObject = null;
        JArray pJArray = null;
        int i = m_pAuctionSellInfoData.Count;
        bool bRemove = false;
        bool bGet= false;
        bool bWithDraw = false;
        while(i > 0)
        {
            --i;
            bRemove = false;
            bGet = false;
            bWithDraw = false;
            if(m_pAuctionSellInfoData[i].Join && m_pAuctionSellInfoData[i].TExtend > 0 )
            {
                m_pAuctionSellInfoData[i].Join = false;
                m_pAuctionSellInfoData[i].Update = true;
            }
            else if(m_pAuctionSellInfoData[i].TExpire <= 0)
            {
                if(m_pAuctionSellInfoData[i].TExtend > 0)
                {
                    m_pAuctionSellInfoData[i].Update = true;
                }
                else
                {
                    bGet = m_pAuctionSellInfoData[i].FinalGold > 0;
                    bWithDraw = !bGet;
                }
            }
            else if(!m_pAuctionSellInfoData[i].Join && m_pAuctionSellInfoData[i].FinalGold > 0 && m_pAuctionSellInfoData[i].TExtend <= 0)
            {
                m_pAuctionSellInfoData[i].Reward = true;
                bGet = true;
            }
            
            if(bGet)
            {
                bRemove = true;
                m_pAuctionSellInfoData[i].Update = false;
                pJObject = new JObject();
                pJArray = new JArray();
                pJArray.Add(m_pAuctionSellInfoData[i].AuctionId);
                pJObject["auctionIds"] = pJArray;
                NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.auction_reward, GetNetworkAPI(E_REQUEST_ID.auction_reward),false,true,null,pJObject);
            }
            else if(bWithDraw)
            {
                bRemove = true;
                m_pAuctionSellInfoData[i].Update = false;
                pJObject = new JObject();
                pJArray = new JArray();
                pJArray.Add(m_pAuctionSellInfoData[i].AuctionId);
                pJObject["auctionIds"] = pJArray;
                Debug.Log("SendMiscarriedAuctionSell----------------------------------E_REQUEST_ID.auction_withdraw");
                NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.auction_withdraw, GetNetworkAPI(E_REQUEST_ID.auction_withdraw),false,true,null,pJObject);
            }
            else if(m_pAuctionSellInfoData[i].Reward)
            {
                bRemove = true;
            }
            
            if(bRemove)
            {
                m_pAuctionSellInfoData.RemoveAt(i);
            }
        }
    }

    void RemoveCompleteAuctionBid()
    {
        int i = m_pAuctionBiddingInfoData.Count;

        while(i > 0)
        {
            --i;
            if(m_pAuctionBiddingInfoData[i].Reward && m_pAuctionBiddingInfoData[i].TExtend <= 0)
            {
                m_pAuctionBiddingInfoData.RemoveAt(i);
            }
        }
    }

    void SendCompleteAuctionBid()
    {
        JObject pJObject = null;
        JArray pJArray = null;

        int i = m_pAuctionBiddingInfoData.Count;

        while(i > 0)
        {
            --i;
            if(!m_pAuctionBiddingInfoData[i].Reward && m_pAuctionBiddingInfoData[i].TExtend <= 0)
            {
                m_pAuctionBiddingInfoData[i].Reward = true;
                if(m_pAuctionBiddingInfoData[i].FinalGold == m_pAuctionBiddingInfoData[i].Gold && m_pAuctionBiddingInfoData[i].FinalToken == m_pAuctionBiddingInfoData[i].Token)
                {
                    pJObject = new JObject();
                    pJArray = new JArray();
                    pJArray.Add(m_pAuctionBiddingInfoData[i].AuctionId);
                    pJObject["auctionIds"] = pJArray;
                    NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.auction_trade, GetNetworkAPI(E_REQUEST_ID.auction_trade),true,true,null,pJObject);
                }
                else
                {
                    pJObject = new JObject();
                    pJArray = new JArray();
                    pJArray.Add(m_pAuctionBiddingInfoData[i].AuctionId);
                    pJObject["auctionIds"] = pJArray;
                    NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.auction_refund, GetNetworkAPI(E_REQUEST_ID.auction_refund),true,true,null,pJObject);
                }
            }
        }
    }

    public void RemoveAuctionSellInfoByPlayerID(ulong id)
    {
        for(int i =0; i < m_pAuctionSellInfoData.Count; ++i)
        {
            if(m_pAuctionSellInfoData[i].Player == id)
            {
                m_pAuctionSellInfoData[i].Reward = true;
                m_pAuctionSellInfoData.RemoveAt(i);
            }
        }
    }

    public void RemoveAuctionSellInfo(uint id)
    {
        for(int i =0; i < m_pAuctionSellInfoData.Count; ++i)
        {
            if(m_pAuctionSellInfoData[i].AuctionId == id)
            {
                m_pAuctionSellInfoData[i].Reward = true;
                m_pAuctionSellInfoData.RemoveAt(i);
            }
        }
    }

    public uint GetAuctionSellInfoIDByPlayerID(ulong id)
    {
        for(int i =0; i < m_pAuctionSellInfoData.Count; ++i)
        {
            if(m_pAuctionSellInfoData[i].Player == id)
            {
                return m_pAuctionSellInfoData[i].AuctionId;
            }
        }
        return 0;
    }

    void RemoveAuctionBidInfoByID(uint id)
    {
        for(int i =0; i < m_pAuctionBiddingInfoData.Count; ++i)
        {
            if(m_pAuctionBiddingInfoData[i].AuctionId == id)
            {
                m_pAuctionBiddingInfoData.RemoveAt(i);
                return;
            }
        }
    }

    void RemoveAuctionSellInfoByID(uint id)
    {
        for(int i =0; i < m_pAuctionSellInfoData.Count; ++i)
        {
            if(m_pAuctionSellInfoData[i].AuctionId == id)
            {
                m_pAuctionSellInfoData.RemoveAt(i);
                return;
            }
        }
    }

    public AuctionSellInfoT GetAuctionSellInfoByID(uint id)
    {
        for(int i =0; i < m_pAuctionSellInfoData.Count; ++i)
        {
            if(m_pAuctionSellInfoData[i].AuctionId == id)
            {
                return m_pAuctionSellInfoData[i];
            }
        }
        return null;
    } 

    void UpdateAuctionSellData(JObject pData)
    {
        ALFUtils.Assert(pData != null, "GameContext UpdateAuctionSellData pData = null!!");
        
        if(pData.ContainsKey("auctionSells"))
        {
            int i = 0;
            uint id = 0;
            JObject item = null;
            long serverTime = NetworkManager.GetGameServerTime().Ticks;
            JArray pArray = (JArray)pData["auctionSells"];
            AuctionSellInfoT pAuctionSellInfo = null;
            float fTicksPerSecond = (float)TimeSpan.TicksPerSecond;
            for(i= 0; i < pArray.Count; ++i)
            {
                item = (JObject)pArray[i];
                id = (uint)item["auctionId"];
                pAuctionSellInfo = GetAuctionSellInfoByID(id);

                if(pAuctionSellInfo == null)
                {
                    pAuctionSellInfo = new AuctionSellInfoT();
                    m_pAuctionSellInfoData.Add(pAuctionSellInfo);
                    pAuctionSellInfo.AuctionId = id;
                    pAuctionSellInfo.Player = (ulong)item["player"];
                    pAuctionSellInfo.Update = true;
                }

                pAuctionSellInfo.Buyer = (ulong)item["buyer"];
                if(item.ContainsKey("finalGold") && item["finalGold"].Type != JTokenType.Null)
                {
                    pAuctionSellInfo.FinalGold = (ulong)item["finalGold"];
                }

                if(item.ContainsKey("msg") && item["msg"].Type != JTokenType.Null)
                {
                    pAuctionSellInfo.Msg = (string)item["msg"];
                }
                
                pAuctionSellInfo.TEnd = (float)(NetworkManager.ConvertLocalGameTimeTick((string)item["tEnd"]) - serverTime) / fTicksPerSecond;
                pAuctionSellInfo.TExtend = (float)(NetworkManager.ConvertLocalGameTimeTick((string)item["tExtend"]) - serverTime) / fTicksPerSecond;
                pAuctionSellInfo.TExpire = (float)(NetworkManager.ConvertLocalGameTimeTick((string)item["tExpire"]) - serverTime) / fTicksPerSecond;
                pAuctionSellInfo.End = false;
                Debug.Log($"pAuctionSellInfo.TExtend-----------id:{pAuctionSellInfo.AuctionId} ----------------------------:{pAuctionSellInfo.TExtend} pAuctionSellInfo.TExpire:{pAuctionSellInfo.TExpire}");
                if(!pAuctionSellInfo.Reward)
                {
                    pAuctionSellInfo.Reward = (DateTime.Parse((string)item["tReward"]).Year > 2000);
                }
                
                Debug.Log($"pAuctionSellInfo --------pAuctionSellInfo.TEnd:{pAuctionSellInfo.TEnd} pAuctionSellInfo.TExtend:{pAuctionSellInfo.TExtend} pAuctionSellInfo.TExpire:{pAuctionSellInfo.TExpire} pAuctionSellInfo.Reward:{pAuctionSellInfo.Reward}");
            }
        }
    }

    void UpdateAuctionBidData(JObject pData)
    {
        ALFUtils.Assert(pData != null, "GameContext UpdateAuctionBidData pData = null!!");
        
        if(pData.ContainsKey("auctionBids"))
        {
            int i = 0;
            uint id = 0;
            JObject item = null;
            long serverTime = NetworkManager.GetGameServerTime().Ticks;
            JArray pArray = (JArray)pData["auctionBids"];
            float fTicksPerSecond = (float)TimeSpan.TicksPerSecond;
            AuctionBiddingInfoT pAuctionBiddingInfo = null;
            for(i= 0; i < pArray.Count; ++i)
            {
                item = (JObject)pArray[i];
                id = (uint)item["auctionId"];
                pAuctionBiddingInfo = GetAuctionBiddingInfoByID(id);
                if(pAuctionBiddingInfo == null)
                {
                    pAuctionBiddingInfo = new AuctionBiddingInfoT();
                    pAuctionBiddingInfo.Update = true;
                    m_pAuctionBiddingInfoData.Add(pAuctionBiddingInfo);
                    pAuctionBiddingInfo.AuctionId = id;

                    pAuctionBiddingInfo.Player = new PlayerT();
                    pAuctionBiddingInfo.Player.PositionFamiliars = new List<byte>();
                    pAuctionBiddingInfo.Player.Ability = new List<PlayerAbilityT>();
                    
                    for(int n =0; n < (int)E_ABILINDEX.AB_END; ++n)
                    {
                        pAuctionBiddingInfo.Player.Ability.Add(new PlayerAbilityT());
                    }
                    
                    UpdatePlayer(pAuctionBiddingInfo.Player,item);
                    if(pAuctionBiddingInfo.Player.Club <= VIRTUAL_ID)
                    {
                        pAuctionBiddingInfo.Player.Id = id;
                    }
                }
                
                pAuctionBiddingInfo.Token = (uint)item["token"];
                pAuctionBiddingInfo.Gold = (ulong)item["gold"];
                pAuctionBiddingInfo.FinalGold = (ulong)item["finalGold"];
                pAuctionBiddingInfo.FinalToken = (uint)item["finalToken"];

                pAuctionBiddingInfo.TExtend = (float)(NetworkManager.ConvertLocalGameTimeTick((string)item["tExtend"]) - serverTime) / fTicksPerSecond;
                if(!pAuctionBiddingInfo.Reward)
                {
                    pAuctionBiddingInfo.Reward = (DateTime.Parse((string)item["tReward"]).Year > 2000);
                }
                
                if(item.ContainsKey("msg") && item["msg"].Type != JTokenType.Null)
                {
                    pAuctionBiddingInfo.Msg = (string)item["msg"];
                }
            }
        }
    }
    
    void UpdateAuctionData(JObject pData)
    {
        ALFUtils.Assert(pData != null,"GameContext UpdateAuctionData pData = null!!");
        ALFUtils.Assert((pData.ContainsKey("data") && pData["data"].Type != JTokenType.Null), "GameContext UpdateAuctionData data = null!!");
        JObject data = (JObject)pData["data"];
        JArray pArray = null;
        
        if(pData.ContainsKey("auctionSells"))
        {
            UpdateAuctionSellData(pData);
        }

        if(pData.ContainsKey("auctionBids"))
        {
            UpdateAuctionBidData(pData);
        }
        
        m_pAuctionInfoData.Round = (uint)data["round"];
        m_pAuctionInfoData.Rank = (byte)data["rank"];
        m_pAuctionInfoData.TExpire = NetworkManager.ConvertLocalGameTimeTick((string)data["tExpire"]);
        m_pAuctionInfoData.List.Clear();
        
        int i = 0;
        int n = 0;
        ulong id =0;
        JObject item = null;
        pArray = (JArray)data["list"];
        bool bRemove = false;
        AuctionPlayerInfoT pAuctionPlayerInfo = null;
        for(i= 0; i < pArray.Count; ++i)
        {
            bRemove = false;
            item = (JObject)pArray[i];
            id = (ulong)item["id"];
            for(n = 0; n < m_pAuctionSellInfoData.Count; ++n)
            {
                if(id == m_pAuctionSellInfoData[n].Player)
                {
                    bRemove = true;
                    break;
                }
            }
            
            if(!bRemove)
            {
                for(n = 0; n < m_pAuctionBiddingInfoData.Count; ++n)
                {
                    if(id == m_pAuctionBiddingInfoData[n].Player.Id)
                    {
                        bRemove = true;
                        break;
                    }
                }
            }
            
            if(bRemove) continue;
            
            pAuctionPlayerInfo = new AuctionPlayerInfoT();
            pAuctionPlayerInfo.Player = new PlayerT();
            pAuctionPlayerInfo.Player.PositionFamiliars = new List<byte>();
            pAuctionPlayerInfo.Player.Ability = new List<PlayerAbilityT>();
            
            for(n =0; n < (int)E_ABILINDEX.AB_END; ++n)
            {
                pAuctionPlayerInfo.Player.Ability.Add(new PlayerAbilityT());
            }
            
            UpdatePlayer(pAuctionPlayerInfo.Player,item);
            
            pAuctionPlayerInfo.AuctionId = (uint)item["auctionId"];
            pAuctionPlayerInfo.InitialGold = (ulong)item["initialGold"];
            if(pAuctionPlayerInfo.Player.Club < VIRTUAL_ID)
            {
                pAuctionPlayerInfo.Player.Id = pAuctionPlayerInfo.AuctionId;
            }
            m_pAuctionInfoData.List.Add(pAuctionPlayerInfo);
        }
    }

    public void LeagueTodayMatch()
    {
        if(m_pGameInfo != null)
        {
            if(m_pGameInfo.LeagueTodayCount <= 0)
            {
                return;
            }

            m_pGameInfo.LeagueTodayCount -=1;
        }
    }

    public int GetLeagueTodayMatchCount()
    {
        if(m_pGameInfo != null)
        {
            return m_pGameInfo.LeagueTodayCount;
        }

        return 0;
    }

    public int GetLeagueTodayMatchMax()
    {
        if(m_pGameInfo != null)
        {
            return m_pGameInfo.LeagueTodayMax;
        }

        return 0;
    }

    void UpdateLeagueOppsData(JObject pData)
    {
        ALFUtils.Assert(pData != null,"GameContext UpdateLeagueOppsData pData = null!!");
        
        if(pData.ContainsKey("matchType") && pData["matchType"].Type != JTokenType.Null)
        {
            m_pGameInfo.MatchType = (int)pData["matchType"];
        }

        if(pData.ContainsKey("seasonNo") && pData["seasonNo"].Type != JTokenType.Null)
        {
            m_pGameInfo.SeasonNo = (uint)pData["seasonNo"];
        }
         
        if(pData.ContainsKey("opps") && pData["opps"].Type != JTokenType.Null)
        {
            JArray pJArray = (JArray)pData["opps"];
            JObject item = null;
            m_pGameInfo.LeagueTodayCount = 0;
            m_pGameInfo.LeagueTodayMax = pJArray.Count;

            for(int i =0; i < pJArray.Count; ++i)
            {
                item = (JObject)pJArray[i];
                
                if((int)item["status"] == 0)
                {
                    m_pGameInfo.LeagueTodayCount +=1;
                }
            }
        }
    }

    void CheckTimeSale(TIME_SALE_TYPE_CODE eCode)
    {
        if(eCode == TIME_SALE_TYPE_CODE.NONE || eCode == TIME_SALE_TYPE_CODE.MAX) return;

        uint current = 0;        
        int index = 0;
        switch(eCode)
        {
            case TIME_SALE_TYPE_CODE.USER_RANK:
            {
                current = GetCurrentUserRank();
            }
            break;
            case TIME_SALE_TYPE_CODE.LICENSE:
            {
                current = GetCurrentClubLicensesID();
            }
            break;
            case TIME_SALE_TYPE_CODE.OVERALL:
            {
                PlayerT pPlayer = null;
                index = GetActiveLineUpType();
                if(m_pLineupPlayerList.ContainsKey(index))
                {
                    for(int i =0; i < CONST_NUMSTARTING; ++i)
                    {
                        pPlayer = GetPlayerByID(m_pLineupPlayerList[index].Data[i]);
                        current += pPlayer.AbilityWeightSum;
                    }
                }
            }
            break;
            case TIME_SALE_TYPE_CODE.LADDER_WIN:
            {
                current = GetTotalWinInClubProfile(E_PLAYER_INFO_TYPE.my);
            }
            break;
            case TIME_SALE_TYPE_CODE.LADDER_LOSE:
            {
                current = GetTotalLoseInClubProfile(E_PLAYER_INFO_TYPE.my);
            }
            break;
        }

        TimeSaleList pTimeSaleList = GetFlatBufferData<TimeSaleList>(E_DATA_TYPE.TimeSale);
        TimeSaleItem? pTimeSaleItem = null;
        index = 0;
        while(index < pTimeSaleList.TimeSaleLength)
        {
            pTimeSaleItem = pTimeSaleList.TimeSale(index);
            if(pTimeSaleItem.Value.Type > (byte)eCode)
            {
                break;
            }

            if(pTimeSaleItem.Value.Type == (byte)eCode)
            {
                if(pTimeSaleItem.Value.Objective <= current)
                {
                    if(!m_pProductTimeSaleInfo.ContainsKey(pTimeSaleItem.Value.No))
                    {
                        JObject pJObject = null;
                        pJObject = new JObject();
                        pJObject["no"] = pTimeSaleItem.Value.No;
                        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.timeSale_put, GetNetworkAPI(E_REQUEST_ID.timeSale_put),true,true,null,pJObject);
                    }
                }
            }
            
            ++index;
        }
    }

    void UpdateClubInfoData(JObject pData)
    {
        ALFUtils.Assert(pData != null, "GameContext UpdateClubInfoData pData = null!!");
        ALFUtils.Assert((pData.ContainsKey("club") && pData["club"].Type != JTokenType.Null),"GameContext UpdateClubInfoData club = null!!");
        uint id = (uint)pData["club"]["id"];
        if(id == GetClubID())
        {
            m_pClubData?.Dispose();
            m_pClubData = ClubData.Create(pData);
        }
        else
        {
            m_pAwayClubData?.Dispose();
            m_pAwayClubData = ClubData.Create(pData);
        }
    }
    
    void UpdateMatchData(JObject pData)
    {
        ALFUtils.Assert(pData != null,"GameContext UpdateMatchData pData = null!!");
        
        m_pMatchData?.Dispose();
        m_pMatchData = MatchData.Create(pData);
    }

    public ProductFlatRateT GetProductFlatRateByProduct(uint product)
    {
        if(m_pProductFlatRateInfo.ContainsKey(product))
        {
            return m_pProductFlatRateInfo[product];
        }

        return null;
    }

    void UpdateProductFlatRate(JArray pArray)
    {
        ALFUtils.Assert(pArray != null,"GameContext UpdateProductFlatRate pArray = null!!");

        JObject pData = null;
        uint product = 0;
        uint max = 0;
        uint day = 0;
        long tUpdate =0;
        long tRefresh =0;
        
        ProductFlatRateT pProductFlatRate = null;
        long serverT = NetworkManager.GetGameServerTime().Ticks;
        for(int i = 0; i < pArray.Count; ++i)
        {
            pData = (JObject)pArray[i];
            tRefresh = NetworkManager.ConvertLocalGameTimeTick((string)pData["tRefresh"]);
            if(serverT < tRefresh)
            {
                product = (uint)pData["product"];
                max = (uint)pData["max"];
                day = (uint)pData["day"];
                tUpdate = NetworkManager.ConvertLocalGameTimeTick((string)pData["tUpdate"]);
                if(m_pProductFlatRateInfo.ContainsKey(product))
                {
                    pProductFlatRate = m_pProductFlatRateInfo[product];
                }
                else
                {
                    pProductFlatRate = new ProductFlatRateT();
                    m_pProductFlatRateInfo.Add(product,pProductFlatRate);
                }
                pProductFlatRate.Product = product;
                pProductFlatRate.Day = day;
                pProductFlatRate.Max = max;
                pProductFlatRate.TUpdate = tUpdate;
                pProductFlatRate.TRefresh = tRefresh;
            }
        }
    }

    bool UpdateProductTimeSale(JArray pArray,bool bShow)
    {
        ALFUtils.Assert(pArray != null,"GameContext UpdateProductTimeSale pArray = null!!");

        JObject pData = null;
        uint no = 0;
        float tExpire =0;
        long tExpireTime = 0;
        byte status = 0;
        TimeSaleT pTimeSale = null;
        DateTime serverTime = NetworkManager.GetGameServerTime();
        float fTicksPerSecond = (float)TimeSpan.TicksPerSecond;
        bool bNew = false;
        for(int i = 0; i < pArray.Count; ++i)
        {
            pData = (JObject)pArray[i];
            no = (uint)pData["no"];
            tExpireTime = NetworkManager.ConvertLocalGameTimeTick((string)pData["tExpire"]);
            status = (byte)pData["status"];
            tExpire = (float)(tExpireTime - serverTime.Ticks) / fTicksPerSecond;
            
            if(m_pProductTimeSaleInfo.ContainsKey(no))
            {
                pTimeSale = m_pProductTimeSaleInfo[no];
            }
            else
            {
                pTimeSale = new TimeSaleT();
                m_pProductTimeSaleInfo.Add(no,pTimeSale);
                if(!bNew && bShow)
                {
                    JObject pJObject = new JObject();
                    pJObject["msgId"] = (uint)E_REQUEST_ID.timeSale_put;
                    pJObject["no"] = no;
                    m_pResponData.Enqueue(pJObject);
                    bNew = true;
                }
            }
            pTimeSale.No = no;
            pTimeSale.TExpire = tExpire;
            pTimeSale.Status = status;
            pTimeSale.TExpireTime = tExpireTime;
        }

        return bNew;
    }

    public List<TimeSaleT> GetTimeSaleProduct()
    {
        List<TimeSaleT> list = new List<TimeSaleT>();
        var itr = m_pProductTimeSaleInfo.GetEnumerator();
        
        while(itr.MoveNext())
        {
            if(itr.Current.Value.Status == 0 && itr.Current.Value.TExpire > 0)
            {
                list.Add(itr.Current.Value);
            }
        }
        if(list.Count > 1)
        {
            list.Sort(delegate(TimeSaleT x, TimeSaleT y) { return x.TExpire.CompareTo(y.TExpire);});
        }

        return list;
    }

    void UpdateAdRewardData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateAdRewardData pArray = null!!");
        JObject pData = null;
        uint no = 0;
        uint amount = 0;
        uint amountMax = 0;
        uint item = 0;
        ulong basic_reward =0;
        ulong ad_reward =0;
        float fActiveTime = -1;
        bool bAdd = true;
        DateTime serverTime = NetworkManager.GetGameServerTime();
        AdRewardList pAdRewardList = GetFlatBufferData<AdRewardList>(E_DATA_TYPE.AdReward);
        AdRewardItem? pAdRewardItem = null;

        for(int i = 0; i < pArray.Count; ++i)
        {
            pData = (JObject)pArray[i];
            no = (uint)pData["no"];
            amount = (uint)pData["amount"];
            item = pData.ContainsKey("item") ? (uint)pData["item"] : 0;
            item = item > 0 ? item : 10;   
            basic_reward = (ulong)pData["basicReward"];
            ad_reward = (ulong)pData["adReward"];
            fActiveTime = -1;
            pAdRewardItem = pAdRewardList.AdRewardByKey(no);
            if(pAdRewardItem != null)
            {
                amountMax = pAdRewardItem.Value.MaxAmount;
            }

            bAdd = true;
            for(int n =0; n < m_pAdRewardInfo.List.Count; ++n)
            {
                if(m_pAdRewardInfo.List[n].No == no)
                {
                    m_pAdRewardInfo.List[n].Activate = false;

                    if(amount != m_pAdRewardInfo.List[n].Amount || basic_reward != m_pAdRewardInfo.List[n].BasicReward || m_pAdRewardInfo.List[n].AdReward != ad_reward)
                    {
                        fActiveTime = UnityEngine.Random.Range(pAdRewardItem.Value.CooldownMin, pAdRewardItem.Value.CooldownMax +1);
                        m_pAdRewardInfo.List[n].TStart = serverTime.Ticks + (long)(TimeSpan.TicksPerSecond * fActiveTime);
                    }
                    else
                    {
                        fActiveTime = (float)(new DateTime(m_pAdRewardInfo.List[n].TStart) - serverTime).TotalSeconds;
                    }

                    m_pAdRewardInfo.List[n].Amount = amount;
                    m_pAdRewardInfo.List[n].BasicReward = basic_reward;
                    m_pAdRewardInfo.List[n].AdReward = ad_reward;
                    m_pAdRewardInfo.List[n].MaxAmount = amountMax;
                    m_pAdRewardInfo.List[n].TActivate = fActiveTime;

                    if(no > 100)
                    {
                        m_pAdRewardInfo.List[n].Activate = true;
                    }
                    
                    m_pAdRewardInfo.List[n].Item = item;
                    bAdd = false;
                    break;
                }
            }
            
            if(bAdd)
            {
                AdRewardDataT info = new AdRewardDataT();
                info.No = no;
                info.Amount = amount;
                info.BasicReward= basic_reward;
                info.AdReward = ad_reward;
                info.Activate = no > 100;
                info.TActivate = UnityEngine.Random.Range(pAdRewardItem.Value.CooldownMin, pAdRewardItem.Value.CooldownMax +1);
                info.TStart = serverTime.Ticks + (long)(TimeSpan.TicksPerSecond * info.TActivate);
                info.MaxAmount = amountMax;
                info.Item = item;
                m_pAdRewardInfo.List.Add(info);
            }
        }
    
        SaveAdRewardData();
    }

    void UpdateAchievementsInfoData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateAchievementsInfoData pArray = null!!");
        JObject pData = null;
        uint parent = 0;
        uint level = 0;
        uint amount = 0;
        AchievementT pAchievement = null;
        for(int i = 0; i < pArray.Count; ++i)
        {
            pData = (JObject)pArray[i];
            parent = (uint)pData["parent"];
            level = (uint)pData["level"];
            amount = (uint)pData["amount"];
            if(!m_pAchievementList.ContainsKey(parent))
            {
                pAchievement = new AchievementT();
                m_pAchievementList.Add(parent,pAchievement);
            }
            else
            {
                pAchievement = m_pAchievementList[parent];
            }
            pAchievement.Parent = parent;
            pAchievement.Amount = amount;
            pAchievement.Level = level;
        }
    }

    void UpdatePassData(JObject pData)
    {
        ALFUtils.Assert(pData != null, "GameContext UpdatePassData pData = null!!");

        m_pPassInfo.No = (uint)pData["no"];
        m_pPassInfo.Level = (uint)pData["level"];
        if(m_pPassInfo.Level == 0)
        {
            for(int i =0 ; i < const_adjsutEventPass.Length; ++i)
            {
                for(int n =0 ; n < m_pUserData.AdjustEvents.Count; ++n)
                {
                    if(m_pUserData.AdjustEvents[n] == const_adjsutEventPass[i])
                    {
                        m_pUserData.AdjustEvents.RemoveAt(n);
                        break;
                    } 
                }
            }
            SaveUserData(true);
        }

        m_pPassInfo.Paid = (int)pData["paid"] == 1;
        m_pPassInfo.Amount = (uint)pData["amount"];
        m_pPassInfo.Level2 = (uint)pData["level2"];
        m_pPassInfo.TExpire = NetworkManager.ConvertLocalGameTimeTick((string)pData["tExpire"]);
    }

    void UpdateMileagesInfoData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateMileagesInfoData pArray = null!!");
        JObject pData = null;
        uint no = 0;
        uint level = 0;
        uint amount = 0;
        
        for(int i = 0; i < pArray.Count; ++i)
        {
            pData = (JObject)pArray[i];
            no = (uint)pData["no"];
            level = (uint)pData["level"];
            amount = (uint)pData["amount"];
            if(!m_pMileagesInfo.ContainsKey(no))
            {
                m_pMileagesInfo.Add(no,new MileageT());
            }
            
            m_pMileagesInfo[no].No = no;
            m_pMileagesInfo[no].Amount = amount;
            m_pMileagesInfo[no].Level = level;
        }
    }
    void UpdateEventData(JObject pJObject)
    {
        ALFUtils.Assert(pJObject != null, "GameContext UpdateEventData pJObject = null!!");
        
        var itr = pJObject.GetEnumerator();

        while(itr.MoveNext())
        {
            if(itr.Current.Key == "clubLicense")
            {
                m_pGameInfo.ClubLicense = (uint)itr.Current.Value;
            }
        }   
    }

    public int GetTotalFastRewardCount()
    {
        E_CONST_TYPE eType = E_CONST_TYPE.licenseFastRewardChance_1;
        if(m_pGameInfo != null && m_pGameInfo.ClubLicense > 0)
        {
            eType = (E_CONST_TYPE)((int)eType + m_pGameInfo.ClubLicense -1);
            return GetConstValue(eType);
        }
    
        return 3;
    }
    void UpdateProductMileageData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateProductMileageData pArray = null!!");
        JObject pData = null;
        uint no = 0;
        uint level = 0;
        uint amount = 0;

        ProductMileageT pProductMileage = null;
        for(int i = 0; i < pArray.Count; ++i)
        {
            pData = (JObject)pArray[i];
            no = (uint)pData["no"];
            level = (uint)pData["level"];
            amount = (uint)pData["amount"];
            if(!m_pProductMileagesInfo.ContainsKey(no))
            {
                pProductMileage = new ProductMileageT();
                m_pProductMileagesInfo.Add(no,pProductMileage);
            }
            else
            {
                pProductMileage = m_pProductMileagesInfo[no];
            }

            pProductMileage.No = no;
            pProductMileage.Amount = amount;
            pProductMileage.Level = level;
        }
    }

    void UpdateQuestMissionData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateQuestMissionData pArray = null!!");
        JObject pData = null;
        uint parent = 0;
        uint level = 0;
        ulong amount = 0;
        uint mission = 0;
        QuestT pQuest = null;
        for(int i = 0; i < pArray.Count; ++i)
        {
            pData = (JObject)pArray[i];
            parent = (uint)pData["parent"];
            level = (uint)pData["level"];
            amount = (ulong)pData["amount"];
            mission = (uint)pData["mission"];
            pQuest = null;
            for(int n =0; n < m_pQuestList.Count; ++n)
            {
                if(m_pQuestList[n].Mission == mission)
                {
                    pQuest = m_pQuestList[n];
                    break;
                }
            }
            if(pQuest == null)
            {
                pQuest = new QuestT();
                m_pQuestList.Add(pQuest);
            }
            
            pQuest.Parent = parent;
            pQuest.Amount = amount;
            pQuest.Level = level;
            pQuest.Mission = mission;
        }
        m_pGameInfo.QuestExpire = (TimeSpan.TicksPerDay - NetworkManager.GetGameServerTime().TimeOfDay.Ticks) / TimeSpan.TicksPerSecond;
    }

    void CheckClubLicensesNoticeIcon(MainScene pMainScene)
    {
        ClubLicenseMissionList pClubLicenseMissionList = GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
            
        MileageMissionList pMileageMissionList = GetFlatBufferData<MileageMissionList>(E_DATA_TYPE.MileageMission);
        MileageMissionItem? pMileageMissionItem = null;
        MileageT pMileage = null;
        if(m_pGameInfo.ClubLicense <= pClubLicenseMissionList.ClubLicenseMissionLength)
        {
            ClubLicensesT pClubLicenses = null;
            ClubLicenseItem? pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMissionByKey(m_pGameInfo.ClubLicense);
            ClubLicenseGroupItem? pClubLicenseGroupItem = null;
            ClubLicenseMissionItem? pClubLicenseMissionItem = null;

            int total =0;
            uint id = 0;
            for(int i = 0; i < pClubLicenseItem.Value.ListLength; ++i)
            {
                pClubLicenseGroupItem = pClubLicenseItem.Value.List(i);
                
                id = pClubLicenseItem.Value.Mileage + pClubLicenseGroupItem.Value.Type;

                pMileageMissionItem = pMileageMissionList.MileageMissionByKey(id);
                pMileage = GetMileagesData(id);
                if(pMileage != null && pMileage.Level < pMileageMissionItem.Value.ListLength)
                {
                    if( pMileageMissionItem.Value.List((int)pMileage.Level).Value.Objective <= pMileage.Amount)
                    {
                        pMainScene.SetupNoticeIcon(E_NOTICE_NODE.clubLicense,true);
                        return;
                    }
                }

                if(m_pClubLicensesList.ContainsKey(pClubLicenseItem.Value.License))
                {
                    List<ClubLicensesT> list = m_pClubLicensesList[pClubLicenseItem.Value.License];

                    for(int n =0; n < pClubLicenseGroupItem.Value.GroupLength; ++n)
                    {
                        ++total;
                        pClubLicenseMissionItem = pClubLicenseGroupItem.Value.Group(n);
                        for(int t =0; t < list.Count; ++t)
                        {
                            pClubLicenses = list[t];
                            if(pClubLicenses.Mission == pClubLicenseMissionItem.Value.Mission)
                            {
                                if(pClubLicenses.Level == 0 && pClubLicenseMissionItem.Value.Objective <= pClubLicenses.Amount)
                                {
                                    pMainScene.SetupNoticeIcon(E_NOTICE_NODE.clubLicense,true);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            pMileage = GetMileagesData(pClubLicenseItem.Value.Mileage);
            if(pMileage != null && pMileage.Level == 0)
            {
                if(pMileage.Amount >= total)
                {
                    pMainScene.SetupNoticeIcon(E_NOTICE_NODE.clubLicense,true);
                    return;
                }
            }
        }
        else
        {
            QuestMissionList pQuestMissionList = GetFlatBufferData<QuestMissionList>(E_DATA_TYPE.QuestMission);
            QuestMissionItem? pQuestMissionItem = pQuestMissionList.QuestMissionByKey(GameContext.QUSET_MISSION_ID);
            pMileage = GetMileagesData(pQuestMissionItem.Value.Mileage);
            uint objective = 0;
            uint amount = 0;
            int level = 0;
            if(pMileage != null)
            {
                level = (int)pMileage.Level;
                amount = pMileage.Amount;
            }

            pMileageMissionItem = pMileageMissionList.MileageMissionByKey(pQuestMissionItem.Value.Mileage);

            if(level < pMileageMissionItem.Value.ListLength)
            {
                objective = pMileageMissionItem.Value.List(level).Value.Objective;
            }
            else
            {
                objective = pMileageMissionItem.Value.List(pMileageMissionItem.Value.ListLength -1).Value.Objective;
            }

            if(level == 0 && amount >= objective)
            {
                pMainScene.SetupNoticeIcon(E_NOTICE_NODE.clubLicense,true);
                return;
            }
            
            QUESTMISSION.MissionItem? pMissionItem = null;
            QuestT pQuest = null;
            for(int i = 0; i < pQuestMissionItem.Value.ListLength; ++i)
            {
                pMissionItem = pQuestMissionItem.Value.List(i);
                pQuest = GetQuestData(pMissionItem.Value.Mission);
                if(pQuest != null)
                {
                    if(pQuest.Level== 0 && pMissionItem.Value.Objective <= pQuest.Amount)
                    {
                        pMainScene.SetupNoticeIcon(E_NOTICE_NODE.clubLicense,true);
                        return;
                    }
                }
            }
        }

        pMainScene.SetupNoticeIcon(E_NOTICE_NODE.clubLicense,false);
    }

    void CheckAchievementNoticeIcon(MainScene pMainScene)
    {
        AchievementT pAchievement = GetAchievementData(ACHIEVEMENT_ID);
        AchievementMissionList pAchievementMissionList = GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
        AchievementGroupItem? pAchievementGroupItem = null;
        AchievementMissionItem? pAchievementMissionItem = null;
        
        for(int i =0; i < pAchievementMissionList.AchievementMissionLength; ++i)
        {
            pAchievementGroupItem = pAchievementMissionList.AchievementMission(i);

            for(int n =0; n < pAchievementGroupItem.Value.ListLength; ++n)
            {
                pAchievementMissionItem = pAchievementGroupItem.Value.List(n);
                
                if(pAchievementMissionItem != null)
                {
                    if(pAchievement.Level < pAchievementMissionItem.Value.Mission)
                    {
                        if(pAchievement.Amount >= pAchievementMissionItem.Value.Objective)
                        {
                            pMainScene.SetupNoticeIcon(E_NOTICE_NODE.trophy,true);
                            return;
                        }
                        else
                        {
                            pMainScene.SetupNoticeIcon(E_NOTICE_NODE.trophy,false);
                            return;                    
                        }
                    }
                }                    
            }
        }

        pMainScene.SetupNoticeIcon(E_NOTICE_NODE.trophy,false);
    }

    void CheckPassMissionNoticeIcon(MainScene pMainScene)
    {
        uint no = GetCurrentPassNo();
        if(no > 0)
        {
            PassMissionList pPassMissionList = GetFlatBufferData<PassMissionList>(E_DATA_TYPE.PassMission);
            PassMissionInfo? pPassMissionInfo = pPassMissionList.PassMissionByKey(no);
            PassMissionItem? pPassMissionItem = null;
            
            uint amount = GetCurrentPassAmount();
            bool bNotice = false;
            no = GetCurrentPassLevel();
            for(int i = 0; i < pPassMissionInfo.Value.ListLength; ++i)
            {
                pPassMissionItem = pPassMissionInfo.Value.List(i);
                if(pPassMissionItem.Value.Mission > no && pPassMissionItem.Value.Objective <= amount)
                {
                    pMainScene.SetupNoticeIcon(E_NOTICE_NODE.seasonPass,true);
                    return;
                }
            }
            
            if(!bNotice && GetCurrentPassPaid())
            {
                no = GetCurrentPassLevel2();
                for(int i = 0; i < pPassMissionInfo.Value.ListLength; ++i)
                {
                    pPassMissionItem = pPassMissionInfo.Value.List(i);
                    if(pPassMissionItem.Value.Mission > no && pPassMissionItem.Value.Objective <= amount)
                    {
                        pMainScene.SetupNoticeIcon(E_NOTICE_NODE.seasonPass,true);
                        return;
                    }
                }
            }
        }

        pMainScene.SetupNoticeIcon(E_NOTICE_NODE.seasonPass,false);
    }

    void CheckMileagesNoticeIcon(MainScene pMainScene)
    {
        MileageT pMileage = GetMileagesData(FAST_REWARD_ID);
        int count = GetTotalFastRewardCount();
        if(pMileage != null)
        {
            count -= (int)pMileage.Amount; 
        }

        pMainScene.SetupNoticeIcon(E_NOTICE_NODE.fastReward,count > 0);
    }
    
    public void UpdateNoticeIcon(E_REQUEST_ID reqID,MainScene pMainScene)
    {
        if(m_pGameInfo == null || pMainScene == null) return;

        if( reqID == E_REQUEST_ID.home_get || reqID == E_REQUEST_ID.home_reset || reqID == E_REQUEST_ID.mail_delete || reqID == E_REQUEST_ID.mail_get || reqID == E_REQUEST_ID.mail_read || reqID == E_REQUEST_ID.mail_reward)
        {
            pMainScene.SetupNoticeIcon(E_NOTICE_NODE.menu,m_pGameInfo.UnreadMailCount > 0);
        }

        if(reqID == E_REQUEST_ID.home_get || reqID == E_REQUEST_ID.home_reset || reqID == E_REQUEST_ID.clubLicense_get || reqID == E_REQUEST_ID.clubLicense_put || reqID == E_REQUEST_ID.quest_get || reqID == E_REQUEST_ID.quest_reward || reqID == E_REQUEST_ID.ladder_clear)
        {
            CheckClubLicensesNoticeIcon(pMainScene);
        }

        if(reqID == E_REQUEST_ID.home_get || reqID == E_REQUEST_ID.home_reset || reqID == E_REQUEST_ID.achievement_get || reqID == E_REQUEST_ID.achievement_reward)
        {
            CheckAchievementNoticeIcon(pMainScene);
        }

        if(reqID == E_REQUEST_ID.home_get || reqID == E_REQUEST_ID.home_reset ||reqID == E_REQUEST_ID.pass_get || reqID == E_REQUEST_ID.pass_reward)
        {
            CheckPassMissionNoticeIcon(pMainScene);
        }

        if(reqID == E_REQUEST_ID.home_get || reqID == E_REQUEST_ID.home_reset ||reqID == E_REQUEST_ID.fastReward_reward )
        {
            CheckMileagesNoticeIcon(pMainScene);
        }
    }

    void UpdateclubLicensesData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateclubLicensesData pArray = null!!");

        ClubLicenseMissionList pClubLicenseMissionList = GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
        if(m_pGameInfo.ClubLicense <= pClubLicenseMissionList.ClubLicenseMissionLength)
        {
            JObject pData = null;
            uint parent = 0;
            uint level = 0;
            ulong amount = 0;
            uint mission = 0;
            ClubLicensesT pClubLicenses = null;

            for(int i = 0; i < pArray.Count; ++i)
            {
                pData = (JObject)pArray[i];
                parent = (uint)pData["parent"];
                
                level = (uint)pData["level"];
                amount = (ulong)pData["amount"];
                mission = (uint)pData["mission"];
                pClubLicenses = null;
                
                if(!m_pClubLicensesList.ContainsKey(parent))
                {
                    List<ClubLicensesT> list = new List<ClubLicensesT>();
                    pClubLicenses = new ClubLicensesT();
                    list.Add(pClubLicenses);
                    m_pClubLicensesList.Add(parent,list);
                }
                else
                {
                    List<ClubLicensesT> list = m_pClubLicensesList[parent];
                    for(int n =0; n < list.Count; ++n)
                    {
                        if(list[n].Mission == mission)
                        {
                            pClubLicenses = list[n];
                            break;
                        }
                    }

                    if(pClubLicenses == null)
                    {
                        pClubLicenses = new ClubLicensesT();
                        list.Add(pClubLicenses);
                    }
                }
                pClubLicenses.Parent = parent;
                pClubLicenses.Amount = amount;
                pClubLicenses.Level = level;
                pClubLicenses.Mission = mission;
            }
        }
    }

    public bool IsAttendReward()
    {
        for( int n = 0; n < m_pAttendInfo.Count; ++n)
        {
            if(m_pAttendInfo[n].Day > m_pAttendInfo[n].Rewarded)
            {
                return true;
            }
        }

        return false;
    }

    void UpdateAttendInfo(JArray pArray)
    {
        JObject pData = null;
        byte eType = 0;
        uint attend = 0;
        bool bAdd = false;
        AttendRewardList pAttendRewardList = GetFlatBufferData<AttendRewardList>(E_DATA_TYPE.AttendReward);

        for(int i =0; i < pArray.Count; ++i)
        {
            bAdd = true;
            pData = (JObject)pArray[i];
            attend = (uint)pData["attend"];
            if(pAttendRewardList.AttendRewardByKey(attend) == null)
            {
                continue;
            }
            eType = (byte)pData["type"];
            for( int n = 0; n < m_pAttendInfo.Count; ++n)
            {
                if(m_pAttendInfo[n].Type == eType && m_pAttendInfo[n].Attend == attend)
                {
                    m_pAttendInfo[n].Day = (uint)pData["day"];
                    m_pAttendInfo[n].Rewarded = (byte)pData["rewarded"];
                    if(m_pAttendInfo[n].Day > m_pAttendInfo[n].Rewarded)
                    {
                        JObject pJObject = new JObject();
                        pJObject["msgId"] = (uint)E_REQUEST_ID.attend_get;
                        pJObject["attendIndex"] = n;
                        pJObject["attend"] = m_pAttendInfo[n].Attend;
                        pJObject["day"] = m_pAttendInfo[n].Day;
                        pJObject["rewarded"] = m_pAttendInfo[n].Rewarded;
                        m_pResponData.Enqueue(pJObject);
                    }
                    bAdd = false;
                    break;
                }
            }

            if(bAdd)
            {
                AttendInfoT info = new AttendInfoT();
                info.Type = eType;
                info.Attend = attend;
                info.Day = (uint)pData["day"];
                info.Rewarded = (byte)pData["rewarded"];
                if(info.Day > info.Rewarded)
                {
                    JObject pJObject = new JObject();
                    pJObject["msgId"] = (uint)E_REQUEST_ID.attend_get;
                    pJObject["attendIndex"] = m_pAttendInfo.Count;
                    pJObject["day"] = info.Day;
                    pJObject["attend"] = info.Attend;
                    pJObject["rewarded"] = info.Rewarded;
                    m_pResponData.Enqueue(pJObject);
                }

                m_pAttendInfo.Add(info);
            }
        }
    }
    
    void UpdateTimeAssetsData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateTimeAssetsData pArray = null!!");

        int i = 0;
        uint no =0;
        ulong amount =0;
        JObject item = null;
        ItemInfoT pItemInfo = null;
        long serverTick = NetworkManager.GetGameServerTime().Ticks;
        ulong amountMax = (ulong)GetConstValue(E_CONST_TYPE.challengeStageMatchChanceMax);
        while(i < pArray.Count)
        {
            item = (JObject)pArray[i];
            no = (uint)item["no"];
            amount = (ulong)item["amount"];

            if(no == GameContext.CHALLENGE_TICKET_ID && amount < 5)
            {
                long tic = serverTick - NetworkManager.ConvertLocalGameTimeTick((string)item["tUpdate"]);
                long value = (long)GetConstValue(E_CONST_TYPE.challengeStageMatchChanceCooldown) * TimeSpan.TicksPerSecond;
                int count = 0;
                while(tic >= value)
                {
                    tic -= value;
                    ++count;
                }
                m_pGameInfo.TChallengeTicketCharge = (value - tic) / (float)TimeSpan.TicksPerSecond;

                amount += (ulong)count;
                if(amount >= amountMax)
                {
                    amount = amountMax;
                    m_pGameInfo.TChallengeTicketCharge = -1;
                }
            }

            if(m_pInventoryInfoList.ContainsKey(no))
            {
                m_pInventoryInfoList[no].Amount = amount;
            }
            else
            {
                pItemInfo = new ItemInfoT();
                pItemInfo.No = no;
                pItemInfo.Amount = amount;
                m_pInventoryInfoList.Add(no,pItemInfo);
            }

            ++i;
        }
    }
    void UpdateInventoryData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateInventoryData pArray = null!!");

        int i = 0;
        uint no =0;
        ulong amount =0;
        JObject item = null;
        ItemInfoT pItemInfo = null;
        
        while(i < pArray.Count)
        {
            item = (JObject)pArray[i];
            no =0;
            amount =0;
            ++i;
            if(!uint.TryParse((string)item["no"],out no))
            {
                JObject pJObject = new JObject();
                pJObject["stack"] = "GameContext:UpdateInventoryData";
                pJObject["item"] = item;
                SingleFunc.SendLog(pJObject.ToString());
                continue;
            }

            if(!ulong.TryParse((string)item["amount"],out amount))
            {
                amount =0;
            }

            if(m_pInventoryInfoList.ContainsKey(no))
            {
                m_pInventoryInfoList[no].Amount = amount;
            }
            else
            {
                pItemInfo = new ItemInfoT();
                pItemInfo.No = no;
                pItemInfo.Amount = amount;
                m_pInventoryInfoList.Add(no,pItemInfo);
            }
        }
    }

    void UpdateMailData(JArray pArray,bool bDelete)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateMailData pArray = null!!");
        int i = 0;
        int n = 0;
        ulong id =0;
        byte type = 0;
        
        if(bDelete)
        {
            m_pMailBoxInfo.Mails.Clear();
        }
        JObject mail = null;
        MailInfoT pMailInfo = null;

        while(i < pArray.Count)
        {
            pMailInfo = null;
            mail = (JObject)pArray[i];
            id = (ulong)mail["id"];
            type = (byte)mail["type"];
            
            for(n = 0; n < m_pMailBoxInfo.Mails.Count; ++n)
            {
                if(m_pMailBoxInfo.Mails[n].Id == id && m_pMailBoxInfo.Mails[n].Type == type)
                {
                    pMailInfo = m_pMailBoxInfo.Mails[n];
                    break;
                }
            }

            if(pMailInfo == null)
            {
                pMailInfo = new MailInfoT();
                pMailInfo.Id = id;
                pMailInfo.Type = type;
                pMailInfo.TSend = NetworkManager.ConvertLocalGameTimeTick((string)mail["tSend"]);
                pMailInfo.TExpire = NetworkManager.ConvertLocalGameTimeTick((string)mail["tExpire"]);
                pMailInfo.Title = (string)mail["title"];
                pMailInfo.Content = (string)mail["content"];
                m_pMailBoxInfo.Mails.Add(pMailInfo);
                pMailInfo.Rewards = new List<uint>();
                pMailInfo.Amounts = new List<ulong>();
            }

            pMailInfo.Data = (string)mail["data"];
            pMailInfo.Status = (byte)mail["status"];
            pMailInfo.Rewards.Clear();
            pMailInfo.Amounts.Clear();
            
            if(mail.ContainsKey("reward1"))
            {
                uint reward = (uint)mail["reward1"];
                if(reward > 0)
                {
                    pMailInfo.Rewards.Add(reward);
                    pMailInfo.Amounts.Add((ulong)mail["rewardAmount1"]);
                }
            }

            if(mail.ContainsKey("reward2"))
            {
                uint reward = (uint)mail["reward2"];
                if(reward > 0)
                {
                    pMailInfo.Rewards.Add(reward);
                    pMailInfo.Amounts.Add((ulong)mail["rewardAmount2"]);
                }
            }
            
            ++i;
        }
    }

    public PassT GetCurrentPassInfo()
    {
        return m_pPassInfo;
    }

    public uint GetCurrentPassNo()
    {
        return m_pPassInfo.No;
    }

    public uint GetCurrentPassAmount()
    {
        return m_pPassInfo.Amount;
    }

    public uint GetCurrentPassLevel()
    {
        return m_pPassInfo.Level;
    }
    public uint GetCurrentPassLevel2()
    {
        return m_pPassInfo.Level2;
    }

    public bool GetCurrentPassPaid()
    {
        return m_pPassInfo.Paid;
    }

    public DateTime GetCurrentPassExpireTime()
    {
        return new DateTime(m_pPassInfo.TExpire);
    }

    public bool IsCurrentPassExpire()
    {        
        return NetworkManager.GetGameServerTime().Ticks > m_pPassInfo.TExpire;
    }
//      - 50: 오늘 레더 경기를 한 횟수
//      no: 70 빠른보상

    public MileageT GetMileagesData(uint no)
    {
        if(m_pMileagesInfo.ContainsKey(no))
        {
            return m_pMileagesInfo[no];
        }
        return null;
    }

    public ProductMileageT GetProductMileageData(uint no)
    {
        if(m_pProductMileagesInfo.ContainsKey(no))
        {
            return m_pProductMileagesInfo[no];
        }
        return null;
    }

    public AchievementT GetAchievementData(uint parent)
    {
        if(m_pAchievementList.ContainsKey(parent))
        {
            return m_pAchievementList[parent];
        }
        return null;
    }

    public List<QuestT> GetQuestDataList()
    {
        return m_pQuestList;
    }

    public QuestT GetQuestData(uint mission)
    {
        for(int i =0; i < m_pQuestList.Count; ++i)
        {
            if(m_pQuestList[i].Mission == mission)
            {
                return m_pQuestList[i];
            }
        }
        return null;
    }
    public ClubLicensesT GetClubLicensesData(uint parent,uint mission)
    {
        if(m_pClubLicensesList.ContainsKey(parent))
        {
            for(int i =0; i < m_pClubLicensesList[parent].Count; ++i)
            {
                if(m_pClubLicensesList[parent][i].Mission == mission)
                {
                    return m_pClubLicensesList[parent][i];
                }
            }
        }
        return null;
    }

    public bool IsAllClearClubLicenses()
    {
        ClubLicenseMissionList pClubLicenseMissionList = GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
        ClubLicenseItem? pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMission(pClubLicenseMissionList.ClubLicenseMissionLength-1);
        if(m_pGameInfo.ClubLicense > pClubLicenseItem.Value.License)
        {
            return true;
        }
        return false;
    }

    public float GetQuestMissionExpireTime()
    {
        return m_pGameInfo.QuestExpire;
    }

    public uint GetCurrentClubLicensesID()
    {
        ClubLicenseMissionList pClubLicenseMissionList = GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
        ClubLicenseItem? pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMission(pClubLicenseMissionList.ClubLicenseMissionLength -1);
        if(m_pGameInfo.ClubLicense > 0 && m_pGameInfo.ClubLicense <= pClubLicenseItem.Value.License ) 
        {
            return m_pGameInfo.ClubLicense;
        }
        else if(m_pGameInfo.ClubLicense > pClubLicenseItem.Value.License )
        {
            return pClubLicenseItem.Value.License;
        }

        MileageT data = null;

        for(int i =0; i < pClubLicenseMissionList.ClubLicenseMissionLength; ++i)
        {
            pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMission(i);
            data = GetMileagesData(pClubLicenseItem.Value.Mileage);
            if(data == null || data.Level == 0)
            {
                return pClubLicenseItem.Value.License;
            }
        }

        return pClubLicenseMissionList.ClubLicenseMission(0).Value.License;
    }

    public uint GetTodayMatchCount()
    {
        MileageT pMileage = GetMileagesData(MILEAGE_MATCH);
        if(pMileage != null)
        {
            return pMileage.Amount;
        }

        return 0;
    }

    public int GetAttendInfoIndex(byte eType)
    {
        for(int i =0; i < m_pAttendInfo.Count; ++i)
        {
            if(m_pAttendInfo[i].Type == eType)
            {
                return i;
            }
        }

        return -1;
    }
    public AttendInfoT GetAttendInfo(int index)
    {
        if(index > -1 && m_pAttendInfo.Count > index)
        {
            return m_pAttendInfo[index];
        }

        return null;
    }

    void UpdateBusinessTimeData(BusinessInfoT pBusinessInfo,string strTime)
    {
        if(string.IsNullOrEmpty(strTime)) return;
            
        BusinessList pBusinessList = GetFlatBufferData<BusinessList>(E_DATA_TYPE.Business);
        BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(pBusinessInfo.No);
        ALFUtils.Assert(pBusinessItemList != null , $"GameContext:UpdateBusinessTime pBusinessItemList = null  pBusinessInfo.No = {pBusinessInfo.No} !!!" );

        BusinessItem? pBusinessItem = pBusinessItemList.Value.List((int)pBusinessInfo.Level);
        pBusinessInfo.SkipUpdate = false;
        float totalSeconds = (float)(NetworkManager.GetGameServerTime().Ticks - NetworkManager.ConvertLocalGameTimeTick(strTime)) / (float)TimeSpan.TicksPerSecond;
        uint count = 0;
        if(totalSeconds < 0)
        {
            totalSeconds = 0;
        }
        if(pBusinessItem.Value.IncomeInterval > 0)
        {
            count = (uint)(totalSeconds / pBusinessItem.Value.IncomeInterval);
        }
        
        float temp = totalSeconds - (count * pBusinessItem.Value.IncomeInterval);

        ALFUtils.Assert(temp >= 0 ,$"--- UpdateBusinessTimeData.No:{pBusinessInfo.No} level:{pBusinessInfo.Level}");

        pBusinessInfo.RemainingTime = (float)(pBusinessItem.Value.IncomeInterval - temp);
        ulong reward = (count * pBusinessItem.Value.RewardAmount) + pBusinessInfo.Redundancy;
        if(reward > pBusinessItem.Value.MaxIncomeAmount)
        {
            reward = pBusinessItem.Value.MaxIncomeAmount;
            pBusinessInfo.RemainingTime = pBusinessItem.Value.IncomeInterval;
            pBusinessInfo.SkipUpdate = true;
        }
        pBusinessInfo.Redundancy = reward;
    }

    void UpdateBusinessData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext UpdateBusinessData pArray = null!!");
        
        JObject business = null;
        uint no =0;
        bool bEmpty = true;
        List<BusinessInfoT> updateList = null;
        int i = 0;
        int n =0;
        BusinessList pBusinessList = GetBusinessData();
        for(i = 0; i < pArray.Count; ++i)
        {
            business = (JObject)pArray[i];
            no = (uint)business["no"];
            bEmpty = true;

            if(no > 1000) // training
            {
                updateList = m_pGameInfo.TrainingInfos;
            }
            else
            {
                updateList = m_pGameInfo.BusinessInfos;
            }

            for (n =0; n < updateList.Count; ++n)
            {
                if(updateList[n].No == no)
                {
                    bEmpty = false;
                    updateList[n].Level = (uint)business["level"];
                    updateList[n].Redundancy = (uint)business["redundancy"];
                    UpdateBusinessTimeData(updateList[n],business["tReward"].ToString());
                    if(updateList[n].Building == (byte)E_BUILDING.youthClub)
                    {
                        BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(no);
                        BusinessItem? pBusinessItem = pBusinessItemList.Value.ListByKey(updateList[n].Level);
                        m_pGameInfo.YouthSlotCount = pBusinessItem.Value.YouthSlotCount;
                        m_pGameInfo.YouthCooldownTime = pBusinessItem.Value.YouthCooldownTime;
                    }
                    break;
                }
            }
            if(bEmpty)
            {
                BusinessInfoT pBusinessInfo = new BusinessInfoT();
                pBusinessInfo.No = no;
                pBusinessInfo.Level = (uint)business["level"];
                pBusinessInfo.Redundancy = (uint)business["redundancy"];
                UpdateBusinessTimeData(pBusinessInfo,business["tReward"].ToString());
                
                BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(pBusinessInfo.No);
                pBusinessInfo.Building =(byte)Enum.Parse(typeof(E_BUILDING), pBusinessItemList.Value.Building);
                updateList.Add(pBusinessInfo);
                if(pBusinessInfo.Building == (byte)E_BUILDING.youthClub)
                {
                    BusinessItem? pBusinessItem = pBusinessItemList.Value.ListByKey(pBusinessInfo.Level);
                    m_pGameInfo.YouthSlotCount = pBusinessItem.Value.YouthSlotCount;
                    m_pGameInfo.YouthCooldownTime = pBusinessItem.Value.YouthCooldownTime;
                }
            }
        }
    }

    public uint GetUseRecoverHPItemCount( ulong[] pPlayerIDs)
    {
        uint count =0;
        if(pPlayerIDs != null)
        {
            PlayerT pPlayer = null;
            for(int i =0; i < pPlayerIDs.Length; ++i)
            {
                pPlayer = GetPlayerByID(pPlayerIDs[i]);
                count += (uint)Mathf.CeilToInt((100 - pPlayer.Hp)/ 5.0f);
            }
            
            return count;
        }

        return 0;
    }

    public ulong[] GetRecoverHpPlayerIDs()
    {
        List<ulong> list = new List<ulong>();
        for(int i=0; i < m_pGameInfo.Players.Count; ++i)
        {
            if(m_pGameInfo.Players[i].Hp < 100)
            {
                list.Add(m_pGameInfo.Players[i].Id);
            }
        }

        if(list.Count > 0)
        {
            return list.ToArray();
        }
        return null;
    }

    public void UpdatePlayer(PlayerT pPlayer, JObject pData)
    {
        int n =0;        
        pPlayer.Id = (ulong)pData["id"];
        pPlayer.Status = pData.ContainsKey("status") == true ? (byte)pData["status"] : (byte)0;
        pPlayer.Club = pData.ContainsKey("club") == true ? (ulong)pData["club"] : 0;
        pPlayer.Forename = pData["forename"].ToString();
        pPlayer.Surname = pData["surname"].ToString();
        pPlayer.Nation = ALFUtils.ConvertThreeLetterNameToTwoLetterName(pData["nation"].ToString());
        pPlayer.Price = pData.ContainsKey("price") == true ? (ulong)pData["price"] : 0;
        pPlayer.RecmdLetter = pData.ContainsKey("recmdLetter") == true ? (uint)pData["recmdLetter"] : 0;
        pPlayer.Age = (byte)pData["age"]; 

        if(pPlayer.Montage == null)
        {
            pPlayer.Montage = new List<ushort>(){100,200,300,400,500,600};
        }
        if(pData.ContainsKey("montage") && pData["montage"].Type != JTokenType.Null)
        {
            JArray pMontage = JArray.Parse((string)pData["montage"]);
            if(pPlayer.Montage.Count == pMontage.Count)
            {
                for(int t =0; t < pMontage.Count; ++t)
                {
                    try{
                        pPlayer.Montage[t] = (ushort)pMontage[t]; 
                    }
                    catch
                    {

                    }
                }
            }
        }
        
        pPlayer.Height = (ushort)pData["height"]; 
        pPlayer.Hp = (byte)pData["hp"];
        if(pPlayer.Hp > 100)
        {
            pPlayer.Hp = 100;
        }
        if(pData.ContainsKey("tHp") == true)
        {
            pPlayer.THp = NetworkManager.ConvertLocalGameTimeTick((string)pData["tHp"]);
        }        
        
        pPlayer.CreateSeason = m_pGameInfo.SeasonNo;
        if(pData.ContainsKey("tCreate"))
        {
            LadderSeasonNoList pLadderSeasonNoList = GetFlatBufferData<LadderSeasonNoList>(E_DATA_TYPE.LadderSeasonNo);
            LadderSeasonNoItem? pLadderSeasonNoItem = null;
            long tCreate = NetworkManager.ConvertLocalGameTimeTick((string)pData["tCreate"]);
            for(n = 0; n < pLadderSeasonNoList.LadderSeasonNoLength; ++n)
            {
                pLadderSeasonNoItem = pLadderSeasonNoList.LadderSeasonNo(n);
                if(DateTime.Parse(pLadderSeasonNoItem.Value.TStart).Ticks <= tCreate && DateTime.Parse(pLadderSeasonNoItem.Value.TEnd).Ticks > tCreate)
                {
                    pPlayer.CreateSeason = pLadderSeasonNoItem.Value.SeasonNo;
                    break;
                }
            }
        }

        pPlayer.Position = (byte)pData["position"];
        pPlayer.PotentialTier = (byte)pData["potentialTier"];
        pPlayer.Potential = (uint)pData["potential"];

        PlayerPotentialWeightSumList pPlayerPotentialWeightSumList = GetFlatBufferData<PlayerPotentialWeightSumList>(E_DATA_TYPE.PlayerPotentialWeightSum); 
        PlayerPotentialWeightSumItem? pPlayerPotentialWeightSumItem = pPlayerPotentialWeightSumList.PlayerPotentialWeightSumByKey(pPlayer.PotentialTier);
        if(pPlayerPotentialWeightSumItem != null)
        {
            pPlayer.Potential = pPlayerPotentialWeightSumItem.Value.Min;    
        }
        
        pPlayer.AbilitySum = (uint)pData["abilitySum"];
        
        pPlayer.PositionFamiliars.Clear();
        JArray pArray = JArray.Parse(pData["positionFamiliars"].ToString());
        for(n =0; n < pArray.Count; ++n)
        {
            pPlayer.PositionFamiliars.Add((byte)pArray[n]);
        }
        
        ALFUtils.Assert(pData.ContainsKey("origin"), "no origin data!!");
        
        pArray = (JArray)pData["origin"];

        for(n =0; n < pArray.Count; ++n)
        {
            pPlayer.Ability[n].Origin = (byte)pArray[n];
            pPlayer.Ability[n].Changed = 0;
            pPlayer.Ability[n].Current = pPlayer.Ability[n].Origin;
        }
        
        if(pData.ContainsKey("changed"))
        {
            pArray = (JArray)pData["changed"];
            int temp = 0;
            for(n =0; n < pArray.Count; ++n)
            {
                pPlayer.Ability[n].Changed = (sbyte)pArray[n];
                temp = pPlayer.Ability[n].Origin + pPlayer.Ability[n].Changed;
                pPlayer.Ability[n].Current = temp > 0 ? (byte)temp : (byte)0;
            }
        }

        CalculatePlayerAbilityTier(pPlayer);
    }

    void UpdatePlayerData(JArray pArray, bool bRemove)
    {
        ALFUtils.Assert(pArray != null, "GameContext:UpdatePlayerData pArray = null!!");
        int i = 0;
        int n =0;
        JObject item = null;
        PlayerT pPlayer = null;
        ulong no = 0;
        int index = 0;
        for(i= 0; i < pArray.Count; ++i)
        {
            item = (JObject)pArray[i];
            no = (ulong)item["id"];
            index = 0;
            pPlayer = ALFUtils.BinarySearch<PlayerT>(m_pGameInfo.Players,(PlayerT d)=> { return d.Id.CompareTo(no);},ref index);
            if(pPlayer == null)
            {
                if(bRemove) continue;

                pPlayer = new PlayerT();
                pPlayer.PositionFamiliars = new List<byte>();
                pPlayer.Ability = new List<PlayerAbilityT>();
                
                for(n =0; n < (int)E_ABILINDEX.AB_END; ++n)
                {
                    pPlayer.Ability.Add(new PlayerAbilityT());
                }
                pPlayer.Id = no;
                m_pGameInfo.Players.Add(pPlayer);
            }
            else
            {
                if(bRemove)
                {
                    m_pGameInfo.Players.RemoveAt(index);
                }
            }
            UpdatePlayer(pPlayer,item);
        }

        m_pGameInfo.Players.Sort(delegate(PlayerT x, PlayerT y) { return x.Id.CompareTo(y.Id);});
    }

    public void UpdateLineupPlayerData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext:UpdateLineupPlayerData pArray = null!!");
         
        int i = 0;
        int n = 0;
        ulong id =0;
        JObject item = null;
        JArray list = null;
        LineupPlayerT pLineupPlayer = null;
        for(i= 0; i < pArray.Count; ++i)
        {
            item = (JObject)pArray[i];
            n = (int)item["type"];

            m_pGameInfo.ActiveLineup = n;
            
            if(!m_pLineupPlayerList.ContainsKey(n))
            {
                pLineupPlayer = new LineupPlayerT();
                pLineupPlayer.Data = new List<ulong>();
                m_pLineupPlayerList.Add(n,pLineupPlayer);
            }
            
            list = JArray.Parse(item["data"].ToString());
            pLineupPlayer = m_pLineupPlayerList[n];
            pLineupPlayer.Data.Clear();
            for(n =0; n < list.Count; ++n)
            {
                id =(ulong)list[n];
                if(GetPlayerByID(id) == null)
                {
                    DefaultLineupPlayer();
                    return;
                }
                pLineupPlayer.Data.Add(id);
            }
        }
    }

    void DefaultLineupPlayer()
    {
        int index = GetActiveLineUpType();
        LineupPlayerT pLineupPlayer = null;
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            pLineupPlayer = m_pLineupPlayerList[index];
        }
        else
        {
            pLineupPlayer = new LineupPlayerT();
            pLineupPlayer.Data = new List<ulong>();
            m_pLineupPlayerList.Add(index,pLineupPlayer);
        }
        int i =0;
        pLineupPlayer.Data.Clear();
        for(i =0; i < m_pGameInfo.Players.Count; ++i)
        {
            if(m_pGameInfo.Players[i].Position == (byte)E_LOCATION.LOC_GK)
            {
                pLineupPlayer.Data.Add(m_pGameInfo.Players[i].Id);
                break;
            }
        }

        for(i =0; i < m_pGameInfo.Players.Count; ++i)
        {
            if(m_pGameInfo.Players[i].Position != (byte)E_LOCATION.LOC_GK)
            {
                if(pLineupPlayer.Data.Count < GameContext.CONST_NUMPLAYER)
                {
                    pLineupPlayer.Data.Add(m_pGameInfo.Players[i].Id);
                }
                else
                {
                    break;
                }
            }
        }

        for(i =0; i < m_pGameInfo.Players.Count; ++i)
        {
            if(m_pGameInfo.Players[i].Position == (byte)E_LOCATION.LOC_GK && !pLineupPlayer.Data.Contains(m_pGameInfo.Players[i].Id))
            {
                pLineupPlayer.Data[GameContext.CONST_NUMSTARTING] = m_pGameInfo.Players[i].Id;
                break;
            }
        }

        JObject json = new JObject();
        json["type"] = index;
        JArray jArray = new JArray();
        uint total =0;
        PlayerT pPlayer = null;

        for(i = 0; i < pLineupPlayer.Data.Count; ++i)
        {
            pPlayer = GetPlayerByID(pLineupPlayer.Data[i]);
            if( i < GameContext.CONST_NUMSTARTING)
            {
                total += pPlayer.AbilityWeightSum;
            }
            jArray.Add(pLineupPlayer.Data[i]);
        }
        
        json["data"] = jArray.ToString(Newtonsoft.Json.Formatting.None);
        json["squadPower"] = total;
        json["playerCount"] = GetTotalPlayerCount();
        json["totalValue"] = GetTotalPlayerValue(0);
        json["countQualified"] = GetTotalPlayerNAbilityTier(null,true);
        json["avgAge"] = GetPlayerAvgAge(null,true);

        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.lineup_put, GetNetworkAPI(E_REQUEST_ID.lineup_put),false,true,null,json);
    }

    public int IsSubstitutionPlayerIndex(int index, ulong id)
    {
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            LineupPlayerT pLineupPlayer = m_pLineupPlayerList[index];
            for(int n =0; n < pLineupPlayer.Data.Count; ++n)
            {
                if(pLineupPlayer.Data[n] == id)
                {
                    return n;
                }
            }
        }

        return -1;
    }

    public void DefaultLineup(int index,ref List<ulong> list)
    {
        if(index == 0 )
        {
            for(int i =0; i < m_pGameInfo.Players.Count; ++i)
            {
                if(m_pGameInfo.Players[i].Position == (int)E_LOCATION.LOC_GK)
                {
                    list[index] = m_pGameInfo.Players[i].Id;
                    return;
                }
            }
        }
        
        if(index == GameContext.CONST_NUMSTARTING)
        {
            for(int i =0; i < m_pGameInfo.Players.Count; ++i)
            {
                if(m_pGameInfo.Players[i].Position == (int)E_LOCATION.LOC_GK && !list.Contains(m_pGameInfo.Players[i].Id))
                {
                    list[index] = m_pGameInfo.Players[i].Id;
                    return;
                }
            }
        }

        for(int i =0; i < m_pGameInfo.Players.Count; ++i)
        {
            if(m_pGameInfo.Players[i].Position != (int)E_LOCATION.LOC_GK && !list.Contains(m_pGameInfo.Players[i].Id))
            {
                list[index] = m_pGameInfo.Players[i].Id;
                return;
            }
        }        
    }

    public int GetLineupPlayerIndex(int index,ulong id)
    {
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            LineupPlayerT pLineupPlayer = m_pLineupPlayerList[index];
            for(int n =0; n < pLineupPlayer.Data.Count; ++n)
            {
                if(pLineupPlayer.Data[n] == id)
                {
                    return n;
                }
            }
        }

        return GameContext.CONST_NUMPLAYER;
    }

    public byte IsLineupPlayer(int index,ulong id)
    {
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            LineupPlayerT pLineupPlayer = m_pLineupPlayerList[index];
            for(int n =0; n < pLineupPlayer.Data.Count; ++n)
            {
                if(pLineupPlayer.Data[n] == id)
                {
                    return (byte)(n < CONST_NUMSTARTING ? 2 : 1);
                }
            }
        }

        return 0;
    }

    public bool IsPositionUpgrade(byte positionValue)
    {
        if(positionValue > 0 && positionValue < 100)
        {
            PositionFamiliarList pPositionFamiliarList = GetFlatBufferData<PositionFamiliarList>(E_DATA_TYPE.PositionFamilar);
            return GetPositionSkill() >= pPositionFamiliarList.PositionFamiliarByKey(positionValue).Value.Point;
        }
        
        return false;
    }

    public int GetPositionUpgradeCount(byte positionValue,ref uint totalCost)
    {
        if(positionValue > 0 && positionValue < 100)
        {
            ulong totalPoint = GetPositionSkill();
            PositionFamiliarList pPositionFamiliarList = GetFlatBufferData<PositionFamiliarList>(E_DATA_TYPE.PositionFamilar);
            byte count = 0;
            uint cost = 0;
            while(true)
            {
                if(positionValue < 100)
                {
                    cost = pPositionFamiliarList.PositionFamiliarByKey(positionValue).Value.Point;
                    if(totalPoint >= cost)
                    {
                        totalCost += cost;
                        totalPoint -= cost;
                        ++count;
                        if(count == 10)
                        {
                            return (int)count;
                        }
                    }
                    else
                    {
                        return (int)count;
                    }
                    ++positionValue;
                }
                else
                {
                    return (int)count;
                }
            }
        }
        
        return 0;
    }

    public uint GetPositionUpgradeCost(byte positionValue)
    {
        if(positionValue > 0 && positionValue < 100)
        {
            PositionFamiliarList pPositionFamiliarList = GetFlatBufferData<PositionFamiliarList>(E_DATA_TYPE.PositionFamilar);
            return pPositionFamiliarList.PositionFamiliarByKey(positionValue).Value.Point;
        }
        
        return 0;
    }

    public List<string> GetClubNationCodeList()
    {
        return m_pClubNationCodeList;
    }

    public List<string> GetPlayerNationCodeList()
    {
        return m_pPlayerNationCodeList;
    }

    public PlayerNationalityItem? GetPlayerNationDataByCode(PLAYERNATIONALITY.NATION_CODE code)
    {
        PlayerNationalityList pPlayerNationalityList = GetFlatBufferData<PlayerNationalityList>(E_DATA_TYPE.PlayerNationality);
        PlayerNationalityItem? pPlayerNationalityItem = pPlayerNationalityList.PlayerNationalityByKey(code.ToString());
        return pPlayerNationalityItem;
    }

    public ClubNationalityItem? GetClubNationalityDataByCode(CLUBNATIONALITY.CLUB_NATION_CODE code)
    {
        ClubNationalityList pClubNationalityList = GetFlatBufferData<ClubNationalityList>(E_DATA_TYPE.ClubNationality);
        ClubNationalityItem? pClubNationalityItem = pClubNationalityList.ClubNationalityByKey(code.ToString());
        return pClubNationalityItem;
    }

    public bool IsPlayerAbilityUp(PlayerT pPlayerT,E_TRAINING_TYPE eType)
    {
        E_ABILINDEX i = (E_ABILINDEX)((int)E_ABILINDEX.AB_ATT_PASS + ((int)eType * 6));
        E_ABILINDEX end = i + 6;
        uint total = 0;
        while(i < end)
        {
            if(pPlayerT.Ability[(int)i].Current > 0)
            {
                total += (uint)(pPlayerT.Ability[(int)i].Current);
            }
            ++i;
        }

        if(total < 600)
        {            
            PlayerPotentialWeightSumList pPlayerPotentialWeightSumList = GetFlatBufferData<PlayerPotentialWeightSumList>(E_DATA_TYPE.PlayerPotentialWeightSum); 
            PlayerPotentialWeightSumItem? pPlayerPotentialWeightSumItem = pPlayerPotentialWeightSumList.PlayerPotentialWeightSumByKey(pPlayerT.PotentialTier);
            
            return pPlayerPotentialWeightSumItem.Value.Min > pPlayerT.AbilityWeightSum;
        }


        return false;
    }

    public void CalculatePlayerAbilityTier(PlayerT pPlayerT)
    {
        if(pPlayerT != null )
        {
            pPlayerT.AbilityTier = 0;
            
            PlayerAbilityWeightConversionList pPlayerAbilityWeightConversionList = GetFlatBufferData<PlayerAbilityWeightConversionList>(E_DATA_TYPE.PlayerAbilityWeightConversion); 
            PlayerAbilityWeightConversionItem? pPlayerAbilityWeightConversionItem = null;

            pPlayerT.AbilityWeightSum = 0;
            int count =0;
            uint total =0;
            E_ABILINDEX n = E_ABILINDEX.AB_ATT_PASS;
            while(n < E_ABILINDEX.AB_END)
            {
                total = 0;
                for(count =0; count < 6; ++count)
                {
                    if(pPlayerT.Ability[(int)n].Current > 0)
                    {
                        total += (uint)(pPlayerT.Ability[(int)n].Current);
                    }
                    
                    ++n;
                }
                
                if(total > 0)
                {
                    pPlayerAbilityWeightConversionItem = pPlayerAbilityWeightConversionList.PlayerAbilityWeightConversionByKey(total);
                    pPlayerT.AbilityWeightSum += pPlayerAbilityWeightConversionItem.Value.Weight;
                }
            }
            
            int left =0;
            byte tier = 0;
            int middle = 0;
            PlayerPotentialWeightSumList pPlayerPotentialWeightSumList = GetFlatBufferData<PlayerPotentialWeightSumList>(E_DATA_TYPE.PlayerPotentialWeightSum); 
            PlayerPotentialWeightSumItem? pPlayerPotentialWeightSumItem = null;
            
            int right = pPlayerPotentialWeightSumList.PlayerPotentialWeightSumLength - 1;

            for (left = 0; left <= right; )
            {
                middle = (left + right) / 2;
                pPlayerPotentialWeightSumItem = pPlayerPotentialWeightSumList.PlayerPotentialWeightSum(middle);
                if (pPlayerT.AbilityWeightSum >= pPlayerPotentialWeightSumItem.Value.Min && pPlayerT.AbilityWeightSum <= pPlayerPotentialWeightSumItem.Value.Max)
                {
                    tier = pPlayerPotentialWeightSumItem.Value.No;
                    pPlayerT.AbilityTier = tier;
                    return;
                }
                else if (pPlayerT.AbilityWeightSum < pPlayerPotentialWeightSumItem.Value.Min)
                {
                    right = middle - 1; 
                } 
                else
                {
                    left = middle + 1; 
                }
            }

            pPlayerT.AbilityTier = pPlayerPotentialWeightSumList.PlayerPotentialWeightSum(left).Value.No;
        }
    }

    public void SaveTacticsInfoName(int type, string name)
    {
        for(int n =0; n < m_pUserData.TacticsInfo.Count; ++n)
        {
            if(m_pUserData.TacticsInfo[n].Type == type)
            {
                m_pUserData.TacticsInfo[n].Name = name;
                SaveUserData(true);
                return;
            }
        }
        TacticsInfoT info = new TacticsInfoT();
        info.Type = type;
        info.Name = name;
        m_pUserData.TacticsInfo.Add(info);
        SaveUserData(true);
    }
    void UpdateTacticsData(JArray pArray)
    {
        ALFUtils.Assert(pArray != null, "GameContext:UpdateTacticsData pArray = null!!");
        
        bool bMakeDefaultTacticsData = true;

        int i = 0;
        JObject item = null;
        int index = 0;
        int n = 0;
        for(i = 0; i < pArray.Count; ++i)
        {
            bMakeDefaultTacticsData = false;

            item = (JObject)pArray[i];
            index = (int)item["type"];
            
            if(!m_pTacticsList.ContainsKey(index))
            {
                m_pTacticsList.Add(index,Newtonsoft.Json.JsonConvert.DeserializeObject<TacticsT>(item["data"].ToString())); 
            }
            else
            {
                m_pTacticsList[index] = Newtonsoft.Json.JsonConvert.DeserializeObject<TacticsT>(item["data"].ToString());
            }
            
            if(m_pTacticsList[index].Formation == null)
            {
                m_pTacticsList[index].Formation = new List<byte>(){0,1,2,4,5,11,12,14,15,22,24};
            }
            
            if(item.ContainsKey("formation"))
            {
                if(item["formation"].Type == JTokenType.Array)
                {
                    JArray pFormationArray = (JArray)item["formation"];
                    for( n =0; n < pFormationArray.Count; ++n)
                    {
                        m_pTacticsList[index].Formation[n] = (byte)pFormationArray[n];
                    }
                }
                else if(item["formation"].Type == JTokenType.String)
                {
                    List<byte> pFormation = Newtonsoft.Json.JsonConvert.DeserializeObject<List<byte>>(item["formation"].ToString());
                    if(pFormation != null)
                    {
                        m_pTacticsList[index].Formation = pFormation;
                    }
                }

                ALFUtils.Assert(m_pTacticsList[index].Formation != null, "m_pTacticsList[index].Formation = null !!!");
            }
        }
        
        if(bMakeDefaultTacticsData)
        {
            DefaultTacticsData();
            DefaultSuggestionPlayerData();
        }
    }

    public void DefaultSuggestionPlayerData()
    {
        LineupPlayerT pLineupPlayer = m_pLineupPlayerList[1];
        SuggestionPlayer(ref pLineupPlayer, m_pTacticsList[1].Formation.ToList());
    }

    void DefaultTacticsData()
    {
        int i =0;

        if(!m_pTacticsList.ContainsKey(1))
        {
            m_pTacticsList.Add(1,new TacticsT());
        }
        
        TacticsT pTactics = m_pTacticsList[1];
        pTactics.Formation = new List<byte>(){ (byte)E_LOCATION.LOC_GK, (byte)E_LOCATION.LOC_DL, (byte)E_LOCATION.LOC_DCL, (byte)E_LOCATION.LOC_DCR, (byte)E_LOCATION.LOC_DR, (byte)E_LOCATION.LOC_ML, (byte)E_LOCATION.LOC_MCL, (byte)E_LOCATION.LOC_MCR, (byte)E_LOCATION.LOC_MR, (byte)E_LOCATION.LOC_FCL, (byte)E_LOCATION.LOC_FCR};

        pTactics.TeamTactics = new List<byte>();
        pTactics.PlayerTactics = new List<PlayerTacticsT>();
        
        for(i = 0; i < (int)E_TEAM_TACTICS.TEAM_TACTICS_END; ++i)
        {
            pTactics.TeamTactics.Add(2);
        }
        pTactics.TeamTactics[(int)E_TEAM_TACTICS.TEAM_TACTICS_WIDTH] = 1;
        pTactics.TeamTactics[(int)E_TEAM_TACTICS.TEAM_TACTICS_TEMPO] = 1;

        PlayerTacticsT pPlayerTactics = null;
        for(i = 0; i < (int)E_LOCATION.LOC_END; ++i)
        {
            pPlayerTactics = new PlayerTacticsT();
            pPlayerTactics.Tactics = new List<byte>();
            for(E_PLAYER_TACTICS n = E_PLAYER_TACTICS.PLAYER_TACTICS_START; n < E_PLAYER_TACTICS.PLAYER_TACTICS_END; ++n)
            {
                pPlayerTactics.Tactics.Add(50);
            }
            
            pTactics.PlayerTactics.Add(pPlayerTactics);
        }
    }

    void CheckLastLoginData()
    {
        string id = PlayerPrefs.GetString("lcid");
        if(!string.IsNullOrEmpty(id) && m_pClubInfo.Id.ToString() == id) return;

        if(!string.IsNullOrEmpty(id))
        {
            PlayerPrefs.DeleteKey($"{id}:ml");
            PlayerPrefs.DeleteKey($"{id}:ar");
            PlayerPrefs.DeleteKey($"{id}:cp");
            PlayerPrefs.DeleteKey("md");
            PlayerPrefs.Save();
        }
    }

    void UpdateClubData(JObject data)
    {
        ALFUtils.Assert(data != null, "GameContext UpdateClubData data = null!!");

        SetClubName((string)data["name"]);
        SetClubNation(ALFUtils.ConvertThreeLetterNameToTwoLetterName((string)data["nation"]));
        SetUserAge((byte)data["age"]);
        SetUserGender((byte)data["gender"]);
        
        m_pClubInfo.TCreate = NetworkManager.ConvertLocalGameTimeTick((string)data["tCreate"]);
        m_pClubInfo.Id = (uint)data["id"];
        byte[] emblem = null;

        if(data.ContainsKey("emblem") && data["emblem"].Type != JTokenType.Null)
        {
            emblem = SingleFunc.GetMakeEmblemData((string)data["emblem"]);
        }
        else
        {
            emblem = SingleFunc.CreateRandomEmblem();
        }
        
        SetEmblem(emblem);

        if(data.ContainsKey("squadCapacity") && data["squadCapacity"].Type != JTokenType.Null)
        {
            m_pClubInfo.SquadCapacity = (uint)data["squadCapacity"];
        }
    }

    public string UpdateGameData(E_REQUEST_ID reqID, JObject data,MainScene pMainScene)
    {
        ALFUtils.Assert(data != null, "GameContext UpdateGameData data = null!!");
        if(m_pUserData == null || m_pGameInfo == null) return null;

        if(data.ContainsKey("tServer") && data["tServer"].Type != JTokenType.Null)
        {
            NetworkManager.SetGameServerTime(DateTime.Parse((string)data["tServer"]));
        }

        if( reqID == E_REQUEST_ID.lineup_put )
        {
            CheckTimeSale(TIME_SALE_TYPE_CODE.OVERALL);
            return null;
        }
        
        if( reqID == E_REQUEST_ID.league_getHistory || reqID == E_REQUEST_ID.challengeStage_getMatches || reqID == E_REQUEST_ID.matchStats_getPlayers || reqID == E_REQUEST_ID.matchStats_getSummary || reqID == E_REQUEST_ID.matchStats_list || reqID == E_REQUEST_ID.playerStats_top100 || reqID == E_REQUEST_ID.playerStats_seasonStats || reqID == E_REQUEST_ID.club_top100 || reqID == E_REQUEST_ID.player_profile || reqID == E_REQUEST_ID.challengeStage_getStandings || reqID == E_REQUEST_ID.challengeStage_searchOpps || reqID == E_REQUEST_ID.league_getStandings || reqID == E_REQUEST_ID.league_getLeaders )
        {
            return null;
        }

        if(reqID == E_REQUEST_ID.league_getTodayFixture)
        {
            UpdateLeagueOppsData(data);
            return null;
        }
        else if(reqID == E_REQUEST_ID.club_profile)
        {
            UpdateClubInfoData(data);
            return null;
        }
        else if(reqID == E_REQUEST_ID.auction_get)
        {
            UpdateAuctionData(data);
            SendCompleteAuctionBid();
            SendMiscarriedAuctionSell();
            ConnectCurrentAuction();
            return null;
        }
        else if(reqID == E_REQUEST_ID.ladder_try || reqID == E_REQUEST_ID.league_try )
        {
            if(data.ContainsKey("target") && data["target"].Type != JTokenType.Null)
            {
                UpdateMatchData((JObject)data["target"]);
                return null;
            }
            
            return "target = null";
        }
        else if(reqID == E_REQUEST_ID.challengeStage_try )
        {
            if(data.ContainsKey("target") && data["target"].Type != JTokenType.Null)
            {
                UpdateMatchData((JObject)data["target"]);
                m_pMatchData.SetStanding((data.ContainsKey("standing") && data["standing"].Type != JTokenType.Null) ? (int)data["standing"] : 0);

                if(data.ContainsKey("timeAssets") && data["timeAssets"].Type != JTokenType.Null)
                {
                    UpdateTimeAssetsData((JArray)data["timeAssets"]);
                }

                return null;
            }
            
            return "challengeStage = null";
        }
        else if(reqID == E_REQUEST_ID.iap_purchase)
        {
            CurrentTransactionFinish();
        }
        else if(reqID == E_REQUEST_ID.iap_reserve)
        {
            return null;
        }

        SaveCachePlayerData(reqID,data);
        if(data.ContainsKey("auctionSells") && data["auctionSells"].Type != JTokenType.Null)
        {
            UpdateAuctionSellData(data);
            SendMiscarriedAuctionSell();
            ConnectAuctionSell();
        }

        if(data.ContainsKey("auctionBids") && data["auctionBids"].Type != JTokenType.Null)
        {
            UpdateAuctionBidData(data);
            SendCompleteAuctionBid();
            ConnectAuctionBid();
        }        

        if(data.ContainsKey("events") && data["events"].Type != JTokenType.Null)
        {
            UpdateEventData((JObject)data["events"]);
        }
        
        if(data.ContainsKey("productMileages") && data["productMileages"].Type != JTokenType.Null)
        {
            UpdateProductMileageData((JArray)data["productMileages"]);
        }

        if(data.ContainsKey("mileages") && data["mileages"].Type != JTokenType.Null)
        {
            UpdateMileagesInfoData((JArray)data["mileages"]);
            
            // if(IsLicenseContentsUnlock(E_CONST_TYPE.licenseContentsUnlock_5) && IsLeagueOpen() && m_pGameInfo.MatchType == GameContext.LADDER_ID)
            if (IsJoinLeague() && m_pGameInfo.MatchType == GameContext.LADDER_ID)
            {
                m_pGameInfo.MatchType = GameContext.CHALLENGE_ID;
            }
            
            UpdateNoticeIcon(E_REQUEST_ID.clubLicense_get,pMainScene);
            if(data.ContainsKey("clubLicenseNo") && data["clubLicenseNo"].Type != JTokenType.Null)
            {
                uint clubLicense = (uint)data["clubLicenseNo"];
                if(clubLicense > 0)
                {
                    m_pGameInfo.ClubLicense = clubLicense;
                    NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.clubLicense_get, GetNetworkAPI(E_REQUEST_ID.clubLicense_get),false,true,null,null);
                }
            }
        }

        if( reqID == E_REQUEST_ID.home_get || reqID == E_REQUEST_ID.home_reset)
        {
            m_pQuestList.Clear();
            m_pClubLicensesList.Clear();

            if( reqID == E_REQUEST_ID.home_reset)
            {
                for(int i = 0; i < m_pUserData.SettingInfo.AdInfos.Count; ++i)
                {
                    if(m_pUserData.SettingInfo.AdInfos[i].No == 1 || m_pUserData.SettingInfo.AdInfos[i].No == 2)
                    {
                        m_pUserData.SettingInfo.AdInfos[i].TAdRefreshTime = -1;
                    }
                }

                Transfer pTransfer =  Director.Instance.GetActiveBaseScene<MainScene>().GetInstance<Transfer>();
                RemoveExpireTimerByUI(pTransfer);
            }

            if((data.ContainsKey("matchType") && data["matchType"].Type != JTokenType.Null))
            {
                m_pGameInfo.MatchType = (int)data["matchType"];
            }
            
            m_pGameInfo.LeagueOn = (data.ContainsKey("leagueOn") && data["leagueOn"].Type != JTokenType.Null) ? (bool)data["leagueOn"] : false;
        }

        if(data.ContainsKey("clubLicenses") && data["clubLicenses"].Type != JTokenType.Null)
        {
            UpdateclubLicensesData((JArray)data["clubLicenses"]);
            UpdateNoticeIcon(E_REQUEST_ID.clubLicense_get,pMainScene);
        }

        if(data.ContainsKey("quests") && data["quests"].Type != JTokenType.Null)
        {
            UpdateQuestMissionData((JArray)data["quests"]);
            UpdateNoticeIcon(E_REQUEST_ID.clubLicense_get,pMainScene);
        }
                
        if(data.ContainsKey("adRewards") && data["adRewards"].Type != JTokenType.Null)
        {
            UpdateAdRewardData((JArray)data["adRewards"]);
        }

        if(data.ContainsKey("pass") && data["pass"].Type != JTokenType.Null)
        {
            UpdatePassData((JObject)data["pass"]);
            
            if(reqID == E_REQUEST_ID.pass_reward)
            {
                if(m_pPassInfo.Level > 0 && m_pPassInfo.Level <= const_adjsutEventPass.Length)
                {
                    SendAdjustEvent(const_adjsutEventPass[m_pPassInfo.Level -1],true, false,-1);
                }
            }
            
            UpdateNoticeIcon(E_REQUEST_ID.pass_get,pMainScene);
        }

        if(data.ContainsKey("flatRates") && data["flatRates"].Type != JTokenType.Null)
        {
            UpdateProductFlatRate((JArray)data["flatRates"]);
        }
        
        if(data.ContainsKey("achievements") && data["achievements"].Type != JTokenType.Null)
        {
            UpdateAchievementsInfoData((JArray)data["achievements"]);
            UpdateNoticeIcon(E_REQUEST_ID.achievement_get,pMainScene);
        }

        if(data.ContainsKey("mails") && data["mails"].Type != JTokenType.Null)
        {
            if( reqID == E_REQUEST_ID.mail_delete || reqID == E_REQUEST_ID.mail_get || reqID == E_REQUEST_ID.mail_read || reqID == E_REQUEST_ID.mail_reward)
            {
                if(data.ContainsKey("amount"))
                {
                    m_pGameInfo.UnreadMailCount = (uint)data["amount"];
                }
            }
            UpdateMailData((JArray)data["mails"],(reqID == E_REQUEST_ID.mail_delete || reqID == E_REQUEST_ID.mail_reward));
        }

        if(data.ContainsKey("items") && data["items"].Type != JTokenType.Null)
        {
            UpdateInventoryData((JArray)data["items"]);
            if(!m_pInventoryInfoList.ContainsKey(GameContext.CHALLENGE_TICKET_ID))
            {
                ItemInfoT pItemInfo = new ItemInfoT();
                pItemInfo.No = GameContext.CHALLENGE_TICKET_ID;
                pItemInfo.Amount = 5;
                m_pInventoryInfoList.Add(GameContext.CHALLENGE_TICKET_ID,pItemInfo);
            }
            if(reqID != E_REQUEST_ID.home_get && reqID != E_REQUEST_ID.home_reset)
            {
                CheckTimeSale(TIME_SALE_TYPE_CODE.USER_RANK);
            }
        }

        if(data.ContainsKey("timeAssets") && data["timeAssets"].Type != JTokenType.Null)
        {
            UpdateTimeAssetsData((JArray)data["timeAssets"]);
        }

        if(data.ContainsKey("businesses") && data["businesses"].Type != JTokenType.Null)
        {
            UpdateBusinessData((JArray)data["businesses"]);
        }

        if(data.ContainsKey("seasonNo") && data["seasonNo"].Type != JTokenType.Null)
        {
            m_pGameInfo.SeasonNo = (uint)data["seasonNo"];
        }

        if(data.ContainsKey("players") && data["players"].Type != JTokenType.Null)
        {
            switch(reqID)
            {
                case E_REQUEST_ID.auction_cancel:
                case E_REQUEST_ID.auction_withdraw:
                case E_REQUEST_ID.auction_reward:
                {
                    JArray pJArray = (JArray)data["players"];
                    JObject pData = null;
                    ulong no = 0;
                    int i = 0;
                    for(int n =0; n < pJArray.Count; ++n)
                    {
                        pData = (JObject)pJArray[n];
                        no = (ulong)pData["id"];
                        i = m_pAuctionSellInfoData.Count;
                        while(i > 0)
                        {
                            --i;
                            if(m_pAuctionSellInfoData[i].Player == no)
                            {
                                m_pAuctionSellInfoData.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                break;
            }

            if(reqID != E_REQUEST_ID.auction_reward)
            {
                UpdatePlayerData((JArray)data["players"],reqID == E_REQUEST_ID.player_release);
            }            
        }

        if(data.ContainsKey("tactics") && data["tactics"].Type != JTokenType.Null)
        {
            UpdateTacticsData((JArray)data["tactics"]);
        }

        if(data.ContainsKey("lineup") && data["lineup"].Type != JTokenType.Null)
        {
            UpdateLineupPlayerData((JArray)data["lineup"]);
        }

        if(data.ContainsKey("lineupTactics") && data["lineupTactics"].Type != JTokenType.Null)
        {
            JObject item = null;
            JArray pArray = (JArray)data["lineupTactics"];
            for(int i= 0; i < pArray.Count; ++i)
            {
                item = (JObject)pArray[i];
                m_pGameInfo.ActiveLineup = (int)item["type"];
            }
        }

        if(data.ContainsKey("tSeasonExpire") && data["tSeasonExpire"].Type != JTokenType.Null)
        {
            m_pGameInfo.TSeasonExpire = NetworkManager.ConvertLocalGameTimeTick((string)data["tSeasonExpire"]);
        }

        if(data.ContainsKey("prevSeasonNo") && data["prevSeasonNo"].Type != JTokenType.Null)
        {
            m_pGameInfo.PrevSeasonNo = (uint)data["prevSeasonNo"];
        }

        if(data.ContainsKey("prevSeasonStanding") && data["prevSeasonStanding"].Type != JTokenType.Null)
        {
            m_pGameInfo.PrevSeasonStanding = (uint)data["prevSeasonStanding"];
        }

        if(data.ContainsKey("prevSeasonUserRank") && data["prevSeasonUserRank"].Type != JTokenType.Null)
        {
            m_pGameInfo.PrevSeasonUserRank = (byte)data["prevSeasonUserRank"];
        }

        if(data.ContainsKey("prevSeasonTrophy") && data["prevSeasonTrophy"].Type != JTokenType.Null)
        {
            m_pGameInfo.PrevSeasonTrophy = (uint)data["prevSeasonTrophy"];
        }

        if(data.ContainsKey("tUserRankReward") && data["tUserRankReward"].Type != JTokenType.Null)
        {
            long pDateTime = (new DateTime(2022,1,1)).Ticks;
            m_pGameInfo.UserRankReward = pDateTime >= NetworkManager.ConvertLocalGameTimeTick((string)data["tUserRankReward"]);
        }

        if(data.ContainsKey("tStandingReward") && data["tStandingReward"].Type != JTokenType.Null)
        {
            long pDateTime = (new DateTime(2022,1,1)).Ticks;
            m_pGameInfo.StandingReward = pDateTime >= NetworkManager.ConvertLocalGameTimeTick((string)data["tStandingReward"]);
        }

        if(data.ContainsKey("tutorial") && data["tutorial"].Type != JTokenType.Null)
        {
            m_pGameInfo.Tutorial = (byte)data["tutorial"];
        }

        if(data.ContainsKey("attends") && data["attends"].Type != JTokenType.Null)
        {
            UpdateAttendInfo((JArray)data["attends"]);
        }

        if(data.ContainsKey("clubHistory") && data["clubHistory"].Type != JTokenType.Null)
        {
            JObject clubHistory = (JObject)data["clubHistory"];
            if(m_pClubData == null)
            {
                m_pClubData = ClubData.CreateHistory(clubHistory);
            }
            else
            {
                m_pClubData.UpdateHistory(clubHistory);
            }
            if(reqID != E_REQUEST_ID.home_get && reqID != E_REQUEST_ID.home_reset)
            {
                CheckTimeSale(TIME_SALE_TYPE_CODE.LADDER_WIN);
                CheckTimeSale(TIME_SALE_TYPE_CODE.LADDER_LOSE);
            }
        }

        if(reqID == E_REQUEST_ID.club_upgradeCapacity && data.ContainsKey("squadCapacity") && data["squadCapacity"].Type != JTokenType.Null)
        {
            if(m_pClubInfo != null)
            {
                m_pClubInfo.SquadCapacity = (uint)data["squadCapacity"];
            }
            
        }        
        else if(reqID == E_REQUEST_ID.ladder_rewardStanding || reqID == E_REQUEST_ID.ladder_rewardUserRank || reqID == E_REQUEST_ID.mileage_reward || reqID == E_REQUEST_ID.attend_reward || reqID == E_REQUEST_ID.auction_trade || reqID == E_REQUEST_ID.auction_refund)
        {
            m_pResponData.Enqueue(data);
        }
        UpdateNoticeIcon(reqID,pMainScene);

        if(data.ContainsKey("timeSales") && data["timeSales"].Type != JTokenType.Null)
        {
            if(UpdateProductTimeSale((JArray)data["timeSales"], reqID != E_REQUEST_ID.home_get && reqID != E_REQUEST_ID.home_reset))
            {
                pMainScene?.CheckShowStartPopup();
            }
        }

        return null;
    }

    public JObject GetResponData()
    {
        if(m_pResponData.Count > 0)
        {
            return m_pResponData.Dequeue();
        }

        return null;
    }

    public ulong GetClubID()
    {
        if(m_pClubInfo != null)
        {
            return m_pClubInfo.Id;
        }
        return 0;
    }

    public ulong GetUserCustomerNo()
    {
        if(m_pUserData != null)
        {
            return m_pUserData.CustomerNo;
        }
        
        return 0;
    }

    public void SetClubNation(string clubNation)
    {
        if(m_pClubInfo != null)
        {
            m_pClubInfo.Nation = clubNation;
        }

        m_pClubData?.SetNation(clubNation);
    }

    public string GetClubNation()
    {
        return m_pClubInfo.Nation;
    }

    public byte[] GetEmblemInfo()
    {
        if(m_pClubInfo != null)
        {
            return m_pClubInfo.Emblem.ToArray();
        }

        return null;
    }

    public int GetCurrentMatchType()
    {
        if(m_pGameInfo != null)
        {
            return m_pGameInfo.MatchType;
        }

        return GameContext.LADDER_ID;
    }

    public bool IsLeagueOpen()
    {
        return m_pGameInfo.LeagueOn;
    }

    public bool CheckSameLineupData(List<ulong> list)
    {
        if(list != null && list.Count > 0)
        {
            List<ulong> temp = list.ToList();
            ulong id = temp[list.Count -1];
            temp.RemoveAt(list.Count -1);
            while(temp.Count > 0)
            {
                if(temp.Contains(id))
                {
                    return false;
                }
                
                id = temp[temp.Count -1];
                temp.RemoveAt(temp.Count -1);
            }

            return true;
        }
        return false;
    }

    public void SetEmblem(byte[] pEmblem)
    {
        if(m_pClubInfo != null)
        {
            m_pClubInfo.Emblem = pEmblem.ToList();
        }
    }
    public string GetClubName()
    {
        if(m_pClubInfo != null)
        {
            return m_pClubInfo.ClubName;
        }
        return null;
    }

    public void SetClubName(string clubName)
    {
        if(m_pClubInfo != null)
        {
            m_pClubInfo.ClubName = clubName;
        }

        m_pClubData?.SetClubName(clubName);
    }

    public void SetUserGender(byte gender)
    {
        if(m_pClubInfo != null)
        {
            m_pClubInfo.Gender = gender;
        }
    }
    public byte GetUserGender()
    {
        return m_pClubInfo.Gender;
    }

    public byte GetUserAge()
    {
        return m_pClubInfo.Age;
    }
    public void SetUserAge(byte age)
    {
        if(m_pClubInfo != null)
        {
            m_pClubInfo.Age = age;
        }
    }

    public void CreateDummyUser()
    {
        if(m_pClubInfo == null)
        {
            m_pClubInfo = new ClubInfoT();
            SetEmblem(SingleFunc.CreateRandomEmblem());
            SetClubName("");
            SetClubNation(ConvertClubNationCodeByString(System.Globalization.RegionInfo.CurrentRegion.ThreeLetterISORegionName.ToUpper(),0).ToString());
            SetUserAge(15);
            SetUserGender(2);
        }
    }

    public uint GetYouthSlotCount()
    {
        return m_pGameInfo.YouthSlotCount;
    }

    public uint GetYouthCooldownTime()
    {
        return m_pGameInfo.YouthCooldownTime;
    }

    public void GetReqSettingData()
    {
        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.settings_get, GetNetworkAPI(E_REQUEST_ID.settings_get),true,true,null,null);
    }

    public void SendReqSettingData()
    {
        JObject pItem = new JObject();
        pItem["id"] = m_pUserData.CustomerNo;
        pItem["adjust"] = Newtonsoft.Json.JsonConvert.SerializeObject(m_pUserData.AdjustEvents, Newtonsoft.Json.Formatting.None,new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore});
        JObject pJObject = new JObject();
        pJObject["data"] = pItem.ToString();
        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.settings_put, GetNetworkAPI(E_REQUEST_ID.settings_put),false,true,null,pJObject);
    }

    public bool IsAdjustEvent(string pEvent)
    {
        return m_pUserData.AdjustEvents.Contains(pEvent);        
    }

    public void SendAdjustEvent(string pEvent,bool bFirst, bool bSend,double revenue)
    {
        if(bFirst)
        {
            if(m_pUserData.AdjustEvents.Contains(pEvent)) return;
            
            m_pUserData.AdjustEvents.Add(pEvent);
        }

        SingleFunc.AdjustEvent(pEvent,revenue);
        if(bSend && NetworkManager.IsLastLogin())
        {
            SendReqSettingData();
        }

        SaveUserData(true);
    }

    public void SetSettingData(JObject pData)
    {
        ALFUtils.Assert(pData != null, "GameContext SetSettingData pData = null!!");

        if(m_pUserData == null) return;

        if(pData.ContainsKey("id"))
        {
            if(m_pUserData.CustomerNo != (ulong)pData["id"])
            {
                IBaseScene pScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
                if(pScene != null)
                {
                    Message pUI = pScene.GetInstance<Message>();
                    pUI.SetMessage("저장된 데이터 불일치!!",GetLocalizingText("MSG_BTN_OKAY"),null);
                    SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
                }
                return;
            }
        }

        if(pData.ContainsKey("data") && pData["data"].Type == JTokenType.String)
        {
            bool bSave = false;
            JObject pSetting = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>((string)pData["data"], new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
            
            if(pSetting.ContainsKey("adjust") && pSetting["adjust"].Type == JTokenType.String)
            {
                try
                {
                    m_pUserData.AdjustEvents = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>((string)pSetting["adjust"]);
                    bSave = true;
                }
                catch
                {
                    return;
                }
            }
            
            SaveUserData(bSave);
        }
    }

    public void SetGameData( JObject homeData)
    {
        ALFUtils.Assert(homeData != null, "GameContext SetUserGameData homeData = null!!");

        DateTime pTime;
        if(System.DateTime.TryParse(homeData["tServer"].ToString(),out pTime))
        {
            NetworkManager.SetGameServerTime(pTime);
            
            TimeSpan pTimeSpan;
            float tAdRefreshTime = 0;
            for( int i =0; i < m_pUserData.SettingInfo.AdInfos.Count; ++i)
            {
                tAdRefreshTime = m_pUserData.SettingInfo.AdInfos[i].TAdRefreshTime;
                if(tAdRefreshTime >= 0)
                {
                    pTimeSpan = new DateTime((long)m_pUserData.SettingInfo.AdInfos[i].TAdViewTime) - pTime;
                    tAdRefreshTime += (float)pTimeSpan.Ticks / (float)TimeSpan.TicksPerSecond;
                    if(tAdRefreshTime <= -1)
                    {
                        m_pUserData.SettingInfo.AdInfos[i].TAdViewTime = (ulong)pTime.Ticks;
                        tAdRefreshTime = -1;
                    }
                    m_pUserData.SettingInfo.AdInfos[i].TAdRefreshTime = tAdRefreshTime;
                }
            }
        }

        if(m_pClubInfo == null)
        {
            m_pClubInfo = new ClubInfoT();
            SetEmblem(SingleFunc.CreateRandomEmblem());
        }

        if(m_pGameInfo == null)
        {
            m_pGameInfo = new GameInfoT();
            m_pAuctionInfoData = new AuctionInfoT();
            m_pAuctionInfoData.List = new List<AuctionPlayerInfoT>();
            m_pAuctionBiddingInfoData = new List<AuctionBiddingInfoT>();
            m_pAuctionSellInfoData = new List<AuctionSellInfoT>();
            m_pGameInfo.TChallengeTicketCharge = -1;
            m_pMailBoxInfo.Mails = new List<MailInfoT>();
            
            MileageT pMileage = new MileageT();
            pMileage.No = 50;
            m_pMileagesInfo.Add(pMileage.No,pMileage);
            pMileage = new MileageT();
            pMileage.No = 70;
            m_pMileagesInfo.Add(pMileage.No,pMileage);
            
            m_pGameInfo.Players = new List<PlayerT>();
            m_pGameInfo.TrainingInfos = new List<BusinessInfoT>();
            m_pGameInfo.BusinessInfos = new List<BusinessInfoT>();
            AchievementT pAchievement = new AchievementT();
            pAchievement.Parent = 1;
            m_pAchievementList.Add(pAchievement.Parent,pAchievement);
        }

        UpdateClubData((JObject)homeData["club"]);
        CheckLastLoginData();
        PlayerPrefs.SetString("lcid",m_pClubInfo.Id.ToString());

        LoadCachePlayerData();
        LoadCacheAdRewardData();
        LoadCacheMatchLogData();
        
        m_pGameInfo.ActiveTactics = 1;
        m_pGameInfo.ActiveLineup = 1;
        m_pGameInfo.PrevSeasonStanding = 0;
        m_pGameInfo.PrevSeasonUserRank = 0;
        m_pGameInfo.PrevSeasonTrophy = 0;
        m_pGameInfo.UserRankReward = false;
        m_pGameInfo.StandingReward = false;
        m_pGameInfo.PrevSeasonNo = 0;
        m_pGameInfo.ClubLicense = 0;
        m_pGameInfo.UnreadMailCount = (uint)homeData["unreadMailCount"];
        UpdateGameData(E_REQUEST_ID.home_get, homeData,null);
        E_CONST_TYPE[] list = new E_CONST_TYPE[]{E_CONST_TYPE.licenseContentsUnlock_1,E_CONST_TYPE.licenseContentsUnlock_2,E_CONST_TYPE.licenseContentsUnlock_3,E_CONST_TYPE.licenseContentsUnlock_4,E_CONST_TYPE.licenseContentsUnlock_5};
        m_pLicenseContentsUnlockIDList.Clear();
        for(int i =0; i < list.Length; ++i)
        {
            if(!IsLicenseContentsUnlock(list[i]))
            {
                m_pLicenseContentsUnlockIDList.Add(list[i]);
            }
        }        
        PlayerPrefs.Save();
        RemoveCompleteAuctionBid();
    }

    public int GetCurrentChallengeTicket()
    {
        if(m_pGameInfo != null)
        {
            return (int)GetItemCountByNO(GameContext.CHALLENGE_TICKET_ID);
        }
        return GetConstValue(E_CONST_TYPE.challengeStageMatchChanceMax);
    }

    public float GetChallengeTicketChargeTime()
    {
        if(m_pGameInfo != null)
        {
            return m_pGameInfo.TChallengeTicketCharge;
        }
        return -1;
    }

    public bool IsJoinLeague()
    {
        if(m_pGameInfo == null) return false;
        return m_pGameInfo.ClubLicense > 3 && IsLeagueOpen();
    }
    
    public bool IsLicenseContentsUnlock(E_CONST_TYPE step)
    {
        if(E_CONST_TYPE.licenseContentsUnlock_1 > step) return false;

        MileageT data = GetMileagesData((uint)GetConstValue(step));
        if(data == null )
        {
            return false;
        }

        MileageMissionList pMileageMissionList = GetFlatBufferData<MileageMissionList>(E_DATA_TYPE.MileageMission);
        MileageMissionItem? pMileageMissionItem = pMileageMissionList.MileageMissionByKey(data.No);
        if(pMileageMissionItem == null)
        {
            return false;
        }

        return data.Level >= pMileageMissionItem.Value.ListLength;
    }

    public bool IsTotalQuestMissionComplete(JObject data)
    {
        uint id = (uint)data["no"];
        QuestMissionList pQuestMissionList = GetFlatBufferData<QuestMissionList>(E_DATA_TYPE.QuestMission);
        QuestMissionItem? pQuestMissionItem = null;

        for(int i =0; i < pQuestMissionList.QuestMissionLength; ++i)
        {
            pQuestMissionItem = pQuestMissionList.QuestMission(i);
            if(pQuestMissionItem.Value.Mileage == id)
            {
                JObject pJObject = new JObject();
                pJObject["msgId"] = (uint)E_REQUEST_ID.mileage_reward;
                pJObject["no"] = id;
                pJObject["Frag"] = 1;
                pJObject["LicenseComplete"] = true;
                m_pResponData.Enqueue(pJObject);
                return true;
            }
        }
        return false;
    }

    public bool IsTotalLicenseComplete(JObject data)
    {
        uint id = (uint)data["no"];
        ClubLicenseMissionList pClubLicenseMissionList = GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
        ClubLicenseItem? pClubLicenseItem = null;

        for(int i =0; i < pClubLicenseMissionList.ClubLicenseMissionLength; ++i)
        {
            pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMission(i);
            if(pClubLicenseItem.Value.Mileage == id)
            {
                JObject pJObject = new JObject();
                pJObject["msgId"] = (uint)E_REQUEST_ID.mileage_reward;
                pJObject["no"] = id;
                pJObject["Frag"] = 0;
                pJObject["LicenseComplete"] = true;
                m_pResponData.Enqueue(pJObject);
                string token = null;
                switch(id)
                {
                    case 10010: token = "abus47";break;
                    case 10020: token = "367ub8";break;
                    case 10030: token = "99xe1i";break;
                    case 10040: token = "2xd0h0";break;
                    case 10050: token = "69cwcu";break;
                    case 10060: token = "wnutkh";break;
                    case 10070: token = "vgf0dm";break;
                    case 10080: token = "toglnc";break; 
                    case 10090: token = "svuoli";break;
                    case 10100: token = "7sqtc4";break;
                }

                if(!string.IsNullOrEmpty(token))
                {
                    SendAdjustEvent(token,false,false,-1);
                }
    
                E_CONST_TYPE eType = CheckLicenseContentsUnlock();
                if(eType != E_CONST_TYPE.MAX)
                {
                    if(eType == CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_5 && !IsLeagueOpen())
                    {
                        return true;        
                    }

                    pJObject = new JObject();
                    pJObject["msgId"] = (uint)E_REQUEST_ID.mileage_reward;
                    pJObject["no"] = id;
                    pJObject["ContentsUnlock"] = true;
                    m_pResponData.Enqueue(pJObject);
                }
                
                CheckTimeSale(TIME_SALE_TYPE_CODE.LICENSE);
                return true;
            }

        }

        return false;
    }
#if !FTM_LIVE
    public void TestCode(int eTag)
    {
        JObject pJObject = new JObject();
        if(eTag == 0)
        {
            pJObject["msgId"] = (uint)E_REQUEST_ID.mileage_reward;
            pJObject["no"] = 80;
            pJObject["LicenseComplete"] = true;
            pJObject["Frag"] = 1;
            m_pResponData.Enqueue(pJObject);

            pJObject = new JObject();
            pJObject["msgId"] = (uint)E_REQUEST_ID.mileage_reward;
            pJObject["no"] = GetConstValue(CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_3);
            pJObject["ContentsUnlock"] = true;
            
            m_pResponData.Enqueue(pJObject);
        }
        else if(eTag == 2)
        {
            m_pAttendInfo[0].Rewarded = 0;
            pJObject["msgId"] = (uint)E_REQUEST_ID.attend_get;
            pJObject["attendIndex"] = 0;
            pJObject["attend"] = 1;
            pJObject["day"] = 1;
            pJObject["rewarded"] = 0;
            m_pResponData.Enqueue(pJObject);
        }
        else if(eTag == 3)
        {
            // if( reqID == E_REQUEST_ID.home_get)
        // {
        //     JArray pJArray = new JArray();
        //     JObject pJObject = new JObject();
        //     pJObject["product"] = 1101;
        //     pJObject["max"] = 28;
        //     pJObject["day"] = 28;
        //     pJObject["tUpdate"] = "2022-10-28T07:20:15Z";
        //     pJObject["tRefresh"] = "2022-11-25T15:00:00Z";
        //     pJArray.Add(pJObject);
        //     UpdateProductFlatRate(pJArray);
        // }
        // if( reqID == E_REQUEST_ID.home_get)
        // {
        //     JArray pJArray = new JArray();
        //     JObject pJObject = new JObject();
        //     pJObject["no"] = 1001;
        //     pJObject["tExpire"] = "2022-10-31T22:00:00Z";
        //     pJArray.Add(pJObject);

        //     pJObject = new JObject();
        //     pJObject["no"] = 1002;
        //     pJObject["tExpire"] = "2022-10-31T23:00:00Z";
        //     pJArray.Add(pJObject);

        //     data["timeSales"] = pJArray;
        // }

        // if(data.ContainsKey("timeSales") && data["timeSales"].Type != JTokenType.Null)
        // {
        //     if(UpdateProductTimeSale((JArray)data["timeSales"]))
        //     {
        //         SaveUserData(true);
        //         SendReqSettingData();
        //         if(pMainScene != null)
        //         {
        //             if(!pMainScene.IsShowInstance<MatchView>())
        //             {
        //                 pMainScene?.ShowStartPopup();
        //             }
        //         }
        //     }
        // }
        }
        else
        {
            JArray pJArray = new JArray();
            JObject item = null;
            
            pJObject = new JObject();
            pJObject["msgId"] = (uint)E_REQUEST_ID.ladder_rewardUserRank;
            for(int i =0; i < 4; ++i)
            {
                item = new JObject();
                item["no"] = GameContext.CASH_ID;
                item["amount"] = 2000;
                pJArray.Add(item);
            }
            pJObject["rewards"] = pJArray;
            pJObject["trophy"] = 10100;
            pJObject["prevSeasonUserRank"] = 1;
            m_pResponData.Enqueue(pJObject);


            pJObject = new JObject();
            pJArray = new JArray();
            pJObject["msgId"] = (uint)E_REQUEST_ID.ladder_rewardStanding;
            for(int i =0; i < 4; ++i)
            {
                item = new JObject();
                item["no"] = GameContext.CASH_ID;
                item["amount"] = 2000;
                pJArray.Add(item);
            }

            pJObject["rewards"] = pJArray;
            
            pJObject["prevSeasonNo"] = 10100;
            pJObject["prevSeasonStanding"] = 100;
            pJObject["prevSeasonTrophy"] = 100;
            pJObject["prevSeasonUserRank"] = 1;
            m_pResponData.Enqueue(pJObject);
        }
    }
#endif

    public E_CONST_TYPE CheckLicenseContentsUnlock()
    {
        for(int i =0; i < m_pLicenseContentsUnlockIDList.Count; ++i)
        {
            if(IsLicenseContentsUnlock(m_pLicenseContentsUnlockIDList[i]))
            {
                E_CONST_TYPE eType = m_pLicenseContentsUnlockIDList[i];
                m_pLicenseContentsUnlockIDList.RemoveAt(i);
                return eType;
            }
        }

        return E_CONST_TYPE.MAX;
    }

    public E_DATA_TYPE GetCurrentLangauge()
    {
        if(m_pUserData == null) return E_DATA_TYPE.en_US;

        return (E_DATA_TYPE)m_pUserData.SettingInfo.Lang;
    }

    public void SetCurrentLangauge(byte lang)
    {
        if(m_pUserData != null)
        {
            m_pUserData.SettingInfo.Lang = lang;
        }

#if USE_HIVE
        SingleFunc.SetGameLanguage((E_DATA_TYPE)lang);
#endif
    }

    public void SetMuteSfx (bool bMute)
    {
        if(m_pUserData != null)
        {
            m_pUserData.SettingInfo.MuteSfx = bMute;
            SaveUserData(true);
            if(m_pUserData.SettingInfo.MuteSfx)
            {
                SoundManager.Instance.AllPauseSFX();
            }
            else
            {
                SoundManager.Instance.AllUnPauseSFX();
            }
        }
    }

    public bool GetMuteSfx()
    {
        if(m_pUserData != null)
        {
            return m_pUserData.SettingInfo.MuteSfx;
        }
        return false;
    }

    public void SetMuteBgm (bool bMute)
    {
        if(m_pUserData != null)
        {
            m_pUserData.SettingInfo.MuteBgm = bMute;

            if(m_pUserData.SettingInfo.MuteBgm)
            {
                SoundManager.Instance.AllPauseBGM();
            }
            else
            {
                SoundManager.Instance.AllUnPauseBGM();
            }

            SaveUserData(true);
        }
    }

    public bool GetMuteBgm()
    {
        if(m_pUserData != null)
        {
            return m_pUserData.SettingInfo.MuteBgm;
        }
        return false;
    }

    public bool GetPush()
    {
        if(m_pUserData != null)
        {
            return m_pUserData.SettingInfo.Push;
        }
        return true;
    }
    public bool GetNightPush()
    {
        if(m_pUserData != null)
        {
            return m_pUserData.SettingInfo.Night;
        }
        return true;
    }
    
    public bool GetLocalPush()
    {
        if(m_pUserData != null)
        {
            return m_pUserData.SettingInfo.LocalPush;
        }
        return true;
    }

    public void SetPush (bool bOn)
    {
        if(m_pUserData != null && m_pUserData.SettingInfo.Push != bOn)
        {
            m_pUserData.SettingInfo.Push = bOn;
            SaveUserData(true);
        }
    }

    public void SetNightPush (bool bOn)
    {
        if(m_pUserData != null && m_pUserData.SettingInfo.Night != bOn)
        {
            m_pUserData.SettingInfo.Night = bOn;
            SaveUserData(true);
        }
    }

    public void SetLocalPush(bool bOn)
    {
        if(m_pUserData != null && m_pUserData.SettingInfo.LocalPush != bOn)
        {
            m_pUserData.SettingInfo.LocalPush = bOn;
            SaveUserData(true);
        }
    }

    public byte MatchViewMode()
    {
        if(m_pUserData != null)
        {
            return m_pUserData.SettingInfo.MatchViewMode;
        }
        return 1;
    }

    public byte MatchViewSpeed()
    {
        if(m_pUserData != null)
        {
            return m_pUserData.SettingInfo.MatchViewSpeed;
        }
        return 1;
    }

    public void SetMatchViewSpeed(byte speed)
    {
        if(m_pUserData != null && m_pUserData.SettingInfo.MatchViewSpeed != speed)
        {
            m_pUserData.SettingInfo.MatchViewSpeed = speed;
            SaveUserData(true);
        }
    }

    public void SetMatchViewMode(byte mode)
    {
        if(m_pUserData != null && m_pUserData.SettingInfo.MatchViewMode != mode)
        {
            m_pUserData.SettingInfo.MatchViewMode = mode;
            SaveUserData(true);
        }
    }

    public void Logout()
    {
        byte eCurrentLangauge = 1;
        if(m_pUserData != null)
        {
            eCurrentLangauge = m_pUserData.SettingInfo.Lang;
            m_pUserData.SettingInfo.AdInfos?.Clear();
            m_pUserData.TacticsInfo?.Clear();
            m_pUserData.AdjustEvents?.Clear();
            m_pUserData = null;
        }

        if(m_pGameInfo != null)
        {
            m_pGameInfo.Players?.Clear();
            m_pGameInfo.BusinessInfos?.Clear();
            m_pGameInfo.TrainingInfos?.Clear();
            m_pGameInfo.Players = null;
            m_pGameInfo.BusinessInfos = null;
            m_pGameInfo.TrainingInfos = null;
            m_pGameInfo = null;
        }
        if(m_pClubInfo != null)
        {
            m_pClubInfo = null;
        }

        if(m_pMatchData != null)
        {
            m_pMatchData.Dispose();
            m_pMatchData = null;
        }
        if(m_pCacheLineupPlayerData != null)
        {
            m_pCacheLineupPlayerData.Data?.Clear();
            m_pCacheLineupPlayerData.Data = null;
            m_pCacheLineupPlayerData = null;
        }

        m_pLineupPlayerList?.Clear();
        m_pTacticsList?.Clear();
        m_pInventoryInfoList?.Clear();
        m_pMailBoxInfo.Mails?.Clear();
        m_pMailBoxInfo.Mails = null;
        m_pMileagesInfo?.Clear();
        m_pProductFlatRateInfo?.Clear();
        m_pProductMileagesInfo?.Clear();
        m_pProductTimeSaleInfo?.Clear();
        m_pPassInfo.No = 0;
        m_pPassInfo.Paid = false;
        m_pPassInfo.Amount = 0;
        m_pPassInfo.Level = 0;
        m_pPassInfo.Level2 = 0;
        m_pPassInfo.TExpire = 0;
        if(m_pAdRewardInfo != null)
        {
            m_pAdRewardInfo.List?.Clear();
            m_pAdRewardInfo.List = null;
            m_pAdRewardInfo = null;
        }
        m_pAttendInfo?.Clear();
        m_pMatchLogRecords = null;
        m_pResponData?.Clear();

        if(m_pAwayClubData != null)
        {
            m_pAwayClubData = null;
        }
        if(m_pClubData != null)
        {
            m_pClubData = null;
        }
        if(m_pCachePlayerData != null)
        {
            m_pCachePlayerData = null;
        }

        if(m_pCachePlayerData != null)
        {
            m_pCachePlayerData = null;
        }        
    
        m_pLastMatchRewardList?.Clear();
        m_pAchievementList?.Clear();
        m_pClubLicensesList?.Clear();
        m_pQuestList?.Clear();
        ClearAllExpireTime();
        m_pLicenseContentsUnlockIDList?.Clear();
        m_pAuctionInfoData = null;
        LeaveCurrentAuction();

        if(m_pAuctionBiddingInfoData != null)
        {
            m_pAuctionBiddingInfoData.Clear();
            m_pAuctionBiddingInfoData = null;
        }

        if(m_pAuctionSellInfoData != null)
        {
            m_pAuctionSellInfoData.Clear();
            m_pAuctionSellInfoData = null;
        }
    
        m_pRestoreInfoList.List?.Clear();
        m_pRewardADCallback = null;
        
        PlayerPrefs.DeleteAll();
        CreateUserData();
        SetCurrentLangauge(eCurrentLangauge);
        SaveUserData(true);
    }

    void CreateUserData()
    {
        m_pUserData = new UserDataT();
        m_pUserData.SettingInfo = new SettingInfoT();
        m_pUserData.TacticsInfo = new List<TacticsInfoT>();
        m_pUserData.SettingInfo.AdInfos = new List<RefreshADTimeT>();
        m_pUserData.AdjustEvents = new List<string>();
    

        // string strCulture = CultureInfo.CurrentCulture.Name.Replace('-','_');
    
        // E_DATA_TYPE eType = E_DATA_TYPE.en_US;
        // if(!Enum.TryParse(strCulture,out eType))
        // {
        //     eType = E_DATA_TYPE.en_US;
        // }
        SetCurrentLangauge((byte)E_DATA_TYPE.ko_KR);
    }

    public void SetUserData(JObject data)
    {
        ALFUtils.Assert( (data != null && data.Count > 0), "GameContext SetUserData data null or empty!! ");
        
        string token = (string)data["tServer"];
        DateTime pTime;
        if(System.DateTime.TryParse(token,out pTime))
        {
            NetworkManager.SetGameServerTime(pTime);
        }
        else
        {
            ALFUtils.Assert( false, $"GameContext SetUserData System.DateTime.TryParse error strServerTime = {token}");
        }

        token = (string)data["tExpire"];
        if(System.DateTime.TryParse(token,out pTime))
        {
            NetworkManager.SetExpireServerAccessTokenTime(pTime);
        }
        else
        {
            ALFUtils.Assert( false, $"GameContext SetUserData System.DateTime.TryParse error strExpire = {token}");
        }

        NetworkManager.SetServerAccessToken((string)data["accessToken"]);
        
        m_pUserData.CustomerNo = (ulong)data["customerNo"];
        SaveUserData(true);
    }

    void LoadRestoreInfo()
    {
        if(m_pUserData == null) return;
        
        string rs = PlayerPrefs.GetString($"{m_pUserData.CustomerNo}:rs");
        if (!string.IsNullOrEmpty(rs))
        {
            try
            {
                m_pRestoreInfoList = RestoreInfo.GetRootAsRestoreInfo(new ByteBuffer(Convert.FromBase64String(rs))).UnPack();
            }
            catch
            {
                m_pRestoreInfoList = new RestoreInfoT();
                m_pRestoreInfoList.List = new List<RestorePurchaseT>();
            }
        }
        else
        {
            if(m_pRestoreInfoList == null)
            {
                m_pRestoreInfoList = new RestoreInfoT();
            }

            m_pRestoreInfoList.List = new List<RestorePurchaseT>();
        }
    }
    
    public bool LoadUserData()
    {
        string ud = PlayerPrefs.GetString("ud");
        if (string.IsNullOrEmpty(ud))
        {
            string temp = PlayerPrefs.GetString("uuid");
            int eCurrentSocialProvider = PlayerPrefs.GetInt("LLSP");
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("uuid",temp);
            PlayerPrefs.SetInt("LLSP",eCurrentSocialProvider);
            PlayerPrefs.Save();
            CreateUserData();
            LoadRestoreInfo();
            return false;
        }
        
        byte[] bytes = Convert.FromBase64String(ud);
        ByteBuffer bb = new ByteBuffer(bytes);
    
        try
        {
            m_pUserData = USERDATA.UserData.GetRootAsUserData(bb).UnPack();

            if(m_pUserData.SettingInfo.MuteSfx)
            {
                SoundManager.Instance.AllPauseSFX();
            }
            else
            {
                SoundManager.Instance.AllUnPauseSFX();
            }

            if(m_pUserData.SettingInfo.MuteBgm)
            {
                SoundManager.Instance.AllPauseBGM();
            }
            else
            {
                SoundManager.Instance.AllUnPauseBGM();
            }

#if USE_HIVE        
            SingleFunc.SetGameLanguage((E_DATA_TYPE)m_pUserData.SettingInfo.Lang);
#endif      
        }
        catch{
            string temp = PlayerPrefs.GetString("uuid");
            int eCurrentSocialProvider = PlayerPrefs.GetInt("LLSP");
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("uuid",temp);
            PlayerPrefs.SetInt("LLSP",eCurrentSocialProvider);
            PlayerPrefs.Save();
            CreateUserData();
            LoadRestoreInfo();
            return false;
        }

        LoadRestoreInfo();
        return true;
    }

    void LoadCacheMatchLogData()
    {
        string ml = PlayerPrefs.GetString($"{m_pClubInfo.Id}:ml");
        if (!string.IsNullOrEmpty(ml))
        {
            byte[] bytes = Convert.FromBase64String(ml);
            ByteBuffer bb = new ByteBuffer(bytes);
        
            try
            {
                m_pMatchLogRecords = STATISTICSDATA.MatchLog.GetRootAsMatchLog(bb).UnPack();
            }
            catch{
                PlayerPrefs.DeleteKey($"{m_pClubInfo.Id}:ml");
            }
        }
        if(m_pMatchLogRecords == null)
        {
            m_pMatchLogRecords = new MatchLogT();
            m_pMatchLogRecords.List = new List<RecordT>();
        }
    }

    public AuctionBiddingInfoT GetAuctionBiddingInfoByIPlayerID(ulong id)
    {
        for(int i =0; i < m_pAuctionBiddingInfoData.Count; ++i)
        {
            if(m_pAuctionBiddingInfoData[i].Player.Id == id)
            {
                return m_pAuctionBiddingInfoData[i];
            }
        }
        return null;
    }

    public AuctionBiddingInfoT GetAuctionBiddingInfoByID(uint id)
    {
        for(int i =0; i < m_pAuctionBiddingInfoData.Count; ++i)
        {
            if(m_pAuctionBiddingInfoData[i].AuctionId == id)
            {
                return m_pAuctionBiddingInfoData[i];
            }
        }
        return null;
    }

    public List<PlayerT> GetAuctionBiddingPlayerInfoList()
    {
        List<PlayerT> list = new List<PlayerT>();
        for(int i =0; i < m_pAuctionBiddingInfoData.Count; ++i)
        {
            list.Add(m_pAuctionBiddingInfoData[i].Player);
        }
        
        return list;
    }

    public int GetCurrentBiddingPlayerCount()
    {
        int count = 0;
        for(int i =0; i < m_pAuctionBiddingInfoData.Count; ++i)
        {
            if(!m_pAuctionBiddingInfoData[i].Reward)
            {
                ++count;
            }
        }
        return count;
    }

    public List<PlayerT> GetAuctionPlayerInfoList()
    {
        List<PlayerT> list = new List<PlayerT>();
        bool bAdd = true;
        for(int i =0; i < m_pAuctionInfoData.List.Count; ++i)
        {
            bAdd = true;
            for(int n =0; n < m_pAuctionBiddingInfoData.Count; ++n)
            {
                if(m_pAuctionBiddingInfoData[n].Player.Id == m_pAuctionInfoData.List[i].Player.Id)
                {
                    bAdd = false;
                    break;
                }
            }
            if(bAdd)
            {
                list.Add(m_pAuctionInfoData.List[i].Player);
            }
        }
        
        return list;
    }

    public long GetTotalPlayerValue(long addPrice)
    {
        long total = 0;

        for(int i =0; i < m_pGameInfo.Players.Count; ++i)
        {
            total += (long)m_pGameInfo.Players[i].Price;
        }

        total = total + addPrice;
        return total;
    }

    public uint GetTotalPlayerNAbilityTier(PlayerT pPlayer,bool bAdd)
    {
        ClubLicenseMissionList pClubLicenseMissionList = GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
        ClubLicenseItem? pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMissionByKey(GetCurrentClubLicensesID());
        ClubLicenseGroupItem? pClubLicenseGroupItem = null;
        ClubLicenseMissionItem? pClubLicenseMissionItem = null;
        int i =0;
        int n =0;
        byte tier = 0;
        for(i =0; i < pClubLicenseItem.Value.ListLength; ++i)
        {
            pClubLicenseGroupItem = pClubLicenseItem.Value.List(i);
            for(n =0; n < pClubLicenseGroupItem.Value.GroupLength; ++n)
            {
                pClubLicenseMissionItem = pClubLicenseGroupItem.Value.Group(n);
                if(pClubLicenseMissionItem.Value.Event == GameContext.PLAYET_N_ABILITY_TIER_ID)
                {
                    tier = (byte)pClubLicenseMissionItem.Value.EventCondition;
                    break;
                }
            }

            if(tier > 0)
            {
                break;
            }
        }
        
        if(tier <= 0) return 0;
        uint total = 0;
        for(i =0; i < m_pGameInfo.Players.Count; ++i)
        {
            if(pPlayer != null && !bAdd && m_pGameInfo.Players[i].Id ==pPlayer.Id )
            {
                continue;
            }

            if(m_pGameInfo.Players[i].AbilityTier >= tier)
            {
                total += 1;
            }
        }

        if(pPlayer != null && pPlayer.AbilityTier >= tier)
        {
            total += 1;
        }
        return total;
    }

    public void SetPlayerHP(STATISTICSDATA.StatisticsPlayersT pStatisticsPlayers)
    {
        PlayerT pPlayer = null;
        // DateTime serverTime = NetworkManager.GetGameServerTime().ToString("yyyy-MM-ddTHH:mm:ss.fff");
        long serverTick= NetworkManager.GetGameServerTime().Ticks;
        // string serverTime = NetworkManager.GetGameServerTime().ToString("yyyy-MM-dd HH:mm:ss");
        
        for(int i =0; i < pStatisticsPlayers.Players.Count; ++i)
        {
            pPlayer = GetPlayerByID(pStatisticsPlayers.Players[i].PlayerId);
            if(pPlayer != null)
            {
                pPlayer.Hp = pStatisticsPlayers.Players[i].Hp;
                pPlayer.THp = serverTick;
            }
        } 
    }

    public void AddMatchLogData(RecordT pData)
    {
        if(pData == null) return;

        RecordT pRecord = null;
        if(m_pMatchLogRecords.List.Count > 0)
        {
            int index = 0;
            pRecord = ALFUtils.BinarySearch<RecordT>(m_pMatchLogRecords.List,(RecordT d)=> { return d.Id.CompareTo(pData.Id);},ref index);
        }
        
        if(pRecord == null)
        {
            while(m_pMatchLogRecords.List.Count > 49)
            {
                m_pMatchLogRecords.List.RemoveAt(0);
            }
            m_pMatchLogRecords.List.Add(pData);
            if(m_pMatchLogRecords.List.Count > 2)
            {
                m_pMatchLogRecords.List = m_pMatchLogRecords.List.OrderByDescending(x => x.Id).ToList();
            }
            
            PlayerPrefs.SetString($"{m_pClubInfo.Id}:ml" ,Convert.ToBase64String(m_pMatchLogRecords.SerializeToBinary()));
            PlayerPrefs.Save();
        }
    }

    public RecordT GetMatchLogData(ulong id)
    {
        int index = 0;
        return ALFUtils.BinarySearch<RecordT>(m_pMatchLogRecords.List,(RecordT d)=> { return d.Id.CompareTo(id);},ref index);
    }

    public byte GetLastLoginSocialProvider()
    {
        return (byte)PlayerPrefs.GetInt("LLSP",(int)ALF.LOGIN.LoginManager.E_SOCIAL_PROVIDER.guest);
    }

    public string GetLastLoginSocialID()
    {
        return PlayerPrefs.GetString("uuid",null);
    }

    public bool IsLastLogin()
    {
        return !string.IsNullOrEmpty(GetLastLoginSocialID());
    }

    public bool IsGetPrevSeasonReward()
    {
        return m_pGameInfo.PrevSeasonStanding > 0;
    }
    public uint GetPrevSeasonNo()
    {
        LadderSeasonNoList pLadderSeasonNoList = GetFlatBufferData<LadderSeasonNoList>(E_DATA_TYPE.LadderSeasonNo);
        LadderSeasonNoItem? pLadderSeasonNoItem = pLadderSeasonNoList.LadderSeasonNoByKey(m_pGameInfo.SeasonNo);
        if(pLadderSeasonNoItem != null && pLadderSeasonNoItem.Value.Id > 1)
        {
            pLadderSeasonNoItem = pLadderSeasonNoList.LadderSeasonNo((int)(pLadderSeasonNoItem.Value.Id -2));
            return pLadderSeasonNoItem.Value.SeasonNo;
        }
        return m_pGameInfo.SeasonNo +1;
    }

    public uint GetCurrentSeasonYear()
    {
        return m_pGameInfo.SeasonNo / 100;
    }

    public uint GetCurrentSeasonCount()
    {
        return m_pGameInfo.SeasonNo % 100;
    }

    public uint GetCurrentSeasonNo()
    {
        return m_pGameInfo.SeasonNo;
    }

    public DateTime GetCurrentSeasonExpireTime()
    {
        return new DateTime(m_pGameInfo.TSeasonExpire);
    }

    public float GetCurrentSeasonExpireRemainTime()
    {
        return (float)(m_pGameInfo.TSeasonExpire - NetworkManager.GetGameServerTime().Ticks) / (float)TimeSpan.TicksPerSecond;
    }

    public void SetAdViewTime(uint id)
    {
        ulong time = (ulong)NetworkManager.GetGameServerTime().Ticks;

        for(int i = 0; i < m_pUserData.SettingInfo.AdInfos.Count; ++i)
        {
            if(m_pUserData.SettingInfo.AdInfos[i].No == id)
            {
                m_pUserData.SettingInfo.AdInfos[i].TAdViewTime = time;
                return;
            }
        }

        RefreshADTimeT pRefreshADTime = new RefreshADTimeT();
        pRefreshADTime.No = id;
        pRefreshADTime.TAdViewTime = time;
        m_pUserData.SettingInfo.AdInfos.Add(pRefreshADTime);
    }

    public void SetAdRefreshTime(uint id,float tick)
    {
        for(int i = 0; i < m_pUserData.SettingInfo.AdInfos.Count; ++i)
        {
            if(m_pUserData.SettingInfo.AdInfos[i].No == id)
            {
                m_pUserData.SettingInfo.AdInfos[i].TAdRefreshTime = tick;
                return;
            }
        }

        RefreshADTimeT pRefreshADTime = new RefreshADTimeT();
        pRefreshADTime.No = id;
        pRefreshADTime.TAdRefreshTime = tick;
        m_pUserData.SettingInfo.AdInfos.Add(pRefreshADTime);
    }

    public uint GetUnreadMailCount()
    {
        return m_pGameInfo.UnreadMailCount;
    }

    public float GetAdRefreshTime(uint id)
    {
        for(int i = 0; i < m_pUserData.SettingInfo.AdInfos.Count; ++i)
        {
            if(m_pUserData.SettingInfo.AdInfos[i].No == id)
            {
                return m_pUserData.SettingInfo.AdInfos[i].TAdRefreshTime;
            }
        }

        return -1;
    }

    public int GetLeagueUpStanding(int iMatchType)
    {
        if(iMatchType == GameContext.LEAGUE1_ID)
        {
            return GetConstValue(E_CONST_TYPE.league_12_promotion_teamCutLine);
        }
        else if(iMatchType == GameContext.LEAGUE2_ID)
        {
            return GetConstValue(E_CONST_TYPE.league_13_promotion_teamCutLine);
        }
        else if(iMatchType == GameContext.LEAGUE3_ID)
        {
            return GetConstValue(E_CONST_TYPE.league_14_promotion_teamCutLine);
        }
        else if(iMatchType == GameContext.LEAGUE4_ID)
        {
            return GetConstValue(E_CONST_TYPE.league_15_promotion_teamCutLine);
        }
        else
        {
            return 0;
        }
    }

    public int GetLeagueDownStanding(int iMatchType)
    {
        if(iMatchType == GameContext.LEAGUE1_ID)
        {
            return GetConstValue(E_CONST_TYPE.league_12_relegation_teamCutLine);
        }
        else if(iMatchType == GameContext.LEAGUE2_ID)
        {
            return GetConstValue(E_CONST_TYPE.league_13_relegation_teamCutLine);
        }
        else if(iMatchType == GameContext.LEAGUE3_ID)
        {
            return GetConstValue(E_CONST_TYPE.league_14_relegation_teamCutLine);
        }
        else if(iMatchType == GameContext.LEAGUE4_ID)
        {
            return GetConstValue(E_CONST_TYPE.league_15_relegation_teamCutLine);
        }
        else
        {
            return 0;
        }
    }

    public int GetConstValue(E_CONST_TYPE eConstType)
    {
        return constValueList[eConstType];
    }

    public byte GetCurrentUserRank()
    {
        return (byte)GetItemCountByNO(GameContext.RANK_ITEM_ID);
    }
    public uint GetCurrentUserTrophy()
    {
        return (uint)GetItemCountByNO(GameContext.TROPHY_ITEM_ID);
    }

    // public void SetFormationByIndex(byte[] list,int index)
    // {
    //     if(m_pTacticsList.ContainsKey(index)) 
    //     {
    //         m_pTacticsList[index].Formation.Clear();
    //         for(int i =0; i < list.Length; ++i)
    //         {
    //             m_pTacticsList[index].Formation.Add(list[i]);
    //         }
    //     }
    // }

    public void UpdateLineupPlayerHP(RecordT pRecord)
    {
        if(pRecord == null) return;

        if(GetCurrentUserRank() <= GetConstValue(E_CONST_TYPE.beginnerBuffUserRank))
        {
            int index = GetActiveLineUpType();
            PlayerT pPlayer = null;
            for(int i =0; i < m_pLineupPlayerList[index].Data.Count; ++i)
            {
                pPlayer = GetPlayerByID( m_pLineupPlayerList[index].Data[i]);
                pRecord.StatisticsRecord.StatisticsPlayers[0].Players[i].Hp = pPlayer.Hp;
            }
        }
    }

    public void CloneTacticsDataByIndex(int index,ref TacticsT pTactics)
    {
        if(m_pTacticsList.ContainsKey(index)) 
        {
            SingleFunc.CloneTacticsDataByIndex(m_pTacticsList[index],ref pTactics);
        }
    }

    public List<TacticsInfoT> GetTacticsInfoList()
    {
        return m_pUserData.TacticsInfo;
    }

    public List<PlayerT> GetMatchAwayTotalPlayerList()
    {
        return m_pMatchData.GetTotalPlayerList();
    }

    public uint GetMatchTeamTotalPlayerAbility()
    {
        return m_pMatchData.GetTotalPlayerAbility();
    }

    public ulong GetMatchAwayID()
    {
        return m_pMatchData.GetID();
    }
    
    public byte GetMatchAwayRank()
    {
        return m_pMatchData.GetRank();
    }

    public uint GetMatchAwayTrophy()
    {
        return m_pMatchData.GetTrophy();
    }

    public int GetMatchAwayStanding()
    {
        return m_pMatchData.GetStanding();
    }

    public string GetMatchAwayClubName()
    {
        return m_pMatchData.GetClubName();
    }
    public byte[] GetMatchAwayFormationArray()
    {
        return m_pMatchData.GetFormationList().ToArray();
    }
    public byte[] GetFormationArrayByIndex(int index)
    {
        if(m_pTacticsList.ContainsKey(index)) 
        {
            return m_pTacticsList[index].Formation.ToArray();
        }
            
        return null;
    }

    public List<byte> GetFormationByIndex(int index)
    {
        if(m_pTacticsList.ContainsKey(index)) 
        {
            return m_pTacticsList[index].Formation;
        }
            
        return null;
    }

    public void SetActiveTacticsType(int type )
    {
        m_pGameInfo.ActiveTactics = type;
    }

    public int GetActiveTacticsType()
    {
        return m_pGameInfo.ActiveTactics;
    }

    public int GetActiveLineUpType()
    {
        return m_pGameInfo.ActiveLineup;
    }

    public PlayerT GetMatchAwayPlayerByID( ulong id)
    {
        return m_pMatchData.GetPlayerByID(id);
    }

    public PlayerT GetAwayClubPlayerByID( ulong id)
    {
        return m_pAwayClubData.GetPlayerByID(id);
    }

    public PlayerT GetPlayerByID( ulong id)
    {
        if(m_pGameInfo == null) return null;

        int index =0;
        return ALFUtils.BinarySearch<PlayerT>(m_pGameInfo.Players,(PlayerT d)=> { return d.Id.CompareTo(id);},ref index);
    }

    public E_LOCATION ConvertPositionByIndex(int p1)
    {
        int t =-1;
        int n =0;
        int c =0;
        for(n =0; n < p1+1; ++n)
        {
            c = n % 3;
            if(c == 0)
            {
                t +=1;
            }
            else if(c == 2)
            {
                t +=1;
            }
        }
         
        return (E_LOCATION)(p1 +t);
    }
    public int ConvertPositionByTag(E_LOCATION p1)
    {
        return const_displayLocationConvertList[(int)p1];
    }

    public string ConvertFormationByTag(E_LOCATION p1)
    {
        if(p1 == E_LOCATION.LOC_GK) return "GK";
        if(p1 < E_LOCATION.LOC_DMCL || p1 == E_LOCATION.LOC_DMR ) return "DF";
        if(p1 < E_LOCATION.LOC_AML || p1 == E_LOCATION.LOC_AMCL || p1 == E_LOCATION.LOC_AMCC || p1 == E_LOCATION.LOC_AMCR ) return "DF";

        return "FW";
    }

    public TeamDataT MakeMatchTeamData(int iMatchType)
    {
        float value = 1;
        if(iMatchType == GameContext.LADDER_ID)
        {
            byte currentRank = GetCurrentUserRank();
            if(currentRank <= GetConstValue(E_CONST_TYPE.beginnerBuffUserRank))
            {
                switch(currentRank)
                {
                    case 1: value = 1f - (GetConstValue(E_CONST_TYPE.beginnerBuffOpponentAbilityDecreaseRate_1) / 10000.0f); break;
                    case 2: value = 1f - (GetConstValue(E_CONST_TYPE.beginnerBuffOpponentAbilityDecreaseRate_2) / 10000.0f); break;
                    case 3: value = 1f - (GetConstValue(E_CONST_TYPE.beginnerBuffOpponentAbilityDecreaseRate_3) / 10000.0f); break;
                    case 4: value = 1f - (GetConstValue(E_CONST_TYPE.beginnerBuffOpponentAbilityDecreaseRate_4) / 10000.0f); break;
                    case 5: value = 1f - (GetConstValue(E_CONST_TYPE.beginnerBuffOpponentAbilityDecreaseRate_5) / 10000.0f); break;
                }
            }
        }
    
        return m_pMatchData.MakeTeamData(value);
    }

    public byte[] GetMatchTeamEmblemData()
    {
        return m_pMatchData.GetEmblemData();
    }

    public TeamDataT MakeTeamData(int index,int iLineup)
    {
        if(!m_pTacticsList.ContainsKey(index) || !m_pLineupPlayerList.ContainsKey(iLineup))
        {
            return null;
        }

        TeamDataT pTeamData = new TeamDataT();
        pTeamData.PlayerData = new List<PlayerDataT>();
        TacticsT pTactics = new TacticsT();
        SingleFunc.CloneTacticsDataByIndex(m_pTacticsList[index],ref pTactics);
        pTeamData.Tactics = pTactics;
        pTeamData.TeamName = GetClubName();
        pTeamData.TeamColor = new List<ushort>(){0,0};
        pTeamData.LineUp = new List<int>();
        pTeamData.TeamWorkTotal = 100;
        PlayerDataT pPlayerData = null;
        int n = 0;
        int i = 0;
        PlayerT pPlayer = null;
        for(i = 0; i < m_pLineupPlayerList[iLineup].Data.Count; ++i)
        {
            pPlayerData = new PlayerDataT();
            pPlayerData.Ability = new List<byte>();
            pPlayer = GetPlayerByID(m_pLineupPlayerList[iLineup].Data[i]);
            for(n = 0; n < pPlayer.Ability.Count; ++n )
            {
                pPlayerData.Ability.Add(pPlayer.Ability[n].Current > 0 ? (byte)pPlayer.Ability[n].Current : (byte)0);
            }
            
            pPlayerData.PosAbil = new List<float>();
            for(n = 0; n < pPlayer.PositionFamiliars.Count; ++n )
            {
                pPlayerData.PosAbil.Add(pPlayer.PositionFamiliars[n]);
            }
            pPlayerData.Talent = new List<byte>();
            for(n = 0; n < (int)E_TALENT.TALENT_END; ++n )
            {
                pPlayerData.Talent.Add(50); // 매치엔진에서 기본값으로 설정
            }
            
            pPlayerData.PlayerName = $"{pPlayer.Forename[0]}. {pPlayer.Surname}";
            pPlayerData.ExperienceLevel = 5;  // 매치엔진에서 기본값으로 설정
            pPlayerData.ExperienceExp = 50;  // 매치엔진에서 기본값으로 설정
            pPlayerData.Hp = pPlayer.Hp;
            pPlayerData.Age = pPlayer.Age;
            pPlayerData.IsValidSlot = true;
            pPlayerData.PlayerId = pPlayer.Id;
            pPlayerData.Injury = new InjuryResultT();
            pPlayerData.PlayerSkill = new List<PlayerSkillT>(){new PlayerSkillT(),new PlayerSkillT()};
            pTeamData.PlayerData.Add(pPlayerData);
            pTeamData.LineUp.Add(i);
        }

        return pTeamData;
    }

    public TacticsT GetTacticsDataByIndex(int index)
    {
        if(m_pTacticsList.ContainsKey(index))
        {
            return m_pTacticsList[index];
        }

        return null;
    }

    void UpdatePlayerHP(MainScene pScene)
    {
        long serverTime = NetworkManager.GetGameServerTime().Ticks;
        int hp = 0;
        long tick = 0;
        bool bUpdate = false;
        for(int i =0; i < m_pGameInfo.Players.Count; ++i)
        {
            if(m_pGameInfo.Players[i].Hp < 100)
            {
                tick = 0;
                hp = 0;
                tick = serverTime - m_pGameInfo.Players[i].THp;
                
                while(tick >= TimeSpan.TicksPerMinute)
                {
                    tick -= TimeSpan.TicksPerMinute;
                    hp += GetConstValue(E_CONST_TYPE.playerHpRecoveryPerMinute);
                }
                
                if(hp > 0)
                {
                    bUpdate = true;
                    m_pGameInfo.Players[i].THp = serverTime - tick;
                    hp += m_pGameInfo.Players[i].Hp;
                    if(hp >= 100)
                    {
                        hp = 100;
                        m_pGameInfo.Players[i].THp = serverTime;
                    }
                    m_pGameInfo.Players[i].Hp = (byte)hp;
                }
            }
        }
        if(bUpdate)
        {
            pScene?.UpdateMatchGauge();
        }
    }

    public uint GetLineupTotalHP()
    {
        int index = GetActiveLineUpType();
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            uint total =0;
            PlayerT pPlayer = null;
            for(int i =0; i < m_pLineupPlayerList[index].Data.Count; ++i)
            {
                pPlayer = GetPlayerByID( m_pLineupPlayerList[index].Data[i]);
                ALFUtils.Assert(pPlayer != null, string.Format("id :{0} = null!!",m_pLineupPlayerList[index].Data[i]));
                total += pPlayer.Hp;
            }

            return total;
        }
        
        return 0;
    }

    public List<ulong> GetLineupPlayerIdListByIndex(int index)
    {
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            return m_pLineupPlayerList[index].Data;
        }
        
        return null;
    }

    public void ChangeLineupPlayerIdByIndex(int index,int i,ulong id)
    {
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            if(i > -1 && i < m_pLineupPlayerList[index].Data.Count)
            {
                m_pLineupPlayerList[index].Data[i] = id;
            }
        }
    }

    public ulong GetLineupPlayerIdByIndex(int index,int i)
    {
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            if(i > -1 && i < m_pLineupPlayerList[index].Data.Count)
            {
                return m_pLineupPlayerList[index].Data[i];
            }
        }
        
        return 0;
    }

    public bool CheckChangeLineupDataByIndex(int index, List<ulong> data)
    {
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            for(int i =0; i < data.Count; ++i)
            {
                if(m_pLineupPlayerList[index].Data[i] != data[i])
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CheckChangeFormationDataByIndex(int index, TacticsT data)
    {
        if(m_pTacticsList.ContainsKey(index))
        {
            int i =0;
            for(i =0; i < data.Formation.Count; ++i)
            {
                if(m_pTacticsList[index].Formation[i] != data.Formation[i])
                {
                    return true;
                }
            }            
        }

        return false;
    }

    public bool ChangeTacticsDataByIndex(int index, TacticsT data)
    {
        if(m_pTacticsList.ContainsKey(index))
        {
            int i =0;
            for(i =0; i < data.Formation.Count; ++i)
            {
                if(m_pTacticsList[index].Formation[i] != data.Formation[i])
                {
                    return true;
                }
            }

            for(i =0; i < data.TeamTactics.Count; ++i)
            {
                if(m_pTacticsList[index].TeamTactics[i] != data.TeamTactics[i])
                {
                    return true;
                }
            }
            
            int n =0;
            for(i =0; i < data.PlayerTactics.Count; ++i)
            {
                for( n =0; n < m_pTacticsList[index].PlayerTactics[i].Tactics.Count; ++n)
                {
                    if(m_pTacticsList[index].PlayerTactics[i].Tactics[n] != data.PlayerTactics[i].Tactics[n])
                    {
                        return true;
                    }
                }
            }            
        }

        return false;
    }

    public bool CheckChangeTacticsDataByIndex(int index, TacticsT data)
    {
        if(m_pTacticsList.ContainsKey(index))
        {
            int i =0;
            for(i =0; i < data.Formation.Count; ++i)
            {
                if(m_pTacticsList[index].Formation[i] != data.Formation[i])
                {
                    return true;
                }
            }

            for(i =0; i < data.TeamTactics.Count; ++i)
            {
                if(m_pTacticsList[index].TeamTactics[i] != data.TeamTactics[i])
                {
                    return true;
                }
            }
            
            int n =0;
            for(i =0; i < data.PlayerTactics.Count; ++i)
            {
                for( n =0; n < m_pTacticsList[index].PlayerTactics[i].Tactics.Count; ++n)
                {
                    if(m_pTacticsList[index].PlayerTactics[i].Tactics[n] != data.PlayerTactics[i].Tactics[n])
                    {
                        return true;
                    }
                }
            }            
        }

        return false;
    }
    public string GetDisplayLocationName(int index)
    {
        if(index > -1 && index < const_displayLocationList.Length)
        {
            return const_displayLocationList[index];
        }
        
        return "";
    }

    public string GetDisplayBackFormationCardByLocationIndex(int index)
    {
        if(index > -1 && index < const_displayBackCardFormationList.Length)
        {
            return const_displayBackCardFormationList[index];
        }
        
        return "";
    }

    public string GetDisplayCardFormationByLocationName(string locationName)
    {
        switch(locationName)
        {
            case "GK": 
            return "GK";
            case "LB": 
            case "CB": 
            case "RB": 
            case "LWB":
            case "RWB":
            return "DF";
            case "CDM": 
            case "LM": 
            case "CM": 
            case "RM":
            case "CAM":
            return "MF";
        }
        
        return "FW";
    }

    public void SuggestionPlayer(ref LineupPlayerT pLineupPlayer,List<byte> formation)
    {
        int i =0;
        int c = 0;

        List<float[]> tempList = new List<float[]>();
        float[] tempValue = null;
        for(i =0; i < m_pGameInfo.Players.Count; ++i)
        {
            if(m_pGameInfo.Players[i].Status != 0) continue;
            
            tempValue = new float[1+ m_pGameInfo.Players[i].PositionFamiliars.Count];
            tempValue[0] = i;
            for( c = 0; c < m_pGameInfo.Players[i].PositionFamiliars.Count; ++c)
            {
                tempValue[1+c] = (float)(m_pGameInfo.Players[i].AbilityTier * (m_pGameInfo.Players[i].PositionFamiliars[c] * 0.01f));
            }
            tempList.Add(tempValue);
        }

        List<float[]> sortedList = null;

        c = 0;
        int n =0;
        int position = 0;
        int count = 0;
        bool bNext = false;
        
        while(formation.Count > 0 )
        {
            position = ConvertPositionByTag((E_LOCATION)formation[c]);
            bNext = true;

            sortedList = tempList.OrderByDescending(x => x[1+position] ).ToList();

            for(i =0; i < sortedList.Count; ++i)
            {
                if(sortedList[i][1+position] > 0)
                {
                    pLineupPlayer.Data[count] = m_pGameInfo.Players[(int)sortedList[i][0]].Id;
                    formation.RemoveAt(c);
                    for(n =0; n < tempList.Count; ++n)
                    {
                        if(tempList[n][0] == sortedList[i][0])
                        {
                            tempList.RemoveAt(n);
                            break;
                        }
                    }
                    ++count;
                    bNext = false;
                    sortedList.RemoveAt(i);
                    break;
                }
            }
            
            c = bNext ? c+1 : 0;

            if(c >= formation.Count || tempList.Count == 0)
            {
                break;
            }
        }

        for( i =0; i < tempList.Count; ++i)
        {
            for( c =3; c < tempList[i].Length; ++c)
            {
                if(tempList[i][2] < tempList[i][c])
                {
                    tempList[i][2] = tempList[i][c];
                }
            }
        }
        
        if(formation.Count > 0)
        {
            for( i =0; i < formation.Count; ++i)
            {
                position = ConvertPositionByTag((E_LOCATION)formation[i]);
                ALFUtils.Assert(position != 0, " SuggestionPlayer position = 0 !!!!");
                for(n =0; n < sortedList.Count; ++n)
                {
                    if(sortedList[n][1] == 0)
                    {
                        pLineupPlayer.Data[count] = m_pGameInfo.Players[(int)sortedList[n][0]].Id;
                        for(i =0; i < tempList.Count; ++i)
                        {
                            if(tempList[i][0] == sortedList[n][0])
                            {
                                tempList.RemoveAt(i);
                                break;
                            }
                        }
                        sortedList.RemoveAt(n);
                        ++count;
                        break;
                    }
                }
            }
        }

        sortedList = tempList.OrderByDescending(x => x[1]).ToList();
        for(n =0; n < sortedList.Count; ++n)
        {
            if(sortedList[n][1] > 0)
            {
                pLineupPlayer.Data[count] = m_pGameInfo.Players[(int)sortedList[n][0]].Id;
                ++count;
                for(i =0; i < tempList.Count; ++i)
                {
                    if(tempList[i][0] == pLineupPlayer.Data[count])
                    {
                        tempList.RemoveAt(i);
                        break;
                    }
                }
                break;
            }
        }

        sortedList = tempList.OrderByDescending(x => x[2]).ToList();

        while( count < CONST_NUMPLAYER)
        {
            for(n =0; n < sortedList.Count; ++n)
            {
                if(sortedList[n][1] == 0)
                {
                    pLineupPlayer.Data[count] = m_pGameInfo.Players[(int)sortedList[n][0]].Id;
                    sortedList.RemoveAt(n);
                    for(i =0; i < tempList.Count; ++i)
                    {
                        if(tempList[i][0] == pLineupPlayer.Data[count])
                        {
                            tempList.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                }
            }
            ++count;
        }
    }

    public byte GetPlayerAge(PlayerT pPlayer)
    {
        if(pPlayer == null)
        {
            return 0;
        }

        byte age = pPlayer.Age;
        LadderSeasonNoList pLadderSeasonNoList = GetFlatBufferData<LadderSeasonNoList>(E_DATA_TYPE.LadderSeasonNo);
        LadderSeasonNoItem? pLadderSeasonNoItem = pLadderSeasonNoList.LadderSeasonNoByKey(m_pGameInfo.SeasonNo);
        if(pLadderSeasonNoItem != null)
        {
            uint id = pLadderSeasonNoItem.Value.Id;
            pLadderSeasonNoItem = pLadderSeasonNoList.LadderSeasonNoByKey(pPlayer.CreateSeason);

            if(pLadderSeasonNoItem != null && (id - pLadderSeasonNoItem.Value.Id >= 0))
            {
                age += (byte)(id - pLadderSeasonNoItem.Value.Id);
            }
        }
        else
        {
            Debug.LogError($"pLadderSeasonNoList.LadderSeasonNoByKey(m_pGameInfo.SeasonNo) = null m_pGameInfo.SeasonNo:{m_pGameInfo.SeasonNo}");
        }
        
        return age;   
    }

    public uint GetPlayerAvgAge(PlayerT pPlayer,bool bAdd)
    {
        uint age = 0;
        
        LadderSeasonNoList pLadderSeasonNoList = GetFlatBufferData<LadderSeasonNoList>(E_DATA_TYPE.LadderSeasonNo);
        LadderSeasonNoItem? pLadderSeasonNoItem = pLadderSeasonNoList.LadderSeasonNoByKey(m_pGameInfo.SeasonNo);
        if(pLadderSeasonNoItem != null)
        {
            uint id = pLadderSeasonNoItem.Value.Id;
            uint total = (uint)m_pGameInfo.Players.Count;
            for(int i =0; i < m_pGameInfo.Players.Count; ++i)
            {
                if(pPlayer != null && !bAdd && pPlayer.Id == m_pGameInfo.Players[i].Id)
                {
                    continue;
                }

                pLadderSeasonNoItem = pLadderSeasonNoList.LadderSeasonNoByKey(m_pGameInfo.Players[i].CreateSeason);
                if(pLadderSeasonNoItem != null)
                {
                    age += id - pLadderSeasonNoItem.Value.Id;
                }
                
                age += m_pGameInfo.Players[i].Age;
            }

            if(pPlayer != null && bAdd)
            {
                pLadderSeasonNoItem = pLadderSeasonNoList.LadderSeasonNoByKey(pPlayer.CreateSeason);
                if(pLadderSeasonNoItem != null)
                {
                    age += id - pLadderSeasonNoItem.Value.Id;
                }
                
                age += pPlayer.Age;
                ++total;
            }
            
            return (uint)(((float)age / total)* 10);
        }
        else
        {
            Debug.LogError($"pLadderSeasonNoList.LadderSeasonNoByKey(m_pGameInfo.SeasonNo) = null m_pGameInfo.SeasonNo:{m_pGameInfo.SeasonNo}");
            return 0;
        }
    }

    public uint GetTotalPlayerAbility(int index)
    {
        uint total =0;
        if(m_pLineupPlayerList.ContainsKey(index))
        {
            PlayerT pPlayer = null;
            ulong id = 0;
            for(int n =0; n < CONST_NUMSTARTING; ++n)
            {
                id = m_pLineupPlayerList[index].Data[n];
                for(int i =0; i < m_pGameInfo.Players.Count; ++i)
                {
                    pPlayer = m_pGameInfo.Players[i];

                    if(id == pPlayer.Id)
                    {
                        total += pPlayer.AbilityWeightSum;
                    }
                }
            }
        }
        
        return total;
    }

    public MatchCommentaryList GetMatchCommentaryData()
    {
        return GetFlatBufferData<MatchCommentaryList>(E_DATA_TYPE.MatchCommentary);
    }

    public uint GetBusinessIDByBuildingName(string name)
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = null;
        for(int i =0; i < pBusinessList.BusinessLength; ++i)
        {
            pBusinessItemList = pBusinessList.Business(i);
            if(pBusinessItemList.Value.Building == name)
            {
                return pBusinessItemList.Value.No;
            }
        }
        ALFUtils.Assert(pBusinessItemList != null, $"GetBusinessIDByBuildingName pBusinessItemList = null!! BuildingName:{name}");
        return 0;
    }

    public BusinessList GetBusinessData()
    {
        return GetFlatBufferData<BusinessList>(E_DATA_TYPE.Business);
    }

    public uint GetTotalBusinessData()
    {
        return (uint)(GetBusinessData().BusinessLength);
    }

    public bool GetBusinessBuildingActiveByName(string name)
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = null; 
        for(int i = 0; i < pBusinessList.BusinessLength; ++i)
        {
            pBusinessItemList = pBusinessList.Business(i); 
            if(pBusinessItemList.Value.Building == name)
            {
                for(i = 0; i < m_pGameInfo.BusinessInfos.Count; ++i)
                {
                    if(m_pGameInfo.BusinessInfos[i].No == pBusinessItemList.Value.No)
                    {
                        return m_pGameInfo.BusinessInfos[i].Level > 0;
                    }
                }

                break;
            }
        }
        return false;
    }

    public ulong GetBusinessRedundancyByName(string name)
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = null; 
        for(int i = 0; i < pBusinessList.BusinessLength; ++i)
        {
            pBusinessItemList = pBusinessList.Business(i); 
            if(pBusinessItemList.Value.Building == name)
            {
                for(i = 0; i < m_pGameInfo.BusinessInfos.Count; ++i)
                {
                    if(m_pGameInfo.BusinessInfos[i].No == pBusinessItemList.Value.No)
                    {
                        return m_pGameInfo.BusinessInfos[i].Redundancy;
                    }
                }

                break;
            }
        }
        return 0;
    }

    public ulong GetTrainingTotalReward()
    {
        ulong total =0;
        for(int i =0; i < m_pGameInfo.TrainingInfos.Count; ++i)
        {
            total += m_pGameInfo.TrainingInfos[i].Redundancy;
        }

        return total;
    }

    public float GetPercentTrainingTotalReward(ulong total)
    {
        if(total > 0)
        {
            uint totalLimit =0;
            BusinessList pBusinessList = GetBusinessData();
            BusinessItemList? pBusinessItemList = null;
            for(int i =0; i < m_pGameInfo.TrainingInfos.Count; ++i)
            {
                pBusinessItemList = pBusinessList.BusinessByKey(m_pGameInfo.TrainingInfos[i].No);
                BusinessItem? pBusinessItem = pBusinessItemList.Value.List((int)m_pGameInfo.TrainingInfos[i].Level);
                totalLimit += pBusinessItem.Value.MaxIncomeAmount;
            }
            ALFUtils.Assert(totalLimit > 0, "GetPercentTrainingTotalReward totalLimit == 0!!!");
            return (float)((double)total / totalLimit);
        }
        return 0;
    }

    public float GetPercentBusinessTotalRewardByName(string name)
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = null;
        for(int i =0; i < pBusinessList.BusinessLength; ++i)
        {
            pBusinessItemList = pBusinessList.Business(i);
            if(pBusinessItemList.Value.Building == name)
            {
                for(i =0; i < m_pGameInfo.BusinessInfos.Count; ++i)
                {
                    if(m_pGameInfo.BusinessInfos[i].No == pBusinessItemList.Value.No)
                    {
                        BusinessItem? pBusinessItem = pBusinessItemList.Value.List((int)m_pGameInfo.BusinessInfos[i].Level);
                        return (float)((double)m_pGameInfo.BusinessInfos[i].Redundancy / pBusinessItem.Value.MaxIncomeAmount);
                    }
                }
                break;
            }
        }

        return 0;
    }

    public uint GetTrainingRewardForTimeByID(uint no)
    {
        for(int i =0; i < m_pGameInfo.TrainingInfos.Count; ++i)
        {
            if(m_pGameInfo.TrainingInfos[i].No == no)
            {
                BusinessList pBusinessList = GetBusinessData();
                BusinessItemList? pBusinessItemList = null;
                BusinessItem? pBusinessItem = null;
                pBusinessItemList = pBusinessList.BusinessByKey(no);
                pBusinessItem = pBusinessItemList.Value.List((int)m_pGameInfo.TrainingInfos[i].Level);
                return (uint)(((float)pBusinessItem.Value.RewardAmount / (float)pBusinessItem.Value.IncomeInterval) * 60);
            }
        }
        return 0;
    }

    public uint GetBusinessTotalRewardForTime()
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = null;
        BusinessItem? pBusinessItem = null;
        uint total =0;
        for(int i =0; i < m_pGameInfo.BusinessInfos.Count; ++i)
        {
            if(m_pGameInfo.BusinessInfos[i].Level > 0)
            {
                pBusinessItemList = pBusinessList.BusinessByKey(m_pGameInfo.BusinessInfos[i].No);
                pBusinessItem = pBusinessItemList.Value.List((int)m_pGameInfo.BusinessInfos[i].Level);
                total += (uint)(((float)pBusinessItem.Value.RewardAmount / (float)pBusinessItem.Value.IncomeInterval) * 60);
            }
        }
        return total;
    }


    public uint GetBusinessNextTotalRewardForTime(uint amount,uint no)
    {
        if(no > 1000)
        {
            amount = 0;
        }
        uint total =0;
        int level = 0;
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = null;
        BusinessItem? pBusinessItem = null;
        for(int i =0; i < m_pGameInfo.BusinessInfos.Count; ++i)
        {
            level = (int)m_pGameInfo.BusinessInfos[i].Level;
            if( m_pGameInfo.BusinessInfos[i].No == no)
            {
                level += (int)amount;
            }

            if( level > 0)
            {
                pBusinessItemList = pBusinessList.BusinessByKey(m_pGameInfo.BusinessInfos[i].No);
                pBusinessItem = pBusinessItemList.Value.List(level);
                if(pBusinessItem != null)
                {
                    total += (uint)(((float)pBusinessItem.Value.RewardAmount / (float)pBusinessItem.Value.IncomeInterval) * 60);
                }
            }
        }
        return total;
    }

    public uint GetBusinessFastReward()
    {
        return GetBusinessTotalRewardForTime() * 120;
    }

    public uint GetTrainingFastReward(uint no)
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = null;
        BusinessItem? pBusinessItem = null;

        for(int i =0; i < m_pGameInfo.TrainingInfos.Count; ++i)
        {
            if(m_pGameInfo.TrainingInfos[i].No == no)
            {
                pBusinessItemList = pBusinessList.BusinessByKey(m_pGameInfo.TrainingInfos[i].No);
                pBusinessItem = pBusinessItemList.Value.List((int)m_pGameInfo.TrainingInfos[i].Level);
                return (uint)(((float)pBusinessItem.Value.RewardAmount / (float)pBusinessItem.Value.IncomeInterval) * 7200);
            }

        }
        return 0;
    }

    public E_BUILDING GetBuildingTypeById(uint id)
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(id);
        return (E_BUILDING)Enum.Parse(typeof(E_BUILDING),pBusinessItemList.Value.Building);
    }

    public string GetBuildingNameByBusinessID(uint id)
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(id);
        return pBusinessItemList.Value.Building;
    }

    public int GetBusinessLevelUpCountByID(uint no,bool bBusiness,ref ulong totalCost)
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(no);
        BusinessItem? pBusinessItem = null;
        int iLevelUpCount = 0;
        
        ulong totalGameMoney = GetGameMoney();
        uint level = 0;
        if(bBusiness)
        {
            for(int i =0; i < m_pGameInfo.BusinessInfos.Count; ++i)
            {
                if(m_pGameInfo.BusinessInfos[i].No == no)
                {
                    level = m_pGameInfo.BusinessInfos[i].Level;
                    while(true)
                    {
                        if(iLevelUpCount < 10)
                        {
                            if(pBusinessItemList.Value.ListLength > level +1 + iLevelUpCount)
                            {
                                pBusinessItem = pBusinessItemList.Value.List((int)level + iLevelUpCount);
                            
                                if(pBusinessItem != null && totalGameMoney >= pBusinessItem.Value.CostAmount)
                                {
                                    totalCost += pBusinessItem.Value.CostAmount;
                                    totalGameMoney -= pBusinessItem.Value.CostAmount;
                                    ++iLevelUpCount;
                                }
                                else
                                {
                                    return iLevelUpCount;
                                }
                            }
                            else
                            {
                                return iLevelUpCount;
                            }
                        }
                        else
                        {
                            return iLevelUpCount;
                        }
                    }
                }
            }
        }
        else
        {
            for(int i =0; i < m_pGameInfo.TrainingInfos.Count; ++i)
            {
                if(m_pGameInfo.TrainingInfos[i].No == pBusinessItemList.Value.No)
                {
                    level = m_pGameInfo.TrainingInfos[i].Level;
                    while(true)
                    {
                        if(iLevelUpCount < 10)
                        {
                            if(pBusinessItemList.Value.ListLength > level + 1+ iLevelUpCount)
                            {
                                pBusinessItem = pBusinessItemList.Value.List((int)level + iLevelUpCount);
                            
                                if(pBusinessItem != null && totalGameMoney >= pBusinessItem.Value.CostAmount)
                                {
                                    totalCost += pBusinessItem.Value.CostAmount;
                                    totalGameMoney -= pBusinessItem.Value.CostAmount;
                                    ++iLevelUpCount;
                                }
                                else
                                {
                                    return iLevelUpCount;
                                }
                            }
                            else
                            {
                                return iLevelUpCount;
                            }
                        }
                        else
                        {
                            return iLevelUpCount;
                        }
                    }
                }
            }
        }
    
        return iLevelUpCount;
    }

    public bool IsBusinessLevelUpByID(uint no,bool bBusiness)
    {
        if(m_pGameInfo == null) return false;

        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(no);
        BusinessItem? pBusinessItem = null;
        ulong totalGameMoney = GetGameMoney();
        if(bBusiness)
        {
            for(int i =0; i < m_pGameInfo.BusinessInfos.Count; ++i)
            {
                if(m_pGameInfo.BusinessInfos[i].No == pBusinessItemList.Value.No)
                {
                    if(pBusinessItemList.Value.ListLength > m_pGameInfo.BusinessInfos[i].Level +1)
                    {
                        pBusinessItem = pBusinessItemList.Value.List((int)m_pGameInfo.BusinessInfos[i].Level);
                    
                        if(pBusinessItem != null)
                        {
                            return totalGameMoney >= pBusinessItem.Value.CostAmount;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            pBusinessItem = pBusinessItemList.Value.List(0);
            return totalGameMoney >= pBusinessItem.Value.CostAmount;
        }
        else
        {
            for(int i =0; i < m_pGameInfo.TrainingInfos.Count; ++i)
            {
                if(m_pGameInfo.TrainingInfos[i].No == pBusinessItemList.Value.No)
                {
                    if(pBusinessItemList.Value.ListLength > m_pGameInfo.TrainingInfos[i].Level +1)
                    {
                        pBusinessItem = pBusinessItemList.Value.List((int)m_pGameInfo.TrainingInfos[i].Level);
                    
                        if(pBusinessItem != null)
                        {
                            return totalGameMoney >= pBusinessItem.Value.CostAmount;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        return false;
    }

    public void HoldUpdateBusinessTime(E_BUILDING eType,bool bHold)
    {
        if(eType == E_BUILDING.trainingGround)
        {
            for(int n = 0; n < m_pGameInfo.TrainingInfos.Count; ++n)
            {
                m_pGameInfo.TrainingInfos[n].SkipUpdate = bHold;
            }
        }
        else
        {
            uint id = GetBusinessIDByBuildingName(eType.ToString());
            for(int n = 0; n < m_pGameInfo.BusinessInfos.Count; ++n)
            {
                if(m_pGameInfo.BusinessInfos[n].No == id)
                {
                    m_pGameInfo.BusinessInfos[n].SkipUpdate = bHold;
                    return;
                }
            }
        }
    }

    void ResetBusinessTime(BusinessInfoT pBusinessInfo,float time, int level)
    {
        BusinessList pBusinessList = GetBusinessData();
        BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(pBusinessInfo.No);
        BusinessItem? pBusinessItem = pBusinessItemList.Value.List(level);

        uint count = 0;
        while(true)
        {
            ++count;
            time += pBusinessItem.Value.IncomeInterval;
            if(time >= 0)
            {
                if(time == 0)
                {
                    time = pBusinessItem.Value.IncomeInterval;
                }
                break;
            }
        }
        pBusinessInfo.Redundancy += count * pBusinessItem.Value.RewardAmount;

        if(pBusinessInfo.Redundancy > pBusinessItem.Value.MaxIncomeAmount)
        {
            pBusinessInfo.SkipUpdate = true;
            pBusinessInfo.Redundancy = pBusinessItem.Value.MaxIncomeAmount;
        }
        pBusinessInfo.RemainingTime = time;
    }

    public static bool IsLoadUserData()
    {
        if( gCtx != null)
        {
            return gCtx.m_pUserData != null;
        }
        
        return false;
    }

    public bool IsLoadGameData()
    {
        return m_pGameInfo != null;
    }

    public void SetTutorialBusiness(Ground pGround)
    {
        if(GetTutorial() != 9) return;

        for(int n = 0; n < m_pGameInfo.BusinessInfos.Count; ++n)
        {
            if(m_pGameInfo.BusinessInfos[n].No == 1)
            {
                if(m_pGameInfo.BusinessInfos[n].Level != 0)
                {
                    BusinessList pBusinessList = GetBusinessData();
                    BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(1);
                    BusinessItem? pBusinessItem = pBusinessItemList.Value.ListByKey(m_pGameInfo.BusinessInfos[n].Level);

                    m_pGameInfo.BusinessInfos[n].Redundancy = pBusinessItem.Value.MaxIncomeAmount;
                    ResetBusinessTime(m_pGameInfo.BusinessInfos[n],0,(int)m_pGameInfo.BusinessInfos[n].Level);
                    pGround?.UpdateBuildingTimer(true,(E_BUILDING)m_pGameInfo.BusinessInfos[n].Building);
                }
                return;
            }
        }
    }

    public AdRewardDataT GetAdRewardDataByID(uint no)
    {
        for(int n =0; n < m_pAdRewardInfo.List.Count; ++n)
        {
            if(m_pAdRewardInfo.List[n].No == no)
            {
                return m_pAdRewardInfo.List[n];
            }
        }

        return null;
    }

    void UpdateTimeSale(float dt,MainScene pScene)
    {
        float time = float.MaxValue;
        if(m_pProductTimeSaleInfo.Count > 0)
        {
            var list = m_pProductTimeSaleInfo.Keys;
            
            foreach(var k in list)
            {
                if(m_pProductTimeSaleInfo[k].Status == 0 && m_pProductTimeSaleInfo[k].TExpire > 0)
                {
                    m_pProductTimeSaleInfo[k].TExpire -= dt;
                        
                    if(time > m_pProductTimeSaleInfo[k].TExpire )
                    {
                        time = m_pProductTimeSaleInfo[k].TExpire;
                    }
                }
            }
        }
        
        pScene?.ShowTimeSale(time < float.MaxValue, (int)time);
    }

    void UpdateAdTime(float dt,MainScene pScene)
    {
        for(int n =0; n < m_pAdRewardInfo.List.Count; ++n)
        {
            if(!m_pAdRewardInfo.List[n].Activate && m_pAdRewardInfo.List[n].MaxAmount > m_pAdRewardInfo.List[n].Amount)
            {
                m_pAdRewardInfo.List[n].TActivate -= dt;
                if(m_pAdRewardInfo.List[n].TActivate <= 0)
                {
                    m_pAdRewardInfo.List[n].Activate = true;
                    pScene?.SetAdNotice(m_pAdRewardInfo.List[n].No,true);
                }
            }
        }
    }

    public float GetExpireTimerByUI(ITimer ui,int index)
    {
        if(m_pExpireTimeList.ContainsKey(ui))
        {
            if(m_pExpireTimeList[ui] != null && m_pExpireTimeList[ui].Count > index)
            {
                return m_pExpireTimeList[ui][index];
            }
        }
        return -1;
    }

    public void RemoveExpireTimerByUI(ITimer ui)
    {
        if(m_pExpireTimeList.ContainsKey(ui))
        {
            m_pExpireTimeList[ui].Clear();
            m_pExpireTimeList.Remove(ui);
        }
    }

    public void AddExpireTimer(ITimer ui,int index, float fExpireTime)
    {
        List<float> list = null;
        if(m_pExpireTimeList.ContainsKey(ui))
        {
            list = m_pExpireTimeList[ui];   
        }
        else
        {
            list = new List<float>();
            m_pExpireTimeList.Add(ui,list);
        }

        if(list.Count > index)
        {
            list[index] = fExpireTime;
        }
        else
        {
            for(int i = list.Count; i < index; ++i)
            {
                list.Add(-1);
            }
            list.Add(fExpireTime);
        }
    }

    public bool IsMaxPlayerCount()
    {
        int total = (int)GetCurrentSquadMax() - GetCurrentBiddingPlayerCount();
        return GetTotalPlayerCount() >= total;
    }

    public void UpdateTimer(float dt,MainScene pScene)
    {
        UpdateBusinessTime(dt,pScene);
        UpdateAdTime(dt,pScene);
        UpdateTimeSale(dt,pScene);

        PlayerInfo pPlayerInfo = null;
        if(pScene != null && pScene.IsShowInstance<PlayerInfo>())
        {
            pPlayerInfo = pScene.GetInstance<PlayerInfo>();
        }

        m_pGameInfo.QuestExpire -=dt;
        JObject pJObject = null;
        JArray pJArray = null;
        bool bSend = false;
        int i = m_pAuctionBiddingInfoData.Count;
        while(i > 0)
        {
            --i;
            if(!m_pAuctionBiddingInfoData[i].Reward)
            {
                m_pAuctionBiddingInfoData[i].TExtend -= dt;
                
                if(!m_pAuctionBiddingInfoData[i].Update) continue;

                if(m_pAuctionBiddingInfoData[i].TExtend <= 0)
                {
                    m_pAuctionBiddingInfoData[i].Reward = true;
                    m_pAuctionBiddingInfoData[i].Update = false;
                    Debug.Log("-------------------------------------------GameContext");
                    if(pPlayerInfo != null)
                    {
                        /**
                        * 옥션 완료된 UI를 보고 있다면 해당 UI에서 연출이후에 서버에 보상 요청해야한다...
                        */
                        bSend = pPlayerInfo.CloseAuction(m_pAuctionBiddingInfoData[i].AuctionId);
                    }

                    if(!bSend)
                    {
                        Debug.Log("-------------------------------------------GameContext111");
                        /**
                        * 옥션 완료시 해당 옥션방을 떠난다..
                        */
                        pJObject = new JObject();
                        pJObject["type"] = E_SOKET.auctionLeave.ToString();
                        pJObject["auctionId"] = m_pAuctionBiddingInfoData[i].AuctionId;
                        NetworkManager.SendMessage(pJObject);

                        if(m_pAuctionBiddingInfoData[i].FinalGold == m_pAuctionBiddingInfoData[i].Gold && m_pAuctionBiddingInfoData[i].FinalToken == m_pAuctionBiddingInfoData[i].Token)
                        {
                            Debug.Log("-------------------------------------------E_REQUEST_ID.auction_trade");
                            pJObject = new JObject();
                            pJArray = new JArray();
                            pJArray.Add(m_pAuctionBiddingInfoData[i].AuctionId);
                            pJObject["auctionIds"] = pJArray;
                            NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.auction_trade, GetNetworkAPI(E_REQUEST_ID.auction_trade),false,true,null,pJObject);
                        }
                        else
                        {
                            pJObject = new JObject();
                            pJArray = new JArray();
                            pJArray.Add(m_pAuctionBiddingInfoData[i].AuctionId);
                            pJObject["auctionIds"] = pJArray;
                            NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.auction_refund, GetNetworkAPI(E_REQUEST_ID.auction_refund),false,true,null,pJObject);
                        }
                    }
                }
            }
        }

        i = m_pAuctionSellInfoData.Count;
        uint iAuctionId =0;
        while(i > 0)
        {
            --i;
            if(m_pAuctionSellInfoData[i].Reward) continue;

            m_pAuctionSellInfoData[i].TExpire -= dt;
            m_pAuctionSellInfoData[i].TEnd -= dt;
            m_pAuctionSellInfoData[i].TExtend -= dt;
            
            if(!m_pAuctionSellInfoData[i].End && !m_pAuctionSellInfoData[i].Join && m_pAuctionSellInfoData[i].TExtend < 10)
            {
                m_pAuctionSellInfoData[i].Update = false;
                m_pAuctionSellInfoData[i].Join = true;
                pJObject = new JObject();
                pJObject["type"]= E_SOKET.auctionJoin.ToString();
                pJObject["auctionId"] = m_pAuctionSellInfoData[i].AuctionId;
                NetworkManager.SendMessage(pJObject);
                continue;
            }

            if(!m_pAuctionSellInfoData[i].Update) continue;

            bSend = false;            

            if(m_pAuctionSellInfoData[i].Join && m_pAuctionSellInfoData[i].TExtend <= 0 && m_pAuctionSellInfoData[i].FinalGold > 0)
            {
                iAuctionId = m_pAuctionSellInfoData[i].AuctionId; 
                bSend = true;
                m_pAuctionSellInfoData[i].Reward = true;
                pJObject = new JObject();
                pJArray = new JArray();
                pJArray.Add(iAuctionId);
                pJObject["auctionIds"] = pJArray;
                NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.auction_reward, GetNetworkAPI(E_REQUEST_ID.auction_reward),false,true,null,pJObject);
                m_pAuctionSellInfoData.RemoveAt(i);
            }
            else if(m_pAuctionSellInfoData[i].TExpire <= 0)
            {
                m_pAuctionSellInfoData[i].Join = true;
                m_pAuctionSellInfoData[i].Reward = true;
                if(m_pAuctionSellInfoData[i].FinalGold == 0)
                {
                    iAuctionId = m_pAuctionSellInfoData[i].AuctionId;
                    bSend = true;
                    pJObject = new JObject();
                    pJArray = new JArray();
                    pJArray.Add(iAuctionId);
                    pJObject["auctionIds"] = pJArray;
                    Debug.Log("UpdateTimer----------------------------------E_REQUEST_ID.auction_withdraw");
                    NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.auction_withdraw, GetNetworkAPI(E_REQUEST_ID.auction_withdraw),false,true,null,pJObject);
                    m_pAuctionSellInfoData.RemoveAt(i);
                }
            }

            if(bSend)
            {
                /**
                * 옥션 완료시 해당 옥션방을 떠난다..
                */
                pJObject = new JObject();
                pJObject["type"] = E_SOKET.auctionLeave.ToString();
                pJObject["auctionId"] = iAuctionId;
                NetworkManager.SendMessage(pJObject);
            }
        }

        /**
        * 광고 보기 갱신 주기 정보 처리
        */
        i = m_pUserData.SettingInfo.AdInfos.Count;
        while(i > 0)
        {
            --i;
            m_pUserData.SettingInfo.AdInfos[i].TAdRefreshTime -= dt;
            if(m_pUserData.SettingInfo.AdInfos[i].TAdRefreshTime <= 0)
            {
                m_pUserData.SettingInfo.AdInfos.RemoveAt(i);
            }
        }

        if(!pScene.IsShowInstance<Match>() && (!pScene.IsShowInstance<MatchView>() || pScene.GetInstance<MatchView>().IsFinishMatchGame()))
        {
            UpdatePlayerHP(pScene);
        }

        /**
        * 각 UI에서 등록한 타이머 처리
        */

        List<ITimer> list = m_pExpireTimeList.Keys.ToList();
        i = list.Count;
        while(i > 0)
        {
            --i;
            List<float> tList = m_pExpireTimeList[list[i]];
            int n =tList.Count;
            while(n > 0)
            {
                --n;
                if(tList[n] > 0)
                {
                    tList[n] -= dt;
                    if(tList[n] <= 0)
                    {
                        list[i].DoExpire(n);
                        tList.RemoveAt(n);
                        if(tList.Count == 0)
                        {
                            m_pExpireTimeList.Remove(list[i]);  
                        }
                    }
                }
            }
        }

        /**
        * Challenge Ticket Charge 타이머 처리 
        */
        if(m_pGameInfo.TChallengeTicketCharge > 0)
        {
            if(m_pInventoryInfoList.ContainsKey(GameContext.CHALLENGE_TICKET_ID))
            {
                m_pGameInfo.TChallengeTicketCharge -= dt;
                if(m_pGameInfo.TChallengeTicketCharge <= 0)
                {
                    i =0;
                    float coolTime = GetConstValue(E_CONST_TYPE.challengeStageMatchChanceCooldown);
                    while(m_pGameInfo.TChallengeTicketCharge <= 0)
                    {
                        m_pGameInfo.TChallengeTicketCharge += coolTime;
                        ++i;
                    }
                    
                    ulong amountMax = (ulong)GetConstValue(E_CONST_TYPE.challengeStageMatchChanceMax);
                    m_pGameInfo.TChallengeTicketCharge = coolTime;
                    m_pInventoryInfoList[GameContext.CHALLENGE_TICKET_ID].Amount += (ulong)i;
                    if(m_pInventoryInfoList[GameContext.CHALLENGE_TICKET_ID].Amount >= amountMax)
                    {
                        m_pInventoryInfoList[GameContext.CHALLENGE_TICKET_ID].Amount = amountMax;
                        m_pGameInfo.TChallengeTicketCharge = -1;
                    }

                    pScene?.UpdateLeagueTodayCount();
                }
            }
        }
    }

    void UpdateBusinessTime(float dt,MainScene pScene)
    {
        int n = 0;
        uint currentLevel = 0;
        float time = 0;
        bool bUpdate = false;

        if(!m_pGameInfo.TrainingInfos[n].SkipUpdate)
        {
            time = m_pGameInfo.TrainingInfos[n].RemainingTime;
            time -= dt;
            if(time <= 0)
            {
                bUpdate = true;
            }
            
            for(n = 0; n < m_pGameInfo.TrainingInfos.Count; ++n)
            {
                if(bUpdate)
                {
                    ResetBusinessTime(m_pGameInfo.TrainingInfos[n],time,(int)(m_pGameInfo.TrainingInfos[n].Level));
                }
                else
                {
                    m_pGameInfo.TrainingInfos[n].RemainingTime = time;
                }
            }

            if(GetTutorial() > 8 && bUpdate)
            {
                pScene?.UpdateBuildingTimer(false,E_BUILDING.trainingGround);
            }
        }
        
        pScene?.UpdateTrainingTimer(time);
        
        for(n = 0; n < m_pGameInfo.BusinessInfos.Count; ++n)
        {
            if(m_pGameInfo.BusinessInfos[n].SkipUpdate) continue;
            
            currentLevel = m_pGameInfo.BusinessInfos[n].Level;
            if(currentLevel > 0)
            {
                time = m_pGameInfo.BusinessInfos[n].RemainingTime;
                time -= dt;
                if(time <= 0)
                {
                    ResetBusinessTime(m_pGameInfo.BusinessInfos[n],time,(int)currentLevel);
                    if(GetTutorial() > 8)
                    {
                        pScene?.UpdateBuildingTimer(true,(E_BUILDING)m_pGameInfo.BusinessInfos[n].Building);
                    }
                }
                else
                {
                    m_pGameInfo.BusinessInfos[n].RemainingTime = time;
                }
            }
        }
    }

    public uint GetCurrentBusinessLevelByID(uint id)
    {
        if(m_pGameInfo != null)
        {
            for(int i =0; i < m_pGameInfo.BusinessInfos.Count; ++i)
            {
                if(m_pGameInfo.BusinessInfos[i].No == id)
                {
                    return m_pGameInfo.BusinessInfos[i].Level;
                }
            }
        }

        return 0;
    }

    public ulong GetYouthNominationCount()
    {
        return GetItemCountByNO(NOMINATION_ID);
    }

    public List<ItemInfoT> GetScoutTickets()
    {
        List<ItemInfoT> list = new List<ItemInfoT>();
        ItemInfoT pItemInfo = null;

        for(uint id = SCOUT_TICKET_ID_START; id <= SCOUT_TICKET_ID_END; ++id)
        {
            if(m_pInventoryInfoList.ContainsKey(id))
            {
                pItemInfo = new ItemInfoT();
                pItemInfo.No = id;
                pItemInfo.Amount = m_pInventoryInfoList[id].Amount;
                list.Add(pItemInfo); 
            }
        }

        return list;
    }

    public ulong GetItemCountByNO(uint no)
    {
        if(m_pInventoryInfoList.ContainsKey(no))
        {
            return m_pInventoryInfoList[no].Amount;
        }

        return 0;
    }

    public ulong GetPositionSkill()
    {
        return GetItemCountByNO(POSITON_POINT_ID);
    }

    public ulong GetTrainingPointByType(E_TRAINING_TYPE eType)
    {
        return GetItemCountByNO(41 + (uint)eType);
    }

    public uint GetCurrentTrainingLevelByID(uint id)
    {
        if(m_pGameInfo != null)
        {
            for(int i =0; i < m_pGameInfo.TrainingInfos.Count; ++i)
            {
                if(m_pGameInfo.TrainingInfos[i].No == id)
                {
                    return m_pGameInfo.TrainingInfos[i].Level;
                }
            }
        }

        return 1;
    }
    public int GetTotalPlayerCount()
    {
        if(m_pGameInfo == null) return GameContext.CONST_NUMPLAYER;
        return m_pGameInfo.Players.Count;
    }

    public List<PlayerT> GetTotalPlayerList()
    {
        if(m_pGameInfo == null) return null;

        return m_pGameInfo.Players;
    }

    public uint GetTotalSquadMax()
    {
        return (uint)GetConstValue(E_CONST_TYPE.squadSizeMax);
    }

    public uint GetCurrentSquadMax()
    {
        return m_pClubInfo.SquadCapacity;
    }

    public void ClearCurrentPurchaseInfo()
    {
        m_pCurrentPurchaseInfo = null;
    }

    bool SavePurchaseReceiptData()
    {
        PlayerPrefs.SetString($"{m_pUserData.CustomerNo}:rs" ,Convert.ToBase64String(m_pRestoreInfoList.SerializeToBinary()));
        PlayerPrefs.Save();
        return true;
    }
    
    public bool SaveUserData(bool bSave)
    {
        PlayerPrefs.SetString("ud" ,Convert.ToBase64String(m_pUserData.SerializeToBinary()));
        if(bSave)
        {
            PlayerPrefs.Save();
        }
        return true;
    }

    public uint GetTotalCash()
    {
        return (uint)(GetItemCountByNO(GameContext.CASH_ID) +GetItemCountByNO(GameContext.FREE_CASH_ID));
    }

    // public void AddCash(int cash,bool bFree, bool bSave)
    // {
    //     long sum = 0;
    //     if(bFree)
    //     {
    //         sum = (long)m_pGameInfo.FreeCash;
    //         sum += cash;
    //         if(sum < 0)
    //         {
    //             sum = sum + m_pGameInfo.Cash;
    //             m_pGameInfo.FreeCash = 0;
    //             AddCash((int)sum,false,bSave);
    //             return;
    //         }
            
    //         m_pGameInfo.FreeCash = (uint)sum;
    //     }
    //     else
    //     {
    //         sum = (long)m_pGameInfo.Cash;
    //         sum += cash;
    //         ALFUtils.Assert(sum >= 0, " AddCash sum < 0 !!!");
    //         m_pGameInfo.Cash = (uint)sum;
    //     }
    // }
    public ulong GetGameMoney()
    {
        return GetItemCountByNO(GameContext.GAME_MONEY_ID);
    }

    public bool AddRootPathCountScore(bool bHintUsed)
    {
        bool bChange = false;
        
        return bChange;
    }

    public byte GetTutorial()
    {
        if(m_pGameInfo != null)
        {
            return m_pGameInfo.Tutorial;
        }
        
        return 0;
    }

    public void SetTutorial(byte step)
    {
        if(m_pGameInfo != null)
        {
            m_pGameInfo.Tutorial = step;
        }
    }
    
    public bool IsReview(byte rank)
    {
        return rank > m_pUserData.SettingInfo.ReviewCount;
    }

    public void DisableReview()
    {
        m_pUserData.SettingInfo.ReviewCount = 3;
        m_pUserData.SettingInfo.IsReview = false;
        SaveUserData(true);
    }
    
    void AddReviewCount(bool bSave)
    {
        m_pUserData.SettingInfo.ReviewCount = (byte)(m_pUserData.SettingInfo.ReviewCount +1);   
        m_pUserData.SettingInfo.IsReview = false;
        SaveUserData(bSave);
    }

    public NATION_CODE ConvertPlayerNationCodeByString(string strCode,ulong id)
    {
        NATION_CODE code = NATION_CODE.ALB;
        if(!Enum.TryParse(strCode, out code))
        {
            code =(NATION_CODE)UnityEngine.Random.Range((int)NATION_CODE.ALB,(int)NATION_CODE.MAX);
            JObject pJObject = new JObject();
            pJObject["playerId"] = id;
            pJObject["nation"] = strCode;
            SingleFunc.SendLog(pJObject.ToString());
        }

        return code;
    }
    public CLUB_NATION_CODE ConvertClubNationCodeByString(string strCode,ulong id)
    {
        CLUB_NATION_CODE code = CLUB_NATION_CODE.ABW;
        if(!Enum.TryParse(strCode, out code))
        {
            code =(CLUB_NATION_CODE)UnityEngine.Random.Range((int)CLUB_NATION_CODE.ABW,(int)CLUB_NATION_CODE.MAX);
            JObject pJObject = new JObject();
            pJObject["clubNation"] = strCode;
            pJObject["clubId"] = id;
            SingleFunc.SendLog(pJObject.ToString());
        }

        return code;
    }

    public CLUB_NATION_CODE GetClubNationCode()
    {
        return ConvertClubNationCodeByString(m_pClubInfo.Nation,m_pClubInfo.Id);
    }

    public string GetClubFoundedTimeInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            DateTime pDateTime = new DateTime(m_pClubInfo.TCreate);
            return $"{pDateTime.Day}/{pDateTime.Month}/{pDateTime.Year} {pDateTime.Hour}:{pDateTime.Minute}";
        }
        else
        {
            return "-";
        }
    }

    public uint GetSquadPowerInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return GetTotalPlayerAbility(GetActiveLineUpType());
        }
        else
        {
            return m_pAwayClubData.SquadPower;
        }
    }

    public CLUB_NATION_CODE GetClubNationCodeInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        ulong id = 0;
        string nation = null;
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            nation = m_pClubData.GetNation();
            id = m_pClubData.GetID();
        }
        else
        {
            nation = m_pAwayClubData.GetNation();
            id = m_pAwayClubData.GetID();
        }

        return ConvertClubNationCodeByString(nation,id);
    }

    public List<ulong> GetCorePlayersInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetCorePlayers();
        }
        else
        {
            return m_pAwayClubData.GetCorePlayers();
        }
    }

    public List<PlayerT> GetTotalPlayersInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pGameInfo.Players;
        }
        else
        {
            return m_pAwayClubData.GetTotalPlayer();
        }
    }
    
    public uint GetCurrentSeasonStandingInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetStanding();
        }
        else
        {
            return m_pAwayClubData.GetStanding();
        }
    }

    public uint GetSeasonTrophyHighInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetSeasonTrophyHigh();
        }
        else
        {
            return m_pAwayClubData.GetSeasonTrophyHigh();
        }
    }

    public uint GetSeasonStandingHighInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetSeasonStandingHigh();
        }
        else
        {
            return m_pAwayClubData.GetSeasonStandingHigh();
        }
    }

    public uint GetTotalTrophyHighInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetTotalTrophyHigh();
        }
        else
        {
            return m_pAwayClubData.GetTotalTrophyHigh();
        }
    }

    public ClubSeasonStats GetLatestSeasonStatsInClubProfile(E_PLAYER_INFO_TYPE eType,int index)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetLatestSeasonStats(index);
        }
        else
        {
            return m_pAwayClubData.GetLatestSeasonStats(index);
        }
    }

    public string GetHistotyInfoInClubProfile(E_PLAYER_INFO_TYPE eType,bool bSeason, E_HISTORY eHistory)
    {
        ClubData pData = null;
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            pData = m_pClubData;
        }
        else
        {
            pData = m_pAwayClubData;
        }

        switch(eHistory)
        {
            case E_HISTORY.matchPlayed: 
            {
                uint value = pData.GetGamesStats(bSeason);
                return value > 0 ? $"{value}" :"-";
            }
            case E_HISTORY.winStreak:
            {
                uint value = pData.GetWinStreakStats(bSeason);
                return value > 0 ? $"{value}" :"-";
            } 
            case E_HISTORY.bigWin: return pData.GetBigWinStats(bSeason);
            case E_HISTORY.goals:
            {
                uint value = pData.GetGoalsStats(bSeason);
                return value > 0 ? $"{value}" :"-";
            } 
            case E_HISTORY.mostGoals: return pData.GetMostGoalsStats(bSeason);
            case E_HISTORY.mostAssists: return pData.GetMostAssistsStats(bSeason);
            case E_HISTORY.transferSpending:
            {
                long value = pData.GetTransferSpendingStats(bSeason);
                return value > 0 ? ALFUtils.NumberToString(value) :"-";
            } 
            case E_HISTORY.transferIncome:
            {
                long value = pData.GetTransferIncomeStats(bSeason);
                return value > 0 ? ALFUtils.NumberToString(value) :"-";
            } 
            case E_HISTORY.netSpend:
            {
                long value = pData.GetNetSpendStats(bSeason);
                return value != 0 ? ALFUtils.NumberToString(value) :"-";
            }
        }

        return "-";
    }

    public ulong GetProfileClubID(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetID();
        }
        else
        {
            return m_pAwayClubData.GetID();
        }
    }

    public uint GetTotalStandingHighInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetTotalStandingHigh();
        }
        else
        {
            return m_pAwayClubData.GetTotalStandingHigh();
        }
    }

    public byte[] GetFormationArrayInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetFormationList().ToArray();
        }
        else
        {
            return m_pAwayClubData.GetFormationList().ToArray();
        }
    }
    public List<ulong> GetLineupPlayerIdListInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetLineupList();
        }
        else
        {
            return m_pAwayClubData.GetLineupList();
        }
    }

    public byte GetClubRankInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetRank();
        }
        else
        {
            return m_pAwayClubData.GetRank();
        }
    }

    public uint GetTrophyInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetTrophy();
        }
        else
        {
            return m_pAwayClubData.GetTrophy();
        }
    }

    public string GetClubNameInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetClubName();
        }
        else
        {
            return m_pAwayClubData.GetClubName();
        }
    }
    
    public byte[] GetClubEmblemDataInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetEmblemData();
        }
        else
        {
            if(m_pAwayClubData != null)
            {
                return m_pAwayClubData.GetEmblemData();
            }
        }

        return null;
    }

    public uint GetThisSeasonDrawInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetThisSeasonDraw();
        }
        else
        {
            return m_pAwayClubData.GetThisSeasonDraw();
        }
    }

    public uint GetThisSeasonLoseInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetThisSeasonLose();
        }
        else
        {
            return m_pAwayClubData.GetThisSeasonLose();
        }
    }

    public uint GetThisSeasonWinInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetThisSeasonWin();
        }
        else
        {
            return m_pAwayClubData.GetThisSeasonWin();
        }
    }

    public uint GetLineupOverallInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetLineupOverall();
        }
        else
        {
            return m_pAwayClubData.GetLineupOverall();
        }
    }

    public uint GetTotalWinInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetTotalWin();
        }
        else
        {
            return m_pAwayClubData.GetTotalWin();
        }
    }

    public uint GetTotalLoseInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetTotalLose();
        }
        else
        {
            return m_pAwayClubData.GetTotalLose();
        }
    }
    public uint GetSeasonEnteredInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetSeasonEntered();
        }
        else
        {
            return m_pAwayClubData.GetSeasonEntered();
        }
    }
    
    public float GetPlayerAgeSumInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetAvgAge();
        }
        else
        {
            return m_pAwayClubData.GetAvgAge();
        }
    }

    // public uint GetPlayerAmountInClubProfile(E_PLAYER_INFO_TYPE eType)
    // {
    //     if(eType == E_PLAYER_INFO_TYPE.my)
    //     {
    //         return m_pClubData.GetPlayerAmount();
    //     }
    //     else
    //     {
    //         return m_pAwayClubData.GetPlayerAmount();
    //     }
    // }

    public ulong GetPlayerValueSumInClubProfile(E_PLAYER_INFO_TYPE eType)
    {
        if(eType == E_PLAYER_INFO_TYPE.my)
        {
            return m_pClubData.GetPlayerTotalValue();
        }
        else
        {
            return m_pAwayClubData.GetPlayerTotalValue();
        }
    }

    public void BundleUpdate(System.Action action)
    {
        Director.StartCoroutine(coLoadAssetAsync(action));
    }

    IEnumerator coLoadAssetAsync(System.Action action)
    {
        AsyncOperationHandle<AFPool> pAsyncOperationHandle = Addressables.LoadAssetAsync<AFPool>("Bundle");
        
        pAsyncOperationHandle.Completed += op =>{
            if(op.Status == AsyncOperationStatus.Failed || op.Result == null)
            {
                IBaseScene pScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
                if(pScene != null)
                {
                    Confirm pUI = pScene.GetInstance<Confirm>();
                    pUI.SetText(GetLocalizingText("MSG_TXT_TRY_AGAIN"),GetLocalizingText("MSG_TXT_RESOURCE_DOWNLOAD_FAIL"),GetLocalizingText("MSG_TXT_TRY_AGAIN"),GetLocalizingText("DIALOG_BTN_CALCEL"));
                    pUI.ShowCloseButton(false);
                    pUI.SetOKAction(() =>{ Application.Quit();});
                    pUI.SetCancelAction(null);
                    SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
                }
            }
            else
            {
                AFPool.UpdateBundle(op.Result);
                Director.StartCoroutine(coGameDataLoad(action));
            }
        };

        IntroScene pIntroScene = Director.Instance.GetActiveBaseScene<IntroScene>();
        pIntroScene.ShowInitInfo(true);

        while(!pAsyncOperationHandle.IsDone)
        { 
            pIntroScene.SetInitInfo(pAsyncOperationHandle.PercentComplete,string.Format(GetLocalizingText("RESOURCEDOWNLOAD_TXT_LOAD"),string.Format("{0:0.0#} %",pAsyncOperationHandle.PercentComplete * 100)));
            yield return null;
        }

        pIntroScene.SetInitInfo(1,string.Format(GetLocalizingText("RESOURCEDOWNLOAD_TXT_LOAD"),"100 %"));
    }

    IEnumerator coGameDataLoad(System.Action action)
    {
        if(AFPool.Instance != null)
        {
            m_pPlayerNationCodeList.Clear();
            m_pClubNationCodeList.Clear();
            dataList.Clear();
            constValueList.Clear();
            TextAsset bin = null;
            ByteBuffer bb = null;
            E_DATA_TYPE dataType = E_DATA_TYPE.None;
            IFlatbufferObject pData = null;

            IntroScene pIntroScene = Director.Instance.GetActiveBaseScene<IntroScene>();
            pIntroScene?.ShowInitInfo(true);

            int count =0;
            
            ICollection<string> list = ALF.AFPool.GetAssetListByType(BIN);
            List<string> languageList = new List<string>();
            foreach( string key in list)
            {
                if(Enum.TryParse<E_DATA_TYPE>(key, out dataType))
                {
                    switch(dataType)
                    {
                        case E_DATA_TYPE.zh_CN:
                        case E_DATA_TYPE.zh_TW:
                        case E_DATA_TYPE.ko_KR:
                        case E_DATA_TYPE.en_US:
                        case E_DATA_TYPE.fr_FR:
                        case E_DATA_TYPE.de_DE:
                        case E_DATA_TYPE.es_ES:
                        case E_DATA_TYPE.pt_BR:
                        case E_DATA_TYPE.in_ID:
                        {
                            languageList.Add(key);
                        }
                        break;
                    }
                }
            }
            /**
            * 언어 팩을 먼저 로드해야한다...
            * 데이터에서 언어 관련 리소스로 변환 작업 진행때문...
            */
            foreach( string key in languageList) 
            {
                bin = ALF.AFPool.GetItem<TextAsset>(BIN,key);
                ALF.ALFUtils.Assert(bin,"bin == null !!!");
                bb = new ByteBuffer(bin.bytes);
                ++count;
                pData = null;
                if(Enum.TryParse<E_DATA_TYPE>(key, out dataType))
                {
                    pData = TextList.GetRootAsTextList(bb);
                    if(pData != null)
                    {
                        AddFlatBufferData(dataType,pData);
                    }

                    pIntroScene?.SetInitInfo( count/(float)list.Count,GetLocalizingText("RESOURCEDOWNLOAD_TXT_RESET"));
                }
            }

            foreach( string key in list)
            {
                if(languageList.Contains(key)) continue;

                bin = ALF.AFPool.GetItem<TextAsset>(BIN,key);
                ALF.ALFUtils.Assert(bin,"bin == null !!!");
                bb = new ByteBuffer(bin.bytes);
                ++count;
                pData = null;
                
                if(Enum.TryParse<E_DATA_TYPE>(key, out dataType))
                {
                    switch(dataType)
                    {
                        case E_DATA_TYPE.TimeSale:
                        {
                            pData = TimeSaleList.GetRootAsTimeSaleList(bb);
                        }
                        break;
                        case E_DATA_TYPE.Fword:
                        {
                            ALFUtils.ClearAllFwordList();

                            FwordList pFwordList = FwordList.GetRootAsFwordList(bb);
                            for(int i =0; i < pFwordList.FwordLength; ++i)
                            {
                                ALFUtils.AddFwordList(pFwordList.Fword(i));
                            }
                        }
                        break;
                        case E_DATA_TYPE.ItemData:
                        {
                            pData = ItemList.GetRootAsItemList(bb);
                        }
                        break;
                        case E_DATA_TYPE.LeagueStandingRewardData:
                        {
                            pData = LeagueStandingRewardList.GetRootAsLeagueStandingRewardList(bb);
                        }
                        break;
                        case E_DATA_TYPE.Push:
                        {
                            pData = PushList.GetRootAsPushList(bb);
                        }
                        break;
                        case E_DATA_TYPE.QuestMission:
                        {
                            pData = QuestMissionList.GetRootAsQuestMissionList(bb);
                        }
                        break;
                        case E_DATA_TYPE.EventCondition:
                        {
                            pData = EventConditionList.GetRootAsEventConditionList(bb);
                        }
                        break;
                        case E_DATA_TYPE.AchievementMission:
                        {
                            pData = AchievementMissionList.GetRootAsAchievementMissionList(bb);
                        }
                        break;
                        case E_DATA_TYPE.ClubLicenseMission:
                        {
                            pData = ClubLicenseMissionList.GetRootAsClubLicenseMissionList(bb);
                        }
                        break;
                        case E_DATA_TYPE.GameTip:
                        {
                            pData = GameTipList.GetRootAsGameTipList(bb);
                        }
                        break;
                        case E_DATA_TYPE.Shop:
                        {
                            pData = SHOP.ShopList.GetRootAsShopList(bb);
                        }
                        break;
                        case E_DATA_TYPE.ShopProduct:
                        {
                            pData = ShopProductList.GetRootAsShopProductList(bb);
                        }
                        break;
                        case E_DATA_TYPE.Pass:
                        {
                            pData = PassList.GetRootAsPassList(bb);
                        }
                        break;
                        case E_DATA_TYPE.PassMission:
                        {
                            pData = PassMissionList.GetRootAsPassMissionList(bb);
                        }
                        break;
                        case E_DATA_TYPE.LadderReward:
                        {
                            pData = LadderRewardList.GetRootAsLadderRewardList(bb);
                        }
                        break;
                        case E_DATA_TYPE.AttendReward:
                        {
                            pData = AttendRewardList.GetRootAsAttendRewardList(bb);
                        }
                        break;
                        case E_DATA_TYPE.MileageMission:
                        {
                            pData = MileageMissionList.GetRootAsMileageMissionList(bb);
                        }
                        break;
                        case E_DATA_TYPE.RewardSet:
                        {
                            pData = RewardSetList.GetRootAsRewardSetList(bb);
                        }
                        break;
                        case E_DATA_TYPE.PlayerPotentialWeightSum:
                        {
                            pData = PlayerPotentialWeightSumList.GetRootAsPlayerPotentialWeightSumList(bb);
                        }
                        break;
                        case E_DATA_TYPE.PlayerNationality:
                        {
                            PlayerNationalityList pPlayerNationalityList = PlayerNationalityList.GetRootAsPlayerNationalityList(bb);
                            PlayerNationalityItem? pPlayerNationalityItem = null;

                            for(NATION_CODE code = NATION_CODE.ALB; code < NATION_CODE.MAX; ++code)
                            {
                                pPlayerNationalityItem = pPlayerNationalityList.PlayerNationalityByKey(code.ToString());
                                m_pPlayerNationCodeList.Add(GetLocalizingText(pPlayerNationalityItem.Value.List(0).Value.NationName) +":" + pPlayerNationalityItem.Value.Nation);
                            }
                            m_pPlayerNationCodeList.Sort();
                            pData = pPlayerNationalityList;
                        }
                        break;
                        case E_DATA_TYPE.ClubNationality:
                        {
                            ClubNationalityList pClubNationalityList = ClubNationalityList.GetRootAsClubNationalityList(bb);
                            ClubNationalityItem? pClubNationalityItem = null;

                            for(CLUB_NATION_CODE code = CLUB_NATION_CODE.ABW; code < CLUB_NATION_CODE.MAX; ++code)
                            {
                                pClubNationalityItem = pClubNationalityList.ClubNationalityByKey(code.ToString());
                                m_pClubNationCodeList.Add(GetLocalizingText(pClubNationalityItem.Value.NationName) +":" + pClubNationalityItem.Value.Nation);
                            }

                            m_pClubNationCodeList.Sort();
                            pData = pClubNationalityList;
                        }
                        break;
                        case E_DATA_TYPE.UserRank:
                        {
                            pData = UserRankList.GetRootAsUserRankList(bb);
                        }
                        break;
                        case E_DATA_TYPE.TrainingCost:
                        {
                            pData = TrainingCostList.GetRootAsTrainingCostList(bb);
                        }
                        break;
                        case E_DATA_TYPE.PositionFamilar:
                        {
                            pData = PositionFamiliarList.GetRootAsPositionFamiliarList(bb);
                        }
                        break;
                        case E_DATA_TYPE.AdReward:
                        {
                            pData = AdRewardList.GetRootAsAdRewardList(bb);
                        }
                        break;
                        case E_DATA_TYPE.Business:
                        {
                            pData = BusinessList.GetRootAsBusinessList(bb);
                        }
                        break;
                        case E_DATA_TYPE.PlayerAbilityWeightConversion:
                        {
                            pData = PlayerAbilityWeightConversionList.GetRootAsPlayerAbilityWeightConversionList(bb);
                        }
                        break;
                        case E_DATA_TYPE.MatchCommentary:
                        {
                            pData = MatchCommentaryList.GetRootAsMatchCommentaryList(bb);
                        }
                        break;
                        case E_DATA_TYPE.LadderStandingReward:
                        {
                            pData = LadderStandingRewardList.GetRootAsLadderStandingRewardList(bb);
                        }
                        break;
                        case E_DATA_TYPE.LadderSeasonNo:
                        {
                            pData = LadderSeasonNoList.GetRootAsLadderSeasonNoList(bb);
                        }
                        break;
                        case E_DATA_TYPE.ConstValue:
                        {
                            ConstList constList = ConstList.GetRootAsConstList(bb);
                        
                            E_CONST_TYPE eConstType = E_CONST_TYPE.MAX;
                            for(int i = 0; i < constList.ConstValueLength; ++i)
                            {
                                if(Enum.TryParse<E_CONST_TYPE>(constList.ConstValue(i).Value.Id, out eConstType))
                                {
                                    constValueList.Add(eConstType,constList.ConstValue(i).Value.Value);
                                }
                            }
                            
                            if(constValueList.ContainsKey(E_CONST_TYPE.sendLog))
                            {
                                SingleFunc.RemoveSendLog();
                                if(constValueList[E_CONST_TYPE.sendLog] > 0)
                                {
#if FTM_LIVE
                                    SingleFunc.SetupSendLog("https://ftm.ncucu.com");
#else
                                    SingleFunc.SetupSendLog("http://dev-ftm.ncucu.com:23000");
#endif
                                }
                            } 
                        }
                        break;
                        default:continue;
                    }

                    if(pData != null)
                    {
                        AddFlatBufferData(dataType,pData);
                    }

                    pIntroScene?.SetInitInfo( count/(float)list.Count,GetLocalizingText("RESOURCEDOWNLOAD_TXT_RESET"));
         
                    yield return null;
                }
            }

            pIntroScene?.ShowInitInfo(false);

            // while(SoundManager.IsPreloadFile())
            // {
            //     yield return null;
            // }
            
            // SoundManager.Instance.VolumeSFX = 1;
            // LoadUserData();
            //유저 데이터에서 선박 초기화
            // gameData.InitData(m_pUserData.GetCurrentCruiseID());
            if(action != null)
            {
                action();
            }
        }
    }

    public string GetLocalizingText(string key)
    {
        if(string.IsNullOrEmpty(key)) return "";
        E_DATA_TYPE eType = (E_DATA_TYPE)m_pUserData.SettingInfo.Lang;
        
        if(dataList.ContainsKey(eType))
        {
            TextList pTextList = GetFlatBufferData<TextList>(eType);
            TextItem? pTextItem = pTextList.TextByKey(key);
            if(pTextItem != null)
            {
                return pTextItem.Value.Value;
            }
        }
        return key;
    }

    public string GetLocalizingErrorMsg(string key)
    {
        if(string.IsNullOrEmpty(key)) return "";
        
        E_DATA_TYPE eType = (E_DATA_TYPE)m_pUserData.SettingInfo.Lang;
        
        if(dataList.ContainsKey(eType))
        {    
            TextList pTextList = GetFlatBufferData<TextList>(eType);
            TextItem? pTextItem = pTextList.ErrorMsgByKey(key);
            if(pTextItem != null)
            {
                return pTextItem.Value.Value;
            }
        }
        return key;
    }

    public void SendReqAttendReward()
    {
        JObject pJObject = new JObject();
        pJObject["type"] = 1;
        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.attend_reward, GetNetworkAPI(E_REQUEST_ID.attend_reward),true,true,null,pJObject);
    }

    public void SendReqLeagueOpps()
    {
        int matchType = GetCurrentMatchType();
        if(IsLeagueOpen() && matchType > GameContext.CHALLENGE_ID)
        {
            JObject pJObject = new JObject();
            pJObject["matchType"] = matchType;
            NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.league_getTodayFixture, GetNetworkAPI(E_REQUEST_ID.league_getTodayFixture),true,true,null,pJObject);
        }
        else
        {
            SendReqClubLicensePut(true);
        }
    }

    public void SendReqClubLicensePut(bool bWait)
    {
        JObject pJObject = new JObject();
        pJObject["squadPower"] = GetTotalPlayerAbility(GetActiveLineUpType());
        pJObject["totalValue"] = GetTotalPlayerValue(0);
        pJObject["countQualified"] = GetTotalPlayerNAbilityTier(null,true);
        pJObject["avgAge"] = GetPlayerAvgAge(null,true);
        pJObject["playerCount"] = GetTotalPlayerCount();
    
        NetworkManager.Instance.SendRequest((uint)E_REQUEST_ID.clubLicense_put, GetNetworkAPI(E_REQUEST_ID.clubLicense_put),true,true,null,pJObject);
    }

//     public void AnalyticsLog(string strLog)
//     { 
//         Adjust.trackEvent(new AdjustEvent(strLog));
// #if USE_HIVE
// // 사용자 트래킹을 위한 이벤트 전송
//         // hive.Analytics.sendEvent(eLog.ToString());
// #endif
//         //     SingleFunc.AnalyticsFirebaseLog(name,new Parameter(FirebaseAnalytics.ParameterCharacter, "Normal"),new Parameter(FirebaseAnalytics.ParameterCharacter, $"{m_pUserData.CurrentNormalSize.W}x{m_pUserData.CurrentNormalSize.H}_{m_pUserData.CurrentNormalType}"),new Parameter(FirebaseAnalytics.ParameterLevel, tileMapData.CurrentLevel+1)); 
//     }

    // public void AnalyticsFirebaseLogSetting(string name,string typeName, bool bType)
    // {
    //     // SingleFunc.AnalyticsFirebaseLog(name,new Parameter(FirebaseAnalytics.ParameterCharacter,typeName),new Parameter(FirebaseAnalytics.ParameterValue, bType? 1:0)); 
    // }

	bool Init()
    {
        LoadUserData();
        return true;
    }
    
    public void Dispose()
    {
        // userData?.Dispose();
        m_pUserData = null;
        m_pClubInfo = null;
        m_pGameInfo = null;
    }
}
