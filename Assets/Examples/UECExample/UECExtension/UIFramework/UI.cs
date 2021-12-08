using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UEC.UIFramework
{
    public class UI : View
    {
        private Dictionary<Type, View> _views;

        private List<StyleSheet> _styleSheets;

        public UI()
        {
            _views = new Dictionary<Type, View>();
            _styleSheets = new List<StyleSheet>();
        }

        public void AddStyleSheet(StyleSheet styleSheet)
        {
            _styleSheets.Add(styleSheet);
        }

        public T AddView<T>() where T : View
        {
            var type = typeof(T);
            if (_views.TryGetValue(type, out var v))
            {
                return (T) v;
            }

            var view = System.Activator.CreateInstance<T>();
            AddStyleSheet(view);
            view.Initialize(Self);
            view.SetUI(this);
            _views.Add(type, view);
            return view;
        }

        private void AddStyleSheet(VisualElement element)
        {
            foreach (var styleSheet in _styleSheets)
            {
                element.styleSheets.Add(styleSheet);
            }
        }

        public T GetView<T>() where T : View
        {
            var type = typeof(T);
            if (_views.TryGetValue(type, out var v))
            {
                return (T) v;
            }

            return null;
        }
    }
}