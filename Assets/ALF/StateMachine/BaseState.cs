using System.Collections;
using System.Collections.Generic;
using ALF.CONDITION;
using ALF.MACHINE;
using UnityEngine;

namespace ALF.STATE
{
    public interface IStateData
    {
        void Dispose();
    }

    public interface IState : IBase
    {
        void Enter(StateMachine pStateMachine);
        void Exit( bool bRemove = false);
        bool Execute(float dt);
        bool IsVaild();

        T GetTarget<T>() where T : IStateTarget;
        void SetTarget( IStateTarget target);
        
        T GetCondition<T>() where T : ICondition;
        void SetCondition( ICondition condition);

        StateMachine GetStateMachine();

        
        uint StateType {get;}
        bool Paused {get; set;}
        int Priority {get; set;}
        // bool ForceRemoved {get;}
        IStateData StateData {get; set;}

        void SetEnterCallback(System.Action<IState> _enterCallback);
        void SetExecuteCallback(System.Func<IState,float,bool,bool> _executeCallback);
        void SetExitCallback(System.Func<IState,IState> _exitCallback);
        void ClearCallback();
    }

    public class BaseStateTarget : IStateTarget
    {
        Object m_pTarget = null;
        public BaseStateTarget(Object target)
        {
            ALFUtils.Assert(target != null, "BaseStateTarget : target is null!!");
            m_pTarget = target;
        }

        public T GetMainTarget<T>() where T : Object
        {
            if(m_pTarget != null)
            {
                return (T)m_pTarget;
            }

            return default(T);
        }
        
        public void Dispose()
        {
            m_pTarget = null;
        }    
    }

    public class BaseState : IState
    {
        IStateData           m_data = null;
        uint                m_eState = 0;
        // bool                m_bForceRemoved = false;
        bool                m_bPaused = false;
        IStateTarget        m_pTarget = null;
        ICondition          m_pkCondition = null;
        int                 m_iPriority = 0;
        StateMachine        m_pStateMachine = null;
        System.Action<IState> m_enterCallback = null;
        System.Func<IState,float,bool,bool>     m_executeCallback = null;
        System.Func<IState,IState>        m_exitCallback = null;

        static public BaseState GetInstance(IStateTarget target,float rt, uint eState, System.Action<IState> _enterCallback = null, System.Func<IState,float,bool,bool> _executeCallback = null, System.Func<IState,IState> _exitCallback = null, int priority = 0)
        {
            BaseState pState = StateCache.Instance.GetState<BaseState>(eState);
            if(pState == null)
            {
                pState = new BaseState();
                pState.m_eState = eState;
            }
            
            pState.Priority = priority;
            pState.SetEnterCallback(_enterCallback);
            pState.SetExecuteCallback(_executeCallback);
            pState.SetExitCallback(_exitCallback);
        
            TimeOutCondition pCondition = StateCache.Instance.GetCondition<TimeOutCondition>(E_CONDITION.TIMEOUT);
            if(pCondition == null)
            {
                pCondition = new TimeOutCondition(rt);
                pCondition.Reset();
            }
            pCondition.SetRemainTime(rt);
            pState.SetTarget(target);
            pState.SetCondition(pCondition);
            return pState;
        }
        public void Dispose()
        {
            if(m_data != null)
            {
                m_data.Dispose();
                m_data = null;
            }
            
            m_pStateMachine = null;
            
            if(m_pTarget != null)
            {
                if(m_pTarget is BaseStateTarget pBaseStateTarget)
                {
                    pBaseStateTarget.Dispose();
                }
                m_pTarget = null;
            }
            
            m_pkCondition = null;
            m_bPaused = false;
            Priority = 0;
            ClearCallback();
        }
        public void ClearCallback()
        {
            m_enterCallback = null;
            m_executeCallback = null;
            m_exitCallback = null;
        }
        public void SetEnterCallback(System.Action<IState> _enterCallback)
        {
            m_enterCallback = _enterCallback;
        }
        public void SetExecuteCallback(System.Func<IState,float,bool,bool> _executeCallback)
        {
            m_executeCallback = _executeCallback;
        }
        public void SetExitCallback(System.Func<IState,IState> _exitCallback)
        {
            m_exitCallback = _exitCallback;
        }

        public StateMachine GetStateMachine()
        {
            return m_pStateMachine;
        }

        protected BaseState() 
        {
            m_pStateMachine = null;
            m_data = null;
            m_pTarget = null;
            m_pkCondition = null;
            m_bPaused = false;
            Priority = 0;
            ClearCallback();
        }

        public void Enter(StateMachine pStateMachine)
        {
            m_pStateMachine = pStateMachine;
            if(m_enterCallback != null)
            {
                m_enterCallback(this);
            }
        }
        public void Exit( bool bForceRemoved)
        {
            if(!IsVaild()) return;
            
            if(m_exitCallback != null)
            {
                IState nextState = m_exitCallback(this);
                if(nextState != null)
                {
                    m_pStateMachine.AddState(nextState);
                }
            }    
            
            if(bForceRemoved)
            {
                m_pStateMachine.RemoveStateList(this);
            }
        }
        public bool Execute(float dt)
        {
            bool end = true;
            if(m_pkCondition != null)
                end = m_pkCondition.Execute(dt);
            
            if(m_executeCallback != null)
                end = m_executeCallback(this,dt,end);
            
            return end;
        }
        public bool IsVaild()
        {
            return m_pStateMachine != null && m_pkCondition != null;
        }
        public T GetTarget<T>() where T : IStateTarget
        {
            return (T)m_pTarget;
        }
        public void SetTarget( IStateTarget target)
        {
            m_pTarget = target;
        }
        
        public T GetCondition<T>() where T : ICondition
        {
            return (T)m_pkCondition;
        }
        public void SetCondition( ICondition condition)
        {
            m_pkCondition = condition;
        }

        // public bool ForceRemoved  {get{return m_bForceRemoved;} }

        public uint StateType {get{return m_eState;} }
        public bool Paused {get{return m_bPaused;} set{m_bPaused = value;}}
        public int Priority {get{return m_iPriority;} set{m_iPriority = value;}}
        public IStateData StateData {get{return m_data;} set{m_data = value;}}
        
    }

    public class StateCache : IBase
    {
        Dictionary<uint,List<IState>> m_stateDatas = new Dictionary<uint,List<IState>>(32);
        Dictionary<E_CONDITION,List<ICondition>> m_conditionDatas = new Dictionary<E_CONDITION,List<ICondition>>(32);
        protected StateCache(){}

        static StateCache instance;
        public static StateCache Instance
        {
            get {return instance;}
        }
        
        public static bool InitInstance()
        {
            if (instance == null)
            {
                instance = new StateCache();
                return true;
            }
            return false;
        }
        public void Dispose()
        {
            RemoveAll();
        }
        public void AddState(IState state)
        {
            ICondition _condition = state.GetCondition<ICondition>();
            E_CONDITION eum = 0;
            if(_condition != null)
            {
                eum = _condition.ConditionEum;
                _condition.Reset();

                if(m_conditionDatas.ContainsKey(eum) && m_conditionDatas[eum] != null)
                {
                    m_conditionDatas[eum].Add(_condition);
                }
                else
                {
                    m_conditionDatas[eum] = new List<ICondition>();
                    m_conditionDatas[eum].Add(_condition);
                }
            }

            uint stateType = state.StateType;
            state.Dispose();
            
            if(m_stateDatas.ContainsKey(stateType) && m_stateDatas[stateType] != null)
            {
                m_stateDatas[stateType].Add(state);
            }
            else
            {
                m_stateDatas[stateType] = new List<IState>();
                m_stateDatas[stateType].Add(state);
            }
        }
        public T GetState<T>(uint stateEum) where T : IState
        {
            if(m_stateDatas.ContainsKey(stateEum) && m_stateDatas[stateEum] != null)
            {
                int count = m_stateDatas[stateEum].Count;
                if(count > 0)
                {
                    IState temp = m_stateDatas[stateEum][count -1];
                    m_stateDatas[stateEum].RemoveAt(count -1);
                    temp.Dispose();
                    return (T)temp;
                }
            }
            
            return default(T);
        }
        public void RemoveAll()
        {
            var itr = m_stateDatas.GetEnumerator();

            // for ( int i = 0; i < m_stateDatas.Count; ++i)
            while(itr.MoveNext())
            {
                var e = itr.Current.Value.GetEnumerator();
                
                // for ( int n = 0; n < itr.Current.Value.Count; ++n)
                while(e.MoveNext())
                {
                    e.Current?.Dispose();
                }

                itr.Current.Value.Clear();
            }

            m_stateDatas.Clear();
            
            var itr1 = m_conditionDatas.GetEnumerator();

            while(itr1.MoveNext())// for ( int i = 0; i < m_conditionDatas.Count; ++i)
            {
                var e = itr1.Current.Value.GetEnumerator();
                
                while(e.MoveNext())// for ( int n = 0; n < itr1.Current.Value.Count; ++n)
                {
                    e.Current.Reset();
                }

                itr1.Current.Value.Clear();
            }
            m_stateDatas.Clear();
        }

        public T GetCondition<T>(E_CONDITION conditionEum) where T : ICondition
        {
            if(m_conditionDatas.ContainsKey(conditionEum) && m_conditionDatas[conditionEum] != null)
            {
                int count = m_conditionDatas[conditionEum].Count;
                if(count > 0)
                {
                    ICondition condition = m_conditionDatas[conditionEum][count -1];
                    condition.Reset();
                    m_conditionDatas[conditionEum].RemoveAt(count -1);
                    return (T)condition;
                }
            }

            return default(T);
        }
    }
}