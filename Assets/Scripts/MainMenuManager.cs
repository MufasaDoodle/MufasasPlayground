using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject gameSelection;

    // Start is called before the first frame update
    void Start()
    {
        EnableMainMenu();
        Application.targetFrameRate = 60;
    }

    public void OnOptionsPressed()
    {

    }

    public void OnQuitPressed()
    {
        Application.Quit(0);
    }

    public void OnMinigamesPressed()
    {
        EnableGamesMenu();
    }

    public void OnHorseRacingPressed()
    {

    }

    public void OnMinigamesBackPressed()
    {
        EnableMainMenu();
    }

    private void EnableMainMenu()
    {
        mainMenu.SetActive(true);
        gameSelection.SetActive(false);
    }

    private void EnableGamesMenu()
    {
        mainMenu.SetActive(false);
        gameSelection.SetActive(true);
    }
}
