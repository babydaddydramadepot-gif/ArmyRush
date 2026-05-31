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
            if (_saveService.Data.currentLevelIndex < int.MaxValue)
            {
                _saveService.Data.currentLevelIndex++;
            }
            _saveService.Data.tutorialCompleted = true;
            _saveService.Save();
        }
    }
}
