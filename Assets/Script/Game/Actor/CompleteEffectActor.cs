using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using ALF;
using TMPro;
using DATA;

public class CompleteEffectActor : IBaseActor
{
    MainScene m_pMainScene = null;
    public RectTransform MainUI { get; private set; }
    public Coroutine AnimationCoroutine { get; private set; }
    public float Speed { get; set;}
    public string ID { get; private set;}


    protected TMP_Text m_comment = null;
    protected RawImage m_pIcon = null;
    protected Animation animations = null;

    public CompleteEffectActor(MainScene rootScene,RectTransform targetUI,string id)
    {
        ALFUtils.Assert(rootScene != null, "CompleteEffectActor : rootScene is null!!");
        ALFUtils.Assert(targetUI != null, "CompleteEffectActor : targetUI is null!!");
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
    
    public T GetMainTarget<T>() where T : Object
    {
        if(MainUI != null)
        {
            Object tm = MainUI;
            return (T)tm;
        }
        return default(T);
    }
    public IBaseActor Clone()
    {
        if(m_pMainScene != null && MainUI != null)
        {
            CompleteEffectActor temp = new CompleteEffectActor(m_pMainScene,GameObject.Instantiate<RectTransform>(MainUI),ID);
            return temp;
        }
        return null;
    }

    void OnInit()
    {
        m_comment = MainUI.GetComponentInChildren<TMP_Text>();
        m_pIcon = MainUI.GetComponentInChildren<RawImage>(true);
        animations = MainUI.GetComponent<Animation>();
    }

    public void ChangeColor(Color color)
    {
        if(m_comment != null)
        {
            m_comment.color = color;
        }
    }

    public void SetupReward( uint reward)
    {
        if(m_comment != null)
        {
            m_comment.SetText($"+{ALFUtils.NumberToString(reward)}");
        }
    }

    public void ChangeIcon(string id)
    {
        if(m_pIcon != null)
        {
            m_pIcon.texture = null;
            Sprite pSprite = AFPool.GetItem<Sprite>("Texture",id);
            m_pIcon.texture = pSprite.texture;
        }
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
            AnimationCoroutine = Director.Runner.StartCoroutine(playAnimation(aniName,delay));
        }   
    }

    public  float GetAnimationDuration(string _name)
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
        ActorManager.Instance.AddBaseActor(this);
    }
    public  void Reset()
    {
        if(m_pIcon != null)
        {
            m_pIcon.texture = null;
        }

        StopAnimation();
    }

    public  bool IsPlaying(string animation)
    {
        if(AnimationCoroutine == null)
        {
            return animations.IsPlaying(animation);
        }

        return true;
    }
    
}
