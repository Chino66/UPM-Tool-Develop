# UPM Tool

UPM Tool是自定义插件包的开发和管理的工具,主要功能由:
1. 快速创建插件包框架
2. 通过git发布和更新插件版本

## 1 使用工具

### 1.1快速创建插件包框架
1. 新建一个空项目,打开[Window->Package Manager]面板,在右上角(2019.3版本),找到添加插件包的选项,选择[Add package from git URL...],输入"ssh://git@github.com/Chino66/UPM-Tool-Develop.git#upm",导入此工具

2. 菜单栏选择[Tool->UPM Tool->PackageCreateTool],工具将通过视图的方式展示一个package.json,然后填写json文件并应用(apply),工具将创建一个自定义插件框架.
**注意**:如果项目已经存在package.json,则PackageCreateTool会显示当前package.json而不会新建,但是package.json需要在指定位置才会被工具识别

3. 创建后的框架结构:
```
Assets
-|_package_
--|_main_
---|Editor
----|_generate_
-----|PackagePath.cs
--|package.json
--|README.md
```
- package.json就是插件包必要的部分,unity通过它识别插件
- README.md用于对插件的使用进行介绍,就像这个README一样
- PackagePath.cs是工具默认创建的脚本,用于资源路径的识别

> 关于PackagePath.cs

**PackagePath**是个很有用的工具类,它只有一个静态变量**MainPath**
PackagePath.MainPath指向插件包中_main_目录的位置

问:为什么要这个类?

答:试想一下,当这个插件包在开发环境中,你想要使用插件包下的Res目录下的资源,此时资源路径为[Assets/_package_/_main_/Res].

当这个插件已经发布,此时Res目录的路径为[Packages/xx/_main_/Res]

你需要为开发环境和发布环境配置两个路径,还需要根据不同环境去识别路径,这比较麻烦

PackagePath.MainPath就是为了解决这个问题而设计的,PackagePath.MainPath指向插件包中_main_目录的位置

不必关心插件包在什么环境下,PackagePath.MainPath始终可以得到正确的_main_的位置

## 1.2 使用git发布和更新插件

### 1.2.1 使用git发布插件
1. 菜单栏选择[Tool->UPM Tool->PackageReleaseTool]打开插件发布面板
    * 打开面板需要等待一段时间,这是工具在进行git信息检查,主要检查插件的文件信息
    * 打开面板后,面板显示插件所有的版本信息
2. 设置一个新的版本号,点击[设置]按钮确认修改
3. 使用git工具提交新版本内容的修改到远程分支,回到面板,点击[刷新]按钮
4. 当[发布]按钮可点击时,点击发布,即可发布新的插件版本

### 1.2.2 使用git更新插件
1. 打开[Window->Package Manager],选择导入的自定义插件
2. 在右边的可以看到[获取版本信息]按钮,点击按钮,等待git通信,之后会得到插件的所有版本信息
3. 选择版本,点击[切换版本]即可更新插件版本


## 3 补充内容
1. 版本的识别通过git的"tag"标记标识
2. 默认使用"upm"分支作为插件的开发分支
3. 导入私有仓库,推荐使用ssh方式,详见[git路径规则](https://docs.unity3d.com/Manual/upm-git.html)