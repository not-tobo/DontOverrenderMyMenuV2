using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using HarmonyLib;
using VRC;
using Il2CppSystem;
using RubyButtonAPI;

namespace DontOverrenderMyMenuV2
{
    public static class ModInfo
    {
        public const string Name = "DontOverrenderMyMenuV2";
        public const string Author = "Topi#1337 [origanal code by Ben]";
        public const string Version = "0.2.2";
        public const string DownloadLink = "https://github.com/not-tobo/DontOverrenderMyMenuV2";
    }

    public class DontOverrenderMyMenuV2 : MelonMod
    {
        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(StartUiManagerInitIEnumerator());
            MelonLogger.Msg("by Topi#1337 [origanal code by Ben]");
        }

        private IEnumerator StartUiManagerInitIEnumerator()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null)
                yield return null;

            VRCUI();
            VRChat_OnUiManagerInit();
        }

        public void VRCUI()
        {
            MelonLogger.Msg("UI Elements Button Initializing...");

            MelonPreferences.CreateCategory(settingsCategory, "DontOverrenderMyMenuV2");
            MelonPreferences.CreateEntry<int>(settingsCategory, "uiMenuX", 2, "UI Elements Button X");
            MelonPreferences.CreateEntry<int>(settingsCategory, "uiMenuY", 2, "UI Elements Button Y");
            DontOverrenderMyMenuV2.uiMenuX = MelonPreferences.GetEntryValue<int>(settingsCategory, "uiMenuX");
            DontOverrenderMyMenuV2.uiMenuY = MelonPreferences.GetEntryValue<int>(settingsCategory, "uiMenuY");

            MelonLogger.Msg("X: " + DontOverrenderMyMenuV2.uiMenuX + " Y: " + DontOverrenderMyMenuV2.uiMenuY);

            var OverrenderButton = new QMToggleButton("UIElementsMenu", DontOverrenderMyMenuV2.uiMenuX, DontOverrenderMyMenuV2.uiMenuY, "Overrender ON", () =>
            {
                overrenderEnabled = true;
                DontOverrenderMyMenuV2.menuCameraClone.SetActive(DontOverrenderMyMenuV2.overrenderEnabled);
                DontOverrenderMyMenuV2.originalCamera.cullingMask = (DontOverrenderMyMenuV2.overrenderEnabled ? DontOverrenderMyMenuV2.newCullingMask : DontOverrenderMyMenuV2.originalCullingMask);
            }, "Overrender OFF", () =>
            {
                overrenderEnabled = false;
                DontOverrenderMyMenuV2.menuCameraClone.SetActive(DontOverrenderMyMenuV2.overrenderEnabled);
                DontOverrenderMyMenuV2.originalCamera.cullingMask = (DontOverrenderMyMenuV2.overrenderEnabled ? DontOverrenderMyMenuV2.newCullingMask : DontOverrenderMyMenuV2.originalCullingMask);
            }, "Toggle for overrendering the Menu");

            MelonLogger.Msg("UI Elements Button Initialized!");

            return;
        }

        public static void VRChat_OnUiManagerInit()
        {
            MelonLogger.Msg("Overrenderer Initializing...");

            VRCVrCamera field_Private_Static_VRCVrCamera_ = VRCVrCamera.field_Private_Static_VRCVrCamera_0;
            bool flag = !field_Private_Static_VRCVrCamera_;
            if (!flag)
            {
                Camera field_Public_Camera_ = field_Private_Static_VRCVrCamera_.field_Public_Camera_0;
                bool flag2 = !field_Public_Camera_;
                if (!flag2)
                {
                    DontOverrenderMyMenuV2.originalCamera = field_Public_Camera_;
                    DontOverrenderMyMenuV2.originalCullingMask = field_Public_Camera_.cullingMask;
                    field_Public_Camera_.cullingMask = (field_Public_Camera_.cullingMask & ~(1 << LayerMask.NameToLayer("UiMenu")) & ~(1 << LayerMask.NameToLayer("UI")));
                    field_Public_Camera_.cullingMask |= 1 << DontOverrenderMyMenuV2.uiPlayerNameplateLayer;
                    DontOverrenderMyMenuV2.newCullingMask = field_Public_Camera_.cullingMask;
                    DontOverrenderMyMenuV2.menuCameraClone = new GameObject();
                    DontOverrenderMyMenuV2.menuCameraClone.transform.parent = field_Public_Camera_.transform.parent;
                    DontOverrenderMyMenuV2.menuCameraUI = DontOverrenderMyMenuV2.menuCameraClone.AddComponent<Camera>();
                    DontOverrenderMyMenuV2.menuCameraUI.cullingMask = (1 << LayerMask.NameToLayer("UiMenu") | 1 << LayerMask.NameToLayer("UI"));
                    DontOverrenderMyMenuV2.menuCameraUI.clearFlags = (CameraClearFlags)3;
                    DontOverrenderMyMenuV2.uiLayer = LayerMask.NameToLayer("UI");
                    DontOverrenderMyMenuV2.uiMenuLayer = LayerMask.NameToLayer("UiMenu");
                    DontOverrenderMyMenuV2.playerLocalLayer = LayerMask.NameToLayer("PlayerLocal");
                    DontOverrenderMyMenuV2.playerLayer = LayerMask.NameToLayer("Player");
                    GameObject gameObject = GameObject.Find("/UserInterface/MenuContent/Popups/LoadingPopup/3DElements/LoadingInfoPanel");
                    DontOverrenderMyMenuV2.SetLayerRecursively(gameObject.transform, DontOverrenderMyMenuV2.uiMenuLayer, -1);
                    GameObject gameObject2 = GameObject.Find("/_Application/TrackingVolume/PlayerObjects/UserCamera");
                    DontOverrenderMyMenuV2.SetLayerRecursively(gameObject2.transform, DontOverrenderMyMenuV2.playerLocalLayer, DontOverrenderMyMenuV2.uiLayer);
                    DontOverrenderMyMenuV2.SetLayerRecursively(gameObject2.transform, DontOverrenderMyMenuV2.playerLocalLayer, DontOverrenderMyMenuV2.uiMenuLayer);
                    HarmonyLib.Harmony harmonyInstance = new HarmonyLib.Harmony("DontOverrenderMyMenuV2");
                    (from m in typeof(VRCUiBackgroundFade).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                     where m.Name.Contains("Method_Public_Void_Single_Action") && !m.Name.Contains("PDM")
                     select m).ToList<MethodInfo>().ForEach(delegate (MethodInfo m)
                     {
                         harmonyInstance.Patch(m, null, new HarmonyMethod(typeof(DontOverrenderMyMenuV2).GetMethod("OnFade", BindingFlags.Static | BindingFlags.NonPublic)), null);
                     });
                    (from m in typeof(SimpleAvatarPedestal).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                     where m.Name.Contains("Method_Private_Void_GameObject")
                     select m).ToList<MethodInfo>().ForEach(delegate (MethodInfo m)
                     {
                         harmonyInstance.Patch(m, null, new HarmonyMethod(typeof(DontOverrenderMyMenuV2).GetMethod("OnAvatarScale", BindingFlags.Static | BindingFlags.NonPublic)), null);
                     });
                    (from m in typeof(PlayerNameplate).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                     where m.Name.Contains("Method_Public_Void_")
                     select m).ToList<MethodInfo>().ForEach(delegate (MethodInfo m)
                     {
                         harmonyInstance.Patch(m, new HarmonyMethod(typeof(DontOverrenderMyMenuV2).GetMethod("OnRebuild", BindingFlags.Static | BindingFlags.NonPublic)), null, null);
                     });

                    DontOverrenderMyMenuV2.overrenderEnabled = MelonPreferences.GetEntryValue<bool>("DontOverrenderMyMenuV2", "overrenderEnabled");
                    DontOverrenderMyMenuV2.menuCameraClone.SetActive(DontOverrenderMyMenuV2.overrenderEnabled);
                    DontOverrenderMyMenuV2.originalCamera.cullingMask = (DontOverrenderMyMenuV2.overrenderEnabled ? DontOverrenderMyMenuV2.newCullingMask : DontOverrenderMyMenuV2.originalCullingMask);

                    MelonLogger.Msg("Overrenderer Initialized!");
                }
            }
        }

        public override void OnUpdate()
        {
            bool flag = DontOverrenderMyMenuV2.menuCameraClone != null && DontOverrenderMyMenuV2.menuCameraClone.activeSelf;
            if (flag)
            {
                DontOverrenderMyMenuV2.menuCameraClone.transform.localPosition = DontOverrenderMyMenuV2.originalCamera.transform.localPosition;
                bool flag2 = DontOverrenderMyMenuV2.menuCameraUI != null;
                if (flag2)
                {
                    DontOverrenderMyMenuV2.menuCameraUI.nearClipPlane = DontOverrenderMyMenuV2.originalCamera.nearClipPlane;
                    DontOverrenderMyMenuV2.menuCameraUI.farClipPlane = DontOverrenderMyMenuV2.originalCamera.farClipPlane;
                }
            }
        }

        private static void OnRebuild(PlayerNameplate __instance)
        {
            bool flag = DontOverrenderMyMenuV2.overrenderEnabled && __instance != null && __instance.gameObject.layer != DontOverrenderMyMenuV2.uiPlayerNameplateLayer;
            if (flag)
            {
                DontOverrenderMyMenuV2.SetLayerRecursively(__instance.transform.parent.parent.parent, DontOverrenderMyMenuV2.uiPlayerNameplateLayer, DontOverrenderMyMenuV2.uiMenuLayer);
                DontOverrenderMyMenuV2.SetLayerRecursively(__instance.transform.parent.parent.parent, DontOverrenderMyMenuV2.uiPlayerNameplateLayer, DontOverrenderMyMenuV2.uiLayer);
            }
        }

        private static void OnAvatarScale(ref SimpleAvatarPedestal __instance, GameObject __0)
        {
            bool flag = DontOverrenderMyMenuV2.overrenderEnabled && __instance != null && __0 != null;
            if (flag)
            {
                bool flag2 = __0.transform.parent.gameObject.name.Equals("AvatarModel");
                if (flag2)
                {
                    DontOverrenderMyMenuV2.SetLayerRecursively(__0.transform.parent, DontOverrenderMyMenuV2.uiMenuLayer, DontOverrenderMyMenuV2.playerLocalLayer);
                }
                else
                {
                    DontOverrenderMyMenuV2.SetLayerRecursively(__0.transform, DontOverrenderMyMenuV2.uiMenuLayer, DontOverrenderMyMenuV2.playerLocalLayer);
                }
            }
        }
        private static void OnFade()
        {
            bool flag = DontOverrenderMyMenuV2.menuCameraClone != null;
            if (flag)
            {
                DontOverrenderMyMenuV2.menuCameraUI.clearFlags = (CameraClearFlags)3;
                foreach (PostProcessLayer postProcessLayer in DontOverrenderMyMenuV2.menuCameraClone.GetComponents<PostProcessLayer>())
                {
                    UnityEngine.Object.Destroy(postProcessLayer);
                }
            }
        }

        public static void SetLayerRecursively(Transform obj, int newLayer, int match)
        {
            bool flag = obj.gameObject.name.Equals("SelectRegion");
            if (!flag)
            {
                bool flag2 = obj.gameObject.layer == match || match == -1;
                if (flag2)
                {
                    obj.gameObject.layer = newLayer;
                }
                foreach (Il2CppSystem.Object @object in obj)
                {
                    Transform obj2 = @object.Cast<Transform>();
                    DontOverrenderMyMenuV2.SetLayerRecursively(obj2, newLayer, match);
                }
            }
        }

        private static int originalCullingMask;
        private static int newCullingMask;
        private static bool overrenderEnabled;
        private static GameObject menuCameraClone;
        private static Camera menuCameraUI;
        private static Camera originalCamera;
        private static int uiLayer;
        private static int uiMenuLayer;
        private static int playerLocalLayer;
        private static int playerLayer;
        private static int uiPlayerNameplateLayer = 30;
        private static string settingsCategory = "DontOverrenderMyMenuV2";
        private static int uiMenuX;
        private static int uiMenuY;
    }
}
