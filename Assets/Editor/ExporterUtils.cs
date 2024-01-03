using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.IO.Compression;

public static class ExporterUtils
{
    public static PackageJSON MaterialDescriptorToJSON(GorillaMaterialDescriptor materialDescriptor)
    {
        PackageJSON packageJSON = new PackageJSON();
        packageJSON.descriptor = new Descriptor();
        packageJSON.config = new Config();
        packageJSON.descriptor.author = materialDescriptor.AuthorName;
        packageJSON.descriptor.objectName = materialDescriptor.Name;
        packageJSON.descriptor.description = materialDescriptor.Description;
        packageJSON.config.customColors = materialDescriptor.CustomColors;
        packageJSON.config.disableInPublicLobbies = materialDescriptor.DisablePublicLobbies;
        //packageJSON.config.position = "rack";
        return packageJSON;
    }
    public static void ExportPackage(GameObject gameObject, string path, string typeName, PackageJSON packageJSON)
    {
        string folderPath = Path.GetDirectoryName(path);
        string pcFileName = Path.GetFileNameWithoutExtension(path);

        Selection.activeObject = gameObject;
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
        EditorSceneManager.SaveScene(gameObject.scene);

        PrefabUtility.SaveAsPrefabAsset(Selection.activeObject as GameObject, $"Assets/_{typeName}.prefab");
        AssetBundleBuild assetBundleBuild = default;
        assetBundleBuild.assetNames = new string[] { $"Assets/_{typeName}.prefab" };
        assetBundleBuild.assetBundleName = pcFileName;

        BuildPipeline.BuildAssetBundles(Application.temporaryCachePath, new AssetBundleBuild[] { assetBundleBuild }, 0, BuildTarget.StandaloneWindows64);
        EditorPrefs.SetString("currentBuildingAssetBundlePath", folderPath);

        // JSON stuff
        packageJSON.pcFileName = pcFileName;
        string json = JsonUtility.ToJson(packageJSON, true);
        File.WriteAllText(Application.temporaryCachePath + "/package.json", json);
        AssetDatabase.DeleteAsset($"Assets/_{typeName}.prefab");

        // Delete the zip if it already exists and re-zip
        if (File.Exists(Application.temporaryCachePath + "/tempZip.zip"))
        {
            File.Delete(Application.temporaryCachePath + "/tempZip.zip");
        }
        CreateZipFile(
            Application.temporaryCachePath + "/tempZip.zip",
            new List<string> { 
                Application.temporaryCachePath + "/" + pcFileName, 
                Application.temporaryCachePath + "/package.json" 
            }
        );
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.Move(Application.temporaryCachePath + "/tempZip.zip", path);
        Object.DestroyImmediate(gameObject);
        AssetDatabase.Refresh();
    }

    public static void CreateZipFile(string fileName, IEnumerable<string> files)
    {
        // Create and open a new ZIP file
        var zip = ZipFile.Open(fileName, ZipArchiveMode.Create);
        foreach (var file in files)
        {
            // Add the entry for each file
            zip.CreateEntryFromFile(file, Path.GetFileName(file), System.IO.Compression.CompressionLevel.Optimal);
        }
        // Dispose of the object when we are done
        zip.Dispose();
    }

}
