using System.IO;
using UEC.UIFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UPMTool;

namespace UEC
{
    public class OverviewViewCompare : View
    {
        private VisualElementCache _cache;
        
        protected override void OnInitialize(VisualElement parent)
        {
            var uxmlPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/overview_view_uxml.uxml");
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            var ussPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/uss.uss");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            var temp = asset.CloneTree();
            temp.styleSheets.Add(styleSheet);
            
//            var temp = parent.Q("list_view_root");
            Add(temp);
            _cache = new VisualElementCache(temp);
        }

    }
}