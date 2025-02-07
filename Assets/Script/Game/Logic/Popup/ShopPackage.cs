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


public class ShopPackage : IBaseUI
{
    const string SCROLL_ITEM_NAME = "ShopPackageItem";

    class PackageItem : IBase
    {
        public uint ID {get; private set;}
        public RectTransform Target {get; private set;}
        public Button IconButton {get; private set;}
        public RawImage Icon {get; private set;}
        
        public PackageItem(RectTransform taget, uint id,ulong amount)
        {
            Target = taget;
            Target.gameObject.name = id.ToString();
            ID = id;
            IconButton = Target.Find("bg/icon").GetComponent<Button>();
            Icon = IconButton.GetComponent<RawImage>();
            
            Sprite pSprite = AFPool.GetItem<Sprite>("Texture",id.ToString());
            Icon.texture = pSprite.texture;

            TMPro.TMP_Text text = Target.Find("bg/count").GetComponent<TMPro.TMP_Text>();
            if(ID == GameContext.FREE_CASH_ID || ID == GameContext.CASH_ID)
            {
                text.SetText(string.Format("{0:#,0}", amount));
            }
            else
            {
                text.SetText(ALFUtils.NumberToString(amount));
            }
            
            text = Target.Find("bg/title").GetComponent<TMPro.TMP_Text>();
            
            text.SetText(GameContext.getCtx().GetLocalizingText($"item_name_{id}"));
        }
        public void Dispose()
        {
            Icon.texture = null;
            IconButton = null;
            Icon = null;
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,Target);
            Target = null;
        }
    }

    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    
    float m_fTimeRefreshTime = 1;
    float m_fExpireTime = -1;
    bool m_bExpireUpdate = false;

    TMPro.TMP_Text m_pPurchaseText = null;
    TMPro.TMP_Text m_pPurchaseCountText = null;
    TMPro.TMP_Text m_pExpire = null;
    TMPro.TMP_Text m_pTitle = null;
    RawImage m_pIcon = null;
    DateTime m_pExpireTime;
    string m_pSku = null;
    uint m_iPackageNo = 0;
    uint m_iLimitAmount =0;

    List<PackageItem> m_pPackageItemList = new List<PackageItem>();
    
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value; }}
    public ShopPackage(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        if(m_pScrollRect != null)
        {
            ClearScroll();
        }
        
        m_pScrollRect = null;
        m_pPurchaseText = null;
        m_pPurchaseCountText = null;
        m_pExpire = null;
        m_pTitle = null;
        m_pSku = null;
        m_pIcon.texture = null;
        m_pIcon = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "ShopPackage : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "ShopPackage : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pTitle = MainUI.Find("root/title").GetComponent<TMPro.TMP_Text>();
        m_pPurchaseText = MainUI.Find("root/purchase/price").GetComponent<TMPro.TMP_Text>();

        m_pPurchaseCountText= MainUI.Find("root/info/time/title").GetComponent<TMPro.TMP_Text>();
        m_pExpire = MainUI.Find("root/info/time/text").GetComponent<TMPro.TMP_Text>();
        m_pIcon = MainUI.Find("root/info/icon").GetComponent<RawImage>();
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupScroll(SHOPPRODUCT.ShopProduct? data)
    {
        ClearScroll();
        GameContext pGameContext = GameContext.getCtx();
        m_pSku = data.Value.Sku;
        m_iPackageNo = data.Value.No;
        m_iLimitAmount = data.Value.LimitAmount;
        m_pTitle.SetText(pGameContext.GetLocalizingText(data.Value.Title));
        m_pPurchaseCountText.gameObject.SetActive(m_iLimitAmount > 0);
        UpdateProductMileageData(false);
        
        m_pIcon.gameObject.SetActive(!string.IsNullOrEmpty(data.Value.Icon));
        if(m_pIcon.gameObject.activeSelf)
        {
            m_pIcon.texture = AFPool.GetItem<Sprite>("Texture",data.Value.Icon).texture;
            m_pIcon.SetNativeSize();
        }

        pGameContext.UpdateCurrencyPrice(m_pPurchaseText,data.Value.Sku,data.Value.CostAmount.ToString());
        
        m_pExpireTime = DateTime.Parse(data.Value.TEnd);
        m_bExpireUpdate = false;
        if(m_pExpireTime.Year < 2037)
        {
            m_pExpire.gameObject.SetActive(true);
            DateTime serverTime = NetworkManager.GetGameServerTime();
        
            if(serverTime >= DateTime.Parse(data.Value.TStart) && serverTime < m_pExpireTime)
            {
                m_bExpireUpdate = true;
                TimeSpan pTimeSpan = m_pExpireTime - serverTime;
                m_fExpireTime = (float)pTimeSpan.Ticks / (float)TimeSpan.TicksPerSecond;
                
                SetupRefreshTime();
                
                SingleFunc.UpdateTimeText((int)pTimeSpan.TotalSeconds,m_pExpire,0);
            }
        }
        else
        {
            m_pExpire.gameObject.SetActive(false);
        }

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
        float h = 0;
        Vector2 anchor = new Vector2(0, 0.5f);
        
        // long severTime = NetworkManager.GetGameServerTime().Ticks;
        var itr = list.GetEnumerator();
        while(itr.MoveNext())
        {
            pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
            
            if(pItem)
            {
                m_pPackageItemList.Add(new PackageItem(pItem,itr.Current.Key,itr.Current.Value));

                pItem.SetParent(m_pScrollRect.content,false);

                pItem.localScale = Vector3.one;
                pItem.anchoredPosition = new Vector2(0,-h);
                size = pItem.sizeDelta;
                h += size.y;
                size.x =0;
                pItem.sizeDelta = size;
            }
        }

        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.horizontalNormalizedPosition = 0;
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        for(int i =0; i < m_pPackageItemList.Count; ++i)
        {
            if(m_pPackageItemList[i].Target == tm)
            {
                ItemTip pUI = m_pMainScene.GetInstance<ItemTip>();
                pUI.SetupData(sender.GetComponent<RectTransform>(), m_pPackageItemList[i].ID);
                pUI.MainUI.SetAsLastSibling();
                SingleFunc.ShowAnimationDailog(pUI.MainUI,null);
                return;
            }
        }
        // m_pMainScene.ShowItemTipPopup(sender);
    }
   
    void ClearScroll()
    {
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);

        int i = m_pPackageItemList.Count;
        while(i > 0)
        {
            --i;
            m_pPackageItemList[i].Dispose();
        }
        m_pPackageItemList.Clear();
        m_pScrollRect.horizontalNormalizedPosition = 0;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }

    void SetupRefreshTime()
    {
        TimeSpan pTimeSpan = m_pExpireTime - NetworkManager.GetGameServerTime();
        
        if(pTimeSpan.TotalDays > 0)
        {
            m_fTimeRefreshTime = (float)pTimeSpan.TotalSeconds % 3600;
        }
        else if(pTimeSpan.TotalHours > 0)
        {
            m_fTimeRefreshTime = (float)pTimeSpan.TotalSeconds % 60;
        }
        else
        {
            m_fTimeRefreshTime = 1;
        }
    }

    public void UpdateTimer(float dt)
    {
        if(m_bExpireUpdate)
        {
            m_fExpireTime -= dt;
            if(m_fExpireTime <= 0)
            {
                m_bExpireUpdate = false;
            }

            m_fTimeRefreshTime -= dt;
            
            if(m_fTimeRefreshTime <= 0)
            {
                SetupRefreshTime();
                SingleFunc.UpdateTimeText((int)m_fExpireTime,m_pExpire,0);
            }
        }
    }

    public void UpdateProductMileageData(bool bUpdate)
    {
        if(m_pPurchaseCountText.gameObject.activeSelf)
        {
            ProductMileageT pProductMileage = GameContext.getCtx().GetProductMileageData(m_iPackageNo);
            int count = (int)m_iLimitAmount;
            if(pProductMileage != null)
            {
                count -= (int)pProductMileage.Amount;
                if(count < 1)
                {
                    count =0;
                    if(bUpdate && MainUI.gameObject.activeSelf)
                    {
                        Close();
						return;
                    }
                }
            }
            m_pPurchaseCountText.SetText(string.Format(GameContext.getCtx().GetLocalizingText("SHOP_TXT_PURCHASE_CHANCE"), count));
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
        m_pSku = null;
        Enable = false;
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "purchase")
        {
            GameContext pGameContext = GameContext.getCtx();
            ProductMileageT pProductMileage = pGameContext.GetProductMileageData(m_iPackageNo);
            int count = (int)m_iLimitAmount;
            if(pProductMileage != null)
            {
                count -= (int)pProductMileage.Amount;
                if(count <= 0)
                {
                    m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingErrorMsg("1806"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                    return;
                }
            }

            m_pMainScene.GetInstance<Store>().PurchaseReserve(m_pSku,0);
            return;
        }

        Close();
    }

}
