using System.IO;
using UEC.UIFramework;
using UnityEditor;
using UnityEngine.UIElements;
using UPMTool;

namespace UEC
{
    public class DetailView: View
    {
        private VisualElementCache _cache;
        
        protected override void OnInitialize(VisualElement parent)
        {
            TemplateContainer templateContainer = new TemplateContainer();
            
            var temp = parent.Q("detail_view_root");
            templateContainer.Add(temp);
            var ussPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/uss.uss");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            templateContainer.styleSheets.Add(styleSheet);
            
            
            Add(templateContainer);
            _cache = new VisualElementCache(temp);
        }

    }
}