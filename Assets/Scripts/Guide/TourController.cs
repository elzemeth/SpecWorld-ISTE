using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TourController : MonoBehaviour
{
    [Header("References")]
    public GuideManager guideManager;

    [Header("Tour Settings")]
    public List<Transform> waypoints;
    public List<AudioClip> routeVoices;
    public float waitAtWaypoint = 2f;

    private int index = 0;
    private bool waiting = false;

    void Start()
    {
        if (guideManager == null)
            guideManager = FindObjectOfType<GuideManager>();

        guideManager.OnDestinationReached += OnArrived;

        if (waypoints != null && waypoints.Count > 0)
        {
            index = 0;
            guideManager.GoToTarget(waypoints[index].position);
        }
    }

    void OnDestroy()
    {
        if (guideManager != null)
            guideManager.OnDestinationReached -= OnArrived;
    }

    void OnArrived()
    {
        if (!waiting)
            StartCoroutine(HandleArrival());
    }

    IEnumerator HandleArrival()
    {
        waiting = true;

        if (routeVoices != null &&
            index < routeVoices.Count &&
            routeVoices[index] != null)
        {
            guideManager.Talk(routeVoices[index]);
        }

        yield return new WaitForSeconds(waitAtWaypoint);

        index++;

        if (index < waypoints.Count)
        {
            guideManager.GoToTarget(waypoints[index].position);
        }
        else
        {
            Debug.Log("Tur tamamlandı.");
        }

        waiting = false;
    }

    public void ResetTour()
    {
        StopAllCoroutines();
        waiting = false;
        index = 0;

        if (waypoints != null && waypoints.Count > 0)
            guideManager.GoToTarget(waypoints[index].position);
    }
}
