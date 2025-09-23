using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh mesh;

    private MeshMap meshMap = new MeshMap();

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        meshMap = new MeshMap();

        Generate();
        UpdateMesh();
    }

    private void Generate()
    {
        AddCube(Vector3.zero, Vector3.one);
        AddCube(new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);
    }

    private void AddCube(Vector3 center, Vector3 size)
    {
        // This assumes that the given collider is a square, which is should be for this example
        Vector3[] verts = new Vector3[8];
        Triangle[] tris = new Triangle[12];
        Face[] faces = new Face[6];

        // Calculate local corner positions
        Vector3 min = center - size * 0.5f;
        Vector3 max = center + size * 0.5f;

        // Note: The vertexes are labelled  Left -> Right, Bottom -> Top, Front -> Back  IN THAT ORDER
        // Normal cube would have 0 in front bottom left and 7 in back top right

        // Get the vertices from the dimensions given
        verts[0] = new Vector3(min.x, min.y, min.z);
        verts[1] = new Vector3(max.x, min.y, min.z);
        verts[2] = new Vector3(min.x, max.y, min.z);
        verts[3] = new Vector3(max.x, max.y, min.z);
        verts[4] = new Vector3(min.x, min.y, max.z);
        verts[5] = new Vector3(max.x, min.y, max.z);
        verts[6] = new Vector3(min.x, max.y, max.z);
        verts[7] = new Vector3(max.x, max.y, max.z);

        /*
         *     Vertex to Tri Guide
         *      2____________3
         *      |\           |
         *      |   \    2   |
         *      |  1   \     |
         *      |         \  |
         *      0------------1
         *      
         *      1 = 0,2,1
         *      2 = 1,2,3
        */

        // Front
        tris[0] = new Triangle(new Vector3[] { verts[0], verts[2], verts[1] });
        tris[1] = new Triangle(new Vector3[] { verts[1], verts[2], verts[3] });
        faces[0] = new Face(tris[0], tris[1]);
        // Left
        tris[2] = new Triangle(new Vector3[] { verts[4], verts[6], verts[0] });
        tris[3] = new Triangle(new Vector3[] { verts[0], verts[6], verts[2] });
        faces[1] = new Face(tris[2], tris[3]);
        // Back
        tris[4] = new Triangle(new Vector3[] { verts[5], verts[7], verts[4] });
        tris[5] = new Triangle(new Vector3[] { verts[4], verts[7], verts[6] });
        faces[2] = new Face(tris[4], tris[5]);
        // Right
        tris[6] = new Triangle(new Vector3[] { verts[1], verts[3], verts[5] });
        tris[7] = new Triangle(new Vector3[] { verts[5], verts[3], verts[7] });
        faces[3] = new Face(tris[6], tris[7]);
        // Top
        tris[8] = new Triangle(new Vector3[] { verts[2], verts[6], verts[3] });
        tris[9] = new Triangle(new Vector3[] { verts[3], verts[6], verts[7] });
        faces[4] = new Face(tris[8], tris[9]);
        //Bottom
        tris[10] = new Triangle(new Vector3[] { verts[4], verts[0], verts[5] });
        tris[11] = new Triangle(new Vector3[] { verts[5], verts[0], verts[1] });
        faces[5] = new Face(tris[10], tris[11]);

        // Loop through all vertices in this cube
        for (int i = 0; i < verts.Length; i++)
        {
            // Check to see if the vertex is already present in the list
            if (!meshMap.vertices.Contains(verts[i]))
            {
                meshMap.AddVertex(verts[i]);
            }
        }

        // Loop through all tris in this cube and set their values within the triangles list
        for(int i = 0; i < faces.Length; i++)
        {
            // If the triangle already exists, remove it from the list since these faces would be touching
            if (meshMap.faces.Contains(faces[i]))
            {
                meshMap.RemoveFace(faces[i]);
            }
            else
            {
                meshMap.AddFace(faces[i]);
            }
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = meshMap.GetVertices();
        mesh.triangles = meshMap.GetTriangles();
    }
}

public class MeshMap
{
    public List<Vector3> vertices { get; private set; } = new List<Vector3>();
    public List<Triangle> triangles { get; private set; } = new List<Triangle>();

    public Dictionary<Vector3, int> vertexLookup = new Dictionary<Vector3, int>();
    public List<Face> faces { get; private set; } = new List<Face>();

    public Vector3[] GetVertices()
    {
        return vertices.ToArray();
    }
    public int[] GetTriangles()
    {
        // Initialize an array to hold all tri vertex index
        int[] tris = new int[triangles.Count * 3];

        // Loop through all tris to grab vertices
        for(int i = 0; i < triangles.Count; i++)
        {
            Triangle t = triangles[i];
            tris[i*3] = vertexLookup[t.vertices[0]];
            tris[i*3+1] = vertexLookup[t.vertices[1]];
            tris[i*3+2] = vertexLookup[t.vertices[2]];
        }

        return tris;
    }

    public void AddVertex(Vector3 vertex)
    {
        int index = vertices.Count;
        vertices.Add(vertex);
        vertexLookup.Add(vertex, index);
    }

    public void AddFace(Face face)
    {
        faces.Add(face);

        for(int i = 0; i < face.triangles.Length; i++)
        {
            triangles.Add(face.triangles[i]);
        }
    }
    public void RemoveFace(Face face)
    {
        if(faces.Contains(face))
            faces.Remove(face);
    }
}
public class Face
{
    public Vector3[] vertices = new Vector3[4];
    public Triangle[] triangles = new Triangle[2];

    public Face(Triangle t1, Triangle t2)
    {
        this.triangles = new Triangle[] { t1, t2 };

        vertices[0] = triangles[0].vertices[0];
        vertices[1] = triangles[0].vertices[2];
        vertices[1] = triangles[0].vertices[1];
        vertices[1] = triangles[1].vertices[2];
    }

    public override bool Equals(object obj)
    {
        Face face = obj as Face;
        return vertices == face.vertices;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(vertices, triangles);
    }
}
public class Triangle
{
    public Vector3[] vertices = new Vector3[3];

    public Triangle(Vector3[] vertices)
    {
        this.vertices = vertices;
    }

    public void SetVertices(Vector3[] vertices)
    {
        for (int i = 0; i < 3; i++)
        {
            this.vertices[i] = vertices[i];
        }
    }

    public override bool Equals(object obj)
    {
        Triangle t = obj as Triangle;
        Vector3[] verts = t.vertices;

        return verts[0] == vertices[0] && verts[1] == vertices[1] && verts[2] == vertices[2];
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(vertices[0], vertices[1], vertices[2]);
    }
}
