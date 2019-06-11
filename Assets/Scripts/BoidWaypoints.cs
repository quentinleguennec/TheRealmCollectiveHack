namespace Tengio
{
    using System.Collections.Generic;
    using UnityEngine;

    public class BoidWaypoints : Singleton<BoidWaypoints>
    {
        private List<Transform> wayPoints = new List<Transform>();
        private int currentWaypointIndex = 0;

        public bool IsEmpty { get => wayPoints.Count == 0; }


        protected BoidWaypoints() { } // This is a singleton

        public void ClearWaypoints()
        {
            wayPoints.ForEach((waypoint) => Destroy(waypoint.gameObject));
            wayPoints.Clear();
            currentWaypointIndex = 0;
        }

        public Transform AddWaypoint(Vector3 position)
        {
            Transform newWaypoint = new GameObject().transform;
            newWaypoint.name = "Waypoint";
            newWaypoint.SetParent(transform);
            newWaypoint.position = position;
            newWaypoint.tag = Tags.BOID_WAYPOINT;
            SphereCollider collider = newWaypoint.gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.enabled = wayPoints.Count == 0;
            wayPoints.Add(newWaypoint);
            return newWaypoint;
        }

        public Transform GetCurrentWaypoint() => wayPoints[currentWaypointIndex];

        public void MoveToNextWaypoint()
        {
            if (wayPoints.Count == 1)
            {
                return;
            }

            wayPoints[currentWaypointIndex].GetComponent<Collider>().enabled = false;

            currentWaypointIndex++;
            if (currentWaypointIndex == wayPoints.Count)
            {
                currentWaypointIndex = 0;
            }

            wayPoints[currentWaypointIndex].GetComponent<Collider>().enabled = true;
        }
    }
}
