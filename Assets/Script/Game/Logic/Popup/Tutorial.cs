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

public class Tutorial : IBaseUI
{
    MainScene m_pMainScene = null;
    MaskImage m_pMaskImage = null;
    RectTransform m_pMessageBox = null;
    RectTransform m_pCharacter = null;
    Animation m_pSwipeGesture = null;
    RectTransform m_pRect = null;
    GameObject m_pNextNode = null;
    Button m_pSkip = null;
    TMPro.TMP_Text m_pMessageText = null;
    
    BaseState m_pBaseState = null;
    Image m_pBack = null;
    Button m_pRectButton = null;
    
    byte m_eTutorialStep = 0;
    bool m_bNoCick = false;
    public RectTransform MainUI { get; private set;}

    public Tutorial(){}
    
    public void Dispose()
    {
        m_pNextNode = null;
        m_pRectButton = null;
        m_pMainScene = null;
        MainUI = null;
        m_pMaskImage = null;
        m_pRect = null;
        m_pMessageBox = null;
        m_pSwipeGesture = null;
        m_pSkip = null;
        m_pMessageText = null;
        m_pCharacter = null;
        m_pBack = null;
        if(m_pBaseState != null)
        {
            m_pBaseState.Exit(true);
            m_pBaseState = null;
        }
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Tutorial : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Tutorial : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pBack = MainUI.Find("back").GetComponent<Image>();
        m_pMaskImage = MainUI.Find("root/mask").GetComponent<MaskImage>();
        m_pRect = m_pMaskImage.transform.Find("rect").GetComponent<RectTransform>();
        m_pRectButton = m_pRect.GetComponent<Button>();
        m_pMessageBox = MainUI.Find("root/tipMessage").GetComponent<RectTransform>();
        m_pCharacter = m_pMessageBox.Find("character").GetComponent<RectTransform>();
        m_pSwipeGesture = MainUI.Find("root/swipeGesture").GetComponent<Animation>();
        m_pNextNode = MainUI.Find("root/tipMessage/next").gameObject;
        m_pSkip = m_pMessageBox.Find("skip").GetComponent<Button>();
        m_pMessageText = m_pMessageBox.Find("message").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);

        RectTransform tm = LayoutManager.Instance.GetMainUI();
        Vector2 size = tm.offsetMax * -1;
        MainUI.offsetMax = size;
    }

    void UpdateState()
    {
        MainUI.SetAsLastSibling();
        MainUI.gameObject.SetActive(true);
        PlayTutorialStep();
    }

    bool executeCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            if(data.Distance > 0)
            {
                data.Distance -= dt;
            }
            else
            {
                BaseStateTarget target = state.GetTarget<BaseStateTarget>();
                Transform tm = target.GetMainTarget<Transform>();
                ALFUtils.FadeObject(tm,data.Out ? -(data.FadeDelta * dt) : data.FadeDelta * dt);
            }
        }
        
        return bEnd;
    }
    IState exitCallback(IState state)
    {
        m_bNoCick = false;
        LayoutManager.Instance.InteractableEnabledAll();
        
        if( state.StateData is DilogMoveStateData data)
        {
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            Transform tm = target.GetMainTarget<Transform>();
            ALFUtils.FadeObject(tm,data.Out ? -1 : 1);
        }
        
        m_pBaseState = null;

        if(m_eTutorialStep == 17 || m_eTutorialStep == 18 || m_eTutorialStep == 20 || m_eTutorialStep == 21 )
        {
            m_pSwipeGesture.Play();
        }
        return null;
    }

    bool executeCheckSequence(IState state,float dt,bool bEnd)
    {
        BaseStateTarget target = state.GetTarget<BaseStateTarget>();
        if(!StateMachine.GetStateMachine().IsCurrentTargetStates(target.GetMainTarget<RectTransform>(),(uint)E_STATE_TYPE.ShowDailog))
        {
            if(m_eTutorialStep == 13)
            {
                Selectable[] selectableList = LayoutManager.Instance.GetMainUI().GetComponentsInChildren<Selectable>(true);
                for(int i =0; i < selectableList.Length; ++i)
                {
                    selectableList[i].enabled = true;
                }
            }
    
            UpdateState();
            bEnd = true;
        }
        
        return bEnd;
    }
    bool executeCheckSequence41(IState state,float dt,bool bEnd)
    {
        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
        
        if(pMatchView.GetTutorialStepUI(m_eTutorialStep).gameObject.activeSelf)
        {    
            UpdateState();
            bEnd = true;
        }
        
        return bEnd;
    }
    bool executeCheckSequence40(IState state,float dt,bool bEnd)
    {
        RectTransform pUI = LayoutManager.Instance.FindUIFormRoot<RectTransform>("Confirm");
        Animation pAnimation = pUI.GetComponent<Animation>();
        if(pAnimation.gameObject.activeSelf && pAnimation.IsPlaying("Popup_open"))
        {
            // pUI.SetParent(MainUI.parent,false);
            UpdateState();
            bEnd = true;
        }
        
        return bEnd;
    }

    bool executeCheckSequence37(IState state,float dt,bool bEnd)
    {
        MatchHelp pMatchHelp = m_pMainScene.GetInstance<MatchHelp>();
        if(!pMatchHelp.MainUI.gameObject.activeSelf)
        {
            ++m_eTutorialStep;
            UpdateState();
            bEnd = true;
        }
        
        return bEnd;
    }

    bool executeCheckSequence36(IState state,float dt,bool bEnd)
    {
        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
        if(pMatchView.CheckTutorialStep(m_eTutorialStep))
        {
            pMatchView.SetViewSpeed(3,false);
            UpdateState();
            bEnd = true;
        }
        
        return bEnd;
    }
    bool executeCheckSequence35(IState state,float dt,bool bEnd)
    {
        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
        if(pMatchView.CheckTutorialStep(m_eTutorialStep))
        {
            UpdateState();
            bEnd = true;
        }
        
        return bEnd;
    }
    bool executeCheckSequence34(IState state,float dt,bool bEnd)
    {
        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
        if(pMatchView.CheckTutorialStep(m_eTutorialStep))
        {
            UpdateState();
            bEnd = true;
        }
        
        return bEnd;
    }
    bool executeCheckSequence33(IState state,float dt,bool bEnd)
    {
        Match pMatch = m_pMainScene.GetInstance<Match>();
        Animation pAnimation = pMatch.MainUI.Find("root").GetComponent<Animation>();
        if(pMatch.MainUI.gameObject.activeSelf)
        {
            if(!StateMachine.GetStateMachine().IsCurrentTargetStates(pMatch.MainUI,(uint)E_STATE_TYPE.ShowDailog) && !pAnimation.IsPlaying("match_open")) 
            {
                UpdateState();
                bEnd = true;   
            }
        }
        
        return bEnd;
    }
    
    bool executeCheckSequence18(IState state,float dt,bool bEnd)
    {
        ScrollRect pScroll = m_pMainScene.GetInstance<TacticsFormation>().GetBenchScroll();
        
        float pos = pScroll.horizontalNormalizedPosition;
        if(pos >= 1)
        {
            pScroll.horizontalNormalizedPosition = 1;
            pScroll.velocity = Vector2.zero;
            // pScroll.enabled = false;
            RectTransform ui = pScroll.content.GetChild(pScroll.content.childCount -2).GetComponent<RectTransform>();
            
            m_pMaskImage.gameObject.SetActive(false);
            Vector2 size = m_pRect.sizeDelta;
            size.x = ui.rect.width;
            size.y = ui.rect.height;
            m_pRect.sizeDelta = size;
            
            m_pRect.pivot = ui.pivot;
            m_pRect.position = ui.position;
            m_pMaskImage.SetMaskNode(m_pRect);
            m_pMaskImage.gameObject.SetActive(true);
            
            m_pSwipeGesture.transform.position = m_pRect.position;
            ui = m_pSwipeGesture.GetComponent<RectTransform>();
            Vector2 ap = ui.anchoredPosition;
            // ap.y += m_pRect.rect.height * -0.5f;
            ap.x += m_pRect.rect.width * 0.5f;
            ui.anchoredPosition = ap;
            m_pSwipeGesture.Play();
            bEnd = true;
        }
        else
        {
            pos += 0.1f;
            pScroll.horizontalNormalizedPosition = pos;
        }
        
        return bEnd;
    }

    bool executeCheckSequence11(IState state,float dt,bool bEnd)
    {
        if(GameContext.getCtx().GetRecuitingCachePlayerDataCount() > 0)
        {
            Transfer pTransfer = m_pMainScene.GetInstance<Transfer>();
            List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(pTransfer.MainUI,(uint)E_STATE_TYPE.ShowDailog);
            if(list.Count == 0)
            {
                UpdateState();
                bEnd = true;
            }
        }
        
        return bEnd;
    }

    bool executeCheckSequence9(IState state,float dt,bool bEnd)
    {
        if(!StateMachine.GetStateMachine().IsCurrentTargetStates(m_pMainScene.GetInstance<Ground>().ShowTutorialButton(true),(uint)E_STATE_TYPE.PlayAndHide))
        {
            UpdateState();
            bEnd = true;
        }
        
        return bEnd;
    }

    bool executeCheckSequence7(IState state,float dt,bool bEnd)
    {
        if(GameContext.getCtx().GetCurrentBusinessLevelByID(1) > 10)
        {
            UpdateState();
            bEnd = true;
        }
        return bEnd;
    }

    bool executeCheckSequence6(IState state,float dt,bool bEnd)
    {
        if(GameContext.getCtx().GetCurrentBusinessLevelByID(1) > 0)
        {
            Management pManagement = m_pMainScene.GetInstance<Management>();
            RectTransform ui = pManagement.GetBusinessItemUI(0,"muti_up");
            List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(ui,(uint)E_STATE_TYPE.Timer);
            if(list.Count > 0)
            {
                CanvasRenderer[] renderers = ui.GetComponentsInChildren<CanvasRenderer>();
                for(int i = 0; i < renderers.Length; ++i)
                {
                    if(renderers[i].GetAlpha() >= 1)
                    {
                        for(i =0; i < list.Count; ++i)
                        {
                            list[i].Paused = true;
                        }

                        UpdateState();
                        bEnd = true;
                        break;
                    }
                }
            }
        }
        return bEnd;
    }

    bool executeCheckSequence5(IState state,float dt,bool bEnd)
    {
        RectTransform ui = m_pMainScene.GetInstance<Ground>().ShowTutorialButton(true).parent.GetComponent<RectTransform>();            
        if(!StateMachine.GetStateMachine().IsCurrentTargetStates(ui,(uint)E_STATE_TYPE.ScrollMove) && !StateMachine.GetStateMachine().IsCurrentTargetStates(m_pMainScene.GetInstance<Management>().MainUI,(uint)E_STATE_TYPE.ShowDailog))
        {
            UpdateState();
            bEnd = true;
        }
        
        return bEnd;
    }

    void CheckTutorialStep()
    {
        System.Func<IState,float,bool,bool> _executeCallback = null;

        switch(m_eTutorialStep)
        {
            case 4:
            case 12:
            case 13:
            case 16:
            case 26:
            {
                RectTransform ui = null;
                if(m_eTutorialStep == 4)
                {
                    Management pManagement = m_pMainScene.ShowManagement();
                    m_pMainScene.SetMainButton(0);
                    ui = pManagement.MainUI;
                }
                else if(m_eTutorialStep == 12)
                {
                    ui = m_pMainScene.GetInstance<PlayerInfo>().MainUI;
                }
                else if(m_eTutorialStep == 13)
                {
                    m_pMainScene.GetInstance<PlayerInfo>().Close();
                    ui = LayoutManager.Instance.FindUIFormRoot<RectTransform>("ToastMessage");
                    Selectable[] list = LayoutManager.Instance.GetMainUI().GetComponentsInChildren<Selectable>();
                    for(int i =0; i < list.Length; ++i)
                    {
                        list[i].enabled = false;
                    }
                }
                else if(m_eTutorialStep == 16)
                {
                    if(m_pMainScene.IsShowInstance<Transfer>())
                    {
                        m_pMainScene.GetInstance<Transfer>().Close();
                    }
                
                    TacticsFormation pTacticsFormation = m_pMainScene.ShowTacticsFormation();
                    m_pMainScene.SetMainButton(2);
                    ui = pTacticsFormation.MainUI;
                }
                else if(m_eTutorialStep == 26)
                {
                    ClubLicense pClubLicense = m_pMainScene.ShowClubLicensePopup();
                    ui = pClubLicense.MainUI;
                }
                
                MainUI.SetAsLastSibling();
                
                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(ui),-1, (uint)E_STATE_TYPE.Timer, null, executeCheckSequence);
                pBaseState.StateData = new FadeInOutStateData(-0.2f);
                StateMachine.GetStateMachine().AddState(pBaseState);
            }
            break;
            case 5:
            {
                _executeCallback = executeCheckSequence5;
            }
            break;
            case 6:
            {
                _executeCallback = executeCheckSequence6;
            }
            break;
            case 7:
            {
                Management pManagement = m_pMainScene.ShowManagement();
                pManagement.Close();
                m_pMainScene.ResetMainButton();
                m_pMaskImage.gameObject.SetActive(false);
                _executeCallback = executeCheckSequence7;
            }
            break;
            case 9:
            {
                Ground pGround = m_pMainScene.GetInstance<Ground>();
                pGround.PlayGetRewardBuildingEffect(pGround.ShowTutorialButton(true));
                _executeCallback = executeCheckSequence9;
            }
            break;
            case 11:
            {
                _executeCallback = executeCheckSequence11;
            }
            break;
            case 17:
            case 18:
            case 23:
            {
                if(m_eTutorialStep == 18)
                {
                    _executeCallback = executeCheckSequence18;
                }
                
                UpdateState();
            }
            break;
            case 19:
            case 20:
            case 21:
            {
                TacticsFormation pTacticsFormation = m_pMainScene.GetInstance<TacticsFormation>();
                pTacticsFormation.SetTutorial();
                if(m_eTutorialStep == 21)
                {
                    m_pMainScene.ResetMainButton();
                }
                UpdateState();
            }
            break;
            case 33:
            {
                _executeCallback = executeCheckSequence33;
            }
            break;
            case 34:
            {
                _executeCallback = executeCheckSequence34;
            }
            break;
            case 35:
            {
                _executeCallback = executeCheckSequence35;
            }
            break;
            case 36:
            {
                _executeCallback = executeCheckSequence36;
            }
            break;
            case 37:
            {
                _executeCallback = executeCheckSequence37;
            }
            break;
            case 40:
            {
                _executeCallback = executeCheckSequence40;
            }
            break;
            case 41:
            {
                _executeCallback = executeCheckSequence41;
            }
            break;
        }
        
        if(_executeCallback != null)
        {
            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(MainUI),-1, (uint)E_STATE_TYPE.Timer, null, _executeCallback);
            StateMachine.GetStateMachine().AddState(pBaseState);
        }
    }

    public void PlayTutorialStep()
    {
        m_bNoCick = true;
        
        LayoutManager.Instance.InteractableDisableAll();
        m_pMessageBox.gameObject.SetActive(true);
        m_pMessageText.gameObject.SetActive(true);
        m_pCharacter.gameObject.SetActive(true);
        BaseStateTarget pBaseStateTarget = null;
        DilogMoveStateData data = null;
        
        float duration = 0;
        ++m_eTutorialStep;
        Debug.Log($"SetTutorialStep-------------------:{m_eTutorialStep}");
        GameContext.getCtx().SetTutorial(m_eTutorialStep);
        m_pNextNode.SetActive(true);

        switch(m_eTutorialStep)
        {
            /**
            * 라지 팝업
            */
            case 1:
            case 8:
            case 10:
            case 14:
            case 22:
            case 24:
            case 27:
            case 28:
            case 29:
            case 30:
            case 32:
            {
                m_pMaskImage.gameObject.SetActive(false);
                m_pMessageBox.sizeDelta = new Vector2(LayoutManager.Width - 200,200);
                m_pMessageBox.anchoredPosition = new Vector2(200,0);
                m_pCharacter.localScale = Vector3.one;
                m_pCharacter.anchoredPosition = new Vector2(-200,0);

                RectTransform ui = m_pMessageText.GetComponent<RectTransform>();
                Vector2 offset = ui.offsetMin;
                offset.x = 120;
                ui.offsetMin = offset;
                offset = ui.offsetMax;
                offset.x = -50;
                ui.offsetMax = offset;

                ALFUtils.FadeObject(MainUI.Find("root"),-1);
                m_pMessageText.SetText(GameContext.getCtx().GetLocalizingText($"TUTORIAL_TXT_{m_eTutorialStep}"));

                pBaseStateTarget = new BaseStateTarget(MainUI.Find("root"));
                duration = 0.8f;
                
                data = new DilogMoveStateData();
                data.Distance = 0.5f;
                data.FadeDelta = 1 / 0.3f;
                data.Out = false;

                if(m_eTutorialStep == 27 || m_eTutorialStep == 28)
                {
                    ClubLicense pClubLicense = m_pMainScene.GetInstance<ClubLicense>();
                    ScrollRect pScroll = pClubLicense.MainUI.GetComponentInChildren<ScrollRect>(true);
                    ui = pScroll.content.GetChild(0).GetComponent<RectTransform>();
                    Vector2 size = m_pRect.sizeDelta;
                    size.x = ui.rect.width;
                    size.y = ui.rect.height;
                    ui = ui.Find("content").GetComponent<RectTransform>();
                    size.y += ui.sizeDelta.y;
                    m_pRect.sizeDelta = size;
                    m_pRect.pivot = ui.pivot;
                    m_pRect.position = ui.position;
                    m_pMaskImage.SetMaskNode(m_pRect);
                    m_pMaskImage.gameObject.SetActive(true);
                    m_pRect.GetComponent<Image>().raycastTarget = false;
                }
                else if(m_eTutorialStep == 29)
                {
                    m_pMainScene.GetInstance<ClubLicense>().Close();
                }
                else if(m_eTutorialStep == 32)
                {
                    m_pMainScene.GetInstance<Rookie>().Close();
                    GameContext.getCtx().SendAdjustEvent("jn3uqm",false,false,-1);
                }
            }
            break;
            /**
            * 라지 팝업
            */
            case 2:
            case 3:
            case 15:
            case 25:
            case 37:
            case 39:
            case 43:
            case 44:
            {
                if(m_eTutorialStep == 25)
                {
                    m_pMainScene.ShowTopSubUI(true);
                }
                m_pMessageBox.sizeDelta = new Vector2(LayoutManager.Width - 200,200);
                m_pCharacter.localScale = Vector3.one;
                m_pCharacter.anchoredPosition = new Vector2(-200,0);
                RectTransform ui = m_pMessageText.GetComponent<RectTransform>();
                Vector2 offset = ui.offsetMin;
                offset.x = 120;
                ui.offsetMin = offset;
                offset = ui.offsetMax;
                offset.x = -50;
                ui.offsetMax = offset;

                ALFUtils.FadeObject(m_pMessageText.transform,-1);
                m_pMessageText.SetText(String.Format(GameContext.getCtx().GetLocalizingText($"TUTORIAL_TXT_{m_eTutorialStep}"),GameContext.getCtx().GetClubName()));
                pBaseStateTarget = new BaseStateTarget(m_pMessageText.transform);
                duration = 0.5f;

                data = new DilogMoveStateData();
                data.Distance = 0;
                data.FadeDelta = 1 / 0.5f;
                data.Out = false;

                if(m_eTutorialStep == 37 || m_eTutorialStep == 39|| m_eTutorialStep == 43 || m_eTutorialStep == 44)
                {
                    m_pRect.sizeDelta = Vector2.zero;
                    m_pMaskImage.SetMaskNode(m_pRect);
                    m_pMaskImage.gameObject.SetActive(true);
                }
            }
            break;
            /**
            * 스몰 팝업
            */
            case 4:
            case 5:
            case 6:
            case 7:
            case 9:
            case 11:
            case 12:
            case 13:
            case 16:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 23:
            case 26:
            case 31:
            case 33:
            case 34:
            case 35:
            case 36:
            case 40:
            case 41:
            case 42:
            {
                m_pMainScene.ShowMainUI(true);
                m_pRect.gameObject.SetActive(true);

                RectTransform ui = null;
                switch(m_eTutorialStep)
                {
                    case 4:
                    {
                        m_pNextNode.SetActive(false);
                        ui = LayoutManager.Instance.FindUI<RectTransform>("MenuBottom/buttons/Management");
                    }
                    break;
                    case 5:
                    {
                        m_pNextNode.SetActive(false);
                        Management pManagement = m_pMainScene.GetInstance<Management>();
                        ui = pManagement.GetBusinessItemUI(0,"btn_up");
                    }
                    break;
                    case 6:
                    {
                        m_pNextNode.SetActive(false);
                        Management pManagement = m_pMainScene.GetInstance<Management>();
                        ui = pManagement.GetBusinessItemUI(0,"btn_up");
                    }
                    break;
                    case 7:
                    {
                        m_pNextNode.SetActive(false);
                        Management pManagement = m_pMainScene.GetInstance<Management>();
                        ui = pManagement.GetBusinessItemUI(0,"muti_up");
                    }
                    break;
                    case 9:
                    {
                        m_pNextNode.SetActive(false);
                        ui = m_pMainScene.GetInstance<Ground>().ShowTutorialButton(false);
                    }
                    break;
                    case 11:
                    {
                        m_pNextNode.SetActive(false);
                        ui = LayoutManager.Instance.FindUI<RectTransform>("MenuBottom/buttons/Transfer");
                    }
                    break;
                    case 12:
                    {
                        m_pNextNode.SetActive(false);
                        Transfer pTransfer = m_pMainScene.GetInstance<Transfer>();
                        ui = pTransfer.GetRecuitingItemUI(1);
                    }
                    break;
                    case 13:
                    {
                        m_pNextNode.SetActive(false);
                        PlayerInfo pPlayerInfo = m_pMainScene.GetInstance<PlayerInfo>();
                        ui = pPlayerInfo.MainUI.Find("root/bg/offer/contact/sign").GetComponent<RectTransform>();
                        ui.Find("on").gameObject.SetActive(true);
                        ui.Find("off").gameObject.SetActive(false);
                    }
                    break;
                    case 16:
                    {
                        m_pNextNode.SetActive(false);
                        ui = LayoutManager.Instance.FindUI<RectTransform>("MenuBottom/buttons/TacticsFormation");
                    }
                    break;
                    case 17:
                    case 18:
                    case 19:
                    {
                        m_pNextNode.SetActive(false);
                        ScrollRect pScroll = m_pMainScene.GetInstance<TacticsFormation>().GetBenchScroll();
                        ui = pScroll.viewport;
                        m_pSwipeGesture.gameObject.SetActive(true);
                    }
                    break;
                    case 20:
                    {
                        m_pNextNode.SetActive(false);
                        ui = m_pMainScene.GetInstance<TacticsFormation>().GetTutorialFormation();
                        m_pSwipeGesture.gameObject.SetActive(true);
                    }
                    break;
                    case 21:
                    {
                        m_pNextNode.SetActive(false);
                        ui = LayoutManager.Instance.FindUI<RectTransform>("MenuBottom/buttons/TacticsFormation");
                        m_pSwipeGesture.gameObject.SetActive(true);
                    }
                    break;
                    case 31:
                    {
                        Rookie pUI = m_pMainScene.GetInstance<Rookie>();
                        pUI.MainUI.SetAsLastSibling();
                        pUI.MainUI.gameObject.SetActive(true);
                        ui = pUI.MainUI;
                        MainUI.SetAsLastSibling();
                        m_pRectButton.GetComponent<Image>().raycastTarget = false;
                    }
                    break;
                    case 23:
                    case 33:
                    {
                        ui = m_pMainScene.GetMatchButton();
                        ui.gameObject.SetActive(true);
                        m_pRectButton.GetComponent<Image>().raycastTarget = m_eTutorialStep == 33;
                    }
                    break;
                    case 26:
                    {
                        m_pNextNode.SetActive(false);
                        ui = LayoutManager.Instance.FindUI<RectTransform>("SubMenu/clubLicense");
                        m_pRectButton.GetComponent<Image>().raycastTarget = true;
                    }
                    break;
                    case 34:
                    {
                        m_pSkip.gameObject.SetActive(false);
                        m_pNextNode.SetActive(false);
                        Match pMatch = m_pMainScene.GetInstance<Match>();
                        ui = pMatch.GetTutorialButton();
                        m_pRectButton.GetComponent<Image>().raycastTarget = true;
                    }
                    break;
                    case 35:
                    {
                        m_pSkip.gameObject.SetActive(false);
                        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
                        ui = pMatchView.GetTutorialStepUI(m_eTutorialStep);
                        m_pNextNode.SetActive(false);
                        m_pRectButton.GetComponent<Image>().raycastTarget = true;
                    }
                    break;
                    case 36:
                    {
                        m_pSkip.gameObject.SetActive(false);
                        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
                        ui = pMatchView.GetTutorialStepUI(m_eTutorialStep);
                        m_pNextNode.SetActive(false);
                        m_pRectButton.GetComponent<Image>().raycastTarget = true;
                    }
                    break;
                    case 40:
                    {
                        m_pSkip.gameObject.SetActive(false);
                        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
                        ui = pMatchView.GetTutorialStepUI(m_eTutorialStep);
                        m_pNextNode.SetActive(false);
                        m_pRectButton.GetComponent<Image>().raycastTarget = true;
                    }
                    break;
                    case 41:
                    {
                        m_pSkip.gameObject.SetActive(false);
                        Confirm pUI = m_pMainScene.GetInstance<Confirm>();
                        ui = pUI.MainUI.Find("root/confirm").GetComponent<RectTransform>();
                        m_pNextNode.SetActive(false);
                        m_pRectButton.GetComponent<Image>().raycastTarget = true;
                    }
                    break;
                    case 42:
                    {
                        m_pSkip.gameObject.SetActive(false);
                        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
                        ui = pMatchView.GetTutorialStepUI(m_eTutorialStep);
                        m_pNextNode.SetActive(false);
                        m_pRectButton.GetComponent<Image>().raycastTarget = true;
                    }
                    break;

                }
                
                Vector2 size = m_pRect.sizeDelta;
                if(ui != null)
                {
                    size.x = ui.rect.width;
                    size.y = ui.rect.height;
                    m_pRect.pivot = ui.pivot;
                    m_pRect.position = ui.position;
                }
                else
                {
                    size.x =0;
                    size.y =0; 
                }
                
                m_pRect.sizeDelta = size;
                
                m_pMaskImage.SetMaskNode(m_pRect);
                m_pMaskImage.gameObject.SetActive(true);
                m_pMessageBox.gameObject.SetActive(true);
                m_pMessageText.gameObject.SetActive(true);
                Vector2 pos = m_pRect.anchoredPosition;
                m_pMessageBox.sizeDelta = new Vector2(600,200);
                m_pSwipeGesture.transform.position = m_pRect.position;

                if(m_eTutorialStep == 21)
                {
                    m_pMessageBox.gameObject.SetActive(false);
                }

                switch(m_eTutorialStep)
                {
                    case 4:
                    {
                        pos.x -= m_pRect.rect.width * m_pRect.pivot.x;
                        pos.y += m_pRect.rect.height * m_pRect.pivot.y + 20;
                    }
                    break;
                    case 5:
                    case 6:
                    case 7:
                    {
                        pos.y -= m_pRect.rect.height * m_pRect.pivot.y + 220 + 100;
                        pos.x -= m_pRect.rect.width * m_pRect.pivot.x + m_pMessageBox.rect.width - m_pRect.rect.width;
                    }
                    break;
                    case 9:
                    {
                        if(LayoutManager.Width - pos.x > m_pMessageBox.rect.width)
                        {
                            pos.x += m_pRect.sizeDelta.x * m_pRect.pivot.x + 20;
                        }
                        else
                        {
                            pos.x -= m_pMessageBox.rect.width * 0.5f;
                            pos.y += m_pRect.rect.height + 20 + 100;
                        }
                    }
                    break;
                    case 11:
                    case 16:
                    case 21:
                    case 40:
                    case 42:
                    {
                        pos.x -= m_pRect.rect.width * -m_pRect.pivot.x + m_pMessageBox.rect.width;
                        pos.y += m_pRect.rect.height * m_pRect.pivot.y + 20;
                    }
                    break;
                    case 12:
                    {
                        pos.x -= m_pRect.rect.width * -m_pRect.pivot.x + m_pMessageBox.rect.width + 20;
                        pos.y -= m_pRect.rect.height * m_pRect.pivot.y + 20 + m_pMessageBox.rect.height + 100;
                    }
                    break;
                    case 13:
                    case 41:
                    {
                        pos.x -= m_pMessageBox.rect.width;
                        pos.y -= m_pRect.rect.height * m_pRect.pivot.y + 20 + m_pMessageBox.rect.height + 100;
                    }
                    break;
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 23:
                    case 33:
                    case 34:
                    case 36:
                    {
                        if(m_eTutorialStep == 20)
                        {
                            pos.x += (m_pRect.rect.width - m_pMessageBox.rect.width * 2 ) * 0.5f + 20;
                            pos.y -= m_pRect.rect.height * m_pRect.pivot.y + m_pMessageBox.rect.height + 120;
                        }
                        else
                        {
                            pos.x -= m_pMessageBox.rect.width * 0.5f;
                            pos.y += m_pRect.rect.height * (1 - m_pRect.pivot.y) + 20;
                        }
                    }
                    break;
                    case 26:
                    {
                        pos.y -= m_pRect.rect.height;
                        pos.x += m_pRect.rect.width + 20;
                    }
                    break;
                    case 31:
                    case 35:
                    {
                        pos.x += (m_pRect.rect.width - m_pMessageBox.rect.width * 2 ) * 0.5f - 20;
                        pos.y -= m_pRect.rect.height * m_pRect.pivot.y - m_pMessageBox.rect.height - 20;
                    }
                    break;
                }

                m_pMessageBox.anchoredPosition = pos;

                m_pCharacter.localScale = Vector3.one * 0.5f;
                m_pCharacter.anchoredPosition = new Vector2(0,0);

                ui = m_pMessageText.GetComponent<RectTransform>();
                Vector2 offset = ui.offsetMin;
                offset.x = 160f;
                ui.offsetMin = offset;
                offset = ui.offsetMax;
                offset.x = -50;
                ui.offsetMax = offset;

                ALFUtils.FadeObject(MainUI.Find("root"),-1);
                m_pMessageText.SetText(GameContext.getCtx().GetLocalizingText($"TUTORIAL_TXT_{m_eTutorialStep}"));
                pBaseStateTarget = new BaseStateTarget(MainUI.Find("root"));
                duration = 0.5f;

                data = new DilogMoveStateData();
                data.Distance = 0;
                data.FadeDelta = 1 / 0.5f;
                data.Out = false;
            }
            break;
            case 45:
            {
                SendTutorialStep();
            }
            return;
        }

        m_pBaseState = BaseState.GetInstance(pBaseStateTarget,duration, (uint)E_STATE_TYPE.ShowDailog, null, this.executeCallback, this.exitCallback);
        m_pBaseState.StateData = data;

        StateMachine.GetStateMachine().AddState(m_pBaseState);
    }

    void Hide()
    {
        GameContext.getCtx().SetTutorial(m_eTutorialStep);
        MainUI.gameObject.SetActive(false);
        m_pMaskImage.gameObject.SetActive(false);
        
        SendTutorialStep();
    }
    
    public void SetTutorialStep(byte eStep)
    {
        m_eTutorialStep = eStep;
        if(m_eTutorialStep == 0)
        {
            GameContext.getCtx().SendAdjustEvent("qw3adx",false,false,-1); // TutorialStart
        }
        m_pBack.raycastTarget = true;
        m_pMainScene.ShowMainUI(m_eTutorialStep > 2);
        m_pCharacter.gameObject.SetActive(false);
        m_pRect.gameObject.SetActive(false);
        m_pMessageBox.gameObject.SetActive(false);
        m_pSwipeGesture.gameObject.SetActive(false);
        m_pMessageText.gameObject.SetActive(false);
        m_pMaskImage.gameObject.SetActive(false);
    }

    public void Close()
    {
        
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {        
        if(sender.name == "back" )
        {
            if(m_bNoCick) return;

            switch(m_eTutorialStep)
            {
                case 4:
                case 5:
                case 6:
                case 7:
                case 9:
                case 11:
                case 12:
                case 13:
                case 16:
                case 19:
                case 20:
                case 21:
                case 26:
                case 33:
                case 34:
                case 35:
                case 36:
                case 40:
                case 41:
                return;
                case 18: Hide();        return;
                case 37: NextStep();    return;
                case 44: Skip();        return;
            }
            
            PlayTutorialStep();
        }
        else if(sender.name == "rect" )
        {
            if(m_eTutorialStep > 32)
            {
                NextStep();
            }
            else
            {
                Hide();
            }
        }
        else if(sender.name == "skip" )
        {
            ShowSkipPopup();
        }
    }

    public void ShowSkipPopup()
    {
        GameContext pGameContext = GameContext.getCtx();
        m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_TUTORIALSKIP_TITLE"),pGameContext.GetLocalizingText("DIALOG_TUTORIALSKIP_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,this.Skip);
    }

    void SendTutorialStep()
    {
        JObject pJObject = new JObject();
        pJObject["progress"] = m_eTutorialStep;
        string token = null;
        switch(m_eTutorialStep)
        {
            case 7: token = "s040b1"; break;
            case 9: token = "q1kivg"; break;
            case 13: token = "6qz4o8"; break;
            case 21: token = "cnn6kq"; break;
            case 23: token = "lgkmdd"; break;
            case 26: token = "hz4brk"; break;
            case 45: token = "ktuojx"; break;
        }
        if(!string.IsNullOrEmpty(token))
        {
            GameContext.getCtx().SendAdjustEvent(token,false,false,-1);
        }

        m_pMainScene.RequestAfterCall(E_REQUEST_ID.tutorial_put,pJObject);
    }
    void Skip()
    {
        m_eTutorialStep = 45;
        GameContext.getCtx().SetTutorial(m_eTutorialStep);
        SendTutorialStep();
    }

    public void NextStep()
    {
        GameContext pGameContext = GameContext.getCtx();
        m_eTutorialStep = pGameContext.GetTutorial();
        SetTutorialStep(m_eTutorialStep);
        switch(m_eTutorialStep)
        {
            case 45:
            {
                m_pMainScene.ShowMainUI(true);
                m_pMainScene.ActiveMatch(true);
                m_pMainScene.ShowTopSubUI(true);
                m_pMainScene.SetupTimer();
                m_pMainScene.ResetMainButton();
                
                if(m_pMainScene.IsShowInstance<Management>())
                {
                    m_pMainScene.GetInstance<Management>().Close();
                }
                else if(m_pMainScene.IsShowInstance<TacticsFormation>())
                {
                    m_pMainScene.GetInstance<TacticsFormation>().Close();
                }
                else if(m_pMainScene.IsShowInstance<Transfer>())
                {
                    m_pMainScene.GetInstance<Transfer>().Close();
                }

                if(m_pMainScene.IsShowInstance<PlayerInfo>())
                {
                    m_pMainScene.GetInstance<PlayerInfo>().Close();
                }

                if(m_pMainScene.IsShowInstance<ClubLicense>())
                {
                    m_pMainScene.GetInstance<ClubLicense>().Close();
                }

                if(pGameContext.IsAttendReward())
                {
                    JObject pJObject = new JObject();
                    pJObject["type"] = 1;
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.attend_reward,pJObject,()=>{
                        if(MainUI.gameObject.activeSelf)
                        {
                            SingleFunc.HideAnimationDailog(MainUI);
                        }
                        else
                        {
                            LayoutManager.Instance.InteractableEnabledAll();
                        }
                    });
                }
                else
                {
                    if(MainUI.gameObject.activeSelf)
                    {
                        SingleFunc.HideAnimationDailog(MainUI);
                    }
                    else
                    {
                        LayoutManager.Instance.InteractableEnabledAll();
                    }
                }
            }
            break;
            case 4:
            case 16:
            case 12:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 23:
            case 26:
            case 33:
            case 34:
            case 35:
            case 36:
            case 37:
            case 40:
            case 41:
            case 42:
            {
                if(m_eTutorialStep == 12)
                {
                    m_pMainScene.GetInstance<Transfer>().SendTutorial();
                }
                else if(m_eTutorialStep == 33)
                {
                    m_pMainScene.TutorialMatch();
                }
                else if(m_eTutorialStep == 34)
                {
                    m_pMainScene.GetInstance<Match>().StartTutorial();
                }
                else if(m_eTutorialStep == 35 || m_eTutorialStep == 36)
                {
                    m_pMainScene.GetInstance<MatchView>().TutorailStep(m_eTutorialStep);
                }
                else if(m_eTutorialStep == 37)
                {
                    m_pMainScene.ShowMatchHelpPopup();
                }
                else if(m_eTutorialStep == 40)
                {
                    m_pMainScene.ShowConfirmPopup( pGameContext.GetLocalizingText("DIALOG_MATCHSKIP_TITLE"),string.Format(pGameContext.GetLocalizingText("DIALOG_MATCHSKIP_TXT"),pGameContext.GetItemCountByNO(GameContext.MATCH_SKIP_ID)),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false);
                }
                else if(m_eTutorialStep == 41)
                {
                    JObject data = new JObject();
                    data["match"] = pGameContext.GetCurrentMatchID();
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.ladder_skipMatch,data,m_pMainScene.GetInstance<MatchView>().FinishGame);
                }
                else if(m_eTutorialStep == 42)
                {
                    GameContext.getCtx().SendAdjustEvent("w3rfyk",false,false,-1); // TutorialStart
                }
                
                CheckTutorialStep();
            }
            break;
            case 5:
            case 6:
            case 7:
            {
                JObject pJObject = new JObject();
                pJObject["no"] = 1;
                pJObject["amount"] = m_eTutorialStep == 7 ? 10 : 1;
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.business_levelUp,pJObject,CheckTutorialStep);
            }
            break;
            case 9:
            {
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.tutorial_business,null,CheckTutorialStep);
            }
            break;
            case 11:
            {
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.tutorial_getRecruit,null,()=>{m_pMainScene.ShowTransfer(1); CheckTutorialStep();});
            }
            break;
            case 13:
            {
                PlayerT pPlayer = m_pMainScene.GetInstance<PlayerInfo>().GetPlayerData();
                long value = (long)SingleFunc.GetPlayerValue(pPlayer);
                JObject pJObject = new JObject();
                pJObject["id"] = pPlayer.Id;
                pJObject["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
                pJObject["totalValue"] = pGameContext.GetTotalPlayerValue(value);
                pJObject["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(pPlayer,true);
                pJObject["avgAge"] = pGameContext.GetPlayerAvgAge(pPlayer,true);
                pJObject["playerCount"] = pGameContext.GetTotalPlayerCount()+1;

                m_pMainScene.RequestAfterCall(E_REQUEST_ID.tutorial_recruit,pJObject,CheckTutorialStep);
            }
            break;

        }
    }
}
