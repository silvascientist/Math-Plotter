using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static System.Math;
using static MathParser;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour {

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public string expression;
    string oldExpression;

    public float xMin = 0;
    public float xMax = 20;
    public float zMin = 0;
    public float zMax = 20;
    public int xSize = 20;
    public int zSize = 20;

    struct State
    {
        public string expression;
        public float xMin;
        public float xMax;
        public float zMin;
        public float zMax;
        public int xSize;
        public int zSize;

    }

    State state;
    State oldState;

    // Start is called before the first frame update
    void Start()
    {

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        expression = "x^2 + z^2";

        state.expression = expression;
        state.xMin = xMin;
        state.xMax = xMax;
        state.zMin = zMin;
        state.zMax = zMax;
        state.xSize = xSize;
        state.zSize = zSize;

        oldState = state;

        CreateShape();
        UpdateMesh();

        //Debug.Log("Hello, World!");

        //Queue<Token> tokenStream = LexExpression("32*x^3/(x^2 + z^2) - 16*x^5/(x^2 + z^2)^2 - 14*x");
        //foreach (Token t in tokenStream)
        //{
        //    switch (t.type)
        //    {
        //        case TokenType.Int:
        //            Debug.Log($"{t.intValue}, Int");
        //            break;
        //        case TokenType.Float:
        //            Debug.Log($"{t.floatValue}, Float");
        //            break;
        //        case TokenType.Operator:
        //            Debug.Log($"{t.opValue}, Operator");
        //            break;
        //        case TokenType.Identifier:
        //            Debug.Log($"{t.identifier}, Identifier");
        //            break;
        //        case TokenType.Delimiter:
        //            Debug.Log($"{t.delimiter}, Delimiter");
        //            break;
        //    }
        //}

        //ExpressionAST expression1 = ParseExpression("32*x^3/(x^2 + z^2) - 16*x^5/(x^2 + z^2)^2 - 14*x");
        //float val = expression1.ASTeval(1, 1);
        //Debug.Log($"{val}");
        //Debug.Log("Here's the expression tree:");
        //expression1.PrintTree();
    }
    private void Update()
    {
        state.expression = expression;
        state.xMin = xMin;
        state.xMax = xMax;
        state.zMin = zMin;
        state.zMax = zMax;
        state.xSize = xSize;
        state.zSize = zSize;

        if (!state.Equals(oldState))
        {
            oldState = state;
            CreateShape();
            UpdateMesh();
        }
    }

    void CreateShape()
    {
        Debug.Log(expression);
        ExpressionAST expressionTree = ParseExpression(expression);

        int xPts = xSize + 1;
        int zPts = zSize + 1;
        vertices = new Vector3[xPts*zPts];

        for (int i = 0, j = 0; j < zPts; j++)
        {
            for (int k = 0; k < xPts; k++)
            {
                float x = xMin + k*(xMax - xMin)/xPts;
                float z = zMin + j*(zMax - zMin)/zPts; 
                float y = expressionTree.ASTeval(x,z);
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
