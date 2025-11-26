using UnityEngine;
using UnityEngine.AI;
using System;

public class GuideManager : MonoBehaviour
{
    [SerializeField] Transform player;
    public float walkSpeed;
    public float waitDistance;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Animator animator;

    [SerializeField] bool isMoving = false;
    [SerializeField] bool isWaiting = false;

    public Action OnDestinationReached;
    void Start()
    {
        agent.speed = walkSpeed;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Update()
    {

        if (isMoving && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            isMoving = false;
            PlayAnimation("Idle");
            LookAtPlayer();

            OnDestinationReached?.Invoke();
        }

        if(isMoving)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > waitDistance)
            {
                agent.isStopped = true;
                isWaiting = true;
                PlayAnimation("Idle");
                LookAtPlayer();
            }
            else
            {
                agent.isStopped = false;
                isWaiting = false;
                PlayAnimation("Walk");
            }
        }
    }

    public void GoToTarget(Vector3 target)
    {
        isMoving = true;
        isWaiting = false;
        agent.isStopped = false;
        agent.SetDestination(target);
        PlayAnimation("Walk");
    }

    public void Talk(AudioClip audio)
    {
        if(audioSource.isPlaying)
            audioSource.Stop();

        audioSource.clip = audio;
        audioSource.Play();

        LookAtPlayer();

        agent.isStopped = true;
        PlayAnimation("Talk");
    }

    public void LookAtPlayer()
    {
        Vector3 playerPos = new Vector3(player.position.x, player.position.y, player.position.z);
        transform.LookAt(playerPos);
    }

    public void PlayAnimation(string state)
    {
        if (state == "Walk")
            animator.SetBool("isWalking", true);
        else
            animator.SetBool("isWalking", false);
    }
}
