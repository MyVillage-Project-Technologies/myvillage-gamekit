using System.Reflection;
using Unity.VisualScripting;

namespace MyVillage.GameKit.VisualScripting.ConfigUnits
{
    /// Visual Scripting node: read a string field from the active MissionConfig
    /// (the SO injected by the host via IMissionHost.Config). Field is looked
    /// up by exact name, public instance only.
    ///
    /// This is the safe alternative to a generic InvokeMember/GetMember unit —
    /// devs can pull data out of their config without unrestricted reflection.
    [UnitCategory("MyVillage/Config")]
    [UnitTitle("Read Config (String)")]
    [UnitSurtitle("MyVillage")]
    public sealed class ReadConfigStringUnit : Unit
    {
        [DoNotSerialize] public ValueInput fieldName;
        [DoNotSerialize] public ValueInput defaultValue;
        [DoNotSerialize] public ValueOutput result;

        protected override void Definition()
        {
            fieldName = ValueInput<string>(nameof(fieldName), string.Empty);
            defaultValue = ValueInput<string>(nameof(defaultValue), string.Empty);
            result = ValueOutput<string>(nameof(result), Read);
        }

        string Read(Flow flow)
        {
            var host = HostResolver.Resolve(flow);
            var config = host?.Config;
            if (config == null) return flow.GetValue<string>(defaultValue);

            var name = flow.GetValue<string>(fieldName);
            if (string.IsNullOrEmpty(name)) return flow.GetValue<string>(defaultValue);

            var field = config.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field == null || field.FieldType != typeof(string))
                return flow.GetValue<string>(defaultValue);
            return (string)field.GetValue(config) ?? flow.GetValue<string>(defaultValue);
        }
    }
}
