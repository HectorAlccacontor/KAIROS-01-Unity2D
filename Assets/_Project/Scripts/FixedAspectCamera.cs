using UnityEngine;

/// <summary>
/// Mantiene una ventana lógica 16:9. Si la pestaña Game o la ventana
/// ejecutable tienen otra proporción, agrega letterbox/pillarbox en lugar
/// de recortar el escenario.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16f / 9f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        Apply();
    }

    private void OnPreCull()
    {
        Apply();
    }

    private void Apply()
    {
        if (cam == null || Screen.height <= 0) return;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            cam.rect = new Rect(
                0f,
                (1f - scaleHeight) * 0.5f,
                1f,
                scaleHeight
            );
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            cam.rect = new Rect(
                (1f - scaleWidth) * 0.5f,
                0f,
                scaleWidth,
                1f
            );
        }
    }
}
