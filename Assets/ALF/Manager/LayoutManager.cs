using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ALF.LAYOUT
{

    public class LayoutManager : IBase
    {
        static LayoutManager instance = null;
        RectTransform m_mainUI = null;
        RectTransform m_canvas = null;
        Transform m_curtain = null;

        Dictionary<string,Stack<RectTransform>> m_mapUIList = new Dictionary<string,Stack<RectTransform>>(32);
        
        bool m_bForceLock = false;

        List<RectTransform> m_popupUIList = new List<RectTransform>(10);

        public static LayoutManager Instance
        {
            get {return instance;}
        }

        public static float Width
        {
            get {
                if (instance == null || instance.m_canvas == null)
                {
                    return 0;
                }

                return instance.m_canvas.offsetMax.x - instance.m_canvas.offsetMin.x;
            }
        }
        public static float Height
        {
            get {
                if (instance == null || instance.m_canvas == null)
                {
                    return 0;
                }

                return instance.m_canvas.offsetMax.y - instance.m_canvas.offsetMin.y;
            }
        }
        public static bool InitInstance()
        {
            if(instance == null)
            {
                instance = new LayoutManager();
                GameObject root = GameObject.Find("Canvas");
                ALFUtils.Assert(root != null, "not find Canvas !");
                instance.m_canvas = root.GetComponent<RectTransform>();
                // Canvas canvas = instance.m_canvas.GetComponent<Canvas>();
                // canvas.renderMode = RenderMode.ScreenSpaceCamera;
                // canvas.worldCamera = Camera.main;
                instance.m_curtain = instance.m_canvas.Find("Curtain");
                instance.m_curtain.gameObject.SetActive(false);
                GameObject.DontDestroyOnLoad(root);
                return true;
            }

            return false;
        }

        public static void SetupMainCamera()
        {
            Canvas canvas = instance.m_canvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }

        public void AddCanvas(RectTransform ui)
        {
            ALFUtils.Assert(ui != null , "AddCanvas : ui == null!!");

            ui.SetParent(m_canvas);
            // ui.localScale =  Vector2.one;
            // ui.sizeDelta = new Vector2(0,0);
            ui.anchoredPosition = Vector2.zero;
        }
        
        protected LayoutManager(){}

        public void Dispose()
        {
            
        }

        public void DestroyUI()
        {
            m_popupUIList.Clear();
            if(m_mainUI != null)
            {
                RemoveMainUI();
            }

            var itr = m_mapUIList.GetEnumerator();
            RectTransform item = null;
            while(itr.MoveNext())
            {
                if(itr.Current.Value != null)
                {
                    Stack<RectTransform> pList = itr.Current.Value;
                    while(pList.Count > 0)
                    {
                        item = pList.Pop();
                        GameObject.Destroy(item.gameObject);
                    }
                }
            }
            item = null;
            m_mapUIList.Clear();
        }

        public void ShowPopup(RectTransform popup)
        {
            m_popupUIList.Add(popup);
        }

        public void HidePopup(RectTransform popup)
        {
            for(int i = 0; i < m_popupUIList.Count; ++i)
            {
                if(m_popupUIList[i] == popup)
                {
                    m_popupUIList.RemoveAt(i);
                    return;
                }
            }
        }

        public bool IsShowPopup()
        {
            return m_popupUIList.Count > 0;
        }

        public RectTransform GetLastPopup()
        {
            if(m_popupUIList.Count > 0)
            {
                RectTransform dlg = m_popupUIList[m_popupUIList.Count -1];
                return dlg;
            }

            return null;
        }
        public RectTransform PopPopup()
        {
            if(m_popupUIList.Count > 0)
            {
                RectTransform dlg = m_popupUIList[m_popupUIList.Count -1];
                m_popupUIList.RemoveAt(m_popupUIList.Count -1);
                return dlg;
            }

            return null;
        }

        public void SetMainUI(RectTransform mainUI)
        {
            m_popupUIList.Clear();
            if(m_mainUI != null)
            {
                RemoveMainUI();
            }
            ALFUtils.Assert(mainUI, "mainUI is null!!");
            m_mainUI = mainUI;
            m_mainUI.SetParent(m_canvas);
            m_mainUI.SetAsFirstSibling();
            m_mainUI.localScale =  Vector2.one;
            m_mainUI.sizeDelta = Vector2.zero;
            m_mainUI.anchoredPosition = Vector2.zero;
        }

        public void AddItem( string itemID,RectTransform tm)
        {
            if(tm == null) return;

            LayoutManager.SetReciveUIButtonEvent(tm, null);
            tm.SetParent(null);
            tm.localPosition = Vector3.zero;
            tm.localScale  = Vector3.one;
            tm.localRotation = Quaternion.Euler(0,0,0);
            tm.gameObject.SetActive(false);

            if(!m_mapUIList.Keys.Contains(itemID))
            {
                m_mapUIList[itemID] = new Stack<RectTransform>();
            }

            if(itemID == "PlayerCard")
            {
                Transform pGroup = tm.Find("nation");
                if(pGroup != null)
                {
                    RawImage nation = pGroup.GetComponent<RawImage>();
                    nation.texture = null;
                }

                pGroup = tm.Find("icon");
                if(pGroup != null)
                {
                    RawImage[] icons = pGroup.GetComponentsInChildren<RawImage>(true);
                    for(int i = 0; i < icons.Length; ++i)
                    {
                        icons[i].texture = null;
                    }
                }

                pGroup = tm.Find("roles");
                if(pGroup != null)
                {                
                    for(int n =0; n < pGroup.childCount; ++n)
                    {
                        pGroup.GetChild(n).GetComponent<RawImage>().texture = null;
                    }  
                }   
            }

            m_mapUIList[itemID].Push(tm);
        }

        public T GetItem<T>( string itemID) where T : Component
        {
            Transform tm = null;
            if(m_mapUIList.Keys.Contains(itemID))
            {
                if (m_mapUIList[itemID].Any())
                {
                    tm = m_mapUIList[itemID].Pop();
                }
            }
            
            if(tm == null)
            {
                T temp = AFPool.GetItem<T>("UI",itemID);
                return Object.Instantiate<T>(temp);
            }
            tm.gameObject.SetActive(true);
            return tm.GetComponent<T>();
        }
        public T FindUIFormRoot<T>(string name) where T : Object
        {
            Transform ui = m_canvas.transform.Find(name);
            if(ui != null)
            {
                return ui.GetComponent<T>();
            }

            ui = GetItem<Transform>(name);
            ui.gameObject.name = name;
            ui.SetAsLastSibling();
            ui.localScale =  Vector2.one;
            ui.SetParent(m_canvas.transform,false);
            return ui.GetComponent<T>();
        }

        public T FindUI<T>(string name) where T : Object
        {
            Transform ui = m_mainUI.Find(name);
            if(ui != null)
            {
                return ui.GetComponent<T>();
            }
            ui = GetItem<Transform>(name);
            ui.gameObject.name = name;
            
            ui.SetAsLastSibling();
            ui.localScale =  Vector2.one;
            ui.SetParent(m_mainUI,false);
            return ui.GetComponent<T>();
        }

        public RectTransform GetMainCanvas()
        {
            return m_canvas;
        }

        public RectTransform GetMainUI()
        {
            return m_mainUI;
        }

        public void RemoveMainUI()
        {
            if(m_mainUI != null)
            {
                LayoutManager.Instance.AddItem(m_mainUI.gameObject.name,m_mainUI);
            }
            m_mainUI = null;
        }

        public void InteractableByTarget(bool bEnable,Transform tm = null)
        {
            Selectable[] list = null;
            if(tm == null)
            {
                list = m_canvas.GetComponentsInChildren<Selectable>(true);
            }
            else
            {
                list = tm.GetComponentsInChildren<Selectable>(true);
            }

            for(int i = 0; i < list.Length; ++i)
            {
                if(list[i] != null)
                {
                    list[i].interactable = bEnable;
                }
            }   
        }

        public void InteractableDisableAll(string name = null,bool bFocus = false)
        {
            if(bFocus)
            {
                m_bForceLock = bFocus;
            }

            Transform tm = null;
            
            if(string.IsNullOrEmpty(name))
            {
                tm = m_canvas.transform;
            }
            else
            {
                tm = FindUI<RectTransform>(name);
            }
            InteractableByTarget(false,tm);
        }

        public void InteractableEnabledAll(string name = null,bool bFocus = false)
        {
            if(m_bForceLock && !bFocus)
            {
                return;
            }

            m_bForceLock = false;

            Transform tm = null;
            
            if(string.IsNullOrEmpty(name))
            {
                tm = m_canvas.transform;
            }
            else
            {
                tm = FindUI<RectTransform>(name);
            }
            InteractableByTarget(true,tm);
        }
        public Transform GetCurtain()
        {
            return m_curtain;
        } 

        public Transform CurtainMake()
        {
            m_curtain.gameObject.SetActive(true);
            ALFUtils.FadeObject(m_curtain,-1);
            return m_curtain;
        } 

        static public void SetReciveUIButtonEvent(RectTransform ui, System.Action<RectTransform,GameObject> _event)
        {
            if(ui != null )
            {
                Button[] list = ui.GetComponentsInChildren<Button>(true);
                Toggle[] listToggle = ui.GetComponentsInChildren<Toggle>(true);

                if(_event == null)
                {
                    for(int i = 0; i < list.Length; ++i)
                    {
                        list[i].onClick.RemoveAllListeners();
                    }

                    for(int i = 0; i < listToggle.Length; ++i)
                    {
                        listToggle[i].onValueChanged.RemoveAllListeners();
                    }
                }
                else
                {
                    RectTransform root = ui;
                    for(int i = 0; i < list.Length; ++i)
                    {
                        Button pButton = list[i];
                        pButton.onClick.RemoveAllListeners();
                        pButton.onClick.AddListener( delegate { _event(root,pButton.gameObject); });
                    }                

                    for(int i = 0; i < listToggle.Length; ++i)
                    {
                        Button pButton = list[i];
                        listToggle[i].onValueChanged.RemoveAllListeners();
                        listToggle[i].onValueChanged.AddListener( delegate { _event(root,pButton.gameObject); });
                    }
                }
            }
        }

        static public void SetReciveUIScollViewEvent(ScrollRect ui, System.Action<ScrollRect,Transform,GameObject> _event)
        {
            if(ui != null )
            {
                if(_event == null)
                {
                    Button[] list = ui.GetComponentsInChildren<Button>(true);
                    for(int i = 0; i < list.Length; ++i)
                    {
                        list[i].onClick.RemoveAllListeners();
                    }
                }
                else
                {
                    for(int t = 0; t < ui.content.childCount; ++t )
                    {
                        Transform tm = ui.content.GetChild(t);
                        Button[] list = tm.GetComponentsInChildren<Button>(true);
                        for(int i = 0; i < list.Length; ++i)
                        {
                            int n =i;
                            list[n].onClick.RemoveAllListeners();
                            list[n].onClick.AddListener( delegate {_event(ui,tm,list[n].gameObject); });
                        }
                    }
                }
            }
        }
    }
}
