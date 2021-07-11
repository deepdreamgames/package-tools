using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace DeepDreamGames
{
    // Modal selection window
    public class SelectionWindow : EditorWindow
    {
        static class Styles
        {
            static public GUIStyle line = "TV Line";
        }

        private class Option : TreeViewItem
        {
            public float height;
            public GUIContent content;
        }
        
        private class MyTreeView : TreeView
        {
            private TreeViewItem root;
            
            // Ctor
            public MyTreeView(TreeViewState treeViewState, List<string> options) : base(treeViewState)
            {
                Initialize(options);
                Reload();
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
                    float height = style.CalcHeight(content, 10000f);
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
            public void Deinitialize()
            {
                root = null;
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
                    result = null;
                    treeView.Deinitialize();
                    Close();
                }
            }
            if (GUI.Button(new Rect(footerRect.x + footerRect.width * 0.5f, footerRect.y, footerRect.width * 0.5f, footerRect.height), "Cancel"))
            {
                Close();
            }

            Repaint();
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
        #endregion
    }
}
