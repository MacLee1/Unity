using UnityEngine;
using UnityEngine.UI;
using ALF;
using System;
using ALF.LAYOUT;
using DATA;
using USERDATA;
using USERRANK;
using MILEAGEMISSION;
using REWARDSET;
using LADDERREWARD;
using ACHIEVEMENTMISSION;


public class TrophyRewardInfo : IBaseUI
{
    const string REWARD_ITEM_NAME = "RewardItem";
    MainScene m_pMainScene = null;
    TMPro.TMP_Text m_pTrophy = null;
    TMPro.TMP_Text m_pTitle = null;
    TMPro.TMP_Text m_pGroupName = null;
    TMPro.TMP_Text m_pPlayerMinText = null;
    TMPro.TMP_Text m_pPlayerMaxText = null;
    RawImage m_pRankIcon = null;
    RawImage[] m_pAbilityIcon = new RawImage[2];
    ScrollRect m_pScrollRect = null;
    RectTransform m_pSeasonRewards = null;
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}

    public TrophyRewardInfo(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        if(m_pScrollRect != null)
        {
            m_pScrollRect.onValueChanged.RemoveAllListeners();
            ClearScroll();
        }
        SingleFunc.ClearRankIcon(m_pRankIcon);
        m_pRankIcon = null;
        for(int i =0; i < m_pAbilityIcon.Length; ++i)
        {
            m_pAbilityIcon[i].texture = null;
        }
        m_pAbilityIcon = null;
    
        m_pScrollRect = null;
        m_pTrophy = null;
        m_pSeasonRewards = null;
        m_pPlayerMinText = null;
        m_pPlayerMaxText = null;
        m_pTitle = null;
        m_pGroupName = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "TrophyRewardInfo : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "TrophyRewardInfo : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pTitle = MainUI.Find("root/title").GetComponent<TMPro.TMP_Text>();
        m_pGroupName = MainUI.Find("root/name").GetComponent<TMPro.TMP_Text>();
        m_pTrophy = MainUI.Find("root/trophy/text").GetComponent<TMPro.TMP_Text>();
        m_pPlayerMinText = MainUI.Find("root/player/min/text").GetComponent<TMPro.TMP_Text>();
        m_pPlayerMaxText = MainUI.Find("root/player/max/text").GetComponent<TMPro.TMP_Text>();
        m_pSeasonRewards = MainUI.Find("root/seasonRewards/rewards").GetComponent<RectTransform>();
        m_pRankIcon = MainUI.Find("root/rank").GetComponent<RawImage>();
        m_pAbilityIcon[0] = m_pPlayerMinText.transform.parent.GetComponent<RawImage>();
        m_pAbilityIcon[1] = m_pPlayerMaxText.transform.parent.GetComponent<RawImage>();

        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupMileageMissionReward(uint group,uint id)
    {
        GameContext pGameContext = GameContext.getCtx();

        AchievementMissionList pAchievementMissionList = pGameContext.GetFlatBufferData<AchievementMissionList>(E_DATA_TYPE.AchievementMission);
        AchievementGroupItem? pAchievementGroupItem = pAchievementMissionList.AchievementMissionByKey(group);
        AchievementMissionItem? pAchievementMissionItem = pAchievementGroupItem.Value.ListByKey(id);

        Sprite pSprite = null;
        RectTransform pItem = null;
        Vector2 size;
        float w = 0;
        Vector2 anchor = new Vector2(0, 0.5f);
        m_pGroupName.SetText(pGameContext.GetLocalizingText(pAchievementGroupItem.Value.GroupName));
        
        if(pAchievementMissionItem == null)
        {
            m_pTitle.SetText(pGameContext.GetLocalizingText("rank_name_1"));
            m_pTrophy.SetText("0");
        }
        else
        {
            m_pTitle.SetText(pGameContext.GetLocalizingText(pAchievementMissionItem.Value.Name));
            m_pTrophy.SetText(pAchievementMissionItem.Value.Objective.ToString());

            if(pAchievementMissionItem.Value.RewardType == GameContext.REWARD_SET)
            {
                RewardSetList pRewardSetList = pGameContext.GetFlatBufferData<RewardSetList>(E_DATA_TYPE.RewardSet);
                Rewards? pRewards = pRewardSetList.RewardSetByKey(pAchievementMissionItem.Value.Reward);
                RewardSetItem? pRewardSetItem = null;

                for(int i =0; i < pRewards.Value.ListLength; ++i)
                {
                    pRewardSetItem = pRewards.Value.List(i);
                    Debug.Log(pRewardSetItem.Value.Item);
                    pItem = SingleFunc.GetRewardIcon(m_pScrollRect.content,REWARD_ITEM_NAME,pRewardSetItem.Value.Item,pRewardSetItem.Value.Amount);
                        
                    if(pItem)
                    {
                        pItem.anchorMax = anchor;
                        pItem.anchorMin = anchor;
                        pItem.pivot = anchor;
                        pItem.anchoredPosition = new Vector2(w,0);
                        size = pItem.sizeDelta;
                        w += size.x + 20;
                    }
                }
                
                if(w > 0)
                {
                    w -= 20;
                }
            }
            else
            {
                pItem = SingleFunc.GetRewardIcon(m_pScrollRect.content,REWARD_ITEM_NAME,pAchievementMissionItem.Value.Reward,pAchievementMissionItem.Value.RewardAmount);
                    
                if(pItem)
                {
                    pItem.anchorMax = anchor;
                    pItem.anchorMin = anchor;
                    pItem.pivot = anchor;

                    pItem.anchoredPosition = new Vector2(w,0);
                    size = pItem.sizeDelta;
                    w += size.x + 20;
                }
            }

            size = m_pScrollRect.content.sizeDelta;
            size.x = w;
            m_pScrollRect.content.sizeDelta = size;
        }
        
        LadderRewardList pLadderRewardList = pGameContext.GetFlatBufferData<LadderRewardList>(E_DATA_TYPE.LadderReward);
        LadderRewardItem? pLadderRewardItem = pLadderRewardList.LadderRewardByKey(id +1);
        
        w = 0;
        pItem = SingleFunc.GetRewardIcon(m_pSeasonRewards,REWARD_ITEM_NAME,pLadderRewardItem.Value.SeasonReward1,pLadderRewardItem.Value.SeasonRewardAmount1);
                
        if(pItem)
        {
            // pItem.gameObject.name = "r1";    
            pItem.anchorMax = anchor;
            pItem.anchorMin = anchor;
            pItem.pivot = anchor;

            pItem.anchoredPosition = new Vector2(w,0);
            size = pItem.sizeDelta;
            w += size.x + 20;
        }

        pItem = SingleFunc.GetRewardIcon(m_pSeasonRewards,REWARD_ITEM_NAME,pLadderRewardItem.Value.SeasonReward2,pLadderRewardItem.Value.SeasonRewardAmount2);
                
        if(pItem)
        {
            // pItem.gameObject.name = "r2";
            pItem.anchorMax = anchor;
            pItem.anchorMin = anchor;
            pItem.pivot = anchor;

            pItem.anchoredPosition = new Vector2(w,0);
            size = pItem.sizeDelta;
        }        

        UserRankList pUserRankList = pGameContext.GetFlatBufferData<UserRankList>(E_DATA_TYPE.UserRank);
        UserRankItem? pUserRankItem = pUserRankList.UserRankByKey(id+1);
        SingleFunc.SetupRankIcon(m_pRankIcon,(byte)(id+1));
        
        m_pPlayerMinText.SetText(pUserRankItem.Value.PlayerTierMin.ToString());

        if(pUserRankItem.Value.PlayerTierMin < 60)
        {
            pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier0");
        }
        else if(pUserRankItem.Value.PlayerTierMin < 70)
        {
            pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier1");
        }
        else if(pUserRankItem.Value.PlayerTierMin < 80)
        {
            pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier2");
        }
        else
        {
            pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier3");
        }

        m_pAbilityIcon[0].texture = pSprite.texture;

        if(pUserRankItem.Value.PlayerTierMax < 60)
        {
            pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier0");
        }
        else if(pUserRankItem.Value.PlayerTierMax < 70)
        {
            pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier1");
        }
        else if(pUserRankItem.Value.PlayerTierMax < 80)
        {
            pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier2");
        }
        else
        {
            pSprite = AFPool.GetItem<Sprite>("Texture","abilityTier3");
        }

        m_pAbilityIcon[1].texture = pSprite.texture;

        m_pPlayerMaxText.SetText(pUserRankItem.Value.PlayerTierMax.ToString());        
    }

    
    void ClearScroll()
    {
        RectTransform item = null;
        Vector2 anchor = new Vector2(0.5f,0.5f);
        int i = m_pScrollRect.content.childCount;
        while(i > 0)
        {
            --i;
            item = m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>();
            SingleFunc.AddRewardIcon(item,REWARD_ITEM_NAME);
        }
        SingleFunc.ClearRankIcon(m_pRankIcon);
        m_pAbilityIcon[0].texture = null;
        m_pAbilityIcon[1].texture = null;

        i = m_pSeasonRewards.childCount;
        while(i > 0)
        {
            --i;
            item = m_pSeasonRewards.GetChild(i).GetComponent<RectTransform>();
            SingleFunc.AddRewardIcon(item,REWARD_ITEM_NAME);
        }
    }

    public void Close()
    {
        Enable = false;
        SingleFunc.HideAnimationDailog(MainUI,()=>{
            ClearScroll();
            MainUI.gameObject.SetActive(false);
            MainUI.Find("root").localScale = Vector3.one;
            LayoutManager.Instance.InteractableEnabledAll();
        });
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
    
}
