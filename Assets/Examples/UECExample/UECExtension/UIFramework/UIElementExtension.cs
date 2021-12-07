using UnityEngine.UIElements;

namespace UEC.UIFramework
{
    public static class UIElementExtension
    {
        public static void SetDisplay(this VisualElement ve, bool display)
        {
            ve.style.display = display
                ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }
    }
}