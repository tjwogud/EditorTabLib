using ADOFAI;

namespace EditorTabLib.Utils
{
    public static class LevelEventUtils
    {
        public static void UpdatePanel(this LevelEvent e)
        {
            if (e.eventType.IsSetting())
                scnEditor.instance.settingsPanel.panelsList.Find(panel => panel.levelEventType == e.eventType).SetProperties(e);
            else if (scnEditor.instance.levelEventsPanel.selectedEvent == e)
                scnEditor.instance.levelEventsPanel.panelsList.Find(panel => panel.levelEventType == e.eventType).SetProperties(e);
        }
    }
}
