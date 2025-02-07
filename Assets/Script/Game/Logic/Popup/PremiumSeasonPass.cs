using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;

using ALF.NETWORK;
using ALF.LAYOUT;
using USERDATA;
using PASS;
using PASSMISSION;
using SHOPPRODUCT;
using REWARDSET;
using DATA;
using Newtonsoft.Json.Linq;

public class PremiumSeasonPass : ITimer
{
    const uint PRODUCT_ID = 9999;
    const string REWARD_ITEM_NAME = "RewardItem";
    MainScene m_pMainScene = null;

    TMPro.TMP_Text m_pPurchaseText = null;
    TMPro.TMP_Text m_pExpire = null;
    TMPro.TMP_Text m_pBonus = null;
    RectTransform m_pRewards = null;
    ScrollRect m_pRewardInfo = null;
    string m_pCurrentPurchaseSKU = null;
    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ if (m_pRewardInfo != null) m_pRewardInfo.enabled = value;}}
    
    public PremiumSeasonPass(){}
    
    public void Dispose()
    {
        ClearReward();
        m_pMainScene = null;
        MainUI = null;   
        m_pPurchaseText = null;
        m_pRewardInfo = null;
        m_pRewards = null;
        m_pExpire = null;
        m_pBonus = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "PremiumSeasonPass : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "PremiumSeasonPass : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pPurchaseText = MainUI.Find("root/bg/purchase/text").GetComponent<TMPro.TMP_Text>();
        m_pRewards = MainUI.Find("root/bg/rewards/rewards").GetComponent<RectTransform>();
        m_pExpire = MainUI.Find("root/bg/expire/time").GetComponent<TMPro.TMP_Text>();
        m_pBonus = m_pPurchaseText.transform.parent.Find("bonus/text").GetComponent<TMPro.TMP_Text>();
        m_pRewardInfo = MainUI.Find("root/bg/rewardscroll/rewardscroll").GetComponent<ScrollRect>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        
        MainUI.gameObject.SetActive(false);
    }

    public void DoExpire(int index)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            Close();
        }
    }

    public void UpdateTimer(float dt)
    { 
        GameContext pGameContext = GameContext.getCtx();
        float tic = pGameContext.GetExpireTimerByUI(this,0);
        if(tic <= 86400)
        {
            SingleFunc.UpdateTimeText((int)tic,m_pExpire,0);
        }
    }

    void ClearReward()
    {
        RectTransform item = null;
        int i = m_pRewards.childCount;
        
        while(i > 0)
        {
            --i;
            item = m_pRewards.GetChild(i).GetComponent<RectTransform>();
            SingleFunc.AddRewardIcon(item,REWARD_ITEM_NAME);
        }
        i = m_pRewardInfo.content.childCount;
        while(i > 0)
        {
            --i;
            item = m_pRewardInfo.content.GetChild(i).GetComponent<RectTransform>();
            SingleFunc.AddRewardIcon(item,REWARD_ITEM_NAME);
        }
    }

    public void SetupData()
    {
        GameContext pGameContext = GameContext.getCtx();
        PassList pPassList = pGameContext.GetFlatBufferData<PassList>(E_DATA_TYPE.Pass);
        uint no = pGameContext.GetCurrentPassNo();
        PassItem? pPassItem = pPassList.PassByKey(no);
        ALFUtils.Assert(pPassItem != null, string.Format("pPassItem = null!! no:{0}",no) );

        DateTime serverTime = NetworkManager.GetGameServerTime();
        
        if(serverTime >= DateTime.Parse(pPassItem.Value.TStart) && serverTime < DateTime.Parse(pPassItem.Value.TEnd))
        {
            if(pGameContext.GetExpireTimerByUI(this,0) <= -1)
            {
                float fTime = pGameContext.GetCurrentSeasonExpireRemainTime();
                pGameContext.AddExpireTimer(this,0,fTime);
                SingleFunc.UpdateTimeText((int)fTime,m_pExpire,0);
            }
            
            m_pPurchaseText.transform.parent.gameObject.SetActive(true);
            PassMissionList pPassMissionList = pGameContext.GetFlatBufferData<PassMissionList>(E_DATA_TYPE.PassMission);
            ShopProductList pShopProductList = pGameContext.GetFlatBufferData<ShopProductList>(E_DATA_TYPE.ShopProduct);
            RewardSetList pRewardSetList = pGameContext.GetFlatBufferData<RewardSetList>(E_DATA_TYPE.RewardSet);
            PassMissionInfo? pPassMissionInfo = pPassMissionList.PassMissionByKey(no);
            PassMissionItem? pPassMissionItem = null;
            ShopList? pShopList = pShopProductList.ShopProductByKey(PRODUCT_ID);
            Dictionary<uint,ulong> list = new Dictionary<uint, ulong>();
            if(pShopList.Value.Shop == PRODUCT_ID)
            {
                ShopProduct? pShopProduct = pShopList.Value.ListByKey(pPassItem.Value.Product);
                if(pShopProduct != null)
                {
                    m_pCurrentPurchaseSKU = pShopProduct.Value.Sku;
                    pGameContext.UpdateCurrencyPrice(m_pPurchaseText,pShopProduct.Value.Sku,pShopProduct.Value.CostAmount.ToString());
                    m_pBonus.SetText($"{pShopProduct.Value.BonusRate}%");
                    m_pBonus.transform.parent.gameObject.SetActive(pShopProduct.Value.BonusRate > 0);

                    if(pShopProduct.Value.RewardType == GameContext.REWARD_SET)
                    {
                        Rewards? pRewards = pRewardSetList.RewardSetByKey(pShopProduct.Value.Reward);
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
                                    list[no] += pRewardSetItem.Value.Amount * pShopProduct.Value.RewardAmount;
                                }
                                else
                                {
                                    list.Add(no,pRewardSetItem.Value.Amount * pShopProduct.Value.RewardAmount);
                                }
                            }
                        }
                    }
                    else
                    {
                        no = pShopProduct.Value.Reward;
                        if(no == GameContext.FREE_CASH_ID)
                        {
                            no = GameContext.CASH_ID;
                        }

                        if(list.ContainsKey(no))
                        {
                            list[no] += pShopProduct.Value.RewardAmount;
                        }
                        else
                        {
                            list.Add(no,pShopProduct.Value.RewardAmount);
                        }
                    }
                }
            }
            SetupRewards(list,m_pRewards,true);
            list.Clear();
            for(int i =0; i < pPassMissionInfo.Value.ListLength; ++i)
            {
                pPassMissionItem = pPassMissionInfo.Value.List(i);

                if(pPassMissionItem.Value.RewardType2 == GameContext.REWARD_SET)
                {
                    Rewards? pRewards = pRewardSetList.RewardSetByKey(pPassMissionItem.Value.Reward2);
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
                                list[no] += pRewardSetItem.Value.Amount * pPassMissionItem.Value.RewardAmount2;
                            }
                            else
                            {
                                list.Add(no,pRewardSetItem.Value.Amount * pPassMissionItem.Value.RewardAmount2);
                            }
                        }
                    }
                }
                else
                {
                    no = pPassMissionItem.Value.Reward2;
                    if(no == GameContext.FREE_CASH_ID)
                    {
                        no = GameContext.CASH_ID;
                    }

                    if(list.ContainsKey(no))
                    {
                        list[no] += pPassMissionItem.Value.RewardAmount2;
                    }
                    else
                    {
                        list.Add(no,pPassMissionItem.Value.RewardAmount2);
                    }
                }
            }
            SetupRewards(list,m_pRewardInfo.content,false);
        }
        else
        {
            m_pPurchaseText.transform.parent.gameObject.SetActive(false);
            m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("ALERT_TXT_SEASON_ALEADY_ENDED"),null);
        }
    }

    void SetupRewards(Dictionary<uint,ulong> list, RectTransform tm,bool bCenter)
    {
        RectTransform pReward = null;
        
        int i =0;
        float width = 0;
        float w = 0;
        Vector2 pos;
        var itr = list.GetEnumerator();
        
        while(itr.MoveNext())
        {
            pReward = SingleFunc.GetRewardIcon(tm,REWARD_ITEM_NAME,itr.Current.Key,itr.Current.Value);
            width = pReward.rect.width;
            
            if(bCenter)
            {
                pReward.anchorMin = Vector2.one * 0.5f;
                pReward.anchorMax = pReward.anchorMin;
                pReward.pivot = pReward.anchorMin;
            }
            else
            {
                pReward.anchorMin = Vector2.up * 0.5f;
                pReward.anchorMax = pReward.anchorMin;
                pReward.pivot =pReward.anchorMin;
            }
            pos = pReward.anchoredPosition;
            pos.x = w;
            pReward.anchoredPosition = pos;
            
            w += width + 20;
        }
        
        if(bCenter)
        {
            w = (w * -0.5f) + (width * 0.5f);
        
            for(i = 0; i < m_pRewards.childCount; ++i)
            {
                pReward = m_pRewards.GetChild(i).GetComponent<RectTransform>();
                pos = pReward.anchoredPosition;
                pos.x += w;
                pReward.anchoredPosition = pos;
            }
        }
        else
        {
            w -= (width * 0.5f);
        }
        
        Vector2 size = tm.sizeDelta;
        size.x = w;
        tm.sizeDelta = size;
    }

    public void PurchaseComplete()
    {
        Close();
    }
    
    public void Close()
    {
        m_pCurrentPurchaseSKU = null;
        SingleFunc.HideAnimationDailog(MainUI,()=>{ClearReward();MainUI.gameObject.SetActive(false);MainUI.Find("root").localScale = Vector3.one;LayoutManager.Instance.InteractableEnabledAll();});
    }

    public void Purchase(bool bOk)
    {
        if(bOk)
        {
            GameContext.getCtx().Purchase(m_pCurrentPurchaseSKU);
        }
    }
    
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "purchase")
        {
            m_pMainScene.GetInstance<Store>().PurchaseReserve(m_pCurrentPurchaseSKU,0);
            return;
        }
        
        Close();
    }
    
}
