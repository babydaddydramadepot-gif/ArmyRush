using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    public enum AudioCue
    {
        Button,
        RunStart,
        CrowdGain,
        CrowdLoss,
        GatePositive,
        GateNegative,
        Shoot,
        Hit,
        EnemyDefeat,
        ObstacleDamage,
        ObstacleDestroyed,
        CoinReward,
        Upgrade,
        BossIntro,
        BossAttack,
        BossDefeat,
        Victory,
        Defeat
    }

    public sealed class AudioService
    {
        private readonly SaveService _saveService;
        private readonly Dictionary<AudioCue, AudioClip> _clips = new Dictionary<AudioCue, AudioClip>();
        private readonly List<AudioSource> _sources = new List<AudioSource>();
        private GameObject _root;
        private AudioSource _musicSource;
        private AudioClip _musicClip;
        private int _nextSourceIndex;
        private float _lastShootTime;
        private float _lastHitTime;
        private float _lastObstacleDamageTime;
        private float _lastCoinRewardTime;
        private float _lastCrowdChangeTime;

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
            if (cue == AudioCue.ObstacleDamage && Time.unscaledTime - _lastObstacleDamageTime < 0.09f)
            {
                return;
            }
            if (cue == AudioCue.CoinReward && Time.unscaledTime - _lastCoinRewardTime < 0.12f)
            {
                return;
            }
            if ((cue == AudioCue.CrowdGain || cue == AudioCue.CrowdLoss) && Time.unscaledTime - _lastCrowdChangeTime < 0.1f)
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
            else if (cue == AudioCue.ObstacleDamage)
            {
                _lastObstacleDamageTime = Time.unscaledTime;
            }
            else if (cue == AudioCue.CoinReward)
            {
                _lastCoinRewardTime = Time.unscaledTime;
            }
            else if (cue == AudioCue.CrowdGain || cue == AudioCue.CrowdLoss)
            {
                _lastCrowdChangeTime = Time.unscaledTime;
            }

            if (!Application.isPlaying)
            {
                return;
            }

            EnsureRuntimeAudio();
            if (!_clips.TryGetValue(cue, out AudioClip clip) || clip == null || _sources.Count == 0)
            {
                return;
            }

            AudioSource source = _sources[_nextSourceIndex];
            _nextSourceIndex = (_nextSourceIndex + 1) % _sources.Count;
            source.pitch = Random.Range(0.96f, 1.04f);
            source.volume = Mathf.Clamp01(_saveService.Data.sfxVolume);
            source.PlayOneShot(clip);
        }

        public void StartMusic()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureRuntimeAudio();
            if (_musicSource == null || _musicClip == null)
            {
                return;
            }

            ApplyMusicVolume();
            if (!_musicSource.isPlaying && _saveService.Data.musicVolume > 0.01f)
            {
                _musicSource.Play();
            }
        }

        public void SetMusicVolume(float volume)
        {
            _saveService.Data.musicVolume = Mathf.Clamp01(volume);
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureRuntimeAudio();
            ApplyMusicVolume();
            if (_musicSource == null)
            {
                return;
            }

            if (_saveService.Data.musicVolume <= 0.01f)
            {
                _musicSource.Stop();
            }
            else if (!_musicSource.isPlaying)
            {
                _musicSource.Play();
            }
        }

        public void SetSfxVolume(float volume)
        {
            _saveService.Data.sfxVolume = Mathf.Clamp01(volume);
        }

        private void EnsureRuntimeAudio()
        {
            if (_root != null)
            {
                return;
            }

            _root = new GameObject("AudioServiceRoot");
            Object.DontDestroyOnLoad(_root);

            for (int i = 0; i < 8; i++)
            {
                AudioSource source = _root.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.loop = false;
                _sources.Add(source);
            }

            _musicSource = _root.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.spatialBlend = 0f;
            _musicSource.loop = true;
            _musicSource.pitch = 1f;

            _clips[AudioCue.Button] = CreateTone("SFX_Button", 620f, 0.055f, 0.18f, 0.04f);
            _clips[AudioCue.RunStart] = CreateArpeggio("SFX_RunStart", 460f, 820f, 0.24f, 0.24f);
            _clips[AudioCue.CrowdGain] = CreateArpeggio("SFX_CrowdGain", 540f, 960f, 0.16f, 0.22f);
            _clips[AudioCue.CrowdLoss] = CreateTone("SFX_CrowdLoss", 180f, 0.13f, 0.2f, 0.03f);
            _clips[AudioCue.GatePositive] = CreateArpeggio("SFX_GatePositive", 520f, 780f, 0.18f, 0.28f);
            _clips[AudioCue.GateNegative] = CreateTone("SFX_GateNegative", 170f, 0.18f, 0.24f, 0.02f);
            _clips[AudioCue.Shoot] = CreateNoiseBurst("SFX_Shoot", 0.045f, 0.09f, 0.45f);
            _clips[AudioCue.Hit] = CreateNoiseBurst("SFX_Hit", 0.05f, 0.16f, 0.72f);
            _clips[AudioCue.EnemyDefeat] = CreateArpeggio("SFX_EnemyDefeat", 420f, 660f, 0.14f, 0.2f);
            _clips[AudioCue.ObstacleDamage] = CreateNoiseBurst("SFX_ObstacleDamage", 0.08f, 0.18f, 0.28f);
            _clips[AudioCue.ObstacleDestroyed] = CreateNoiseBurst("SFX_Destroy", 0.22f, 0.28f, 0.85f);
            _clips[AudioCue.CoinReward] = CreateArpeggio("SFX_CoinReward", 760f, 1180f, 0.2f, 0.32f);
            _clips[AudioCue.Upgrade] = CreateArpeggio("SFX_Upgrade", 580f, 980f, 0.2f, 0.3f);
            _clips[AudioCue.BossIntro] = CreateArpeggio("SFX_BossIntro", 120f, 360f, 0.5f, 0.36f);
            _clips[AudioCue.BossAttack] = CreateTone("SFX_BossAttack", 96f, 0.22f, 0.32f, 0.08f);
            _clips[AudioCue.BossDefeat] = CreateNoiseBurst("SFX_BossDefeat", 0.42f, 0.36f, 0.62f);
            _clips[AudioCue.Victory] = CreateArpeggio("SFX_Victory", 520f, 1040f, 0.42f, 0.34f);
            _clips[AudioCue.Defeat] = CreateTone("SFX_Defeat", 140f, 0.32f, 0.28f, 0.01f);
            _musicClip = CreateMusicLoop();
            _musicSource.clip = _musicClip;
            ApplyMusicVolume();
        }

        private static AudioClip CreateTone(string name, float frequency, float duration, float gain, float vibrato)
        {
            const int sampleRate = 22050;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                float mod = vibrato > 0f ? Mathf.Sin(t * 42f) * vibrato : 0f;
                data[i] = Mathf.Sin((frequency + frequency * mod) * Mathf.PI * 2f * t) * envelope * gain;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateArpeggio(string name, float startFrequency, float endFrequency, float duration, float gain)
        {
            const int sampleRate = 22050;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float normalized = i / (float)Mathf.Max(1, samples - 1);
                float t = i / (float)sampleRate;
                float step = Mathf.Floor(normalized * 4f) / 3f;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, Mathf.Clamp01(step));
                float envelope = Mathf.Sin(normalized * Mathf.PI);
                data[i] = Mathf.Sin(frequency * Mathf.PI * 2f * t) * envelope * gain;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateNoiseBurst(string name, float duration, float gain, float toneBlend)
        {
            const int sampleRate = 22050;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            uint seed = 1234567u;
            for (int i = 0; i < samples; i++)
            {
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed >> 16) / 32768f) - 1f;
                float normalized = i / (float)Mathf.Max(1, samples - 1);
                float envelope = Mathf.Pow(1f - normalized, 2.2f);
                float tone = Mathf.Sin((180f + toneBlend * 800f) * Mathf.PI * 2f * i / sampleRate);
                data[i] = Mathf.Lerp(noise, tone, 0.24f) * envelope * gain;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateMusicLoop()
        {
            const int sampleRate = 22050;
            const float duration = 8f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            float[] notes = { 110f, 146.83f, 164.81f, 196f, 220f, 196f, 164.81f, 146.83f };

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float normalized = i / (float)Mathf.Max(1, samples - 1);
                int noteIndex = Mathf.FloorToInt(normalized * notes.Length) % notes.Length;
                float bass = Mathf.Sin(notes[noteIndex] * Mathf.PI * 2f * t) * 0.11f;
                float pulse = Mathf.Sin(notes[(noteIndex + 2) % notes.Length] * Mathf.PI * 4f * t) * 0.035f;
                float tick = Mathf.Sin(880f * Mathf.PI * 2f * t) * Mathf.Pow(Mathf.Clamp01(1f - (t % 0.5f) / 0.08f), 5f) * 0.025f;
                float edgeFade = Mathf.Min(Mathf.Clamp01(t / 0.12f), Mathf.Clamp01((duration - t) / 0.12f));
                data[i] = (bass + pulse + tick) * edgeFade * 0.42f;
            }

            AudioClip clip = AudioClip.Create("Music_RuntimeMarchLoop", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void ApplyMusicVolume()
        {
            if (_musicSource != null)
            {
                _musicSource.volume = Mathf.Clamp01(_saveService.Data.musicVolume) * 0.32f;
            }
        }
    }
}
