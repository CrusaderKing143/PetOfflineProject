using System;
using UnityEngine;

namespace PetOffline.Core
{
    public sealed class StorySaveService
    {
        public const int CurrentVersion = 1;
        public const string DefaultPlayerPrefsKey = "PetOffline.StorySave";

        private readonly string _playerPrefsKey;
        private SavePayload _save;

        public StorySaveService(string playerPrefsKey = DefaultPlayerPrefsKey)
        {
            _playerPrefsKey = string.IsNullOrWhiteSpace(playerPrefsKey)
                ? DefaultPlayerPrefsKey
                : playerPrefsKey;
            _save = CreateEmptySave();
        }

        public StoryProgress Progress => ToProgress(_save);

        public StoryProgress Load()
        {
            if (!PlayerPrefs.HasKey(_playerPrefsKey))
            {
                _save = CreateEmptySave();
                return Progress;
            }

            string json = PlayerPrefs.GetString(_playerPrefsKey, string.Empty);
            if (!TryDeserialize(json, out SavePayload loaded))
            {
                DeleteInvalidSave();
                return Progress;
            }

            _save = loaded;
            return Progress;
        }

        public bool MarkDay1Complete()
        {
            if (_save.day1Complete)
            {
                return false;
            }

            _save.day1Complete = true;
            Persist();
            return true;
        }

        public bool MarkDay2Complete(FinalChoice choice)
        {
            if (!IsEndingChoice(choice))
            {
                throw new ArgumentOutOfRangeException(nameof(choice));
            }

            bool unchanged = _save.day1Complete
                && _save.day2Complete
                && _save.finalChoice == (int)choice;
            if (unchanged)
            {
                return false;
            }

            _save.day1Complete = true;
            _save.day2Complete = true;
            _save.finalChoice = (int)choice;
            Persist();
            return true;
        }

        public void Clear()
        {
            _save = CreateEmptySave();
            PlayerPrefs.DeleteKey(_playerPrefsKey);
            PlayerPrefs.Save();
        }

        private static SavePayload CreateEmptySave()
        {
            return new SavePayload
            {
                version = CurrentVersion,
                finalChoice = (int)FinalChoice.None
            };
        }

        private static StoryProgress ToProgress(SavePayload save)
        {
            return new StoryProgress(
                save.day1Complete,
                save.day2Complete,
                (FinalChoice)save.finalChoice);
        }

        private static bool TryDeserialize(string json, out SavePayload save)
        {
            save = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                save = JsonUtility.FromJson<SavePayload>(json);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return IsValid(save);
        }

        private static bool IsValid(SavePayload save)
        {
            if (save == null || save.version != CurrentVersion)
            {
                return false;
            }

            FinalChoice choice = (FinalChoice)save.finalChoice;
            if (!save.day2Complete)
            {
                return choice == FinalChoice.None;
            }

            return save.day1Complete && IsEndingChoice(choice);
        }

        private static bool IsEndingChoice(FinalChoice choice)
        {
            return choice == FinalChoice.RestoreConnection
                || choice == FinalChoice.KeepQuiet;
        }

        private void Persist()
        {
            _save.version = CurrentVersion;
            PlayerPrefs.SetString(_playerPrefsKey, JsonUtility.ToJson(_save));
            PlayerPrefs.Save();
        }

        private void DeleteInvalidSave()
        {
            _save = CreateEmptySave();
            PlayerPrefs.DeleteKey(_playerPrefsKey);
            PlayerPrefs.Save();
        }

        [Serializable]
        private sealed class SavePayload
        {
            public int version;
            public bool day1Complete;
            public bool day2Complete;
            public int finalChoice;
        }
    }
}
