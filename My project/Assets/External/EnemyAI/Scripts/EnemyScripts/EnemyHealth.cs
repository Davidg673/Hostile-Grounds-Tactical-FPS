using UnityEngine;
using UnityEngine.AI;

namespace EnemyAI
{
	// EnemyHealth is a the enemy NPC specific health manager.
	// Any in-game entity that reacts to a shot must have a HealthManager script.
	public class EnemyHealth : HealthManager
	{
		[Tooltip("The current NPC health.")]
		public float health = 100f;
		[Tooltip("The NPC health HUD prefab.")]
		public GameObject healthHUD;
		[Tooltip("The game object particle emitted when hit.")]
		public GameObject bloodSample;
		[Tooltip("Use headshot damage multiplier?")]
		public bool headshot;

		private float totalHealth;                                  // The total NPC initial health.
		private Transform weapon;                                   // The NPC weapon.
		private float originalBarScale;                             // The initial NPC health bar size.
		private Animator anim;                                      // The NPC animator controller.
		private StateController controller;                         // The NPC AI FSM controller.
		private static readonly int Hit = Animator.StringToHash("Hit");
		[SerializeField] AudioSource sfxSource;
		private HealthBillboardManager marker;
		[SerializeField] private GameObject player;	


		private void Awake()
		{

			// Set up the references.
			totalHealth = health;

			anim = GetComponent<Animator>();
			controller = GetComponent<StateController>();

			// Find the NPC weapon.
			foreach (Transform child in anim.GetBoneTransform(HumanBodyBones.RightHand))
			{
				weapon = child.Find("muzzle");
				if (weapon != null)
				{
					break;
				}
			}
			weapon = weapon.parent;
			marker = GetComponentInChildren<HealthBillboardManager>();
		}

		// Receive damage from shots taken.
		public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart, GameObject origin = null)
		{
			// Headshot multiplier. On default values, instantly kills NPC.
			if (!dead && headshot && bodyPart.transform == anim.GetBoneTransform(HumanBodyBones.Head))
			{
				// Default damage multiplier is 10x.
				damage *= 2;
				// Call headshot HUD callback, if any.
				GameObject.FindGameObjectWithTag("GameController").SendMessage("HeadShotCallback", SendMessageOptions.DontRequireReceiver);
				sfxSource.PlayOneShot(controller.headShotClip);
			}

			// Create spouted blood particle on shot location.
			Instantiate(bloodSample, location, Quaternion.LookRotation(-direction), this.transform);
			// Take damage received from current health.
			health -= damage;
			if (bodyPart.transform != anim.GetBoneTransform(HumanBodyBones.Head)) sfxSource.PlayOneShot(controller.bodyShotClip);
			// Is the NPC alive?
			if (!dead)
			{
				// Trigger hit animation.
				if(!anim.IsInTransition(3) && anim.GetCurrentAnimatorStateInfo(3).IsName("No hit"))
					anim.SetTrigger(Hit);

				// Update FSM related references.
				controller.variables.feelAlert = true;
				controller.personalTarget = controller.aimTarget.position;
			}
			// Time to die.
			if (health <= 0)
			{
				// Kill the NPC?
				if (!dead)
				{
					if (origin == player)
						PlayerUI.AddToMoney(500);
					ScoreboardAgent originAgent = origin.GetComponentInParent<ScoreboardAgent>();
					if (originAgent == null) originAgent = origin.GetComponent<ScoreboardAgent>();
					originAgent.CallbackScoreboard(originAgent.id, 0, 1, 500);
					ScoreboardAgent objectAgent = GetComponent<ScoreboardAgent>();
					objectAgent.CallbackScoreboard(objectAgent.id, 1, 0, 0);
	
					Kill();
                }

				// Shooting a dead body? Just apply shot force on the ragdoll part.
				bodyPart.GetComponent<Rigidbody>().AddForce(100f * direction.normalized, ForceMode.Impulse);
			}
		}

		// Remove unnecessary components on killed NPC and set as dead.
		public void Kill()
		{
			// Destroy all other MonoBehaviour scripts attached to the NPC.
			foreach (MonoBehaviour mb in this.GetComponents<MonoBehaviour>())
			{
				if (this != mb)
					Destroy(mb);
			}
			Destroy(this.GetComponent<NavMeshAgent>());
			RemoveAllForces();
			anim.enabled = false;
			Destroy(weapon.gameObject);
			//			Destroy(hud.gameObject);
			dead = true;

			sfxSource.PlayOneShot(controller.deathClips[Random.Range(0, controller.deathClips.Length)]);
			Animator animator = GetComponent<Animator>();
			animator.SetFloat("Speed", 0);
			StateController.OnRemoveFromPool?.Invoke(gameObject);
			marker.gameObject.SetActive(false);	
		}


		// Remove existing forces and set ragdoll parts as not kinematic to interact with physics.
		private void RemoveAllForces()
		{
			foreach (Rigidbody member in GetComponentsInChildren<Rigidbody>())
			{
				member.isKinematic = false;
				member.linearVelocity = Vector3.zero;
			}
		}
	}
}