using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaMenu : MonoBehaviour
{
    public GameObject menu;
    public Slider staminaBar;
    public PlayerMovement playerMovement;

    private float delay = 0.25f;
    private float delayTimer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerMovement.climbTimer > 0f && !menu.activeInHierarchy)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= delay)
            {
                delayTimer = 0f;
                menu.SetActive(true);
            }
        }

        if (playerMovement.climbTimer <= 0f)
        {
            if (delayTimer > 0f)
            {
                delayTimer = 0f;
            }

            if (menu.activeInHierarchy)
            {
                menu.SetActive(false);
            }
        }

        if (menu.activeInHierarchy)
        {
            staminaBar.value = playerMovement.climbMaxTime - playerMovement.climbTimer > 0 ? playerMovement.climbMaxTime - playerMovement.climbTimer : 0;
        }
    }
}
