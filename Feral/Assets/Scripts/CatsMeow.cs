using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatsMeow : MonoBehaviour
{
    public AudioClip[] meowSounds;
    public AudioSource speaker;
    public Animator anim;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Meow();
        }
    }

    void Meow()
    {
        if (anim.GetBool("IsMeowing"))
        {
            // Already meowing. Ignore.
            return;
        }
        anim.SetBool("IsMeowing", true);
        speaker.clip = meowSounds[Random.Range(0,meowSounds.Length)];
        speaker.Play();
    }
}
