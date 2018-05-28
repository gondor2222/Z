using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProperties : MonoBehaviour {
    Transform tr;
    Renderer re;
    public int Z;
    public int N;
    public int E;
    void Awake()
    {
        Z = 0;
        N = 0;
        E = 0;
        tr = GetComponent<Transform>();
        tr.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        re = GetComponent<Renderer>();
        re.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        re.sortingOrder = 8;
        re.material = Resources.Load("materials/shells/ParticleMaterial", typeof(Material)) as Material;


    }

    void Start()
    {

    }

    public void SetProperties(int Z, int N, int E)
    {
        this.Z = Z;
        this.N = N;
        this.E = E;


        gameObject.name = string.Format("(BASIC) {0}-{1}", Z, Z+N);
    }

    void Update()
    {
        tr.position = Vector3.Lerp(tr.position, Data.ZNtopos(Z, N), 0.3f);
    }
}
