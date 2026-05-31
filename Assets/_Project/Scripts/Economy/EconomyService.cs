using System;

namespace ArmyRush
{
    public sealed class EconomyService
    {
        private readonly SaveService _saveService;

        public event Action<int> CoinsChanged;
        public event Action<int> GemsChanged;

        public int Coins => _saveService.Data.coins;
        public int Gems => _saveService.Data.gems;

        public EconomyService(SaveService saveService)
        {
            _saveService = saveService;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _saveService.Data.coins = PlayerSaveData.ClampCurrency((long)_saveService.Data.coins + amount);
            _saveService.Save();
            CoinsChanged?.Invoke(_saveService.Data.coins);
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (_saveService.Data.coins < amount)
            {
                return false;
            }

            _saveService.Data.coins = PlayerSaveData.ClampCurrency((long)_saveService.Data.coins - amount);
            _saveService.Save();
            CoinsChanged?.Invoke(_saveService.Data.coins);
            return true;
        }

        public void AddGems(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _saveService.Data.gems = PlayerSaveData.ClampCurrency((long)_saveService.Data.gems + amount);
            _saveService.Save();
            GemsChanged?.Invoke(_saveService.Data.gems);
        }
    }
}
