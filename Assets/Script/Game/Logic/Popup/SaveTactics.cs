using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.LAYOUT;
using USERDATA;
using CONSTVALUE;
using DATA;

public class SaveTactics : IBaseUI
{
    GameObject m_pSelectGameObject = null;
    MainScene m_pMainScene = null;
    Button[] m_pSlotList = null;
    Button m_pSave = null;
    TMPro.TMP_InputField m_pNameInput = null;
    public RectTransform MainUI { get; private set;}
    public SaveTactics(){}
    
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
        m_pSave = null;
        m_pSelectGameObject = null;
        m_pNameInput.onValueChanged.RemoveAllListeners();
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "SaveTactics : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "SaveTactics : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pSave = MainUI.Find("root/save").GetComponent<Button>();
        m_pSlotList = MainUI.Find("root/bg").GetComponentsInChildren<Button>(true);
        MainUI.gameObject.SetActive(false);
        
        m_pNameInput = MainUI.Find("root/name").GetComponent<TMPro.TMP_InputField>();
        m_pNameInput.characterLimit = GameContext.getCtx().GetConstValue(E_CONST_TYPE.clubNameLengthLimit) +1;
        m_pNameInput.onValueChanged.AddListener(delegate {  InputSumit(m_pNameInput); });

        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    void InputSumit(TMPro.TMP_InputField input )
    {
        if(input == null) return;

        // if(SingleFunc.IsMatchString(input.text,@"[\'\""\/\`\\]"))
        // {
        //     m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("MSG_TXT_STRING_ERROR"),null);
        //     input.text = "";
        //     return;
        // }
        input.text = ALFUtils.FwordFilter(input.text);
        
        if(input.characterLimit <= input.text.Length )
        {
            m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("MSG_TXT_TRY_AGAIN"),null);
            input.text = "";
            return;
        }
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
            index = -1;
            pButton = m_pSlotList[i];
            
            for(n =0; n < list.Count; ++n)
            {
                if(list[n].Type.ToString() == pButton.gameObject.name)
                {
                    index = n;
                    break;
                }
            }
            
            if(index > -1)
            {
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
            pButton.transform.Find("select").gameObject.SetActive(false);
        }
    }

    void EnableConfirm(bool bEnable)
    {
        if(m_pSave.enabled == bEnable) return;

        m_pSave.transform.Find("on").gameObject.SetActive(bEnable);
        m_pSave.transform.Find("off").gameObject.SetActive(!bEnable);
        m_pSave.enabled = bEnable;
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "save")
        {
            int index =0;
            if(m_pSelectGameObject != null && int.TryParse(m_pSelectGameObject.name,out index))
            {
                // if(SingleFunc.IsMatchString(m_pNameInput.text,@"[\'\""\/\`\\]"))
                // {
                //     m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("MSG_TXT_STRING_ERROR"),null);
                //     m_pNameInput.text = "";
                //     return;
                // }

                GameContext.getCtx().SaveTacticsInfoName(index, m_pNameInput.text);
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.tactics_put,m_pMainScene.MakeTacticsData(index),Close);
            }
        }
        else if(sender.name == "back")
        {
            Close();
        }
        else
        {
            EnableConfirm(true);
            if(m_pSelectGameObject == sender) return;

            if(m_pSelectGameObject != null)
            {
                m_pSelectGameObject.transform.Find("select").gameObject.SetActive(false);
            }
            
            m_pSelectGameObject = sender;
            m_pSelectGameObject.transform.Find("select").gameObject.SetActive(true);
        }
    }
}
