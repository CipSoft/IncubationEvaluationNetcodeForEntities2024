using Chafficui.BuildPro.Editor.Utils;
using System;
using UnityEditor;

namespace Chafficui.BuildPro.Editor
{
#if UNITY_EDITOR
    [Serializable]
    public struct BuildConfiguration
    {
        public string Name;
        [FolderPath]
        public string OutputPath;
        public UnityScene[] Scenes;
        public BuildTarget Target;
        //public BuildTargetGroup TargetGroup;
        public StandaloneBuildSubtarget Subtarget;
        public BuildOptions Options;
        public string[] ExtraScriptingDefines;
        [FolderPath]
        public string AssetBundleManifestPath;
        public ScriptingImplementation ScriptingBackend;
    }
#endif
}