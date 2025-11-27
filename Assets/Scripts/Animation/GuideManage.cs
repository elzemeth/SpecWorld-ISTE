using UnityEngine;
using UnityEngine.AI;
using System;

public class GuideManage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private AudioSource audioSource;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float waitDistance = 300f;

    public event Action OnDestinationReached;

    private bool isMoving;
    private bool isWaiting;
    private bool destinationEventSent;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent");
            return;
        }

        agent.speed = walkSpeed;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        PlayIdle();
    }

    private void Update()
    {
        if (!isMoving || agent == null)
            return;

        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.001f)
                {
                    if (!destinationEventSent)
                    {
                        destinationEventSent = true;
                        isMoving = false;
                        isWaiting = false;
                        agent.isStopped = true;

                        PlayIdle();
                        LookAtPlayer();

                        Debug.Log("Event Trigger");
                        OnDestinationReached?.Invoke();
                    }
                }
            }
        }

        //if (player != null && isMoving && !destinationEventSent)
        //{
        //    float distance = Vector3.Distance(transform.position, player.position);

        //    if (distance > waitDistance)
        //    {
        //        if (!isWaiting)
        //        {
        //            isWaiting = true;
        //            agent.isStopped = true;
        //            PlayIdle();
        //            LookAtPlayer();
        //            Debug.Log("Waiting Player");
        //        }
        //    }
        //    else
        //    {
        //        if (isWaiting)
        //        {
        //            isWaiting = false;
        //            agent.isStopped = false;
        //            PlayWalking();
        //            Debug.Log("Keep Going");
        //        }
        //    }
        //}
    }

    public void GoToTarget(Vector3 target)
    {
        if (agent == null)
        {
            Debug.LogError("No Agent");
            return;
        }

        Debug.Log("GoToTarget Called" + target);

        destinationEventSent = false;
        isMoving = true;
        isWaiting = false;

        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.SetDestination(target);

        PlayWalking();
    }

    public void Talk(AudioClip clip)
    {
        Debug.Log("Talk Called");

        if (audioSource != null && clip != null)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = clip;
            audioSource.Play();
        }

        if (agent != null)
        {
            agent.isStopped = true;
        }

        isMoving = false;
        isWaiting = false;

        LookAtPlayer();
        PlayTalking();
    }

    public void PlayIdle()
    {
        if (AnimationManager.Instance != null)
            AnimationManager.Instance.ChangeState(AnimationStateId.Idle);
    }

    public void PlayWalking()
    {
        if (AnimationManager.Instance != null)
            AnimationManager.Instance.ChangeState(AnimationStateId.Walking);
    }

    public void PlayTalking()
    {
        if (AnimationManager.Instance != null)
            AnimationManager.Instance.ChangeState(AnimationStateId.Talking);
    }

    public void PlayPointing()
    {
        if (AnimationManager.Instance != null)
            AnimationManager.Instance.ChangeState(AnimationStateId.Pointing);
    }

    public void PlayVictory()
    {
        if (AnimationManager.Instance != null)
            AnimationManager.Instance.ChangeState(AnimationStateId.Victory);
    }

    public void PlayRejected()
    {
        if (AnimationManager.Instance != null)
            AnimationManager.Instance.ChangeState(AnimationStateId.Rejected);
    }

    public void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 target = player.position;
        target.y = transform.position.y;

        transform.LookAt(target);
    }
}
