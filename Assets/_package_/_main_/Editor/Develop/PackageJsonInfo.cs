using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UPMTool;

namespace UPMTool
{
    /// <summary>
    /// 插件开发信息
    /// </summary>
    public class PackageJsonInfo : ScriptableObject
    {
        /// <summary>
        /// 插件包名成:com.xx.xx
        /// 例:com.chino.upmtool
        /// </summary>
        public string name = "";

        /// <summary>
        /// 插件包显示名称:xx
        /// 例:UPM Tool
        /// </summary>
        public string displayName = "";

        /// <summary>
        /// 版本信息
        /// semver命名规则
        /// 例:1.1.1
        /// </summary>
        public string version = "";

        /// <summary>
        /// Unity版本号
        /// 例:2019.3
        /// </summary>
        public string unity = "";

        /// <summary>
        /// 插件简介
        /// </summary>
        public string description = "";

        /// <summary>
        /// 为包管理器提供其他信息的常量
        /// 保留供内部使用
        /// </summary>
        public string type = "";

        /// <summary>
        /// 插件包依赖项数组
        /// 用于Unity插件,PackageDependency.version只能是x.x.x格式
        /// </summary>
        public List<PackageDependency> dependencies;
        
        /// <summary>
        /// 插件包依赖项数组
        /// 用于UPMTool插件,PackageDependency.version可是是x.x.x也可以是git路径
        /// 这个依赖添加到package.json的"dependenciesUt"字段中
        /// </summary>
        public List<PackageDependency> dependenciesUt;
        

        public List<PackageDependency> GetDependenciesByType(string dependType)
        {
            if (dependType.Equals("dependencies"))
            {
                return dependencies;
            }

            if (dependType.Equals("dependenciesUt"))
            {
                return dependenciesUt;
            }

            return null;
        }

        public string GetAssetsPath() => $@"Assets\UPM\{displayName}\";

        public string GetPackagesPath() => $@"Packages\{name}\{displayName}\";
    }
}