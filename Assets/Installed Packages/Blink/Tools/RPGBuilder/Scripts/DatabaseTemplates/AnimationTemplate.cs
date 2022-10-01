using System.Collections.Generic;

namespace BLINK.RPGBuilder.Templates
{
    public class AnimationTemplate : RPGBuilderDatabaseEntry
    {
        public AnimationParameterType ParameterType;
        public AnimationEntryParameterType EntryParameterType = AnimationEntryParameterType.Single;
        public string ParameterName;
        public List<string> ParameterNames = new List<string>();
        public int IntValue;
        public float FloatValue;
        public bool BoolValue;
        public bool IsToggle;
        public bool ResetAfterDuration;
        public float Duration = 1;
        public bool ToggleOtherBool;
        public string ToggledParameterName;
        public bool ToggledBoolValue;
        public bool EnableRootMotion;
        public float RootMotionDuration = 1;
        
        public void UpdateEntryData(AnimationTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            ParameterType = newEntryData.ParameterType;
            EntryParameterType = newEntryData.EntryParameterType;
            ParameterName = newEntryData.ParameterName;
            ParameterNames = newEntryData.ParameterNames;
            IntValue = newEntryData.IntValue;
            FloatValue = newEntryData.FloatValue;
            BoolValue = newEntryData.BoolValue;
            IsToggle = newEntryData.IsToggle;
            ResetAfterDuration = newEntryData.ResetAfterDuration;
            Duration = newEntryData.Duration;
            ToggleOtherBool = newEntryData.ToggleOtherBool;
            ToggledParameterName = newEntryData.ToggledParameterName;
            ToggledBoolValue = newEntryData.ToggledBoolValue;
            EnableRootMotion = newEntryData.EnableRootMotion;
            RootMotionDuration = newEntryData.RootMotionDuration;
        }
    }
}
