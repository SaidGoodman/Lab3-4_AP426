using DetectiveGame.Models;
using System.IO;
using System;
using Newtonsoft.Json;

namespace DetectiveGame.Services
{
    public class SaveLoadService
    {
        private readonly string _saveFilePath = "Data/SaveGame.json";

        public void SaveGame(GameState gameState)
        {
            string directoryPath = Path.GetDirectoryName(_saveFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var json = JsonConvert.SerializeObject(gameState, Formatting.Indented);
            File.WriteAllText(_saveFilePath, json);
            Console.WriteLine("Игра сохранена.");
        }

        public GameState LoadGame()
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    var json = File.ReadAllText(_saveFilePath);
                    var gameState = JsonConvert.DeserializeObject<GameState>(json) ?? new GameState();
                    Console.WriteLine("Игра загружена.");
                    return gameState;
                }
            }
            catch
            {
                Console.WriteLine("Сохранение повреждено. Начинаем новую игру.");
            }

            Console.WriteLine("Сохраненная игра не найдена.");
            return new GameState();
        }

        public void DeleteSave()
        {
            // Сброс сохранения
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
            }
        }
    }
}
