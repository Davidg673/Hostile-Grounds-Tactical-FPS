using UnityEngine;

public class ResetWeaponPanels : MonoBehaviour
{
    public GameObject defaultPanel;
    public GameObject SecondPanel;
    void Awake()
    {
        BuyableMenuController.OnResetWeaponPanels += ResetPanels;
    }

    public void ResetPanels()
    {
        defaultPanel.SetActive(true);
        SecondPanel.SetActive(false);
    }
}
