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
using DATA;
using CLUBLICENSEMISSION;
using STATEDATA;
using UnityEngine.UI.Extensions;
using MILEAGEMISSION;


public class ClubLicenseComplete : IBaseUI
{
    MainScene m_pMainScene = null;
    TMPro.TMP_Text m_pComment = null;
    EmblemBake m_pEmblemBake = null;
    ParticleSystem m_pParticleSystem = null;
    
    public RectTransform MainUI { get; private set;}

    public ClubLicenseComplete(){}
    
    public void Dispose()
    {
        m_pParticleSystem = null;
        m_pMainScene = null;
        m_pEmblemBake.material = null;
        m_pEmblemBake = null;
        MainUI = null;
        m_pComment = null;

    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "ClubLicenseComplete : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "ClubLicenseComplete : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_pEmblemBake = MainUI.Find("root/reward/emblem").GetComponent<EmblemBake>();
        TMPro.TMP_Text pClubName = MainUI.Find("root/reward/name").GetComponent<TMPro.TMP_Text>();
        m_pComment = MainUI.Find("root/reward/comment").GetComponent<TMPro.TMP_Text>();
        m_pEmblemBake.CopyPoint(m_pMainScene.GetMyEmblem());
        pClubName.SetText(GameContext.getCtx().GetClubName());
        m_pParticleSystem = MainUI.Find("root/FX_3").GetComponent<ParticleSystem>();
        
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void PlayEffect()
    {
        m_pParticleSystem.gameObject.SetActive(true);
        m_pParticleSystem.time = 0;
        m_pParticleSystem.Play(true);
        UIParticleSystem[] list = m_pParticleSystem.GetComponentsInChildren<UIParticleSystem>(true);
        for(int i =0; i < list.Length; ++i)
        {
            list[i].StartParticleEmission();
        }
        LayoutManager.Instance.InteractableEnabledAll();
    }

    public void SetupData(JObject data)
    {
        GameContext pGameContext = GameContext.getCtx();
        uint no =(uint)data["no"];
        if((int)data["Frag"] == 0)
        {
            ClubLicenseMissionList pClubLicenseMissionList = pGameContext.GetFlatBufferData<ClubLicenseMissionList>(E_DATA_TYPE.ClubLicenseMission);
            ClubLicenseItem? pClubLicenseItem = null;

            for(int i =0; i < pClubLicenseMissionList.ClubLicenseMissionLength; ++i)
            {
                pClubLicenseItem = pClubLicenseMissionList.ClubLicenseMission(i);
                if(pClubLicenseItem.Value.Mileage == no)
                {
                    m_pComment.SetText(string.Format("V{0} {1}",pClubLicenseItem.Value.License,pGameContext.GetLocalizingText("LISENCECOMPLETE_LEVELUP_TITLE")));   
                    break;
                }
            }
        }
        else
        {
            MileageMissionList pMileageMissionList = pGameContext.GetFlatBufferData<MileageMissionList>(E_DATA_TYPE.MileageMission);
            MileageMissionItem? pMileageMissionItem = pMileageMissionList.MileageMissionByKey(no);
            string[] list = pGameContext.GetLocalizingText(pMileageMissionItem.Value.Title).Split(';');
            m_pComment.SetText(string.Format("{0} {1}",list[1],pGameContext.GetLocalizingText("LISENCECOMPLETE_LEVELUP_TITLE")));   
        }
    }

    public void Close()
    {
        m_pParticleSystem.gameObject.SetActive(false);
        SingleFunc.HideAnimationDailog(MainUI);
    }
    
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
    
}
