using UnityEngine;
using UnityEditor;

public class EveryplayPackageImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Don't compress Everyplay textures, makes importing faster
        if (assetPath.Contains("Plugins/Everyplay"))
        {
            TextureImporter textureImporter = (TextureImporter) assetImporter;
            if (textureImporter != null)
            {
                #if UNITY_5_5_OR_NEWER
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                #else
                textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                #endif
            }
        }
    }
}
