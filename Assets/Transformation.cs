using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using System.IO;
using System.Globalization;
using UnityEditor;
using Random = UnityEngine.Random;


namespace allProgram
{
    public class Transformation : MonoBehaviour
    {
        float Tx, Ty, Tz, R0, R1, R2;

        public void Ransac(List<Vector3> srcPts, List<Vector3> tgtPts, GameObject point, Transform transform)
        {
            int iterations = 400;
            float[,] allPossibilites = new float[iterations, 7];

            for (int i = 0; i < iterations; i++)
            {
                // take 3 points randomly selected from first set
                int indexA = Random.Range(0, srcPts.Count);
                int indexB = Random.Range(0, srcPts.Count);
                int indexC = Random.Range(0, srcPts.Count);
                Vector3 pointA = srcPts[indexA];
                Vector3 pointB = srcPts[indexB];
                Vector3 pointC = srcPts[indexC];

                float[,] possibilites = new float[tgtPts.Count, 7];

                // loop all points in the second set
                for (int n = 0; n < tgtPts.Count - 1; n++)
                {
                    // take one point form all points
                    Vector3 px = tgtPts[n];
                    Vector3 py = tgtPts[n + 1];
                    int matchNum = 0;

                    calculateRT(pointA, pointB, px, py);

                    if (n + 2 < tgtPts.Count)
                    {
                        for (int j = n + 2; j < tgtPts.Count; j++)
                        {
                            Vector3 pz = tgtPts[j];

                            matchNum += estimateRigidTransformation(pointC, pz);
                        }
                        possibilites[n, 0] = Tx;
                        possibilites[n, 1] = Ty;
                        possibilites[n, 2] = Tz;
                        possibilites[n, 3] = (float)matchNum;
                        possibilites[n, 4] = R0;
                        possibilites[n, 5] = R1;
                        possibilites[n, 6] = R2;
                    }
                } // for-all points

                int maxIndex = 0;
                float MAX = 0;

                for (int m = 0; m < tgtPts.Count; m++)
                {
                    if (possibilites[m, 3] > MAX)
                    {
                        maxIndex = m;
                        MAX = possibilites[m, 3];
                    }
                }

                allPossibilites[i, 0] = possibilites[maxIndex, 0];
                allPossibilites[i, 1] = possibilites[maxIndex, 1];
                allPossibilites[i, 2] = possibilites[maxIndex, 2];
                allPossibilites[i, 3] = possibilites[maxIndex, 3];
                allPossibilites[i, 4] = possibilites[maxIndex, 4];
                allPossibilites[i, 5] = possibilites[maxIndex, 5];
                allPossibilites[i, 6] = possibilites[maxIndex, 6];
            } // for-iterations

            int maxInd = 0;
            float max = 0;

            for (int k = 0; k < iterations; k++)
            {
                if (allPossibilites[k, 3] > max)
                {
                    maxInd = k;
                    max = allPossibilites[k, 3];
                }
            }

            Debug.Log("Exact Match Percentage: " + tgtPts.Count / allPossibilites[maxInd, 3]);
            rigidTransformationPart1(allPossibilites[maxInd, 4], allPossibilites[maxInd, 5], allPossibilites[maxInd, 6], allPossibilites[maxInd, 0], allPossibilites[maxInd, 1], allPossibilites[maxInd, 2], tgtPts, point, transform);
        }

        public void rigidTransformationPart1(float rx, float ry, float rz, float tx, float ty, float tz, List<Vector3> tgtPts, GameObject point, Transform transform)
        {

            for (int i = 0; i < tgtPts.Count; i++)
            {
                float nx = (rx * tgtPts[i][0]) + tx;
                float ny = (ry * tgtPts[i][1]) + ty;
                float nz = (rz * tgtPts[i][2]) + tz;

                Vector3 transformedObject = new Vector3(nx, ny, nz);

                Debug.DrawLine(tgtPts[i], transformedObject, Color.white, 9000, false);

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale = new Vector3(30f, 30f, 30f);  // Adjust scale as needed
                sphere.transform.position = new Vector3(nx, ny, nz);

                sphere.GetComponent<Renderer>().material.color = Color.blue;
            }

        }

        public void calculateRT(Vector3 firstPoint, Vector3 secondPoint, Vector3 targetPoint1, Vector3 targetPoint2)
        {
            //P2 = R * P1 + T
            Tx = 0;
            Ty = 0;
            Tz = 0;
            R0 = 0;
            R1 = 0;
            R2 = 0;

            R0 = (float)((float)firstPoint[0] - (float)secondPoint[0]) / (float)((float)targetPoint1[0] - (float)targetPoint2[0]);
            R1 = (float)((float)firstPoint[1] - (float)secondPoint[1]) / (float)((float)targetPoint1[1] - (float)targetPoint2[1]);
            R2 = (float)((float)firstPoint[2] - (float)secondPoint[2]) / (float)((float)targetPoint1[2] - (float)targetPoint2[2]);

            Tx = (float)firstPoint[0] - (float)((float)targetPoint1[0] * R0);
            Ty = (float)firstPoint[1] - (float)((float)targetPoint1[1] * R1);
            Tz = (float)firstPoint[2] - (float)((float)targetPoint1[2] * R2);
        }

        public int estimateRigidTransformation(Vector3 thirdPoint, Vector3 targetPoint3)
        {
            float r1, r2, r3;

            r1 = (thirdPoint[0] - Tx) / targetPoint3[0];
            r2 = (thirdPoint[1] - Ty) / targetPoint3[1];
            r3 = (thirdPoint[2] - Tz) / targetPoint3[2];

            r1 = (float)Math.Round(r1, 1, MidpointRounding.ToEven);
            r2 = (float)Math.Round(r2, 1, MidpointRounding.ToEven);
            r3 = (float)Math.Round(r3, 1, MidpointRounding.ToEven);

            R0 = (float)Math.Round(R0, 1, MidpointRounding.ToEven);
            R1 = (float)Math.Round(R1, 1, MidpointRounding.ToEven);
            R2 = (float)Math.Round(R2, 1, MidpointRounding.ToEven);

            if (r1 == R0 && r2 == R1 && r3 == R2)
            {
                return 1;
            }

            return 0;
        }
    }
}
