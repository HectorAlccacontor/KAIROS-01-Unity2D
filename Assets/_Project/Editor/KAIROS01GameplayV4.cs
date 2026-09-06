using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KAIROS01GameplayV4
{
    private const string Marker = "ProjectSettings/KAIROS01-Gameplay-v4.done";

    private const string BgPath =
        "Assets/_Project/Art/Environment/bg_laboratorio_1280x720.png";

    private const string Props =
        "Assets/_Project/Art/Props/";

    private const float WorldHeight = 10f;
    private const float WorldWidth = 17.7777778f;

    // Medición del usuario sobre la referencia 1280x720.
    private const float FloorPixelsFromBottom = 132f;
    private const float ReferenceHeightPx = 720f;

    private const float MoveSpeed = 4.4f;
    private const float JumpForce = 11.5f;
    private const float GravityScale = 4f;

    private static float FloorTop =>
        -WorldHeight * 0.5f +
        (FloorPixelsFromBottom / ReferenceHeightPx) * WorldHeight;

    static KAIROS01GameplayV4()
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
            Debug.Log("[KAIROS-01] Gameplay v4 aplicado correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [MenuItem("Tools/KAIROS-01/Aplicar Gameplay v4")]
    public static void Apply()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0)
            throw new Exception("No existe la capa Ground.");

        SetupCameraAndBackground();
        SetupFloorAndWalls(groundLayer);
        SetupPlayer(groundLayer);
        SetupCrate(groundLayer);

        RemovePlatforms();

        // Coordenadas = SUPERFICIE SUPERIOR de cada plataforma.
        // Ruta: piso -> P1 -> P2 -> P3 -> P4/core -> bajar al piso -> puerta.
        CreateOneWayPlatform(
            "Platform_01",
            new Vector2(-4.00f, -1.95f),
            1.55f,
            "platform_hover_small.png",
            groundLayer
        );

        CreateOneWayPlatform(
            "Platform_02",
            new Vector2(-1.35f, -1.45f),
            2.30f,
            "platform_long.png",
            groundLayer
        );

        CreateOneWayPlatform(
            "Platform_03",
            new Vector2(1.20f, -2.05f),
            1.55f,
            "platform_hover_small.png",
            groundLayer
        );

        CreateOneWayPlatform(
            "Platform_04",
            new Vector2(3.85f, -1.45f),
            2.30f,
            "platform_long.png",
            groundLayer
        );

        SetupCore();
        SetupGate();

        ValidateRoute();

        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = false;

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = GameObject.Find("Kiro_Player");

        Debug.Log(
            $"[KAIROS-01] GAMEPLAY V4 OK | " +
            $"Floor Y={FloorTop:F3} | Speed={MoveSpeed:F1} | Jump={JumpForce:F1}"
        );
    }

    private static void SetupCameraAndBackground()
    {
        GameObject camGo = GameObject.Find("Main Camera");
        if (camGo == null)
            throw new Exception("No se encontró Main Camera.");

        Camera cam = camGo.GetComponent<Camera>();
        if (cam == null)
            throw new Exception("Main Camera no tiene Camera.");

        // Quitamos el letterbox dinámico anterior. El juego final se ejecutará
        // a 1280x720 exactos; en el Editor debe elegirse Game View 16:9.
        FixedAspectCamera fixedAspect = camGo.GetComponent<FixedAspectCamera>();
        if (fixedAspect != null)
            UnityEngine.Object.DestroyImmediate(fixedAspect);

        cam.rect = new Rect(0f, 0f, 1f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = WorldHeight * 0.5f;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.transform.rotation = Quaternion.identity;

        GameObject bg = GameObject.Find("Background_Art");
        if (bg == null)
            throw new Exception("No se encontró Background_Art.");

        SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
            throw new Exception("Background_Art no tiene Sprite.");

        // Fill 16:9 sin deformar. Por la diferencia subpíxel del PNG
        // (1672x941 frente a 16:9), se recorta menos de 1 px vertical.
        float sx = WorldWidth / sr.sprite.bounds.size.x;
        float sy = WorldHeight / sr.sprite.bounds.size.y;
        float uniformScale = Mathf.Max(sx, sy);

        bg.transform.position = new Vector3(0f, 0f, 5f);
        bg.transform.localScale = Vector3.one * uniformScale;
    }

    private static void SetupFloorAndWalls(int groundLayer)
    {
        GameObject floor = GameObject.Find("Floor");
        if (floor == null) floor = new GameObject("Floor");

        floor.layer = groundLayer;
        floor.transform.position = new Vector3(0f, FloorTop - 0.12f, 0f);
        floor.transform.localScale = Vector3.one;

        DisableRenderer(floor);

        BoxCollider2D floorCol = EnsureBox(floor);
        floorCol.size = new Vector2(16.25f, 0.24f);
        floorCol.offset = Vector2.zero;
        floorCol.usedByEffector = false;

        // Laterales sólidos hasta la zona decorativa superior.
        SetupWall("Wall_Left", -8.05f, groundLayer);
        SetupWall("Wall_Right", 8.05f, groundLayer);
    }

    private static void SetupWall(string name, float x, int groundLayer)
    {
        const float wallTop = 1.40f;
        float height = wallTop - FloorTop;
        float centerY = (wallTop + FloorTop) * 0.5f;

        GameObject wall = GameObject.Find(name);
        if (wall == null) wall = new GameObject(name);

        wall.layer = groundLayer;
        wall.transform.position = new Vector3(x, centerY, 0f);
        wall.transform.localScale = Vector3.one;

        DisableRenderer(wall);

        BoxCollider2D col = EnsureBox(wall);
        col.size = new Vector2(0.28f, height);
        col.offset = Vector2.zero;
        col.usedByEffector = false;
    }

    private static void SetupPlayer(int groundLayer)
    {
        GameObject player = GameObject.Find("Kiro_Player");
        if (player == null)
            throw new Exception("No se encontró Kiro_Player.");

        player.transform.position =
            new Vector3(-7.00f, FloorTop + 0.55f, 0f);
        player.transform.localScale = Vector3.one;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.mass = 1f;
            rb.gravityScale = GravityScale;
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

            SerializedProperty pMove = so.FindProperty("moveSpeed");
            SerializedProperty pJump = so.FindProperty("jumpForce");
            SerializedProperty pRadius = so.FindProperty("groundCheckRadius");
            SerializedProperty pLayer = so.FindProperty("groundLayer");

            if (pMove != null) pMove.floatValue = MoveSpeed;
            if (pJump != null) pJump.floatValue = JumpForce;
            if (pRadius != null) pRadius.floatValue = 0.16f;
            if (pLayer != null) pLayer.intValue = 1 << groundLayer;

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
            new Vector3(-5.35f, FloorTop + 0.525f, 0f);
        crate.transform.localScale = Vector3.one;

        BoxCollider2D col = crate.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(1.05f, 1.05f);
            col.offset = Vector2.zero;

            PhysicsMaterial2D mat =
                AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(
                    "Assets/_Project/CrateGrip.physicsMaterial2D"
                );

            if (mat == null)
            {
                mat = new PhysicsMaterial2D("CrateGrip")
                {
                    friction = 0.75f,
                    bounciness = 0f
                };

                AssetDatabase.CreateAsset(
                    mat,
                    "Assets/_Project/CrateGrip.physicsMaterial2D"
                );
            }

            col.sharedMaterial = mat;
        }

        Rigidbody2D rb = crate.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.mass = 3.5f;
            rb.gravityScale = GravityScale;
            rb.linearDamping = 6.0f;
            rb.angularDamping = 0.05f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private static void RemovePlatforms()
    {
        string[] names =
        {
            "Platform_Main", "Platform_Left",
            "Platform_01", "Platform_02",
            "Platform_03", "Platform_04"
        };

        foreach (string n in names)
        {
            GameObject go = GameObject.Find(n);
            if (go != null)
                UnityEngine.Object.DestroyImmediate(go);
        }
    }

    private static void CreateOneWayPlatform(
        string name,
        Vector2 topCenter,
        float targetWidth,
        string spriteFile,
        int groundLayer)
    {
        Sprite sprite =
            AssetDatabase.LoadAssetAtPath<Sprite>(Props + spriteFile);

        if (sprite == null)
            throw new Exception("No se encontró " + spriteFile);

        GameObject go = new GameObject(name);
        go.layer = groundLayer;
        go.transform.position = new Vector3(topCenter.x, topCenter.y, 0f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(targetWidth * 0.91f, 0.16f);
        col.offset = new Vector2(0f, -0.08f);
        col.usedByEffector = true;

        PlatformEffector2D eff = go.AddComponent<PlatformEffector2D>();
        eff.useOneWay = true;
        eff.useSideFriction = false;
        eff.surfaceArc = 175f;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform, false);

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        // Escala UNIFORME: jamás se altera la relación de aspecto.
        float scale = targetWidth / sprite.bounds.size.x;
        visual.transform.localScale = Vector3.one * scale;

        float height = sprite.bounds.size.y * scale;

        // La parte superior del dibujo coincide con la superficie física.
        visual.transform.localPosition =
            new Vector3(0f, -height * 0.5f, 0f);
    }

    private static void SetupCore()
    {
        GameObject core = GameObject.Find("Kinetic_Core");
        if (core == null)
            throw new Exception("No se encontró Kinetic_Core.");

        // Sobre P4, pero no sobre la puerta.
        core.transform.position = new Vector3(4.00f, -0.78f, 0f);
        core.transform.localScale = Vector3.one;
    }

    private static void SetupGate()
    {
        GameObject gate = GameObject.Find("Exit_Gate");
        if (gate == null)
            throw new Exception("No se encontró Exit_Gate.");

        // Extremo derecho. Al caer desde P4 (x≈4) Kiro aterriza en piso;
        // luego debe caminar hasta la puerta (x≈7).
        gate.transform.position =
            new Vector3(6.95f, FloorTop + 0.95f, 0f);
        gate.transform.localScale = Vector3.one;

        BoxCollider2D col = gate.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(0.92f, 1.90f);
            col.offset = Vector2.zero;
            col.isTrigger = false;
        }
    }

    private static void ValidateRoute()
    {
        float g = Mathf.Abs(Physics2D.gravity.y) * GravityScale;
        float rise = JumpForce * JumpForce / (2f * g);

        // Superficies superiores.
        Vector2[] route =
        {
            new Vector2(-7.00f, FloorTop),
            new Vector2(-4.00f, -1.95f),
            new Vector2(-1.35f, -1.45f),
            new Vector2( 1.20f, -2.05f),
            new Vector2( 3.85f, -1.45f)
        };

        float[] widths =
        {
            0f, 1.55f, 2.30f, 1.55f, 2.30f
        };

        for (int i = 0; i < route.Length - 1; i++)
        {
            Vector2 a = route[i];
            Vector2 b = route[i + 1];

            float dy = b.y - a.y;

            if (dy > rise + 0.03f)
            {
                throw new Exception(
                    $"Salto {i}->{i + 1} imposible verticalmente: " +
                    $"dy={dy:F2}, rise={rise:F2}"
                );
            }

            // Tiempo de llegada a la altura b en la rama descendente.
            float discriminant =
                JumpForce * JumpForce - 2f * g * dy;

            if (discriminant < 0f)
                throw new Exception("Trayectoria sin solución.");

            float t =
                (JumpForce + Mathf.Sqrt(discriminant)) / g;

            float maxDx = MoveSpeed * t;

            if (i > 0)
            {
                float edgeGap =
                    Mathf.Abs(b.x - a.x) -
                    (widths[i] + widths[i + 1]) * 0.5f;

                edgeGap = Mathf.Max(0f, edgeGap);

                if (edgeGap > maxDx * 0.82f)
                {
                    throw new Exception(
                        $"Salto {i}->{i + 1} muy exigente: " +
                        $"gap={edgeGap:F2}, alcance={maxDx:F2}"
                    );
                }
            }
        }

        // Comprobación del techo con la plataforma más alta.
        const float playerHeight = 1.10f;
        const float safeDecorBoundary = 1.40f;
        const float highestPlatform = -1.45f;

        float maxHeadY =
            highestPlatform + rise + playerHeight;

        if (maxHeadY >= safeDecorBoundary)
        {
            throw new Exception(
                $"Kiro invade la zona superior: head={maxHeadY:F2}"
            );
        }

        // Core/puerta deben estar suficientemente separados para que una caída
        // desde P4 no termine el juego accidentalmente.
        float horizontalSeparation = 6.95f - 4.00f;
        if (horizontalSeparation < 2.0f)
        {
            throw new Exception("Core y puerta quedaron demasiado cerca.");
        }

        Debug.Log(
            $"[KAIROS-01] VALIDACIÓN V4 OK | " +
            $"salto vertical={rise:F2} u | " +
            $"core-puerta={horizontalSeparation:F2} u | " +
            $"cabeza máxima={maxHeadY:F2} u"
        );
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
}
