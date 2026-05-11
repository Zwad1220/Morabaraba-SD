using UnityEngine;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public TMP_Dropdown difficultyDropdown;
    public GameObject MainMenuScreen;
    public Placement placement;

    void Awake()
    {
        placement.enabled = false;
    }

    void Start()
    {
        // Populate the difficulty dropdown with options
        difficultyDropdown.options.Clear();
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Easy"));
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Medium"));
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Hard"));

        difficultyDropdown.value = 0;// set default to Easy
    }

    public void OnPlayAIButtonClicked()// called when the "Play vs AI" button is clicked
    {
        placement.enabled = true;
        MainMenuScreen.SetActive(false);

        AIManager.instance.isAIActive = true;// enable AI for human vs AI mode
        AIManager.instance.currentDifficulty = (AIManager.Difficulty)difficultyDropdown.value;// set the AI difficulty based on the dropdown selection
    }

    public void OnPlayHumanButtonClicked()// called when the "Play vs Human" button is clicked
    {
        placement.enabled = true;// enable the placement script for human interaction
        MainMenuScreen.SetActive(false);

        AIManager.instance.isAIActive = false;// disable AI for human vs human mode
    }

    public void OnExitToMenuClicked()
    {
        // Re-activate the Main Menu UI
        MainMenuScreen.SetActive(true);

        // Disable game input
        placement.enabled = false;
        Movement movementScript = FindObjectOfType<Movement>();
        if (movementScript != null) movementScript.enabled = false;

        // Match the name in GameManager.cs (ResetGame)
        GameManager.instance.ResetGame();
    }

}
