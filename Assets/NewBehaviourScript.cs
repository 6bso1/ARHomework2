using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.UI;

namespace allProgram
{
    public class NewBehaviourScript : MonoBehaviour
    {
        private List<Vector3> pointSetP = new List<Vector3>();
        private List<Vector3> pointSetQ = new List<Vector3>();
        public GameObject sphere;

        Transformation myRansac;

        void Start()
        {

            // Find the button component
            Button button = GetComponent<Button>();

            // Attach the method to the button click event
           
            Debug.Log("Reading points from file...");
            // Call your file reading function here
            ReadPointCloudFromFile("./Assets/file1.txt", pointSetP);
            ReadPointCloudFromFile("./Assets/file2.txt", pointSetQ);

            // Log the number of points read from each file
            Debug.Log("Number of points in pointSetP: " + pointSetP.Count);
            Debug.Log("Number of points in pointSetQ: " + pointSetQ.Count);

            // Call your point cloud alignment function here
            //AlignPointClouds(pointSetP, pointSetQ);
        }

        void Update()
        {
            // You can add any continuous update logic here if needed
        }

        void ReadPointCloudFromFile(string filePath, List<Vector3> pointSet)
        {
            // Check if the file exists
            if (File.Exists(filePath))
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(filePath);

                // Iterate through lines and parse coordinates
                foreach (string line in lines)
                {
                    string[] coordinates = line.Split(' ');
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.localScale = new Vector3(30f, 30f, 30f);  // Adjust scale as needed
                    sphere.transform.position = new Vector3(float.Parse(coordinates[0]), float.Parse(coordinates[1]), float.Parse(coordinates[2]));

                    if (coordinates.Length == 3)
                    {
                        if (filePath == "./Assets/file1.txt")
                        {
                            sphere.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);
                        }
                        else
                        {
                            sphere.GetComponent<Renderer>().material.color = new Color(0.5f, 0f, 1f);
                        }
                        float x = float.Parse(coordinates[0]);
                        float y = float.Parse(coordinates[1]);
                        float z = float.Parse(coordinates[2]);

                        // Create a Vector3 and add it to the point set
                        Vector3 point = new Vector3(x, y, z);
                        pointSet.Add(point);
                    }
                }
            }
            else
            {
                // Log an error if the file is not found
                Debug.LogError("File not found: " + filePath);
            }
        }

        void AlignPointClouds(List<Vector3> pointSetP, List<Vector3> pointSetQ)
        {
            // Implement your point cloud alignment algorithm here
            // This could include RANSAC and the transformation calculations
            // Use Unity's Transform component to apply the transformations to GameObjects

            // Log a message to indicate that the function is called
            Debug.Log("AlignPointClouds function called.");
        }
        void OnButtonClick()
        {
            // Call your point cloud alignment function here
            Debug.Log("OnButtonClick function called.");
            AlignPointClouds(pointSetP, pointSetQ);
        }

        public void callRansac()
        {
            myRansac = gameObject.AddComponent<Transformation>();
            myRansac.Ransac(pointSetP,pointSetQ, sphere, transform);
        }
    }
}
