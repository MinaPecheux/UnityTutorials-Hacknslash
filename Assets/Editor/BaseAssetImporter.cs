using System.IO;
using UnityEditor;

namespace Assets
{

    public class BaseAssetImporter : AssetPostprocessor
    {
        protected static readonly string _MATERIALS_FOLDER = "Materials";
        protected static readonly string _TEXTURES_FOLDER = "Textures";
        private static string[] _TEXTURE_TYPES = new string[]
        {
            "__diffuse",
            "__normal",
            "__specular",
        };

        protected virtual string _GetProcessorFolder() => "";

        protected static bool _IsTexture(string assetPath)
        {
            string p = assetPath.ToLower();
            return p.EndsWith(".jpg") || p.EndsWith(".jpeg") || p.EndsWith(".png") || p.EndsWith(".tga");
        }

        protected static bool _ShouldProcessModel(BaseAssetImporter instance, string assetPath)
            => _ShouldProcessModelWithReference(assetPath, instance._GetProcessorFolder());
        protected static bool _ShouldProcessModelWithReference(string assetPath, string reference)
        {
            // only process the files in: "Imports/<_PROCESSOR_FOLDER>"
            if (!assetPath.Contains(Path.Combine("Imports", reference)))
                return false;

            // only process FBX files
            if (!assetPath.EndsWith(".fbx"))
                return false;

            return true;
        }

        protected static bool _ShouldProcessTexture(BaseAssetImporter instance, string assetPath)
            => _ShouldProcessTextureWithReference(assetPath, instance._GetProcessorFolder());
        protected static bool _ShouldProcessTextureWithReference(string assetPath, string reference)
        {
            // only process the files in: "<_TEXTURES_FOLDER>/<_PROCESSOR_FOLDER>"
            if (!assetPath.Contains(
                Path.Combine(_TEXTURES_FOLDER, reference)))
                return false;

            return true;
        }

        protected static string _GetCharacterFolder(string assetPath)
        {
            return Path.GetFileName(Path.GetDirectoryName(assetPath));
        }

        protected static string _GetModelFilePath(string assetPath)
        {
            string[] assetPaths = Directory.GetFiles(Path.GetDirectoryName(assetPath));
            foreach (string p in assetPaths)
            {
                if (Path.GetFileName(p).StartsWith("_"))
                    return p;
            }
            return "";
        }

        protected static (string, string) _ParseTexturePath(string texPath)
        {
            foreach (string type in _TEXTURE_TYPES)
                if (texPath.Contains(type))
                {
                    string materialName =
                        Path.GetFileNameWithoutExtension(texPath.Replace(type, ""));
                    return (materialName, type);
                }

            return ("", "Unknown");
        }
    }

}
