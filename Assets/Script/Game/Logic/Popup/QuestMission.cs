using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;

using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
using TRAININGCOST;
// using UnityEngine.EventSystems;
using STATEDATA;
using Newtonsoft.Json.Linq;
using CONSTVALUE;
using EVENTCONDITION;
using QUESTMISSION;
using MILEAGEMISSION;

public class QuestMission : IBaseUI//, IBaseNetwork
{
    const string E_SCROLL_ITEM  = "MissionItem";
    
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    Animation m_pAnimation = null;
    
    // TMPro.TMP_Text  m_pResetTimer = null;
    Image m_pTotalGauge = null;
    TMPro.TMP_Text  m_pTotalGaugeCount = null;
    TMPro.TMP_Text m_pCurrentQuestName = null;
    TMPro.TMP_Text m_pCurrentQuestLabel= null;
    RawImage m_pCurrentQuestRewardIcon = null;
    TMPro.TMP_Text m_pCurrentQuestRewardCount = null;
    TMPro.TMP_Text m_pRefreshText = null;
    GameObject m_pComplete = null;
    bool m_bRefreshUpdate = false;
    float m_fTimeRefreshTime =0;

    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    public QuestMission(){}
    
    public void Dispose()
    {
        m_pComplete = null;
        
        m_pMainScene = null;
        MainUI = null;
        m_pRefreshText = null;
        m_pAnimation = null;
        ClearScroll();
        m_pScrollRect = null;
        
        m_pTotalGauge = null;
        m_pTotalGaugeCount = null;
        m_pCurrentQuestName = null;
        m_pCurrentQuestLabel= null;
        m_pCurrentQuestRewardIcon = null;
        m_pCurrentQuestRewardCount = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "QuestMission : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "QuestMission : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        
        m_pTotalGauge = MainUI.Find("root/quest/gauge/fill").GetComponent<Image>();
        m_pCurrentQuestName = MainUI.Find("root/quest/title").GetComponent<TMPro.TMP_Text>();
        m_pTotalGaugeCount = MainUI.Find("root/quest/gauge/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentQuestLabel = MainUI.Find("root/quest/tag/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentQuestRewardIcon = MainUI.Find("root/quest/reward/icon").GetComponent<RawImage>();
        m_pCurrentQuestRewardCount = MainUI.Find("root/quest/reward/count").GetComponent<TMPro.TMP_Text>();
        m_pComplete = MainUI.Find("root/quest/complete").gameObject;
        m_pRefreshText  = MainUI.Find("root/quest/timer/time").GetComponent<TMPro.TMP_Text>();
        m_pScrollRect = MainUI.Find("root/scroll").GetComponent<ScrollRect>();
        MainUI.gameObject.SetActive(false);
    }

    public void UpdateTimer(float dt)
    {
        if(!m_bRefreshUpdate) return;
        
        m_fTimeRefreshTime -= dt;    
        bool bUpdate = false;
        if(m_fTimeRefreshTime <= 0)
        {
            bUpdate = true;
            while(m_fTimeRefreshTime <= -1)
            {
                m_fTimeRefreshTime += 1;
            }
            
            m_fTimeRefreshTime = 1 - m_fTimeRefreshTime;
        }

        float tic = GameContext.getCtx().GetQuestMissionExpireTime();
        
        if(tic <= 0)
        {
            m_bRefreshUpdate = false; 
            m_fTimeRefreshTime = 0;
            m_pRefreshText.SetText("00:00");
        }
        else if(bUpdate)
        {
            m_pRefreshText.SetText(ALFUtils.SecondToString((int)tic,true,false,false));
        }
    }
    public void SetupQuestData()
    {
        GameContext pGameContext = GameContext.getCtx();
        float tic = pGameContext.GetQuestMissionExpireTime();
        // if(m_iCurrentQuestMissionID != pGameContext.GetCurrentQuestMissionID())    
        SetupScroll();
        
        m_bRefreshUpdate = true;
        m_fTimeRefreshTime = tic - (int)tic; 
        m_pRefreshText.SetText(ALFUtils.SecondToString((int)tic,true,false,false));

        UpdateQuest();
    }
    bool executeCallback(IState state,float dt,bool bEnd)
    {
        Animation target = state.GetTarget<BaseStateTarget>().GetMainTarget<Animation>();
        if(target != null)
        {
            if(!target.IsPlaying("reward_complete"))
            {
                UpdateQuest();
                m_pAnimation = null;
                return true;
            }
        }
        
        return bEnd;
    }

    void CompleteQuest()
    {
        if(m_pAnimation != null)
        {
            m_pAnimation.Play();
            StateMachine.GetStateMachine().AddState(BaseState.GetInstance(new BaseStateTarget(m_pAnimation),1, (uint)E_STATE_TYPE.ShowDailog, null, executeCallback, null));
        }
        else
        {
            UpdateQuest();
        }
    }

    void SetupScroll()
    {
        ClearScroll();
        GameContext pGameContext = GameContext.getCtx();
        
        QuestMissionList pQuestMissionList = pGameContext.GetFlatBufferData<QuestMissionList>(E_DATA_TYPE.QuestMission);
        QuestMissionItem? pQuestMissionItem = pQuestMissionList.QuestMissionByKey(1);
        
        QUESTMISSION.MissionItem? pMissionItem = null;
        MileageMissionList pMileageMissionList = pGameContext.GetFlatBufferData<MileageMissionList>(E_DATA_TYPE.MileageMission);
        MileageMissionItem? pMileageMissionItem = pMileageMissionList.MileageMissionByKey(pQuestMissionItem.Value.Mileage);
        EventConditionList pEventConditionList = pGameContext.GetFlatBufferData<EventConditionList>(E_DATA_TYPE.EventCondition);
        string[] names = pGameContext.GetLocalizingText(pMileageMissionItem.Value.Title).Split(';');
        m_pCurrentQuestName.SetText(names[1]);
        m_pCurrentQuestLabel.SetText(names[0]);
        
        MileageT pMileage = pGameContext.GetMileagesData(pQuestMissionItem.Value.Mileage);
        uint idValue = 0;
        long amount = 0;
        if(pMileage == null || pMileage.Level == 0 || pMileageMissionItem.Value.ListLength <= pMileage.Level)
        {
            idValue = pMileageMissionItem.Value.List(0).Value.Reward;
            amount = pMileageMissionItem.Value.List(0).Value.RewardAmount;
        }
        else
        {
            idValue = pMileageMissionItem.Value.List((int)pMileage.Level).Value.Reward;
            amount = pMileageMissionItem.Value.List((int)pMileage.Level).Value.RewardAmount;
        }

        if(idValue == GameContext.FREE_CASH_ID || idValue == GameContext.CASH_ID)
        {
            idValue = GameContext.CASH_ID;
            m_pCurrentQuestRewardCount.SetText(string.Format("{0:#,0}", amount));
        }
        else
        {
            m_pCurrentQuestRewardCount.SetText(ALFUtils.NumberToString(amount));
        }

        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",idValue.ToString());
        m_pCurrentQuestRewardIcon.texture = pSprite.texture;

        TMPro.TMP_Text text = null;
        RectTransform pItem = null;
        
        Vector2 size;
        float h = 0;
        
        RawImage icon = null;

        ConditionItem? pConditionItem = null;
        string token = null;
        List<QuestT> pList = pGameContext.GetQuestDataList();

        for(int i = 0; i < pList.Count; ++i)
        {
            pItem = LayoutManager.Instance.GetItem<RectTransform>(E_SCROLL_ITEM);
            if(pItem)
            {
                pMissionItem = pQuestMissionItem.Value.ListByKey(pList[i].Mission);
                pItem.gameObject.name = pList[i].Mission.ToString();
                token = "";
                
                pItem.SetParent(m_pScrollRect.content,false);
                
                text = pItem.Find("title").GetComponent<TMPro.TMP_Text>();
                
                pConditionItem = pEventConditionList.EventConditionByKey(pMissionItem.Value.Event).Value.ListByKey(pMissionItem.Value.EventCondition);
                
                if(pConditionItem != null)
                {
                    token = pGameContext.GetLocalizingText(pConditionItem.Value.ConditionText);
                }

                text.SetText(string.Format(pGameContext.GetLocalizingText(pMissionItem.Value.Title),token, ALFUtils.NumberToString(pMissionItem.Value.Objective)));
                text = pItem.Find("reward/count").GetComponent<TMPro.TMP_Text>();
                idValue = pMissionItem.Value.Reward;
                if(idValue == GameContext.FREE_CASH_ID || idValue == GameContext.CASH_ID)
                {
                    idValue = GameContext.CASH_ID;
                    text.SetText(string.Format("{0:#,0}", pMissionItem.Value.RewardAmount));
                }
                else
                {
                    text.SetText(ALFUtils.NumberToString(pMissionItem.Value.RewardAmount));
                }

                pSprite = AFPool.GetItem<Sprite>("Texture",idValue.ToString());

                icon = pItem.Find("reward/icon").GetComponent<RawImage>();
                icon.texture = pSprite.texture;

                pItem.localScale = Vector3.one;       
                pItem.anchoredPosition = new Vector2(0,-h);
                size = pItem.sizeDelta;
                h += size.y;
                size.x = 0;
                pItem.sizeDelta = size;
            }
        }

        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pScrollRect.content.sizeDelta = size;

        m_pScrollRect.content.anchoredPosition = Vector2.zero; 
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
    }

    void UpdateQuest()
    {
        GameContext pGameContext = GameContext.getCtx();
        QuestMissionList pQuestMissionList = pGameContext.GetFlatBufferData<QuestMissionList>(E_DATA_TYPE.QuestMission);
        QuestMissionItem? pQuestMissionItem = pQuestMissionList.QuestMissionByKey(GameContext.QUSET_MISSION_ID);
        MileageMissionList pMileageMissionList = pGameContext.GetFlatBufferData<MileageMissionList>(E_DATA_TYPE.MileageMission);
        MileageMissionItem? pMileageMissionItem = pMileageMissionList.MileageMissionByKey(pQuestMissionItem.Value.Mileage);
        MileageT pMileage = pGameContext.GetMileagesData(pQuestMissionItem.Value.Mileage);
        
        Button pButton = m_pCurrentQuestRewardIcon.transform.parent.GetComponent<Button>();
        uint objective = 0;
        uint amount = 0;
        int level = 0;
        if(pMileage != null)
        {
            level = (int)pMileage.Level;
            amount = pMileage.Amount;
        }

        if(level < pMileageMissionItem.Value.ListLength)
        {
            objective = pMileageMissionItem.Value.List(level).Value.Objective;
            pButton.enabled = amount >= objective;
            m_pComplete.SetActive(false);
            pButton.gameObject.SetActive(true);
        }
        else
        {
            objective = pMileageMissionItem.Value.List(pMileageMissionItem.Value.ListLength -1).Value.Objective;
            pButton.enabled = false;
            pButton.gameObject.SetActive(false);
            m_pComplete.SetActive(true);
        }

        pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);

        if(amount > objective)
        {
            amount=objective;
        }
        m_pTotalGauge.fillAmount = amount / (float)objective; 

        m_pTotalGaugeCount.SetText($"{amount}/{objective}");

        Transform pQuestItem = null;
        uint id = 0;
        List<Transform> completeList = new List<Transform>();
        List<Transform> activeList = new List<Transform>();
        List<Transform> runningList = new List<Transform>();

        for(int i = 0; i < m_pScrollRect.content.childCount; ++i)
        {
            pQuestItem = m_pScrollRect.content.GetChild(i);
            if(uint.TryParse(pQuestItem.gameObject.name,out id))
            {
                UpdateMissionItem(pQuestItem, id,ref completeList,ref activeList,ref runningList);
            } 
        }
        float y = 0;
        if(activeList.Count > 0)
        {
            SetPositionNode(activeList.OrderBy(x => int.Parse(x.gameObject.name)).ToList(),ref y);
        }
        if(runningList.Count > 0)
        {
            SetPositionNode(runningList.OrderBy(x => int.Parse(x.gameObject.name)).ToList(),ref y);
        }
        if(completeList.Count > 0)
        {
            SetPositionNode(completeList.OrderBy(x => int.Parse(x.gameObject.name)).ToList(),ref y);
        }
    }

    void SetPositionNode(List<Transform> list,ref float start)
    {
        RectTransform item = null;
        Vector2 pos;
        for(int i =0; i < list.Count; ++i)
        {
            item = list[i].GetComponent<RectTransform>();
            pos = item.anchoredPosition;
            pos.y = start;
            item.anchoredPosition = pos;
            start -= item.rect.height;
        }
    }
    void UpdateMissionItem(Transform pItem, uint iMissionId,ref List<Transform> completeList,ref List<Transform> activeList,ref List<Transform> runningList )
    {
        GameContext pGameContext = GameContext.getCtx();
        
        QuestMissionList pQuestMissionList = pGameContext.GetFlatBufferData<QuestMissionList>(E_DATA_TYPE.QuestMission);
        QuestMissionItem? pQuestMissionItem = pQuestMissionList.QuestMissionByKey(GameContext.QUSET_MISSION_ID);
        
        TMPro.TMP_Text text = null;
        Button pButton = null;
        int level = 0;
        ulong amount = 0;
        Image pGauge = null;

        QUESTMISSION.MissionItem? pMissionItem = null;
        QuestT pQuest = null;
        pMissionItem = pQuestMissionItem.Value.ListByKey(iMissionId);
        pQuest = pGameContext.GetQuestData(iMissionId);
        pGauge = pItem.Find("gauge/fill").GetComponent<Image>();
        text = pItem.Find("gauge/count").GetComponent<TMPro.TMP_Text>();
        if(pQuest != null)
        {
            level = (int)pQuest.Level;
            amount = pQuest.Amount;
        }
        
        pButton = pItem.Find("reward/get").GetComponent<Button>();
        if( level ==1)
        {
            completeList.Add(pItem);
            pItem.Find("complete").gameObject.SetActive(true);
            pGauge.fillAmount = 1;
            pButton.transform.parent.gameObject.SetActive(false);
            pGauge.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            pGauge.transform.parent.gameObject.SetActive(true);
            pButton.transform.parent.gameObject.SetActive(true);
            pItem.Find("complete").gameObject.SetActive(false);
            pGauge.fillAmount = Mathf.Min(amount / (float)pMissionItem.Value.Objective, 1.0f);
            pButton.enabled = pGauge.fillAmount >= 1;
            pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
            pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
            if(pButton.enabled)
            {
                activeList.Add(pItem);
            }
            else
            {
                runningList.Add(pItem);
            }
        }
        
        if(amount > pMissionItem.Value.Objective)
        {
            amount=pMissionItem.Value.Objective;
        }

        text.SetText($"({amount}/{pMissionItem.Value.Objective})");
    }

    void ClearScroll()
    {
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        int i = m_pScrollRect.content.childCount;

        RawImage icon = null;
        Transform tm = null;
        RectTransform item = null;
        Button[] list = null;
        while(i > 0)
        {
            --i;
            item = m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>();
            list = item.GetComponentsInChildren<Button>(true);
            for(int n =0; n < list.Length; ++n)
            {
                list[n].enabled = true;
            }

            icon = item.Find("reward/icon").GetComponent<RawImage>();
            icon.texture = null;
            tm = item.Find("content");
            LayoutManager.Instance.AddItem(E_SCROLL_ITEM,item);
        }

        m_pScrollRect.content.anchoredPosition = Vector2.zero;        
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }
    public void Close()
    {
        if(m_pAnimation != null)
        {
            List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pAnimation);
            for(int n =0; n < list.Count; ++n)
            {
                list[n].Exit(true);
            }
            m_pAnimation = null;
        }

        SingleFunc.HideAnimationDailog(MainUI);
    }
    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        uint id = 0;
        if(uint.TryParse(tm.gameObject.name,out id))
        {
            JObject pJObject = new JObject();
            pJObject["no"] = GameContext.QUSET_MISSION_ID;
            pJObject["mission"]=id;
                m_pAnimation = sender.transform.parent.parent.GetComponent<Animation>();
                sender.GetComponent<Button>().enabled = false;
            m_pMainScene.RequestAfterCall(E_REQUEST_ID.quest_reward,pJObject,CompleteQuest);
        }
    }

    void GetMileageReward(MileageT pMileage,System.Action endCallback)
    {
        if(pMileage != null)
        {
            JObject pJObject = new JObject();
            pJObject["no"] = pMileage.No;
            pJObject["mission"]=pMileage.Level+1;
            m_pMainScene.RequestAfterCall(E_REQUEST_ID.mileage_reward,pJObject,endCallback);
        }
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "reward" )
        {
            GameContext pGameContext = GameContext.getCtx();
            QuestMissionList pQuestMissionList = pGameContext.GetFlatBufferData<QuestMissionList>(E_DATA_TYPE.QuestMission);
            QuestMissionItem? pQuestMissionItem = pQuestMissionList.QuestMissionByKey(GameContext.QUSET_MISSION_ID);
            GetMileageReward(pGameContext.GetMileagesData(pQuestMissionItem.Value.Mileage),Close);

            return;
        }
        else if(sender.name == "tip")
        {
            m_pMainScene.ShowGameTip("game_tip_license_title");
            return;
        }
        
        Close();
    }
}
