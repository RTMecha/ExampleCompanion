using System;

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

using UnityEngine;

using LSFunctions;

using ExampleCompanion.Managers;

using RTFunctions.Functions.Managers;

namespace ExampleCompanion
{
    [BepInPlugin("com.mecha.examplecompanion", "ExampleCompanion", "1.0.0")]
    public class ExamplePlugin : BaseUnityPlugin
    {
        public static ExamplePlugin inst;
        public static string className = "[<color=#3AD4F5>ExampleCompanion</color>] " + PluginInfo.PLUGIN_VERSION + "\n";
        readonly Harmony harmony = new Harmony("Example");
        public static ConfigFile ModConfig => inst.Config;

        public static Action<EditorManager> onEditorAwake = delegate (EditorManager x) { };
        public static Action<bool> onEditorToggle = delegate (bool x) { };
        public static Action<string> onSceneLoad = delegate (string x) { };

        public static Action onInit = delegate ()
        {
            var gameObject = new GameObject("ExampleManager");
            gameObject.transform.SetParent(SystemManager.inst.transform);
            gameObject.AddComponent<ExampleManager>();
        };

        #region Configs

        public static ConfigEntry<bool> ExampleSpawns { get; set; }
        public static ConfigEntry<float> ExampleVisibility { get; set; }
        public static ConfigEntry<bool> ExampleVisible { get; set; }

        public static ConfigEntry<KeyCode> ExampleVisiblityToggle { get; set; }

        public static ConfigEntry<bool> EnabledInEditor { get; set; }
        public static ConfigEntry<bool> EnabledInMenus { get; set; }
        public static ConfigEntry<bool> EnabledInArcade { get; set; }

        #endregion

        void Awake()
        {
            ExampleSpawns = Config.Bind("Spawning", "Enabled", true);
            ExampleVisibility = Config.Bind("Visibility", "Amount", 0.5f);
            ExampleVisible = Config.Bind("Visibility", "Enabled", false);
            ExampleVisiblityToggle = Config.Bind("Visibility", "Toggle KeyCode", KeyCode.O);

            EnabledInArcade = Config.Bind("Visibility", "In Arcade", false, "Includes Editor Preview.");
            EnabledInEditor = Config.Bind("Visibility", "In Editor", true);
            EnabledInMenus = Config.Bind("Visibility", "In Menus", false);

            Config.SettingChanged += new EventHandler<SettingChangedEventArgs>(UpdateSettings);

            harmony.PatchAll(typeof(ExamplePlugin));

            if (!ModCompatibility.mods.ContainsKey("ExampleCompanion"))
            {
                var mod = new ModCompatibility.Mod(this, GetType());

                mod.methods.Add("InitExample", GetType().GetMethod("InitExample"));

                ModCompatibility.mods.Add("ExampleCompanion", mod);
            }

            // Plugin startup logic
            Logger.LogInfo($"Plugin ExampleCompanion is loaded!");
        }

        void Update()
        {
            if (Input.GetKeyDown(ExampleVisiblityToggle.Value) && !LSHelpers.IsUsingInputField())
                ExampleVisible.Value = !ExampleVisible.Value;
        }

        public static void InitExample()
        {
            if (!ExampleManager.inst && ExampleSpawns.Value) ExampleManager.Init();
        }

        static void UpdateSettings(object sender, EventArgs e)
        {
            if (!ExampleManager.inst && ExampleSpawns.Value)
                ExampleManager.Init();
        }

        [HarmonyPatch(typeof(EditorManager), "Awake")]
        [HarmonyPostfix]
        static void EditorAwakePostfix(EditorManager __instance) => onEditorAwake(__instance);

        [HarmonyPatch(typeof(EditorManager), "ToggleEditor")]
        [HarmonyPostfix]
        static void ToggleEditorPostfix(EditorManager __instance) => onEditorToggle(__instance.isEditing);

        [HarmonyPatch(typeof(SystemManager), "Awake")]
        [HarmonyPostfix]
        static void SystemAwakePostfix() => InitExample();

        [HarmonyPatch(typeof(SceneManager), "LoadScene", new Type[] { typeof(string) })]
        [HarmonyPostfix]
        static void LoadScenePostfix(string __0) => onSceneLoad(__0);

        public static void LogFormat(string str, params object[] v) => Debug.LogFormat("{0}{1}", className, string.Format(str, v));
    }
}
