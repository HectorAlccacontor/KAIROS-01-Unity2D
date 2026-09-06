using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KAIROS01PhysicsPolishV5
{
    private const string Marker =
        "ProjectSettings/KAIROS01-PhysicsPolish-v5.done";

    static KAIROS01PhysicsPolishV5()
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
            Debug.Log("[KAIROS-01] Física v5 aplicada correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [MenuItem("Tools/KAIROS-01/Aplicar física final v5")]
    public static void Apply()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");

        if (groundLayer < 0)
            throw new Exception("No existe la capa Ground.");

        GameObject player = GameObject.Find("Kiro_Player");

        if (player == null)
            throw new Exception("No se encontró Kiro_Player.");

        // --------------------------------------------------------------
        // 1. HITBOX DEL JUGADOR
        // --------------------------------------------------------------
        // El sprite completo NO define el hitbox. Brazos, antena y piernas
        // extendidas pueden sobresalir visualmente sin engancharse.
        foreach (BoxCollider2D oldBox in player.GetComponents<BoxCollider2D>())
            UnityEngine.Object.DestroyImmediate(oldBox);

        CapsuleCollider2D capsule =
            player.GetComponent<CapsuleCollider2D>();

        if (capsule == null)
            capsule = player.AddComponent<CapsuleCollider2D>();

        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.size = new Vector2(0.44f, 0.96f);
        capsule.offset = new Vector2(0f, -0.04f);
        capsule.isTrigger = false;

        PhysicsMaterial2D playerMat =
            AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(
                "Assets/_Project/PlayerNoFriction.physicsMaterial2D"
            );

        if (playerMat == null)
        {
            playerMat = new PhysicsMaterial2D("PlayerNoFriction")
            {
                friction = 0f,
                bounciness = 0f
            };

            AssetDatabase.CreateAsset(
                playerMat,
                "Assets/_Project/PlayerNoFriction.physicsMaterial2D"
            );
        }

        capsule.sharedMaterial = playerMat;

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

        // --------------------------------------------------------------
        // 2. SENSOR DE SUELO
        // --------------------------------------------------------------
        Transform groundCheck = player.transform.Find("GroundCheck");

        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(player.transform, false);
            groundCheck = gc.transform;
        }

        // El borde inferior de la cápsula está en Y=-0.55.
        // El sensor nace 0.05 u por encima y proyecta 0.12 u hacia abajo.
        groundCheck.localPosition = new Vector3(0f, -0.50f, 0f);

        PlayerController pc = player.GetComponent<PlayerController>();

        if (pc == null)
            throw new Exception("Kiro_Player no tiene PlayerController.");

        SerializedObject pcSO = new SerializedObject(pc);

        SetFloat(pcSO, "moveSpeed", 4.4f);
        SetFloat(pcSO, "jumpForce", 11.5f);
        SetObject(pcSO, "groundCheck", groundCheck);
        SetFloat(pcSO, "groundCheckDistance", 0.08f);

        SerializedProperty layerProp =
            pcSO.FindProperty("groundLayer");

        if (layerProp != null)
            layerProp.intValue = 1 << groundLayer;

        pcSO.ApplyModifiedPropertiesWithoutUndo();

        // --------------------------------------------------------------
        // 3. PLATAFORMAS SEMISÓLIDAS
        // --------------------------------------------------------------
        for (int i = 1; i <= 4; i++)
        {
            GameObject platform =
                GameObject.Find($"Platform_0{i}");

            if (platform == null)
                continue;

            platform.layer = groundLayer;

            BoxCollider2D col =
                platform.GetComponent<BoxCollider2D>();

            PlatformEffector2D eff =
                platform.GetComponent<PlatformEffector2D>();

            Transform visualT =
                platform.transform.Find("Visual");

            SpriteRenderer visual =
                visualT != null
                    ? visualT.GetComponent<SpriteRenderer>()
                    : null;

            if (col == null || eff == null || visual == null || visual.sprite == null)
                throw new Exception(
                    $"Platform_0{i} no tiene configuración completa."
                );

            float visualWidth =
                visual.sprite.bounds.size.x *
                Mathf.Abs(visualT.localScale.x);

            // El collider representa solo la superficie caminable del sprite,
            // no los soportes metálicos decorativos de los extremos.
            col.size = new Vector2(
                visualWidth * 0.84f,
                0.14f
            );

            col.offset = new Vector2(0f, -0.07f);
            col.usedByEffector = true;

            eff.useOneWay = true;
            eff.useSideBounce = false;
            eff.useSideFriction = false;
            eff.surfaceArc = 160f;
        }

        // --------------------------------------------------------------
        // 4. AUDITORÍA
        // --------------------------------------------------------------
        SpriteRenderer kiroVisual =
            player.transform.Find("Visual")?.GetComponent<SpriteRenderer>();

        if (kiroVisual != null)
        {
            float visualW = kiroVisual.bounds.size.x;
            float visualH = kiroVisual.bounds.size.y;

            Debug.Log(
                $"[KAIROS-01] HITBOX AUDIT | " +
                $"visual={visualW:F2}x{visualH:F2} u | " +
                $"capsule=0.44x0.96 u | " +
                $"ratioWidth={(0.44f / visualW):P0}"
            );
        }

        // Debe quedar suficientemente estrecho para no engancharse.
        if (capsule.size.x >= 0.60f)
            throw new Exception("Hitbox del jugador sigue demasiado ancho.");

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = player;

        Debug.Log(
            "[KAIROS-01] VALIDACIÓN V5 OK | " +
            "capsule estrecha + 3 rayos de suelo + plataformas one-way sin lados."
        );
    }

    private static void SetFloat(
        SerializedObject so,
        string name,
        float value)
    {
        SerializedProperty p = so.FindProperty(name);
        if (p != null) p.floatValue = value;
    }

    private static void SetObject(
        SerializedObject so,
        string name,
        UnityEngine.Object value)
    {
        SerializedProperty p = so.FindProperty(name);
        if (p != null) p.objectReferenceValue = value;
    }
}

