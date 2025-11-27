using UnityEngine;

public class StoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GuideManage guide;
    [SerializeField] private StoryStep[] steps;

    [Header("Options")]
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private bool debugLogs = true;

    private int _currentIndex = -1;
    private bool _isRunning;
    private float _stepTimer;

    private void Start()
    {
        if (guide == null)
            guide = FindFirstObjectByType<GuideManage>();

        if (guide == null)
        {
            Debug.LogError("GuideManage not found");
            return;
        }

        guide.OnDestinationReached -= OnGuideDestinationReached;
        guide.OnDestinationReached += OnGuideDestinationReached;

        if (autoStartOnPlay)
            StartStory();
    }

    public void StartStory()
    {
        if (steps == null || steps.Length == 0)
        {
            Debug.LogWarning("Steps empty");
            return;
        }

        _currentIndex = -1;
        _isRunning = true;
        _stepTimer = 0f;

        Log("Story");
        NextStep();
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        // Maybe ill make FixedUpdate
        if (_stepTimer > 0f)
        {
            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                Log("Timer bitti, NextStep çağrılıyor.");
                NextStep();
            }
        }
    }

    private void NextStep()
    {
        _currentIndex++;

        if (_currentIndex >= steps.Length)
        {
            Log("Story Over");
            _isRunning = false;
            guide?.PlayIdle();
            return;
        }

        var step = steps[_currentIndex];
        Log($"Step {_currentIndex} - {step.stepName} - {step.actionType}");

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

            default:
                Log("Unkown ActionType");
                NextStep();
                break;
        }
    }

    private void HandleMoveTo(StoryStep step)
    {
        if (step.moveTarget == null)
        {
            Log("No Target");
            NextStep();
            return;
        }

        Log($"MoveTo -> {step.moveTarget.name}");
        guide.GoToTarget(step.moveTarget.position);

        _stepTimer = 0f;
    }

    private void HandleTalk(StoryStep step)
    {
        Log("Talk step");

        guide.Talk(step.voiceClip);

        float clipLen = (step.voiceClip != null) ? step.voiceClip.length : 0f;
        _stepTimer = clipLen + Mathf.Max(0f, step.waitDuration);

        if (_stepTimer <= 0f)
        {
            NextStep();
        }
    }

    private void HandleWait(StoryStep step)
    {
        Log("Wait step");
        guide.PlayIdle();
        _stepTimer = Mathf.Max(0.1f, step.waitDuration);
    }

    private void HandlePlayAnimation(StoryStep step)
    {
        Log($"PlayAnimation step: {step.animationId}");

        if (AnimationManager.Instance != null)
            AnimationManager.Instance.ChangeState(step.animationId);

        if (step.waitDuration > 0f)
        {
            _stepTimer = step.waitDuration;
        }
        else
        {
            NextStep();
        }
    }

    private void OnGuideDestinationReached()
    {
        if (!_isRunning) return;

        Log("MoveTo Step Complete");

        if (_currentIndex < 0 || _currentIndex >= steps.Length)
            return;

        var step = steps[_currentIndex];
        if (step.actionType != StoryActionType.MoveTo)
            return;

        if (step.waitDuration > 0f)
        {
            guide.PlayIdle();
            _stepTimer = step.waitDuration;
        }
        else
        {
            NextStep();
        }
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"StoryController: {msg}");
    }
}
