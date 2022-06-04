# EditorTabLib
먼저 모드를 유니티모드매니저로 적용한 후, steamapps/common/A Dance of Fire and Ice/Mods/EditorTabLib 에 위치한 EditorTabLib.dll을 종속성으로 추가하여 사용할 수 있습니다.

## 사용법 / How to use
```cs
// index는 생략 가능
// index is optional
CustomTabManager.AddTab<PageType>(Sprite icon, int levelEventType, string eventName, Dictionary<SystemLanguage, string> title, int index)

CustomTabManager.AddTab(Sprite icon, int levelEventType, string eventName, Dictionary<SystemLanguage, string> title, List<Dictionary<string, object>> properties, int index)

CustomTabManager.AddTab(Sprite icon, int levelEventType, string eventName, Dictionary<SystemLanguage, string> title, List<Property> properties, int index)

CustomTabManager.DeleteTab(int levelEventType)

CustomTabManager.DeleteTab(string eventName)
```

## 예시 / Example
```cs
public static class Main
{
    public static UnityModManager.ModEntry.ModLogger Logger;
    public static Harmony harmony;
    public static bool IsEnabled = false;
    public static Sprite icon;

    public static void Setup(UnityModManager.ModEntry modEntry)
    {
        Logger = modEntry.Logger;
        modEntry.OnToggle = OnToggle;
        modEntry.OnUpdate = OnUpdate;

        // 이미지를 Sprite로 불러옴
        // load image as Sprite
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Mods", "TestMod", "test.png");
        if (File.Exists(path))
        {
            byte[] FileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(0, 0);
            if (texture.LoadImage(FileData))
                icon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
    {
        IsEnabled = value;
        if (value)
        {
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // -------------------------------------------------------------------------------------------------------------------------------------------
            CustomTabManager.AddTab<TestPage>(icon, 900, "TestEvent1", new Dictionary<SystemLanguage, string>()
            {
                { SystemLanguage.English, "TestEventPage1" },
                { SystemLanguage.Korean, "테스트이벤트페이지1" }
            });
            // -------------------------------------------------------------------------------------------------------------------------------------------
            CustomTabManager.AddTab(icon, 901, "TestEvent2", new Dictionary<SystemLanguage, string>()
            {
                { SystemLanguage.English, "TestEventPage2" },
                { SystemLanguage.Korean, "테스트이벤트페이지2" }
            }, new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "name", "testField1" },
                    { "type", "Enum:ToggleBool" },
                    { "default", "Enabled" },
                    { "key", "testMod.editor.testField1" }
                },
                new Dictionary<string, object>()
                {
                    { "name", "testField2" },
                    { "type", "Float" },
                    { "default", 1f },
                    { "min", 0.001f },
                    { "max", 10000f },
                    { "enableIf", new List<object>() { "testField1", "Enabled" } },
                    { "key", "testMod.editor.testField2" }
                },
                new Dictionary<string, object>()
                {
                    { "name", "testField3" },
                    { "type", "String" },
                    { "default", "default text" },
                    { "enableIf", new List<object>() { "testField1", "Disabled" } },
                    { "key", "testMod.editor.testField3" }
                }
            });
            // -------------------------------------------------------------------------------------------------------------------------------------------
            CustomTabManager.AddTab(icon, 902, "TestEvent3", new Dictionary<SystemLanguage, string>()
            {
                { SystemLanguage.English, "TestEventPage3" },
                { SystemLanguage.Korean, "테스트이벤트페이지3" }
            }, new List<Property>()
            {
                new Property_Enum<ToggleBool>(
                    name: "testField1",
                    value_default: ToggleBool.Enabled,
                    key: "testMod.editor.testField1"
                ),
                new Property_InputField(
                    name: "testField2",
                    type: Property_InputField.InputType.Float,
                    value_default: 1f,
                    min: 0.001f,
                    max: 10000f,
                    key: "testMod.editor.testField2"),
                    enableIf: new Dictionary<string, string>() { { "testField1", "Enabled" } }
                ),
                new Property_InputField(
                    name: "testField3",
                    type: Property_InputField.InputType.String,
                    value_default: "default text",
                    key: "testMod.editor.testField3"),
                    enableIf: new Dictionary<string, string>() { { "testField1", "Disabled" } }
                )
            });
            // -------------------------------------------------------------------------------------------------------------------------------------------
        }
        else
        {
            harmony.UnpatchAll(modEntry.Info.Id);
            CustomTabManager.DeleteTab(500);
        }
        return true;
    }
    
    public class TestPage : CustomTabBehaviour
    {
        private Text testLabel;
    
        private void Awake()
        {
            testLabel = new GameObject().AddComponent<Text>();
            testLabel.transform.SetParent(base.transform, false);
            testLabel.color = Color.green;
            testLabel.SetLocalizedFont();
            testLabel.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300f, 25f);
            testLabel.fontSize = 19;
            testLabel.text = "테스트";
            testLabel.gameObject.SetActive(true);
        }
        
        public override void OnFocused() {
        }
        
        public override void OnUnFocused() {
        }
    }
}
```