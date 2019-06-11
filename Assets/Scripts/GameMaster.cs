namespace Tengio
{
    using UnityEngine;

    public static class Layers
    {
        public static class Id
        {
            public const int BOIDS = 13;
            public const int BOIDS_OBSTACLES = 15;

            public static int GetMask(params int[] layerIds)
            {
                int mask = 0;
                for (int i = 0; i < layerIds.Length; i++)
                {
                    mask |= 1 << layerIds[i];
                }
                return mask;
            }
        }

        public static class Name
        {
            public const string BOIDS = "Boids";
            public const string BOIDS_OBSTACLES = "BoidsObstacles";

            public static int GetMask(params string[] layerIds) => LayerMask.GetMask(layerIds);
        }
    }

    public static class Tags
    {
        public const string BOID_WAYPOINT = "BoidWaypoint";
    }
}
