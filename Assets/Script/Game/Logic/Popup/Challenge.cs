using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.NETWORK;
using ALF.LAYOUT;
using ALF.SOUND;
using USERDATA;
using DATA;
using CONSTVALUE;
using Newtonsoft.Json.Linq;

public class Challenge : ITimer
{
    const string SCROLL_ITEM_NAME = "ChallengeListItem";
    ulong m_ulId = 0;
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    Button m_pRefreshButton = null;
    TMPro.TMP_Text m_pRefreshText = null;
    TMPro.TMP_Text m_pTicketText = null;
    TMPro.TMP_Text m_pTimeText = null;
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    List<ChallengeItem> m_pChallengeList = new List<ChallengeItem>();
    List<ChallengeItemData> m_pChallengeDataList = new List<ChallengeItemData>();

    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;


    string m_strRefresh = null;
    GameObject[] m_pRefreshGameObjects = new GameObject[2];
    public Challenge(){}
    
    public void Dispose()
    {
        ClearScroll();
        m_pScrollRect.onValueChanged.RemoveAllListeners();
        m_pChallengeDataList = null;
        m_pChallengeList = null;
        m_pScrollRect = null;
        m_pMainScene = null;
        MainUI = null; 
        m_pRefreshButton = null;
        m_pRefreshText = null;
        m_pTicketText = null;
        m_pTimeText = null;
        m_pRefreshGameObjects[0]= null;
        m_pRefreshGameObjects[1]= null;
        m_pRefreshGameObjects= null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {        
        ALFUtils.Assert(pBaseScene != null, "Challenge : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Challenge : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_strRefresh = GameContext.getCtx().GetLocalizingText("CHALLENGESTAGE_MATCH_BTN_REFRESH");
        m_pRefreshButton = MainUI.Find("root/bg/refresh").GetComponent<Button>();
        m_pRefreshText = m_pRefreshButton.transform.Find("text").GetComponent<TMPro.TMP_Text>();
        m_pRefreshGameObjects[0] = m_pRefreshButton.transform.Find("on").gameObject;
        m_pRefreshGameObjects[1] = m_pRefreshButton.transform.Find("off").gameObject;

        m_pTicketText = MainUI.Find("root/bg/count/text").GetComponent<TMPro.TMP_Text>();
        m_pTimeText = MainUI.Find("root/bg/next/text").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        MainUI.gameObject.SetActive(false);
        SetupScroll();
        // SetupTicketChargeTime();
        // UpdateTicketCount();
        ChangeOnRefresh();
    }

    public void HideScroll()
    {
        m_pScrollRect.gameObject.SetActive(false);
    }

    void ResetScroll()
    {
        Vector2 pos;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;

        ChallengeItem pItem = null;
        
        for(int i = 0; i < m_pChallengeList.Count; ++i)
        {
            pItem = m_pChallengeList[i];
            itemSize = pItem.Target.rect.height;
            viewSize -= itemSize;
            pItem.Target.gameObject.SetActive(viewSize > -itemSize);

            pos = pItem.Target.anchoredPosition;            
            pos.y = -i * itemSize;
            pItem.Target.anchoredPosition = pos;
        }
        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
    }

    void SetupScroll()
    {
        RectTransform pItem = null;
        Vector2 size;
        
        m_iTotalScrollItems = 0;
        m_iStartIndex = 0;

        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        float h = 0;
        
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
                m_pChallengeList.Add(new ChallengeItem(pItem,SCROLL_ITEM_NAME));
            }
        }
        
        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pPrevDir.x = 0;
        
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
    }

    void UpdateTicketCount()
    {
        GameContext pGameContext = GameContext.getCtx();
        m_pTicketText.SetText($"{pGameContext.GetCurrentChallengeTicket()}/{pGameContext.GetConstValue(E_CONST_TYPE.challengeStageMatchChanceMax)}");
    }
    
    void SetupTicketChargeTime()
    {
        GameContext pGameContext = GameContext.getCtx();
        float time = pGameContext.GetChallengeTicketChargeTime();
        if(time >= 0)
        {
            SingleFunc.UpdateTimeText((int)time,m_pTimeText,0);
            pGameContext.AddExpireTimer(this,0,time);
        }
        else
        {
            m_pTimeText.SetText("-");
        }
        UpdateTicketCount();
    }
    public void UpdateTimer(float dt)
    {
        GameContext pGameContext = GameContext.getCtx();
        float tic = pGameContext.GetExpireTimerByUI(this,1);
        if(tic > 0)
        {
            SingleFunc.UpdateTimeText((int)tic,m_pRefreshText,0);
        }
        
        if(pGameContext.GetCurrentChallengeTicket() < pGameContext.GetConstValue(E_CONST_TYPE.challengeStageMatchChanceMax))
        {
            tic = pGameContext.GetExpireTimerByUI(this,0);
            if(tic > 0)
            {
                SingleFunc.UpdateTimeText((int)tic,m_pTimeText,0);
            }
        }
    }

    public void DoExpire(int index)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            if(index == 1)
            {
                ChangeOnRefresh();
            }
            else
            {
                SetupTicketChargeTime();
            }
        }
    }

    void ChangeOnRefresh()
    {
        m_pRefreshText.SetText(m_strRefresh);
        m_pRefreshButton.enabled = true;
        // m_pRefreshText.color = GameContext.GREEN;
        m_pRefreshGameObjects[0].SetActive(true);
        m_pRefreshGameObjects[1].SetActive(false);
    }

    public void SetupData(JObject data)
    {
        if( MainUI == null || !MainUI.gameObject.activeSelf) return;

        ResetScroll();

        if(data != null && data.ContainsKey("opps") && data["opps"].Type == JTokenType.Array)
        {
            m_pScrollRect.gameObject.SetActive(true);
            int i = m_pChallengeDataList.Count;
            while(i > 0)
            {
                --i;
                m_pChallengeDataList[i].Dispose();
            }
            m_pChallengeDataList.Clear();
            m_iStartIndex = 0;

            GameContext pGameContext = GameContext.getCtx();
            SetupTicketChargeTime();
            UpdateTicketCount();
            
            JArray pJArray = (JArray)data["opps"];
            for( i =0; i < pJArray.Count; ++i)
            {
                m_pChallengeDataList.Add(new ChallengeItemData((JObject)pJArray[i]));       
            }

            float viewSize = m_pScrollRect.viewport.rect.height;
            float itemSize = 0;
            ChallengeItem pChallengeItem = null;
            for(i =0; i < m_pChallengeList.Count; ++i)
            {
                pChallengeItem = m_pChallengeList[i];
                itemSize = pChallengeItem.Target.rect.height;

                if(m_pChallengeDataList.Count <= i)
                {
                    pChallengeItem.Target.gameObject.SetActive(false);
                }
                else
                {
                    if(viewSize > -itemSize)
                    {    
                        viewSize -= itemSize;
                        pChallengeItem.Target.gameObject.SetActive(viewSize > -itemSize);
                    }
                }

                if(m_pChallengeDataList.Count > i)
                {
                    pChallengeItem.UpdateInfo(m_pChallengeDataList[i]);
                }
            }

            Vector2 size = m_pScrollRect.content.sizeDelta;
            size.y = m_pChallengeDataList.Count * itemSize;
            m_pScrollRect.content.sizeDelta = size;
            m_pScrollRect.verticalNormalizedPosition = 1;
            m_pPrevDir.y = 1;
            m_pScrollRect.content.anchoredPosition = Vector2.zero;
            m_pScrollRect.enabled = true;
            LayoutManager.Instance.InteractableEnabledAll();
        }
    }

    void ClearScroll()
    {
        int i =0;
        for(i =0; i < m_pChallengeDataList.Count; ++i)
        {
            m_pChallengeDataList[i].Dispose();
        }

        for(i =0; i < m_pChallengeList.Count; ++i)
        {
            m_pChallengeList[i].Dispose();
        }
        m_pChallengeDataList.Clear();
        m_pChallengeList.Clear();

        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);

        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }

    void ShowClubProfileOverview()
    {
        m_pMainScene.ShowUserProfile(m_ulId,0);
        m_ulId = 0;
    }

    public void FailChallenge()
    {
        Close();
    }

    void TryChallengeStage()
    {
        m_pMainScene.UpdateLeagueTodayCount();
        Close();
        m_pMainScene.ShowMainUI(false);
        m_pMainScene.ShowMatchPopup(GameContext.CHALLENGE_ID);
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        
        int i = 0;
        ChallengeItem pItem = null;
        
        if(index > iTarget)
        {
            pItem = m_pChallengeList[iTarget];
            m_pChallengeList[iTarget] = m_pChallengeList[index];
            i = iTarget +1;
            ChallengeItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pChallengeList[i];
                m_pChallengeList[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pChallengeList[index] = pItem;
            pItem = m_pChallengeList[iTarget];
        }
        else
        {
            pItem = m_pChallengeList[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pChallengeList[i -1] = m_pChallengeList[i];
                ++i;
            }

            m_pChallengeList[iTarget] = pItem;
        }
        
        i = m_iStartIndex + iTarget + iCount;

        if(i < 0 || m_pChallengeDataList.Count <= i) return;

        pItem.UpdateInfo(m_pChallengeDataList[i]);
    }
    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_iTotalScrollItems < m_pChallengeDataList.Count && value.y != m_pPrevDir.y)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        for(int i =0; i < m_pChallengeList.Count; ++i)
        {
            if(m_pChallengeList[i].Target == tm)
            {
                if(tm.gameObject == sender )
                {
                    m_ulId = m_pChallengeList[i].ID;
                    m_pMainScene.RequestClubProfile(m_ulId,ShowClubProfileOverview);
                }
                else
                {
                    GameContext pGameContext = GameContext.getCtx();
                    if(pGameContext.GetCurrentChallengeTicket() > 0)
                    {
                        JObject data = new JObject();
                        data["target"] = m_pChallengeList[i].ID;
                        data["standing"] = m_pChallengeList[i].Standing;
                        if(pGameContext.GetLineupTotalHP() / 1800.0f != 1)
                        {
                            m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_LEAGUE_SQUAD_HP_LOW_TITLE"),pGameContext.GetLocalizingText("DIALOG_LEAGUE_SQUAD_HP_LOW_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,() =>{
                                m_pMainScene.RequestAfterCall(E_REQUEST_ID.challengeStage_try,data,TryChallengeStage);
                            } );
                        }
                        else
                        {
                            m_pMainScene.RequestAfterCall(E_REQUEST_ID.challengeStage_try,data,TryChallengeStage);
                        }
                    }
                    else
                    {
                        m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_CHALLENGE_STAGE_TICKET_NOT_ENOUGH"),null);
                    }
                }
                return;
            }
        }
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI,()=>{
            GameContext.getCtx().RemoveExpireTimerByUI(this);
            ChangeOnRefresh();
            LayoutManager.Instance.InteractableEnabledAll();
            MainUI.Find("root").localScale = Vector3.one;
            MainUI.gameObject.SetActive(false);
        });
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(sender.name)
        {
            case "close":
            case "back":
            {
                Close();
                
            }
            break;
            case "refresh":
            {
                GameContext.getCtx().AddExpireTimer(this,1,5);
                m_pRefreshButton.enabled = false;
                // m_pRefreshText.color = GameContext.GRAY;
                m_pRefreshGameObjects[0].SetActive(false);
                m_pRefreshGameObjects[1].SetActive(true);
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.challengeStage_searchOpps,null);
            }
            break;
        }
    }
    
}
