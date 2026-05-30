using UnityEngine;

namespace ArmyRush
{
    public enum HapticCue
    {
        Light,
        Warning,
        Medium,
        Success,
        Failure
    }

    public sealed class HapticsService
    {
        private readonly SaveService _saveService;
        private float _lastVibrationTime;

        public HapticsService(SaveService saveService)
        {
            _saveService = saveService;
        }

        public void Play(HapticCue cue)
        {
            if (!_saveService.Data.hapticsEnabled)
            {
                return;
            }

            if (Time.unscaledTime - _lastVibrationTime < 0.25f)
            {
                return;
            }

            _lastVibrationTime = Time.unscaledTime;

#if UNITY_IOS && !UNITY_EDITOR
            if (cue == HapticCue.Warning || cue == HapticCue.Medium || cue == HapticCue.Success || cue == HapticCue.Failure)
            {
                Handheld.Vibrate();
            }
#endif
        }
    }
}
