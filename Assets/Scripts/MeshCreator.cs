namespace Tengio
{
    using System.Collections;
    using UnityEngine;

    public class MeshCreator : MonoBehaviour
    {
        [SerializeField]
        private Material material;

        [SerializeField]
        [Range(0.0f, 3.0f)]
        public float width = 1.0f;

        [SerializeField]
        [Range(0.0f, 3.0f)]
        public float height = 1.0f;

        [SerializeField]
        [Range(0.0f, 3.0f)]
        public float depth = 1.0f;

        [SerializeField]
        [Range(0.0f, 3.0f)]
        public float groupALength = 1.0f;

        [SerializeField]
        [Range(0.0f, 3.0f)]
        public float groupBLength = 1.0f;

        [SerializeField]
        [Range(0.0f, 3.0f)]
        public float groupCLength = 1.0f;

        [SerializeField]
        [Range(0.0f, 3.0f)]
        public float groupDLength = 1.0f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        public float sphereToCubeLerp = 0.0f;

        private Mesh mesh;
        private Coroutine lerpSphereToCubeCoroutine;


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


        // Corners
        private static int[] groupA = new int[8] { 0, 2, 4, 6, 16, 18, 20, 22 };

        // Faces centers
        private static int[] groupB = new int[6] { 9, 11, 13, 15, 44, 49 };

        // Edges centers
        private static int[] groupC = new int[12] { 1, 3, 5, 7, 8, 10, 12, 14, 17, 19, 21, 23 };

        // Quarter centers
        private static int[] groupD = new int[24] { 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 45, 46, 47, 48 };

        private static Vector3[] baseSphereVertices = new Vector3[50] {
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


        private static Vector3[] baseCubeVertices = new Vector3[50] {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0f, -0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0f),

            new Vector3(-0.5f, 0f, -0.5f),
            new Vector3(0f, 0f, -0.5f),
            new Vector3(0.5f, 0f, -0.5f),
            new Vector3(0.5f, 0f, 0f),
            new Vector3(0.5f, 0f, 0.5f),
            new Vector3(0f, 0f, 0.5f),
            new Vector3(-0.5f, 0f, 0.5f),
            new Vector3(-0.5f, 0f, 0f),

            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0f),

            new Vector3(-0.25f, -0.25f, -0.5f),
            new Vector3(0.25f, -0.25f, -0.5f),
            new Vector3(0.5f, -0.25f, -0.25f),
            new Vector3(0.5f, -0.25f, 0.25f),
            new Vector3(0.25f, -0.25f, 0.5f),
            new Vector3(-0.25f, -0.25f, 0.5f),
            new Vector3(-0.5f, -0.25f, 0.25f),
            new Vector3(-0.5f, -0.25f, -0.25f),

            new Vector3(-0.25f, 0.25f, -0.5f),
            new Vector3(0.25f, 0.25f, -0.5f),
            new Vector3(0.5f, 0.25f, -0.25f),
            new Vector3(0.5f, 0.25f, 0.25f),
            new Vector3(0.25f, 0.25f, 0.5f),
            new Vector3(-0.25f, 0.25f, 0.5f),
            new Vector3(-0.5f, 0.25f, 0.25f),
            new Vector3(-0.5f, 0.25f, -0.25f),

            new Vector3(-0.25f, 0.5f, -0.25f),
            new Vector3(0.25f, 0.5f, -0.25f),
            new Vector3(0.25f, 0.5f, 0.25f),
            new Vector3(-0.25f, 0.5f, 0.25f),
            new Vector3(0f, 0.5f, 0f),


            new Vector3(-0.25f, -0.5f, -0.25f),
            new Vector3(0.25f, -0.5f, -0.25f),
            new Vector3(0.25f, -0.5f, 0.25f),
            new Vector3(-0.25f, -0.5f, 0.25f),
            new Vector3(0f, -0.5f, 0f)
        };

        private void Start()
        {
            //GameObject gameObject = new GameObject();

            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = material;
            mesh = new Mesh();
            mesh.vertices = baseSphereVertices;
            mesh.triangles = getTriangles();
            gameObject.AddComponent<MeshFilter>().mesh = mesh;
            //lerpSphereToCubeCoroutine = StartCoroutine(LerpSphereToCube(1.0f));

            //mesh.RecalculateNormals();
            //mesh.RecalculateTangents();
        }

        // To test shapes
        private void Update()
        {
            //Vector3[] vertices = baseSphereVertices.Clone() as Vector3[];
            //for (int i = 0; i < groupA.Length; i++)
            //{
            //    vertices[groupA[i]] *= groupALength;
            //}

            //for (int i = 0; i < groupB.Length; i++)
            //{
            //    vertices[groupB[i]] *= groupBLength;
            //}

            //for (int i = 0; i < groupC.Length; i++)
            //{
            //    vertices[groupC[i]] *= groupCLength;
            //}

            //for (int i = 0; i < groupD.Length; i++)
            //{
            //    vertices[groupD[i]] *= groupDLength;
            //}


            //Vector3[] vertices = sphereToCube(sphereToCubeLerp);

            Vector3[] vertices = GetCubeToSpikeyLerpedVertices(sphereToCubeLerp);

            Resize(ref vertices, 0.1f);
            mesh.vertices = vertices;

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


        private void OnDisable()
        {
            if (lerpSphereToCubeCoroutine != null)
            {
                StopCoroutine(lerpSphereToCubeCoroutine);
            }
        }


        /// <summary>
        ///  Lerp the Shape form a cube to a spikey shape. lerpValue = 0 is a cube and lerpValue = 1 is a spikey shape.
        /// </summary>
        /// <param name="morphValue"></param>
        private Vector3[] GetCubeToSpikeyLerpedVertices(float morphValue)
        {
            Vector3[] vertices = baseSphereVertices.Clone() as Vector3[];

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



        //private void Update()
        //{
        //    populateVertices(mesh);
        //    //populateTriangles(mesh);
        //}

        private void populateVertices(Mesh mesh)
        {
            Vector3[] vertices = baseSphereVertices;

            //vertices[0] = new Vector3(width * -0.5f, height * -0.5f, depth * -0.5f);
            //vertices[1] = new Vector3(0f, height * -0.5f, depth * -0.5f);
            //vertices[2] = new Vector3(width * 0.5f, height * -0.5f, depth * -0.5f);
            //vertices[3] = new Vector3(width * 0.5f, height * -0.5f, 0f);
            //vertices[4] = new Vector3(width * 0.5f, height * -0.5f, depth * 0.5f);
            //vertices[5] = new Vector3(0f, height * -0.5f, depth * 0.5f);
            //vertices[6] = new Vector3(width * -0.5f, height * -0.5f, depth * 0.5f);
            //vertices[7] = new Vector3(width * -0.5f, height * -0.5f, 0f);

            //vertices[8] = new Vector3(width * -0.5f, 0f, depth * -0.5f);
            //vertices[9] = new Vector3(0f, 0f, depth * -0.5f);
            //vertices[10] = new Vector3(width * 0.5f, 0f, depth * -0.5f);
            //vertices[11] = new Vector3(width * 0.5f, 0f, 0f);
            //vertices[12] = new Vector3(width * 0.5f, 0f, depth * 0.5f);
            //vertices[13] = new Vector3(0f, 0f, depth * 0.5f);
            //vertices[14] = new Vector3(width * -0.5f, 0f, depth * 0.5f);
            //vertices[15] = new Vector3(width * -0.5f, 0f, 0f);

            //vertices[16] = new Vector3(width * -0.5f, height * 0.5f, depth * -0.5f);
            //vertices[17] = new Vector3(0f, height * 0.5f, depth * -0.5f);
            //vertices[18] = new Vector3(width * 0.5f, height * 0.5f, depth * -0.5f);
            //vertices[19] = new Vector3(width * 0.5f, height * 0.5f, 0f);
            //vertices[20] = new Vector3(width * 0.5f, height * 0.5f, depth * 0.5f);
            //vertices[21] = new Vector3(0f, height * 0.5f, depth * 0.5f);
            //vertices[22] = new Vector3(width * -0.5f, height * 0.5f, depth * 0.5f);
            //vertices[23] = new Vector3(width * -0.5f, height * 0.5f, 0f);

            //vertices[24] = new Vector3(width * -0.25f, height * -0.25f, depth * -0.5f);
            //vertices[25] = new Vector3(width * 0.25f, height * -0.25f, depth * -0.5f);
            //vertices[26] = new Vector3(width * 0.5f, height * -0.25f, depth * -0.25f);
            //vertices[27] = new Vector3(width * 0.5f, height * -0.25f, depth * 0.25f);
            //vertices[28] = new Vector3(width * 0.25f, height * -0.25f, depth * 0.5f);
            //vertices[29] = new Vector3(width * -0.25f, height * -0.25f, depth * 0.5f);
            //vertices[30] = new Vector3(width * -0.5f, height * -0.25f, depth * 0.25f);
            //vertices[31] = new Vector3(width * -0.5f, height * -0.25f, depth * -0.25f);

            //vertices[32] = new Vector3(width * -0.25f, height * 0.25f, depth * -0.5f);
            //vertices[33] = new Vector3(width * 0.25f, height * 0.25f, depth * -0.5f);
            //vertices[34] = new Vector3(width * 0.5f, height * 0.25f, depth * -0.25f);
            //vertices[35] = new Vector3(width * 0.5f, height * 0.25f, depth * 0.25f);
            //vertices[36] = new Vector3(width * 0.25f, height * 0.25f, depth * 0.5f);
            //vertices[37] = new Vector3(width * -0.25f, height * 0.25f, depth * 0.5f);
            //vertices[38] = new Vector3(width * -0.5f, height * 0.25f, depth * 0.25f);
            //vertices[39] = new Vector3(width * -0.5f, height * 0.25f, depth * -0.25f);

            //vertices[40] = new Vector3(width * -0.25f, height * 0.5f, depth * -0.25f);
            //vertices[41] = new Vector3(width * 0.25f, height * 0.5f, depth * -0.25f);
            //vertices[42] = new Vector3(width * 0.25f, height * 0.5f, depth * 0.25f);
            //vertices[43] = new Vector3(width * -0.25f, height * 0.5f, depth * 0.25f);
            //vertices[44] = new Vector3(0f, height * 0.5f, 0f);


            //vertices[45] = new Vector3(width * -0.25f, height * -0.5f, depth * -0.25f);
            //vertices[46] = new Vector3(width * 0.25f, height * -0.5f, depth * -0.25f);
            //vertices[47] = new Vector3(width * 0.25f, height * -0.5f, depth * 0.25f);
            //vertices[48] = new Vector3(width * -0.25f, height * -0.5f, depth * 0.25f);
            //vertices[49] = new Vector3(0f, height * -0.5f, 0f);

            //for (int i = 0; i < groupA.Length; i++)
            //{
            //    vertices[groupA[i]] *= groupALength;
            //}

            //for (int i = 0; i < groupB.Length; i++)
            //{
            //    vertices[groupB[i]] *= groupBLength;
            //}

            //for (int i = 0; i < groupC.Length; i++)
            //{
            //    vertices[groupC[i]] *= groupCLength;
            //}

            //for (int i = 0; i < groupD.Length; i++)
            //{
            //    vertices[groupD[i]] *= groupDLength;
            //}

            //toSphere(ref vertices);
            mesh.vertices = vertices;
        }

        private void toSphere(ref Vector3[] vertices)
        {
            string plop = "";
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] /= vertices[i].magnitude;
                plop += "new Vector3(" + vertices[i].x + "f, " + vertices[i].y + "f, " + vertices[i].z + "f),\n";
            }
            Debug.Log(plop);
        }


        private Vector3[] sphereToCube(float lerpValue)
        {
            Vector3[] vertices = baseSphereVertices.Clone() as Vector3[];
            // To square
            // Corners => Mathf.Sqrt(3.0f)
            for (int i = 0; i < groupA.Length; i++)
            {
                vertices[groupA[i]] *= 1.0f + lerpValue * (Mathf.Sqrt(3.0f) - 1.0f);
            }

            //// Faces centers => 1.0f
            /// Nothing to do here.

            // Edges centers => Mathf.Sqrt(2.0f)
            for (int i = 0; i < groupC.Length; i++)
            {
                vertices[groupC[i]] *= 1.0f + lerpValue * (Mathf.Sqrt(2.0f) - 1.0f);
            }

            // Quarter centers => Mathf.Sqrt(3.0f / 2.0f)
            for (int i = 0; i < groupD.Length; i++)
            {
                vertices[groupD[i]] *= 1.0f + lerpValue * (Mathf.Sqrt(3.0f / 2.0f) - 1.0f);
            }

            return vertices;
        }



        private IEnumerator LerpSphereToCube(float loopDuration)
        {
            float t = 0F; // Always between 0 and 1;
            bool lerpIncrease = true;

            while (true)
            {
                Vector3[] vertices = sphereToCube(t);
                Resize(ref vertices, 0.1f);
                mesh.vertices = vertices;

                t += (lerpIncrease ? 1 : -1) * Time.deltaTime * 2 / loopDuration;
                if (t > 1F)
                {
                    t = 1.0f;
                    lerpIncrease = false;
                }
                else if (t < 0)
                {
                    t = 0.0f;
                    lerpIncrease = true;
                }
                yield return null;
            }
        }

        private int[] getTriangles()
        {
            int[] triangles = new int[96 * 3];

            // 0
            triangles[0] = 0;
            triangles[1] = 24;
            triangles[2] = 1;

            // 1
            triangles[3] = 1;
            triangles[4] = 25;
            triangles[5] = 2;

            // 2
            triangles[6] = 2;
            triangles[7] = 26;
            triangles[8] = 3;

            // 3
            triangles[9] = 3;
            triangles[10] = 27;
            triangles[11] = 4;

            // 4
            triangles[12] = 4;
            triangles[13] = 28;
            triangles[14] = 5;

            // 5
            triangles[15] = 5;
            triangles[16] = 29;
            triangles[17] = 6;

            // 6
            triangles[18] = 6;
            triangles[19] = 30;
            triangles[20] = 7;

            // 7
            triangles[21] = 7;
            triangles[22] = 31;
            triangles[23] = 0;

            // 8
            triangles[24] = 0;
            triangles[25] = 8;
            triangles[26] = 24;

            // 9
            triangles[27] = 1;
            triangles[29] = 9;
            triangles[28] = 24;

            // 10
            triangles[30] = 1;
            triangles[31] = 9;
            triangles[32] = 25;

            // 11
            triangles[33] = 2;
            triangles[35] = 10;
            triangles[34] = 25;

            // 12
            triangles[36] = 2;
            triangles[37] = 10;
            triangles[38] = 26;

            // 13
            triangles[39] = 3;
            triangles[41] = 11;
            triangles[40] = 26;

            // 14
            triangles[42] = 3;
            triangles[43] = 11;
            triangles[44] = 27;

            // 15
            triangles[45] = 4;
            triangles[47] = 12;
            triangles[46] = 27;

            // 16
            triangles[48] = 4;
            triangles[49] = 12;
            triangles[50] = 28;

            // 17
            triangles[51] = 5;
            triangles[53] = 13;
            triangles[52] = 28;

            // 18
            triangles[54] = 5;
            triangles[55] = 13;
            triangles[56] = 29;

            // 19
            triangles[57] = 6;
            triangles[59] = 14;
            triangles[58] = 29;

            // 20
            triangles[60] = 6;
            triangles[61] = 14;
            triangles[62] = 30;

            // 21
            triangles[63] = 7;
            triangles[65] = 15;
            triangles[64] = 30;

            // 22
            triangles[66] = 7;
            triangles[67] = 15;
            triangles[68] = 31;

            // 23
            triangles[69] = 8;
            triangles[71] = 31;
            triangles[70] = 0;

            // 24
            triangles[72] = 8;
            triangles[73] = 9;
            triangles[74] = 24;

            // 25
            triangles[75] = 9;
            triangles[76] = 10;
            triangles[77] = 25;

            // 26
            triangles[78] = 10;
            triangles[79] = 11;
            triangles[80] = 26;

            // 27
            triangles[81] = 11;
            triangles[82] = 12;
            triangles[83] = 27;

            // 28
            triangles[84] = 12;
            triangles[85] = 13;
            triangles[86] = 28;

            // 29
            triangles[87] = 13;
            triangles[88] = 14;
            triangles[89] = 29;

            // 30
            triangles[90] = 14;
            triangles[91] = 15;
            triangles[92] = 30;

            // 31
            triangles[93] = 15;
            triangles[94] = 8;
            triangles[95] = 31;

            // 32
            triangles[96] = 8;
            triangles[97] = 32;
            triangles[98] = 9;

            // 33
            triangles[99] = 9;
            triangles[100] = 33;
            triangles[101] = 10;

            // 34
            triangles[102] = 10;
            triangles[103] = 34;
            triangles[104] = 11;

            // 35
            triangles[105] = 11;
            triangles[106] = 35;
            triangles[107] = 12;

            // 36
            triangles[108] = 12;
            triangles[109] = 36;
            triangles[110] = 13;

            // 37
            triangles[111] = 13;
            triangles[112] = 37;
            triangles[113] = 14;

            // 38
            triangles[114] = 14;
            triangles[115] = 38;
            triangles[116] = 15;

            // 39
            triangles[117] = 8;
            triangles[118] = 15;
            triangles[119] = 39;

            // 40
            triangles[120] = 8;
            triangles[121] = 16;
            triangles[122] = 32;

            // 41
            triangles[123] = 9;
            triangles[124] = 32;
            triangles[125] = 17;

            // 42
            triangles[126] = 9;
            triangles[127] = 17;
            triangles[128] = 33;

            // 43
            triangles[129] = 10;
            triangles[130] = 33;
            triangles[131] = 18;

            // 44
            triangles[132] = 10;
            triangles[133] = 18;
            triangles[134] = 34;

            // 45
            triangles[135] = 11;
            triangles[136] = 34;
            triangles[137] = 19;

            // 46
            triangles[138] = 11;
            triangles[139] = 19;
            triangles[140] = 35;

            // 47
            triangles[141] = 12;
            triangles[142] = 35;
            triangles[143] = 20;

            // 48
            triangles[144] = 12;
            triangles[145] = 20;
            triangles[146] = 36;

            // 49
            triangles[147] = 13;
            triangles[148] = 36;
            triangles[149] = 21;

            // 50
            triangles[150] = 13;
            triangles[151] = 21;
            triangles[152] = 37;

            // 51
            triangles[153] = 14;
            triangles[154] = 37;
            triangles[155] = 22;

            // 52
            triangles[156] = 14;
            triangles[157] = 22;
            triangles[158] = 38;

            // 53
            triangles[159] = 15;
            triangles[160] = 38;
            triangles[161] = 23;

            // 54
            triangles[162] = 15;
            triangles[163] = 23;
            triangles[164] = 39;

            // 55
            triangles[165] = 8;
            triangles[166] = 39;
            triangles[167] = 16;

            // 56
            triangles[168] = 16;
            triangles[169] = 17;
            triangles[170] = 32;

            // 57
            triangles[171] = 17;
            triangles[172] = 18;
            triangles[173] = 33;

            // 58
            triangles[174] = 18;
            triangles[175] = 19;
            triangles[176] = 34;

            // 59
            triangles[177] = 19;
            triangles[178] = 20;
            triangles[179] = 35;

            // 60
            triangles[180] = 20;
            triangles[181] = 21;
            triangles[182] = 36;

            // 61
            triangles[183] = 21;
            triangles[184] = 22;
            triangles[185] = 37;

            // 62
            triangles[186] = 22;
            triangles[187] = 23;
            triangles[188] = 38;

            // 63
            triangles[189] = 23;
            triangles[190] = 16;
            triangles[191] = 39;

            // 64
            triangles[192] = 16;
            triangles[193] = 40;
            triangles[194] = 17;

            // 65
            triangles[195] = 17;
            triangles[196] = 41;
            triangles[197] = 18;

            // 66
            triangles[198] = 18;
            triangles[199] = 41;
            triangles[200] = 19;

            // 67
            triangles[201] = 19;
            triangles[202] = 42;
            triangles[203] = 20;

            // 68
            triangles[204] = 20;
            triangles[205] = 42;
            triangles[206] = 21;

            // 69
            triangles[207] = 21;
            triangles[208] = 43;
            triangles[209] = 22;

            // 70
            triangles[210] = 22;
            triangles[211] = 43;
            triangles[212] = 23;

            // 71
            triangles[213] = 23;
            triangles[214] = 40;
            triangles[215] = 16;

            // 72
            triangles[216] = 44;
            triangles[217] = 17;
            triangles[218] = 40;

            // 73
            triangles[219] = 44;
            triangles[220] = 41;
            triangles[221] = 17;

            // 74
            triangles[222] = 44;
            triangles[223] = 19;
            triangles[224] = 41;

            // 75
            triangles[225] = 44;
            triangles[226] = 42;
            triangles[227] = 19;

            // 76
            triangles[228] = 44;
            triangles[229] = 21;
            triangles[230] = 42;

            // 77
            triangles[231] = 44;
            triangles[232] = 43;
            triangles[233] = 21;

            // 78
            triangles[234] = 44;
            triangles[235] = 23;
            triangles[236] = 43;

            // 79
            triangles[237] = 44;
            triangles[238] = 40;
            triangles[239] = 23;

            // 80
            triangles[240] = 1;
            triangles[241] = 45;
            triangles[242] = 0;

            // 81
            triangles[243] = 2;
            triangles[244] = 46;
            triangles[245] = 1;

            // 82
            triangles[246] = 3;
            triangles[247] = 46;
            triangles[248] = 2;

            // 83
            triangles[249] = 4;
            triangles[250] = 47;
            triangles[251] = 3;

            // 84
            triangles[252] = 5;
            triangles[253] = 47;
            triangles[254] = 4;

            // 85
            triangles[255] = 6;
            triangles[256] = 48;
            triangles[257] = 5;

            // 86
            triangles[258] = 7;
            triangles[259] = 48;
            triangles[260] = 6;

            // 87
            triangles[261] = 0;
            triangles[262] = 45;
            triangles[263] = 7;

            // 88
            triangles[264] = 49;
            triangles[265] = 45;
            triangles[266] = 1;

            // 89
            triangles[267] = 49;
            triangles[268] = 1;
            triangles[269] = 46;

            // 90
            triangles[270] = 49;
            triangles[271] = 46;
            triangles[272] = 3;

            // 91
            triangles[273] = 49;
            triangles[274] = 3;
            triangles[275] = 47;

            // 92
            triangles[276] = 49;
            triangles[277] = 47;
            triangles[278] = 5;

            // 93
            triangles[279] = 49;
            triangles[280] = 5;
            triangles[281] = 48;

            // 94
            triangles[282] = 49;
            triangles[283] = 48;
            triangles[284] = 7;

            // 95
            triangles[285] = 49;
            triangles[286] = 7;
            triangles[287] = 45;

            return triangles;
        }
    }
}
