// Designed by Kinemation, 2023

using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.Attributes
{
    public class TransformRetarget : EditorWindow
    {
        AnimationClip sourceClip;
        AnimationClip targetClip;

        Transform sourceBone;
        Transform sourceRoot;

        Transform targetBone;
        Transform targetRoot;

        [MenuItem("Window/Transform Retarget")]
        public static void ShowWindow()
        {
            GetWindow<TransformRetarget>("Transform Retarget");
        }

        private void OnGUI()
        {
            sourceClip = (AnimationClip) EditorGUILayout.ObjectField("Source Animation Clip", sourceClip,
                typeof(AnimationClip), false);
            targetClip = (AnimationClip) EditorGUILayout.ObjectField("Target Animation Clip", targetClip,
                typeof(AnimationClip), false);

            sourceRoot = (Transform) EditorGUILayout.ObjectField("Source Root Transform", sourceRoot, typeof(Transform),
                true);
            sourceBone = (Transform) EditorGUILayout.ObjectField("Source Transform", sourceBone, typeof(Transform),
                true);

            targetRoot = (Transform) EditorGUILayout.ObjectField("Target Root Transform", targetRoot, typeof(Transform),
                true);
            targetBone = (Transform) EditorGUILayout.ObjectField("Target Transform", targetBone, typeof(Transform),
                true);
            
            if (GUILayout.Button("Retarget Animation"))
            {
                if (sourceClip == null || targetClip == null)
                {
                    Debug.LogError("Transform Retarget: failed. Source or Target clip is null");
                    return;
                }

                if (sourceRoot == null || sourceBone == null)
                {
                    Debug.LogError("Transform Retarget: failed. Source is null");
                    return;
                }

                if (targetRoot == null || targetBone == null)
                {
                    Debug.LogError("Transform Retarget: failed. Target is null");
                    return;
                }
                
                RetargetAnimation();
            }
        }

        private void RetargetAnimation()
        {
            // Get all curve bindings from the source clip
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(sourceClip);
            foreach (var binding in curveBindings)
            {
                // If this curve belongs to the source transform
                if (binding.path.Equals(AnimationUtility.CalculateTransformPath(sourceBone, sourceRoot)))
                {
                    // Create a new binding that points to the target transform instead
                    EditorCurveBinding newBinding = new EditorCurveBinding()
                    {
                        path = AnimationUtility.CalculateTransformPath(targetBone, targetRoot),
                        type = binding.type,
                        propertyName = binding.propertyName
                    };

                    // Copy the curve from the source clip to the target clip
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                    AnimationUtility.SetEditorCurve(targetClip, newBinding, curve);
                }
            }
        }
    }
}