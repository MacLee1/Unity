using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using ALF;

public class GameActor : IBaseActor
{
    // readonly static Color[] BASE_COLOR = new Color[]{ new Color(0.9f,0.9f,0.6f,1),new Color(0.93f,0.68f,0.44f,1),new Color(0.8f,0.7f,0.1f,1),new Color(0.68f,0.37f,0.1f,1),new Color(0.31f,0.16f,0.01f,1) };
    // readonly static string[] HAIR_LIST = new string[]{ "mh0001","mh0002","mh0003","mh0004","mh0005","mh0006"};
    // readonly static string[] COSTUME_LIST = new string[]{ "ms0001","ms0002","ms0003","ms0004","ms0005","ms0006"};
    // readonly static string[] ACCESSORIES_LIST = new string[]{ "ma0001","ma0002","ma0003","ma0004"};
    // const string defaultSkinName = "etc";

    // protected SpriteRenderer spriteRenderer = null;
    MainScene m_pMainScene = null;
    public RectTransform MainUI { get; private set; }
    public Coroutine AnimationCoroutine { get; private set; }
    public float Speed { get; set;}
    public string ID { get; private set;}

    protected Graphic[] uiGraphics = null;
    protected Animation animations = null;

    public GameActor(MainScene rootScene,RectTransform targetUI,string id)
    {
        ALFUtils.Assert(rootScene != null, "GameActor : rootScene is null!!");
        ALFUtils.Assert(targetUI != null, "GameActor : targetUI is null!!");
        m_pMainScene = rootScene;
        MainUI = targetUI;
        ID = id;
        OnInit();
    }

    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
    }

    public void Flip(bool bFlip)
    {
        
    }

    public T GetMainTarget<T>() where T : UnityEngine.Object
    {
        if(MainUI != null)
        {
            UnityEngine.Object tm = MainUI;
            return (T)tm;
        }
        return default(T);
    }

    public IBaseActor Clone()
    {
        if(m_pMainScene != null && MainUI != null)
        {
            GameActor temp = new GameActor(m_pMainScene,GameObject.Instantiate<RectTransform>(MainUI),ID);
            return temp;
        }
        return null;
    }

    void OnInit()
    {
        Graphic[] list = MainUI.GetComponentsInChildren<Graphic>(true);
        uiGraphics = new Graphic[list.Length];
        animations = MainUI.GetComponent<Animation>();
        
        int ii = 0;
        for(int n =0; n < list.Length; ++n)
        {
            ii = int.Parse(list[n].gameObject.name);
            uiGraphics[ii] = list[n];
        }
        MainUI.gameObject.SetActive(false);

        // LayoutManager.SetReciveUIButtonEvent(m_pTargetUI,this.ButtonEventCall);
    }

    public void StopAnimation()
    {
        if(AnimationCoroutine != null)
        {
            Director.Runner.StopCoroutine(AnimationCoroutine);
            AnimationCoroutine = null;
        }

        if(animations != null)
        {
            animations.Stop();
        }
    }

    public void ChangeAnimation(string aniName,float delay = 0, byte dir = 0)
    {
        if(animations != null)
        {
            for(byte i =0; i < uiGraphics.Length; ++i)
            {
                uiGraphics[i].gameObject.SetActive(dir == i);
            }
            AnimationCoroutine = Director.Runner.StartCoroutine(playAnimation(aniName,delay));
        }   
    }

    public float GetAnimationDuration(string _name)
    {
        if(animations != null)
        {
            AnimationState pAnimationState = animations[_name];
            if(pAnimationState != null)
            {
                return pAnimationState.length;
            }
        }

        return 0;
    }

    IEnumerator playAnimation(string aniName, float delay) 
    {
        if(delay > 0)
        {
            ALFUtils.FadeObject(MainUI,-1);
            yield return new WaitForSeconds(delay);
            ALFUtils.FadeObject(MainUI,1);
        }

        AnimationCoroutine = null;
        animations.Play(aniName);
        yield return new WaitForSeconds(GetAnimationDuration(aniName));
    }

    public void Reset()
    {
        StopAnimation();
    }
    public bool IsPlaying(string animation)
    {
        if(AnimationCoroutine == null)
        {
            return animations.IsPlaying(animation);
        }

        return true;
    }
}
