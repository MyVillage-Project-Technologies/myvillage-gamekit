using UnityEditor;

namespace MyVillage.GameKit.Editor
{
    /// Menu: MyVillage -> Build Mission Bundle
    ///
    /// Wraps Unity's AssetBundle build with the conventions M-UNI expects:
    /// one scene per bundle, platform-specific outputs to builds/<platform>/.
    ///
    /// v1.0 ships the menu entry; full implementation lands during M1.
    public static class BundleBuilder
    {
        [MenuItem("MyVillage/Build Mission Bundle")]
        public static void BuildMissionBundle()
        {
            EditorUtility.DisplayDialog(
                "Build Mission Bundle",
                "Bundle builder lands during M1 of the GameKit rollout. "
                + "For the alpha, use the standard AssetBundle build workflow "
                + "and place outputs under builds/<platform>/ for `myvillage deploy`.",
                "OK");
        }
    }
}
