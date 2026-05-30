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
            _saveService.Data.currentLevelIndex++;
            _saveService.Data.tutorialCompleted = true;
            _saveService.Save();
        }
    }
}
