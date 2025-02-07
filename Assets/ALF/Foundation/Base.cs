// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
#if USE_NETWORK
using ALF.NETWORK;
#endif

namespace ALF
{
    public interface IBase
    {
        void Dispose();
    }

    public interface IUpdate
    {
        void Update(float dt);
    }

#if USE_NETWORK
public interface IBaseScene : IBase , IBaseNetwork, ISoketNetwork
#else
public interface IBaseScene : IBase
#endif
    {
        bool Transition {get; set;}
        
        void OnEnter();
        void OnTransitionEnter();
        void OnExit();
#if USE_NETWORK
        void RefreshServerData();
#endif
        T GetInstance<T>() where T : IBaseUI, new();
        bool IsShowInstance<T>() where T : IBaseUI;
    }

    public interface IBaseUI : IBase
    {
        void OnInit( IBaseScene pBaseScene,RectTransform pMainUI);
        RectTransform MainUI {get;}

        void Close();
    }

    public interface ITimer : IBaseUI
    {
        void DoExpire(int index);
    }

    public interface IStateTarget : IBase
    {
        T GetMainTarget<T>() where T : Object;
    }

    public interface IBaseActor : IStateTarget
    {
        RectTransform MainUI {get;}
        Coroutine AnimationCoroutine {get;}
        string ID { get;}
        public float Speed { get; set;}
        void ChangeAnimation(string animation,float delay = 0, byte dir = 0);
        bool IsPlaying(string animation);
        void StopAnimation();
        void Reset();
        float GetAnimationDuration(string name);
        void Flip(bool bFlip);

        IBaseActor Clone();
    }

    
    public class MonoBase : MonoBehaviour, IBase
    {
        System.Action<bool> onApplicationFocus = null;
        System.Action<bool> onApplicationPause = null;
        System.Action onApplicationQuit  = null;

        public void Dispose()
        {
            OnDispose();
        }

        public virtual void OnDispose(){}

        void OnDestroy() 
        {
            Dispose();
        }

        public void SetApplicationFocus(System.Action<bool> applicationFocusCallback)
        {
            onApplicationFocus = applicationFocusCallback;
        }

        public void SetApplicationPause(System.Action<bool> applicationPauseCallback)
        {
            onApplicationPause = applicationPauseCallback;
        }

        public void SetApplicationQuit(System.Action applicationQuit)
        {
            onApplicationQuit = applicationQuit;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if(onApplicationFocus != null)
            {
                onApplicationFocus(hasFocus);
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {   
            if(onApplicationPause != null)
            {
                onApplicationPause(pauseStatus);
            }
        }

        void OnApplicationQuit()
        {
            if(onApplicationQuit != null)
            {
                onApplicationQuit();
            }

        }
    }
}
