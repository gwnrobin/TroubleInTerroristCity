// Designed by KINEMATION, 2023

using UnityEngine;

[CreateAssetMenu(fileName = "NewRecoilPattern", menuName = "FPS Animator Demo/Recoil Pattern")]
public class RecoilPattern : ScriptableObject
{
    [Min(0f)] public float smoothing;
    public float acceleration;
    public float step;
    public Vector2 horizontalVariation = Vector3.zero;
    [Range(0f, 1f)] public float aimRatio;
    [Range(0f, 1f)] public float cameraWeight;
    [Min(0f)] public float cameraRestoreSpeed;
}