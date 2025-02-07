using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using USERDATA;
using ALF.NETWORK;
// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
using Newtonsoft.Json.Linq;
using ALF.SOUND;
using DATA;
using REWARDSET;
using STATEDATA;


public class MonthlyPackage : IBaseUI
{
    MainScene m_pMainScene = null;   
    float m_fTimeRefreshTime = 1;

    bool m_bRefreshUpdate = false;
    bool m_bExpireUpdate = false;
    uint m_iPackageNo = 0;

    TMPro.TMP_Text m_pPurchaseText = null;
    TMPro.TMP_Text m_pExpire = null;
    TMPro.TMP_Text m_pTitle = null;
    TMPro.TMP_Text m_pNextTimeText = null;
    
    GameObject m_pActiveNode = null;
    GameObject[] m_pItemOnList = new GameObject[2];
    TMPro.TMP_Text[] m_pItemInfoTextList = new TMPro.TMP_Text[2];
    RawImage m_pIcon = null;
    
    string m_pSku = null;
    

    public RectTransform MainUI { get; private set;}
    public MonthlyPackage(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        m_pPurchaseText = null;
        m_pExpire = null;
        m_pTitle = null;
        m_pSku = null;
        m_pActiveNode = null;
        m_pNextTimeText = null;
        
        for(int i =0; i < m_pItemOnList.Length; ++i)
        {
            m_pItemOnList[i] = null;
            m_pItemInfoTextList[i] = null;
        }
        m_pItemOnList = null;
        m_pItemInfoTextList = null;
        m_pIcon.texture = null;
        m_pIcon = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "MonthlyPackage : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "MonthlyPackage : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pTitle = MainUI.Find("root/title/text").GetComponent<TMPro.TMP_Text>();
        m_pPurchaseText = MainUI.Find("root/purchase/price").GetComponent<TMPro.TMP_Text>();
        m_pExpire = MainUI.Find("root/active/text").GetComponent<TMPro.TMP_Text>();

        m_pNextTimeText = MainUI.Find("root/next/text").GetComponent<TMPro.TMP_Text>();
        m_pActiveNode = MainUI.Find("root/active").gameObject;

        m_pItemOnList[0] = MainUI.Find("root/item/1/on").gameObject;
        m_pItemOnList[1] = MainUI.Find("root/item/2/on").gameObject;
        m_pItemInfoTextList[0] = MainUI.Find("root/item/1/text").GetComponent<TMPro.TMP_Text>();
        m_pItemInfoTextList[1] = MainUI.Find("root/item/2/text").GetComponent<TMPro.TMP_Text>();
        m_pIcon = MainUI.Find("root/info/icon").GetComponent<RawImage>();

        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupData(SHOPPRODUCT.ShopProduct? data)
    {
        GameContext pGameContext = GameContext.getCtx();

        DateTime serverTime = NetworkManager.GetGameServerTime();
        if(serverTime < DateTime.Parse(data.Value.TStart) && serverTime >= DateTime.Parse(data.Value.TEnd))
        {
            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingErrorMsg("1803"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"),Close);
            return;
        }
        
        m_pSku = data.Value.Sku;
        m_iPackageNo = data.Value.No;
        m_pTitle.SetText(pGameContext.GetLocalizingText(data.Value.Title));
        
        UpdateProductFlatRateData();

        SHOPPRODUCT.ShopProductList pShopProductList = pGameContext.GetFlatBufferData<SHOPPRODUCT.ShopProductList>(E_DATA_TYPE.ShopProduct);
        SHOPPRODUCT.ShopList? pShopProduct = pShopProductList.ShopProductByKey((uint)E_SHOP_ITEM.ShopMonthlyItem);
        SHOPPRODUCT.ShopProduct? pSProduct = pShopProduct.Value.ListByKey(m_iPackageNo);

        pGameContext.UpdateCurrencyPrice(m_pPurchaseText,data.Value.Sku,data.Value.CostAmount.ToString());

        m_pIcon.gameObject.SetActive(!string.IsNullOrEmpty(data.Value.Icon));
        if(m_pIcon.gameObject.activeSelf)
        {
            m_pIcon.texture = AFPool.GetItem<Sprite>("Texture",data.Value.Icon).texture;
            m_pIcon.SetNativeSize();
        }
        
        CONSTVALUE.E_CONST_TYPE eConstType = CONSTVALUE.E_CONST_TYPE.MAX;
        if(Enum.TryParse<CONSTVALUE.E_CONST_TYPE>($"tokenTicketDailyReward_{m_iPackageNo}",out eConstType))
        {
            m_pItemInfoTextList[0].SetText(string.Format(pGameContext.GetLocalizingText("SHOP_TOKENTICKET_TXT_REWARD_1"), pSProduct.Value.RewardAmount));
            m_pItemInfoTextList[1].SetText(string.Format(pGameContext.GetLocalizingText("SHOP_TOKENTICKET_TXT_REWARD_2"), pGameContext.GetConstValue(eConstType),pSProduct.Value.Duration));
        }
    }

    void UpdateProductFlatRateData()
    {
        GameContext pGameContext = GameContext.getCtx();
        ProductFlatRateT pProductFlatRate = pGameContext.GetProductFlatRateByProduct(m_iPackageNo);

        if(pProductFlatRate != null)
        {
            m_pPurchaseText.transform.parent.gameObject.SetActive(false);
            m_pItemOnList[0].SetActive(true);
            m_pItemOnList[1].SetActive(pProductFlatRate.Day >= pProductFlatRate.Max);
            m_pActiveNode.SetActive(!m_pItemOnList[1].activeSelf);
            
            if(m_pActiveNode.activeSelf)
            {    
                m_pExpire.SetText(string.Format(pGameContext.GetLocalizingText("SHOP_TXT_TOKEN_TICKET_LEFT_TIME_END"),pProductFlatRate.Max - pProductFlatRate.Day));
            }
            
            m_pNextTimeText.transform.parent.gameObject.SetActive(m_pItemOnList[1].activeSelf);
            if(m_pItemOnList[1].activeSelf)
            {
                m_bRefreshUpdate = true;
                TimeSpan pTimeSpan = new DateTime(pProductFlatRate.TRefresh) - NetworkManager.GetGameServerTime();
                m_fTimeRefreshTime = (float)((pTimeSpan.TotalMilliseconds - pTimeSpan.TotalSeconds) / TimeSpan.TicksPerMillisecond);
                SingleFunc.UpdateTimeText((int)pTimeSpan.TotalSeconds,m_pNextTimeText,0);
            }
        }
        else
        {
            m_pActiveNode.SetActive(false);
            m_pPurchaseText.transform.parent.gameObject.SetActive(true);
            m_pItemOnList[0].SetActive(false);
            m_pItemOnList[1].SetActive(false);
            m_pNextTimeText.transform.parent.gameObject.SetActive(false);
        }
    }

    public void UpdateTimer(float dt)
    {
        if(m_bRefreshUpdate)
        {
            m_fTimeRefreshTime -= dt;
            if(m_fTimeRefreshTime <= 0)
            {
                while(m_fTimeRefreshTime <= -1)
                {
                    m_fTimeRefreshTime += 1;
                }
                
                m_fTimeRefreshTime = 1 - m_fTimeRefreshTime;

                ProductFlatRateT pProductFlatRate = GameContext.getCtx().GetProductFlatRateByProduct(m_iPackageNo);
                TimeSpan pTimeSpan = new DateTime(pProductFlatRate.TRefresh) - NetworkManager.GetGameServerTime();
                if(pTimeSpan.TotalSeconds > 0)
                {
                    SingleFunc.UpdateTimeText((int)pTimeSpan.TotalSeconds,m_pNextTimeText,0);
                }
                else
                {
                    m_fTimeRefreshTime = 0;
                    m_bRefreshUpdate = false;
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.shop_get,null,UpdateProductFlatRateData);
                }
            }
        }
    }

    public void Purchase(bool bOk)
    {
        if(bOk)
        {
            GameContext.getCtx().Purchase(m_pSku);
        }
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
        m_pSku = null;
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "purchase")
        {
            m_pMainScene.GetInstance<Store>().PurchaseReserve(m_pSku,0);
            return;
        } 

        Close();
    }

}
