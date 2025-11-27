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
    [Tooltip("Extra Delay")]
    public float talkExtraWait = 0.5f;

    [Header("Wait")]
    public float waitDuration = 1f;

    [Header("PlayAnimation")]
    public AnimationStateId animationId;
    [Tooltip("Extra Delay")]
    public float animationDuration = 1f;

    [Header("DoorOpen")]
    public DoorController door;
    [Tooltip("Unlock and Open Door")]
    public bool unlockBeforeOpen = false;
}
