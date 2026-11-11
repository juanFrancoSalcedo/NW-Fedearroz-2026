using UnityEngine;

public class StereoRig : MonoBehaviour
{
    [Header("Ojos")]
    [SerializeField] private Camera camLeft;
    [SerializeField] private Camera camRight;

    [Header("Parametros")]
    [SerializeField] private float ipd = 0.064f;
    [SerializeField] private bool applyViewportRects = true;

    [Header("Ajuste de separacion")]
    [SerializeField] private float step = 0.002f;
    [SerializeField] private float minIpd = 0.045f;
    [SerializeField] private float maxIpd = 0.085f;
    [SerializeField] private bool persistSeparation = true;

    private const string PrefKey = "stereo_ipd";

    public float Ipd => ipd;
    public float MinIpd => minIpd;
    public float MaxIpd => maxIpd;

    private void Awake()
    {
        if (persistSeparation && PlayerPrefs.HasKey(PrefKey))
            ipd = Mathf.Clamp(PlayerPrefs.GetFloat(PrefKey, ipd), minIpd, maxIpd);
        Configure();
    }

    
    public void SetIpd(float value)
    {
        ipd = Mathf.Clamp(value, minIpd, maxIpd);
        Configure();

        if (persistSeparation)
        {
            PlayerPrefs.SetFloat(PrefKey, ipd);
            PlayerPrefs.Save();
        }
    }
    [ContextMenu("Increase")]
    public void IncreaseSeparation() => SetIpd(ipd + step);
    [ContextMenu("Decrease")]
    public void DecreaseSeparation() => SetIpd(ipd - step);

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        Configure();
    }
#endif
    public void Configure()
    {
        if (camLeft == null || camRight == null)
            return;

        camLeft.transform.localPosition = new Vector3(-ipd * 0.5f, 0f, 0f);
        camRight.transform.localPosition = new Vector3(ipd * 0.5f, 0f, 0f);

        if (applyViewportRects)
        {
            camLeft.rect = new Rect(0f, 0f, 0.5f, 1f);
            camRight.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
    }

}
