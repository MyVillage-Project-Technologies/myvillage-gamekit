using System.Reflection;
using Unity.VisualScripting;

namespace MyVillage.GameKit.VisualScripting.ConfigUnits
{
    /// Visual Scripting node: read an int field from the active MissionConfig.
    [UnitCategory("MyVillage/Config")]
    [UnitTitle("Read Config (Int)")]
    [UnitSurtitle("MyVillage")]
    public sealed class ReadConfigIntUnit : Unit
    {
        [DoNotSerialize] public ValueInput fieldName;
        [DoNotSerialize] public ValueInput defaultValue;
        [DoNotSerialize] public ValueOutput result;

        protected override void Definition()
        {
            fieldName = ValueInput<string>(nameof(fieldName), string.Empty);
            defaultValue = ValueInput<int>(nameof(defaultValue), 0);
            result = ValueOutput<int>(nameof(result), Read);
        }

        int Read(Flow flow)
        {
            var host = HostResolver.Resolve(flow);
            var config = host?.Config;
            if (config == null) return flow.GetValue<int>(defaultValue);

            var name = flow.GetValue<string>(fieldName);
            if (string.IsNullOrEmpty(name)) return flow.GetValue<int>(defaultValue);

            var field = config.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field == null || field.FieldType != typeof(int))
                return flow.GetValue<int>(defaultValue);
            return (int)field.GetValue(config);
        }
    }
}
