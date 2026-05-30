using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class BonusCrateController : MonoBehaviour
    {
        [SerializeField] private Damageable _damageable;
        [SerializeField] private TextMesh _healthLabel;
        [SerializeField] private TextMesh _rewardLabel;

        private RunManager _runManager;
        private int _coinReward = 25;
        private bool _paid;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
        }

        private void OnEnable()
        {
            _paid = false;
            if (_damageable != null)
            {
                _damageable.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (_damageable != null)
            {
                _damageable.Died -= OnDied;
            }
        }

        public void Configure(int health, int coinReward, RunManager runManager)
        {
            _coinReward = Mathf.Max(0, coinReward);
            _runManager = runManager != null ? runManager : FindAnyObjectByType<RunManager>();
            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }

            _damageable.Configure(CombatTargetKind.Bonus, Mathf.Max(1, health), _healthLabel);
            UpdateLabel();
        }

        private void OnDied(Damageable damageable)
        {
            if (_paid)
            {
                return;
            }

            _paid = true;
            Vector3 rewardPosition = transform.position + Vector3.up * 1.3f;
            _runManager?.AddBonusCoins(_coinReward, rewardPosition);
            VfxManager.Spawn(VfxCue.CoinBurst, rewardPosition);
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.CoinReward);
            }
            gameObject.SetActive(false);
        }

        private void UpdateLabel()
        {
            if (_rewardLabel != null)
            {
                _rewardLabel.text = "+" + _coinReward;
            }
        }
    }
}
