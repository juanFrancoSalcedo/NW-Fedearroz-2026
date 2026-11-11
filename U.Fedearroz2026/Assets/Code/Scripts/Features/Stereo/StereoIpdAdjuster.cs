using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StereoIpdAdjuster : MonoBehaviour
{
    [SerializeField] private StereoRig rig;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private TMP_Text valueLabel;

    private void Awake()
    {
        if (increaseButton != null) increaseButton.onClick.AddListener(Increase);
        if (decreaseButton != null) decreaseButton.onClick.AddListener(Decrease);
    }

    private void Start() => Refresh();

    private void OnEnable() => Refresh();

    private void Increase()
    {
        if (rig != null) rig.IncreaseSeparation();
        Refresh();
    }

    private void Decrease()
    {
        if (rig != null) rig.DecreaseSeparation();
        Refresh();
    }

    private void Refresh()
    {
        if (valueLabel != null && rig != null)
            valueLabel.text = (rig.Ipd * 1000f).ToString("F0") + " mm";
    }
}
