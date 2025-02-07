using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALF;
using System;

// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
// using DATA;
// using UnityEngine.EventSystems;
using STATEDATA;


public class Message : IBaseUI
{
    TMPro.TMP_Text m_pMessage = null;
    TMPro.TMP_Text m_pConfirmText = null;
    IBaseScene m_pMainScene = null;
    Action m_pAction = null;
    public RectTransform MainUI { get; private set;}
    public Message(){}
    
    public void Dispose()
    {
        if(MainUI.gameObject.activeInHierarchy)
        {
            MainUI.gameObject.SetActive(false);
        }
        m_pMainScene = null;
        MainUI = null;
        m_pMessage = null;
        m_pConfirmText = null;
        m_pAction = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pMainUI != null, "Message : targetUI is null!!");

        m_pMainScene = pBaseScene;
        MainUI = pMainUI;

        m_pMessage = MainUI.Find("root/bg/message").GetComponent<TMPro.TMP_Text>();
        m_pConfirmText = MainUI.Find("root/confirm/text").GetComponent<TMPro.TMP_Text>();
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void SetMessage(string message, string ok, Action pAction = null)
    {
        m_pMessage?.SetText(message);
        m_pConfirmText?.SetText(ok);
        m_pAction = pAction;
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI,() => {
                LayoutManager.Instance.InteractableEnabledAll();
                MainUI.gameObject.SetActive(false);
                MainUI.Find("root").localScale = Vector3.one;
                m_pConfirmText.transform.parent.gameObject.SetActive(true);
                if(m_pAction != null)
                {
                    m_pAction();
                    m_pAction = null;
                }
            });
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
    
}
