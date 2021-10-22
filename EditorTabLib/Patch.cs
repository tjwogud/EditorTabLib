using ADOFAI;
using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static EditorTabLib.CustomTabManager;

namespace EditorTabLib
{
    public static class Patch
    {
        [HarmonyPatch]
        public static class ParseEnumPatch
        {
            public static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(RDUtils), "ParseEnum", null, null).MakeGenericMethod(new Type[]
                {
                typeof(LevelEventType)
                });
            }

            public static bool Prefix(string str, ref LevelEventType __result)
            {
                if (CustomTabManager.byName.TryGetValue(str, out CustomTab tab))
                {
                    __result = (LevelEventType)tab.type;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(scnEditor), "Awake")]
        public static class AwakePatch
        {
            public static bool Prefix()
            {
                Main.AddOrDeleteAllTabs(true);
                return true;
            }
        }

        [HarmonyPatch(typeof(EditorConstants), "IsSetting")]
        public static class IsSettingPatch
        {
            public static bool Prefix(LevelEventType type, ref bool __result)
            {
                if (CustomTabManager.byType.ContainsKey((int)type))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InspectorPanel), "ShowPanel")]
        public static class ShowPanelPatch
        {
            public static bool Prefix(InspectorPanel __instance, LevelEventType eventType, int eventIndex = 0)
            {
                if (!CustomTabManager.byType.TryGetValue((int)eventType, out CustomTab tab))
                    return true;
                __instance.Set("showingPanel", true);
                __instance.editor.SaveState(true, false);
                __instance.editor.changingState++;
                PropertiesPanel propertiesPanel = null;
                foreach (PropertiesPanel propertiesPanel2 in __instance.panelsList)
                {
                    if (propertiesPanel2.levelEventType == eventType)
                    {
                        propertiesPanel2.gameObject.SetActive(true);
                        propertiesPanel = propertiesPanel2;
                    }
                    else
                    {
                        propertiesPanel2.gameObject.SetActive(false);
                    }
                }
                __instance.title.text = tab.title;
                LevelEvent levelEvent = new LevelEvent(0, (LevelEventType)tab.type);
                int num = 1;

                if (propertiesPanel == null)
                {
                    goto IL_269;
                }
                if (levelEvent == null)
                {
                    goto IL_269;
                }
                __instance.selectedEvent = levelEvent;
                __instance.selectedEventType = levelEvent.eventType;
                propertiesPanel.SetProperties(levelEvent, true);
                IEnumerator enumerator2 = __instance.tabs.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    RectTransform rect = (RectTransform)enumerator2.Current;
                    InspectorTab component = rect.gameObject.GetComponent<InspectorTab>();
                    if (!(component == null))
                    {
                        if (eventType == component.levelEventType)
                        {
                            component.SetSelected(true);
                            component.eventIndex = eventIndex;
                            if (component.cycleButtons != null)
                            {
                                component.cycleButtons.text.text = string.Format("{0}/{1}", eventIndex + 1, num);
                            }
                        }
                        else
                        {
                            component.SetSelected(false);
                        }
                    }
                }
                goto IL_269;
            IL_269:
                __instance.editor.changingState--;
                __instance.Set("showingPanel", false);
                return false;
            }
        }

        [HarmonyPatch(typeof(PropertiesPanel), "Init")]
        public static class PropertyPanelPatch
        {
            public static bool Prefix(PropertiesPanel __instance, InspectorPanel panel, LevelEventInfo levelEventInfo)
            {
                if (CustomTabManager.byType.TryGetValue((int)levelEventInfo.type, out CustomTab tab))
                {
                    __instance.inspectorPanel = panel;
                    VerticalLayoutGroup layoutGroup = __instance.content.GetComponent<VerticalLayoutGroup>();
                    ContentSizeFitter sizeFitter = layoutGroup.GetOrAddComponent<ContentSizeFitter>();
                    sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    layoutGroup.childControlHeight = false;
                    layoutGroup.childControlWidth = false;
                    __instance.content.gameObject.GetOrAddComponent<ScrollRect>().movementType = ScrollRect.MovementType.Unrestricted;
                    AccessTools.Method(typeof(GameObject), "AddComponent", null, new Type[] { tab.page }).Invoke(__instance.content.gameObject, null);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InspectorTab), "SetSelected")]
        public static class SetSelectedPatch
        {
            public static bool Prefix(InspectorTab __instance, bool selected)
            {
                if (scnEditor.instance == null)
                {
                    Main.Logger.Log("editor null");
                    return true;
                }
                int type = (int)__instance.levelEventType;
                if (!CustomTabManager.byType.ContainsKey(type))
                    return true;
                if (!selected)
                {
                    __instance.eventIndex = 0;
                }
                __instance.cycleButtons.gameObject.SetActive(false);
                RectTransform component = __instance.GetComponent<RectTransform>();
                float num = 0f;
                Vector2 endValue = new Vector2(num, component.sizeDelta.y);
                component.DOKill(false);
                component.DOSizeDelta(endValue, 0.05f, false).SetUpdate(true);
                float num2 = selected ? 0f : 3f;
                num2 *= (__instance.Get<bool>("isEventTab") ? -1f : 1f);
                num2 -= num / 2f;
                component.DOAnchorPosX(num2, 0.05f, false).SetUpdate(true);
                float alpha = selected ? 0.7f : 0.45f;
                ColorBlock colors = __instance.button.colors;
                colors.normalColor = Color.white.WithAlpha(alpha);
                __instance.button.colors = colors;
                __instance.icon.DOKill(false);
                float alpha2 = selected ? 1f : 0.6f;
                __instance.icon.DOColor(Color.white.WithAlpha(alpha2), 0.05f).SetUpdate(true);
                return false;
            }
        }
    }
}
