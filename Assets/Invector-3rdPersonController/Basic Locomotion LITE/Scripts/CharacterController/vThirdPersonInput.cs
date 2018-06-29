using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace Invector.CharacterController
{
    public class vThirdPersonInput : NetworkBehaviour
    {
        #region variables

        [Header("Default Inputs")]
        public bool isBot = false;
        public string horizontalInput = "Horizontal";
        public string verticallInput = "Vertical";
        public KeyCode jumpInput = KeyCode.Space;
        public KeyCode strafeInput = KeyCode.Tab;
        public KeyCode sprintInput = KeyCode.LeftShift;
        public KeyCode walkInput = KeyCode.CapsLock;
        public KeyCode shootInput = KeyCode.Mouse0;
        public KeyCode aimInput = KeyCode.Mouse1;
        public KeyCode crouchInput = KeyCode.C;
        public KeyCode proneInput = KeyCode.Z;
        public KeyCode reloadInput = KeyCode.R;
        public KeyCode throwGrenadeInput = KeyCode.G;
        public KeyCode bayonetInput = KeyCode.F;
        public KeyCode specialAbilityInput = KeyCode.Q;

        [Header("Camera Settings")]
        public string rotateCameraXInput = "Mouse X";
        public string rotateCameraYInput = "Mouse Y";

        protected vThirdPersonCamera tpCamera;                // acess camera info        
        [HideInInspector]
        public string customCameraState;                    // generic string to change the CameraState        
        [HideInInspector]
        public string customlookAtPoint;                    // generic string to change the CameraPoint of the Fixed Point Mode        
        [HideInInspector]
        public bool changeCameraState;                      // generic bool to change the CameraState        
        [HideInInspector]
        public bool smoothCameraState;                      // generic bool to know if the state will change with or without lerp  
        [HideInInspector]
        public bool keepDirection;                          // keep the current direction in case you change the cameraState

        protected vThirdPersonController cc;                // access the ThirdPersonController component    

        protected float isHoldingAim = 0;
        protected float holdAimThreshold = 1.5f; //held for a second and a half

        #endregion

        protected virtual void Start()
        {
            CharacterInit();
        }

        protected virtual void CharacterInit()
        {
            cc = GetComponent<vThirdPersonController>();
            cc.isBot = isBot;

            if (cc != null)
                cc.Init();

            tpCamera = this.gameObject.GetComponentInChildren<vThirdPersonCamera>();
            if (tpCamera)
            {
                if (isLocalPlayer)
                {
                    tpCamera.SetMainTarget(this.transform);
                }
                else
                {
                    tpCamera.gameObject.GetComponentInChildren<Camera>().enabled = false;
                    tpCamera.gameObject.GetComponentInChildren<AudioListener>().enabled = false;
                    tpCamera.enabled = false;
                }
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Debug.Log("Name of tpCamera = " + tpCamera.gameObject.name);
        }

        protected virtual void LateUpdate()
        {
            //if this is another client connected, just return
            if (!isLocalPlayer)
            {
                return;
            }
            if (cc == null) return;             // returns if didn't find the controller
            if(!cc.isDead)
            {
                InputHandle();                      // update input methods
            }
            ExitGameInput();
            UpdateCameraStates();               // update camera states
        }

        protected virtual void FixedUpdate()
        {
            //if this is another client connected, just return
            if (!isLocalPlayer)
            {
                return;
            }
            if (!cc.isDead)
            {
                cc.AirControl();
            }
            
        }

        protected virtual void Update()
        {
            //if this is another client connected, just return
            if (!isLocalPlayer)
            {
                return;
            }
            if (!cc.isDead)
            {
                cc.UpdateMotor();                   // call ThirdPersonMotor methods               
                cc.UpdateAnimator();                // call ThirdPersonAnimator methods	
            }               
        }

        protected virtual void InputHandle()
        {
            CameraInput();
            if (!cc.lockMovement && !cc.isDead)
            {
                MoveCharacter();
                SprintInput();
                WalkInput();
                StrafeInput();
                CrouchInput();
                ProneInput();
                JumpInput();
                SelectWeaponInput();
                AimInput();
                ShootInput();
                ReloadInput();
                ThrowGrenadeInput();
                BayonetInput();
                SpecialAbilityInput();
            }

        }

        #region Basic Locomotion Inputs
        protected virtual void SpecialAbilityInput()
        {
            if(Input.GetKeyDown(specialAbilityInput))
            {
                cc.SpecialAbility(tpCamera.transform.GetChild(0).gameObject);
            }
        }
        protected virtual void BayonetInput()
        {
            if(Input.GetKeyDown(bayonetInput))
            {
                cc.BayonetAttack();
            }
        }
        protected virtual void ThrowGrenadeInput()
        {
            if(Input.GetKeyDown(throwGrenadeInput))
            {
                StartCoroutine(cc.ThrowGrenadeTowardCamera(tpCamera.gameObject));
            }
        }
        protected virtual void SelectWeaponInput()
        {
            if(Input.GetAxis("Mouse ScrollWheel") > 0f) //scroll down
            {
                cc.SelectWeapon(true);
            }
            else if(Input.GetAxis("Mouse ScrollWheel") < 0f) //scroll up
            {
                cc.SelectWeapon(false);
            }
        }
        protected virtual void AimInput()
        {
            if(Input.GetKeyDown(aimInput) && !cc.isAiming)
            {
                cc.Aim(true);
                cc.Walk(true);
                isHoldingAim = 0;
            }
            else if(Input.GetKeyDown(aimInput) && cc.isAiming)
            {
                cc.Aim(false);
                cc.Walk(false);
            }
            else if (Input.GetKey(aimInput) && cc.isAiming)
            {
                isHoldingAim += Time.deltaTime;
            }
            else if(Input.GetKeyUp(aimInput) && cc.isAiming)
            {
                if(isHoldingAim > holdAimThreshold)
                {
                    cc.Aim(false);
                    cc.Walk(false);
                }
                isHoldingAim = 0;
            }
        }
        protected virtual void CrouchInput()
        {
            if (Input.GetKeyDown(crouchInput) && !cc.isCrouching && !cc.isSprinting)
                cc.Crouch(true);
            else if (Input.GetKeyDown(crouchInput) && cc.isCrouching)
            {
                cc.Crouch(false);
            }
        }
        protected virtual void ProneInput()
        {
            if (Input.GetKeyDown(proneInput) && !cc.isProning && !cc.isSprinting)
                cc.Prone(true);
            else if (Input.GetKeyDown(proneInput) && cc.isProning)
            {
                cc.Prone(false);
                cc.Aim(false);
                cc.Walk(false);
            }
                
        }
        protected virtual void ShootInput()
        {
            if (Input.GetKey(shootInput))
            {
                cc.Shoot();
            }
        }
        protected virtual void ReloadInput()
        {
            if (Input.GetKey(reloadInput))
            {
                cc.Reload();
            }
        }

        protected virtual void MoveCharacter()
        {
            cc.input.x = Input.GetAxis(horizontalInput);
            cc.input.y = Input.GetAxis(verticallInput);
        }

        protected virtual void StrafeInput()
        {
            if (Input.GetKeyDown(strafeInput))
            {
                cc.Strafe();
            }
        }

        protected virtual void SprintInput()
        {
            if (Input.GetKeyDown(sprintInput))
                cc.Sprint(true);
            else if (Input.GetKeyUp(sprintInput))
                cc.Sprint(false);
        }

        protected virtual void JumpInput()
        {
            if (Input.GetKeyDown(jumpInput) && !cc.isProning && cc.CanJumpFromCrouch())
            {
                cc.Jump();
            }
            else if(Input.GetKeyDown(jumpInput) && cc.isProning)
            {
                cc.Prone(false);
                cc.Aim(false);
                cc.Walk(false);
            }
        }

        protected virtual void WalkInput()
        {
            if (Input.GetKeyDown(walkInput))
            {
                cc.Walk(true);
            }
            else if (Input.GetKeyUp(walkInput))
            {
                cc.Walk(false);
            }
        }

        protected virtual void ExitGameInput()
        {
            // just a example to quit the application 
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Cursor.visible)
                    Cursor.visible = true;
                else
                    Application.Quit();
            }
        }

        #endregion

        #region Camera Methods

        protected virtual void CameraInput()
        {
            if (tpCamera == null)
                return;
            var Y = Input.GetAxis(rotateCameraYInput);
            var X = Input.GetAxis(rotateCameraXInput);

            tpCamera.RotateCamera(X, Y);

            // tranform Character direction from camera if not KeepDirection
            if (!keepDirection && !cc.isDead)
                cc.UpdateTargetDirection(tpCamera != null ? tpCamera.transform : null);
            // rotate the character with the camera while strafing    
            
            if(!cc.isDead)
                RotateWithCamera(tpCamera != null ? tpCamera.transform : null);

            tpCamera.ChangeCameraMode(cc.isAiming, cc.isCrouching, cc.isProning);
        }

        protected virtual void UpdateCameraStates()
        {
            // CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData
            if (tpCamera == null)
            {
                tpCamera = this.transform.parent.GetComponentInChildren<vThirdPersonCamera>();//FindObjectOfType<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }
        }

        protected virtual void RotateWithCamera(Transform cameraTransform)
        {
            if (cc.isStrafing && !cc.lockMovement && !cc.lockMovement)
            {
                cc.RotateWithAnotherTransform(cameraTransform);
            }
        }
        #endregion     

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Debug.Log("Local player has started.");
        }
    }
}