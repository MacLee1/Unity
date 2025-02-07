using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;

using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.NETWORK;
using ALF.SOUND;
using DATA;
// using UnityEngine.EventSystems;
using STATEDATA;
using USERDATA;
using Newtonsoft.Json.Linq;

public class PositionTraining : IBaseUI
{
    const float HIDE_BUTTON_TIME = 5;
    MainScene m_pMainScene = null;

    TMPro.TMP_Text m_pNatiralPosition = null;
    TMPro.TMP_Text m_pSelectPosition = null;
    TMPro.TMP_Text m_pCost = null;
    TMPro.TMP_Text m_pTotalPoint = null;
    TMPro.TMP_Text m_pCurrentFarmiliarity = null;
    TMPro.TMP_Text m_pNextFarmiliarity = null;
    Button[] m_BoardPosition = new Button[(int)E_LOCATION.LOC_END];
    RectTransform m_Mark = null;
    
    E_LOCATION m_eSelectLocation = E_LOCATION.LOC_NONE;
    Transform m_pTrainingMax = null;
    Button m_pTrainingButton = null;
    Transform m_pMutiLevelupButton = null;
    int m_iAmount = 0;
    PlayerT m_pPlayerData = null;
    public RectTransform MainUI { get; private set;}
    public PositionTraining(){}
    
    public void Dispose()
    {
        int i =0;

        for(i =0; i < m_BoardPosition.Length; ++i)
        {
            m_BoardPosition[i] = null;
        }

        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pMutiLevelupButton);
        for(i =0; i < list.Count; ++i)
        {
            list[i].Exit(true);
        }

        m_BoardPosition = null;
        m_pMainScene = null;
        MainUI = null;
        m_Mark = null;
        m_pNatiralPosition = null;
        m_pSelectPosition = null;
        m_pCost = null;
        m_pCurrentFarmiliarity = null;
        m_pNextFarmiliarity = null;
        m_pTotalPoint = null;
        m_pTrainingMax = null;
        m_pPlayerData = null;
        m_pTrainingButton = null;
        m_pMutiLevelupButton = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "PositionTraining : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "PositionTraining : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        Transform tm = MainUI.Find("root/board");
        int n =0;
        int iTabIndex = 0;
        for(n =0; n < tm.childCount; ++n)
        {
            iTabIndex = (int)((E_LOCATION)Enum.Parse(typeof(E_LOCATION), tm.GetChild(n).gameObject.name));
            m_BoardPosition[iTabIndex] = tm.GetChild(n).GetComponent<Button>();
        }

        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("back").GetComponent<RectTransform>(),ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("root/tip").GetComponent<RectTransform>(),ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(tm.GetComponent<RectTransform>(),ButtonEventCall);
        
        m_pTotalPoint = MainUI.Find("root/trainingPoint/text").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(m_pTotalPoint.transform.parent.GetComponent<RectTransform>(),ButtonEventCall);

        m_pTrainingMax = MainUI.Find("root/trainingMax");
        m_pTrainingMax.gameObject.SetActive(false);
        m_pTrainingButton = MainUI.Find("root/training").GetComponent<Button>();
        LayoutManager.SetReciveUIButtonEvent(m_pTrainingButton.GetComponent<RectTransform>(),ButtonEventCall);
        m_pCost = m_pTrainingButton.transform.Find("training/text").GetComponent<TMPro.TMP_Text>();
        m_pMutiLevelupButton = MainUI.Find("root/mutiTraining");
        LayoutManager.SetReciveUIButtonEvent(m_pMutiLevelupButton.GetComponent<RectTransform>(),ButtonEventCall);
        m_pMutiLevelupButton.gameObject.SetActive(false);
        
        m_pNatiralPosition = MainUI.Find("root/natural/text").GetComponent<TMPro.TMP_Text>();
        m_pSelectPosition = MainUI.Find("root/select/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentFarmiliarity = MainUI.Find("root/farmiliarity/current").GetComponent<TMPro.TMP_Text>();
        m_pNextFarmiliarity = MainUI.Find("root/farmiliarity/next").GetComponent<TMPro.TMP_Text>();
        m_Mark = MainUI.Find("root/farmiliarity/mark").GetComponent<RectTransform>();

        MainUI.gameObject.SetActive(false);
    }

    void PlaytAbilityUpEffect()
    {
        CompleteEffectActor pEffectActor = ActorManager.Instance.GetActor<CompleteEffectActor>("GaugeEffect"); 
        pEffectActor.MainUI.localScale = Vector3.one *2;
        pEffectActor.MainUI.SetParent(m_pCurrentFarmiliarity.transform,false);
        pEffectActor.ChangeAnimation("eff");
    }

    void UpdatePositionButtonColor(int index, byte value)
    {
        Transform tm = null;
        if(m_BoardPosition[index] != null)
        {
            tm = m_BoardPosition[index].transform;
            tm.Find("c1").gameObject.SetActive(false);
            tm.Find("c2").gameObject.SetActive(false);
            tm.Find("c3").gameObject.SetActive(false);
            tm.Find("c4").gameObject.SetActive(false);
            if(value > 0)
            {
                if(value >= 90)
                {
                    tm.Find("c4").gameObject.SetActive(true);
                    return;
                }
                if(value >= 60)
                {
                    tm.Find("c3").gameObject.SetActive(true);
                    return;
                }
                if(value >= 30)
                {
                    tm.Find("c2").gameObject.SetActive(true);
                    return;
                }
                tm.Find("c1").gameObject.SetActive(true);
            }
        }
    }

    public void SetPlayerData(PlayerT pPlayer)
    {
        ALFUtils.Assert(pPlayer != null, "pPlayer = null!!!!");

        m_pPlayerData = pPlayer;
        m_eSelectLocation = GameContext.getCtx().ConvertPositionByIndex(m_pPlayerData.Position);
        m_pNatiralPosition.SetText(GameContext.getCtx().GetDisplayLocationName(m_pPlayerData.Position));
        m_pTotalPoint.SetText(ALFUtils.NumberToString(GameContext.getCtx().GetPositionSkill()));
        
        Transform tm = null;
        int i =0;
        int index =0;
        for(i =0; i < m_BoardPosition.Length; ++i)
        {
            if(m_BoardPosition[i] != null)
            {
                tm = m_BoardPosition[i].transform;
                tm.Find("select").gameObject.SetActive(false);
                
                index = GameContext.getCtx().ConvertPositionByTag((E_LOCATION)i);
                m_BoardPosition[i].enabled = m_pPlayerData.PositionFamiliars[index] > 0;
                m_BoardPosition[i].interactable = m_BoardPosition[i].enabled;

                UpdatePositionButtonColor(i,m_pPlayerData.PositionFamiliars[index]);
            }
        }
       
        SetUpgradePosition(GameContext.getCtx().ConvertPositionByIndex(m_pPlayerData.Position));
    }

    void SetUpgradePosition(E_LOCATION eLoc)
    {
        Transform tm = null;
        if(m_eSelectLocation != E_LOCATION.LOC_END)
        {
            if(m_BoardPosition[(int)m_eSelectLocation] != null)
            {
                tm = m_BoardPosition[(int)m_eSelectLocation].transform;
                tm.Find("select").gameObject.SetActive(false);
            }
        }
        
        m_eSelectLocation = eLoc;
        
        if(m_eSelectLocation != E_LOCATION.LOC_END)
        {
            RectTransform rTm = null;
            int index = GameContext.getCtx().ConvertPositionByTag(m_eSelectLocation);
            m_pSelectPosition.SetText(GameContext.getCtx().GetDisplayLocationName(index));

            if(m_BoardPosition[(int)m_eSelectLocation] != null )
            {
                tm = m_BoardPosition[(int)m_eSelectLocation].transform;
                tm.Find("select").gameObject.SetActive(true);

                m_pCurrentFarmiliarity.SetText($"{m_pPlayerData.PositionFamiliars[index]}%");
                m_pNextFarmiliarity.SetText($"{m_pPlayerData.PositionFamiliars[index]+1}%");
                UpdatePositionCost(m_pPlayerData.PositionFamiliars[index]);
                m_pTrainingMax.gameObject.SetActive(m_pPlayerData.PositionFamiliars[index] == 100);
                m_pTrainingButton.gameObject.SetActive(!m_pTrainingMax.gameObject.activeSelf);
                m_pCurrentFarmiliarity.gameObject.SetActive(m_pTrainingButton.gameObject.activeSelf);
                m_Mark.gameObject.SetActive(m_pTrainingButton.gameObject.activeSelf);

                if(m_pTrainingMax.gameObject.activeSelf)
                {    
                    m_pNextFarmiliarity.SetText(m_pCurrentFarmiliarity.text);
                    rTm = m_pNextFarmiliarity.GetComponent<RectTransform>();

                    rTm.offsetMin = new Vector2(0,rTm.offsetMin.y);
                    rTm.offsetMax = new Vector2(0,rTm.offsetMax.y);
                }
                else
                {
                    bool bActive = GameContext.getCtx().IsPositionUpgrade(m_pPlayerData.PositionFamiliars[index]);
                    m_pTrainingButton.transform.Find("on").gameObject.SetActive(bActive);
                    m_pTrainingButton.transform.Find("off").gameObject.SetActive(!bActive);

                    rTm = m_pNextFarmiliarity.GetComponent<RectTransform>();
                    
                    rTm.offsetMin = new Vector2(241,rTm.offsetMin.y);
                    rTm.offsetMax = new Vector2(18,rTm.offsetMax.y);
                }
            }
        }
    }

    void UpdatePositionCost(byte positionValue)
    {
        m_pTotalPoint.SetText(ALFUtils.NumberToString(GameContext.getCtx().GetPositionSkill()));
        m_pCost.SetText(ALFUtils.NumberToString(GameContext.getCtx().GetPositionUpgradeCost(positionValue)));
    }

    bool executeLevelUpButtonCallback(IState state,float dt,bool bEnd)
    {
        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
        if(condition.GetRemainTime() <= 1)
        {
            ALFUtils.FadeObject(m_pMutiLevelupButton,-dt);
        }
        
        return bEnd;
    }
    IState exitLevelUpButtonCallback(IState state)
    {
        ALFUtils.FadeObject(m_pMutiLevelupButton,1);
        m_pMutiLevelupButton.gameObject.SetActive(false);
        
        return null;
    }

    void UpdateMutiLevelUpInfo(int index)
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pMutiLevelupButton);
        
        if(!m_pTrainingMax.gameObject.activeSelf)
        {
            GameContext pGameContext = GameContext.getCtx();
            uint totalCost = 0;
            m_iAmount = pGameContext.GetPositionUpgradeCount(m_pPlayerData.PositionFamiliars[index],ref totalCost);

            if(m_iAmount > 1)
            {
                Animation pAnimation = m_pMutiLevelupButton.GetComponent<Animation>();
                pAnimation.Stop();
                m_pMutiLevelupButton.Find("eff").GetComponent<Graphic>().color = Color.white;
                m_pMutiLevelupButton.Find("eff").gameObject.SetActive(false);
                
                if(list.Count == 0)
                {
                    m_pMutiLevelupButton.gameObject.SetActive(true);        
                    BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pMutiLevelupButton),HIDE_BUTTON_TIME, (uint)E_STATE_TYPE.Timer, null, this.executeLevelUpButtonCallback,this.exitLevelUpButtonCallback);
                    StateMachine.GetStateMachine().AddState(pBaseState);
                }
                else
                {
                    TimeOutCondition condition = null;
                    for(int n = 0; n < list.Count; ++n)
                    {
                        condition = list[n].GetCondition<TimeOutCondition>();
                        ALFUtils.FadeObject(m_pMutiLevelupButton,1);
                        condition.Reset();
                        condition.SetRemainTime(HIDE_BUTTON_TIME);
                    }
                }
                
                TMPro.TMP_Text pText = m_pMutiLevelupButton.Find("cost").GetComponent<TMPro.TMP_Text>();
                pText.SetText(ALFUtils.NumberToString(totalCost));
                pText = m_pMutiLevelupButton.Find("lv").GetComponent<TMPro.TMP_Text>();
                pText.SetText( pGameContext.GetLocalizingText("MANAGEMENT_BTN_LEVEL_UP") + $" x{m_iAmount}");
               return; 
            }
        }

        for(int n = 0; n < list.Count; ++n)
        {
            list[n].Exit(true);
        }
        m_pMutiLevelupButton.gameObject.SetActive(false);
    }

    void ClearTimerState()
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pMutiLevelupButton);

        for(int n = 0; n < list.Count; ++n)
        {
            list[n].Exit(true);
        }
        m_pMutiLevelupButton.gameObject.SetActive(false);
    }

    public void Close()
    {
        ClearTimerState();
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(root.gameObject.name)
        {
            case "trainingPoint":
            {
                m_pMainScene.ShowItemTipPopup(sender);
            }
            break;
            case "tip":
            {
                m_pMainScene.ShowGameTip("game_tip_positiontraining_title");
            }
            break;
            case "board":
            {
                SetUpgradePosition((E_LOCATION)Enum.Parse(typeof(E_LOCATION), sender.name));
                ClearTimerState();
            }
            break;
            case "training":
            case "mutiTraining":
            {
                Animation pAnimation = sender.GetComponent<Animation>();
                pAnimation.Stop();
                sender.transform.Find("eff").GetComponent<Graphic>().color = Color.white;
                sender.transform.Find("eff").gameObject.SetActive(false);
                pAnimation.Play();
                JObject pJObject = new JObject();
                pJObject["player"] = m_pPlayerData.Id;
                pJObject["position"]=GameContext.getCtx().ConvertPositionByTag(m_eSelectLocation);
                pJObject["amount"] = sender.name == "mutiTraining" ? m_iAmount : 1;
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.player_positionFamiliarUp,pJObject,NetworkUpdate);
                m_iAmount = 0;
            }
            break;
            default:
            {
                Close();
            }
            break;

        }
        // SoundManager.Instance.PlaySFX("sfx_01");
    }

    void NetworkUpdate()
    {    
        int index = GameContext.getCtx().ConvertPositionByTag(m_eSelectLocation);
        UpdatePositionButtonColor((int)m_eSelectLocation,m_pPlayerData.PositionFamiliars[index]);
        SetUpgradePosition(m_eSelectLocation);
        UpdateMutiLevelUpInfo(index);
        PlaytAbilityUpEffect();
    }
}