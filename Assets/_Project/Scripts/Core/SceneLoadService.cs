using UnityEngine.SceneManagement;

namespace ArmyRush
{
    public sealed class SceneLoadService
    {
        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void LoadGameplay()
        {
            SceneManager.LoadScene("Game");
        }
    }
}
