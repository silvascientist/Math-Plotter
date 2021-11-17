using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static System.Math;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour {

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public float xMin = 0;
    public float xMax = 20;
    public float zMin = 0;
    public float zMax = 20;
    public int xSize = 20;
    public int zSize = 20;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
 
        CreateShape();
        UpdateMesh();  
    }

    void CreateShape()
    {
        int xPts = xSize + 1;
        int zPts = zSize + 1;
        vertices = new Vector3[xPts*zPts];

        for (int i = 0, j = 0; j < zPts; j++)
        {
            for (int k = 0; k < xPts; k++)
            {
                float x = xMin + k*(xMax - xMin)/xPts;
                float z = zMin + j*(zMax - zMin)/zPts; 
                float y = 2 * (float) Sin(x*x/5 + z*z/5); 
                vertices[i] = new Vector3(x,y,z);
                i++; 
            }
        }

        triangles = new int[xSize*zSize*6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        doubleMesh(mesh);

        mesh.RecalculateNormals();
    }
/*****************************
    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }
******************************/
    void doubleMesh(Mesh mesh)
    {
        int lenVerts = mesh.vertices.Length;
        int lenTris  = mesh.triangles.Length;
        Vector3[] vertices = mesh.vertices;
        Vector3[] doubleVertices = new Vector3[2*lenVerts];
        int[] triangles = mesh.triangles;
        int[] doubleTriangles = new int[2*lenTris];

        System.Array.Copy(vertices, doubleVertices, lenVerts);
        System.Array.Copy(vertices, 0, doubleVertices, lenVerts, lenVerts);
        System.Array.Copy(triangles, doubleTriangles, lenTris);
        for (int i = 0; i <= lenTris - 3; i += 3)
        {
            doubleTriangles[lenTris + i] = triangles[i] + lenVerts;
            doubleTriangles[lenTris + i + 1] = triangles[i + 2] + lenVerts;
            doubleTriangles[lenTris + i + 2] = triangles[i + 1] + lenVerts;
        }

        mesh.vertices = doubleVertices;
        mesh.triangles = doubleTriangles;
    }

/*****
    Queue<Token> lexer(string mathExpression)
    {

    }
*****/
}
