using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class CharacterAssetImporter : AssetPostprocessor
{
    private static readonly string _PROCESSOR_FOLDER = "Characters";
    private static readonly string _MATERIALS_FOLDER = "Materials";
    private static readonly string _TEXTURES_FOLDER = "Textures";
    private static string[] _TEXTURE_TYPES = new string[]
    {
        "__diffuse",
        "__normal",
        "__specular",
    };

    private static Dictionary<string, Avatar> _avatarsPerModelFile =
        new Dictionary<string, Avatar>();
    private static int _incompleteAssets = 0;

    void OnPreprocessModel()
    {
        if (!_ShouldProcessModel(assetPath)) return;

        ModelImporter modelImporter = assetImporter as ModelImporter;

        // "Model" options - shared between all model files
        modelImporter.bakeAxisConversion = true;

        // "Rig" / "Materials" options - depend on the model/anim files
        // - for the model file
        if (Path.GetFileName(assetPath).StartsWith("_"))
        {
            modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            modelImporter.optimizeGameObjects = true;

            modelImporter.ExtractTextures(Path.Combine(
                "Assets", _TEXTURES_FOLDER,
                _PROCESSOR_FOLDER, _GetCharacterFolder(assetPath)));
        }
        // - for the other files
        else
        {
            modelImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            // find matching "model file" (in same folder) and try to get
            // the associated avatar (using a cache dictionary to avoid
            // reloading the avatar again and again)
            string modelFilePath = _GetModelFilePath(assetPath);
            if (modelFilePath != "")
            {
                Avatar avatar;
                if (!_avatarsPerModelFile.TryGetValue(modelFilePath, out avatar))
                {
                    avatar = (Avatar)AssetDatabase
                        .LoadAllAssetsAtPath(modelFilePath)
                        .First(x => x.GetType() == typeof(Avatar));
                    _avatarsPerModelFile[modelFilePath] = avatar;
                }

                if (avatar != null)
                    modelImporter.sourceAvatar = avatar;
                else
                    _incompleteAssets++;
            }

            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
        }
    }

    void OnPreprocessAnimation()
    {
        if (!_ShouldProcessModel(assetPath)) return;

        string f = Path.GetFileNameWithoutExtension(assetPath);
        ModelImporter modelImporter = assetImporter as ModelImporter;
        ModelImporterClipAnimation[] animations =
            modelImporter.defaultClipAnimations;

		if (animations != null && animations.Length > 0)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                animations[i].name = f.EndsWith("@")
                    ? f.Substring(0, f.Length - 1) : f;
                if (f.EndsWith("@"))
                    animations[i].loopTime = true;
            }

            modelImporter.clipAnimations = animations;
        }
    }

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        string materialsRootPath = Path.Combine(
            "Assets", _MATERIALS_FOLDER, _PROCESSOR_FOLDER);
        string materialRefFolder, materialAssetDir;
        foreach (string path in importedAssets)
        {
            materialRefFolder = _GetCharacterFolder(path);
            materialAssetDir = Path.Combine(materialsRootPath, materialRefFolder);

            if (_ShouldProcessModel(path))
            {
                // create associated material folder if need be
                if (!Directory.Exists(materialAssetDir))
                    AssetDatabase.CreateFolder(materialsRootPath, materialRefFolder);

                // extract materials
                IEnumerable<Object> materials = AssetDatabase
                    .LoadAllAssetsAtPath(path)
                    .Where(x => x.GetType() == typeof(Material));
                string materialAssetPath, error;
                foreach (Object material in materials)
                {
                    materialAssetPath = Path.Combine(
                        materialAssetDir, $"{material.name}.mat");
                    error = AssetDatabase.ExtractAsset(material, materialAssetPath);
                    if (error != "")
                        Debug.LogWarning(
                            $"Could not extract material '{material.name}': {error}",
                            material);
                    else
                    {
                        AssetDatabase.WriteImportSettingsIfDirty(path);
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    }
                }
            }

            else if (_IsTexture(path) && _ShouldProcessTexture(path))
            {
                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (tex == null)
                {
                    Debug.LogWarning($"Could not find texture '{path}'"
                        + "- no auto-linking of the texture");
                    return;

                }

                (string materialName, string mapType) = _ParseTexturePath(path);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(
                    Path.Combine(materialAssetDir, $"{materialName}.mat"));
                if (material == null)
                {
                    Debug.LogWarning($"Could not find material '{materialName}'"
                        + " - no auto-linking of the textures");
                    return;
                }

                if (mapType == "__diffuse")
                    material.SetTexture("_MainTex", tex);
                else if (mapType == "__normal")
                    material.SetTexture("_BumpMap", tex);
                else if (mapType == "__specular")
                    material.SetTexture("_MetallicGlossMap", tex);
            }
        }

        int n = _incompleteAssets;
        _incompleteAssets = 0;
        if (n > 0)
            AssetDatabase.ForceReserializeAssets();
    }

    private static bool _ShouldProcessModel(string assetPath)
    {
        // only process the files in: "Imports/<_PROCESSOR_FOLDER>"
        if (!assetPath.Contains(Path.Combine("Imports", _PROCESSOR_FOLDER)))
            return false;

        // only process FBX files
        if (!assetPath.EndsWith(".fbx"))
            return false;

        return true;
    }

    private static bool _IsTexture(string assetPath)
    {
        string p = assetPath.ToLower();
        return p.EndsWith(".jpg") || p.EndsWith(".jpeg") || p.EndsWith(".png") || p.EndsWith(".tga");
    }

    private static bool _ShouldProcessTexture(string assetPath)
    {
        // only process the files in: "<_TEXTURES_FOLDER>/<_PROCESSOR_FOLDER>"
        if (!assetPath.Contains(Path.Combine(_TEXTURES_FOLDER, _PROCESSOR_FOLDER)))
            return false;

        return true;
    }

    private static string _GetCharacterFolder(string assetPath)
    {
        return Path.GetFileName(Path.GetDirectoryName(assetPath));
    }

    private static string _GetModelFilePath(string assetPath)
    {
        string[] assetPaths = Directory.GetFiles(Path.GetDirectoryName(assetPath));
        foreach (string p in assetPaths)
        {
            if (Path.GetFileName(p).StartsWith("_"))
                return p;
        }
        return "";
    }

    private static (string, string) _ParseTexturePath(string texPath)
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
