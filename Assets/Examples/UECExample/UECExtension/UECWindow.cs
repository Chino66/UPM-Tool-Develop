using UnityEditor;
using UnityEngine;

namespace UEC
{
    public class UECWindow : EditorWindow
    {
        [MenuItem("Tools/UECWindow")]
        static void ShowWindow()
        {
            var window = EditorWindow.CreateWindow<UECWindow>();
            window.titleContent = new GUIContent("UEC");
            window.Show();
        }

        private void OnEnable()
        {
            var root = UECUI.CreateUI();
            rootVisualElement.Add(root);
        }
    }
}