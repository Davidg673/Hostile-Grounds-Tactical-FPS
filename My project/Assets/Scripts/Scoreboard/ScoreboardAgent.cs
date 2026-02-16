using UnityEngine;

public class ScoreboardAgent : MonoBehaviour
{
    public int id;

    public void CallbackScoreboard(int id,int deathsIncrease, int killsIncrease,int moneyIncrease)
    {
        ScoreboardManager.OnScoreboardChange?.Invoke(id, deathsIncrease, killsIncrease, moneyIncrease); 
    }
}
