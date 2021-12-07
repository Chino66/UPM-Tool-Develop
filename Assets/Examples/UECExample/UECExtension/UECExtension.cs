using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UEC
{
    [InitializeOnLoad]
    public class UECExtension : IPackageManagerExtension
    {
        static UECExtension()
        {
            PackageManagerExtensions.RegisterExtension(new UECExtension());
        }
        
        #region IPackageManagerExtension实现

        private UECUI _ui;

        public VisualElement CreateExtensionUI()
        {
            if (_ui == null)
            {
                _ui = UECUI.CreateUI();
                _ui.Show();
            }

            return _ui.Self;
        }
        
        public void OnPackageSelectionChange(PackageInfo packageInfo)
        {
            if (_ui == null)
            {
                return;
            }

            if (packageInfo == null)
            {
                return;
            }
            
            _ui.SetDisplay(true);
            
            return;

            // todo 开发时界面挂在UPM Tool上
            if (packageInfo.displayName.Equals("UPM Tool"))
            {
                _ui.SetDisplay(true);
            }
            else
            {
                _ui.SetDisplay(false);
            }
        }

        public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
        {
        }

        public void OnPackageRemoved(PackageInfo packageInfo)
        {
        }

        #endregion
    }
}