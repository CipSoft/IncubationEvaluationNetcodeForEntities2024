#if UNITY_EDITOR
using UnityEngine;
using Chafficui.BuildPro.Editor.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;

namespace Chafficui.BuildPro.Editor
{
    public class BuildScript : ScriptableObject
    {
        public static BuildScript Instance { get; private set;}

        public List<BuildConfiguration> Configurations = new List<BuildConfiguration>();

        public BuildScript()
        {
            Instance = this;
        }

        [MenuItem("Build Pro/Build All Configurations")]
        [MenuItem("File/Build Pro/Build All Configurations")]
        public static void BuildAllConfigurations()
        {
            BuildConfigurationCollection(Instance.Configurations.ToArray());
        }

        public static void BuildConfigurationCollection(BuildConfiguration[] collection, bool debug = false)
        {
            foreach (var configuration in collection)
            {
                BuildConfiguration(configuration, debug);
            }
        }

        public static void BuildConfiguration(BuildConfiguration configuration, bool debug = false)
        {
            var buildPath = configuration.OutputPath + "/" + configuration.Name;

            switch (configuration.Target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    buildPath += ".exe";
                    break;
                case BuildTarget.StandaloneLinux64:
                    buildPath += ".x86_64";
                    break;
                default:
                    if (debug)
                    {
                        EditorUtility.DisplayDialog("Build failed", "Unsupported build target", "OK");
                    }
                    Debug.LogError("Unsupported build target");
                    return;
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = UnityScene.ToStringArray(configuration.Scenes),
                locationPathName = buildPath,
                target = configuration.Target,
                targetGroup = BuildTargetGroup.Standalone,
                subtarget = (int)configuration.Subtarget, // Use Server subtarget,
                options = configuration.Options,
                assetBundleManifestPath = configuration.AssetBundleManifestPath,
                extraScriptingDefines = configuration.ExtraScriptingDefines
            };

            // Set scripting backend to IL2CPP
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, configuration.ScriptingBackend);

            // Perform the build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            // Check the build result
            if (summary.result == BuildResult.Succeeded)
            {
                if (debug)
                {
                    EditorUtility.DisplayDialog("Build succeeded", "Build size: " + summary.totalSize + " bytes", "OK");
                }
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                EditorUtility.RevealInFinder(buildPath);
            }

            if (summary.result == BuildResult.Failed)
            {
                if (debug)
                {
                    EditorUtility.DisplayDialog("Build failed", "Build failed", "OK");
                }
                Debug.LogError("Build failed");
            }
        }
    }
}
#endif