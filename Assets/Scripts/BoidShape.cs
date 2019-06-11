namespace Tengio
{
    using System;
    using System.Collections;
    using UnityEngine;

    public class BoidShape : MonoBehaviour
    {
        [SerializeField]
        private Material material;

        private Mesh mesh;
        private bool isBreathing = false;
        private float breathingValue = 0f;
        private Coroutine morphCoroutine;
        private Coroutine breathCoroutine;

        private const float Size = 0.1f;

        private const float SphereAValue = 1f;
        private const float SphereBValue = 1f;
        private const float SphereCValue = 1f;
        private const float SphereDValue = 1f;

        private const float CubeAValue = 1.7320508075688773f; // 1f + Mathf.Sqrt(3f) - 1f
        private const float CubeBValue = 1f;
        private const float CubeCValue = 1.414213562373095f; // 1f + Mathf.Sqrt(2f) - 1f
        private const float CubeDValue = 1.224744871391589f; //  1f + Mathf.Sqrt(3f / 2f) - 1f

        private const float SpikeyAValue = 1.2f;
        private const float SpikeyBValue = 1.2f;
        private const float SpikeyCValue = 0.4f;
        private const float SpikeyDValue = 0.4f;

        public Quaternion Rotation
        {
            get => transform.rotation;
            set => transform.rotation = value;
        }

        private void Awake()
        {
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = material;
            mesh = new Mesh();
            SetAsSpikey();
        }

        private void Start()
        {
            mesh.triangles = meshTriangles;
            gameObject.AddComponent<MeshFilter>().mesh = mesh;
            //mesh.RecalculateNormals();
            //mesh.RecalculateTangents();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Resize(ref Vector3[] vertices, float newSize)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] *= newSize;
            }
        }

        private float GetVerticesRatio(float from, float to, float morphValue)
        {
            return from + morphValue * (to - from);
        }

        /// <summary>
        ///  Lerp the Shape form a sphere to a cube. lerpValue = 0 is a sphere and lerpValue = 1 is a cube.
        /// </summary>
        /// <param name="morphValue"></param>
        private Vector3[] GetSphereToCubeLerpedVertices(float morphValue)
        {
            Vector3[] vertices = baseSphereVertices.Clone() as Vector3[];
            Resize(ref vertices, Size);

            // Corners => Mathf.Sqrt(3f)
            for (int i = 0; i < groupA.Length; i++)
            {
                vertices[groupA[i]] *= GetVerticesRatio(SphereAValue, CubeAValue, morphValue);
            }

            //// Faces centers => 1f
            /// Nothing to do here.

            // Edges centers => Mathf.Sqrt(2f)
            for (int i = 0; i < groupC.Length; i++)
            {
                vertices[groupC[i]] *= GetVerticesRatio(SphereCValue, CubeCValue, morphValue);
            }

            // Quarter centers => Mathf.Sqrt(3f / 2f)
            for (int i = 0; i < groupD.Length; i++)
            {
                vertices[groupD[i]] *= GetVerticesRatio(SphereDValue, CubeDValue, morphValue);
            }

            return vertices;
        }

        /// <summary>
        ///  Lerp the Shape form a sphere to a spikey shape. lerpValue = 0 is a sphere and lerpValue = 1 is a spikey shape.
        /// </summary>
        /// <param name="morphValue"></param>
        private Vector3[] GetSphereToSpikeyLerpedVertices(float morphValue)
        {
            Vector3[] vertices = baseSphereVertices.Clone() as Vector3[];
            Resize(ref vertices, Size);

            // Corners
            for (int i = 0; i < groupA.Length; i++)
            {
                vertices[groupA[i]] *= GetVerticesRatio(SphereAValue, SpikeyAValue, morphValue);
            }

            //// Faces centers
            for (int i = 0; i < groupB.Length; i++)
            {
                vertices[groupB[i]] *= GetVerticesRatio(SphereBValue, SpikeyBValue, morphValue);
            }

            // Edges centers
            for (int i = 0; i < groupC.Length; i++)
            {
                vertices[groupC[i]] *= GetVerticesRatio(SphereCValue, SpikeyCValue, morphValue);
            }

            // Quarter centers
            for (int i = 0; i < groupD.Length; i++)
            {
                vertices[groupD[i]] *= GetVerticesRatio(SphereDValue, SpikeyDValue, morphValue);
            }

            return vertices;
        }


        /// <summary>
        ///  Lerp the Shape form a cube to a spikey shape. lerpValue = 0 is a cube and lerpValue = 1 is a spikey shape.
        /// </summary>
        /// <param name="morphValue"></param>
        private Vector3[] GetCubeToSpikeyLerpedVertices(float morphValue)
        {
            Vector3[] vertices = baseSphereVertices.Clone() as Vector3[];
            Resize(ref vertices, Size);

            // Corners
            for (int i = 0; i < groupA.Length; i++)
            {
                vertices[groupA[i]] *= GetVerticesRatio(CubeAValue, SpikeyAValue, morphValue);
            }

            //// Faces centers
            for (int i = 0; i < groupB.Length; i++)
            {
                vertices[groupB[i]] *= GetVerticesRatio(CubeBValue, SpikeyBValue, morphValue);
            }

            // Edges centers
            for (int i = 0; i < groupC.Length; i++)
            {
                vertices[groupC[i]] *= GetVerticesRatio(CubeCValue, SpikeyCValue, morphValue);
            }

            // Quarter centers
            for (int i = 0; i < groupD.Length; i++)
            {
                vertices[groupD[i]] *= GetVerticesRatio(CubeDValue, SpikeyDValue, morphValue);
            }

            return vertices;
        }

        public void SetAsSphere()
        {
            mesh.vertices = GetSphereToCubeLerpedVertices(0f);
        }

        public void SetAsCube()
        {
            mesh.vertices = GetSphereToCubeLerpedVertices(1f);
        }

        public void SetAsSpikey()
        {
            mesh.vertices = GetSphereToSpikeyLerpedVertices(1f);
        }

        public void MorphSphereToCube(Action callback = null)
        {
            StopBreathing();
            if (morphCoroutine != null) StopCoroutine(morphCoroutine);
            morphCoroutine = StartCoroutine(LerpSphereToCube(1.5f, callback));
        }

        public void MorphCubeToSphere(Action callback = null)
        {
            float initialMorphValue = 1f;
            if (isBreathing)
            {
                StopBreathing();
                initialMorphValue = breathingValue;
            }

            if (morphCoroutine != null) StopCoroutine(morphCoroutine);
            morphCoroutine = StartCoroutine(LerpCubeToSphere(1.5f, callback, initialMorphValue));
        }

        public void MorphSpikeyToSphere(Action callback = null)
        {
            StopBreathing();
            if (morphCoroutine != null) StopCoroutine(morphCoroutine);
            morphCoroutine = StartCoroutine(LerpSpikeyToSphere(1.5f, callback));
        }

        public void MorphSpikeyToCube(Action callback = null)
        {
            StopBreathing();
            if (morphCoroutine != null) StopCoroutine(morphCoroutine);
            morphCoroutine = StartCoroutine(LerpSpikeyToCube(1.5f, callback));
        }

        public void StartBreathing()
        {
            StopBreathing();
            isBreathing = true;
            breathCoroutine = StartCoroutine(LerpSphereToCubeLooping(1f, 0.7f, 1f));
        }

        public void StopBreathing()
        {
            isBreathing = false;
            if (breathCoroutine != null) StopCoroutine(breathCoroutine);
        }

        private IEnumerator LerpSphereToCube(float duration, Action callback = null)
        {
            float morphValue = 0f;
            while (morphValue < 1f)
            {
                mesh.vertices = GetSphereToCubeLerpedVertices(Mathf.Clamp01(morphValue));
                morphValue += Time.deltaTime / duration;
                yield return null;
            }
            mesh.vertices = GetSphereToCubeLerpedVertices(1f);
            callback?.Invoke();
        }

        private IEnumerator LerpCubeToSphere(float duration, Action callback = null, float initialMorphValue = 1f)
        {
            float morphValue = initialMorphValue;
            while (morphValue > 0f)
            {
                mesh.vertices = GetSphereToCubeLerpedVertices(Mathf.Clamp01(morphValue));
                morphValue -= Time.deltaTime / duration;
                yield return null;
            }
            mesh.vertices = GetSphereToCubeLerpedVertices(0f);
            callback?.Invoke();
        }

        private IEnumerator LerpSpikeyToSphere(float duration, Action callback = null)
        {
            float morphValue = 1f;
            while (morphValue > 0f)
            {
                mesh.vertices = GetSphereToSpikeyLerpedVertices(Mathf.Clamp01(morphValue));
                morphValue -= Time.deltaTime / duration;
                yield return null;
            }
            mesh.vertices = GetSphereToSpikeyLerpedVertices(0f);
            callback?.Invoke();
        }

        private IEnumerator LerpSpikeyToCube(float duration, Action callback = null)
        {
            float morphValue = 1f;
            while (morphValue > 0f)
            {
                mesh.vertices = GetCubeToSpikeyLerpedVertices(Mathf.Clamp01(morphValue));
                morphValue -= Time.deltaTime / duration;
                yield return null;
            }
            mesh.vertices = GetCubeToSpikeyLerpedVertices(0f);
            callback?.Invoke();
        }

        private IEnumerator LerpSphereToCubeLooping(float loopDuration, float from, float to)
        {
            bool lerpIncrease = true;
            float morphValue = to;

            while (true)
            {
                Vector3[] vertices = GetSphereToCubeLerpedVertices(morphValue);
                mesh.vertices = vertices;

                morphValue += (lerpIncrease ? 1 : -1) * Time.deltaTime / (loopDuration / (to - from));
                if (morphValue > to)
                {
                    morphValue = to;
                    lerpIncrease = false;
                }
                else if (morphValue < from)
                {
                    morphValue = from;
                    lerpIncrease = true;
                }
                breathingValue = morphValue;
                yield return null;
            }
        }

        // Corners
        private static readonly int[] groupA = new int[8] { 0, 2, 4, 6, 16, 18, 20, 22 };

        // Faces centers
        private static readonly int[] groupB = new int[6] { 9, 11, 13, 15, 44, 49 };

        // Edges centers
        private static readonly int[] groupC = new int[12] { 1, 3, 5, 7, 8, 10, 12, 14, 17, 19, 21, 23 };

        // Quarter centers
        private static readonly int[] groupD = new int[24] { 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 45, 46, 47, 48 };

        private static readonly Vector3[] baseSphereVertices = new Vector3[50] {
            new Vector3(-0.5773503f, -0.5773503f, -0.5773503f),
            new Vector3(0f, -0.7071068f, -0.7071068f),
            new Vector3(0.5773503f, -0.5773503f, -0.5773503f),
            new Vector3(0.7071068f, -0.7071068f, 0f),
            new Vector3(0.5773503f, -0.5773503f, 0.5773503f),
            new Vector3(0f, -0.7071068f, 0.7071068f),
            new Vector3(-0.5773503f, -0.5773503f, 0.5773503f),
            new Vector3(-0.7071068f, -0.7071068f, 0f),
            new Vector3(-0.7071068f, 0f, -0.7071068f),
            new Vector3(0f, 0f, -1f),
            new Vector3(0.7071068f, 0f, -0.7071068f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0.7071068f, 0f, 0.7071068f),
            new Vector3(0f, 0f, 1f),
            new Vector3(-0.7071068f, 0f, 0.7071068f),
            new Vector3(-1f, 0f, 0f),
            new Vector3(-0.5773503f, 0.5773503f, -0.5773503f),
            new Vector3(0f, 0.7071068f, -0.7071068f),
            new Vector3(0.5773503f, 0.5773503f, -0.5773503f),
            new Vector3(0.7071068f, 0.7071068f, 0f),
            new Vector3(0.5773503f, 0.5773503f, 0.5773503f),
            new Vector3(0f, 0.7071068f, 0.7071068f),
            new Vector3(-0.5773503f, 0.5773503f, 0.5773503f),
            new Vector3(-0.7071068f, 0.7071068f, 0f),
            new Vector3(-0.4082483f, -0.4082483f, -0.8164966f),
            new Vector3(0.4082483f, -0.4082483f, -0.8164966f),
            new Vector3(0.8164966f, -0.4082483f, -0.4082483f),
            new Vector3(0.8164966f, -0.4082483f, 0.4082483f),
            new Vector3(0.4082483f, -0.4082483f, 0.8164966f),
            new Vector3(-0.4082483f, -0.4082483f, 0.8164966f),
            new Vector3(-0.8164966f, -0.4082483f, 0.4082483f),
            new Vector3(-0.8164966f, -0.4082483f, -0.4082483f),
            new Vector3(-0.4082483f, 0.4082483f, -0.8164966f),
            new Vector3(0.4082483f, 0.4082483f, -0.8164966f),
            new Vector3(0.8164966f, 0.4082483f, -0.4082483f),
            new Vector3(0.8164966f, 0.4082483f, 0.4082483f),
            new Vector3(0.4082483f, 0.4082483f, 0.8164966f),
            new Vector3(-0.4082483f, 0.4082483f, 0.8164966f),
            new Vector3(-0.8164966f, 0.4082483f, 0.4082483f),
            new Vector3(-0.8164966f, 0.4082483f, -0.4082483f),
            new Vector3(-0.4082483f, 0.8164966f, -0.4082483f),
            new Vector3(0.4082483f, 0.8164966f, -0.4082483f),
            new Vector3(0.4082483f, 0.8164966f, 0.4082483f),
            new Vector3(-0.4082483f, 0.8164966f, 0.4082483f),
            new Vector3(0f, 1f, 0f),
            new Vector3(-0.4082483f, -0.8164966f, -0.4082483f),
            new Vector3(0.4082483f, -0.8164966f, -0.4082483f),
            new Vector3(0.4082483f, -0.8164966f, 0.4082483f),
            new Vector3(-0.4082483f, -0.8164966f, 0.4082483f),
            new Vector3(0f, -1f, 0f),
        };

        private static readonly int[] meshTriangles = new int[96 * 3] {
            0, 24, 1,    // 0
            1, 25, 2,    // 1
            2, 26, 3,    // 2
            3, 27, 4,    // 3
            4, 28, 5,    // 4
            5, 29, 6,    // 5
            6, 30, 7,    // 6
            7, 31, 0,    // 7
            0, 8, 24,    // 8
            1, 24, 9,    // 9
            1, 9, 25,    // 10
            2, 25, 10,   // 11
            2, 10, 26,   // 12
            3, 26, 11,   // 13
            3, 11, 27,   // 14
            4, 27, 12,   // 15
            4, 12, 28,   // 16
            5, 28, 13,   // 17
            5, 13, 29,   // 18
            6, 29, 14,   // 19
            6, 14, 30,   // 20
            7, 30, 15,   // 21
            7, 15, 31,   // 22
            8, 0, 31,    // 23
            8, 9, 24,    // 24
            9, 10, 25,   // 25
            10, 11, 26,  // 26
            11, 12, 27,  // 27
            12, 13, 28,  // 28
            13, 14, 29,  // 29
            14, 15, 30,  // 30
            15, 8, 31,   // 31
            8, 32, 9,    // 32
            9, 33, 10,   // 33
            10, 34, 11,  // 34
            11, 35, 12,  // 35
            12, 36, 13,  // 36
            13, 37, 14,  // 37
            14, 38, 15,  // 38
            8, 15, 39,   // 39
            8, 16, 32,   // 40
            9, 32, 17,   // 41
            9, 17, 33,   // 42
            10, 33, 18,  // 43
            10, 18, 34,  // 44
            11, 34, 19,  // 45
            11, 19, 35,  // 46
            12, 35, 20,  // 47
            12, 20, 36,  // 48
            13, 36, 21,  // 49
            13, 21, 37,  // 50
            14, 37, 22,  // 51
            14, 22, 38,  // 52
            15, 38, 23,  // 53
            15, 23, 39,  // 54
            8, 39, 16,   // 55
            16, 17, 32,  // 56
            17, 18, 33,  // 57
            18, 19, 34,  // 58
            19, 20, 35,  // 59
            20, 21, 36,  // 60
            21, 22, 37,  // 61
            22, 23, 38,  // 62
            23, 16, 39,  // 63
            16, 40, 17,  // 64
            17, 41, 18,  // 65
            18, 41, 19,  // 66
            19, 42, 20,  // 67
            20, 42, 21,  // 68
            21, 43, 22,  // 69
            22, 43, 23,  // 70
            23, 40, 16,  // 71
            44, 17, 40,  // 72
            44, 41, 17,  // 73
            44, 19, 41,  // 74
            44, 42, 19,  // 75
            44, 21, 42,  // 76
            44, 43, 21,  // 77
            44, 23, 43,  // 78
            44, 40, 23,  // 79
            1, 45, 0,    // 80
            2, 46, 1,    // 81
            3, 46, 2,    // 82
            4, 47, 3,    // 83
            5, 47, 4,    // 84
            6, 48, 5,    // 85
            7, 48, 6,    // 86
            0, 45, 7,    // 87
            49, 45, 1,   // 88
            49, 1, 46,   // 89
            49, 46, 3,   // 90
            49, 3, 47,   // 91
            49, 47, 5,   // 92
            49, 5, 48,   // 93
            49, 48, 7,   // 94
            49, 7, 45,   // 95
        };
    }
}


