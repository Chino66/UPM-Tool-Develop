using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UEC.UIFramework
{
    public class VisualElementCache
    {
        private Dictionary<string, VisualElement> _cache = new Dictionary<string, VisualElement>();

        private VisualElement _root;

        public VisualElementCache(VisualElement root)
        {
            _root = root;
        }

        private T Create<T>(string query) where T : VisualElement
        {
            return _root.Q<T>(query);
        }

        public T Get<T>(string query) where T : VisualElement
        {
            if (!_cache.ContainsKey(query))
            {
                _cache[query] = Create<T>(query);
            }

            return _cache[query] as T;
        }
    }
}