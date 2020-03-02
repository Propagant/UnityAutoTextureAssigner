#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;

/// <summary>
/// Texture Auto Assigner editor window made by Matej Vanco 27.01.2020 for internal purposes
/// The lincence is free, you are free to share or distribute...
/// 
/// How to use the Texture Auto Assign?
/// The Texture Auto Assigner is specially made for Standard Shader models only!
/// Open the main window in Window/Texture Auto Assign. Set up the specific asset path for directory containing target materials you want to use for assign.
/// Set up the second path (doesn't have to be specific) for directory that contain textures for selected materials.
/// Both textures and materials must have the same name, at least the textures must contain the material name. Additionally you can add 'naming macro' which is nice for
/// differing albedo or specular maps... Example: I'm going to assign just Albedo maps. My albedo texture name is 'texture_AlbedoTransparency'. So my naming macro would be '_AlbedoTransparency'
/// The material name should be at least 'texture'
/// 
/// If you have any troubles setting up the Texture Auto Assigner, contact me here https://matejvanco.com/contact/
/// </summary>
public class TextureAutoAssigner : EditorWindow
{
    [MenuItem("Window/Texture Auto Assigner")]
    public static void Init()
    {
        TextureAutoAssigner win = (TextureAutoAssigner)GetWindow(typeof(TextureAutoAssigner));
        win.minSize = new Vector2(329, 330);
        win.maxSize = new Vector2(329, 330);
        win.Show();
    }

    private List<Material> allMaterials = new List<Material>();
    private string specPathToGetMaterials = "Assets/"; //---Specific path containing all materials for texture assignation
    private string pathToGetTextures = "Assets/"; //---Specific path for located textures (the program will look up in all sub-directories)
    private string namingMacro; //---Additional naming macro for detailed specification
    private string extension = ".png"; //---Any extension... Unity recommends using png format
    private enum texType { Albedo, Normal, Metallic }; //---Texture type
    private texType textureType;

    private Color matColor = Color.white; //---Additional material color (sometimes the imported materials contain colored information)

    private void OnGUI()
    {
        s();
        l("_Texture Auto Assigner_",18, TextAnchor.MiddleCenter);
        l("Quick editor extension to make your life much easier...\nby Matej Vanco 2020", 10, TextAnchor.MiddleCenter);
        s(15);
        l("Specific Path To Get All Materials");
        specPathToGetMaterials = GUILayout.TextField(specPathToGetMaterials);
        s(5);
        l("Path To Get All Textures (with sub-dirs)");
        pathToGetTextures = GUILayout.TextField(pathToGetTextures);
        s();
        l("Additional Naming Macro");
        namingMacro = GUILayout.TextField(namingMacro);
        s(5);
        l("Additional Extension");
        extension = GUILayout.TextField(extension);

        s();
        matColor = EditorGUILayout.ColorField("Material Color",matColor);
        textureType = (texType)EditorGUILayout.EnumPopup("Texture Type", textureType);
        s();
        if (GUILayout.Button("Assign Textures"))
        {
            string storage_p1 = specPathToGetMaterials;
            string storage_p2 = pathToGetTextures;
            specPathToGetMaterials += "/";
            pathToGetTextures += "/";
            allMaterials.Clear();

            for (int i = 0; i < Directory.GetFiles(specPathToGetMaterials).Length; i++)
            {
                string cPath = Directory.GetFiles(specPathToGetMaterials)[i];
                if (Path.GetExtension(cPath) == ".meta")
                    continue;
                Material m = AssetDatabase.LoadAssetAtPath(specPathToGetMaterials + Path.GetFileName(cPath), typeof(Material)) as Material;
                allMaterials.Add(m);
            }

            foreach (Material mm in allMaterials)
            {
                string specFile = Directory.GetFiles(pathToGetTextures, mm.name + namingMacro + extension, SearchOption.AllDirectories).FirstOrDefault();
                if (specFile == null)
                    Debug.Log("Not Assigned [" + mm.name + "]");
                else
                {
                    Texture2D tex = AssetDatabase.LoadAssetAtPath(specFile, typeof(Texture2D)) as Texture2D;
                    mm.color = matColor;
                    if (textureType == texType.Albedo)
                        mm.SetTexture("_MainTex", tex);
                    else if (textureType == texType.Normal)
                    {
                        TextureImporter importer = AssetImporter.GetAtPath(specFile) as TextureImporter;
                        importer.textureType = TextureImporterType.NormalMap;
                        AssetDatabase.WriteImportSettingsIfDirty(specFile);
                        mm.SetTexture("_BumpMap", tex);
                    }
                    else if (textureType == texType.Metallic)
                        mm.SetTexture("_MetallicGlossMap", tex);
                    Debug.Log("Assigned to " + mm.name);
                }
            }
            specPathToGetMaterials = storage_p1;
            pathToGetTextures = storage_p2;
            
            AssetDatabase.Refresh();
        }
    }

    #region GUI Internal & Editor Methods

    private static void s(float siz = 10)
    {
        GUILayout.Space(siz);
    }
    private static void l(string t, int fontSize = 12, TextAnchor align = TextAnchor.MiddleLeft)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = fontSize;
        style.alignment = align;
        try
        {
            GUILayout.Label("   " + t, style);
        }
        catch { }
    }

    #endregion
}
#endif
