using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
using USERDATA;
using DATA;
using ADREWARD;
using Newtonsoft.Json.Linq;
using STATEDATA;


public class AD : IBaseUI
{
    const string REWARD_ITEM_NAME = "RewardItem";
    MainScene m_pMainScene = null;
    // enum E_AD : byte { ownerChance = 0,sponsorShip,legendaryTraining,specialScout,comeFromBehindWin,losingStreak,matchBonus,winningStreak,MAX}
    // Transform[] m_pADList = new Transform[(int)E_AD.MAX];
    uint m_RewardNo = 0;
    public RectTransform MainUI { get; private set;}

    TMPro.TMP_Text m_pTitle = null;
    TMPro.TMP_Text m_pAdAmount = null;
    TMPro.TMP_Text m_pSkipCount = null;
    TMPro.TMP_Text m_pSkip = null;
    
    TMPro.TMP_Text m_pDescription = null;
    Transform m_pReward = null;
   string[]  m_pAdjustEventKeys = new string[2];

    public AD(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        m_pTitle = null;
        m_pDescription = null;
        m_pReward = null;
        m_pAdAmount = null;
        m_pSkipCount = null;
        m_pSkip = null;
        m_pAdjustEventKeys[0] = null;
        m_pAdjustEventKeys[1] = null;
        m_pAdjustEventKeys = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "AD : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "AD : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_pTitle = MainUI.Find("root/title").GetComponent<TMPro.TMP_Text>();
        m_pDescription = MainUI.Find("root/dec").GetComponent<TMPro.TMP_Text>();
        m_pReward = MainUI.Find("root/rewards/item");
        
        m_pAdAmount = MainUI.Find("root/go/text").GetComponent<TMPro.TMP_Text>();
        m_pSkipCount = MainUI.Find("root/skip/icon/text").GetComponent<TMPro.TMP_Text>();
        m_pSkip = MainUI.Find("root/skip/text").GetComponent<TMPro.TMP_Text>();
        
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void SetupRewardData(uint no)
    {
        m_RewardNo = no;
        GameContext pGameContext = GameContext.getCtx();
        if(!pGameContext.IsRewardVideoLoaded() && !pGameContext.IsRewardVideoLoading())
        {
            pGameContext.LoadRewardVideo();
        }

        AdRewardList pAdRewardList = pGameContext.GetFlatBufferData<AdRewardList>(E_DATA_TYPE.AdReward);
        AdRewardItem? pAdRewardItem = pAdRewardList.AdRewardByKey(no);
        AdRewardDataT pAdRewardInfo = pGameContext.GetAdRewardDataByID(no);

        m_pAdjustEventKeys[0] = pAdRewardItem.Value.BasicRewardAnalytics;
        m_pAdjustEventKeys[1] = pAdRewardItem.Value.AdRewardAnalytics;

        m_pTitle.SetText(pGameContext.GetLocalizingText(pAdRewardItem.Value.Name));
        m_pDescription.SetText(pGameContext.GetLocalizingText(pAdRewardItem.Value.Description));

        ulong count = pGameContext.GetItemCountByNO(GameContext.AD_SKIP_ID);
        m_pSkipCount.transform.parent.parent.gameObject.SetActive((count > 0));
        m_pAdAmount.transform.parent.gameObject.SetActive(!(count > 0));
        m_pSkipCount.SetText($"x{count}");
        
        Transform ui = MainUI.Find("root/info");
        for(int n =0; n < ui.childCount; ++n)
        {
            ui.GetChild(n).gameObject.SetActive(false);
        }
        ui.Find(no.ToString()).gameObject.SetActive(true);

        ulong rewardAmount = pAdRewardInfo.BasicReward;
        if(rewardAmount <= 0)
        {
            MainUI.Find("root/claim").gameObject.SetActive(false);
            MainUI.Find("root/next").gameObject.SetActive(true);
            rewardAmount = pAdRewardInfo.AdReward;
            m_pSkip.SetText(pGameContext.GetLocalizingText("ADS_BTN_CLAIM_AD"));
            m_pAdAmount.SetText(pGameContext.GetLocalizingText("ADS_BTN_CLAIM_AD"));
        }
        else
        {
            MainUI.Find("root/claim").gameObject.SetActive(true);
            MainUI.Find("root/next").gameObject.SetActive(false);
            m_pSkip.SetText(pGameContext.GetLocalizingText("ADS_BTN_CLAIM_AD_MULTIPLY_5"));
            m_pAdAmount.SetText(pGameContext.GetLocalizingText("ADS_BTN_CLAIM_AD_MULTIPLY_5"));
        }
        RectTransform pReward = SingleFunc.GetRewardIcon(m_pReward,REWARD_ITEM_NAME, no == 4 ? 4 : pAdRewardInfo.Item,rewardAmount);
        pReward.Find("text").gameObject.SetActive(rewardAmount > 0);
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI,()=>{
            m_pReward.GetChild(0).Find("text").gameObject.SetActive(true);
            SingleFunc.AddRewardIcon(m_pReward.GetChild(0).GetComponent<RectTransform>() ,REWARD_ITEM_NAME);
            MainUI.gameObject.SetActive(false); 
            MainUI.Find("root").localScale = Vector3.one; LayoutManager.Instance.InteractableEnabledAll();});
    }

    void RefreshRewardCallback(E_AD_STATUS eStatus, bool bAdd ,bool useSkipItem)//( GoogleMobileAds.Api.Reward reward)
    { 
        if(eStatus != E_AD_STATUS.RewardComplete) return;
// no: 광고 no
// ad: 1: 광고 보기 보상 획득, 0: 일반 보상 획득(+'다음에')
// * 트리거 광고인 경우에는 ad는 1로 고정된다.
// skip: 1: 광고 스킵권(1103) 사용, 0: 미사용
// * 광고 스킵권 사용 시에는 광보 보기를 skip
        JObject pJObject = new JObject();
        pJObject["no"] = m_RewardNo;
        pJObject["ad"] = bAdd ? 1 : 0;
        pJObject["skip"] = useSkipItem ? 1 : 0;
        string pAdjustEventKey = null;
        if(bAdd)
        {
            pAdjustEventKey = m_pAdjustEventKeys[0];
        }
        else
        {
            pAdjustEventKey = m_pAdjustEventKeys[1];
        }

        if(!string.IsNullOrEmpty(pAdjustEventKey))
        {
            GameContext.getCtx().SendAdjustEvent(pAdjustEventKey,false, false,-1);
        }

        m_pMainScene.RequestAfterCall(E_REQUEST_ID.adReward_reward,pJObject,Close);
    }

    bool CheckPlayerTotalCount()
    {
        if(m_RewardNo == 4)
        {
            GameContext pGameContext = GameContext.getCtx();        
            if(!pGameContext.IsMaxPlayerCount()) return true;
            
            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("EXPANDSQUAD_TXT_EXPAND_DESC"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));    

            return false;
        }
        
        return true;
    }
    
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "go")
        {
            if(CheckPlayerTotalCount())
            {
                GameContext pGameContext = GameContext.getCtx();
                if(!pGameContext.ShowRewardVideo((E_AD_STATUS eStatus)=>{ RefreshRewardCallback(eStatus,true,false);} ))
                {
                    m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_AD_NOT_PREPARED"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                }
            }
            return;
        }
        else if(sender.name == "skip")
        {
            RefreshRewardCallback(E_AD_STATUS.RewardComplete,true,true);
            return;
        }
        else if(sender.name == "claim")
        {
            if(CheckPlayerTotalCount())
            {
                RefreshRewardCallback(E_AD_STATUS.RewardComplete,false,false);
            }
            
            return;
        }
        
        Close();
    }
}
