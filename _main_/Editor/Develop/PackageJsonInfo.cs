using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// using LitJson;

namespace UPMToolDevelop
{
    /// <summary>
    /// 插件开发信息
    /// </summary>
    public class PackageJsonInfo
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
        /// 插件包依赖
        /// todo 可编辑ui没有做
        /// JObject是Newtonsoft.Json中的键值对类
        /// </summary>
        // public JsonData dependencies = null;
        public JObject dependencies = null;

        public string ToJson()
        {
            
            // LitJson
            // var sb = new StringBuilder();
            // var jw = new JsonWriter(sb) {PrettyPrint = true};
            // JsonMapper.ToJson(this, jw);
            // // tip 关于Unicode编码在C#中的转义
            // // LitJson的编码方式是Unicode
            // // Regex.Unescape的作用:
            // // 起因:C#会将'\'转成'\\',而Unicode编码的中文形如:"\u8FD9",在C#中会变成"\\u8FD9",原来的'\'将被转义
            // // 所以输出为"\u8FD9",Regex.Unescape的作用是将转变后的'\\'还原回'\',这样系统就能识别正确的中文Unicode编码
            // return Regex.Unescape(sb.ToString());

            // Newtonsoft.Json
            var setting = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, setting);
            return json;
        }

        public string GetAssetsPath() => $@"Assets\UPM\{displayName}\";

        public string GetPackagesPath() => $@"Packages\{name}\{displayName}\";
    }
}