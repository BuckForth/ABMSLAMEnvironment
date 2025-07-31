using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;


public class ABMController : MonoBehaviour {
    public int numChosen = 1;
    private static ABMController singleton = null;
    public static string filePrefix = "AgentPaths\\agent_";
    public float timeDelta = 4.0f / 60.0f; //4/60 = 15 fps
    public int mapLength = 100;
    public float cellScale = 10;
    public List<AgentABM> chosenAgents;

    public int visionSegments = 5;
    public ControlMode globalControlMode;
    public int predCount = 8;
    public int preyCount = 60;
    public float upkeepCost = 0.02f;
    public int initialBreedDelay = 100;
    public List<AgentABM> agents;
    public int agentABMVisionDist = 2;

    public ABMMapTile[,] map;
    public Color theColor = new Color32(255, 0, 255, 255);
    public GameObject miniMap;
    private Texture2D miniMapImage;
    public Gradient grassColorFood;

    public float grassRegrowth = 0.01f;
    // Use this for initialization

    public int iterationCount = 0;

    public int globalPopulationCount = 0;
    private List<int> popCountHistory = new List<int>();

    public int globalPreyPopulationCount = 0;
    private List<int> popPreyCountHistory = new List<int>();

    public int globalPredPopulationCount = 0;
    private List<int> popPredCountHistory = new List<int>();

    public int globalPreySLAMPopulationCount = 0;
    private List<int> popPreySLAMCountHistory = new List<int>();

    public int globalPredSLAMPopulationCount = 0;
    private List<int> popPredSLAMCountHistory = new List<int>();


    public bool useSLAM = true;
    public Color PredColor = new Color32(230, 0, 160, 255);
    public Color PredColorSLAM = new Color32(230, 0, 160, 255);
    public Color PreyColor = new Color32(0, 160, 230, 255);
    public Color PreyColorSLAM = new Color32(0, 160, 230, 255);
    Color deadColor = new Color32(230, 0, 20, 255);
    public GameObject dummyTemplate;

    public Text metricReadoutObj;

    public ABMMetricChart preyChart;
    public ABMMetricChart predChart;
    public ABMMetricChart predDChart;
    public ABMMetricChart preyDChart;

    public HeightmapWorld terrainGen;
    private float tileLength;

    public bool drawBoundingBox = false;
    public bool usingCNN = true;
    public ComputeShader CNNcompute;

    public int trialNumb = 0;

    void writeOut()
    {
        string output = "" + globalPopulationCount + " , " + globalPreyPopulationCount + " , " + globalPredPopulationCount + " , " + globalPreySLAMPopulationCount + " , " + globalPredSLAMPopulationCount + " , " + iterationCount + "\n";

        string outPath = Application.dataPath + "/../Trails/";
        if (!Directory.Exists(outPath))
        {
            Directory.CreateDirectory(outPath);
        }
        File.AppendAllText(outPath + "trial.txt", output);
    }

    void Start() {
        trialNumb = Random.Range(0, 100000);
        chosenAgents = new List<AgentABM>();
        for (int ii = 0; ii < numChosen; ii++)
        {
            chosenAgents.Add(null);
        }
        if (singleton == null) {
            singleton = this;
            InitializeABM();
        } else {
            Destroy(this.gameObject);
        }
    }

    public List<AgentABM> VisableAgents(Plane[] planes)
    {
        List<AgentABM>  rVal= new List<AgentABM>();
        foreach (AgentABM agent in agents)
        {
            if (GeometryUtility.TestPlanesAABB(planes, agent.dummyBody.GetComponent<Collider>().bounds))
            {
                rVal.Add(agent);
            }
        }
        return rVal;
    }

    public void UpdateChosen()
    {
        for (int ii = 0; ii < chosenAgents.Count; ii++)
        {
            if (chosenAgents[ii] == null || chosenAgents[ii].isDead == true)
            {
                chosenAgents.RemoveAt(ii);
                bool newAgent = false;
                while (!newAgent)
                {
                    AgentABM newChosen = agents[Random.Range(0, agents.Count)];
                    if (!isChosen(newChosen))
                    {
                        newAgent = true;
                        chosenAgents.Add(newChosen);
                        newChosen.isChosen = true;
                    }
                }
            }
        }
    }

    public bool isChosen(AgentABM agent)
    {
        bool isAgent = false;
        for (int ii = 0; ii < chosenAgents.Count && !isAgent; ii++)
        {
            if (chosenAgents[ii] == agent)
            {
                isAgent = true;
            }
        }
        return isAgent;
    }

    public Vector3 tileToWorldPoint(Vector2Int pos)
    {
        Vector3 rval = Vector3.zero;
        if (terrainGen != null)
        {
            rval.x = (tileLength / 2f) + (tileLength * (pos.x)) - (tileLength * mapLength / 2);
            rval.z = (tileLength / 2f) + (tileLength * (pos.y)) - (tileLength * mapLength / 2);
            rval.y = terrainGen.getHeightAt(rval.x, rval.z);
        }
        return rval;
    }

    public Vector2Int worldPointToTile(Vector3Int pos)
    {
        Vector2Int rval = Vector2Int.zero;
        if (terrainGen != null)
        {
            rval.x = (int)((pos.x - (0.5f * tileLength)) / tileLength);
            rval.y = (int)((pos.z - (0.5f * tileLength)) / tileLength);
        }
        return rval;
    }

    void UpdateOverworld()
    {
        //foreach (AgentABM agent in agents)
       // {
       //     Vector3 worldpos = tileToWorldPoint(agent.ABMPosition);
       //     agent.dummyBody.transform.position = worldpos;
       // }
    }

    void InitializeABM()
    {
        if (terrainGen != null)
        {
            terrainGen.generateWorld();
        }
        miniMapImage = new Texture2D(mapLength, mapLength);
        miniMapImage.filterMode = FilterMode.Point;
        map = new ABMMapTile[mapLength, mapLength];
        agents = new List<AgentABM>();
        for (int predCNT = 0; predCNT < predCount; predCNT++) {
            AgentABM agent = new AgentABM();
            agent.isPred = true;
            agent.metabolicEnergy *= 2f;//Suspect
            agent.reproThreshhold = 50f;
            agent.ABMPosition = new Vector2Int((int)(Random.value * mapLength), (int)(Random.value * mapLength));
            agent.breedDelay = initialBreedDelay;
            agent.brain = new AgentDecisionNetwork();
            agent.controlMode = globalControlMode;
            agent.generationCost = upkeepCost;
            agent.dummyBody = Instantiate(dummyTemplate);
            agent.dummyBody.GetComponent<MeshRenderer>().material.color = PredColor;
            agent.colorCode = PredColor;
            agent.cont = this;
            agents.Add(agent);
            agent.visSegments = visionSegments;
            agent.CNNcompute = CNNcompute;
            if(useSLAM == true && predCNT < predCount/2)
            {
                agent.controlMode = ControlMode.SLAM_integrated;
                agent.colorCode = PredColorSLAM;
            }
        }
        for (int preyCNT = 0; preyCNT < preyCount; preyCNT++) {
            AgentABM agent = new AgentABM();
            agent.isPred = false;
            agent.reproThreshhold = 50f;
            agent.ABMPosition = new Vector2Int((int)(Random.value * mapLength), (int)(Random.value * mapLength));
            agent.breedDelay = initialBreedDelay;
            agent.brain = new AgentDecisionNetwork();
            agent.controlMode = globalControlMode;
            agent.generationCost = upkeepCost;
            agent.dummyBody = Instantiate(dummyTemplate);
            agent.dummyBody.GetComponent<MeshRenderer>().material.color = PreyColor;
            agent.colorCode = PreyColor;
            agent.cont = this;
            agents.Add(agent);
            agent.visSegments = visionSegments;
            agent.CNNcompute = CNNcompute;
            if(useSLAM == true && preyCNT < preyCount / 2)
            {
                agent.controlMode = ControlMode.SLAM_integrated;
                agent.colorCode = PreyColorSLAM;
            }
        }

        //Time.timeScale (0);
        //Initialize ABM-scale map
        for (int iy = 0; iy < mapLength; iy++) {
            for (int ix = 0; ix < mapLength; ix++) {
                float rr = grassRegrowth * Time.deltaTime;
                float th = 0f;
                map[ix, iy] = new ABMMapTile(rr, th);
            }
        }
        //Init meshGenerator settings
        if (terrainGen != null)
        {
            tileLength = terrainGen.tileSize;
            terrainGen.tileCountX = mapLength;
            terrainGen.tileCountY = mapLength;
        }

        //Init GUI
        UpdateMiniMap();
        SimStart();
    }

	void UpdateMetrics()
	{
        if (iterationCount >= 100000 || (globalPredPopulationCount == 0 && iterationCount > 10) || (globalPreyPopulationCount == 0 && iterationCount > 10))
        {
            writeOut();
            Debug.Log("writeout");
            SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
            // Debug.Break();
            DestroyImmediate(this.gameObject);
        }

        iterationCount++;
		string s = "";
		s += "iteration count: " + iterationCount + "\n";
		s += ("Population Count: " + globalPopulationCount + "(" + globalPreyPopulationCount + " prey| " + globalPredPopulationCount + " pred)\n");

		popCountHistory.Add (globalPopulationCount);
		globalPopulationCount = 0;

        popPredCountHistory.Add(globalPredPopulationCount);
        globalPredPopulationCount = 0;
        popPredSLAMCountHistory.Add(globalPredSLAMPopulationCount);
        globalPredSLAMPopulationCount = 0;

        popPreyCountHistory.Add(globalPreyPopulationCount);
        globalPreyPopulationCount = 0;
        popPreySLAMCountHistory.Add(globalPreySLAMPopulationCount);
        globalPreySLAMPopulationCount = 0;


        metricReadoutObj.text = s;

		preyChart.updateTexture (popPreyCountHistory, popPreyCountHistory.Count);
		predChart.updateTexture (popPredCountHistory, popPredCountHistory.Count);
		preyDChart.updateTexture (popPreySLAMCountHistory, popPreySLAMCountHistory.Count);
		predDChart.updateTexture (popPredSLAMCountHistory, popPredSLAMCountHistory.Count);
	}

	void UpdateMiniMap()
	{
		Texture2D texture = miniMapImage;
		miniMap.GetComponent<Renderer>().material.mainTexture = miniMapImage;
		for (int iy = 0; iy < texture.height; iy++)
		{
			for (int ix = 0; ix < texture.width; ix++)
			{
				float fvalue = map [ix,iy].getMinMapTileColor ();
				Color color = grassColorFood.Evaluate (fvalue);
				//color = ((ix & iy) != 0 ? Color.white : Color.gray);
				texture.SetPixel(ix, iy, color);
			}
		}
		for(int ii = 0; ii < agents.Count; ii++) {
			AgentABM agent = agents [ii];
			if (!agent.isDead) {
				if (agent.isPred) {
                    if (agent.controlMode == ControlMode.SLAM_integrated)
                    {
                        texture.SetPixel(agent.ABMPosition.x, agent.ABMPosition.y, PredColorSLAM);
                    }
                    else
                    {
                        texture.SetPixel(agent.ABMPosition.x, agent.ABMPosition.y, PredColor);
                    }
				} else {
                    if (agent.controlMode == ControlMode.SLAM_integrated)
                    {
                        texture.SetPixel(agent.ABMPosition.x, agent.ABMPosition.y, PreyColorSLAM);
                    }
                    else
                    {
                        texture.SetPixel(agent.ABMPosition.x, agent.ABMPosition.y, PreyColor);
                    }
				}
			} else {
				texture.SetPixel (agent.ABMPosition.x, agent.ABMPosition.y, deadColor);
			}
		}
		texture.Apply();
	}

	void SimStart()
	{

	}

	void Update()
	{
		if (singleton == this) {
			UpdateStep ();
		}
	}

	void UpdateStep () {

        UpdateChosen();
		List<AgentABM> newList = new List<AgentABM> ();
        for (int ii = 0; ii < agents.Count; ii++) {
            AgentABM agent = agents[ii];
            agent.Act();
            if (agent.isDead) {
                agent.deadframes--;
            }
            if (agent.deadframes > 0) {
                newList.Add(agent);
            }
            else
            {
                Destroy(agent.dummyBody);
            }
		}
		for (int xx = 0; xx < mapLength; xx++) {
			for (int yy = 0; yy < mapLength; yy++) {
				map [xx,yy].tileUpdate ();
			}
		}
        UpdateMetrics ();
        UpdateMiniMap();
        UpdateOverworld();
        agents = newList;
        
	}

	public static List<AgentABM> getAgentsNear(Vector2Int position)
	{
		ABMController controller = getABM ();
		List<AgentABM> rList = new List<AgentABM> ();
		foreach (AgentABM agent in controller.agents)
		{
			if ((agent.ABMPosition.x == position.x && (Mathf.Abs(agent.ABMPosition.y - position.y) < 2)) || (agent.ABMPosition.y == position.y && (Mathf.Abs(agent.ABMPosition.x - position.x) < 2)))
			{
				rList.Add (agent);
			}
		}
		return rList;
	}

	public Neighbourhood giveNeighbourhood(int xPos, int yPos)
	{
		Neighbourhood rVal = new Neighbourhood (agentABMVisionDist);
        for (int ix = 0; ix < (2 * agentABMVisionDist) + 1; ix++)
        {
            for (int iy = 0; iy < (2 * agentABMVisionDist) + 1; iy++)
            {
                int globalX = (mapLength+(ix + (xPos - agentABMVisionDist))) % mapLength;
                int globalY = (mapLength+(iy + (yPos - agentABMVisionDist))) % mapLength;
                rVal.map[ix, iy] = map[globalX, globalY];
            }
        }

       // for (int xx = xPos - agentABMVisionDist; xx < xPos + agentABMVisionDist; xx++)
       // {
       //     for (int yy = yPos - agentABMVisionDist; yy < yPos + agentABMVisionDist; yy++)
       //     {
       //         rVal.map[xx - (xPos - agentABMVisionDist), yy - (yPos - agentABMVisionDist)] = map[xx % mapLength, yy % mapLength];
       //     }
       // }
        foreach (AgentABM agent in agents)
        {
            if (Mathf.Abs(agent.ABMPosition.x - xPos) < 3 && Mathf.Abs(agent.ABMPosition.y - yPos) < 3)
            {
                rVal.otherAgents.Add(agent);
            }
        }
		return rVal;
	}

	public static ABMController getABM()
	{		return singleton;	}

}
