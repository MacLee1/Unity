using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.LAYOUT;
using ALF.SOUND;
using USERDATA;
using DATA;
using Newtonsoft.Json.Linq;

public class LoadTactics : IBaseUI
{
    GameObject m_pSelectGameObject = null;
    MainScene m_pMainScene = null;
    Button[] m_pSlotList = null;
    Button m_pLoad = null;
    public RectTransform MainUI { get; private set;}
    public LoadTactics(){}
    
    public void Dispose()
    {
        int i =0;
        for(i =0; i < m_pSlotList.Length; ++i)
        {
            m_pSlotList[i] = null;
        }
        m_pSlotList = null;
        m_pMainScene = null;
        MainUI = null;
        m_pSelectGameObject = null;
        m_pLoad = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "LoadTactics : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "LoadTactics : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_pLoad = MainUI.Find("root/load").GetComponent<Button>();
        m_pSlotList = MainUI.Find("root/bg").GetComponentsInChildren<Button>(true);
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void SetupTacticsInfo()
    {
        m_pSelectGameObject = null;
        EnableConfirm(false);
        List<TacticsInfoT> list = GameContext.getCtx().GetTacticsInfoList();
        TMPro.TMP_Text text = null;
        Button pButton = null;
        Transform tm = null;
        List<byte> formation = null;
        
        int n = 0;
        int index = 0;
        for(int i =0; i < m_pSlotList.Length; ++i)
        {
            pButton = m_pSlotList[i];
            pButton.enabled = false;

            for(n =0; n < list.Count; ++n)
            {
                if(list[n].Type.ToString() == pButton.gameObject.name)
                {
                    index = n;
                    pButton.enabled = true;
                    break;
                }    
            }
            
            if(pButton.enabled)
            {
                pButton.transform.Find("on/select").gameObject.SetActive(false);
                pButton.transform.Find("on").gameObject.SetActive(true);
                pButton.transform.Find("empty").gameObject.SetActive(false);
                
                tm = pButton.transform.Find("on/formation");

                text = tm.Find("text").GetComponent<TMPro.TMP_Text>();
                formation = GameContext.getCtx().GetFormationByIndex(list[index].Type);
                text.SetText(TacticsFormation.UpdateFormationText(formation));
                
                for(n = 1; n < 26; ++n)
                {
                    tm.Find(n.ToString()).gameObject.SetActive(false);
                }

                for(n =1; n < formation.Count; ++n)
                {
                    tm.Find(formation[n].ToString()).gameObject.SetActive(true);
                }
                
                text = pButton.transform.Find("on/name").GetComponent<TMPro.TMP_Text>();
                text.SetText(list[index].Name);
            }
            else
            {
                pButton.transform.Find("on").gameObject.SetActive(false);
                pButton.transform.Find("empty").gameObject.SetActive(true);
            }
        }
    }

    void EnableConfirm(bool bEnable)
    {
        if(m_pLoad.enabled == bEnable) return;

        m_pLoad.transform.Find("on").gameObject.SetActive(bEnable);
        m_pLoad.transform.Find("off").gameObject.SetActive(!bEnable);
        m_pLoad.enabled = bEnable;
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "load")
        {
            int index =0;
            if(m_pSelectGameObject != null && int.TryParse(m_pSelectGameObject.name,out index))
            {
                JObject pJObject = new JObject();
                pJObject["type"] = index;
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.tactics_load,pJObject,Close);
            }
        }
        else if(sender.name == "back" )
        {
            Close();
        }
        else
        {
            bool bActive = m_pSelectGameObject != sender;
            
            if(bActive)
            {
                bActive = sender.name != GameContext.getCtx().GetActiveTacticsType().ToString();
                EnableConfirm(bActive);

                if(m_pSelectGameObject != null)
                {
                    m_pSelectGameObject.transform.Find("on/select").gameObject.SetActive(false);
                }
                
                m_pSelectGameObject = sender;
                m_pSelectGameObject.transform.Find("on/select").gameObject.SetActive(true);
            }
        }
    }
}