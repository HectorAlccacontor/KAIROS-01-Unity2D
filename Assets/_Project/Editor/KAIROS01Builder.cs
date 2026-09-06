using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KAIROS01Builder
{
    private const string Marker = "ProjectSettings/KAIROS01.done";
    private const string Root = "Assets/_Project";
    private const string ScenePath = Root + "/Scenes/Main.unity";

    static KAIROS01Builder()
    {
        EditorApplication.delayCall += BuildOnce;
    }

    private static void BuildOnce()
    {
        if (File.Exists(Marker))
            return;

        try
        {
            Build();
            Directory.CreateDirectory("ProjectSettings");
            File.WriteAllText(Marker, DateTime.Now.ToString("O"));
            Debug.Log("[KAIROS-01] Prototipo generado correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [MenuItem("Tools/KAIROS-01/Reconstruir prototipo")]
    public static void RebuildFromMenu()
    {
        if (File.Exists(Marker))
            File.Delete(Marker);
        Build();
        File.WriteAllText(Marker, DateTime.Now.ToString("O"));
    }

    public static void Build()
    {
        EnsureFolders();

        int groundLayer = EnsureLayer("Ground");
        Sprite square = EnsureSprite(Root + "/Sprites/square.png", false);
        Sprite circle = EnsureSprite(Root + "/Sprites/circle.png", true);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        var environment = new GameObject("Environment");
        var gameplay = new GameObject("Gameplay");
        new GameObject("UI_Root");

        CreateStaticBlock("Floor", new Vector2(0f, -4.25f), new Vector2(18f, 0.5f),
            new Color(0.16f, 0.20f, 0.25f), square, environment.transform, groundLayer);
        CreateStaticBlock("Wall_Left", new Vector2(-8.75f, 0f), new Vector2(0.5f, 8.5f),
            new Color(0.13f, 0.17f, 0.22f), square, environment.transform, groundLayer);
        CreateStaticBlock("Wall_Right", new Vector2(8.75f, 0f), new Vector2(0.5f, 8.5f),
            new Color(0.13f, 0.17f, 0.22f), square, environment.transform, groundLayer);

        // Plataforma elevada: el cajón permite llegar de forma más cómoda y da propósito al empuje.
        CreateStaticBlock("Platform_Main", new Vector2(1.3f, -0.8f), new Vector2(4.0f, 0.4f),
            new Color(0.20f, 0.30f, 0.38f), square, environment.transform, groundLayer);
        CreateStaticBlock("Platform_Left", new Vector2(5.2f, -2.15f), new Vector2(1.8f, 0.35f),
            new Color(0.20f, 0.30f, 0.38f), square, environment.transform, groundLayer);

        CreatePlayer(square, gameplay.transform, groundLayer);
        CreateCrate(square, gameplay.transform, groundLayer);
        CreateCore(circle, gameplay.transform);
        CreateExit(square, gameplay.transform);

        var gm = new GameObject("GameManager");
        gm.transform.SetParent(gameplay.transform);
        gm.AddComponent<GameManager>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        PlayerSettings.productName = "KAIROS-01";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = true;

        Selection.activeObject = GameObject.FindWithTag("Player");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateCamera()
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        var cam = go.AddComponent<Camera>();
        go.AddComponent<AudioListener>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.035f, 0.055f, 0.075f);
        cam.allowHDR = false;
        cam.allowMSAA = false;
        go.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void CreatePlayer(Sprite sprite, Transform parent, int groundLayer)
    {
        var player = new GameObject("Kiro_Player");
        player.tag = "Player";
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(-6.6f, -3.35f, 0f);
        player.transform.localScale = new Vector3(0.72f, 1.15f, 1f);

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.25f, 0.90f, 1.00f);

        var col = player.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        var rb = player.AddComponent<Rigidbody2D>();
        rb.mass = 1.0f;
        rb.gravityScale = 4.0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var sensor = new GameObject("GroundCheck");
        sensor.transform.SetParent(player.transform);
        sensor.transform.localPosition = new Vector3(0f, -0.54f, 0f);
        sensor.transform.localScale = Vector3.one;

        var controller = player.AddComponent<PlayerController>();
        var so = new SerializedObject(controller);
        so.FindProperty("moveSpeed").floatValue = 7f;
        so.FindProperty("jumpForce").floatValue = 15f;
        so.FindProperty("groundCheck").objectReferenceValue = sensor.transform;
        so.FindProperty("groundCheckRadius").floatValue = 0.18f;
        so.FindProperty("groundLayer").intValue = 1 << groundLayer;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateCrate(Sprite sprite, Transform parent, int groundLayer)
    {
        var box = new GameObject("Calibration_Crate");
        box.transform.SetParent(parent);
        box.transform.position = new Vector3(-2.4f, -3.45f, 0f);
        box.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
        box.layer = groundLayer;

        var sr = box.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1.00f, 0.55f, 0.16f);

        box.AddComponent<BoxCollider2D>();

        var rb = box.AddComponent<Rigidbody2D>();
        rb.mass = 2.2f;
        rb.gravityScale = 4f;
        rb.linearDamping = 1.2f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private static void CreateCore(Sprite sprite, Transform parent)
    {
        var core = new GameObject("Kinetic_Core");
        core.transform.SetParent(parent);
        core.transform.position = new Vector3(1.6f, -0.2f, 0f);
        core.transform.localScale = new Vector3(0.46f, 0.46f, 1f);

        var sr = core.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1.00f, 0.90f, 0.20f);

        var col = core.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        core.AddComponent<KineticCore>();
    }

    private static void CreateExit(Sprite sprite, Transform parent)
    {
        var exit = new GameObject("Exit_Gate");
        exit.transform.SetParent(parent);
        exit.transform.position = new Vector3(7.5f, -3.05f, 0f);
        exit.transform.localScale = new Vector3(0.75f, 1.75f, 1f);

        var sr = exit.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        var col = exit.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        exit.AddComponent<ExitGate>();
    }

    private static void CreateStaticBlock(
        string name, Vector2 position, Vector2 scale, Color color,
        Sprite sprite, Transform parent, int layer)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        go.layer = layer;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;

        go.AddComponent<BoxCollider2D>();
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "_Project");
        EnsureFolder(Root, "Scripts");
        EnsureFolder(Root, "Sprites");
        EnsureFolder(Root, "Scenes");
        EnsureFolder(Root, "Prefabs");
        EnsureFolder(Root, "Audio");
        EnsureFolder(Root, "UI");
        EnsureFolder(Root, "Editor");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0) return existing;

        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );
        var layers = tagManager.FindProperty("layers");

        for (int i = 8; i < 32; i++)
        {
            var p = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(p.stringValue))
            {
                p.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return i;
            }
        }

        throw new Exception("No hay una capa libre para Ground.");
    }

    private static Sprite EnsureSprite(string assetPath, bool circle)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (existing != null) return existing;

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(1f, 1f, 1f, 0f);
        float center = (size - 1) / 2f;
        float radius = size * 0.44f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool visible = true;
                if (circle)
                {
                    float dx = x - center;
                    float dy = y - center;
                    visible = dx * dx + dy * dy <= radius * radius;
                }

                tex.SetPixel(x, y, visible ? Color.white : clear);
            }
        }

        tex.Apply();
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32f;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}

