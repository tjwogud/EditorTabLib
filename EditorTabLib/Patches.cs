using ADOFAI;
using DG.Tweening;
using EditorTabLib.Components;
using EditorTabLib.Utils;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EditorTabLib
{
    internal static class Patches
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

            public static bool cancelled = true;

            public static bool Prefix(string str, ref LevelEventType __result)
            {
                if (CustomTabManager.byName.TryGetValue(str, out CustomTabManager.CustomTab tab))
                {
                    __result = (LevelEventType)tab.type;
                    cancelled = false;
                    return false;
                }
                return true;
            }

            public static void Postfix(string str, ref LevelEventType __result)
            {
                if (!cancelled)
                {
                    cancelled = true;
                    return;
                }
                if (CustomTabManager.byName.TryGetValue(str, out CustomTabManager.CustomTab tab))
                    __result = (LevelEventType)tab.type;
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
            public static void Postfix()
            {
                CustomTabManager.SortTab();
            }
        }

        [HarmonyPatch(typeof(EditorConstants), "IsSetting")]
        public static class IsSettingPatch
        {
            public static void Postfix(LevelEventType type, ref bool __result)
            {
                if (CustomTabManager.byType.ContainsKey((int)type))
                    __result = true;
            }
        }

        [HarmonyPatch(typeof(scnEditor), "GetSelectedFloorEvents")]
        public static class GetSelectedFloorEventsPatch
        {
            public static void Postfix(LevelEventType eventType, ref List<LevelEvent> __result)
            {
                if (CustomTabManager.byType.ContainsKey((int)eventType) && __result == null)
                    __result = new List<LevelEvent>();
            }
        }

        [HarmonyPatch(typeof(InspectorPanel), "ShowPanel")]
        public static class ShowPanelPatch
        {
            public static readonly Dictionary<LevelEventType, LevelEvent> saves = new Dictionary<LevelEventType, LevelEvent>();
            public static bool cancelled = true;

            public static bool Prefix(InspectorPanel __instance, LevelEventType eventType)
            {
                Postfix(__instance, eventType);
                cancelled = false;
                return !CustomTabManager.byType.ContainsKey((int)eventType);
            }

            public static void Postfix(InspectorPanel __instance, LevelEventType eventType)
            {
                if (!cancelled)
                {
                    cancelled = true;
                    return;
                }
                if (!CustomTabManager.byType.TryGetValue((int)eventType, out CustomTabManager.CustomTab tab))
                    return;
                __instance.Set("showingPanel", true);
                scnEditor.instance.SaveState(true, false);
                scnEditor.instance.changingState++;
                PropertiesPanel propertiesPanel = null;
                foreach (PropertiesPanel propertiesPanel2 in __instance.panelsList)
                    if (propertiesPanel2.levelEventType == eventType)
                        (propertiesPanel = propertiesPanel2).gameObject.SetActive(true);
                    else
                        propertiesPanel2.gameObject.SetActive(false);
                __instance.title.text =
                    tab.title.TryGetValue(RDString.language, out string title) ?
                    title :
                    (tab.title.TryGetValue(SystemLanguage.English, out title) ?
                    title :
                    (tab.title.Values.Count > 0 ?
                    tab.title.Values.ElementAt(0) :
                    tab.name));
                LevelEvent levelEvent = tab.saveSetting && saves.TryGetValue((LevelEventType)tab.type, out LevelEvent e) ? e : new LevelEvent(0, (LevelEventType)tab.type, GCS.settingsInfo[tab.name]);
                if (tab.saveSetting)
                    saves[(LevelEventType)tab.type] = levelEvent;
                if (propertiesPanel == null)
                    goto end;
                if (levelEvent == null)
                    goto end;
                __instance.selectedEvent = levelEvent;
                __instance.selectedEventType = levelEvent.eventType;
                levelEvent.UpdatePanel();
                IEnumerator enumerator2 = __instance.tabs.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    RectTransform rect = (RectTransform)enumerator2.Current;
                    InspectorTab component = rect.gameObject.GetComponent<InspectorTab>();
                    component?.SetSelected(eventType == component.levelEventType);
                }
                goto end;
            end:
                scnEditor.instance.changingState--;
                __instance.Set("showingPanel", false);
            }
        }

        [HarmonyPatch(typeof(PropertiesPanel), "Init")]
        public static class PropertyPanelPatch
        {
            public static void Postfix(PropertiesPanel __instance, LevelEventInfo levelEventInfo)
            {
                if (CustomTabManager.byType.TryGetValue((int)levelEventInfo.type, out CustomTabManager.CustomTab tab))
                {
                    if (tab.page != null)
                    {
                        VerticalLayoutGroup layoutGroup = __instance.content.GetComponent<VerticalLayoutGroup>();
                        ContentSizeFitter sizeFitter = layoutGroup.GetOrAddComponent<ContentSizeFitter>();
                        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        layoutGroup.childControlHeight = false;
                        layoutGroup.childControlWidth = false;
                        __instance.content.gameObject.GetOrAddComponent<ScrollRect>().movementType = ScrollRect.MovementType.Unrestricted;
                        __instance.content.gameObject.AddComponent(tab.page).Set("properties", __instance);
                    } else if (tab.onFocused != null || tab.onUnFocused != null)
                    {
                        DefaultTabBehaviour page = __instance.content.gameObject.AddComponent<DefaultTabBehaviour>();
                        page.onFocused = tab.onFocused;
                        page.onUnFocused = tab.onUnFocused;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InspectorTab), "SetSelected")]
        public static class SetSelectedPatch
        {
            public static void Postfix(InspectorTab __instance, bool selected)
            {
                int type = (int)__instance.levelEventType;
                if (!CustomTabManager.byType.ContainsKey(type))
                    return;
                RectTransform rect = __instance.panel.panelsList.Find(panel => panel.inspectorPanel == __instance.panel).content;
                CustomTabBehaviour behaviour = __instance.panel.panelsList.Find(panel => panel.levelEventType == __instance.levelEventType)?.content?.GetComponent<CustomTabBehaviour>();
                if (selected)
                    behaviour?.OnFocused();
                else if (__instance.icon.color.a == 1)
                    behaviour?.OnUnFocused();
                if (!selected)
                    __instance.eventIndex = 0;
                __instance.cycleButtons.gameObject.SetActive(false);
                RectTransform component = __instance.GetComponent<RectTransform>();
                float num = 0f;
                Vector2 endValue = new Vector2(num, component.sizeDelta.y);
                component.DOKill(false);
                component.DOSizeDelta(endValue, 0.05f, false).SetUpdate(true);
                float num2 = selected ? 0f : 3f;
                num2 -= num / 2f;
                component.DOAnchorPosX(num2, 0.05f, false).SetUpdate(true);
                float alpha = selected ? 0.7f : 0.45f;
                ColorBlock colors = __instance.button.colors;
                colors.normalColor = Color.white.WithAlpha(alpha);
                __instance.button.colors = colors;
                __instance.icon.DOKill(false);
                float alpha2 = selected ? 1f : 0.6f;
                __instance.icon.DOColor(Color.white.WithAlpha(alpha2), 0.05f).SetUpdate(true);
            }
        }

        [HarmonyPatch]
        public static class SetupPatch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(PropertyControl), "Setup");
                yield return AccessTools.Method(typeof(PropertyControl_Toggle), "EnumSetup");
            }

            public static IEnumerator PostfixCo(PropertyControl instance)
            {
                yield return new WaitForEndOfFrame();
                if (instance.propertyInfo.canBeDisabled
                    && instance.propertiesPanel.properties[instance.propertyInfo.name] is Property property
                    && RDString.GetWithCheck($"editor.{property.key}.help", out bool exists) != null
                    && exists
                    && property.helpButton.transform is RectTransform rect)
                {
                    rect.SetAsLastSibling();
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition3D.x - 28, rect.anchoredPosition.y);
                }
            }

            public static void Postfix(PropertyControl __instance)
            {
                StaticCoroutine.Do(PostfixCo(__instance));
                if (!(__instance is PropertyControl_Export instance) || !(__instance.propertyInfo.value_default is UnityAction action))
                    return;
                instance.exportButton.onClick.RemoveAllListeners();
                instance.exportButton.onClick.AddListener(action);
                if (instance.propertyInfo.customLocalizationKey == null)
                {
                    string str = "editor." + instance.propertyInfo.levelEventInfo.name + "." + instance.propertyInfo.name;
                    instance.buttonText.text = RDString.GetWithCheck(str, out bool flag, null);
                    if (!flag)
                        instance.buttonText.text = RDString.GetWithCheck("editor." + instance.propertyInfo.name, out _, null);
                }
                else
                    instance.buttonText.text = (instance.propertyInfo.customLocalizationKey == "") ? "" : RDString.Get(instance.propertyInfo.customLocalizationKey, null);
            }
        }

        [HarmonyPatch(typeof(Property), "info", MethodType.Setter)]
        public static class SetInfoPatch
        {
            public static void Postfix(Property __instance)
            {
                if (__instance.info.type == PropertyType.Export)
                    __instance.label.text = "";
            }
        }

        public static class ValueChangePatches
        {
            // Color: OnEndEdit - string s
            // File: ProcessFile - string newFilename
            // LongText: Add Listener On Setup
            // Rating: SetInt - int var
            // Text: Add Listener On Setup
            // Toggle: SelectVar - string var
            // Vector2: SetVectorVals - string sX, string sY

            [HarmonyPatch]
            public static class ValueChangePatch1
            {
                public static IEnumerable<MethodBase> TargetMethods()
                {
                    yield return AccessTools.Method(typeof(PropertyControl_Color), "OnEndEdit");
                    yield return AccessTools.Method(typeof(PropertyControl_File), "ProcessFile");
                    yield return AccessTools.Method(typeof(PropertyControl_Rating), "SetInt");
                    yield return AccessTools.Method(typeof(PropertyControl_Toggle), "SelectVar");
                    yield return AccessTools.Method(typeof(PropertyControl_Vector2), "SetVectorVals");
                }

                public static void Prefix(PropertyControl __instance, PropertiesPanel ___propertiesPanel, ADOFAI.PropertyInfo ___propertyInfo, ref object __state)
                {
                    if (__instance is PropertyControl_Toggle control && control.settingText)
                        return;
                    ___propertiesPanel.inspectorPanel.selectedEvent.data.TryGetValue(___propertyInfo.name, out __state);
                }

                public static void Postfix(PropertyControl __instance, PropertiesPanel ___propertiesPanel, ADOFAI.PropertyInfo ___propertyInfo, object __state)
                {
                    if (__instance is PropertyControl_Toggle control && control.settingText)
                        return;
                    LevelEvent e = ___propertiesPanel.inspectorPanel.selectedEvent;
                    object newVar = e[___propertyInfo.name];
                    if (CustomTabManager.byType.TryGetValue((int)___propertyInfo.levelEventInfo.type, out CustomTabManager.CustomTab tab)
                        && tab.onChange != null
                        && !tab.onChange.Invoke(e, ___propertyInfo.name, __state, newVar))
                    {
                        e[___propertyInfo.name] = __state;
                        e.UpdatePanel();
                    }
                }
            }

            [HarmonyPatch]
            public static class ValueChangePatch2
            {
                public static IEnumerable<MethodBase> TargetMethods()
                {
                    yield return AccessTools.Method(typeof(PropertyControl_LongText), "Setup");
                    yield return AccessTools.Method(typeof(PropertyControl_Text), "Setup");
                }

                public static void Postfix(PropertyControl __instance, PropertiesPanel ___propertiesPanel, ADOFAI.PropertyInfo ___propertyInfo)
                {
                    if (!CustomTabManager.byType.TryGetValue((int)___propertyInfo.levelEventInfo.type, out CustomTabManager.CustomTab tab)
                        || tab.onChange == null)
                        return;
                    InputField inputField = __instance.Get<InputField>("inputField");
                    object obj = typeof(UnityEventBase).Get("m_Calls", inputField.onEndEdit);
                    object runtime = obj.Get("m_RuntimeCalls");
                    object var = null;
                    string strVar = null;
                    UnityAction<string> prefix = v => { var = ___propertiesPanel.inspectorPanel.selectedEvent[___propertyInfo.name]; strVar = v; };
                    UnityAction<string> postfix = v =>
                    {
                        LevelEvent e = ___propertiesPanel.inspectorPanel.selectedEvent;
                        object newVar = e[___propertyInfo.name];
                        if (!tab.onChange.Invoke(e, ___propertyInfo.name, var, newVar))
                        {
                            e[___propertyInfo.name] = var;
                            e.UpdatePanel();
                        }
                    };
                    object prefixObj = inputField.onEndEdit.Method("GetDelegate", new object[] { prefix });
                    object postfixObj = inputField.onEndEdit.Method("GetDelegate", new object[] { postfix });
                    runtime.Method("Insert", new object[] { 0, prefixObj });
                    runtime.Method("Add", new object[] { postfixObj });
                    obj.Set("m_RuntimeCalls", runtime);
                    obj.Set("m_NeedsUpdate", true);
                }
            }
        }
    }
}
