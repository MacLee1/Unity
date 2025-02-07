using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using USERDATA;
using ALF;
using ALF.LAYOUT;
using ALF.SOUND;
using BUSINESS;
using DATA;


public class FastReward : ITimer
{
    MainScene m_pMainScene = null;
    TMPro.TMP_Text m_pGold = null;
    TMPro.TMP_Text[] m_pTraining = new TMPro.TMP_Text[5];
    TMPro.TMP_Text m_pResetTime = null;
    TMPro.TMP_Text m_pReceiveText = null;
    TMPro.TMP_Text m_pRemainigText = null;
    float m_fExpireTime = 0;
    float m_fTimeRefreshTime = 1;
    bool m_bExpireUpdate = false;
    Transform m_pCommentAll = null;

    int m_iTotalCount = 0;
    int m_iCount = 0;
    
    public RectTransform MainUI { get; private set;}

    public FastReward(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;

        m_pGold = null;
        
        for(int i =0; i < m_pTraining.Length; ++i)
        {            
            m_pTraining[i] = null;
        }
        m_pTraining = null;
        m_pResetTime = null;
        m_pReceiveText = null;
        m_pRemainigText = null;
        m_pCommentAll = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "FastReward : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "FastReward : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        Transform ui = MainUI.Find("root/rewards/training");
        
        for(int n =0; n < m_pTraining.Length; ++n)
        {
            m_pTraining[n] = ui.Find($"{41 + n}/text").GetComponent<TMPro.TMP_Text>();
        }

        ui = MainUI.Find("root/rewards");
        m_pGold = ui.Find("business/10/text").GetComponent<TMPro.TMP_Text>();
        m_pResetTime = ui.Find("reset/time/text").GetComponent<TMPro.TMP_Text>();
        m_pRemainigText = ui.Find("remainig").GetComponent<TMPro.TMP_Text>();
        m_pReceiveText = MainUI.Find("root/receive/text").GetComponent<TMPro.TMP_Text>();
        m_pCommentAll = ui.Find("comment");

        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void SetupRewardData(bool bSendAdjustEvent)
    {
        GameContext pGameContext = GameContext.getCtx();
        if(bSendAdjustEvent && m_iTotalCount > 0)
        {
            int count = m_iTotalCount - m_iCount;
            string[] list = new string[]{"zahyd2","u0ymxi","5xc0qt","nejq22","yssl6i","8pvgqa","mq0j3v","uf1jb6"};
            if(count < list.Length)
            {
                pGameContext.SendAdjustEvent(list[count],false,false,-1);
            }
        }

        m_iTotalCount = pGameContext.GetTotalFastRewardCount();

        MileageT pMileage = pGameContext.GetMileagesData(GameContext.FAST_REWARD_ID);
        m_iCount = m_iTotalCount;
        if(pMileage != null)
        {
            m_iCount -= (int)pMileage.Amount; 
        }

        RectTransform tm = m_pReceiveText.transform.parent.GetComponent<RectTransform>();
        if(m_iCount <= 0)
        {
            m_pCommentAll.gameObject.SetActive(true);
            m_pCommentAll.transform.parent.Find("business").gameObject.SetActive(false);
            m_pCommentAll.transform.parent.Find("training").gameObject.SetActive(false);
            Vector2 pos = tm.anchoredPosition;
            tm.gameObject.SetActive(false);
            tm = MainUI.Find("root/cancel").GetComponent<RectTransform>();
            pos.x = 0;
            tm.anchoredPosition = pos;   
        }
        else
        {
            tm.gameObject.SetActive(true);
            Vector2 pos = tm.anchoredPosition;
            tm = MainUI.Find("root/cancel").GetComponent<RectTransform>();
            pos.x *= -1;
            tm.anchoredPosition = pos;

            m_pReceiveText.transform.parent.gameObject.SetActive(true);
            m_pCommentAll.transform.parent.Find("business").gameObject.SetActive(true);
            m_pCommentAll.transform.parent.Find("training").gameObject.SetActive(true);
            m_pCommentAll.gameObject.SetActive(false);

            m_pGold.SetText(ALFUtils.NumberToString(pGameContext.GetBusinessFastReward()));
            uint no = 0;
            for(int n =0; n < m_pTraining.Length; ++n)
            {
                no = uint.Parse(m_pTraining[n].transform.parent.gameObject.name) + 960;
                m_pTraining[n].SetText(ALFUtils.NumberToString(pGameContext.GetTrainingFastReward(no)));
            }
        }
        m_bExpireUpdate = true;
        m_fTimeRefreshTime = 1;
        m_fExpireTime = ALF.NETWORK.NetworkManager.GetGameServerTimeOfDayTime();
        pGameContext.AddExpireTimer(this,0,m_fExpireTime);
        m_pResetTime.SetText( ALFUtils.SecondToString((int)m_fExpireTime,true,false,false));
        m_pRemainigText.SetText(string.Format(pGameContext.GetLocalizingText("FASTREWARD_TXT_REMAINING_CHANCE"), m_iCount));
        m_pReceiveText.transform.Find("icon").gameObject.SetActive(m_iCount != m_iTotalCount);
        m_pReceiveText.SetText(m_iCount == m_iTotalCount ? pGameContext.GetLocalizingText("FASTREWARD_BTN_FREE") : (m_iTotalCount - m_iCount).ToString());
    }

    public void UpdateTimer(float dt)
    {
        if(m_bExpireUpdate)
        {
            m_fExpireTime -= dt;
            m_fTimeRefreshTime -= dt;

            if(m_fTimeRefreshTime <= 0)
            {
                while(m_fTimeRefreshTime < -1)
                {
                    m_fTimeRefreshTime += 1;
                }
                
                m_fTimeRefreshTime = 1 - m_fTimeRefreshTime;
                m_pResetTime.SetText(ALFUtils.SecondToString((int)m_fExpireTime,true,true,true));
            }

            if(m_fExpireTime <= 0)
            {
                m_fTimeRefreshTime = 1;
                m_bExpireUpdate = false;
            }
        }
    }
    
    public void DoExpire(int index)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            SingleFunc.HideAnimationDailog(MainUI);
        }
    }
    
    public void Close()
    {
        GameContext.getCtx().RemoveExpireTimerByUI(this);
        SingleFunc.HideAnimationDailog(MainUI);
    }
    
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "receive")
        {
            if(m_pReceiveText.transform.Find("icon").gameObject.activeSelf)
            {
                GameContext pGameContext = GameContext.getCtx();
                m_pMainScene.ShowConfirmPopup( pGameContext.GetLocalizingText("DIALOG_QUICKREWARD_TITLE"),string.Format(pGameContext.GetLocalizingText("DIALOG_QUICKREWARD_TXT"),m_pReceiveText.text),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,()=>{
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.fastReward_reward,null); 
                });
            }
            else
            {
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.fastReward_reward,null); 
            }
            
            return;
        } 
        else if(sender.name == "tip")
        {
            m_pMainScene.ShowGameTip("game_tip_fastreward_title");
            return;
        }
        else if(sender.name == "cancel" || sender.name == "back")
        {
            Close();
        }
        else
        {
            m_pMainScene.ShowItemTipPopup(sender);
        }
    }
    
}
