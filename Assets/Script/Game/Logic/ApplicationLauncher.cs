using UnityEngine;
using UnityEngine.UI;
using ALF;
using STATEDATA;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.NETWORK;
using UnityEngine.SceneManagement;


public class ApplicationLauncher : ALF.MonoBase
{
    Message m_pMessage = null;
    Slider m_pLoadingBar = null;
    TMPro.TMP_Text m_pLoadingText = null;
    RectTransform m_pSplash = null;
    public class FladeInfoData : IStateData
    {
        public float FadeInTime {get; set; }
        public float FadeInDelta {get;}
        public FladeInfoData(float fadeIn, float fadeInDelta )
        {
            FadeInTime = fadeIn;
            FadeInDelta = fadeInDelta;
        }

        public void Dispose(){}
    }

    public class SceneLoadData : IStateData
    {
        public float FadeInTime {get; set;}
        public float FadeOutTime {get; set;}
        public float FadeInDelta {get; set;}
        public float FadeOutDelta {get; set;}
        public AsyncOperation Operation {get; set;}
        
        public SceneLoadData(){}
        
        public void Dispose()
        {
            Operation = null;
        }
    }

    public override void OnDispose()
    {
        m_pSplash = null;
        m_pLoadingBar = null;
        m_pLoadingText = null;
    }

    void Awake()
    {
        Application.targetFrameRate = -1;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
 #if !FTM_LIVE
        /**
        * 디버그 정보 표시 라이브에서는 사용X
        */
        Instantiate(Resources.Load<GameObject>("Reporter"));
#endif
        
        Director.InitInstance();
        SingleFunc.InitializSDK();
        LayoutManager pLayoutManager = LayoutManager.Instance;

        m_pSplash = pLayoutManager.FindUIFormRoot<RectTransform>("Splash");
        m_pLoadingBar = pLayoutManager.FindUIFormRoot<Slider>("Slider");
        m_pLoadingText = m_pLoadingBar.transform.Find("message").GetComponent<TMPro.TMP_Text>();
        NetworkManager.SetWaitMark(pLayoutManager.FindUIFormRoot<Animation>("UI_Wait"));
        ALFUtils.FadeObject(m_pSplash,-1);
        m_pLoadingBar.gameObject.SetActive(false);
        
        if(m_pMessage == null)
        {
            /**
            *  메세지 팝업 설정 
            *  MainScene 로드 이후는 해당 객체는 삭제된다. 이후 내부적으로 AssetPool에서 관리 처리한다. 
            */
            m_pMessage = new Message();
            m_pMessage.OnInit(null,LayoutManager.Instance.FindUIFormRoot<RectTransform>("Message"));
        }
    }
    void Start()
    {
        /**
        *  메인 업데이트 초기화
        */
        StateMachine.ScheduleUpdate(false);
        SingleFunc.CancelLocalNotification();
        
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pSplash),2, (uint)E_STATE_TYPE.Slpash, null, this.executeSplashCallback);
        pBaseState.StateData = new FladeInfoData(2, 1f / 2f);
        StateMachine.GetStateMachine().AddState(pBaseState);
        GameContext pGameContext =  GameContext.getCtx();
        pGameContext.SendAdjustEvent("vczf0v",true,false,-1);

        string token = null;
#if UNITY_ANDROID
        token = "pmxil6";
#elif ONESTORE
        token = "t8snn7";
#endif
        if(!string.IsNullOrEmpty(token))
        {
            pGameContext.SendAdjustEvent(token,true,false,-1);
        }
    }

    public void DisposeMessagePopup()
    {
        if(m_pMessage != null)
        {
            m_pMessage.Dispose();
            m_pMessage = null;
        }
    }

    public Message ShowMessagePopup(string message,System.Action action = null)
    {
        if(m_pMessage == null)
        {
            m_pMessage = new Message();
            m_pMessage.OnInit(null,LayoutManager.Instance.FindUIFormRoot<RectTransform>("Message"));
        }
        m_pMessage.SetMessage(message,"확인",action);
        SingleFunc.ShowAnimationDailog(m_pMessage.MainUI,null);
        return m_pMessage;
    }

    bool executeSplashCallback(IState state,float dt,bool bEnd)
    {
        if (state.StateData is FladeInfoData data)
        {            
            RectTransform splash = state.GetTarget<BaseStateTarget>().GetMainTarget<RectTransform>();
            if(data.FadeInTime > 0)
            {
                data.FadeInTime -= dt;
                float alpha = 1 - (data.FadeInDelta * data.FadeInTime); 
                if(alpha > 1)
                {
                    alpha = 1;
                }
                ALFUtils.FadeObject(splash, alpha);
            }

            if(bEnd)
            {
                /**
                *  메인 씬 로드
                */
                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(Director.Runner),-1, (uint)E_STATE_TYPE.SceneLoaging, this.enterLoadingCallback, this.executeLoadingCallback, this.exitLoadingCallback);
                StateMachine.GetStateMachine().AddState(pBaseState);
            }
        }
    
        return bEnd;
    }

    void enterLoadingCallback(IState state)
    {
        m_pLoadingBar.gameObject.SetActive(true);
        ALFUtils.FadeObject(m_pLoadingBar.transform,1);
        
        SceneLoadData data = new SceneLoadData();
        data.FadeInTime = 0;
        data.FadeInDelta = 0;
        data.FadeOutTime = 0;
        data.FadeOutDelta = 0;

        state.StateData = data;
    }

    bool executeLoadingCallback(IState state,float dt,bool bEnd)
    {
        if (state.StateData is SceneLoadData data)
        {
            if(data.FadeOutTime > 0)
            {
                data.FadeOutTime -= dt;
                float alpha = -(dt * data.FadeOutDelta);
                if(alpha < -1)
                {
                    alpha = -1;
                }

                ALFUtils.FadeObject(m_pLoadingBar.transform,alpha);
                if(data.FadeOutTime <= 0 )
                {
                    #if USE_HIVE
                    if(!GameContext.getCtx().IsInitializeHive) return false;
                    #endif
                    data.Operation.allowSceneActivation = true;
                    return true;
                }
            }
            else if (data.Operation == null)
            {
                data.Operation = SceneManager.LoadSceneAsync("Main");
                data.Operation.allowSceneActivation = false;
                data.Operation.completed += SingleFunc.SceneLoadCompleted;
            }
            else if(!data.Operation.isDone)
            {
                m_pLoadingBar.value = data.Operation.progress;
                if(data.Operation.progress >= 0.8f)
                {
                    data.FadeOutTime = 1;
                    data.FadeOutDelta = 1 / data.FadeOutTime;
                    m_pLoadingBar.value = 1;
                }
                m_pLoadingText.SetText($"Load {(int)(m_pLoadingBar.value * 100)}%");
            }
        }
    
        return bEnd;
    }

    IState exitLoadingCallback(IState state)
    {
        GameObject.Destroy(m_pSplash.gameObject);
        GameObject.Destroy(m_pLoadingBar.gameObject);
        m_pMessage?.Dispose();
        m_pMessage = null;
        m_pLoadingBar = null;
        m_pSplash = null;
        m_pLoadingText = null;

        return null;
    }
}