using UnityEngine;
using UnityEditor;

namespace VRCWahooTools
{
    public class MeshBoneViewer : EditorWindow, IHasCustomMenu
    {
        [SerializeField]
        private SkinnedMeshRenderer selected = null;

        [System.NonSerialized]
        private Vector2 scrollPosition = new Vector2(0, 0);

        [System.NonSerialized]
        private GUIStyle lockButtonStyle;

        [System.NonSerialized]
        private bool isLocked = false;

        internal void ShowButton(Rect position)
        {
            if (this.lockButtonStyle == null)
            {
                this.lockButtonStyle = "IN LockButton";
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                this.isLocked = GUI.Toggle(position, this.isLocked, GUIContent.none, this.lockButtonStyle);
                if (check.changed)
                {
                    OnSelectionChange();
                }
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Lock"), this.isLocked, () =>
            {
                this.isLocked = !this.isLocked;
                OnSelectionChange();
            });
        }


        [MenuItem("Tools/WahuTool/MeshBone Viewer")]
        [MenuItem("CONTEXT/SkinnedMeshRenderer/Show Internal Bones")]
        public static void OpenWindow()
        {
            var window = GetWindow<MeshBoneViewer>("MeshBone Viewer");
            window.minSize = new Vector2(250, 250);
            window.Show();
        }

        void OnEnable()
        {
            OnSelectionChange();
            Undo.undoRedoPerformed += OnHierarchyChange;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnHierarchyChange;
        }

        void OnSelectionChange()
        {
            if (isLocked)
            {
                return;
            }

            Object activeObject = Selection.activeObject;
            if (activeObject != null)
            {
                switch (activeObject)
                {
                    case GameObject x:
                        selected = x.GetComponent<SkinnedMeshRenderer>();
                        break;
                    case SkinnedMeshRenderer x:
                        selected = x;
                        break;
                }
            }

            Repaint();
        }

        void OnHierarchyChange()
        {
            Repaint();
        }


        void OnGUI()
        {
            selected = EditorGUILayout.ObjectField(selected, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
            if (selected != null)
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
                {
                    scrollPosition = scrollView.scrollPosition;

                    var obj = new SerializedObject(selected);
                    var listProperty = obj.FindProperty("m_Bones.Array");
                    if (listProperty != null && listProperty.isArray)
                    {
                        var w = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = w / 2;

                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            var targetDepth = listProperty.depth + 1;
                            while (listProperty.Next(true) && listProperty.depth >= targetDepth)
                            {
                                if (listProperty.depth != targetDepth)
                                    continue;

                                using (new EditorGUI.DisabledGroupScope(listProperty.propertyType == SerializedPropertyType.ArraySize))
                                {
                                    EditorGUILayout.PropertyField(listProperty);
                                }
                            }

                            if (check.changed)
                            {
                                //Debug.Log("Change saved.");
                                obj.ApplyModifiedProperties();
                            }
                        }

                        EditorGUIUtility.labelWidth = w;
                    }

                    EditorGUILayout.Space();
                }

            }

        }
    }
}