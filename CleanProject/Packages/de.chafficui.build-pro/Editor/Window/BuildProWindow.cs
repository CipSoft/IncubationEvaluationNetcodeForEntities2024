#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Chafficui.BuildPro.Editor
{
    public class BuildProWindow : EditorWindow
    {
        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Build Pro")]
        [MenuItem("File/Build Pro/Build Pro Window")]
        public static void ShowWindow()
        {
            // Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(BuildProWindow), false, "BuildPro");
        }

        private void OnGUI()
        {
            var buildScript = BuildScript.Instance;
            if (buildScript == null)
            {
                EditorGUILayout.HelpBox("Internal Error. No BuildScript found!", MessageType.Error);
                Debug.LogError("Internal Error. No BuildScript found!");
                return;
            }

            EditorGUILayout.LabelField("Build Configurations", EditorStyles.boldLabel);
            foreach (var configuration in buildScript.Configurations)
            {
                if (GUILayout.Button(configuration.Name + " - " + configuration.Target + " " + configuration.Subtarget))
                {
                    BuildScript.BuildConfiguration(configuration);
                }
                GUILayout.Space(5);
            }

            if (GUILayout.Button("Build All Configurations"))
            {
                BuildScript.BuildAllConfigurations();
            }

            if (GUILayout.Button("Create New Configuration"))
            {
                CreateNewConfigWindow.ShowWindow();
            }
        }
    }

    public class CreateNewConfigWindow : EditorWindow
    {
        private BuildConfiguration _Configuration = new BuildConfiguration();

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(CreateNewConfigWindow), false, "Create New Configuration");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create New Configuration", EditorStyles.boldLabel);

            _Configuration.Name = EditorGUILayout.TextField("Name", _Configuration.Name);
            _Configuration.OutputPath = EditorGUILayout.TextField("Output Path", _Configuration.OutputPath);
            _Configuration.Target = (BuildTarget)EditorGUILayout.EnumPopup("Target", _Configuration.Target);
            _Configuration.Subtarget = (StandaloneBuildSubtarget)EditorGUILayout.EnumPopup("Subtarget", _Configuration.Subtarget);
            _Configuration.Options = (BuildOptions)EditorGUILayout.EnumFlagsField("Options", _Configuration.Options);
            _Configuration.AssetBundleManifestPath = EditorGUILayout.TextField("Asset Bundle Manifest Path", _Configuration.AssetBundleManifestPath);
            _Configuration.ScriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", _Configuration.ScriptingBackend);

            if (GUILayout.Button("Save"))
            {
                BuildScript.Instance.Configurations.Add(_Configuration);
                Close();
            }
        }
    }
#endif
}