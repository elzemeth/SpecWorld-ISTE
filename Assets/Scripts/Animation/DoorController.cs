using System;
using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Rotation (local Y)")]
    [Tooltip("Kapalý durumdaki local Y açýsý")]
    [SerializeField] private float closedAngleY = 180f;

    [Tooltip("Açýk durumdaki local Y açýsý")]
    [SerializeField] private float openAngleY = 60f;

    [Tooltip("Kapýnýn tamamen açýlma süresi (saniye)")]
    [SerializeField] private float openDuration = 1.0f;

    [Tooltip("0-1 arasý easing için kullanýlacak eðri")]
    [SerializeField]
    private AnimationCurve openCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("State")]
    [SerializeField] private bool startLocked = false;
    [SerializeField] private bool startOpen = false;

    private bool isLocked;
    private bool isOpen;
    private bool isAnimating;
    private Coroutine openRoutine;

    public bool IsLocked => isLocked;
    public bool IsOpen => isOpen;

    /// <summary>Kapý baþarýyla açýldýðýnda tetiklenir.</summary>
    public event Action<DoorController> OnDoorOpened;

    /// <summary>Kapý kilitli olduðu için açýlamadýðýnda tetiklenir.</summary>
    public event Action<DoorController> OnDoorOpenFailed;

    private void Awake()
    {
        isLocked = startLocked;
        isOpen = startOpen;

        // Baþlangýç açýsýný ayarla
        float y = isOpen ? openAngleY : closedAngleY;
        SetLocalYRotation(y);
    }

    // ========== DIÞARIDAN KONTROL ==========

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    /// <summary>
    /// Kapýyý açmayý dener. Kilitliyse açýlmaz.
    /// StoryController burayý çaðýracak.
    /// </summary>
    public void TryOpen()
    {
        if (isOpen)
        {
            Debug.Log("[DoorController] Kapý zaten açýk, TryOpen yoksayýlýyor.");
            return;
        }

        if (isLocked)
        {
            Debug.Log("[DoorController] Kapý kilitli, açýlamadý.");
            OnDoorOpenFailed?.Invoke(this);
            return;
        }

        if (isAnimating)
        {
            Debug.Log("[DoorController] Kapý zaten açýlma animasyonu oynatýyor.");
            return;
        }

        Debug.Log("[DoorController] Kapý açýlmaya baþlýyor (kod ile rotate).");

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(OpenDoorRoutine());
    }

    // ========== ROTATION ANÝMASYONU ==========

    private IEnumerator OpenDoorRoutine()
    {
        isAnimating = true;

        float startY = transform.localEulerAngles.y;
        float targetY = openAngleY;

        float time = 0f;
        float duration = Mathf.Max(0.01f, openDuration);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            // Eðri uygula (ease in/out vs)
            float curvedT = openCurve != null ? openCurve.Evaluate(t) : t;

            float y = Mathf.LerpAngle(startY, targetY, curvedT);
            SetLocalYRotation(y);

            yield return null;
        }

        // Garantile
        SetLocalYRotation(targetY);
        isOpen = true;
        isAnimating = false;
        openRoutine = null;

        Debug.Log("[DoorController] Kapý açýlma animasyonu bitti, OnDoorOpened tetikleniyor.");
        OnDoorOpened?.Invoke(this);
    }

    // ========== HELPER ==========

    private void SetLocalYRotation(float y)
    {
        var euler = transform.localEulerAngles;
        euler.y = y;
        transform.localEulerAngles = euler;
    }
}
