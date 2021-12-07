using System.IO;
using UEC.UIFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UPMTool;

namespace UEC
{
    public class UECUI : UI
    {
        public static UECUI CreateUI(VisualElement parent = null)
        {
            var ui = new UECUI();
            if (parent == null)
            {
                parent = new VisualElement();
            }

            ui.Initialize(parent);
            return ui;
        }

        private UECUI()
        {
            
        }

        protected override void OnInitialize(VisualElement parent)
        {
            var uxmlPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/uec_view_uxml.uxml");
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            var ussPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/uss.uss");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            var temp = asset.CloneTree();
            temp.styleSheets.Add(styleSheet);
            Add(temp);

            AddView<OverviewView>();
            AddView<DetailView>();
        }
    }
}