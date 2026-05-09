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
        difficultyDropdown.options.Clear();
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Easy"));
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Medium"));
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Hard"));

        difficultyDropdown.value = 0;
    }

    public void OnPlayAIButtonClicked()
    {
        placement.enabled = true;
        MainMenuScreen.SetActive(false);

        AIManager.instance.isAIActive = true;
        AIManager.instance.currentDifficulty =
            (AIManager.Difficulty)difficultyDropdown.value;
    }

    public void OnPlayHumanButtonClicked()
    {
        placement.enabled = true;
        MainMenuScreen.SetActive(false);

        AIManager.instance.isAIActive = false;
    }
}
