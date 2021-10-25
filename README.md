# EditorTabLib
먼저 모드를 유니티모드매니저로 적용한 후, steamapps/common/A Dance of Fire and Ice/Mods/EditorTabLib 에 위치한 EditorTabLib.dll을 종속성으로 추가하여 사용할 수 있습니다.

## 사용법
```cs
CustomTabManager.AddTab(Sprite icon, int levelEventType, string eventName, string title, Type pageType)
// 1.2.0 이후부터
CustomTabManager.AddTab(Sprite icon, int levelEventType, string eventName, string title, Type pageType, int index)
CustomTabManager.DeleteTab(int levelEventType)
CustomTabManager.DeleteTab(string eventName)
```

## 예시
Main.cs
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
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Mods", "TestMod", "test.png");
        Texture2D Tex2D;
        byte[] FileData;
        if (File.Exists(path))
        {
            FileData = File.ReadAllBytes(path);
            Tex2D = new Texture2D(0, 0);
            if (Tex2D.LoadImage(FileData))
                icon = Sprite.Create(Tex2D, new Rect(0, 0, Tex2D.width, Tex2D.height), new Vector2(0.5f, 0.5f));
        }
    }

    private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
    {
        IsEnabled = value;
        if (value)
        {
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            CustomTabManager.AddTab(icon, 500, "TestEvent", "테스트", typeof(TestPage));
        }
        else
        {
            harmony.UnpatchAll(modEntry.Info.Id);
            CustomTabManager.DeleteTab(500);
        }
        return true;
    }
}
```

TestPage.cs
```cs
public class TestPage : MonoBehaviour
{
    private void Awake()
    {
        testLabel = new GameObject().AddComponent<Text>();
        testLabel.transform.SetParent(base.transform, false);
        testLabel.color = Color.green;
        testLabel.font = RDC.data.prefab_property.transform.GetChild(1).GetComponent<Text>().font;
        testLabel.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300f, 25f);
        testLabel.fontSize = 19;
        testLabel.text = "테스트";
        testLabel.gameObject.SetActive(true);
    }

    private Text testLabel;
}
```
