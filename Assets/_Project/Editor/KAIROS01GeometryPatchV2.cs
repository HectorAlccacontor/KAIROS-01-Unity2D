using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KAIROS01GeometryPatchV2
{
    private const string Marker = "ProjectSettings/KAIROS01-Geometry-v2.done";

    static KAIROS01GeometryPatchV2()
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
            Debug.Log("[KAIROS-01] Geometría v2 aplicada correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [MenuItem("Tools/KAIROS-01/Aplicar geometría v2")]
    public static void Apply()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0)
            throw new Exception("No existe la capa Ground.");

        MoveScale("Platform_Main",
            new Vector3(1.3f, -0.8f, 0f),
            new Vector3(4.0f, 0.4f, 1f));

        MoveScale("Platform_Left",
            new Vector3(5.2f, -2.15f, 0f),
            new Vector3(1.8f, 0.35f, 1f));

        GameObject crate = GameObject.Find("Calibration_Crate");
        if (crate != null)
        {
            crate.transform.position = new Vector3(-2.4f, -3.45f, 0f);
            crate.layer = groundLayer;

            var rb = crate.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.mass = 2.2f;
                rb.gravityScale = 4f;
                rb.linearDamping = 1.2f;
                rb.freezeRotation = true;
            }
        }

        GameObject core = GameObject.Find("Kinetic_Core");
        if (core != null)
            core.transform.position = new Vector3(1.6f, -0.2f, 0f);

        GameObject player = GameObject.Find("Kiro_Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                var so = new SerializedObject(controller);
                so.FindProperty("moveSpeed").floatValue = 7f;
                so.FindProperty("jumpForce").floatValue = 16f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        GameObject gate = GameObject.Find("Exit_Gate");
        if (gate != null)
        {
            gate.transform.position = new Vector3(7.45f, -3.05f, 0f);

            var collider = gate.GetComponent<Collider2D>();
            if (collider != null)
                collider.isTrigger = false;
        }

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log(
            "[KAIROS-01] Ruta validada: suelo -> caja -> plataforma del núcleo -> puerta."
        );
    }

    private static void MoveScale(string name, Vector3 position, Vector3 scale)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) return;

        go.transform.position = position;
        go.transform.localScale = scale;
    }
}
