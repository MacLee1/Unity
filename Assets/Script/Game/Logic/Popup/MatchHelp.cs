using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.CONDITION;
using ALF.STATE;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
using STATEDATA;
using MATCHTEAMDATA;
using Newtonsoft.Json.Linq;
// using PLAYERNATIONALITY;

public class MatchHelp : IBaseUI
{
    MainScene m_pMainScene = null;
    MaskImage[] m_pMaskImages = null;
    RectTransform[] m_pRects = null;
    BaseState m_pBaseState = null; 
    Transform m_pRoot = null;
    Animation[] m_pStepRoot = null;
    byte m_eHelpStep = 0;
    public RectTransform MainUI { get; private set;}

    public MatchHelp(){}
    
    public void Dispose()
    {
        m_pRoot = null;
        MainUI = null;
        int i =0;
        for(i =0; i < m_pMaskImages.Length; ++i)
        {
            m_pMaskImages[i] = null;
            m_pRects[i] = null;
        }
        for(i =0; i < m_pStepRoot.Length; ++i)
        {
            m_pStepRoot[i] = null;
        }

        m_pStepRoot = null;
        m_pMaskImages = null;
        m_pRects = null;
        if(m_pBaseState != null)
        {
            m_pBaseState.Exit(true);
            m_pBaseState = null;
        }
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "MatchHelp : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "MatchHelp : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pRoot = MainUI.Find("root");

        m_pMaskImages = MainUI.GetComponentsInChildren<MaskImage>(true);
        m_pRects = new RectTransform[m_pMaskImages.Length];
        for(int i =0; i < m_pMaskImages.Length; ++i)
        {
            m_pRects[i] = m_pMaskImages[i].transform.Find("rect").GetComponent<RectTransform>();
        }

        m_pStepRoot = m_pRoot.GetComponentsInChildren<Animation>(true);
        
        for(int i = 0; i < m_pStepRoot.Length; ++i)
        {
            m_pStepRoot[i].gameObject.SetActive(false);
        }

        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
        RectTransform ui = pMatchView.GetHelpTarget(1);

        Vector2 size = m_pRects[0].sizeDelta;
        size.x = ui.rect.width;
        size.y = ui.rect.height;
        m_pRects[0].sizeDelta = size;
        m_pRects[0].pivot = ui.pivot;
        m_pRects[0].position = ui.position;
        m_pMaskImages[0].SetMaskNode(m_pRects[0]);
        
        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        MainUI.gameObject.SetActive(false);
        m_pRoot.gameObject.SetActive(false);
    }
    
    void SetupStep()
    {
        if(m_eHelpStep > 5) return;

        m_pRoot.gameObject.SetActive(true);

        DilogMoveStateData data =new DilogMoveStateData();
        data.FadeDelta = 1;
        ALFUtils.FadeObject(m_pRoot,0);
        data.Out = false;
        float duration = 0.5f;
        if(m_eHelpStep == 5)
        {
            data.Out = true;
        }
        else
        {
            m_pStepRoot[m_eHelpStep - 1].gameObject.SetActive(true);
            m_pStepRoot[m_eHelpStep - 1].Play();
            duration = m_pStepRoot[m_eHelpStep - 1].clip.length;
            MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
            RectTransform ui = pMatchView.GetHelpTarget(m_eHelpStep);
            
            if(m_eHelpStep == 1)
            {
                m_pRects[1].sizeDelta = Vector2.zero;
                m_pRects[1].anchoredPosition = Vector2.zero;
                m_pMaskImages[1].SetMaskNode(m_pRects[1]);
            }
            else
            {
                Vector2 size = m_pRects[1].sizeDelta;
                if(m_eHelpStep == 2)
                {
                    size.x = ui.rect.width;
                    size.y = ui.rect.height;
                    m_pRects[1].pivot = ui.pivot;
                    m_pRects[1].position = ui.position;
                }
                else
                {
                    size.x = ui.position.x + (ui.rect.width - ui.rect.width * ui.pivot.x) - m_pRects[1].position.x;
                }
                m_pRects[1].sizeDelta = size;
                m_pMaskImages[1].SetMaskNode(m_pRects[1]);
                m_pRects[1].gameObject.SetActive(true);
                m_pMaskImages[1].gameObject.SetActive(false);
                m_pMaskImages[1].gameObject.SetActive(true);
            }
        }

        m_pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pRoot),duration +0.5f, (uint)E_STATE_TYPE.Timer, null, executeCallback,exitCallback);
        m_pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(m_pBaseState);
    }

    bool executeCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            Transform tm = target.GetMainTarget<Transform>();
            if(data.Out)
            {
                ALFUtils.FadeObject(tm,-(data.FadeDelta * dt));
            }
            else
            {
                ALFUtils.FadeObject(tm,data.FadeDelta * dt);
            }
        }
        
        return bEnd;
    }

    IState exitCallback(IState state)
    {
        m_pBaseState = null;
        if(m_eHelpStep == 1)
        {
            m_pMainScene.GetInstance<MatchView>().PauseMatch();
        }
        else if(m_eHelpStep == 5)
        {
            Close();
        }

        return null;
    }

    public void PlayNextStep()
    {
        if(m_pBaseState != null)
        {
            if(m_eHelpStep < 5)
            {
                m_pStepRoot[m_eHelpStep - 1][m_pStepRoot[m_eHelpStep - 1].clip.name].time  = m_pStepRoot[m_eHelpStep - 1].clip.length;
                m_pBaseState.Exit(true);
            }
            else
            {
                Close();
                return;
            }
        }

        ++m_eHelpStep;    
        SetupStep();    
    }
    
    public void Close()
    {
        if(GameContext.getCtx().GetTutorial() == 38)
        {
            
            // GameContext.getCtx().SetTutorial(255);
            // JObject pJObject = new JObject();
            // pJObject["progress"] = 255;
            // m_pMainScene.RequestAfterCall(E_REQUEST_ID.tutorial_put,pJObject);
        }

        if(m_pBaseState != null)
        {
            m_pBaseState.SetExitCallback(null);
            m_pBaseState.Exit(true);
            m_pBaseState = null;
        }

        m_eHelpStep = 0;

        for(int i = 0; i < m_pStepRoot.Length; ++i)
        {
            m_pStepRoot[i].gameObject.SetActive(false);
        }
        ALFUtils.FadeObject(m_pRoot,0);
        LayoutManager.Instance.InteractableEnabledAll();
        MainUI.gameObject.SetActive(false);
        m_pMainScene.GetInstance<MatchView>().ResumeMatch();
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {        
        if(sender.name == "back" )
        {
            PlayNextStep();
        }
    }
}
