using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using Newtonsoft.Json.Linq;
// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
using USERDATA;
using DATA;
using STATEDATA;


public class Conference : IBaseUI
{
    MainScene m_pMainScene = null;
    public RectTransform MainUI { get; private set;}

    Animation m_pAnimation = null;
    List<ulong> m_iPlayerIDList = new List<ulong>();
    Transform m_pComment = null;
    TMPro.TMP_Text m_pPlayerName = null;
    Transform m_pMontageIcon = null;

    const string PLAYER_NAME = " <color=#259D00>{0} {1}</color> ";

    public Conference(){}
    
    public void Dispose()
    {
        m_pComment = null;
        m_pPlayerName = null;
        m_pMainScene = null;
        MainUI = null;
        m_pMontageIcon = null;
        m_pAnimation?.Stop();
        m_pAnimation = null;
        m_iPlayerIDList.Clear();
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Conference : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Conference : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pMontageIcon = MainUI.Find("root/mrc/icon");
        m_pAnimation = MainUI.Find("root").GetComponent<Animation>();
        m_pComment = MainUI.Find("root/BG_01/text");
        RawImage pLine = MainUI.Find("root/BG_01/line").GetComponent<RawImage>();
        m_pPlayerName = pLine.transform.Find("text").GetComponent<TMPro.TMP_Text>();
        MainUI.gameObject.SetActive(false);
        Material pMaterial = m_pMainScene.GetMyEmblemMaterial();
        if(pMaterial != null)
        {
            pLine.color = pMaterial.GetColor("_Pattern1Color");
        }
        else 
        {
            pLine.color = Color.white;
        }
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void Show()
    {
        if(m_iPlayerIDList.Count > 0)
        {
            ulong id = m_iPlayerIDList[m_iPlayerIDList.Count -1];
            m_iPlayerIDList.RemoveAt(m_iPlayerIDList.Count -1);
            m_pComment.gameObject.SetActive(m_iPlayerIDList.Count ==0);
            PlayerT pPlayer = GameContext.getCtx().GetPlayerByID(id);

            string comment = GameContext.getCtx().GetLocalizingText("ALERT_TXT_PLAYER_CONTRACT");
            m_pPlayerName.SetText(string.Format(comment,string.Format(PLAYER_NAME,pPlayer.Forename,pPlayer.Surname)));
            SingleFunc.SetupPlayerFace(pPlayer,m_pMontageIcon);
            m_pAnimation.Play("show");
        }
    }

    public void SetupData(JObject data)
    {
        m_pMontageIcon.transform.parent.gameObject.SetActive(false);
        m_pComment.transform.parent.gameObject.SetActive(false);
        
        m_iPlayerIDList.Clear();
        JArray players = (JArray)data["players"];
        JObject item = null;

        int i = players.Count;
        while(i > 0)
        {
            --i;
            item = (JObject)players[i];
            m_iPlayerIDList.Add((ulong)item["id"]);
        }
    }

    public void Close()
    {
        m_pAnimation.Play("hide");
        SingleFunc.HideAnimationDailog(MainUI,()=>{SingleFunc.ClearPlayerFace(m_pMontageIcon); MainUI.gameObject.SetActive(false); MainUI.Find("root").localScale = Vector3.one; LayoutManager.Instance.InteractableEnabledAll();});
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {        
        // Close();
        if(m_pAnimation.IsPlaying("hide")) return;

        if(!m_pAnimation.IsPlaying("show"))
        {
            if(m_iPlayerIDList.Count > 0)
            {
                Show();
            }
            else
            {
                Close();
            }
        }
    }
}
