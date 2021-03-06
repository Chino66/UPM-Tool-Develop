using System;
using UnityEngine;

namespace UPMTool
{
    /// <summary>
    /// 插件依赖的插件包信息
    /// </summary>
    [Serializable]
    public class PackageDependency
    {
        /// <summary>
        /// 插件包名:xx.xx.xx
        /// </summary>
        public string packageName;

        /// <summary>
        /// 插件版本号(或者插件引用路径)
        /// </summary>
        public string version;
    }
}
