using System;

namespace Skyjoo.GameLogic.Bots
{
    public abstract partial class BaseBot
    {
        public string Name;
        public int PlayerIndex;
        public BotDifficulty Difficulty;
        protected Random random;
        private static string[] names = { "Chrissi", "Lucas", "Dave", "Tobi", "Domi", "Michi", "Heider", "Steger", "Nadja", "Sarah", "Juliane", "Rebecca" };

        public static string GetRandomBotName(int random)
        {
            return "Bot " + names[random % names.Length];
        }

        public static string GetBotString(BotDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BotDifficulty.EASY:
                    return "BotEasy";
                case BotDifficulty.MEDIUM:
                    return "BotMedium";
                case BotDifficulty.HARD:
                    return "BotHard";
                default:
                    return "BotDead";
            }
        }

        public static string GetFriendlyBotString(BotDifficulty difficulty, Android.Content.Res.Resources resource)
        {
            switch (difficulty)
            {
                case BotDifficulty.EASY:
                    return resource.GetString(Resource.String.bot_easy);
                case BotDifficulty.MEDIUM:
                    return resource.GetString(Resource.String.bot_medium);
                case BotDifficulty.HARD:
                    return resource.GetString(Resource.String.bot_hard);
                default:
                    return "BotDead";
            }
        }

    }

    public enum BotDifficulty
    {
        EASY,
        MEDIUM,
        HARD
    }
}