using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;

public class AgentABM{
    private static int agentCount = 0;
    public int agentID = -1;
	public float metabolicEnergy = 5f;
	public Vector2Int ABMPosition = new Vector2Int();
	public float mutationIndex = 0.1f;
	public float reproThreshhold = 20f;
	public float greed = 0.3f;
	public int breedDelay = 100;
	private int lastBread = 0;
	public int deadframes = 60;
	public bool isDead = false;
	public bool isPred = false;
	public float generationCost=0.02f;
    public GameObject dummyBody;
    public Camera dummyCam;
    public ABMController cont;
    public Color colorCode;
    public int visSegments = 5;
    public bool isChosen = false;

    public ControlMode controlMode;

    private Neighbourhood neighbourhood;
    public AgentDecisionNetwork brain;

    RenderTexture viewPoint;

    private Vector3 currentWorldPosition;
    private float currentWorldRotation;
    public ComputeShader CNNcompute;
    public SLAMModel slamModel = null;

    public static int nextID()
    {
        agentCount++;
        return agentCount;
    }


    public void CallCNN(Texture2D texture, string dirPath)
    {
        int kernelHandle = CNNcompute.FindKernel("CSMain");
        //layer1
        RenderTexture featuremap = new RenderTexture(240 - 2, 160 - 2, 24);
        RenderTexture featuremapb = new RenderTexture(240 - 2, 160 - 2, 24);
        RenderTexture poolingText = new RenderTexture((int)(240 - 2) / 3, (int)(160 - 2) / 3, 24);
        RenderTexture poolingTextb = new RenderTexture((int)(240 - 2) / 3, (int)(160 - 2) / 3, 24);
        //layer2
        RenderTexture featuremap2Text = new RenderTexture(((int)(240 - 2) / 3) - 2, ((int)(160 - 2) / 3) - 2, 24);
        RenderTexture pooling2Text = new RenderTexture((int)(((int)(240 - 2) / 3) - 2) / 3, (int)(((int)(160 - 2) / 3) - 2) / 3, 24);
        RenderTexture featuremap3Text = new RenderTexture(((int)(240 - 2) / 3) - 2, ((int)(160 - 2) / 3) - 2, 24);
        RenderTexture pooling3Text = new RenderTexture((int)(((int)(240 - 2) / 3) - 2) / 3, (int)(((int)(160 - 2) / 3) - 2) / 3, 24);
        RenderTexture featuremap4Text = new RenderTexture(((int)(240 - 2) / 3) - 2, ((int)(160 - 2) / 3) - 2, 24);
        RenderTexture pooling4Text = new RenderTexture((int)(((int)(240 - 2) / 3) - 2) / 3, (int)(((int)(160 - 2) / 3) - 2) / 3, 24);
        RenderTexture featuremap5Text = new RenderTexture(((int)(240 - 2) / 3) - 2, ((int)(160 - 2) / 3) - 2, 24);
        RenderTexture pooling5Text = new RenderTexture((int)(((int)(240 - 2) / 3) - 2) / 3, (int)(((int)(160 - 2) / 3) - 2) / 3, 24);
        RenderTexture featuremap6Text = new RenderTexture(((int)(240 - 2) / 3) - 2, ((int)(160 - 2) / 3) - 2, 24);
        RenderTexture pooling6Text = new RenderTexture((int)(((int)(240 - 2) / 3) - 2) / 3, (int)(((int)(160 - 2) / 3) - 2) / 3, 24);
        RenderTexture featuremap7Text = new RenderTexture(((int)(240 - 2) / 3) - 2, ((int)(160 - 2) / 3) - 2, 24);
        RenderTexture pooling7Text = new RenderTexture((int)(((int)(240 - 2) / 3) - 2) / 3, (int)(((int)(160 - 2) / 3) - 2) / 3, 24);
        //layer1
        featuremap.enableRandomWrite = true;
        featuremap.Create();
        featuremapb.enableRandomWrite = true;
        featuremapb.Create();
        poolingText.enableRandomWrite = true;
        poolingText.Create();
        poolingTextb.enableRandomWrite = true;
        poolingTextb.Create();
        //layer2
        featuremap2Text.enableRandomWrite = true;
        featuremap2Text.Create();
        pooling2Text.enableRandomWrite = true;
        pooling2Text.Create();
        featuremap3Text.enableRandomWrite = true;
        featuremap3Text.Create();
        pooling3Text.enableRandomWrite = true;
        pooling3Text.Create();
        featuremap4Text.enableRandomWrite = true;
        featuremap4Text.Create();
        pooling4Text.enableRandomWrite = true;
        pooling4Text.Create();
        featuremap5Text.enableRandomWrite = true;
        featuremap5Text.Create();
        pooling5Text.enableRandomWrite = true;
        pooling5Text.Create();
        featuremap6Text.enableRandomWrite = true;
        featuremap6Text.Create();
        pooling6Text.enableRandomWrite = true;
        pooling6Text.Create();
        featuremap7Text.enableRandomWrite = true;
        featuremap7Text.Create();
        pooling7Text.enableRandomWrite = true;
        pooling7Text.Create();
        //Set texutres
        CNNcompute.SetTexture(kernelHandle, "ImageInput", texture);
        //layer1
        CNNcompute.SetTexture(kernelHandle, "featureMap", featuremap);
        CNNcompute.SetTexture(kernelHandle, "featureMapb", featuremapb);
        CNNcompute.SetTexture(kernelHandle, "poolingMap", poolingText);
        CNNcompute.SetTexture(kernelHandle, "poolingMapb", poolingTextb);
        //layer2
        CNNcompute.SetTexture(kernelHandle, "featureMap2", featuremap2Text);
        CNNcompute.SetTexture(kernelHandle, "poolingMap2", pooling2Text);
        CNNcompute.SetTexture(kernelHandle, "featureMap3", featuremap3Text);
        CNNcompute.SetTexture(kernelHandle, "poolingMap3", pooling3Text);
        CNNcompute.SetTexture(kernelHandle, "featureMap4", featuremap4Text);
        CNNcompute.SetTexture(kernelHandle, "poolingMap4", pooling4Text);
        CNNcompute.SetTexture(kernelHandle, "featureMap5", featuremap5Text);
        CNNcompute.SetTexture(kernelHandle, "poolingMap5", pooling5Text);
        CNNcompute.SetTexture(kernelHandle, "featureMap6", featuremap6Text);
        CNNcompute.SetTexture(kernelHandle, "poolingMap6", pooling6Text);
        CNNcompute.SetTexture(kernelHandle, "featureMap7", featuremap7Text);
        CNNcompute.SetTexture(kernelHandle, "poolingMap7", pooling7Text);

        //Set Kernals
        ComputeBuffer kernal = new ComputeBuffer(10, Marshal.SizeOf(typeof(System.Single)), ComputeBufferType.Default);
        kernal.SetData(new float[] { -1f, -1f, -1f, -1f, 8f, -1f, -1f, -1f, -1f, 1f });//Testing Line finder
        CNNcompute.SetBuffer(kernelHandle, "KernalBuffer", kernal);

        ComputeBuffer kernalb = new ComputeBuffer(10, Marshal.SizeOf(typeof(System.Single)), ComputeBufferType.Default);
        kernalb.SetData(new float[] { -1f, -1f, -1f, -1f, 8f, -1f, -1f, -1f, -1f, 1f });//Testing Line finder
        CNNcompute.SetBuffer(kernelHandle, "KernalBufferb", kernalb);

        ComputeBuffer kernal2 = new ComputeBuffer(10, Marshal.SizeOf(typeof(System.Single)), ComputeBufferType.Default);
        kernal2.SetData(new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f / 9f });//Testing Gausian Blur
        CNNcompute.SetBuffer(kernelHandle, "KernalBuffer2", kernal2);

        ComputeBuffer kernal3 = new ComputeBuffer(10, Marshal.SizeOf(typeof(System.Single)), ComputeBufferType.Default);
        kernal3.SetData(new float[] { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f});//Testing identity
        CNNcompute.SetBuffer(kernelHandle, "KernalBuffer3", kernal3);

        ComputeBuffer kernal4 = new ComputeBuffer(10, Marshal.SizeOf(typeof(System.Single)), ComputeBufferType.Default);
        kernal4.SetData(new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f / 9f });//Testing Gausian Blur
        CNNcompute.SetBuffer(kernelHandle, "KernalBuffer4", kernal4);

        //Dispatch Kernal
        CNNcompute.Dispatch(kernelHandle, 512 / 8, 1024 / 8, 1);

        //Write output images
        RenderTexture.active = featuremap;
        Texture2D featuremapTexture = new Texture2D(240 - 4, 160 - 4);
        featuremapTexture.ReadPixels(new Rect(0, 0, featuremap.width, featuremap.height), 0, 0);
        featuremapTexture.Apply();

        byte[] imageBytes = featuremapTexture.EncodeToPNG();
        File.WriteAllBytes(dirPath + "_featuremap_1" + ".png", imageBytes);
        //*
        RenderTexture.active = poolingText;
        Texture2D poolingTextTexture = new Texture2D((int)(240 - 2) / 3, (int)(160 - 2) / 3);
        poolingTextTexture.ReadPixels(new Rect(0, 0, poolingText.width, poolingText.height), 0, 0);
        poolingTextTexture.Apply();

        imageBytes = poolingTextTexture.EncodeToPNG();
        File.WriteAllBytes(dirPath + "_featuremap_1_pool" + ".png", imageBytes);
        //
        RenderTexture.active = pooling2Text;
        Texture2D poolingText2Texture = new Texture2D((int)(((int)(240 - 2) / 3) - 2) / 3, (int)(((int)(160 - 2) / 3) - 2) / 3);
        poolingText2Texture.ReadPixels(new Rect(0, 0, pooling2Text.width, pooling2Text.height), 0, 0);
        poolingText2Texture.Apply();

        imageBytes = poolingText2Texture.EncodeToPNG();
        File.WriteAllBytes(dirPath + "_featuremap_2_pool" + ".png", imageBytes);
        //
        RenderTexture.active = featuremap2Text;
        Texture2D featureText2Texture = new Texture2D((int)(240 - 2) / 3, (int)(160 - 2) / 3);
        featureText2Texture.ReadPixels(new Rect(0, 0, featuremap2Text.width, featuremap2Text.height), 0, 0);
        featureText2Texture.Apply();

        imageBytes = featureText2Texture.EncodeToPNG();
        File.WriteAllBytes(dirPath + "_featuremap_2" + ".png", imageBytes);
        //
        RenderTexture.active = pooling6Text;
        Texture2D poolingText6Texture = new Texture2D((int)(((int)(240 - 2) / 3) - 2) / 3, (int)(((int)(160 - 2) / 3) - 2) / 3);
        poolingText6Texture.ReadPixels(new Rect(0, 0, pooling2Text.width, pooling2Text.height), 0, 0);
        poolingText6Texture.Apply();

        imageBytes = poolingText6Texture.EncodeToPNG();
        File.WriteAllBytes(dirPath + "_featuremap_6_pool" + ".png", imageBytes);
        //
        RenderTexture.active = featuremap6Text;
        Texture2D featureText6Texture = new Texture2D((int)(240 - 2) / 3, (int)(160 - 2) / 3);
        featureText6Texture.ReadPixels(new Rect(0, 0, featuremap6Text.width, featuremap6Text.height), 0, 0);
        featureText6Texture.Apply();

        imageBytes = featureText6Texture.EncodeToPNG();
        File.WriteAllBytes(dirPath + "_featuremap_6" + ".png", imageBytes);
        //*/

    }

    public List<AgentABM> getObservableAgents()
    {
        List<AgentABM> visableAgents = cont.VisableAgents(GeometryUtility.CalculateFrustumPlanes(dummyCam));
        List<AgentABM> observableAgents = new List<AgentABM>();
        foreach (AgentABM agent in visableAgents) if (agent.agentID != agentID)
            {
                Bounds bounds = agent.dummyBody.GetComponent<Collider>().bounds;

                Rect viewBox = GetBoundingBoxOnScreen2(bounds, dummyCam);
                Ray losRay = dummyCam.ScreenPointToRay(viewBox.center);
                RaycastHit losHit;
                if (Physics.Raycast(losRay, out losHit, 1000f)) if (losHit.collider.gameObject == agent.dummyBody.gameObject)
                {
                        if (!agent.isDead)
                        { ///Agent is visavble and living 
                            observableAgents.Add(agent);
                        }
                }
            }
        return observableAgents;
    }

    public void move(Vector2Int delta)
    {
        int maplength = ABMController.getABM().mapLength;
        Vector3 oldPosition = cont.tileToWorldPoint(ABMPosition) + (Vector3.up * 0.65f);
        ABMPosition += delta;
        ABMPosition.x = (ABMPosition.x + maplength) % maplength;
        ABMPosition.y = (ABMPosition.y + maplength) % maplength;
        Vector3 newPosition = cont.tileToWorldPoint(ABMPosition) + (Vector3.up *0.65f);
        float oldRotation = currentWorldRotation;
        float newRotation = oldRotation;
        if (delta.x == 1)
        {
            newRotation = 270+180;
        }
        else if (delta.y == -1)
        {
            newRotation = 0 + 180;
        }
        else if (delta.x == -1)
        {
            newRotation = 90 + 180;
        }
        else if (delta.y == 1)
        {
            newRotation = 180 + 180;
        }


        if (isChosen)
        {//*
         //Interpolate and generate visual data
            dummyCam.gameObject.SetActive(true);
            for (float ratio = 0f; ratio <= 1f; ratio += (1f / (float)visSegments))
            {
                Vector3 interPos = Vector3.Lerp(oldPosition, newPosition, ratio);
                float interAngle = Mathf.LerpAngle(oldRotation, newRotation, ratio);
                if (viewPoint == null)
                {
                    viewPoint = new RenderTexture(240, 160, 16);
                    dummyCam.targetTexture = viewPoint;
                }
                interPos.y = cont.terrainGen.getHeightAt(interPos.x, interPos.z);
                dummyBody.gameObject.transform.position = interPos;
                dummyBody.transform.rotation = Quaternion.Euler(0f, interAngle, 0f);
                dummyCam.Render();
                Texture2D viewPoint2D = toTexture2D(viewPoint);

                string dirPath = Application.dataPath + "/../SaveImages/agent" + agentID.ToString() + "/";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                //Write metadata
                List<AgentABM> visableAgents = cont.VisableAgents(GeometryUtility.CalculateFrustumPlanes(dummyCam));
                List<AgentABM> observableAgents = new List<AgentABM>();
                string metaData = "Global Position: " + this.dummyBody.transform.position.ToString() + "\n";
                metaData += "Global Rotation: " + this.dummyBody.transform.eulerAngles.ToString() + "\n";
                metaData += "Energy: " + this.metabolicEnergy + "\n\n";
                foreach (AgentABM agent in visableAgents) if (agent.agentID != agentID)
                    {
                        Bounds bounds = agent.dummyBody.GetComponent<Collider>().bounds;
                        //Rect viewBox = GetBoundingBoxOnScreen(agent.dummyBody.GetComponent<MeshFilter>().mesh, dummyCam);
                        Rect viewBox = GetBoundingBoxOnScreen2(bounds, dummyCam);
                        Ray losRay = dummyCam.ScreenPointToRay(viewBox.center);
                        RaycastHit losHit;
                        if (Physics.Raycast(losRay, out losHit, 1000f)) if (losHit.collider.gameObject == agent.dummyBody.gameObject)
                        {
                            observableAgents.Add(agent);
                            metaData += "Agent: " + agent.agentID + "\n";
                            if (agent.isPred) { metaData += "Type: Predator\n"; }
                            else { metaData += "Type: Prey\n"; }
                            metaData += "ViewBox:(" + viewBox.ToString() + ")\n";
                            metaData += "Bounds:" + bounds.ToString() + "\n";
                            if (agent.isDead)
                            { metaData += "Status: Dead\n"; }
                            else
                            { metaData += "Status: Alive\n"; }
                            Vector2Int screenSize = new Vector2Int(viewPoint2D.width, viewPoint2D.height);

                            Vector2 min = viewBox.min;
                            Vector2 max = viewBox.max;
                            if (ABMController.getABM().drawBoundingBox)
                            {
                                for (int xx = (int)min.x; xx < (int)max.x; xx++)
                                {
                                    int yy = (int)(min.y);
                                    viewPoint2D.SetPixel(xx, yy, Color.red);
                                    yy = (int)(max.y);
                                    viewPoint2D.SetPixel(xx, yy, Color.yellow);
                                }
                                for (int yy = (int)min.y; yy < (int)max.y; yy++)
                                {
                                    int xx = (int)(min.x);
                                    viewPoint2D.SetPixel(xx, yy, Color.blue);
                                    xx = (int)(max.x);
                                    viewPoint2D.SetPixel(xx, yy, Color.green);
                                }
                            }
                        }
                    }
                //if (ABMController.getABM().usingCNN)
                //{
                //    CallCNN(viewPoint2D, (dirPath + cont.iterationCount + "(" + ratio + ")"));
                //}
                viewPoint2D.Apply();
                byte[] imageBytes = viewPoint2D.EncodeToPNG();
                File.WriteAllBytes(dirPath + cont.iterationCount + "(" + ratio + ")" + ".png", imageBytes);
                File.WriteAllText(dirPath + cont.iterationCount + "(" + ratio + ")_meta" + ".txt", metaData);
            }
            dummyCam.gameObject.SetActive(false);
            //*/
        }
        dummyBody.gameObject.transform.position = newPosition;
        dummyBody.transform.rotation = Quaternion.Euler(dummyBody.transform.eulerAngles.x, newRotation, dummyBody.transform.eulerAngles.z);
        //Update exact position variable
        currentWorldPosition = newPosition;
        currentWorldRotation = newRotation;
    }

    public Rect GetBoundingBoxOnScreen2(Bounds bounds, Camera camera)
    {
        Vector3 max = camera.WorldToScreenPoint(bounds.max);
        Vector3 min = camera.WorldToScreenPoint(bounds.min);
        Rect retVal = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

        return retVal;
    }

        public Rect GetBoundingBoxOnScreen(Mesh mesh, Camera camera)
    {
        //https://gamedev.stackexchange.com/questions/187446/unity-how-to-get-the-visible-bounds-of-an-object-i-e-as-it-is-seen-from-the-c
        List<Vector3> vertices = new List<Vector3>();
        mesh.GetVertices(vertices);

        Rect retVal = Rect.MinMaxRect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 v = camera.WorldToScreenPoint(vertices[i]);
            if (v.x < retVal.xMin)
            {
                retVal.xMin = v.x;
            }

            if (v.y < retVal.yMin)
            {
                retVal.yMin = v.y;
            }

            if (v.x > retVal.xMax)
            {
                retVal.xMax = v.x;
            }

            if (v.y > retVal.yMax)
            {
                retVal.yMax = v.y;
            }
        }

        return retVal;
    }
   

    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(240, 160, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    public void Act(){
        if(agentID < 0)
        {
            agentID = nextID();
        }
        if (dummyCam == null && dummyBody != null)
        {
            dummyCam = dummyBody.GetComponentInChildren<Camera>();
            dummyCam.gameObject.SetActive(false);
        }
		if (IsHealthy ()) {
			applyUpkeepCost ();
            //View ABM
            neighbourhood = getControlNeighbourhood();
            //Control Unit
            if (controlMode == ControlMode.RandomMove)
            {
                eat();
                float val = Random.value;
                if (val < 0.25f)
                {
                    move(new Vector2Int(0, 1));
                }
                else if (val < 0.5f)
                {
                    move(new Vector2Int(1, 0));
                }
                else if (val < 0.75f)
                {
                    move(new Vector2Int(0, -1));
                }
                else
                {
                    move(new Vector2Int(-1, 0));
                }
                if (metabolicEnergy > reproThreshhold)
                {
                    if (lastBread > 0)
                    {
                        lastBread--;
                    }
                    else
                    {
                        reproduce();
                    }
                }
            }
            else if (controlMode == ControlMode.SLAM_integrated)
            {
                //get closest agent
                List<AgentABM> agents = getObservableAgents();
                AgentABM closest = null;
                int DistanceMH = int.MaxValue;
                foreach (AgentABM agent in agents)
                {
                    int distance = Mathf.Abs(agent.ABMPosition.x - ABMPosition.x) + Mathf.Abs(agent.ABMPosition.y - ABMPosition.y);
                    if (distance < DistanceMH && agent != this)
                    {
                        DistanceMH = distance;
                        closest = agent;
                    }
                }
                //update SLAM module
                if (slamModel == null)
                {
                    slamModel = new SLAMModel(100);
                }
                if (closest != null)
                {
                    slamModel.reWeight(closest.ABMPosition);
                }
                //Resample
                slamModel.reSample();
                //Predict
                slamModel.extrapolate();
                //If (any nearby spot is > 0 probabillity)
                //     Move towards min probabillity
                //else(
                float[] values = new float[4];
                values[0] = slamModel.querry(ABMPosition + new Vector2Int(0, 1));
                values[1] = slamModel.querry(ABMPosition + new Vector2Int(1, 0));
                values[2] = slamModel.querry(ABMPosition + new Vector2Int(0, -1));
                values[3] = slamModel.querry(ABMPosition + new Vector2Int(-1, 0));
                int choice = -1;
                float smallest = float.MaxValue;
                float largest = float.MinValue;
                for (int ii = 0; ii < values.Length; ii++)
                {
                    float value = values[ii];
                    if (value < smallest)
                    {
                        choice = ii;
                        smallest = value;
                    }
                    if(value > largest)
                    {
                        largest = value;
                    }
                }
                eat();
                if (largest < 0.1f)//If no threat, move randomly
                {
                    float val2 = Random.value;
                    if (val2 < 0.25f)
                    {
                        move(new Vector2Int(0, 1));
                    }
                    else if (val2 < 0.5f)
                    {
                        move(new Vector2Int(1, 0));
                    }
                    else if (val2 < 0.75f)
                    {
                        move(new Vector2Int(0, -1));
                    }
                    else
                    {
                        move(new Vector2Int(-1, 0));
                    }
                }
                else if (choice == 0)//Make an informed decision
                {
                    move(new Vector2Int(0, 1));
                }
                else if (choice == 1)
                {
                    move(new Vector2Int(1, 0));
                }
                else if (choice == 2)
                {
                    move(new Vector2Int(0, -1));
                }
                else
                {
                    move(new Vector2Int(-1, 0));
                }
                if (metabolicEnergy > reproThreshhold)
                {
                    if (lastBread > 0)
                    {
                        lastBread--;
                    }
                    else
                    {
                        reproduce();
                    }
                }

                //end of controlmode
            }
            else if (controlMode == ControlMode.BrainMove)
            {
                float[] output = brain.calculateZone(neighbourhood, this);
                int choice = -1;
                float highest = float.MinValue;
                for (int choiceII = 0; choiceII < output.Length; choiceII++)
                {
                    if (output[choiceII] > highest)
                    {
                        highest = output[choiceII];
                        choice = choiceII;
                    }
                }
                eat();
                if (lastBread > 0)
                {
                    lastBread--;
                }
                if (choice == 0)
                {
                    if (lastBread <= 0)
                    {
                        reproduce();
                    }
                }
                else if (choice == 1)
                {
                    float val = Random.value;
                    if (val < 0.25f)
                    {
                        move(new Vector2Int(0, 1));
                    }
                    else if (val < 0.5f)
                    {
                        move(new Vector2Int(1, 0));
                    }
                    else if (val < 0.75f)
                    {
                        move(new Vector2Int(0, -1));
                    }
                    else
                    {
                        move(new Vector2Int(-1, 0));
                    }
                }
                else if (choice == 2)
                {
                    move(new Vector2Int(0, 1));
                }
                else if (choice == 3)
                {
                    move(new Vector2Int(1, 0));
                }
                else if (choice == 4)
                {
                    move(new Vector2Int(0, -1));
                }
                else if (choice == 5)
                {
                    move(new Vector2Int(-1, 0));
                }

            }
            else if (controlMode == ControlMode.BrainMove2)
            {
                float[] output = brain.calculateZone(neighbourhood, this);
                int choice = -1;
                float highest = float.MinValue;
                for (int choiceII = 0; choiceII < output.Length; choiceII++)
                {
                    if (output[choiceII] > highest)
                    {
                        highest = output[choiceII];
                        choice = choiceII;
                    }
                }
                eat();
                if (lastBread > 0)
                {
                    lastBread--;
                }
                if (metabolicEnergy > reproThreshhold)
                {
                    if (lastBread > 0)
                    {
                        lastBread--;
                    }
                    else
                    {
                        reproduce();
                    }
                }
                if (choice == 1)
                {
                    float val = Random.value;
                    if (val < 0.25f)
                    {
                        move(new Vector2Int(0, 1));
                    }
                    else if (val < 0.5f)
                    {
                        move(new Vector2Int(1, 0));
                    }
                    else if (val < 0.75f)
                    {
                        move(new Vector2Int(0, -1));
                    }
                    else
                    {
                        move(new Vector2Int(-1, 0));
                    }
                }
                else if (choice == 2)
                {
                    move(new Vector2Int(0, 1));
                }
                else if (choice == 3)
                {
                    move(new Vector2Int(1, 0));
                }
                else if (choice == 4)
                {
                    move(new Vector2Int(0, -1));
                }
                else if (choice == 5)
                {
                    move(new Vector2Int(-1, 0));
                }

            }

            //Update Statistics
            ABMController control = ABMController.getABM ();
			control.globalPopulationCount++;
			if (isPred) {
                if (controlMode == ControlMode.SLAM_integrated)
                {
                    control.globalPredSLAMPopulationCount++;
                }
				control.globalPredPopulationCount++;
			} else {
                if (controlMode == ControlMode.SLAM_integrated)
                {
                    control.globalPreySLAMPopulationCount++;
                }
                control.globalPreyPopulationCount++;
			}

		}
		else {
			isDead = true;
            dummyBody.GetComponent<MeshRenderer>().material.color = new Color32(230, 0, 20, 255);
        }
	}

    public Neighbourhood getControlNeighbourhood()
    {
        return ABMController.getABM().giveNeighbourhood(ABMPosition.x, ABMPosition.y);
    }


    public void eat ()
	{
		if (isPred) {
			List<AgentABM> agents = ABMController.getAgentsNear (ABMPosition);
			bool eaten = false;
			for (int ii = 0; ii < agents.Count && !eaten; ii++) {
				if (agents [ii].isPred == false && agents [ii].isDead == false) {
					eaten = true;
					metabolicEnergy += (agents [ii].metabolicEnergy)/2f;
					agents [ii].metabolicEnergy = 0;
					agents [ii].isDead = true;
				}
			}

		} else {
			float eatAmount = ABMController.getABM ().map [ABMPosition.x, ABMPosition.y].isEaten (greed);
			metabolicEnergy += eatAmount;
		}
	}



	public void reproduce()
	{
		AgentABM newAgent = new AgentABM ();
		this.lastBread = this.breedDelay;
		newAgent.lastBread = this.breedDelay;
		newAgent.isPred = isPred;
		newAgent.ABMPosition = this.ABMPosition;
		newAgent.metabolicEnergy = this.metabolicEnergy/4f;
		this.metabolicEnergy = this.metabolicEnergy/2f;
        newAgent.generationCost = this.generationCost;
        newAgent.colorCode = colorCode;
        if (dummyBody != null)
        {
            dummyCam.gameObject.SetActive(true);
            GameObject newBody = GameObject.Instantiate(this.dummyBody);
            newAgent.dummyBody = newBody;
            newAgent.dummyBody.GetComponent<MeshRenderer>().material.color = colorCode;
            newAgent.dummyCam = newAgent.dummyBody.GetComponentInChildren<Camera>();
            newAgent.dummyCam.gameObject.SetActive(false);
            dummyCam.gameObject.SetActive(false);
        }
        newAgent.controlMode = this.controlMode;
		newAgent.breedDelay = this.breedDelay + (Random.Range (-5, 6));
		newAgent.mutationIndex = mutationIndex + ((Random.value-0.5f)*mutationIndex);
		newAgent.reproThreshhold = reproThreshhold + ((Random.value-0.5f)*mutationIndex);
        newAgent.greed = greed + ((Random.value - 0.5f) * mutationIndex);
        newAgent.cont = cont;
        newAgent.visSegments = visSegments;
        newAgent.CNNcompute = CNNcompute;
        if (newAgent.greed > 1) {
			newAgent.greed = 1;
		} else if (newAgent.greed < 0) {
			newAgent.greed = 0;
		}
        newAgent.brain = new AgentDecisionNetwork(this.brain, mutationIndex/10f);

        ABMController.getABM ().agents.Add (newAgent);
	}

	public bool	IsHealthy()
	{
		return (metabolicEnergy > 0f && !isDead);
	}

	public void applyUpkeepCost()
	{
		metabolicEnergy -= generationCost;
	}

    public string filePrefix()
    {
        return ABMController.filePrefix + agentID.ToString() + ".txt";
    }
}

public class Neighbourhood
{
	public ABMMapTile[,] map;
	public List<AgentABM> otherAgents;

	public Neighbourhood(int radius)
	{
		map = new ABMMapTile[radius*2+1,radius*2+1];
		otherAgents = new List<AgentABM> ();
	}
}

public class SLAMModel
{
    private Vector2Int[] samples = new Vector2Int[100];
    private float[] sampleWeights = new float[100];
    private float directionChance = 0.25f; // 1/4 possible directions each step

    public SLAMModel(int sampleCount)
    {
        Vector2Int[] samples = new Vector2Int[sampleCount];
        float[] sampleWeights = new float[sampleCount];
        for (int ii = 0; ii < sampleCount; ii++)
        {
            sampleWeights[ii] = 1f / sampleCount;
            samples[ii] = new Vector2Int(Random.Range(0, 100), Random.Range(0, 100));//Initialized as random distribution
        }
    }

    public void reWeight(Vector2Int closestSeenAgentPos)
    {
        //Calculate weights given observation
        float totalWeight = 0f;
        for (int ii = 0; ii < samples.Length; ii++)
        {
            int manhattanDistance = Mathf.Abs(closestSeenAgentPos.x - samples[ii].x);
            manhattanDistance += Mathf.Abs(closestSeenAgentPos.y - samples[ii].y);
            sampleWeights[ii] = Mathf.Pow(directionChance, manhattanDistance); //Lower odds for each step away from registered position
            totalWeight += sampleWeights[ii];
        }
        //Renormalize Distribution
        float finaltotalWeight = 0f;
        for (int ii = 0; ii < sampleWeights.Length; ii++)
        {
            sampleWeights[ii] = sampleWeights[ii] / totalWeight;
            finaltotalWeight += sampleWeights[ii];
        }//Weights now will sum to 1
        //Debug.Log(finaltotalWeight);
    }

    public void reSample()
    {
        Vector2Int[] newSamples = new Vector2Int[samples.Length];
        float[] newWeights = new float[samples.Length];
        for (int ii = 0; ii < samples.Length; ii++)
        {
            float select = Random.value;
            bool found = false;
            int jj = 0;
            while (!found)
            {
                select = select - sampleWeights[jj];
                if (select > 0)
                {
                    found = true;
                }
                else
                {
                    jj++;
                    if (jj >= samples.Length)
                    {
                        found = true;
                        jj--;
                    }
                }
            }
            newSamples[ii] = samples[jj];
            newWeights[ii] = 1f / samples.Length;
        }
        samples = newSamples;
        sampleWeights = newWeights;
    }

    public void extrapolate()
    {
        for (int ii = 0; ii < samples.Length; ii++)
        {
            float select = Random.value;
            if (select > 0.5f)//randomly select a single direction to move prediction to.
            {
                if (select > 0.75f)
                {
                    samples[ii].y = samples[ii].y + 1;
                }
                else
                {
                    samples[ii].y = samples[ii].y - 1;
                }
            }
            else
            {
                if (select > 0.25f)
                {
                    samples[ii].x = samples[ii].x + 1;
                }
                else
                {
                    samples[ii].x = samples[ii].x - 1;
                }
            }
        }
    }
    
    public float querry(Vector2Int position)
    {
        float probabillity = 0f;
        foreach (Vector2Int sample in samples)
        {
            if (sample.x == position.x && sample.y == position.y)
            {
                probabillity++;
            }
        }
        return probabillity / samples.Length;
    }
}



[System.Serializable]
public class AgentDecisionNetwork
{
    public Neighbourhood input;

    public int[] shape = { 51, 20 , 20, 5 };
    public List<List<List<float>>> weights = new List<List<List<float>>>();

    public AgentDecisionNetwork()
    {
        initializeRandomWeights();
    }

    public AgentDecisionNetwork(AgentDecisionNetwork other, float mutationIndex)
    {
        for (int ii = 0; ii < other.shape.Length;ii++)
        {
            shape[ii] = other.shape[ii];
        }
        weights = new List<List<List<float>>>();
        for (int layerii = 0; layerii < shape.Length - 1; layerii++)
        {
            List<List<float>> layerWeights = new List<List<float>>();
            for (int nodeII = 0; nodeII < shape[layerii]; nodeII++)
            {
                List<float> nodeWeights = new List<float>();
                for (int nextII = 0; nextII < shape[layerii + 1]; nextII++)
                {
                    nodeWeights.Add(other.weights[layerii][nodeII][nextII] + (Random.value - 0.5f) * mutationIndex);//Random value ranging from -(mutationindex/2) to (mutationindex/2 )
                }
                layerWeights.Add(nodeWeights);
            }
            weights.Add(layerWeights);
        }
    }

    public void initializeRandomWeights()
    {
        weights = new List<List<List<float>>>();
        for (int layerii = 0; layerii < shape.Length-1; layerii++)
        {
            List<List<float>> layerWeights = new List<List<float>>();
            for (int nodeII = 0; nodeII < shape[layerii]; nodeII++)
            {
                List<float> nodeWeights = new List<float>();
                for(int nextII = 0; nextII < shape[layerii+1]; nextII++)
                {
                    nodeWeights.Add((Random.value - 0.5f) * 0.125f);//Random value ranging from -0.25 to 0.25
                }
                layerWeights.Add(nodeWeights);
            }
            weights.Add(layerWeights);
        }

    }

    public float[] calculateZone(Neighbourhood neighbourhood, AgentABM self)
    {
        List<float> rVal = new List<float>();
        //Set up input from neighbourhood object
        List<float> inputValues = new List<float>();
        inputValues.Add(self.metabolicEnergy);
        for (int ix = 0; ix < neighbourhood.map.GetLength(0); ix++)
        {
            for (int iy = 0; iy < neighbourhood.map.GetLength(1); iy++)
            {
                ABMMapTile tile = neighbourhood.map[ix, iy];
                if (tile == null)
                {
                    Debug.Log("Error Tile(" + ix + "," + iy + ") set to null");
                }
                else
                {
                    inputValues.Add(tile.currFood);
                }
            }
        }
        for (int ix = 0; ix < neighbourhood.map.GetLength(0); ix++)
        {
            for (int iy = 0; iy < neighbourhood.map.GetLength(1); iy++)
            {
                foreach(AgentABM agent in neighbourhood.otherAgents) 
                {
                    int foundVal = 0;
                    if (agent.ABMPosition.x == ix && agent.ABMPosition.y == iy && agent != self)
                    {
                        foundVal = 1;
                        if (agent.isPred != self.isPred)
                        {
                            foundVal = -1;
                        }
                    }
                    inputValues.Add(foundVal);
                }
            }
        }
        //Evaluate each layer
        List<List<float>> values = new List<List<float>>();
        float[] pastValues = inputValues.ToArray();
        float[] nextValues = new float[0];
        for (int ii = 0; ii < shape.Length-1; ii++)
        {
            nextValues = new float[shape[ii+1]];
            for (int nodeII = 0; nodeII < shape[ii]; nodeII++)
            {
                for (int nextII = 0; nextII < nextValues.Length; nextII++)
                {
                    nextValues[nextII] += (pastValues[nodeII] * weights[ii][nodeII][nextII]);
                }
            }
            pastValues = nextValues;
        }
        return nextValues;
    }
}

public enum ControlMode
{
    RandomMove,
    BrainMove,
    BrainMove2,
    SLAM_integrated
};