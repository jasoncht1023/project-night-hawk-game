using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunTemple {


    public class Door : MonoBehaviour {
        private PlayerUIManager playerUIManager;

        public bool IsLocked = false;
        public bool DoorClosed = true;
        public float OpenRotationAmount = 90;
        public float RotationSpeed = 1f;
        private readonly float MaxDistance = 2.0f;
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



        void Start() {
            StartRotation = transform.localEulerAngles;
            DoorCollider = GetComponent<BoxCollider>();

            if (!DoorCollider) {
                Debug.LogWarning(this.GetType().Name + ".cs on " + gameObject.name + "door has no collider", gameObject);
                scriptIsEnabled = false;
                return;
            }

            Player = GameObject.FindGameObjectWithTag(playerTag);

            if (!Player) {
                Debug.LogWarning(this.GetType().Name + ".cs on " + this.name + ", No object tagged with " + playerTag + " found in Scene", gameObject);
                scriptIsEnabled = false;
                return;
            }

            inputManager = FindFirstObjectByType<InputManager>();
            if (!inputManager) {
                Debug.LogWarning(this.GetType().Name + ", No InputManager found in Scene", gameObject);
                scriptIsEnabled = false;
            }

            playerUIManager = FindFirstObjectByType<PlayerUIManager>();
            if (!playerUIManager) {
                Debug.LogWarning(this.GetType().Name + ", No PlayerUIManager found in Scene", gameObject);
                scriptIsEnabled = false;
            }

            cursor = CursorManager.instance;

            if (cursor != null) {
                cursor.SetCursorToDefault();
            }


        }

        void Update() {
            if (scriptIsEnabled) {
                if (Rotating) {
                    Rotate();
                }

                CheckPlayerDistance();

                if (playerInRange && inputManager.interactInput) {
                    TryToOpen();
                }
            }

        }


        void LateUpdate() {
            if (scriptIsEnabled) {
                if (cursor != null && playerInRange) {
                    UpdateCursorHint();
                }
            }

        }


        void CheckPlayerDistance() {
            playerInRange = Mathf.Abs(Vector3.Distance(transform.position, Player.transform.position)) <= MaxDistance;
        }


        void TryToOpen() {
            if (IsLocked == false) {
                Activate();
            }
        }



        void UpdateCursorHint() {
            float distance = Mathf.Abs(Vector3.Distance(transform.position, Player.transform.position));
            // Debug.Log("Distance to door: " + distance);
            if (distance <= MaxDistance - 0.1f) {
                Debug.Log("Player is in range of door");
                if (IsLocked) {
                    //cursor.SetCursorToLocked();
                    Debug.Log("Door is locked");
                    playerUIManager.ActionUIText("Door is locked");
                }
                else {
                    //cursor.SetCursorToDoor();
                    Debug.Log("Door is unlocked");
                    if (DoorClosed)
                        playerUIManager.ActionUIText("E : Open Door");
                    else
                        playerUIManager.ActionUIText("E : Close Door");
                }
            }
            else {
                cursor.SetCursorToDefault();
                playerUIManager.ActionUIText("");
            }
        }




        public void Activate() {
            if (DoorClosed)
                Open();
            else
                Close();
        }







        void Rotate() {
            CurrentLerpTime += Time.deltaTime * RotationSpeed;
            if (CurrentLerpTime > LerpTime) {
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



        void Open() {
            DoorCollider.enabled = false;
            DoorClosed = false;
            StartAngle = transform.localEulerAngles.y;
            EndAngle = StartRotation.y + OpenRotationAmount;
            CurrentLerpTime = 0;
            Rotating = true;
        }



        void Close() {
            DoorCollider.enabled = false;
            DoorClosed = true;
            StartAngle = transform.localEulerAngles.y;
            EndAngle = transform.localEulerAngles.y - OpenRotationAmount;
            CurrentLerpTime = 0;
            Rotating = true;
        }

    }
}