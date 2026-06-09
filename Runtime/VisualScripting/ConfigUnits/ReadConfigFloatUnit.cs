using System.Reflection;
using Unity.VisualScripting;

namespace MyVillage.GameKit.VisualScripting.ConfigUnits
{
    /// Visual Scripting node: read a float field from the active MissionConfig.
    [UnitCategory("MyVillage/Config")]
    [UnitTitle("Read Config (Float)")]
    [UnitSurtitle("MyVillage")]
    public sealed class ReadConfigFloatUnit : Unit
    {
        [DoNotSerialize] public ValueInput fieldName;
        [DoNotSerialize] public ValueInput defaultValue;
        [DoNotSerialize] public ValueOutput result;

        protected override void Definition()
        {
            fieldName = ValueInput<string>(nameof(fieldName), string.Empty);
            defaultValue = ValueInput<float>(nameof(defaultValue), 0f);
            result = ValueOutput<float>(nameof(result), Read);
        }

        float Read(Flow flow)
        {
            var host = HostResolver.Resolve(flow);
            var config = host?.Config;
            if (config == null) return flow.GetValue<float>(defaultValue);

            var name = flow.GetValue<string>(fieldName);
            if (string.IsNullOrEmpty(name)) return flow.GetValue<float>(defaultValue);

            var field = config.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field == null || field.FieldType != typeof(float))
                return flow.GetValue<float>(defaultValue);
            return (float)field.GetValue(config);
        }
    }
}
