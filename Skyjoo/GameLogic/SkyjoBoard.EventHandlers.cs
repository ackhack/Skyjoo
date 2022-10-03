using System.Threading;

namespace Skyjoo.GameLogic
{
    public partial class SkyjoBoard
    {
        /// <summary>
        /// Handles the field updates recieved from the host
        /// </summary>
        /// <param name="playerIndex">Which playingField to update</param>
        /// <param name="fieldIndex">Which FieldCard to update</param>
        /// <param name="type">What update happend</param>
        public void HandleFieldUpdate(int playerIndex, int fieldIndex, FieldUpdateType type)
        {
            switch (type)
            {
                case FieldUpdateType.RevealOnField:
                    Players[playerIndex].PlayingField.FieldCards[fieldIndex].IsVisible = true;
                    if (!hasGameStarted())
                    {
                        Players[playerIndex].InitRevealedFields--;
                        updateChangelog(type, playerIndex);
                        updateUI();
                        if (hasGameStarted())
                        {
                            if (CurrentActivePlayerIndex == OwnPlayerIndex)
                                ShowToast(activity.Resources.GetString(Resource.String.game_your_turn));
                            else if (isHostGame && Players[CurrentActivePlayerIndex].IsBot)
                            {
                                Players[CurrentActivePlayerIndex].Bot.PlayMove(this);
                            }
                        }
                        return;
                    }
                    break;

                case FieldUpdateType.CurrentToField:
                    Players[playerIndex].PlayingField.FieldCards[fieldIndex].IsVisible = true;
                    ReverseSkyjoCardStack.AddCards(Players[playerIndex].PlayingField.FieldCards[fieldIndex]);
                    Players[playerIndex].PlayingField.FieldCards[fieldIndex] = Players[playerIndex].PlayingField.CurrentCard;
                    Players[playerIndex].PlayingField.CurrentCard = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                    break;

                case FieldUpdateType.StackToCurrent:
                    Players[playerIndex].PlayingField.CurrentCard = SkyjoCardStack.GetTopCard();
                    Players[playerIndex].PlayingField.CurrentCard.IsVisible = true;
                    break;

                case FieldUpdateType.ReversedStackToCurrent:
                    Players[playerIndex].PlayingField.CurrentCard = ReverseSkyjoCardStack.GetTopCard();
                    Players[playerIndex].PlayingField.CurrentCard.IsVisible = true;
                    break;

                case FieldUpdateType.CurrentToReverseStack:
                    ReverseSkyjoCardStack.AddCards(Players[playerIndex].PlayingField.CurrentCard);
                    Players[playerIndex].PlayingField.CurrentCard = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                    break;
                case FieldUpdateType.ShuffleStack:
                    //ignore as it should never trigger here
                    break;
            }

            if (playerIndex != OwnPlayerIndex)
                lastMove = type;

            updateChangelog(type, playerIndex);

            clearRows(CurrentActivePlayerIndex);
            if (SkyjoMoveRules.IsEndingMove(type))
            {
                checkLastRoundStart();
                NextPlayer();
            }

            updateUI();
        }

        /// <summary>
        /// Restarts the game
        /// </summary>
        /// <param name="stackSeed">The seed to randomise the stack</param>
        public void HandleGameRestart(int stackSeed)
        {
            activity.RunOnUiThread(() =>
            {
                activity.SetContentView(Resource.Layout.game_layout);
                reloadGame(stackSeed);
            });
        }

        /// <summary>
        /// Handles Stack shuffle reqeusted
        /// </summary>
        /// <param name="number">The randomness seed</param>
        public void HandleStackShuffle(int number)
        {
            SkyjoCardStack.AddCards(ReverseSkyjoCardStack.GetAllCards().ToArray());
            SkyjoCardStack.Shuffle(number);
            updateUI();
        }

        /// <summary>
        /// Sets a player inactive
        /// </summary>
        /// <param name="ip">The players ip</param>
        public void HandlePlayerLogout(string ip)
        {
            foreach (SkyjoPlayer player in Players)
            {
                if (player.Ip == ip)
                {
                    player.IsActive = false;
                    return;
                }
            }
        }
    }
}