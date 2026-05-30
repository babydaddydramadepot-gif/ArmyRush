using UnityEngine;

namespace ArmyRush
{
    public enum AudioCue
    {
        Button,
        GatePositive,
        GateNegative,
        Shoot,
        Hit,
        EnemyDefeat,
        ObstacleDestroyed,
        CoinReward,
        Upgrade,
        Victory,
        Defeat
    }

    public sealed class AudioService
    {
        private readonly SaveService _saveService;
        private float _lastShootTime;
        private float _lastHitTime;

        public AudioService(SaveService saveService)
        {
            _saveService = saveService;
        }

        public void Play(AudioCue cue)
        {
            if (_saveService.Data.sfxVolume <= 0.01f)
            {
                return;
            }

            if (cue == AudioCue.Shoot && Time.unscaledTime - _lastShootTime < 0.08f)
            {
                return;
            }

            if (cue == AudioCue.Hit && Time.unscaledTime - _lastHitTime < 0.05f)
            {
                return;
            }

            if (cue == AudioCue.Shoot)
            {
                _lastShootTime = Time.unscaledTime;
            }
            else if (cue == AudioCue.Hit)
            {
                _lastHitTime = Time.unscaledTime;
            }

            // Runtime audio files are intentionally data-driven later; this hook keeps all calls centralized.
        }
    }
}
