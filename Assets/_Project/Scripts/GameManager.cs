using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Estado mínimo del prototipo: núcleo recogido y finalización de la prueba.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool HasCore { get; private set; }
    public bool Won { get; private set; }

    private GUIStyle hudStyle;
    private GUIStyle titleStyle;
    private GUIStyle objectiveStyle;
    private GUIStyle centerStyle;

    private void Awake()
    {
        Instance = this;
    }

    public void CollectCore()
    {
        HasCore = true;
    }

    public void CompleteTest()
    {
        if (HasCore)
            Won = true;
    }

    private void Update()
    {
        if (Won && Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void InitStyles()
    {
        if (hudStyle != null) return;

        hudStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 19,
            fontStyle = FontStyle.Bold
        };
        hudStyle.normal.textColor = Color.white;

        titleStyle = new GUIStyle(hudStyle)
        {
            fontSize = 24
        };
        titleStyle.normal.textColor = new Color(0.45f, 0.95f, 1f);

        objectiveStyle = new GUIStyle(hudStyle)
        {
            fontSize = 17
        };
        objectiveStyle.normal.textColor = new Color(0.80f, 0.90f, 0.95f);

        centerStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        centerStyle.normal.textColor = Color.white;
    }

    private void OnGUI()
    {
        InitStyles();

        GUI.Label(new Rect(20, 15, 520, 35),
            "KAIROS-01 // LABORATORIO CINÉTICO", titleStyle);

        GUI.Label(new Rect(20, 48, 600, 28),
            "Mover: A/D o ←/→   ·   Saltar: Espacio", hudStyle);

        string objective = HasCore
            ? "OBJETIVO: Núcleo recuperado. Cruza la puerta verde."
            : "OBJETIVO: Empuja la caja, úsala como apoyo y recupera el núcleo.";

        GUI.Label(new Rect(20, 78, 760, 28), objective, objectiveStyle);

        GUI.Label(new Rect(20, 106, 480, 28),
            HasCore ? "Núcleo cinético: RECUPERADO" : "Núcleo cinético: PENDIENTE",
            hudStyle);

        if (Won)
        {
            GUI.Box(
                new Rect(Screen.width / 2f - 230, Screen.height / 2f - 85, 460, 170),
                "PRUEBA COMPLETADA\nKiro ha estabilizado el laboratorio.\n\nPulsa R para reiniciar",
                centerStyle
            );
        }
    }
}
