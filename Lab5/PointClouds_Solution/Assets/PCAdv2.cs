using UnityEngine;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using TMPro;
using static GlobalReferences;

public class PCAdv2 : MonoBehaviour
{

    // File
    public string dataPath;
    private string filename;
    public Material matVertex;

    // GUI
    private float progress = 0;
    private string guiText;
    private bool loaded = false;

    // PointCloud
    private GameObject pointCloud;

    public float scale = 1;
    public bool invertYZ = false;
    public bool forceReload = false;

    public int numPoints;
    public int numPointGroups;
    private int limitPoints = 65000;

    private Vector3[] points;
    private Color[] colors;
    private Vector3 minValue;
    private Vector3 maxValue;

    //plane finding
    private Color[] colorsP;
    private int[,] planes;
    private int[] planeSizes;


    void Start()
    {
        // Garantee Parsing as en-Us culture
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Create Resources folder
        createFolders();

        dataPath = GlobalReferences.CrossSceneText;
        Debug.Log("DataPath: " + dataPath);
        filename = Path.GetFileName(dataPath);

        forceReload = GlobalReferences.CSReload;
        Debug.Log("Reload: " + forceReload);

        invertYZ = GlobalReferences.CSInvert;
        Debug.Log("InvertYZ: " + invertYZ);  

        scale = GlobalReferences.CSScale;
        Debug.Log("Scale: " + scale);  

        loadScene();
    }



    void loadScene()
    {
        // Check if the PointCloud was loaded previously
        if (!Directory.Exists(Application.dataPath + "/Resources/PointCloudMeshes/" + filename))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets/Resources/PointCloudMeshes", filename);
            loadPointCloud();
        }
        else if (forceReload)
        {
            UnityEditor.FileUtil.DeleteFileOrDirectory(Application.dataPath + "/Resources/PointCloudMeshes/" + filename);
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.AssetDatabase.CreateFolder("Assets/Resources/PointCloudMeshes", filename);
            loadPointCloud();
        }
        else
            // Load stored PointCloud
            loadStoredMeshes();
    }


    void loadPointCloud()
    {
        // Check what file exists
        if (File.Exists(Application.dataPath + dataPath + ".off"))
            // load off
            StartCoroutine("loadOFF", dataPath + ".off");
        else if (File.Exists(Application.dataPath + dataPath + ".txt"))
            // load txt
            StartCoroutine("loadLiDAR", dataPath + ".txt");
        else if (File.Exists(Application.dataPath + dataPath + ".pts"))
            // load pts
            StartCoroutine("loadPTS", dataPath + ".pts");
        else
            Debug.Log("File '" + dataPath + "' could not be found");

    }

    // Load stored PointCloud
    void loadStoredMeshes()
    {

        Debug.Log("Using previously loaded PointCloud: " + filename);

        GameObject pointGroup = Instantiate(Resources.Load("PointCloudMeshes/" + filename)) as GameObject;

        loaded = true;
    }

    // Start Coroutine of reading the points from the OFF file and creating the meshes
    IEnumerator loadOFF(string dPath)
    {

        // Read file
        StreamReader sr = new StreamReader(Application.dataPath + dPath);
        sr.ReadLine(); // OFF
        string[] buffer = sr.ReadLine().Split(); // nPoints, nFaces

        numPoints = int.Parse(buffer[0]);
        points = new Vector3[numPoints];
        colors = new Color[numPoints];
        minValue = new Vector3();

        for (int i = 0; i < numPoints; i++)
        {
            buffer = sr.ReadLine().Split();

            if (!invertYZ)
                points[i] = new Vector3(float.Parse(buffer[0]) * scale, float.Parse(buffer[1]) * scale, float.Parse(buffer[2]) * scale);
            else
                points[i] = new Vector3(float.Parse(buffer[0]) * scale, float.Parse(buffer[2]) * scale, float.Parse(buffer[1]) * scale);

            if (buffer.Length >= 5)
                colors[i] = new Color(int.Parse(buffer[3]) / 255.0f, int.Parse(buffer[4]) / 255.0f, int.Parse(buffer[5]) / 255.0f);
            else
                colors[i] = Color.cyan;

            // Relocate Points near the origin
            calculateMin(points[i]);
            calculateMax(points[i]);

            // GUI
            progress = i * 1.0f / (numPoints - 1) * 1.0f;
            if (i % Mathf.FloorToInt(numPoints / 20) == 0)
            {
                guiText = i.ToString() + " out of " + numPoints.ToString() + " loaded";
                yield return null;
            }
        }
        
        points = closerOrigin(points,numPoints);

        StartCoroutine("findPlanes2");
        // colors = findPlanes(points, numPoints, colors);

        // Instantiate Point Groups
        numPointGroups = Mathf.CeilToInt(numPoints * 1.0f / limitPoints * 1.0f);

        pointCloud = new GameObject(filename);

        for (int i = 0; i < numPointGroups - 1; i++)
        {
            InstantiateMesh(i, limitPoints);
            if (i % 10 == 0)
            {
                guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                yield return null;
            }
        }
        InstantiateMesh(numPointGroups - 1, numPoints - (numPointGroups - 1) * limitPoints);

        //Store PointCloud
        UnityEditor.PrefabUtility.CreatePrefab("Assets/Resources/PointCloudMeshes/" + filename + ".prefab", pointCloud);

        loaded = true;
    }


    /*
	 * Method to Load LiDAR Simulator Point Clouds
	 */
    // Start Coroutine of reading the points from the OFF file and creating the meshes
    IEnumerator loadLiDAR(string dPath)
    {
        //Count lines
        int numPoints = -4;
        string line;
        TextReader reader = new StreamReader(Application.dataPath + dPath);
        while ((line = reader.ReadLine()) != null)
        {
            numPoints++;
        }
        reader.Close();      

        // Read file
        StreamReader sr = new StreamReader(Application.dataPath + dPath);
        sr.ReadLine(); // LiDAR
        string[] buffer = sr.ReadLine().Split(); // useless first line

        points = new Vector3[numPoints];
        colors = new Color[numPoints];
        minValue = new Vector3();

        for (int i = 0; i < numPoints; i++)
        {
            buffer = sr.ReadLine().Split();

            if (!invertYZ)
                points[i] = new Vector3(float.Parse(buffer[1]) * scale, float.Parse(buffer[2]) * scale, float.Parse(buffer[3]) * scale);
            else
                points[i] = new Vector3(float.Parse(buffer[1]) * scale, float.Parse(buffer[3]) * scale, float.Parse(buffer[2]) * scale);

            //reading laser id and determine color
            int id = int.Parse(buffer[7]);

            //to normalize
            float norm_id = id / 255.0f;

            //computing the vertex color using HSV since the final result is better
            colors[i] = Color.HSVToRGB(norm_id, 1, 1);

            // Relocate Points near the origin
            calculateMin(points[i]);
            calculateMax(points[i]);

            // GUI
            progress = i * 1.0f / (numPoints - 1) * 1.0f;
            if (i % Mathf.FloorToInt(numPoints / 20) == 0)
            {
                guiText = i.ToString() + " out of " + numPoints.ToString() + " loaded";
                yield return null;
            }
        }

        points = closerOrigin(points,numPoints);

        StartCoroutine("findPlanes2");
        // colors = findPlanes(points, numPoints, colors);

        // Instantiate Point Groups
        numPointGroups = Mathf.CeilToInt(numPoints * 1.0f / limitPoints * 1.0f);

        pointCloud = new GameObject(filename);

        for (int i = 0; i < numPointGroups - 1; i++)
        {
            InstantiateMesh(i, limitPoints);
            if (i % 10 == 0)
            {
                guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                yield return null;
            }
        }
        InstantiateMesh(numPointGroups - 1, numPoints - (numPointGroups - 1) * limitPoints);

        //Store PointCloud
        UnityEditor.PrefabUtility.CreatePrefab("Assets/Resources/PointCloudMeshes/" + filename + ".prefab", pointCloud);

        loaded = true;
    }

    IEnumerator loadPTS(string dPath)
    {
        // Read file
        StreamReader sr = new StreamReader(Application.dataPath + dPath);
        string[] buffer = sr.ReadLine().Split(); // nPoints
        numPoints = int.Parse(buffer[0]);
        Debug.Log("Points: " + numPoints);


        points = new Vector3[numPoints];
        colors = new Color[numPoints];
        minValue = new Vector3();

        for (int i = 0; i < numPoints; i++)
        {
            buffer = sr.ReadLine().Split();

            if (!invertYZ)
                points[i] = new Vector3(float.Parse(buffer[0]) * scale, float.Parse(buffer[1]) * scale, float.Parse(buffer[2]) * scale);
            else
                points[i] = new Vector3(float.Parse(buffer[0]) * scale, float.Parse(buffer[2]) * scale, float.Parse(buffer[1]) * scale);
            
            colors[i] = new Color(1, 1, 0);

            calculateMin(points[i]);
            calculateMax(points[i]);


            // GUI
            progress = i * 1.0f / (numPoints - 1) * 1.0f;
            if (i % Mathf.FloorToInt(numPoints / 20) == 0)
            {
                guiText = i.ToString() + " out of " + numPoints.ToString() + " loaded";
                yield return null;
            }
        }

        points = closerOrigin(points,numPoints);

        StartCoroutine("findPlanes2");
        // colors = findPlanes(points, numPoints, colors);


        // Instantiate Point Groups
        numPointGroups = Mathf.CeilToInt(numPoints * 1.0f / limitPoints * 1.0f);

        pointCloud = new GameObject(filename);

        for (int i = 0; i < numPointGroups - 1; i++)
        {
            InstantiateMesh(i, limitPoints);
            if (i % 10 == 0)
            {
                guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                yield return null;
            }
        }
        InstantiateMesh(numPointGroups - 1, numPoints - (numPointGroups - 1) * limitPoints);

        //Store PointCloud
        UnityEditor.PrefabUtility.CreatePrefab("Assets/Resources/PointCloudMeshes/" + filename + ".prefab", pointCloud);

        loaded = true;
    }


    void InstantiateMesh(int meshInd, int nPoints)
    {
        // Create Mesh
        GameObject pointGroup = new GameObject(filename + meshInd);
        pointGroup.AddComponent<MeshFilter>();
        pointGroup.AddComponent<MeshRenderer>();
        pointGroup.GetComponent<Renderer>().material = matVertex;

        pointGroup.GetComponent<MeshFilter>().mesh = CreateMesh(meshInd, nPoints, limitPoints);
        pointGroup.transform.parent = pointCloud.transform;


        // Store Mesh
        UnityEditor.AssetDatabase.CreateAsset(pointGroup.GetComponent<MeshFilter>().mesh, "Assets/Resources/PointCloudMeshes/" + filename + @"/" + filename + meshInd + ".asset");
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }

    Mesh CreateMesh(int id, int nPoints, int limitPoints)
    {

        Mesh mesh = new Mesh();

        Vector3[] myPoints = new Vector3[nPoints];
        int[] indecies = new int[nPoints];
        Color[] myColors = new Color[nPoints];

        for (int i = 0; i < nPoints; ++i)
        {
            myPoints[i] = points[id * limitPoints + i] - minValue;
            indecies[i] = i;
            myColors[i] = colors[id * limitPoints + i];
        }


        mesh.vertices = myPoints;
        mesh.colors = myColors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.uv = new Vector2[nPoints];
        mesh.normals = new Vector3[nPoints];


        return mesh;
    }

    void calculateMin(Vector3 point)
    {
        if (minValue.magnitude == 0)
            minValue = point;


        if (point.x < minValue.x)
            minValue.x = point.x;
        if (point.y < minValue.y)
            minValue.y = point.y;
        if (point.z < minValue.z)
            minValue.z = point.z;
    }
    void calculateMax(Vector3 point)
    {
        if (maxValue.magnitude == 0)
            maxValue = point;


        if (point.x > maxValue.x)
            maxValue.x = point.x;
        if (point.y > maxValue.y)
            maxValue.y = point.y;
        if (point.z > maxValue.z)
            maxValue.z = point.z;
    }

    double calcDistance(Vector3 point1, Vector3 point2)
    {
        return Math.Sqrt(Math.Pow((point1.x-point2.x),2) + Math.Pow((point1.y-point2.y),2) + Math.Pow((point1.z-point2.z),2));
    }

    Vector3[] closerOrigin(Vector3[] points, int numPoints)
    {
        Vector3[] points2 = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            points2[i].x = points[i].x - (minValue.x + maxValue.x)/2;
            points2[i].y = points[i].y;
            points2[i].z = points[i].z - (minValue.z + maxValue.z)/2;
        }
        return points2;
    }


    Color[] findPlanes(Vector3[] points, int numPoints, Color[] colors)
    {
        int maxPlanes = 5;
        int maxPointsInPlane = 50;
        planes = new int[maxPlanes,maxPointsInPlane];
        planeSizes = new int[numPoints];
        int[] visited = new int[numPoints];
        int visitIdx = 0;
        int planeIdx = 0;

        Color[] colors2 = new Color[numPoints];

        for (int i = 0; i < numPoints; i++)
        {
            if (visited.Contains(i)) continue;
            colors2[i] = colors[i];
            int planesize = 0;
            visited[visitIdx] = i;
            visitIdx++;
            Debug.Log("i= " + i);

            for (int j = i + 1; j < numPoints; j++)
            {
                if (visited.Contains(j)) continue;
                colors2[j] = colors[j];
                // if (points[j].x * 0.95 < points[i].x && points[i].x < points[j].x * 1.05){
                if (calcDistance(points[i], points[j]) < 1) {
                    if (planesize == maxPointsInPlane) break;
                    if (planesize == 0){
                        planes[planeIdx,0] = i;
                        colors2[i] = Color.cyan;
                        planesize++;
                    }
                    Debug.Log("Adding point " + j + " to plane " + planeIdx);
                    planes[planeIdx,planesize] = j; 
                    planesize++;
                    visited[visitIdx] = j;
                    colors2[j] = Color.cyan;
                    visitIdx++;
                    
                }
            }
            if (visitIdx == numPoints) break;
            planeSizes[planeIdx] = planesize;
            Debug.Log("Size of plane " + planeIdx + " : " + (planesize));
            planeIdx++;
            if (planeIdx == maxPlanes) break;
        }
        Debug.Log("Final PlaneIdx " + planeIdx);
        
        return colors2;
    }

    IEnumerator findPlanes2()
    {
        int maxPlanes = 5;
        int maxPointsInPlane = 50;
        planes = new int[maxPlanes,maxPointsInPlane];
        planeSizes = new int[numPoints];
        int[] visited = new int[numPoints];
        int visitIdx = 0;
        int planeIdx = 0;

        Color[] colors2 = new Color[numPoints];

        for (int i = 0; i < numPoints; i++)
        {
            if (visited.Contains(i)) continue;
            colors2[i] = colors[i];
            int planesize = 0;
            visited[visitIdx] = i;
            visitIdx++;
            Debug.Log("i= " + i);

            for (int j = i + 1; j < numPoints; j++)
            {
                if (visited.Contains(j)) continue;
                colors2[j] = colors[j];
                // if (points[j].x * 0.95 < points[i].x && points[i].x < points[j].x * 1.05){
                if (calcDistance(points[i], points[j]) < 1) {
                    if (planesize == maxPointsInPlane) break;
                    if (planesize == 0){
                        planes[planeIdx,0] = i;
                        colors2[i] = Color.cyan;
                        planesize++;
                    }
                    Debug.Log("Adding point " + j + " to plane " + planeIdx);
                    planes[planeIdx,planesize] = j; 
                    planesize++;
                    visited[visitIdx] = j;
                    colors2[j] = Color.cyan;
                    visitIdx++;
                    
                }
            }
            if (visitIdx == numPoints) break;
            planeSizes[planeIdx] = planesize;
            Debug.Log("Size of plane " + planeIdx + " : " + (planesize));
            planeIdx++;
            if (planeIdx == maxPlanes) break;
            yield return null;
        }
        Debug.Log("Final PlaneIdx " + planeIdx);
        yield return null;

    }


    void createFolders()
    {
        if (!Directory.Exists(Application.dataPath + "/Resources/")) {
            Debug.Log("Creating Folder at: "+Application.dataPath + "/Resources/");
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!Directory.Exists(Application.dataPath + "/Resources/PointCloudMeshes/")) {
            Debug.Log("Creating Folder at: "+ Application.dataPath + "/Resources/PointCloudMeshes/");
            UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "PointCloudMeshes");
        }
    }


    void OnGUI()
    {


        if (!loaded)
        {
            GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2, 400.0f, 20));
            GUI.Box(new Rect(0, 0, 200.0f, 20.0f), guiText);
            GUI.Box(new Rect(0, 0, progress * 200.0f, 20), "");
            GUI.EndGroup();
        }
    }
}

