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
        private Transform birdParent;
        [SerializeField]
        private GameObject boidAudioSourcePrefab;
        [SerializeField]
        private Material commonBoidMaterial;
        [SerializeField]
        private Material leaderBoidMaterial;
        [SerializeField]
        private Pool boidAudioSourcePool;
        [SerializeField]
        private AudioClip[] flyingAudioClips;
        [SerializeField]
        private AudioClip[] perchedAudioClips;
        [SerializeField]
        private AudioClip[] fishAudioClips;
        [SerializeField]
        private AudioClip shapeTransitionAudioClip;
        [SerializeField]
        private Transform instantiatePosition;

        public List<Boid> Boids { get; private set; } = new List<Boid>(150);
        public int[][] BoidsGroups { get; private set; }

        private int nextAudioClipIndex = 0;

        public const int InitialBoidsCount = 150;


        protected BoidsInstantiator() { } // This is a singleton

        private void Start()
        {
            InstantiateBoids();
            //UpdateGroupsLooping(0.5f);
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
            Boid boid = Instantiate(boidPrefab, position, Quaternion.identity, birdParent).GetComponent<Boid>();
            Boids.Add(boid);
            boid.Index = Boids.IndexOf(boid);
            boid.IsFish = isFish;

            if (boid.Index < InitialBoidsCount || boid.Index % 10 == 0)
            {
                BoidAudioSource audioSource = Instantiate(boidAudioSourcePrefab, boid.transform).GetComponent<BoidAudioSource>();
                boid.AudioSource = audioSource;
                audioSource.FlyingAudioClip = flyingAudioClips[nextAudioClipIndex];
                audioSource.PerchedAudioClip = perchedAudioClips[nextAudioClipIndex];
                audioSource.FishAudioClip = fishAudioClips[nextAudioClipIndex];
                audioSource.ShapeTransitionAudioClip = shapeTransitionAudioClip;
                nextAudioClipIndex++;
                // TODO: Refacto: Not the best, all the audio clip arrays should have the same size.
                if (nextAudioClipIndex == flyingAudioClips.Length)
                {
                    nextAudioClipIndex = 0;
                }
                if (boid.Index < InitialBoidsCount - 1)
                {
                    Utils.ExecuteAfterDelay(UnityEngine.Random.Range(0f, 7f), this, () => audioSource.PlayFish());
                }
            }
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

            //Debug.Log("Group size = " + groups[0]);
            //string boidsIndexes = "";
            //foreach (var value in groups[0])
            //{
            //    boidsIndexes += ", " + value;
            //}
            //Debug.Log(boidsIndexes);

            //foreach (var group in groups)
            //{
            //    foreach (var value in group)
            //    {
            //        if (group.FindAll(item => item == value).Count > 1)
            //        {
            //            Debug.Log("Error: duplicate!" + value);
            //            break;
            //        }
            //    }
            //}

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

        private Vector3[] GetStartingPositions()
        {
            return new Vector3[150] {
                new Vector3(4.056f, 0.05f, 1.671f),
                new Vector3(2.881f, 0.05f, 0.924f),
                new Vector3(2.262f, 0.05f, 1.052f),
                new Vector3(2.607f, 0.05f, 0.060f),
                new Vector3(3.909f, 0.05f, 1.849f),
                new Vector3(2.377f, 0.05f, 3.596f),
                new Vector3(3.286f, 0.05f, 1.593f),
                new Vector3(4.328f, 0.05f, 2.398f),
                new Vector3(3.922f, 0.05f, 2.320f),
                new Vector3(3.203f, 0.05f, 1.064f),
                new Vector3(2.961f, 0.05f, 3.269f),
                new Vector3(1.761f, 0.05f, 1.306f),
                new Vector3(2.686f, 0.05f, 3.085f),
                new Vector3(2.629f, 0.05f, 1.314f),
                new Vector3(3.166f, 0.05f, 0.090f),
                new Vector3(2.258f, 0.05f, 1.469f),
                new Vector3(3.706f, 0.05f, 0.458f),
                new Vector3(1.297f, 0.05f, 2.452f),
                new Vector3(3.895f, 0.05f, 3.294f),
                new Vector3(2.404f, 0.05f, 1.919f),
                new Vector3(1.891f, 0.05f, 2.025f),
                new Vector3(2.841f, 0.05f, 2.791f),
                new Vector3(2.029f, 0.05f, 0.386f),
                new Vector3(2.646f, 0.05f, 0.267f),
                new Vector3(2.572f, 0.05f, 1.850f),
                new Vector3(4.963f, 0.05f, 2.071f),
                new Vector3(4.687f, 0.05f, 1.444f),
                new Vector3(1.263f, 0.05f, 1.472f),
                new Vector3(4.094f, 0.05f, 3.654f),
                new Vector3(1.858f, 0.05f, 0.444f),
                new Vector3(4.320f, 0.05f, 3.305f),
                new Vector3(4.544f, 0.05f, 2.119f),
                new Vector3(3.677f, 0.05f, 0.226f),
                new Vector3(4.795f, 0.05f, 2.128f),
                new Vector3(2.259f, 0.05f, 3.139f),
                new Vector3(1.864f, 0.05f, 3.162f),
                new Vector3(4.414f, 0.05f, 1.845f),
                new Vector3(2.991f, 0.05f, 0.441f),
                new Vector3(4.971f, 0.05f, 2.263f),
                new Vector3(2.131f, 0.05f, 0.266f),
                new Vector3(3.352f, 0.05f, 0.638f),
                new Vector3(3.826f, 0.05f, 1.633f),
                new Vector3(3.125f, 0.05f, 3.367f),
                new Vector3(2.286f, 0.05f, 3.37f),
                new Vector3(3.843f, 0.05f, 1.384f),
                new Vector3(2.077f, 0.05f, 2.738f),
                new Vector3(3.118f, 0.05f, 0.879f),
                new Vector3(2.334f, 0.05f, 0.658f),
                new Vector3(3.474f, 0.05f, 3.263f),
                new Vector3(2.042f, 0.05f, 2.215f),
                new Vector3(3.738f, 0.05f, 1.147f),
                new Vector3(1.402f, 0.05f, 1.326f),
                new Vector3(3.280f, 0.05f, 2.312f),
                new Vector3(3.439f, 0.05f, 3.648f),
                new Vector3(1.830f, 0.05f, 2.830f),
                new Vector3(4.182f, 0.05f, 2.203f),
                new Vector3(2.466f, 0.05f, 0.890f),
                new Vector3(2.133f, 0.05f, 1.994f),
                new Vector3(4.937f, 0.05f, 1.845f),
                new Vector3(4.698f, 0.05f, 1.236f),
                new Vector3(3.637f, 0.05f, 2.843f),
                new Vector3(4.571f, 0.05f, 2.688f),
                new Vector3(2.576f, 0.05f, 2.194f),
                new Vector3(3.215f, 0.05f, 2.591f),
                new Vector3(2.970f, 0.05f, 0.652f),
                new Vector3(4.338f, 0.05f, 0.972f),
                new Vector3(3.367f, 0.05f, 0.420f),
                new Vector3(3.458f, 0.05f, 0.905f),
                new Vector3(1.759f, 0.05f, 2.232f),
                new Vector3(3.658f, 0.05f, 0.658f),
                new Vector3(4.852f, 0.05f, 2.836f),
                new Vector3(2.048f, 0.05f, 1.507f),
                new Vector3(2.289f, 0.05f, 2.702f),
                new Vector3(2.952f, 0.05f, 3.664f),
                new Vector3(1.620f, 0.05f, 2.325f),
                new Vector3(2.490f, 0.05f, 2.531f),
                new Vector3(3.564f, 0.05f, 3.093f),
                new Vector3(2.259f, 0.05f, 2.945f),
                new Vector3(2.678f, 0.05f, 3.384f),
                new Vector3(4.212f, 0.05f, 2.881f),
                new Vector3(3.612f, 0.05f, 2.210f),
                new Vector3(3.079f, 0.05f, 2.014f),
                new Vector3(2.124f, 0.05f, 1.347f),
                new Vector3(4.200f, 0.05f, 3.406f),
                new Vector3(2.671f, 0.05f, 3.800f),
                new Vector3(2.953f, 0.05f, 2.272f),
                new Vector3(4.11f, 0.05f, 1.940f),
                new Vector3(3.557f, 0.05f, 2.338f),
                new Vector3(2.639f, 0.05f, 0.447f),
                new Vector3(2.877f, 0.05f, 3.144f),
                new Vector3(4.004f, 0.05f, 1.468f),
                new Vector3(4.404f, 0.05f, 0.780f),
                new Vector3(3.786f, 0.05f, 3.551f),
                new Vector3(4.375f, 0.05f, 2.628f),
                new Vector3(3.505f, 0.05f, 3.934f),
                new Vector3(4.897f, 0.05f, 2.482f),
                new Vector3(1.324f, 0.05f, 1.068f),
                new Vector3(3.555f, 0.05f, 1.571f),
                new Vector3(1.187f, 0.05f, 2.045f),
                new Vector3(3.176f, 0.05f, 0.429f),
                new Vector3(3.772f, 0.05f, 2.723f),
                new Vector3(2.796f, 0.05f, 1.473f),
                new Vector3(3.433f, 0.05f, 1.184f),
                new Vector3(1.522f, 0.05f, 1.746f),
                new Vector3(2.26f, 0.05f, 2.196f),
                new Vector3(2.307f, 0.05f, 1.605f),
                new Vector3(2.993f, 0.05f, 0.059f),
                new Vector3(4.156f, 0.05f, 0.657f),
                new Vector3(4.333f, 0.05f, 2.137f),
                new Vector3(2.477f, 0.05f, 1.397f),
                new Vector3(2.963f, 0.05f, 1.982f),
                new Vector3(4.100f, 0.05f, 1.222f),
                new Vector3(1.550f, 0.05f, 1.008f),
                new Vector3(4.513f, 0.05f, 3.300f),
                new Vector3(3.376f, 0.05f, 2.626f),
                new Vector3(2.911f, 0.05f, 0.276f),
                new Vector3(2.443f, 0.05f, 2.956f),
                new Vector3(3.970f, 0.05f, 0.579f),
                new Vector3(2.280f, 0.05f, 0.319f),
                new Vector3(2.115f, 0.05f, 0.083f),
                new Vector3(4.593f, 0.05f, 3.096f),
                new Vector3(2.121f, 0.05f, 0.646f),
                new Vector3(2.873f, 0.05f, 3.479f),
                new Vector3(4.361f, 0.05f, 1.291f),
                new Vector3(3.020f, 0.05f, 1.188f),
                new Vector3(2.602f, 0.05f, 3.641f),
                new Vector3(3.261f, 0.05f, 1.431f),
                new Vector3(2.668f, 0.05f, 3.232f),
                new Vector3(1.510f, 0.05f, 3.233f),
                new Vector3(1.143f, 0.05f, 1.284f),
                new Vector3(3.561f, 0.05f, 1.707f),
                new Vector3(3.949f, 0.05f, 0.33f),
                new Vector3(4.551f, 0.05f, 2.898f),
                new Vector3(1.578f, 0.05f, 1.339f),
                new Vector3(3.650f, 0.05f, 0.934f),
                new Vector3(2.518f, 0.05f, 1.681f),
                new Vector3(4.257f, 0.05f, 2.714f),
                new Vector3(3.975f, 0.05f, 3.114f),
                new Vector3(4.656f, 0.05f, 1.824f),
                new Vector3(1.701f, 0.05f, 1.853f),
                new Vector3(3.358f, 0.05f, 0.152f),
                new Vector3(2.011f, 0.05f, 0.929f),
                new Vector3(4.116f, 0.05f, 0.914f),
                new Vector3(4.555f, 0.05f, 2.522f),
                new Vector3(1.835f, 0.05f, 0.761f),
                new Vector3(2.179f, 0.05f, 3.558f),
                new Vector3(1.275f, 0.05f, 2.781f),
                new Vector3(1.769f, 0.05f, 1.552f),
                new Vector3(4.094f, 0.05f, 0.502f),
                new Vector3(2.126f, 0.05f, 1.719f),
            };
        }
    }
}
