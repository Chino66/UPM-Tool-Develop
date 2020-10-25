using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UPMTool;
using PackageInfo = UnityEditor.PackageInfo;

public class JsonEditorExample : Editor
{
    [MenuItem("Test/JsonEditorExample")]
    static void Excute()
    {
//        string str = typeof(RunExample).Assembly.Location;
        string str = "Assets/_package_/aa/bb/cc/package_example.json";

        FileInfo f = new FileInfo(str);
        str = f.Directory.Parent.Parent.FullName;
        Debug.Log($"1. {str}");
//        str = Path.GetDirectoryName(str).PadLeft(5);
//        Debug.Log($"2. {str}");
//        str = Path.GetDirectoryName(str);
//        Debug.Log($"3. {str}");
//        str = Path.GetDirectoryName(str);
        return;
        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_package_/package_example.json");
        var json = textAsset.text;

        // 不使用json解析
        // 使用正则表达式
        // 取出:"dependenciesUt": {"com.chino.upmtool": "ssh://git@github.com/Chino66/UPM-Tool-Develop.git#upm"}
        // 这样一段文本,然后取出里面对应的插件依赖信息
        // 正则表达式:"dependenciesUt":([^}])*}
        // 意思是以"dependenciesUt":开头匹配所有不是"}"的字符,并以"}"结尾

        var regex = new Regex("\"dependenciesUt\":([^}])*}");

        // 如果没有匹配,则返回""
        var value = regex.Match(json).Groups[0].Value;

        Debug.Log(value);

        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        // 得到的匹配内容,提取所有双引号中的内容:"dependenciesUt","com.chino.upmtool","ssh://git@github.com/Chino66/UPM-Tool-Develop.git#upm"
        // 正则表达式:"([^\"])*",所有双引号的内容
        // 可知[0]是"dependenciesUt",[1]和[2]分别是依赖包的名称和路径

        // 使用@后""(两个双引号)可以表示"(正则表达式中的双号)
//        regex = new Regex(@"""([^\""])*""");
        // 使用这个表达式可以直接获取双引号中的文本而不含双引号
        regex = new Regex("(?<=\")[^:].*?(?=\")");
        // 使用Matches获取所有匹配内容,Match只匹配第一个匹配项
        var groups = regex.Matches(value);

        // 只有一项匹配,说明只有"dependenciesUt"而没有具体依赖包,不需要处理
        if (groups.Count <= 1)
        {
            return;
        }

        for (int i = 0; i < groups.Count; i++)
        {
            Debug.Log(groups[i].Value);
        }

        List(null);
    }

    private static ListRequest ListRequest;

    public static void List(Action<Dictionary<string, PackageInfo>> completed)
    {
        ListRequest = Client.List();
        EditorApplication.update += ListProgress;
    }

    private static void ListProgress()
    {
        if (ListRequest.IsCompleted)
        {
            Debug.Log("list completed");
            if (ListRequest.Status == StatusCode.Success)
            {
                var list = new Dictionary<string, PackageInfo>();
                foreach (var package in ListRequest.Result)
                {
                }
            }
            else if (ListRequest.Status >= StatusCode.Failure)
            {
                Debug.Log(ListRequest.Error.message);
            }

            EditorApplication.update -= ListProgress;
        }
    }
}