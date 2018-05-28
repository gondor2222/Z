using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
    public Rigidbody rb;
    public Transform tr;
    public Camera ca;
    public Renderer re;
    public readonly float rotationSpeed = 3;
    public readonly float rotationInertia = 0.1f;
    public readonly float moveSpeed = 10;
    public readonly float zoomSpeed = 10;
    public readonly float startZoom = -1f;
    private float zoom;
    public int framesPerSecond = 10;
    public int numFrames = 16;
    public float max_zoom = -1f;
    public float min_zoom = -20f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        ca = GetComponentInChildren<Camera>();
        re = GetComponent<Renderer>();
        zoom = startZoom;
    }

    void Update()
    {

    }
	void FixedUpdate()
    {
        if (GameObject.Find("Daemon").GetComponent<Data>().IsPaused())
        {
            return;
        }
        float dx = Input.GetAxis("X1") * moveSpeed * Time.deltaTime;
        float dy = Input.GetAxis("X2") * moveSpeed * Time.deltaTime;
        float dz = Input.GetAxis("X3") * moveSpeed * Time.deltaTime;
        float dr = Input.GetAxis("R") * rotationSpeed * Time.deltaTime;
        float dp = Input.GetAxis("P") * rotationSpeed * Time.deltaTime;
        float dyaw = Input.GetAxis("Y") * rotationSpeed * Time.deltaTime;
        float dzoom1 = Input.GetAxis("Zoom");
        float dzoom2 = Input.GetAxis("Zoom2");
        zoom = zoom * Mathf.Exp(-(dzoom1 != 0 ? dzoom1 : dzoom2) * zoomSpeed * Time.deltaTime);
        zoom = Mathf.Clamp(zoom, min_zoom, max_zoom);
        int mouseHeldDown = (Input.GetMouseButton(0) ? 1 : 0);
        rb.velocity = rb.velocity + (ca.transform.right * dx) + (ca.transform.up * dy) + (ca.transform.forward * dz);
        rb.angularVelocity = rb.angularVelocity + ((rb.transform.up * dyaw * mouseHeldDown - rb.transform.right * dp * mouseHeldDown - rb.transform.forward * dr * 5)
         * rotationInertia);
        //Debug.Log(zoom);

    }

    void LateUpdate()
    {
        ca.transform.position = Vector3.Lerp(ca.transform.position, tr.position + zoom* ca.transform.forward,
            0.04f);
    }
}
