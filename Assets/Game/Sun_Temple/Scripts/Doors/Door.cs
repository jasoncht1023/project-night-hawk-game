using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunTemple
{
   

    public class Door : MonoBehaviour
    {
		public bool IsLocked = false;
        public bool DoorClosed = true;
        public float OpenRotationAmount = 90;
        public float RotationSpeed = 1f;
        public float MaxDistance = 3.0f;
		public string playerTag = "Player";
		private Collider DoorCollider;

		private GameObject Player;
		private InputManager inputManager;
		private CursorManager cursor;

        Vector3 StartRotation;
        float StartAngle = 0;
        float EndAngle = 0;
        float LerpTime = 1f;
        float CurrentLerpTime = 0;
        bool Rotating;
		private bool playerInRange = false;


		private bool scriptIsEnabled = true;



        void Start(){
            StartRotation = transform.localEulerAngles ;
			DoorCollider = GetComponent<BoxCollider> ();

			if (!DoorCollider) {
				Debug.LogWarning (this.GetType ().Name + ".cs on " + gameObject.name + "door has no collider", gameObject);
				scriptIsEnabled = false;
				return;
			}

			Player = GameObject.FindGameObjectWithTag (playerTag);

			if (!Player) {
				Debug.LogWarning (this.GetType ().Name + ".cs on " + this.name + ", No object tagged with " + playerTag + " found in Scene", gameObject);
				scriptIsEnabled = false;
				return;
			}

			inputManager = FindFirstObjectByType<InputManager>();
			if (!inputManager) {
				Debug.LogWarning (this.GetType ().Name + ", No InputManager found in Scene", gameObject);
				scriptIsEnabled = false;
			}
		
			cursor = CursorManager.instance;

			if (cursor != null) {
				cursor.SetCursorToDefault ();
			}

					
        }



		void Update()
		{
			if (scriptIsEnabled) {
				if (Rotating) {
					Rotate ();
				}

				CheckPlayerDistance();

				if (playerInRange && inputManager.interactInput) {
					TryToOpen();
				}

				if (cursor != null && playerInRange) {
					UpdateCursorHint();
				}
			}

		}


		void CheckPlayerDistance() {
			playerInRange = Mathf.Abs(Vector3.Distance(transform.position, Player.transform.position)) <= MaxDistance;
		}


		void TryToOpen(){
			if (IsLocked == false) {
				Activate();
			}
		}



		void UpdateCursorHint(){
            // check range and set cursor hint only if in range
            float distance = Mathf.Abs(Vector3.Distance(transform.position, Player.transform.position));
            if (distance <= MaxDistance - 0.5f)
            {
                if (IsLocked)
                {
                    cursor.SetCursorToLocked();
                }
                else
                {
                    cursor.SetCursorToDoor();
                }
            }
            else
            {
                cursor.SetCursorToDefault();
            }
		}




        public void Activate()
        {
            if (DoorClosed)
                Open();
            else
                Close();
        }



       



        void Rotate()
        {
            CurrentLerpTime += Time.deltaTime * RotationSpeed;
            if (CurrentLerpTime > LerpTime)
            {
                CurrentLerpTime = LerpTime;
            }

            float _Perc = CurrentLerpTime / LerpTime;

            float _Angle = CircularLerp.Clerp(StartAngle, EndAngle, _Perc);
            transform.localEulerAngles = new Vector3(transform.eulerAngles.x, _Angle, transform.eulerAngles.z);

			if (CurrentLerpTime == LerpTime) {
				Rotating = false;
				DoorCollider.enabled = true;
			}
              
           
        }



        void Open()
        {
			DoorCollider.enabled = false;
            DoorClosed = false;
            StartAngle = transform.localEulerAngles.y;
            EndAngle =  StartRotation.y + OpenRotationAmount;
            CurrentLerpTime = 0;
            Rotating = true;
        }



        void Close()
        {
			DoorCollider.enabled = false;
            DoorClosed = true;
            StartAngle = transform.localEulerAngles.y;
            EndAngle = transform.localEulerAngles.y - OpenRotationAmount;
            CurrentLerpTime = 0;
            Rotating = true;
        }

    }
}