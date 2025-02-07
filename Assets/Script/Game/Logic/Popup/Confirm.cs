using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALF;
// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
// using DATA;
// using UnityEngine.EventSystems;
using STATEDATA;


public class Confirm : IBaseUI
{
    // Start is called before the first frame update
    IBaseScene m_pScene = null;
    TMPro.TMP_Text m_pTitle = null;
    TMPro.TMP_Text m_pMessage = null;
    TMPro.TMP_Text m_pConfirmText = null;
    TMPro.TMP_Text m_pCancelText = null;

    RectTransform m_pConfirm = null;
    RectTransform m_pCancel = null;
    RectTransform m_pClose = null;

    System.Action m_pActionOK = null;
    System.Action m_pActionCancel = null;
    bool m_bOK = false;

    public RectTransform MainUI { get; private set;}
    public Confirm(){}
    
    public void Dispose()
    {
        m_pScene = null;
        MainUI = null;
        m_pTitle = null;
        m_pMessage = null;
        m_pConfirmText = null;
        m_pCancelText = null;
        m_pConfirm = null;
        m_pCancel = null;
        m_pClose = null;
        m_pActionOK = null;
        m_pActionCancel = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pMainUI != null, "Confirm : targetUI is null!!");

        m_pScene = pBaseScene;
        MainUI = pMainUI;

        m_pMessage = MainUI.Find("root/bg/message").GetComponent<TMPro.TMP_Text>();
        m_pTitle = MainUI.Find("root/title").GetComponent<TMPro.TMP_Text>();
        m_pConfirm = MainUI.Find("root/confirm").GetComponent<RectTransform>();
        m_pCancel = MainUI.Find("root/cancel").GetComponent<RectTransform>();
        m_pConfirmText = m_pConfirm.Find("text").GetComponent<TMPro.TMP_Text>();
        m_pCancelText = m_pCancel.Find("text").GetComponent<TMPro.TMP_Text>();   
        m_pClose = MainUI.Find("root/close").GetComponent<RectTransform>();

        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void ShowCloseButton(bool bShow)
    {
        m_pClose?.gameObject.SetActive(bShow);
    }

    public void SetText(string title, string message, string ok,string cancel = null)
    {
        m_bOK = false;
        m_pTitle?.SetText(title);
        m_pMessage?.SetText(message);
        m_pConfirmText?.SetText(ok);
        
        bool bShowCancel = !string.IsNullOrEmpty(cancel);
        m_pCancel.gameObject.SetActive(bShowCancel);
        Vector2 pos = m_pCancel.anchoredPosition;
        if(bShowCancel)
        {
            m_pCancelText?.SetText(cancel);
            pos.x *= -1;
        }
        else
        {
            pos.x = 0;
        }

        m_pConfirm.anchoredPosition = pos;
    }

    public void SetOKAction(System.Action pAction)
    {
        m_pActionOK = pAction;
    }

    public void SetCancelAction(System.Action pAction)
    {
        m_pActionCancel = pAction;
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI,()=>{
            LayoutManager.Instance.InteractableEnabledAll();
            MainUI.Find("root").localScale = Vector3.one;
            MainUI.gameObject.SetActive(false);
            
            if(m_bOK && m_pActionOK != null )
            {
                m_pActionOK();
                m_pActionOK = null;
            }

            if(!m_bOK && m_pActionCancel != null )
            {
                m_pActionCancel();
                m_pActionCancel = null;
            }
        });
    }
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        m_bOK = false;
        if(sender.name == "confirm" )
        {
            m_bOK = true;
        }

        Close();
    }
    
}
