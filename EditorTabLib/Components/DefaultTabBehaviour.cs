﻿using System;

namespace EditorTabLib.Components
{
    public class DefaultTabBehaviour : CustomTabBehaviour
    {
        public Action onFocused;
        public Action onUnFocused;

        public override void OnFocused()
        {
            onFocused?.Invoke();
        }

        public override void OnUnFocused()
        {
            onUnFocused?.Invoke();
        }
    }
}
