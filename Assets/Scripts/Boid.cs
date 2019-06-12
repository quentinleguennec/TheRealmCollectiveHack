namespace Tengio
{
    using System.Collections.Generic;
    using UnityEngine;

    public struct Force
    {
        public Vector3 direction;
        public Vector3 position;
    };

    public class BoidSettings
    {
        public BoidSettings()
        {
            ResetWithFishBoid();
        }

        private float separationOptimalDistance = 0.3f;
        private float colissionOptimalDistance = 0.2f;

        public float SpeedMultiplier { get; set; } = 9.0f;

        /// <summary>
        /// Max distance at which a boid see (and take into account) other boids. 
        /// </summary>
        public float FlyingViewRadius { get; set; } = 0.6f;

        public float SeparationOptimalDistance
        {
            get => separationOptimalDistance;
            set
            {
                separationOptimalDistance = value;
                SeparationOptimalDistanceFactor = separationOptimalDistance * separationOptimalDistance * 0.5f;
            }
        }

        public float SeparationOptimalDistanceFactor { get; private set; } = 0.3f * 0.3f * 0.5f;

        public float ColissionOptimalDistance
        {
            get => colissionOptimalDistance;
            set
            {
                colissionOptimalDistance = value;
                ColissionForceAtOptimalDistance = colissionOptimalDistance * 0.5f;
            }
        }

        public float ColissionForceAtOptimalDistance { get; private set; } = 0.2f * 0.5f;

        public float MinSpeed => 0.1f * SpeedMultiplier;

        public float AligmentForcePart => 0.0002f;

        public float TotalForceMultipliyer => 1f;

        public float Inertness => 0.5f;

        public float VerticalPriority => 1.22f;

        public float AttractrionForce { get; set; } = 0.15f;

        public float CollisionAvoidanceForceFactor1 => FlyingViewRadius;

        public float CollisionAvoidanceForceFactor2 => -2 * SpeedMultiplier * ColissionForceAtOptimalDistance * ColissionOptimalDistance / (ColissionOptimalDistance - FlyingViewRadius);

        public LayerMask InteractionLayerMask { get; set; } = Layers.Id.GetMask(Layers.Id.BOIDS, Layers.Id.BOIDS_OBSTACLES);


        public void ResetWithBirdBoid()
        {
            SpeedMultiplier = 9f;
            FlyingViewRadius = 0.6f;
            SeparationOptimalDistance = 0.3f;
            ColissionOptimalDistance = 0.2f;
            AttractrionForce = 0.15f;
            InteractionLayerMask = Layers.Id.GetMask(Layers.Id.BOIDS, Layers.Id.BOIDS_OBSTACLES);
        }

        public void ResetWithFishBoid()
        {
            SpeedMultiplier = 3f;
            FlyingViewRadius = 0.6f;
            SeparationOptimalDistance = 0.3f;
            ColissionOptimalDistance = 0.2f;
            AttractrionForce = 0.25f;
            InteractionLayerMask = Layers.Id.GetMask(Layers.Id.BOIDS, Layers.Id.BOIDS_OBSTACLES);
        }
    }

    public class Boid : MonoBehaviour
    {
        [SerializeField]
        private BoidShape shape;

        public BoidSettings Settings { get; } = new BoidSettings();
        public int Index { get; set; }
        public Vector3 Velocity { get; private set; } = Vector3.zero;
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public int[] NeighborsIndexes { get; private set; } = new int[0];

        private new Rigidbody rigidbody;
        private bool didComputeThisFrame = false;

        /// <summary>
        /// The type of boid will influence it's behaviour.
        /// </summary>
        public bool IsFish
        {
            get => isFish;
            set
            {
                isFish = value;
                if (isFish)
                {
                    Settings.ResetWithFishBoid();
                }
                else
                {
                    Settings.ResetWithBirdBoid();
                }
            }
        }
        private bool isFish = false;

        /// <summary>
        /// Buffer for [Physics.OverlapSphereNonAlloc] to reduce garbage collection. The array must be big enough to contain all possible colliding objects.
        /// </summary>
        private Collider[] overlapSphereNeighbors = new Collider[200];


        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            didComputeThisFrame = false;
        }

        // Play with the physics timestep to test performance.
        private void FixedUpdate()
        {
            MoveRigidbody();

            if (didComputeThisFrame) return;
            didComputeThisFrame = true;


            //Algorithm based on
            //http://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
            //http://www.red3d.com/cwr/boids/

            //Bird is affected by 3 base forces:
            // cohesion
            // separation + collisionAvoidance
            // alignmentForce

            //Geometric center of visible birds
            var centroid = Vector3.zero;

            var collisionAvoidance = Vector3.zero;
            var avgSpeed = Vector3.zero;
            var neighborBoidsCount = 0;

            //Store it as an optimization
            var direction = Velocity.normalized;
            var currentPosition = rigidbody.position;

            int neighborsCount = Physics.OverlapSphereNonAlloc(currentPosition, Settings.FlyingViewRadius, overlapSphereNeighbors, Settings.InteractionLayerMask);
            List<int> neighborBoidsIndexes = new List<int>();
            for (int i = 0; i < neighborsCount; i++)
            {
                Collider neighbor = overlapSphereNeighbors[i];
                Vector3 neighborPosition = neighbor.transform.position;
                Boid neighborBoid;

                if ((neighborBoid = neighbor.GetComponent<Boid>()) != null) // Boid processing
                {
                    if (neighborBoid == this)
                    {
                        continue;
                    }

                    neighborBoidsIndexes.Add(neighborBoid.Index);
                    collisionAvoidance += ComputeSeparationForce(currentPosition, neighborPosition, Settings);
                    neighborBoidsCount++;
                    centroid += neighborPosition;
                    avgSpeed += neighborBoid.Velocity;
                }
                else // Obstacle processing
                {
                    Vector3 pointOnBounds = MathTools.CalcPointOnBounds(neighbor, currentPosition);
                    if (ComputeCollisionAvoidanceForce(currentPosition, direction, neighbor, pointOnBounds, out Force force, Settings))
                    {
                        collisionAvoidance += force.direction;
                    }
                }
            }

            NeighborsIndexes = neighborBoidsIndexes.ToArray();

            if (neighborBoidsCount > 0)
            {
                //Cohesion force. It makes united formula with BoidTools.SeparationForce
                centroid = centroid / neighborBoidsCount - currentPosition;

                //Spherical shape of flock looks unnatural, so let's scale it along y axis
                centroid.y *= Settings.VerticalPriority;

                //Difference between current bird speed and average speed of visible birds
                avgSpeed = avgSpeed / neighborBoidsCount - Velocity;
            }


            var positionForce = (1.0f - Settings.AligmentForcePart) * Settings.SpeedMultiplier * (centroid + collisionAvoidance);
            var alignmentForce = Settings.AligmentForcePart * avgSpeed / Time.fixedDeltaTime;
            var attractionForce = ComputeAttractionForce(currentPosition, direction, Settings);
            var totalForce = Settings.TotalForceMultipliyer * (positionForce + alignmentForce + attractionForce);
            Vector3 newVelocity = (1 - Settings.Inertness) * (totalForce * Time.fixedDeltaTime) + Settings.Inertness * Velocity;

            Velocity = ComputeNewVelocity(Velocity, newVelocity, direction, Settings);
            Rotation = Quaternion.LookRotation(Velocity);
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.tag == Tags.BOID_WAYPOINT)
            {
                BoidWaypoints.Instance.MoveToNextWaypoint();
            }
        }

        private void MoveRigidbody()
        {
            //rigidbody.velocity = Velocity; // Would only work if non kynematic.
            rigidbody.MovePosition(rigidbody.position + Velocity * Time.fixedDeltaTime);
            shape.Rotation = Rotation;
        }

        #region ForcesComputation  

        //Force which attracts birds to waypoints
        static Vector3 ComputeAttractionForce(Vector3 currentPosition, Vector3 boidDirection, BoidSettings boidSettings)
        {
            Vector3 attractorPosition = Vector3.zero;
            if (!BoidWaypoints.Instance.IsEmpty)
            {
                attractorPosition = BoidWaypoints.Instance.GetCurrentWaypoint().position;
            }
            else
            {
                return Vector3.zero;
            }

            Vector3 attractorDirection = (attractorPosition - currentPosition).normalized;

            //The force have an effect only on direction and shouldn't increase speed if bird flies in the right direction
            float factor = boidSettings.AttractrionForce * boidSettings.SpeedMultiplier * MathTools.AngleToFactor(attractorDirection, boidDirection);

            return factor * attractorDirection;
        }

        static Vector3 ComputeNewVelocity(Vector3 currentVelocity, Vector3 desiredVelocity, Vector3 defaultVelocity, BoidSettings boidSettings)
        {
            //We have to take into account that bird can't change their direction instantly. That's why
            //dsrVel (desired velocity) influence first of all on flying direction and after that on
            //velocity magnitude oneself

            var currentVelocityMag = currentVelocity.magnitude;

            if (currentVelocityMag > Mathf.Epsilon)
                currentVelocity /= currentVelocityMag;
            else
            {
                currentVelocity = defaultVelocity;
                currentVelocityMag = 1;
            }

            var desiredVelocityMag = desiredVelocity.magnitude;
            var resultMag = boidSettings.MinSpeed;

            if (desiredVelocityMag > Mathf.Epsilon)
            {
                desiredVelocity /= desiredVelocityMag;

                //We spend a part of velocity magnitude on bird rotation and the rest of it on speed magnitude changing

                //Map rotation to factor [0..1]
                var angleFactor = MathTools.AngleToFactor(desiredVelocity, currentVelocity);

                //If desiredVelocity magnitude is twice bigger than currentVelocityMag then bird can rotate on any angle
                var rotReqLength = 2f * currentVelocityMag * angleFactor;

                //Velocity magnitude remained after rotation
                var speedRest = desiredVelocityMag - rotReqLength;

                if (speedRest > 0)
                {
                    currentVelocity = desiredVelocity;
                    resultMag = speedRest;
                }
                else
                {
                    currentVelocity = Vector3.Slerp(currentVelocity, desiredVelocity, desiredVelocityMag / rotReqLength);
                }

                if (resultMag < boidSettings.MinSpeed)
                    resultMag = boidSettings.MinSpeed;
            }

            return currentVelocity * resultMag;
        }

        static public Vector3 ComputeSeparationForce(Vector3 currentPosition, Vector3 otherPosition, BoidSettings boidSettings)
        {
            Vector3 invertedDirection = currentPosition - otherPosition;
            float squaredDistance = invertedDirection.sqrMagnitude;
            Vector3 force = invertedDirection * boidSettings.SeparationOptimalDistanceFactor / squaredDistance;
            return force;
        }

        //Force between birds and obstacles
        //We make an asumption that between an obstacle and a bird on the distance OptDistance should exists same
        //force as between two birds on the same distance
        static public bool ComputeCollisionAvoidanceForce(Vector3 currentPosition, Vector3 boidDirection, Collider obstacleCollider, Vector3 pointOnBounds, out Force force, BoidSettings boidSettings)
        {
            Vector3 reversedDirection = currentPosition - pointOnBounds;
            float distance = reversedDirection.magnitude;

            if (distance <= Mathf.Epsilon)
            {
                //Let's setup the direction to outside of colider
                reversedDirection = (pointOnBounds - obstacleCollider.transform.position).normalized;

                //and distance to N percent of OptDistance
                distance = 0.1f * boidSettings.ColissionOptimalDistance;
            }
            else
            {
                reversedDirection /= distance;
            }

            //Force depends on direction of bird: no need to turn a bird if it is flying in opposite direction
            force.direction = reversedDirection * (boidSettings.CollisionAvoidanceForceFactor2 * (boidSettings.CollisionAvoidanceForceFactor2 / distance - 1) * MathTools.AngleToFactor(reversedDirection, boidDirection));
            force.position = pointOnBounds;
            return true;
        }
        #endregion  
    }
}