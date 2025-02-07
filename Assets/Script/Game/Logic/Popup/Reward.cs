using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALF;
using UnityEngine.UI;
using ALF.LAYOUT;
using DATA;
using STATEDATA;
using ALF.STATE;
using ALF.MACHINE;
using Newtonsoft.Json.Linq;


public class Reward : IBaseUI
{
    const string REWARD_ITEM_NAME = "RewardItem";
    MainScene m_pMainScene = null;
    Transform m_pTab = null;
    GridLayoutGroup m_pRewards = null;
    bool m_bSkip = false;
    public RectTransform MainUI { get; private set;}

    public Reward(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        m_pTab = null;
        m_pRewards = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Reward : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Reward : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pRewards = MainUI.Find("root/rewards").GetComponent<GridLayoutGroup>();
        m_pTab = MainUI.Find("root/text");
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }
    public void ShowRewardItems()
    {
        Animation pAnimation = null;
        BaseState pBaseState = null;
        float fDelay = 0f;
        for(int i =0; i < m_pRewards.transform.childCount; ++i)
        {
            pAnimation = m_pRewards.transform.GetChild(i).GetComponent<Animation>();
            pBaseState = BaseState.GetInstance(new BaseStateTarget(pAnimation),fDelay, (uint)E_STATE_TYPE.Shake, null, null, exitCallback);
            StateMachine.GetStateMachine().AddState(pBaseState);
            fDelay += 0.3f;
        }
    }

    void SkipReward()
    {
        Animation pAnimation = null;
        BaseStateTarget target = null;
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>((uint)E_STATE_TYPE.Shake);
        for(int i =0; i < list.Count; ++i)
        {
            target = list[i].GetTarget<BaseStateTarget>();
            pAnimation = target.GetMainTarget<Animation>();
            
            if(pAnimation.IsPlaying("blink_target"))
            {
                pAnimation["blink_target"].time = pAnimation["blink_target"].length;
            }
            else
            {
                if(!pAnimation.gameObject.activeSelf)
                {
                    pAnimation.gameObject.SetActive(true);
                    pAnimation.Play("blink_target");
                    pAnimation["blink_target"].time = pAnimation["blink_target"].length;
                }
            }

            list[i].SetExitCallback(null);
            list[i].Exit(true);
        }
    }

    IState exitCallback(IState state)
    {
        if(MainUI != null && MainUI.gameObject.activeSelf)
        {
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            Animation pAnimation = target.GetMainTarget<Animation>();
            
            List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>((uint)E_STATE_TYPE.Shake);
            if(list.Count <= 1)
            {
                StateMachine.GetStateMachine().AddState(BaseState.GetInstance(new BaseStateTarget(pAnimation),pAnimation["blink_target"].length, (uint)E_STATE_TYPE.Shake, null, null, exitSkipCallback));
            }
            pAnimation.gameObject.SetActive(true);
            pAnimation.Play("blink_target");
        }
        
        return null;
    }

    IState exitSkipCallback(IState state)
    {
        m_bSkip = true;
        return null;
    }

    public void SetupRewardItems(JObject data)
    {
        m_pTab.gameObject.SetActive(false);
        m_bSkip = false;
        RectTransform pReward = null;
        JObject item = null;
        JArray pRewards = (JArray)data["rewards"];
        uint id =0;
        int i = pRewards.Count;
        Dictionary<uint,ulong> list = new Dictionary<uint, ulong>();
        while(i > 0)
        {
            --i;
            item = (JObject)pRewards[i];
            id = (uint)item["no"];

            if(id == 1)
            {
                id = GameContext.CASH_ID;
            }
            
            if(!list.ContainsKey(id))
            {
                list.Add(id,0);
            }

            list[id] += (ulong)item["amount"];
        }

        var itr = list.GetEnumerator();
        
        while(itr.MoveNext())
        {
            pReward = SingleFunc.GetRewardIcon(m_pRewards.transform,REWARD_ITEM_NAME,itr.Current.Key,itr.Current.Value);
            pReward.Find("eff").gameObject.SetActive(true);
            pReward.Find("eff").GetComponent<Graphic>().color = Color.white;
            pReward.gameObject.SetActive(false);
        }

        int count = m_pRewards.transform.childCount % m_pRewards.constraintCount;
        float h = (m_pRewards.cellSize.y * count) + ((count -1) * m_pRewards.spacing.y);
        if(h > 460)
        {
            h += 180;
        }
        else
        {
            h = 640;
        }

        pReward = MainUI.Find("root").GetComponent<RectTransform>();
        Vector2 size = pReward.sizeDelta;
        size.y = h;
        pReward.sizeDelta = size;
    }

    public void ShowTab()
    {
        m_pTab.gameObject.SetActive(true);
    }

    void ClearReward()
    {
        m_bSkip = false;
        Transform tm = m_pRewards.transform;
        int i = tm.childCount;
        RectTransform item = null;
        while(i > 0)
        {
            --i;
            item = tm.GetChild(i).GetComponent<RectTransform>();
            SingleFunc.AddRewardIcon(item,REWARD_ITEM_NAME);
        }
        
        LayoutManager.Instance.InteractableEnabledAll();
        MainUI.gameObject.SetActive(false);
    }

    public void Close()
    {
        if(!m_bSkip)
        {    
            m_bSkip = true;
            SkipReward();
            return;
        }
        SingleFunc.HideAnimationDailog(MainUI,ClearReward);
    }
    
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
}
