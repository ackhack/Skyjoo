using Skyjoo.GameLogic.Bots;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Skyjoo.GameLogic
{
    public partial class SkyjoBoard
    {
        //Game Vars
        public SkyjoPlayer[] Players;
        public SkyjoCardStack SkyjoCardStack;
        public ReverseSkyjoCardStack ReverseSkyjoCardStack;
        public int CurrentActivePlayerIndex;
        public int FieldWidth;
        public int FieldHeight;
        private int roundStarter;
        private bool gameEnded;
        private bool gameStarted;
        private int lastRoundStarter;


        public SkyjoPlayerField DisplayedField;
        public int CurrentDisplayedPlayerIndex;
        public int OwnPlayerIndex;
        public event EventHandler<FieldUpdateEventArgs> FieldUpdated;
        private bool isHostGame;
        private FieldUpdateType lastMove;
        private GameActivity activity;
        private SkyjoBoardGridAdapter cardAdapter;

        //Dev Vars
        private bool ignoreRules = false;
        private bool debugBots = false;

        public SkyjoBoard(GameActivity activity, int ownPlayerIndex)
        {
#if DEBUG
            //ignoreRules = true;
            debugBots = true;
#endif
            OwnPlayerIndex = ownPlayerIndex;
            this.activity = activity;
            isHostGame = Dependency.DependencyClass.Server != null;
            //TODO Try to resend message when failed (multi thread sending)
            //TODO reset connection on back (disconnect, clear dependency, destroy activities)
            //TODO new skinpack original card backs
            //TODO better bot add menu
            //TODO hard bot
            //TODO remove from playerList
        }

        /// <summary>
        /// Initializes the game.
        /// </summary>
        /// <param name="skyjoGameInfo">The SkyjoGameInfo provided by the host</param>
        public void InitGame(SkyjoGameInfo skyjoGameInfo)
        {
            FieldWidth = skyjoGameInfo.FieldWidth;
            FieldHeight = skyjoGameInfo.FieldHeight;
            gameEnded = false;
            gameStarted = false;
            lastMove = FieldUpdateType.RevealOnField;
            lastRoundStarter = -10;
            CurrentActivePlayerIndex = 0;
            CurrentDisplayedPlayerIndex = -1;
            roundStarter = 0;

            ReverseSkyjoCardStack = new ReverseSkyjoCardStack();
            SkyjoCardStack = SkyjoCardStack.GetCardStack(skyjoGameInfo.StackSeed);

            createPlayers(skyjoGameInfo.Players, skyjoGameInfo.BotSeed);

            updateCurrentPlayerDisplay(CurrentActivePlayerIndex);
            initUI(FieldWidth);
            ShowToast(activity.Resources.GetString(Resource.String.game_start));

            foreach (var player in Players)
            {
                if (player.IsBot)
                    player.Bot.RevealTwoCards(this);
            }
        }

        /// <summary>
        /// Creates the players
        /// </summary>
        /// <param name="players">The players (ip, name)</param>
        private void createPlayers(Dictionary<string, string> players, int botSeed)
        {
            var random = new Random(botSeed);
            Players = new SkyjoPlayer[players.Count];
            int n = 0;
            foreach (var pair in players)
            {
                Players[n] = new SkyjoPlayer(pair.Key, pair.Value, FieldWidth, FieldHeight);
                int fieldSize = FieldWidth * FieldHeight;
                Players[n].PlayingField.FieldCards = new SkyjoCard[fieldSize];
                for (int i = 0; i < fieldSize; i++)
                {
                    Players[n].PlayingField.FieldCards[i] = SkyjoCardStack.GetTopCard();
                }
                if (pair.Key.StartsWith("Bot"))
                {
                    Players[n].Name = BaseBot.GetRandomBotName(random.Next());
                    Players[n].IsBot = true;

                    if (pair.Key.StartsWith(BaseBot.GetBotString(BotDifficulty.EASY)))
                    {
                        Players[n].Bot = new EasyBot(pair.Key, n, random.Next());
                    }
                    else if (pair.Key.StartsWith(BaseBot.GetBotString(BotDifficulty.MEDIUM)))
                    {
                        Players[n].Bot = new MediumBot(pair.Key, n, random.Next());
                    }
                    else if (pair.Key.StartsWith(BaseBot.GetBotString(BotDifficulty.HARD)))
                    {
                        Players[n].Bot = new HardBot(pair.Key, n, random.Next());
                    }
                }
                n++;
            }
        }

        /// <summary>
        /// Reloads the game interally
        /// </summary>
        /// <param name="stackSeed">The seed to randomise the stack</param>
        private void reloadGame(int stackSeed)
        {
            gameEnded = false;
            gameStarted = false;
            lastMove = FieldUpdateType.RevealOnField;
            lastRoundStarter = -10;
            do
            {
                roundStarter++;
                if (roundStarter == Players.Length) roundStarter = 0;
            }
            while (!Players[roundStarter].IsActive);
            CurrentActivePlayerIndex = roundStarter;
            SkyjoCardStack = SkyjoCardStack.GetCardStack(stackSeed);
            ReverseSkyjoCardStack.Clear();
            int fieldSize = FieldWidth * FieldHeight;
            foreach (SkyjoPlayer player in Players)
            {
                player.InitRevealedFields = 2;
                player.PlayingField.CurrentCard = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                player.PlayingField.FieldCards = new SkyjoCard[fieldSize];
                for (int i = 0; i < fieldSize; i++)
                {
                    player.PlayingField.FieldCards[i] = SkyjoCardStack.GetTopCard();
                }
            }

            initUI(FieldWidth);
            ShowToast(activity.Resources.GetString(Resource.String.game_start));
            foreach (var player in Players)
            {
                if (player.IsBot)
                    player.Bot.RevealTwoCards(this);
            }
        }

        /// <summary>
        /// Move to next Player
        /// </summary>
        public void NextPlayer()
        {
            if (Players.Length == 1)
            {
                if (lastRoundStarter == 0)
                    endGame();
                return;
            }

            if (!hasGameStarted()) return;

            do
            {
                CurrentActivePlayerIndex = (CurrentActivePlayerIndex + 1) % Players.Length;
            }
            while (!Players[CurrentActivePlayerIndex].IsActive);
            updateCurrentPlayerDisplay(CurrentActivePlayerIndex);

            if (lastRoundStarter == CurrentActivePlayerIndex)
                endGame();
            else if (CurrentActivePlayerIndex == OwnPlayerIndex)
            {
                if (!debugBots)
                {
                    ShowToast(activity.Resources.GetString(Resource.String.game_your_turn));
                }
#if DEBUG
                if (debugBots && lastRoundStarter < 0)
                {
                    CurrentActivePlayerIndex = (CurrentActivePlayerIndex + 1) % Players.Length;
                }
#endif
            }

            if (!gameEnded && isHostGame && Players[CurrentActivePlayerIndex].IsBot)
            {
                updateUI();
#if DEBUG
                Thread.Sleep(1000);
#endif
                Players[CurrentActivePlayerIndex].Bot.PlayMove(this);
            }
        }

        /// <summary>
        /// Validates a move and sends it if valid
        /// </summary>
        /// <param name="type">What type of move</param>
        /// <param name="fieldIndex">The index of a fieldCard if needed</param>
        public void ValidateMove(int playerIndex, FieldUpdateType type, int fieldIndex = -1)
        {
            if (ignoreRules)
            {
                FieldUpdated?.Invoke(this, new FieldUpdateEventArgs(playerIndex, fieldIndex, type));
                return;
            }

            if (gameEnded) return;
            if (type == FieldUpdateType.ShuffleStack) return;

            if (!hasGameStarted())
            {
                if (type == FieldUpdateType.RevealOnField && Players[playerIndex].InitRevealedFields > 0)
                {
                    FieldUpdated?.Invoke(this, new FieldUpdateEventArgs(playerIndex, fieldIndex, type));
                }
                return;
            }

            if (CurrentActivePlayerIndex != playerIndex) return;

            if ((SkyjoMoveRules.IsEndingMove(lastMove) && SkyjoMoveRules.IsStartingMove(type)) || SkyjoMoveRules.IsValidNextNove(type, lastMove))
            {
                FieldUpdated?.Invoke(this, new FieldUpdateEventArgs(playerIndex, fieldIndex, type));
                lastMove = type;
            }

            if (SkyjoCardStack.Count == 0)
            {
                FieldUpdated?.Invoke(this, new FieldUpdateEventArgs(playerIndex, -1, FieldUpdateType.ShuffleStack));
            }
        }

        /// <summary>
        /// Adds placeholders to the field if a row has only cards of the same number
        /// </summary>
        private void clearRows(int playerIndex, bool ignoreVisibility = false)
        {
            Players[playerIndex].PlayingField.ClearRows();
        }

        /// <summary>
        /// Checks whether all cards are visible, starts the last Round
        /// </summary>
        private void checkLastRoundStart()
        {
            if (gameEnded || lastRoundStarter >= 0) return;
            foreach (SkyjoCard card in Players[CurrentActivePlayerIndex].PlayingField.FieldCards)
            {
                if (!card.IsVisible) return;
            }
            lastRoundStarter = CurrentActivePlayerIndex;
            ShowToast(activity.Resources.GetString(Resource.String.game_last_round_start));
        }

        /// <summary>
        /// Sets all cards to visible and sets gameEnded to true
        /// </summary>
        private void endGame()
        {
            gameEnded = true;
            for (int i = 0; i < Players.Length; i++)
            {
                foreach (SkyjoCard card in Players[i].PlayingField.FieldCards)
                {
                    card.IsVisible = true;
                }
                clearRows(i, true);
            }
            ShowToast(activity.Resources.GetString(Resource.String.game_end));
        }

        /// <summary>
        /// Checks if everyone has revealed two fields
        /// </summary>
        private bool hasGameStarted()
        {
            if (gameStarted) return true;
            foreach (SkyjoPlayer player in Players)
            {
                if (player.InitRevealedFields > 0) return false;
            }
            gameStarted = true;
            return true;
        }

        /// <summary>
        /// Shows game stats and the winner
        /// </summary>
        private void showWinner()
        {
            //calculate points
            var resultsList = new int[Players.Length][];
            for (int i = 0; i < Players.Length; i++)
            {
                var player = Players[i];

                int points = 0;
                foreach (SkyjoCard card in player.PlayingField.FieldCards)
                {
                    points += card.GetValue();
                }
                resultsList[i] = new int[] { i, points };
            }
            Array.Sort(resultsList, new WinComparer());

            //double points if lastRoundStarter isn't first
            for (int i = 0; i < resultsList.Length; i++)
            {

                if (i != 0 && resultsList[i][0] == lastRoundStarter)
                {
                    resultsList[i][1] *= 2;
                    break;
                }
            }
            Array.Sort(resultsList, new WinComparer());

            //show list to player
            var displayedList = new string[resultsList.Length];
            for (int i = 0; i < resultsList.Length; i++)
            {
                Players[resultsList[i][0]].Points += resultsList[i][1];
                displayedList[i] = (Players[resultsList[i][0]].Name.ToString() + ": ").PadRight(20) + resultsList[i][1].ToString().PadRight(5) + " | " + Players[resultsList[i][0]].Points.ToString().PadLeft(5);
            }
            displayResultsList(displayedList);
        }
    }
}