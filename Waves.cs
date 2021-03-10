using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves : MonoBehaviour {
    //Public variables
    public int Dimension = 10;
    public float UVScale;
    public Octave[] Octaves;
    
    //Protected variables
    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

	// Use this for initialization
	void Start () {
        Debug.Log("It calls Start");
        //Mesh set up
        Mesh = new Mesh();
        Mesh.name = gameObject.name;
        //Mesh.position = gameObject.position;

        Mesh.vertices = GenerateVerts();
        Mesh.triangles = GenerateTries();
        Mesh.uv = GenerateUVs();
        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();

        MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.mesh = Mesh;
	}


    public float GetHeight(Vector3 position)

    {
        //Scale factor and position in local space
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((position - transform.position), scale);

        //Get edge points

        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //Clamp if the position is outside the plane

        p1.x = Mathf.Clamp(p1.x, 0, Dimension);
        p1.z = Mathf.Clamp(p1.z, 0, Dimension);
        p2.x = Mathf.Clamp(p2.x, 0, Dimension);
        p2.z = Mathf.Clamp(p2.z, 0, Dimension);
        p3.x = Mathf.Clamp(p3.x, 0, Dimension);
        p3.z = Mathf.Clamp(p3.z, 0, Dimension);
        p4.x = Mathf.Clamp(p4.x, 0, Dimension);
        p4.z = Mathf.Clamp(p4.z, 0, Dimension);


        //Get the max distance to one of the edges and take that to compute max - distance

        var max = Mathf.Max(Vector3.Distance(p1, localPos), Vector3.Distance(p2, localPos), Vector3.Distance(p3, localPos), Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        var dist = (max - Vector3.Distance(p1, localPos))
                 + (max - Vector3.Distance(p2, localPos))
                 + (max - Vector3.Distance(p3, localPos))
                 + (max - Vector3.Distance(p4, localPos) + Mathf.Epsilon);

        //Weighted sum
        var height = Mesh.vertices[index(p1.x, p1.z)].y * (max - Vector3.Distance(p1, localPos))
                   + Mesh.vertices[index(p2.x, p2.z)].y * (max - Vector3.Distance(p2, localPos))
                   + Mesh.vertices[index(p3.x, p3.z)].y * (max - Vector3.Distance(p3, localPos))
                   + Mesh.vertices[index(p4.x, p4.z)].y * (max - Vector3.Distance(p4, localPos));

        //Scale
        return height * transform.lossyScale.y / dist;
    }

    private Vector3[] GenerateVerts()
    {
        var verts = new Vector3[(Dimension + 1) * (Dimension + 1)];

        //Equally distribute verticies
        for (int x = 0; x <= Dimension; x++)
            for (int z = 0; z <= Dimension; z++)
                verts[index(x, z)] = new Vector3(x, 0, z);
        return verts;
    }

    private int[] GenerateTries()
    {
        var tries = new int[Mesh.vertices.Length * 6];

        //Two Triangles are one tile
        for (int x = 0; x < Dimension; x++)
        {
            for (int z = 0; z < Dimension; z++)
            {
                tries[index(x, z) * 6 + 0] = index(x, z);
                tries[index(x, z) * 6 + 1] = index(x + 1, z + 1);
                tries[index(x, z) * 6 + 2] = index(x + 1, z);
                tries[index(x, z) * 6 + 3] = index(x, z);
                tries[index(x, z) * 6 + 4] = index(x, z + 1);
                tries[index(x, z) * 6 + 5] = index(x + 1, z + 1);
            }
        }

        return tries;
    }

    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[Mesh.vertices.Length];

        //Always one UV over n tiles, flipping after each tile
        for (int x = 0; x <= Dimension; x++)
        {
            for ( int z = 0; z <= Dimension; z++)
            {
                var vec = new Vector2((x / UVScale) % 2, (z / UVScale) % 2);
                uvs[index(x, z)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
            }
        }

        return uvs;
    }

    private int index(int x, int z)
    {
        return x * (Dimension + 1) + z;
    }
    // Overloaded Index for floats
    private int index(float x, float z)

    {
        return index((int)x, (int)z);
    }

    // Update is called once per frame
    void Update () {
        var verts = Mesh.vertices;
        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                var y = 0f;
                for (int n = 0; n < Octaves.Length; n++)
                {
                    if (Octaves[n].alternate)
                    {
                        var perl = Mathf.PerlinNoise((x * Octaves[n].scale.x) / Dimension, (z * Octaves[n].scale.y) / Dimension) * Mathf.PI * 2f;
                        y += Mathf.Cos(perl + Octaves[n].speed.magnitude * Time.time) * Octaves[n].height;
                    }
                    else
                    {
                        var perl = Mathf.PerlinNoise((x * Octaves[n].scale.x + Time.time * Octaves[n].speed.x) / Dimension, (z * Octaves[n].scale.y + Time.time * Octaves[n].speed.y) / Dimension) - 0.5f;
                        y += perl * Octaves[n].height;
                    }
                }
                verts[index(x, z)] = new Vector3(x, y, z);
            }
        }
        Mesh.vertices = verts;
        Mesh.RecalculateNormals();
	}

    [Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }
}
