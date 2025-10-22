using System.Collections;
using UnityEngine;

public class DroppedWeapon : MonoBehaviour
{
    private string weaponName;
    private int ammoInStock;
    private int ammoInMag;
    public bool isGrenade;
    public GameObject parentGrenade;
    bool canPickUp;
    public GameObject parentWeapon;

    private void OnEnable()
    {
        StartCoroutine(SetInteracteable());
    }
    private void OnTriggerStay(Collider collider)
    {
        if (!canPickUp) return;
        WeaponHandler tempScript = collider.GetComponentInParent<WeaponHandler>();
        
        if (!isGrenade)
        {   
            if (tempScript.PickUpWeapon(parentWeapon, ammoInStock, ammoInMag)) Destroy(gameObject);
        }
        else
        {
            if (tempScript.AddToThrowables(parentGrenade)) Destroy(gameObject);
        }

    }

    public void SetVariables(int ammoInStock, int ammoInMag)
    {
        this.ammoInStock = ammoInStock;
        this.ammoInMag = ammoInMag;
    }
    
    private IEnumerator SetInteracteable()
    {
        yield return new WaitForSeconds(0.5f);
        canPickUp = true;
    }

}
