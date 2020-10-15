using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace UPMTool
{
    [ExcludeFromPreset]
    [ScriptedImporter(0, "json")]
    public sealed class PackageManifestImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            Debug.Log($"{ctx.assetPath}");
        }
    }
}