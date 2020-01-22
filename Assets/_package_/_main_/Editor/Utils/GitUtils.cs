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
    }
}