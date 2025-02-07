using UnityEngine;
using UnityEditor;
using DATA;
using ListView;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;
using System;
using ALF;

namespace TOOL.LISTVIEW 
{

#if UNITY_EDITOR
    public class ListData : TreeViewItem,IBase
    {
        string uid = string.Empty;
        protected string file = string.Empty;
        Vector3 pos = Vector3.zero;
        Quaternion rot = new Quaternion();
        Vector3 scale = Vector3.one;
        public string ResourceFile { set{file = value;}}
        public Quaternion Rot {get{return rot;} set{rot = value;}}
        public Vector3 Pos {get{return pos;} set{pos = value;}}
        public Vector3 Scale {get{return scale;} set{scale = value;}}        
        
        public string ID {get{return uid;} set{uid = value;}}
        public GameObject GO {get; protected set; }
        // public TileMapData ParentTileMap {get; protected set; }
        protected ListData(){}
        public void Dispose()
        {
            ID = null;
            GO = null;
        }

        virtual public void OnDestroy()
        {
            Dispose();
        }
    }
#endif

    public class ListViewDelegate : IListViewDelegate<ListData> 
    {
        MultiColumnHeader header = null;
        // public MultiColumnHeader Header => new MultiColumnHeader(new MultiColumnHeaderState(new[]{
        //     new MultiColumnHeaderState.Column{headerContent = new GUIContent("ObjectID"), width = 20},
        //     new MultiColumnHeaderState.Column{headerContent = new GUIContent("Size"), width = 20},
        //     // new MultiColumnHeaderState.Column{headerContent = new GUIContent("Last name"), width = 20},
        //     // new MultiColumnHeaderState.Column{headerContent = new GUIContent("Age"), width = 10},
        // }));

        public MultiColumnHeader Header{ get {return header;} }

        private readonly List<ListData> raw = new List<ListData>();
            // Enumerable.Repeat(0,7).Select(PersonGenerator.Generate).ToList();
        private E_DATA_TYPE dataType = E_DATA_TYPE.None;

        private System.Func<ListData,bool> clickCallback = null;
        private ListViewDelegate(){}
        public static ListViewDelegate Create(MultiColumnHeader header,E_DATA_TYPE t)
        {
            ListViewDelegate _delegate = new ListViewDelegate();
            _delegate.header = header;
            _delegate.dataType = t;
            return _delegate;
        }

        public void OnDestroy()
        {
            header = null;
            raw.Clear();
        }
        public void Add(ListData item)
        {
            raw.Add(item);
        }
        
        public void ClearAll()
        {
            raw.Clear();
        }

        public void SetClickCallback(System.Func<ListData,bool> callback)
        {
            clickCallback = callback;
        }

        public IList<T> RemoveList<T>(IList<int> list) where T : ListData
        {
            List<T> values = new List<T>();
            for(int i = 0; i < list.Count; ++i)
            {
                for(int n = raw.Count -1; n >= 0; --n)
                {
                    if( raw[n].id == list[i])
                    {
                        if (raw[n] is T data)
                        {
                            raw.RemoveAt(n);
                            values.Add(data);
                            break;
                        }
                    }
                }
            }
            return values;
        }

        public T RemoveEnd<T>() where T : ListData
        {
            if(raw.Count > 0)
            {
                ListData item = raw[raw.Count-1];
                if (item is T data)
                {
                    raw.RemoveAt(raw.Count-1);
                    return data;
                }
            }
            return null;
        }
        public int GetListCount()
        {
            return raw.Count;
        }

        public IList<T> GetData<T>(IList<int> list) where T : ListData
        {
            IList<T> temp = new List<T>();

            for(int i = 0; i < list.Count; ++i)
            {
                for(int n = 0; n < raw.Count; ++n)
                {
                    if(raw[n].id == list[i])
                    {
                        if(raw[n] is T data)
                        {
                            temp.Add(data);
                        }
                    }
                }
            }

            return temp;
        } 

        public bool GetDataIdByGameObject(GameObject go, ref int outId)
        {
            for(int n = 0; n < raw.Count; ++n)
            {
                if(raw[n].GO == go)
                {
                    outId = raw[n].id;
                    return true;
                }
            }

            return false;
        }
        public List<TreeViewItem> GetData()
        {
            return raw.Cast<TreeViewItem>().ToList();
        }

        public List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending)
            => GetSortedData0(columnIndex, isAscending).Cast<TreeViewItem>().ToList();

        private IEnumerable<ListData> GetSortedData0(int columnIndex, bool isAscending)
        {
            switch (columnIndex)
            {
                case 0:
                    return isAscending
                        ? raw.OrderBy(item => item.ID)
                        : raw.OrderByDescending(item => item.ID);
                case 1:
                    return isAscending
                        ? raw.OrderBy(item => item.ID)
                        : raw.OrderByDescending(item => item.ID);
                // case 2:
                //     return isAscending
                //         ? raw.OrderBy(item => item.Lastname)
                //         : raw.OrderByDescending(item => item.Lastname);
                // case 3:
                //     return isAscending
                //         ? raw.OrderBy(item => item.Age)
                //         : raw.OrderByDescending(item => item.Age);
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
            }
        }

        public void Draw(Rect rect, int columnIndex, ListData data, bool selected)
        {
            var labelStyle = selected ? EditorStyles.whiteLabel : EditorStyles.label;
            labelStyle.alignment = TextAnchor.MiddleLeft;
            string text = "";
            switch (columnIndex)
            {
                case 0 : 
                        text = data.ID;
                    break;
                case 1 : 
                    // if(dataType == E_DATA_TYPE.GameObjectData )
                    // {
                    //     // if (data is GameObjectData item)
                    //     // {
                    //     //     text = item.Type;
                    //     // }
                    //     // else
                    //     // {
                    //     //     text = string.Format("x:{0} y:{1} w:{2} h:{3}",data.Size.X,data.Size.Y,data.Size.W,data.Size.H);
                    //     // }
                    // }
                    
                    break;
                // case 2 : 
                //     EditorGUI.LabelField(rect, data.Lastname, labelStyle);
                //     break;
                // case 3 : 
                //     EditorGUI.LabelField(rect, data.Age.ToString(), labelStyle);
                //     break;
                default: 
                    throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
            }

            EditorGUI.LabelField(rect, text, labelStyle);
        }
        
        public void OnItemClick(int id)
        {
            Debug.Log("Click on "+id);
            
            for(int i = 0; i < raw.Count; ++i)
            {
                if(raw[i].id == id )
                {
                    bool bSkipClickEvent = false;
                    if(clickCallback != null)
                    {
                        bSkipClickEvent = clickCallback(raw[i]);
                    }
                    if(!bSkipClickEvent && raw[i].GO != null)
                    {
                        // Selection.objects = new UnityEngine.Object[] { raw[i].GO };
                        Selection.activeGameObject = raw[i].GO;
                    }
                    
                    return;
                }
            }
        }

        public void OnContextClick()
        {
            Debug.Log("ContextClick");
        }
    }

    public class ExportDataViewItem : TreeViewItem
    {
        string size = string.Empty;
        
        public string Size{get{return size;}}
        
        public ExportDataViewItem(){}
        public ExportDataViewItem(int _id, string _size,int _type,string _name)
        {
            id = _id;
            depth = _type;
            size = _size;
            displayName = _name;
        }
        
    }

    public class ExportListViewDelegate : IListViewDelegate<ExportDataViewItem> 
    {

        // List<TreeViewItem> GetData();
        // List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending);
        // void Draw(Rect rect, int columnIndex, T data, bool selected);
        // void OnItemClick(int id);
        // void OnContextClick();


        public MultiColumnHeader Header => new MultiColumnHeader(new MultiColumnHeaderState(new[]{
            new MultiColumnHeaderState.Column{headerContent = new GUIContent("Path"), width = 100},
            new MultiColumnHeaderState.Column{headerContent = new GUIContent("Size"), width = 40},
            new MultiColumnHeaderState.Column{headerContent = new GUIContent("Type"), width = 20}
        }));

        private readonly List<ExportDataViewItem> raw = new List<ExportDataViewItem>();
            // Enumerable.Repeat(0,7).Select(PersonGenerator.Generate).ToList();
        private System.Action<ExportDataViewItem> clickCallback = null;
        private ExportListViewDelegate(){}
        public static ExportListViewDelegate Create()
        {
            ExportListViewDelegate _delegate = new ExportListViewDelegate();
            return _delegate;
        }

        public void OnDestroy()
        {
            raw.Clear();
        }
        public void Add(ExportDataViewItem item)
        {
            raw.Add(item);
        }
        
        public void ClearAll()
        {
            raw.Clear();
        }

        public void SetClickCallback(System.Action<ExportDataViewItem> callback)
        {
            clickCallback = callback;
        }

        public IList<T> RemoveList<T>(IList<int> list) where T : TreeViewItem
        {
            List<T> values = new List<T>();
            for(int i = 0; i < list.Count; ++i)
            {
                for(int n = raw.Count -1; n >= 0; --n)
                {
                    if( raw[n].id == list[i])
                    {
                        if (raw[n] is T data)
                        {
                            raw.RemoveAt(n);
                            values.Add(data);
                            break;
                        }
                    }
                }
            }
            return values;
        }

        public T RemoveEnd<T>() where T : TreeViewItem
        {
            if(raw.Count > 0)
            {
                TreeViewItem item = raw[raw.Count-1];
                if (item is T data)
                {
                    raw.RemoveAt(raw.Count-1);
                    return data;
                }
            }
            return null;
        }
        public int GetListCount()
        {
            return raw.Count;
        }

        public IList<T> GetData<T>(IList<int> list) where T : TreeViewItem
        {
            IList<T> temp = new List<T>();

            for(int i = 0; i < list.Count; ++i)
            {
                for(int n = 0; n < raw.Count; ++n)
                {
                    if(raw[n].id == list[i])
                    {
                        if(raw[n] is T data)
                        {
                            temp.Add(data);
                        }
                    }
                }
            }

            return temp;
        } 

        public List<TreeViewItem> GetData()
        {
            return raw.Cast<TreeViewItem>().ToList();
        }

        public List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending)
            => GetSortedData0(columnIndex, isAscending).Cast<TreeViewItem>().ToList();

        private IEnumerable<ExportDataViewItem> GetSortedData0(int columnIndex, bool isAscending)
        {
            switch (columnIndex)
            {
                case 0:
                    return isAscending
                        ? raw.OrderBy(item => item.id)
                        : raw.OrderByDescending(item => item.id);
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
            }
        }

        public void Draw(Rect rect, int columnIndex, ExportDataViewItem data, bool selected)
        {
            var labelStyle = selected ? EditorStyles.whiteLabel : EditorStyles.label;
            labelStyle.alignment = TextAnchor.MiddleLeft;
            string text = "";
            switch (columnIndex)
            {
                case 0 : 
                    text = data.displayName;
                    break;
                case  1: 
                    text = data.Size;
                break;
                case 2 : 
                    text = data.depth.ToString();
                    
                    break;
                default: 
                    throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
            }

            EditorGUI.LabelField(rect, text, labelStyle);
        }
        
        public void OnItemClick(int id)
        {
            Debug.Log("Click on "+id);
            
            for(int i = 0; i < raw.Count; ++i)
            {
                if(raw[i].id == id )
                {
                    if(clickCallback != null)
                    {
                        clickCallback(raw[i]);
                    }
                    return;
                }
            }
        }

        public void OnContextClick()
        {
            Debug.Log("ContextClick");
        }
    }
}