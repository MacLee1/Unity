using System;
using System.Linq;
// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
using STATEDATA;
using Newtonsoft.Json.Linq;
using ALF.NETWORK;
public class Ground : IBaseUI,IBaseNetwork
{
    E_BUILDING m_eSendRequestBuildingType = E_BUILDING.MAX;
    MainScene m_pMainScene = null;
    
    Button[] m_pBuildingButton = new Button[(int)E_BUILDING.ClubHouse];
    RawImage m_pStadium = null;
    RectTransform[] m_pBusPoints = null;
    RectTransform[,] m_pCarPointList = null;
    ScrollRect m_pBuildingScrollRect = null;
    Animation m_pCrowdAni = null;
    public RectTransform MainUI { get; private set;}

    Dictionary<int,List<GameActor>> m_pMoveObject = new Dictionary<int, List<GameActor>>();

    Color CompleteColor = new Color(0.1372549f,0.7254902f,1);
    public Ground(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        m_pCrowdAni = null;
        m_pBuildingScrollRect = null;
        if(m_pStadium != null)
        {
            m_pStadium.texture = null;
        }
        m_pStadium = null;
        
        int i =0;

        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>((int)E_STATE_TYPE.ObjectMove);
        for(i =0; i < list.Count; ++i)
        {
            ActorManager.Instance.AddBaseActor(list[i].GetTarget<GameActor>());
            list[i].SetExitCallback(null);
            list[i].Exit(true);
        }

        for(i =0; i < m_pBuildingButton.Length; ++i)
        {
            m_pBuildingButton[i] = null;
        }
        m_pBuildingButton = null;

        for(i =0; i < m_pBusPoints.Length; ++i)
        {
            m_pBusPoints[i] = null;
        }
        m_pBusPoints = null;
        int n = 0;
        for(i =0; i < m_pCarPointList.GetLength(0); ++i)
        {
            for(n =0; n < m_pCarPointList.GetLength(1); ++n)
            {
                m_pCarPointList[i,n] = null;
            }
        }
        m_pCarPointList = null;

        var itr = m_pMoveObject.GetEnumerator();

        while(itr.MoveNext())
        {
            itr.Current.Value.Clear();
            // m_pMoveObject[itr.Current.Key] = null;
        }
        m_pMoveObject.Clear();
        m_pMoveObject = null;
    }

    public void OnInit( IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Ground : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Ground : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        if(!ActorManager.IsBaseResource("Bus"))
        {
            ActorManager.AddBaseResource(new GameActor(m_pMainScene,AFPool.GetItem<RectTransform>("Object","Bus"),"Bus"));
        }
        if(!ActorManager.IsBaseResource("Car_G"))
        {
            ActorManager.AddBaseResource(new GameActor(m_pMainScene,AFPool.GetItem<RectTransform>("Object","Car_G"),"Car_G"));
        }
        if(!ActorManager.IsBaseResource("Car_R"))
        {
            ActorManager.AddBaseResource(new GameActor(m_pMainScene,AFPool.GetItem<RectTransform>("Object","Car_R"),"Car_R"));
        }
        if(!ActorManager.IsBaseResource("Car_Y"))
        {
            ActorManager.AddBaseResource(new GameActor(m_pMainScene,AFPool.GetItem<RectTransform>("Object","Car_Y"),"Car_Y"));
        }

        if(!ActorManager.IsBaseResource("BuildingOpenEffect"))
        {
            ActorManager.AddBaseResource(new EffectActor(m_pMainScene,AFPool.GetItem<RectTransform>("Object","BuildingOpenEffect"),"BuildingOpenEffect"));
        }

        m_pBuildingScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        
        Transform item = null;
        
        for(E_BUILDING eType = E_BUILDING.office; eType < E_BUILDING.MAX; ++eType)
        {
            item = m_pBuildingScrollRect.content.Find(eType.ToString());
            if(item != null)
            {
                if(m_pBuildingButton.Length > (int)eType)
                {
                    m_pBuildingButton[(int)eType] = item.GetComponentInChildren<Button>();
                    m_pBuildingButton[(int)eType].gameObject.SetActive(false);
                }

                if(eType == E_BUILDING.Stadium)
                {
                    m_pStadium = item.Find("build").GetComponent<RawImage>();
                }

                if(eType < E_BUILDING.trainingGround )
                {
                    item.Find("build").GetComponent<Graphic>().material.SetFloat("_Saturation",0);
                }
            }
        }
        
        m_pCrowdAni = m_pBuildingScrollRect.content.Find(E_BUILDING.Stadium.ToString()).GetComponentInChildren<Animation>();
        m_pCrowdAni.transform.Find("crowd").gameObject.SetActive(false);

        int i =0;
        for(i =0; i < m_pBuildingButton.Length; ++i)
        {
            LayoutManager.SetReciveUIButtonEvent(m_pBuildingButton[i].transform.parent.GetComponent<RectTransform>() ,ButtonEventCall);
        }

        List<RectTransform> list = new List<RectTransform>();
        List<RectTransform> carlist = new List<RectTransform>();
        List<RectTransform> car1list = new List<RectTransform>();
        List<RectTransform> car2list = new List<RectTransform>();
        int count = 0;
        for(i =0; i < m_pBuildingScrollRect.content.childCount; ++i)
        {
            item = m_pBuildingScrollRect.content.GetChild(i);
            if(item.gameObject.name.Contains("b_point_"))
            {
                list.Add(item.GetComponent<RectTransform>());
            }
            else if(item.gameObject.name.Contains("c0_point_"))
            {
                carlist.Add(item.GetComponent<RectTransform>());
            }
            else if(item.gameObject.name.Contains("c1_point_"))
            {
                car1list.Add(item.GetComponent<RectTransform>());
            }
            else if(item.gameObject.name.Contains("c2_point_"))
            {
                car2list.Add(item.GetComponent<RectTransform>());
            }
        }

        m_pBusPoints = new RectTransform[list.Count];
        int id =0;
        for(i =0; i < list.Count; ++i)
        {
            string[] names = list[i].gameObject.name.Split('_');
            id = int.Parse(names[names.Length -1]);
            m_pBusPoints[id] = list[i];
            m_pBusPoints[id].gameObject.name = names[names.Length -2].ToString();
        }

        count = Mathf.Max(carlist.Count,Mathf.Max(car1list.Count,car2list.Count));
        m_pCarPointList = new RectTransform[3,count];

        for(i =0; i < carlist.Count; ++i)
        {
            string[] names = carlist[i].gameObject.name.Split('_');
            id = int.Parse(names[names.Length -1]);
            m_pCarPointList[0,id] = carlist[i];
            m_pCarPointList[0,id].gameObject.name = names[names.Length -2].ToString();
        }

        for(i =0; i < car1list.Count; ++i)
        {
            string[] names = car1list[i].gameObject.name.Split('_');
            id = int.Parse(names[names.Length -1]);
            m_pCarPointList[1,id] = car1list[i];
            m_pCarPointList[1,id].gameObject.name = names[names.Length -2].ToString();
        }

        for(i =0; i < car2list.Count; ++i)
        {
            string[] names = car2list[i].gameObject.name.Split('_');
            id = int.Parse(names[names.Length -1]);
            m_pCarPointList[2,id] = car2list[i];
            m_pCarPointList[2,id].gameObject.name = names[names.Length -2].ToString();
        }

        SetupStadium();
    }

    public void ChangeStadium()
    {
        LayoutManager.Instance.InteractableDisableAll(null,true);
        RectTransform pObject = m_pCrowdAni.GetComponent<RectTransform>();
        
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pObject),0.4f, (uint)E_STATE_TYPE.ScrollMove, null, this.executeStadiumChangeCallback);
        DilogMoveStateData data = new DilogMoveStateData();

        m_pBuildingScrollRect.content.localScale = Vector3.one *2;
        Vector3 pos = m_pBuildingScrollRect.transform.worldToLocalMatrix.MultiplyPoint(pObject.position);
        data.Distance = Vector3.Distance(Vector3.zero, pos);
        data.Direction = Vector3.Normalize(Vector3.zero - pos);
        data.Original = m_pBuildingScrollRect.content.localPosition;
        m_pBuildingScrollRect.content.localScale = Vector3.one;
        data.FadeDelta = 2;
        data.Out = false;
        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);   
    }

    public void SetupStadium()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.IsLicenseContentsUnlock( CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_2))
        {
            m_pStadium.texture = AFPool.GetItem<Sprite>("Texture","licenseContentsUnlock_2").texture;
        }
        else if(pGameContext.IsLicenseContentsUnlock( CONSTVALUE.E_CONST_TYPE.licenseContentsUnlock_4))
        {
            m_pStadium.texture = AFPool.GetItem<Sprite>("Texture","licenseContentsUnlock_4").texture;
        }
        else
        {
            m_pStadium.texture = AFPool.GetItem<Sprite>("Texture","Stadium0").texture;
        }
    }

    public RectTransform ShowTutorialButton(bool bSkip)
    {
        E_BUILDING eType = GameContext.getCtx().GetBuildingTypeById(1);
        Button pButton = m_pBuildingButton[(int)eType];
        if(!bSkip)
        {
            GameContext.getCtx().SetTutorialBusiness(this);
        }

        return pButton.GetComponent<RectTransform>();
    }

    public void StopPlayGroundObject()
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>((uint)E_STATE_TYPE.ObjectMove);

        for(int i =0; i < list.Count; ++i)
        {
            list[i].Paused = true;
        }
    }

    public void PlayGroundObject()
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>((uint)E_STATE_TYPE.ObjectMove);
        int i = 0;
        if(list.Count > 0)
        {
            for(i =0; i < list.Count; ++i)
            {
                list[i].Paused = false;
            }
            return;
        }

        Button pButton = m_pBuildingButton[(int)E_BUILDING.droneCamera];
        Animation pAnimation = pButton.transform.parent.Find("build").GetComponent<Animation>();
        if(!pAnimation.IsPlaying("Dron_Idle"))
        {
            pAnimation.Play("Dron_Idle");
        }
        
        List<Transform> listPoint = new List<Transform>();
        for(i = 1; i < m_pBusPoints.Length; ++i)
        {
            listPoint.Add(m_pBusPoints[i]); 
        }
        listPoint.Add(m_pBusPoints[0]);
        SetupPlayGroundObject("Bus",1,listPoint,100);

        int count = 12;
        int key =0;
        string[] idList = new string[3]{"Car_G","Car_R","Car_Y"};
        while(count > 0)
        {
            listPoint.Clear();
            key = UnityEngine.Random.Range(0,m_pCarPointList.GetLength(0));
            for(i = 1; i < m_pCarPointList.GetLength(1); ++i)
            {
                if(m_pCarPointList[key,i] == null) continue;
                listPoint.Add(m_pCarPointList[key,i]); 
            }
            listPoint.Add(m_pCarPointList[key,0]);
            
            SetupPlayGroundObject(idList[count % 3],1,listPoint,key);

            --count;
        }
    }

    void SetupPlayGroundObject(string id,int count,List<Transform> list, int key)
    {
        float speed = id == "Bus" ? 150 : 200;
        Transform pint0 = list[list.Count -1];
        Transform pint1 = list[0];

        GameActor pGameActor = null;
        
        GameActor pFrontGameActor = null;
        BaseState pBaseState = null;
        ObjectMoveStateData data = null;
        List<GameActor> pMoveList = null;
        if(m_pMoveObject.ContainsKey(key))
        {
            pMoveList = m_pMoveObject[key];
            int index = pMoveList.Count;
            if(index > 0)
            {
                --index;
                pFrontGameActor = pMoveList[index];
            }
        }
        else
        {
            pMoveList = new List<GameActor>();
            m_pMoveObject.Add(key,pMoveList);
        }

        float delay = 0;
        while(count > 0)
        {
            pGameActor = ActorManager.Instance.GetActor<GameActor>(id);
            pMoveList.Add(pGameActor);
            pGameActor.Speed = speed;
            
            pBaseState = BaseState.GetInstance(pGameActor,-1, (uint)E_STATE_TYPE.ObjectMove, null, this.executeMoveCallback,this.exitMoveCallback);
            pGameActor.MainUI.SetParent(pint0,false);
            pGameActor.ChangeAnimation("idle",0,0);
            data = new ObjectMoveStateData();
            data.Key = key;
            if(pFrontGameActor != null)
            {
                data.FrontMove = pFrontGameActor.MainUI;
            }
            else
            {
                data.FrontMove = null;
            }
            
            data.Next = pint0.worldToLocalMatrix.MultiplyPoint(pint1.position);
            data.Direction = Vector3.Normalize(pint1.position - pint0.position);
            data.Distance = Vector3.Distance(Vector3.zero, data.Next);

            pGameActor.MainUI.localPosition = Vector3.zero;
            
            data.Idle = true;
            data.Time = delay;
            data.Dir = byte.Parse(pint0.gameObject.name);
            
            data.Positons = new List<Transform>();
            int num = id == "Bus" ? list.Count : list.Count -1;
            for(int i = 0; i < num; ++i)
            {
                data.Positons.Add(list[i]); 
            }

            pBaseState.StateData = data;
            StateMachine.GetStateMachine().AddState(pBaseState);
            delay += UnityEngine.Random.Range(0.8f,2f);
            pFrontGameActor = pGameActor;
            --count;
        }
    }

    bool executeMoveCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is ObjectMoveStateData data)
        {
            if(data.Idle)
            {
                data.Time -= dt;
                if(data.Time <= 0)
                {
                    bool bMove = true;
                    GameActor pTarget = state.GetTarget<GameActor>();
                    if(data.FrontMove != null)
                    {
                        bMove = Vector3.Distance(pTarget.MainUI.position,data.FrontMove.position) > 200;
                    }
                    
                    if(bMove)
                    {
                        data.Idle = false;
                        data.Time = 0;
                        pTarget.ChangeAnimation("run",0,data.Dir);
                    }
                }
            }
            else 
            {
                GameActor pTarget = state.GetTarget<GameActor>();
                Vector3 pos = pTarget.MainUI.localPosition;
                pos += data.Direction * dt * pTarget.Speed;
                pTarget.MainUI.localPosition = pos;

                float dis = Vector3.Distance(data.Next,pos);
                if(data.Distance > dis)
                {
                    data.Distance = dis;
                }
                else
                {
                    if(data.Positons.Count > 1)
                    {
                        data.Dir = byte.Parse(data.Positons[0].gameObject.name);
                        pTarget.ChangeAnimation("run",0,data.Dir);
                        data.Next = data.Positons[0].worldToLocalMatrix.MultiplyPoint(data.Positons[1].position);

                        pTarget.MainUI.SetParent(data.Positons[0],false);
                        pTarget.MainUI.localPosition = Vector3.zero;
                        data.Direction = Vector3.Normalize(data.Positons[1].position - data.Positons[0].position);
                        data.Distance = Vector3.Distance(Vector3.zero, data.Next);
                        data.Positons.RemoveAt(0);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
        return bEnd;
    }

    IState exitMoveCallback(IState state)
    {
        if( state.StateData is ObjectMoveStateData data)
        {
            List<Transform> listPoint = new List<Transform>();
            GameActor pGameActor = state.GetTarget<GameActor>();
            string id = pGameActor.ID;
            int key = 100;
            if(id == "Bus")
            {
                for(int i = 1; i < m_pBusPoints.Length; ++i)
                {
                    listPoint.Add(m_pBusPoints[i]); 
                }
                listPoint.Add(m_pBusPoints[0]);
            }
            else
            {
                key = UnityEngine.Random.Range(0,m_pCarPointList.GetLength(0));
                for(int i = 1; i < m_pCarPointList.GetLength(1); ++i)
                {
                    if(m_pCarPointList[key,i] == null) continue;
                    listPoint.Add(m_pCarPointList[key,i]); 
                }
                listPoint.Add(m_pCarPointList[key,0]);
            }
            if(m_pMoveObject.ContainsKey(data.Key))
            {
                List<GameActor> list =  m_pMoveObject[data.Key];
                for(int i =0; i < list.Count; ++i)
                {
                    if(list[i] == pGameActor)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }
            ActorManager.Instance.AddBaseActor(pGameActor);
            pGameActor = null;
            
            SetupPlayGroundObject(id,1,listPoint,key);
        }
        
        return null;
    }

    bool executeStadiumChangeCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            if(data.FadeDelta == 2)
            {
                TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                float fPercent = ALFUtils.EaseIn(condition.GetTimePercent(),2);
                m_pBuildingScrollRect.content.localScale = Vector3.one + (Vector3.one  * fPercent);
                m_pBuildingScrollRect.content.localPosition = data.Original + (data.Direction * (data.Distance * fPercent));

                if(bEnd)
                {
                    bEnd = false;
                    data.FadeDelta = 1f;
                    RectTransform pTarget = state.GetTarget<BaseStateTarget>().GetMainTarget<RectTransform>();
                    EffectActor pEffectActor = ActorManager.Instance.GetActor<EffectActor>("BuildingOpenEffect"); 
                    pEffectActor.ChangeAnimation(null);
                    pEffectActor.MainUI.SetParent(pTarget,false);
                    SoundManager.Instance.PlaySFX("sfx_business_open");
                    SetupStadium();
                }
            }
            else 
            {
                if(data.FadeDelta > 0)
                {
                    bEnd = false;
                    data.FadeDelta -= dt;
                    if(data.FadeDelta <= 0)
                    {
                        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                        RectTransform  pTarget = state.GetTarget<BaseStateTarget>().GetMainTarget<RectTransform>();
                        condition.Reset();
                        condition.SetRemainTime(0.3f);

                        Vector3 scale = m_pBuildingScrollRect.content.localScale;
                        m_pBuildingScrollRect.content.localScale = Vector3.one;
                        Vector3 pos = m_pBuildingScrollRect.transform.worldToLocalMatrix.MultiplyPoint(pTarget.position);
                        data.Distance = Vector3.Distance(Vector3.zero, pos);
                        data.Direction = Vector3.Normalize(Vector3.zero - pos);
                        data.Original = m_pBuildingScrollRect.content.localPosition;
                        m_pBuildingScrollRect.content.localScale = scale;
                    }
                }
                else
                {
                    TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                    float fPercent = ALFUtils.EaseIn(condition.GetTimePercent(),2);
                    m_pBuildingScrollRect.content.localPosition = data.Original + (data.Direction * (data.Distance * fPercent));
                    m_pBuildingScrollRect.content.localScale = Vector3.one * (2 - fPercent);
                    if(bEnd)
                    {
                        LayoutManager.Instance.InteractableEnabledAll(null,true);
                    }
                }
            }
        }
        return bEnd;
    }

    bool executeStadiumCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
            float fPercent = ALFUtils.EaseIn(condition.GetTimePercent(),2);
            float alpha = data.FadeDelta * dt;
            if(data.Out)
            {
                ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("SubMenu"),alpha);
                ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("MenuBottom"),alpha);
                ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("MenuTop"),alpha);
                
                m_pBuildingScrollRect.content.localPosition = data.Original + (data.Direction * (data.Distance * fPercent));
                m_pBuildingScrollRect.content.localScale = Vector3.one * (2 - fPercent);
                if(bEnd)
                {
                    LayoutManager.Instance.InteractableEnabledAll(null,true);
                }
            }
            else
            {
                ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("SubMenu"), alpha);
                ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("MenuBottom"),alpha);
                ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("MenuTop"),alpha);

                m_pBuildingScrollRect.content.localScale = Vector3.one + (Vector3.one  * fPercent);
                m_pBuildingScrollRect.content.localPosition = data.Original + (data.Direction * (data.Distance * fPercent));
                if(bEnd)
                {
                    LayoutManager.Instance.FindUI<Transform>("SubMenu").gameObject.SetActive(false);
                    LayoutManager.Instance.FindUI<Transform>("MenuBottom").gameObject.SetActive(false);
                    LayoutManager.Instance.FindUI<Transform>("MenuTop").gameObject.SetActive(false);

                    LayoutManager.Instance.InteractableEnabledAll(null,true);
                    m_pMainScene.ShowMatchPopup(GameContext.LADDER_ID);
                }
            }
        }
        return bEnd;
    }

    bool executeScrollMoveCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is DilogMoveStateData data)
        {
            if(data.FadeDelta == 2)
            {
                TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                float fPercent = ALFUtils.EaseIn(condition.GetTimePercent(),2);
                m_pBuildingScrollRect.content.localScale = Vector3.one + (Vector3.one  * fPercent);
                m_pBuildingScrollRect.content.localPosition = data.Original + (data.Direction * (data.Distance * fPercent));

                if(bEnd)
                {
                    bEnd = false;
                    data.FadeDelta = 1f;
                    RectTransform pTarget = state.GetTarget<BaseStateTarget>().GetMainTarget<RectTransform>();
                    EffectActor pEffectActor = ActorManager.Instance.GetActor<EffectActor>("BuildingOpenEffect"); 
                    pEffectActor.ChangeAnimation(null);
                    pEffectActor.MainUI.SetParent(pTarget,false);
                    SoundManager.Instance.PlaySFX("sfx_business_open");
                }
            }
            else 
            {
                if(data.FadeDelta > 0)
                {
                    bEnd = false;
                    data.FadeDelta -= dt;
                    if(data.FadeDelta <= 0)
                    {
                        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                        RectTransform  pTarget = state.GetTarget<BaseStateTarget>().GetMainTarget<RectTransform>();
                        UpdateBuilding(true,(E_BUILDING)Enum.Parse(typeof(E_BUILDING), pTarget.gameObject.name));
                        condition.Reset();
                        condition.SetRemainTime(0.3f);

                        Vector3 scale = m_pBuildingScrollRect.content.localScale;
                        m_pBuildingScrollRect.content.localScale = Vector3.one;
                        Vector3 pos = m_pBuildingScrollRect.transform.worldToLocalMatrix.MultiplyPoint(pTarget.position);
                        data.Distance = Vector3.Distance(Vector3.zero, pos);
                        data.Direction = Vector3.Normalize(Vector3.zero - pos);
                        data.Original = m_pBuildingScrollRect.content.localPosition;
                        m_pBuildingScrollRect.content.localScale = scale;
                    }
                }
                else
                {
                    TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
                    float fPercent = ALFUtils.EaseIn(condition.GetTimePercent(),2);
                    m_pBuildingScrollRect.content.localPosition = data.Original + (data.Direction * (data.Distance * fPercent));
                    m_pBuildingScrollRect.content.localScale = Vector3.one * (2 - fPercent);
                    if(bEnd)
                    {
                        LayoutManager.Instance.InteractableEnabledAll(null,true);
                        m_pMainScene.ShowManagement();
                        m_pMainScene.SetMainButton(0);
                    }
                }
            }
        }
        return bEnd;
    }

    void CreateBuildingCompleteEffect(JObject data)
    {
        if(data == null) return;

        if(data.ContainsKey("rewards"))
        {
            CompleteEffectActor pCompleteEffectActor = null;

            JArray pBusinesses = (JArray)data["businesses"];
            JArray pArray = (JArray)data["rewards"];
            JObject pObject = null;
            uint no =0;
            uint amount =0;
            string name = null;
            E_BUILDING eType;
            float delay = 0;
            for(int i = 0; i < pArray.Count; ++i)
            {
                pObject = (JObject)pBusinesses[i];
                no = (uint)pObject["no"];
                name = GameContext.getCtx().GetBuildingNameByBusinessID(no);
                pObject = (JObject)pArray[i];
                no = (uint)pObject["no"];
                amount = (uint)pObject["amount"];
                
                eType = (E_BUILDING)Enum.Parse(typeof(E_BUILDING), name);

                pCompleteEffectActor = ActorManager.Instance.GetActor<CompleteEffectActor>("CompleteBusiness"); 
                pCompleteEffectActor.ChangeColor(eType == E_BUILDING.trainingGround ? Color.blue:Color.green);

                pCompleteEffectActor.ChangeIcon(no.ToString());
                pCompleteEffectActor.SetupReward(amount);
                pCompleteEffectActor.ChangeAnimation("complete",delay);

                Button pButton = m_pBuildingButton[(int)eType];
                pCompleteEffectActor.MainUI.SetParent(pButton.transform.parent,false);
                pCompleteEffectActor.MainUI.position = pButton.transform.position;
                delay += 0.5f;
            }
        }
    }
    
    public void SetupTimer()
    {
        for(E_BUILDING eType = E_BUILDING.office; eType < E_BUILDING.ClubHouse; ++eType)
        {
            UpdateBuilding(eType < E_BUILDING.trainingGround, eType);
        }
    }

    public void FocusStadium(bool bFocus)
    {
        LayoutManager.Instance.InteractableDisableAll(null,true);
        RectTransform pObject = m_pCrowdAni.GetComponent<RectTransform>();
        
        if(bFocus)
        {
            SoundManager.Instance.ChangeBGMVolume(0.0f,0.5f);
            GameContext.getCtx().PlayCrowdSFX();
            EffectActor pEffectActor = ActorManager.Instance.GetActor<EffectActor>("BuildingOpenEffect"); 
            pEffectActor.ChangeAnimation(null);
            pEffectActor.MainUI.SetParent(pObject,false);
            ShowCrowd(true);

            ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("SubMenu"),1);
            ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("MenuBottom"),1);
            ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("MenuTop"),1);
        }
        else
        {
            LayoutManager.Instance.FindUI<Transform>("SubMenu").gameObject.SetActive(true);
            LayoutManager.Instance.FindUI<Transform>("MenuBottom").gameObject.SetActive(true);
            LayoutManager.Instance.FindUI<Transform>("MenuTop").gameObject.SetActive(true);

            ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("SubMenu"),0);
            ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("MenuBottom"),0);
            ALFUtils.FadeObject(LayoutManager.Instance.FindUI<Transform>("MenuTop"),0);

            SoundManager.Instance.ChangeBGMVolume(1,0.5f);
            ShowCrowd(false);
        }
        
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pObject),0.4f, (uint)E_STATE_TYPE.ScrollMove, null, this.executeStadiumCallback);
        DilogMoveStateData data = new DilogMoveStateData();

        m_pBuildingScrollRect.content.localScale = Vector3.one *2;
        Vector3 pos = m_pBuildingScrollRect.transform.worldToLocalMatrix.MultiplyPoint(pObject.position);
        data.Distance = Vector3.Distance(Vector3.zero, pos);
        data.Direction = Vector3.Normalize(Vector3.zero - pos);
        data.Original = m_pBuildingScrollRect.content.localPosition;
        data.Out = !bFocus;
        data.FadeDelta = data.Out ? 1f /0.4f : 1f / -0.4f;
        m_pBuildingScrollRect.content.localScale = Vector3.one;
        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    public void FocusBuildingObject(uint id)
    {
        LayoutManager.Instance.InteractableDisableAll(null,true);
        E_BUILDING eType = GameContext.getCtx().GetBuildingTypeById(id);
        
        RectTransform pObject = m_pBuildingButton[(int)eType].transform.parent.GetComponent<RectTransform>();
        if(!pObject.gameObject.activeSelf) pObject.gameObject.SetActive(true);
        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pObject),0.4f, (uint)E_STATE_TYPE.ScrollMove, null, this.executeScrollMoveCallback);
        DilogMoveStateData data = new DilogMoveStateData();

        m_pBuildingScrollRect.content.localScale = Vector3.one *2;
        Vector3 pos = m_pBuildingScrollRect.transform.worldToLocalMatrix.MultiplyPoint(pObject.position);
        data.Distance = Vector3.Distance(Vector3.zero, pos);
        data.Direction = Vector3.Normalize(Vector3.zero - pos);
        data.Original = m_pBuildingScrollRect.content.localPosition;
        m_pBuildingScrollRect.content.localScale = Vector3.one;
        data.FadeDelta = 2;
        pBaseState.StateData = data;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }

    public void UpdateBuilding(bool bBusiness, E_BUILDING eType)
    {
        GameContext pGameContext = GameContext.getCtx();
        bool bActive = false;
        Button pButton = m_pBuildingButton[(int)eType];
        if(bBusiness)
        {
            bActive = pGameContext.GetBusinessBuildingActiveByName(eType.ToString());
        }
        else
        {
            bActive = true;
        }
        
        if(bActive)
        {
            if(!pButton.transform.parent.gameObject.activeSelf) pButton.transform.parent.gameObject.SetActive(true);

            Graphic pGraphic = pButton.transform.parent.Find("build").GetComponent<Graphic>();
            if(pGraphic.material != null)
            {
                pGraphic.material = null;
            }

            UpdateBuildingTimer(bBusiness, eType);
        }
    }
    public void UpdateBuildingTimer(bool bBusiness, E_BUILDING eType)
    {
        GameContext pGameContext = GameContext.getCtx();

        Button pButton = m_pBuildingButton[(int)eType];
        ulong total = 0;
        float percent = 0;
        if(bBusiness)
        {
            total = pGameContext.GetBusinessRedundancyByName(eType.ToString());
            percent = pGameContext.GetPercentBusinessTotalRewardByName(eType.ToString());
        }
        else
        {
            total = pGameContext.GetTrainingTotalReward();
            percent = pGameContext.GetPercentTrainingTotalReward(total);
        }

        if(total <= 0)
        {
            pButton.gameObject.SetActive(false);
        }
        else
        {
            Image pGauge = pButton.transform.Find("gauge").GetComponent<Image>();
            TMPro.TMP_Text pText = pButton.transform.Find("text").GetComponent<TMPro.TMP_Text>();
            pText.SetText(ALFUtils.NumberToString(total));
            pGauge.fillAmount = percent;
            pButton.GetComponent<RawImage>().color = percent >= 1 ? CompleteColor : Color.black;
            Animation pAnimation = pButton.GetComponent<Animation>();
            List<BaseState> pBaseStateList = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(pAnimation);
            for(int i = 0; i < pBaseStateList.Count; ++i)
            {
                StateMachine.GetStateMachine().RemoveState(pBaseStateList[i]);
            }

            if(!pButton.gameObject.activeSelf)
            {
                pButton.enabled = true;
                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pAnimation),-1f, (uint)E_STATE_TYPE.PlayAndEnable, this.enterAnimationPlayCallback, this.executeAnimationPlayCallback,this.exitAnimationPlayAndEnableCallback);
                pBaseState.StateData = new AnimationStateData("bubble_show");
                StateMachine.GetStateMachine().AddState(pBaseState);
            }
        }
    }

    void ShowCrowd(bool bActive)
    {
        m_pCrowdAni.transform.Find("crowd").gameObject.SetActive(bActive);
        if(bActive)
        {
            m_pCrowdAni.Play();
        }
        else
        {
            m_pCrowdAni.Stop();
        }
    }

    public void PlayGetRewardBuildingEffect(Transform pBuilding)
    {
        Animation pAnimation = pBuilding.GetComponent<Animation>();
        if(pAnimation != null)
        {
            SoundManager.Instance.PlaySFX("sfx_collect");
            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(pAnimation),-1f, (uint)E_STATE_TYPE.PlayAndHide, this.enterAnimationPlayCallback, this.executeAnimationPlayCallback,this.exitAnimationPlayAndHideCallback);
            pBaseState.StateData = new AnimationStateData("bubble_hide");
            StateMachine.GetStateMachine().AddState(pBaseState);
        }
    }

    void enterAnimationPlayCallback(IState state)
    {
        if (state.StateData is AnimationStateData data)
        {   
            Animation target = state.GetTarget<BaseStateTarget>().GetMainTarget<Animation>();
            LayoutManager.Instance.InteractableByTarget(false,target.transform);

            AnimationState pAnimationState = target[data.AnimationName];
            TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
            float duration = pAnimationState.length;
            if(data.Delay > 0)
            {
                target.gameObject.SetActive(false);
                duration += data.Delay;
            }
            else
            {
                target.gameObject.SetActive(true);
                target.Play(data.AnimationName);
            }

            condition.SetRemainTime(duration);
        }
    }
    bool executeAnimationPlayCallback(IState state,float dt,bool bEnd)
    {
        if (state.StateData is AnimationStateData data)
        {       
            if(data.Delay > 0)
            {
                data.Delay -= dt;
                if(data.Delay <= 0)
                {
                    Animation target = state.GetTarget<BaseStateTarget>().GetMainTarget<Animation>();
                    data.Delay = -1;
                    target.gameObject.SetActive(true);
                    target.Play(data.AnimationName);
                }

                return false;
            }
        }
        return bEnd;
    }
    IState exitAnimationPlayAndHideCallback(IState state)
    {
        if (state.StateData is AnimationStateData data)
        {    
            state.GetTarget<BaseStateTarget>().GetMainTarget<Animation>().gameObject.SetActive(false);
        }

        return null;
    }

    IState exitAnimationPlayAndEnableCallback(IState state)
    {
        if (state.StateData is AnimationStateData data)
        { 
            Animation pAnimation = state.GetTarget<BaseStateTarget>().GetMainTarget<Animation>();
            LayoutManager.Instance.InteractableByTarget(true,pAnimation.transform);
            pAnimation.transform.localScale = Vector3.one;
        }

        return null;
    }

    public void Close()
    {
        
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        PlayGetRewardBuildingEffect(sender.transform);
        m_eSendRequestBuildingType = (E_BUILDING)Enum.Parse(typeof(E_BUILDING), root.gameObject.name);
        Button pButton = m_pBuildingButton[(int)m_eSendRequestBuildingType];
        pButton.enabled = false;
        GameContext pGameContext = GameContext.getCtx();
        pGameContext.HoldUpdateBusinessTime(m_eSendRequestBuildingType,true);

        if(m_eSendRequestBuildingType == E_BUILDING.trainingGround)
        {
            m_pMainScene.RequestAfterCall(E_REQUEST_ID.business_rewardTraining,null);
        }
        else
        {
            JObject pJObject = new JObject();
            pJObject["no"] = pGameContext.GetBusinessIDByBuildingName(root.gameObject.name);
            m_pMainScene.RequestAfterCall(E_REQUEST_ID.business_reward,pJObject);
        }
    }

    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
        if(data == null) return;
        E_REQUEST_ID eID = (E_REQUEST_ID)data.Id;
        
        if(bSuccess)
        {
            if(eID == E_REQUEST_ID.business_reward || eID == E_REQUEST_ID.business_rewardTraining || eID == E_REQUEST_ID.tutorial_business )
            {
                CreateBuildingCompleteEffect(data.Json);
            }
            else if( eID == E_REQUEST_ID.business_levelUp)
            {
                m_eSendRequestBuildingType = E_BUILDING.MAX;
            }
        }
        else
        {
            if(eID == E_REQUEST_ID.business_reward || eID == E_REQUEST_ID.business_rewardTraining || eID == E_REQUEST_ID.tutorial_business )
            {
                GameContext.getCtx().HoldUpdateBusinessTime(m_eSendRequestBuildingType,false);
                        
                Button pButton = m_pBuildingButton[(int)m_eSendRequestBuildingType];
                pButton.gameObject.SetActive(false);
                UpdateBuildingTimer(true,m_eSendRequestBuildingType);
                m_eSendRequestBuildingType = E_BUILDING.MAX;
            }
        }
    }
}
