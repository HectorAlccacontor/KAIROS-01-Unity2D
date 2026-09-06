using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KAIROS01CoreFix
{
    private const string Marker = "ProjectSettings/KAIROS01-CoreFix.done";

    static KAIROS01CoreFix()
    {
        EditorApplication.delayCall += ApplyOnce;
    }

    private static void ApplyOnce()
    {
        if (File.Exists(Marker))
            return;

        GameObject core = GameObject.Find("Kinetic_Core");
        if (core == null)
        {
            Debug.LogError("[KAIROS-01] No se encontró Kinetic_Core.");
            return;
        }

        Collider2D col = core.GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        Rigidbody2D rb = core.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = core.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.simulated = true;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Directory.CreateDirectory("ProjectSettings");
        File.WriteAllText(Marker, DateTime.Now.ToString("O"));

        Debug.Log("[KAIROS-01] Núcleo corregido: Trigger + Rigidbody2D Kinematic + detección por PlayerController.");
    }
}
