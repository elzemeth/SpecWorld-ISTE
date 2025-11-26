using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TourController : MonoBehaviour
{
    public GuideManager guideManager;
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    public float waitAtWaypoint = 3.0f;

    private void Start()
    {
        guideManager.OnDestinationReached += HandleArrival;
        MoveToNextPoint();
    }

    private void OnDestroy()
    {
        if (guideManager != null)
            guideManager.OnDestinationReached -= HandleArrival;
    }

    void HandleArrival()
    {
        StartCoroutine(WaitAndGoNext());
    }

    IEnumerator WaitAndGoNext()
    {
        Debug.Log("Hedefe varýldý, bekleniyor...");
        yield return new WaitForSeconds(waitAtWaypoint);

        currentWaypointIndex++;
        MoveToNextPoint();
    }

    public void MoveToNextPoint()
    {
        if (waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
        {
            Debug.Log("Tour finished or no waypoints set.");
            return;
        }

        Transform targetPoint = waypoints[currentWaypointIndex];
        guideManager.GoToTarget(targetPoint.position);
    }

    public void ResetTour()
    {
        currentWaypointIndex = 0;
        MoveToNextPoint();
    }
}
