using Android.Widget;
using System;

namespace Skyjoo.GameLogic
{
    public partial class SkyjoBoard
    {
        /// <summary>
        /// Handles clicks on CurrentCard
        /// </summary>
        private void onCurrentCardClick(object sender, EventArgs e)
        {
            if (gameEnded)
            {
                showWinner();
            }
        }

        /// <summary>
        /// Handles clicks on Stack
        /// </summary>
        private void onStackClick(object sender, EventArgs e)
        {
            if (CurrentDisplayedPlayerIndex != OwnPlayerIndex) return;
            if (SkyjoCardStack.Count == 0) return;
            if (Players[CurrentDisplayedPlayerIndex].PlayingField.CurrentCard.IsPlaceholder)
                validateMove(FieldUpdateType.StackToCurrent);
        }

        /// <summary>
        /// Handles clicks on ReverseStack
        /// </summary>
        private void onReverseStackClick(object sender, EventArgs e)
        {
            if (CurrentDisplayedPlayerIndex != OwnPlayerIndex) return;
            if (Players[CurrentDisplayedPlayerIndex].PlayingField.CurrentCard.IsPlaceholder)
            {
                if (ReverseSkyjoCardStack.GetTopImageId() == SkyjoCard.GetPlaceholderImageId()) return;
                if (ReverseSkyjoCardStack.Count == 0) return;
                validateMove(FieldUpdateType.ReversedStackToCurrent);
            }
            else
                validateMove(FieldUpdateType.CurrentToReverseStack);
        }

        /// <summary>
        /// Handles clicks on fieldCards
        /// </summary>
        private void onFieldCardClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (CurrentDisplayedPlayerIndex != OwnPlayerIndex) return;
            if (Players[CurrentDisplayedPlayerIndex].PlayingField.FieldCards[e.Position].IsPlaceholder) return;
            if (!Players[CurrentDisplayedPlayerIndex].PlayingField.CurrentCard.IsPlaceholder)
                validateMove(FieldUpdateType.CurrentToField, e.Position);
            else
            {
                if (!Players[CurrentDisplayedPlayerIndex].PlayingField.FieldCards[e.Position].IsVisible)
                    validateMove(FieldUpdateType.RevealOnField, e.Position);
            }
        }

        /// <summary>
        /// Handles click on New Game button
        /// </summary>
        private void onRestartGameButtonClick(object sender, EventArgs e)
        {
            activity.RestartGame();
        }

        /// <summary>
        /// Displays the selected players field
        /// </summary>
        private void onPlayerSpinnerSelect(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            setDisplayToPlayer(e.Position);
        }
    }
}