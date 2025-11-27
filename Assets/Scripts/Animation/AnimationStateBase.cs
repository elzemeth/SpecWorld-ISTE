using UnityEngine;

public abstract class AnimationStateBase : IAnimation
{
    public abstract AnimationStateId Id { get; }

    protected virtual string TriggerName => Id.ToString();

    public virtual void Enter(Animator animator)
    {
        if (animator == null) return;
        ResetAllTriggers(animator);
        animator.SetTrigger(TriggerName);
    }

    public virtual void Tick(Animator animator, float deltaTime)
    {
        // Every Tick
    }

    public virtual void Exit(Animator animator)
    {
        // When Exit
    }

    protected void ResetAllTriggers(Animator animator)
    {
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walking");
        animator.ResetTrigger("Talking");
        animator.ResetTrigger("Pointing");
        animator.ResetTrigger("Victory");
        animator.ResetTrigger("Rejected");
    }
}
