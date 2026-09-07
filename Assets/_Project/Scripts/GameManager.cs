using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool HasCore { get; private set; }
    public bool Won { get; private set; }

    private GUIStyle titleStyle;
    private GUIStyle labelStyle;
    private GUIStyle objectiveStyle;
    private GUIStyle statusStyle;
    private GUIStyle completeTitleStyle;
    private GUIStyle completeBodyStyle;
    private GUIStyle restartStyle;

    private Texture2D pixel;

    private static readonly Color PanelColor =
        new Color(0.018f, 0.045f, 0.075f, 0.86f);

    private static readonly Color PanelBorder =
        new Color(0.18f, 0.72f, 0.88f, 0.88f);

    private static readonly Color Cyan =
        new Color(0.34f, 0.90f, 1.00f, 1f);

    private static readonly Color Gold =
        new Color(1.00f, 0.82f, 0.28f, 1f);

    private void Awake()
    {
        Instance = this;
        pixel = Texture2D.whiteTexture;
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

    private void EnsureStyles(float scale)
    {
        int titleSize = Mathf.RoundToInt(18f * scale);
        int labelSize = Mathf.RoundToInt(12f * scale);
        int objectiveSize = Mathf.RoundToInt(11f * scale);
        int statusSize = Mathf.RoundToInt(12f * scale);
        int completeTitleSize = Mathf.RoundToInt(28f * scale);
        int completeBodySize = Mathf.RoundToInt(15f * scale);
        int restartSize = Mathf.RoundToInt(13f * scale);

        if (titleStyle != null && titleStyle.fontSize == titleSize)
            return;

        titleStyle = CreateStyle(
            titleSize,
            FontStyle.Bold,
            Cyan,
            TextAnchor.MiddleLeft
        );

        labelStyle = CreateStyle(
            labelSize,
            FontStyle.Bold,
            Color.white,
            TextAnchor.MiddleLeft
        );

        objectiveStyle = CreateStyle(
            objectiveSize,
            FontStyle.Normal,
            new Color(0.88f, 0.94f, 0.98f, 1f),
            TextAnchor.MiddleLeft
        );

        statusStyle = CreateStyle(
            statusSize,
            FontStyle.Bold,
            Gold,
            TextAnchor.MiddleLeft
        );

        completeTitleStyle = CreateStyle(
            completeTitleSize,
            FontStyle.Bold,
            Color.white,
            TextAnchor.MiddleCenter
        );

        completeBodyStyle = CreateStyle(
            completeBodySize,
            FontStyle.Normal,
            new Color(0.90f, 0.96f, 1f, 1f),
            TextAnchor.MiddleCenter
        );

        restartStyle = CreateStyle(
            restartSize,
            FontStyle.Bold,
            Cyan,
            TextAnchor.MiddleCenter
        );
    }

    private GUIStyle CreateStyle(
        int fontSize,
        FontStyle fontStyle,
        Color color,
        TextAnchor alignment)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Max(8, fontSize),
            fontStyle = fontStyle,
            alignment = alignment,
            clipping = TextClipping.Clip,
            wordWrap = false
        };

        style.normal.textColor = color;
        return style;
    }

    private void OnGUI()
    {
        float scale = Mathf.Min(
            Screen.width / 1280f,
            Screen.height / 720f
        );

        scale = Mathf.Max(scale, 0.65f);
        EnsureStyles(scale);

        DrawHud(scale);

        if (Won)
            DrawCompletion(scale);
    }

    private void DrawHud(float scale)
    {
        Rect panel = SRect(18f, 16f, 650f, 104f, scale);

        DrawPanel(
            panel,
            PanelColor,
            PanelBorder,
            1.5f * scale
        );

        GUI.DrawTexture(
            new Rect(
                panel.x,
                panel.y,
                Mathf.Max(4f, 5f * scale),
                panel.height
            ),
            pixel,
            ScaleMode.StretchToFill,
            true,
            0f,
            Cyan,
            0f,
            0f
        );

        GUI.Label(
            SRect(34f, 22f, 550f, 24f, scale),
            "KAIROS-01  //  LABORATORIO CINÉTICO",
            titleStyle
        );

        GUI.Label(
            SRect(34f, 49f, 520f, 20f, scale),
            "Mover  A/D o ←/→    ·    Saltar  ESPACIO",
            labelStyle
        );

        string objective = HasCore
            ? "Objetivo: núcleo recuperado. Dirígete a la compuerta de salida."
            : "Objetivo: usa la caja como apoyo y recupera el núcleo cinético.";

        GUI.Label(
            SRect(34f, 72f, 595f, 18f, scale),
            objective,
            objectiveStyle
        );

        string status = HasCore
            ? "NÚCLEO  //  RECUPERADO"
            : "NÚCLEO  //  PENDIENTE";

        statusStyle.normal.textColor =
            HasCore ? new Color(0.42f, 1.00f, 0.66f, 1f) : Gold;

        GUI.Label(
            SRect(34f, 94f, 360f, 18f, scale),
            status,
            statusStyle
        );
    }

    private void DrawCompletion(float scale)
    {
        GUI.DrawTexture(
            new Rect(0f, 0f, Screen.width, Screen.height),
            pixel,
            ScaleMode.StretchToFill,
            true,
            0f,
            new Color(0f, 0f, 0f, 0.48f),
            0f,
            0f
        );

        float width = 500f * scale;
        float height = 194f * scale;

        Rect panel = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );

        DrawPanel(
            panel,
            new Color(0.018f, 0.045f, 0.072f, 0.97f),
            PanelBorder,
            2f * scale
        );

        GUI.DrawTexture(
            new Rect(
                panel.x,
                panel.y,
                panel.width,
                Mathf.Max(5f, 6f * scale)
            ),
            pixel,
            ScaleMode.StretchToFill,
            true,
            0f,
            Cyan,
            0f,
            0f
        );

        GUI.Label(
            new Rect(
                panel.x + 28f * scale,
                panel.y + 30f * scale,
                panel.width - 56f * scale,
                36f * scale
            ),
            "PRUEBA COMPLETADA",
            completeTitleStyle
        );

        GUI.Label(
            new Rect(
                panel.x + 34f * scale,
                panel.y + 78f * scale,
                panel.width - 68f * scale,
                42f * scale
            ),
            "Kiro ha estabilizado el laboratorio y desbloqueado la salida.",
            completeBodyStyle
        );

        DrawThinLine(
            new Rect(
                panel.x + 68f * scale,
                panel.y + 132f * scale,
                panel.width - 136f * scale,
                Mathf.Max(1f, scale)
            ),
            new Color(0.20f, 0.55f, 0.68f, 0.65f)
        );

        GUI.Label(
            new Rect(
                panel.x + 34f * scale,
                panel.y + 145f * scale,
                panel.width - 68f * scale,
                24f * scale
            ),
            "R  ·  REINICIAR PRUEBA",
            restartStyle
        );
    }

    private void DrawPanel(
        Rect rect,
        Color fill,
        Color border,
        float borderWidth)
    {
        GUI.DrawTexture(
            rect,
            pixel,
            ScaleMode.StretchToFill,
            true,
            0f,
            fill,
            0f,
            0f
        );

        DrawThinLine(
            new Rect(rect.x, rect.y, rect.width, borderWidth),
            border
        );

        DrawThinLine(
            new Rect(
                rect.x,
                rect.yMax - borderWidth,
                rect.width,
                borderWidth
            ),
            border
        );

        DrawThinLine(
            new Rect(rect.x, rect.y, borderWidth, rect.height),
            border
        );

        DrawThinLine(
            new Rect(
                rect.xMax - borderWidth,
                rect.y,
                borderWidth,
                rect.height
            ),
            border
        );
    }

    private void DrawThinLine(Rect rect, Color color)
    {
        GUI.DrawTexture(
            rect,
            pixel,
            ScaleMode.StretchToFill,
            true,
            0f,
            color,
            0f,
            0f
        );
    }

    private Rect SRect(
        float x,
        float y,
        float width,
        float height,
        float scale)
    {
        return new Rect(
            x * scale,
            y * scale,
            width * scale,
            height * scale
        );
    }
}
