using System.Collections;
using UnityEngine;

public class StoryController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GuideManage guide;
    [SerializeField] private AnimationManager animationManager;

    [Header("Story")]
    [SerializeField] private bool autoStartOnStart = true;
    [SerializeField] private StoryStep[] steps;

    private int _currentStepIndex = -1;
    private bool _storyStarted = false;

    private bool _waitingForMove = false;
    private Coroutine _activeRoutine;

    private void Awake()
    {
        if (guide == null)
            guide = FindFirstObjectByType<GuideManage>();

        if (animationManager == null)
            animationManager = FindFirstObjectByType<AnimationManager>();

        if (guide == null)
            Debug.LogWarning("[StoryController] GuideManage not found.");

        if (animationManager == null)
            Debug.LogWarning("[StoryController] AnimationManager not found.");
    }

    private void OnEnable()
    {
        if (guide != null)
            guide.OnDestinationReached += OnDestinationReached;
    }

    private void OnDisable()
    {
        if (guide != null)
            guide.OnDestinationReached -= OnDestinationReached;
    }

    private void Start()
    {
        if (autoStartOnStart)
        {
            StartStory();
        }
    }

    public void StartStory()
    {
        if (_storyStarted)
            return;

        _storyStarted = true;

        if (steps == null || steps.Length == 0)
        {
            Log("No story steps found. Story cannot be started.");
            return;
        }

        _currentStepIndex = 0;
        Log("Story started.");

        RunCurrentStep();
    }

    public void NextStep()
    {
        _waitingForMove = false;

        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
            _activeRoutine = null;
        }

        _currentStepIndex++;

        if (_currentStepIndex >= steps.Length)
        {
            Log("Story ended.");
            return;
        }

        RunCurrentStep();
    }

    private void RunCurrentStep()
    {
        if (_currentStepIndex < 0 || _currentStepIndex >= steps.Length)
            return;

        var step = steps[_currentStepIndex];
        Log($"Step {_currentStepIndex} - {step.stepName} - {step.actionType}");

        switch (step.actionType)
        {
            case StoryActionType.MoveTo:
                HandleMoveTo(step);
                break;

            case StoryActionType.Talk:
                HandleTalk(step);
                break;

            case StoryActionType.Wait:
                HandleWait(step);
                break;

            case StoryActionType.PlayAnimation:
                HandlePlayAnimation(step);
                break;

            case StoryActionType.DoorOpen:
                HandleDoorOpen(step);
                break;
        }
    }
    private void HandleMoveTo(StoryStep step)
    {
        if (guide == null)
        {
            Log("Guide not found, skipping MoveTo step.");
            NextStep();
            return;
        }

        if (step.moveTarget == null)
        {
            Log("MoveTo target missing, skipping step.");
            NextStep();
            return;
        }

        Log($"MoveTo -> {step.moveTarget.name}");
        _waitingForMove = true;

        guide.GoToTarget(step.moveTarget.position);
    }

    private void OnDestinationReached()
    {
        if (!_waitingForMove)
            return;

        _waitingForMove = false;
        Log("Destination reached, NextStep.");
        NextStep();
    }

    private void HandleTalk(StoryStep step)
    {
        if (guide == null)
        {
            Log("Guide not found, skipping Talk step.");
            NextStep();
            return;
        }

        if (step.voiceClip == null)
        {
            Log("Talk step has no voiceClip, skipping.");
            NextStep();
            return;
        }

        Log($"Talk -> {step.voiceClip.name}");

        guide.Talk(step.voiceClip);

        float waitTime = step.voiceClip.length + step.talkExtraWait;
        if (waitTime <= 0f)
            waitTime = 0.1f;

        StartStepCoroutine(TalkRoutine(waitTime));
    }

    private IEnumerator TalkRoutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Log("Talk finished, NextStep.");
        NextStep();
    }
    private void HandleWait(StoryStep step)
    {
        float duration = Mathf.Max(0.01f, step.waitDuration);
        Log($"Wait -> {duration} seconds");
        StartStepCoroutine(WaitRoutine(duration));
    }

    private IEnumerator WaitRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        NextStep();
    }

    private void HandlePlayAnimation(StoryStep step)
    {
        if (animationManager == null)
        {
            Log("AnimationManager not found, skipping PlayAnimation step.");
            NextStep();
            return;
        }

        Log($"PlayAnimation -> {step.animationId}");

        animationManager.ChangeState(step.animationId);

        Animator anim = animationManager.GetComponent<Animator>();
        if (anim == null)
        {
            Log("Animator not found, skipping to next step.");
            NextStep();
            return;
        }

        StartStepCoroutine(WaitForAnimationEndRoutine(anim));
    }

    private IEnumerator WaitForAnimationEndRoutine(Animator anim)
    {
        int layer = 0;

        yield return null;

        while (anim.IsInTransition(layer))
            yield return null;

        var state = anim.GetCurrentAnimatorStateInfo(layer);

        if (state.length <= 0f)
        {
            Log("State length is 0, skipping.");
            NextStep();
            yield break;
        }

        float clipLength = state.length;
        Log($"Animation auto length detected: {clipLength} seconds");

        while (true)
        {
            state = anim.GetCurrentAnimatorStateInfo(layer);

            if (state.normalizedTime >= 1f && !anim.IsInTransition(layer))
                break;

            yield return null;
        }

        Log("Animation finished, NextStep.");
        NextStep();
    }

    private void HandleDoorOpen(StoryStep step)
    {
        if (step.door == null)
        {
            Log("DoorOpen step has no door reference, skipping.");
            NextStep();
            return;
        }

        Log($"DoorOpen -> {step.door.name}");

        if (step.unlockBeforeOpen)
        {
            step.door.Unlock();
            Log("Door was unlocked before opening.");
        }

        if (step.door.IsOpen)
        {
            Log($"Door {step.door.name} is already open, NextStep.");
            NextStep();
            return;
        }

        step.door.OnDoorOpened -= OnDoorOpenedFromStory;
        step.door.OnDoorOpenFailed -= OnDoorOpenFailedFromStory;

        step.door.OnDoorOpened += OnDoorOpenedFromStory;
        step.door.OnDoorOpenFailed += OnDoorOpenFailedFromStory;

        step.door.TryOpen();
    }

    private void OnDoorOpenedFromStory(DoorController door)
    {
        door.OnDoorOpened -= OnDoorOpenedFromStory;
        door.OnDoorOpenFailed -= OnDoorOpenFailedFromStory;

        Log($"[DoorOpen] {door.name} opened, NextStep.");
        NextStep();
    }

    private void OnDoorOpenFailedFromStory(DoorController door)
    {
        door.OnDoorOpened -= OnDoorOpenedFromStory;
        door.OnDoorOpenFailed -= OnDoorOpenFailedFromStory;

        Log($"[DoorOpen] {door.name} failed to open (locked?), NextStep anyway.");
        NextStep();
    }

    private void StartStepCoroutine(IEnumerator routine)
    {
        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);

        _activeRoutine = StartCoroutine(routine);
    }

    private void Log(string msg)
    {
        Debug.Log($"[StoryController] {msg}");
    }
}
