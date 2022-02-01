
using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{
    private PlayerMovement characterController;

    [Header("References")]
    public Animator WeaponAnimator;

    [Header("Settings")]
    public WeaponSettingsModel settings;
    bool isInitialised;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;


    Vector3 TargetWeaponRotation;
    Vector3 TargetWeaponRotationVelocity;




    Vector3 newWeapoMovementnRotation;
    Vector3 newWeaponMovementRotationVelocity;


    Vector3 TargetWeaponMovementRotation;
    Vector3 TargetWeaponMovementRotationVelocity;

    private bool isGroundedTrigger;

    [Header("Weapon Breathing")]
    public  float fallingDelay;
    public float swayAmountA = 1;
    public float swayAmountB = 2;
    public float swayScale = 600;
    public float swayLerpSpeed = 14;
    public float swayTime;
    public Vector3 swayPostion;
    public Transform weaponSwayObject;

    [HideInInspector]
    public bool isAimingIn;

    [Header("Sights")]
    public Transform sightTarget;
    public float sightOffset;
    public float aimingInTime;
    private Vector3 weaponSwayPosition;
    private Vector3 weaponSwayPositionVelocity;

   



    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;
    }


    public void Initialise(PlayerMovement CharacterController)
    {
        characterController = CharacterController;
        isInitialised = true;
    }
    private void Update()
    {
        if (!isInitialised)
        {
            return;
        }
        WeaponAnimator.speed = characterController.WeaponAnimationSpeed;
        CalculateWeaponRotation();
        SetWeaponAnimation();
        CalculateWeaponSway();
        CalculateAimingIn();


    }
    
    private void CalculateAimingIn()
    {
        var TargetPosition = transform.position;

        if (isAimingIn)
        {
            TargetPosition = characterController.cameraHolder.transform.position + (weaponSwayObject.transform.position - sightTarget.transform.position) + (characterController.cameraHolder.transform.forward * sightOffset); ;
        }

        weaponSwayPosition=weaponSwayObject.transform.position;
        weaponSwayPosition=Vector3.SmoothDamp(weaponSwayPosition,TargetPosition,ref weaponSwayPositionVelocity,aimingInTime);
        weaponSwayObject.transform.position=weaponSwayPosition+ swayPostion;

    }

    public void TriggerJump()
    {
        isGroundedTrigger = false;
        WeaponAnimator.SetTrigger("Jump");
    }
    private void CalculateWeaponRotation()
    {

        TargetWeaponRotation.y += (isAimingIn?settings.SwayAmount/3:settings.SwayAmount) * (settings.SwayXInverted ? -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;
        TargetWeaponRotation.x += (isAimingIn ? settings.SwayAmount / 3 : settings.SwayAmount) * (settings.SwayYInverted ? characterController.input_View.y : characterController.input_View.y) * Time.deltaTime;

        TargetWeaponRotation.x = Mathf.Clamp(TargetWeaponRotation.x, -settings.swayClampX, settings.swayClampX);
        TargetWeaponRotation.y = Mathf.Clamp(TargetWeaponRotation.y, -settings.swayClampY, settings.swayClampY);
        TargetWeaponRotation.z = isAimingIn ? 0 : TargetWeaponRotation.y;

        TargetWeaponRotation = Vector3.SmoothDamp(TargetWeaponRotation, Vector3.zero, ref TargetWeaponRotationVelocity, settings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, TargetWeaponRotation, ref newWeaponRotationVelocity, settings.SwaySmoothing);

        TargetWeaponMovementRotation.z = (isAimingIn?settings.MovementSwayX/3:settings.MovementSwayX) * (settings.MovementSwayXInverted ? -characterController.input_Movemnet.x : characterController.input_Movemnet.x);
        TargetWeaponMovementRotation.x = (isAimingIn? settings.MovementSwayY/3:settings.MovementSwayY) * (settings.MovementSwayYInverted ? -characterController.input_Movemnet.y : characterController.input_Movemnet.y);

        TargetWeaponMovementRotation = Vector3.SmoothDamp(TargetWeaponMovementRotation, Vector3.zero, ref TargetWeaponMovementRotationVelocity, settings.MovementSwaySmooting);
        newWeapoMovementnRotation = Vector3.SmoothDamp(newWeapoMovementnRotation, TargetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.MovementSwaySmooting);




        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeapoMovementnRotation);

    }

    private void SetWeaponAnimation()
    {
        if (isGroundedTrigger)
        {
            fallingDelay = 0;
        }
        else
        {
            fallingDelay += Time.deltaTime;
        }

        if (characterController.isGrounded && !isGroundedTrigger && fallingDelay > 0.1f)
        {
            WeaponAnimator.SetTrigger("Land");
            isGroundedTrigger = true;
        }
        else if (!characterController.isGrounded && !isGroundedTrigger)
        {
            Debug.Log("Trigger Falling");
            WeaponAnimator.SetTrigger("Falling");
            isGroundedTrigger = false;
        }


        WeaponAnimator.SetBool("isSprinting",characterController.isSprinting);
        WeaponAnimator.SetFloat("WeaponAnimationSpeed",characterController.WeaponAnimationSpeed);
    }
    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB)/(isAimingIn ? swayScale*3:swayScale);

        swayPostion=Vector3.Lerp(swayPostion, targetPosition,Time.smoothDeltaTime*swayLerpSpeed);
        swayTime +=Time.deltaTime;

        if(swayTime > 6.3f)
        {
            swayTime = 0;
        }

    }


    private Vector3 LissajousCurve(float Time,float A,float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }
}
