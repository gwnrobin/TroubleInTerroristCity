using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEngine;

public class FPSPlayerMovement : FPSPlayerComponent
{
    private bool IsGrounded
    { 
        get => PlayerController.Controller.isGrounded;
    }

    private Vector3 Velocity
    {
        get => PlayerController.Controller.velocity;
    }

    public Vector3 SurfaceNormal { get; private set; }

    private float SlopeLimit
    {
        get => PlayerController.Controller.slopeLimit;
    }

    public float DefaultHeight { get; private set; }

    private static readonly int InAir = Animator.StringToHash("InAir");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int VelocityHash = Animator.StringToHash("Velocity");
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int Sliding = Animator.StringToHash("Sliding");
    private static readonly int Sprinting = Animator.StringToHash("Sprinting");
    private static readonly int Proning = Animator.StringToHash("Proning");

    [Header("General")] 
    [SerializeField] private NetworkPlayerAnimController networkPlayerAnimController;
    [SerializeField] private float moveSmoothing = 2f;
    [SerializeField] private LayerMask m_ObstacleCheckMask = ~0;
    [SerializeField] private float gravity;

    [Space] [Header("Dynamic Motions")] 
    [SerializeField] private IKAnimation aimMotionAsset;

    [SerializeField] private IKAnimation leanMotionAsset;
    [SerializeField] private IKAnimation crouchMotionAsset;
    [SerializeField] private IKAnimation unCrouchMotionAsset;
    [SerializeField] private IKAnimation onJumpMotionAsset;
    [SerializeField] private IKAnimation onLandedMotionAsset;

    [Space] [SerializeField] [Group] private CoreMovementModule m_CoreMovement;

    [SerializeField] [Group] private MovementStateModule m_RunState;

    [SerializeField] [Group] private LowerHeightStateModule m_CrouchState;

    [SerializeField] [Group] private LowerHeightStateModule m_ProneState;

    [SerializeField] [Group] private JumpStateModule m_JumpState;

    [SerializeField] [Group] private SlidingStateModule m_SlidingState;
    
    public Vector2 AnimatorVelocity { get; private set; }

    private MovementStateModule m_CurrentMovementState;
    
    private Vector2 _smoothAnimatorMove;
    
    private CollisionFlags _mCollisionFlags;

    private float m_DistMovedSinceLastCycleEnded;
    private float m_CurrentStepLength;

    private Vector3 m_SlideVelocity;
    private Vector3 m_DesiredVelocityLocal;
    private bool m_PreviouslyGrounded;
    private float m_LastLandTime;
    private float m_NextTimeCanChangeHeight;

    private float _sprintAnimatorInterp = 8f;

    private void Start()
    {
        DefaultHeight = PlayerController.Controller.height;

        Player.Slide.SetStartTryer(TrySlide);

        Player.Slide.AddStartListener(StartSlide);

        Player.Crouch.AddStartListener(Crouch);
        Player.Crouch.AddStopListener(Standup);

        Player.Crouch.SetStartTryer(() => { return Try_ToggleCrouch(m_CrouchState); });
        Player.Crouch.SetStopTryer(() => { return Try_ToggleCrouch(null); });

        Player.Prone.AddStartListener(Prone);
        Player.Prone.AddStopListener(CancelProne);

        Player.Prone.SetStartTryer(() => { return Try_ToggleProne(m_ProneState); });
        Player.Prone.SetStopTryer(() => { return Try_ToggleProne(null); });

        Player.Jump.SetStartTryer(Try_Jump);

        Player.Sprint.SetStartTryer(TryStartSprint);
        Player.Sprint.AddStartListener(StartSprint);
        Player.Sprint.AddStopListener(StopSprint);

        Player.Lean.AddStartListener(Lean);
        Player.Lean.AddStopListener(() => Player.CharAnimData.leanDirection = 0);

        Player.DisabledMovement.SetStartTryer(TryDisableMovement);
    }

    private void Update()
    {
        if (Player.DisabledMovement.Active)
            return;

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

        _mCollisionFlags = PlayerController.Controller.Move(translation);

        if ((_mCollisionFlags & CollisionFlags.Below) == CollisionFlags.Below && !m_PreviouslyGrounded)
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
        if (((_mCollisionFlags & CollisionFlags.Above) == CollisionFlags.Above && !PlayerController.Controller.isGrounded) &&
            m_DesiredVelocityLocal.y > 0)
            m_DesiredVelocityLocal.y *= -.05f;

        Vector3 targetVelocity = CalcTargetVelocity(Player.MoveInput.Get());

        if (!IsGrounded)
            UpdateAirborneMovement(deltaTime, targetVelocity, ref m_DesiredVelocityLocal);
        else if (!Player.Jump.Active)
            UpdateGroundedMovement(deltaTime, targetVelocity, ref m_DesiredVelocityLocal);



        UpdateMovementAnimations();
        Player.IsGrounded.Set(IsGrounded);
        Player.Velocity.Set(Velocity);

        m_PreviouslyGrounded = IsGrounded;
    }

    private bool TrySlide()
    {
        if (Player.Sprint.Active)
        {
            return true;
        }

        return false;
    }

    private void StartSlide()
    {
        PlayerController.Animator.CrossFade(Sliding, 0.1f);
        Player.Slide.ForceStop();
    }

    private bool TryDisableMovement()
    {
        return true;
    }

    #region Sprint

    private bool TryStartSprint()
    {
        if (!m_RunState.Enabled || Player.Stamina.Get() < 15f)
            return false;

        bool wantsToMoveBack = Player.MoveInput.Get().y < 0f;
        bool canChangeState = Player.IsGrounded.Get() && !wantsToMoveBack && !Player.Crouch.Active &&
                              !Player.Aim.Active && !Player.Prone.Active;

        if (canChangeState)
            m_CurrentMovementState = m_RunState;

        return canChangeState;
    }

    private void StartSprint()
    {
        networkPlayerAnimController.LookLayer.SetLayerAlpha(0.5f);
        networkPlayerAnimController.AdsLayer.SetLayerAlpha(0f);
        networkPlayerAnimController.LocoLayer.SetReadyWeight(0f);
    }

    private void StopSprint()
    {
        if (Player.Crouch.Active)
        {
            return;
        }

        m_CurrentMovementState = null;
        networkPlayerAnimController.LookLayer.SetLayerAlpha(1f);
        networkPlayerAnimController.AdsLayer.SetLayerAlpha(1f);
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
        networkPlayerAnimController.LookLayer.SetPelvisWeight(0f);
        PlayerController.Animator.SetBool(Crouching, true);
        networkPlayerAnimController.SlotLayer.PlayMotion(crouchMotionAsset);
    }

    private void Standup()
    {
        networkPlayerAnimController.LookLayer.SetPelvisWeight(1f);
        PlayerController.Animator.SetBool(Crouching, false);
        networkPlayerAnimController.SlotLayer.PlayMotion(unCrouchMotionAsset);
    }

    #endregion

    private bool Try_ToggleProne(LowerHeightStateModule lowerHeightState)
    {
        if (!m_ProneState.Enabled)
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

    private void Prone()
    {
        networkPlayerAnimController.LookLayer.SetPelvisWeight(1f);
        PlayerController.Animator.SetBool(Crouching, false);
        PlayerController.Animator.SetBool(Proning, true);
        networkPlayerAnimController.SlotLayer.PlayMotion(unCrouchMotionAsset);
    }

    private void CancelProne()
    {
        networkPlayerAnimController.LookLayer.SetPelvisWeight(0f);
        PlayerController.Animator.SetBool(Crouching, true);
        PlayerController.Animator.SetBool(Proning, false);
        networkPlayerAnimController.SlotLayer.PlayMotion(unCrouchMotionAsset);
    }

    private void Lean()
    {
        if (Player.Sprint.Active)
            return;

        if (!Player.Holster.Active)
        {
            Player.CharAnimData.leanDirection = (int)Player.Lean.Parameter;
            networkPlayerAnimController.SlotLayer.PlayMotion(leanMotionAsset);
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

        if (Player.Prone.Active)
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

    // ReSharper disable Unity.PerformanceAnalysis
    private void UpdateGroundedMovement(float deltaTime, Vector3 targetVelocity, ref Vector3 velocity)
    {
        AdjustSpeedOnSteepSurfaces(targetVelocity);
        UpdateVelocity(deltaTime, targetVelocity, ref velocity);
        UpdateWalkActivity(targetVelocity);
        CheckAndStopSprint(targetVelocity);
        HandleSliding(targetVelocity, deltaTime, ref velocity);
        AdvanceStepCycle(deltaTime);
    }

    private void AdjustSpeedOnSteepSurfaces(Vector3 targetVelocity)
    {
        float surfaceAngle = Vector3.Angle(Vector3.up, SurfaceNormal);
        targetVelocity *= m_CoreMovement.SlopeSpeedMult.Evaluate(surfaceAngle / SlopeLimit);
    }

    private void UpdateVelocity(float deltaTime, Vector3 targetVelocity, ref Vector3 velocity)
    {
        float targetAccel = (targetVelocity.sqrMagnitude > 0f) ? m_CoreMovement.Acceleration : m_CoreMovement.Damping;
        velocity = Vector3.Lerp(velocity, targetVelocity, targetAccel * deltaTime);
    }

    private void UpdateWalkActivity(Vector3 targetVelocity)
    {
        bool wantsToMove = targetVelocity.sqrMagnitude > 0.05f && !Player.Sprint.Active && !Player.Crouch.Active;

        if (!Player.Walk.Active && wantsToMove)
            Player.Walk.ForceStart();
        else if (Player.Walk.Active &&
                 (!wantsToMove || Player.Sprint.Active || Player.Crouch.Active || Player.Prone.Active))
            Player.Walk.ForceStop();
    }

    private void CheckAndStopSprint(Vector3 targetVelocity)
    {
        if (Player.Sprint.Active)
        {
            bool wantsToMoveBackwards = Player.MoveInput.Get().y < 0f;
            bool runShouldStop = wantsToMoveBackwards || targetVelocity.sqrMagnitude == 0f || Player.Stamina.Is(0f);

            if (runShouldStop)
                Player.Sprint.ForceStop();
        }
    }

    private void HandleSliding(Vector3 targetVelocity, float deltaTime, ref Vector3 velocity)
    {
        if (m_SlidingState.Enabled)
        {
            float surfaceAngle = Vector3.Angle(Vector3.up, SurfaceNormal);

            if (surfaceAngle > m_SlidingState.SlideTreeshold && Player.MoveInput.Get().sqrMagnitude == 0f)
            {
                Vector3 slideDirection = (SurfaceNormal + Vector3.down);
                m_SlideVelocity += slideDirection * (m_SlidingState.SlideSpeed * deltaTime);
            }
            else
            {
                m_SlideVelocity = Vector3.Lerp(m_SlideVelocity, Vector3.zero, deltaTime * 10f);
            }

            velocity += transform.InverseTransformVector(m_SlideVelocity);
        }
    }

    private void AdvanceStepCycle(float deltaTime)
    {
        m_DistMovedSinceLastCycleEnded += m_DesiredVelocityLocal.magnitude * deltaTime;

        float targetStepLength = (m_CurrentMovementState != null)
            ? m_CurrentMovementState.StepLength
            : m_CoreMovement.StepLength;
        m_CurrentStepLength = Mathf.MoveTowards(m_CurrentStepLength, targetStepLength, deltaTime);

        if (m_DistMovedSinceLastCycleEnded > m_CurrentStepLength)
        {
            m_DistMovedSinceLastCycleEnded -= m_CurrentStepLength;
            Player.MoveCycleEnded.Send();
        }

        Player.MoveCycle.Set(m_DistMovedSinceLastCycleEnded / m_CurrentStepLength);
    }

    private void UpdateAirborneMovement(float deltaTime, Vector3 targetVelocity, ref Vector3 velocity)
    {
        AdjustVelocityForJump(deltaTime, ref velocity);
        ApplyAirborneControl(targetVelocity, deltaTime, ref velocity);
        ApplyGravity(deltaTime, ref velocity);
        PlayMotionBasedOnGroundedState();
    }

    private void AdjustVelocityForJump(float deltaTime, ref Vector3 velocity)
    {
        if (m_PreviouslyGrounded && !Player.Jump.Active)
            velocity.y = 0f;
    }

    private void ApplyAirborneControl(Vector3 targetVelocity, float deltaTime, ref Vector3 velocity)
    {
        velocity += targetVelocity * (m_CoreMovement.Acceleration * m_CoreMovement.AirborneControl * deltaTime);
    }

    private void ApplyGravity(float deltaTime, ref Vector3 velocity)
    {
        velocity.y -= gravity * deltaTime;
    }

    private void PlayMotionBasedOnGroundedState()
    {
        networkPlayerAnimController.SlotLayer.PlayMotion(!IsGrounded ? onJumpMotionAsset : onLandedMotionAsset);
    }

    private Vector3 CalcTargetVelocity(Vector2 moveInput)
    {
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        bool wantsToMove = moveInput.sqrMagnitude > 0f;

        // Calculate the direction (relative to the us), in which the player wants to move.
        Vector3 targetDirection =
            (wantsToMove ? new Vector3(moveInput.x, 0f, moveInput.y) : m_DesiredVelocityLocal.normalized);

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

        var animatorVelocity = normInput;

        animatorVelocity *= Player.IsGrounded.Get() ? 1f : 0f;

        AnimatorVelocity = Vector2.Lerp(AnimatorVelocity, animatorVelocity,
            FPSAnimLib.ExpDecayAlpha(2, Time.deltaTime));

        if (Player.Sprint.Active)
        {
            normInput.x = rawInput.x = 0f;
            normInput.y = rawInput.y = 2f;
        }

        PlayerController._smoothMove = FPSAnimLib.ExpDecay(PlayerController._smoothMove, normInput, moveSmoothing, Time.deltaTime);

        moveX = PlayerController._smoothMove.x;
        moveY = PlayerController._smoothMove.y;

        Player.CharAnimData.moveInput = normInput;

        bool moving = Mathf.Approximately(0f, normInput.magnitude);

        PlayerController.Animator.SetBool(Moving, !moving);
        PlayerController.Animator.SetFloat(MoveX, AnimatorVelocity.x);
        PlayerController.Animator.SetFloat(MoveY, AnimatorVelocity.y);
        PlayerController.Animator.SetFloat(VelocityHash, AnimatorVelocity.magnitude);

        float a = PlayerController.Animator.GetFloat(Sprinting);
        float b = Player.Sprint.Active ? 1f : 0f;

        a = Mathf.Lerp(a, b, FPSAnimLib.ExpDecayAlpha(_sprintAnimatorInterp, Time.deltaTime));
        PlayerController.Animator.SetFloat(Sprinting, a);
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
            if (height > PlayerController.Controller.height)
            {
                if (DoCollisionCheck(true, Mathf.Abs(height - PlayerController.Controller.height)))
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
        Vector3 rayOrigin = transform.position + (checkAbove ? Vector3.up * PlayerController.Controller.height : Vector3.zero);
        Vector3 rayDirection = checkAbove ? Vector3.up : Vector3.down;

        return Physics.Raycast(rayOrigin, rayDirection, maxDistance, m_ObstacleCheckMask,
            QueryTriggerInteraction.Ignore);
    }

    private void SetHeight(float height)
    {
        PlayerController.Controller.height = height;
        PlayerController.Controller.center = Vector3.up * height * 0.5f;
    }
}