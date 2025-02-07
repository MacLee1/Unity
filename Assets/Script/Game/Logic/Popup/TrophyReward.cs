using UnityEngine;
using UnityEngine.UI;
using ALF;
using System;
using System.Collections.Generic;
// using ALF.STATE;
using ALF.NETWORK;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
// using MILEAGEMISSION;
using ACHIEVEMENTMISSION;
using Newtonsoft.Json.Linq;
using STATEDATA;
using USERRANK;


public class TrophyReward : ITimer
{
    const string SCROLL_ITEM_NAME = "TrophyRewardItem";

    static int iCurrentRewardIndex = -1;

    class TrophyRewardItem : IBase
    {
        public uint Id {get; private set;}
        public uint Group {get; private set;}
        
        public RawImage RankIcon {get; private set;}
        public TMPro.TMP_Text RankText {get; private set;}
        public TMPro.TMP_Text AlreadyText {get; private set;}

        public GameObject On  {get; private set;}
        public GameObject Off  {get; private set;}

        public GameObject Lock  {get; private set;}
        public TMPro.TMP_Text Title {get; private set;}
        public TMPro.TMP_Text TrophyText {get; private set;}
        
        public GameObject FillOn  {get; private set;}
        public RectTransform Target  {get; private set;}

        public GameObject ClaimOn  {get; private set;}
        public GameObject ClaimOff  {get; private set;}
        
        Button m_pClaimButton = null;
        Button m_pButton = null;

        public TrophyRewardItem(RectTransform target)
        {
            Target = target;
            
            RankIcon = target.Find("rank").GetComponent<RawImage>();
            RankText = RankIcon.transform.Find("num").GetComponent<TMPro.TMP_Text>();
            Title = target.Find("title").GetComponent<TMPro.TMP_Text>();
            TrophyText = target.Find("trophy/text").GetComponent<TMPro.TMP_Text>();
            AlreadyText = target.Find("already").GetComponent<TMPro.TMP_Text>();
            On = target.Find("normal").gameObject;
            Off = target.Find("off").gameObject;

            Lock = target.Find("lock").gameObject;
            
            FillOn = target.Find("gaugeOn").gameObject;

            m_pClaimButton = target.Find("claim").GetComponent<Button>();
            m_pButton = target.GetComponent<Button>();
            ClaimOn = m_pClaimButton.transform.Find("on").gameObject;
            ClaimOff = m_pClaimButton.transform.Find("off").gameObject;
        }

        public void Dispose()
        {
            ClaimOn = null;
            ClaimOff = null;
            RankText = null;
            AlreadyText = null;
            FillOn = null;
            
            On = null;
            Off = null;
            Lock = null;
            Title = null;
            TrophyText = null;
        
            RankIcon.texture = null;
            RankIcon = null;
            
            m_pClaimButton.onClick.RemoveAllListeners();
            m_pClaimButton = null;
            m_pButton.onClick.RemoveAllListeners();
            m_pButton = null;
         
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,Target);
            Target = null;
        }

        public void UpdateInfo(uint group,AchievementMissionItem? pAchievementMissionItem,int index)
        {
            GameContext pGameContext = GameContext.getCtx();
            float height = Target.rect.height;

            if(pAchievementMissionItem == null)
            {
                Group = 1;
                Id = 0;

                Title.SetText(pGameContext.GetLocalizingText("rank_name_1"));
                TrophyText.SetText("0");
                Off.SetActive(false);
                On.SetActive(true);
                FillOn.SetActive(false);
                m_pClaimButton.gameObject.SetActive(false);
                AlreadyText.gameObject.SetActive(false);
                Lock.SetActive(false);
                SingleFunc.SetupRankIcon(RankIcon,1);

                return;
            }
            else
            {
                Group = group;
                Id = pAchievementMissionItem.Value.Mission;
                
                Title.SetText(pGameContext.GetLocalizingText(pAchievementMissionItem.Value.Name));
                TrophyText.SetText(pAchievementMissionItem.Value.Objective.ToString());
                SingleFunc.SetupRankIcon(RankIcon,pAchievementMissionItem.Value.Icon);
            }

            AchievementT pAchievement = pGameContext.GetAchievementData(GameContext.ACHIEVEMENT_ID);
            uint trophy = pGameContext.GetCurrentUserTrophy();
            
            uint mission1 = 0;
            uint objective = 0;
            uint before = 0;

            if(pAchievementMissionItem != null)
            {
                mission1 = pAchievementMissionItem.Value.Mission;
                objective = pAchievementMissionItem.Value.Objective;
            }
            
            if(mission1 > 1)
            {
                AchievementMissionList pAchievementMissionList = pGameContext.GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
                AchievementGroupItem? pAchievementGroupItem = pAchievementMissionList.AchievementMissionByKey(group);
                AchievementMissionItem? pMissionItem = pAchievementGroupItem.Value.ListByKey(Id-1);
                if(pMissionItem != null)
                {
                    before = pMissionItem.Value.Objective;
                }
                else
                {
                    pAchievementGroupItem = pAchievementMissionList.AchievementMissionByKey(group -1);
                    if(pAchievementGroupItem != null)
                    {
                        pMissionItem = pAchievementGroupItem.Value.ListByKey(Id-1);
                        if(pMissionItem != null)
                        {
                            before = pMissionItem.Value.Objective;
                        }
                    }
                }
            }
            
            float fill = 0;
            
            if((int)trophy - before > 0)
            {
                fill = (float)(trophy - before) / (float)(pAchievementMissionItem.Value.Objective - before);    
                if(fill > 1)
                {
                    fill = 1;
                }
            }
            
            before = 0;
            
            FillOn.SetActive(false); 

            if(fill >= 1)
            {
                AchievementMissionList pAchievementMissionList = pGameContext.GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
                AchievementGroupItem? pAchievementGroupItem = pAchievementMissionList.AchievementMissionByKey(group);
                AchievementMissionItem? pMissionItem = pAchievementGroupItem.Value.ListByKey(Id+1);
                
                if(pMissionItem != null)
                {
                    before = pMissionItem.Value.Objective;
                }
                else
                {
                    pAchievementGroupItem = pAchievementMissionList.AchievementMissionByKey(group +1);
                    if(pAchievementGroupItem != null)
                    {
                        pMissionItem = pAchievementGroupItem.Value.ListByKey(Id+1);
                        if(pMissionItem != null)
                        {
                            before = pMissionItem.Value.Objective;
                        }
                        else
                        {
                            before = trophy +1;
                        }
                    }
                    else
                    {
                        before = trophy +1;
                    }
                }
                
                FillOn.SetActive(trophy < before); 
            }
            
            Off.SetActive(false);
            On.SetActive(true);
            m_pClaimButton.gameObject.SetActive(false);
            AlreadyText.gameObject.SetActive(false);
            Lock.SetActive(false);                

            if(mission1 <= pAchievement.Level)
            {
                AlreadyText.gameObject.SetActive(true);
            }
            else
            {
                if(pAchievement.Amount >= objective)
                {
                    m_pClaimButton.gameObject.SetActive(true);
                    ClaimOn.SetActive(iCurrentRewardIndex == index);
                    ClaimOff.SetActive(!(iCurrentRewardIndex == index));
                }
                else
                {
                    Off.SetActive(true);
                    On.SetActive(false);
                    Lock.SetActive(true);
                }
            }
        }
    }

    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;

    RectTransform m_pGauge = null;
    RectTransform m_pGaugeBG = null;
    Image m_pGaugeFill = null;
    Image m_pGaugeFill_O = null;
    TMPro.TMP_Text m_pCurrentTrophy = null;
    TMPro.TMP_Text m_pCurrentExpireTime = null;
    RawImage m_pCurrentViewTrophy = null;
    TMPro.TMP_Text m_pViewTrophyTitle = null;
    byte m_eCurrentViewRank = 0;
    bool m_bSnapChange = false;
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}

    List<TrophyRewardItem> m_pTrophyRewardItems = new List<TrophyRewardItem>();
    List<int> m_pAchievementMissionDatas = new List<int>();

    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;

    public TrophyReward(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        m_pGauge = null;
        m_pGaugeBG = null;
        SingleFunc.ClearRankIcon(m_pCurrentViewTrophy);
        m_pCurrentViewTrophy = null;
        m_pViewTrophyTitle = null;
        MainUI = null;
        m_pGaugeFill = null;
        m_pGaugeFill_O = null;
        if(m_pScrollRect != null)
        {
            ClearScroll();
            m_pScrollRect.onValueChanged.RemoveAllListeners();
        }
        m_pAchievementMissionDatas.Clear();
        m_pScrollRect = null;
        m_pCurrentTrophy = null;
        m_pCurrentExpireTime = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "TrophyReward : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "TrophyReward : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pCurrentTrophy = MainUI.Find("root/title/trophy/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentExpireTime = MainUI.Find("root/title/time/text").GetComponent<TMPro.TMP_Text>();
        m_pCurrentViewTrophy = MainUI.Find("root/title/icon").GetComponent<RawImage>();
        m_pViewTrophyTitle = MainUI.Find("root/title/title").GetComponent<TMPro.TMP_Text>();
        
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        m_pGaugeBG = m_pScrollRect.transform.Find("gauge/fill").GetComponent<RectTransform>();
        
        m_pGauge = m_pScrollRect.viewport.Find("gauge").GetComponent<RectTransform>();
    
        m_pGaugeFill_O = m_pGauge.Find("fill_o").GetComponent<Image>();
        m_pGaugeFill = m_pGauge.Find("fill").GetComponent<Image>();
        
        Vector2 size = m_pGauge.sizeDelta;
        size.y = 0;
        m_pGaugeBG.sizeDelta = size;
        m_pGauge.sizeDelta = size;
        m_eCurrentViewRank = 0;
        
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        SetupScroll();
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

    public void UpdateTimer(float dt)
    {
        GameContext pGameContext = GameContext.getCtx();
        float tic = pGameContext.GetExpireTimerByUI(this,0);
        if(tic <= 86400)
        {
            SingleFunc.UpdateTimeText((int)tic,m_pCurrentExpireTime,0);
        }
    }

    void SetupViewRank()
    {
        int index = (int)(m_iTotalScrollItems * 0.5f) +1;
        if(m_pTrophyRewardItems[index].Group != m_eCurrentViewRank)
        {
            m_eCurrentViewRank = (byte)m_pTrophyRewardItems[index].Group;
            byte rank = (byte)(m_eCurrentViewRank * 10 - 5);
            SingleFunc.SetupRankIcon(m_pCurrentViewTrophy,rank);
            GameContext pGameContext = GameContext.getCtx();
            AchievementMissionList pAchievementMissionList = pGameContext.GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
            AchievementGroupItem? pAchievementGroupItem = pAchievementMissionList.AchievementMissionByKey((uint)m_eCurrentViewRank);
            m_pViewTrophyTitle.SetText(pGameContext.GetLocalizingText(pAchievementGroupItem.Value.GroupName));
        }
    }

    public void SetupMileageMission()
    {
        ResetScroll();
        
        GameContext pGameContext = GameContext.getCtx();

        m_pCurrentTrophy.SetText(pGameContext.GetCurrentUserTrophy().ToString());

        if(pGameContext.GetExpireTimerByUI(this,0) <= -1)
        {
            float fTime = pGameContext.GetCurrentSeasonExpireRemainTime();
            pGameContext.AddExpireTimer(this,0,fTime);
            SingleFunc.UpdateTimeText((int)fTime,m_pCurrentExpireTime,0);
        }
        
        CallbackUpdate();
        SetCurrentSnap();
    }
    
    void SetCurrentSnap()
    {
        m_bSnapChange = true;
        m_pScrollRect.velocity = Vector2.zero;
        GameContext pGameContext = GameContext.getCtx();
        int iRank = pGameContext.GetCurrentUserRank();
        float itemSize = m_pTrophyRewardItems[0].Target.rect.height;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float height = m_pAchievementMissionDatas.Count * itemSize;
        
        m_pScrollRect.verticalNormalizedPosition = 0;
        Vector2 value = m_pScrollRect.content.anchoredPosition;
        int half = (int)(m_iTotalScrollItems * 0.5f) +1;
        
        if(iRank < half)
        {
            iRank =0;
        }
        else if(iRank > m_pAchievementMissionDatas.Count - m_iTotalScrollItems)
        {
            iRank = m_pAchievementMissionDatas.Count - m_iTotalScrollItems;
        }
        else
        {
            iRank -= half;
        }
        value.y = height - iRank * itemSize - viewSize;
        m_pScrollRect.content.anchoredPosition = value;

        m_iStartIndex = m_pAchievementMissionDatas.Count - m_pTrophyRewardItems.Count - iRank;
        
        value.x =0;
        int index =0;
        int i = m_pTrophyRewardItems.Count;
        height -= itemSize * (iRank +1);
        AchievementMissionList pAchievementMissionList = pGameContext.GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
        AchievementGroupItem? pAchievementGroupItem = null;
        AchievementMissionItem? pAchievementMissionItem = null;
        while(i > 0)
        {
            --i;
            index = m_iStartIndex + i;
            value = m_pTrophyRewardItems[i].Target.anchoredPosition;
            value.y = -height;
            m_pTrophyRewardItems[i].Target.anchoredPosition = value;
            height -= itemSize;
            
            if( index > -1 && index < m_pAchievementMissionDatas.Count)
            {
                if(m_pAchievementMissionDatas[index] >= 0)
                {
                    int g = m_pAchievementMissionDatas[index] / 100;
                    int n = m_pAchievementMissionDatas[index] % 100;
                    
                    pAchievementGroupItem = pAchievementMissionList.AchievementMission(g);
                    pAchievementMissionItem = pAchievementGroupItem.Value.List(n);
                    m_pTrophyRewardItems[i].UpdateInfo(pAchievementGroupItem.Value.Group,pAchievementMissionItem,index);
                }
                else
                {
                    m_pTrophyRewardItems[i].UpdateInfo(1,null,-1);
                }
            }
        }

        Vector2 size = m_pScrollRect.content.anchoredPosition;
        size.x = m_pGauge.anchoredPosition.x;
        m_pGauge.anchoredPosition = size;
        
        SetupViewRank();
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
        TrophyRewardItem pTrophyRewardItem = null;

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

                pTrophyRewardItem = new TrophyRewardItem(pItem);
                m_pTrophyRewardItems.Add(pTrophyRewardItem);
            }
        }

        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);

        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pScrollRect.content.sizeDelta = size;

        AchievementMissionList pAchievementMissionList = pGameContext.GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
        AchievementGroupItem? pAchievementGroupItem = null;
        
        int i = pAchievementMissionList.AchievementMissionLength;
        
        while(i > 0)
        {
            --i;
            pAchievementGroupItem = pAchievementMissionList.AchievementMission(i);
            int n = pAchievementGroupItem.Value.ListLength;

            while(n > 0)
            {
                --n;
                m_pAchievementMissionDatas.Add(i* 100 + n);
            }
        }
        m_pAchievementMissionDatas.Add(-1);
    }

    void ClearScroll()
    {
        int i = m_pTrophyRewardItems.Count;
        while(i > 0)
        {
            --i;
            m_pTrophyRewardItems[i].Dispose(); 
        }
        
        m_pTrophyRewardItems.Clear();
        
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }

    void ResetScroll()
    {
        GameContext pGameContext = GameContext.getCtx();
        AchievementMissionList pAchievementMissionList = pGameContext.GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
        AchievementGroupItem? pAchievementGroupItem = null;
        
        int i = 0;

        float itemSize = m_pTrophyRewardItems[0].Target.rect.height;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float height = m_pAchievementMissionDatas.Count * itemSize;

        m_pScrollRect.verticalNormalizedPosition = 0;
        Vector2 value = m_pScrollRect.content.sizeDelta;
        value.y = height;
        m_pScrollRect.content.sizeDelta = value;
        
        value = m_pGauge.sizeDelta;
        value.y = height;
        m_pGauge.sizeDelta = value;
        value = m_pScrollRect.content.anchoredPosition;
        value.y = height - viewSize;
        m_pScrollRect.content.anchoredPosition = value;
        value = m_pGauge.anchoredPosition;
        value.y = height - viewSize;
        m_pGauge.anchoredPosition = value;

        TrophyRewardItem pItem = null;
        i = m_pTrophyRewardItems.Count;
        int count = m_pAchievementMissionDatas.Count;
        AchievementMissionItem? pAchievementMissionItem = null;
        m_iStartIndex = m_pAchievementMissionDatas.Count -1 - m_iTotalScrollItems;
        m_pPrevDir.y = 0;
        
        while(i > 0 )
        {
            --i;
            --count;
            pItem = m_pTrophyRewardItems[i];
            itemSize = pItem.Target.rect.height;
            viewSize -= itemSize;
            if(viewSize > -itemSize)
            {
                pItem.Target.gameObject.SetActive(true);
            }
            else
            {
                pItem.Target.gameObject.SetActive(false);
            }

            if(m_pAchievementMissionDatas[count] >= 0)
            {
                int g = m_pAchievementMissionDatas[count] / 100;
                int n = m_pAchievementMissionDatas[count] % 100;
                pAchievementGroupItem = pAchievementMissionList.AchievementMission(g);
                pAchievementMissionItem = pAchievementGroupItem.Value.List(n);
                pItem.UpdateInfo(pAchievementGroupItem.Value.Group,pAchievementMissionItem,count);
            }
            else
            {
                pItem.UpdateInfo(1,null,-1);
            }            
                
            value = pItem.Target.anchoredPosition;            
            value.y = -height + itemSize;
            pItem.Target.anchoredPosition = value;
            height -= itemSize;
        }
    }

    void CallbackUpdate()
    {
        GameContext pGameContext = GameContext.getCtx();
        AchievementT pAchievement = pGameContext.GetAchievementData(GameContext.ACHIEVEMENT_ID);
        AchievementMissionList pAchievementMissionList = pGameContext.GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
        AchievementGroupItem? pAchievementGroupItem = null;
        AchievementMissionItem? pAchievementMissionItem = null;
        
        iCurrentRewardIndex = 0;
        int i = m_pAchievementMissionDatas.Count;
        int g = 0;
        int n = 0;
        while(i > 0)
        {
            --i;
            if(m_pAchievementMissionDatas[i] >= 0)
            {
                g = m_pAchievementMissionDatas[i] / 100;
                n = m_pAchievementMissionDatas[i] % 100;
                pAchievementGroupItem = pAchievementMissionList.AchievementMission(g);
                pAchievementMissionItem = pAchievementGroupItem.Value.List(n);
                if(UpdateItem(pAchievementGroupItem.Value.Group,pAchievement.Level,pAchievement.Amount,pAchievementMissionItem))
                {
                    iCurrentRewardIndex = i;
                    break;
                }
            }
        }

        float size = m_pTrophyRewardItems[0].Target.rect.height;
        
        float[] h = new float[2];
        uint before = 0;
        
        i = m_pAchievementMissionDatas.Count;
        uint trophy = pGameContext.GetCurrentUserTrophy();
        float value =0;
        float value1 =0;
        while(i > 0)
        {
            --i;
            before = 0;
            value =0;
            value1 =0;
            if(m_pAchievementMissionDatas[i] >= 0)
            {
                if(m_pAchievementMissionDatas.Count -2 > i)
                {
                    g = m_pAchievementMissionDatas[i +1] / 100;
                    n = m_pAchievementMissionDatas[i +1] % 100;
                    pAchievementGroupItem = pAchievementMissionList.AchievementMission(g);
                    pAchievementMissionItem = pAchievementGroupItem.Value.List(n);
                    before = pAchievementMissionItem.Value.Objective;
                }
                g = m_pAchievementMissionDatas[i] / 100;
                n = m_pAchievementMissionDatas[i] % 100;
                pAchievementGroupItem = pAchievementMissionList.AchievementMission(g);
                pAchievementMissionItem = pAchievementGroupItem.Value.List(n);
                
                if((int)pAchievement.Amount - before > 0)
                {
                    value = (float)(pAchievement.Amount - before) / (float)(pAchievementMissionItem.Value.Objective - before);    
                    if(value > 1)
                    {
                        value = 1;
                    }
                }

                h[0] += size * value;
                
                if((int)trophy - before > 0)
                {
                    value1 = (float)(trophy - before) / (float)(pAchievementMissionItem.Value.Objective - before);    
                    if(value1 > 1)
                    {
                        value1 = 1;
                    }
                }

                h[1] += size * value1;

                if(value <= 0 && value1 <= 0)
                {
                    break;
                }
            }
        }

        h[0] += size * 0.5f;
        h[1] += size * 0.5f;
        value = m_pScrollRect.content.rect.height;

        m_pGaugeFill.fillAmount = h[0] / value;
        m_pGaugeFill_O.fillAmount = h[1] / value;

        i = m_pTrophyRewardItems.Count;
        int index =0;
        while(i > 0)
        {
            --i;
            index = m_iStartIndex + i;
            if(index > -1 && index < m_pAchievementMissionDatas.Count)
            {
                if(m_pAchievementMissionDatas[index] >= 0)
                {
                    g = m_pAchievementMissionDatas[index] / 100;
                    n = m_pAchievementMissionDatas[index] % 100;
                    pAchievementGroupItem = pAchievementMissionList.AchievementMission(g);
                    pAchievementMissionItem = pAchievementGroupItem.Value.List(n);

                    m_pTrophyRewardItems[i].UpdateInfo(pAchievementGroupItem.Value.Group,pAchievementMissionItem,index); 
                }
                else
                {
                    m_pTrophyRewardItems[i].UpdateInfo(1,null,-1);
                }
            }
        }
    }

    bool UpdateItem(uint group, uint mission, uint amount,AchievementMissionItem? pAchievementMissionItem)
    {
        GameContext pGameContext = GameContext.getCtx();
        uint mission1 = 0;
        uint objective = 0;
        
        if(pAchievementMissionItem != null)
        {
            mission1 = pAchievementMissionItem.Value.Mission;
            objective = pAchievementMissionItem.Value.Objective;
        }
        
        return mission1 > mission && amount >= objective;
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        TrophyRewardItem pItem = null;
        if(index > iTarget)
        {
            pItem = m_pTrophyRewardItems[iTarget];
            m_pTrophyRewardItems[iTarget] = m_pTrophyRewardItems[index];
            i = iTarget +1;
            TrophyRewardItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pTrophyRewardItems[i];
                m_pTrophyRewardItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pTrophyRewardItems[index] = pItem;
            pItem = m_pTrophyRewardItems[iTarget];
        }
        else
        {
            pItem = m_pTrophyRewardItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pTrophyRewardItems[i -1] = m_pTrophyRewardItems[i];
                ++i;
            }

            m_pTrophyRewardItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex + iTarget + iCount;
        
        if(i < 0 || m_pAchievementMissionDatas.Count <= i) return;
        
        if(m_pAchievementMissionDatas[i] < 0)
        {
            pItem.UpdateInfo(1,null,i);
        }
        else
        {
            GameContext pGameContext = GameContext.getCtx();
            AchievementMissionList pAchievementMissionList = pGameContext.GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
            
            int g = m_pAchievementMissionDatas[i] / 100;
            int n = m_pAchievementMissionDatas[i] % 100;
            
            AchievementGroupItem? pAchievementGroupItem = pAchievementMissionList.AchievementMission(g);
            AchievementMissionItem? pAchievementMissionItem = pAchievementGroupItem.Value.List(n);
            pItem.UpdateInfo(pAchievementGroupItem.Value.Group,pAchievementMissionItem,i); 
        }
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        Vector2 size = m_pScrollRect.content.anchoredPosition;
        size.x = m_pGauge.anchoredPosition.x;
        m_pGauge.anchoredPosition = size;

        if(m_bSnapChange)
        {
            m_bSnapChange = false;
            return;
        }

        if(m_pAchievementMissionDatas.Count <= 0) return;

        size = m_pGaugeBG.sizeDelta;
        size.y = MathF.Max(0,m_pScrollRect.content.anchoredPosition.y - m_pScrollRect.content.rect.height + m_pScrollRect.viewport.rect.height);
        m_pGaugeBG.sizeDelta = size;

        if(m_iTotalScrollItems < m_pAchievementMissionDatas.Count && value.y != m_pPrevDir.y)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;

            SetupViewRank();
        }
    }
    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        for(int i =0; i < m_pTrophyRewardItems.Count; ++i)
        {
            if(m_pTrophyRewardItems[i].Target == tm)
            {
                if(sender.transform == tm)
                {
                    m_pMainScene.ShowTrophyRewardInfoPopup(m_pTrophyRewardItems[i].Group,m_pTrophyRewardItems[i].Id);
                }
                else
                {
                    JObject pJObject = new JObject();
                    pJObject["no"] = GameContext.ACHIEVEMENT_ID;
                    pJObject["mission"] = m_pTrophyRewardItems[i].Id;
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.achievement_reward,pJObject,CallbackUpdate);
                }

                return;
            }
        }
    }

    public void Close()
    {
        Enable = false;
        SingleFunc.HideAnimationDailog(MainUI);
    }
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "close")
        {
            Close();
        }
        else if(sender.name == "tip")
        {
            m_pMainScene.ShowGameTip("game_tip_trophyreward_title");
        }
            
    }
}
