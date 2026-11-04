using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroIKVariantHandler : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private List<GameObject> targets = new List<GameObject>();
    [SerializeField] private int currentIndex;

    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Position")]
    [SerializeField] private Slider positionX;
    [SerializeField] private Slider positionY;
    [SerializeField] private Slider positionZ;

    [Header("Scale")]
    [SerializeField] private Slider scale;

    [Header("Rotation Y")]
    [SerializeField] private Slider rotationY;

    [Header("Value Texts")]
    [SerializeField] private TMP_Text positionXText;
    [SerializeField] private TMP_Text positionYText;
    [SerializeField] private TMP_Text positionZText;
    [SerializeField] private TMP_Text scaleText;
    [SerializeField] private TMP_Text rotationYText;

    public int CurrentIndex => currentIndex;

    public GameObject CurrentTarget =>
        (currentIndex >= 0 && currentIndex < targets.Count) ? targets[currentIndex] : null;

    private Transform TargetTransform
    {
        get
        {
            GameObject current = CurrentTarget;
            return current != null ? current.transform : null;
        }
    }

    private void Awake()
    {
        positionX.onValueChanged.AddListener(OnSliderChanged);
        positionY.onValueChanged.AddListener(OnSliderChanged);
        positionZ.onValueChanged.AddListener(OnSliderChanged);
        scale.onValueChanged.AddListener(OnSliderChanged);
        rotationY.onValueChanged.AddListener(OnSliderChanged);

        if (previousButton != null)
            previousButton.onClick.AddListener(Previous);
        if (nextButton != null)
            nextButton.onClick.AddListener(Next);
    }

    private void Start()
    {
        ActivateCurrentTarget();
        LoadFromTarget();
    }

    public void Select(int index)
    {
        if (targets.Count == 0)
            return;

        currentIndex = Mathf.Clamp(index, 0, targets.Count - 1);
        ActivateCurrentTarget();
        LoadFromTarget();
    }

    public void Next()
    {
        Select(currentIndex + 1);
    }

    public void Previous()
    {
        Select(currentIndex - 1);
    }

    private void ActivateCurrentTarget()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
                targets[i].SetActive(i == currentIndex);
        }
    }

    private void OnSliderChanged(float value)
    {
        ApplyToTarget();
        RefreshTexts();
    }

    private void LoadFromTarget()
    {
        Transform target = TargetTransform;
        if (target == null)
            return;

        positionX.value = target.localPosition.x;
        positionY.value = target.localPosition.y;
        positionZ.value = target.localPosition.z;
        scale.value = target.localScale.x;
        rotationY.value = Mathf.DeltaAngle(0f, target.localEulerAngles.y);
        RefreshTexts();
    }

    private void ApplyToTarget()
    {
        Transform target = TargetTransform;
        if (target == null)
            return;

        target.localPosition = new Vector3(positionX.value, positionY.value, positionZ.value);
        target.localScale = Vector3.one * scale.value;

        Vector3 euler = target.localEulerAngles;
        target.localEulerAngles = new Vector3(euler.x, rotationY.value, euler.z);
    }

    private void RefreshTexts()
    {
        positionXText.text = positionX.value.ToString("F2");
        positionYText.text = positionY.value.ToString("F2");
        positionZText.text = positionZ.value.ToString("F2");
        scaleText.text = scale.value.ToString("F2");
        rotationYText.text = rotationY.value.ToString("F2");
    }
}
