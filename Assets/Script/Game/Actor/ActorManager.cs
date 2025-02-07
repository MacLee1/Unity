using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALF;

public class ActorManager : IBase
{
    static ActorManager instance = null;
    const int BaseActorActiveMaxCount = 100;
    Dictionary<string,List<IBaseActor>> m_mapBaseActorList = new Dictionary<string,List<IBaseActor>>(100);
    
    public static ActorManager Instance
    {
        get { return instance;}
    }

    public static bool InitInstance()
    {
        if (instance == null)
        {
            instance = new ActorManager();
            return true;
        }
        return false;
    }

    public static void AddBaseResource( IBaseActor baseResource)
    {
        if(baseResource != null)
        {
            Instance.AddBaseActor(baseResource);
        }
    }

    public static bool IsBaseResource( string id )
    {
        return Instance.m_mapBaseActorList.ContainsKey(id);
    }

    private ActorManager(){}
    
    public void Dispose()
    {
        RemoveAll();
    }
    public void AddBaseActor(IBaseActor actor)
    {
        ALFUtils.Assert(actor != null, "AddBaseActor : actor is null!!");

        actor.Reset();
        actor.MainUI.SetParent(null);
        actor.MainUI.localPosition = Vector3.zero;
        actor.MainUI.localScale = Vector3.one;
        actor.MainUI.gameObject.SetActive(false);
        
        if(m_mapBaseActorList.ContainsKey(actor.ID) && m_mapBaseActorList[actor.ID] != null)
        {
            m_mapBaseActorList[actor.ID].Add(actor);
        }
        else
        {
            m_mapBaseActorList[actor.ID] = new List<IBaseActor>();
            m_mapBaseActorList[actor.ID].Add(actor);
        }
    }
    public T GetActor<T>(string id) where T : IBaseActor
    {
        if(m_mapBaseActorList.Count < BaseActorActiveMaxCount)
        {
            T temp;

            if(m_mapBaseActorList.ContainsKey(id) && m_mapBaseActorList[id] != null && m_mapBaseActorList[id].Count > 1)
            {
                int count = m_mapBaseActorList[id].Count;
                temp = (T)(m_mapBaseActorList[id][count -1]);
                m_mapBaseActorList[id].RemoveAt(count -1);
            }
            else
            {
                // temp = Object.Instantiate<BaseActor>(m_mapBaseActorList[typeof(T).Name][0]);
                // temp = Object.Instantiate<BaseActor>(m_mapBaseActorList[id][0]);
                temp = (T)(m_mapBaseActorList[id][0].Clone());
            }
            
            temp.MainUI.gameObject.SetActive(true);
            temp.Reset();
            return temp;
        }
        
        return default(T);
    }
    
    public void RemoveAll()
    {
        var itr = m_mapBaseActorList.GetEnumerator();
        while(itr.MoveNext())
        {
            for(int i = 0; i < itr.Current.Value.Count; ++i)
            {
                itr.Current.Value[i].Dispose();
                itr.Current.Value[i] = null;
            }

            itr.Current.Value.Clear();
        }
        m_mapBaseActorList.Clear();
    }
}