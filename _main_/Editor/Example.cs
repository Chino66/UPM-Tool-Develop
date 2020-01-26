using UPMTool;

namespace UnityEditor
{
    public class Example
    {
        [MenuItem("Tool/do something")]
        static void Dosomething()
        {
            PackagePathGenerator.Generate("UMPTool", "Assets/temp.cs");
            AssetDatabase.Refresh();
        }
        
    }
}