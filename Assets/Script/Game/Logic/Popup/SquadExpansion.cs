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
// using UnityEngine.EventSystems;
using STATEDATA;
using CONSTVALUE;

public class SquadExpansion : IBaseUI
{
    // Start is called before the first frame update
    MainScene m_pMainScene = null;
    
    // Transform m_pTab = null;
    TMPro.TMP_Text m_pPrice = null;
    TMPro.TMP_Text m_pCount = null;
    TMPro.TMP_Text m_pCurrentCount = null;
    Button m_pExpand = null;
    Button m_pPlus = null;
    Button m_pMinus = null;
    uint m_iCurrentCount = 0;
    uint m_iCount = 0;

    public RectTransform MainUI { get; private set;}

    public SquadExpansion(){}
    
    public void Dispose()
    {
        m_pPlus = null;
        m_pMinus = null;
        m_pMainScene = null;
        MainUI = null;
        m_pPrice = null;
        m_pCount = null;
        m_pCurrentCount = null;
        m_pExpand = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "SquadExpansion : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "SquadExpansion : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        // m_pTab = MainUI.Find("root/text");
        MainUI.gameObject.SetActive(false);

        m_pPrice = MainUI.Find("root/price/icon/count").GetComponent<TMPro.TMP_Text>();
        m_pCount = MainUI.Find("root/expandSize/size/count").GetComponent<TMPro.TMP_Text>();
        m_pCurrentCount = MainUI.Find("root/current/count").GetComponent<TMPro.TMP_Text>();
        m_pExpand = MainUI.Find("root/expand").GetComponent<Button>();
        m_pPlus = MainUI.Find("root/expandSize/size/plus").GetComponent<Button>();
        m_pMinus = MainUI.Find("root/expandSize/size/minus").GetComponent<Button>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    void UpdateButton()
    {
        m_pPlus.enabled = m_iCount + m_iCurrentCount < GameContext.getCtx().GetConstValue(E_CONST_TYPE.squadSizeMax);
        m_pMinus.enabled = m_iCount > 1;
        
        m_pMinus.transform.Find("on").gameObject.SetActive(m_pMinus.enabled);
        m_pMinus.transform.Find("off").gameObject.SetActive(!m_pMinus.enabled);
        m_pPlus.transform.Find("on").gameObject.SetActive(m_pPlus.enabled);
        m_pPlus.transform.Find("off").gameObject.SetActive(!m_pPlus.enabled);
    }

    public void SetupSquadExpansionData()
    {
        m_iCurrentCount = GameContext.getCtx().GetCurrentSquadMax();
        m_iCount = 1;
        m_pCurrentCount.SetText(m_iCurrentCount.ToString());

        bool bEnable = m_iCurrentCount < GameContext.getCtx().GetTotalSquadMax();
        
        MainUI.Find("root/max").gameObject.SetActive(!bEnable);
        MainUI.Find("root/cancel").gameObject.SetActive(!bEnable);
        MainUI.Find("root/expandSize").gameObject.SetActive(bEnable);
        MainUI.Find("root/price").gameObject.SetActive(bEnable);
        m_pExpand.gameObject.SetActive(bEnable);

        if(bEnable)
        {
            UpdateButton();
            UpdatePrice();
        }
    }

    void UpdatePrice()
    {
        uint price = m_iCount * (uint)GameContext.getCtx().GetConstValue(E_CONST_TYPE.squadExpandTokenCost);
        m_pPrice.SetText(price.ToString());
        m_pCount.SetText(m_iCount.ToString());
        bool bEnable = GameContext.getCtx().GetTotalCash() >= price;
        m_pExpand.transform.Find("off").gameObject.SetActive(!bEnable);
        m_pExpand.transform.Find("on").gameObject.SetActive(bEnable);
        m_pExpand.enabled = m_pExpand;
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }
    
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "expand")
        {
            GameContext pGameContext = GameContext.getCtx();
            
            m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_SQUADEXPAND_TITLE"), string.Format(pGameContext.GetLocalizingText("DIALOG_SQUADEXPAND_TXT"),m_pPrice.text),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,()=>{
                JObject pJObject = new JObject();
                pJObject["amount"] = m_iCount;
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.club_upgradeCapacity,pJObject,()=>{
                    SingleFunc.HideAnimationDailog(MainUI);
                    m_pMainScene.UpdateSquadCount();
                }); 
            });
            return;
        }

        if(sender.name == "minus")
        {
            if(m_iCount > 1)
            {
                --m_iCount;
                UpdatePrice();
            }
            UpdateButton();

            return;
        }

        if(sender.name == "plus")
        {
            if(m_iCount + m_iCurrentCount < GameContext.getCtx().GetConstValue(E_CONST_TYPE.squadSizeMax))
            {
                ++m_iCount;
                UpdatePrice();
            }
            UpdateButton();
            return;
        }

        Close();
    }
    
}
