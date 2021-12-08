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
            var temp = parent.Q("detail_view_root");
            Add(temp);
            _cache = new VisualElementCache(temp);
        }

    }
}