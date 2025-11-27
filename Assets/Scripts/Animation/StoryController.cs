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

    private Coroutine _activeRoutine;

    private bool _waitingForMove = false;

    private void Awake()
    {
        if (guide == null)
            guide = FindFirstObjectByType<GuideManage>();

        if (animationManager == null)
            animationManager = FindFirstObjectByType<AnimationManager>();

        if (guide == null)
            Debug.LogWarning("No GuideManage");
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
        if (steps == null || steps.Length == 0)
        {
            Log("Story Steps Empty");
            return;
        }

        _currentStepIndex = 0;
        Log("Story begin");

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
            Log("Story Over");
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
            Log("Null Guide");
            NextStep();
            return;
        }

        if (step.moveTarget == null)
        {
            Log("Null MoveTo Target");
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
        Log("Finished. Next Step");
        NextStep();
    }

    private void HandleTalk(StoryStep step)
    {
        if (guide == null)
        {
            Log("No Guide");
            NextStep();
            return;
        }

        if (step.voiceClip == null)
        {
            Log("No Voice");
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
        Log("Talk is Over");
        NextStep();
    }

    private void HandleWait(StoryStep step)
    {
        float duration = Mathf.Max(0.01f, step.waitDuration);
        Log($"Wait -> {duration} sn");
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
            Log("No AnimationManager");
            NextStep();
            return;
        }

        Log($"PlayAnimation -> {step.animationId}");
        animationManager.ChangeState(step.animationId);

        if (step.animationDuration > 0f)
        {
            StartStepCoroutine(AnimationWaitRoutine(step.animationDuration));
        }
        else
        {
            NextStep();
        }
    }

    private IEnumerator AnimationWaitRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        NextStep();
    }

    private void HandleDoorOpen(StoryStep step)
    {
        if (step.door == null)
        {
            Log("Null Door Referance");
            NextStep();
            return;
        }

        Log($"DoorOpen -> {step.door.name}");

        if (step.unlockBeforeOpen)
        {
            step.door.Unlock();
            Log("unlockBeforeOpen");
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

        Log($"[DoorOpen] {door.name} is open.");
        NextStep();
    }

    private void OnDoorOpenFailedFromStory(DoorController door)
    {
        door.OnDoorOpened -= OnDoorOpenedFromStory;
        door.OnDoorOpenFailed -= OnDoorOpenFailedFromStory;

        Log($"[DoorOpen] {door.name} locked.");
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
