using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UPMToolDevelop;

namespace UPMTool
{
    /// <summary>
    /// UPMTool拓展的UI界面
    /// </summary>
    public class UPMToolExtensionUI : VisualElement
    {
        public static UPMToolExtensionUI CreateUI()
        {
            return new UPMToolExtensionUI();
        }

        private readonly VisualElement _root;

        private readonly List<string> _tagsList;

        private readonly Button _getGitTagsButton;
        private readonly PopupField<string> _versionTagsPopupField;
        private readonly Button _changeVersionButton;

        private UPMToolExtensionUI()
        {
            _root = new VisualElement {name = "ui_root"};

            _getGitTagsButton = new Button();
            _getGitTagsButton.name = "get_git_tags";
            _getGitTagsButton.text = "获取版本信息";
            _root.Add(_getGitTagsButton);

            _tagsList = new List<string> {"-select version-"};
            _versionTagsPopupField = new PopupField<string>("Version:", _tagsList, 0) {value = "-select version-"};
            _versionTagsPopupField.SetEnabled(false);
            _root.Add(_versionTagsPopupField);

            _changeVersionButton = new Button {name = "change_version", text = "切换版本"};
            _changeVersionButton.SetEnabled(false);
            _root.Add(_changeVersionButton);

            Add(_root);
        }

        /// <summary>
        /// 如果enable为false,则UI置灰
        /// </summary>
        /// <param name="enable"></param>
        public void SetUIEnable(bool enable)
        {
            _root.SetEnabled(enable);
        }

        /// <summary>
        /// 设置这个UI界面的显隐
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetUIVisible(bool isVisible)
        {
            if (isVisible == false)
            {
                if (_root.parent == this)
                {
                    Remove(_root);
                }
            }
            else
            {
                if (_root.parent != this)
                {
                    Add(_root);
                }
            }
        }

        public void Init(UPMToolExtension upmToolExtension)
        {
            _getGitTagsButton.clicked += () => { upmToolExtension.GetGitTags(_tagsList, ApplyChoices); };

            _versionTagsPopupField.RegisterValueChangedCallback<string>(upmToolExtension.SelectVersion);

            _changeVersionButton.clicked += upmToolExtension.ChangeVersion;
        }

        private void ApplyChoices(List<string> choices)
        {
            if (choices.Count > 0)
            {
                _versionTagsPopupField.SetEnabled(true);
                _versionTagsPopupField.value = choices[0];
                _changeVersionButton.SetEnabled(true);
            }
        }
    }
}