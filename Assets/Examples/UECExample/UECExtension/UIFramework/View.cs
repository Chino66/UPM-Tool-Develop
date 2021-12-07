using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace UEC.UIFramework
{
    public abstract class View : TemplateContainer
    {
        public UI UI;

        public VisualElement Parent => this.Parent;
        public VisualElement Self => this;

        public View()
        {
        }

        public virtual void Initialize(VisualElement parent)
        {
            Self.name = $"{GetType().Name}";
            parent.Add(Self);
            OnInitialize(parent);
        }

        protected virtual void OnInitialize(VisualElement parent)
        {
        }

        public virtual void SetDisplay(bool value)
        {
            Self?.SetDisplay(value);
        }

        public virtual void Show()
        {
            Self?.SetDisplay(true);
        }

        public virtual void Hide()
        {
            Self?.SetDisplay(false);
        }

        public virtual void SetUI(UI ui)
        {
            this.UI = ui;
        }
    }

    public class View<T> : View where T : UI
    {
        public T UI;

        public override void SetUI(UI ui)
        {
            this.UI = (T) ui;
        }
    }
}