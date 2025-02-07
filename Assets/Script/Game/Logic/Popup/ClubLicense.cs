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
using CLUBLICENSEMISSION;
using MILEAGEMISSION;

public class ClubLicense : IBaseUI//, IBaseNetwork
{
    enum E_SCROLL_ITEM : byte { LicenseItem = 0,MissionItem,MAX}
    
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    Animation m_pAnimation = null;
    uint m_iCurrentClubLicensesID = 0;
    // TMPro.TMP_Text  m_pResetTimer = null;
    Image m_pTotalGauge = null;
    TMPro.TMP_Text  m_pTotalGaugeCount = null;
    TMPro.TMP_Text m_pCurrentClubLicenseName = null;
    TMPro.TMP_Text m_pCurrentClubLicenseLabel= null;
    RawImage m_pCurrentClubLicenseRewardIcon = null;
    TMPro.TMP_Text m_pCurrentClubLicenseRewardCount = null;

    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    public ClubLicense(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        m_pAnimation = null;
        ClearScroll();
        m_pScrollRect = null;
        
        m_pTotalGauge = null;
        m_pTotalGaugeCount = null;
        m_pCurrentClubLicenseName = null;
        m_pCurrentClubLicenseLabel= null;
        m_pCurrentClubLicenseRewardIcon = null;
        m_pCurrentClubLicenseRewardCount = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "ClubLicense : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "ClubLicense : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        
        m_pTotalGauge = MainUI.Find("root/license/gauge/fill").GetComponent<Image>();
        m_pCurrentClubLicenseName = MainUI.Find("root/license/title").GetComponent<TMPro.TMP_Text>();
        m_pTotalGaugeCount = MainUI.Find("root/license/gauge/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentClubLicenseLabel = MainUI.Find("root/license/tag/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentClubLicenseRewardIcon = MainUI.Find("root/license/reward/icon").GetComponent<RawImage>();
        m_pCurrentClubLicenseRewardCount = MainUI.Find("root/license/reward/count").GetComponent<TMPro.TMP_Text>();

        m_pScrollRect = MainUI.Find("root/scroll").GetComponent<ScrollRect>();
        SetupScroll();
        MainUI.gameObject.SetActive(false);
    }
    public void SetupLicenseData()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(m_iCurrentClubLicensesID != pGameContext.GetCurrentClubLicensesID())
        {
            ClearScroll();
            SetupScroll();
        }

        UpdateLicense();
    }

    bool executeCallback(IState state,float dt,bool bEnd)
    {
        Animation target = state.GetTarget<BaseStateTarget>().GetMainTarget<Animation>();
        if(target != null)
        {
            if(!target.IsPlaying("reward_complete"))
            {
                SetupLicenseData();
                m_pAnimation = null;
                return true;
            }
        }
        
        return bEnd;
    }

    void CompleteLicense()
    {
        if(m_pAnimation != null)
        {
            m_pAnimation.Play();
            StateMachine.GetStateMachine().AddState(BaseState.GetInstance(new BaseStateTarget(m_pAnimation),1, (uint)E_STATE_TYPE.ShowDailog, null, executeCallback, null));
        }
        else
        {
            SetupLicenseData();
        }
    }

    void SetupScroll()
    {
        GameContext pGameContext = GameContext.getCtx();
        m_iCurrentClubLicensesID = pGameContext.GetCurrentClubLicensesID();
        ClubLicenseMissionList pClubLicenseMissionList = pGameContext.GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
        ClubLicenseItem? pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMissionByKey(m_iCurrentClubLicensesID);
        ClubLicenseGroupItem? pClubLicenseGroupItem = null;
        ClubLicenseMissionItem? pClubLicenseMissionItem = null;
        MileageMissionList pMileageMissionList = pGameContext.GetFlatBufferData<MileageMissionList>(E_DATA_TYPE.MileageMission);
        MileageMissionItem? pMileageMissionItem = pMileageMissionList.MileageMissionByKey(pClubLicenseItem.Value.Mileage);
        EventConditionList pEventConditionList = pGameContext.GetFlatBufferData<EventConditionList>(E_DATA_TYPE.EventCondition);
        string[] names = pGameContext.GetLocalizingText(pMileageMissionItem.Value.Title).Split(';');
        m_pCurrentClubLicenseName.SetText(names[1]);
        m_pCurrentClubLicenseLabel.SetText(names[0]);
        
        MileageT pMileage = pGameContext.GetMileagesData(pClubLicenseItem.Value.Mileage);
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
            m_pCurrentClubLicenseRewardCount.SetText(string.Format("{0:#,0}", amount));
        }
        else
        {
            m_pCurrentClubLicenseRewardCount.SetText(ALFUtils.NumberToString(amount));
        }

        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",idValue.ToString());
        m_pCurrentClubLicenseRewardIcon.texture = pSprite.texture;
        

        TMPro.TMP_Text text = null;
        RectTransform pItem = null;
        RectTransform pContent = null;
        RectTransform pMissionItem = null;
        
        Vector2 size;
        float h = 0;
        float hh =0;

        RawImage icon = null;
        int n = 0;
        Vector3 rot;

        ConditionItem? pConditionItem = null;
        string token = null;
        for(int i = 0; i < pClubLicenseItem.Value.ListLength; ++i)
        {
            pItem = LayoutManager.Instance.GetItem<RectTransform>(E_SCROLL_ITEM.LicenseItem.ToString());
            if(pItem)
            {
                pClubLicenseGroupItem = pClubLicenseItem.Value.List(i);
                idValue = pClubLicenseItem.Value.Mileage + pClubLicenseGroupItem.Value.Type;
                pMileageMissionItem = pMileageMissionList.MileageMissionByKey(idValue);
                pItem.gameObject.name = pClubLicenseGroupItem.Value.Type.ToString();
                text = pItem.Find("title").GetComponent<TMPro.TMP_Text>();
                
                idValue = pMileageMissionItem.Value.List(0).Value.Reward;
                amount = pMileageMissionItem.Value.List(0).Value.RewardAmount;
                text = pItem.Find("reward/count").GetComponent<TMPro.TMP_Text>();

                if(idValue == GameContext.FREE_CASH_ID || idValue == GameContext.CASH_ID)
                {
                    idValue = GameContext.CASH_ID;
                    text.SetText(string.Format("{0:#,0}", amount));
                }
                else
                {
                    text.SetText(ALFUtils.NumberToString(amount));
                }

                pSprite = AFPool.GetItem<Sprite>("Texture",idValue.ToString());

                icon = pItem.Find("reward/icon").GetComponent<RawImage>();
                icon.texture = pSprite.texture;
                pContent = pItem.Find("content").GetComponent<RectTransform>();
                hh = 20;
                for(n = 0; n < pClubLicenseGroupItem.Value.GroupLength; ++n)
                {
                    token = "";
                    pClubLicenseMissionItem = pClubLicenseGroupItem.Value.Group(n);
                    pMissionItem = LayoutManager.Instance.GetItem<RectTransform>(E_SCROLL_ITEM.MissionItem.ToString());
                    pMissionItem.gameObject.name = pClubLicenseMissionItem.Value.Mission.ToString();
                    pMissionItem.SetParent(pContent,false);
                    
                    text = pMissionItem.Find("title").GetComponent<TMPro.TMP_Text>();
                    
                    if(pClubLicenseMissionItem.Value.Event == 2300)
                    {
                        text.SetText(string.Format(pGameContext.GetLocalizingText(pClubLicenseMissionItem.Value.Title),token, pGameContext.GetLocalizingText($"rank_name_{pClubLicenseMissionItem.Value.Objective}")));
                    }
                    else
                    {
                        pConditionItem = pEventConditionList.EventConditionByKey(pClubLicenseMissionItem.Value.Event).Value.ListByKey(pClubLicenseMissionItem.Value.EventCondition);
                    
                        if(pConditionItem != null)
                        {
                            token = pGameContext.GetLocalizingText(pConditionItem.Value.ConditionText);
                        }

                        text.SetText(string.Format(pGameContext.GetLocalizingText(pClubLicenseMissionItem.Value.Title),token, ALFUtils.NumberToString(pClubLicenseMissionItem.Value.Objective)));
                    }
                    
                    text = pMissionItem.Find("reward/count").GetComponent<TMPro.TMP_Text>();
                    idValue = pClubLicenseMissionItem.Value.Reward;
                    if(idValue == GameContext.FREE_CASH_ID || idValue == GameContext.CASH_ID)
                    {
                        idValue = GameContext.CASH_ID;
                        text.SetText(string.Format("{0:#,0}", amount));
                    }
                    else
                    {
                        text.SetText(ALFUtils.NumberToString(pClubLicenseMissionItem.Value.RewardAmount));
                    }

                    pSprite = AFPool.GetItem<Sprite>("Texture",idValue.ToString());

                    icon = pMissionItem.Find("reward/icon").GetComponent<RawImage>();
                    icon.texture = pSprite.texture;

                    pMissionItem.localScale = Vector3.one;       
                    pMissionItem.anchoredPosition = new Vector2(0,-hh+20);
                    size = pMissionItem.sizeDelta;
                    hh += size.y;
                    size.x = 0;
                    pMissionItem.sizeDelta = size;
                }

                size = pContent.sizeDelta;
                size.x = 0;
                size.y = hh;
                pContent.sizeDelta = size;
                pContent = pItem.Find("mark").GetComponent<RectTransform>();
                rot = pContent.localEulerAngles;
                rot.z = hh > 20 ? 0 : 90;
                pContent.localEulerAngles = rot;

                pItem.SetParent(m_pScrollRect.content,false);
                
                pItem.localScale = Vector3.one;       
                pItem.anchoredPosition = new Vector2(0,-h);
                size = pItem.sizeDelta;
                h += size.y +hh;
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

    void UpdateLicense()
    {
        GameContext pGameContext = GameContext.getCtx();
        ClubLicenseMissionList pClubLicenseMissionList = pGameContext.GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
        ClubLicenseItem? pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMissionByKey(m_iCurrentClubLicensesID);
        ClubLicenseGroupItem? pClubLicenseGroupItem = null;
        MileageMissionList pMileageMissionList = pGameContext.GetFlatBufferData<MileageMissionList>(E_DATA_TYPE.MileageMission);
        MileageMissionItem? pMileageMissionItem = pMileageMissionList.MileageMissionByKey(pClubLicenseItem.Value.Mileage);
        MileageT pMileage = pGameContext.GetMileagesData(pClubLicenseItem.Value.Mileage);
        
        Button pButton = m_pCurrentClubLicenseRewardIcon.transform.parent.GetComponent<Button>();
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
            if(pButton.enabled)
            {
                for(int i =0; i < pClubLicenseItem.Value.ListLength; ++i)
                {
                    pClubLicenseGroupItem = pClubLicenseItem.Value.List(i);
                    pMileage = pGameContext.GetMileagesData(pClubLicenseItem.Value.Mileage + pClubLicenseGroupItem.Value.Type);
                    if(pMileage == null || pMileage.Level != 1)
                    {
                        pButton.enabled = false;
                        break;
                    }
                }
            }   
        }
        else
        {
            objective = pMileageMissionItem.Value.List(pMileageMissionItem.Value.ListLength -1).Value.Objective;
            pButton.enabled = false;
        }

        pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);

        if(amount > objective)
        {
            amount=objective;
        }
        m_pTotalGauge.fillAmount = amount / (float)objective; 

        m_pTotalGaugeCount.SetText($"{amount}/{objective}");

        Transform pLicenseItem = null;
        uint type = 0;
        for(int i = 0; i < m_pScrollRect.content.childCount; ++i)
        {
            pLicenseItem = m_pScrollRect.content.GetChild(i);
            if(uint.TryParse(pLicenseItem.gameObject.name,out type))
            {
                UpdateMissionItem(pLicenseItem, type);
            } 
        }
    }

    void UpdatePositionLicenseItem(Transform item, float h )
    {
        Transform pLicenseItem = null;
        Vector3 pos;
        for(int i = 0; i < m_pScrollRect.content.childCount; ++i)
        {
            pLicenseItem = m_pScrollRect.content.GetChild(i);
            if(pLicenseItem == item)
            {
                ++i;
                while(i < m_pScrollRect.content.childCount)
                {
                    pLicenseItem = m_pScrollRect.content.GetChild(i);
                    pos = pLicenseItem.localPosition;
                    pos.y += h;
                    pLicenseItem.localPosition = pos;
                    ++i;
                }
                return;
            }
        }
    }

    void UpdateLicenseItem( Transform pItem, int iLicense)
    {
        GameContext pGameContext = GameContext.getCtx();
        
        Transform pLicenseItem = null;
        string iLid = iLicense.ToString();
        for(int i = 0; i < m_pScrollRect.content.childCount; ++i)
        {
            pLicenseItem = m_pScrollRect.content.GetChild(i);
                
            if(pLicenseItem.gameObject.name == iLid)
            {
                return;
            }
        }
    }

    void UpdateMissionItem(Transform pItem, uint iMissionId)
    {
        GameContext pGameContext = GameContext.getCtx();
        uint parent = pGameContext.GetCurrentClubLicensesID();
        ClubLicenseMissionList pClubLicenseMissionList = pGameContext.GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
        ClubLicenseItem? pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMissionByKey(parent);
        ClubLicenseGroupItem? pClubLicenseGroupItem = pClubLicenseItem.Value.ListByKey(iMissionId);
        
        MileageMissionList pMileageMissionList = pGameContext.GetFlatBufferData<MileageMissionList>(E_DATA_TYPE.MileageMission);
        uint id = pClubLicenseItem.Value.Mileage + iMissionId;
        MileageMissionItem? pMileageMissionItem = pMileageMissionList.MileageMissionByKey(id);
        MileageT pMileage = pGameContext.GetMileagesData(id);
        
        TMPro.TMP_Text text = null;
        Button pButton = null;
        int level = 0;
        ulong amount = 0;
        uint max = 0;
        if(pMileage != null )
        {
            level = (int)pMileage.Level;
            amount = pMileage.Amount;
        }
        
        if(level >= pMileageMissionItem.Value.ListLength)
        {
            pItem.Find("reward").gameObject.SetActive(false);
            pItem.Find("complete").gameObject.SetActive(true);
            max = pMileageMissionItem.Value.List(pMileageMissionItem.Value.ListLength -1).Value.Objective;
        }
        else
        {
            pItem.Find("reward").gameObject.SetActive(true);
            pItem.Find("complete").gameObject.SetActive(false);
            max = pMileageMissionItem.Value.List(level).Value.Objective;
            pButton = pItem.Find("reward").GetComponent<Button>();
            pButton.enabled = max == amount;
            pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
            pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
        }        
        text = pItem.Find("title").GetComponent<TMPro.TMP_Text>();
        text.SetText($"{pGameContext.GetLocalizingText(pMileageMissionItem.Value.Title)} ({amount}/{max})");
        
        Image pGauge = null;

        Transform tm = pItem.Find("content");
        ClubLicenseMissionItem? pClubLicenseMissionItem = null;
        ClubLicensesT pClubLicenses = null;
        Transform pMission = null;
        int iFolding = 0;
        for(int i =0; i <tm.childCount; ++i)
        {
            pMission = tm.GetChild(i);
            if(uint.TryParse(pMission.gameObject.name,out id))
            {
                level = 0;
                amount = 0;
                pClubLicenseMissionItem = pClubLicenseGroupItem.Value.GroupByKey(id);
                pClubLicenses = pGameContext.GetClubLicensesData(parent,id);
                pGauge = pMission.Find("gauge/fill").GetComponent<Image>();
                text = pMission.Find("gauge/count").GetComponent<TMPro.TMP_Text>();
                if(pClubLicenses != null)
                {
                    level = (int)pClubLicenses.Level;
                    amount = pClubLicenses.Amount;
                }
                
                pButton = pMission.Find("reward/get").GetComponent<Button>();
                if( level ==1)
                {
                    pMission.Find("complete").gameObject.SetActive(true);
                    pGauge.fillAmount = 1;
                    pButton.transform.parent.gameObject.SetActive(false);
                    pGauge.transform.parent.gameObject.SetActive(false);
                    iFolding +=1;
                }
                else
                {
                    pGauge.transform.parent.gameObject.SetActive(true);
                    pButton.transform.parent.gameObject.SetActive(true);
                    pMission.Find("complete").gameObject.SetActive(false);
                    pGauge.fillAmount = Mathf.Min(amount / (float)pClubLicenseMissionItem.Value.Objective, 1.0f);
                    pButton.enabled = pGauge.fillAmount >= 1;
                    pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
                    pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
                }
                if(amount > pClubLicenseMissionItem.Value.Objective)
                {
                    amount=pClubLicenseMissionItem.Value.Objective;
                }
                text.SetText($"({amount}/{pClubLicenseMissionItem.Value.Objective})");
            }
        }
        
        Transform mark = pItem.Find("mark");
        if(tm.childCount == iFolding && mark.localEulerAngles.z != 90)
        {
            ScrollViewItemButtonEventCall(m_pScrollRect, pItem,pItem.Find("button").gameObject);
        }
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
            LayoutManager.Instance.AddItem(E_SCROLL_ITEM.LicenseItem.ToString(),item);

            if(tm != null)
            {
                while(tm.childCount > 0)
                {
                    item = tm.GetChild(tm.childCount -1).GetComponent<RectTransform>();
                    icon = item.Find("reward/icon").GetComponent<RawImage>();
                    icon.texture = null;
                    LayoutManager.Instance.AddItem(E_SCROLL_ITEM.MissionItem.ToString(),item);
                }
            }
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
        if(sender.name == "get")
        {
            uint id = 0;
            if(uint.TryParse(sender.transform.parent.parent.gameObject.name,out id))
            {
                JObject pJObject = new JObject();
                pJObject["no"] = m_iCurrentClubLicensesID;
                pJObject["mission"]=id;
                m_pAnimation = sender.transform.parent.parent.GetComponent<Animation>();
                sender.GetComponent<Button>().enabled = false;
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.clubLicense_reward,pJObject,CompleteLicense);
            }
        }
        else if(sender.name == "reward")
        {
            uint id = 0;
            if(uint.TryParse(tm.gameObject.name,out id))
            {
                GameContext pGameContext = GameContext.getCtx();
                ClubLicenseMissionList pClubLicenseMissionList = pGameContext.GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
                ClubLicenseItem? pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMissionByKey(m_iCurrentClubLicensesID);
                m_pAnimation = tm.GetComponent<Animation>();
                sender.GetComponent<Button>().enabled = false;
                GetMileageReward(pGameContext.GetMileagesData(pClubLicenseItem.Value.Mileage + id),CompleteLicense);
            }
        }
        else
        {
            Transform mark = tm.Find("mark");
            Vector3 rot = mark.localEulerAngles;
            RectTransform content = tm.Find("content").GetComponent<RectTransform>();
            Vector2 size = content.sizeDelta;
            float h = 0;
            if(rot.z == 90)
            {
                rot.z = 0;
                size.y = 20;
                for( int n = 0; n < content.childCount; ++n)
                {
                    content.GetChild(n).gameObject.SetActive(true);
                    size.y += content.GetChild(n).GetComponent<RectTransform>().sizeDelta.y;
                }
                h = -size.y + 20;
            }
            else
            {
                rot.z = 90;
                size.y = 20;
                for( int n = 0; n < content.childCount; ++n)
                {
                    h += content.GetChild(n).GetComponent<RectTransform>().sizeDelta.y;
                    content.GetChild(n).gameObject.SetActive(false);
                }
            }
            content.sizeDelta = size;
            mark.localEulerAngles = rot;
            UpdatePositionLicenseItem(tm,h);
            size = root.content.sizeDelta;
            size.y -= h; 
            root.content.sizeDelta = size;
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
            ClubLicenseMissionList pClubLicenseMissionList = pGameContext.GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
            ClubLicenseItem? pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMissionByKey(m_iCurrentClubLicensesID);
            GetMileageReward(pGameContext.GetMileagesData(pClubLicenseItem.Value.Mileage),Close);

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
