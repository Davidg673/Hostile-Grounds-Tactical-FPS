using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class ScoreboardElement : MonoBehaviour
{
    private static int elementCount;
    public int id;
    public TMP_Text nameText;
    public TMP_Text moneyText;
    public TMP_Text killsText;
    public TMP_Text deathsText;

    public UnityAction<int, int, int> changeInfo;

    private int totalDeaths=0;
    private int totalKills=0;
    private int totalMoney=500;



    public void Setup(string name)
    {
        id = elementCount;
        elementCount++;
        if (id == 0) nameText.text = "player";
        else nameText.text = name;
        moneyText.text = totalMoney.ToString();   
        killsText.text = totalKills.ToString();        
        deathsText.text = totalDeaths.ToString();        
    }
    public void UpdateElement(int id,int deathsIncrease, int killsIncrease,int moneyIncrease)
    {
        if (id == this.id)
        {
            totalDeaths += deathsIncrease;
            totalKills += killsIncrease;
            totalMoney = (totalMoney+moneyIncrease >0) ? totalMoney+moneyIncrease : totalMoney;

            moneyText.text = totalMoney.ToString();
            killsText.text = totalKills.ToString();
            deathsText.text = totalDeaths.ToString();
        }
    }
}
