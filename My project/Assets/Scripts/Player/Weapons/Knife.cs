using UnityEngine;

public class Knife : MonoBehaviour
{
    public Transform handTransform;
    private WeaponHandler weaponHandlerScript;
    private PlayerUI playerUI;
    public float cooldown = 0.4f;
    private float cooldownCounter = 0f;

    private void Start()
    {
        weaponHandlerScript=GetComponentInParent<WeaponHandler>();
        weaponHandlerScript.OnWeaponUseStarted+= PlayAnimation;
        playerUI = GetComponentInParent<PlayerUI>();
    }

    private void Update()
    {
        if (cooldownCounter<cooldown)
            cooldownCounter+=Time.deltaTime;
    }


    public void CheckHit()
    {
        Collider[] hitsResult = Physics.OverlapBox(handTransform.position,new Vector3(0.2f,0.2f,0.2f));
        Collider hitInstance = default;

        if (hitsResult.Length > 1)
        {
            hitInstance = hitsResult[0];
        }
        else return;

        int objectLayer = hitInstance.gameObject.layer; 
        string objectTag = hitInstance.gameObject.tag;
        Vector3 point = hitInstance.ClosestPoint(transform.position);

        if (((1<<objectLayer) & playerUI.enemyLayer) != 0) //bitwise operation to see if the two match
        {
            hitInstance.SendMessageUpwards("HitCallback", new HealthManager.DamageInfo(point, transform.position, 10, hitInstance,gameObject), SendMessageOptions.DontRequireReceiver);        
        }
    }

    private void PlayAnimation()
    {
        if (cooldownCounter > cooldown)
        {
            GetComponent<Animator>().SetTrigger("Punch");
            cooldownCounter=0f;            
        }
    }

}
