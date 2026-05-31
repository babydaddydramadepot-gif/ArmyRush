using UnityEngine;

namespace ArmyRush
{
    public sealed class SaveService
    {
        private const string SaveKey = "ArmyRush_SaveData_v1";

        public PlayerSaveData Data { get; private set; }

        public void Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            bool shouldSave = false;
            if (string.IsNullOrWhiteSpace(json))
            {
                Data = new PlayerSaveData();
                shouldSave = true;
            }
            else
            {
                try
                {
                    Data = JsonUtility.FromJson<PlayerSaveData>(json) ?? new PlayerSaveData();
                }
                catch
                {
                    Debug.LogWarning("Save data was unreadable. Creating a fresh save.");
                    Data = new PlayerSaveData();
                    shouldSave = true;
                }
            }

            shouldSave |= Data.Normalize();
            if (shouldSave)
            {
                Save();
            }
        }

        public void Save()
        {
            if (Data == null)
            {
                Data = new PlayerSaveData();
            }

            Data.Normalize();
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

#if UNITY_EDITOR
        public void ResetSave()
        {
            Data = new PlayerSaveData();
            Save();
        }
#endif
    }
}
