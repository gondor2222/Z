using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class Data : MonoBehaviour {

    private enum SCENE {MAIN, MAP};
    protected enum Goal { LEFT, RIGHT, UP, DOWN, FORWARD, BACKWARD, PITCHUP, PITCHDOWN, YAWLEFT, YAWRIGHT, ROLLLEFT, ROLLRIGHT };
    protected class GoalArrow
    {
        public float progress;
        private GameObject arrow;
        private GameObject label;
        public Goal goal;
        private string[] labelTexts = new string[] {
            "A to move left", "D to move right", "R to move up", "F to move down", "W to move forward", "S to move backward",
            "Drag mouse up to rotate up", "Drag mouse down to rotate down", "Drag mouse left to rotate left", "Drag mouse right to rotate right",
            "Q to roll left", "E to roll right"
        };
        public GoalArrow(GameObject player, Goal goal, Vector3 position, Vector3 view, Vector3 up)
        {
            this.goal = goal;
            progress = 0;
            arrow = new GameObject("Arrow_" + System.Enum.GetName(typeof(Goal), (int)goal), typeof(MeshFilter), typeof(MeshRenderer));
            Mesh arrowMesh = (Mesh)Resources.Load("Objects/arrow", typeof(Mesh));
            Debug.Log(arrowMesh.name);
            arrow.GetComponent<MeshRenderer>().material = Resources.Load("materials/Materials/Layer_1Mat") as Material;
            arrow.GetComponent<MeshFilter>().mesh = arrowMesh;
            arrow.transform.position = position;
            arrow.transform.rotation = Quaternion.LookRotation(view, up);
            arrow.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            arrow.transform.parent = player.transform;
            label = new GameObject(arrow.name + "_text");
            label.transform.parent = arrow.transform;
            TextMesh textMesh = label.AddComponent<TextMesh>();
            textMesh.text = labelTexts[(int)goal];
            textMesh.fontSize = 24;
            textMesh.color = new Color(1, 1, 1, 1);
            textMesh.transform.position = position;
            textMesh.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            label.transform.parent = player.transform;
            textMesh.transform.LookAt(player.transform.position + 10 * player.transform.forward, player.transform.up);


        }
        public void Destroy()
        {
            GameObject.Destroy(label);
            GameObject.Destroy(arrow);
        }
        public void UpdateProgress()
        {
            arrow.GetComponent<MeshRenderer>().material.color = new Color(1 - progress, 1, 1 - progress, 0.5f);
        }
    }
    private static readonly float CREDITS_SCROLL_SPEED = 1.2f;
    private List<GameObject> nuclei;
    private List<GameObject> mapNuclei;
    private List<GameObject> mapObjects;
    private GameObject credits1;
    private GameObject forwardIndicator;
    private GameObject backwardIndicator;
    private GameObject creditsCanvas;
    private GameObject pauseMenu;
    private GameObject infoPanel;
    private GameObject synthesisTypePanel;
    private Texture2D velocityIcon1;
    private Texture2D velocityIcon2;
    private List<GameObject> mapParticles;
    private List<GoalArrow> tutorialArrows;
    private float progressPerFrame = 0.01f;
    private float timer;
    private int tutorialPhase = -1;
    private int creditsStart = -1;
    private GameObject ZLabel;
    private static readonly float timerStart = 20;
    private int synthesisType;
    private static readonly float[] timerRates = {1, 1, 3, 5};
    private static readonly string[] synthesisTypes = {
        "PP / CNO", "Alpha Process", "S process", "R process"
    };
    private static readonly int maxParticles = 1500;
    private Vector3 credits1Start = new Vector3(0, Screen.height, 0);
    private float creditsPosition;

    private static readonly Properties.Del[] colorFunctions = new Properties.Del[] {
        delegate(int Z, int N, int E) { //Z
            return new Color(1,1,1);
        },
        delegate(int Z, int N, int E) { //N
            return new Color(1,1,1);
        },
        delegate(int Z, int N, int E) { //weight
            return new Color(1,1,1);
        },
        delegate(int Z, int N, int E) { //shell group
            return Constants.ShellColor(Z, N, E);
        },
        delegate(int Z, int N, int E) { //radius
            return new Color(1,1,1);
        },
        delegate(int Z, int N, int E) { //discovery year
            return Constants.DiscoveryColor(Z, N, E);
        },
        delegate(int Z, int N, int E) { //melting point
            return (Z) > 99 ? new Color(1,0,1) : (Z + N) > 0 ? Constants.MeltingColor(Z) : new Color(1,1,1);
        },
        delegate(int Z, int N, int E) { //boiling point
            return (Z) > 99 ? new Color(1,0,1) : (Z + N) > 0 ? Constants.BoilingColor(Z) : new Color(1,1,1);
        },
        delegate(int Z, int N, int E) { //half-life
            return Constants.MainHalfLifeColor(Z, N, E);
        },
        delegate(int Z, int N, int E) { //abundance
            return (Z + N) > 0 ? Constants.AbundanceColor(Z) : new Color(1,1,1);
        },
        delegate(int Z, int N, int E) { //group
            return Constants.GroupColor(Z, N, E);
        },
        delegate(int Z, int N, int E) { //ionization energy
            return new Color(1,1,1);
        }
    };
    private static readonly Properties.Del2[] textFunctions = new Properties.Del2[] {
        delegate(int Z, int N, int E) { //Z
            string ret = Constants.MapLabelText(Z, N, E) + "\n";
            ret += (Z + N) > 0 ? Z.ToString() : "";
            return ret;
        },
        delegate(int Z, int N, int E) { //N
            string ret = Constants.MainLabelText(Z, N, E) + "\n";
            ret += (Z + N) > 0 ? Constants.bestN[Z].ToString() : "";
            return ret;
        },
        delegate(int Z, int N, int E) { //weight
            string ret = Constants.MapLabelText(Z, N, E) + "\n";
            if (Z + N > 0) {
                return ret + Constants.atomicWeights[Z].ToString();
            }
            else {
                return ret + Constants.electronMass_amu.ToString();
            }
        },
        delegate(int Z, int N, int E) { //shell group
            string ret = Constants.MapLabelText(Z, N, E) + "\n";
            ret += Z > 0 ? Constants.subshells[Z] : "";
            return ret;
        },
        delegate(int Z, int N, int E) { //radius
            string ret = Constants.MapLabelText(Z, N, E) + "\n";
            ret += (N + Z) > 0 ? Constants.radii[Z].ToString() + " pm" : "";
            return ret;
        },
        delegate(int Z, int N, int E) { //discovery year
            string ret = Constants.MapLabelText(Z, N, E) + "\n";
            if (N + Z > 0) {
                return ret + Constants.discoveryYears[Z];
            }
            else if (E == 1) {
                return ret + Constants.electronDiscoveryYear;
            }
            else {
                return ret + Constants.positronDiscoveryYear;
            }
        },
        delegate(int Z, int N, int E) { //melting point
            string ret = Constants.MapLabelText(Z, N, E) + "\n";
            ret += Z > 99 ? "Unknown" : (N + Z) > 0 ? Constants.meltingPoints[Z].ToString() + " K" : "";
            return ret;
        },
        delegate(int Z, int N, int E) { //boiling point
            string ret = Constants.MapLabelText(Z, N, E) + "\n";
            ret += Z > 99 ? "Unknown" : (N + Z) > 0 ? Constants.boilingPoints[Z].ToString() + " K": "";
            return ret;
        },
        delegate(int Z, int N, int E) { //half-life
            string ret = Constants.MainLabelText(Z, N, E) + "\n";
            ret+= Constants.GetFormattedLife(Z, N);
            return ret;
        },
        delegate(int Z, int N, int E) { //abundance
            string ret = Constants.MapLabelText(Z, N, E) + "\n"; ;
            ret += (Z + N) > 0 ? Constants.GetAbundance(Z) : "";
            return ret;
        },
        delegate(int Z, int N, int E) { //group
            string ret = Constants.MapLabelText(Z, N, E) + "\n";
            ret += (Z + N) > 0 ? Constants.typeNames2[Constants.elementType2[Z]] : "Leptons";
            return ret;
        },
        delegate(int Z, int N, int E) { //ionization energy
           string ret = Constants.MapLabelText(Z, N, E) + "\n";
           ret += Z > 104? "Unknown" : (Z + N) > 0 ? Constants.ionizationEnergies[Z].ToString() + " eV": "";
           return ret;
        }
    };
    private Texture2D minimap;
    private GUIStyle GUIstyle;
    private Texture2D crosshair;
    private Texture2D axis1;
    private Texture2D axis2;
    private Camera ca;
    private Camera mapCamera;
    int selectedMap;
    private bool paused;
    private SCENE scene;
    public static readonly Vector3[] mapDirections = new Vector3[]
    {
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 0),
        new Vector3(0, 0, -1),
        new Vector3(-1, 0, 0)
    };
    GameObject player;
    const float MAP_DISTANCE = 5;
    const float MAX_DISTANCE = 250;
    const float SPAWN_DISTANCE = 0.8f * MAX_DISTANCE;
    const float SPAWN_V_FACTOR = 25f;
    const int MAX_PARTICLES = 1000;
    const int AVG_SPAWN_VELOCITY = 1;
    public const int START_P = 98;
    public const int START_N = 138;
    public const int START_E = 96;

    void Awake()
    {
        ZLabel = GameObject.Find("ZLabel");
        ZLabel.SetActive(false);
        mapParticles = new List<GameObject>();
        tutorialArrows = new List<GoalArrow>();
        QualitySettings.antiAliasing = 4;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        Debug.Log(Constants.GetHalfLife(START_P, START_N));
        selectedMap = 0;
        synthesisType = 0;
        timer = timerStart;
        credits1 = GameObject.Find("Credits");
        creditsCanvas = GameObject.Find("CreditsCanvas");
        creditsCanvas.SetActive(false);
        forwardIndicator = GameObject.Find("ForwardIndicator");
        backwardIndicator = GameObject.Find("BackwardIndicator");
        pauseMenu = GameObject.Find("PauseMenu");
        infoPanel = GameObject.Find("Info Panel");
        synthesisTypePanel = GameObject.Find("SynthesisTypeLabel");
        if (infoPanel == null)
        {
            Debug.Log("info panel not found");
        }
        forwardIndicator.SetActive(false);
        backwardIndicator.SetActive(false);
        velocityIcon1 = new Texture2D(51, 51);
        velocityIcon2 = new Texture2D(51, 51);
        for (int i = 0; i < maxParticles; i++)
        {
            mapParticles.Add(RandMapParticle());
        }
    }

	void Start() {
        creditsPosition = -1;
        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        mapObjects = new List<GameObject>();
        scene = SCENE.MAIN;
        paused = false;
        mapCamera = GameObject.Find("MapCamera").GetComponent<Camera>();
        mapCamera.fieldOfView = 90 / Mathf.Sqrt(((float)mapCamera.pixelWidth / mapCamera.pixelHeight));

        pauseMenu.GetComponent<Canvas>().enabled = false;
        minimap = new Texture2D(31, 31);
        crosshair = new Texture2D(10, 10);
        axis1 = Resources.Load("materials/Zarrow") as Texture2D;
        axis2 = Resources.Load("materials/Narrow") as Texture2D;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Color c;
                c.r = 1 * (i/2 + j/2) % 2;
                c.g = c.r;
                c.b = c.g;
                if (i < 1 || i > 8 || j < 1 || j > 8)
                {
                    c.a = 1;
                }
                else
                {
                    c.a = 0;
                }
                crosshair.SetPixel(i, j, c);
            }
        }
        crosshair.Apply();
        minimap.filterMode = FilterMode.Point;
        Application.runInBackground = true;
        player = GameObject.Find("Player");
        player.GetComponent<Rigidbody>().detectCollisions = true;
        player.GetComponent<SphereCollider>().isTrigger = true;
        player.GetComponent<SphereCollider>().radius = 30;
        Properties p = player.AddComponent<Properties>() as Properties;
        p.SetColorFunction(delegate (int Z1, int N1, int E1) {
            return Constants.MainHalfLifeColor(Z1, N1, E1);
        });
        p.SetTextFunction(delegate (int Z1, int N1, int E1) {
            return Constants.MainLabelText(Z1, N1, E1);
        });

        p.ca = player.GetComponentInChildren<Camera>();
        ca = p.ca;
        ca.fieldOfView = 120 / ((float)ca.pixelWidth / ca.pixelHeight);
        p.isPlayer = true;
        p.SetProperties(START_P,START_N,START_E);
        nuclei = new List<GameObject>();
        mapNuclei = new List<GameObject>();
        nuclei.Add(player);

        GameObject mapAdd;
        for (int i = 0; i <= Constants.MAXP; i++)
        {
            mapAdd = MakeNucleus(i, Constants.bestN[i], i, MAP_DISTANCE *
                    new Vector3(Constants.mapX[i] - 9.5f, -Constants.mapY[i] + 4.5f, 9), new Vector3(0, 0, 0), mapCamera);
            mapAdd.GetComponent<SphereCollider>().radius = 0.8f / mapAdd.GetComponent<Properties>().radius;
            Destroy(mapAdd.GetComponent<Rigidbody>());
            mapAdd.GetComponent<Properties>().ca = mapCamera;
            mapAdd.GetComponent<Properties>().SetColorFunction(delegate (int Z1, int N1, int E1) {
                return Constants.ShellColor(Z1, N1, E1);
            });
            mapAdd.GetComponent<Properties>().SetTextFunction(delegate (int Z1, int N1, int E1) {
                return Constants.MapLabelText(Z1, N1, E1);
            });
            mapAdd.GetComponent<Properties>().SetTextScale(4, 0.7f);
            mapAdd.GetComponent<Properties>().ForceTextUpdate();
            mapAdd.GetComponent<Properties>().FixText();
            mapNuclei.Add(mapAdd);
            mapAdd.SetActive(false);
        }

        for (int i = -1; i <= 1; i += 2) {
            mapAdd = MakeNucleus(0, 0, i, MAP_DISTANCE * new Vector3(7 + (1-i)/2 - 9.5f, 4.5f, 9), new Vector3(0, 0, 0), mapCamera);
            Destroy(mapAdd.GetComponent<Rigidbody>());
            mapAdd.GetComponent<SphereCollider>().radius = 0.8f / mapAdd.GetComponent<Properties>().radius;
            mapAdd.GetComponent<Properties>().ca = mapCamera;
            mapAdd.GetComponent<Properties>().SetColorFunction(delegate (int Z1, int N1, int E1) {
                return Constants.ShellColor(Z1, N1, E1);
            });
            mapAdd.GetComponent<Properties>().SetTextFunction(delegate (int Z1, int N1, int E1) {
                return Constants.MapLabelText(Z1, N1, E1);
            });
            mapAdd.GetComponent<Properties>().SetTextScale(4, 0.7f);
            mapAdd.GetComponent<Properties>().ForceTextUpdate();
            mapAdd.GetComponent<Properties>().FixText();
            mapNuclei.Add(mapAdd);
            mapAdd.SetActive(false);
        }

        //MAP OBJECTS
        Transform mapTransform = GameObject.Find("Map Objects").transform;
        int numChildren = mapTransform.childCount;
        for (int i = 0; i < numChildren; i++)
        {
            mapObjects.Add(mapTransform.GetChild(i).gameObject);
        }
        foreach (GameObject ob in mapObjects) {
            ob.SetActive(false);
        }

        for (int i = 0; i < MAX_PARTICLES / 2; i++)
        {
            nuclei.Add(RandNucleus(ca));
        }
        watch.Stop();
        Debug.Log(string.Format("Took {0} ms to initialize objects.\n", watch.ElapsedMilliseconds));
        TogglePause();
        pauseMenu.SetActive(false);
    }

    void OnGUI()
    {
        if (paused || scene == SCENE.MAP)
        {
            return;
        }
        else
        {
            Properties p = player.GetComponent<Properties>();
            int Z = p.GetZ();
            int N = p.GetN();
            for (int i = 0; i < minimap.height; i++)
            {
                for (int j = 0; j < minimap.width; j++)
                {
                    minimap.SetPixel(j, i, Constants.halflifemap.GetPixel(Z + j + 100 - (minimap.width - 1) / 2, N + i + 100 - (minimap.height - 1) / 2));
                }
            }
            minimap.Apply();
            GUI.DrawTexture(new Rect(0, 0, 310, 310), minimap);
            GUI.DrawTexture(new Rect(150, 150, 10, 10), crosshair);
            GUI.DrawTexture(new Rect(250, 280, 50, 20), axis1);
            GUI.DrawTexture(new Rect(0, 0, 20, 50), axis2);
        }
    }

	// Update is called once per frame
	void Update () {
        if (creditsPosition != -1)
        {
            creditsPosition += 0.61f;
            Vector3 transform1 = credits1.transform.position;
            credits1.transform.position = new Vector3(Screen.width/2, -2650 + creditsPosition * CREDITS_SCROLL_SPEED, transform1.z);
            int frame = (Time.frameCount - creditsStart + 103) % 145;
            int frame2 = (Time.frameCount - creditsStart + 31) % 145;
            if (frame < 15)
            {
                ZLabel.GetComponent<MeshRenderer>().material.color = new Color(1, frame / 15.0f, frame / 15.0f, 1);
            }
            else if (frame2 < 15)
            {
                ZLabel.GetComponent<MeshRenderer>().material.color = new Color(frame2 / 15.0f, 1, frame2 / 15.0f, 1);
            }
            else
            {
                ZLabel.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 1);
            }
            if (creditsPosition > 4500)
            {
                ZLabel.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 1);
                StopCredits();
            }
        }
        if (Input.GetButtonDown("Screenshot"))
        {
            ScreenCapture.CaptureScreenshot("screenshot.png", 1);
            Debug.Log("TOOK A SCREENSHOT");
        }
        if (paused)
        {
            return;
        }
        if (scene == SCENE.MAP)
        {
            timer -= Time.deltaTime * timerRates[synthesisType];
            if (timer < 0)
            {
                synthesisType = (synthesisType + 1 + synthesisTypes.Length ) % synthesisTypes.Length;
                synthesisTypePanel.GetComponent<UnityEngine.UI.Text>().text = synthesisTypes[synthesisType];
                timer = timerStart;
            }
            foreach (GameObject g in mapNuclei)
            {

                g.GetComponent<Properties>().text.GetComponent<Transform>().rotation =
                    Quaternion.LookRotation(mapCamera.transform.forward, mapCamera.transform.up); //rotate text to face player
            }
            if (Vector3.Angle(mapCamera.transform.forward, mapDirections[selectedMap]) > 0.02)
            {
                mapCamera.transform.forward = Vector3.RotateTowards(mapCamera.transform.forward, mapDirections[selectedMap], 0.05f, 1);

            }
            else if (Vector3.Angle(mapCamera.transform.forward, mapDirections[selectedMap]) > 0.005)
            {
                mapCamera.transform.forward = mapDirections[selectedMap];
            }
            for (int i = mapParticles.Count - 1; i >= 0; i--)
            {
                GameObject current = mapParticles[i];
                if (Random.value < 0.0001 || Random.value < (mapParticles.Count - maxParticles) / maxParticles)
                {
                    Destroy(current);
                    mapParticles.RemoveAt(i);
                    continue;
                }
                else
                {
                    BasicProperties p = current.GetComponent<BasicProperties>();
                    int p1 = p.Z;
                    int n1 = p.N;
                    int e1 = p.E;
                    if (Constants.ShouldDecay(p1, n1, 1e6)) //decay
                    {
                        double[,] types = Constants.GetDecayTypes(p1, n1);
                        float rand = Random.value;
                        double start = 0;
                        int index = 0;
                        while (start < rand && index < types.GetLength(0))
                        {
                            start += types[index, 1];
                            index++;
                        }
                        if (index == 0)
                        {
                            Debug.Log("INDEX IS 0");
                            index = 1;
                        }

                        DoBasicDecay(mapParticles, mapParticles[i], (int)types[index - 1, 0]);
                    }
                    else
                    {
                        float rand = Random.value;
                        if (synthesisType == 0 && p.Z == 1) // PP / CNO
                        {
                            if (rand < 0.6)
                            {
                                p.Z += 1;
                            }
                            else if (rand < 0.6002)
                            {
                                p.Z += 1;
                                p.N += 2;
                            }
                        }
                        else if (synthesisType == 1 && rand < 4.0 / (p.Z + p.N) / (p.Z + p.N)) //Alpha
                        {
                            p.Z += 2;
                            p.N += 2;
                            if (rand < 0.002 / (p.Z + p.N) / (p.Z + p.N))
                            {
                                p.Z += 2;
                                p.N += 2;
                            }
                        }
                        else if (synthesisType == 2 && rand < 0.14) //S
                        {
                            p.N += 1;
                        }
                        else if (synthesisType == 3) //R
                        {
                            p.N += (int)Mathf.Floor(rand * 10);
                        }
                    }
                }
            }
            if (mapParticles.Count < maxParticles && Random.value < 0.3)
            {
                mapParticles.Add(RandMapParticle());
            }
            return;
        }
        float r = Random.value;
        int count = nuclei.Count;
        Camera ca = GameObject.Find("Player").GetComponentInChildren<Camera>();
        foreach (GameObject g in nuclei)
        {

            g.GetComponent<Properties>().text.GetComponent<Transform>().rotation =
                Quaternion.LookRotation(player.transform.forward, player.transform.up); //rotate text to face player
        }
        if (count < MAX_PARTICLES && r < 0.01 * (MAX_PARTICLES - count))
        {
            nuclei.Add(RandNucleus(ca));
        }
        Vector3 v = player.GetComponent<Rigidbody>().velocity;

        if (v.magnitude > 1)
        {
            forwardIndicator.SetActive(true);
            backwardIndicator.SetActive(true);
            forwardIndicator.transform.position = player.transform.position + 400 * v.normalized;
            backwardIndicator.transform.position = player.transform.position -400 * v.normalized;
        }
        else
        {
            forwardIndicator.SetActive(false);
            backwardIndicator.SetActive(false);
        }
	}

    void LateUpdate()
    {
        if (paused || scene != SCENE.MAIN)
        {            
            ZLabel.transform.rotation = ZLabel.transform.rotation * Quaternion.Euler(-ca.transform.forward * 0.25f);
            return;
        }

        if (tutorialArrows.Count > 0)
        {
            for (int i = tutorialArrows.Count - 1; i >= 0; i--)
            {
                GoalArrow arrow = tutorialArrows[i];
                switch (arrow.goal)
                {
                    case Goal.UP:
                        if (Input.GetAxis("X2") > 0)
                        {
                            arrow.progress += progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.DOWN:
                        if (Input.GetAxis("X2") < 0)
                        {
                            arrow.progress += progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.FORWARD:
                        if (Input.GetAxis("X3") > 0)
                        {
                            arrow.progress += progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.BACKWARD:
                        if (Input.GetAxis("X3") < 0)
                        {
                            arrow.progress += progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.LEFT:
                        if (Input.GetAxis("X1") < 0)
                        {
                            arrow.progress += progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.RIGHT:
                        if (Input.GetAxis("X1") > 0)
                        {
                            arrow.progress += progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.PITCHUP:
                        if (Input.GetAxis("P") > 0 && Input.GetMouseButton(0))
                        {
                            arrow.progress += 2 * progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.PITCHDOWN:
                        if (Input.GetAxis("P") < 0 && Input.GetMouseButton(0))
                        {
                            arrow.progress += 2 * progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.YAWLEFT:
                        if (Input.GetAxis("Y") < 0 && Input.GetMouseButton(0))
                        {
                            arrow.progress += 2 * progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.YAWRIGHT:
                        if (Input.GetAxis("Y") > 0 && Input.GetMouseButton(0))
                        {
                            arrow.progress += 2 * progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.ROLLLEFT:
                        if (Input.GetAxis("R") < 0)
                        {
                            arrow.progress += progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    case Goal.ROLLRIGHT:
                        if (Input.GetAxis("R") > 0)
                        {
                            arrow.progress += progressPerFrame;
                        }
                        else
                        {
                            arrow.progress -= progressPerFrame;
                        }
                        break;
                    default:
                        Debug.Log("Unknown goal arrow case.");
                        break;
                }
                if (arrow.progress < 0)
                {
                    arrow.progress = 0;
                }
                arrow.UpdateProgress();
                if (arrow.progress > 1)
                {
                    arrow.Destroy();
                    tutorialArrows.RemoveAt(i);
                }
            }
            if (tutorialArrows.Count == 0)
            {
                tutorialPhase++;
                Vector3 t = player.transform.position;
                Vector3 t2 = t - 0.5f * ca.transform.up;
                switch (tutorialPhase)
                {
                    case 1: //up and down
                        tutorialArrows.Add(new GoalArrow(player, Goal.UP, t + 0.5f * ca.transform.up, ca.transform.up, ca.transform.right));
                        tutorialArrows.Add(new GoalArrow(player, Goal.DOWN, t - 0.5f * ca.transform.up, -ca.transform.up, -ca.transform.right));
                        break;
                    case 2: //left and right
                        tutorialArrows.Add(new GoalArrow(player, Goal.RIGHT, t + 0.5f * ca.transform.right, ca.transform.right, ca.transform.up));
                        tutorialArrows.Add(new GoalArrow(player, Goal.LEFT, t - 0.5f * ca.transform.right, -ca.transform.right, -ca.transform.up));
                        break;
                    case 3: //pitch up and down
                        tutorialArrows.Add(new GoalArrow(player, Goal.PITCHUP, t + 0.25f * ca.transform.forward - 0.25f * ca.transform.up, ca.transform.forward + ca.transform.up, ca.transform.up));
                        tutorialArrows.Add(new GoalArrow(player, Goal.PITCHDOWN, t + 0.5f * ca.transform.forward + 0.25f * ca.transform.up, ca.transform.forward - ca.transform.up, -ca.transform.up));
                        break;
                    case 4: //yaw left and right
                        tutorialArrows.Add(new GoalArrow(player, Goal.YAWLEFT, t + 0.25f * ca.transform.forward - 0.25f * ca.transform.right, -ca.transform.forward - ca.transform.right, ca.transform.up));
                        tutorialArrows.Add(new GoalArrow(player, Goal.YAWRIGHT, t + 0.25f * ca.transform.forward + 0.25f * ca.transform.right, -ca.transform.forward + ca.transform.right, -ca.transform.up));
                        break;
                    case 5: //roll left and right
                        tutorialArrows.Add(new GoalArrow(player, Goal.ROLLLEFT, t + 0.25f * ca.transform.up - 0.25f * ca.transform.right, -ca.transform.up - ca.transform.right, ca.transform.up));
                        tutorialArrows.Add(new GoalArrow(player, Goal.ROLLRIGHT, t + 0.25f * ca.transform.up + 0.25f * ca.transform.right, -ca.transform.up + ca.transform.right, -ca.transform.up));
                        break;
                    case 6: //tutorial done
                        tutorialPhase = -1;
                        break;
                    default:
                        Debug.Log("Unknown tutorial phase for new list: " + tutorialPhase);
                        break;
                }
            }
        }
        for (int i = nuclei.Count - 1; i >= 0; i--)
        {
            if (nuclei[i].GetComponent<Properties>().IsDestroyed())
            {
                Destroy(nuclei[i]);
                nuclei.RemoveAt(i);
            }
            else
            {
                Vector3 p1 = player.transform.position;
                Vector3 p2 = nuclei[i].transform.position;
                float dx = p1.x - p2.x;
                float dy = p1.y - p2.y;
                float dz = p1.z - p2.z;
                if (dx * dx + dy * dy + dz * dz > MAX_DISTANCE * MAX_DISTANCE)
                {
                    Destroy(nuclei[i]);
                    nuclei.RemoveAt(i);
                }
                else
                {
                    Properties thisP = nuclei[i].GetComponent<Properties>();
                    int Z1 = thisP.GetZ();
                    int N1 = thisP.GetN();
                    if (Constants.ShouldDecay(thisP.GetZ(), thisP.GetN(), 1)) //decay
                    {
                        double[,] types = Constants.GetDecayTypes(thisP.GetZ(), thisP.GetN());
                        float rand = Random.value;
                        double start = 0;
                        int index = 0;
                        while (start < rand)
                        {
                            start += types[index, 1];
                            index++;
                        }

                        DoDecay(nuclei, nuclei[i], (int)types[index - 1, 0]);
                    }
                    else if (thisP.GetE() > Z1 && Constants.electronAffinities[Z1] < 0 && (Z1 + N1) > 0) //check for electron emission
                    {
                        if (Random.value > Mathf.Exp((float)Constants.electronAffinities[Z1] / 1500))
                        {
                            DoDecay(nuclei, nuclei[i], 22);
                        }
                    }
                }
            }
        }
    }
    public static Vector3 ZNtopos(int Z, int N) {
        return new Vector3(-49f, -28.15f + 0.28f * (N + 1), -18.35f + 0.28f * (Z + 1));
    }

    private GameObject RandMapParticle()
    {
        GameObject toAdd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        int Z = 1;
        int N = 0;
        int E = 0;
        if (Random.value < 0.3)
        {
            Z = 2; //Helium
            N = 2;
        }

        toAdd.AddComponent<BasicProperties>();
        toAdd.GetComponent<BasicProperties>().SetProperties(Z, N, E);

        toAdd.transform.position = ZNtopos(Z,N);
        Destroy(toAdd.GetComponent<SphereCollider>());
        return toAdd;
    }

    private GameObject MapParticle(int Z, int N, int E, int initZ, int initN)
    {
        GameObject toAdd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        toAdd.AddComponent<BasicProperties>();
        toAdd.GetComponent<BasicProperties>().SetProperties(Z, N, E);
        toAdd.transform.position = ZNtopos(initZ, initN);
        Destroy(toAdd.GetComponent<SphereCollider>());
        return toAdd;
    }

    private GameObject RandNucleus(Camera ca)
    {
        Vector3 pos = Random.onUnitSphere * SPAWN_DISTANCE * (1 - 0.5f * Random.value);
        pos = pos + player.transform.position;
        Vector3 v = Random.onUnitSphere * AVG_SPAWN_VELOCITY * Mathf.Exp(2 * Random.value - 1);
        float rand = Random.value;
        if (rand < 0.2)
        {
            return MakeNucleus(1, 0, 0, pos, v, ca);
        }
        else if (rand < 0.5)
        {
            return MakeNucleus(0, 0, 1, pos, v, ca);
        }
        else if (rand < 0.6)
        {
            return MakeNucleus(1, 1, 0, pos, v, ca);
        }
        else if (rand < 0.9)
        {
            return MakeNucleus(2, 2, 0, pos, v, ca);
        }
        else if (rand < 0.95)
        {
            return MakeNucleus(0, 1, 0, pos, v, ca);
        }
        else
        {
            return MakeNucleus(1, 2, 0, pos, v, ca);
        }
    }

    void DoDecay(List<GameObject> nuclei, GameObject selected, int type)
    {
        Properties thisP = selected.GetComponent<Properties>();
        int thisZ = thisP.GetZ();
        int thisN = thisP.GetN();
        int thisE = thisP.GetE();
        Vector3 pos = selected.transform.position;
        Vector3 v = selected.GetComponent<Rigidbody>().velocity;
        Vector3 r1 = Random.onUnitSphere;
        Vector3 r2 = Vector3.RotateTowards(r1, -r1, 2 * Mathf.PI / 3, 1f);
        Vector3 r3 = Vector3.RotateTowards(r1, -r1, -2 * Mathf.PI / 3, 1f);
        Vector3 r11 = Random.onUnitSphere;
        int thisA = thisZ + thisN;
        switch (type)
        {
            case 0: //B-
                nuclei.Add(MakeNucleus(0, 0, 1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ + 1, thisN - 1, thisE);
                break;
            case 1: //B+
                nuclei.Add(MakeNucleus(0, 0, -1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ - 1, thisN + 1, thisE);
                break;
            case 2: //n
                nuclei.Add(MakeNucleus(0, 1, 0, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ, thisN - 1, thisE);
                break;
            case 3: //p
                nuclei.Add(MakeNucleus(1, 0, 0, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ - 1, thisN, thisE);
                break;
            case 4: //2n
                nuclei.Add(MakeNucleus(0, 1, 0, pos +  r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(0, 1, 0, pos -  r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ, thisN - 2, thisE);
                break;
            case 5: //2p
                nuclei.Add(MakeNucleus(1, 0, 0, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(1, 0, 0, pos -  r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ - 2, thisN, thisE);
                break;
            case 6: //3n
                nuclei.Add(MakeNucleus(0, 1, 0, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(0, 1, 0, pos + r2, v + SPAWN_V_FACTOR * r2, ca));
                nuclei.Add(MakeNucleus(0, 1, 0, pos + r3, v + SPAWN_V_FACTOR * r3, ca));
                thisP.SetProperties(thisZ, thisN - 3, thisE);
                break;
            case 7: //3p
                nuclei.Add(MakeNucleus(1, 0, 0, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(1, 0, 0, pos + r2, v + SPAWN_V_FACTOR * r2, ca));
                nuclei.Add(MakeNucleus(1, 0, 0, pos + r3, v + SPAWN_V_FACTOR * r3, ca));
                thisP.SetProperties(thisZ - 3, thisN, thisE);
                break;
            case 8: //A
                nuclei.Add(MakeNucleus(2, 2, 0, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ - 2, thisN - 2, thisE);
                break;
            case 9: //B-A
                nuclei.Add(MakeNucleus(0, 0, 1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(2, 2, 0, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ - 1, thisN - 3, thisE);
                break;
            case 10: //B+A
                nuclei.Add(MakeNucleus(0, 0, -1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(2, 2, 0, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ - 3, thisN - 1, thisE);
                break;
            case 11: //B-n
                nuclei.Add(MakeNucleus(0, 0, 1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(0, 1, 0, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ + 1, thisN - 2, thisE);
                break;
            case 12: //B-2n
                nuclei.Add(MakeNucleus(0, 0, 1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(0, 2, 0, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ + 1, thisN - 3, thisE);
                break;
            case 13: //B-3n
                nuclei.Add(MakeNucleus(0, 0, 1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(0, 2, 0, pos + r2, v + SPAWN_V_FACTOR * r2, ca));
                nuclei.Add(MakeNucleus(0, 1, 0, pos + r3, v + SPAWN_V_FACTOR * r3, ca));
                thisP.SetProperties(thisZ + 1, thisN - 4, thisE);
                break;
            case 14: //B-p
                nuclei.Add(MakeNucleus(0, 0, 1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(1, 0, 0, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ, thisN - 1, thisE);
                break;
            case 15: //B+p
                nuclei.Add(MakeNucleus(0, 0, -1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(1, 0, 0, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ - 2, thisN + 1, thisE);
                break;
            case 16: //B+2p
                nuclei.Add(MakeNucleus(0, 0, -1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(2, 0, 0, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ - 3, thisN + 1, thisE);
                break;
            case 17: //B-t
                nuclei.Add(MakeNucleus(0, 0, 1, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                nuclei.Add(MakeNucleus(1, 2, 0, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ, thisN - 3, thisE);
                break;
            case 18: //EC
                if (thisE > 0)
                {
                    thisP.SetProperties(thisZ - 1, thisN + 1, thisE - 1);
                }
                break;
            case 19: //SF
                if (thisA > 236) //34Si
                {
                    nuclei.Add(MakeNucleus(14, 20, 0, pos + 2*r1, v + SPAWN_V_FACTOR * r1, ca));
                    thisP.SetProperties(thisZ - 14, thisN - 20, thisE);
                }
                else //14C
                {
                    nuclei.Add(MakeNucleus(6, 8, 0, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                    thisP.SetProperties(thisZ - 6, thisN - 8, thisE);
                }
                break;
            case 20: //B-SF
                if (thisA > 236) //34Si
                {
                    nuclei.Add(MakeNucleus(14, 20, 0, pos + 2*r1, v + SPAWN_V_FACTOR * r1, ca));
                    nuclei.Add(MakeNucleus(0, 0, 1, pos - r1, v - r1, ca));
                    thisP.SetProperties(thisZ - 13, thisN - 21, thisE);
                }
                else //14C
                {
                    nuclei.Add(MakeNucleus(6, 8, 0, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                    nuclei.Add(MakeNucleus(0, 0, 1, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                    thisP.SetProperties(thisZ - 5, thisN - 9, thisE);
                }
                break;
            case 21: //B+SF
                if (thisA > 236) //34Si
                {
                    nuclei.Add(MakeNucleus(14, 20, 0, pos + 2*r1, v + SPAWN_V_FACTOR * r1, ca));
                    nuclei.Add(MakeNucleus(0, 0, -1, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                    thisP.SetProperties(thisZ - 15, thisN - 19, thisE);
                }
                else //14C
                {
                    nuclei.Add(MakeNucleus(6, 8, 0, pos + r1, v + SPAWN_V_FACTOR * r1, ca));
                    nuclei.Add(MakeNucleus(0, 0, -1, pos - r1, v - SPAWN_V_FACTOR * r1, ca));
                    thisP.SetProperties(thisZ - 7, thisN - 7, thisE);
                }
                break;
            case 22: //electron emission
                nuclei.Add(MakeNucleus(0, 0, 1, pos + 5 * r1, v + SPAWN_V_FACTOR * r1, ca));
                thisP.SetProperties(thisZ, thisN, thisE - 1);
                break;
        }
    }

    void DoBasicDecay(List<GameObject> nuclei, GameObject selected, int type)
    {
        BasicProperties thisP = selected.GetComponent<BasicProperties>();
        int thisZ = thisP.Z;
        int thisN = thisP.N;
        int thisE = thisP.E;
        int thisA = thisZ + thisN;
        switch (type)
        {
            case 0: //B-
                thisP.SetProperties(thisZ + 1, thisN - 1, thisE);
                break;
            case 1: //B+
                thisP.SetProperties(thisZ - 1, thisN + 1, thisE);
                break;
            case 2: //n
                //ignore emission, don't care about neutrons
                thisP.SetProperties(thisZ, thisN - 1, thisE);
                break;
            case 3: //p
                //ignore emission, protons already represented in chart
                thisP.SetProperties(thisZ - 1, thisN, thisE);
                break;
            case 4: //2n
                //ignore emission, don't care about neutrons
                thisP.SetProperties(thisZ, thisN - 2, thisE);
                break;
            case 5: //2p
                //ignore emission, proton already represented in chart
                thisP.SetProperties(thisZ - 2, thisN, thisE);
                break;
            case 6: //3n
                //ignore emission, don't care about neutrons
                thisP.SetProperties(thisZ, thisN - 3, thisE);
                break;
            case 7: //3p
                //ignore emission, proton already represented in chart
                thisP.SetProperties(thisZ - 3, thisN, thisE);
                break;
            case 8: //A
                //ignore emission, helium already represented in chart
                thisP.SetProperties(thisZ - 2, thisN - 2, thisE);
                break;
            case 9: //B-A
                //ignore emission, helium already represented in chart
                thisP.SetProperties(thisZ - 1, thisN - 3, thisE);
                break;
            case 10: //B+A
                //ignore emission, helium already represented in chart
                thisP.SetProperties(thisZ - 3, thisN - 1, thisE);
                break;
            case 11: //B-n
                //ignore emission, don't care about neutrons
                thisP.SetProperties(thisZ + 1, thisN - 2, thisE);
                break;
            case 12: //B-2n
                //ignore emission, don't care about neutrons
                thisP.SetProperties(thisZ + 1, thisN - 3, thisE);
                break;
            case 13: //B-3n
                //ignore emission, don't care about neutrons
                thisP.SetProperties(thisZ + 1, thisN - 4, thisE);
                break;
            case 14: //B-p
                //ignore emission, hydrogen already represented in chart
                thisP.SetProperties(thisZ, thisN - 1, thisE);
                break;
            case 15: //B+p
                //ignore emission, hydrogen already represented in chart
                thisP.SetProperties(thisZ - 2, thisN + 1, thisE);
                break;
            case 16: //B+2p
                //ignore emission, hydrogen already represented in chart
                thisP.SetProperties(thisZ - 3, thisN + 1, thisE);
                break;
            case 17: //B-t
                nuclei.Add(MapParticle(1, 2, 0, thisZ, thisN));
                thisP.SetProperties(thisZ, thisN - 3, thisE);
                break;
            case 18: //EC
                if (thisE > 0)
                {
                    thisP.SetProperties(thisZ - 1, thisN + 1, thisE - 1);
                }
                break;
            case 19: //SF
                if (thisA > 236) //34Si
                {
                    nuclei.Add(MapParticle(14, 20, 0, thisZ, thisN));
                    thisP.SetProperties(thisZ - 14, thisN - 20, thisE);
                }
                else //14C
                {
                    nuclei.Add(MapParticle(6, 8, 0, thisZ, thisN));
                    thisP.SetProperties(thisZ - 6, thisN - 8, thisE);
                }
                break;
            case 20: //B-SF
                if (thisA > 236) //34Si
                {
                    nuclei.Add(MapParticle(14, 20, 0, thisZ, thisN));
                    thisP.SetProperties(thisZ - 13, thisN - 21, thisE);
                }
                else //14C
                {
                    nuclei.Add(MapParticle(6, 8, 0, thisZ, thisN));
                    thisP.SetProperties(thisZ - 5, thisN - 9, thisE);
                }
                break;
            case 21: //B+SF
                if (thisA > 236) //34Si
                {
                    nuclei.Add(MapParticle(14, 20, 0, thisZ, thisN));
                    thisP.SetProperties(thisZ - 15, thisN - 19, thisE);
                }
                else //14C
                {
                    nuclei.Add(MapParticle(6, 8, 0, thisZ, thisN));
                    thisP.SetProperties(thisZ - 7, thisN - 7, thisE);
                }
                break;
        }
    }

    GameObject MakeNucleus(int Z, int N, int E, Vector3 pos, Vector3 v, Camera aimTo)
    {
        GameObject toAdd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        toAdd.transform.position = pos;
        toAdd.AddComponent<Rigidbody>();
        toAdd.GetComponent<Rigidbody>().velocity = v;
        toAdd.GetComponent<Rigidbody>().detectCollisions = true;
        toAdd.GetComponent<SphereCollider>().isTrigger = true;
        toAdd.AddComponent<Properties>();
        toAdd.GetComponent<Properties>().ca = aimTo;
        toAdd.GetComponent<Properties>().SetColorFunction(delegate (int Z1, int N1, int E1) {
            return Constants.MainHalfLifeColor(Z1, N1, E1);});
        toAdd.GetComponent<Properties>().SetTextFunction(delegate (int Z1, int N1, int E1){
            return Constants.MainLabelText(Z1, N1, E1);});
        toAdd.GetComponent<Properties>().SetProperties(Z, N, E);
        if (aimTo == mapCamera)
        {
            toAdd.GetComponent<SphereCollider>().radius = 0.8f;
        }
        else
        {
            toAdd.GetComponent<SphereCollider>().radius = 30;
        }
        return toAdd;
    }

    public void TogglePause()
    {
        paused = !paused;
        pauseMenu.SetActive(paused);
        pauseMenu.GetComponent<Canvas>().enabled = paused;
        ZLabel.SetActive(paused && !GameObject.Find("TutorialCanvas").activeSelf);
        creditsPosition = -1;
        if (paused)
        {
            GetComponent<Music>().Pause();
        }
        else
        {
            GetComponent<Music>().UnPause();
        }
        if (scene == SCENE.MAIN) {
            if (paused)
            {
                forwardIndicator.SetActive(false);
                backwardIndicator.SetActive(false);
                foreach (GameObject ob in nuclei)
                {
                    ob.GetComponent<Properties>().Sleep();
                    ob.GetComponent<Properties>().enabled = false;
                }
            }
            else
            {
                foreach (GameObject ob in nuclei)
                {
                    ob.GetComponent<Properties>().enabled = true;
                    ob.GetComponent<Properties>().WakeUp();
                }
            }
        }
    }

    public void ToggleMapMode()
    {
        if (scene == SCENE.MAP)
        {
            scene = SCENE.MAIN;
            foreach (GameObject ob in mapNuclei)
            {
                ob.SetActive(false);
            }
            foreach (GameObject ob in mapObjects)
            {
                ob.SetActive(false);
            }
            foreach (GameObject ob in nuclei)
            {
                ob.SetActive(true);
            }
            ca.enabled = true;
            ca.GetComponent<AudioListener>().enabled = true;
            GetComponent<Music>().SwitchToMain();
        }
        else if (scene == SCENE.MAIN)
        {
            scene = SCENE.MAP;
            forwardIndicator.SetActive(false);
            backwardIndicator.SetActive(false);
            foreach (GameObject ob in mapNuclei)
            {
                ob.SetActive(true);
            }
            foreach (GameObject ob in mapObjects)
            {
                ob.SetActive(true);
            }
            foreach (GameObject ob in nuclei)
            {
                ob.SetActive(false);
            }
            ca.enabled = false;
            ca.GetComponent<AudioListener>().enabled = false;
            infoPanel.SetActive(false);
            GetComponent<Music>().SwitchToMap();
        }
        TogglePause();
    }

    //For communication with other objects
    public void Signal(int Z, int N, int E)
    {
        if (scene == SCENE.MAP && !paused)
        {
            //infoPanel.SetActive(true);
            infoPanel.GetComponent<InfoMenu>().Signal(Z, N, E);
        }
    }

    public void CloseInfo()
    {
        infoPanel.SetActive(false);
    }

    public void EnterCredits()
    {
        creditsStart = Time.frameCount;
        creditsPosition = 0;
        GetComponent<Music>().SwitchToCredits();
        creditsCanvas.SetActive(true);
        pauseMenu.GetComponent<Canvas>().enabled = false;

    }

    void FixedUpdate()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (creditsPosition != -1)
            {
                StopCredits();
            }
            else
            {
                TogglePause();
            }
        }
    }

    public void StopCredits()
    {
        creditsPosition = -1;
        GetComponent<Music>().StopCredits();
        creditsCanvas.SetActive(false);
        pauseMenu.GetComponent<Canvas>().enabled = true;
        credits1.transform.position = credits1Start;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public bool IsPaused()
    {
        return paused;
    }


    public void ChangeColorScheme(int scheme)
    {
        foreach (GameObject ob in mapNuclei) {
            ob.GetComponent<Properties>().SetColorFunction(colorFunctions[scheme]);
            ob.GetComponent<Properties>().SetTextFunction(textFunctions[scheme]);
            ob.GetComponent<Properties>().ForceTextUpdate();
        }
    }

    public void ChangeMap(int offset)
    {
        selectedMap = (selectedMap + offset + mapDirections.Length) % mapDirections.Length;
    }

    public void Restart()
    {
        nuclei[0].GetComponent<Properties>().SetProperties(1, 0, 0);
    }

    public void ActivateTutorial()
    {
        if (paused)
        {
            TogglePause();
        }
        foreach (GoalArrow arrow in tutorialArrows)
        {
            arrow.Destroy();
        }
        Vector3 t = player.transform.position - 0.5f * ca.transform.up;
        tutorialArrows = new List<GoalArrow>();
        tutorialArrows.Add(new GoalArrow(player, Goal.FORWARD, t + 0.5f*ca.transform.forward, ca.transform.forward, ca.transform.up));
        tutorialArrows.Add(new GoalArrow(player, Goal.BACKWARD, t - 0.5f*ca.transform.forward, -ca.transform.forward, -ca.transform.up));
        tutorialPhase = 0;
    }
}
