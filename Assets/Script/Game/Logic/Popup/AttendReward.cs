using UnityEngine;
using UnityEngine.UI;
using ALF;
using System.Collections.Generic;
using ALF.NETWORK;
using ALF.LAYOUT;
using ALF.SOUND;
using STATEDATA;
using ALF.STATE;
using ALF.MACHINE;
using USERDATA;
using DATA;
using ATTENDREWARD;
using Newtonsoft.Json.Linq;

public class AttendReward : IBaseUI
{
    const string REWARD_ITEM_NAME = "RewardIcon";
    MainScene m_pMainScene = null;
    RectTransform m_pRewards = null;
    Transform m_pTarget = null;
    int m_iDay =0;
    int m_iRewarded = 0;
    BaseState m_pBaseState = null;
    public RectTransform MainUI { get; private set;}
    
    public AttendReward(){}
    
    public void Dispose()
    {
        if(m_pBaseState != null)
        {
            m_pBaseState.Exit(true);
        }
        ClearReward();
        m_pMainScene = null;
        MainUI = null;
        m_pRewards = null;
        m_pTarget = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {   
        ALFUtils.Assert(pBaseScene != null, "AttendReward : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "AttendReward : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_pRewards = MainUI.Find("root/rewards").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        SetupData();

        MainUI.gameObject.SetActive(false);
    }

    void SetupData()
    {
        GameContext pGameContext = GameContext.getCtx(); 
        
        AttendInfoT pAttendInfo = pGameContext.GetAttendInfo(pGameContext.GetAttendInfoIndex(1));
        if(pAttendInfo != null)
        {
            AttendRewardList pAttendRewardList = pGameContext.GetFlatBufferData<AttendRewardList>(E_DATA_TYPE.AttendReward);
            Rewards? pRewards = pAttendRewardList.AttendRewardByKey(pAttendInfo.Attend);
            AttendRewardItem? pAttendRewardItem = null;
            Transform pItem = null;
            uint id =0;
            for(int i =0; i < m_pRewards.childCount; ++i)
            {
                pAttendRewardItem = pRewards.Value.List(i);
                pItem = m_pRewards.Find($"rewad{i}");
                if(pAttendRewardItem.Value.RewardType == GameContext.REWARD_NORMAL)
                {
                    id = pAttendRewardItem.Value.Reward == 1 ? GameContext.CASH_ID : pAttendRewardItem.Value.Reward;
                    SingleFunc.SetupRewardIcon(pItem,REWARD_ITEM_NAME,id,pAttendRewardItem.Value.RewardAmount);
                }
            }
        }
    }

    public void SetupRewardAnimation()
    {
        for(int i = 0; i < m_pRewards.childCount; ++i)
        {
            // m_pRewards.GetChild(i).Find("eff").gameObject.SetActive(false);
            m_pRewards.GetChild(i).gameObject.SetActive(false);
        }

        GameContext pGameContext = GameContext.getCtx(); 
        
        AttendInfoT pAttendInfo = pGameContext.GetAttendInfo(pGameContext.GetAttendInfoIndex(1));
        if(pAttendInfo != null)
        {
            m_iDay = (int)pAttendInfo.Day;
            m_iRewarded = pAttendInfo.Rewarded;

            AttendRewardList pAttendRewardList = pGameContext.GetFlatBufferData<AttendRewardList>(E_DATA_TYPE.AttendReward);
            Setup(pAttendRewardList.AttendRewardByKey(pAttendInfo.Attend));
        }
    }

    void Setup(Rewards? pRewards)
    {
        if(pRewards == null) return;

        Transform pItem = null;
        int day = m_iDay % (pRewards.Value.ListLength +1);
        int rewarded = m_iRewarded % (pRewards.Value.ListLength +1);
        m_pTarget = null;
        
        for(int i =0; i < m_pRewards.childCount; ++i)
        {
            pItem = m_pRewards.Find($"rewad{i}");
            pItem.gameObject.SetActive(false);
            if(i == day -1)
            {
                m_pTarget = pItem;
            }
            
            pItem.Find("ok").gameObject.SetActive(i+1 <= rewarded);
        }
    }

    public void ShowReward()
    {
        if(m_pTarget == null)
        {
            LayoutManager.Instance.InteractableEnabledAll();
            return;
        }

        Transform tm = null;
        
        for(int i = 0; i < m_pRewards.childCount; ++i)
        {
            tm = m_pRewards.GetChild(i);
            tm.gameObject.SetActive(true);

            if(m_pTarget != tm)
            {
                tm.GetComponent<Animation>().Play("blink_s");
            }
        }
        Animation pAnimation = m_pTarget.GetComponent<Animation>();
        pAnimation.Play("blink_target");
        
        m_pBaseState = BaseState.GetInstance(new BaseStateTarget(pAnimation),pAnimation["blink_target"].length, (uint)E_STATE_TYPE.Shake, null, null, exitCallback);
        StateMachine.GetStateMachine().AddState(m_pBaseState);
    }

    IState exitCallback(IState state)
    {
        LayoutManager.Instance.InteractableEnabledAll();
        m_pBaseState = null;
        // BaseStateTarget target = state.GetTarget<BaseStateTarget>();
        // Animation pAnimation = target.GetMainTarget<Animation>();
        // pAnimation.Play("blink_target");
        return null;
    }

    public void UpdateData(JObject data)
    {
        GameContext pGameContext = GameContext.getCtx();
        
        m_iDay = (int)data["day"];
        m_iRewarded = (int)data["rewarded"];
        AttendRewardList pAttendRewardList = pGameContext.GetFlatBufferData<AttendRewardList>(E_DATA_TYPE.AttendReward);
        Setup(pAttendRewardList.AttendRewardByKey((uint)data["attend"]));
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ClearReward()
    {
        int i = m_pRewards.childCount;
        
        RectTransform tm = null;
        Vector2 vec = Vector2.one * 0.5f;
        while(i > 0)
        {
            --i;
            tm = m_pRewards.GetChild(i).Find("icon").GetChild(0).GetComponent<RectTransform>();
            if(tm != null)
            {
                SingleFunc.AddRewardIcon(tm,REWARD_ITEM_NAME);
            }
        }
        
        MainUI.Find("root").localScale = Vector3.one;
        MainUI.gameObject.SetActive(false);
    }

    void Play()
    {
        LayoutManager.Instance.InteractableDisableAll();
        Animation pAnimation = null;
        if(m_pTarget != null)
        {
            pAnimation = m_pTarget.GetComponent<Animation>();
            pAnimation.Play("blink_s");
            pAnimation.PlayQueued("attend_complete");
        }

        if(pAnimation != null)
        {
            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pAnimation),-1, (uint)E_STATE_TYPE.Timer, null,executeCallback, null);
            StateMachine.GetStateMachine().AddState(pBaseState);
        }
        else
        {
            Close();
        }
    }

     bool executeCallback(IState state,float dt,bool bEnd)
    {
        BaseStateTarget target = state.GetTarget<BaseStateTarget>();
        Animation pAnimation = target.GetMainTarget<Animation>();
        if(!pAnimation.IsPlaying("blink_s") && !pAnimation.IsPlaying("attend_complete"))
        {
            Close();
            return true;
        }

        return bEnd;
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(m_iDay > m_iRewarded)
        {
            Play();
        }
        else
        {
            Close();
        }
    }
    
}
