using System.IO;
using UnityEngine;
using UnityEditor;

namespace Assets
{

    public class StaticAssetImporter : BaseAssetImporter
    {
        protected override string _GetProcessorFolder() => "Static";

        private Material _lowPolyMaterial;
        private Material _LowPolyMaterial
        {
            get
            {
                if (_lowPolyMaterial == null)
                    _lowPolyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                        Path.Combine("Assets", _MATERIALS_FOLDER, "LowPoly.mat"));
                return _lowPolyMaterial;
            }
        }

        void OnPreprocessModel()
        {
            if (!_ShouldProcessModel(this, assetPath)) return;

            ModelImporter modelImporter = assetImporter as ModelImporter;

            // "Model" options - shared between all model files
            modelImporter.bakeAxisConversion = true;

            // "Rig" options
            modelImporter.animationType = ModelImporterAnimationType.None;
            modelImporter.avatarSetup = ModelImporterAvatarSetup.NoAvatar;

            // "Animation" options
            modelImporter.importAnimation = false;

            // "Materials" options
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
        }

        void OnPostprocessModel(GameObject g)
        {
            if (!_ShouldProcessModel(this, assetPath)) return;
            Debug.Log(Path.Combine("Assets", _MATERIALS_FOLDER, "LowPoly.mat"));
            Debug.Log(_LowPolyMaterial);
            g.GetComponent<Renderer>().material = _LowPolyMaterial;
        }

    }

}
