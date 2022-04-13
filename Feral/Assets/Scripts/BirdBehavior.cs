using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBehavior : MonoBehaviour
{
    private Vector3 start;
    public MeshFilter mesh;
    public Mesh flight;
    public Mesh stand;
    // Start is called before the first frame update
    void Start()
    {
        start = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Reset()
    {
        mesh.mesh = stand;
        transform.position = start;
    }

    public void Fly()
    {
        mesh.mesh = flight;
        transform.position += (new Vector3(0f, 25f, 0f) + transform.forward * 10f) * Time.deltaTime;
    }
}
