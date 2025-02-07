using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using Newtonsoft.Json.Linq;
using UnityEngine.UI.Extensions;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
// using DATA;
// using UnityEngine.EventSystems;
using STATEDATA;


public class ContentsUnlock : IBaseUI
{
    MainScene m_pMainScene = null;
    TMPro.TMP_Text m_pComment = null;
    bool m_bChangeStadium = false;
    RawImage m_pIcon = null;
    Transform m_pRoot = null;

    ParticleSystem[] m_pParticleSystem = new ParticleSystem[2];
    
    public RectTransform MainUI { get; private set;}

    public ContentsUnlock(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        m_pRoot = null;
        MainUI = null;
        m_pComment = null;
        if(m_pIcon != null)
        {
            m_pIcon.texture = null;
            m_pIcon = null;
        }
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "ContentsUnlock : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "ContentsUnlock : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pRoot = MainUI.Find("root");
        m_pParticleSystem[0] = MainUI.Find("FX_1").GetComponent<ParticleSystem>();
        m_pParticleSystem[1] = m_pRoot.Find("FX_2").GetComponent<ParticleSystem>();
        m_pComment = m_pRoot.Find("contents/title/text").GetComponent<TMPro.TMP_Text>();
        m_pIcon = m_pRoot.Find("contents/icon").GetComponent<RawImage>();
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    bool execute(IState state,float dt,bool bEnd)
    {
        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
        float t = condition.GetTimePercent();
        if(t >= 0.5f)
        {
            if(!m_pParticleSystem[1].isPlaying)
            {
                m_pParticleSystem[1].time = 0;
                m_pParticleSystem[1].Play(true);
                UIParticleSystem[] list = m_pParticleSystem[1].GetComponentsInChildren<UIParticleSystem>(true);
                for(int i =0; i < list.Length; ++i)
                {
                    list[i].StartParticleEmission();
                }
            }
           ALFUtils.FadeObject(m_pRoot,condition.GetTimePercent()); 
        }
        
        return bEnd;
    }

    IState exit(IState state)
    {
        LayoutManager.Instance.InteractableEnabledAll();
        ALFUtils.FadeObject(m_pRoot,1);
        return null;
    }

    public void PlayEffect()
    {
        m_pParticleSystem[0].time = 0;
        m_pParticleSystem[0].Play(true);
        m_pParticleSystem[1].gameObject.SetActive(true);
        UIParticleSystem[] list = m_pParticleSystem[0].GetComponentsInChildren<UIParticleSystem>(true);
        for(int i =0; i < list.Length; ++i)
        {
            list[i].StartParticleEmission();
        }

        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pRoot),m_pParticleSystem[0].main.duration+1, (uint)E_STATE_TYPE.Timer, null, execute,exit);
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    public void SetupData(JObject data)
    {
        m_bChangeStadium = false;
        ALFUtils.FadeObject(m_pRoot,-1);
        if(data != null && data.ContainsKey("ContentsUnlock"))
        {
            GameContext pGameContext = GameContext.getCtx();
            uint no = (uint)data["no"];
            string token = "";
            CONSTVALUE.E_CONST_TYPE eType = CONSTVALUE.E_CONST_TYPE.playerHpRecoveryPerMinute;
            if(no == pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_1))
            {
                if(pGameContext.IsLicenseContentsUnlock( CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_1))
                {
                    eType = CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_1;
                    token = "LISENCECOMPLETE_NEWCONTENTS_TXT_PERSONAL_TACTICS";
                }
            }
            else if(no == pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_2))
            {
                m_bChangeStadium = pGameContext.IsLicenseContentsUnlock( CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_2);
                if(m_bChangeStadium)
                {
                    eType = CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_2;
                    token = "LISENCECOMPLETE_STADIUMCHANGE_TXT_NEW_STADIUM";
                }
            }
            else if(no == pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_3))
            {
                if(pGameContext.IsLicenseContentsUnlock( CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_3))
                {
                    eType = CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_3;
                    token = "LISENCECOMPLETE_NEWCONTENTS_TXT_YOUTH";
                }
            }
            else if(no == pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_4))
            {
                m_bChangeStadium = pGameContext.IsLicenseContentsUnlock( CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_4);
                if(m_bChangeStadium)
                {
                    eType = CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_4;
                    token = "LISENCECOMPLETE_STADIUMCHANGE_TXT_NEW_STADIUM";
                }
            }
            else if(no == pGameContext.GetConstValue(CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_5))
            {
                if(pGameContext.IsLicenseContentsUnlock( CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_5))
                {
                    eType = CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_5;
                    token = "LISENCECOMPLETE_NEWCONTENTS_TXT_LEAGUE";
                }
            }

            if(eType != CONSTVALUE.E_CONST_TYPE.playerHpRecoveryPerMinute)
            {
                Sprite pSprite = AFPool.GetItem<Sprite>("Texture",eType.ToString());
                m_pIcon.texture = pSprite.texture;
                m_pIcon.SetNativeSize();
            }

            m_pComment.SetText(pGameContext.GetLocalizingText(token));
        }
    }

    public void Close()
    {
        if(m_bChangeStadium)
        {    
            m_pMainScene.GetInstance<Ground>().ChangeStadium();
        }
        m_pParticleSystem[1].gameObject.SetActive(false);
        SingleFunc.HideAnimationDailog(MainUI);
    }
    
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
    
}
