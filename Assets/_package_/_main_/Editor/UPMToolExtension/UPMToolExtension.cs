using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UPMTool;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UPMToolDevelop
{
    /// <summary>
    /// UPM Tool在PackageManager上的拓展
    /// </summary>
    [InitializeOnLoad]
    public class UPMToolExtension : IPackageManagerExtension
    {
        static UPMToolExtension()
        {
            PackageManagerExtensions.RegisterExtension(new UPMToolExtension());
        }

        private PackageInfo selectPackageInfo;

        private string selectVersion;

        private string gitUrl;

        private UPMToolExtensionUI ui;

        public VisualElement CreateExtensionUI()
        {
            if (ui == null)
            {
                ui = UPMToolExtensionUI.CreateUI();
                InitUI();
            }

            return ui;
        }

        private void InitUI()
        {
            if (ui == null)
            {
                return;
            }

            var choices = ui.TagsList;

            ui.GetGitTagsButton.clicked += () =>
            {
                GitUtils.GetTags(gitUrl, tags =>
                {
                    choices.Clear();

                    for (int i = 0; i < tags.Length; i++)
                    {
                        choices.Add(tags[i]);
                    }

                    choices.Sort((a, b) =>
                    {
                        var sa = a.Split('.');
                        var sb = b.Split('.');
                        if (sa.Length != 3 || sb.Length != 3)
                        {
                            return 0;
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            try
                            {
                                var ia = int.Parse(sa[i]);
                                var ib = int.Parse(sb[i]);
                                if (ia == ib)
                                {
                                    continue;
                                }

                                if (ia > ib)
                                {
                                    return -1;
                                }

                                if (ia < ib)
                                {
                                    return 1;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return 0;
                            }
                        }

                        return 0;
                    });

                    if (choices.Count > 0)
                    {
                        ui.versionTagsPopupField.SetEnabled(true);
                        ui.versionTagsPopupField.value = choices[0];
                        ui.ChangeVersionButton.SetEnabled(true);
                    }
                });
            };

            ui.versionTagsPopupField.RegisterValueChangedCallback<string>((evt) => { selectVersion = evt.newValue; });

            ui.ChangeVersionButton.clicked += () => { PackageUtils.AddOrUpdatePackage(GetPackageIdByNewGitUrl()); };
        }

        private string GetPackageIdByNewGitUrl()
        {
            if (selectPackageInfo == null)
            {
                return "";
            }

            return $"{selectPackageInfo.name}@{gitUrl}#{selectVersion}";
        }

        public void OnPackageSelectionChange(PackageInfo packageInfo)
        {
            if (ui == null)
            {
                return;
            }

            selectPackageInfo = packageInfo;

            var packageId = selectPackageInfo.packageId;

            if (packageId.Contains("chino"))
            {
                gitUrl = GetGitUrl(packageId);
                ui.SetUIVisible(true);
            }
            else
            {
                ui.SetUIVisible(false);
            }
        }

        private string GetGitUrl(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                Debug.LogError("packageId is null");
                return "";
            }

            var s = packageId.Split('@');
            if (s.Length < 2)
            {
                Debug.LogError("packageId is bad");
                return "";
            }

            var u = s[1].Split('#');

            if (u.Length < 2)
            {
                Debug.LogError("packageId is bad");
                return "";
            }

            return u[0];
        }

        public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
        {
        }

        public void OnPackageRemoved(PackageInfo packageInfo)
        {
        }
    }
}