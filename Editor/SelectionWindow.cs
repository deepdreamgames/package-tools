using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeepDreamGames
{
    // Modal selection window
    public class SelectionWindow : EditorWindow
    {
        static class Styles
        {
            static public GUIStyle line = new GUIStyle("TV Line")
            {
                alignment = TextAnchor.MiddleLeft,
            };
        }

        private class Option : TreeViewItem
        {
            public float height;
            public GUIContent content;
        }
        
        private class MyTreeView : TreeView
        {
            private TreeViewItem root;
            public Action<int> onDoubleClickedItem;

            // Ctor
            public MyTreeView(TreeViewState treeViewState, List<string> options) : base(treeViewState)
            {
                Initialize(options);
                Repaint();
            }
            
            // 
            public void Initialize(List<string> options)
            {
                root = new TreeViewItem(-1, -1);
                
                float position = 0f;
                GUIStyle style = Styles.line;
                for (int i = 0; i < options.Count; i++)
                {
                    GUIContent content = new GUIContent(options[i]);
                    float height = style.CalcHeight(content, 10000f) + 6;
                    Option item = new Option()
                    {
                        id = i,
                        displayName = options[i],
                        height = height,
                        content = content,
                    };
                    position += height;

                    root.AddChild(item);
                }

                SetupDepthsFromParentsAndChildren(root);

                Reload();
            }

            // 
            protected override void RowGUI(RowGUIArgs args)
            {
                if (Event.current.rawType == EventType.Repaint)
                {
                    Option option = args.item as Option;
                    Rect rect = args.rowRect;
                    rect.xMin += GetContentIndent(option) + extraSpaceBeforeIconAndLabel;
                    Styles.line.Draw(rect, option.displayName, false, false, args.selected, args.focused);
                }
            }

            // 
            public void Deinitialize()
            {
                root = null;
            }

            // 
            protected override void DoubleClickedItem(int id)
            {
                if (onDoubleClickedItem != null)
                {
                    onDoubleClickedItem(id);
                }
            }

            // 
            protected override float GetCustomRowHeight(int row, TreeViewItem item)
            {
                return (item as Option).height;
            }

            // 
            protected override TreeViewItem BuildRoot()
            {
                showAlternatingRowBackgrounds = true;
                showBorder = true;
                return root;
            }
        }

        #region Private Fields
        private MyTreeView treeView;
        private TreeViewState treeViewState;
        private List<int> result;                               // Reference to list which will be filled with selection prior to closing the window
        #endregion

        #region Public Methods
        // 
        static public SelectionWindow Open(string title, List<string> options, List<int> result)
        {
            SelectionWindow instance = Get<SelectionWindow>();
            if (title != null)
            {
                instance.titleContent = new GUIContent(title);
            }
            
            if (instance.treeView == null)
            {
                if (instance.treeViewState == null)
                {
                    instance.treeViewState = new TreeViewState();
                }
                instance.treeView = new MyTreeView(instance.treeViewState, options);
                instance.treeView.onDoubleClickedItem = instance.OnDoubleClickedItem;
            }
            else
            {
                instance.treeView.Initialize(options);
            }
            result.Clear();
            instance.result = result;
            instance.position = new Rect(40, 40, 640, 480);
            instance.ShowModal();

            return instance;
        }
        #endregion

        #region EditorWindow
        // 
        void OnGUI()
        {
            // List
            treeView.OnGUI(EditorGUILayout.GetControlRect(false, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));

            // Footer
            Rect footerRect = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
            using (new EditorGUI.DisabledScope(!treeView.HasSelection()))
            {
                if (GUI.Button(new Rect(footerRect.x, footerRect.y, footerRect.width * 0.5f, footerRect.height), "OK"))
                {
                    result.AddRange(treeView.GetSelection());
                    result.Sort();
                    Close();
                }
            }
            if (GUI.Button(new Rect(footerRect.x + footerRect.width * 0.5f, footerRect.y, footerRect.width * 0.5f, footerRect.height), "Cancel"))
            {
                Close();
            }

            Repaint();
        }

        // 
        void OnDisable()
        {
            result = null;
            treeView.Deinitialize();
        }
        #endregion

        #region Private Methods
        // 
        static private T Get<T>() where T : EditorWindow
        {
            System.Type type = typeof(T);
            Object[] array = Resources.FindObjectsOfTypeAll(type);
            EditorWindow editorWindow = array.Length > 0 ? (EditorWindow)array[0] : null;
            if (editorWindow == null)
            {
                editorWindow = CreateInstance(type) as EditorWindow;
            }
            return (T)editorWindow;
        }
        
        // 
        private void OnDoubleClickedItem(int id)
        {
            IList<int> selection = treeView.GetSelection();
            if (selection.Count == 1 && selection[0] == id)
            {
                result.Add(id);
                result.Sort();
                Close();
            }
            // If more than 1 item is selected or selected item differs from the one which was double-clicked - 
            // don't do anything to prevent undesired behavior
        }
        #endregion
    }
}
