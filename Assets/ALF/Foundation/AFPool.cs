//#define LLOG_BMPOOL

using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ALF
{
    public class AFPool : ScriptableObject,IBase 
    {
        const string poolSettingsAssetName = "AFPool";
        const string poolSettingsPath = "Resources";
        const string poolSettingsAssetExtension = ".asset";

        public SerializableDictionary<string, SerializableDictionary<string,UnityEngine.Object>> assets;

        static AFPool instance;
        static bool initialized;

        Dictionary<string, Dictionary<string,UnityEngine.Object>> bundles = new Dictionary<string, Dictionary<string,UnityEngine.Object>>();
#if UNITY_EDITOR
        Dictionary<string, Dictionary<string,UnityEngine.Object>> origin = new Dictionary<string, Dictionary<string,UnityEngine.Object>>();
#endif

        public static void UpdateBundle(AFPool afpool)
        {
            if (instance != null)
            {
                Instance.bundles.Clear();
                var itr = afpool.assets.GetEnumerator();
                Dictionary<string, UnityEngine.Object> data = null;
                while(itr.MoveNext())
                {
                    data = new Dictionary<string, UnityEngine.Object>();
                    var list = itr.Current.Value.Keys;
                    List<string> keyList = new List<string>();
                    if(Instance.assets.ContainsKey(itr.Current.Key))
                    {
                        foreach( string key in list)
                        {
                            if(Instance.assets[itr.Current.Key].ContainsKey(key))
                            {
#if UNITY_EDITOR
                                if(!Instance.origin.ContainsKey(itr.Current.Key))
                                {
                                    Instance.origin.Add(itr.Current.Key,new Dictionary<string, UnityEngine.Object>());
                                }
                                
                                if(!Instance.origin[itr.Current.Key].ContainsKey(key))
                                {
                                    Instance.origin[itr.Current.Key].Add(key,Instance.assets[itr.Current.Key][key]);
                                }
#endif
                                Instance.assets[itr.Current.Key].Remove(key);
                                Instance.assets[itr.Current.Key].Add(key,itr.Current.Value[key]);
                            }
                            else
                            {
                                keyList.Add(key);
                                Instance.assets[itr.Current.Key].Add(key,itr.Current.Value[key]);
                            }
                        }
                    }
                    else
                    {
                        Instance.assets.Add(itr.Current.Key, new SerializableDictionary<string,UnityEngine.Object>());
                        foreach( string key in list)
                        {
                            keyList.Add(key);
                            Instance.assets[itr.Current.Key].Add(key,itr.Current.Value[key]);
                        }
                    }

#if UNITY_EDITOR
                    for(int i =0; i < keyList.Count; ++i)
                    {
                        if(!data.ContainsKey(keyList[i]))
                        {
                            data.Add(keyList[i],itr.Current.Value[keyList[i]]);   
                        }
                    }
#else
                    foreach( string key in list)
                    {
                        if(!data.ContainsKey(key))
                        {
                            data.Add(key,itr.Current.Value[key]);   
                        }
                    }
#endif

                    Instance.bundles.Add(itr.Current.Key, data);
                }
            }
        }

        public static bool Setup(AFPool afpool)
        {
            if (instance != null)
            {
                instance.Reset();
                instance = null;
            }
            instance = afpool;
            instance.Initialize();
            return true;
        }

        public static AFPool Instance
        {
            get { 
                    ALF.ALFUtils.Assert(instance, "AFPool Instance is null!!!!");
                    return instance;
                }
        }

        public static bool IsInitialized()
        {
            return instance != null;
        }

    #if UNITY_EDITOR

        [MenuItem("ALF/Resource Pool Settings")]
        public static void Edit () 
        {

            AFPool _instance = CreateInstance<AFPool>();

            string properPath = Path.Combine(Application.dataPath, poolSettingsPath);

            if (!Directory.Exists(properPath)) {
                AssetDatabase.CreateFolder("Assets", poolSettingsPath);
            }

            string fullPath = Path.Combine(Path.Combine("Assets", poolSettingsPath),
                                        poolSettingsAssetName + poolSettingsAssetExtension
                                        );
            AssetDatabase.CreateAsset(_instance, fullPath);

            Selection.activeObject = _instance;
        }
    #endif

        // void OnEnable () {
        //     DontDestroyOnLoad(this);
        // }

        public static ICollection<string> GetAssetListByType(string itemType)
        {
            if (Instance.assets.ContainsKey(itemType))
            {
                return Instance.assets[itemType].Keys;
            }

            return null;
        }

        public void Dispose()
        {
            if(assets != null)
            {
                var bItr = bundles.GetEnumerator();

                while(bItr.MoveNext())
                {
                    if(assets.ContainsKey(bItr.Current.Key))
                    {
                        var it = bItr.Current.Value.Keys;
                        foreach(string k in it)
                        {
                            if(assets[bItr.Current.Key].ContainsKey(k))
                            {
                                assets[bItr.Current.Key].Remove(k);
                            }
                        }
                    }
                }

#if !UNITY_EDITOR
                var itr = assets.GetEnumerator();
                while(itr.MoveNext())
                {
                
                    if(itr.Current.Value != null)
                    {
                        var it = itr.Current.Value.GetEnumerator();
                        while(it.MoveNext())
                        {
                            if(it.Current.Value != null)
                            {
                                UnityEngine.GameObject.Destroy(it.Current.Value);
                            }
                        }
                        itr.Current.Value.Clear();
                    }
                }

                assets.Clear();
                assets = null;
#else
                var itr = origin.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current.Value != null)
                    {
                        var it = itr.Current.Value.GetEnumerator();
                        while(it.MoveNext())
                        {
                            if(it.Current.Value != null)
                            {
                                assets[itr.Current.Key].Remove(it.Current.Key);
                                assets[itr.Current.Key].Add(it.Current.Key,it.Current.Value);
                            }
                        }
                        itr.Current.Value.Clear();
                    }
                }

                origin.Clear();
#endif
            }
        }

        public void Reset()
        {
    
        }

        public static void SetItem(string itemType, string itemID, UnityEngine.Object obj)
        {
// #if !UNITY_EDITOR
            if (!Instance.assets.ContainsKey(itemType))
            {
                Instance.assets.Add(itemType, new SerializableDictionary<string,UnityEngine.Object>());
            }

            if (Instance.assets[itemType].ContainsKey(itemID))
            {
                UnityEngine.GameObject.Destroy(Instance.assets[itemType][itemID]);
                Instance.assets[itemType].Remove(itemID);
            }

            Instance.assets[itemType].Add(itemID,obj);
// #endif
        }

        public static T GetItem<T>(string itemType, string itemID) where T : UnityEngine.Object
        {
            if (Instance.assets.ContainsKey(itemType) && Instance.assets[itemType].ContainsKey(itemID))
            {
                UnityEngine.Object obj = Instance.assets[itemType][itemID];

                if (obj is UnityEngine.GameObject data)
                {
                    return data.GetComponent<T>();
                }
                else if (obj is T _data)
                {
                    return _data;
                }
            }
            return null;
        }

        public static List<string> GetAllItemName<T>(string itemType)
        {
            List<string> list = new List<string>();

            if (Instance.assets.ContainsKey(itemType))
            {
                var itr = Instance.assets[itemType].GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current.Value != null)
                    {
                        if (itr.Current.Value is UnityEngine.GameObject data)
                        {
                            if(data.GetComponent<T>() != null)
                            {
                                list.Add(itr.Current.Key);
                            }
                        }
                        else if (itr.Current.Value is T _data)
                        {
                            list.Add(itr.Current.Key);
                        }
                    }
                }
            }

            return list;
        }

        void Initialize()
        {
            if (initialized) return;
            initialized = true;

            var itr = assets.Keys;
            foreach(string k in itr)
            {
                if(assets[k] != null && assets[k].Count > 0)
                {
                    var it = assets[k].Keys;
                    foreach(string k1 in it)
                    {
                        if(assets[k][k1] == null)
                        {
                            assets[k].Remove(k1);
                        }
                    }
                }
            }
        }
    }

}