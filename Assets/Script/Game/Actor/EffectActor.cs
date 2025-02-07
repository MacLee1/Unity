using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using ALF;
using TMPro;

public class EffectActor : IBaseActor
{
    protected ParticleSystem m_pParticle = null;

    MainScene m_pMainScene = null;
    public RectTransform MainUI { get; private set; }
    public Coroutine AnimationCoroutine { get; private set; }
    public float Speed { get; set;}
    public string ID { get; private set;}


    // protected MaskableGraphic[] m_iconList = null;
    // protected Animation animations = null;

    public EffectActor(MainScene rootScene,RectTransform targetUI,string id)
    {
        ALFUtils.Assert(rootScene != null, "EffectActor : rootScene is null!!");
        ALFUtils.Assert(targetUI != null, "EffectActor : targetUI is null!!");
        m_pMainScene = rootScene;
        MainUI = targetUI;
        ID = id;
        OnInit();
    }

    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        m_pParticle = null;
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
            EffectActor temp = new EffectActor(m_pMainScene,GameObject.Instantiate<RectTransform>(MainUI),ID);
            return temp;
        }
        return null;
    }

    void OnInit()
    {
        m_pParticle = MainUI.GetComponentInChildren<ParticleSystem>();
    }


    public void StopAnimation()
    {
        if(AnimationCoroutine != null)
        {
            Director.Runner.StopCoroutine(AnimationCoroutine);
            AnimationCoroutine = null;
        }

        if(m_pParticle != null)
        {
            m_pParticle.Stop();
        }
    }

    public void ChangeAnimation(string aniName,float delay = 0, byte dir = 0)
    {
        if(m_pParticle != null)
        {
            AnimationCoroutine = Director.Runner.StartCoroutine(playAnimation(aniName,delay));
        }   
    }

    public float GetAnimationDuration(string _name)
    {
        if(m_pParticle != null)
        {
            return m_pParticle.main.duration;
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
        m_pParticle.Play(true);
        
        yield return new WaitForSeconds(GetAnimationDuration(aniName));
        ActorManager.Instance.AddBaseActor(this);
    }
    public void Reset()
    {
        StopAnimation();
    }

    public bool IsPlaying(string animation)
    {
        if(AnimationCoroutine == null)
        {
            return m_pParticle.isPlaying;
        }

        return true;
    }
}
