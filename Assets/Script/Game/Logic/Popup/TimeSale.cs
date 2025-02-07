using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using USERDATA;
using ALF.NETWORK;
// using ALF.STATE;
// using ALF.CONDITION;
using TIMESALE;
using ALF.LAYOUT;
using Newtonsoft.Json.Linq;
using ALF.SOUND;
using DATA;
using REWARDSET;
using STATEDATA;


public class TimeSale : IBaseUI
{
    const string REWARD_ITEM_NAME = "RewardItem";

    const float CONST_NORMAL_TIME = 300.0f;

    const uint TIME_SALE_SHOP = 10000;
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    bool m_bExpireUpdate = false;
    float m_fTimeRefreshTime = 1;
    
    TMPro.TMP_Text m_pPurchaseText = null;
    TMPro.TMP_Text m_pExpire = null;
    TMPro.TMP_Text m_pTitle = null;
    string m_pSku = null;
    RawImage m_pIcon = null;
    GameObject m_pPurchaseOff = null;

    TMPro.TMP_Text m_pBonusRateText = null;

    Button m_pPurchase = null;
    Button m_pLeft = null;
    Button m_pRight = null;

    int m_iCurrentIndex = 0;

    uint m_iCurrentTimeSaleNo =0;

    GameObject m_pLeftOn = null;
    GameObject m_pRightOn = null;

    List<TimeSaleT> m_pProductTimeSaleInfo = new List<TimeSaleT>();

    // List<ShopPackage.PackageItem> m_pPackageItemList = new List<ShopPackage.PackageItem>();
    
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value; }}
    public TimeSale(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        if(m_pScrollRect != null)
        {
            ClearScroll();
        }
        m_pBonusRateText = null;
        m_pProductTimeSaleInfo.Clear();
        m_pProductTimeSaleInfo = null;
        m_pPurchaseOff = null;
        m_pIcon.texture = null;
        m_pIcon = null;
        m_pScrollRect = null;
        m_pPurchaseText = null;
        m_pExpire = null;
        m_pTitle = null;
        m_pSku = null;
        m_pLeft = null;
        m_pRight = null;

        m_pLeftOn = null;
        m_pRightOn = null;
        m_pPurchase = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "TimeSale : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "TimeSale : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pTitle = MainUI.Find("root/info/title/text").GetComponent<TMPro.TMP_Text>();
        m_pIcon = MainUI.Find("root/info/icon").GetComponent<RawImage>();
        m_pPurchase = MainUI.Find("root/purchase").GetComponent<Button>();
        m_pPurchaseText = m_pPurchase.transform.Find("price").GetComponent<TMPro.TMP_Text>();
        
        m_pPurchaseOff = m_pPurchase.transform.Find("off").gameObject;
        
        m_pExpire = MainUI.Find("root/time/text").GetComponent<TMPro.TMP_Text>();

        m_pBonusRateText = MainUI.Find("root/info/tag/text").GetComponent<TMPro.TMP_Text>();
    
        m_pLeft = MainUI.Find("root/left").GetComponent<Button>();
        m_pRight = MainUI.Find("root/right").GetComponent<Button>();

        m_pLeftOn = m_pLeft.transform.Find("on").gameObject;
        m_pRightOn = m_pRight.transform.Find("on").gameObject;
        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupData(uint no)
    {
        m_pProductTimeSaleInfo.Clear();
        m_pProductTimeSaleInfo = GameContext.getCtx().GetTimeSaleProduct();
        if(m_pProductTimeSaleInfo.Count == 0)
        {
            Close();
            return;
        }
        m_iCurrentIndex = 0;
        if(no > 0)
        {
            for(int i =0; i < m_pProductTimeSaleInfo.Count; ++i)
            {
                if(m_pProductTimeSaleInfo[i].No == no)
                {
                    m_iCurrentIndex = i;
                    break;
                }
            }
        }

        SetupScroll(m_iCurrentIndex);
    }

    void UpdateSideButton()
    {
        if(m_pProductTimeSaleInfo.Count > 1)
        {
            m_pLeft.gameObject.SetActive(true);
            m_pRight.gameObject.SetActive(true);
            m_pLeft.enabled = m_iCurrentIndex > 0;
            m_pRight.enabled = m_iCurrentIndex < m_pProductTimeSaleInfo.Count -1;
            m_pLeftOn.SetActive(m_pLeft.enabled); 
            m_pRightOn.SetActive(m_pRight.enabled);
        }
        else
        {
            m_pLeft.gameObject.SetActive(false);
            m_pRight.gameObject.SetActive(false);
            m_pLeftOn.SetActive(false); 
            m_pRightOn.SetActive(false);
        }
    }

    void SetupScroll(int index)
    {
        m_iCurrentIndex = index;
        ClearScroll();
        
        UpdateSideButton();

        GameContext pGameContext = GameContext.getCtx();

        TimeSaleList pTimeSaleList = pGameContext.GetFlatBufferData<TimeSaleList>(E_DATA_TYPE.TimeSale);
        TimeSaleItem? pTimeSaleItem = pTimeSaleList.TimeSaleByKey(m_pProductTimeSaleInfo[m_iCurrentIndex].No);
        m_iCurrentTimeSaleNo = pTimeSaleItem.Value.No;
        SHOPPRODUCT.ShopProductList pShopProductList = pGameContext.GetFlatBufferData<SHOPPRODUCT.ShopProductList>(E_DATA_TYPE.ShopProduct);
        SHOPPRODUCT.ShopList? pShopList = pShopProductList.ShopProductByKey(TIME_SALE_SHOP);

        SHOPPRODUCT.ShopProduct? data = pShopList.Value.ListByKey(pTimeSaleItem.Value.Product);

        if(data.Value.BonusRate > 0)
        {   
            m_pBonusRateText.transform.parent.gameObject.SetActive(true);
            m_pBonusRateText.SetText($"+{data.Value.BonusRate}%");
        }
        else
        {
            m_pBonusRateText.transform.parent.gameObject.SetActive(false);
        }
        
        m_pSku = data.Value.Sku;
        
        pGameContext.UpdateCurrencyPrice(m_pPurchaseText,data.Value.Sku,data.Value.CostAmount.ToString());

        m_pIcon.gameObject.SetActive(!string.IsNullOrEmpty(data.Value.Icon));
        if(m_pIcon.gameObject.activeSelf)
        {
            m_pIcon.texture = AFPool.GetItem<Sprite>("Texture",data.Value.Icon).texture;
            m_pIcon.SetNativeSize();
        }

        m_pTitle.SetText(pGameContext.GetLocalizingText(data.Value.Title));
 
        m_bExpireUpdate = m_pProductTimeSaleInfo[m_iCurrentIndex].TExpire > 0;

        m_pPurchaseOff.SetActive(!m_bExpireUpdate);
        
        uint no = 0;
        Dictionary<uint,ulong> list = new Dictionary<uint, ulong>();

        if(data.Value.RewardType == GameContext.REWARD_SET)
        {
            RewardSetList pRewardSetList = pGameContext.GetFlatBufferData<RewardSetList>(E_DATA_TYPE.RewardSet);
            Rewards? pRewards = pRewardSetList.RewardSetByKey(data.Value.Reward);
            if(pRewards != null)
            {
                RewardSetItem? pRewardSetItem = null;
                for(int n =0; n < pRewards.Value.ListLength; ++n)
                {
                    pRewardSetItem = pRewards.Value.List(n);
                    no = pRewardSetItem.Value.Item;
                    if(no == GameContext.FREE_CASH_ID)
                    {
                        no = GameContext.CASH_ID;
                    }

                    if(list.ContainsKey(no))
                    {
                        list[no] += pRewardSetItem.Value.Amount * data.Value.RewardAmount;
                    }
                    else
                    {
                        list.Add(no,pRewardSetItem.Value.Amount * data.Value.RewardAmount);
                    }
                }
            }
        }
        else
        {
            no = data.Value.Reward;
            if(no == GameContext.FREE_CASH_ID)
            {
                no = GameContext.CASH_ID;
            }

            if(list.ContainsKey(no))
            {
                list[no] += data.Value.RewardAmount;
            }
            else
            {
                list.Add(no,data.Value.RewardAmount);
            }
        }

        RectTransform pItem = null;
        Vector2 size;
        float w = 0;
        float width =0;
        var itr = list.GetEnumerator();
        
        while(itr.MoveNext())
        {
            pItem = SingleFunc.GetRewardIcon(m_pScrollRect.content,REWARD_ITEM_NAME,itr.Current.Key,itr.Current.Value);
    
            if(pItem)
            {
                width = pItem.rect.width;
                pItem.localScale = Vector3.one;
                size = pItem.anchoredPosition;
                size.x = w;
                pItem.anchoredPosition = size;
                w += width + 10;
            }
        }

        size = m_pScrollRect.content.sizeDelta;
        size.x = w;
        m_pScrollRect.content.sizeDelta = size;

        w = (w * -0.5f) + (width * 0.5f);
    
        for(int i = 0; i < m_pScrollRect.content.childCount; ++i)
        {
            pItem = m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>();
            size = pItem.anchoredPosition;
            size.x += w;
            pItem.anchoredPosition = size;
        }
        m_pScrollRect.horizontalNormalizedPosition = 0;
        m_pPurchase.enabled = m_bExpireUpdate;
        
        if(m_bExpireUpdate)
        {
            SingleFunc.UpdateTimeText((int)m_pProductTimeSaleInfo[m_iCurrentIndex].TExpire,m_pExpire,0);
            if(CONST_NORMAL_TIME < m_pProductTimeSaleInfo[m_iCurrentIndex].TExpire)
            {
                m_pExpire.color = Color.white;
            }
            else
            {
                m_pExpire.color = Color.red;
            }
        }
        else
        {
            m_pExpire.SetText("00:00:00");
            m_pExpire.color = GameContext.GRAY;
        }
    }

    void ClearScroll()
    {
        RectTransform item = null;
        int i = m_pScrollRect.content.childCount;
        while(i > 0)
        {
            --i;
            item = m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>();
            SingleFunc.AddRewardIcon(item,REWARD_ITEM_NAME);
        }
        
        m_pScrollRect.horizontalNormalizedPosition = 0;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.x =0;
        m_pScrollRect.content.sizeDelta = size;
    }

    public void UpdateTimer(float dt)
    {
        if(m_bExpireUpdate && m_pProductTimeSaleInfo.Count > 0)
        {
            if(m_pProductTimeSaleInfo[m_iCurrentIndex].TExpire <= 0)
            {
                m_bExpireUpdate = false;
            }
            
            if(m_bExpireUpdate)
            {
                m_fTimeRefreshTime -= dt;
                
                if(m_fTimeRefreshTime <= 0)
                {
                    while(m_fTimeRefreshTime <= -1)
                    {
                        m_fTimeRefreshTime += 1;
                    }
                    
                    m_fTimeRefreshTime = 1 - m_fTimeRefreshTime;

                    if(CONST_NORMAL_TIME < m_pProductTimeSaleInfo[m_iCurrentIndex].TExpire)
                    {
                        m_pExpire.color = Color.white;
                    }
                    else
                    {
                        m_pExpire.color = Color.red;
                    }

                    SingleFunc.UpdateTimeText((int)m_pProductTimeSaleInfo[m_iCurrentIndex].TExpire,m_pExpire,0);
                }
            }
            else
            {
                m_pExpire.SetText("00:00:00");
                m_pExpire.color = GameContext.GRAY;
                m_pPurchaseOff.SetActive(true);
                m_pPurchase.enabled = false;
            }
        }
    }

    public uint GetTimeSaleNo()
    {
        return m_iCurrentTimeSaleNo;
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
        Enable = false;
        m_pSku = null;
        m_iCurrentTimeSaleNo = 0;
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "purchase")
        {
            if(Application.isEditor)
            {
                JObject pJObject = new JObject();
                pJObject["sku"] = m_pSku;
                pJObject["timeSale"] = m_iCurrentTimeSaleNo;
                pJObject["store"] = "hive";
            
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.iap_purchase,pJObject);
            }
            else
            {
                m_pMainScene.GetInstance<Store>().PurchaseReserve(m_pSku,m_iCurrentTimeSaleNo);
            }

            return;
        }
        else if(sender.name == "left")
        {
            --m_iCurrentIndex;
            if(m_iCurrentIndex < 0) m_iCurrentIndex = 0;

            SetupScroll(m_iCurrentIndex);
            return;
        }
        else if(sender.name == "right")
        {
            ++m_iCurrentIndex;
            if(m_iCurrentIndex > m_pProductTimeSaleInfo.Count) m_iCurrentIndex = m_pProductTimeSaleInfo.Count -1;
            SetupScroll(m_iCurrentIndex);
            return;
        }

        Close();
    }

}
