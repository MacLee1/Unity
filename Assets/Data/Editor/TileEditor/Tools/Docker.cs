using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class Docker
{
    public enum DockPosition
    {
        Left,
        Top,
        Right,
        Bottom
    }

    /// <summary>
    /// Docks the second window to the first window at the given position
    /// </summary>
    public static void DockTo(this EditorWindow thiz, EditorWindow target, DockPosition position)
    {
        var mousePosition = GetFakeMousePosition(target, position);

        // Translated from Editor/Mono/GUI/DockArea.cs:537
        var assembly = typeof(EditorWindow).Assembly;
        var __ContainerWindow = assembly.GetType("UnityEditor.ContainerWindow");
        var __DockArea = assembly.GetType("UnityEditor.DockArea");
        var __IDropArea = assembly.GetType("UnityEditor.IDropArea");

        object dropInfo = null;
        object targetView = null;

        var windows = __ContainerWindow.GetProperty("windows", BindingFlags.Static | BindingFlags.Public).GetValue(null, null) as object[];

        if (windows != null)
        {
            foreach(var window in windows)
            {
                var rootSplitView = window.GetType().GetProperty("rootSplitView", BindingFlags.Instance | BindingFlags.Public).GetValue(window, null);
                if (rootSplitView != null)
                {
                    var method = rootSplitView.GetType().GetMethod("DragOverRootView", BindingFlags.Instance | BindingFlags.Public);
                    dropInfo = method.Invoke(rootSplitView, new object[] { mousePosition });
                    targetView = rootSplitView;
                }

                if (dropInfo == null)
                {
                    var rootView = window.GetType().GetProperty("rootView", BindingFlags.Instance | BindingFlags.Public).GetValue(window, null);
                    var allChildren = rootView.GetType().GetProperty("allChildren", BindingFlags.Instance | BindingFlags.Public).GetValue(rootView, null) as object[];
                    foreach(var view in allChildren)
                    {
                        if (__IDropArea.IsAssignableFrom(view.GetType()))
                        {
                            var method = view.GetType().GetMethod("DragOver", BindingFlags.Instance | BindingFlags.Public);
                            dropInfo = method.Invoke(view, new object[] { target, mousePosition });
                            if (dropInfo != null)
                            {
                                targetView = view;
                                break;
                            }
                        }
                    }
                }

                if (dropInfo != null)
                {
                    break;
                }
            }
        }
        if (dropInfo != null && targetView != null)
        {
            var otherParent = thiz.GetType().GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(thiz);
            __DockArea.GetField("s_OriginalDragSource", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, otherParent);
            var method = targetView.GetType().GetMethod("PerformDrop", BindingFlags.Instance | BindingFlags.Public);
            method.Invoke(targetView, new object[] { thiz, dropInfo, mousePosition });
        }
    }

    private static Vector2 GetFakeMousePosition(EditorWindow wnd, DockPosition position)
    {
        Vector2 mousePosition = Vector2.zero;

        // The 80 is required to make the docking work.
        // Smaller values might not work when faking the mouse position.
        var padding = 80;
        switch (position)
        {
            case DockPosition.Left:
                mousePosition = new Vector2(padding, wnd.position.size.y / 2);
                break;
            case DockPosition.Top:
                mousePosition = new Vector2(wnd.position.size.x / 2, padding);
                break;
            case DockPosition.Right:
                mousePosition = new Vector2(wnd.position.size.x - padding, wnd.position.size.y / 2);
                break;
            case DockPosition.Bottom:
                mousePosition = new Vector2(wnd.position.size.x / 2, wnd.position.size.y - padding);
                break;
        }

        return new Vector2(wnd.position.x + mousePosition.x, wnd.position.y + mousePosition.y);
    }
}