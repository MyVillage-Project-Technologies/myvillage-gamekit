using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MyVillage.GameKit.Editor
{
    /// Menu: MyVillage → Build Mission Bundle / Build All Platforms
    ///
    /// Builds the project's AssetBundles into builds/&lt;platform&gt;/ following the
    /// layout `myvillage deploy` expects. Scenes under Assets/Scenes/ become
    /// bundles named after their filenames (lowercased, no extension). One
    /// scene = one bundle.
    public static class BundleBuilder
    {
        const string SCENES_DIR = "Assets/Scenes";
        const string BUILDS_ROOT = "builds";

        [MenuItem("MyVillage/Build Mission Bundle %#b")]
        public static void BuildCurrentPlatform()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var platform = TargetToPlatformDir(target);
            if (platform == null)
            {
                EditorUtility.DisplayDialog(
                    "Unsupported platform",
                    $"Current build target is {target}. MyVillage supports iOS, Android, and WebGL. " +
                    "Switch the target in File → Build Settings, or use Build All Platforms.",
                    "OK");
                return;
            }
            BuildForPlatform(target, platform);
            EditorUtility.DisplayDialog(
                "Bundle built",
                $"Wrote AssetBundle(s) to {BUILDS_ROOT}/{platform}/.\n\nDeploy with: myvillage deploy --platform {platform}",
                "OK");
        }

        [MenuItem("MyVillage/Build All Platforms")]
        public static void BuildAllPlatforms()
        {
            if (!EditorUtility.DisplayDialog(
                "Build All Platforms",
                "Build AssetBundles for iOS, Android, and WebGL.\n\n" +
                "Unity will switch the active build target three times, which can take several minutes " +
                "on the first run (asset reimport per platform). Subsequent builds are fast.\n\n" +
                "Continue?",
                "Yes, build all",
                "Cancel"))
            {
                return;
            }

            var startTarget = EditorUserBuildSettings.activeBuildTarget;
            var startGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            try
            {
                BuildForTarget(BuildTarget.iOS, BuildTargetGroup.iOS, "ios");
                BuildForTarget(BuildTarget.Android, BuildTargetGroup.Android, "android");
                BuildForTarget(BuildTarget.WebGL, BuildTargetGroup.WebGL, "webgl");
            }
            finally
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(startGroup, startTarget);
            }

            EditorUtility.DisplayDialog(
                "All platforms built",
                $"Wrote AssetBundles to {BUILDS_ROOT}/ios/, {BUILDS_ROOT}/android/, and {BUILDS_ROOT}/webgl/.\n\n" +
                "Deploy with: myvillage deploy",
                "OK");
        }

        // ── internals ──────────────────────────────────────────────────────

        static void BuildForTarget(BuildTarget target, BuildTargetGroup group, string platformDir)
        {
            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
                EditorUtility.DisplayProgressBar("MyVillage", $"Switching build target to {target}...", 0.1f);
                EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            }
            try
            {
                BuildForPlatform(target, platformDir);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static void BuildForPlatform(BuildTarget target, string platformDir)
        {
            var outputDir = Path.Combine(BUILDS_ROOT, platformDir);
            Directory.CreateDirectory(outputDir);

            var builds = DiscoverSceneBundles();
            if (builds.Count == 0)
            {
                Debug.LogWarning(
                    $"[MyVillage] No scenes found under {SCENES_DIR}/. Add a .unity scene to that folder " +
                    "before building, or use BuildPipeline.BuildAssetBundles directly with a custom asset list.");
                return;
            }

            Debug.Log($"[MyVillage] Building {builds.Count} bundle(s) for {target} → {outputDir}/");
            BuildPipeline.BuildAssetBundles(
                outputDir,
                builds.ToArray(),
                BuildAssetBundleOptions.ChunkBasedCompression,
                target);
        }

        /// Find every .unity file under Assets/Scenes/ and treat it as a single
        /// bundle named after the filename (lowercased). This matches the
        /// "one bundle = one scene" GameKit convention.
        static List<AssetBundleBuild> DiscoverSceneBundles()
        {
            var result = new List<AssetBundleBuild>();
            if (!Directory.Exists(SCENES_DIR)) return result;

            foreach (var path in Directory.GetFiles(SCENES_DIR, "*.unity", SearchOption.AllDirectories))
            {
                var rel = path.Replace('\\', '/');
                var name = Path.GetFileNameWithoutExtension(rel).ToLowerInvariant().Replace(' ', '-');
                result.Add(new AssetBundleBuild
                {
                    assetBundleName = name,
                    assetNames = new[] { rel },
                });
            }
            return result;
        }

        static string TargetToPlatformDir(BuildTarget t)
        {
            switch (t)
            {
                case BuildTarget.iOS: return "ios";
                case BuildTarget.Android: return "android";
                case BuildTarget.WebGL: return "webgl";
                default: return null;
            }
        }
    }
}
