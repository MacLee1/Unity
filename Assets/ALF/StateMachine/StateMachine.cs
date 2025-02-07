using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ALF.STATE;

namespace ALF.MACHINE
{
    public class Stater : IBase
    {
        uint m_StateType = 0;
        Object m_pTarget = null;

        public Stater(uint type, Object target)
        {
            m_StateType = type;
            m_pTarget = target;
        }

        public Object GetTarget()
        {
            return m_pTarget;
        }
        public uint GetStateType()
        {
            return m_StateType;
        }
        public void Dispose()
        {
            m_pTarget = null;
        }
    }

    public class StateMachine : IBase, IUpdate
    {
        Dictionary<Stater,List<IState>> m_mapStaterList = new Dictionary<Stater,List<IState>>(32);
        List<IState> m_vecStateList = new List<IState>(32);
        List<IState> m_addStateList = new List<IState>();
        List<IState> m_vecBackStateList = new List<IState>(32);
        List<IState> m_vecFrontStateList = new List<IState>(32);
        List<IState> m_vecRemoveStateList = new List<IState>(32);
        bool m_bPaused = false;
        static StateMachine instance = null;

        protected StateMachine(){}

        public static StateMachine Create()
        {
            return new StateMachine();
        }
        public static bool InitDefaultInstance()
        {
            if(instance == null)
            {
                StateMachine.SetStateMachine(StateMachine.Create());
                StateCache.InitInstance();
                return true;
            }

            return false;
        }
        static public void SetStateMachine(StateMachine pStateMachine)
        {
            if(instance != null)
            {
                instance.RemoveAll();
                instance = null;
            }

            instance = pStateMachine;
        }
        static public StateMachine GetStateMachine()
        {
            return instance;
        }

        static public void ScheduleUpdate(bool bPaused)
        {
            StateMachine stateMachine = StateMachine.GetStateMachine();
            stateMachine.m_bPaused = bPaused;
            Director.Instance.AddSchedule(stateMachine);
        }

        static public void UnscheduleUpdate()
        {
            StateMachine stateMachine = StateMachine.GetStateMachine();
            stateMachine.Pause();
            Director.Instance.RemoveSchedule(stateMachine);
            stateMachine.Dispose();
        }

        static public void ResumeSchedulerAndActions()
        {
            StateMachine.GetStateMachine().Resume();
        }
        static public void PauseSchedulerAndActions()
        {
            StateMachine.GetStateMachine().Pause();
        }

        public void Resume()
        {
            m_bPaused = false;
        }
        public void Pause()
        {
            m_bPaused = true;
        }
        
        public void Dispose()
        {
            RemoveAll();
        }

        public List<T> GetCurrentTargetStates<T>( List<uint> eStateList,Object pTarget = null) where T : IState
        {
            List<T> list = new List<T>();
            uint temp = 0;
            bool bAdd = false;
            for( int i =0; i < eStateList.Count; ++i)
            {
                temp = eStateList[i];
                var itr = m_mapStaterList.GetEnumerator();
                
                while(itr.MoveNext()) 
                {
                    bAdd = itr.Current.Key.GetStateType() == temp;
                    if(bAdd)
                    {
                        if(pTarget != null)
                        {
                            bAdd = itr.Current.Key.GetTarget() == pTarget;
                        }
                    }

                    if(bAdd)
                    {
                        var e = itr.Current.Value.GetEnumerator();
                        
                        while(e.MoveNext())
                        {
                            T pState = (T)e.Current;
                            if(pState != null)
                            {
                                list.Add(pState);
                            }
                        }
                    }
                }
            }

            return list;
        }
        public List<T> GetCurrentTargetStates<T>(Object target, uint eState) where T : IState
        {
            List<T> list = new List<T>();
            if(target != null)
            {
                var itr = m_mapStaterList.GetEnumerator();

                while( itr.MoveNext())
                {               
                    if(itr.Current.Key.GetStateType() == eState && itr.Current.Key.GetTarget() == target)
                    {
                        var e = itr.Current.Value.GetEnumerator();
                        while(e.MoveNext())
                        {
                            T pState = (T)e.Current;
                            if(pState != null)
                            {
                                list.Add(pState);
                            }
                        }
                        return list;
                    }
                }
            }
            return list;
        }
    
        public List<T> GetCurrentTargetStates<T>( Object target) where T : IState
        {
            List<T> list = new List<T>();
            if(target != null)
            {
                var itr = m_mapStaterList.GetEnumerator();

                while(itr.MoveNext())
                {
                    if(itr.Current.Key.GetTarget() == target)
                    {
                        var e = itr.Current.Value.GetEnumerator();
                        while(e.MoveNext())
                        {
                            T pState = (T)e.Current;
                            if(pState != null)
                            {
                                list.Add(pState);
                            }
                        }
                    }
                }
            }
            
            return list;
        }

        public bool IsCurrentTargetStates(Object target, uint eState)
        {
            if(target != null)
            {
                var itr = m_mapStaterList.GetEnumerator();

                while( itr.MoveNext())
                {               
                    if(itr.Current.Key.GetStateType() == eState && itr.Current.Key.GetTarget() == target)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsCurrentTargetStates( Object target)
        {
            if(target != null)
            {
                var itr = m_mapStaterList.GetEnumerator();

                while(itr.MoveNext())
                {
                    if(itr.Current.Key.GetTarget() == target)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public bool IsCurrentTargetStates( uint eState)
        {
            var itr = m_mapStaterList.GetEnumerator();

            while(itr.MoveNext())
            {
                if(itr.Current.Key.GetStateType() == eState)
                {
                    return true;
                }
            }
            return false;
        }
    
        public List<T> GetCurrentTargetStates<T>( uint eState)
        {
            List<T> list = new List<T>();
            
            var itr = m_mapStaterList.GetEnumerator();

            while(itr.MoveNext())
            {
                if(itr.Current.Key.GetStateType() == eState)
                {
                    var e = itr.Current.Value.GetEnumerator();
                    while(e.MoveNext())
                    {
                        T pState = (T)e.Current;
                        if(pState != null)
                        {
                            list.Add(pState);
                        }
                    }
                }
            }
            return list;
        }

        public void AddStaterList(IState pkState)
        {
            if(pkState != null)
            {
                Object pTarget = pkState.GetTarget<IStateTarget>().GetMainTarget<Object>();
                uint type = pkState.StateType;
                var itr = m_mapStaterList.GetEnumerator();
                
                while(itr.MoveNext())
                {
                    if(itr.Current.Key.GetStateType() == type && itr.Current.Key.GetTarget() == pTarget)
                    {
                        itr.Current.Value.Add(pkState);
                        return;
                    }
                }
                
                List<IState> list = new List<IState>();
                list.Add(pkState);
                m_mapStaterList[new Stater(type,pTarget)] = list;
            }
        }

        public void AddState( IState pkState)
        {
            if(pkState != null)
            {
                m_addStateList.Add(pkState);
                AddStaterList(pkState);
                pkState.Enter(this);
            }
        }

        public void RemoveAll()
        {
            int i = 0;
            
            for(i = 0; i < m_addStateList.Count; ++i)
            {
                if(!m_vecRemoveStateList.Contains(m_addStateList[i]))
                {
                    m_vecRemoveStateList.Add(m_addStateList[i]);
                }
            }
            for(i = 0; i < m_vecBackStateList.Count; ++i)
            {
                if(!m_vecRemoveStateList.Contains(m_vecBackStateList[i]))
                {
                    m_vecRemoveStateList.Add(m_vecBackStateList[i]);
                }
            }
            for( i = 0; i < m_vecStateList.Count; ++i)
            {
                if(!m_vecRemoveStateList.Contains(m_vecStateList[i]))
                {
                    m_vecRemoveStateList.Add(m_vecStateList[i]);
                }
            }

            for(i = 0; i < m_vecFrontStateList.Count; ++i)
            {
                if(!m_vecRemoveStateList.Contains(m_vecFrontStateList[i]))
                {
                    m_vecRemoveStateList.Add(m_vecFrontStateList[i]);
                }
            }

            for(i = 0; i < m_vecRemoveStateList.Count; ++i)
            {
                StateCache.Instance.AddState(m_vecRemoveStateList[i]);
            }

            var itr = m_mapStaterList.GetEnumerator();
            
            while(itr.MoveNext())
            {
                itr.Current.Key.Dispose();
                itr.Current.Value.Clear();
            }
            m_vecRemoveStateList.Clear();
            m_vecFrontStateList.Clear();
            m_mapStaterList.Clear();
            m_vecStateList.Clear();
            m_vecBackStateList.Clear();
            m_addStateList.Clear();
        }

        bool DoRemoveStateList(IState pState,List<IState> list)
        {
            for(int i = 0; i < list.Count; ++i)
            {
                if(pState == list[i])
                {
                    if(!m_vecRemoveStateList.Contains(pState))
                    {
                        m_vecRemoveStateList.Add(pState);
                    }

                    if(m_addStateList.Contains(pState))
                    {
                        m_addStateList.Remove(pState);
                    }
                    
                    return true;
                } 
            }
            return false;
        }

        public void RemoveStateList(IState pState)
        {
            if(pState == null) return;

            pState.Paused = true;
            if(!m_vecRemoveStateList.Contains(pState))
            {
                m_vecRemoveStateList.Add(pState);
            }

            if(m_addStateList.Contains(pState))
            {
                m_addStateList.Remove(pState);
            }
        }
 
        public void RemoveState( Object target)
        {
            var it = m_mapStaterList.GetEnumerator();
            while(it.MoveNext())
            {
                if(it.Current.Key.GetTarget() == target)
                {
                    List<IState> list = it.Current.Value;

                    for(int i =0; i < list.Count; ++i)
                    {
                        RemoveStateList(list[i]);
                    }
                }
            }
        }

        public void RemoveState(uint tag)
        {
            var it = m_mapStaterList.GetEnumerator();
            while(it.MoveNext())
            {
                if(it.Current.Key.GetStateType() == tag)
                {
                    List<IState> list = it.Current.Value;

                    for(int i =0; i < list.Count; ++i)
                    {
                        RemoveStateList(list[i]);
                    }
                }
            }
        }

        public void RemoveState( Object target,uint tag)
        {
            var it = m_mapStaterList.GetEnumerator();
            while(it.MoveNext())
            {
                if(it.Current.Key.GetStateType() == tag && it.Current.Key.GetTarget() == target)
                {
                    List<IState> list = it.Current.Value;

                    for(int i =0; i < list.Count; ++i)
                    {
                        RemoveStateList(list[i]);
                    }
                    
                    return;
                }
            }
        }

        public void RemoveState( IState pkState)
        {
            if(pkState != null)
            {
                RemoveStateList(pkState);
            }
        }

        public bool RemoveStater( IState pkState)
        {
            if(pkState != null)
            {
                Object pTarget = pkState.GetTarget<IStateTarget>().GetMainTarget<Object>();
                uint type = pkState.StateType;
                
                var it = m_mapStaterList.GetEnumerator();
                while(it.MoveNext())
                {
                    if(it.Current.Key.GetStateType() == type && it.Current.Key.GetTarget() == pTarget)
                    {
                        int i =m_mapStaterList[it.Current.Key].Count;
                        while(i > 0)
                        {
                            --i;
                            if(m_mapStaterList[it.Current.Key][i] == pkState)
                            {
                                m_mapStaterList[it.Current.Key].RemoveAt(i);
                                if(m_mapStaterList[it.Current.Key].Count == 0)
                                {
                                    it.Current.Key.Dispose();
                                    m_mapStaterList.Remove(it.Current.Key);
                                }
                                return true;
                            }
                        }
                    }
                }
            }
            
            return false;
        }
        
        bool DoRemoveState(IState pRemoveState, ref List<IState> list)
        {
            for(int i = 0; i < list.Count; ++i)
            {
                if(pRemoveState == list[i])
                {
                    list.RemoveAt(i);
                    StateCache.Instance.AddState(pRemoveState);
                    return true;
                } 
            }
            return false;
        }
    
        void DoUpdate(float delta, List<IState> list)
        {
            IState pState = null;
            for(int i = 0; i < list.Count; ++i)
            {
                pState = list[i];
                if( !pState.Paused )
                {
                    if(pState.Execute(delta))
                    {
                         // Execute 이후에 IsVaild 체크.
                        pState.Exit(false);
                        if(pState.IsVaild())
                        {
                            pState.Paused = true;
                            if(!m_vecRemoveStateList.Contains(pState))
                            {
                                m_vecRemoveStateList.Add(pState);
                            }
                        }
                    }
                }
            }
        }
        public void Update(float delta)
        {
            IState pState = null;
            int n = 0;
            for(n = 0; n < m_addStateList.Count; ++n)
            {
                pState = m_addStateList[n];
                
                if(pState.Priority == 0)
                {
                    m_vecStateList.Add(pState);
                }
                else if(pState.Priority == -1)
                {
                    m_vecFrontStateList.Add(pState);
                }
                else if(pState.Priority == 1)
                {
                    m_vecBackStateList.Add(pState);
                }
            }
            m_addStateList.Clear();
                        
            if(!m_bPaused)
            {
                DoUpdate(delta,m_vecFrontStateList);
                DoUpdate(delta,m_vecStateList);
                DoUpdate(delta,m_vecBackStateList);
            }
            
            StateCache pStateCache = StateCache.Instance;
            for(n = 0; n < m_vecRemoveStateList.Count; ++n)
            {
                pState = m_vecRemoveStateList[n];
                RemoveStater(pState);
                if(DoRemoveState(pState,ref m_vecFrontStateList))
                {
                    continue;
                }
                if(DoRemoveState(pState,ref m_vecStateList))
                {
                    continue;
                }
                if(DoRemoveState(pState,ref m_vecBackStateList))
                {
                    continue;
                }
                
                pStateCache.AddState(pState);
            }
            m_vecRemoveStateList.Clear();
        }
    }
}