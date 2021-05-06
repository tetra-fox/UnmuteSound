using Harmony;
using MelonLoader;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnmuteSound
{
    public class BuildInfo
    {
        public const string Name = "UnmuteSound";
        public const string Author = "tetra";
        public const string Version = "1.0.1";
        public const string DownloadLink = "https://github.com/tetra-fox/UnmuteSound/releases/download/1.0.1/UnmuteSound.dll";
    }

    public class Mod : MelonMod
    {
        private static HarmonyInstance _harmony;
        public static AudioSource UnmuteBlop;

        private static bool _wasUnmuted;

        public override void VRChat_OnUiManagerInit()
        {
            MelonLogger.Msg("Patching methods...");
            _harmony = HarmonyInstance.Create(BuildInfo.Name);

            typeof(DefaultTalkController).GetMethods()
                .Where(m => m.Name.StartsWith("Method_Public_Static_Void_Boolean_") && !m.Name.Contains("PDM")).ToList()
                .ForEach(m =>
                {
                    _harmony.Patch(m,
                        prefix: new HarmonyMethod(typeof(Mod).GetMethod("ToggleVoicePrefix",
                            BindingFlags.NonPublic | BindingFlags.Static)));
                    MelonLogger.Msg("Patched " + m.Name);
                });

            MelonLogger.Msg("Creating audio source...");

            // this is the actual name of the audio clip lol
            AudioClip Blop = GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/VoiceDotParent")
                .GetComponent<HudVoiceIndicator>().field_Public_AudioClip_0;

            UnmuteBlop = GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/VoiceDotParent")
                .AddComponent<AudioSource>();

            UnmuteBlop.clip = Blop;
            UnmuteBlop.playOnAwake = false;
            UnmuteBlop.pitch = 1.2f;

            VRCAudioManager audioManager = VRCAudioManager.field_Private_Static_VRCAudioManager_0;
            UnmuteBlop.outputAudioMixerGroup = new[]
            {
                audioManager.field_Public_AudioMixerGroup_0,
                audioManager.field_Public_AudioMixerGroup_1,
                audioManager.field_Public_AudioMixerGroup_2
            }.Single(mg => mg.name == "UI");

            MelonLogger.Msg("Initialized!");
        }

        [HarmonyPrefix]
        private static bool ToggleVoicePrefix(bool __0)
        {
            // __0 true on mute
            if (__0) _wasUnmuted = false;
            if (__0 || _wasUnmuted) return true;
            UnmuteBlop.Play();
            _wasUnmuted = true;
            return true;
        }
    }
}