using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InfoMenu : MonoBehaviour {
    float startX;
    float startY;
    Sprite[] elementSprites;
    Sprite[] leptonSprites;
    private Texture2D electronIcon;
    Image elementPicture;
    Text photoCredit;
    Text symbol;
    Text title1;
    Text title2;
    string fullDescription;
    Text identifier;
    Text description;
    Text shellText;
    Text bestIsotope;
    Text valenceText;
    Text typeText;
    Text radiusText;
    Text ENText;
    Text meltBoilText;
    Text abundanceText;
    Text discoveryText;
    string[] photoCredits;
    public float _itemWidth;
    public float _itemHeight;
    bool dragging;
    int textFrame;
    int selectedZ;
    RectTransform tr;
    RectTransform shellTransform;
    RectTransform osTransform;
    Texture2D[] oxidationTextures;
    Data dataParent;
	// Use this for initialization
	void Start () {
        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        selectedZ = 0;
        dragging = false;
        dataParent = GameObject.Find("Daemon").GetComponent<Data>();
        electronIcon = Resources.Load("materials/Electron") as Texture2D;
        shellTransform = GameObject.Find("ShellDiagram").GetComponent<RectTransform>();
        osTransform = GameObject.Find("OxidationLabels").GetComponent<RectTransform>();
        ENText = GameObject.Find("ENText").GetComponent<Text>();
        meltBoilText = GameObject.Find("MeltBoilText").GetComponent<Text>();
        bestIsotope = GameObject.Find("IsotopeText").GetComponent<Text>();
        elementPicture = GameObject.Find("Picture").GetComponent<Image>();
        photoCredit = GameObject.Find("Photo Credit").GetComponent<Text>();
        description = GameObject.Find("Description").GetComponent<Text>();
        shellText = GameObject.Find("ShellText").GetComponent<Text>();
        valenceText = GameObject.Find("ValenceText").GetComponent<Text>();
        typeText = GameObject.Find("TypeText").GetComponent<Text>();
        symbol = GameObject.Find("Symbol").GetComponent<Text>();
        title1 = GameObject.Find("Title1").GetComponent<Text>();
        title2 = GameObject.Find("Title2").GetComponent<Text>();
        radiusText = GameObject.Find("RadiusText").GetComponent<Text>();
        identifier = GameObject.Find("Identifier").GetComponent<Text>();
        elementSprites = new Sprite[Constants.MAXP + 1];
        abundanceText = GameObject.Find("AbundanceText").GetComponent<Text>();
        discoveryText = GameObject.Find("DiscoveryText").GetComponent<Text>();
        fullDescription = "";
        leptonSprites = new Sprite[]
        {
            Resources.Load("materials/Elements/Electron", typeof(Sprite)) as Sprite,
            Resources.Load("materials/Elements/Positron", typeof(Sprite)) as Sprite
        };

        oxidationTextures = new Texture2D[]
        {
            Resources.Load("materials/unstableOS") as Texture2D,
            Resources.Load("materials/stableOS") as Texture2D,
            Resources.Load("materials/preferredOS") as Texture2D
        };

        photoCredits = new string[Constants.MAXP + 1];
        for (int i = 0; i <= Constants.MAXP; i++)
        {
            if (string.CompareOrdinal(Constants.photoCredits[i],"?") == 0)
            {
                elementSprites[i] = Resources.Load("materials/Elements/Unknown", typeof(Sprite)) as Sprite;
                photoCredits[i] = "";
            }
            else
            {
                elementSprites[i] = Resources.Load("materials/Elements/" + Constants.elementNames[i], typeof(Sprite)) as Sprite;
                photoCredits[i] = "Photo credit: " + Constants.photoCredits[i];
            }            
        }
        tr = GetComponent<RectTransform>();

        elementPicture.sprite = Resources.Load("materials/unstableOS", typeof(Sprite)) as Sprite;
        description.text = "";
        photoCredit.text = "";
        symbol.text = "";
        title1.text = "";
        title2.text = "";
        typeText.text = "";
        valenceText.text = "";
        radiusText.text = "";
        ENText.text = "";
        meltBoilText.text = "";
        abundanceText.text = "";
        textFrame = 0;

        watch.Stop();
        Debug.Log(string.Format("Took {0} ms to initialize infoMenu.\n", watch.ElapsedMilliseconds));
    }
	
	void OnGUI() {
        int[] osStates = Constants.oxidationStates[selectedZ];
        int[] electronShell = Constants.electronShell[selectedZ];
        for (int n = 0; n < electronShell.Length; n++)
        {
            float r = (n + 1) * (n + 1) * 10 / Mathf.Sqrt(selectedZ + 1);
            int size = Constants.shellSize[n];
            for (int i = 0; i < Constants.electronShell[selectedZ][n]; i++)
            {
                float theta = (2 * Mathf.PI) * ((i % 2 + (i / 2) * 3) * 1.0f / (3 * size / 2))
                    + Time.time * selectedZ / (n + 1) / (n + 1) / 4;
                GUI.DrawTexture(new Rect(shellTransform.position.x + r * Mathf.Cos(theta) - 2,
                        Screen.height - (shellTransform.position.y + r * Mathf.Sin(theta)) - 2, 5, 5), electronIcon);
            }
        }
        for (int i = 0; i < osStates.Length; i++)
        {
            GUI.DrawTexture(new Rect(osTransform.position.x - 134 + 21.5f * i, Screen.height - osTransform.position.y, 15, 15), oxidationTextures[osStates[i]]);
        }
        if (dataParent.IsPaused())
        {
            return;
        }
        if (textFrame <= fullDescription.Length)
        {
            description.text = fullDescription.Substring(0, textFrame);
            textFrame+=2;
            if (textFrame > fullDescription.Length) {
                textFrame = fullDescription.Length;
            }
        }

        
        
	}

    public void BeginDrag()
    {
        startX = tr.position.x - Input.mousePosition.x;
        startY = tr.position.y - Input.mousePosition.y;
        dragging = true;
    }

    public void Drag()
    {
        Vector3 newPosition = new Vector3(startX + Input.mousePosition.x, startY + Input.mousePosition.y);
        newPosition.x = Mathf.Clamp(newPosition.x, _itemWidth / 2, Screen.width - _itemWidth / 2); 
        tr.position = newPosition;

    }

    public void EndDrag()
    {
        dragging = false;
    }

    public void Signal(int Z, int N, int E)
    {
        
        Vector2 mousePoint = tr.InverseTransformPoint(Input.mousePosition);
        if (tr.rect.Contains(mousePoint) && isActiveAndEnabled && gameObject.activeSelf)
        {
            return;
        }
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        Sprite toSetPicture;
        string toSetCredit;
        string toSetSymbol;
        string toSetName;
        string toSetAltName;
        if (N + Z > 0)
        {
            discoveryText.text = "Discovery year\n" + Constants.discoveryYears[Z];
            toSetPicture = elementSprites[Z];
            toSetCredit = photoCredits[Z];
            toSetName = Constants.elementNames[Z];
            toSetAltName = Constants.altElementNames[Z];
            toSetSymbol = Constants.names[Z];
            identifier.text = Z.ToString();
            fullDescription = Constants.descriptions[Z];
            selectedZ = Z;
            shellText.text = Constants.shellTexts[Z];
            valenceText.text = "Valence: \n " + Constants.valenceElectrons[Z].ToString();
            abundanceText.text = "Solar System Abundance\n" + Constants.GetAbundance(Z);
            double negativity = Constants.electroNegativities[Z];
            if (negativity == 0)
            {
                ENText.text = "Electronegativity \n Unknown";
            }
            else
            {
                ENText.text = "Electronegativity \n " + negativity.ToString("F");
            }

            if (Z == 0)
            {
                typeText.text = "Nucleon \n (Noble Gas)";
                radiusText.text = "Radius \n 0.8 fm";
            }
            else if (Z == 1)
            {
                typeText.text = "Nucleon \n (" + Constants.typeNames[Constants.elementType[Z]] + ")";
                radiusText.text = "Radius \n" + Constants.radii[Z] + " pm";
            }
            else
            {
                typeText.text = Constants.typeNames[Constants.elementType[Z]];
                radiusText.text = "Radius \n" + Constants.radii[Z] + " pm";
            }
            if (Z >= 102) {
                bestIsotope.text = "<i>" + toSetSymbol + "-" + (Z + Constants.bestN[Z]).ToString() + (Z >= 98 ? " (Predicted)" : "")
                    + "\n" + Constants.GetFormattedLife(Z, Constants.bestN[Z]) + "</i>";
                if (Constants.halflives[Z,Constants.bestN[Z]] != -1)
                {
                    double[,] decayTypes = Constants.decaytypes[Z, Constants.bestN[Z]];
                    int best = 0;
                    double bestChance = 0;
                    for (int i = 0; i < decayTypes.Length / 2; i++)
                    {
                        if (decayTypes[i, 1] > bestChance)
                        {
                            best = i;
                            bestChance = decayTypes[i, 1];
                        }
                    }
                    bestIsotope.text = bestIsotope.text + ": " + Constants.decaynames[(int)decayTypes[best,0]];
                }
            }
            else {
                bestIsotope.text = toSetSymbol + "-" + (Z + Constants.bestN[Z]).ToString() + "\n" + Constants.GetFormattedLife(Z, Constants.bestN[Z]);
                if (Constants.halflives[Z, Constants.bestN[Z]] != -1)
                {
                    double[,] decayTypes = Constants.decaytypes[Z, Constants.bestN[Z]];
                    int best = 0;
                    double bestChance = 0;
                    for (int i = 0; i < decayTypes.Length / 2; i++)
                    {
                        if (decayTypes[i, 1] > bestChance)
                        {
                            best = i;
                            bestChance = decayTypes[i, 1];
                        }
                    }
                    bestIsotope.text = bestIsotope.text + ": " + Constants.decaynames[(int)decayTypes[best, 0]];
                }
            }

            if (Z <= 99)
            {
                meltBoilText.text = "Melting Point: " + Constants.meltingPoints[Z].ToString("F") + " K\n" +
                    "Boiling Point: " + Constants.boilingPoints[Z].ToString("F") + " K";
            }
            else
            {
                meltBoilText.text = "Melting Point: Unknown \nBoiling Point: Unknown";
            }
            
        }
        else if (E == 1)
        {
            toSetPicture = leptonSprites[0];
            toSetCredit = "Photo Credit: Wikipedia (CC)";
            toSetName = "Electron";
            toSetAltName = "";
            toSetSymbol = "e-";
            identifier.text = "";
            fullDescription = Constants.electronDescription;
            selectedZ = 0;
            shellText.text = "";
            valenceText.text = "";
            typeText.text = "Lepton";
            bestIsotope.text = "stable";
            radiusText.text = "Radius \n N/A";
            abundanceText.text = "";
            meltBoilText.text = "";
            discoveryText.text = "Discovery year\n" + Constants.electronDiscoveryYear;
        }
        else {

            toSetPicture = leptonSprites[1];
            toSetCredit = "Photo Credit: Wikipedia (CC)";
            toSetName = "Positron";
            toSetAltName = "";
            toSetSymbol = "e+";
            identifier.text = "";
            fullDescription = Constants.positronDescription;
            selectedZ = 0;
            shellText.text = "";
            valenceText.text = "";
            typeText.text = "Antilepton";
            bestIsotope.text = "stable";
            radiusText.text = "Radius \n N/A";
            abundanceText.text = "";
            meltBoilText.text = "";
            discoveryText.text = "Discovery year\n" + Constants.positronDiscoveryYear;
        }

        elementPicture.sprite = toSetPicture;
        photoCredit.text = toSetCredit;
        symbol.text = toSetSymbol;
        title1.text = toSetName;
        title2.text = toSetAltName;
        textFrame = 0;
    }
}
