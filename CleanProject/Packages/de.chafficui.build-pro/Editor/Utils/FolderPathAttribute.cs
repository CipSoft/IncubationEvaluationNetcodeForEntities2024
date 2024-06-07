using UnityEditor;
using UnityEngine;

namespace Chafficui.BuildPro.Editor.Utils
{
#if UNITY_EDITOR
    public class FolderPathAttribute : PropertyAttribute
    {
        public string DialogTitle;

        public FolderPathAttribute(string dialogTitle = "Select Folder")
        {
            DialogTitle = dialogTitle;
        }
    }

    [CustomPropertyDrawer(typeof(FolderPathAttribute))]
    public class FolderPathDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FolderPathAttribute folderPathAttribute = (FolderPathAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginProperty(position, label, property);

                Rect textFieldPosition = position;
                textFieldPosition.width -= 70;

                property.stringValue = EditorGUI.TextField(textFieldPosition, label, property.stringValue);

                Rect buttonPosition = position;
                buttonPosition.x += position.width - 70;
                buttonPosition.width = 70;

                if (GUI.Button(buttonPosition, "Browse"))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel(folderPathAttribute.DialogTitle, property.stringValue, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        property.stringValue = selectedPath;
                    }
                }

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
#endif
}