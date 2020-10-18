using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UPMToolDevelop;

namespace UPMTool
{
    public class PackageJson
    {
        /// <summary>
        /// package.json文本转为PackageJsonInfo对象
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static PackageJsonInfo Parse(string json)
        {
            try
            {
                var jObject = JsonConvert.DeserializeObject<JObject>(json);
                var packageJson = (PackageJsonInfo) PackageJsonInfo.CreateInstance(typeof(PackageJsonInfo));

                packageJson.name = (string) jObject["name"];
                packageJson.displayName = (string) jObject["displayName"];
                packageJson.version = (string) jObject["version"];
                packageJson.unity = (string) jObject["unity"];
                packageJson.description = (string) jObject["description"];
                packageJson.type = (string) jObject["type"];

                // 读取Unity依赖方式
                packageJson.dependencies = new List<PackageDependency>();
                if (jObject.ContainsKey("dependencies"))
                {
                    var ds = (JObject) jObject["dependencies"];
                    foreach (var d in ds)
                    {
                        var pd = new PackageDependency {packageName = (string) d.Key, version = (string) d.Value};
                        packageJson.dependencies.Add(pd);
                    }
                }

                // 读取UPMTool依赖方式
                packageJson.dependenciesUt = new List<PackageDependency>();
                if (jObject.ContainsKey("dependenciesUt"))
                {
                    var ds = (JObject) jObject["dependenciesUt"];
                    foreach (var d in ds)
                    {
                        var pd = new PackageDependency {packageName = (string) d.Key, version = (string) d.Value};
                        packageJson.dependenciesUt.Add(pd);
                    }
                }

                return packageJson;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public static string ToJson(PackageJsonInfo packageJsonInfo)
        {
            var setting = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            var jObject = new JObject
            {
                ["name"] = packageJsonInfo.name,
                ["displayName"] = packageJsonInfo.displayName,
                ["version"] = packageJsonInfo.version,
                ["unity"] = packageJsonInfo.unity,
                ["description"] = packageJsonInfo.description,
                ["type"] = packageJsonInfo.type
            };

            // Unity的依赖方式
            var dependencies = packageJsonInfo.dependencies;
            if (dependencies != null && dependencies.Count > 0)
            {
                var ds = new JObject();
                foreach (var dependency in dependencies)
                {
                    if (string.IsNullOrEmpty(dependency.packageName) || string.IsNullOrEmpty(dependency.version))
                    {
                        continue;
                    }

                    ds[dependency.packageName] = dependency.version;
                }

                if (ds.Count > 0)
                {
                    jObject["dependencies"] = ds;
                }
            }

            // UPMTool的依赖方式
            var dependenciesUt = packageJsonInfo.dependenciesUt;
            if (dependenciesUt != null && dependenciesUt.Count > 0)
            {
                var ds = new JObject();
                foreach (var dependency in dependenciesUt)
                {
                    if (string.IsNullOrEmpty(dependency.packageName) || string.IsNullOrEmpty(dependency.version))
                    {
                        continue;
                    }

                    ds[dependency.packageName] = dependency.version;
                }

                if (ds.Count > 0)
                {
                    jObject["dependenciesUt"] = ds;
                }
            }

            var json = JsonConvert.SerializeObject(jObject, Formatting.Indented, setting);
            return json;
        }
    }
}