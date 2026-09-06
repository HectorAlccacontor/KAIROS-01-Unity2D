using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KAIROS01LevelCameraV3
{
    private const string Marker = "ProjectSettings/KAIROS01-LevelCamera-v3.done";

    private const string EnvPath =
        "Assets/_Project/Art/Environment/bg_laboratorio_1280x720.png";

    private const string Props =
        "Assets/_Project/Art/Props/";

    // Ventana lógica del juego.
    private const float WorldHeight = 10f;
    private const float WorldWidth = 17.7777778f; // 16:9 con altura 10

    // Medición proporcionada: la parte superior del piso se encuentra
    // 132 px por encima del borde inferior en la referencia 1280x720.
    private const float FloorPixelsFromBottom = 132f;
    private const float ReferenceHeightPx = 720f;

    private static float FloorTop =>
        -WorldHeight * 0.5f +
        (FloorPixelsFromBottom / ReferenceHeightPx) * WorldHeight;

    static KAIROS01LevelCameraV3()
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
            Debug.Log("[KAIROS-01] Nivel/cámara v3 aplicado.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [MenuItem("Tools/KAIROS-01/Aplicar nivel y cámara v3")]
    public static void Apply()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0)
            throw new Exception("No existe la capa Ground.");

        Sprite background = ConfigureBackgroundImporter();

        Camera cam = SetupCamera();
        SetupBackground(background);

        SetupFloorAndWalls(groundLayer);
        SetupPlayer(groundLayer);
        SetupCrate(groundLayer);

        // Se eliminan solamente las plataformas flotantes anteriores.
        DestroyIfExists("Platform_Main");
        DestroyIfExists("Platform_Left");
        DestroyIfExists("Platform_01");
        DestroyIfExists("Platform_02");
        DestroyIfExists("Platform_03");
        DestroyIfExists("Platform_04");

        // Ruta deliberadamente jugable:
        // suelo -> caja -> P1 -> P2 -> P3 -> P4/núcleo -> suelo/puerta.
        CreateOneWayPlatform(
            "Platform_01",
            new Vector2(-3.75f, -1.85f),
            1.55f,
            "platform_hover_small.png",
            groundLayer
        );

        CreateOneWayPlatform(
            "Platform_02",
            new Vector2(-1.05f, -0.95f),
            2.30f,
            "platform_long.png",
            groundLayer
        );

        CreateOneWayPlatform(
            "Platform_03",
            new Vector2(1.75f, -1.75f),
            1.55f,
            "platform_hover_small.png",
            groundLayer
        );

        CreateOneWayPlatform(
            "Platform_04",
            new Vector2(4.45f, -0.95f),
            2.30f,
            "platform_long.png",
            groundLayer
        );

        SetupCore();
        SetupGate();

        // La consola adicional duplicaba información ya dibujada en el fondo.
        DestroyIfExists("Console_Decor");

        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;

        ValidateRoute();

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = GameObject.Find("Kiro_Player");

        Debug.Log(
            $"[KAIROS-01] Piso alineado: Y={FloorTop:F3}. " +
            "Cámara 16:9 completa y plataformas one-way listas."
        );
    }

    private static Camera SetupCamera()
    {
        GameObject go = GameObject.Find("Main Camera");
        if (go == null)
        {
            go = new GameObject("Main Camera");
            go.tag = "MainCamera";
        }

        Camera cam = go.GetComponent<Camera>();
        if (cam == null)
            cam = go.AddComponent<Camera>();

        if (go.GetComponent<AudioListener>() == null)
            go.AddComponent<AudioListener>();

        if (go.GetComponent<FixedAspectCamera>() == null)
            go.AddComponent<FixedAspectCamera>();

        go.transform.position = new Vector3(0f, 0f, -10f);
        go.transform.rotation = Quaternion.identity;

        cam.orthographic = true;
        cam.orthographicSize = WorldHeight * 0.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.01f, 0.015f, 0.025f);
        cam.allowHDR = false;
        cam.allowMSAA = false;

        return cam;
    }

    private static Sprite ConfigureBackgroundImporter()
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(EnvPath) as TextureImporter;

        if (importer == null)
            throw new Exception("No se encontró el fondo: " + EnvPath);

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(EnvPath);
        if (tex == null)
        {
            AssetDatabase.ImportAsset(EnvPath, ImportAssetOptions.ForceUpdate);
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(EnvPath);
        }

        if (tex == null)
            throw new Exception("No se pudo leer la textura del fondo.");

        // Hace que la altura completa del PNG equivalga exactamente a 10
        // unidades de mundo. No redimensiona ni deforma el archivo.
        float ppu = tex.height / WorldHeight;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = ppu;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.maxTextureSize = 2048;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spritePivot = new Vector2(0.5f, 0.5f);
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>(EnvPath);
        if (bg == null)
            throw new Exception("No se pudo cargar el Sprite del fondo.");

        return bg;
    }

    private static void SetupBackground(Sprite sprite)
    {
        GameObject env = GameObject.Find("Environment");
        if (env == null)
            env = new GameObject("Environment");

        Transform t = env.transform.Find("Background_Art");
        GameObject bg = t != null ? t.gameObject : new GameObject("Background_Art");

        if (t == null)
            bg.transform.SetParent(env.transform, false);

        SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = bg.AddComponent<SpriteRenderer>();

        sr.sprite = sprite;
        sr.sortingOrder = -100;

        bg.transform.position = new Vector3(0f, 0f, 5f);
        bg.transform.rotation = Quaternion.identity;

        // Fit completo conservando proporción. La imagen original es
        // prácticamente 16:9; cualquier margen residual es subpíxel visual.
        float sx = WorldWidth / sprite.bounds.size.x;
        float sy = WorldHeight / sprite.bounds.size.y;
        float scale = Mathf.Min(sx, sy);

        bg.transform.localScale = Vector3.one * scale;
    }

    private static void SetupFloorAndWalls(int groundLayer)
    {
        GameObject floor = EnsureRoot("Floor");
        floor.layer = groundLayer;
        floor.transform.position = new Vector3(0f, FloorTop - 0.12f, 0f);
        floor.transform.localScale = Vector3.one;
        DisableRenderer(floor);

        BoxCollider2D floorCol = EnsureBox(floor);
        floorCol.size = new Vector2(16.35f, 0.24f);
        floorCol.offset = Vector2.zero;
        floorCol.usedByEffector = false;

        const float wallTop = 1.45f;
        float wallHeight = wallTop - FloorTop;
        float wallY = (wallTop + FloorTop) * 0.5f;

        SetupWall("Wall_Left", -8.12f, wallY, wallHeight, groundLayer);
        SetupWall("Wall_Right", 8.12f, wallY, wallHeight, groundLayer);
    }

    private static void SetupWall(
        string name, float x, float y, float height, int groundLayer)
    {
        GameObject wall = EnsureRoot(name);
        wall.layer = groundLayer;
        wall.transform.position = new Vector3(x, y, 0f);
        wall.transform.localScale = Vector3.one;
        DisableRenderer(wall);

        BoxCollider2D col = EnsureBox(wall);
        col.size = new Vector2(0.30f, height);
        col.offset = Vector2.zero;
        col.usedByEffector = false;
    }

    private static void SetupPlayer(int groundLayer)
    {
        GameObject player = GameObject.Find("Kiro_Player");
        if (player == null)
            throw new Exception("No se encontró Kiro_Player.");

        player.transform.position =
            new Vector3(-7.15f, FloorTop + 0.55f, 0f);

        player.transform.localScale = Vector3.one;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.mass = 1f;
            rb.gravityScale = 4f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        BoxCollider2D col = player.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(0.68f, 1.10f);
            col.offset = Vector2.zero;
        }

        Transform gc = player.transform.Find("GroundCheck");
        if (gc != null)
            gc.localPosition = new Vector3(0f, -0.58f, 0f);

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            SerializedObject so = new SerializedObject(pc);
            SerializedProperty move = so.FindProperty("moveSpeed");
            SerializedProperty jump = so.FindProperty("jumpForce");
            SerializedProperty radius = so.FindProperty("groundCheckRadius");
            SerializedProperty layer = so.FindProperty("groundLayer");

            if (move != null) move.floatValue = 7f;
            if (jump != null) jump.floatValue = 9.5f;
            if (radius != null) radius.floatValue = 0.16f;
            if (layer != null) layer.intValue = 1 << groundLayer;

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetupCrate(int groundLayer)
    {
        GameObject crate = GameObject.Find("Calibration_Crate");
        if (crate == null)
            throw new Exception("No se encontró Calibration_Crate.");

        crate.layer = groundLayer;
        crate.transform.position =
            new Vector3(-5.15f, FloorTop + 0.525f, 0f);
        crate.transform.localScale = Vector3.one;

        BoxCollider2D col = crate.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(1.05f, 1.05f);
            col.offset = Vector2.zero;
        }

        Rigidbody2D rb = crate.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.mass = 2.2f;
            rb.gravityScale = 4f;
            rb.linearDamping = 1.2f;
            rb.angularDamping = 0.05f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private static void CreateOneWayPlatform(
        string name,
        Vector2 topCenter,
        float visualWidth,
        string spriteFile,
        int groundLayer)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Props + spriteFile);
        if (sprite == null)
            throw new Exception("No se encontró " + spriteFile);

        GameObject go = new GameObject(name);
        go.layer = groundLayer;
        go.transform.position = new Vector3(topCenter.x, topCenter.y, 0f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(visualWidth * 0.92f, 0.18f);
        col.offset = new Vector2(0f, -0.09f);
        col.usedByEffector = true;

        PlatformEffector2D eff = go.AddComponent<PlatformEffector2D>();
        eff.useOneWay = true;
        eff.useSideFriction = false;
        eff.surfaceArc = 170f;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform, false);

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        float uniformScale = visualWidth / sprite.bounds.size.x;
        visual.transform.localScale = Vector3.one * uniformScale;

        float visualHeight = sprite.bounds.size.y * uniformScale;

        // El borde superior visual coincide con la superficie física.
        visual.transform.localPosition =
            new Vector3(0f, -visualHeight * 0.5f, 0f);
    }

    private static void SetupCore()
    {
        GameObject core = GameObject.Find("Kinetic_Core");
        if (core == null)
            throw new Exception("No se encontró Kinetic_Core.");

        // Encima de Platform_04.
        core.transform.position = new Vector3(4.45f, -0.27f, 0f);
        core.transform.localScale = Vector3.one;
    }

    private static void SetupGate()
    {
        GameObject gate = GameObject.Find("Exit_Gate");
        if (gate == null)
            throw new Exception("No se encontró Exit_Gate.");

        gate.transform.position =
            new Vector3(7.05f, FloorTop + 0.95f, 0f);

        gate.transform.localScale = Vector3.one;

        BoxCollider2D col = gate.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(0.92f, 1.90f);
            col.offset = Vector2.zero;
            col.isTrigger = false;
        }
    }

    private static GameObject EnsureRoot(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go : new GameObject(name);
    }

    private static BoxCollider2D EnsureBox(GameObject go)
    {
        BoxCollider2D col = go.GetComponent<BoxCollider2D>();
        if (col == null)
            col = go.AddComponent<BoxCollider2D>();
        return col;
    }

    private static void DisableRenderer(GameObject go)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;
    }

    private static void DestroyIfExists(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
            UnityEngine.Object.DestroyImmediate(go);
    }

    private static void ValidateRoute()
    {
        const float gravityScale = 4f;
        const float jumpForce = 9.5f;
        const float moveSpeed = 7f;

        float g = Mathf.Abs(Physics2D.gravity.y) * gravityScale;
        float jumpRise = jumpForce * jumpForce / (2f * g);
        float totalAirTime = 2f * jumpForce / g;
        float horizontalRange = moveSpeed * totalAirTime;

        // Superficies superiores.
        float crateTop = FloorTop + 1.05f;

        Vector2[] route =
        {
            new Vector2(-5.15f, crateTop),
            new Vector2(-3.75f, -1.85f),
            new Vector2(-1.05f, -0.95f),
            new Vector2( 1.75f, -1.75f),
            new Vector2( 4.45f, -0.95f)
        };

        for (int i = 0; i < route.Length - 1; i++)
        {
            Vector2 a = route[i];
            Vector2 b = route[i + 1];

            float dy = b.y - a.y;
            float dx = Mathf.Abs(b.x - a.x);

            bool verticalOK = dy <= jumpRise + 0.05f;
            bool horizontalOK = dx <= horizontalRange * 0.95f;

            if (!verticalOK || !horizontalOK)
            {
                throw new Exception(
                    $"Ruta no validada {i + 1}->{i + 2}: " +
                    $"dx={dx:F2}, dy={dy:F2}, " +
                    $"salto={jumpRise:F2}, rango={horizontalRange:F2}"
                );
            }
        }

        // El primer nivel NO debe poder alcanzarse directamente desde el suelo;
        // la caja debe tener utilidad real.
        float floorToP1 = -1.85f - FloorTop;
        if (floorToP1 <= jumpRise)
        {
            throw new Exception(
                "P1 quedó alcanzable desde el suelo; la caja dejaría de ser necesaria."
            );
        }

        // Altura máxima del collider del personaje desde la plataforma más alta.
        const float playerHeight = 1.10f;
        float highestTop = -0.95f;
        float maxPlayerTop = highestTop + jumpRise + playerHeight;

        if (maxPlayerTop >= 1.45f)
        {
            throw new Exception(
                $"El salto invade la franja superior del laboratorio: {maxPlayerTop:F2}"
            );
        }

        Debug.Log(
            $"[KAIROS-01] VALIDACIÓN DE RUTA OK | " +
            $"salto vertical={jumpRise:F2} u | " +
            $"rango horizontal teórico={horizontalRange:F2} u | " +
            $"techo máx. jugador={maxPlayerTop:F2} u"
        );
    }
}
