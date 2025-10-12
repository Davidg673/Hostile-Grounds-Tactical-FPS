using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRecoil : MonoBehaviour
{

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;


    void Start()
    {
        
    }

    void Update()
    {
        float mouseY = Input.GetAxisRaw("Mouse Y");
        if (mouseY < 0f)
        {
            targetRotation = Vector3.MoveTowards(targetRotation, Vector3.zero, Mathf.Abs(mouseY) *10f);
        }

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(float recoilX,float recoilY,float recoilZ)
    {
        targetRotation += new Vector3(-recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }

}
