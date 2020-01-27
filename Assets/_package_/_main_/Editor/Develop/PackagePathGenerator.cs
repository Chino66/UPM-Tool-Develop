using System.Reflection;
using System.CodeDom.Compiler;
using System.CodeDom;

namespace UPMTool
{
    public class PackagePathGenerator
    {
        public static void Generate(string nameSpace, string path, string className = "PackagePath")
        {
            CodeCompileUnit unit = new CodeCompileUnit();
            CodeNamespace theNamespace = new CodeNamespace(nameSpace);
            unit.Namespaces.Add(theNamespace);

            theNamespace.Imports.Add(new CodeNamespaceImport("System.Reflection"));

            CodeTypeDeclaration theClass = new CodeTypeDeclaration(className);
            theClass.TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;
            theNamespace.Types.Add(theClass);

            CodeMemberField LocalPath = new CodeMemberField(typeof(string), "LocalPath");
            LocalPath.Attributes = MemberAttributes.Private | MemberAttributes.Const;
            LocalPath.InitExpression = new CodePrimitiveExpression(@"Assets/_package_/_main_/");
            theClass.Members.Add(LocalPath);

            CodeMemberField _mainPath = new CodeMemberField(typeof(string), "_mainPath");
            _mainPath.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            theClass.Members.Add(_mainPath);

            // MainPath属性
            CodeMemberProperty MainPath = new CodeMemberProperty();
            MainPath.Type = new CodeTypeReference(typeof(string));
            MainPath.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            MainPath.Name = "MainPath";
            MainPath.HasGet = true;
            MainPath.HasSet = false;
            theClass.Members.Add(MainPath);

            // if语句
            CodeConditionStatement ifCondition = new CodeConditionStatement();
            ifCondition.Condition = new CodeVariableReferenceExpression("string.IsNullOrEmpty(_mainPath)");

            CodeExpressionStatement s = new CodeExpressionStatement();
            s.Expression = new CodeVariableReferenceExpression(
                $"var p = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetAssembly(typeof({className})));");
            ifCondition.TrueStatements.Add(s);

            CodeConditionStatement ifCondition2 = new CodeConditionStatement();
            ifCondition2.Condition = new CodeVariableReferenceExpression("p == null");
            ifCondition2.TrueStatements.Add(new CodeVariableReferenceExpression("_mainPath = LocalPath"));
            ifCondition2.FalseStatements.Add(
                new CodeVariableReferenceExpression("_mainPath = p.assetPath + \"/_main_/\""));
            ifCondition.TrueStatements.Add(ifCondition2);

            MainPath.GetStatements.Add(ifCondition);

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement();
            returnStatement.Expression = new CodeVariableReferenceExpression("_mainPath");

            MainPath.GetStatements.Add(returnStatement);

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