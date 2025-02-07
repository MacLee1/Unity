using System;
// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using ALF;
// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
using ALF.NETWORK;
using DATA;
using USERDATA;
// using UnityEngine.EventSystems;
using STATEDATA;
using Newtonsoft.Json.Linq;
using SHOP;
using SHOPPRODUCT;

public class Store : IBaseUI,IBaseNetwork
{
    enum E_TAB : byte { special = 1,free,currency,ticket,MAX }

    uint m_iProductNo = 0;
    
    float m_fTimeRefreshTime = 1;
    bool m_bRefreshUpdate = false;
    bool m_bSnapChange = false;
    string m_pCurrentPurchaseSKU = null;
    string  m_pAdjustEventKey = null;
    
    Dictionary<ProductFlatRateT,TMPro.TMP_Text> m_pProductFlatRate = new Dictionary<ProductFlatRateT,TMPro.TMP_Text>();
    Transform[] m_pNaviNodeList = new Transform[(int)E_TAB.MAX];
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    TMPro.TMP_Text[] m_pItems = new TMPro.TMP_Text[2];
    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    public Store(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        if(m_pScrollRect != null)
        {
            m_pScrollRect.onValueChanged.RemoveAllListeners();
            ClearScroll();
        }

        int i =0;
        for(i = 0; i < m_pNaviNodeList.Length; ++i)
        {
            m_pNaviNodeList[i] = null;
        }
        m_pNaviNodeList= null;
        m_pScrollRect = null;
        for(i = 0; i < m_pItems.Length; ++i)
        {
            m_pItems[i] = null;
        }
        m_pItems = null;
       
        m_pProductFlatRate = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Store : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Store : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        
        Transform ui = MainUI.Find("root/tabs");
        int iTabIndex = -1;
        Transform item = null;

        for(int n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n);
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pNaviNodeList[iTabIndex] = item;
        }

        MainUI.gameObject.SetActive(false);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        m_pScrollRect.onValueChanged.AddListener( this.ScrollViewChangeValueEventCall);
        
        m_pItems[0] = m_pScrollRect.content.Find("currency/ShopManageItem/item/1300/text").GetComponent<TMPro.TMP_Text>();
        m_pItems[1] = m_pScrollRect.content.Find("currency/ShopManageItem/item/1301/text").GetComponent<TMPro.TMP_Text>();

        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void SetScroll(byte type,string name = null)
    {
        m_bSnapChange = true;
        m_pScrollRect.velocity = Vector2.zero;
        E_TAB eType = (E_TAB)type;
        m_pScrollRect.content.Find(eType.ToString());
        RectTransform item = m_pScrollRect.content.Find(eType.ToString()).GetComponent<RectTransform>();
        if(name != null)
        {
            if(item.Find(name)!= null)
            {
                item = item.Find(name).GetComponent<RectTransform>();
            }
        }

        m_pScrollRect.SnapTo(item);
        
        Button pButton = null;
        for(int i =1; i < m_pNaviNodeList.Length; ++i)
        {
            pButton = m_pNaviNodeList[i].GetComponent<Button>();
            pButton.enabled = i != type;
            m_pNaviNodeList[i].Find("on").gameObject.SetActive(!pButton.enabled);
        }
    }

    void UpdateNaviNodeList()
    {
        float x = -50;
        int i =m_pNaviNodeList.Length;
        RectTransform tm = null;
        Vector2 pos;
        while(i > 1)
        {
            --i;
            if(m_pNaviNodeList[i].gameObject.activeSelf)
            {
                tm = (RectTransform)m_pNaviNodeList[i];
                pos = tm.anchoredPosition;
                pos.x = x;
                tm.anchoredPosition = pos;
                x -= tm.rect.width + 60;
            }
        }
    }

    public void SetupScroll(bool bReset)
    {
        Vector2 pos = m_pScrollRect.content.anchoredPosition;

        ClearScroll();

        GameContext pGameContext = GameContext.getCtx();
        SHOP.ShopList pShopList = pGameContext.GetFlatBufferData<SHOP.ShopList>(E_DATA_TYPE.Shop);
        SHOPPRODUCT.ShopProductList pShopProductList = pGameContext.GetFlatBufferData<SHOPPRODUCT.ShopProductList>(E_DATA_TYPE.ShopProduct);
        SHOP.ShopItem? pShopItem = null;
        SHOP.Shop? pShop = null;
        SHOPPRODUCT.ShopList? pShopProduct = null;
        SHOPPRODUCT.ShopProduct? pSProduct = null;
        
        TMPro.TMP_Text text = null;
        RectTransform pCategory = null;
        RectTransform pSubCategory = null;
        RectTransform pItems = null;
        
        RectTransform pItem = null;
        Vector2 size;
        float h = 0;
        float subH = 0;
        int n = 0;
        
        E_TAB eTab = E_TAB.MAX;
        E_SHOP_ITEM eItme = E_SHOP_ITEM.MAX;
        RawImage icon = null;
        
        long startT;
        long endT;
        long serverT = NetworkManager.GetGameServerTime().Ticks;

        int i =0;
        int t =0;
        int count =0;
        int totalCount =0;
        for(i =1; i < m_pNaviNodeList.Length; ++i)
        {
            m_pNaviNodeList[i].gameObject.SetActive(true);
            m_pNaviNodeList[i].Find("on").gameObject.SetActive(false);
        }

        for(i =0; i < m_pScrollRect.content.childCount; ++i)
        {
            pItem = m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>();
            pItem.gameObject.SetActive(true);
            for(count =0; count < pItem.childCount; ++count)
            {
                pItem.GetChild(count).gameObject.SetActive(true); 
            }
            size = pItem.anchoredPosition;
            size.y =0;
            pItem.anchoredPosition = size;
        }
        count =0;
        m_bRefreshUpdate = true;
        DateTime serverTime = NetworkManager.GetGameServerTime();
        bool bActive = false;
        for(i =0; i < pShopList.ShopLength; ++i)
        {
            pShopItem = pShopList.Shop(i);
            eTab = (E_TAB)pShopItem.Value.Category;
            
            pCategory = m_pScrollRect.content.Find(eTab.ToString()).GetComponent<RectTransform>();
            if(pCategory)
            {
                bActive = false;
                size = pCategory.anchoredPosition;
                size.y = -h;
                pCategory.anchoredPosition = size;
                subH = 0;
                
                for(n =0; n < pShopItem.Value.ListLength; ++n)
                {
                    pShop = pShopItem.Value.List(n);
                    startT = DateTime.Parse(pShop.Value.TStart).Ticks;
                    endT = DateTime.Parse(pShop.Value.TEnd).Ticks;
                    eItme = (E_SHOP_ITEM)pShop.Value.No;
                    
                    pSubCategory = pCategory.Find(eItme.ToString()).GetComponent<RectTransform>();
                    text = pSubCategory.Find("icon/title").GetComponent<TMPro.TMP_Text>();
                    text.SetText(pGameContext.GetLocalizingText(pShop.Value.Title));
                    pSubCategory.gameObject.SetActive(startT <= serverT && endT > serverT);
                    totalCount =0;
                    
                    if(pSubCategory.gameObject.activeSelf)
                    {
                        pShopProduct = pShopProductList.ShopProductByKey(pShop.Value.No);
                        size = pSubCategory.anchoredPosition;
                        size.y = -subH;
                        pSubCategory.anchoredPosition = size;
                        pItems = pSubCategory.Find("items").GetComponent<RectTransform>();
                        size = pItems.anchoredPosition;
                        size.y = -pSubCategory.rect.height;
                        pItems.anchoredPosition = size;
                        GridLayoutGroup pGridLayoutGroup = pItems.GetComponent<GridLayoutGroup>();
                        
                        for(t =0; t < pShopProduct.Value.ListLength; ++t)
                        {
                            pSProduct = pShopProduct.Value.List(t);
                            count = (int)pSProduct.Value.LimitAmount;

                            ProductMileageT pProductMileage = pGameContext.GetProductMileageData(pSProduct.Value.No);
                            if(pProductMileage != null)
                            {
                                count -= (int)pProductMileage.Amount;
                                
                                if(count < 1 )
                                {
                                    if(pSProduct.Value.Reset == 0 || pSProduct.Value.Reset == 99) continue;

                                    count = 0;
                                }
                            }

                            startT = DateTime.Parse(pSProduct.Value.TStart).Ticks;
                            endT = DateTime.Parse(pSProduct.Value.TEnd).Ticks;
                            
                            if(startT > serverT || endT <= serverT) continue;
                            
                            pItem = LayoutManager.Instance.GetItem<RectTransform>(eItme.ToString());
                            if(pItem)
                            {
                                ++totalCount;
                                pItem.gameObject.name = pSProduct.Value.No.ToString();
                                if(pItem.Find("bg/title") != null)
                                {
                                    text = pItem.Find("bg/title").GetComponent<TMPro.TMP_Text>();
                                    text.SetText(pGameContext.GetLocalizingText(pSProduct.Value.Title));
                                }

                                pItem.localScale = Vector3.one;
                                pItem.SetParent(pItems,false);

                                if(pItem.Find("bg/icon") != null)
                                {
                                    icon = pItem.Find("bg/icon").GetComponent<RawImage>();
                                    icon.gameObject.SetActive(!string.IsNullOrEmpty(pSProduct.Value.Icon));
                                    if(icon.gameObject.activeSelf)
                                    {
                                        icon.texture = AFPool.GetItem<Sprite>("Texture",pSProduct.Value.Icon).texture;
                                        icon.SetNativeSize();
                                    }
                                }

                                if(!string.IsNullOrEmpty(pSProduct.Value.Banner))
                                {
                                    icon = pItem.Find("bg").GetComponent<RawImage>();
                                    icon.texture = AFPool.GetItem<Sprite>("Texture",pSProduct.Value.Banner).texture;
                                    icon.SetNativeSize();
                                }

                                if(pSProduct.Value.Cost == GameContext.PRODUCT_CASH_ID)
                                {
                                    if(eItme == E_SHOP_ITEM.ShopSpecialItem )
                                    {
                                        pItem.GetComponent<Button>().enabled = true;
                                        text = pItem.Find("bg/price").GetComponent<TMPro.TMP_Text>();
                                        pGameContext.UpdateCurrencyPrice(text,pSProduct.Value.Sku,pSProduct.Value.CostAmount.ToString());

                                        text = pItem.Find("bg/orgin").GetComponent<TMPro.TMP_Text>();
                                        if(pSProduct.Value.BonusRate > 0)
                                        {   
                                            text.gameObject.SetActive(true);
                                            pGameContext.UpdateCurrencyBonusPrice(text, pSProduct.Value.Sku, (pSProduct.Value.BonusRate / 100.0f));
                                            pItem.Find("bg/tag").gameObject.SetActive(true);
                                            text = pItem.Find("bg/tag/text").GetComponent<TMPro.TMP_Text>();
                                            text.SetText($"+{pSProduct.Value.BonusRate}%");
                                        }
                                        else
                                        {
                                            text.gameObject.SetActive(false);
                                            pItem.Find("bg/tag").gameObject.SetActive(false);
                                        }
                                    }
                                    else if(eItme == E_SHOP_ITEM.ShopLimitedItem)
                                    {
                                        pItem.GetComponent<Button>().enabled = true;
                                        text = pItem.Find("bg/price").GetComponent<TMPro.TMP_Text>();
                                        pGameContext.UpdateCurrencyPrice(text,pSProduct.Value.Sku,pSProduct.Value.CostAmount.ToString());
                                        
                                        text = pItem.Find("bg/limit").GetComponent<TMPro.TMP_Text>();
                                        if(pSProduct.Value.LimitAmount > 0)
                                        {
                                            text.gameObject.SetActive(true);
                                            text.SetText(string.Format(pGameContext.GetLocalizingText("SHOP_TXT_PURCHASE_CHANCE"), count));
                                        }
                                        else
                                        {
                                            text.gameObject.SetActive(false);
                                        }
                                        
                                        text = pItem.Find("bg/orgin").GetComponent<TMPro.TMP_Text>();
                                        
                                        if(pSProduct.Value.BonusRate > 0)
                                        {
                                            text.gameObject.SetActive(true);
                                            pGameContext.UpdateCurrencyBonusPrice(text, pSProduct.Value.Sku, (pSProduct.Value.BonusRate / 100.0f));
                                            
                                            pItem.Find("bg/tag").gameObject.SetActive(true);
                                            text = pItem.Find("bg/tag/text").GetComponent<TMPro.TMP_Text>();
                                            text.SetText($"+{pSProduct.Value.BonusRate}%");
                                        }
                                        else
                                        {
                                            text.gameObject.SetActive(false);
                                            pItem.Find("bg/tag").gameObject.SetActive(false);
                                        }
                                        
                                        if(DateTime.Parse(pSProduct.Value.TEnd).Year < 2037)
                                        {                                            
                                            text = pItem.Find("bg/time/text").GetComponent<TMPro.TMP_Text>();
                                            text.transform.parent.gameObject.SetActive(true);
                                            TimeSpan pTimeSpan = DateTime.Parse(pSProduct.Value.TEnd) - serverTime;
                                            SingleFunc.UpdateTimeText((int)pTimeSpan.TotalSeconds,text,0);   
                                        }
                                        else
                                        {
                                            pItem.Find("bg/time").gameObject.SetActive(false);
                                        }
                                    }
                                    else if(eItme == E_SHOP_ITEM.ShopMonthlyItem)
                                    {
                                        ProductFlatRateT pProductFlatRate = pGameContext.GetProductFlatRateByProduct(pSProduct.Value.No);
                                        
                                        if(pProductFlatRate == null)
                                        {
                                            pItem.Find("bg/info/on").gameObject.SetActive(false);
                                            pItem.Find("bg/info/normal").gameObject.SetActive(true);
                                            pItem.Find("bg/info/time").gameObject.SetActive(false);
                                            text = pItem.Find("bg/info/normal/text").GetComponent<TMPro.TMP_Text>();
                                            pGameContext.UpdateCurrencyPrice(text,pSProduct.Value.Sku,pSProduct.Value.CostAmount.ToString());
                                            text = pItem.Find("bg/info/normal/title").GetComponent<TMPro.TMP_Text>();
                                            text.SetText(string.Format(pGameContext.GetLocalizingText("SHOP_TXT_TOKEN_TICKET_DAY"),pSProduct.Value.Duration));
                                        }
                                        else
                                        {
                                            pItem.Find("bg/info/on").gameObject.SetActive(true);
                                            pItem.Find("bg/info/normal").gameObject.SetActive(false);
                                            pItem.Find("bg/info/time").gameObject.SetActive(pProductFlatRate.Day >= pProductFlatRate.Max);

                                            if(pProductFlatRate.Day >= pProductFlatRate.Max)
                                            {
                                                TimeSpan pTimeSpan = new DateTime(pProductFlatRate.TRefresh) - serverTime;
                                                if(pTimeSpan.TotalSeconds > 0)
                                                {
                                                    text = pItem.Find("bg/info/time/text").GetComponent<TMPro.TMP_Text>();
                                                    m_pProductFlatRate.Add(pProductFlatRate,text);
                                                    SingleFunc.UpdateTimeText((int)pTimeSpan.TotalSeconds,text,0);
                                                }
                                            }
                                            else
                                            {
                                                text = pItem.Find("bg/info/on/text").GetComponent<TMPro.TMP_Text>();
                                                text.SetText(string.Format(pGameContext.GetLocalizingText("SHOP_TXT_TOKEN_TICKET_LEFT_TIME_END"),pProductFlatRate.Max - pProductFlatRate.Day));
                                            }                                            
                                        }

                                        CONSTVALUE.E_CONST_TYPE eConstType = CONSTVALUE.E_CONST_TYPE.MAX;
                                        if(Enum.TryParse<CONSTVALUE.E_CONST_TYPE>($"tokenTicketDailyReward_{pSProduct.Value.No}",out eConstType))
                                        {
                                            text = pItem.Find("bg/comment1").GetComponent<TMPro.TMP_Text>();
                                            text.SetText(string.Format(pGameContext.GetLocalizingText("SHOP_TXT_TOKEN_TICKET_DETAIL_1"),pSProduct.Value.Duration,pGameContext.GetConstValue(eConstType)));
                                        }
                                        
                                        text = pItem.Find("bg/comment2").GetComponent<TMPro.TMP_Text>();
                                        text.SetText(string.Format(pGameContext.GetLocalizingText("SHOP_TXT_TOKEN_TICKET_DETAIL_2"), pSProduct.Value.RewardAmount));
                                    } 
                                    else if(eItme == E_SHOP_ITEM.ShopCurrencyItem)
                                    {
                                        text = pItem.Find("bg/buy/price").GetComponent<TMPro.TMP_Text>();
                                        pGameContext.UpdateCurrencyPrice(text,pSProduct.Value.Sku,pSProduct.Value.CostAmount.ToString());
                                        text = pItem.Find("bg/count").GetComponent<TMPro.TMP_Text>();
                                        text.SetText(string.Format("{0:#,0}", pSProduct.Value.RewardAmount));
                                    }
                                }
                                else
                                {
                                    Button pButton = pItem.Find("bg/buy").GetComponent<Button>();

                                    text = pItem.Find("bg/count").GetComponent<TMPro.TMP_Text>();
                                    bool bOn = true;
                                    if(eItme == E_SHOP_ITEM.ShopManageItem )
                                    {
                                        icon = pItem.Find("bg/buy/icon").GetComponent<RawImage>();
                                        icon.texture = AFPool.GetItem<Sprite>("Texture",pSProduct.Value.Cost.ToString()).texture;
                                        if(pSProduct.Value.Reward == GameContext.GAME_MONEY_ID)
                                        {
                                            ulong total = pSProduct.Value.RewardAmount * pGameContext.GetBusinessTotalRewardForTime();
                                            text.SetText(ALFUtils.NumberToString(total));
                                            bOn =total > 0;
                                        }
                                        else
                                        {
                                            text.SetText(ALFUtils.NumberToString(pSProduct.Value.RewardAmount * pGameContext.GetTrainingRewardForTimeByID((pSProduct.Value.Reward - 40) + 1000)));
                                        }
                                    }
                                    else
                                    {
                                        if(pSProduct.Value.Reward == GameContext.CASH_ID || pSProduct.Value.Reward == GameContext.FREE_CASH_ID)
                                        {
                                            text.SetText(string.Format("{0:#,0}", pSProduct.Value.RewardAmount));
                                        }
                                        else
                                        {
                                            text.SetText(ALFUtils.NumberToString(pSProduct.Value.RewardAmount));
                                        }
                                    }
                                    
                                    if(eItme == E_SHOP_ITEM.ShopFreeItem )
                                    {
                                        ulong itemCount = pGameContext.GetItemCountByNO(GameContext.AD_SKIP_ID);
                                        Transform tm = pButton.transform.Find("skip");
                                        tm.gameObject.SetActive(itemCount > 0);

                                        if(itemCount > 0)
                                        {
                                            text = tm.Find("text").GetComponent<TMPro.TMP_Text>();
                                            text.SetText($"x{itemCount}");
                                        }

                                        if(pProductMileage == null )
                                        {
                                            itemCount = 0;
                                        }
                                        else
                                        {
                                            itemCount = pProductMileage.Amount;
                                        }

                                        text = tm.Find("price").GetComponent<TMPro.TMP_Text>();
                                        text.SetText($"{pSProduct.Value.LimitAmount - itemCount}/{pSProduct.Value.LimitAmount}");
                                        text = pButton.transform.Find("price").GetComponent<TMPro.TMP_Text>();
                                        text.SetText($"{pSProduct.Value.LimitAmount - itemCount}/{pSProduct.Value.LimitAmount}");
                                        pButton.enabled= pSProduct.Value.LimitAmount > itemCount;
                                        if(!pButton.enabled)
                                        {
                                            tm.gameObject.SetActive(false);
                                        }
                                    }
                                    else
                                    {
                                        text = pButton.transform.Find("price").GetComponent<TMPro.TMP_Text>();
                                        pGameContext.UpdateCurrencyPrice(text,pSProduct.Value.Sku,pSProduct.Value.CostAmount.ToString());

                                        if(bOn)
                                        {
                                            uint id = pSProduct.Value.Cost;
                                            if(id == GameContext.FREE_CASH_ID || id == GameContext.CASH_ID)
                                            {
                                                pButton.enabled = pGameContext.GetTotalCash() >= pSProduct.Value.CostAmount; 
                                            }
                                            else
                                            {
                                                pButton.enabled = pGameContext.GetItemCountByNO(pSProduct.Value.Cost) >= pSProduct.Value.CostAmount;
                                            }
                                        }
                                        else
                                        {
                                            pButton.enabled = false;
                                        }
                                    }
                                    
                                    pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
                                    pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
                                }
                            }
                        }

                        size = pItems.sizeDelta;
                        size.y = totalCount / pGridLayoutGroup.constraintCount;
                        size.y += (totalCount % pGridLayoutGroup.constraintCount) > 0 ? 1 : 0;
                        size.y = pGridLayoutGroup.cellSize.y * size.y;
                        pItems.sizeDelta = size;
                        subH += size.y + pSubCategory.rect.height;

                        if(pSubCategory.Find("item") != null && totalCount > 0)
                        {
                            size = pItems.anchoredPosition;
                            size.y -= 80;
                            pItems.anchoredPosition = size;
                            subH += 80;
                        }
                    }

                    if(totalCount > 0)
                    {
                        bActive = true;
                        subH += 10;
                    }
                    else
                    {
                        subH -= pSubCategory.rect.height;
                        pSubCategory.gameObject.SetActive(false);
                    }
                }
                if(bActive)
                {
                    h += subH + 20;
                }
                else
                {
                    pCategory.gameObject.SetActive(false);
                    m_pNaviNodeList[i +1].gameObject.SetActive(false);
                }
            }
        }

        UpdateNaviNodeList();
        pItem = LayoutManager.Instance.GetItem<RectTransform>("PurchasePolicy");
        pItem.gameObject.name = "PurchasePolicy";
        size = pItem.sizeDelta;
        pItem.SetParent(m_pScrollRect.content,false);

        pItem.anchoredPosition = new Vector2(0,-h);
        size = pItem.sizeDelta;
        size.x = 0;
        pItem.sizeDelta = size;
        h += pItem.rect.height;
        
        size = m_pScrollRect.content.sizeDelta;
        size.y = h + 60;
        if(size.y > m_pScrollRect.viewport.sizeDelta.y - 110)
        {
            size.y += 110;
        }
        m_pScrollRect.content.sizeDelta = size;
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
        SingleFunc.SetupLocalizingText(m_pScrollRect.content);
        UpdateItemCount();
        
        if(!bReset)
        {
            m_pScrollRect.content.anchoredPosition = pos;
        }
    }
    
    public void UpdateItemCount()
    {
        GameContext pGameContext = GameContext.getCtx();
        m_pItems[0].SetText(ALFUtils.NumberToString(pGameContext.GetItemCountByNO(1300)));
        m_pItems[1].SetText(ALFUtils.NumberToString(pGameContext.GetItemCountByNO(1301)));
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
                UpdateRefreshTimeText();
            }
        }
    }

    void UpdateRefreshTimeText()
    {
        if(MainUI == null || !MainUI.gameObject.activeSelf) return;
        
        if(m_pProductFlatRate.Count > 0)
        {
            DateTime serverTime = NetworkManager.GetGameServerTime();
            var itr = m_pProductFlatRate.GetEnumerator();
            while(itr.MoveNext())
            {
                TimeSpan pTimeSpan = new DateTime(itr.Current.Key.TRefresh) - serverTime;
                SingleFunc.UpdateTimeText((int)pTimeSpan.TotalSeconds,itr.Current.Value,0);
            }
        }
    }

    void UpdateRefreshTimeButton(Transform item,uint no)
    {
        GameContext pGameContext = GameContext.getCtx();

        ProductMileageT pProductMileage = pGameContext.GetProductMileageData(no);
        if(pProductMileage != null)
        {
            SHOPPRODUCT.ShopProductList pShopProductList = pGameContext.GetFlatBufferData<SHOPPRODUCT.ShopProductList>(E_DATA_TYPE.ShopProduct);
            SHOPPRODUCT.ShopList? pShopList = pShopProductList.ShopProductByKey((uint)E_SHOP_ITEM.ShopFreeItem);
            ShopProduct? pShopProduct = pShopList.Value.ListByKey(no);
            
            bool bActive = pProductMileage.Amount < pShopProduct.Value.LimitAmount;
            
            if(bActive)
            {
                item.Find("bg/buy/on").gameObject.SetActive(true);
                item.Find("bg/buy/off").gameObject.SetActive(false);
                item.Find("bg/buy").GetComponent<Button>().enabled = true;
            }
            else
            {
                item.Find("bg/buy/on").gameObject.SetActive(false);
                item.Find("bg/buy/off").gameObject.SetActive(true);
                item.Find("bg/buy").GetComponent<Button>().enabled = false;
            }
        
            pGameContext.SaveUserData(true);
        }
    }

    public void ClearScroll()
    {
        m_pProductFlatRate.Clear();
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        int i = m_pScrollRect.content.childCount;
        Transform pGroup = null;
        RectTransform item = null;
        int n = 0;
        string str = null;
        Transform tm = null;
        int t = 0;
        while(i > 0)
        {
            --i;
            pGroup = m_pScrollRect.content.GetChild(i);

            if(pGroup.gameObject.name == "PurchasePolicy")
            {
                LayoutManager.Instance.AddItem("PurchasePolicy",pGroup.GetComponent<RectTransform>());
                continue;
            }
            
            for(n =0; n < pGroup.childCount; ++n)
            {
                tm = pGroup.GetChild(n);
                str = tm.gameObject.name;
                tm = tm.Find("items");
                t = tm.childCount;
                while(t > 0)
                {
                    --t;
                    item = tm.GetChild(t).GetComponent<RectTransform>();
                    item.Find("bg/icon").GetComponent<RawImage>().texture = null;
                    if(str == "ShopManageItem")
                    {
                        item.Find("bg/buy/icon").GetComponent<RawImage>().texture = null;
                    }
                    LayoutManager.Instance.AddItem(str,item);
                }
            }
        }

        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    void RefreshRewardCallback(E_AD_STATUS eStatus, bool useSkipItem)//( GoogleMobileAds.Api.Reward reward)
    {
        if(eStatus != E_AD_STATUS.RewardComplete) return;

        JObject pJObject = new JObject();
        pJObject["no"] = m_iProductNo;
        pJObject["amount"] = 1;
        pJObject["skip"] = useSkipItem ? 1 : 0;

        m_pMainScene.RequestAfterCall(E_REQUEST_ID.shop_buy,pJObject);
        if(!string.IsNullOrEmpty(m_pAdjustEventKey))
        {
            GameContext.getCtx().SendAdjustEvent(m_pAdjustEventKey,false, false,-1);
        }
        m_pAdjustEventKey = null;
        m_iProductNo = 0;
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        GameContext pGameContext = GameContext.getCtx();
        SHOPPRODUCT.ShopProductList pShopProductList = pGameContext.GetFlatBufferData<SHOPPRODUCT.ShopProductList>(E_DATA_TYPE.ShopProduct);
        SHOPPRODUCT.ShopList? pShopProduct = null;

        if(tm.name == "special")
        {
            E_SHOP_ITEM eItem = (E_SHOP_ITEM)Enum.Parse(typeof(E_SHOP_ITEM),sender.transform.parent.parent.name);
            pShopProduct = pShopProductList.ShopProductByKey((uint)eItem);
            uint id =0;
            if(uint.TryParse(sender.name, out id))
            {
                SHOPPRODUCT.ShopProduct? pSProduct = pShopProduct.Value.ListByKey(id);
                m_pCurrentPurchaseSKU = pSProduct.Value.Sku;
                m_pAdjustEventKey = null;
                if( eItem == E_SHOP_ITEM.ShopMonthlyItem)
                {
                    m_pMainScene.ShowMonthlyPackagePopup(pSProduct);
                }
                else
                {
                    m_pMainScene.ShowShopPackagePopup(pSProduct);
                }
            }
        }
        else if(sender.name == "buy" )
        {
            E_SHOP_ITEM eItem = (E_SHOP_ITEM)Enum.Parse(typeof(E_SHOP_ITEM),sender.transform.parent.parent.parent.parent.name);
            pShopProduct = pShopProductList.ShopProductByKey((uint)eItem);
            uint id =0;
            if(uint.TryParse(sender.transform.parent.parent.name, out id))
            {
                SHOPPRODUCT.ShopProduct? pSProduct = pShopProduct.Value.ListByKey(id);

                if(eItem == E_SHOP_ITEM.ShopCurrencyItem)
                {
                    m_pCurrentPurchaseSKU = pSProduct.Value.Sku;
                    PurchaseReserve(m_pCurrentPurchaseSKU,0);
                }
                else
                {
                    if(eItem == E_SHOP_ITEM.ShopFreeItem)
                    {
                        m_pAdjustEventKey = pSProduct.Value.Analytics;
                        m_iProductNo = pSProduct.Value.No;
                        if(sender.transform.Find("skip").gameObject.activeSelf)
                        {
                            RefreshRewardCallback(E_AD_STATUS.RewardComplete,true);
                        }
                        else
                        {
                            if(!pGameContext.ShowRewardVideo((E_AD_STATUS eStatus)=>{ RefreshRewardCallback(eStatus,false);} ))
                            {
                                m_pAdjustEventKey = null;
                                m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_AD_NOT_PREPARED"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                            }
                        }
                    }
                    else if(eItem == E_SHOP_ITEM.ShopManageItem)
                    {
                        m_pAdjustEventKey = null;
                        m_pMainScene.ShowManagementPlanPopup(pSProduct);
                    }
                    else
                    {
                        m_pAdjustEventKey = pSProduct.Value.Analytics;
                        bool bEnable = true;
                        if(pSProduct.Value.Cost == GameContext.FREE_CASH_ID || pSProduct.Value.Cost == GameContext.CASH_ID)
                        {
                            bEnable = pGameContext.GetTotalCash() >= pSProduct.Value.CostAmount;
                        }
                        else
                        {
                            bEnable = pGameContext.GetItemCountByNO(pSProduct.Value.Cost) >= pSProduct.Value.CostAmount;
                        }
                        if(bEnable)
                        {
                            m_pMainScene.ShowConfirmPopup( pGameContext.GetLocalizingText("DIALOG_SHOPITEMBUY_TITLE"),string.Format(pGameContext.GetLocalizingText("DIALOG_SHOPITEMBUY_TXT"),pSProduct.Value.CostAmount) ,pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,()=>{
                                m_iProductNo = pSProduct.Value.No;
                                RefreshRewardCallback(E_AD_STATUS.RewardComplete,false);
                            });
                            
                        }
                        else
                        {
                            m_pMainScene.ShowMessagePopup( pGameContext.GetLocalizingErrorMsg("11113"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"));
                            m_pAdjustEventKey = null;
                        }
                    }
                }
            }
        }
        else if(sender.name == "info")
        {
            E_SHOP_ITEM eItem = (E_SHOP_ITEM)Enum.Parse(typeof(E_SHOP_ITEM),sender.transform.parent.parent.parent.parent.name);
            if(eItem == E_SHOP_ITEM.ShopFreeItem)
            {
                pShopProduct = pShopProductList.ShopProductByKey((uint)eItem);
                uint id =0;
                if(uint.TryParse(sender.transform.parent.parent.name, out id))
                {
                    SHOPPRODUCT.ShopProduct? pSProduct = pShopProduct.Value.ListByKey(id);
                    ItemTip pUI = m_pMainScene.GetInstance<ItemTip>();
                    pUI.SetupData(sender.transform.parent.Find("icon").GetComponent<RectTransform>(), pSProduct.Value.Reward);
                    pUI.MainUI.SetAsLastSibling();
                    SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
                }
            }
        }
        else
        {
            m_pMainScene.ShowItemTipPopup(sender);
        }
    }

    public void PurchaseReserve(string sku,uint timeSale)
    {
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.IsTransactionFinish())
        {
            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_TRY_AGAIN"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
            return;
        }

        JObject pJObject = new JObject();
#if UNITY_IOS
        pJObject["store"] = ALF.LOGIN.LoginManager.E_STORE.apple.ToString();
#elif ONESTORE
        pJObject["store"] = ALF.LOGIN.LoginManager.E_STORE.onestore.ToString();
#elif UNITY_ANDROID
        pJObject["store"] = ALF.LOGIN.LoginManager.E_STORE.google.ToString();
#endif
        pJObject["sku"] = sku;

        pJObject["timeSale"] = timeSale;
        
        if(Application.isEditor)
        {
            pJObject["store"] = "test";
        }

        m_pMainScene.RequestAfterCall(E_REQUEST_ID.iap_reserve,pJObject,null);
    }
    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_bSnapChange)
        {
            m_bSnapChange = false;
            return;
        }

        RectTransform tm = null;
        Vector2 endPos;
        Button pButton = null;
        for(int i =0; i < m_pScrollRect.content.childCount; ++i)
        {
            tm = m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>();
            
            endPos = (Vector2)m_pScrollRect.viewport.InverseTransformPoint( tm.position );
            if(endPos.y <= 0 && endPos.y >= m_pScrollRect.viewport.rect.height * -0.6f)
            {
                for(int n =1; n < m_pNaviNodeList.Length; ++n)
                {
                    pButton = m_pNaviNodeList[n].GetComponent<Button>();
                    pButton.enabled = n != i +1;
                    m_pNaviNodeList[n].Find("on").gameObject.SetActive(!pButton.enabled);
                }

                return;
            }
        }
    }

    public void Close()
    {
        m_pMainScene.HideMoveDilog(MainUI,Vector3.right);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(sender.name)
        {
            case "back":
            {
                Close();
                m_pMainScene.ResetMainButton();
            }
            break;
            case "special":
            {
                SetScroll((byte)E_TAB.special);
            }
            break;
            case "free":
            {
                SetScroll((byte)E_TAB.free);
            }
            break;
            case "currency":
            {
                SetScroll((byte)E_TAB.currency);
            }
            break;
            case "ticket":
            {
                SetScroll((byte)E_TAB.ticket);
            }
            break;
        }
    }

    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
        if(!MainUI.gameObject.activeInHierarchy || data == null) return;
        
        E_REQUEST_ID eType = (E_REQUEST_ID)data.Id;
        switch(eType)
        {
            case E_REQUEST_ID.iap_reserve:
            {    
                if(string.IsNullOrEmpty(data.ErrorCode))
                {
                    bool bOk = (int)data.Json["errorCode"] == 0;
                    if(m_pMainScene.IsShowInstance<ShopPackage>())
                    {
                        m_pMainScene.GetInstance<ShopPackage>().Purchase(bOk);
                    }
                    else if(m_pMainScene.IsShowInstance<MonthlyPackage>())
                    {
                        m_pMainScene.GetInstance<MonthlyPackage>().Purchase(bOk);
                    }
                    else
                    {
                        if(bOk)
                        {
                            GameContext.getCtx().Purchase(m_pCurrentPurchaseSKU);
                        }
                    }
                }

                m_pCurrentPurchaseSKU = null;
            }
            break;
            case E_REQUEST_ID.iap_purchase:
            case E_REQUEST_ID.shop_buy:
            {
                SetupScroll(false);
            }
            break;
        }
    }
}
