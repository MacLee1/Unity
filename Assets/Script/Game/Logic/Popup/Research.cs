using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ALF;

// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
using ALF.NETWORK;
using DATA;
using Newtonsoft.Json.Linq;
// using UnityEngine.EventSystems;
using STATEDATA;


public class Research : IBaseUI
{
    enum E_GENDER :byte { female,male,hide,MAX };
    MainScene m_pMainScene = null;
    TMPro.TMP_InputField m_pAgeInput = null;
    E_GENDER m_eGender = E_GENDER.hide;
    Transform[] m_eGenderList = new Transform[(int)E_GENDER.MAX];   
    RectTransform[] m_pResearchList = new RectTransform[2];
    public RectTransform MainUI { get; private set;}
    public Research(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        m_pAgeInput.onSubmit.RemoveAllListeners();
        m_pAgeInput = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Research : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Research : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;   

        m_pResearchList[0] = MainUI.Find("root/age").GetComponent<RectTransform>();
        m_pResearchList[1] = MainUI.Find("root/gender").GetComponent<RectTransform>();

        m_pAgeInput = MainUI.Find("root/age/input").GetComponent<TMPro.TMP_InputField>(); 
        m_pAgeInput.onSubmit.AddListener(delegate {  InputSumit(m_pAgeInput); });

        m_eGenderList[0] = m_pResearchList[1].Find("female/on");
        m_eGenderList[1] = m_pResearchList[1].Find("male/on");
        m_eGenderList[2] = m_pResearchList[1].Find("hide/on");
        for(int i = 0; i < m_eGenderList.Length; ++i)
        {
            m_eGenderList[i].gameObject.SetActive(false);
        }
        m_eGenderList[(int)m_eGender].gameObject.SetActive(true);

        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(m_pResearchList[0],this.ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(m_pResearchList[1],this.ButtonEventCall);
    }

    public void SetFocus()
    {
        // m_pAgeInput.text = "";
        // m_pAgeInput.OnControlClick();
        // EventSystemManager.currentSystem.SetSelectedGameObject(inputField.gameObject, null);
        // inputField.OnPointerClick (null);
        // EventSystem.current.SetSelectedGameObject(m_pAgeInput.gameObject,null);
        // m_pAgeInput.OnPointerClick(new PointerEventData(EventSystem.current));
        m_pAgeInput.ActivateInputField();
        m_pAgeInput.Select();
    }
    public void SetupResearch()
    {
        GameContext.getCtx().CreateDummyUser();
        m_pAgeInput.text = "";
        m_pResearchList[0].gameObject.SetActive(true);
        m_pResearchList[1].gameObject.SetActive(false);
        m_eGenderList[0].gameObject.SetActive(false);
        m_eGenderList[1].gameObject.SetActive(false);
        m_eGender = E_GENDER.hide;
        m_eGenderList[(int)m_eGender].gameObject.SetActive(true);
        Button pButton = m_pResearchList[0].Find("ok").GetComponent<Button>();
        pButton.enabled = false;
        pButton.transform.Find("on").gameObject.SetActive(false);
        pButton.transform.Find("off").gameObject.SetActive(true);

        // m_pMainScene.ShowDilog(m_pResearchList[0]);

        // m_pAgeInput.OnControlClick();
        // EventSystemManager.currentSystem.SetSelectedGameObject(inputField.gameObject, null);
        // inputField.OnPointerClick (null);
        // EventSystem.current.SetSelectedGameObject(m_pAgeInput.gameObject);
        // m_pAgeInput.OnPointerClick(new PointerEventData(EventSystem.current));
        // m_pAgeInput.ActivateInputField();
        // m_pAgeInput.Select();
        
    }

    void InputSumit(TMPro.TMP_InputField input )
    {
        if(input == null) return;
        
        Button pButton = m_pResearchList[0].Find("ok").GetComponent<Button>();
            
        uint age = 0;
        if(uint.TryParse(input.text,out age))
        {
            if(age > 80)
            {
                age = 80;
            }
            pButton.enabled = true;
            pButton.transform.Find("on").gameObject.SetActive(true);
            pButton.transform.Find("off").gameObject.SetActive(false);
            GameContext.getCtx().SetUserAge((byte)age);
        }
        else
        {
            m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("MSG_TXT_TRY_AGAIN"),null);
            pButton.enabled = false;
            pButton.transform.Find("on").gameObject.SetActive(false);
            pButton.transform.Find("off").gameObject.SetActive(true);            
        }
    }

    void NextStep()
    {
        m_pResearchList[0].gameObject.SetActive(false);
        m_pResearchList[1].gameObject.SetActive(true);
        m_pMainScene.ShowDilog(m_pResearchList[1]);
        m_pMainScene.HideDilog(m_pResearchList[0]);
    }

    public void Close()
    {
        
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {        
        if(sender.name == "ok" )
        {
            if(root == m_pResearchList[0])
            {
                NextStep();
            }
            else
            {
                SingleFunc.HideAnimationDailog(MainUI);
                m_pMainScene.ShowCreateClub();
            }
            return;
        }
        else if(sender.name == "no" )
        {
            if(root == m_pResearchList[0])
            {
                NextStep();
            }
            else
            {
                SingleFunc.HideAnimationDailog(MainUI);
                m_pMainScene.ShowCreateClub();
            }

            return;
        }
        
        m_eGenderList[(int)m_eGender].gameObject.SetActive(false);
        m_eGender = (E_GENDER)Enum.Parse(typeof(E_GENDER), sender.name); 
        m_eGenderList[(int)m_eGender].gameObject.SetActive(true);
        GameContext.getCtx().SetUserGender((byte)m_eGender);
    }
    
}
