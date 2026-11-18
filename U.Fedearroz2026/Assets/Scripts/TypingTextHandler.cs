using System.Collections;
using TMPro;
using UnityEngine;

public class TypingTextHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text target;
    [SerializeField, TextArea] private string text;
    [SerializeField] private float timeWrite = 0.05f;
    [SerializeField] private float delay = 0f;
    [SerializeField] private bool playOnEnable = true;

    private Coroutine routine;
    private bool playPending;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (playOnEnable || playPending)
            Play();
    }

    public void Play()
    {
        if (target == null)
            target = GetComponent<TMP_Text>();

        if (target == null)
            return;

        if (!isActiveAndEnabled)
        {
            playPending = true;
            return;
        }

        playPending = false;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(TypeRoutine());
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator TypeRoutine()
    {
        target.text = string.Empty;

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        var typing = new TypingAnimation(timeWrite);
        yield return typing.DoAnimateText(target, text);

        routine = null;
    }
}
