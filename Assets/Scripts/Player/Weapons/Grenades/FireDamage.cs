using System;
using System.Collections;
using UnityEngine;

public class FireDamage : MonoBehaviour
{
    private bool canDamage=true;
    PlayerHealth healthScript;
    [SerializeField] private float damageCooldown;
    Vector3 hitPoint;
    Vector3 direction;


    void Update()
    {
        if (canDamage && healthScript!=null)
        {
            canDamage = false;
            healthScript.TakeDamage(hitPoint, direction, 5f,null,gameObject);
            StartCoroutine(ResetCooldown());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        healthScript = other.GetComponent<PlayerHealth>();
        hitPoint = other.ClosestPoint(transform.position);
        direction = hitPoint - transform.position;
    }


    void OnTriggerExit(Collider other)
    {
        healthScript = null;
    }


    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        canDamage = true;
    }
    
}
