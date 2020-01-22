using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UPMTool
{
    /// <summary>
    /// UPM Tool拓展的UI界面
    /// </summary>
    public class UPMToolExtensionUI : VisualElement
    {
        public static UPMToolExtensionUI CreateUI()
        {
            return new UPMToolExtensionUI();
        }

        private VisualElement root;

        public List<string> TagsList;

        public Button GetGitTagsButton;
        public PopupField<string> versionTagsPopupField;
        public Button ChangeVersionButton;

        public UPMToolExtensionUI()
        {
            root = new VisualElement();
            root.name = "ui_root";
            
            GetGitTagsButton = new Button();
            GetGitTagsButton.name = "get_git_tags";
            GetGitTagsButton.text = "获取版本信息";
            root.Add(GetGitTagsButton);

            TagsList = new List<string> {"-select version-"};
            versionTagsPopupField = new PopupField<string>("Version:", TagsList, 0);
            versionTagsPopupField.value = "-select version-";
            versionTagsPopupField.SetEnabled(false);
            root.Add(versionTagsPopupField);

            ChangeVersionButton = new Button();
            ChangeVersionButton.name = "change_version";
            ChangeVersionButton.text = "切换版本";
            ChangeVersionButton.SetEnabled(false);
            root.Add(ChangeVersionButton);

            Add(root);
        }

        public void SetUIVisible(bool isVisible)
        {
            if (isVisible == false)
            {
                if (root.parent == this)
                {
                    Remove(root);
                }
            }
            else
            {
                if (root.parent != this)
                {
                    Add(root);
                }
            }
        }
    }
}