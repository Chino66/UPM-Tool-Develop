using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UPMTool
{
    /// <summary>
    /// Git工具类
    /// </summary>
    public static class GitUtils
    {
        /// <summary>
        /// 获取指定git远程仓库的所有tag标签
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        public static void GetTags(string url, Action<string[]> callback)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("url is null");
                return;
            }

            BatUtils.RunBat(url, s =>
            {
                string[] tags = s.Split('\n');

                // 格式:2c0eadd9ba8010d6a8ec2ff4599141aa817c2d03  refs/tags/1.0.0
                // 取最后面的版本号
                // 版本采用semver命名规则
                string pattern = "[0-9]+\\.[0-9]+\\.[0-9]+";
                List<string> rst = new List<string>();
                for (int i = 0; i < tags.Length; i++)
                {
                    string tag = tags[i];
                    if (string.IsNullOrEmpty(tag))
                    {
                        continue;
                    }

                    Match match = Regex.Match(tag, pattern);

                    if (match.Success)
                    {
                        rst.Add(match.Value);
                    }
                }

                callback?.Invoke(rst.ToArray());
            });
        }

        /// <summary>
        /// 将git地址转为unity包地址
        /// 如果是https地址,则直接返回就是正确的格式
        /// 如果是ssh地址,有特殊情况:
        /// 真实地址为:git@<path>:<user_name>/<project_name>.git
        /// 在manifest.json中,会变成ssh://git@<path>/<user_name>/<project_name>.git
        /// 可以看到<path>:<user_name>中间的":"变成了"/"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GitPathConvertUnityPackagePath(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("uri is null");
                return "";
            }

            if (url.StartsWith("https"))
            {
                return url;
            }

            if (url.StartsWith("git@"))
            {
                var index = url.IndexOf(':');
                if (index != -1)
                {
                    url = url.Remove(index, 1).Insert(index, "/");
                    url = "ssh://" + url;
                    return url;
                }
            }
            
            Debug.LogError("url is bad");
            return url;
        }
        
        /// <summary>
        /// 将unity包地址转为git地址
        /// 如果是https地址,则直接返回就是正确的格式
        /// 如果是ssh地址,有特殊情况:
        /// 真实地址为:git@<path>:<user_name>/<project_name>.git
        /// 在manifest.json中,会变成ssh://git@<path>/<user_name>/<project_name>.git
        /// 可以看到<path>:<user_name>中间的":"变成了"/"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string UnityPackagePathConvertGitPath(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("uri is null");
                return "";
            }

            if (url.StartsWith("https"))
            {
                return url;
            }

            if (url.StartsWith("git@"))
            {
                var index = url.IndexOf('/');
                if (index != -1)
                {
                    url = url.Remove(index, 1).Insert(index, ":");
                    return url;
                }
            }
            
            Debug.LogError("url is bad");
            return url;
        }
    }
}