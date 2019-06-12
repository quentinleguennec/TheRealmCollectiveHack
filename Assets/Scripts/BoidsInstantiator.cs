namespace Tengio
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class BoidsInstantiator : Singleton<BoidsInstantiator>
    {
        [SerializeField]
        private GameObject boidPrefab;
        [SerializeField]
        private Transform boidsParent;
        [SerializeField]
        private Material commonBoidMaterial;
        [SerializeField]
        private Transform instantiatePosition;

        public List<Boid> Boids { get; private set; } = new List<Boid>(150);
        public int[][] BoidsGroups { get; private set; }

        public const int InitialBoidsCount = 150;

        protected BoidsInstantiator() { } // This is a singleton

        private void Start()
        {
            InstantiateBoids();
        }

        private void InstantiateBoids()
        {
            for (int i = 0; i < InitialBoidsCount; i++)
            {
                InstantiateBoid(instantiatePosition.position + UnityEngine.Random.insideUnitSphere * 0.3f, isFish: true);
            }
        }

        public Boid InstantiateBoid(Vector3 position, bool isFish = false)
        {
            Boid boid = Instantiate(boidPrefab, position, Quaternion.identity, boidsParent).GetComponent<Boid>();
            Boids.Add(boid);
            boid.Index = Boids.IndexOf(boid);
            boid.IsFish = isFish;
            return boid;
        }

        public void UpdateBoidsGroups()
        {
            List<List<int>> newGroups = GetBoidsGroups();

            if (DidGroupsChange(newGroups))
            {
                BoidsGroups = new int[newGroups.Count][];
                for (int i = 0; i < BoidsGroups.Length; i++)
                {
                    BoidsGroups[i] = newGroups[i].ToArray();
                }
            }
        }

        private bool DidGroupsChange(List<List<int>> newGroups)
        {
            if (BoidsGroups != null && newGroups.Count == BoidsGroups.Length)
            {
                for (int i = 0; i < newGroups.Count; i++)
                {
                    for (int j = 0; j < newGroups[i].Count; j++)
                    {
                        if (!BoidsGroups[i].Contains(newGroups[i][j]))
                        {
                            return true;
                        }
                    }
                }
            }
            return true;
        }

        private List<List<int>> GetBoidsGroups()
        {
            List<List<int>> groups = new List<List<int>>();
            bool[] areBoidsGroupedList = new bool[Boids.Count];

            int boidsProcessedCount = 0;
            while (boidsProcessedCount < Boids.Count)
            {
                int nextUnsortedBoidIndex = Array.IndexOf(areBoidsGroupedList, false);
                List<int> currentGroup = new List<int>();
                currentGroup.Add(nextUnsortedBoidIndex);
                areBoidsGroupedList[nextUnsortedBoidIndex] = true;
                AddNeighborsToGroupRecursive(Boids[nextUnsortedBoidIndex].NeighborsIndexes, ref currentGroup, ref areBoidsGroupedList);
                groups.Add(currentGroup);
                boidsProcessedCount += currentGroup.Count;
            }
            return groups;
        }

        private void AddNeighborsToGroupRecursive(int[] neighbors, ref List<int> group, ref bool[] areBoidsGroupedList)
        {
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (!group.Contains(neighbors[i]))
                {
                    areBoidsGroupedList[neighbors[i]] = true;
                    group.Add(neighbors[i]);
                    AddNeighborsToGroupRecursive(Boids[neighbors[i]].NeighborsIndexes, ref group, ref areBoidsGroupedList);
                }
            }
        }

        public void UpdateGroupsLooping(float delayBetweenLoops)
        {
            StartCoroutine(UpdateGroupsLoopingEnumerator(delayBetweenLoops));
        }

        private IEnumerator UpdateGroupsLoopingEnumerator(float delayBetweenLoops)
        {
            while (true)
            {
                yield return new WaitForSeconds(delayBetweenLoops);
                UpdateBoidsGroups();
            }
        }
    }
}
