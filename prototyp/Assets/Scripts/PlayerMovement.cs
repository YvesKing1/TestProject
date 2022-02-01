using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Models;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController characterController;
    private DefaultInput defaultInput;
    [HideInInspector]
    public Vector2 input_Movemnet;
    [HideInInspector]
    public Vector2 input_View;

    private Vector3 newCamRotation;
    private Vector3 newCharacterRotation;


    [Header("Referrences")]
    public Transform cameraHolder;
    public Transform feetTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;
    public LayerMask playerMask;
    public LayerMask groundMask;

    [Header("Gravity")]
    public float gravityAmout;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;
    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStancesmoothing;
    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;
    private float stanceCheckErrorMargin = 0.05f;

    private float CameraHeight;
    private float CameraHeightVelocity;
    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    [HideInInspector]
    public bool isSprinting;
    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;


    [Header("Weapon")]
    public WeaponController currentWeapon;
    public float WeaponAnimationSpeed;
    public bool isGrounded;
    [HideInInspector]
    public bool isFalling;

    [Header("Aiming In")]
    public bool isAimingIn;



    #region- Awake-
    private void Awake()
    {
        defaultInput= new DefaultInput();

        defaultInput.Character.Movement.performed+=e=>input_Movemnet=e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();

        defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Prone.performed += e => Prone();
        defaultInput.Character.Sprint.performed += e => ToddleSprinte();
        defaultInput.Character.SprintRealese.performed += e => StopSprinte();

        defaultInput.Weapon.Firet2Press.performed += e => AimingInPressed();
        defaultInput.Weapon.Fire2Released.performed += e => AimingInReleased();

        defaultInput.Enable();
        newCamRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController= GetComponent<CharacterController>();

        CameraHeight = cameraHolder.localPosition.y;

        if (currentWeapon)
        {
            currentWeapon.Initialise(this);
        }





    }
    #endregion
    #region-Update-
    private void Update()
    {
        SetIsGrounded();    
        SetIsFalling();
        CalculateView();
        CalculateMocement();
        CalculateJump();
        CalculateStance();
        CalculateAimingIn();
     
    }
    #endregion
    #region-Aiming In-

    private void AimingInPressed()
    {
        isAimingIn=true;
    }

    private void AimingInReleased()
    {
        isAimingIn =false;
    }
    private void CalculateAimingIn()
    {
        if (!currentWeapon)
        {
            return;
        }
        currentWeapon.isAimingIn =isAimingIn;
    }


    #endregion
    #region-IsFalling/isGrounded-
    private void SetIsGrounded()
    {

        isGrounded =Physics.CheckSphere(feetTransform.position,playerSettings.isGroundedRadius,groundMask);
    }
    private void SetIsFalling()
    {    
        isFalling = (!isGrounded && characterController.velocity.magnitude >= playerSettings.isFallingSpeed);
        
    }
    #endregion

    #region-View/Movement-
    private void CalculateView()
    {
        newCharacterRotation.y+=(isAimingIn? playerSettings.ViewXSensitivity*playerSettings.AimingSensitivityEffector:playerSettings.ViewXSensitivity)*(playerSettings.ViewXInverted?-input_View.x:input_View.x)*Time.deltaTime;
        transform.localRotation=Quaternion.Euler(newCharacterRotation);

        newCamRotation.x += (isAimingIn ? playerSettings.ViewYSensitivity*playerSettings.AimingSensitivityEffector:playerSettings.ViewYSensitivity) *(playerSettings.ViewYInverted?input_View.y:-input_View.y)*Time.deltaTime;
        newCamRotation.x = Mathf.Clamp(newCamRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCamRotation);

    }
    private void CalculateMocement()
    {
        if (input_Movemnet.y<=0.2f)
        {
            isSprinting = false;
        }

        var verticalSpeed = playerSettings.WalkingForwardSpeed;
        var horizontalSpeed = playerSettings.RunningStrafeSpeed;

        if (isSprinting)
        {
            verticalSpeed = playerSettings.RunningForwardSpeed;
            horizontalSpeed = playerSettings.RunningStrafeSpeed;
        }


        verticalSpeed *= playerSettings.SpeedEffector;
        horizontalSpeed*=playerSettings.SpeedEffector;

        if(!isGrounded)
        {
            playerSettings.SpeedEffector = playerSettings.FallingSpeedeffector;
        }
        else if (playerStance == PlayerStance.Crouch)
        {
            playerSettings.SpeedEffector = playerSettings.CrouchSpeedEffector;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            playerSettings.SpeedEffector = playerSettings.ProneSpeedEffector;
        }
        else if (isAimingIn)
        {
            playerSettings.SpeedEffector = playerSettings.AimingSpeedeffector;
        }
        else
        {
            playerSettings.SpeedEffector = 1;
        }


        WeaponAnimationSpeed = characterController.velocity.magnitude/(playerSettings.WalkingForwardSpeed*playerSettings.SpeedEffector);

        if(WeaponAnimationSpeed > 1)
        {
            WeaponAnimationSpeed = 1;
        }

        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * input_Movemnet.x * Time.deltaTime, 0, verticalSpeed * input_Movemnet.y * Time.deltaTime),ref newMovementSpeedVelocity,isGrounded? playerSettings.Movemnetsmoothing:playerSettings.FallingSmoothing);
       var MovementSpeed = cameraHolder.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravityAmout*Time.deltaTime;
        }
        playerGravity-=gravityAmout*Time.deltaTime;
        
        if(playerGravity < -0.1f&&isGrounded)
        {
            playerGravity = - 0.1f;
        }
       

        MovementSpeed.y += playerGravity;
        MovementSpeed += jumpingForce*Time.deltaTime;


        characterController.Move(MovementSpeed);
    }

    private void CalculateStance()
    {
        var currentStance = playerStandStance;
        if (playerStance == PlayerStance.Crouch)
        {
            currentStance = playerCrouchStance;
        }
        else if(playerStance == PlayerStance.Prone)
        {
            currentStance = playerProneStance;
        }

       CameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.CameraHeight,ref CameraHeightVelocity,playerStancesmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, CameraHeight, cameraHolder.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StandCollider.height, ref stanceCapsuleHeightVelocity,playerStancesmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center,currentStance.StandCollider.center,ref stanceCapsuleCenterVelocity,playerStancesmoothing);
    }
    #endregion
    #region-Jumping-
    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce,Vector3.zero,ref jumpingForceVelocity,playerSettings.JumpingFalloff);
    }
    private void Jump()
    {
        if (!isGrounded|| playerStance == PlayerStance.Prone)
        {
            return;
        }
        if (playerStance==PlayerStance.Crouch)
        {

            if (Stancecheck(playerStandStance.StandCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
            playerGravity = 0;
        currentWeapon.TriggerJump();
        
        
    }
    #endregion

    #region-Stance-
    private void Crouch()
    {
        if(playerStance == PlayerStance.Crouch)
        {
            if (Stancecheck(playerStandStance.StandCollider.height))
            {
                return;
            }




            playerStance= PlayerStance.Stand;
            return;
        }
        if (Stancecheck(playerCrouchStance.StandCollider.height))
        {
            return;
        }
        playerStance = PlayerStance.Crouch;
    }
    private void Prone()
    {
        playerStance= PlayerStance.Prone;
    }
    private bool Stancecheck(float stanceCheckheight)
    {
        var start = new Vector3(feetTransform.position.x,feetTransform.position.y+characterController.radius +stanceCheckErrorMargin,feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - characterController.radius + stanceCheckErrorMargin+stanceCheckheight, feetTransform.position.z);








        return Physics.CheckCapsule(start,end,characterController.radius,playerMask);
    }
    #endregion
    #region-Sprinting-
    private void ToddleSprinte()
    {

        if (input_Movemnet.y <= 0.2f)
        {
            isSprinting = false;
        return;
        }

        isSprinting = !isSprinting;
    }
    private void StopSprinte()
    {

        if (playerSettings.sprintingHold)
        {
            isSprinting = false;

        }
        
            
        

       
    }
    #endregion

    #region-Gizmos-
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feetTransform.position, playerSettings.isGroundedRadius);
    }
    #endregion
}
