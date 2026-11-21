using UnityEngine;

/// <summary>
/// Controla las pantallas de detalle que se abren desde el menú de Target 2 (MockUp2.0).
/// Al activarse el panel siempre vuelve al menú.
/// </summary>
public class Target2DetailController : MonoBehaviour
{
    [SerializeField] private GameObject menuScreen;

    [Header("Detalles")]
    [SerializeField] private GameObject detailAltaTemperatura;
    [SerializeField] private GameObject detailBeneficio;

    private void OnEnable()
    {
        MostrarMenu();
    }

    /// <summary>Abre el detalle de "Alta temperatura" (MockUp2.2).</summary>
    public void AbrirDetalle()
    {
        Abrir(detailAltaTemperatura);
    }

    /// <summary>Abre la pantalla de beneficios (MockUp2.1).</summary>
    public void AbrirBeneficio()
    {
        Abrir(detailBeneficio);
    }

    public void CerrarDetalle()
    {
        MostrarMenu();
    }

    private void Abrir(GameObject detail)
    {
        if (menuScreen != null)
            menuScreen.SetActive(false);

        if (detailAltaTemperatura != null)
            detailAltaTemperatura.SetActive(detail == detailAltaTemperatura);

        if (detailBeneficio != null)
            detailBeneficio.SetActive(detail == detailBeneficio);

        if (detail != null)
            detail.SetActive(true);
    }

    private void MostrarMenu()
    {
        if (menuScreen != null)
            menuScreen.SetActive(true);

        if (detailAltaTemperatura != null)
            detailAltaTemperatura.SetActive(false);

        if (detailBeneficio != null)
            detailBeneficio.SetActive(false);
    }
}
