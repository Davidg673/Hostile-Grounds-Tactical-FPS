using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreboardManager : MonoBehaviour
{
    public static UnityAction<int, int, int, int> OnScoreboardChange;
    public static ScoreboardManager Instance { get; set;}
    public GameObject TsideParent;
    public GameObject CTsideParent;
    public GameObject element;
    public GameObject scoreboardCanv;
    public TMP_Text TSideCount;
    public TMP_Text CTSideCount;
    private int currentCTCount;
    private int currentTCount;
    public List<string> namesList = new List<string>
    {
        "Alpha",
        "Bravo",
        "Charlie",
        "Delta",
        "Echo",
        "Foxtrot",
        "Golf",
        "Hotel",
        "India",
        "Juliett",
        "Kilo",
        "Lima",
        "Mike",
        "November",
        "Oscar",
        "Papa",
        "Quebec",
        "Romeo",
        "Sierra",
        "Tango",
        "Uniform",
        "Victor",
        "Whiskey",
        "X-ray",
        "Yankee",
        "Zulu"
    };

    List<GameObject> elementList = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        OnScoreboardChange += ChangeScoreboard;
    }

    void OnDisable()
    {
        Instance = null;
        OnScoreboardChange -= ChangeScoreboard;
    }

    void Update()
    {
        CheckForInput();
    }

    public static void GlobalAddElement(GameController.Team team)
    {
        Instance.AddElement(team);
    }
    public void AddElement(GameController.Team team)
    {
        GameObject elementInstance = default;

        if (team == GameController.Team.CT)
        {
            elementInstance = Instantiate(element, CTsideParent.transform);
            currentCTCount++;
            TSideCount.text= $"Counter Terrorists - {currentCTCount} {(currentCTCount ==1? "player" :"players")}";

        }
        else
        {
            currentTCount++;
            elementInstance = Instantiate(element, TsideParent.transform);
            TSideCount.text= $"Terrorists - {currentTCount} {(currentTCount ==1? "player" :"players")}";
        }

        if (elementInstance != null)
        {
            elementList.Add(elementInstance);
            elementInstance.SetActive(true);
            int index = Random.Range(0, namesList.Count);
            elementInstance.GetComponent<ScoreboardElement>().Setup(namesList[index]);
            namesList.RemoveAt(index);
        }
        else Debug.LogError("Scoreboard element is null. Cannot add to list.");

    }

    private void ChangeScoreboard(int id, int deathsIncrease, int killsIncrease, int moneyIncrease)
    {
        foreach (GameObject element in elementList)
        {
            element.GetComponent<ScoreboardElement>()?.UpdateElement(id, deathsIncrease, killsIncrease, moneyIncrease);
        }
    }

    private void CheckForInput()
    {
        if (Input.GetKey(KeyCode.Tab)) scoreboardCanv.SetActive(true);
        else scoreboardCanv.SetActive(false);
    }

}
