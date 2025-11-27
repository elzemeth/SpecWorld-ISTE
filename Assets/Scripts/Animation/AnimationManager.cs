using System.Collections.Generic;
using UnityEngine;

/// AnimationManager.Instance.ChangeState(AnimationStateId.Talking);
public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Animator animator;

    private readonly Dictionary<AnimationStateId, IAnimation> _states =
        new Dictionary<AnimationStateId, IAnimation>();

    private IAnimation _currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (animator == null)
            animator = GetComponent<Animator>();

        RegisterState(new IdleAnimation());
        RegisterState(new WalkingAnimation());
        RegisterState(new TalkingAnimation());
        RegisterState(new PointingAnimation());
        RegisterState(new VictoryAnimation());
        RegisterState(new RejectedAnimation());

        ChangeState(AnimationStateId.Idle);
    }

    private void Update()
    {
        if (_currentState != null)
        {
            _currentState.Tick(animator, Time.deltaTime);
        }
    }

    public void RegisterState(IAnimation state)
    {
        if (state == null)
        {
            Debug.LogWarning("Null State");
            return;
        }

        var id = state.Id;

        if (_states.ContainsKey(id))
        {
            Debug.LogWarning($"Override state");
            _states[id] = state; 
        }
        else
        {
            _states.Add(id, state);
        }
    }

    public void ChangeState(AnimationStateId newStateId)
    {
        if (_currentState != null && _currentState.Id == newStateId)
            return;

        if (!_states.TryGetValue(newStateId, out var newState))
        {
            Debug.LogWarning($"State Not Found");
            return;
        }

        _currentState?.Exit(animator);

        _currentState = newState;

        _currentState.Enter(animator);
    }

    public AnimationStateId CurrentStateId =>
        _currentState != null ? _currentState.Id : AnimationStateId.Idle;
}
