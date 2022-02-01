using System;
using System.Collections.Generic;
using UnityEngine;

public static class Models
{
    #region-Player-
    public enum PlayerStance
    {
        Stand,
        Crouch,
        Prone
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;

        public float AimingSensitivityEffector;

        [Header("Movement Settings")]
        public bool sprintingHold;
        public float Movemnetsmoothing;

        public bool ViewXInverted;
        public bool ViewYInverted;
        [Header("Movement-Running")]
        public float RunningForwardSpeed;
        public float RunningStrafeSpeed;

        [Header("Movement-Walking")]
        public float WalkingForwardSpeed;
        public float WalkingBackwardSpeed;
        public float WalkingStrafeSpeed;

        [Header("Jumping")]
        public float JumpingHeight;
        public float JumpingFalloff;
        public float FallingSmoothing;

        [Header("Speed Effectors")]
        public float SpeedEffector = 1;
        public float CrouchSpeedEffector;
        public float ProneSpeedEffector;
        public float FallingSpeedeffector;
        public float AimingSpeedeffector;

        [Header("Is Grounded/Falling")]
        public float isGroundedRadius;
        public float isFallingSpeed;


    }
    [Serializable]
    public class CharacterStance
    {
        public float CameraHeight;
        public CapsuleCollider StandCollider;
    }

    #endregion

    #region - Weapon - 

    [Serializable]
    public class WeaponSettingsModel
    {
        [Header("Weapon Sway")]
        public float SwayAmount;
        public bool SwayYInverted;
        public bool SwayXInverted;
        public float SwaySmoothing;
        public float SwayResetSmoothing;
        public float swayClampX;
        public float swayClampY;




        [Header("Weapon Movement Sway")]
        public float MovementSwayX;
        public float MovementSwayY;
        public bool MovementSwayYInverted;
        public bool MovementSwayXInverted;
        public float MovementSwaySmooting;

    }    





    #endregion
}
