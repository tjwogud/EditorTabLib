using ADOFAI;

namespace EditorTabLib.Utils
{
    public static class LevelEventUtils
    {
        public static void UpdatePanel(this LevelEvent e)
        {
            scnEditor.instance.settingsPanel.panelsList.Find(panel => panel.levelEventType == e.eventType).SetProperties(e);
        }
    }
}
