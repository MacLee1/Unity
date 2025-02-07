using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALF.LAYOUT;
#if USE_NETWORK
using ALF.NETWORK;
#endif


namespace ALF
{
    public class Director : IBase
    {
        static MonoBase runner = null;
        static Director instance = null;
        Coroutine updateCoroutine = null;
        Coroutine sceneUpdateCoroutine = null;
        List<ALF.IUpdate> m_scheduleList = new List<ALF.IUpdate>(32);
        IBaseScene runningScene = null;
        IBaseScene nextScene = null;
        // double m_PreFrameFime = 0;
        public static MonoBase Runner
        {
            get { return runner;}
        }
        public static GameObject gameObject
        {
            get { return runner.gameObject;}
        }

        public static Director Instance
        {
            get { return instance;}
        }
        /**
        * 게임에 필요한 시스템 객체 초기화
        */
        public static bool InitInstance()
        {
            if(instance == null)
            {
                instance = new Director();
                GameObject root = new GameObject("Director");
                runner = root.AddComponent<MonoBase>();
                GameObject.DontDestroyOnLoad(root);
                ALF.MACHINE.StateMachine.InitDefaultInstance();    
#if USE_NETWORK
                ALF.NETWORK.NetworkManager.InitInstance();
                ALF.LOGIN.LoginManager.InitInstance();
#endif
                LayoutManager.InitInstance();
                ALF.SOUND.SoundManager.InitInstance(); 

                return true;
            }

            return false;
        }

        static public Coroutine StartCoroutine(IEnumerator routine)
        {
            return runner.StartCoroutine(routine);
        }

        static public void SetApplicationFocus(System.Action<bool> applicationFocusCallback)
        {
            runner.SetApplicationFocus(applicationFocusCallback);
        }

        static public void SetApplicationPause(System.Action<bool> applicationPauseCallback)
        {
            runner.SetApplicationPause(applicationPauseCallback);
        }

        static public void SetApplicationQuit(System.Action applicationQuit)
        {
            runner.SetApplicationQuit(applicationQuit);
        }

        public void Dispose()
        {
#if USE_NETWORK			
            NetworkManager.Instance.Dispose(); 
#endif
            LayoutManager.Instance.Dispose();
            ALF.SOUND.SoundManager.Instance.Dispose();
            AFPool.Instance.Dispose();
            
            runner = null;
            instance = null;
            updateCoroutine = null;
            sceneUpdateCoroutine = null;
            m_scheduleList.Clear();
            m_scheduleList = null;
            runningScene = null;
            nextScene = null; 
        }

        IEnumerator coUpdate()
        {
            while (true)
            {
                // double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
                // float dt = (float)(realtimeSinceStartupAsDouble - m_PreFrameFime);
                float dt = Time.deltaTime;
                for( int i = 0; i < m_scheduleList.Count; ++i)
                {
                    m_scheduleList[i].Update(dt);
                }
                yield return null;
                // m_PreFrameFime = realtimeSinceStartupAsDouble;
            }
        }

        public void AddSchedule(IUpdate _base)
        {
            if(_base != null)
            {
                m_scheduleList.Add(_base);
                if(updateCoroutine == null)
                {
                    // m_PreFrameFime = Time.realtimeSinceStartupAsDouble;
                    updateCoroutine = StartCoroutine(coUpdate());
                }
            }
        }

        public void RemoveSchedule(IUpdate _base)
        {
            if(_base != null)
            {
                for( int i = m_scheduleList.Count -1; i >= 0; --i)
                {
                    if(_base == m_scheduleList[i])
                    {
                        m_scheduleList.RemoveAt(i);
                        if(m_scheduleList.Count == 0 && updateCoroutine != null)
                        {
                            runner.StopCoroutine(updateCoroutine);
                            updateCoroutine = null;
                        }
                        return;
                    }
                }
            }
        }

        public void RemoveAllSchedule()
        {
            m_scheduleList.Clear();
            runner.StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }

        public float OpenCurtain(Transform curtain)
        {
            Color color = Color.white;            
            color.r = Random.Range(0.6f,1f);
            color.g = Random.Range(0.6f,1f);
            color.b = Random.Range(0.6f,1f);
            UnityEngine.UI.Image pImage = curtain.GetComponent<UnityEngine.UI.Image>();
            Animator pAnimator = curtain.GetComponent<Animator>();
            pImage.color = color;
            pImage.enabled = true;
            pAnimator.Play("curtain_in",0,0);
            pAnimator.Update(0.0f);
            AnimatorStateInfo pAnimatorStateInfo = pAnimator.GetCurrentAnimatorStateInfo(0);
            return pAnimatorStateInfo.length;
        }

        public float HideCurtain(Transform curtain)
        {
            // ALF.ACTOR.BaseActor[] list = curtain.GetComponentsInChildren<ALF.ACTOR.BaseActor>(true);
            // ALF.ACTOR.BaseActor pGameActor = null;
            // for(int i =0; i < list.Length; ++i)
            // {
            //     pGameActor = list[i];
            //     pGameActor.ChangeAnimation("hide");
            // }
            Animator pAnimator = curtain.GetComponent<Animator>();
            pAnimator.Play("curtain_out",0,0);
            pAnimator.Update(0.0f);
            
            AnimatorStateInfo pAnimatorStateInfo = pAnimator.GetCurrentAnimatorStateInfo(0);
            return pAnimatorStateInfo.length;
        }

        IEnumerator coRunWithScene(float duration,bool bCurtain)
        {
            ALFUtils.Assert(nextScene != null,"nextScene = null !!");
            
            Transform curtain = LayoutManager.Instance.GetCurtain();
            curtain.gameObject.SetActive(true);

            float fadeInTime = duration * 0.5f;
            float total = duration;
            UnityEngine.UI.Image pImage = curtain.GetComponent<UnityEngine.UI.Image>();
            float _duration = 0;
            
            float fadeInDelta = 1 / fadeInTime;
            bool bInit = true;
            
            if(runningScene != null)
            {
                runningScene.Transition = true;
            }

            if(sceneUpdateCoroutine != null)
            {
                runner.StopCoroutine(sceneUpdateCoroutine);
                sceneUpdateCoroutine = null;
            }
            
            nextScene.Transition = true;
            
            if(bCurtain)
            {
                yield return new WaitForSeconds(_duration);

                fadeInTime = 1;
                total = 0.5f; 
            }
            bool bRun = total >= 0;
            while (bRun || total > 0)
            {
                bRun = false;
                
                total -= Time.deltaTime;
                
                if(total >= fadeInTime)
                {
                    ALFUtils.FadeObject(curtain,+(fadeInDelta * Time.deltaTime) + 0.001f);
                }
                else
                {
                    if (bInit)
                    {
                        Camera.main.backgroundColor = Color.black;
                        if(runningScene != null)
                        {
                            runningScene.Transition = false;
                            runningScene.OnExit();
                            runningScene.Dispose();
                            ALF.MACHINE.StateMachine.UnscheduleUpdate();
                        }
                        
                        LayoutManager.Instance.DestroyUI();     
#if USE_NETWORK
                        NetworkManager.SetAdapterScene(nextScene);
#endif
                        nextScene.OnEnter();
                        bInit = false;
                        
                        if(bCurtain)
                        {
                            total = 0.2f;
                            fadeInTime = 100;
                        }
                    }

                    if(!bCurtain)
                    {
                        ALFUtils.FadeObject(curtain,-(fadeInDelta * Time.deltaTime + 0.001f));
                    }
                }
                
                if(duration > 0)
                {
                    yield return null;
                }
            }
            runningScene = nextScene;
            nextScene = null;
            runningScene.OnTransitionEnter();

            curtain.gameObject.SetActive(false);
            pImage.enabled = false;

            runningScene.Transition = false;
        }

        public void RunWithScene(IBaseScene scene,float duration,bool bCurtain = false)
        {
            nextScene = scene;
            StartCoroutine(coRunWithScene(duration,bCurtain));
        }

        public T GetActiveBaseScene<T>() where T : IBaseScene
        {
            if(runningScene != null)
            {
                return (T)runningScene;
            }

            return default(T);
        }
    }
}

