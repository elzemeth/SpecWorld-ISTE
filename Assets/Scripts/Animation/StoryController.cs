using System.Collections;
using UnityEngine;

public class StoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GuideManage guide;
    [SerializeField] private StoryStep[] steps;

    [Header("Options")]
    [SerializeField] private bool autoStartOnPlay = true;

    private int _stepIndex = -1;
    private bool _isRunning = false;
    private Coroutine _waitCoroutine;

    private void Start()
    {
        if (autoStartOnPlay)
        {
            StartStory();
        }
    }

    public void StartStory()
    {
        if (_isRunning)
        {
            Log("Story Already Working");
            return;
        }

        if (guide == null)
        {
            guide = FindFirstObjectByType<GuideManage>();
        }

        if (guide == null)
        {
            Debug.LogError("No GuideManage");
            return;
        }

        if (steps == null || steps.Length == 0)
        {
            Debug.LogWarning("Empty Story Steps");
            return;
        }

        _isRunning = true;
        _stepIndex = -1;

        guide.OnDestinationReached -= OnGuideDestinationReached;

        Log("Story begin");
        NextStep();
    }

    private void OnDestroy()
    {
        if (guide != null)
        {
            guide.OnDestinationReached -= OnGuideDestinationReached;
        }
    }

    private void NextStep()
    {
        if (_waitCoroutine != null)
        {
            StopCoroutine(_waitCoroutine);
            _waitCoroutine = null;
        }

        _stepIndex++;

        if (steps == null || _stepIndex >= steps.Length)
        {
            Log("Story Over");
            _isRunning = false;
            return;
        }

        var step = steps[_stepIndex];

        Log($"Step {_stepIndex} - {step.actionType} - {step.stepName}");

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
        }
    }

    private void HandleMoveTo(StoryStep step)
    {
        if (step.moveTarget == null)
        {
            Log("No MoveTarget");
            NextStep();
            return;
        }

        Log($"MoveTo -> {step.moveTarget.name}");

        guide.OnDestinationReached -= OnGuideDestinationReached;
        guide.OnDestinationReached += OnGuideDestinationReached;

        guide.GoToTarget(step.moveTarget.position);
    }

    private void OnGuideDestinationReached()
    {
        guide.OnDestinationReached -= OnGuideDestinationReached;

        Log("NextStep after loc");
        NextStep();
    }

    private void HandleTalk(StoryStep step)
    {
        if (step.voiceClip == null)
        {
            Log("Talk no Voice");
            NextStep();
            return;
        }

        Log($"Talk -> {step.voiceClip.name}");

        guide.Talk(step.voiceClip);

        float duration = step.voiceClip.length + step.talkExtraDelay;
        if (duration <= 0f) duration = 1f;

        _waitCoroutine = StartCoroutine(WaitAndNextStep(duration));
    }

    private void HandleWait(StoryStep step)
    {
        if (step.waitDuration <= 0f)
        {
            Log("Wait <= 0");
            NextStep();
            return;
        }

        Log($"Wait -> {step.waitDuration} sn");

        _waitCoroutine = StartCoroutine(WaitAndNextStep(step.waitDuration));
    }

    private void HandlePlayAnimation(StoryStep step)
    {
        Log($"PlayAnimation -> {step.animationId}");

        if (AnimationManager.Instance != null)
        {
            AnimationManager.Instance.ChangeState(step.animationId);
        }

        float duration = step.animationDuration;
        if (duration <= 0f) duration = 1f;

        _waitCoroutine = StartCoroutine(WaitAndNextStep(duration));
    }

    private IEnumerator WaitAndNextStep(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _waitCoroutine = null;
        NextStep();
    }

    private void Log(string msg)
    {
        Debug.Log($"[StoryController] {msg}");
    }
}
