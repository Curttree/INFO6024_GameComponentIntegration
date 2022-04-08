using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenu;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused) // Resume
            {
                Resume();
            }
            else // Pause
            {
                pauseMenu.SetActive(true);
                Time.timeScale = 0.0f;
                isPaused = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Options()
    {

    }
}
