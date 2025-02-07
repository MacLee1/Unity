using UnityEngine;
using UnityEngine.UI;
using ALF;
using PASS;
using System;
using System.Collections.Generic;
using PASSMISSION;
using ALF.LAYOUT;
using ALF.NETWORK;
using Newtonsoft.Json.Linq;
using ALF.SOUND;
using DATA;
// using UnityEngine.EventSystems;
using STATEDATA;


public class SeasonPass : ITimer
{
    const string SCROLL_ITEM_NAME = "SeasonPassItem";

    class SeasonPassItem : IBase
    {
        public uint Id {get; private set;}
        
        public RawImage NormalIcon {get; private set;}
        public RawImage PremiumIcon {get; private set;}

        public RectTransform Normal  {get; private set;}

        public RectTransform Premium  {get; private set;}
        public GameObject Arrow  {get; private set;}

        public GameObject NormalOn  {get; private set;}
        public GameObject NormalOff  {get; private set;}
        public GameObject NormalSelect  {get; private set;}
        public GameObject NormalGet  {get; private set;}

        public GameObject PremiumLock  {get; private set;}
        public GameObject PremiumOn  {get; private set;}
        public GameObject PremiumOff  {get; private set;}

        public GameObject PremiumSelect  {get; private set;}
        public GameObject PremiumGet  {get; private set;}

        public TMPro.TMP_Text PremiumCount {get; private set;}
        public TMPro.TMP_Text NormalCount {get; private set;}

        public TMPro.TMP_Text GaugeCount {get; private set;}
        
        public RectTransform Target  {get; private set;}
        
        Button m_pNormalButton = null;
        Button m_pPremiumButton = null;

        public Button PremiumInfo  {get; private set;}
        public Button NormalInfo  {get; private set;}
        
        public SeasonPassItem(RectTransform target,float fItemWidth)
        {
            Target = target;
            
            Normal = target.Find("item/normal").GetComponent<RectTransform>();
            NormalIcon = Normal.Find("icon").GetComponent<RawImage>();
            
            NormalOn = Normal.Find("on").gameObject;
            NormalOff = Normal.Find("off").gameObject;
            NormalSelect = Normal.Find("select").gameObject;
            NormalGet = Normal.Find("get").gameObject;
            NormalCount = Normal.Find("count").GetComponent<TMPro.TMP_Text>();
            
            Premium = target.Find("item/premium").GetComponent<RectTransform>();
            PremiumIcon = Premium.Find("icon").GetComponent<RawImage>();
            PremiumLock = Premium.Find("lock").gameObject;
            PremiumOn = Premium.Find("on").gameObject;
            PremiumOff = Premium.Find("off").gameObject;
            PremiumSelect = Premium.Find("select").gameObject;
            PremiumGet = Premium.Find("get").gameObject;
            PremiumCount = Premium.Find("count").GetComponent<TMPro.TMP_Text>();

            Arrow = target.Find("gauge/arr").gameObject;
            GaugeCount = target.Find("gauge/count").GetComponent<TMPro.TMP_Text>();
            PremiumInfo = Premium.Find("info").GetComponent<Button>();
            NormalInfo = Normal.Find("info").GetComponent<Button>();
            m_pNormalButton = Normal.GetComponent<Button>();
            m_pPremiumButton = Premium.GetComponent<Button>();

            Vector2 value = Normal.sizeDelta;
            value.x = fItemWidth;
            Normal.sizeDelta = value;

            value = Premium.sizeDelta;
            value.x = fItemWidth;
            Premium.sizeDelta = value;
        }

        public void Dispose()
        {
            NormalIcon.texture = null;
            NormalIcon = null;
            PremiumIcon.texture = null;
            PremiumIcon = null;
            Normal = null;
            Premium = null;
            Arrow = null;
            NormalOn = null;
            NormalOff = null;
            NormalSelect = null;
            NormalGet = null;
            PremiumLock = null;
            PremiumOn = null;
            PremiumOff = null;
            PremiumSelect = null;
            PremiumGet = null;
            PremiumCount = null;
            NormalCount = null;
            GaugeCount = null;
            
            m_pNormalButton.onClick.RemoveAllListeners();
            m_pNormalButton = null;
            m_pPremiumButton.onClick.RemoveAllListeners();
            m_pPremiumButton = null;
            PremiumInfo.onClick.RemoveAllListeners();
            PremiumInfo = null;
            NormalInfo.onClick.RemoveAllListeners();
            NormalInfo = null;
         
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,Target);
            Target = null;
        }

        public void UpdateInfo(PassMissionInfo? pPassMissionInfo,int index)
        {
            if(pPassMissionInfo == null) return;
            
            PassMissionItem? pPassMissionItem = pPassMissionInfo.Value.List(index);
            Id = pPassMissionItem.Value.Mission;
            
            uint id = pPassMissionItem.Value.Reward;
            if(id == GameContext.FREE_CASH_ID)
            {
                id = GameContext.CASH_ID;
            }
            Sprite sprite = ALF.AFPool.GetItem<Sprite>("Texture",id.ToString());
            NormalIcon.texture = sprite.texture;

            if(pPassMissionItem.Value.Reward == GameContext.FREE_CASH_ID || pPassMissionItem.Value.Reward == GameContext.CASH_ID)
            {
                NormalCount.SetText(string.Format("{0:#,0}", pPassMissionItem.Value.RewardAmount));
            }
            else if(pPassMissionItem.Value.Reward == GameContext.GAME_MONEY_ID || (pPassMissionItem.Value.Reward > 40 && pPassMissionItem.Value.Reward < 46))
            {
                NormalCount.SetText($"x{ALFUtils.NumberToString(pPassMissionItem.Value.RewardAmount)}");
            }
            else
            {
                NormalCount.SetText($"x{pPassMissionItem.Value.RewardAmount}");
            }
        
            id = pPassMissionItem.Value.Reward2;
            if(id == GameContext.FREE_CASH_ID)
            {
                id = GameContext.CASH_ID;
            }
            sprite = ALF.AFPool.GetItem<Sprite>("Texture",id.ToString());
            PremiumIcon.texture = sprite.texture;

            if(pPassMissionItem.Value.Reward2 == GameContext.FREE_CASH_ID || pPassMissionItem.Value.Reward2 == GameContext.CASH_ID)
            {
                PremiumCount.SetText(string.Format("{0:#,0}", pPassMissionItem.Value.RewardAmount2));
            }
            else if(pPassMissionItem.Value.Reward2 == GameContext.GAME_MONEY_ID || (pPassMissionItem.Value.Reward2 > 40 && pPassMissionItem.Value.Reward2 < 46))
            {
                PremiumCount.SetText($"x{ALFUtils.NumberToString(pPassMissionItem.Value.RewardAmount2)}");
            }
            else
            {
                PremiumCount.SetText($"x{pPassMissionItem.Value.RewardAmount2}");
            }

            GaugeCount.SetText(pPassMissionItem.Value.Objective.ToString());


            GameContext pGameContext = GameContext.getCtx();
            uint amount = pGameContext.GetCurrentPassAmount();
            bool bPaid = pGameContext.GetCurrentPassPaid();
            
            bool bEnable = amount >= pPassMissionItem.Value.Objective;
            PremiumLock.SetActive(!bPaid);

            // uint before = Id <= 1 ? 0 : pPassMissionInfo.Value.ListByKey(Id-1).Value.Objective;
            // float fill = 0;
            // if((int)amount - before > 0)
            // {
            //     fill = (amount - before) / (float)(pPassMissionItem.Value.Objective - before);    
            //     if(fill > 1)
            //     {
            //         fill = 1;
            //     }
            // }

            Arrow.SetActive(false);
            
            NormalSelect.SetActive(false);
            
            m_pNormalButton.enabled = bEnable;
            if(bEnable)
            {
                if(pGameContext.GetCurrentPassLevel() >= Id)
                {
                    m_pNormalButton.enabled = false;
                    NormalGet.SetActive(true);
                    NormalOn.SetActive(true);
                    NormalOff.SetActive(false);
                }
                else
                {
                    NormalGet.SetActive(false);
                    m_pNormalButton.enabled = pGameContext.GetCurrentPassLevel() +1 == Id;

                    NormalOn.SetActive(m_pNormalButton.enabled);
                    NormalOff.SetActive(!m_pNormalButton.enabled);
                    NormalSelect.SetActive(m_pNormalButton.enabled);
                }
                
                Arrow.SetActive(m_pNormalButton.enabled);
                PremiumSelect.SetActive(false);
                
                if(pGameContext.GetCurrentPassLevel2() >= Id)
                {
                    m_pPremiumButton.enabled = false;
                    PremiumGet.SetActive(true);
                    PremiumOn.SetActive(true);
                    PremiumOff.SetActive(false);
                }
                else
                {
                    PremiumGet.SetActive(false);
                    m_pPremiumButton.enabled = pGameContext.GetCurrentPassLevel2() +1 == Id;
                    PremiumSelect.SetActive(m_pPremiumButton.enabled);
                    PremiumOn.SetActive(m_pPremiumButton.enabled);
                    PremiumOff.SetActive(!m_pPremiumButton.enabled);
                }
            }
            else
            {
                m_pPremiumButton.enabled = false;
                NormalOn.SetActive(false);
                NormalOff.SetActive(true);
                NormalGet.SetActive(false);
                PremiumSelect.SetActive(false);
                PremiumOn.SetActive(false);
                PremiumOff.SetActive(true);
                PremiumGet.SetActive(false);
            }
        }
    }
    
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    RectTransform m_pGauge = null;

    RectTransform m_pGaugeRect = null;
    Image m_pGaugeFill = null;
    TMPro.TMP_Text m_pTitle = null;
    TMPro.TMP_Text m_pExpire = null;
    TMPro.TMP_Text m_pPassPoint = null;
    Button m_pPassPurchase = null;
    Button m_pReciveAll = null;

    float m_fItemWidth = 0;
    
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}

    List<SeasonPassItem> m_pSeasonPassItems = new List<SeasonPassItem>();
    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;
    int m_iDataCount = 0;

    public SeasonPass(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        m_pGaugeFill = null;
        m_pGaugeRect = null;
        m_pGauge = null;
        MainUI = null;
        if(m_pScrollRect != null)
        {
            ClearScroll();
            m_pScrollRect.onValueChanged.RemoveAllListeners();
        }
        m_pSeasonPassItems = null;
        m_pScrollRect = null;
        m_pTitle = null;
        m_pExpire = null;
        m_pPassPurchase = null;
        m_pPassPoint = null;
        m_pReciveAll = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "SeasonPass : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "SeasonPass : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        float w = 0;
        float wh = 0;
        
        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        float ax = 0;
        RectTransform ui = MainUI.Find("root/reward/tab").GetComponent<RectTransform>();
        w = (ui.rect.width / ui.childCount);
        m_fItemWidth = w - 42;
        
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        int n =0;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }

        Transform tm = MainUI.Find("root/title"); 
        m_pTitle = tm.Find("title").GetComponent<TMPro.TMP_Text>();
        m_pExpire = tm.Find("expire/time").GetComponent<TMPro.TMP_Text>();
        
        m_pPassPurchase = tm.Find("purchase").GetComponent<Button>();
        m_pPassPoint = MainUI.Find("root/reward/gauge/text").GetComponent<TMPro.TMP_Text>();
        m_pReciveAll =  MainUI.Find("root/allReceive").GetComponent<Button>();
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        m_pGauge = m_pScrollRect.transform.Find("gauge/fill").GetComponent<RectTransform>();
        size = m_pGauge.sizeDelta;
        size.y = 0;
        m_pGauge.sizeDelta = size;
        m_pGaugeFill = m_pScrollRect.viewport.Find("gauge").GetComponent<Image>();
        m_pGaugeRect = m_pGaugeFill.GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        SetupScroll();
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        
        MainUI.gameObject.SetActive(false);
    }

    public void DoExpire(int index)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            GameContext pGameContext = GameContext.getCtx();
            m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_SEASON_ALEADY_ENDED"),null);
            Close();
        }
    }
    
    void SetupPassTitle()
    {
        uint no = GameContext.getCtx().GetCurrentPassNo();
        if(no > 0)
        {
            uint count = no % 100;
            m_pTitle.SetText(string.Format(GameContext.getCtx().GetLocalizingText("SEASONPASS_TITLE"),(no - count)/100,count));
            //pGameContext.GetCurrentPassExpireTime();
            //pGameContext.IsCurrentPassExpire();
        }
    }
    
    void SetupPassButton()
    {
        GameContext pGameContext = GameContext.getCtx();
        m_pPassPurchase.enabled = !pGameContext.GetCurrentPassPaid();
        m_pPassPurchase.transform.Find("on").gameObject.SetActive(m_pPassPurchase.enabled);
        m_pPassPurchase.transform.Find("off").gameObject.SetActive(!m_pPassPurchase.enabled);
        TMPro.TMP_Text text = m_pPassPurchase.transform.Find("title").GetComponent<TMPro.TMP_Text>();
        if(m_pPassPurchase.enabled)
        {
            text.SetText(pGameContext.GetLocalizingText("SEASONPASS_BTN_GET_PREMIUM"));
        }
        else
        {
            text.SetText(pGameContext.GetLocalizingText("SEASONPASS_BTN_PREMIUM_ACTIVATED"));
        }
    }

    public void UpdateTimer(float dt)
    {   
        GameContext pGameContext = GameContext.getCtx();
        float tic = pGameContext.GetExpireTimerByUI(this,0);
        if(tic <= 86400)
        {
            SingleFunc.UpdateTimeText((int)tic,m_pExpire,0);
        }
    }
    public void SetupData()
    {
        ResetScroll();
        m_iDataCount = 0;
        GameContext pGameContext = GameContext.getCtx();
        uint no = pGameContext.GetCurrentPassNo();
        float itemSize = 0;

        if(no > 0)
        {
            m_iStartIndex = 0;
            float viewSize = m_pScrollRect.viewport.rect.height;
            
            PassMissionList pPassMissionList = pGameContext.GetFlatBufferData<PassMissionList>(E_DATA_TYPE.PassMission);
            PassMissionInfo? pPassMissionInfo = pPassMissionList.PassMissionByKey(no);
            m_iDataCount = pPassMissionInfo.Value.ListLength;

            SeasonPassItem pSeasonPassItem = null;

            for(int i =0; i < m_pSeasonPassItems.Count; ++i)
            {
                pSeasonPassItem = m_pSeasonPassItems[i];
                itemSize = pSeasonPassItem.Target.rect.height;

                if(m_iDataCount <= i)
                {
                    pSeasonPassItem.Target.gameObject.SetActive(false);
                }
                else
                {
                    if(viewSize > -itemSize)
                    {    
                        viewSize -= itemSize;
                        pSeasonPassItem.Target.gameObject.SetActive(viewSize > -itemSize);
                    }
                }
            }
            
            Vector2 size = m_pScrollRect.content.sizeDelta;
            size.y = m_iDataCount * itemSize;
            m_pScrollRect.content.sizeDelta = size;
            size = m_pGaugeRect.sizeDelta;
            size.y = m_pScrollRect.content.sizeDelta.y;
            m_pGaugeRect.sizeDelta = size;

            m_pScrollRect.verticalNormalizedPosition = 1;
            m_pPrevDir.y = 1;
            m_pScrollRect.content.anchoredPosition = Vector2.zero;

            UpdateScroll();
        }
    }

    public void UpdateScroll()
    {
        GameContext pGameContext = GameContext.getCtx();
        uint no = pGameContext.GetCurrentPassNo();
        if(no > 0)
        {
            if(pGameContext.GetExpireTimerByUI(this,0) <= -1)
            {
                float fTime = pGameContext.GetCurrentSeasonExpireRemainTime();
                pGameContext.AddExpireTimer(this,0,fTime);
                SingleFunc.UpdateTimeText((int)fTime,m_pExpire,0);
            }
        
            SetupPassButton();
            SetupPassTitle();

            m_pPassPoint.SetText(pGameContext.GetCurrentPassAmount().ToString());

            PassMissionList pPassMissionList = pGameContext.GetFlatBufferData<PassMissionList>(E_DATA_TYPE.PassMission);
            PassMissionInfo? pPassMissionInfo = pPassMissionList.PassMissionByKey(no);
            SeasonPassItem pSeasonPassItem = null;
            int index =0;
            for(int i =0; i < m_pSeasonPassItems.Count; ++i)
            {
                pSeasonPassItem = m_pSeasonPassItems[i];
                index = m_iStartIndex + i;
                if(index > -1 && index < m_iDataCount)
                {
                    pSeasonPassItem.UpdateInfo(pPassMissionInfo,m_iStartIndex + i);
                }
            }

            m_pReciveAll.enabled = GetRewardItemList() != null;
            m_pReciveAll.transform.Find("on").gameObject.SetActive(m_pReciveAll.enabled);
            m_pReciveAll.transform.Find("off").gameObject.SetActive(!m_pReciveAll.enabled);
            float size = m_pSeasonPassItems[0].Target.rect.height;
            uint amount = pGameContext.GetCurrentPassAmount();
            
            PassMissionItem? pPassMissionItem = null;
            uint before = 0;
            float h = 0;
            float fill = 0;
            for(int i =0; i < pPassMissionInfo.Value.ListLength; ++i)
            {
                pPassMissionItem = pPassMissionInfo.Value.List(i);
                before = i <= 0 ? 0 : pPassMissionInfo.Value.List(i-1).Value.Objective;
            
                fill = 0;
                if((int)amount - before > 0)
                {
                    fill = (amount - before) / (float)(pPassMissionItem.Value.Objective - before);    
                    if(fill > 1)
                    {
                        fill = 1;
                    }
                }

                if(fill <= 0)
                {
                    break;
                }

                h += size * fill;
            } 

            h -= size * 0.5f;
            if(h < 0)
            {
                h =0;
            }
            fill = m_pScrollRect.content.rect.height;
            m_pGaugeFill.fillAmount = h / fill;
        }
    }

    void SetupScroll()
    {
        GameContext pGameContext = GameContext.getCtx();
        Vector2 size;
        float h = 0;
        RectTransform pItem = null;
        
        m_iTotalScrollItems = 0;
        m_iStartIndex = 0;

        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        SeasonPassItem pSeasonPassItem = null;

        while(viewSize > -itemSize)
        {
            if(viewSize > 0)
            {
                ++m_iTotalScrollItems;
            }
            
            pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
                
            if(pItem)
            {
                pItem.SetParent(m_pScrollRect.content,false);
                pItem.anchoredPosition = new Vector2(0,-h);
                pItem.localScale = Vector3.one;
                size = pItem.sizeDelta;
                size.x = 0;
                pItem.sizeDelta = size;
                itemSize = pItem.rect.height;
                h += itemSize;
                viewSize -= itemSize;

                pSeasonPassItem = new SeasonPassItem(pItem,m_fItemWidth);
                m_pSeasonPassItems.Add(pSeasonPassItem);
            }
        }

        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);

        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.content.anchoredPosition = Vector2.zero; 
    }

    void ClearScroll()
    {
        int i = m_pSeasonPassItems.Count;
        while(i > 0)
        {
            --i;
            m_pSeasonPassItems[i].Dispose(); 
        }
        
        m_pSeasonPassItems.Clear();
        
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }
    void ResetScroll()
    {
        Vector2 value;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        SeasonPassItem pItem = null;
        for(int i = 0; i < m_pSeasonPassItems.Count; ++i)
        {
            pItem = m_pSeasonPassItems[i];
            itemSize = pItem.Target.rect.height;
            viewSize -= itemSize;
            pItem.Target.gameObject.SetActive(viewSize > -itemSize);

            value = pItem.Target.anchoredPosition;            
            value.y = -i * itemSize;
            pItem.Target.anchoredPosition = value;
        }

        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
        m_pPrevDir.y = 1;
        value = m_pGaugeRect.anchoredPosition;
        value.y = 0;
        m_pGaugeRect.anchoredPosition = value;
        m_pGaugeFill.fillAmount = 0;
    }

    public void Close()
    {
        Enable = false;
        SingleFunc.HideAnimationDailog(MainUI);
    }
    uint[] GetRewardItemList()
    {
        GameContext pGameContext =GameContext.getCtx();
        uint no = GameContext.getCtx().GetCurrentPassNo();
        if(no > 0)
        {
            PassMissionList pPassMissionList = pGameContext.GetFlatBufferData<PassMissionList>(E_DATA_TYPE.PassMission);
            PassMissionInfo? pPassMissionInfo = pPassMissionList.PassMissionByKey(no);
            PassMissionItem? pPassMissionItem = null;
            uint amount = pGameContext.GetCurrentPassAmount();
            List<uint> list = new List<uint>();

            no = pGameContext.GetCurrentPassLevel();
            for(int i = 0; i < pPassMissionInfo.Value.ListLength; ++i)
            {
                pPassMissionItem = pPassMissionInfo.Value.List(i);
                if(pPassMissionItem.Value.Mission > no && pPassMissionItem.Value.Objective <= amount)
                {
                    list.Add(pPassMissionItem.Value.Mission);
                }
            }
            if(pGameContext.GetCurrentPassPaid())
            {
                no = pGameContext.GetCurrentPassLevel2();
                for(int i = 0; i < pPassMissionInfo.Value.ListLength; ++i)
                {
                    pPassMissionItem = pPassMissionInfo.Value.List(i);
                    if(pPassMissionItem.Value.Mission > no && pPassMissionItem.Value.Objective <= amount)
                    {
                        list.Add(pPassMissionItem.Value.Mission + 100);
                    }
                }
            }
            if(list.Count > 0)
            {
                return list.ToArray();
            }
        }

        return null;
    }
    
    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        SeasonPassItem pItem = null;
        if(index > iTarget)
        {
            pItem = m_pSeasonPassItems[iTarget];
            m_pSeasonPassItems[iTarget] = m_pSeasonPassItems[index];
            i = iTarget +1;
            SeasonPassItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pSeasonPassItems[i];
                m_pSeasonPassItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pSeasonPassItems[index] = pItem;
            pItem = m_pSeasonPassItems[iTarget];
        }
        else
        {
            pItem = m_pSeasonPassItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pSeasonPassItems[i -1] = m_pSeasonPassItems[i];
                ++i;
            }

            m_pSeasonPassItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex + iTarget + iCount;
        
        if(i < 0 || m_iDataCount <= i) return;

        GameContext pGameContext = GameContext.getCtx();
        uint no = pGameContext.GetCurrentPassNo();
        PassMissionList pPassMissionList = pGameContext.GetFlatBufferData<PassMissionList>(E_DATA_TYPE.PassMission);
        PassMissionInfo? pPassMissionInfo = pPassMissionList.PassMissionByKey(no);
        
        pItem.UpdateInfo(pPassMissionInfo,i);
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        Vector2 _value = m_pGaugeRect.anchoredPosition;
        _value.y = m_pScrollRect.content.anchoredPosition.y; 
        m_pGaugeRect.anchoredPosition = _value;

        if(m_iDataCount <= 0) return;
        
        _value = m_pGauge.sizeDelta;
        if(m_pScrollRect.content.anchoredPosition.y <= 0)
        {
            _value.y = MathF.Max(0,m_pScrollRect.content.anchoredPosition.y * -1);
        }
        else
        {
            _value.y = 0;
        }
        m_pGauge.sizeDelta = _value;

        if(m_iTotalScrollItems < m_iDataCount && value.y != m_pPrevDir.y)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
        
    }
    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.IsCurrentPassExpire())
        {
            Close();
            m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_SEASON_ALEADY_ENDED"),null);
        }
        else
        {
            for(int i =0; i < m_pSeasonPassItems.Count; ++i)
            {
                if(m_pSeasonPassItems[i].Target == tm)
                {
                    if(m_pSeasonPassItems[i].Premium.gameObject == sender)
                    {
                        if(pGameContext.GetCurrentPassPaid())
                        {
                            if(m_pSeasonPassItems[i].Id -1 == pGameContext.GetCurrentPassLevel2())
                            {
                                JObject pJObject = new JObject();
                                JArray pJArray = new JArray();
                                pJArray.Add(m_pSeasonPassItems[i].Id +100);
                                pJObject["missions"] = pJArray;
                                m_pMainScene.RequestAfterCall(E_REQUEST_ID.pass_reward,pJObject);
                            }
                        }
                        else
                        {
                            m_pMainScene.ShowConfirmPopup( pGameContext.GetLocalizingText("MSG_TITLE_NOTICE"),pGameContext.GetLocalizingErrorMsg("2414"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),null,false);
                        }
                        
                        return;
                    }
                    
                    if(m_pSeasonPassItems[i].Normal.gameObject == sender)
                    {
                        if(m_pSeasonPassItems[i].Id -1 == pGameContext.GetCurrentPassLevel())
                        {
                            JObject pJObject = new JObject();
                            JArray pJArray = new JArray();
                            pJArray.Add(m_pSeasonPassItems[i].Id);
                            pJObject["missions"] = pJArray;

                            m_pMainScene.RequestAfterCall(E_REQUEST_ID.pass_reward,pJObject);
                            return;
                        }
                    }

                    uint no = pGameContext.GetCurrentPassNo();
                    PassMissionList pPassMissionList = pGameContext.GetFlatBufferData<PassMissionList>(E_DATA_TYPE.PassMission);
                    PassMissionInfo? pPassMissionInfo = pPassMissionList.PassMissionByKey(no);    
                    PassMissionItem? pPassMissionItem = pPassMissionInfo.Value.ListByKey(m_pSeasonPassItems[i].Id);
                    RectTransform iconTM = null;
                    uint id = 0;

                    if(m_pSeasonPassItems[i].NormalInfo.gameObject == sender)
                    {    
                        id = pPassMissionItem.Value.Reward == GameContext.FREE_CASH_ID ? GameContext.CASH_ID : pPassMissionItem.Value.Reward;
                        iconTM = m_pSeasonPassItems[i].NormalIcon.GetComponent<RectTransform>();
                    }
                    else if(m_pSeasonPassItems[i].PremiumInfo.gameObject == sender)
                    {
                        id = pPassMissionItem.Value.Reward2 == GameContext.FREE_CASH_ID ? GameContext.CASH_ID : pPassMissionItem.Value.Reward2;
                        iconTM = m_pSeasonPassItems[i].PremiumIcon.GetComponent<RectTransform>();
                    }

                    ItemTip pUI = m_pMainScene.GetInstance<ItemTip>();
                    pUI.SetupData(iconTM, id);
                    pUI.MainUI.SetAsLastSibling();
                    SingleFunc.ShowAnimationDailog(pUI.MainUI,null);

                    return;

                    // m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_RECEIVE_PREVIOUS_REWARD"),null);
                }
            }
        }
    }
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "allReceive")
        {
            GameContext pGameContext =GameContext.getCtx();
            if(pGameContext.IsCurrentPassExpire())
            {
                Close();
                m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_SEASON_ALEADY_ENDED"),null);
            }
            else
            {
                uint[] ids = GetRewardItemList();
                if(ids != null)
                {
                    JObject pJObject = new JObject();
                    JArray pJArray = new JArray(); 
                    for(int i =0; i < ids.Length; ++i)
                    {
                        pJArray.Add(ids[i]);
                    }
                    pJObject["missions"] = pJArray;
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.pass_reward,pJObject);
                }
            }
            return;
        }
        else if(sender.name == "tip")
        {
            m_pMainScene.ShowGameTip("game_tip_seasonpass_title");
            return;
        }
        else if(sender == m_pPassPurchase.gameObject)
        {
            GameContext pGameContext =GameContext.getCtx();
            if(pGameContext.IsCurrentPassExpire())
            {
                Close();
                m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_SEASON_ALEADY_ENDED"),null);
            }
            else
            {
                m_pMainScene.ShowPremiumSeasonPassPopup();
            }
            return;
        }

        Close();
    }
    
}
