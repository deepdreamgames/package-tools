using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DeepDreamGames
{
    // Modal selection window
    public class SelectionWindow : EditorWindow
    {
        static class Styles
        {
            // Using TreeView styles https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUI/TreeView/TreeViewGUI.cs
            static public GUIStyle line = "TV Line";
            static public GUIStyle selection = "TV Selection";
            static public GUIStyle backgroundEven = "OL EntryBackEven";
            static public GUIStyle backgroundOdd = "OL EntryBackOdd";
        }

        private struct Option
        {
            public float height;
            public GUIContent content;
        }

        #region Private Fields
        private List<Option> options = new List<Option>();      // 
        private float contentHeight;                            // Sum of all options heights
        private Vector2 scrollPosition;                         // 
        private HashSet<int> selection = new HashSet<int>();    // 
        private int fromIndex = -1;                             // Shift selection index
        private List<int> result;                               // Reference to list which will be filled with selection prior to closing the window
        #endregion

        #region EditorWindow
        // 
        void OnGUI()
        {
            bool focused = EditorWindow.focusedWindow == this;    // Always focused since the window is modal

            // Get indented rect
            Rect totalRect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            totalRect = IndentRect(totalRect, 6f, 6f, 6f, 6f);

            // Draw border
            float size = 1f;
            DrawBorder(totalRect, size, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f, 1.333f) : new Color(0.6f, 0.6f, 0.6f, 1.333f));
            totalRect = IndentRect(totalRect, size, size, size, size);

            // Scrollbar
            bool scrollbar = contentHeight > totalRect.height;
            Rect viewRect = totalRect;
            if (scrollbar)
            {
                GUIStyle styleScrollbar = GUI.skin.verticalScrollbar;
                float scrollbarWidth = styleScrollbar.fixedWidth;
                Rect scrollRect = new Rect(totalRect.x + totalRect.width - scrollbarWidth, totalRect.y, scrollbarWidth, totalRect.height);
                viewRect.width -= scrollRect.width;
                scrollPosition.y = GUI.VerticalScrollbar(scrollRect, scrollPosition.y, scrollRect.height, 0f, contentHeight, styleScrollbar);
            }

            // Content background
            DrawRect(viewRect, new Color(0.22f, 0.22f, 0.22f, 1f));

            // Find visible items range
            int start = -1, end = -1;
            float y = 0f, offset = 0f;
            bool flag = false;
            float bottom = scrollPosition.y + totalRect.height;
            for (int i = 0; i < options.Count; i++)
            {
                Option option = options[i];
                float yMax = y + option.height;
                bool visible = scrollPosition.y <= yMax && bottom >= y; // [scrollPosition.y, bottom] intersects [y, yMax] range check
                if (flag != visible)
                {
                    flag = visible;
                    if (visible)
                    {
                        offset = y - scrollPosition.y;
                        start = i;
                    }
                    else
                    {
                        end = i;
                        break;
                    }
                }
                y = yMax;
            }
            if (start >= 0 && end < 0) { end = options.Count; }

            // Clipping area
            GUI.BeginClip(viewRect, Vector2.zero, Vector2.zero, false);

            // Draw visible items
            int hoverIndex = -1;
            Event current = Event.current;
            EventType type = current.type;

            int hash = GetHashCode();
            y = offset;
            for (int i = start; i < end; i++)
            {
                Option option = options[i];
                Rect rect = new Rect(0f, y, viewRect.width, option.height);
                y += rect.height;

                int controlId = GUIUtility.GetControlID(hash, FocusType.Passive, rect);
                bool mouseOver = rect.Contains(current.mousePosition);
                if (mouseOver)
                {
                    hoverIndex = i;
                }
                switch (type)
                {
                    case EventType.MouseDown:
                        if (mouseOver && current.button == 0)
                        {
                            GUIUtility.hotControl = controlId;
                            current.Use();
                        }
                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlId && mouseOver)
                        {
                            GUIUtility.hotControl = 0;
                            current.Use();

                            if (current.shift)
                            {
                                if (fromIndex >= 0)
                                {
                                    if (!current.control)
                                    {
                                        selection.Clear();
                                    }
                                    if (fromIndex < i)
                                    {
                                        for (int j = fromIndex; j <= i; j++)
                                        {
                                            selection.Add(j);
                                        }
                                    }
                                    else
                                    {
                                        for (int j = i; j <= fromIndex; j++)
                                        {
                                            selection.Add(j);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!current.control)
                                {
                                    selection.Clear();
                                }

                                if (selection.Contains(i))
                                {
                                    if (fromIndex == i) { fromIndex = -1; }
                                    selection.Remove(i);
                                }
                                else
                                {
                                    fromIndex = i;
                                    selection.Add(i);
                                }
                            }
                        }
                        break;
                    case EventType.Repaint:

                        GUIStyle bgStyle = i % 2 == 0 ? Styles.backgroundEven : Styles.backgroundOdd;
                        bgStyle.Draw(rect, false, false, false, false);

                        if (selection.Contains(i))
                        {
                            Styles.selection.Draw(rect, false, false, true, focused);
                        }

                        Styles.line.Draw(IndentRect(rect, 8f, 0f, 0f, 0f), option.content, false, false, false, false);
                        break;
                }
            }

            // Deselect when clicking on an empty space within clipping area
            if (Event.current.type == EventType.MouseUp && !current.control && hoverIndex < 0)
            {
                fromIndex = -1;
                selection.Clear();
            }

            // End clipping area
            GUI.EndClip();

            // Handle scroll
            if (Event.current.type == EventType.ScrollWheel && totalRect.Contains(Event.current.mousePosition))
            {
                scrollPosition.y = Mathf.Clamp(scrollPosition.y + Event.current.delta.y * 20f, 0f, contentHeight - totalRect.height);
                if (scrollPosition.y < 0f) { scrollPosition.y = 0f; }
                Event.current.Use();
            }

            // Footer
            Rect footerRect = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
            using (new EditorGUI.DisabledScope(selection.Count == 0))
            {
                if (GUI.Button(new Rect(footerRect.x, footerRect.y, footerRect.width * 0.5f, footerRect.height), "OK"))
                {
                    result.AddRange(selection);
                    result.Sort();
                    result = null;
                    options.Clear();
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

        #region Public Methods
        // 
        static public SelectionWindow Open(string title, List<string> options, List<int> result)
        {
            SelectionWindow instance = Get<SelectionWindow>();
            if (title != null) 
            {
                instance.titleContent = new GUIContent(title);
            }
            instance.selection.Clear();
            instance.scrollPosition = Vector2.zero;
            instance.options.Clear();
            float position = 0f;
            GUIStyle style = Styles.line;
            for (int i = 0; i < options.Count; i++)
            {
                GUIContent content = new GUIContent(options[i]);
                float height = style.CalcHeight(content, 10000f);
                Option option = new Option()
                {
                    height = height,
                    content = content,
                };
                position += height;
                instance.options.Add(option);
            }
            instance.contentHeight = position;
            result.Clear();
            instance.result = result;
            instance.position = new Rect(40, 40, 640, 480);
            instance.ShowModal();

            return instance;
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
        static private void DrawBorder(Rect rect, float size, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Color color2 = GUI.color;
                GUI.color *= color;
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, size), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(rect.x, rect.yMax - size, rect.width, size), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(rect.x, rect.y + 1f, size, rect.height - 2f * size), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(rect.xMax - size, rect.y + 1f, size, rect.height - 2f * size), EditorGUIUtility.whiteTexture);
                GUI.color = color2;
            }
        }

        // 
        static private void DrawRect(Rect rect, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Color color2 = GUI.color;
                GUI.color *= color;
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
                GUI.color = color2;
            }
        }

        // 
        static private Rect IndentRect(Rect rect, float left, float right, float top, float bottom)
        {
            return new Rect(rect.x + left, rect.y + top, Mathf.Max(rect.width - left - right, 0f), Mathf.Max(rect.height - top - bottom, 0f));
        }
        #endregion
    }
}
