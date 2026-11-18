using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reinicia el alpha de la Image al desactivarse, para que las animaciones de fade
/// de Anima DG (AnimationUIController) se vuelvan a reproducir cada vez que el
/// objeto se activa de nuevo.
/// </summary>
[RequireComponent(typeof(Image))]
public class ResetImageFadeOnDisable : MonoBehaviour
{
    [SerializeField] private float alphaOnDisable = 0f;

    private void OnDisable()
    {
        Image image = GetComponent<Image>();

        if (image == null)
            return;

        Color color = image.color;
        color.a = alphaOnDisable;
        image.color = color;
    }
}
