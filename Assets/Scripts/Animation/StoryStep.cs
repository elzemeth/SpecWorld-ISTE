using System;
using UnityEngine;

[Serializable]
public class StoryStep
{
    [Header("Genel")]
    public string stepName;
    public StoryActionType actionType;

    [Header("MoveTo")]
    public Transform moveTarget;

    [Header("Talk")]
    public AudioClip voiceClip;
    public float talkExtraDelay = 0.2f;

    [Header("Wait")]
    public float waitDuration = 1f;

    [Header("PlayAnimation")]
    public AnimationStateId animationId = AnimationStateId.Idle;
    public float animationDuration = 1.5f;
}