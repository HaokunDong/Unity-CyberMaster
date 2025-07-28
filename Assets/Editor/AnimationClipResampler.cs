using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AnimationClipResampler : EditorWindow
{
    private float targetFrameRate = 30f;
    private bool overwriteOriginal = false;
    private bool includeSubfolders = true;
    private DefaultAsset folderToScan;

    [MenuItem("Tools/Animation Clips重采样")]
    static void ShowWindow()
    {
        GetWindow<AnimationClipResampler>("Animation Clips重采样");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation Clips重采样", EditorStyles.boldLabel);

        targetFrameRate = EditorGUILayout.FloatField("目标帧率", targetFrameRate);
        overwriteOriginal = EditorGUILayout.Toggle("是否覆盖原始clip", overwriteOriginal);
        includeSubfolders = EditorGUILayout.Toggle("检查子文件夹", includeSubfolders);
        folderToScan = (DefaultAsset)EditorGUILayout.ObjectField("目标目录", folderToScan, typeof(DefaultAsset), false);

        if (folderToScan != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(folderToScan);
            EditorGUILayout.LabelField("完整路径", assetPath);
        }

        if (GUILayout.Button("重采样"))
        {
            var clips = new List<AnimationClip>();

            if (folderToScan != null)
            {
                string folderPath = AssetDatabase.GetAssetPath(folderToScan);
                string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folderPath });

                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!includeSubfolders && Path.GetDirectoryName(path) != folderPath)
                        continue;

                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (clip != null)
                        clips.Add(clip);
                }
            }
            else
            {
                foreach (var obj in Selection.objects)
                {
                    if (obj is AnimationClip clip)
                        clips.Add(clip);
                }
            }

            ResampleClips(clips.ToArray(), targetFrameRate, overwriteOriginal);
        }
    }

    private static ObjectReferenceKeyframe[] ResampleSpriteCurve(ObjectReferenceKeyframe[] originalKeys, int frameCount, float fps)
    {
        var newKeys = new List<ObjectReferenceKeyframe>();

        for (int i = 0; i <= frameCount; i++)
        {
            float t = i / fps;

            // 找当前时间之前最后一个sprite key
            ObjectReferenceKeyframe nearest = originalKeys[0];
            foreach (var k in originalKeys)
            {
                if (k.time > t) break;
                nearest = k;
            }

            newKeys.Add(new ObjectReferenceKeyframe { time = t, value = nearest.value });
        }

        return newKeys.ToArray();
    }

    private static void ClampLastKeyToLength(AnimationCurve curve, float targetLength)
    {
        if (curve.length == 0) return;

        Keyframe lastKey = curve[curve.length - 1];
        if (Mathf.Abs(lastKey.time - targetLength) > 0.0001f)
        {
            float value = curve.Evaluate(targetLength);
            curve.AddKey(new Keyframe(targetLength, value));
        }
    }


    private static void ResampleClips(AnimationClip[] clips, float newFrameRate, bool overwrite)
    {
        foreach (var originalClip in clips)
        {
            string path = AssetDatabase.GetAssetPath(originalClip);
            AnimationClip newClip = new AnimationClip();
            newClip.frameRate = newFrameRate;

            float originalLength = originalClip.length;
            int frameCount = Mathf.RoundToInt(originalLength * newFrameRate);
            float fixedLength = frameCount / newFrameRate;

            int totalOriginalCurveKeys = 0;
            int totalNewCurveKeys = 0;
            int totalOriginalSpriteFrames = 0;
            int totalNewSpriteFrames = 0;

            // ---------- 动画曲线 ----------
            var bindings = AnimationUtility.GetCurveBindings(originalClip);
            foreach (var binding in bindings)
            {
                AnimationCurve originalCurve = AnimationUtility.GetEditorCurve(originalClip, binding);
                AnimationCurve newCurve = new AnimationCurve();

                for (int i = 0; i <= frameCount; i++)
                {
                    float t = i / newFrameRate;
                    float value = originalCurve.Evaluate(t);
                    newCurve.AddKey(new Keyframe(t, value));
                }

                // 确保最后关键帧位于 fixedLength
                ClampLastKeyToLength(newCurve, fixedLength);

                totalOriginalCurveKeys += originalCurve.length;
                totalNewCurveKeys += newCurve.length;

                AnimationUtility.SetEditorCurve(newClip, binding, newCurve);
            }

            // ---------- Sprite曲线 ----------
            var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(originalClip);
            foreach (var binding in objectBindings)
            {
                var originalKeys = AnimationUtility.GetObjectReferenceCurve(originalClip, binding);
                var newKeys = ResampleSpriteCurve(originalKeys, frameCount, newFrameRate);

                totalOriginalSpriteFrames += originalKeys.Length;
                totalNewSpriteFrames += newKeys.Length;

                AnimationUtility.SetObjectReferenceCurve(newClip, binding, newKeys);
            }

            // ---------- 动画事件 ----------
            AnimationUtility.SetAnimationEvents(newClip, AnimationUtility.GetAnimationEvents(originalClip));

            // ---------- Clip设置 ----------
            SerializedObject originalSerialized = new SerializedObject(originalClip);
            SerializedObject newSerialized = new SerializedObject(newClip);

            var originalSettings = originalSerialized.FindProperty("m_AnimationClipSettings");
            var newSettings = newSerialized.FindProperty("m_AnimationClipSettings");

            if (originalSettings != null && newSettings != null)
            {
                newSettings.FindPropertyRelative("m_LoopTime").boolValue = originalSettings.FindPropertyRelative("m_LoopTime").boolValue;
                newSettings.FindPropertyRelative("m_LoopBlend").boolValue = originalSettings.FindPropertyRelative("m_LoopBlend").boolValue;
                newSettings.FindPropertyRelative("m_LoopBlendOrientation").boolValue = originalSettings.FindPropertyRelative("m_LoopBlendOrientation").boolValue;
                newSettings.FindPropertyRelative("m_LoopBlendPositionY").boolValue = originalSettings.FindPropertyRelative("m_LoopBlendPositionY").boolValue;
                newSettings.FindPropertyRelative("m_LoopBlendPositionXZ").boolValue = originalSettings.FindPropertyRelative("m_LoopBlendPositionXZ").boolValue;
                newSerialized.ApplyModifiedProperties();
            }

            // ---------- 输出信息 ----------
            Debug.Log(
                $"[重采样] {originalClip.name}" +
                $"  原时长: {originalLength:F3}s ➜ 新时长: {fixedLength:F3}s" +
                $"  曲线键数: {totalOriginalCurveKeys} ➜ {totalNewCurveKeys}" +
                $"  Sprite帧数: {totalOriginalSpriteFrames} ➜ {totalNewSpriteFrames}"
            );

            // ---------- 保存 ----------
            if (overwrite)
            {
                string backupPath = path + "~bak";
                if (!File.Exists(backupPath))
                    File.Copy(path, backupPath);

                EditorUtility.CopySerialized(newClip, originalClip);
                originalClip.name = Path.GetFileNameWithoutExtension(path);
                Debug.Log($"覆盖原始动画：{path}，备份: {backupPath}");
            }
            else
            {
                string newPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + $"_{(int)newFrameRate}fps.anim";
                AssetDatabase.CreateAsset(newClip, newPath);
                Debug.Log($"保存新动画：{newPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
