using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ITEM;
using ALF;
using ALF.LAYOUT;
using ALF.STATE;
using ALF.MACHINE;
using STATEDATA;
using UnityEngine.EventSystems;
using USERDATA;
using USERRANK;
using DATA;
using MATCHTEAMDATA;
using ALF.SOUND;
using PUSH;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions; 
// using System.Runtime.CompilerServices;
// using GoogleMobileAds.Api;
// using GoogleMobileAds.Common;
using System.Globalization;


using System.Threading.Tasks;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using com.adjust.sdk;

#if USE_HIVE
using hive;
#else
using Facebook.Unity;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
// using GoogleMobileAds.Api.Mediation.UnityAds;
// using GoogleMobileAds.Api.Mediation.AdColony;
// using GoogleMobileAds.Api.Mediation;

#if UNITY_IPHONE || UNITY_IOS
using Unity.Notifications.iOS;

#elif UNITY_ANDROID
using Unity.Notifications.Android;
#endif

#endif



//woncomzdev
// android ca-app-pub-1754121709036440~6246048493
// iOS ca-app-pub-1754121709036440~2321269443

// 신규 안드로이드 
// 애드몹 앱 id : ca-app-pub-5466681013076873~5019236893
// IOS 
// 애드몹 앱 ID : ca-app-pub-5466681013076873~4635899999

public static class SingleFunc
{
    // private static InterstitialAd interstitial = null;
    // private static BannerView bannerView = null;
    // private static List<RewardedAd> rewardedAdList = new List<RewardedAd>();

    // private static System.Action<Reward> successRewardCallback = null;

    private static bool bFirstSend = false;
    static float fLastSendLogTime = 0;

    
    public static void SceneLoadCompleted(AsyncOperation op)
    {
        StateMachine.UnscheduleUpdate();
        StateCache.Instance.RemoveAll();
        
        op.completed -= SceneLoadCompleted;
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] list = scene.GetRootGameObjects();
        for(int i =0; i < list.Length; ++i)
        {
            if(list[i].name == "AFPool")
            {
                AssetData data = list[i].GetComponent<AssetData>();
                if(data != null)
                {
                    AFPool.Setup(data.afpool);
                    data.afpool = null; // afpool 포인터를 다른 곳으로 넘긴다.
                    GameObject.Destroy(list[i]);

                    ActorManager.InitInstance();
                    GameContext.GameDataLoad(()=>{Director.Instance.RunWithScene(IntroScene.Create(E_LOGIN_TYPE.auto),0);});
                    return;
                }
            }
        }

        ALFUtils.Assert(false, "can't find game object!! (AFPool)");
    }


#if !USE_HIVE
    #if UNITY_IOS
    private static string m_sWebClientId = "784084728577-dbk5dvibtmm2aq0am8rir11qbeadfrv1.apps.googleusercontent.com";
    #elif UNITY_ANDROID
    private static string m_sWebClientId = "784084728577-tblieraqtf7acd7oqd3pu2qjch45hst7.apps.googleusercontent.com";
    #elif ONESTORE
    private static string m_sWebClientId = "784084728577-61t7k3v4a6okt3fht8gs21e327vfvt1l.apps.googleusercontent.com";
    #else 
    private static string m_sWebClientId = "";
    #endif

    private static System.Action<object,bool> adCallback = null;


    // public static void HandleInitCompleteAction(InitializationStatus initStatus)
    // {
    //     Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
    //     foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
    //     {
    //         string className = keyValuePair.Key;
    //         AdapterStatus status = keyValuePair.Value;
    //         switch (status.InitializationState)
    //         {
    //             case AdapterState.NotReady:
    //                 // The adapter initialization did not complete.
    //                 Debug.Log("Adapter: " + className + " not ready.");
    //                 break;
    //             case AdapterState.Ready:
    //                 // The adapter was successfully initialized.
    //                 Debug.Log("Adapter: " + className + " is initialized.");
    //                 break;
    //         }
    //     }

    //     // Callbacks from GoogleMobileAds are not guaranteed to be called on
    //     // main thread.
    //     // In this example we use MobileAdsEventExecutor to schedule these calls on
    //     // the next Update() loop.

    //     // foreach(var item in initStatus.getAdapterStatusMap())
    //     // {            
    //     //     Debug.Log($"{item.Key}:---------------------item.Value.Description: {item.Value.Description} item.Value.InitializationState : {item.Value.InitializationState}");
    //     // }

    //     // MobileAdsEventExecutor.ExecuteInUpdate(() =>
    //     // {
    //     //     Debug.Log("---------------------MobileAdsEventExecutor.ExecuteInUpdate");
    //     //     // if(isShowBanner)
    //     //     {
    //     //         RequestBannerAd();
    //     //     }
    //     // });
    // }

//     public static void RequestRewardAd()
//     {
//         if(rewardedAdList.Count > 4) return;

// // #if UNITY_ANDROID
// //     adUnitId = "ca-app-pub-1754121709036440/2715900973";
// // #elif UNITY_IPHONE
// //     adUnitId = "ca-app-pub-1754121709036440/5371119946";
// // #else
// //     adUnitId = "unexpected_platform";
// // #endif
// // #if DEBUG_CHEAT
// //         #if UNITY_ANDROID
// //             string adUnitId = "ca-app-pub-3940256099942544/5224354917";
// //         #elif UNITY_IPHONE
// //             string adUnitId = "ca-app-pub-3940256099942544/1712485313";
// //         #else
// //             string adUnitId = "unexpected_platform";
// //         #endif
// // #else
//     #if UNITY_ANDROID
//         string adUnitId = "ca-app-pub-5466681013076873/3131440158";
//     #elif UNITY_IPHONE
//         string adUnitId = "ca-app-pub-5466681013076873/5565838282";
//     #else
//         string adUnitId = "unexpected_platform";
//     #endif
// // #endif

//         RewardedAd rewardedAd = new RewardedAd(adUnitId);

//         rewardedAd.OnAdLoaded += HandleOnAdLoaded;
//         rewardedAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;
//         rewardedAd.OnAdOpening += HandleOnAdOpened;
//         rewardedAd.OnAdClosed += HandleOnAdClosed;
//         rewardedAd.OnPaidEvent += HandleOnPaidEvent;
//         rewardedAd.OnAdFailedToShow += HandleOnAdFailedToShow;
//         rewardedAd.OnAdDidRecordImpression += HandleOnAdDidRecordImpression;
        
//         rewardedAd.OnUserEarnedReward += HandleOnUserEarnedReward;
        
//         rewardedAdList.Add(rewardedAd);        

//         AdRequest request = new AdRequest.Builder().Build();

//         // Set ad request parameters
        

//         // Load the rewarded ad with the request.
//         rewardedAd.LoadAd(request);   
//     }

//     public static void RequestInterstitial()
//     {
//         // #if UNITY_ANDROID
//         //     string adUnitId = "ca-app-pub-1754121709036440/2687980816";
//         // #elif UNITY_IOS
//         //     string adUnitId = "ca-app-pub-1754121709036440/9365177609";
//         // #else
//         //     string adUnitId = "unexpected_platform";
//         // #endif
// // #if DEBUG_CHEAT
// //         #if UNITY_ANDROID
// //             string adUnitId = "ca-app-pub-3940256099942544/1033173712";
// //         #elif UNITY_IOS
// //             string adUnitId = "ca-app-pub-3940256099942544/4411468910";
// //         #else
// //             string adUnitId = "unexpected_platform";
// //         #endif
// // #else
//         #if UNITY_ANDROID
//             string adUnitId = "ca-app-pub-5466681013076873/1626786799";
//         #elif UNITY_IOS
//             string adUnitId = "ca-app-pub-5466681013076873/7808858243";
//         #else
//             string adUnitId = "unexpected_platform";
//         #endif
// // #endif
//         if(interstitial != null)
//         {
//             interstitial.Destroy();
//         }
//         // Initialize an InterstitialAd.
//         interstitial = new InterstitialAd(adUnitId);

//         interstitial.OnAdLoaded += HandleOnAdLoaded;
//         interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
//         interstitial.OnAdOpening += HandleOnAdOpened;
//         interstitial.OnAdClosed += HandleOnAdClosed;
//         interstitial.OnPaidEvent += HandleOnPaidEvent;
//         interstitial.OnAdFailedToShow += HandleOnAdFailedToShow;
//         interstitial.OnAdDidRecordImpression += HandleOnAdDidRecordImpression;

//         AdRequest request = new AdRequest.Builder().Build();
//         // Load the interstitial with the request.
//         interstitial.LoadAd(request);
//     }

//     public static void RequestBannerAd(System.Action<object,bool> _openCallback)
//     {
//         // #if UNITY_ANDROID
//         //     string adUnitId = "ca-app-pub-1754121709036440/1758042526";
//         // #elif UNITY_IOS
//         //     string adUnitId = "ca-app-pub-1754121709036440/8232893488";
//         // #else
//         //     string adUnitId = "unexpected_platform";
//         // #endif

// // #if DEBUG_CHEAT
// //         #if UNITY_ANDROID
// //             string adUnitId = "ca-app-pub-3940256099942544/2934735716";
// //         #elif UNITY_IOS
// //             string adUnitId = "ca-app-pub-3940256099942544/2934735716";
// //         #else
// //             string adUnitId = "unexpected_platform";
// //         #endif
// // #else
//         #if UNITY_ANDROID
//             string adUnitId = "ca-app-pub-5466681013076873/2365153394";
//         #elif UNITY_IOS
//             string adUnitId = "ca-app-pub-5466681013076873/1243449894";
//         #else
//             string adUnitId = "unexpected_platform";
//         #endif
// // #endif
//         DestroyBannerView();

//         AdSize adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

//         // Create a 320x50 banner at the top of the screen.
//         bannerView = new BannerView(adUnitId, adaptiveSize, AdPosition.Bottom);
//         // bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);
        
//         adCallback = _openCallback;

//         bannerView.OnAdLoaded += HandleOnAdLoaded;
//         bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoad;
//         bannerView.OnAdOpening += HandleOnAdOpened;
//         bannerView.OnAdClosed += HandleOnAdClosed;
//         bannerView.OnPaidEvent += HandleOnPaidEvent;

//         AdRequest request = new AdRequest.Builder().Build();

//         // Load a banner ad.
//         bannerView.LoadAd(request);
//     }

//     public static bool ShowInterstitial(System.Action<object,bool> _openCallback)
//     {
//         if (interstitial.IsLoaded()) 
//         {
//             adCallback = _openCallback;
//             interstitial.Show();
//             return false;
//         }

//         return true;
//     }

//     public static bool IsWatchRewardAd()
//     {
//         for(int i = 0; i < rewardedAdList.Count; ++i)
//         {
//             if(rewardedAdList[i].IsLoaded())
//             {
//                 return true;
//             }
//         } 
//         // if (this.rewardedAd.IsLoaded()) {
//         //     this.rewardedAd.Show();
//         // }
//         return false;
//     }

//     public static void DestroyBannerView()
//     {
//         if(bannerView != null)
//         {
//             bannerView.Destroy();
//             bannerView = null;
//             adCallback = null;
//         }
//     } 

//     #region Banner callback handlers

//     static void HandleOnUserEarnedReward(object sender, Reward args)
//     {
//         Time.timeScale = 1;
//         if(successRewardCallback != null)
//         {
//             successRewardCallback(args);
//         }
//     }

//     static void HandleOnAdFailedToShow(object sender, AdErrorEventArgs args)
//     {
//         Time.timeScale = 1;
//         if(sender is RewardedAd ad)
//         {
//             RemoveRewardAd(ad);
//         }
     
//     }
//     static void HandleOnAdDidRecordImpression(object sender, EventArgs args)
//     {
//         if(sender is InterstitialAd interstitial)
//         {
//             // banner
//         }
//         else if(sender is RewardedAd ad)
//         {
            
//         }
//     }

//     static void HandleOnAdLoaded(object sender, EventArgs args)
//     {
//         if(sender is BannerView banner)
//         {
//             if(adCallback != null)
//             {
//                 adCallback(sender,false);
//                 adCallback = null;
//             }
//             // MonoBehaviour.print("HandleAdLoaded event received BannerView!!");
//             // banner
//         }
//         // else if(sender is InterstitialAd interstitial)
//         // {
//         //     // MonoBehaviour.print("HandleAdLoaded event received InterstitialAd!!");
//         //     // banner
//         // }
//         // else if(sender is RewardedAd ad)
//         // {
//         //     // MonoBehaviour.print("HandleAdLoaded event received RewardedAd!!");
//         //     // banner
//         // }
//     }

//     static void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
//     {
//         if(sender is BannerView banner)
//         {
//             MonoBehaviour.print("HandleOnAdFailedToLoad event received BannerView!!");
//             // banner
//         }
//         else if(sender is InterstitialAd interstitial)
//         {
//             if(!SoundManager.Instance.IsAllPause)
//             {
//                 SoundManager.Instance.ChangeBGMVolume(1, 0.5f,false);
//             }
//             if(adCallback != null)
//             {
//                 adCallback(sender,true);
//                 adCallback = null;
//             }
//             MonoBehaviour.print("HandleOnAdFailedToLoad event received interstitial!!");
//             RequestInterstitial();
//         }
//         else if(sender is RewardedAd ad)
//         {
//             if(!SoundManager.Instance.IsAllPause)
//             {
//                 SoundManager.Instance.ChangeBGMVolume(1, 0.5f,false);
//             }
//             if(adCallback != null)
//             {
//                 adCallback(sender,true);
//                 adCallback = null;
//             }
//             MonoBehaviour.print("HandleOnAdFailedToLoad event received RewardedAd!!");
//             RemoveRewardAd(ad);
//             RequestRewardAd();
//         }
//         MonoBehaviour.print("HandleFailedToReceiveAd event received with message: " + args.LoadAdError.ToString());
//     }

//     static void HandleOnAdOpened(object sender, System.EventArgs args)
//     {
//         if(sender is BannerView banner)
//         {
//             // MonoBehaviour.print("HandleOnAdOpened event received BannerView!!");
//             // banner
//         }
//         else 
//         {
//             Time.timeScale = 0;
//             if(!SoundManager.Instance.IsAllPause)
//             {
//                 SoundManager.Instance.ChangeBGMVolume(0, 0.5f,false);
//             }
//             if(adCallback != null)
//             {
//                 adCallback(sender,false);
//             }
            
//             if(sender is InterstitialAd interstitial)
//             {
                
//             }
//             else if(sender is RewardedAd ad)
//             {
                
//             }
//         }

//         MonoBehaviour.print("HandleAdOpened event received");
//     }

//     static void HandleOnAdClosed(object sender, System.EventArgs args)
//     {
//         if(sender is BannerView banner)
//         {
//             MonoBehaviour.print("HandleOnAdClosed event received BannerView!!");
//         }
//         else
//         {
//             Time.timeScale = 1;
//             if(!SoundManager.Instance.IsAllPause)
//             {
//                 SoundManager.Instance.ChangeBGMVolume(1, 0.5f,false);
//             }

//             if(adCallback != null)
//             {
//                 adCallback(sender,true);
//                 adCallback = null;
//             }
            
//             if(sender is InterstitialAd interstitial)
//             {
//                 RequestInterstitial();
//             }
//             else if(sender is RewardedAd ad)
//             {
//                 RemoveRewardAd(ad);
//                 RequestRewardAd();
//             }
//         } 
    
//         MonoBehaviour.print("HandleAdClosed event received");
//     }

//     static void HandleOnPaidEvent(object sender, AdValueEventArgs args)
//     {
//         if(sender is BannerView banner)
//         {
//             // banner
//         }
//         else if(sender is InterstitialAd interstitial)
//         {
//             // banner
//         }
//         else if(sender is RewardedAd ad)
//         {
//             // banner
//         }
//         MonoBehaviour.print("HandleOnPaidEvent event received");
//     }

//     static void RemoveRewardAd(RewardedAd ad)
//     {
//         for(int i =0; i < rewardedAdList.Count; ++i)
//         {
//             if(rewardedAdList[i] == ad)
//             {
//                 rewardedAdList.RemoveAt(i);
//                 return;
//             }
//         }
//     }

//     #endregion

    // public static bool ShowRewardAd(System.Action<object,bool> _openCallback,System.Action<GoogleMobileAds.Api.Reward> _successCallback)
    // public static bool ShowRewardAd(System.Action<object,bool> _openCallback,System.Action<bool> _successCallback)
    // {
    //     // for(int i =0; i < rewardedAdList.Count; ++i)
    //     // {
    //     //     if(rewardedAdList[i].IsLoaded())
    //     //     {
    //     //         adCallback = _openCallback;
    //     //         successRewardCallback = _successCallback;
    //     //         rewardedAdList[i].Show();
    //     //         return true;
    //     //     }
    //     // }

    //     if(_successCallback != null) // todo remove
    //     {
    //         _successCallback(false);
    //     }

    //     return true;
    //     // return false;
    // }

#endif

    /**
    * 게임에 사용하는 SDK 객체 초기화
    * HIVE SDK , Adjust ..
    */
    public static void InitializSDK()
    {
#if USE_HIVE
        HIVEUnityPlugin.InitPlugin();
#if FTM_LIVE
        AdjustEnvironment pAdjustEnvironment = AdjustEnvironment.Production;
        AdjustLogLevel logLevel = AdjustLogLevel.Info;
#else
        AdjustEnvironment pAdjustEnvironment = AdjustEnvironment.Sandbox;
        AdjustLogLevel logLevel = AdjustLogLevel.Verbose;
#endif        
        AdjustConfig adjustConfig = new AdjustConfig("5lrwqsh5x4hs", pAdjustEnvironment,logLevel == AdjustLogLevel.Suppress);
        adjustConfig.setLogLevel(logLevel);
        adjustConfig.setEventBufferingEnabled(false);
        adjustConfig.setLaunchDeferredDeeplink(true);
        adjustConfig.setSendInBackground(false);
        adjustConfig.setDefaultTracker(null);
        
        adjustConfig.setLogDelegate(msg => Debug.Log(msg));
        adjustConfig.setAppSecret(1 ,1616997654 ,1542458725 ,2030283062 ,232767124);
        adjustConfig.setUrlStrategy(AdjustUrlStrategy.Default.ToLowerCaseString());
        
        adjustConfig.setDelayStart(0);
        adjustConfig.setNeedsCost(false);
        adjustConfig.setPreinstallTrackingEnabled(false);
        adjustConfig.setPreinstallFilePath(null);
        adjustConfig.setAllowiAdInfoReading(true);
        adjustConfig.setAllowAdServicesInfoReading(true);
        adjustConfig.setAllowIdfaReading(true);
        adjustConfig.setCoppaCompliantEnabled(false);
        adjustConfig.setPlayStoreKidsAppEnabled(false);
        adjustConfig.setLinkMeEnabled(false);
        adjustConfig.deactivateSKAdNetworkHandling();
#if !FTM_LIVE
        adjustConfig.setEventSuccessDelegate(EventSuccessCallback);
        adjustConfig.setEventFailureDelegate(EventFailureCallback);
        adjustConfig.setSessionSuccessDelegate(SessionSuccessCallback);
        adjustConfig.setSessionFailureDelegate(SessionFailureCallback);
        adjustConfig.setDeferredDeeplinkDelegate(DeferredDeeplinkCallback);
        adjustConfig.setAttributionChangedDelegate(AttributionChangedCallback);
#endif
        Adjust.start(adjustConfig);
        GameContext.InitCtx();
        GameContext.getCtx().IsInitializeHive = false;
        Configuration.setHiveCertificationKey("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJIaXZlIiwiaWF0IjoxNjY1MDM1MTI4LCJqdGkiOiIxMzkyNDI4NzIzIn0.o-31fR4UXBCzV2sb1i37R2dTsNiH7l3vJ44SGpjesjU");
        AuthV4.setup(OnAuthV4Setup);
        return;
#else
        GameContext.InitCtx();
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) 
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                // FirebaseAnalytics.SetUserProperty( FirebaseAnalytics.UserPropertySignUpMethod,"Google");
                //   FirebaseAnalytics.SetUserId("uber_user_510");// Set the user ID.
                FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));// Set default session duration values.
            } 
            else 
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
        
        if (!FB.IsInitialized) 
        {
        // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else 
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }

        ALF.LOGIN.LoginManager.SetWebClientId(m_sWebClientId);
#endif
        
//         MobileAds.SetiOSAppPauseOnBackground(true);

// //         List<string> deviceIds = new List<string>() { AdRequest.TestDeviceSimulator };

// // //         // Initialize the Google Mobile Ads SDK.
#if UNITY_IOS
// //         deviceIds.Add("111f7585f987e2ed953de23e882ef120");
// // //         deviceIds.Add("5e99766ae2daeb06c2456f6d3f1944b7");
// // //         deviceIds.Add("ebc1fa66498c0b6a3649b17cd1a7a295");
// // //         deviceIds.Add("e7ccadf39219d10a760053f7e5f9feda");
// // //         deviceIds.Add("1ea5f416ab2ea984715ce99082650391");
// // //         deviceIds.Add("f4b02b5fc78898999bdceab1b11326ae");
// // //         deviceIds.Add("bd86c479e93e73c6d9e95f562531b283");
#elif UNITY_ANDROID
// //         deviceIds.Add("2EA0ED520695D302E874CD35D36AA241");
#endif
//          // Configure TagForChildDirectedTreatment and test device IDs.
//         // RequestConfiguration requestConfiguration = new RequestConfiguration.Builder().SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified).SetTestDeviceIds(deviceIds).build();
//         RequestConfiguration requestConfiguration = new RequestConfiguration.Builder().SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified).build();
//         MobileAds.SetRequestConfiguration(requestConfiguration);
//         MobileAds.Initialize(SingleFunc.HandleInitCompleteAction);
        
//         UnityAds.SetGDPRConsentMetaData(true);

//         // AdColonyAppOptions.SetTestMode(true);

//         AdColonyAppOptions.SetGDPRRequired(true);
//         AdColonyAppOptions.SetGDPRConsentString("1");


//         // AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
// // #if DEBUG_CHEAT
// //   MediationTestSuite.AdRequest = new AdRequest.Builder()
// //         .AddTestDevice("5e99766ae2daeb06c2456f6d3f1944b7")
// //         .AddTestDevice("5e99766ae2daeb06c2456f6d3f1944b7")
// //         .AddTestDevice("5e99766ae2daeb06c2456f6d3f1944b7")
// //         .Build();
// // #endif
      
    }

    public static void HiveLogEvent(string pCategory)
    {
        JSONObject pLogData = new JSONObject();
        pLogData.AddField("category", pCategory);
        // pLogData.AddField("lv", Accumulate.GetData(Stat.Cat_LevelUp));
        // pLogData.AddField("atk", Player.GetStat(Stat.atk).ToString());
        // pLogData.AddField("chapter", Player.GetChapterNow());
        // pLogData.AddField("stage", Player.GetStageNow());

        Analytics.sendAnalyticsLog(pLogData);
    }

    public static void AdjustEvent(string strLog, double revenue)
    { 
        if(string.IsNullOrEmpty(strLog)) return;
        AdjustEvent adjustEvent = new AdjustEvent(strLog);
        if(revenue > 0)
        {
            adjustEvent.setRevenue(revenue, "KRW");
        }
        
        Adjust.trackEvent(adjustEvent);
        
// #if USE_HIVE
// // 사용자 트래킹을 위한 이벤트 전송
//         // hive.Analytics.sendEvent(eLog.ToString());
// #endif
//         //     SingleFunc.AnalyticsFirebaseLog(name,new Parameter(FirebaseAnalytics.ParameterCharacter, "Normal"),new Parameter(FirebaseAnalytics.ParameterCharacter, $"{m_pUserData.CurrentNormalSize.W}x{m_pUserData.CurrentNormalSize.H}_{m_pUserData.CurrentNormalType}"),new Parameter(FirebaseAnalytics.ParameterLevel, tileMapData.CurrentLevel+1)); 
    }

    static void EventSuccessCallback(AdjustEventSuccess eventSuccessData)
    {
        Debug.Log($"EventSuccessCallback : \nMessage:{eventSuccessData.Message} \nTimestamp:{eventSuccessData.Timestamp} \nAdid:{eventSuccessData.Adid} \nEventToken:{eventSuccessData.EventToken} \nCallbackId:{eventSuccessData.CallbackId} \nJsonResponse:{eventSuccessData.GetJsonResponse()}");
    }

    static void EventFailureCallback(AdjustEventFailure eventFailureData)
    {
        Debug.Log($"EventFailureCallback : \nMessage:{eventFailureData.Message} \nTimestamp:{eventFailureData.Timestamp} \nAdid:{eventFailureData.Adid} \nEventToken:{eventFailureData.EventToken} \nCallbackId:{eventFailureData.CallbackId} \nJsonResponse:{eventFailureData.GetJsonResponse()}\n WillRetry :{eventFailureData.WillRetry}");
    }

    static void SessionSuccessCallback(AdjustSessionSuccess sessionSuccessData)
    {
        Debug.Log($"SessionSuccessCallback : \nMessage:{sessionSuccessData.Message} \nTimestamp:{sessionSuccessData.Timestamp} \nAdid:{sessionSuccessData.Adid} \nJsonResponse:{sessionSuccessData.GetJsonResponse()}");
    }

    static void SessionFailureCallback(AdjustSessionFailure sessionFailureData)
    {
        Debug.Log($"SessionFailureCallback : \nMessage:{sessionFailureData.Message} \nTimestamp:{sessionFailureData.Timestamp} \nAdid:{sessionFailureData.Adid} \nJsonResponse:{sessionFailureData.GetJsonResponse()}\n WillRetry :{sessionFailureData.WillRetry}");
    }

    static void DeferredDeeplinkCallback(string deeplinkURL)
    {
        Debug.Log($"DeferredDeeplinkCallback : \ndeeplinkURL:{deeplinkURL}");
    }

    static void AttributionChangedCallback(AdjustAttribution attributionData)
    {
        Debug.Log($"AttributionChangedCallback : \trackerName:{attributionData.trackerName} \trackerToken:{attributionData.trackerToken} \network:{attributionData.network} \nadgroup:{attributionData.adgroup} \ncreative:{attributionData.creative} \nclickLabel:{attributionData.clickLabel} \nadid:{attributionData.adid}");
    }


#if USE_HIVE

    public static void MarketConnect()
    {
        IAPV4.marketConnect(OnIAPV4MarketConnectCB);
    }

    static void OnAuthV4Setup(ResultAPI pResult, Boolean isAutoSignIn, String did, List<AuthV4.ProviderType> pProviderTypeList)
    {
        GameContext pGameContext = GameContext.getCtx();
        pGameContext.IsInitializeHive = pResult.isSuccess();
        if(pGameContext.IsInitializeHive)
        {
            int apple = 0;//(int)AuthV4.ProviderType.APPLE;
            int google = 0;//(int)AuthV4.ProviderType.GOOGLE;
            int facebook = 0;//(int)AuthV4.ProviderType.FACEBOOK;
            
            if (!Application.isEditor)
            {
                for(int i =0; i < pProviderTypeList.Count; ++i)
                {
                    if(pProviderTypeList[i] == AuthV4.ProviderType.FACEBOOK)
                    {
                        facebook = (int)AuthV4.ProviderType.FACEBOOK;
                    }
                    else if(pProviderTypeList[i] == AuthV4.ProviderType.SIGNIN_APPLE)
                    {
                        apple = (int)AuthV4.ProviderType.SIGNIN_APPLE;
                    }
                    else if(pProviderTypeList[i] == AuthV4.ProviderType.GOOGLE)
                    {
                        google = (int)AuthV4.ProviderType.GOOGLE;
                    }
                }
                pGameContext.InitializeWithADOPConsent();
            }
            pGameContext.ProviderTypeList = (apple << 16) | (google << 8) | (facebook << 0);
            Push.getRemotePush(OnRemotePushCB);
            MarketConnect();
        }
        else
        {
            if(Camera.main != null)
            {
                ApplicationLauncher pApplicationLauncher = Camera.main.GetComponent<ApplicationLauncher>();
                if(pApplicationLauncher != null)
                {
                    pApplicationLauncher.ShowMessagePopup("인터넷 연결 상태를 확인해주세요.",()=>{
                        pApplicationLauncher.DisposeMessagePopup();
                        AuthV4.setup(OnAuthV4Setup);
                    });
                }
            }
        }
    }

    static void OnRemotePushCB(ResultAPI pResult, RemotePush pRemotePush)
    {
        GameContext pGameContext = GameContext.getCtx();
        if (pResult.isSuccess())
        {
            pGameContext.SetPush(pRemotePush.isAgreeNotice);
            pGameContext.SetNightPush(pRemotePush.isAgreeNight);
        }
        else
        {
            pGameContext.SetPush(true);
            pGameContext.SetNightPush(true);
        }
    }

    static void OnIAPV4MarketConnectCB(ResultAPI pResult, List<IAPV4.IAPV4Type> pMarketIdList)
    {
        GameContext pGameContext = GameContext.getCtx();
        
        if(pResult.code == ResultAPI.Code.Success)
        {
            pGameContext.LoadProductInfo();
        }
        else
        {
            IBaseScene pScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
            if(pScene != null)
            {
                Message pMessage = pScene.GetInstance<Message>();
                pMessage.SetMessage(pGameContext.GetLocalizingErrorMsg(pResult.code.ToString()),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                SingleFunc.ShowAnimationDailog(pMessage.MainUI,null);
            }
        }
    }

// // 지급 완료된 아이템들의 완료 요청 결과 콜백 핸들러
// public void onIAPV4TransactionMultiFinishCB(List resultList, List marketPidList) {
// hive.Logger.log("IAPTestView.onIAPV4TransactionMultiFinishCB() CallbacknresultList = " + resultList + "nmarketPidList : " + marketPidList);
// };

// List marketPidList = new List();
// marketPidList.Add("{YOUR_PRODUCT_MARKET_PID_01}");
// marketPidList.Add("{YOUR_PRODUCT_MARKET_PID_02}");

// // 지급 완료된 아이템의 완료 요청
// hive.IAPV4.transactionMultiFinish(marketPidList);

    public static void ShowTerms(AuthV4.onAuthV4ShowTerms listener)
    {
        hive.AuthV4.showTerms(listener);
    }

    public static void IAPV4Restore(IAPV4.onIAPV4Restore pIAPV4RestoreCB)
    {
        IAPV4.restore(pIAPV4RestoreCB);
    }

    public static void SetServerID(string strServer)
    {
        Configuration.setServerId(strServer);
        // Configuration.setServerId(ConfigMgr.Instance.GetType());
// #if FTM_LIVE
//         // Configuration.setServerId("Live");
//         // hive.Configuration.setZone(ZoneType.REAL);
// #else
//         Configuration.setServerId("Dev");
//         // hive.Configuration.setZone(ZoneType.SANDBOX);
// #endif
    }

    public static void OutputPaylodData(JArray pArray, ref ulong finalGold,ref uint finalToken)
    {
        if(pArray == null || pArray.Count < 1) return;

        ulong money =0;
        uint cash =0;
        finalGold = 0;
        finalToken = 0;
        
        JObject item = null;
        for(int i =0; i < pArray.Count; ++i)
        {
            item = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>((string)pArray[i], new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
            money = (ulong)item["gold"];
            cash = (uint)item["token"];
            if(money > finalGold)
            {
                finalGold = money;
            }

            if(cash > finalToken)
            {
                finalToken = cash;
            }                
        }
    }    

    public static void SetGameLanguage(E_DATA_TYPE eType)
    {
        string token = "en";
        switch(eType)
        {
            case E_DATA_TYPE.ko_KR:
            {
                token = "ko";
            }
            break;
            case E_DATA_TYPE.zh_CN:
            {
                token = "zh-hans";
            }
            break;
            case E_DATA_TYPE.zh_TW:
            {
                token = "zh-hant";
            }
            break;
        }
        Configuration.setGameLanguage(token);
    }

#else
    
    static void InitCallback()
    {
        if (FB.IsInitialized) 
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        } 
        else 
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    static void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown) 
        {
            // Pause the game - we will need to hide
            // Time.timeScale = 0;
        }
        else 
        {
            // Resume the game - we're getting focus again
            // Time.timeScale = 1;
        }
    }

#endif

    // public static void AnalyticsFirebaseLog(string name, params Parameter[] parameters) 
    // {
    //     // Log an event with multiple parameters.
    //     //   DebugLog("Logging a level up event.");
    //     FirebaseAnalytics.LogEvent(name,parameters);
    //     //     new Parameter(FirebaseAnalytics.ParameterLevel, 5),
    //     //     new Parameter(FirebaseAnalytics.ParameterCharacter, "mrspoon"),
    // //     new Parameter("hit_accuracy", 3.14f));
    // }

    static void SendNotification(int id, int timeInterval, string title, string msg)
    {
#if USE_HIVE
        // 로컬 푸시 설정
        LocalPush localPush = new LocalPush(id,title,msg,timeInterval);

#if UNITY_ANDROID

		// // \~korean 이하 Android에서 로컬 푸시를 커스터마이징하기 위한 필드 정의
		// // \~english Followings are field definition for customizing local push on Android.

		// public String bigmsg;			///< \~korean 큰 글씨 \~english Big-text
		// public String ticker;			///< \~korean 메시지 티커 \~english Message Ticker
		// public String type;				///< \~korean 알림 형태 (bar, popup, toast 등) \~english Notification type (bar, popup, toast etc.)
		// public String icon;				///< \~korean 푸시 아이콘 \~english Push icon
		// public String sound;			///< \~korean 푸시 알림음 \~english Notification sound
		// public String active;			///< \~korean 수행할 동작 \~english Action to take

		// public String broadcastAction;
		// public int buckettype;
		// public int bucketsize;
		// public String bigpicture;
		// public String icon_color;

        localPush.icon = "ic_stat_notificationicon";
#endif
        // 로컬 푸시 등록하기
        hive.Push.registerLocalPush(localPush,null);
#else
        DateTime time = DateTime.Now.AddSeconds(timeInterval);
        
#if UNITY_IOS || UNITY_IPHONE

        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = time.Subtract(DateTime.Now),
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can specify a custom identifier which can be used to manage the notification later.
            // If you don't provide one, a unique string will be generated automatically.
            Identifier = id,
            Title = title,
            Body = msg,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "FTM",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);

        #elif UNITY_ANDROID

        int originalId = Int32.Parse(id);

        AndroidNotification n = new AndroidNotification();
        n.Title = title;
        n.Text = msg;
        n.FireTime = time;
        n.Group = "FTM";
        n.SmallIcon = "ic_c2s_notification_small_icon";
        // n.LargeIcon = "my_custom_large_icon_id";

        AndroidNotificationChannel c = new AndroidNotificationChannel();
        c.Id = "ftm_channel";
        c.Name = "ftm Channel";
        c.Description = "ftm channel";
        c.Importance = Importance.High;

        AndroidNotificationCenter.RegisterNotificationChannel(c);
        AndroidNotificationCenter.SendNotificationWithExplicitID(n, c.Id, originalId);

#endif

#endif
    }

    public static void SendLocalNotification()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.GetLocalPush())
        {
            PushList pPushList = pGameContext.GetFlatBufferData<PushList>(E_DATA_TYPE.Push);
            PushItem? pPushItem = null;
            int count = 2;
            if(pGameContext.IsLoadGameData())
            {    
                count = pPushList.PushLength;
            }
            int timeInterval = -1;
            for(int i =0; i < count; ++i)
            {
                pPushItem = pPushList.Push(i);
                timeInterval = pGameContext.IsSendLocalNotification(pPushItem);
                if(timeInterval > -1)
                {
                    SendNotification((i+1),timeInterval,pGameContext.GetLocalizingText(pPushItem.Value.Title),pGameContext.GetLocalizingText(pPushItem.Value.Text));
                }
            }
        }
    }

    public static void CancelLocalNotification()
    {
#if USE_HIVE
        hive.Push.unregisterAllLocalPushes();
#else
        #if UNITY_IOS || UNITY_IPHONE
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        #elif UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
        #endif
#endif
    }

    // // Reset analytics data for this app instance.
    // public static void ResetAnalyticsData() 
    // {
    // //   DebugLog("Reset analytics data.");
    //     FirebaseAnalytics.ResetAnalyticsData();
    // }

    // // Get the current app instance ID.
    // public static Task<string> DisplayAnalyticsInstanceId() 
    // {
    //     return FirebaseAnalytics.GetAnalyticsInstanceIdAsync().ContinueWithOnMainThread(task => {
    //         if (task.IsCanceled) {
    //         //   DebugLog("App instance ID fetch was canceled.");
    //         } else if (task.IsFaulted) {
    //         //   DebugLog(String.Format("Encounted an error fetching app instance ID {0}", task.Exception.ToString()));
    //         } else if (task.IsCompleted) {
    //         //   DebugLog(String.Format("App instance ID: {0}", task.Result));
    //         }
    //         return task;
    //     }).Unwrap();
    // }

    public static Vector2 GetSnapToPositionToBringChildIntoView(ScrollRect instance, Transform child)
    {
        Canvas.ForceUpdateCanvases();
        Vector2 viewportLocalPosition = instance.viewport.localPosition;
        Vector2 childLocalPosition = child.localPosition;
        Vector2 result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            0 - (viewportLocalPosition.y + childLocalPosition.y)
        );
        return result;
    }

    public static void ShowMoveDilog(Vector3 dir, RectTransform dlg,System.Action<IState> _enterCallback, System.Func<IState,float,bool,bool> _executeCallback, System.Func<IState,IState> _exitCallback,bool bAni)
    {
        if(dlg == null) return;
        // superBlurFast.enabled = true;
        LayoutManager.Instance.ShowPopup(dlg);
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),bAni ? 0.3f : 0.0f, (uint)E_STATE_TYPE.ShowDailog, _enterCallback, _executeCallback,_exitCallback);
        DilogMoveStateData data = new DilogMoveStateData();
        data.Direction = dir;
        data.FadeDelta = 1 / 0.3f;
        data.Out = false;
        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    public static void HideMoveDilog(Vector3 dir, RectTransform dlg,System.Action<IState> _enterCallback, System.Func<IState,float,bool,bool> _executeCallback, System.Func<IState,IState> _exitCallback,bool bAni)
    {
        if(dlg == null) return;
        LayoutManager.Instance.HidePopup(dlg);
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),bAni ? 0.3f : 0.0f, (uint)E_STATE_TYPE.HideDailog, _enterCallback, _executeCallback,_exitCallback);
        DilogMoveStateData data = new DilogMoveStateData();
        data.FadeDelta = 1 / 0.3f;
        data.Direction = dir;
        data.Out = true;
        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    public static void ShowAnimationDailog(RectTransform dlg,System.Action endCallback, string openAni = "Popup_open")
    {
        if(dlg == null) return;

        DailogStateData data = new DailogStateData(openAni,dlg.GetComponent<Animation>(),endCallback);
        
        dlg.gameObject.SetActive(true);
        AnimationClip pAnimationClip = data.Animation.GetClip(data.AnimationName);
        data.Animation.Play(data.AnimationName);
        LayoutManager.Instance.ShowPopup(dlg);
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),-1, (uint)E_STATE_TYPE.ShowDailog, null, SingleFunc.executeDailogCallback);
        LayoutManager.Instance.InteractableDisableAll();
        
        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }


    public static void HideAnimationDailog(RectTransform dlg,System.Action endCallback = null)
    {
        if(dlg == null) return;

        LayoutManager.Instance.HidePopup(dlg);

        if(endCallback == null)
        {
            endCallback = () => {
                LayoutManager.Instance.InteractableEnabledAll();
                dlg.Find("root").localScale = Vector3.one;
                dlg.gameObject.SetActive(false);
            };
        }

        DailogStateData data = new DailogStateData("Popup_hide",dlg.GetComponent<Animation>(),endCallback);

        AnimationClip pAnimationClip = data.Animation.GetClip(data.AnimationName);
        data.Animation.Play(data.AnimationName);
        
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(dlg),-1, (uint)E_STATE_TYPE.HideDailog, null, SingleFunc.executeDailogCallback);
        LayoutManager.Instance.InteractableDisableAll();

        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    static bool executeDailogCallback(IState state,float dt,bool bEnd)
    {
        if (state.StateData is DailogStateData data)
        {
            if(!data.Animation.IsPlaying(data.AnimationName))
            {
                if(data.EndCallback != null)
                {
                    data.EndCallback();
                }
                else
                {
                    LayoutManager.Instance.InteractableEnabledAll();
                }

                return true;
            }
        }

        return bEnd;
    }

    public static ulong GetPlayerValue(PlayerT pPlayerData)
    {
        if(pPlayerData == null ) return 0;

        float powerFactor1 = (65 - pPlayerData.AbilityTier) / 100f;
        float powerFactor2 = 120f / pPlayerData.AbilityTier;
        
        float divisor = Mathf.Pow(Mathf.Pow(100000f,powerFactor1),powerFactor2);
        float currentValue = 1000f + (200000000f - 1000f) / (1 + divisor);     //최소가 1000에 수렴, 최고가 2억에 수렴
        float potentialValue = Mathf.Pow(1.23f,pPlayerData.PotentialTier);
        byte age = GameContext.getCtx().GetPlayerAge(pPlayerData);
        if(age < 1)
        {
            age = pPlayerData.Age;
        }
        float ageRate = Mathf.Pow((28f / age),2.2f);
        
        return (ulong)((currentValue + potentialValue) * ageRate);
    }

    public static void ClearPlayerCard(Transform pCard)
    {
        if(pCard == null) return;
        
        Transform pGroup = pCard.Find("nation");
        if(pGroup != null)
        {
            pGroup.GetComponent<RawImage>().texture = null;
        }

        pGroup = pCard.Find("icon");
        if(pGroup != null)
        {
            ClearPlayerFace(pGroup);
        }

        pGroup = pCard.Find("roles");
        if(pGroup != null)
        {
            for(int n = 0; n < pGroup.childCount; ++n)
            {
                pGroup.GetChild(n).GetComponent<RawImage>().texture = null;
            }            
        }

        pGroup = pCard.Find("ability");
        if(pGroup != null)
        {
            RawImage icon = pGroup.GetComponent<RawImage>();
            if(icon != null)
            {
                icon.texture = null;    
            }
        }
    }

    public static void ClearPlayerFace(Transform pCard)
    {
        if(pCard == null) return;
        RawImage icon = pCard.Find("face").GetComponent<RawImage>();
        icon.texture = null;
        icon = pCard.Find("hair").GetComponent<RawImage>();
        icon.texture = null;
        icon = pCard.Find("eyebrow").GetComponent<RawImage>();
        icon.texture = null;
        icon = pCard.Find("eyes").GetComponent<RawImage>();
        icon.texture = null;
        icon = pCard.Find("nose").GetComponent<RawImage>();
        icon.texture = null;
        icon = pCard.Find("mouth").GetComponent<RawImage>();
        icon.texture = null;
    }

    public static void SetupPlayerFace(PlayerT pPlayerT,Transform pCard)
    {
        // face:ushort = 100(id:9);
    // hair:ushort = 200(id:10);
    // eyes:ushort = 300(id:11);
    // eyebrows:ushort = 400(id:12);
    // nose:ushort = 500(id:13);
    // mouth:ushort = 600(id:14);
        if(pCard == null) return;
        RawImage icon = pCard.Find("face").GetComponent<RawImage>();
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",$"player{pPlayerT.Montage[0]}");
        icon.texture = pSprite.texture;
        icon = pCard.Find("hair").GetComponent<RawImage>();
        pSprite = AFPool.GetItem<Sprite>("Texture",$"player{pPlayerT.Montage[1]}");
        icon.texture = pSprite.texture;
        icon = pCard.Find("eyes").GetComponent<RawImage>();
        pSprite = AFPool.GetItem<Sprite>("Texture",$"player{pPlayerT.Montage[2]}");
        icon.texture = pSprite.texture;
        icon = pCard.Find("eyebrow").GetComponent<RawImage>();
        pSprite = AFPool.GetItem<Sprite>("Texture",$"player{pPlayerT.Montage[3]}");
        icon.texture = pSprite.texture;
        icon = pCard.Find("nose").GetComponent<RawImage>();
        pSprite = AFPool.GetItem<Sprite>("Texture",$"player{pPlayerT.Montage[4]}");
        icon.texture = pSprite.texture;
        icon = pCard.Find("mouth").GetComponent<RawImage>();
        pSprite = AFPool.GetItem<Sprite>("Texture",$"player{pPlayerT.Montage[5]}");
        icon.texture = pSprite.texture;
    }

    public static void SetupPlayerCard(PlayerT pPlayerT,Transform pCard,E_ALIGN algin = E_ALIGN.Center,E_ALIGN talgin = E_ALIGN.Center)
    {
        if(pPlayerT == null || pCard == null) return;

        GameContext pGameContext = GameContext.getCtx();
        pCard.Find("DF")?.gameObject.SetActive(false);
        pCard.Find("FW")?.gameObject.SetActive(false);
        pCard.Find("MF")?.gameObject.SetActive(false);
        pCard.Find("GK")?.gameObject.SetActive(false);
        pCard.Find(pGameContext.GetDisplayBackFormationCardByLocationIndex(pPlayerT.Position))?.gameObject.SetActive(true);

        Transform pGroup = pCard.Find("nation");
        if(pGroup != null)
        {
            RawImage nation = pGroup.GetComponent<RawImage>();
            Sprite pSprite = AFPool.GetItem<Sprite>("Texture",pPlayerT.Nation);
            nation.texture = pSprite.texture;
        }

        pGroup = pCard.Find("icon");
        if(pGroup != null)
        {
            SetupPlayerFace(pPlayerT,pGroup);
        }

        pGroup = pCard.Find("position");
        if(pGroup != null)
        {
            TMPro.TMP_Text pPosition = pGroup.GetComponent<TMPro.TMP_Text>();
            int n = 0;
            string strPosition = "";
            for(n = 0; n < pPlayerT.PositionFamiliars.Count; ++n)
            {
                if(pPlayerT.PositionFamiliars[n] >= 80)
                {
                    strPosition += $"/{pGameContext.GetDisplayLocationName(n)}";
                }
            }

            pPosition.SetText(strPosition.Substring(1));
        }
   
        pGroup = pCard.Find("roles");
        if(pGroup != null)
        {
            TMPro.TMP_Text text = null;
            RawImage icon = null;
            Sprite pSprite = null;

            float width = pGroup.GetChild(0).GetComponent<RectTransform>().rect.width;
            float w = 0;
            Vector3 pos;

            int n =0;
            for(n = 0; n < pGroup.childCount; ++n)
            {
                pGroup.GetChild(n).gameObject.SetActive(false);                
            }
            
            List<string> locList= new List<string>();
            
            locList.Add(pGameContext.GetDisplayLocationName(pPlayerT.Position));
            for(n = 0; n < pPlayerT.PositionFamiliars.Count; ++n)
            {
                if(pPlayerT.Position != n && pPlayerT.PositionFamiliars[n] >= 80)
                {
                    locList.Add(pGameContext.GetDisplayLocationName(n));
                }
            }
            Transform tm = null;
            for(n = 0; n < locList.Count; ++n)
            {
                if(pGroup.childCount > n)
                {
                    tm = pGroup.GetChild(n);
                    icon = tm.GetComponent<RawImage>();
                    icon.gameObject.SetActive(true);

                    pos = tm.localPosition;
                    pos.x = w;
                    tm.localPosition = pos;
                    w += width;

                    pSprite = AFPool.GetItem<Sprite>("Texture",pGameContext.GetDisplayCardFormationByLocationName(locList[n]));
                    icon.texture = pSprite.texture;
                    text = icon.transform.Find("text").GetComponent<TMPro.TMP_Text>();
                    text.SetText(locList[n]);
                }
            }

            if(talgin == E_ALIGN.Center)
            {
                w = (w * -0.5f) + (width * 0.5f);
            }
            else if(talgin == E_ALIGN.Left)
            {
                w = (pGroup.GetComponent<RectTransform>().rect.width * -pGroup.GetComponent<RectTransform>().pivot.x) + (width * 0.5f);
            }
            else
            {
                w = (pGroup.GetComponent<RectTransform>().rect.width * pGroup.GetComponent<RectTransform>().pivot.x) + (width * -0.5f);
            }
            
            for(n = 0; n < pGroup.childCount; ++n)
            {
                tm = pGroup.GetChild(n);
                
                if(tm.gameObject.activeSelf)
                {
                    pos = tm.localPosition;
                    pos.x += w;
                    tm.localPosition = pos;
                }
            }  
        }

        pGroup = pCard.Find("ability");
        if(pGroup != null)
        {
            TMPro.TMP_Text pAbilityTier = pGroup.GetComponentInChildren<TMPro.TMP_Text>();
            pAbilityTier?.SetText(pPlayerT.AbilityTier.ToString());
            Sprite pSprite = null;
            if(pPlayerT.AbilityTier < 60)
            {
                pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier0");
            }
            else if(pPlayerT.AbilityTier < 70)
            {
                pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier1");
            }
            else if(pPlayerT.AbilityTier < 80)
            {
                pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier2");
            }
            else
            {
                pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier3");
            }

            RawImage icon = pGroup.GetComponent<RawImage>();
            icon.texture = pSprite.texture;
        }
        
        pGroup = pCard.Find("quality");
        if(pGroup != null)
        {
            SingleFunc.SetupQuality(pPlayerT,pGroup.GetComponent<RectTransform>(),false,algin);
        }
    }

    public static void UpdateTacticsData( TacticsT pTempTactics, LineupPlayerT pLineupPlayer, int teamIndex, ref MatchTeamDataT pMatchTeamData)
    {
        TacticsT pTacticsT = pMatchTeamData.TeamData[teamIndex].Tactics;

        CloneTacticsDataByIndex(pTempTactics,ref pTacticsT);

        pMatchTeamData.TeamData[teamIndex].Tactics = pTacticsT;

        if(pLineupPlayer != null)
        {
            int n =0;
            int i =0;
            ALFUtils.Assert(pLineupPlayer.Data.Count == pMatchTeamData.TeamData[teamIndex].PlayerData.Count, " UpdateTacticsData : pLineupPlayer.Data.Count != pMatchTeamData.TeamData[teamIndex].PlayerData.Count !!");
            PlayerDataT pPlayerData = null;
            
            for(n =0; n < pLineupPlayer.Data.Count; ++n)
            {
                pPlayerData = pMatchTeamData.TeamData[teamIndex].PlayerData[n];

                for(i =0; i < pMatchTeamData.TeamData[teamIndex].PlayerData.Count; ++i)
                {
                    if(pMatchTeamData.TeamData[teamIndex].PlayerData[i].PlayerId == pLineupPlayer.Data[n] && n != i)
                    {
                        pMatchTeamData.TeamData[teamIndex].PlayerData[n] = pMatchTeamData.TeamData[teamIndex].PlayerData[i];
                        pMatchTeamData.TeamData[teamIndex].PlayerData[i] = pPlayerData;
                        break;
                    }
                }
            }
        }
    }

    public static void ClearRankIcon(RawImage icon)
    {
        if(icon == null) return;
        icon.texture = null;
        if(icon.transform.Find("num"))
        {
            icon.transform.Find("num").GetComponent<RawImage>().texture = null;
        }
    }

    // static void OnRewardIconClick(BaseEventData e)
    // {
    //     if(e is PointerEventData data)
    //     {
    //         if(data.pointerClick != null)
    //         {
    //             Director.Instance.GetActiveBaseScene<MainScene>().ShowItemTipPopup(data.pointerClick);
    //         }
    //     }
    // }
    
    public static void SetupRewardIcon(Transform pReward, string strName,uint id,ulong amount)
    {
        RectTransform pItem = GetRewardIcon(pReward.Find("icon"),strName,id, amount);
        pItem.anchoredPosition = Vector2.zero;
        TMPro.TMP_Text text = null;
        string temp = null;
        if(pItem.Find("text"))
        {
            text = pItem.Find("text").GetComponent<TMPro.TMP_Text>();
            temp = text.text;
            text.SetText("");
        }
        else
        {
            temp = (id == GameContext.GAME_MONEY_ID || (id > 40 && id < 46))? ALFUtils.NumberToString(amount) : string.Format("{0:#,0}", amount);
        }
        
        text = pReward.Find("text").GetComponent<TMPro.TMP_Text>();        
        text.SetText(temp);
    }

    public static RectTransform GetRewardIcon(Transform parent, string strName,uint id,ulong amount)
    {
        Vector2 anchor = new Vector2(0.5f, 0.5f);
        RectTransform pReward = LayoutManager.Instance.GetItem<RectTransform>(strName);
        pReward.localScale = Vector3.one;
        pReward.anchorMax = anchor;
        pReward.anchorMin = anchor;
        pReward.pivot = anchor;
        pReward.SetParent(parent,false);
        
        if(id == GameContext.FREE_CASH_ID)
        {
            id = GameContext.CASH_ID;
        }
    
        RawImage icon = pReward.Find("icon").GetComponent<RawImage>();

        pReward.gameObject.name = $"{id}";
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",pReward.gameObject.name);
        icon.texture = pSprite.texture;
        Transform tm = pReward.Find("text");
        if(tm != null)
        {
            TMPro.TMP_Text text = tm.GetComponent<TMPro.TMP_Text>();
            text.SetText( (id == GameContext.GAME_MONEY_ID || (id > 40 && id < 46))? ALFUtils.NumberToString(amount) : string.Format("{0:#,0}", amount));
        }
        tm = pReward.Find("eff");
        if(tm != null)
        {
            tm.gameObject.SetActive(false);
        }

        GameContext pGameContext = GameContext.getCtx();
        ItemList pItemList = pGameContext.GetFlatBufferData<ItemList>(E_DATA_TYPE.ItemData);
        Item? pItem = pItemList.ItemByKey(id);
        if(pItem != null)
        {
            Button pButton = pReward.GetComponent<Button>();
            pButton.interactable = true;
            pButton.enabled = true;
            pButton.onClick.RemoveAllListeners();
            pButton.onClick.AddListener( delegate { 
                Director.Instance.GetActiveBaseScene<MainScene>().ShowItemTipPopup(pButton.gameObject);
            });
        }
        
        // EventTrigger pEventTrigger = pReward.GetComponent<EventTrigger>();
        // EventTrigger.Entry pEntry = new EventTrigger.Entry();
        // pEntry.eventID = EventTriggerType.PointerClick;
        // pEntry.callback.AddListener(OnRewardIconClick);
        // pEventTrigger.triggers.Add(pEntry);

        return pReward;
    }

    public static void AddRewardIcon(RectTransform pReward,string strName)
    {
        if(pReward == null) return;
        
        Vector2 anchor = new Vector2(0.5f, 0.5f);
        RawImage icon = pReward.Find("icon").GetComponent<RawImage>();
        icon.texture = null;
        if(pReward.Find("receive") != null)
        {
            icon = pReward.Find("receive").GetComponent<RawImage>();
            icon.texture = null;
        }

        if(pReward.Find("eff") != null)
        {
            pReward.Find("eff").localScale = Vector2.one;
        }

        pReward.anchorMax = anchor;
        pReward.anchorMin = anchor;
        pReward.pivot = anchor;
        Button pButton = pReward.GetComponent<Button>();
        if(pButton != null)
        {
            pButton.onClick.RemoveAllListeners();
        }
        
        // UnityEngine.EventSystems.EventTrigger pEventTrigger = pReward.GetComponent<UnityEngine.EventSystems.EventTrigger>();

        // for(int n = 0; n < pEventTrigger.triggers.Count; ++n)
        // {
        //     pEventTrigger.triggers[n].callback.RemoveAllListeners();
        //     pEventTrigger.triggers[n] = null;
        // }
        // pEventTrigger.triggers.Clear();

        LayoutManager.Instance.AddItem(strName,pReward);
    }

    public static void SetupRankIcon(RawImage icon,byte rank)
    {
        if(icon == null) return;

        Sprite pSprite = null;
        if(rank <= 5)
        {
            pSprite = ALF.AFPool.GetItem<Sprite>("Texture","rank_5");
        }
        else if(rank <= 15)
        {
            pSprite = ALF.AFPool.GetItem<Sprite>("Texture","rank_15");
            rank -=5;
        }
        else if(rank <= 25)
        {
            pSprite = ALF.AFPool.GetItem<Sprite>("Texture","rank_25");
            rank -=15;
        }
        else if(rank <= 35)
        {
            pSprite = ALF.AFPool.GetItem<Sprite>("Texture","rank_35");
            rank -=25;
        }
        else
        {
            pSprite = ALF.AFPool.GetItem<Sprite>("Texture","rank_40");
            rank -=30;
        }
        
        icon.texture = pSprite.texture;
        Transform num = icon.transform.Find("num");
        if(num != null)
        {
            pSprite = ALF.AFPool.GetItem<Sprite>("Texture",$"num{rank}");
            num.GetComponent<RawImage>().texture = pSprite.texture;
        }
    }

    public static void CloneTacticsDataByIndex(TacticsT pSrcTactics,ref TacticsT pTactics)
    {
        if(pSrcTactics == null || pTactics == null) return;

        if(pTactics.Formation == null)
        {
            pTactics.Formation = new List<byte>();
        }
        if(pTactics.TeamTactics == null)
        {
            pTactics.TeamTactics = new List<byte>();
        }
        if(pTactics.PlayerTactics == null)
        {
            pTactics.PlayerTactics = new List<PlayerTacticsT>();
        }
        
        pTactics.Formation.Clear();
        pTactics.TeamTactics.Clear();
        pTactics.PlayerTactics.Clear();
        
        int i =0;
        int n =0;
        for(i =0; i < pSrcTactics.Formation.Count; ++i)
        {
            pTactics.Formation.Add(pSrcTactics.Formation[i]);
        }
        for(i =0; i < pSrcTactics.TeamTactics.Count; ++i)
        {
            pTactics.TeamTactics.Add(pSrcTactics.TeamTactics[i]);
        }
        
        PlayerTacticsT pPlayerTactics = null;
        for(i =0; i < pSrcTactics.PlayerTactics.Count; ++i)
        {
            pPlayerTactics = new PlayerTacticsT();
            pPlayerTactics.Tactics = new List<byte>();
            for(n =0; n < pSrcTactics.PlayerTactics[i].Tactics.Count; ++n)
            {
                pPlayerTactics.Tactics.Add(pSrcTactics.PlayerTactics[i].Tactics[n]);
            }
            
            pTactics.PlayerTactics.Add(pPlayerTactics);
        }
    }

    public static void SetupQuality(PlayerT pPlayerT,Transform pGroup,bool bTier = false,E_ALIGN algin = E_ALIGN.Center)
    {
        if(pPlayerT == null || pGroup == null) return;
        int n = 0;
        
        GameContext pGameContext = GameContext.getCtx();
        UserRankList pUserRankList = pGameContext.GetFlatBufferData<UserRankList>(E_DATA_TYPE.UserRank);
        UserRankItem? pUserRankItem = pUserRankList.UserRankByKey(pGameContext.GetCurrentUserRank());
        
        Transform tm = null;
        
        for(n = 0; n < pGroup.childCount; ++n)
        {
            tm = pGroup.GetChild(n);
            tm.gameObject.SetActive(false);
            tm.Find("h")?.gameObject.SetActive(false);
            tm = tm.Find("on");
            tm?.gameObject.SetActive(false);
        }
        
        int rate = 1;
        float width = pGroup.GetChild(0).GetComponent<RectTransform>().rect.width;
        float w = 0;
        Vector3 pos;
        if(bTier)
        {
            rate = UnityEngine.Mathf.RoundToInt((float)(pPlayerT.PotentialTier - pUserRankItem.Value.PlayerTierMin) / (float)( pUserRankItem.Value.PlayerTierMax - pUserRankItem.Value.PlayerTierMin) * 10);
            int count = rate / 2;
            for(n = 0; n < count; ++n)
            {
                tm = pGroup.GetChild(n);
                tm.gameObject.SetActive(true);
                pos = tm.localPosition;
                pos.x = w;
                tm.localPosition = pos;
                w += width;
                if(n < count && n == 4)
                {
                    w += width*0.4f;
                }
            }
            
            if(rate % 2 == 1)
            {
                tm = pGroup.Find("h");
                pos = tm.localPosition;
                pos.x = w;
                tm.localPosition = pos;
                tm.gameObject.SetActive(true);
                w += width;
            }

            if(algin == E_ALIGN.Center)
            {
                w = (w * -0.5f) + (width * 0.5f);
            }
            else if(algin == E_ALIGN.Left)
            {
                w = (pGroup.GetComponent<RectTransform>().rect.width * -pGroup.GetComponent<RectTransform>().pivot.x) + (width * 0.5f);
            }
            else
            {
                w = (pGroup.GetComponent<RectTransform>().rect.width * pGroup.GetComponent<RectTransform>().pivot.x) + (width * -0.5f);
            }

            for(n = 0; n < pGroup.childCount; ++n)
            {
                tm = pGroup.GetChild(n);
                
                if(tm.gameObject.activeSelf)
                {
                    pos = tm.localPosition;
                    pos.x += w;
                    tm.localPosition = pos;
                }
            }

            int rate2 = UnityEngine.Mathf.RoundToInt((float)(pPlayerT.AbilityTier - pUserRankItem.Value.PlayerTierMin) / (float)( pUserRankItem.Value.PlayerTierMax - pUserRankItem.Value.PlayerTierMin) * 10);
            
            if(pUserRankItem.Value.PlayerTierMin >= pPlayerT.AbilityTier)
            {
                rate2 = 1;
            }

            for(n = 0; n < rate2 / 2; ++n)
            {
                pGroup.GetChild(n).Find("on").gameObject.SetActive(true);
            }

            if(rate2 % 2 == 1)
            {
                n = rate2 / 2;
                pGroup.GetChild(n).Find("h").gameObject.SetActive(true);
            }
        }
        else
        {
            if(pUserRankItem.Value.PlayerTierMin >= pPlayerT.PotentialTier)
            {
                rate = 1;
            }
            else if(pUserRankItem.Value.PlayerTierMax < pPlayerT.PotentialTier)
            {
                rate = 10;
                pGroup.Find("plus").gameObject.SetActive(true);
            }
            else
            {
                rate = UnityEngine.Mathf.RoundToInt((float)(pPlayerT.PotentialTier - pUserRankItem.Value.PlayerTierMin) / (float)( pUserRankItem.Value.PlayerTierMax - pUserRankItem.Value.PlayerTierMin) * 10);
            }

            for(n = 0; n < rate / 2; ++n)
            {
                tm = pGroup.GetChild(n);
                tm.gameObject.SetActive(true);
                pos = tm.localPosition;
                pos.x = w;
                tm.localPosition = pos;
                w += width;
            }
            
            if(rate % 2 == 1)
            {
                tm = pGroup.Find("h_star");
                pos = tm.localPosition;
                pos.x = w;
                tm.localPosition = pos;
                tm.gameObject.SetActive(true);
                w += width;
            }
            
            if(algin == E_ALIGN.Center)
            {
                w = (w * -0.5f) + (width * 0.5f);
            }
            else if(algin == E_ALIGN.Left)
            {
                w = (pGroup.GetComponent<RectTransform>().rect.width * -pGroup.GetComponent<RectTransform>().pivot.x) + (width * 0.5f);
            }
            else
            {
                w = (pGroup.GetComponent<RectTransform>().rect.width * pGroup.GetComponent<RectTransform>().pivot.x) + (width * -0.5f);
            }
            
            for(n = 0; n < pGroup.childCount; ++n)
            {
                tm = pGroup.GetChild(n);
                
                if(tm.gameObject.name != "plus" && tm.gameObject.activeSelf)
                {
                    pos = tm.localPosition;
                    pos.x += w;
                    tm.localPosition = pos;
                }
            }

            int rate2 = 0;
            if(pUserRankItem.Value.PlayerTierMin >= pPlayerT.AbilityTier)
            {
                rate2 = 1;
            }
            else if(pUserRankItem.Value.PlayerTierMax < pPlayerT.AbilityTier)
            {
                rate2 = 10;
                pGroup.Find("plus").Find("on").gameObject.SetActive(true);
            }
            else
            {
                rate2 = UnityEngine.Mathf.RoundToInt((float)(pPlayerT.AbilityTier - pUserRankItem.Value.PlayerTierMin) / (float)( pUserRankItem.Value.PlayerTierMax - pUserRankItem.Value.PlayerTierMin) * 10);
            }

            for(n = 0; n < rate2 / 2; ++n)
            {
                pGroup.GetChild(n).Find("on").gameObject.SetActive(true);
            }

            if(rate2 == rate)
            {
                pGroup.Find("h_star").Find("on").gameObject.SetActive(true);
            }
            else
            {
                if(rate2 % 2 == 1)
                {
                    n = rate2 / 2;
                    pGroup.GetChild(n).Find("h").gameObject.SetActive(true);
                }
            }
        }
    }

    public static void SetupLocalizingText(Transform pNode)
    {
        if(pNode == null) return;

        LocalizingText[] list = pNode.GetComponentsInChildren<LocalizingText>(true);
        for(int i =0; i < list.Length; ++i)
        {
            list[i].UpdateLocalizing();
        }   
    }

    public static void UpdateTimeText(int second, TMPro.TMP_Text pText,byte eType )
    {
        int index = -1;
        List<int> list = ALFUtils.Time2String(second,ref index);
        GameContext pGameContext =GameContext.getCtx();
        switch(index)
        {
            case 2:
            {
                index = list.Count;
                int d =0;
                int h =0;
                if(index == 1)
                {
                    h = list[0];
                }
                else if(index == 2)
                {
                    d = list[0];
                    h = list[1];
                }

                if(eType == 0)
                {
                    pText.SetText(string.Format( pGameContext.GetLocalizingText("TIMECOUNT_TXT_DAY_HOUR"),d,h));
                }
                else if(eType == 1)
                {
                    if(d > 0)
                    {
                        pText.SetText(string.Format( pGameContext.GetLocalizingText("TIMECOUNT_TXT_EXPIRE_TIME_DAY"),d));
                    }
                    else
                    {
                        pText.SetText(string.Format( pGameContext.GetLocalizingText("TIMECOUNT_TXT_EXPIRE_TIME_HOUR"),h));
                    }
                }
                else
                {
                    if(d > 0)
                    {
                        pText.SetText(string.Format( pGameContext.GetLocalizingText("TIMECOUNT_TXT_RECEIVE_TIME_DAY"),d));
                    }
                    else
                    {
                        pText.SetText(string.Format( pGameContext.GetLocalizingText("TIMECOUNT_TXT_RECEIVE_TIME_HOUR"),h));
                    }
                }
            }
            break;
            case 3:
            {
                index = list.Count;
                int h =0;
                int m =0;
                if(index == 1)
                {
                    m = list[0];
                }
                else if(index == 2)
                {
                    h = list[0];
                    m = list[1];
                }

                if(eType == 0)
                {
                    pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_HOUR_MINUTE"),h,m));
                }
                else if(eType == 1)
                {
                    if(h > 0)
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_EXPIRE_TIME_HOUR"),h));
                    }
                    else
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_EXPIRE_TIME_MINUTE"),m));
                    }
                }
                else
                {
                    if(h > 0)
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_RECEIVE_TIME_HOUR"),h));
                    }
                    else
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_RECEIVE_TIME_MINUTE"),m));
                    }
                }
            }
            break;
            case 4:
            {
                index = list.Count;

                if(eType == 0)
                {
                    if(index == 1)
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_SECOND"),list[0]));
                    }
                    else if(index == 2)
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_MINUTE_SECOND"),list[0],list[1]));
                    }
                }
                else if(eType == 1)
                {
                    if(index == 1)
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_EXPIRE_TIME_SECOND"),list[0]));
                    }
                    else if(index == 2)
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_EXPIRE_TIME_MINUTE"),list[0]));
                    }
                }
                else
                {
                    if(index == 1)
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_RECEIVE_TIME_SECOND"),list[0]));
                    }
                    else if(index == 2)
                    {
                        pText.SetText(string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_RECEIVE_TIME_MINUTE"),list[0]));
                    }
                }
            }
            break;

            default :
            {
                if(eType == 0)
                {
                    pText.SetText(second == 0 ? string.Format(pGameContext.GetLocalizingText("TIMECOUNT_TXT_SECOND"),second) : "");
                }
                else if(eType == 1)
                {
                    pText.SetText(second == 0 ? pGameContext.GetLocalizingText("TIMECOUNT_TXT_EXPIRED"): "");
                }
                else
                {
                    pText.SetText(second == 0 ? pGameContext.GetLocalizingText("TIMECOUNT_TXT_RECEIVE_TIME_NOW") : "");
                }
            }
            break;
        }
    }

    public static Bounds InternalGetBounds(Vector3[] corners, Matrix4x4 viewWorldToLocalMatrix)
    {
        Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Vector3 v;
        for (int j = 0; j < 4; j++)
        {
            v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        Bounds bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);
        return bounds;
    }

    private static readonly Vector3[] m_pCorners = new Vector3[4];
    public static Bounds GetBoundsItem(RectTransform pItem,Matrix4x4 viewWorldToLocalMatrix )
    {
        // if (m_Content == null)
        //     return new Bounds();

        // int offset = index - itemTypeStart;
        // if (offset < 0 || offset >= m_Content.childCount)
        //     return new Bounds();

        // var rt = m_Content.GetChild(offset) as RectTransform;
        // if (rt == null)
        //     return new Bounds();
        // rt.GetWorldCorners(m_Corners);

        // var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
        // return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
        // RectTransform pItem = scroller.content.GetChild(index).GetComponent<RectTransform>();
        pItem.GetWorldCorners(m_pCorners);

        return SingleFunc.InternalGetBounds(m_pCorners, viewWorldToLocalMatrix);
    }

    public static void ScrollViewChangeValue( this ScrollRect scroller, Vector2 pDir,ref int iStartIndex,System.Action<int,int,int> pCallback = null)
    {
        Bounds pViewBounds = new Bounds(scroller.viewport.rect.center, scroller.viewport.rect.size);
        Bounds pBounds;
        bool bActive = false;
        List<int> moveList = new List<int>();
        Vector2 pos;
        RectTransform pItem = null;
        
        if( scroller.horizontal ) 
        {
            if(pDir.x == 0) 
            {
                scroller.velocity =Vector2.zero;
                return;
            }
            
            int i = 0;
            int n = 0;
            
            if(pDir.x > 0)
            {   
                pItem = scroller.content.GetChild(scroller.content.childCount -1).GetComponent<RectTransform>();
                float fMax = scroller.content.rect.width - pItem.rect.width;
                if(pItem.anchoredPosition.x > fMax)
                {
                    pItem.gameObject.SetActive(false);
                    scroller.velocity =Vector2.zero;
                    return;
                }
                i = 0;
                n = 1;
            }
            else
            {
                pItem = scroller.content.GetChild(0).GetComponent<RectTransform>();
                if(pItem.anchoredPosition.x < 0)
                {
                    pItem.gameObject.SetActive(false);
                    scroller.velocity =Vector2.zero;
                    return;
                }
                i = scroller.content.childCount -1;
                n = -1;
            }

            pBounds = GetBoundsItem(pItem,scroller.viewport.worldToLocalMatrix);
            bActive = pViewBounds.Intersects(pBounds);

            if(bActive)
            {
                int total = scroller.content.childCount;
                float fMax = scroller.content.rect.width - pItem.rect.width;
                while(total > 0 )
                {
                    --total;
                    pItem = scroller.content.GetChild(i).GetComponent<RectTransform>();
                    pBounds = GetBoundsItem(pItem,scroller.viewport.worldToLocalMatrix);
                    bActive = pViewBounds.Intersects(pBounds);
                    pItem.gameObject.SetActive(bActive);
                    if(!bActive)
                    {
                        moveList.Add(i);
                    }
                    else
                    {
                        pos = pItem.anchoredPosition;
                        if(pos.x < 0 || pos.x > fMax)
                        {
                            pItem.gameObject.SetActive(false);
                        }
                    }
                    
                    i += n;
                }   
            }
        }
        else if( scroller.vertical ) 
        {
            if(pDir.y == 0)
            {
                scroller.velocity =Vector2.zero;
                return;
            }
            
            int i = 0;
            int n = 0;
            
            if(pDir.y > 0)
            {   
                pItem = scroller.content.GetChild(0).GetComponent<RectTransform>();
                if(pItem.anchoredPosition.y > 0)
                {
                    pItem.gameObject.SetActive(false);
                    scroller.velocity =Vector2.zero;
                    return;
                }
                i = scroller.content.childCount -1;
                n = -1;
            }
            else
            {
                pItem = scroller.content.GetChild(scroller.content.childCount -1).GetComponent<RectTransform>();
                float fMax = -scroller.content.rect.height + pItem.rect.height;
                if(pItem.anchoredPosition.y < fMax)
                {
                    pItem.gameObject.SetActive(false);
                    scroller.velocity =Vector2.zero;
                    return;
                }
                i = 0;
                n = 1;
            }

            pBounds = GetBoundsItem(pItem,scroller.viewport.worldToLocalMatrix);
            bActive = pViewBounds.Intersects(pBounds);

            if(bActive)
            {
                int total = scroller.content.childCount;
                float fMax = -scroller.content.rect.height + pItem.rect.height;
                while(total > 0 )
                {
                    --total;
                    pItem = scroller.content.GetChild(i).GetComponent<RectTransform>();
                    pBounds = GetBoundsItem(pItem,scroller.viewport.worldToLocalMatrix);
                    bActive = pViewBounds.Intersects(pBounds);
                    pItem.gameObject.SetActive(bActive);
                    if(!bActive)
                    {
                        moveList.Add(i);
                    }
                    else
                    {
                        pos = pItem.anchoredPosition;
                        if(pos.y > 0 || pos.y < fMax)
                        {
                            pItem.gameObject.SetActive(false);
                        }
                    }
                    
                    i += n;
                }   
            }
        }

        if(moveList.Count > 0)
        {
            int i =0;
            int index = 0;
            int total = moveList.Count -1;
            int count = scroller.content.childCount -1;
            if( scroller.horizontal ) 
            {
                if(pDir.x > 0)
                {
                    iStartIndex += moveList.Count;
                    
                    for(i =0; i < moveList.Count; ++i)
                    {
                        index = moveList[i] -i;
                        pItem = scroller.content.GetChild(count).GetComponent<RectTransform>();
                        pos = pItem.anchoredPosition;
                        pos.x += pItem.rect.width;
                        pItem = scroller.content.GetChild(index).GetComponent<RectTransform>();
                        pItem.anchoredPosition = pos;
                        pItem.gameObject.SetActive(true);
                        if(pCallback != null)
                        {
                            pCallback(index,count,moveList[total - i]*-1);
                        }
                        pItem.SetAsLastSibling();
                    }
                }
                else
                {
                    iStartIndex -= moveList.Count;
                    for(i =0; i < moveList.Count; ++i)
                    {
                        index = moveList[i] +i;
                        pItem = scroller.content.GetChild(0).GetComponent<RectTransform>();
                        pos = pItem.anchoredPosition;
                        pos.x -= pItem.rect.width;
                        pItem = scroller.content.GetChild(index).GetComponent<RectTransform>();
                        pItem.anchoredPosition = pos;
                        pItem.gameObject.SetActive(true);
                        if(pCallback != null)
                        {
                            pCallback(index,0,total - i);
                        }
                        
                        pItem.SetAsFirstSibling();
                    }
                }
            }
            else
            {
                if(pDir.y > 0)
                {
                    iStartIndex -= moveList.Count;
                    for(i =0; i < moveList.Count; ++i)
                    {
                        index = moveList[i] +i;
                        pItem = scroller.content.GetChild(0).GetComponent<RectTransform>();
                        pos = pItem.anchoredPosition;
                        pos.y += pItem.rect.height;
                        pItem = scroller.content.GetChild(index).GetComponent<RectTransform>();
                        pItem.anchoredPosition = pos;
                        pItem.gameObject.SetActive(true);
                        if(pCallback != null)
                        {
                            pCallback(index,0, total - i);
                        }
                        pItem.SetAsFirstSibling();
                    }
                }
                else
                {
                    iStartIndex += moveList.Count;
                    for(i =0; i < moveList.Count; ++i)
                    {
                        index = moveList[i] -i;
                        pItem = scroller.content.GetChild(count).GetComponent<RectTransform>();
                        pos = pItem.anchoredPosition;
                        pos.y -= pItem.rect.height;
                        pItem = scroller.content.GetChild(index).GetComponent<RectTransform>();
                        pItem.anchoredPosition = pos;
                        pItem.gameObject.SetActive(true);
                        if(pCallback != null)
                        {
                            pCallback(index,count, moveList[total - i]*-1);
                        }
                        pItem.SetAsLastSibling();
                    }
                }
            }
        }
    }

    public static void SnapTo( this ScrollRect scroller, RectTransform child )
    {
        Canvas.ForceUpdateCanvases();

        Vector2 endPos = (Vector2)scroller.transform.InverseTransformPoint( scroller.content.position ) - (Vector2)scroller.transform.InverseTransformPoint( child.position );
        // If no horizontal scroll, then don't change contentPos.x
        if( !scroller.horizontal ) 
        {
            endPos.x = scroller.content.anchoredPosition.x;
        }
        // If no vertical scroll, then don't change contentPos.y
        if( !scroller.vertical ) 
        {
            endPos.y = scroller.content.anchoredPosition.y;
        }
        scroller.content.anchoredPosition = endPos;
    }
    public static string CheckStringLength(string str,int limit)
    {
        string temp = str.Replace('\n','\t');
        int count = 0;
        char[] charArr = temp.ToCharArray();
        for(int i =0; i < charArr.Length; ++i)
        {
            count += char.GetUnicodeCategory(charArr[i])==System.Globalization.UnicodeCategory.OtherLetter ? 2 : 1;
            if(count >= limit) return temp.Substring(0,i) +"...";
        }

        return temp;
    }

    public static byte[] CreateRandomEmblem()
    {
        return new byte[]{(byte)UnityEngine.Random.Range(0,10),(byte)UnityEngine.Random.Range(0,10),(byte)UnityEngine.Random.Range(0,10),(byte)UnityEngine.Random.Range(0,255),(byte)UnityEngine.Random.Range(0,255),(byte)UnityEngine.Random.Range(0,255),0};
    }

    public static bool IsMatchString(string str, string token)
    {
        if(string.IsNullOrEmpty(str) || string.IsNullOrEmpty(token)) return false;
        // Regex regex = new Regex(@"^01[01678]-[0-9]{4}-[0-9]{4}$");
        Regex regex = new Regex(token);
        return regex.IsMatch(str);
    }

    public static byte[] GetMakeEmblemData(string strInfo)
    {
        JArray pEmblem = JArray.Parse(strInfo);
        byte[] info = SingleFunc.CreateRandomEmblem();
        for(int t =0; t < pEmblem.Count; ++t)
        {
            try{
                info[t] = (byte)pEmblem[t]; 
            }
            catch
            {
                info[t] = 0;
            }
        }

        if(info[0] > 9)
        {
            info[0] = 0;
        }

        if(info[1] > 9)
        {
            info[1] = 0;
        }

        if(info[2] > 35)
        {
            info[2] = 0;
        }

        return info;
    }

#if USE_NETWORK
    public static void SetupSendLog(string url)
    {
        if(string.IsNullOrEmpty(url)) return;
        // #if !UNITY_EDITOR
        ALF.NETWORK.NetworkManager.SetLogServerUrl(url);
        Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
        Application.logMessageReceivedThreaded += LogMessageReceivedThreaded;
        // #endif
    }

    public static void RemoveSendLog()
    {
        // bFirstSend = false;
        #if !UNITY_EDITOR
        ALF.NETWORK.NetworkManager.SetLogServerUrl("");
        Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
        #endif
    }

    static void LogMessageReceivedThreaded(string condition, string stackTrace, UnityEngine.LogType type)
    {
        if(bFirstSend) return;

        if(type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception)
        {
            bFirstSend = true;

            JObject log = new JObject();
            log["user"] = GameContext.getCtx().GetClubID();
            log["log"] = condition == null ? "" : condition;
            log["type"] = $"{type}";
            log["stackTrace"] = stackTrace == null ? "" : stackTrace;
#if FTM_LIVE
            log["service"] = "Live";
#else
            log["service"] = "Dev";
#endif

            // log["stack"] = GetStackTrace();
            JObject pJObject = new JObject();
            pJObject["data"] = log.ToString();
            pJObject["ver"] = ALFUtils.GetVersionCode();

            ALF.NETWORK.NetworkManager.SendLog(pJObject);
        }
    }

    public static void SendLog(string msg)
    {
#if !FTM_LIVE
        return;
#endif
        JObject log = new JObject();
#if FTM_LIVE
        log["service"] = "Live";
#else
        log["service"] = "Dev";
#endif
        float time = Time.realtimeSinceStartup;
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.IsLoadGameData())
        {
            log["user"] = pGameContext.GetClubID();
            //log["log"] = condition == null ? "" : condition;
            //log["type"] = $"{type}";
        }
        else
        {    
            log["LastSendLogTime"] = fLastSendLogTime;
            log["time"] = time - fLastSendLogTime;
        }
        
        log["log"] = msg;
        
        JObject pJObject = new JObject();
        pJObject["data"] = log.ToString();
        fLastSendLogTime = time;

        pJObject["ver"] = ALFUtils.GetVersionCode();

        ALF.NETWORK.NetworkManager.SendLog(pJObject);
    }

#endif

    public static IEnumerator CheckCatalogs(System.Action<List<IResourceLocator>,AsyncOperationStatus> pAction) 
    {
        yield return Addressables.InitializeAsync();
                
        AsyncOperationHandle<List<string>> checkForUpdateHandle = Addressables.CheckForCatalogUpdates();
        
        checkForUpdateHandle.Completed += op =>
        {
            if(op.Status == AsyncOperationStatus.Failed || op.Result == null)
            {
                if(pAction != null)
                {
                    pAction(null,AsyncOperationStatus.Failed);
                }
            }
            else
            {
                List<string> catalogsToUpdate = new List<string>();
                catalogsToUpdate.AddRange(op.Result);

                if (catalogsToUpdate.Count > 0) 
                {
                    AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
                    updateHandle.Completed += _op =>
                    {
                        if(_op.Status == AsyncOperationStatus.Failed )
                        {
                            if(pAction != null)
                            {
                                pAction(null,_op.Status);
                            }
                        }
                        else if(_op.Result != null && pAction != null)
                        {
                            pAction(_op.Result,updateHandle.Status);
                        }     
                    };
                }
                else
                {
                    if(pAction != null)
                    {
                        pAction(null,op.Status);
                    }
                }
            }
        };
    }
}