using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void OnGamesPressed()
    {
        EnableGamesMenu();
    }

    public void OnMinigamesPressed()
    {
        SceneManager.LoadScene("MinigamesMenu");
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
