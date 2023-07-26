using Kinemation.FPSFramework.Runtime.Camera;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : PlayerComponent
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

    private bool _hasActiveAction;

    // Used primarily for function calls from Animation Events
    // Runs once at the beginning of the next update
    protected CoreToolkitLib.PostUpdateDelegate queuedAnimEvents;

    private IEnumerator Start()
    {
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
}
