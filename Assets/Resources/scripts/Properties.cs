using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Properties : MonoBehaviour {
    private int Z;
    public delegate Color Del(int Z, int N, int E);
    public delegate string Del2(int Z, int N, int E);
    private Del colorFunction;
    private Del2 textFunction;
    private int N;
    private int E;
    public float radius;
    public bool isPlayer;
    Transform tr;
    Renderer re;
    Rigidbody rb;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 lastVelocity;
    private float textScale;
    public Camera ca;
    GameObject innerShell;
    public GameObject text;
    public GameObject text2;
    Movement mo;
    private bool zoomed;
    private bool destroyed;
    private bool inMapMode;
    private Data dataParent;
    private static readonly int SPHERE_LAYER = 0;
    private static readonly int NAME_LAYER_1 = 1;
    private static readonly int ION_LAYER = 7;
    private static readonly float TEXT_OPACITY = 0.6f;
    private static AudioClip combineClip;

    public void SetColorFunction(Del del)
    {
        colorFunction = del;
    }
    public void SetTextFunction(Del2 del2) {
        textFunction = del2;
    }

    public void ForceTextUpdate()
    {
        Color c = colorFunction(Z, N, E);
        c.a = 0.4f;
        text.GetComponent<TextMesh>().color = c;
        text.GetComponent<TextMesh>().text = textFunction(Z, N, E);
    }

    public int GetZ()
    {
        return Z;
    }

    public int GetN()
    {
        return N;    
    }

    public int GetE()
    {
        return E;
    }

    public bool IsDestroyed()
    {
        return destroyed;
    }

    void Awake()
    {
        combineClip = Resources.Load("Audio/Sounds/combine") as AudioClip;
        textScale = 1;
        dataParent = GameObject.Find("Daemon").GetComponent<Data>();
        destroyed = false;
        zoomed = false;
        innerShell = new GameObject();
        tr = GetComponent<Transform>();

        re = GetComponent<Renderer>();
        re.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        re.sortingOrder = SPHERE_LAYER;
        rb = GetComponent<Rigidbody>();

        innerShell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(innerShell.GetComponent<Collider>());
        Destroy(innerShell.GetComponent<Rigidbody>());
        innerShell.SetActive(false);
        innerShell.transform.parent = tr;
        innerShell.transform.localPosition = new Vector3(0, 0, 0);
          

        isPlayer = false;
        rb.useGravity = false;
        text = new GameObject("text");
        text2 = new GameObject("ion text");
        text.transform.parent = tr;
        text2.transform.parent = text.transform;
        TextMesh mesh = text.AddComponent<TextMesh>();
        TextMesh mesh2 = text2.AddComponent<TextMesh>();
        mesh.text = "";
        mesh2.text = (Z - E).ToString("+00;-00;+00");
        mesh.alignment = TextAlignment.Center;
        mesh2.alignment = TextAlignment.Left;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh2.anchor = TextAnchor.UpperLeft;
        mesh.fontSize = 120;
        mesh2.fontSize = 60;
        Destroy(text.GetComponent<Collider>());
        Destroy(text2.GetComponent<Collider>());
        mesh.color = new Color(1, 1, 1, TEXT_OPACITY);
        mesh2.color = new Color(1, 1, 1, TEXT_OPACITY);
        mesh.font = Resources.Load("Fonts/cour", typeof(Font)) as Font;
        mesh2.font = Resources.Load("Fonts/cour", typeof(Font)) as Font;
        text.GetComponent<Renderer>().material = Resources.Load("Fonts/cour", typeof(Material)) as Material;
        text.GetComponent<Renderer>().material.mainTexture.filterMode = FilterMode.Bilinear;
        text.GetComponent<Renderer>().sortingOrder = NAME_LAYER_1;
        text.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        text2.GetComponent<Renderer>().material = Resources.Load("Fonts/cour", typeof(Material)) as Material;
        text2.GetComponent<Renderer>().material.mainTexture.filterMode = FilterMode.Bilinear;
        text2.GetComponent<Renderer>().sortingOrder = ION_LAYER;
        text2.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    public void FixText()
    {
        text.transform.position = tr.position - 2 * ca.transform.up;
        text2.GetComponent<Renderer>().enabled = false;
    }

    public void SetProperties(int Z, int N, int E)
    {
        
        this.Z = Z;
        this.N = N;
        this.E = E;
        radius = (float)(Constants.GetRadius(Z) * Constants.scaleFactor);
        tr.localScale = new Vector3(2*radius, 2*radius, 2*radius);
        float dist = Vector3.Magnitude(tr.position - ca.transform.position);
        if (isPlayer)
        {
            //text.transform.localPosition = new Vector3(0.0f, -0.0f, -0.5f);
            text.transform.position = tr.position -1f * radius * ca.transform.up;
        }
        else
        {
            text.transform.position = tr.position -1f * radius * ca.transform.up;
        }
        TextMesh mesh = text.GetComponent<TextMesh>();
        mesh.text = textFunction(Z, N, E);
        Color c = colorFunction(Z, N, E);
        c.a = Mathf.Clamp(20 * TEXT_OPACITY / (dist+10), 0, TEXT_OPACITY);
        mesh.color = c;
        if (isPlayer)
        {
            text.GetComponent<Transform>().localScale = new Vector3(0.1f, 0.1f, 0.1f) / tr.localScale.x;
        }
        else
        {
            text.GetComponent<Transform>().localScale = new Vector3(0.1f, 0.1f, 0.1f) / tr.localScale.x;
        }

        text.GetComponent<Transform>().LookAt(ca.transform.position);
        text.GetComponent<Transform>().Rotate(0, 180, 0);
        text2.transform.localScale = new Vector3(1, 1, 1);
        text2.transform.position = text.transform.position + 0.35f * text.transform.right * text.GetComponent<TextMesh>().text.Length
                + 0.8f*text.transform.up;
        text2.GetComponent<TextMesh>().text = (Z - E).ToString("+00;-00;+00");

        if (!gameObject.name.Equals("Player"))
        {
            gameObject.name = text.GetComponent<TextMesh>().text;
        }
        
        innerShell.name = text.GetComponent<TextMesh>().text + "(inner)";


        if (isPlayer)
        {
            mo = GetComponent<Movement>();
            mo.max_zoom = -radius;
            rb.angularDrag = 0.8f;
            rb.drag = 1.5f;
        }
        else
        {
            rb.angularDrag = 0.1f;
            rb.drag = 0.1f;
        }
        rb.mass = 1f * (Z+N) + 0.001f*Mathf.Abs(E);


        string shellType = Constants.subshells[Z];
        re.material = Resources.Load("materials/shells/" + Constants.period[Z] + "s" + "Material", typeof(Material)) as Material;
        re.sortingOrder = ("materials/shells/" + Constants.period[Z] + "s" + "Material").GetHashCode();
        if (shellType[1] == 's')
        {
            innerShell.SetActive(false);
        }
        else
        {
            innerShell.GetComponent<Renderer>().material = Resources.Load("materials/shells/" + shellType + "Material", typeof(Material)) as Material;
            innerShell.GetComponent<Renderer>().sortingOrder = ("materials/shells/" + shellType + "Material").GetHashCode();
            innerShell.GetComponent<Transform>().localScale = new Vector3(0.5f, 0.5f, 0.5f);
            innerShell.SetActive(true);
        }
    }

    void OnTriggerStay(Collider other_c)
    {
        GameObject other = other_c.gameObject;
        Rigidbody rb2 = other.GetComponent<Rigidbody>();
        Properties p2 = other.GetComponent<Properties>();
        if (p2 == null)
        {
            return;
        }
        Vector3 r = tr.position - other.GetComponent<Transform>().position;
        float r1 = radius;
        float r2 = p2.radius;
        float r3 = Mathf.Min(r1, r2);
        float dist = r.magnitude;
        int Z2 = p2.Z;
        //int N2 = p2.N;
        int E2 = p2.E;
        float strength1 = (Z * Z2) / (dist * dist); //nuclear repulsion term
        float strength2 = (Z * Z2) / (dist * dist); //nuclear repulsion term

        strength1 -= (Z * E2 / dist / dist / r2 * (r2 - Mathf.Exp(-2 * dist / r2) * (r2 + 2 * dist))); //attraction between nucleus 1 and cloud 2
        strength1 -= (Z2 * E / dist / dist / r1 * (r1 - Mathf.Exp(-2 * dist / r1) * (r1 + 2 * dist))); //attraction between nucleus 2 and cloud 1
        strength2 -= (Z2 * E / dist / dist / r1 * (r1 - Mathf.Exp(-2 * dist / r1) * (r1 + 2 * dist))); //attraction between nucleus 2 and cloud 1
        strength2 -= (Z * E2 / dist / dist / r2 * (r2 - Mathf.Exp(-2 * dist / r2) * (r2 + 2 * dist))); //attraction between nucleus 1 and cloud 2

        strength1 += (E * E2 / dist / dist / r3 * (r3 - Mathf.Exp(-2 * dist / r3) * (r3 + 2 * dist))); //APPROXIMATE repulsion between clouds
        strength2 += (E * E2 / dist / dist / r3 * (r3 - Mathf.Exp(-2 * dist / r3) * (r3 + 2 * dist))); //APPROXIMATE repulsion between clouds

        strength1 *= Constants.EM_CONSTANT;
        strength2 *= Constants.EM_CONSTANT;
        //strength1 += Constants.SF_CONSTANT * (Z + N) * (p2.GetZ() + p2.GetN()) * (float)(-1 * (dist - 0.05) * Mathf.Exp(-5 * dist)) / (dist * dist);
        //strength2 += Constants.SF_CONSTANT * (Z + N) * (p2.GetZ() + p2.GetN()) * (float)(-1 * (dist - 0.05) * Mathf.Exp(-5 * dist)) / (dist * dist);

        GetComponent<Rigidbody>().AddForce(strength1 * r.normalized);
        other.GetComponent<Rigidbody>().AddForce(-strength2 * r.normalized);
        float distmax = Mathf.Max(radius, p2.radius);
        if ((IsLepton() && p2.IsLepton()) || ((IsLepton() || p2.IsLepton()) && dist > 2*distmax + 0.1) || (!IsLepton() && !p2.IsLepton() && dist > (radius + p2.radius) / 3))
        {
            return;
        }
        if (isPlayer)
        {
            Properties newP = other.GetComponent<Properties>();
            int newZ = Z + newP.Z;
            int newN = N + newP.N;
            int newE = E + newP.E;


            if (newP.E == -1 && E < 1)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(combineClip, tr.position);
            SetProperties(newZ, newN, newE);
            rb.velocity = (rb.velocity * rb.mass + rb2.velocity * rb2.mass) / (rb.mass + rb2.mass);
            p2.destroyed = true;
        }
        else if (!other.GetComponent<Properties>().isPlayer && !destroyed)
        {
            Properties newP = other.GetComponent<Properties>();
            int newZ = Z + newP.Z;
            int newN = N + newP.N;
            int newE = E + newP.E;
            if ((newP.E == -1 && E < 1) || (E == -1 && newE < 1)) {
                return;
            }

            AudioSource.PlayClipAtPoint(combineClip, tr.position);
            SetProperties(newZ, newN, newE);
            rb.velocity = (rb.velocity * rb.mass + rb2.velocity * rb2.mass) / (rb.mass + rb2.mass);
            p2.destroyed = true;
            if (newZ == 0 && newN == 0 && newE == 0)
            {
                destroyed = true;
            }
        }
    }

    void Update()
    {
        if (dataParent.IsPaused())
        {
            return;
        }
        float dist = Vector3.Magnitude(tr.position - ca.transform.position);
        if (isPlayer) {
            return;
        }
        Vector3 caScreen = ca.WorldToScreenPoint(tr.position);
        caScreen.z = 0;
        if (dist > 10 && Vector3.Magnitude(Input.mousePosition - caScreen) < 25)
        {
            zoomed = true;
            text.GetComponent<TextMesh>().transform.localScale = new Vector3(0.01f*dist, 0.01f*dist, 0.01f*dist) * textScale / tr.localScale.x;
        }
        else if (dist < 10 || (zoomed && Vector3.Magnitude(Input.mousePosition - caScreen) > 25))
        {
            zoomed = false;
            text.GetComponent<TextMesh>().transform.localScale = new Vector3(0.1f, 0.1f, 0.1f) * textScale / tr.localScale.x;
        }
    }

    public void Sleep()
    {
        lastPosition = rb.position;
        lastRotation = rb.rotation;
        lastVelocity = rb.velocity;

        rb.isKinematic = true;
}

    public void WakeUp()
    {
        rb.isKinematic = false;
        rb.position = lastPosition;
        rb.rotation = lastRotation;
        rb.velocity = lastVelocity;
    }

    public bool IsLepton()
    {
        return (Z + N == 0) && E != 0;
    }

    void OnMouseDown()
    {
        dataParent.Signal(Z, N, E);
    }

    void OnCollisionEnter(Collision collision)
    {

    }

    void OnCollisionStay(Collision collision)
    {

    }

    void OnCollisionExit(Collision collision)
    {

    }

    public void SetTextScale(int size, float fontShrink)
    {
        textScale = size;
        text.GetComponent<TextMesh>().fontSize = (int)(text.GetComponent<TextMesh>().fontSize / size * fontShrink);
        text.GetComponent<TextMesh>().transform.localScale *= size;
    }
}
