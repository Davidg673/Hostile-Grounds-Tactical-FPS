using UnityEngine;
using UnityEngine.UI;

namespace EnemyAI
{
	// HealthBillboardManager aligns the health HUD above the object to always face the main camera.
	public class HealthBillboardManager : MonoBehaviour
	{              
		public bool canDisplay;
		[SerializeField] private GameController.Team team;

		private void Start()
		{
			SetMarker();
			if (!canDisplay) gameObject.SetActive(false);
		}

		//Orient billboard after all camera movement is completed in this frame to avoid jittering
		void LateUpdate()
		{
			if (!canDisplay) return;

			else if (gameObject.activeSelf == false) gameObject.SetActive(true);

			// Orientate marker.
			if (Camera.main!=null)
				transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
					Camera.main.transform.rotation * Vector3.up);
		}

		void SetMarker()
        {
			if (GameController.playerTeam == team)
			{
				canDisplay = true;
			}
			else canDisplay = false;
        }

	}
}