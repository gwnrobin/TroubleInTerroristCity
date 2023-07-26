using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : PlayerComponent
{
    #region Internal
    [Serializable]
    public class MovementStateModule
    {
        public bool Enabled = true;

        [ShowIf("Enabled", true)]
        [Range(0f, 10f)]
        public float SpeedMultiplier = 4.5f;

        [ShowIf("Enabled", true)]
        [Range(0f, 3f)]
        public float StepLength = 1.9f;
    }

    [Serializable]
    public class CoreMovementModule
    {
        [Range(0f, 20f)]
        public float Acceleration = 5f;

        [Range(0f, 20f)]
        public float Damping = 8f;

        [Range(0f, 1f)]
        public float AirborneControl = 0.15f;

        [Range(0f, 3f)]
        public float StepLength = 1.2f;

        [Range(0f, 10f)]
        public float ForwardSpeed = 2.5f;

        [Range(0f, 10f)]
        public float BackSpeed = 2.5f;

        [Range(0f, 10f)]
        public float SideSpeed = 2.5f;

        public AnimationCurve SlopeSpeedMult = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

        public float AntiBumpFactor = 1f;

        [Range(0f, 1f)]
        public float HeadBounceFactor = 0.65f;
    }

    [Serializable]
    public class JumpStateModule
    {
        public bool Enabled = true;

        [ShowIf("Enabled", true)]
        [Range(0f, 3f)]
        public float JumpHeight = 1f;

        [ShowIf("Enabled", true)]
        [Range(0f, 1.5f)]
        public float JumpTimer = 0.3f;
    }

    [Serializable]
    public class LowerHeightStateModule : MovementStateModule
    {
        [ShowIf("Enabled", true)]
        [Range(0f, 2f)]
        public float ControllerHeight = 1f;

        [ShowIf("Enabled", true)]
        [Range(0f, 1f)]
        public float TransitionDuration = 0.3f;
    }

    [Serializable]
    public class SlidingStateModule
    {
        public bool Enabled = false;

        [ShowIf("Enabled", true)]
        [Range(20f, 90f)]
        public float SlideTreeshold = 32f;

        [ShowIf("Enabled", true)]
        [Range(0f, 50f)]
        public float SlideSpeed = 15f;
    }
    #endregion

    public bool IsGrounded { get => controller.isGrounded; }
    public Vector3 Velocity { get => controller.velocity; }
    public Vector3 SurfaceNormal { get; private set; }
    public float SlopeLimit { get => controller.slopeLimit; }
    public float DefaultHeight { get; private set; }

    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int TurnRight = Animator.StringToHash("TurnRight");
    private static readonly int TurnLeft = Animator.StringToHash("TurnLeft");

    [Header("General")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerAnimController playerAnimController;
    [SerializeField] private float moveSmoothing = 2f;
    [SerializeField] private LayerMask m_ObstacleCheckMask = ~0;
    [SerializeField] private float gravity;

    [Space]

    [Header("Camera")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform firstPersonCamera;
    [SerializeField] private float sensitivity;
    [SerializeField] private Vector2 freeLookAngle;

    [Space]

    [Header("Dynamic Motions")]
    [SerializeField] private DynamicMotion aimMotion;
    [SerializeField] private DynamicMotion leanMotion;

    [Space]

    [Header("Turning")]
    [SerializeField] private float turnInPlaceAngle;
    [SerializeField] private AnimationCurve turnCurve = new AnimationCurve(new Keyframe(0f, 0f));
    [SerializeField] private float turnSpeed = 1f;

    [Space]

    [SerializeField]
    [Group]
    private CoreMovementModule m_CoreMovement;

    [SerializeField]
    [Group]
    private MovementStateModule m_RunState;

    [SerializeField]
    [Group]
    private LowerHeightStateModule m_CrouchState;

    [SerializeField]
    [Group]
    private LowerHeightStateModule m_ProneState;

    [SerializeField]
    [Group]
    private JumpStateModule m_JumpState;

    [SerializeField]
    [Group]
    private SlidingStateModule m_SlidingState;

    private MovementStateModule m_CurrentMovementState;


    private CollisionFlags m_CollisionFlags;

    private Quaternion moveRotation;
    private float turnProgress = 1f;
    private bool isTurning = false;

    private Vector2 _freeLookInput;
    private Vector2 _smoothAnimatorMove;
    private Vector2 _smoothMove;

    private Vector2 look;
    private bool _freeLook;

    private float m_DistMovedSinceLastCycleEnded;
    private float m_CurrentStepLength;

    private Vector3 m_SlideVelocity;
    private Vector3 m_DesiredVelocityLocal;
    private bool m_PreviouslyGrounded;
    private float m_LastLandTime;
    private float m_NextTimeCanChangeHeight;

    private void Start()
    {
        DefaultHeight = controller.height;

        moveRotation = transform.rotation;

        Player.Crouch.AddStartListener(Crouch);
        Player.Crouch.AddStopListener(Standup);

        Player.Crouch.SetStartTryer(() => { return Try_ToggleCrouch(m_CrouchState); });
        Player.Crouch.SetStopTryer(() => { return Try_ToggleCrouch(null); });

        Player.Jump.SetStartTryer(Try_Jump);

        Player.Sprint.SetStartTryer(TryStartSprint);
        Player.Sprint.AddStartListener(StartSprint);
        Player.Sprint.AddStopListener(StopSprint);

        Player.Lean.AddStartListener(Lean);
        Player.Lean.AddStopListener(() => Player.CharAnimData.leanDirection = 0);

        playerAnimController.FpsAnimator.OnPostAnimUpdate += UpdateCameraRotation;
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;

        Vector3 translation;

        if (IsGrounded)
        {
            translation = transform.TransformVector(m_DesiredVelocityLocal) * deltaTime;

            if (!Player.Jump.Active)
                translation.y = -.05f;
        }
        else
            translation = transform.TransformVector(m_DesiredVelocityLocal * deltaTime);

        m_CollisionFlags = controller.Move(translation);

        if ((m_CollisionFlags & CollisionFlags.Below) == CollisionFlags.Below && !m_PreviouslyGrounded)
        {
            bool wasJumping = Player.Jump.Active;

            if (Player.Jump.Active)
                Player.Jump.ForceStop();

            //Player.FallImpact.Send(Mathf.Abs(m_DesiredVelocityLocal.y));

            m_LastLandTime = Time.time;

            if (wasJumping)
                m_DesiredVelocityLocal = Vector3.ClampMagnitude(m_DesiredVelocityLocal, 1f);
        }

        // Check if the top of the controller collided with anything,
        // If it did then add a counter force
        if (((m_CollisionFlags & CollisionFlags.Above) == CollisionFlags.Above && !controller.isGrounded) && m_DesiredVelocityLocal.y > 0)
            m_DesiredVelocityLocal.y *= -.05f;

        Vector3 targetVelocity = CalcTargetVelocity(Player.MoveInput.Get());

        if (!IsGrounded)
            UpdateAirborneMovement(deltaTime, targetVelocity, ref m_DesiredVelocityLocal);
        else if (!Player.Jump.Active)
            UpdateGroundedMovement(deltaTime, targetVelocity, ref m_DesiredVelocityLocal);

        UpdateLookInput();
        UpdateMovementAnimations();
        Player.IsGrounded.Set(IsGrounded);
        Player.Velocity.Set(Velocity);

        m_PreviouslyGrounded = IsGrounded;
    }
    #region Sprint
    private bool TryStartSprint()
    {
        if (!m_RunState.Enabled || Player.Stamina.Get() < 15f)
            return false;

        bool wantsToMoveBack = Player.MoveInput.Get().y < 0f;
        bool canChangeState = Player.IsGrounded.Get() && !wantsToMoveBack && !Player.Crouch.Active && !Player.Aim.Active && !Player.Prone.Active;

        if (canChangeState)
            m_CurrentMovementState = m_RunState;

        return canChangeState;
    }

    private void StartSprint()
    {
        playerAnimController.LookLayer.SetLayerAlpha(0.5f);
        playerAnimController.AdsLayer.SetLayerAlpha(0f);
        playerAnimController.LocoLayer.SetReadyWeight(0f);

        Player.MovementState.Set(FPSMovementState.Sprinting);
        Player.ActionState.Set(FPSActionState.None);
    }

    private void StopSprint()
    {
        if (Player.PoseState.Val == FPSPoseState.Crouching)
        {
            return;
        }

        m_CurrentMovementState = null;
        playerAnimController.LookLayer.SetLayerAlpha(1f);
        playerAnimController.AdsLayer.SetLayerAlpha(1f);
        Player.MovementState.Set(FPSMovementState.Walking);
    }
    #endregion

    #region Crouch
    private bool Try_ToggleCrouch(LowerHeightStateModule lowerHeightState)
    {
        if (!m_CrouchState.Enabled)
            return false;

        bool toggledSuccesfully;

        if (!Player.Crouch.Active)
        {
            toggledSuccesfully = Try_ChangeControllerHeight(lowerHeightState);
        }
        else
        {
            toggledSuccesfully = Try_ChangeControllerHeight(null);
        }


        //Stop the prone state if the crouch state is enabled
        if (toggledSuccesfully && Player.Prone.Active)
            Player.Prone.ForceStop();

        return toggledSuccesfully;
    }

    private void Crouch()
    {
        playerAnimController.LookLayer.SetPelvisWeight(0f);

        Player.PoseState.Set(FPSPoseState.Crouching);
        animator.SetBool(Crouching, true);
    }

    private void Standup()
    {
        playerAnimController.LookLayer.SetPelvisWeight(1f);

        Player.PoseState.Set(FPSPoseState.Standing);
        animator.SetBool(Crouching, false);
    }
    #endregion
    private void Lean()
    {
        if (Player.MovementState.Val == FPSMovementState.Sprinting)
            return;

        if (Player.ActionState.Val != FPSActionState.Ready)
        {
            Player.CharAnimData.leanDirection = (int)Player.Lean.Parameter;
            playerAnimController.SlotLayer.PlayMotion(leanMotion);
        }
    }

    private bool Try_Jump()
    {
        // If crouched, stop crouching first
        if (Player.Crouch.Active)
        {
            Player.Crouch.TryStop();
            return false;
        }
        else if (Player.Prone.Active)
        {
            if (!Player.Prone.TryStop())
                Player.Crouch.TryStart();

            return false;
        }

        bool canJump = m_JumpState.Enabled &&
            IsGrounded &&
            !Player.Crouch.Active &&
            Time.time > m_LastLandTime + m_JumpState.JumpTimer;

        if (!canJump)
            return false;

        float jumpSpeed = Mathf.Sqrt(2 * gravity * m_JumpState.JumpHeight);
        m_DesiredVelocityLocal = new Vector3(m_DesiredVelocityLocal.x, jumpSpeed, m_DesiredVelocityLocal.z);

        return true;
    }
    private void UpdateGroundedMovement(float deltaTime, Vector3 targetVelocity, ref Vector3 velocity)
    {
        // Make sure to lower the speed when moving on steep surfaces.
        float surfaceAngle = Vector3.Angle(Vector3.up, SurfaceNormal);
        targetVelocity *= m_CoreMovement.SlopeSpeedMult.Evaluate(surfaceAngle / SlopeLimit);

        // Calculate the rate at which the current speed should increase / decrease. 
        // If the player doesn't press any movement button, use the "m_Damping" value, otherwise use "m_Acceleration".
        float targetAccel = targetVelocity.sqrMagnitude > 0f ? m_CoreMovement.Acceleration : m_CoreMovement.Damping;

        velocity = Vector3.Lerp(velocity, targetVelocity, targetAccel * deltaTime);

        // If we're moving and not running, start the "Walk" activity.
        if (!Player.Walk.Active && targetVelocity.sqrMagnitude > 0.05f && !Player.Sprint.Active && !Player.Crouch.Active)
            Player.Walk.ForceStart();
        // If we're running, or not moving, stop the "Walk" activity.
        else if (Player.Walk.Active && (targetVelocity.sqrMagnitude < 0.05f || Player.Sprint.Active || Player.Crouch.Active || Player.Prone.Active))
            Player.Walk.ForceStop();

        if (Player.Sprint.Active)
        {
            bool wantsToMoveBackwards = Player.MoveInput.Get().y < 0f;
            bool runShouldStop = wantsToMoveBackwards || targetVelocity.sqrMagnitude == 0f || Player.Stamina.Is(0f);

            if (runShouldStop)
                Player.Sprint.ForceStop();
        }

        if (m_SlidingState.Enabled)
        {
            // Sliding...
            if (surfaceAngle > m_SlidingState.SlideTreeshold && Player.MoveInput.Get().sqrMagnitude == 0f)
            {
                Vector3 slideDirection = (SurfaceNormal + Vector3.down);
                m_SlideVelocity += slideDirection * m_SlidingState.SlideSpeed * deltaTime;
            }
            else
                m_SlideVelocity = Vector3.Lerp(m_SlideVelocity, Vector3.zero, deltaTime * 10f);

            velocity += transform.InverseTransformVector(m_SlideVelocity);
        }

        // Advance step
        m_DistMovedSinceLastCycleEnded += m_DesiredVelocityLocal.magnitude * deltaTime;

        // Which step length should be used?
        float targetStepLength = m_CoreMovement.StepLength;

        if (m_CurrentMovementState != null)
            targetStepLength = m_CurrentMovementState.StepLength;

        m_CurrentStepLength = Mathf.MoveTowards(m_CurrentStepLength, targetStepLength, deltaTime);

        // If the step cycle is complete, reset it, and send a notification.
        if (m_DistMovedSinceLastCycleEnded > m_CurrentStepLength)
        {
            m_DistMovedSinceLastCycleEnded -= m_CurrentStepLength;
            Player.MoveCycleEnded.Send();
        }

        Player.MoveCycle.Set(m_DistMovedSinceLastCycleEnded / m_CurrentStepLength);
    }

    private void UpdateAirborneMovement(float deltaTime, Vector3 targetVelocity, ref Vector3 velocity)
    {
        if (m_PreviouslyGrounded && !Player.Jump.Active)
            velocity.y = 0f;

        // Modify the current velocity by taking into account how well we can change direction when not grounded (see "m_AirControl" tooltip).
        velocity += targetVelocity * m_CoreMovement.Acceleration * m_CoreMovement.AirborneControl * deltaTime;

        // Apply gravity.
        velocity.y -= gravity * deltaTime;
    }
    private void UpdateLookInput()
    {
        _freeLook = Input.GetKey(KeyCode.X);

        float deltaMouseX = Player.LookInput.Get().x * sensitivity;
        float deltaMouseY = -Player.LookInput.Get().y * sensitivity;

        if (_freeLook)
        {
            // No input for both controller and animation component. We only want to rotate the camera

            _freeLookInput.x += deltaMouseX;
            _freeLookInput.y += deltaMouseY;

            _freeLookInput.x = Mathf.Clamp(_freeLookInput.x, -freeLookAngle.x, freeLookAngle.x);
            _freeLookInput.y = Mathf.Clamp(_freeLookInput.y, -freeLookAngle.y, freeLookAngle.y);

            return;
        }

        _freeLookInput = FPSAnimLib.ExpDecay(_freeLookInput, Vector2.zero, 15f, Time.deltaTime);

        look.x += deltaMouseX;
        look.y += deltaMouseY;

        look.y = Mathf.Clamp(look.y, -90f, 90f);
        moveRotation *= Quaternion.Euler(0f, deltaMouseX, 0f);
        TurnInPlace();

        float moveWeight = Mathf.Clamp01(Mathf.Abs(_smoothMove.magnitude));
        transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, moveWeight);
        look.x *= 1f - moveWeight;

        Player.CharAnimData.SetAimInput(look);
        Player.CharAnimData.AddDeltaInput(new Vector2(deltaMouseX, Player.CharAnimData.deltaAimInput.y));
    }

    private void UpdateCameraRotation()
    {
        Vector2 finalInput = new Vector2(look.x, look.y);
        (Quaternion, Vector3) cameraTransform =
            (transform.rotation * Quaternion.Euler(finalInput.y, finalInput.x, 0f),
                firstPersonCamera.position);

        cameraHolder.rotation = cameraTransform.Item1;
        cameraHolder.position = cameraTransform.Item2;

        mainCamera.rotation = cameraHolder.rotation * Quaternion.Euler(_freeLookInput.y, _freeLookInput.x, 0f);
    }

    private void TurnInPlace()
    {
        float turnInput = look.x;
        look.x = Mathf.Clamp(look.x, -90f, 90f);
        turnInput -= look.x;

        float sign = Mathf.Sign(look.x);
        if (Mathf.Abs(look.x) > turnInPlaceAngle)
        {
            if (!isTurning)
            {
                turnProgress = 0f;

                animator.ResetTrigger(TurnRight);
                animator.ResetTrigger(TurnLeft);

                animator.SetTrigger(sign > 0f ? TurnRight : TurnLeft);
            }

            isTurning = true;
        }

        transform.rotation *= Quaternion.Euler(0f, turnInput, 0f);

        float lastProgress = turnCurve.Evaluate(turnProgress);
        turnProgress += Time.deltaTime * turnSpeed;
        turnProgress = Mathf.Min(turnProgress, 1f);

        float deltaProgress = turnCurve.Evaluate(turnProgress) - lastProgress;

        look.x -= sign * turnInPlaceAngle * deltaProgress;

        transform.rotation *= Quaternion.Slerp(Quaternion.identity,
            Quaternion.Euler(0f, sign * turnInPlaceAngle, 0f), deltaProgress);

        if (Mathf.Approximately(turnProgress, 1f) && isTurning)
        {
            isTurning = false;
        }
    }

    private Vector3 CalcTargetVelocity(Vector2 moveInput)
    {
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        bool wantsToMove = moveInput.sqrMagnitude > 0f;

        // Calculate the direction (relative to the us), in which the player wants to move.
        Vector3 targetDirection = (wantsToMove ? new Vector3(moveInput.x, 0f, moveInput.y) : m_DesiredVelocityLocal.normalized);

        float desiredSpeed = 0f;

        if (wantsToMove)
        {
            // Set the default speed.
            desiredSpeed = m_CoreMovement.ForwardSpeed;
            // If the player wants to move sideways...
            if (Mathf.Abs(moveInput.x) > 0f)
                desiredSpeed = m_CoreMovement.SideSpeed;

            // If the player wants to move backwards...
            if (moveInput.y < 0f)
                desiredSpeed = m_CoreMovement.BackSpeed;

            // If we're currently running...
            if (Player.Sprint.Active)
            {
                // If the player wants to move forward or sideways, apply the run speed multiplier.
                if (desiredSpeed == m_CoreMovement.ForwardSpeed || desiredSpeed == m_CoreMovement.SideSpeed)
                    desiredSpeed = m_CurrentMovementState.SpeedMultiplier;
            }
            else
            {
                // If we're crouching/pronning...
                if (m_CurrentMovementState != null)
                    desiredSpeed *= m_CurrentMovementState.SpeedMultiplier;
            }
        }

        return targetDirection * (desiredSpeed * Player.MovementSpeedFactor.Val);
    }

    private void UpdateMovementAnimations()
    {
        float moveX = Player.MoveInput.Get().x;
        float moveY = Player.MoveInput.Get().y;

        Vector2 rawInput = new Vector2(moveX, moveY);
        Vector2 normInput = new Vector2(moveX, moveY);
        normInput.Normalize();

        if (Player.MovementState.Val == FPSMovementState.Sprinting)
        {
            normInput.x = rawInput.x = 0f;
            normInput.y = rawInput.y = 2f;
        }

        _smoothMove = FPSAnimLib.ExpDecay(_smoothMove, normInput, moveSmoothing, Time.deltaTime);

        moveX = _smoothMove.x;
        moveY = _smoothMove.y;

        Player.CharAnimData.moveInput = normInput;

        _smoothAnimatorMove.x = FPSAnimLib.ExpDecay(_smoothAnimatorMove.x, rawInput.x, 5f, Time.deltaTime);
        _smoothAnimatorMove.y = FPSAnimLib.ExpDecay(_smoothAnimatorMove.y, rawInput.y, 4f, Time.deltaTime);

        bool moving = Mathf.Approximately(0f, normInput.magnitude);

        animator.SetBool(Moving, !moving);
        animator.SetFloat(MoveX, _smoothAnimatorMove.x);
        animator.SetFloat(MoveY, _smoothAnimatorMove.y);
    }

    private bool Try_ChangeControllerHeight(LowerHeightStateModule lowerHeightState)
    {
        bool canChangeHeight =
            (Time.time > m_NextTimeCanChangeHeight || m_NextTimeCanChangeHeight == 0f) &&
            Player.IsGrounded.Get() &&
            !Player.Sprint.Active;


        if (canChangeHeight)
        {
            float height = (lowerHeightState == null) ? DefaultHeight : lowerHeightState.ControllerHeight;

            //If the "lowerHeightState" height is bigger than the current one check if there's anything over the Player's head
            if (height > controller.height)
            {
                if (DoCollisionCheck(true, Mathf.Abs(height - controller.height)))
                    return false;
            }

            if (lowerHeightState != null)
                m_NextTimeCanChangeHeight = Time.time + lowerHeightState.TransitionDuration;

            SetHeight(height);

            m_CurrentMovementState = lowerHeightState;
        }

        return canChangeHeight;
    }

    private bool DoCollisionCheck(bool checkAbove, float maxDistance)
    {
        Vector3 rayOrigin = transform.position + (checkAbove ? Vector3.up * controller.height : Vector3.zero);
        Vector3 rayDirection = checkAbove ? Vector3.up : Vector3.down;

        return Physics.Raycast(rayOrigin, rayDirection, maxDistance, m_ObstacleCheckMask, QueryTriggerInteraction.Ignore);
    }

    private void SetHeight(float height)
    {
        controller.height = height;
        controller.center = Vector3.up * height * 0.5f;
    }
}
