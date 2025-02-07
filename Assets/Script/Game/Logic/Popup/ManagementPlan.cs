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

public class ManagementPlan : IBaseUI
{
    // Start is called before the first frame update
    MainScene m_pMainScene = null;
    
    // Transform m_pTab = null;
    TMPro.TMP_Text m_pCost = null;
    TMPro.TMP_Text m_pCount = null;
    TMPro.TMP_Text m_pRewardCount = null;
    TMPro.TMP_Text m_pComment = null;
    TMPro.TMP_Text m_pTitle = null;
    RawImage m_pItemIcon = null;
    RawImage m_pCostIcon = null;
    Button m_pPlus = null;
    Button m_pMinus = null;
    Button m_pMax = null;
    ulong m_iRewardAmount = 0;
    ulong m_iCurrentCount = 0;
    uint m_iCount = 0;
    uint m_iID = 0;
    string m_pAdjustEventKey = null;
    uint m_iReward = 0;
    public RectTransform MainUI { get; private set;}

    public ManagementPlan(){}
    
    public void Dispose()
    {
        if(m_pItemIcon != null)
        {
            m_pItemIcon.texture = null;
        }
        m_pCostIcon = null;
        m_pTitle = null;
        m_pComment = null;
        m_pItemIcon = null;
        m_pMainScene = null;
        MainUI = null;
        m_pCost = null;
        m_pCount = null;
        m_pRewardCount = null;
        m_pPlus = null;
        m_pMinus = null;
        m_pMax = null;
        m_pAdjustEventKey = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "ManagementPlan : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "ManagementPlan : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        // m_pTab = MainUI.Find("root/text");
        MainUI.gameObject.SetActive(false);
        m_pItemIcon = MainUI.Find("root/reward/icon").GetComponent<RawImage>();
        m_pCostIcon = MainUI.Find("root/cost/icon").GetComponent<RawImage>();
        m_pCost = MainUI.Find("root/cost/icon/count").GetComponent<TMPro.TMP_Text>();
        m_pCount = MainUI.Find("root/amount/size/count").GetComponent<TMPro.TMP_Text>();
        m_pComment = MainUI.Find("root/comment/text").GetComponent<TMPro.TMP_Text>();
        m_pTitle = MainUI.Find("root/title").GetComponent<TMPro.TMP_Text>();
        m_pRewardCount = MainUI.Find("root/reward/count").GetComponent<TMPro.TMP_Text>();
        m_pPlus = MainUI.Find("root/amount/size/plus").GetComponent<Button>();
        m_pMinus = MainUI.Find("root/amount/size/minus").GetComponent<Button>();
        m_pMax =  MainUI.Find("root/amount/size/max").GetComponent<Button>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    void UpdateButton()
    {
        m_pPlus.enabled = m_iCount < m_iCurrentCount;
        m_pMinus.enabled = m_iCount > 1;
        m_pMinus.transform.Find("on").gameObject.SetActive(m_pMinus.enabled);
        m_pMinus.transform.Find("off").gameObject.SetActive(!m_pMinus.enabled);
        m_pPlus.transform.Find("on").gameObject.SetActive(m_pPlus.enabled);
        m_pPlus.transform.Find("off").gameObject.SetActive(!m_pPlus.enabled);
        m_pMax.enabled = m_pPlus.enabled;
        m_pMax.transform.Find("on").gameObject.SetActive(m_pMax.enabled);
        m_pMax.transform.Find("off").gameObject.SetActive(!m_pMax.enabled);
    }

    public void SetupData(SHOPPRODUCT.ShopProduct? pSProduct)
    {
        ALFUtils.Assert(pSProduct != null, "ManagementPlan:SetupData pSProduct = null!");
        
        m_pAdjustEventKey = pSProduct.Value.Analytics;
        m_iRewardAmount = pSProduct.Value.RewardAmount;
        m_iID = pSProduct.Value.No;
        m_iReward = pSProduct.Value.Reward;
        GameContext pGameContext = GameContext.getCtx();
        uint id = pSProduct.Value.Cost;
        if(id == GameContext.FREE_CASH_ID || id == GameContext.CASH_ID)
        {
            m_iCurrentCount = pGameContext.GetTotalCash();
        }
        else
        {
            m_iCurrentCount = pGameContext.GetItemCountByNO(pSProduct.Value.Cost);
        }
        
        m_iCount = 1;
        UpdateButton();
        m_pCostIcon.texture = AFPool.GetItem<Sprite>("Texture",pSProduct.Value.Cost.ToString()).texture;
        m_pTitle.SetText(pGameContext.GetLocalizingText($"item_name_{pSProduct.Value.Cost}"));
        m_pItemIcon.texture = AFPool.GetItem<Sprite>("Texture",m_iReward.ToString()).texture;

        if(m_iReward == GameContext.GAME_MONEY_ID)
        {
            m_pComment.SetText(pGameContext.GetLocalizingText("SHOP_PLANITEMBUY_TXT_TIP_MONEY_PLAN_BUY"));
        }
        else
        {    
            m_pComment.SetText(pGameContext.GetLocalizingText("SHOP_PLANITEMBUY_TXT_TIP_TRAINING_PLAN_BUY"));
        }

        UpdateAmount();
    }

    void UpdateAmount()
    {
        GameContext pGameContext = GameContext.getCtx();

        if(m_iReward == GameContext.GAME_MONEY_ID)
        {
            m_pRewardCount.SetText(ALFUtils.NumberToString(m_iRewardAmount * pGameContext.GetBusinessTotalRewardForTime() * m_iCount));
        }
        else
        {
            m_pRewardCount.SetText(ALFUtils.NumberToString(m_iCount * m_iRewardAmount * pGameContext.GetTrainingRewardForTimeByID((m_iReward - 40) + 1000)));
        }

        m_pCost.SetText(m_iCount.ToString());
        m_pCount.SetText(m_iCount.ToString());
    }

    public void Close()
    {
        m_iCount = 1;
        m_pAdjustEventKey = null;
        SingleFunc.HideAnimationDailog(MainUI);
    }
    
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "ok")
        {
            JObject pJObject = new JObject();
            pJObject["no"] = m_iID;
            pJObject["amount"] = m_iCount;
            if(!string.IsNullOrEmpty(m_pAdjustEventKey))
            {
                GameContext.getCtx().SendAdjustEvent(m_pAdjustEventKey,false, false,-1);
            }
            m_pAdjustEventKey = null;
            m_pMainScene.RequestAfterCall(E_REQUEST_ID.shop_buy,pJObject,Close);
            return;
        }
        
        if(sender.name == "minus")
        {
            if(m_iCount > 1)
            {
                --m_iCount;
                UpdateAmount();
            }
            UpdateButton();
            return;
        }
        if(sender.name == "plus")
        {
            if(m_iCount < m_iCurrentCount)
            {
                ++m_iCount;
                UpdateAmount();
            }
            UpdateButton();
            return;
        }
        if(sender.name == "max")
        {
            if(m_iCount != m_iCurrentCount)
            {
                m_iCount = (uint)m_iCurrentCount;
                UpdateAmount();
                UpdateButton();
            }
            return;
        }

        Close();
    }
    
}
