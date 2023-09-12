using Kinemation.FPSFramework.Runtime.Camera;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;
using Unity.Netcode;
using System.Collections;
using UnityEngine;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class PlayerAnimController : PlayerNetworkComponent
{
    public FPSCameraShake shake;
    private FPSCamera fpsCamera;
    public CoreAnimComponent FpsAnimator => fpsAnimator;

    private RecoilAnimation recoilAnimation;
    private CoreAnimComponent fpsAnimator;

    [HideInInspector] public LookLayer LookLayer;
    [HideInInspector] public AdsLayer AdsLayer;
    [HideInInspector] public SwayLayer SwayLayer;
    [HideInInspector] public LocomotionLayer LocoLayer;
    [HideInInspector] public SlotLayer SlotLayer;

    //public CharAnimStates _charAnimStates = new CharAnimStates();

    protected NetworkVariable<CharAnimData> charAnimData = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    protected NetworkVariable<CharAnimStates> charAnimStates = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private bool _hasActiveAction;

    // Used primarily for function calls from Animation Events
    // Runs once at the beginning of the next update
    protected CoreToolkitLib.PostUpdateDelegate queuedAnimEvents;

    private CharAnimStates _charAnimStates;

    private IEnumerator Start()
    {
        _charAnimStates = new CharAnimStates();

        yield return new WaitForEndOfFrame();

        if (fpsCamera != null)
        fpsAnimator.OnPostAnimUpdate += fpsCamera.UpdateCamera;
    }

    private void Awake()
    {
        recoilAnimation = GetComponentInChildren<RecoilAnimation>();
        fpsAnimator = GetComponentInChildren<CoreAnimComponent>();

        InitAnimController();
    }

    private void Update()
    {
        UpdateAnimController();

        if (IsOwner)
        {
            charAnimData.Value = Player.CharAnimData;
            _charAnimStates.action = (int)Player.ActionState.Val;
            _charAnimStates.pose = (int)Player.PoseState.Val;
            charAnimStates.Value = _charAnimStates;
        }
        else
        {
            Player.CharAnimData = charAnimData.Value;
            _charAnimStates = charAnimStates.Value;

            if(charAnimStates.Value.pose == (int)FPSPoseState.Crouching)
            {
                Player.Crouch.TryStart();
            }
            else
            {
                Player.Crouch.TryStop();
            }

            if(_charAnimStates.action == (int)FPSActionState.Ready)
            {
                Player.Holster.TryStart(true);
            }
            else if(_charAnimStates.action == (int)FPSActionState.Aiming)
            {
                Player.Aim.TryStart(true);
            }
            else if (_charAnimStates.action == (int)FPSActionState.PointAiming)
            {
                Player.PointAim.TryStart(true);
            }
            else
            {
                Player.PointAim.ForceStop();
                Player.Holster.ForceStop();
                Player.Aim.ForceStop();
            }

            Player.MoveInput.Set(Player.CharAnimData.moveInput);
        }

        fpsAnimator.SetCharData(Player.CharAnimData);
    }

    public void SetActionActive(int isActive)
    {
        _hasActiveAction = isActive != 0;
    }

    protected void InitAnimController()
    {
        fpsAnimator = GetComponentInChildren<CoreAnimComponent>();
        fpsAnimator.animGraph.InitPlayableGraph();
        fpsAnimator.InitializeLayers();

        Player.CharAnimData = new CharAnimData();

        fpsCamera = GetComponentInChildren<FPSCamera>();

        LookLayer = GetComponentInChildren<LookLayer>();
        AdsLayer = GetComponentInChildren<AdsLayer>();
        LocoLayer = GetComponentInChildren<LocomotionLayer>();
        SwayLayer = GetComponentInChildren<SwayLayer>();
        SlotLayer = GetComponentInChildren<SlotLayer>();
    }

    // Call this during Update after all the gameplay logic
    protected void UpdateAnimController()
    {
        if (queuedAnimEvents != null)
        {
            queuedAnimEvents.Invoke();
            queuedAnimEvents = null;
        }

        Player.CharAnimData.recoilAnim = new LocRot(recoilAnimation.OutLoc, Quaternion.Euler(recoilAnimation.OutRot));
        fpsAnimator.SetCharData(Player.CharAnimData);
        fpsAnimator.animGraph.UpdateGraph();
        fpsAnimator.UpdateCoreComponent();
    }
    public CoreAnimGraph GetAnimGraph()
    {
        return fpsAnimator.animGraph;
    }

    // Call this to play a Camera shake
    public void PlayCameraShake(FPSCameraShake shake)
    {
        if (fpsCamera != null)
        {
            fpsCamera.PlayShake(shake.shakeInfo);
        }
    }
    public void PlayController(RuntimeAnimatorController controller, AnimSequence motion)
    {
        if (motion == null) return;
        fpsAnimator.animGraph.PlayController(controller, motion.clip, motion.blendTime.blendInTime);
    }

    // Call this to play a static pose on the character upper body
    public void PlayPose(AnimSequence motion)
    {
        if (motion == null) return;
        fpsAnimator.animGraph.PlayPose(motion.clip, motion.blendTime.blendInTime);
    }

    // Call this to play an animation on the character upper body
    public void PlayAnimation(AnimSequence motion)
    {
        if (motion == null) return;
        fpsAnimator.animGraph.PlayAnimation(motion.clip, motion.blendTime, motion.curves.ToArray(), motion.mask);
    }

    public void StopAnimation(float blendTime = 0f)
    {
        fpsAnimator.animGraph.StopAnimation(blendTime);
    }

    protected struct CharAnimStates : INetworkSerializable
    {
        public int action;
        public int pose;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref action);
            serializer.SerializeValue(ref pose);
        }
    }

}

