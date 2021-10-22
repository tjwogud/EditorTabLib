# EditorTabLib
먼저 모드를 유니티모드매니저로 적용한 후, steamapps/common/A Dance of Fire and Ice/Mods/EditorTabLib 에 위치한 EditorTabLib.dll을 종속성으로 추가하여 사용할 수 있습니다.

## 예시
```cs
CustomTabManager.AddTab(Sprite icon, int levelEventType, string eventName, string title, Type pageType)
CustomTabManager.DeleteTab(int levelEventType)
```