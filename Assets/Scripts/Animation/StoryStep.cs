using UnityEngine;

[System.Serializable]
public class StoryStep
{
    public string stepName;

    public StoryActionType actionType = StoryActionType.MoveTo;

    [Header("MoveTo")]
    public Transform moveTarget;

    [Header("Talk")]
    public AudioClip voiceClip;

    [Header("Delay")]
    public float waitDuration = 0f;

    [Header("PlayAnimation")]
    public AnimationStateId animationId = AnimationStateId.Idle;
}
