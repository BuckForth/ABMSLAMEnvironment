using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HMChunk : MonoBehaviour
{
    public float avgHeight = float.MinValue;
    public float maxHeight = float.MinValue;
    public float minHeight = float.MaxValue;
    public Vector2Int chunkCord = Vector2Int.zero;
    public int subdivisions = 32;
    public float tileSize = 10;
 

    public NoiseGenerator noise;

    public Mesh mesh;
    public List<Vector3> vertices;
    public List<Vector2> uvs;
    public List<int> trigs;

    private void Start()
    {
        //updateMesh();//temporary for testing single chunks
    }

    public void setNoise(NoiseGenerator newNoise)
    {
        noise = newNoise; 
    }

    public void updateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> trigs = new List<int>();
        float heightTally = 0f;
        maxHeight = float.MinValue;
        minHeight = float.MaxValue;
        //calculate vertices && UVs
        for (int vX = 0; vX < subdivisions + 1; vX++)
        {
            for (int vY = 0; vY < subdivisions + 1; vY++)
            {
                float posX = ((chunkCord.x * tileSize) - (tileSize / 2f)) + ((tileSize / (float)subdivisions) * vX);
                float posY = ((chunkCord.y * tileSize) - (tileSize / 2f)) + ((tileSize / (float)subdivisions) * vY);
                if (vY % 2 == 1)
                {
                    posX += ((tileSize / (float)subdivisions) * 0.5f);
                }
                float posZ = noise.getHeight(posX, posY);
                vertices.Add(new Vector3(posX, posZ, posY));
                heightTally += posZ;
                if (posZ < minHeight) { minHeight = posZ; }
                if (posZ > maxHeight) { maxHeight = posZ; }
                float uvX = (float)vX / subdivisions + 1;
                float uvY = (float)vY / subdivisions + 1;
                if (vY % 2 == 1)
                {
                    uvX += ((1f / (float)subdivisions) * 0.5f);
                }
                uvs.Add(new Vector2(uvX, uvY));
            }
        }
        avgHeight = heightTally / vertices.Count;
        //calculate triangles
        for (int vX = 0; vX < subdivisions; vX++)
        {
            for (int vY = 0; vY < subdivisions; vY++)
            {
                int index1 = (vX * (subdivisions + 1)) + vY;
                int index2 = (vX * (subdivisions + 1)) + vY+1;
                int index3 = ((vX + 1) * (subdivisions + 1)) + vY;
                int index4 = ((vX + 1) * (subdivisions + 1)) + vY+1;
                if (vY % 2 == 0)
                {
                    //trig 1/2
                    trigs.Add(index1);
                    trigs.Add(index2);
                    trigs.Add(index3);
                    //trig 2/2
                    trigs.Add(index2);
                    trigs.Add(index4);
                    trigs.Add(index3);
                }
                else
                {
                    //trig 1/2
                    trigs.Add(index1);
                    trigs.Add(index2);
                    trigs.Add(index3);
                    //trig 2/2
                    trigs.Add(index2);
                    trigs.Add(index4);
                    trigs.Add(index3);
                }

            }
        }
        //Update mesh properties
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        if (mesh == null)
        {
            mesh = new Mesh();
        }
        mesh.Clear();
        mesh.SetVertices(vertices.ToArray());
        mesh.SetTriangles(trigs.ToArray(), 0);
        mesh.SetUVs(0, uvs.ToArray());
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.UploadMeshData(false);
        MeshCollider col = GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = mesh;
        }
    }
}