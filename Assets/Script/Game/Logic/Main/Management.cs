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
using ALF.SOUND;
using DATA;
using BUSINESS;
using Newtonsoft.Json.Linq;
using TEXT;
// using UnityEngine.EventSystems;
using ALF.NETWORK;
using STATEDATA;

public class Management : IBaseUI, IBaseNetwork
{
    const float HIDE_BUTTON_TIME = 5;
    const string SCROLL_ITEM_NAME = "ManagementItem";
    enum E_TAB : byte { business = 0,training,MAX}

    class ManagementItem : IBase
    {
        public uint ID {get; private set;} 
        public RectTransform Target  {get; private set;}
        public ScrollRect Parent  {get; private set;}
        public RawImage Icon  {get; private set;}
        public RawImage Reward {get; private set;}
        public TMPro.TMP_Text RewardCount {get; private set;}
        public TMPro.TMP_Text Time {get; private set;}
        public TMPro.TMP_Text Level {get; private set;}
        public TMPro.TMP_Text Name {get; private set;}
        public Button LevelUp {get; private set;}
        public Button MutiUp {get; private set;}
        public Animation MutiUpAnimation {get; private set;}
        public Animation LevelUpAnimation {get; private set;}
        public Animation IconAnimation {get; private set;}

        public Transform Lock {get; private set;}

        public TMPro.TMP_Text LevelUpComment {get; private set;}
        public TMPro.TMP_Text LevelUpCost {get; private set;}
        public TMPro.TMP_Text LevelUpAddValue {get; private set;}
        public Graphic MutiEff {get; private set;}
        public Graphic LevelUpEff {get; private set;}

        public TMPro.TMP_Text MutiUpComment {get; private set;}
        public TMPro.TMP_Text MutiUpCost {get; private set;}
        public TMPro.TMP_Text MutiUpAddValue {get; private set;}

        
        public int Amount {get; set;}

        GameObject m_pLevelUpOn = null;
        GameObject m_pLevelUpOff = null;
        GameObject m_pMax= null;
        GameObject m_pOpen = null;

        bool m_bBusiness = false;
        int m_iLevel = -1;
        public int GetCurrentLevel()
        {
            return m_iLevel;
        }
        public bool IsBusiness()
        {
            return m_bBusiness;
        }

        public void Play(bool bMuti)
        {
            if(bMuti)
            {
                MutiUpAnimation.Stop();
                MutiEff.color = Color.white;
                MutiEff.gameObject.SetActive(false);
                MutiUpAnimation.Play();
            }
            else
            {
                LevelUpAnimation.Stop();
                LevelUpEff.color = Color.white;
                LevelUpEff.gameObject.SetActive(false);
                LevelUpAnimation.Play();
            }
        }

        public ManagementItem( BusinessItemList? pBusinessItemList,RectTransform taget, ScrollRect parent,Material pMaterial)
        {
            GameContext pGameContext = GameContext.getCtx();

            ID = pBusinessItemList.Value.No;
            Target = taget;
            Parent = parent;

            Target.gameObject.name = ID.ToString();
            Name = Target.Find("name").GetComponent<TMPro.TMP_Text>();
            Name.SetText(pGameContext.GetLocalizingText(pBusinessItemList.Value.Name));
            
            Sprite pSprite = AFPool.GetItem<Sprite>("Texture",pBusinessItemList.Value.Icon);
            Icon = Target.Find("icon").GetComponent<RawImage>();
            IconAnimation = Icon.GetComponent<Animation>();
            Icon.transform.Find("eff").gameObject.SetActive(false);
            
            Icon.material = new Material(pMaterial);
            Icon.texture = pSprite.texture;
            m_bBusiness = pBusinessItemList.Value.No < 1000;
            Level = Target.Find("level/text").GetComponent<TMPro.TMP_Text>(); 
            Time = Target.Find("time/text").GetComponent<TMPro.TMP_Text>();
            Time.transform.parent.gameObject.SetActive(m_bBusiness);
            MutiUp = Target.Find("muti_up").GetComponent<Button>();
            MutiUpAnimation = MutiUp.GetComponent<Animation>();
            MutiEff = MutiUp.transform.Find("eff").GetComponent<Graphic>();

            MutiUpCost = MutiUp.transform.Find("top/cost").GetComponent<TMPro.TMP_Text>();
            MutiUpComment = MutiUp.transform.Find("text").GetComponent<TMPro.TMP_Text>();
            MutiUpAddValue = MutiUp.transform.Find("pay").GetComponent<TMPro.TMP_Text>();
        
            Lock = Target.Find("lock");

            LevelUp = Target.Find("btn_up").GetComponent<Button>();
            LevelUpEff = LevelUp.transform.Find("eff").GetComponent<Graphic>();
            LevelUpAnimation = LevelUp.GetComponent<Animation>();

            m_pLevelUpOn = LevelUp.transform.Find("on").gameObject;
            m_pLevelUpOff = LevelUp.transform.Find("off").gameObject;
            m_pOpen = LevelUp.transform.Find("open").gameObject;

            LevelUpComment = LevelUp.transform.Find("text").GetComponent<TMPro.TMP_Text>();
            LevelUpCost = LevelUp.transform.Find("top/cost").GetComponent<TMPro.TMP_Text>();
            LevelUpAddValue = LevelUp.transform.Find("pay").GetComponent<TMPro.TMP_Text>();

            m_iLevel = (int)(m_bBusiness ? pGameContext.GetCurrentBusinessLevelByID(pBusinessItemList.Value.No) : pGameContext.GetCurrentTrainingLevelByID(pBusinessItemList.Value.No));
            
            BusinessItem? pBusinessItem = pBusinessItemList.Value.List(m_iLevel);
            Reward = Target.Find("reward/icon").GetComponent<RawImage>();
            RewardCount = Target.Find("reward/text").GetComponent<TMPro.TMP_Text>();
            
            if(pBusinessItem != null)
            {
                pSprite = AFPool.GetItem<Sprite>("Texture",pBusinessItem.Value.Reward.ToString());
                Reward.texture = pSprite.texture;
            }

            TMPro.TMP_Text pMax = Target.Find("max/text").GetComponent<TMPro.TMP_Text>();
            pMax.SetText(pGameContext.GetLocalizingText("MANAGEMENT_BTN_LEVEL_MAX"));
            m_pMax = pMax.transform.parent.gameObject;
            MutiUp.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            IconAnimation.Stop();
            MutiUpAnimation.Stop();
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,Target);
            
            Icon.texture = null;
            Icon.material = null;
            Reward.texture = null;
            
            Target = null;
            Parent = null;
            Icon = null;
            Reward = null;
            RewardCount = null;
            Time = null;
            Level = null;
            Name = null;
            LevelUp = null;
            MutiUp = null;
            MutiUpAnimation = null;
            LevelUpAnimation = null;
            IconAnimation = null;
            Lock = null;
            LevelUpComment = null;
            LevelUpCost = null;
            LevelUpAddValue = null;
            MutiEff = null;
            MutiUpComment = null;
            MutiUpCost = null;
            MutiUpAddValue = null;
            m_pLevelUpOn = null;
            m_pLevelUpOff = null;
            m_pMax= null;
            m_pOpen = null;
        }

        public void SetupLevelUp(bool bActive)
        {
            LevelUp.enabled = bActive;
            m_pLevelUpOff.SetActive(!bActive);
            m_pLevelUpOn.SetActive(bActive);
        }

        public void UpdateItem(int level,bool bMuti)
        {
            GameContext pGameContext = GameContext.getCtx();
            BusinessList pBusinessList = pGameContext.GetBusinessData();
            BusinessItemList? pBusinessItemList = null;
            
            bool bLevelMax = false;
            
            m_iLevel = level;
            Lock.gameObject.SetActive(m_iLevel == 0);
            uint money = 0;
            pBusinessItemList = pBusinessList.BusinessByKey(ID);            

            BusinessItem? pBusinessItem = pBusinessItemList.Value.List(m_iLevel);
            if(pBusinessItem != null)
            {
                money = pBusinessItem.Value.RewardAmount;
                RewardCount.SetText(money > 0 ? ALFUtils.NumberToString(money):"-");
                Time.SetText(pBusinessItem.Value.IncomeInterval > 0 ? ALFUtils.TimeToString((int)pBusinessItem.Value.IncomeInterval,false,true,pBusinessItem.Value.IncomeInterval % 3600 != 0, pBusinessItem.Value.IncomeInterval % 60 != 0,true):"-");
                LevelUpCost.SetText(ALFUtils.NumberToString(pBusinessItem.Value.CostAmount));
                if(m_bBusiness)
                {
                    money = (uint)(((float)pBusinessItem.Value.RewardAmount / (float)pBusinessItem.Value.IncomeInterval) * 60);
                }
                
                if(pBusinessItemList.Value.ListLength > m_iLevel+1)
                {
                    LevelUpCost.transform.parent.gameObject.SetActive(true);
                    pBusinessItem = pBusinessItemList.Value.List(m_iLevel+1);
                    if(pBusinessItem != null)
                    {
                        if(m_bBusiness)
                        {
                            money = (uint)(((float)pBusinessItem.Value.RewardAmount / (float)pBusinessItem.Value.IncomeInterval) * 60) - money;
                            LevelUpAddValue.SetText($"+{ALFUtils.NumberToString(money)}/m");
                        }
                        else
                        {
                            money = pBusinessItem.Value.RewardAmount - money;
                            LevelUpAddValue.SetText($"+{ALFUtils.NumberToString(money)}");
                        }
                    }
                }
                else
                {
                    bLevelMax = true;
                    LevelUpCost.transform.parent.gameObject.SetActive(false);
                }
            }

            LevelUp.enabled = bLevelMax ? false : pGameContext.IsBusinessLevelUpByID(ID,m_bBusiness);

            LevelUpComment.color = LevelUp.enabled ? GameContext.GREEN : GameContext.GRAY;
            LevelUpAddValue.color = LevelUpComment.color;
            
            m_pLevelUpOn.SetActive(LevelUp.enabled);
            m_pLevelUpOff.SetActive(!LevelUp.enabled);

            Level.SetText( m_iLevel > 0 ? $"Lv {m_iLevel}" : "-");
            m_pMax.SetActive(bLevelMax);

            if(m_iLevel > 0 )
            {
                LevelUpAddValue.gameObject.SetActive(true);
                LevelUpComment.SetText(pGameContext.GetLocalizingText("MANAGEMENT_BTN_LEVEL_UP"));
                LevelUp.gameObject.SetActive(!bLevelMax);
                Icon.color = Color.white;
                m_pOpen.SetActive(false);
                Icon.material.SetFloat("_Saturation",1);
            }
            else
            {
                LevelUpComment.color = LevelUp.enabled ? GameContext.BULE : GameContext.GRAY;
                LevelUpAddValue.color = LevelUpComment.color;

                LevelUpComment.SetText(pGameContext.GetLocalizingText("MANAGEMENT_BTN_OPEN"));
                LevelUpAddValue.gameObject.SetActive(false);
                Icon.material.SetFloat("_Saturation",0);
                m_pOpen.SetActive(true);
                m_pLevelUpOn.SetActive(false);
            }

            if( bMuti)
            {
                UpdateMutiLevelUpInfo();
            }
        }

        public void UpdateMutiLevelUpInfo()
        {
            GameContext pGameContext = GameContext.getCtx();

            ulong totalCost = 0;
            Amount = 0;
            Amount = pGameContext.GetBusinessLevelUpCountByID(ID,m_bBusiness,ref totalCost);
            
            if(Amount > 1)
            {
                MutiUpAnimation.Stop();
                MutiEff.color = Color.white;
                MutiEff.gameObject.SetActive(false);
                
                BusinessList pBusinessList = pGameContext.GetBusinessData();
                BusinessItemList? pBusinessItemList = pBusinessList.BusinessByKey(ID);
                BusinessItem? pBusinessItem = pBusinessItemList.Value.List((int)m_iLevel + Amount);
                if(pBusinessItem != null)
                {
                    uint money = 0;
                    if(m_bBusiness)
                    {
                        money = (uint)(((float)pBusinessItem.Value.RewardAmount / (float)pBusinessItem.Value.IncomeInterval) * 60) - money;
                        MutiUpAddValue.SetText($"+{ALFUtils.NumberToString(money)}/m");
                    }
                    else
                    {
                        money = pBusinessItem.Value.RewardAmount - money;
                        MutiUpAddValue.SetText($"+{ALFUtils.NumberToString(money)}");
                    }

                    MutiUpComment.SetText( pGameContext.GetLocalizingText("MANAGEMENT_BTN_LEVEL_UP") + $" x{Amount}");
                }
                MutiUpCost.SetText(ALFUtils.NumberToString(totalCost));
            }
            else
            {
                MutiUp.gameObject.SetActive(false);
            }
        }
    }

    List<ManagementItem> m_pBusinessList = new List<ManagementItem>();
    List<ManagementItem> m_pTrainingList = new List<ManagementItem>();
    Material m_pMaterial = null;
    E_TAB m_eCurrentTab = E_TAB.business;
    MainScene m_pMainScene = null;
    ScrollRect[] m_pScrollRects = new ScrollRect[(int)E_TAB.MAX];
    Transform[] m_pTabUIList = new Transform[(int)E_TAB.MAX];
    Transform[] m_pTabButtonList = new Transform[(int)E_TAB.MAX];
    TMPro.TMP_Text[] m_pTrainingPoint = new TMPro.TMP_Text[(int)E_TRAINING_TYPE.MAX];
    TMPro.TMP_Text m_pTrainingTime = null;
    TMPro.TMP_Text m_pBusinessTotalTimeValue = null;

    List<Transform> m_pMutiLevelupButtonList = new List<Transform>();
    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ 
        for(int i =0; i < m_pScrollRects.Length; ++i)
            m_pScrollRects[i].enabled = value;
        }}

    public Management(){}

    public void Dispose()
    {
        m_pMaterial = null;
        m_pMainScene = null;
        MainUI = null;

        int i =0;
        
        for(i =0; i < m_pScrollRects.Length; ++i)
        {
            ClearScroll(i);
            m_pScrollRects[i] = null;
            m_pTabButtonList[i] = null;
            m_pTabUIList[i] = null;
        }

        for(i =0; i < m_pTrainingPoint.Length; ++i)
        {
            m_pTrainingPoint[i] = null;
        }
        
        m_pTrainingList = null;
        m_pBusinessList = null;
        m_pScrollRects = null;
        m_pTrainingPoint = null;
        m_pTabButtonList = null;
        m_pTabUIList = null;
        m_pTrainingTime = null;
        m_pBusinessTotalTimeValue = null;
        m_pMutiLevelupButtonList = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Management : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Management : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        float w = 0;
        float wh = 0;
        
        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        float ax = 0;
        int iTabIndex = -1;
        RectTransform ui = MainUI.Find("root/tabs").GetComponent<RectTransform>();
        w = (ui.rect.width / ui.childCount);
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        int n =0;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;
            m_pTabUIList[iTabIndex] = MainUI.Find($"root/sub/{item.gameObject.name}");
            m_pTabUIList[iTabIndex].gameObject.SetActive(false);
            m_pScrollRects[iTabIndex] = MainUI.Find($"root/{item.gameObject.name}").GetComponent<ScrollRect>();
            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }
        MainUI.gameObject.SetActive(false);

        TMPro.TMP_Text[] list = m_pTabUIList[(int)E_TAB.training].Find("training").GetComponentsInChildren<TMPro.TMP_Text>(true);
        for(n =0; n < list.Length; ++n)
        {
            iTabIndex = (int)((E_TRAINING_TYPE)Enum.Parse(typeof(E_TRAINING_TYPE), list[n].transform.parent.gameObject.name));
            m_pTrainingPoint[iTabIndex] = list[n];
        }

        m_pBusinessTotalTimeValue = m_pTabUIList[(int)E_TAB.business].Find("time").GetComponent<TMPro.TMP_Text>();
        m_pTrainingTime = m_pTabUIList[(int)E_TAB.training].Find("time").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        SetupScroll(E_TAB.business);
        SetupScroll(E_TAB.training);
    }

    void SetupScroll(E_TAB eTab)
    {
        ClearScroll((int)eTab);

        ScrollRect pScrollRect = m_pScrollRects[(int)eTab];
        RectTransform pItem = null;
        Vector2 size;
        float h = 0;
        bool bBusiness = eTab == E_TAB.business;
        
        int i =0;
        
        GameContext pGameContext = GameContext.getCtx();
        BusinessList pBusinessList = pGameContext.GetBusinessData();
        BusinessItemList? pBusinessItemList = null;
        ManagementItem pManagementItem = null;
        for(i =0; i < pBusinessList.BusinessLength; ++i)
        {
            pBusinessItemList = pBusinessList.Business(i);
            if(bBusiness)
            {
                if(pBusinessItemList.Value.No >= 1000) continue;
            }
            else
            {
                if(pBusinessItemList.Value.No < 1000) continue;
            }

            pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
                
            if(pItem)
            {
                if(m_pMaterial == null)
                {
                    m_pMaterial = pItem.Find("icon").GetComponent<RawImage>().material;
                }
                pManagementItem = new ManagementItem(pBusinessItemList,pItem,pScrollRect,m_pMaterial);
                if(bBusiness)
                {
                    m_pBusinessList.Add(pManagementItem);
                }
                else
                {
                    m_pTrainingList.Add(pManagementItem);
                }
                
                pManagementItem.UpdateItem(pManagementItem.GetCurrentLevel(),false);
                pItem.SetParent(pScrollRect.content,false);
                pItem.localScale = Vector3.one;       
                pItem.anchoredPosition = new Vector2(0,-h);
                size = pItem.sizeDelta;
                size.x = 0;
                pItem.sizeDelta = size;
                h += pItem.rect.height;
            }
        }

        size = pScrollRect.content.sizeDelta;
        size.y = h;
        pScrollRect.content.sizeDelta = size;
        LayoutManager.SetReciveUIScollViewEvent(pScrollRect,ScrollViewItemButtonEventCall);
        pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    void ClearScroll(int eTab)
    {
        ScrollRect pScrollRect = m_pScrollRects[eTab];
        LayoutManager.SetReciveUIScollViewEvent(pScrollRect,null);
        int i =  0;
        if(eTab == 0)
        {
            i = m_pBusinessList.Count;
            while(i > 0)
            {
                --i;
                m_pBusinessList[i].Dispose();
            }
            m_pBusinessList.Clear();
        }
        else
        {
            i = m_pTrainingList.Count;
            while(i > 0)
            {
                --i;
                m_pTrainingList[i].Dispose();
            }
            m_pTrainingList.Clear();
        }

        Vector2 size = pScrollRect.content.sizeDelta;
        size.y =0;
        pScrollRect.content.sizeDelta = size;
        pScrollRect.content.anchoredPosition = Vector2.zero;

        ClearState();
    }

    public void UpdateTrainingTimer(float time)
    {
        if(m_pTrainingTime.gameObject.activeInHierarchy)
        {
            int iTime = (int)time;
            if(iTime <= 0)
            {
                m_pTrainingTime.SetText("--");
            }
            else
            {
                m_pTrainingTime.SetText(ALFUtils.TimeToString(iTime,false,true,true,true,true));
            }
        }
    }

    bool executeLevelUpButtonCallback(IState state,float dt,bool bEnd)
    {
        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
        BaseStateTarget target = state.GetTarget<BaseStateTarget>();
        Transform tm = target.GetMainTarget<Transform>();
        if(condition.GetRemainTime() <= 1)
        {
            ALFUtils.FadeObject(tm,-dt);
        }
        
        return bEnd;
    }
    IState exitLevelUpButtonCallback(IState state)
    {
        BaseStateTarget target = state.GetTarget<BaseStateTarget>();
        Transform tm = target.GetMainTarget<Transform>();
        ALFUtils.FadeObject(tm,1);
        tm.gameObject.SetActive(false);
        for(int i = 0; i < m_pMutiLevelupButtonList.Count; ++i)
        {
            if(m_pMutiLevelupButtonList[i] == tm)
            {
                m_pMutiLevelupButtonList.RemoveAt(i);
            }
        }
        return null;
    }

    void UpdateScroll(ManagementItem pManagementItem,bool bMuti,bool bAlll = false)
    {
        GameContext pGameContext = GameContext.getCtx();
        uint iLevel = pManagementItem.IsBusiness() ? pGameContext.GetCurrentBusinessLevelByID(pManagementItem.ID) : pGameContext.GetCurrentTrainingLevelByID(pManagementItem.ID);
        pManagementItem.UpdateItem((int)iLevel,bMuti);
        
        if( bMuti)
        {
            UpdateMutiLevelUpInfo(pManagementItem);
        }
        
        if(bAlll)
        {
            BusinessList pBusinessList = pGameContext.GetBusinessData();
            List<ManagementItem> list = null;
            bool bBusiness = pManagementItem.IsBusiness();
            if(bBusiness)
            {
                list = m_pBusinessList;
            }
            else
            {
                list = m_pTrainingList;
            }
            
            for(int i =0; i < list.Count; ++i)
            {
                if(list[i] != pManagementItem)
                {
                    list[i].SetupLevelUp(pGameContext.IsBusinessLevelUpByID(list[i].ID,bBusiness));
                    
                    if(bMuti)
                    {
                        if(StateMachine.GetStateMachine().IsCurrentTargetStates(list[i].MutiUp.transform))
                        {                
                            UpdateMutiLevelUpInfo(list[i]);
                        }
                    }
                }
            }
        }
    }

    public RectTransform GetBusinessItemUI(int index,string item)
    {
        return m_pScrollRects[0].content.GetChild(index).Find(item).GetComponent<RectTransform>();
    }

    void ClearState()
    {
        int i = m_pMutiLevelupButtonList.Count;
        List<BaseState> list = null;
        while(i > 0)
        {
            --i;
            list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pMutiLevelupButtonList[i]);
            for(int n = 0; n < list.Count; ++n)
            {
                list[n].Exit(true);
            }
        }
        m_pMutiLevelupButtonList.Clear();
    }

    public void ShowTabUI(byte eTab)
    {
        ClearState();
        int i = 0;
        for(i = 0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i].gameObject.SetActive(eTab == i);
            m_pTabButtonList[i].Find("on").gameObject.SetActive(eTab == i);
            m_pScrollRects[i].gameObject.SetActive(eTab == i);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = eTab == i ? Color.white : GameContext.GRAY;
        }
        m_eCurrentTab = (E_TAB)eTab;
        UpdateTabUI();

        List<ManagementItem> list = null;
        if(m_eCurrentTab == E_TAB.business)
        {
            list = m_pBusinessList;
        }
        else
        {
            list = m_pTrainingList;
        }

        for( i =0; i < list.Count; ++i)
        {
            UpdateScroll(list[i],false,false);
        }
    }

    void UpdateTrainingPointUI()
    {
        GameContext pGameContext = GameContext.getCtx();
        for(int i =0; i < m_pTrainingPoint.Length; ++i)
        {
            m_pTrainingPoint[i].SetText(ALFUtils.NumberToString(pGameContext.GetTrainingPointByType((E_TRAINING_TYPE)i)));
        }
    }

    void UpdateTabUI()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(m_eCurrentTab == E_TAB.training)
        {
            UpdateTrainingPointUI();
        }
        else
        {
            m_pBusinessTotalTimeValue.SetText(string.Format(pGameContext.GetLocalizingText("MANAGEMENT_TXT_MONEY_PER_MINUTE"),ALFUtils.NumberToString(pGameContext.GetBusinessTotalRewardForTime())));
        }
    }

    void UpdateMutiLevelUpInfo(ManagementItem pManagementItem)
    {
        pManagementItem.UpdateMutiLevelUpInfo();
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(pManagementItem.MutiUp.transform);
        
        if(pManagementItem.Amount > 1)
        {
            if(list.Count == 0)
            {
                m_pMutiLevelupButtonList.Add(pManagementItem.MutiUp.transform);
                pManagementItem.MutiUp.transform.gameObject.SetActive(true);
                
                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pManagementItem.MutiUp.transform),HIDE_BUTTON_TIME, (uint)E_STATE_TYPE.Timer, null, this.executeLevelUpButtonCallback,this.exitLevelUpButtonCallback);
                StateMachine.GetStateMachine().AddState(pBaseState);
            }
            else
            {
                TimeOutCondition condition = null;
                for(int n = 0; n < list.Count; ++n)
                {
                    list[n].Paused = false;
                    condition = list[n].GetCondition<TimeOutCondition>();
                    ALFUtils.FadeObject(pManagementItem.MutiUp.transform,1);
                    condition.Reset();
                    condition.SetRemainTime(HIDE_BUTTON_TIME);
                }
            }
        }
        else
        {
            for(int n = 0; n < list.Count; ++n)
            {
                list[n].Exit(true);
            }
        }
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        uint ID = uint.Parse(tm.gameObject.name);
        GameContext pGameContext = GameContext.getCtx();
        bool bBusiness = m_eCurrentTab == E_TAB.business;

        if(pGameContext.IsBusinessLevelUpByID(ID,bBusiness))
        {
            List<ManagementItem> list = bBusiness ? m_pBusinessList: m_pTrainingList;

            for(int i =0; i < list.Count; ++i)
            {
                if(list[i].ID == ID && list[i].Target.transform == tm)
                {
                    list[i].Play(sender.name == "muti_up");
                    list[i].IconAnimation.Stop();
                    uint iAmount = sender.name == "muti_up" ? (uint)list[i].Amount : 1;
                    
                    ulong timeValue = pGameContext.GetBusinessNextTotalRewardForTime(iAmount,ID);
                    JObject pJObject = new JObject();
                    pJObject["no"] = ID;
                    pJObject["businessOutput"]=timeValue;
                    pJObject["amount"] = iAmount;
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.business_levelUp,pJObject);
                    list[i].Amount = 0;
                    return;
                }
            }
        }
    }

    public void Close()
    {
        m_pMainScene.HideMoveDilog(MainUI,Vector3.right);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(sender.name)
        {
            case "back":
            {
                Close();
                m_pMainScene.ResetMainButton();
            }
            break;
            case "business":
            {
                ShowTabUI((byte)E_TAB.business);
            }
            break;
            case "training":
            {
                ShowTabUI((byte)E_TAB.training);
            }
            break;
            case "tip":
            {
                m_pMainScene.ShowGameTip("game_tip_management_title");
            }
            break;
            default:
            {
                m_pMainScene.ShowItemTipPopup(sender);
            }
            break;
        }
    }

    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
         if(data == null) return;

        E_REQUEST_ID eID = (E_REQUEST_ID)data.Id;

        if(eID == E_REQUEST_ID.business_rewardTraining)
        {
            UpdateTrainingPointUI();
            return;
        }

        if(!MainUI.gameObject.activeInHierarchy) return;
 
        if(eID == E_REQUEST_ID.business_levelUp)
        {
            if(data.Json.ContainsKey("businesses"))
            {
                JArray pArray = (JArray)data.Json["businesses"];
                JObject business = null;
                GameContext pGameContext = GameContext.getCtx();
                uint id = 0;
                string token = null;
                BusinessList pBusinessList = pGameContext.GetBusinessData();
                BusinessItemList? pBusinessItemList = null;
                uint iLevel = 0;
                for(int i = 0; i < pArray.Count; ++i)
                {
                    business = (JObject)pArray[i];
                    id = (uint)business["no"];
                    if((uint)business["level"] == 1)
                    {
                        switch(id)
                        {
                            case 2: token = "63zozz"; break;
                            case 3: token = "33gc5k"; break;
                            case 4: token = "tsihbc"; break;
                            case 5: token = "ehalps"; break;
                            case 6: token = "89ie7s"; break;
                            case 7: token = "20za5t"; break;
                            case 8: token = "xzpzoy"; break;
                            case 9: token = "o4a8w3"; break;
                            case 10: token = "8l6q5n"; break;
                            case 11: token = "gwnukm"; break;
                        }
                        if(!string.IsNullOrEmpty(token))
                        {
                            pGameContext.SendAdjustEvent(token,true,true,-1);
                        }
                        m_pMainScene.FocusBuildingObject(id);

                        Close();
                        m_pMainScene.ResetMainButton();
                        return;
                    }

                    bool bBusiness = true;
                    List<ManagementItem> list = null;
                    if(id > 1000)
                    {
                        bBusiness = false;
                        list = m_pTrainingList;
                    }
                    else
                    {
                        list = m_pBusinessList;
                    }        
                    
                    pBusinessItemList = pBusinessList.BusinessByKey(id);
                    iLevel = bBusiness ? pGameContext.GetCurrentBusinessLevelByID(id) : pGameContext.GetCurrentTrainingLevelByID(id);
                    if(bBusiness)
                    {
                        if(pBusinessItemList.Value.ListLength <= iLevel +1)
                        {
                            string[] eventList = new string[]{"5cpmpk","h4ne8s","radfab","w7sdza","hk8s70","omf4ru","bvou3o","nyfqhg","z94vxl","3cb4wq","75anet","c3rsxa","yrnbt6","lestnu","hs1w64"};
                            pGameContext.SendAdjustEvent(eventList[id -1],true, true,-1);
                        }
                    }
                    else
                    {
                        string[] eventList = null;
                        if(iLevel >= 400)
                        {
                            eventList = new string[]{"o7x7da","8tocj8","xnqj13","icdak6","4f5iaz"};
                        }
                        else if(iLevel >= 300)
                        {
                            eventList = new string[]{"5ou2ab","3tsct0","vmqa68","ixvfon","pyaj9i"};
                        }
                        else if(iLevel >= 200)
                        {
                            eventList = new string[]{"80f95d","voxkvf","hg2m4c","jd0fpo","hoowye"};
                        }
                        else if(iLevel >= 100)
                        {
                            eventList = new string[]{"jwu0xp","1s02rv","yolydu","pzjy6e","eemo5c"};
                        }
                        
                        if(eventList != null)
                        {
                            iLevel = id - 1001;
                            if(eventList.Length > iLevel)
                            {
                                pGameContext.SendAdjustEvent(eventList[iLevel],true, true,-1);
                            }
                        }
                    }

                    for(int n =0; n < list.Count; ++n)
                    {
                        if( id == list[n].ID)
                        {
                            UpdateScroll(list[n],true,true);
                            m_pMainScene.UpdateBuilding(bBusiness,(E_BUILDING)Enum.Parse(typeof(E_BUILDING), pGameContext.GetBuildingNameByBusinessID(id)));
                            break;
                        }
                    }
                }

                UpdateTabUI();
                SoundManager.Instance.PlaySFX("sfx_business_levelup");
            }
        }
    }
}

