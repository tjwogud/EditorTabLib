using ADOFAI;
using EditorTabLib.Components;
using EditorTabLib.Properties;
using EditorTabLib.Utils;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
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
                CustomTabBehaviour behaviour = scnEditor.instance?.settingsPanel?.panelsList?.Find(panel => panel.levelEventType == __instance.levelEventType)?.content?.GetComponent<CustomTabBehaviour>();
                if (selected)
                    behaviour?.OnFocused();
                else if (__instance.icon?.color.a == 1)
                    behaviour?.OnUnFocused();
            }
        }

        // 커스텀 탭의 버튼에 리스너 추가, 버튼 텍스트 설정
        [HarmonyPatch]
        internal static class PropertyControl_ExportSetupPatch
        {
            internal static MethodBase TargetMethod()
            {
                return AccessTools.Method(ADOFAITypes.control, "Setup");
            }

            internal static void Postfix(object __instance)
            {
                ADOFAI.PropertyInfo info = __instance.Get<ADOFAI.PropertyInfo>("propertyInfo");
                if (!(info.value_default is UnityAction action))
                    return;
                Button button = __instance.Get<Button>("exportButton");
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
                object text = __instance.Get("buttonText");
                if (info.customLocalizationKey == null)
                {
                    string str = "editor." + info.levelEventInfo.name + "." + info.name;
                    text.Set("text", RDString.GetWithCheck(str, out bool flag, null));
                    if (!flag)
                        text.Set("text", RDString.GetWithCheck("editor." + info.name, out _, null));
                }
                else
                    text.Set("text", (info.customLocalizationKey == "") ? "" : RDString.Get(info.customLocalizationKey, null));
            }
        }

        // 버튼의 설명 텍스트 제거
        [HarmonyPatch(typeof(ADOFAI.Property), "info", MethodType.Setter)]
        internal static class Propertyset_infoPatch
        {
            internal static void Postfix(ADOFAI.Property __instance)
            {
                if (__instance.info.type == PropertyType.Export)
                    __instance.label.text = "";
            }
        }

        // Property_List 관련 기능
        [HarmonyPatch]
        internal static class PropertyInfoConstructor
        {
            internal static MethodBase TargetMethod()
            {
                return AccessTools.Constructor(typeof(ADOFAI.PropertyInfo), new Type[] { typeof(Dictionary<string, object>), typeof(LevelEventInfo) });
            }

            internal static void Prefix(Dictionary<string, object> dict, out (bool, string) __state)
            {
                string text = dict["type"] as string;
                if (text.StartsWith("Enum:") && text.Length > 5 && typeof(Property_List.Dummy).Equals(Type.GetType(text.Substring(5))))
                {
                    __state = (true, dict["default"] as string);
                    dict.Remove("default");
                }
                else
                    __state = (false, null);
            }

            internal static void Postfix(ADOFAI.PropertyInfo __instance, (bool, string) __state)
            {
                if (__state.Item1)
                    __instance.value_default = __state.Item2;
            }
        }

        // Property_List 관련 기능
        [HarmonyPatch(typeof(TweakableDropdownItem), "localizedValue", MethodType.Getter)]
        internal static class TweakableDropdownItemgetlocalizedValuePatch
        {
            internal static bool Prefix(TweakableDropdownItem __instance, ref string __result)
            {
                if (__instance.localizeValue && __instance.dropdown.transform.parent?.GetComponent(ADOFAITypes.controls["Toggle"])?.Get<ADOFAI.PropertyInfo>("propertyInfo").enumType is Type t && t.Equals(typeof(Property_List.Dummy)))
                {
                    __result = RDStringEx.GetOrOrigin(__instance.value);
                    return false;
                }
                return true;
            }
        }

        // Property_List 관련 기능
        [HarmonyPatch]
        internal static class PropertyControl_ToggleEnumSetupPatch
        {
            internal static MethodBase TargetMethod()
            {
                return AccessTools.Method(ADOFAITypes.controls["Toggle"], "EnumSetup");
            }

            internal static void Prefix(object __instance, ref string enumTypeString, ref List<string> enumVals)
            {
                if (enumTypeString != null)
                {
                    Type type = typeof(ADOBase).Assembly.GetType(enumTypeString);
                    if (type != null || (type = Type.GetType(enumTypeString))?.Assembly == typeof(ADOBase).Assembly)
                        enumTypeString = type.FullName;
                }
                ADOFAI.PropertyInfo info = __instance.Get<ADOFAI.PropertyInfo>("propertyInfo");
                if (typeof(Property_List.Dummy).Equals(info.enumType))
                    enumVals = info.unit.Split(';').ToList();
            }
            
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>();
                Label? targetLabel1 = null;
                for (int i = 0; i < instructions.Count(); i++)
                {
                    CodeInstruction code = instructions.ElementAt(i);
                    if (code.opcode == OpCodes.Ldarg_3)
                    {
                        CodeInstruction nextCode = instructions.ElementAt(i + 1);
                        if (nextCode.opcode == OpCodes.Brtrue_S)
                            targetLabel1 = (Label)nextCode.operand;
                    }
                    if (targetLabel1.HasValue && code.labels.Contains(targetLabel1.Value))
                    {
                        Label label1 = generator.DefineLabel();
                        Label label2 = (Label)instructions.ElementAt(i - 1).operand;
                        codes.Add(new CodeInstruction(OpCodes.Ldarg_0).WithLabels(code.ExtractLabels()));
                        codes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(ADOFAITypes.control, "propertyInfo")));
                        codes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ADOFAI.PropertyInfo), "enumType")));
                        codes.Add(new CodeInstruction(OpCodes.Ldtoken, typeof(Property_List.Dummy)));
                        codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Type), "GetTypeFromHandle")));
                        codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Type), "Equals", new Type[] { typeof(Type) })));
                        codes.Add(new CodeInstruction(OpCodes.Brfalse, label1));
                        codes.Add(new CodeInstruction(instructions.ElementAt(i - 3)));
                        codes.Add(new CodeInstruction(instructions.ElementAt(i - 2)));
                        codes.Add(new CodeInstruction(OpCodes.Ldnull));
                        codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RDStringEx), "GetOrOrigin")));
                        codes.Add(new CodeInstruction(OpCodes.Br, label2));
                        codes.Add(code.WithLabels(label1));

                        continue;
                    }
                    codes.Add(code);
                }
                return codes;
            }
        }

        // Property_List 관련 기능
        //[HarmonyPatch(typeof(PropertyControl_Toggle), "SelectVar")]
        internal static class PropertyControl_ToggleSelectVarPatch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>();
                for (int i = 0; i < instructions.Count(); i++)
                {
                    CodeInstruction code = instructions.ElementAt(i);
                    if (code.opcode == OpCodes.Call && (code.operand is MethodInfo method) && method.Name == "Parse")
                    {
                        CodeInstruction nextCode = instructions.ElementAt(i + 1);
                        if (nextCode.opcode != OpCodes.Unbox_Any)
                        {
                            codes.RemoveAt(codes.Count() - 2);
                            codes.RemoveAt(codes.Count() - 1);
                            Label label1 = generator.DefineLabel();
                            Label label2 = generator.DefineLabel();
                            codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                            codes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(ADOFAITypes.control, "propertyInfo")));
                            codes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ADOFAI.PropertyInfo), "enumType")));
                            codes.Add(new CodeInstruction(OpCodes.Ldtoken, typeof(Property_List.Dummy)));
                            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Type), "GetTypeFromHandle")));
                            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Type), "Equals", new Type[] { typeof(Type) })));
                            codes.Add(new CodeInstruction(OpCodes.Brfalse, label1));
                            codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                            codes.Add(new CodeInstruction(OpCodes.Br, label2));
                            codes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(ADOFAITypes.control, "propertyInfo")).WithLabels(label1));
                            codes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ADOFAI.PropertyInfo), "enumType")));
                            codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Enum), "Parse", new Type[] { typeof(Type), typeof(string) })));
                            codes.Add(nextCode.WithLabels(label2));
                            i++;
                            continue;
                        }
                    }
                    codes.Add(code);
                }
                return codes;
            }
        }

        [HarmonyPatch]
        public static class SetupPatch
        {
            internal static MethodBase TargetMethod()
            {
                return AccessTools.Method(ADOFAITypes.controls["Tile"], "Setup");
            }

            public static void Postfix(object __instance)
            {
                if (__instance.Get<ADOFAI.PropertyInfo>("propertyInfo").dict.TryGetValue("hideButtons", out object value) && value is int i)
                {
                    if ((i & Property_Tile.THIS_TILE) != 0)
                        __instance.Get<MonoBehaviour>("buttonThisTile").gameObject.SetActive(false);
                    if ((i & Property_Tile.START) != 0)
                        __instance.Get<MonoBehaviour>("buttonFirstTile").gameObject.SetActive(false);
                    if ((i & Property_Tile.END) != 0)
                        __instance.Get<MonoBehaviour>("buttonLastTile").gameObject.SetActive(false);
                }
            }
        }

        // 값이 바뀔 때 onChange 호출
        internal static class ValueChangePatches
        {
            // Bool: SetValue - bool flag
            // Color: OnEndEdit|OnChange - string s
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
                    yield return AccessTools.Method(ADOFAITypes.controls["Bool"], "SetValue");
                    yield return AccessTools.Method(ADOFAITypes.controls["Color"], "OnEndEdit") ?? AccessTools.Method(ADOFAITypes.controls["Color"], "OnChange");
                    yield return AccessTools.Method(ADOFAITypes.controls["File"], "ProcessFile");
                    yield return AccessTools.Method(ADOFAITypes.controls["Rating"], "SetInt");
                    yield return AccessTools.Method(ADOFAITypes.controls["Toggle"], "SelectVar");
                    yield return AccessTools.Method(ADOFAITypes.controls["Vector2"], "SetVectorVals");
                }

                internal static void Prefix(object __instance, PropertiesPanel ___propertiesPanel, ADOFAI.PropertyInfo ___propertyInfo, ref object __state)
                {
                    if (__instance.GetType().Equals(ADOFAITypes.controls["Toggle"]) && __instance.Get<bool>("settingText"))
                        return;
                    ___propertiesPanel.inspectorPanel.selectedEvent.data.TryGetValue(___propertyInfo.name, out __state);
                }

                internal static void Postfix(object __instance, PropertiesPanel ___propertiesPanel, ADOFAI.PropertyInfo ___propertyInfo, object __state)
                {
                    if (__instance.GetType().Equals(ADOFAITypes.controls["Toggle"]) && __instance.Get<bool>("settingText"))
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
                    yield return AccessTools.Method(ADOFAITypes.controls["LongText"], "Setup");
                    yield return AccessTools.Method(ADOFAITypes.controls["Text"], "Setup");
                }

                internal static void Postfix(object __instance, PropertiesPanel ___propertiesPanel, ADOFAI.PropertyInfo ___propertyInfo)
                {
                    if (!CustomTabManager.byType.TryGetValue((int)___propertyInfo.levelEventInfo.type, out CustomTabManager.CustomTab tab)
                        || tab.onChange == null)
                        return;
                    TMP_InputField inputField = __instance.Get<TMP_InputField>("inputField");
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
                    object prefixObj = inputField.onEndEdit.Method("GetDelegate", new object[] { prefix }, new Type[] { typeof(UnityAction<string>) });
                    object postfixObj = inputField.onEndEdit.Method("GetDelegate", new object[] { postfix }, new Type[] { typeof(UnityAction<string>) });
                    runtime.Method("Insert", new object[] { 0, prefixObj }, new Type[] { typeof(int), Reflections.GetType("UnityEngine.Events.BaseInvokableCall") });
                    runtime.Method("Add", new object[] { postfixObj }, new Type[] { Reflections.GetType("UnityEngine.Events.BaseInvokableCall") });
                    obj.Set("m_NeedsUpdate", true);
                }
            }

            [HarmonyPatch(typeof(PropertiesPanel), "UpdateEnabledButton")]
            internal static class EnabledButtonPatch
            {
                internal static void Postfix(ADOFAI.Property property, bool disabled)
                {
                    if (!disabled && property.helpButton.gameObject.activeInHierarchy)
                        property.enabledButton.GetComponent<RectTransform>().offsetMax = new Vector2(-30, 0);
                }
            }
        }
    }
}
