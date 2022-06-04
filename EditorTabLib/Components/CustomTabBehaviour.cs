using ADOFAI;

namespace EditorTabLib.Components
{
    public abstract class CustomTabBehaviour : ADOBase
    {
        public PropertiesPanel properties;

        public abstract void OnFocused();
        public abstract void OnUnFocused();
    }
}
