using System.Reflection;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.ComponentModel;

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

            // DisplayName
            var DisplayName = new CodeMemberField(typeof(string), "DisplayName");
            DisplayName.Attributes = MemberAttributes.Public | MemberAttributes.Const;
            DisplayName.InitExpression = new CodePrimitiveExpression(displayName);
            @class.Members.Add(DisplayName);

            // 静态构造方法
            var sc = new CodeTypeConstructor
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static
            };
            var Type = new CodeTypeReference(className);
            // var importer = new CodeVariableDeclarationStatement(Type, $"importer = new {className}()");
            var s = new CodeExpressionStatement();
            s.Expression =
                new CodeVariableReferenceExpression($"PackageManagerExtensions.RegisterExtension(new {className}())");
            sc.Statements.Add(s);
            @class.Members.Add(sc);

            // 类实现IPackageManagerExtension接口
            var ipme = new CodeTypeReference("IPackageManagerExtension");
            @class.BaseTypes.Add(ipme);


            // 实现public VisualElement CreateExtensionUI();方法
            var method = new CodeMemberMethod();
            method.Name = "CreateExtensionUI";
            method.Attributes = MemberAttributes.Public | MemberAttributes.FamilyAndAssembly;
            // 返回类型
            var returnType = new CodeTypeReference("VisualElement");
            method.ReturnType = returnType;
            // 定义一个变量的语句
            var v = new CodeVariableDeclarationStatement(returnType, "ve = null");
            method.Statements.Add(v);
            var ret = new CodeMethodReturnStatement {Expression = new CodeVariableReferenceExpression("ve")};
            method.Statements.Add(ret);
            @class.Members.Add(method);

            // 参数PackageInfo packageInfo
            var piType = new CodeTypeReference("PackageInfo");
            var piName = "packageInfo";

            // 实现public void OnPackageSelectionChange(PackageInfo packageInfo);方法
            method = new CodeMemberMethod();
            method.Name = "OnPackageSelectionChange";
            method.Attributes = MemberAttributes.Public;
            method.ReturnType = null;
            method.Parameters.Add(new CodeParameterDeclarationExpression(piType, piName));
            @class.Members.Add(method);

            // 实现public void OnPackageAddedOrUpdated(PackageInfo packageInfo);方法
            method = new CodeMemberMethod();
            method.Name = "OnPackageAddedOrUpdated";
            method.Attributes = MemberAttributes.Public;
            method.ReturnType = null;
            method.Parameters.Add(new CodeParameterDeclarationExpression(piType, piName));
            @class.Members.Add(method);

            // 实现public void OnPackageRemoved(PackageInfo packageInfo);方法
            method = new CodeMemberMethod();
            method.Name = "OnPackageRemoved";
            method.Attributes = MemberAttributes.Public;
            method.ReturnType = null;
            method.Parameters.Add(new CodeParameterDeclarationExpression(piType, piName));
            @class.Members.Add(method);

            // 给类添加[InitializeOnLoad]特性
            var attribute = new CodeAttributeDeclaration(new CodeTypeReference("InitializeOnLoad"));
            @class.CustomAttributes.Add(attribute);

            //生成代码

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            CodeGeneratorOptions options = new CodeGeneratorOptions();

            options.BracingStyle = "C";

            options.BlankLinesBetweenMembers = true;

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path))
            {
                provider.GenerateCodeFromCompileUnit(unit, sw, options);
            }
        }
    }
}