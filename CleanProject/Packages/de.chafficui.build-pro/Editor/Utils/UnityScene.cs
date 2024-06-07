// Based on MIT License Copyright(c) 2024 Filip Slavov, https://github.com/NibbleByte/UnitySceneReference

using System;
using UnityEngine;
using System.Linq;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Chafficui.BuildPro.Editor.Utils
{
	/// <summary>
	/// Keeps reference to a scene asset and tracks it's path, so it can be used in the game runtime.
	///
	/// It's a well known fact that scenes can't be referenced like prefabs etc.
	/// The <see cref="UnityEngine.SceneManagement.SceneManager"/> API works with relative scene paths or names.
	/// Use this class to avoid manually typing and updating scene path strings - it will try to do it for you as best as it can,
	/// including when <b>building the player</b>.
	///
	/// Using <see cref="ISerializationCallbackReceiver" /> was inspired by the <see cref="https://github.com/JohannesMP/unity-scene-reference">unity-scene-reference</see> implementation.
	/// </summary>
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	[Serializable]
	public class UnityScene : ISerializationCallbackReceiver
	{

#if UNITY_EDITOR
		// Reference to the asset used in the editor. Player builds don't know about SceneAsset.
		// Will be used to update the scene path.
		[SerializeField] private SceneAsset _SceneAsset;

#pragma warning disable 0414 // Never used warning - will be used by SerializedProperty.
		// Used to dirtify the data when needed upon displaying in the inspector.
		// Otherwise the user will never get the changes to save (unless he changes any other field of the object / scene).
		[SerializeField] private bool _IsDirty;
#pragma warning restore 0414
#endif

		// Player builds will use the path stored here. Should be updated in the editor or during build.
		// If scene is deleted, path will remain.
		[SerializeField]
		private string _ScenePath = string.Empty;


		/// <summary>
		/// Returns the scene path to be used in the <see cref="UnityEngine.SceneManagement.SceneManager"/> API.
		/// While in the editor, this path will always be up to date (if asset was moved or renamed).
		/// If the referred scene asset was deleted, the path will remain as is.
		/// </summary>
		public string ScenePath
		{
			get {
#if UNITY_EDITOR
				AutoUpdateReference();
#endif

				return _ScenePath;
			}

			set {
				_ScenePath = value;

#if UNITY_EDITOR
				if (string.IsNullOrEmpty(_ScenePath)) {
					_SceneAsset = null;
					return;
				}

				_SceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(_ScenePath);
				if (_SceneAsset == null) {
					Debug.LogError($"Setting {nameof(UnityScene)} to {value}, but no scene could be located there.");
				}
#endif
			}
		}

		/// <summary>
		/// Returns the name of the scene without the extension.
		/// </summary>
		public string SceneName => Path.GetFileNameWithoutExtension(ScenePath);

		public bool IsEmpty => string.IsNullOrEmpty(ScenePath);

		public UnityScene() { }

		public UnityScene(string scenePath)
		{
			ScenePath = scenePath;
		}

		public UnityScene(UnityScene other)
		{
			_ScenePath = other._ScenePath;

#if UNITY_EDITOR
			_SceneAsset = other._SceneAsset;
			_IsDirty = other._IsDirty;

			AutoUpdateReference();
#endif
		}

#if UNITY_EDITOR
		private static bool _ReloadingAssemblies = false;

		static UnityScene()
		{
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
		}

		private static void OnBeforeAssemblyReload()
		{
			_ReloadingAssemblies = true;
		}
#endif

		public UnityScene Clone() => new UnityScene(this);

		public override string ToString()
		{
			return _ScenePath;
		}

		public static string[] ToStringArray(UnityScene[] scenes)
		{
			string[] paths = new string[scenes.Length];
			for (int i = 0; i < scenes.Length; i++) {
				paths[i] = scenes[i].ScenePath;
			}

			return paths;
		}

		[Obsolete("Needed for the editor, don't use it in runtime code!", true)]
		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			// In rare cases this error may be logged when trying to change UnityScene while assembly is reloading:
			// "Objects are trying to be loaded during a domain backup. This is not allowed as it will lead to undefined behaviour!"
			if (_ReloadingAssemblies)
				return;

			AutoUpdateReference();
#endif
		}

		[Obsolete("Needed for the editor, don't use it in runtime code!", true)]
		public void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			// OnAfterDeserialize is called in the deserialization thread so we can't touch Unity API.
			// Wait for the next update frame to do it.
			EditorApplication.update += OnAfterDeserializeHandler;
#endif
		}


#if UNITY_EDITOR
		private void OnAfterDeserializeHandler()
		{
			EditorApplication.update -= OnAfterDeserializeHandler;
			AutoUpdateReference();
		}

		private void AutoUpdateReference()
		{
			if (_SceneAsset == null) {
				if (string.IsNullOrEmpty(_ScenePath))
					return;

				SceneAsset foundAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(_ScenePath);
				if (foundAsset) {
					_SceneAsset = foundAsset;
					_IsDirty = true;

					if (!Application.isPlaying) {
						// NOTE: This doesn't work for scriptable objects, hence the _IsDirty.
						EditorSceneManager.MarkAllScenesDirty();
					}
				}
			} else {
				string foundPath = AssetDatabase.GetAssetPath(_SceneAsset);
				if (string.IsNullOrEmpty(foundPath))
					return;

				if (foundPath != _ScenePath) {
					_ScenePath = foundPath;
					_IsDirty = true;

					if (!Application.isPlaying) {
						// NOTE: This doesn't work for scriptable objects, hence the _IsDirty.
						EditorSceneManager.MarkAllScenesDirty();
					}
				}
			}
		}
#endif
	}





#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(UnityScene))]
	[CanEditMultipleObjects]
	internal class UnityScenePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Ensure the property has the expected fields
			var isDirtyProperty = property.FindPropertyRelative("_IsDirty");
			var sceneAssetProperty = property.FindPropertyRelative("_SceneAsset");
			var scenePathProperty = property.FindPropertyRelative("_ScenePath");

			if (isDirtyProperty == null || sceneAssetProperty == null || scenePathProperty == null)
			{
				Debug.LogError("Properties not found. Ensure the field names in UnityScene match the property drawer.");
				return;
			}

			if (isDirtyProperty.boolValue)
			{
				isDirtyProperty.boolValue = false;
				// This will force change in the property and make it dirty.
				// After the user saves, he'll actually see the changed changes and commit them.
			}

			EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			const float buildSettingsWidth = 20f;
			const float padding = 2f;

			Rect assetPos = position;
			assetPos.width -= buildSettingsWidth + padding;

			Rect buildSettingsPos = position;
			buildSettingsPos.x += position.width - buildSettingsWidth + padding;
			buildSettingsPos.width = buildSettingsWidth;

			bool hadReference = sceneAssetProperty.objectReferenceValue != null;

			EditorGUI.PropertyField(assetPos, sceneAssetProperty, GUIContent.none);

			string guid = string.Empty;
			int indexInSettings = -1;

			if (sceneAssetProperty.objectReferenceValue)
			{
				long localId;
				if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sceneAssetProperty.objectReferenceValue, out guid, out localId))
				{
					indexInSettings = Array.FindIndex(EditorBuildSettings.scenes, s => s.guid.ToString() == guid);
				}
			}
			else if (hadReference)
			{
				scenePathProperty.stringValue = string.Empty;
			}

			GUIContent settingsContent = indexInSettings != -1
				? new GUIContent("-", "Scene is already in the Editor Build Settings. Click here to remove it.")
				: new GUIContent("+", "Scene is missing in the Editor Build Settings. Click here to add it.");

			Color prevBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = indexInSettings != -1 ? Color.red : Color.green;

			GUI.backgroundColor = prevBackgroundColor;

			EditorGUI.EndProperty();
		}
	}
#endif
}