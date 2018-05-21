using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NuclideTable : MonoBehaviour {
    Text label;
    Text description;
    Transform tr;
    public string labelName;
    public string descriptionName;
    public int transformDirection;
    Camera ca;
	// Use this for initialization
	void Start () {
        label = GameObject.Find(labelName).GetComponent<Text>();
        description = GameObject.Find(descriptionName).GetComponent<Text>();
        tr = GetComponent<Transform>();
        ca = GameObject.Find("MapCamera").GetComponent<Camera>();
        if (transformDirection == 3)
        {
            GetComponent<Image>().color = new Color(1,1,1,0.5f);
        }
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 pos = Input.mousePosition;
        Ray ray = ca.ScreenPointToRay(pos);
        Vector3 v = 49f * ray.direction * (transformDirection == 1 ? 1 : -1) / ray.direction.x;
        int x = (int)Mathf.Floor(-(v.z * (transformDirection == 1? 1 : -1) - 13f * 5f * 0.28f) / 0.28f);
        int y = (int)Mathf.Floor((v.y +  20f * 0.5f * 10 * 0.28f) / 0.28f);

        if (x >= 0 && x <= Constants.MAXP && y >= 0 && y <= Constants.MAXN)
        {
            label.text = Constants.MainLabelText(x, y, x);
        }
	}

    void FixedUpdate()
    {
        if ((ca.transform.forward - Data.mapDirections[transformDirection]).sqrMagnitude > 0.1)
        {
            return;
        }
        if (Input.GetMouseButton(0))
        {

            Vector2 pos = Input.mousePosition;
            Ray ray = ca.ScreenPointToRay(pos);
            Vector3 v = 49f * ray.direction * (transformDirection == 1 ? 1 : -1) / (ray.direction.x);
            int x = (int)Mathf.Floor(-(v.z * (transformDirection == 1 ? 1 : -1) - 13f * 5f * 0.28f) / 0.28f);
            int y = (int)Mathf.Floor((v.y + 20f * 0.5f * 10 * 0.28f) / 0.28f);
            if (x >= 0 && x <= Constants.MAXP && y >= 0 && y <= Constants.MAXN)
            {
                double halflife = Constants.GetHalfLife(x, y);
                double[,] decayModes = Constants.GetDecayTypes(x, y);
                description.text = Constants.MainLabelText(x, y, x) + "\n" + Constants.GetFormattedLife(x,y) + "\n";
                if (halflife != -1 && decayModes != null)
                {
                    for (int i = 0; i < decayModes.Length / 2; i++)
                    {
                        description.text += Constants.decaynames[(int)decayModes[i, 0]] + " " + decayModes[i, 1].ToString("F2") + "\n";
                    }
                }
            }
        }
    }
}
