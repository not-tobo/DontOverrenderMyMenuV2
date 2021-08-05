using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using HarmonyLib;
using VRC;
using RubyButtonAPI;

namespace DontOverrenderMyMenuV2
{
    public static class ModInfo
    {
        public const string Name = "DontOverrenderMyMenuV2";
        public const string Author = "Topi#1337 [origanal code by Ben]";
        public const string Version = "0.2.5";
        public const string DownloadLink = "https://github.com/not-tobo/DontOverrenderMyMenuV2";
    }

    public class DontOverrenderMyMenuV2 : MelonMod
    {
        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(StartUiManagerInitIEnumerator());
            MelonCoroutines.Start(WaitForFirstJoin());
            MelonLogger.Msg(ConsoleColor.DarkMagenta, "by Topi#1337 [origanal code by Ben]");
            MelonLogger.Msg(ConsoleColor.DarkMagenta, "Toggle Hotkey is: ctrl + o [o for overrender]");
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
            MelonLogger.Msg(ConsoleColor.DarkYellow, "UI Elements Button Initializing...");

            MelonPreferences.CreateCategory(settingsCategory, "DontOverrenderMyMenuV2");
            MelonPreferences.CreateEntry<int>(settingsCategory, "uiMenuX", 2, "UI Elements Button X");
            MelonPreferences.CreateEntry<int>(settingsCategory, "uiMenuY", 2, "UI Elements Button Y");
            MelonPreferences.CreateEntry<bool>(settingsCategory, "MenuOverrenderONOFFsaving", MenuOverrenderONOFFsaving, "", null, true);

            MenuOverrenderONOFFsaving = MelonPreferences.GetEntryValue<bool>(settingsCategory, "MenuOverrenderONOFFsaving");
            uiMenuX = MelonPreferences.GetEntryValue<int>(settingsCategory, "uiMenuX");
            uiMenuY = MelonPreferences.GetEntryValue<int>(settingsCategory, "uiMenuY");

            MelonLogger.Msg(ConsoleColor.DarkYellow, "X: " + uiMenuX + " Y: " + uiMenuY);

            var OverrenderButton = new QMToggleButton("UIElementsMenu", uiMenuX, uiMenuY, "Overrender ON", () =>
            {
                MenuOverrenderDoStuffOn();
            }, "Overrender OFF", () =>
            {
                MenuOverrenderDoStuffOff();
            }, "Toggle for overrendering the Menu", null, null, false, MenuOverrenderONOFFsaving ? true : false);

            MelonLogger.Msg(ConsoleColor.Green, "UI Elements Button Initialized!");

            return;
        }

        private IEnumerator WaitForFirstJoin()
        {
            while (RoomManager.field_Internal_Static_ApiWorldInstance_0 == null || RoomManager.field_Internal_Static_ApiWorld_0 == null)
                yield return null;

            if (MenuOverrenderONOFFsaving == true)
            {
                MenuOverrenderDoStuffOn();
            }
            else
            {
                MenuOverrenderDoStuffOff();
            }
        }

        public void MenuOverrenderDoStuffOn()
        {
            MelonLogger.Msg(ConsoleColor.DarkMagenta, "Menu Overrrenderer ON");
            overrenderEnabled = true;
            MelonPreferences.SetEntryValue(settingsCategory, "MenuOverrenderONOFFsaving", true);
            MelonPreferences.Save();
            menuCameraClone.SetActive(overrenderEnabled);
            originalCamera.cullingMask = (overrenderEnabled ? newCullingMask : originalCullingMask);
        }

        public void MenuOverrenderDoStuffOff()
        {
            MelonLogger.Msg(ConsoleColor.DarkMagenta, "Menu Overrrenderer OFF");
            overrenderEnabled = false;
            MelonPreferences.SetEntryValue(settingsCategory, "MenuOverrenderONOFFsaving", false);
            MelonPreferences.Save();
            menuCameraClone.SetActive(overrenderEnabled);
            originalCamera.cullingMask = (overrenderEnabled ? newCullingMask : originalCullingMask);
        }

        public static void VRChat_OnUiManagerInit()
        {
            MelonLogger.Msg(ConsoleColor.DarkYellow, "Overrenderer Initializing...");

            VRCVrCamera field_Private_Static_VRCVrCamera_ = VRCVrCamera.field_Private_Static_VRCVrCamera_0;
            bool flag = !field_Private_Static_VRCVrCamera_;
            if (!flag)
            {
                Camera field_Public_Camera_ = field_Private_Static_VRCVrCamera_.field_Public_Camera_0;
                bool flag2 = !field_Public_Camera_;
                if (!flag2)
                {
                    originalCamera = field_Public_Camera_;
                    originalCullingMask = field_Public_Camera_.cullingMask;
                    field_Public_Camera_.cullingMask = (field_Public_Camera_.cullingMask & ~(1 << LayerMask.NameToLayer("UiMenu")) & ~(1 << LayerMask.NameToLayer("UI")));
                    field_Public_Camera_.cullingMask |= 1 << uiPlayerNameplateLayer;
                    newCullingMask = field_Public_Camera_.cullingMask;
                    menuCameraClone = new GameObject();
                    menuCameraClone.transform.parent = field_Public_Camera_.transform.parent;
                    menuCameraUI = menuCameraClone.AddComponent<Camera>();
                    menuCameraUI.cullingMask = (1 << LayerMask.NameToLayer("UiMenu") | 1 << LayerMask.NameToLayer("UI"));
                    menuCameraUI.clearFlags = (CameraClearFlags)3;
                    uiLayer = LayerMask.NameToLayer("UI");
                    uiMenuLayer = LayerMask.NameToLayer("UiMenu");
                    playerLocalLayer = LayerMask.NameToLayer("PlayerLocal");
                    playerLayer = LayerMask.NameToLayer("Player");
                    GameObject gameObject = GameObject.Find("/UserInterface/MenuContent/Popups/LoadingPopup/3DElements/LoadingInfoPanel");
                    SetLayerRecursively(gameObject.transform, uiMenuLayer, -1);
                    GameObject gameObject2 = GameObject.Find("/_Application/TrackingVolume/PlayerObjects/UserCamera");
                    SetLayerRecursively(gameObject2.transform, playerLocalLayer, uiLayer);
                    SetLayerRecursively(gameObject2.transform, playerLocalLayer, uiMenuLayer);
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

                    overrenderEnabled = MelonPreferences.GetEntryValue<bool>("DontOverrenderMyMenuV2", "overrenderEnabled");
                    menuCameraClone.SetActive(overrenderEnabled);
                    originalCamera.cullingMask = (overrenderEnabled ? newCullingMask : originalCullingMask);

                    MelonLogger.Msg(ConsoleColor.Green, "Overrenderer Initialized!");
                }
            }
        }

        public override void OnUpdate()
        {
            bool flag = menuCameraClone != null && menuCameraClone.activeSelf;
            if (flag)
            {
                menuCameraClone.transform.localPosition = originalCamera.transform.localPosition;
                bool flag2 = menuCameraUI != null;
                if (flag2)
                {
                    menuCameraUI.nearClipPlane = originalCamera.nearClipPlane;
                    menuCameraUI.farClipPlane = originalCamera.farClipPlane;
                }
            }
            KeyUpdate();
        }

        private static void OnRebuild(PlayerNameplate __instance)
        {
            bool flag = overrenderEnabled && __instance != null && __instance.gameObject.layer != uiPlayerNameplateLayer;
            if (flag)
            {
                SetLayerRecursively(__instance.transform.parent.parent.parent, uiPlayerNameplateLayer, uiMenuLayer);
                SetLayerRecursively(__instance.transform.parent.parent.parent, uiPlayerNameplateLayer, uiLayer);
            }
        }

        private static void OnAvatarScale(ref SimpleAvatarPedestal __instance, GameObject __0)
        {
            bool flag = overrenderEnabled && __instance != null && __0 != null;
            if (flag)
            {
                bool flag2 = __0.transform.parent.gameObject.name.Equals("AvatarModel");
                if (flag2)
                {
                    SetLayerRecursively(__0.transform.parent, uiMenuLayer, playerLocalLayer);
                }
                else
                {
                    SetLayerRecursively(__0.transform, uiMenuLayer, playerLocalLayer);
                }
            }
        }

        private static void OnFade()
        {
            bool flag = menuCameraClone != null;
            if (flag)
            {
                menuCameraUI.clearFlags = (CameraClearFlags)3;
                foreach (PostProcessLayer postProcessLayer in menuCameraClone.GetComponents<PostProcessLayer>())
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
                    SetLayerRecursively(obj2, newLayer, match);
                }
            }
        }

        private void KeyUpdate()
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 == null || RoomManager.field_Internal_Static_ApiWorld_0 == null)
                return;

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.O))
            {
                if (overrenderEnabled == false)
                {
                    MenuOverrenderDoStuffOn();
                }
                else
                {
                    MenuOverrenderDoStuffOff();
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
        private static bool MenuOverrenderONOFFsaving;
    }
}
