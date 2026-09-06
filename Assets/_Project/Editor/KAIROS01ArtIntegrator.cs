using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KAIROS01ArtIntegrator
{
    private const string Marker = "ProjectSettings/KAIROS01-Art-v1.done";

    private const string Kiro = "Assets/_Project/Art/Characters/Kiro/";
    private const string Props = "Assets/_Project/Art/Props/";
    private const string Env = "Assets/_Project/Art/Environment/";

    static KAIROS01ArtIntegrator()
    {
        EditorApplication.delayCall += ApplyOnce;
    }

    private static void ApplyOnce()
    {
        if (File.Exists(Marker))
            return;

        try
        {
            Apply();
            Directory.CreateDirectory("ProjectSettings");
            File.WriteAllText(Marker, DateTime.Now.ToString("O"));
            Debug.Log("[KAIROS-01] Arte integrado correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [MenuItem("Tools/KAIROS-01/Reintegrar arte")]
    public static void Reintegrate()
    {
        Apply();
    }

    public static void Apply()
    {
        ConfigureAllImporters();

        Camera cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
        if (cam == null)
            throw new Exception("No se encontró Main Camera.");

        IntegrateBackground(cam);
        IntegratePlayer();
        IntegrateCrate();
        IntegrateCore();
        IntegrateGate();
        IntegratePlatforms();
        IntegrateOptionalDecor();

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[KAIROS-01] Fondo, personaje, props y plataformas listos.");
    }

    private static void ConfigureAllImporters()
    {
        ConfigureSprite(Env + "bg_laboratorio_1280x720.png", new Vector2(0.5f, 0.5f), 100f, 2048);

        string[] kiroFiles =
        {
            "kiro_idle_right.png","kiro_run_right_1.png","kiro_run_right_2.png",
            "kiro_jump_right.png","kiro_fall_right.png","kiro_push_right.png",
            "kiro_idle_left.png","kiro_run_left_1.png","kiro_run_left_2.png",
            "kiro_jump_left.png","kiro_fall_left.png","kiro_push_left.png"
        };

        foreach (string f in kiroFiles)
            ConfigureSprite(Kiro + f, new Vector2(0.5f, 0.0f), 100f, 2048);

        string[] propFiles =
        {
            "core_energy.png","crate_calibration.png",
            "exit_gate_locked.png","exit_gate_unlocked.png",
            "platform_hover_small.png","platform_long.png",
            "floor_block.png","wall_block.png",
            "stairs_3step.png","console_lab.png"
        };

        foreach (string f in propFiles)
            ConfigureSprite(Props + f, new Vector2(0.5f, 0.5f), 100f, 2048);

        AssetDatabase.Refresh();
    }

    private static void ConfigureSprite(string path, Vector2 pivot, float ppu, int maxSize)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new Exception("No se pudo importar: " + path);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = ppu;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.maxTextureSize = maxSize;

        // Unity 6: alignment y pivot se escriben mediante TextureImporterSettings.
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = pivot;
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null)
            throw new Exception("Sprite no encontrado: " + path);
        return s;
    }

    private static GameObject EnsureChild(GameObject parent, string name)
    {
        Transform existing = parent.transform.Find(name);
        if (existing != null)
            return existing.gameObject;

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static SpriteRenderer EnsureVisual(GameObject root, int sortingOrder)
    {
        GameObject visual = EnsureChild(root, "Visual");
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = visual.AddComponent<SpriteRenderer>();

        sr.sortingOrder = sortingOrder;

        SpriteRenderer rootSr = root.GetComponent<SpriteRenderer>();
        if (rootSr != null)
            rootSr.enabled = false;

        return sr;
    }

    private static void IntegrateBackground(Camera cam)
    {
        GameObject envRoot = GameObject.Find("Environment");
        if (envRoot == null)
            envRoot = new GameObject("Environment");

        GameObject bg = EnsureChild(envRoot, "Background_Art");
        SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = bg.AddComponent<SpriteRenderer>();

        sr.sprite = LoadSprite(Env + "bg_laboratorio_1280x720.png");
        sr.sortingOrder = -100;

        bg.transform.position = new Vector3(0f, 0f, 5f);
        bg.transform.rotation = Quaternion.identity;

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        Vector2 size = sr.sprite.bounds.size;
        float scale = Mathf.Max(worldWidth / size.x, worldHeight / size.y);
        bg.transform.localScale = Vector3.one * scale;
    }

    private static void IntegratePlayer()
    {
        GameObject player = GameObject.Find("Kiro_Player");
        if (player == null)
            throw new Exception("No se encontró Kiro_Player.");

        // Física estable, independiente del tamaño visual.
        player.transform.localScale = Vector3.one;

        BoxCollider2D box = player.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = new Vector2(0.68f, 1.10f);
            box.offset = Vector2.zero;
        }

        Transform groundCheck = player.transform.Find("GroundCheck");
        if (groundCheck != null)
            groundCheck.localPosition = new Vector3(0f, -0.58f, 0f);

        SpriteRenderer visual = EnsureVisual(player, 20);
        visual.sprite = LoadSprite(Kiro + "kiro_idle_right.png");
        visual.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        ScaleVisualToHeight(visual, 1.38f);

        KiroVisualController vc = player.GetComponent<KiroVisualController>();
        if (vc == null)
            vc = player.AddComponent<KiroVisualController>();

        SerializedObject so = new SerializedObject(vc);
        so.FindProperty("visualRenderer").objectReferenceValue = visual;

        SetSprite(so, "idleRight", "kiro_idle_right.png");
        SetSprite(so, "runRight1", "kiro_run_right_1.png");
        SetSprite(so, "runRight2", "kiro_run_right_2.png");
        SetSprite(so, "jumpRight", "kiro_jump_right.png");
        SetSprite(so, "fallRight", "kiro_fall_right.png");
        SetSprite(so, "pushRight", "kiro_push_right.png");

        SetSprite(so, "idleLeft", "kiro_idle_left.png");
        SetSprite(so, "runLeft1", "kiro_run_left_1.png");
        SetSprite(so, "runLeft2", "kiro_run_left_2.png");
        SetSprite(so, "jumpLeft", "kiro_jump_left.png");
        SetSprite(so, "fallLeft", "kiro_fall_left.png");
        SetSprite(so, "pushLeft", "kiro_push_left.png");

        so.FindProperty("runFramesPerSecond").floatValue = 10f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSprite(SerializedObject so, string property, string file)
    {
        SerializedProperty p = so.FindProperty(property);
        if (p != null)
            p.objectReferenceValue = LoadSprite(Kiro + file);
    }

    private static void IntegrateCrate()
    {
        GameObject crate = GameObject.Find("Calibration_Crate");
        if (crate == null) return;

        crate.transform.localScale = Vector3.one;

        BoxCollider2D box = crate.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = new Vector2(1.05f, 1.05f);
            box.offset = Vector2.zero;
        }

        SpriteRenderer sr = EnsureVisual(crate, 12);
        sr.sprite = LoadSprite(Props + "crate_calibration.png");
        sr.transform.localPosition = Vector3.zero;
        ScaleVisualToWidth(sr, 1.12f);
    }

    private static void IntegrateCore()
    {
        GameObject core = GameObject.Find("Kinetic_Core");
        if (core == null) return;

        core.transform.localScale = Vector3.one;

        CircleCollider2D col = core.GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.radius = 0.31f;
            col.offset = Vector2.zero;
            col.isTrigger = true;
        }

        SpriteRenderer sr = EnsureVisual(core, 15);
        sr.sprite = LoadSprite(Props + "core_energy.png");
        sr.transform.localPosition = Vector3.zero;
        ScaleVisualToWidth(sr, 0.72f);
    }

    private static void IntegrateGate()
    {
        GameObject gate = GameObject.Find("Exit_Gate");
        if (gate == null) return;

        gate.transform.localScale = Vector3.one;

        BoxCollider2D col = gate.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(0.92f, 1.72f);
            col.offset = Vector2.zero;
            col.isTrigger = false;
        }

        SpriteRenderer sr = EnsureVisual(gate, 10);
        sr.sprite = LoadSprite(Props + "exit_gate_locked.png");
        sr.transform.localPosition = Vector3.zero;
        ScaleVisualToHeight(sr, 1.95f);

        ExitGate exit = gate.GetComponent<ExitGate>();
        if (exit == null)
            exit = gate.AddComponent<ExitGate>();

        SerializedObject so = new SerializedObject(exit);
        so.FindProperty("visualRenderer").objectReferenceValue = sr;
        so.FindProperty("lockedSprite").objectReferenceValue = LoadSprite(Props + "exit_gate_locked.png");
        so.FindProperty("unlockedSprite").objectReferenceValue = LoadSprite(Props + "exit_gate_unlocked.png");
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void IntegratePlatforms()
    {
        GameObject main = GameObject.Find("Platform_Main");
        if (main != null)
        {
            SpriteRenderer sr = EnsureVisual(main, 5);
            sr.sprite = LoadSprite(Props + "platform_long.png");
            sr.transform.localPosition = Vector3.zero;
            ScaleVisualToWidth(sr, 4.05f);
        }

        GameObject small = GameObject.Find("Platform_Left");
        if (small != null)
        {
            SpriteRenderer sr = EnsureVisual(small, 5);
            sr.sprite = LoadSprite(Props + "platform_hover_small.png");
            sr.transform.localPosition = Vector3.zero;
            ScaleVisualToWidth(sr, 1.85f);
        }

        // Oculta los rectángulos del floor/walls: el fondo ya contiene el acabado visual.
        HideRootRenderer("Floor");
        HideRootRenderer("Wall_Left");
        HideRootRenderer("Wall_Right");
    }

    private static void IntegrateOptionalDecor()
    {
        GameObject envRoot = GameObject.Find("Environment");
        if (envRoot == null) return;

        // Consola decorativa, sin collider.
        GameObject console = EnsureChild(envRoot, "Console_Decor");
        SpriteRenderer sr = console.GetComponent<SpriteRenderer>();
        if (sr == null) sr = console.AddComponent<SpriteRenderer>();

        sr.sprite = LoadSprite(Props + "console_lab.png");
        sr.sortingOrder = -10;
        console.transform.position = new Vector3(-5.9f, 2.8f, 0f);
        console.transform.rotation = Quaternion.identity;
        ScaleVisualToWidth(sr, 1.55f);

        // Importados y disponibles para el siguiente ajuste de nivel:
        // floor_block.png
        // wall_block.png
        // stairs_3step.png
    }

    private static void HideRootRenderer(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null) return;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
    }

    private static void ScaleVisualToWidth(SpriteRenderer sr, float targetWidth)
    {
        if (sr == null || sr.sprite == null) return;

        float current = sr.sprite.bounds.size.x;
        float scale = current > 0.0001f ? targetWidth / current : 1f;
        sr.transform.localScale = Vector3.one * scale;
    }

    private static void ScaleVisualToHeight(SpriteRenderer sr, float targetHeight)
    {
        if (sr == null || sr.sprite == null) return;

        float current = sr.sprite.bounds.size.y;
        float scale = current > 0.0001f ? targetHeight / current : 1f;
        sr.transform.localScale = Vector3.one * scale;
    }
}

