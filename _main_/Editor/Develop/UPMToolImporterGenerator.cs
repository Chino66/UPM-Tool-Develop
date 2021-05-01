using System;
using System.Reflection;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.ComponentModel;
using UnityEditor.PackageManager.Requests;
using UnityEngine.UIElements;

namespace UPMTool
{
    public static class UPMToolImporterGenerator
    {
        public static void Generate(string nameSpace, string path, string displayName,
            string className = "UPMToolImporter")
        {
            // 命名空间
            CodeCompileUnit unit = new CodeCompileUnit();
            CodeNamespace theNamespace = new CodeNamespace(nameSpace);
            unit.Namespaces.Add(theNamespace);

            // 引用
            theNamespace.Imports.Add(new CodeNamespaceImport("System"));
            theNamespace.Imports.Add(new CodeNamespaceImport("UnityEditor"));
            theNamespace.Imports.Add(new CodeNamespaceImport("UnityEditor.PackageManager"));
            theNamespace.Imports.Add(new CodeNamespaceImport("UnityEditor.PackageManager.Requests"));
            theNamespace.Imports.Add(new CodeNamespaceImport("UnityEditor.PackageManager.UI"));
            theNamespace.Imports.Add(new CodeNamespaceImport("UnityEngine.UIElements"));
            theNamespace.Imports.Add(new CodeNamespaceImport("PackageInfo = UnityEditor.PackageManager.PackageInfo"));

            // 类名
            var @class = new CodeTypeDeclaration(className)
            {
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Class
            };
            theNamespace.Types.Add(@class);

            // 给类添加[InitializeOnLoad]特性
            var attribute = new CodeAttributeDeclaration(new CodeTypeReference("InitializeOnLoad"));
            @class.CustomAttributes.Add(attribute);

            // 静态构造方法static UPMToolImporter()
            var sc = new CodeTypeConstructor
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static
            };

            // CheckList(exist=>{if(exist == false){PackageManagerExtensions.RegisterExtension(new UPMToolImporter());}});
            var statement = new CodeExpressionStatement
            {
                Expression =
                    new CodeVariableReferenceExpression(
                        "CheckList(exist=>{if(exist == false){PackageManagerExtensions.RegisterExtension(new UPMToolImporter());}});")
            };
            sc.Statements.Add(statement);
            @class.Members.Add(sc);

            // 插件名称public const string DisplayName = "UPM Tool";
            var memberField = new CodeMemberField(typeof(string), "DisplayName")
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Const,
                InitExpression = new CodePrimitiveExpression(displayName)
            };
            @class.Members.Add(memberField);

            // private bool existUPMTool = false;
            memberField = new CodeMemberField(typeof(bool), "existUPMTool")
            {
                Attributes = MemberAttributes.Private,
                InitExpression = new CodePrimitiveExpression(false)
            };
            @class.Members.Add(memberField);

            AddModule(@class);

            CheckListModule(@class);

            // 类继承IPackageManagerExtension接口
            var ipme = new CodeTypeReference("IPackageManagerExtension");
            @class.BaseTypes.Add(ipme);

            // 实现CreateExtensionUI方法
            ImplementingCreateExtensionUI(@class);

            // 参数PackageInfo packageInfo
            var piType = new CodeTypeReference("PackageInfo");
            var piName = "packageInfo";

            // 实现OnPackageSelectionChange方法
            ImplementingOnPackageSelectionChange(@class, piType, piName);

            // 实现OnPackageAddedOrUpdated方法
            ImplementingOnPackageAddedOrUpdated(@class, piType, piName);

            // 实现OnPackageRemoved方法
            ImplementingOnPackageRemoved(@class, piType, piName);

            ShowHideButtonModule(@class);

            //生成代码
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var options = new CodeGeneratorOptions
            {
                BracingStyle = "C",
                BlankLinesBetweenMembers = true
            };
            using (var sw = new System.IO.StreamWriter(path))
            {
                provider.GenerateCodeFromCompileUnit(unit, sw, options);
            }
        }

        /// <summary>
        /// 实现CreateExtensionUI接口
        /// public VisualElement CreateExtensionUI();
        /// </summary>
        private static void ImplementingCreateExtensionUI(CodeTypeDeclaration @class)
        {
            var buttonType = new CodeTypeReference("Button");

            // 成员变量private Button button;
            var memberButton = new CodeMemberField(buttonType, "button");
            memberButton.Attributes = MemberAttributes.Private;
            @class.Members.Add(memberButton);

            // 实现public VisualElement CreateExtensionUI();方法
            var method = new CodeMemberMethod
            {
                Name = "CreateExtensionUI",
                Attributes = MemberAttributes.Public
            };
            @class.Members.Add(method);

            // button实例化
            var statement = new CodeExpressionStatement
            {
                Expression = new CodeVariableReferenceExpression("button = new Button()")
            };
            method.Statements.Add(statement);

            //button.text = "Import UPM Tool";
            statement = new CodeExpressionStatement
            {
                Expression = new CodeVariableReferenceExpression($"button.text = \"Import UPM Tool\"")
            };
            method.Statements.Add(statement);

            // button.clickable.clicked+=()=>{var path="https://gitee.com/Chino66/UPM-Tool-Develop.git#upm";button.SetEnabled(false);Add(path,()=>{});};
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "button.clickable.clicked+=()=>{var path=\"https://gitee.com/Chino66/UPM-Tool-Develop.git#upm\";button.SetEnabled(false);Add(path,()=>{});};")
            };
            method.Statements.Add(statement);

            // HideButton();
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "HideButton();")
            };
            method.Statements.Add(statement);
            
            // CheckList(exist=>{existUPMTool=exist; if (exist){HideButton();}else{ShowButton();}});
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "CheckList(exist=>{existUPMTool=exist; if (exist){HideButton();}else{ShowButton();}});")
            };
            method.Statements.Add(statement);

            // 返回类型
            var returnType = new CodeTypeReference("VisualElement");
            method.ReturnType = returnType;
            var ret = new CodeMethodReturnStatement {Expression = new CodeVariableReferenceExpression("button")};
            method.Statements.Add(ret);
        }

        /// <summary>
        /// 实现OnPackageSelectionChange接口
        /// public void OnPackageSelectionChange(PackageInfo packageInfo);
        /// </summary
        private static void ImplementingOnPackageSelectionChange(CodeTypeDeclaration @class, CodeTypeReference piType,
            string piName)
        {
            var method = new CodeMemberMethod();
            method.Name = "OnPackageSelectionChange";
            method.Attributes = MemberAttributes.Public;
            method.ReturnType = null;
            method.Parameters.Add(new CodeParameterDeclarationExpression(piType, piName));
            @class.Members.Add(method);

            // if(!existUPMTool&&packageInfo.displayName==DisplayName){ShowButton();}else{HideButton();}
            var statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "if(!existUPMTool&&packageInfo.displayName==DisplayName){ShowButton();}else{HideButton();}")
            };
            method.Statements.Add(statement);
        }

        /// <summary>
        /// 实现OnPackageAddedOrUpdated接口
        /// public void OnPackageAddedOrUpdated(PackageInfo packageInfo);
        /// </summary
        private static void ImplementingOnPackageAddedOrUpdated(CodeTypeDeclaration @class, CodeTypeReference piType,
            string piName)
        {
            var method = new CodeMemberMethod();
            method.Name = "OnPackageAddedOrUpdated";
            method.Attributes = MemberAttributes.Public;
            method.ReturnType = null;
            method.Parameters.Add(new CodeParameterDeclarationExpression(piType, piName));
            @class.Members.Add(method);

            // CheckList(exist=>{existUPMTool=exist; if (exist){HideButton();}else{ShowButton();}});
            var statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "CheckList(exist=>{existUPMTool=exist; if (exist){HideButton();}else{ShowButton();}});")
            };
            method.Statements.Add(statement);
        }

        /// <summary>
        /// 实现OnPackageRemoved接口
        /// public void OnPackageRemoved(PackageInfo packageInfo);
        /// </summary
        private static void ImplementingOnPackageRemoved(CodeTypeDeclaration @class, CodeTypeReference piType,
            string piName)
        {
            var method = new CodeMemberMethod();
            method.Name = "OnPackageRemoved";
            method.Attributes = MemberAttributes.Public;
            method.ReturnType = null;
            method.Parameters.Add(new CodeParameterDeclarationExpression(piType, piName));
            @class.Members.Add(method);

            // CheckList(exist=>{existUPMTool=exist; if (exist){HideButton();}else{ShowButton();}});
            var statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "CheckList(exist=>{existUPMTool=exist; if (exist){HideButton();}else{ShowButton();}});")
            };
            method.Statements.Add(statement);
        }

        /// <summary>
        /// CheckList模块
        /// </summary>
        /// <param name="class"></param>
        private static void CheckListModule(CodeTypeDeclaration @class)
        {
            // private static ListRequest _checkListRequest;
            var memberField = new CodeMemberField(typeof(ListRequest), "_checkListRequest")
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static
            };
            @class.Members.Add(memberField);

            // private static Action<bool> _checkListCompleteCallback;
            memberField = new CodeMemberField(typeof(Action<bool>), "_checkListCompleteCallback")
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static
            };
            @class.Members.Add(memberField);

            // private static void CheckList(Action<bool> action)
            var method = new CodeMemberMethod
            {
                Name = "CheckList",
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                ReturnType = null
            };
            method.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Action<bool>)), "action"));
            @class.Members.Add(method);

            // _checkListRequest = Client.List();
            var statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "_checkListRequest = Client.List();")
            };
            method.Statements.Add(statement);

            // _checkListCompleteCallback = action;
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "_checkListCompleteCallback = action;")
            };
            method.Statements.Add(statement);

            // EditorApplication.update += CheckListProgress;
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "EditorApplication.update += CheckListProgress;")
            };
            method.Statements.Add(statement);

            //--
            // private static void CheckListProgress()
            method = new CodeMemberMethod
            {
                Name = "CheckListProgress",
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                ReturnType = null
            };
            @class.Members.Add(method);

            // if (!_checkListRequest.IsCompleted) return;
            var conditionStatement = new CodeConditionStatement();
            conditionStatement.Condition = new CodeVariableReferenceExpression("!_checkListRequest.IsCompleted");
            conditionStatement.TrueStatements.Add(new CodeMethodReturnStatement());
            @method.Statements.Add(conditionStatement);

            // EditorApplication.update -= CheckListProgress;
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression("EditorApplication.update -= CheckListProgress;")
            };
            method.Statements.Add(statement);

            // if (_checkListRequest.Status != StatusCode.Success) return;
            conditionStatement = new CodeConditionStatement();
            conditionStatement.Condition =
                new CodeVariableReferenceExpression("_checkListRequest.Status != StatusCode.Success");
            conditionStatement.TrueStatements.Add(new CodeMethodReturnStatement());
            method.Statements.Add(conditionStatement);

            // var exist = false;
            var variable = new CodeVariableDeclarationStatement(typeof(bool), "exist");
            variable.InitExpression = new CodePrimitiveExpression(false);
            method.Statements.Add(variable);

            // foreach (var package in _checkListRequest.Result){if (package.displayName.Equals("UPM Tool")){exist = true;break;}}
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "foreach (var package in _checkListRequest.Result){if (package.displayName.Equals(\"UPM Tool\")){exist = true;break;}}")
            };
            method.Statements.Add(statement);

            // _checkListCompleteCallback?.Invoke(exist);
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression("_checkListCompleteCallback?.Invoke(exist);")
            };
            method.Statements.Add(statement);

            // _checkListCompleteCallback = null;
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression("_checkListCompleteCallback = null;")
            };
            method.Statements.Add(statement);
        }

        /// <summary>
        /// Add模块
        /// </summary>
        /// <param name="class"></param>
        private static void AddModule(CodeTypeDeclaration @class)
        {
            // private static AddRequest _addRequest;
            var memberField = new CodeMemberField(typeof(AddRequest), "_addRequest")
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static
            };
            @class.Members.Add(memberField);

            // private static Action _addCompleteCallback;
            memberField = new CodeMemberField(typeof(Action), "_addCompleteCallback")
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static
            };
            @class.Members.Add(memberField);

            // private static void Add(string packageId, Action action)
            var method = new CodeMemberMethod
            {
                Name = "Add",
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                ReturnType = null
            };
            method.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "packageId"));
            method.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Action)), "action"));
            @class.Members.Add(method);

            // _addRequest = Client.Add(packageId);
            var statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "_addRequest = Client.Add(packageId);")
            };
            method.Statements.Add(statement);

            // _addCompleteCallback = action;
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "_addCompleteCallback = action;")
            };
            method.Statements.Add(statement);

            // EditorApplication.update += AddProgress;
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "EditorApplication.update += AddProgress;")
            };
            method.Statements.Add(statement);

            // private static void AddProgress()
            method = new CodeMemberMethod
            {
                Name = "AddProgress",
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                ReturnType = null
            };
            @class.Members.Add(method);

            // if (!_addRequest.IsCompleted) return;
            var conditionStatement = new CodeConditionStatement();
            conditionStatement.Condition = new CodeVariableReferenceExpression("!_addRequest.IsCompleted");
            conditionStatement.TrueStatements.Add(new CodeMethodReturnStatement());
            @method.Statements.Add(conditionStatement);

            // EditorApplication.update -= AddProgress;
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression("EditorApplication.update -= AddProgress;")
            };
            method.Statements.Add(statement);

            // _addCompleteCallback?.Invoke();
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression("_addCompleteCallback?.Invoke();")
            };
            method.Statements.Add(statement);

            // _addCompleteCallback = null;
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression("_addCompleteCallback = null;")
            };
            method.Statements.Add(statement);
        }

        private static void ShowHideButtonModule(CodeTypeDeclaration @class)
        {
            // private void ShowButton()
            var method = new CodeMemberMethod();
            method.Name = "ShowButton";
            method.Attributes = MemberAttributes.Private;
            method.ReturnType = null;
            @class.Members.Add(method);

            // button.SetEnabled(true);button.style.height = new StyleLength {value = 20};
            var statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "button.visible=true;button.SetEnabled(true);button.style.height = new StyleLength {value = 20};")
            };
            method.Statements.Add(statement);

            // private void HideButton()
            method = new CodeMemberMethod();
            method.Name = "HideButton";
            method.Attributes = MemberAttributes.Private;
            method.ReturnType = null;
            @class.Members.Add(method);

            // button.SetEnabled(false);button.style.height = new StyleLength {value = 0};
            statement = new CodeExpressionStatement()
            {
                Expression = new CodeVariableReferenceExpression(
                    "button.visible=false;button.SetEnabled(false);button.style.height = new StyleLength {value = 0};")
            };
            method.Statements.Add(statement);
        }
    }
}