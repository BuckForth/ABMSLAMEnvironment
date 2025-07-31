using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightmapWorld : MonoBehaviour
{

    public int subdivisions = 32;
    public float tileSize = 10;
    public NoiseGenerator noise;

    public Material grassMat;

    public int tileCountX = 100;
    public int tileCountY = 100;

    private List<GameObject> currchunks;
    // Start is called before the first frame update
    void Start()
    {
        //generateWorld();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float getHeightAt(float xx, float yy)
    {
        return noise.getHeight(xx, yy);
    }

    public void generateWorld()
    {
        for (int ii = 0; currchunks!= null && ii < currchunks.Count; ii++)
        {
            Destroy(currchunks[ii]);
        }
        currchunks = new List<GameObject>();

        for (int cx = -tileCountX / 2; cx < tileCountX / 2; cx++)
        {
            for (int cy = -tileCountY / 2; cy < tileCountY / 2; cy++)
            {
                GameObject newChunkGO = new GameObject("Chunk_(" + cx + ", " + cy + ")");
                newChunkGO.AddComponent<MeshFilter>();
                newChunkGO.AddComponent<MeshRenderer>();
                newChunkGO.GetComponent<MeshRenderer>().material = grassMat;
                newChunkGO.AddComponent<MeshCollider>();
                HMChunk newChunk = newChunkGO.AddComponent<HMChunk>();
                newChunk.chunkCord = new Vector2Int(cx, cy);
                newChunk.setNoise(noise);
                newChunk.tileSize = tileSize;
                newChunk.subdivisions = subdivisions;
                newChunk.updateMesh();
                currchunks.Add(newChunkGO);
            }
        }
    }
}
