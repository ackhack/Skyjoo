using System;
using System.Collections.Generic;

namespace Skyjoo.GameLogic
{
    public partial class SkyjoBoard
    {
        public SkyjoPlayer[] Players;
        public SkyjoPlayerField DisplayedField;
        public int CurrentDisplayedPlayerIndex;
        public int OwnPlayerIndex;
        public event EventHandler<FieldUpdateEventArgs> FieldUpdated;

        public SkyjoCardStack SkyjoCardStack;
        public ReverseSkyjoCardStack ReverseSkyjoCardStack;
        public int CurrentActivePlayerIndex;

        private int roundStarter;
        private bool ignoreRules;
        private FieldUpdateType lastMove;
        private bool gameEnded;
        private bool gameStarted;
        private int lastRoundStarter;
        private GameActivity activity;
        private SkyjoBoardGridAdapter cardAdapter;

        public SkyjoBoard(GameActivity activity, SkyjoGameInfo skyjoGameInfo, int ownPlayerIndex)
        {
            ignoreRules = false;
#if DEBUG
            //ignoreRules = true;
#endif
            OwnPlayerIndex = ownPlayerIndex;
            this.activity = activity;
            initGame(skyjoGameInfo.StackSeed, skyjoGameInfo.Players);
        }

        /// <summary>
        /// Initializes the game
        /// </summary>
        private void initGame(int stackSeed, Dictionary<string, string> players)
        {
            gameEnded = false;
            gameStarted = false;
            lastMove = FieldUpdateType.RevealOnField;
            lastRoundStarter = -10;
            CurrentActivePlayerIndex = 0;
            CurrentDisplayedPlayerIndex = -1;
            roundStarter = 0;
            ReverseSkyjoCardStack = new ReverseSkyjoCardStack();
            SkyjoCardStack = SkyjoCardStack.GetCardStack(stackSeed);
            createPlayers(players);
            initUI();
            showToast(activity.Resources.GetString(Resource.String.game_start));
        }

        /// <summary>
        /// Creates the players
        /// </summary>
        /// <param name="players">The players (ip, name)</param>
        private void createPlayers(Dictionary<string, string> players)
        {
            Players = new SkyjoPlayer[players.Count];
            int n = 0;
            foreach (var pair in players)
            {
                Players[n] = new SkyjoPlayer(pair.Key, pair.Value);
                Players[n].PlayingField.FieldCards = new SkyjoCard[12];
                for (int i = 0; i < 12; i++)
                {
                    Players[n].PlayingField.FieldCards[i] = SkyjoCardStack.GetTopCard();
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
            roundStarter++;
            CurrentActivePlayerIndex = roundStarter;
            SkyjoCardStack = SkyjoCardStack.GetCardStack(stackSeed);
            ReverseSkyjoCardStack.Clear();
            foreach (SkyjoPlayer player in Players)
            {
                player.InitRevealedFields = 2;
                player.PlayingField.CurrentCard = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                player.PlayingField.FieldCards = new SkyjoCard[12];
                for (int i = 0; i < 12; i++)
                {
                    player.PlayingField.FieldCards[i] = SkyjoCardStack.GetTopCard();
                }
            }

            initUI();
            showToast(activity.Resources.GetString(Resource.String.game_start));
        }

        /// <summary>
        /// Move to next Player
        /// </summary>
        private void nextPlayer()
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
                CurrentActivePlayerIndex++;
                if (CurrentActivePlayerIndex == Players.Length) CurrentActivePlayerIndex = 0;
            }
            while (!Players[CurrentActivePlayerIndex].Active);

            if (lastRoundStarter == CurrentActivePlayerIndex)
                endGame();
            else if (CurrentActivePlayerIndex == OwnPlayerIndex)
                showToast(activity.Resources.GetString(Resource.String.game_your_turn));
        }

        /// <summary>
        /// Validates a move and sends it if valid
        /// </summary>
        /// <param name="type">What type of move</param>
        /// <param name="fieldIndex">The index of a fieldCard if needed</param>
        private void validateMove(FieldUpdateType type, int fieldIndex = -1)
        {
            if (ignoreRules)
            {
                FieldUpdated?.Invoke(this, new FieldUpdateEventArgs(OwnPlayerIndex, fieldIndex, type));
                lastMove = type;
                return;
            }

            if (gameEnded) return;
            if (type == FieldUpdateType.ShuffleStack) return;

            if (!hasGameStarted())
            {
                if (type == FieldUpdateType.RevealOnField && Players[OwnPlayerIndex].InitRevealedFields > 0)
                {
                    FieldUpdated?.Invoke(this, new FieldUpdateEventArgs(OwnPlayerIndex, fieldIndex, type));
                }
                return;
            }

            if (CurrentActivePlayerIndex != OwnPlayerIndex) return;

            if ((SkyjoMoveRules.IsEndingMove(lastMove) && SkyjoMoveRules.IsStartingMove(type)) || SkyjoMoveRules.IsValidNextNove(type, lastMove))
            {
                FieldUpdated?.Invoke(this, new FieldUpdateEventArgs(OwnPlayerIndex, fieldIndex, type));
                lastMove = type;
            }

            if (SkyjoCardStack.Count == 0)
            {
                FieldUpdated?.Invoke(this, new FieldUpdateEventArgs(OwnPlayerIndex, -1, FieldUpdateType.ShuffleStack));
            }
        }

        /// <summary>
        /// Adds placeholders to the field if a row has only cards of the same number
        /// </summary>
        private void clearRows(int playerIndex, bool ignoreVisibility = false)
        {
            for (int i = 0; i < 4; i++)
            {
                if (ignoreVisibility || Players[playerIndex].PlayingField.FieldCards[i].IsVisible &&
                    Players[playerIndex].PlayingField.FieldCards[i + 4].IsVisible &&
                    Players[playerIndex].PlayingField.FieldCards[i + 8].IsVisible)
                    if (Players[playerIndex].PlayingField.FieldCards[i].Number ==
                        Players[playerIndex].PlayingField.FieldCards[i + 4].Number &&
                        Players[playerIndex].PlayingField.FieldCards[i].Number ==
                        Players[playerIndex].PlayingField.FieldCards[i + 8].Number &&
                        !Players[playerIndex].PlayingField.FieldCards[i].IsPlaceholder)
                    {
                        Players[playerIndex].PlayingField.FieldCards[i] = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                        Players[playerIndex].PlayingField.FieldCards[i + 4] = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                        Players[playerIndex].PlayingField.FieldCards[i + 8] = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                    }
            }
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
            showToast(activity.Resources.GetString(Resource.String.game_last_round_start));
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
            showToast(activity.Resources.GetString(Resource.String.game_end));
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