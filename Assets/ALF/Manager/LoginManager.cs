#if USE_NETWORK
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Globalization;

using System.Text;

using ALF;
using ALF.MACHINE;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.NETWORK;

#if USE_HIVE
using hive;
#else
using Facebook.Unity;
using Google;
using System.Threading.Tasks;

#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif
#endif


namespace ALF.LOGIN
{
    public class LoginManager : IBase
    {
        public enum E_SOCIAL_PROVIDER : byte { guest =0,facebook,google,apple, dev, gamecenter,hive, MAX }
        public enum E_STORE : byte { google =0,apple,onestore }
        public enum E_PLATFORM : byte { aos =0,ios,web,other}

        const uint STATE = 100001;

        static LoginManager instance = null;

        uint m_iApiID = 0;
        string m_strLoginAPI = null;
        string m_strCurrentSocialID = null;
        string m_strEmail = null;
        E_SOCIAL_PROVIDER m_eCurrentSocialProvider = E_SOCIAL_PROVIDER.MAX;
        E_STORE m_eCurrentStore = E_STORE.google;
        E_PLATFORM m_eCurrentPlatform = E_PLATFORM.other;
        System.Action<int> m_pSuccessCallback = null;

#if !USE_HIVE
        string m_strWebClientId = "784084728577-s0vtdjrn7rr513k2ich0nc9qte71h4vt.apps.googleusercontent.com";
        GoogleSignInConfiguration m_pGoogleConfiguration = null;
#if UNITY_IOS
        IAppleAuthManager appleAuthManager;
        BaseState pAppleAuthState = null;
#endif

        public static void SetWebClientId (string webClientId)
        {
            if(instance != null && !string.IsNullOrEmpty(webClientId) )
            {
                instance.m_strWebClientId = webClientId;
            }
        }
#endif

        public static LoginManager Instance
        {
            get {return instance;}
        }

        public static bool InitInstance()
        {
            if(instance == null)
            {
                instance = new LoginManager();
                
                return true;
            }

            return false;
        }

        public static void UpdateCurrentSocialProvider(E_SOCIAL_PROVIDER eProvider)
        {
            if(instance != null )
            {
                instance.m_eCurrentSocialProvider = eProvider;
                PlayerPrefs.SetInt("LLSP",(int)eProvider);
                PlayerPrefs.Save();
            }
        }

        public static E_SOCIAL_PROVIDER GetCurrentSocialProvider()
        {
            if(instance != null )
            {
                return instance.m_eCurrentSocialProvider;
            }

            return E_SOCIAL_PROVIDER.MAX;
        }

        public static string GetCurrentSocialID()
        {
            if(instance != null )
            {
                return instance.m_strCurrentSocialID;
            }

            return null;
        }

        protected LoginManager()
        {
#if !USE_HIVE
#if UNITY_IOS
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                // Creates an Apple Authentication manager with the deserializer
                appleAuthManager = new AppleAuthManager(new PayloadDeserializer());
                appleAuthManager.SetCredentialsRevokedCallback(CredentialsRevokedCallback);
            }
#endif
#endif
        }

        public void Dispose()
        {
            m_pSuccessCallback = null;
#if !USE_HIVE
#if UNITY_IOS
            appleAuthManager = null;
            if(pAppleAuthState != null)
            {
                pAppleAuthState.Exit(true);
                pAppleAuthState = null;
            }
#endif
#endif
        }

        public void Connect(E_SOCIAL_PROVIDER eProvider, System.Action<int> successCallback)//public void Connect(E_SOCIAL_PROVIDER eProvider, System.Action<int,hive.AuthV4.PlayerInfo> successCallback)
        {
#if USE_HIVE
             AuthV4.ProviderType providerType = AuthV4.ProviderType.GUEST;
            if(eProvider == E_SOCIAL_PROVIDER.facebook)
            {
                providerType = AuthV4.ProviderType.FACEBOOK;
            }
            else if(eProvider == E_SOCIAL_PROVIDER.google)
            {
                providerType = AuthV4.ProviderType.GOOGLE;
            }
            else if(eProvider == E_SOCIAL_PROVIDER.apple)
            {
                // providerType = AuthV4.ProviderType.APPLE;
                providerType = AuthV4.ProviderType.SIGNIN_APPLE;
            }
            else
            {
                return;
            }
            m_pSuccessCallback = successCallback;
            AuthV4.Helper.connect(providerType,OnAuthV4Connect);
            // AuthV4.connect(providerType,OnAuthV4Connect);
#endif
        }

        public void PlayerDelete(System.Action<int> successCallback)
        {
            #if USE_HIVE
            m_pSuccessCallback = successCallback;

            AuthV4.playerDelete(OnAuthV4SignOut);

            #endif
        }

        public void LogOut(System.Action<int> successCallback)
        {
            #if USE_HIVE
            
            m_pSuccessCallback = successCallback;

            AuthV4.signOut(OnAuthV4SignOut);

            #endif
        }

        public void Login(uint apiID,string loginAPI, E_SOCIAL_PROVIDER provider, System.Action<int> successCallback)
        {
            NetworkManager.ShowWaitMark(true);
            m_eCurrentSocialProvider = provider;
            #if UNITY_IOS
                m_eCurrentStore = E_STORE.apple;
                m_eCurrentPlatform = E_PLATFORM.ios;
            #elif ONESTORE
                m_eCurrentStore = E_STORE.onestore;
                m_eCurrentPlatform = E_PLATFORM.aos;
            #elif UNITY_ANDROID
                m_eCurrentStore = E_STORE.google;
                m_eCurrentPlatform = E_PLATFORM.aos;
            #endif
            m_pSuccessCallback = successCallback;
            m_iApiID = apiID;
            m_strLoginAPI = loginAPI;
        
            switch(provider)
            {
                case E_SOCIAL_PROVIDER.guest:
                {
                    E_SOCIAL_PROVIDER eProvider = (E_SOCIAL_PROVIDER)PlayerPrefs.GetInt("LLSP",(int)E_SOCIAL_PROVIDER.MAX);
                    m_strCurrentSocialID = PlayerPrefs.GetString("uuid");
#if USE_HIVE
                    if(!Application.isEditor)
                    {
                        // AuthV4.Helper.signIn(OnAuthV4SignIn);
                        AuthV4.signIn(AuthV4.ProviderType.GUEST,OnAuthV4SignIn); 
                        return;
                    }
#endif

                    if(eProvider != m_eCurrentSocialProvider || string.IsNullOrEmpty(m_strCurrentSocialID))
                    {
                        m_strCurrentSocialID = SystemInfo.deviceUniqueIdentifier;
                    }
                    // m_strCurrentSocialID = "63e254bbf7bc5fc093bdb7e7a407f25c";
                    ServerLgoin();
                }
                break;
                case E_SOCIAL_PROVIDER.facebook:
                {
                    FacebookLogin();
                }
                break;
                case E_SOCIAL_PROVIDER.google:
                {
                    GoogleLogin();
                }
                break;
                case E_SOCIAL_PROVIDER.apple:
                {
#if UNITY_IOS
                    SignInWithApple();
                    //// AttemptQuickLogin();
#endif
                }
                break;
            }
        }

#if USE_HIVE

        public void ShowSignInHive(uint apiID,string pLoginAPI,E_SOCIAL_PROVIDER eProvider,System.Action<int> successCallback)
        {
            m_pSuccessCallback = successCallback;
            m_iApiID = apiID;
            m_strLoginAPI = pLoginAPI;
            m_eCurrentSocialProvider = eProvider;
            #if UNITY_IOS
                m_eCurrentStore = E_STORE.apple;
                m_eCurrentPlatform = E_PLATFORM.ios;
            #elif ONESTORE
                m_eCurrentStore = E_STORE.onestore;
                m_eCurrentPlatform = E_PLATFORM.aos;
            #elif UNITY_ANDROID
                m_eCurrentStore = E_STORE.google;
                m_eCurrentPlatform = E_PLATFORM.aos;
            #endif
            
            if(Application.isEditor)
            {
                Login(apiID, pLoginAPI, m_eCurrentSocialProvider,null);
                return;
            }
            
            NetworkManager.ShowWaitMark(true);
            AuthV4.Helper.signIn(OnAuthV4SignIn);

            // if(m_eCurrentSocialProvider == E_SOCIAL_PROVIDER.guest)
            // {
            //     AuthV4.signIn(AuthV4.ProviderType.GUEST,OnAuthV4SignIn);
            // }
            // else
            // {
            //     AuthV4.Helper.signIn(OnAuthV4SignIn);
            // }
        }

        void OnAuthV4BlackList(ResultAPI result, List<AuthV4.AuthV4MaintenanceInfo> maintenanceInfoList)
        {
            switch(result.code)
            {
                case ResultAPI.Code.Success:
                    ServerLgoin();
                    // 일반 유저인 경우
                    break;
                case ResultAPI.Code.AuthUserInBlacklistActionDefault_DoExit:
                case ResultAPI.Code.AuthUserInBlacklistActionOpenURL_DoExit:
                case ResultAPI.Code.AuthUserInBlacklistActionExit_DoExit:
                case ResultAPI.Code.AuthUserInBlacklistActionDone:
                case ResultAPI.Code.AuthUserInBlacklistTimeover_DoExit:
                    // 제재 유저인 경우 (Hive 제재 팝업 UI가 닫히고 전달된 이벤트에 따라 처리하세요.)
                    break;
                default:
                    // 제재 여부 확인 요청 실패
                    break;
            }
        }

        void OnAuthV4Connect(ResultAPI result, AuthV4.PlayerInfo conflictPlayer)
        {
            // switch(result.code) 
            // {
            //     case ResultAPI.Code.Success:
            //     {
            //         if(m_pSuccessCallback != null)
            //         {
            //             m_pSuccessCallback((int)result.code);
            //             m_pSuccessCallback = null;
            //         }                     
            //     }
            //         break;
            //     case ResultAPI.Code.AuthV4ConflictPlayer:
            //     {
            //         AuthV4.Helper.showConflict(OnAuthV4Conflict);
            //     }
            //     break;
            //     default:
            //     // 기타 예외 상황
            //         break;
            // }

            if(result.code == ResultAPI.Code.AuthV4ConflictPlayer)
            {
                // 충돌 사용자 게임 정보 객체 생성
                // AuthV4.Helper.ConflictSingleViewInfo conflictInfo = new AuthV4.Helper.ConflictSingleViewInfo(conflictPlayer.playerId);
                // conflictInfo.setValue("Gold", 2048);
                // conflictInfo.setValue("Gem", 220);

                AuthV4.Helper.showConflict(OnAuthV4Conflict);
                return;
            }

            if(m_pSuccessCallback != null)
            {
                m_pSuccessCallback((int)result.code);
                m_pSuccessCallback = null;
            }
        }
        void OnAuthV4SignOut(ResultAPI result)
        {   
            if(m_pSuccessCallback != null)
            {
                m_pSuccessCallback((int)result.code);
                m_pSuccessCallback = null;
            }

            // switch(result.code) 
            // {
            //     case ResultAPI.Code.Success:
            //     {
            //         if(m_pSuccessCallback != null)
            //         {
            //             m_pSuccessCallback(true);
            //             m_pSuccessCallback = null;
            //         } 
            //     }
            //         break;
            //     default:
            //     // 기타 예외 상황
            //         break;
            // }
        }
        void OnAuthV4SignIn(ResultAPI pResult, AuthV4.PlayerInfo pPlayerInfo)
        {
            if(pResult.code == ResultAPI.Code.Success || pResult.code == ResultAPI.Code.AuthV4AlreadyAuthorized )
            {
                pPlayerInfo = AuthV4.getPlayerInfo();
                // pPlayerInfo.providerInfoData[0].
                // m_strEmail = task.Result.Email;
                m_strCurrentSocialID = pPlayerInfo.playerId.ToString();
                // public Dictionary<ProviderType, ProviderInfo> providerInfoData = new Dictionary<ProviderType, ProviderInfo>();
                AuthV4.checkBlacklist(true,OnAuthV4BlackList);
            }
            else if( pResult.code == ResultAPI.Code.AuthV4HelperImplifiedLoginFail)
            {
                AuthV4.ProviderType eProviderType = AuthV4.ProviderType.GUEST;
                if(m_eCurrentSocialProvider == E_SOCIAL_PROVIDER.apple)
                {
                    eProviderType = AuthV4.ProviderType.APPLE;
                }
                else if(m_eCurrentSocialProvider == E_SOCIAL_PROVIDER.facebook)
                {
                    eProviderType = AuthV4.ProviderType.FACEBOOK;
                }
                else if(m_eCurrentSocialProvider == E_SOCIAL_PROVIDER.google)
                {
                    eProviderType = AuthV4.ProviderType.GOOGLE;
                }
                AuthV4.signIn(eProviderType,OnAuthV4SignIn);
                return;
            }

            // switch (pResult.code)
            // {
            //     case ResultAPI.Code.Success:                           // 로그인 성공     
            //     case ResultAPI.Code.AuthV4AlreadyAuthorized:           // 인증된 상태 (리스타트)
            //     {
            //         pPlayerInfo = AuthV4.getPlayerInfo();

            //         // pPlayerInfo.providerInfoData[0].
            //         // m_strEmail = task.Result.Email;
            //         m_strCurrentSocialID = pPlayerInfo.playerId.ToString();

            //         // public Dictionary<ProviderType, ProviderInfo> providerInfoData = new Dictionary<ProviderType, ProviderInfo>();
            //         AuthV4.checkBlacklist(true,OnAuthV4BlackList);
            //     }
            //         return;
            //     case ResultAPI.Code.AuthV4HelperImplifiedLoginFail:    // 묵시적 로그인에 실패
            //     {
            //         // AuthV4.ProviderType eProviderType = AuthV4.ProviderType.GUEST;
            //         // if(m_eCurrentSocialProvider == E_SOCIAL_PROVIDER.apple)
            //         // {
            //         //     eProviderType = AuthV4.ProviderType.APPLE;
            //         // }
            //         // else if(m_eCurrentSocialProvider == E_SOCIAL_PROVIDER.facebook)
            //         // {
            //         //     eProviderType = AuthV4.ProviderType.FACEBOOK;
            //         // }
            //         // else if(m_eCurrentSocialProvider == E_SOCIAL_PROVIDER.google)
            //         // {
            //         //     eProviderType = AuthV4.ProviderType.GOOGLE;
            //         // }
            //         // AuthV4.signIn(eProviderType,OnAuthV4SignIn);
            //     }
            //     return;
            //     case ResultAPI.Code.AuthV4CancelDialog:                // 로그인 화면 닫기
            //     {
            //         // IBaseScene pIBaseScene = Director.Instance.GetActiveBaseScene<IBaseScene>();
            //         // AuthV4.showSignIn(OnAuthV4SignIn);
            //     }
            //         break;
            //     case ResultAPI.Code.AuthV4ConflictPlayer:              // 계정 충돌
            //         // AuthV4.Helper.showConflict(OnAuthV4Conflict);
            //         break;
            //     case ResultAPI.Code.AuthV4FacebookCancel:
            //     case ResultAPI.Code.AuthV4GoogleLoginCancel:
            //     case ResultAPI.Code.AuthV4AppleLoginCancel:
            //         // AuthV4.showSignIn(OnAuthV4SignIn);
            //         break;
            //     default:
            //         // WaitingMgr.Instance.Hide();
            //         // OneButtonMgr.Instance.Show(SystemMgr.Instance.Quit, _Result.code.ToString(), _Result.errorMessage, "end");
            //         break;
            // }

            if(m_pSuccessCallback != null)
            {
                m_pSuccessCallback((int)pResult.code);
                m_pSuccessCallback = null;
            }
        }

        void OnAuthV4Conflict(ResultAPI pResult, AuthV4.PlayerInfo pPlayerInfo)
        {
            if(m_pSuccessCallback != null)
            {
                m_pSuccessCallback((int)pResult.code);
                m_pSuccessCallback = null;
            }

            // switch (pResult.code)
            // {
            //     case ResultAPI.Code.AuthV4PlayerChange:
            //     // m_pSuccessCallback = successCallback;
            //         // SystemMgr.Instance.Restart();
            //         // 계정 전환 : 게임 재시작 필요
            //         break;
            //     case ResultAPI.Code.AuthV4PlayerResolved:
            //         // if(SystemMgr.Instance.GetScene() == "Title")
            //         // {
            //         //     _PlayerInfo = AuthV4.getPlayerInfo();

            //         //     PrefsMgr.Instance.SetLoginType("HIVE");
            //         //     PrefsMgr.Instance.SetLoginID(_PlayerInfo.playerId.ToString());

            //         //     TitleMgr.Instance.LoginSuccess();
            //         // }
            //         // 현재 사용자 유지
            //         break;
            //     default:
            //         // WaitingMgr.Instance.Hide();

            //         // OneButtonMgr.Instance.Show(SystemMgr.Instance.Quit, _Result.code.ToString(), _Result.errorMessage, "end");
            //         // 기타 예외 상황 처리
            //         break;
            // }
        }
#else
        void GoogleSilentlyAuthenticationFinished(Task<GoogleSignInUser> task)
        {
            if (task.IsFaulted || task.IsCanceled) 
            {
                // using (IEnumerator<System.Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator()) 
                // {
                //     if (enumerator.MoveNext()) 
                //     {
                //         GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                //         // AddStatusText("Got Error: " + error.Status + " " + error.Message);
                //     }
                //     else 
                //     {
                //         // AddStatusText("Got Unexpected Exception?!?" + task.Exception);
                //     }
                // }
                NetworkManager.ShowWaitMark(false);
            } 
            else  
            {
                m_strEmail = task.Result.Email;
                m_strCurrentSocialID = task.Result.UserId;
                // task.Result.IdToken;
                // AddStatusText("Welcome: " + task.Result.DisplayName + "!");

                ServerLgoin();
#if UNITY_IOS
                if (AppleAuthManager.IsCurrentPlatformSupported)
                {
                    appleAuthManager.SetCredentialsRevokedCallback(null);
                    if(pAppleAuthState != null)
                    {
                        pAppleAuthState.Exit(true);
                        pAppleAuthState = null;
                    }
                }
#endif
            }
        }

        void GoogleAuthenticationFinished(Task<GoogleSignInUser> task)
        {
            if (task.IsFaulted || task.IsCanceled) 
            {
                PlayerPrefs.DeleteKey("LLSP");
                PlayerPrefs.DeleteKey("uuid");
                GoogleLogin();
                NetworkManager.ShowWaitMark(false);
            } 
            else  
            {
                m_strEmail = task.Result.Email;
                m_strCurrentSocialID = task.Result.UserId;
                // task.Result.IdToken;
                // AddStatusText("Welcome: " + task.Result.DisplayName + "!");

                ServerLgoin();
#if UNITY_IOS
                if (AppleAuthManager.IsCurrentPlatformSupported)
                {
                    appleAuthManager.SetCredentialsRevokedCallback(null);
                    if(pAppleAuthState != null)
                    {
                        pAppleAuthState.Exit(true);
                        pAppleAuthState = null;
                    }
                }
#endif
            }
        }

        
        // public void OnSignInSilently() 
        // {
        //     GoogleSignIn.Configuration = configuration;
        //     GoogleSignIn.Configuration.UseGameSignIn = false;
        //     GoogleSignIn.Configuration.RequestIdToken = true;
        //     AddStatusText("Calling SignIn Silently");

        //     GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
        // }

        // public void OnGamesSignIn() 
        // {
        //     GoogleSignIn.Configuration = configuration;
        //     GoogleSignIn.Configuration.UseGameSignIn = true;
        //     GoogleSignIn.Configuration.RequestIdToken = false;

        //     AddStatusText("Calling Games SignIn");

        //     GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
        // }

        void FacebookAuthCallback (ILoginResult result) 
        {
            // FB.Android.RetrieveLoginStatus
            
            if (FB.IsLoggedIn)
            {
                // AccessToken class will have session details
                AccessToken aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
                // Print current access token's User ID
                m_strCurrentSocialID = aToken.UserId;
                ServerLgoin();
                // Print current access token's granted permissions
                // foreach (string perm in aToken.Permissions) {
                //     Debug.Log(perm);
                // }
#if UNITY_IOS
                if (AppleAuthManager.IsCurrentPlatformSupported)
                {
                    appleAuthManager.SetCredentialsRevokedCallback(null);
                    if(pAppleAuthState != null)
                    {
                        pAppleAuthState.Exit(true);
                        pAppleAuthState = null;
                    }
                }
#endif
            } 
            else 
            {
                NetworkManager.ShowWaitMark(false);
            }
        }

        void RefreshCallback(IAccessTokenRefreshResult result) 
        {
            if (FB.IsLoggedIn) 
            {
                Debug.Log (result.AccessToken.ExpirationTime.ToString());
            }
        }

#if UNITY_IOS
        void CredentialsRevokedCallback(string result)
        {
            // Debug.Log("Received revoked callback :" + result);
            // PlayerPrefs.DeleteKey(AppleUserIdKey);
        }
        bool executeAppleLoinUpdate(IState state,float dt,bool bEnd)
        {
            appleAuthManager.Update();
            return bEnd;
        }

        void AttemptQuickLogin()
        {
            if(pAppleAuthState == null)
            {
                pAppleAuthState = BaseState.GetInstance( new BaseStateTarget(Director.Runner),-1, STATE, null, executeAppleLoinUpdate,null, -1);
                StateMachine.GetStateMachine().AddState(pAppleAuthState);
            }

            E_SOCIAL_PROVIDER eProvider = (E_SOCIAL_PROVIDER)PlayerPrefs.GetInt("LLSP",(int)E_SOCIAL_PROVIDER.MAX);
            string uuid = PlayerPrefs.GetString("uuid",null);

            if(eProvider == m_eCurrentSocialProvider && !string.IsNullOrEmpty(uuid))
            {
                CheckCredentialStatusForUserId(uuid);
                return;
            }

            AppleAuthQuickLoginArgs quickLoginArgs = new AppleAuthQuickLoginArgs();
            
            // Quick login should succeed if the credential was authorized before and not revoked
            appleAuthManager.QuickLogin(
                quickLoginArgs,
                credential =>
                {
                    // If it's an Apple credential, save the user ID, for later logins
                    IAppleIDCredential appleIdCredential = credential as IAppleIDCredential;
                    if (appleIdCredential != null)
                    {
                        if(pAppleAuthState != null)
                        {
                            pAppleAuthState.Exit(true);
                            pAppleAuthState = null;
                        }
                        m_strCurrentSocialID = appleIdCredential.User;
                        
                        // Email (Received ONLY in the first login)
                        // string email = appleIdCredential.Email;

                        // Full name (Received ONLY in the first login)
                        // IPersonName fullName = appleIdCredential.FullName;

                        // Identity token
                        // string identityToken = Encoding.UTF8.GetString(
                        //     appleIdCredential.IdentityToken,
                        //     0,
                        //     appleIdCredential.IdentityToken.Length);

                        // // Authorization code
                        // var authorizationCode = Encoding.UTF8.GetString(
                        //     appleIdCredential.AuthorizationCode,
                        //     0,
                        //     appleIdCredential.AuthorizationCode.Length);

                        // And now you have all the information to create/login a user in your system
                        ServerLgoin();   
                    }
                    else
                    {
                        NetworkManager.ShowWaitMark(false);    
                        // Debug.Log("1111appleIdCredential = null !!! ");
                    }
                    // this.SetupGameMenu(credential.User, credential);
                },
                error =>
                {
                    NetworkManager.ShowWaitMark(false);
                    // If Quick Login fails, we should show the normal sign in with apple menu, to allow for a normal Sign In with apple
                    // AuthorizationErrorCode authorizationErrorCode = error.GetAuthorizationErrorCode();
                    // Debug.LogWarning("Quick Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                    // this.SetupLoginMenuForSignInWithApple();
                });
        }

        void CheckCredentialStatusForUserId(string appleUserId)
        {
            // If there is an apple ID available, we should check the credential state
            appleAuthManager.GetCredentialState( appleUserId,
                state =>
                {
                    switch (state)
                    {
                        // If it's authorized, login with that user id
                        case CredentialState.Authorized:
                            // this.SetupGameMenu(appleUserId, null);
                            m_strCurrentSocialID = appleUserId;
                            ServerLgoin();
                            return;
                        
                        // If it was revoked, or not found, we need a new sign in with apple attempt
                        // Discard previous apple user id
                        case CredentialState.Revoked:
                        case CredentialState.NotFound:
                            // this.SetupLoginMenuForSignInWithApple();
                            PlayerPrefs.DeleteKey("LLSP");
                            PlayerPrefs.DeleteKey("uuid");
                            SignInWithApple();
                            return;
                    }
                },
                error =>
                {
                    NetworkManager.ShowWaitMark(false);
                    // AuthorizationErrorCode authorizationErrorCode = error.GetAuthorizationErrorCode();
                    // Debug.LogWarning("Error while trying to get credential state " + authorizationErrorCode.ToString() + " " + error.ToString());
                    // this.SetupLoginMenuForSignInWithApple();
                });
        }
#endif
#endif

        void GoogleLogin()
        {
#if USE_HIVE
            AuthV4.signIn(AuthV4.ProviderType.GOOGLE, OnAuthV4SignIn);
#else
            if(m_pGoogleConfiguration == null)
            {
                m_pGoogleConfiguration = new GoogleSignInConfiguration { WebClientId = m_strWebClientId, RequestIdToken = true };
                GoogleSignIn.Configuration = m_pGoogleConfiguration;
                GoogleSignIn.Configuration.UseGameSignIn = false;
                GoogleSignIn.Configuration.RequestIdToken = true;
            }

            E_SOCIAL_PROVIDER eProvider = (E_SOCIAL_PROVIDER)PlayerPrefs.GetInt("LLSP",(int)E_SOCIAL_PROVIDER.MAX);
            string uuid = PlayerPrefs.GetString("uuid",null);

            if(eProvider == m_eCurrentSocialProvider && !string.IsNullOrEmpty(uuid))
            {
                GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(GoogleSilentlyAuthenticationFinished);
            }
            else
            {
                GoogleSignIn.DefaultInstance.SignIn().ContinueWith(GoogleAuthenticationFinished);
            }
#endif
        }
        void GoogleLoginOut()
        {
#if USE_HIVE
            // AuthV4.signIn(AuthV4.ProviderType.GOOGLE, OnAuthV4SignIn);
#else
            // AddStatusText("Calling SignOut");
            GoogleSignIn.DefaultInstance.SignOut();
#endif
        }

        void FacebookLogin()
        {
#if USE_HIVE
            AuthV4.signIn(AuthV4.ProviderType.FACEBOOK, OnAuthV4SignIn);
            // AuthV4.Helper.signIn(OnAuthV4SignIn);
#else
            E_SOCIAL_PROVIDER eProvider = (E_SOCIAL_PROVIDER)PlayerPrefs.GetInt("LLSP",(int)E_SOCIAL_PROVIDER.MAX);
            string uuid = PlayerPrefs.GetString("uuid",null);
            if(eProvider == m_eCurrentSocialProvider && !string.IsNullOrEmpty(uuid))
            {
                m_strCurrentSocialID = uuid;

                #if !UNITY_EDITOR
                if (FB.IsLoggedIn)
                #endif
                {
                    ServerLgoin();
                    return;
                }
            }    
            
            FB.LogInWithReadPermissions(new List<string>(){"public_profile", "email"}, FacebookAuthCallback);
            // FB.Android.RetrieveLoginStatus
#endif
        }
        
#if UNITY_IOS
        void SignInWithApple()
        {
#if USE_HIVE
            AuthV4.signIn(AuthV4.ProviderType.SIGNIN_APPLE, OnAuthV4SignIn);
            // AuthV4.Helper.signIn(OnAuthV4SignIn);
#else
            if(pAppleAuthState == null)
            {
                pAppleAuthState = BaseState.GetInstance( new BaseStateTarget(Director.Runner),-1, STATE, null, executeAppleLoinUpdate,null, -1);
                StateMachine.GetStateMachine().AddState(pAppleAuthState);
            }

            E_SOCIAL_PROVIDER eProvider = (E_SOCIAL_PROVIDER)PlayerPrefs.GetInt("LLSP",(int)E_SOCIAL_PROVIDER.MAX);
            string uuid = PlayerPrefs.GetString("uuid",null);

            if(eProvider == m_eCurrentSocialProvider && !string.IsNullOrEmpty(uuid))
            {
                CheckCredentialStatusForUserId(uuid);
                return;
            }

            AppleAuthLoginArgs loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

            appleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    // Obtained credential, cast it to IAppleIDCredential
                    IAppleIDCredential appleIdCredential = credential as IAppleIDCredential;
                    if (appleIdCredential != null)
                    {
                        if(pAppleAuthState != null)
                        {
                            pAppleAuthState.Exit(true);
                            pAppleAuthState = null;
                        }
                        
                        // Apple User ID
                        // You should save the user ID somewhere in the device
                        m_strCurrentSocialID = appleIdCredential.User;
                        
                        // Email (Received ONLY in the first login)
                        // string email = appleIdCredential.Email;

                        // Full name (Received ONLY in the first login)
                        // IPersonName fullName = appleIdCredential.FullName;

                        // Identity token
                        // string identityToken = Encoding.UTF8.GetString(
                        //     appleIdCredential.IdentityToken,
                        //     0,
                        //     appleIdCredential.IdentityToken.Length);

                        // // Authorization code
                        // string authorizationCode = Encoding.UTF8.GetString(
                        //     appleIdCredential.AuthorizationCode,
                        //     0,
                        //     appleIdCredential.AuthorizationCode.Length);

                        // And now you have all the information to create/login a user in your system
                        ServerLgoin();
                    }
                    else
                    {
                        NetworkManager.ShowWaitMark(false);    
                        // Debug.Log("appleIdCredential = null !!! ");
                    }
                },
                error =>
                {
                    NetworkManager.ShowWaitMark(false);
                    // Something went wrong
                    // AuthorizationErrorCode authorizationErrorCode = error.GetAuthorizationErrorCode();
                    // Debug.Log("SignInWithApple Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                });

#endif
        }
#endif
        void ServerLgoin()
        {
            if(m_pSuccessCallback != null)
            {
                m_pSuccessCallback(0);
                m_pSuccessCallback = null;
            }
            PlayerPrefs.SetInt("LLSP",(int)m_eCurrentSocialProvider);
            //m_strCurrentSocialID = "49345803121";

            PlayerPrefs.SetString("uuid",m_strCurrentSocialID); //dev, 49345803121
            Debug.Log($"===========>  m_strCurrentSocialID:{m_strCurrentSocialID} m_eCurrentSocialProvider:{m_eCurrentSocialProvider}");
            JObject data = new JObject();
            
            if(Application.isEditor)
            {
                data["socialProvider"] = "dev";
            }
            else
            {
#if USE_HIVE
                data["socialProvider"] = "hive";
#else
                data["socialProvider"] = m_eCurrentSocialProvider.ToString();
#endif
            }
            
            data["socialId"] = m_strCurrentSocialID;
            JObject pJObject = new JObject();
            pJObject["nation"]=System.Globalization.RegionInfo.CurrentRegion.ThreeLetterISORegionName.ToUpper();
            pJObject["lang"]= Application.systemLanguage.ToString().ToLower();
            pJObject["platform"]= m_eCurrentPlatform.ToString();
            pJObject["store"]= m_eCurrentStore.ToString();
            pJObject["email"] = m_strEmail == null ? "":m_strEmail;
#if USE_HIVE
            if(!Application.isEditor)
            {
                AuthV4.PlayerInfo pPlayerInfo = AuthV4.getPlayerInfo();
                pJObject["did"] = pPlayerInfo.did;
                pJObject["player_id"] = pPlayerInfo.playerId;
                pJObject["authToken"] = pPlayerInfo.playerToken;
            }
#endif
            data["registerInfo"] = pJObject;
            NetworkManager.SetLastLoginData(data.ToString());
            NetworkManager.Instance.SendRequest(m_iApiID, m_strLoginAPI,true,true,null,data); 
        }
    }
}
#endif