# CHANGELOG

### 0.0.1
1. 基础功能实现

### 0.0.2
1. 添加一些日志,便于调试

### 0.0.3 
1. 不在使用../Develop/Version.cs
    * 本身也没有使用它,而是使用了System.Version
    * 插件导入到其他项目后,可能会报找不到库的错误,故整个代码注释了

### 0.0.4
1. "PackageCreateTool"创建插件包结构后,会立刻生成PackagePath.cs
    原本使用"PackageCreateTool"创建插件包结构后,需要运行一次才能创建PackagePath.cs(因为使用了[InitializeOnLoadMethod]属性去做检查)

### 0.0.5
1. "Package Create Tool"改名为"Package Json Tool",若检查不到package.json,则绘制创建界面,否则绘制package.json信息,且可以编辑
2. PackageJsonEditor逻辑和界面代码重构,使用UIElements的Bind实现UI控件与数据绑定
3. "Package Json Tool"添加了插件依赖的编辑界面,并重新设计了编辑界面
4. 使用UPMTool创建插件模板时,默认对创建的Package.json添加"UPMTool"的插件依赖
    * 创建的插件并不是必须依赖"UPMTool",而是"UPMTool"可以给插件拓展更多功能,如发布,更新等

### 0.0.6-0.0.7
1. 用于UPMTool插件的依赖使用git路径而不是semver,所以添加"dependenciesUt"字段到package.json的中,支持路径的依赖
例:
```
"dependencies": {
"com.chino.upmtool": "1.0.0"
},
"dependenciesUt": {
"com.chino.upmtool": "ssh://git@github.com/Chino66/UPM-Tool-Develop.git#upm"
}
```
2. 安装UPMTool工具后,在PackageManager面板可以看到插件的"dependenciesUt"依赖安装情况,点击"install"安装指定依赖

# 0.0.8
1. 关闭一些输出
