using ADOFAI;
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
        // string을 LevelEventType로 변환할 시 커스텀 탭의 LevelEventType도 리턴
        [HarmonyPatch]
        internal static class RDUtilsParseEnumPatch
        {
            internal static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(RDUtils), "ParseEnum", null, null).MakeGenericMethod(new Type[]
                {
                typeof(LevelEventType)
                });
            }

            internal static bool cancelled = true;

            internal static bool Prefix(string str, ref LevelEventType __result)
            {
                if (CustomTabManager.byName.TryGetValue(str, out CustomTabManager.CustomTab tab))
                {
                    __result = (LevelEventType)tab.type;
                    cancelled = false;
                    return false;
                }
                return true;
            }

            internal static void Postfix(string str, ref LevelEventType __result)
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

        // 에디터에 들어갈 시 모든 탭 추가 후 정렬
        [HarmonyPatch(typeof(scnEditor), "Awake")]
        internal static class scnEditorAwakePatch
        {
            internal static bool Prefix()
            {
                Main.AddOrDeleteAllTabs(true);
                return true;
            }
            internal static void Postfix()
            {
                CustomTabManager.SortTab();
            }
        }

        // 커스텀 탭의 LevelEventType이 설정 탭으로 인식되도록 함
        [HarmonyPatch(typeof(EditorConstants), "IsSetting")]
        internal static class EditorConstantsIsSettingPatch
        {
            internal static void Postfix(LevelEventType type, ref bool __result)
            {
                if (CustomTabManager.byType.ContainsKey((int)type))
                    __result = true;
            }
        }

        // NullReferenceException 방지
        [HarmonyPatch(typeof(scnEditor), "GetSelectedFloorEvents")]
        internal static class scnEditorGetSelectedFloorEventsPatch
        {
            internal static bool Prefix(LevelEventType eventType, ref List<LevelEvent> __result)
            {
                if (scnEditor.instance.selectedFloors == null || scnEditor.instance.selectedFloors.Count == 0 || (CustomTabManager.byType.ContainsKey((int)eventType) && __result == null)) {
                    __result = new List<LevelEvent>();
                    return false;
                }
                return true;
            }
        }

        // 패널을 표시할 시 여러 설정
        [HarmonyPatch(typeof(InspectorPanel), "ShowPanel")]
        internal static class InspectorPanelShowPanelPatch
        {
            internal static readonly Dictionary<LevelEventType, LevelEvent> saves = new Dictionary<LevelEventType, LevelEvent>();
            internal static bool cancelled = true;

            internal static bool Prefix(InspectorPanel __instance, LevelEventType eventType)
            {
                Postfix(__instance, eventType);
                cancelled = false;
                return !CustomTabManager.byType.ContainsKey((int)eventType);
            }

            internal static void Postfix(InspectorPanel __instance, LevelEventType eventType)
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

        // 도움말 버튼과 on/off 버튼이 겹치는 현상 해결
        // 탭 내용 추가
        [HarmonyPatch(typeof(PropertiesPanel), "Init")]
        internal static class PropertyPanelInitPatch
        {
            internal static void Prefix(out bool __state)
            {
                __state = SteamIntegration.Instance.initialized;
                SteamIntegration.Instance.initialized = true;
            }

            internal static void Postfix(PropertiesPanel __instance, LevelEventInfo levelEventInfo, bool __state)
            {
                SteamIntegration.Instance.initialized = __state;
                __instance.properties.ToList().ForEach(pair =>
                {
                    if (pair.Value.info.canBeDisabled
                        && RDString.GetWithCheck($"editor.{pair.Key}.help", out bool exists) != null
                        && exists
                        && pair.Value.helpButton.transform is RectTransform rect)
                    {
                        rect.SetAsLastSibling();
                        rect.anchoredPosition = new Vector2(rect.anchoredPosition3D.x - 28, rect.anchoredPosition.y);
                    }
                });
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

        // OnFocused과 OnUnFocused 호출
        [HarmonyPatch(typeof(InspectorTab), "SetSelected")]
        internal static class InspectorTabSetSelectedPatch
        {
            internal static void Postfix(InspectorTab __instance, bool selected)
            {
                int type = (int)__instance.levelEventType;
                if (!CustomTabManager.byType.ContainsKey(type))
                    return;
                CustomTabBehaviour behaviour = __instance.panel.panelsList.Find(panel => panel.levelEventType == __instance.levelEventType)?.content?.GetComponent<CustomTabBehaviour>();
                if (selected)
                    behaviour?.OnFocused();
                else if (__instance.icon.color.a == 1)
                    behaviour?.OnUnFocused();
            }
        }

        // 커스텀 탭의 버튼에 리스너 추가, 버튼 텍스트 설정
        [HarmonyPatch(typeof(PropertyControl), "Setup")]
        internal static class PropertyControl_ExportSetupPatch
        {
            internal static void Postfix(PropertyControl_Export __instance)
            {
                if (!(__instance.propertyInfo.value_default is UnityAction action))
                    return;
                __instance.exportButton.onClick.RemoveAllListeners();
                __instance.exportButton.onClick.AddListener(action);
                if (__instance.propertyInfo.customLocalizationKey == null)
                {
                    string str = "editor." + __instance.propertyInfo.levelEventInfo.name + "." + __instance.propertyInfo.name;
                    __instance.buttonText.text = RDString.GetWithCheck(str, out bool flag, null);
                    if (!flag)
                        __instance.buttonText.text = RDString.GetWithCheck("editor." + __instance.propertyInfo.name, out _, null);
                }
                else
                    __instance.buttonText.text = (__instance.propertyInfo.customLocalizationKey == "") ? "" : RDString.Get(__instance.propertyInfo.customLocalizationKey, null);
            }
        }

        // 버튼의 설명 텍스트 제거
        [HarmonyPatch(typeof(Property), "info", MethodType.Setter)]
        internal static class Propertyset_infoPatch
        {
            internal static void Postfix(Property __instance)
            {
                if (__instance.info.type == PropertyType.Export)
                    __instance.label.text = "";
            }
        }

        // 값이 바뀔 때 onChange 호출
        internal static class ValueChangePatches
        {
            // Color: OnEndEdit - string s
            // File: ProcessFile - string newFilename
            // LongText: Add Listener On Setup
            // Rating: SetInt - int var
            // Text: Add Listener On Setup
            // Toggle: SelectVar - string var
            // Vector2: SetVectorVals - string sX, string sY

            [HarmonyPatch]
            internal static class ValueChangePatch1
            {
                internal static IEnumerable<MethodBase> TargetMethods()
                {
                    yield return AccessTools.Method(typeof(PropertyControl_Color), "OnEndEdit");
                    yield return AccessTools.Method(typeof(PropertyControl_File), "ProcessFile");
                    yield return AccessTools.Method(typeof(PropertyControl_Rating), "SetInt");
                    yield return AccessTools.Method(typeof(PropertyControl_Toggle), "SelectVar");
                    yield return AccessTools.Method(typeof(PropertyControl_Vector2), "SetVectorVals");
                }

                internal static void Prefix(PropertyControl __instance, PropertiesPanel ___propertiesPanel, ADOFAI.PropertyInfo ___propertyInfo, ref object __state)
                {
                    if (__instance is PropertyControl_Toggle control && control.settingText)
                        return;
                    ___propertiesPanel.inspectorPanel.selectedEvent.data.TryGetValue(___propertyInfo.name, out __state);
                }

                internal static void Postfix(PropertyControl __instance, PropertiesPanel ___propertiesPanel, ADOFAI.PropertyInfo ___propertyInfo, object __state)
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
            internal static class ValueChangePatch2
            {
                internal static IEnumerable<MethodBase> TargetMethods()
                {
                    yield return AccessTools.Method(typeof(PropertyControl_LongText), "Setup");
                    yield return AccessTools.Method(typeof(PropertyControl_Text), "Setup");
                }

                internal static void Postfix(PropertyControl __instance, PropertiesPanel ___propertiesPanel, ADOFAI.PropertyInfo ___propertyInfo)
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
