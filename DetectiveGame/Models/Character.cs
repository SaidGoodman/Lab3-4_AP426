using System;
using System.Linq;

namespace DetectiveGame.Models
{
    public class Character
    {
        public string Name { get; set; }
        public string Role { get; set; }
        // В какой локации сейчас находится персонаж
        public string LocationName { get; set; }

        public bool IsSuspect { get; set; } // Персонажи
        public int TimesInterrogated { get; set; }

        public Character() { }

        public Character(string name, string role, string locationName, bool isSuspect = false)
        {
            Name = name;
            Role = role;
            LocationName = locationName;
            IsSuspect = isSuspect;
            TimesInterrogated = 0;
        }

        public string Interrogate(GameState state)
        {
            TimesInterrogated++;

            bool Has(string evidenceName)
            {
                return state?.CollectedEvidence?.Any(e =>
                    e.Description != null &&
                    e.Description.Equals(evidenceName, StringComparison.OrdinalIgnoreCase)) == true;
            }

            switch (Name)
            {
                case "Сержант":
                    if (TimesInterrogated == 1)
                        return "Осмотрели дом? В таких делах туман любит прятать правду. Не верьте первым словам.";
                    if (Has("Квитанция ломбарда"))
                        return "Ломбард? Значит, долги. Это уже мотив. Только смотрите: у нас тут половина Лондона в долгах.";
                    return "Если найдете что-то весомое — принесите. Мне нужны факты, а не слухи.";

                case "Бармен":
                    if (TimesInterrogated == 1)
                        return "Блэквуд пил редко. Но три дня назад пришёл — руки дрожали, просил 'лекарство от нервов'.";
                    if (Has("Бутылка"))
                        return "Эта бутылка не его. Пахнет не виски, а чем-то аптечным. Такое у врачей водится.";
                    return "В тот вечер он ушёл не один. Высокий джентльмен в перчатках — будто боялся запачкаться.";

                case "Леди Эвелин":
                    if (TimesInterrogated == 1)
                        return "Мой брат — беспечный человек, детектив. Он мог исчезнуть из-за карточных долгов.";
                    if (Has("Письмо с печатью"))
                        return "Это... не должно было попасть вам в руки. Хорошо. Я признаю: он шантажировал кого-то из 'уважаемых'.";
                    return "Если вы ищете убийцу — ищите того, кто боится огласки. Я слишком хорошо знаю этот страх.";

                case "Ростовщик Грим":
                    if (TimesInterrogated == 1)
                        return "Я не убийца, я бухгалтерия человеческой слабости. Блэквуд закладывал трость и часы.";
                    if (Has("Серебряная пуговица"))
                        return "Пуговица? У меня такие носят лишь те, кто привык к стерильности и порядку. Не рабочий и не бармен.";
                    return "Он боялся. И это был не страх бедняка — это страх человека, которому есть что потерять.";

                case "Доктор Харроу":
                    if (TimesInterrogated == 1)
                        return "Пропажи случаются. Лондон широк. Я лечу людей, а не охочусь за ними.";
                    if (Has("Журнал пациентов") || Has("Рецепт с инициалами"))
                        return "Вы роетесь в моих бумагах? Опасная привычка, детектив. Бумага часто приводит к крови.";
                    if (Has("Флакон карболки"))
                        return "Запах карболки преследует каждого врача. Вы делаете выводы, не зная медицины.";
                    return "Я занят. Уходите, пока у меня хорошее настроение.";

                case "Газетчик":
                    if (TimesInterrogated == 1)
                        return "Слыхали? Мужика искали у доков. Говорят, кто-то ночью тащил мешок, а туман всё съел.";
                    if (Has("Пятно крови"))
                        return "Кровь — лучший заголовок. Только вот богатые умеют прятать её под ковром и улыбками.";
                    return "Если хотите правду — ищите того, кто везде 'случайно' появляется и всегда чист.";

                default:
                    return "Мне нечего вам сказать.";
            }
        }
    }
}
