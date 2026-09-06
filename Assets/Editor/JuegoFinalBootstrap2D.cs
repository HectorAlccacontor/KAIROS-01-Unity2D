using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class JuegoFinalBootstrap2D
{
    private const string Marker = "ProjectSettings/JuegoFinalBootstrap2D.done";
    private const string MainScene = "Assets/Scenes/Main.unity";

    static JuegoFinalBootstrap2D()
    {
        EditorApplication.delayCall += AutoSetup;
    }

    private static void AutoSetup()
    {
        if (File.Exists(Marker))
            return;

        Setup();
        Directory.CreateDirectory("ProjectSettings");
        File.WriteAllText(Marker, DateTime.Now.ToString("O"));
        Debug.Log("[JuegoFinal] Bootstrap 2D completado.");
    }

    [MenuItem("Tools/JuegoFinal/Reaplicar configuracion 2D")]
    public static void Setup()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Scripts");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Audio");
        EnsureFolder("Assets", "Art");
        EnsureFolder("Assets/Art", "Backgrounds");
        EnsureFolder("Assets/Art", "Characters");
        EnsureFolder("Assets/Art", "Props");
        EnsureFolder("Assets/Art", "UI");
        EnsureFolder("Assets", "Editor");

        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

        Scene scene = SceneManager.GetActiveScene();

        Camera cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.tag = "MainCamera";
        }

        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.allowHDR = false;
        cam.allowMSAA = false;

        foreach (var light in UnityEngine.Object.FindObjectsByType<Light>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (light.type == LightType.Directional)
                UnityEngine.Object.DestroyImmediate(light.gameObject);
        }

        EnsureRoot("Environment");
        EnsureRoot("Gameplay");
        EnsureRoot("UI_Root");

        PlayerSettings.productName = "JuegoFinal";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, MainScene);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainScene, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = cam.gameObject;

        Debug.Log("[JuegoFinal] Proyecto configurado como 2D ligero. Escena: " + MainScene);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static void EnsureRoot(string name)
    {
        var existing = SceneManager.GetActiveScene()
            .GetRootGameObjects()
            .FirstOrDefault(g => g.name == name);

        if (existing == null)
            new GameObject(name);
    }
}
