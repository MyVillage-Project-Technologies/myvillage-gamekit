using System;
using System.IO;
using System.Linq;
using MyVillage.GameKit;
using MyVillage.GameKit.MockHost;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyVillage.GameKit.Editor
{
    /// Menu: MyVillage -> Create Starter Mission Scene
    ///
    /// Builds a fresh scene with a "Mission" GameObject carrying:
    ///   - MockMissionHost (so Play works in the editor)
    ///   - The dev's MissionBase subclass (auto-discovered if there's
    ///     exactly one; otherwise the dev attaches manually)
    ///   - A Main Camera + AudioListener so the scene renders cleanly
    ///
    /// Why a menu rather than a CLI-generated .unity file: Unity's scene
    /// YAML is involved enough that hand-templating is fragile across
    /// editor versions. EditorSceneManager APIs build a correct scene every
    /// time and stay future-proof.
    public static class StarterSceneCreator
    {
        const string DEFAULT_SCENE_PATH = "Assets/Scenes/MyMission.unity";

        [MenuItem("MyVillage/Create Starter Mission Scene")]
        public static void Create()
        {
            // 1. Ask the dev where to save (with a sensible default).
            var path = EditorUtility.SaveFilePanel(
                "Save Starter Mission Scene",
                "Assets/Scenes",
                "MyMission",
                "unity");
            if (string.IsNullOrEmpty(path)) return;

            // Convert to project-relative for AssetDatabase.
            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
            if (!path.StartsWith(projectRoot))
            {
                EditorUtility.DisplayDialog(
                    "Invalid location",
                    "Save the scene somewhere inside this Unity project's Assets/ folder.",
                    "OK");
                return;
            }
            var relPath = path.Substring(projectRoot.Length + 1);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // 2. Create the scene.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // The default scene has a Main Camera + Directional Light; that's fine.
            // Add our Mission GameObject.
            var missionGo = new GameObject("Mission");
            var mockHost = missionGo.AddComponent<MockMissionHost>();
            Debug.Log($"[StarterSceneCreator] Added MockMissionHost to '{missionGo.name}'.");

            // 3. Try to auto-attach a MissionBase subclass if there's exactly one.
            var missionType = FindUserMissionType();
            if (missionType != null)
            {
                missionGo.AddComponent(missionType);
                Debug.Log($"[StarterSceneCreator] Auto-attached {missionType.Name}. Press Play to run.");
            }
            else
            {
                Debug.Log("[StarterSceneCreator] No unique MissionBase subclass found in your project. " +
                          "Attach your MissionBase script to the 'Mission' GameObject manually, then press Play.");
            }

            // 4. Save and open the scene.
            EditorSceneManager.SaveScene(scene, relPath);
            EditorSceneManager.OpenScene(relPath, OpenSceneMode.Single);

            // 5. Make sure the scene is in the build list so Play works without
            //    Unity nagging about "Scene not in build settings."
            EnsureSceneInBuildList(relPath);

            Selection.activeGameObject = missionGo;
            EditorUtility.DisplayDialog(
                "Starter Mission Scene Created",
                $"Scene saved to: {relPath}\n\n" +
                (missionType != null
                    ? $"{missionType.Name} is attached. Press Play to run."
                    : "Drag your MissionBase script onto the 'Mission' GameObject, then press Play."),
                "OK");
        }

        /// Find the user's MissionBase subclass via reflection. Returns null
        /// if there's zero or more than one (we don't want to guess when the
        /// dev has multiple missions in the project).
        static Type FindUserMissionType()
        {
            var subclasses = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.StartsWith("MyVillage.GameKit")
                            && !a.FullName.StartsWith("Unity.")
                            && !a.FullName.StartsWith("UnityEngine.")
                            && !a.FullName.StartsWith("UnityEditor."))
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
                })
                .Where(t => t != null
                            && !t.IsAbstract
                            && t.IsSubclassOf(typeof(MissionBase)))
                .ToList();

            return subclasses.Count == 1 ? subclasses[0] : null;
        }

        static void EnsureSceneInBuildList(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == scenePath)) return;
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[StarterSceneCreator] Added '{scenePath}' to Build Settings (index 0).");
        }
    }
}
