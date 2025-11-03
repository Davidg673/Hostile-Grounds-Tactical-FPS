using UnityEngine;

public class BuyZone : MonoBehaviour
{
    public GameController.Team team;

    void OnTriggerEnter(Collider other)
    {
        WeaponHandler weaponHandlerScript = other.GetComponent<WeaponHandler>();
        if (weaponHandlerScript) weaponHandlerScript.InBuyZone(team);
    }
    
    void OnTriggerExit(Collider other)
    {
        WeaponHandler weaponHandlerScript = other.GetComponent<WeaponHandler>();
        if (weaponHandlerScript) weaponHandlerScript.OutOfBuyZone(team);
    }
}
