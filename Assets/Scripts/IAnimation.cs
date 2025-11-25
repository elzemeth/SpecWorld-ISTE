using UnityEngine;
public enum AnimationStateId
{
    Idle,
    Walk,
    Talk,
    Pointing,
    Victory,
    Rejected
}

public interface IAnimation
{
    AnimationStateId Id { get; }
    void Enter(Animator animator);
    void Tick(Animator animator, float deltaTime);
    void Exit(Animator animator);
}
