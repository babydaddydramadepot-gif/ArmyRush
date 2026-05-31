namespace ArmyRush
{
    public sealed class ProgressionService
    {
        private readonly SaveService _saveService;

        public int CurrentLevelIndex => _saveService.Data.currentLevelIndex;

        public ProgressionService(SaveService saveService)
        {
            _saveService = saveService;
        }

        public void CompleteCurrentLevel()
        {
            int completedLevel = _saveService.Data.currentLevelIndex;
            ClearFailureStreak(completedLevel);
            if (_saveService.Data.currentLevelIndex < int.MaxValue)
            {
                _saveService.Data.currentLevelIndex++;
            }
            _saveService.Data.tutorialCompleted = true;
            _saveService.Save();
        }

        public int GetFailureStreak(int levelIndex)
        {
            if (levelIndex <= 0 || _saveService.Data.lastFailedLevelIndex != levelIndex)
            {
                return 0;
            }

            return _saveService.Data.consecutiveLevelFailures;
        }

        public void RecordLevelFailure(int levelIndex)
        {
            if (levelIndex <= 0)
            {
                return;
            }

            if (_saveService.Data.lastFailedLevelIndex == levelIndex)
            {
                _saveService.Data.consecutiveLevelFailures++;
            }
            else
            {
                _saveService.Data.lastFailedLevelIndex = levelIndex;
                _saveService.Data.consecutiveLevelFailures = 1;
            }

            _saveService.Save();
        }

        public void ClearFailureStreak(int levelIndex)
        {
            if (levelIndex <= 0 || _saveService.Data.lastFailedLevelIndex != levelIndex)
            {
                return;
            }

            _saveService.Data.lastFailedLevelIndex = 0;
            _saveService.Data.consecutiveLevelFailures = 0;
        }
    }
}
