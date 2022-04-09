using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSpawner : MonoBehaviour
{
    public BirdBehavior[] birds;
    public GameObject player;
    public float approachDistance;
    public bool scattering;
    Renderer m_Renderer;
    // Start is called before the first frame update
    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!scattering && Vector3.Distance(transform.position, player.transform.position) <= approachDistance)
        {
            scattering = true;
        }

        if (scattering && !m_Renderer.isVisible && Vector3.Distance(transform.position, player.transform.position) >= approachDistance * 2f)
        {
            scattering = false;
            foreach(BirdBehavior bird in birds)
            {
                bird.Reset();
            }
        }
        if (scattering)
        {
            foreach (BirdBehavior bird in birds)
            {
                bird.Fly();
            }
        }
    }
}
