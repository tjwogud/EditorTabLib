using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorTabLib.Properties
{
    public class Property_InputField : Property
    {
        public enum InputType
        {
            Float, Int, String, Vector2
        }

        private readonly InputType type;
        private readonly object value_default;
        private readonly object min;
        private readonly object max;
        private readonly string unit;

        public Property_InputField(string name, InputType type, object value_default = null, object min = null, object max = null, string unit = null, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            this.type = type;
            this.unit = unit;
            switch (type)
            {
                case InputType.Float:
                    this.value_default = Convert.ToSingle(value_default);
                    this.min = min != null ? Convert.ToSingle(min) : float.NegativeInfinity;
                    this.max = max != null ? Convert.ToSingle(max) : float.PositiveInfinity;
                    break;
                case InputType.Int:
                    this.value_default = Convert.ToInt32(value_default);
                    this.min = min != null ? Convert.ToInt32(min) : int.MinValue;
                    this.max = max != null ? Convert.ToInt32(max) : int.MaxValue;
                    break;
                case InputType.String:
                    this.value_default = RDString.Get(value_default as string ?? string.Empty);
                    this.min = min != null ? Convert.ToInt32(min) : int.MinValue;
                    this.max = max != null ? Convert.ToInt32(max) : int.MaxValue;
                    break;
                case InputType.Vector2:
                    this.value_default = value_default != null ? new List<object> { ((Vector2)value_default).x, ((Vector2)value_default).y } : new List<object> { 0, 0 };
                    this.min = min != null ? new List<object>() { ((Vector2)min).x, ((Vector2)min).y } : new List<object> { float.NegativeInfinity, float.NegativeInfinity };
                    this.max = max != null ? new List<object>() { ((Vector2)max).x, ((Vector2)max).y } : new List<object> { float.PositiveInfinity, float.PositiveInfinity };
                    break;
            }
        }

        public override Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
                    {
                        { "name", name },
                        { "type", Enum.GetName(typeof(InputType), type) },
                        { "default", value_default },
                        { "min", min },
                        { "max", max },
                        { "unit", unit },
                        { "canBeDisabled", canBeDisabled },
                        { "startEnabled", startEnabled },
                        { "enableIf", enableIf },
                        { "disableIf", disableIf },
                        { "key", key }
                    };
        }
    }
}
