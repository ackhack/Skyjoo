using Android.Content.Res;
using Android.Views;
using Android.Widget;
using Skyjoo.Dependency;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace Skyjoo.GameLogic
{
    public partial class SkyjoBoard
    {
        /// <summary>
        /// Inits the UI
        /// </summary>
        private void initUI(int fieldWidth)
        {
            activity.RunOnUiThread(() =>
            {
                DisplayedField = Players[OwnPlayerIndex].PlayingField;
                var spinner = activity.FindViewById<Spinner>(Resource.Id.spinner_player_picker);

                var nameList = new List<string>();
                var buttons = new List<Button>();
                foreach (SkyjoPlayer player in Players)
                {
                    nameList.Add(player.Name);
                }
                var adapter = new SkyjoBoardPlayerAdapter(buttons.ToArray());
                spinner.Adapter = new ArrayAdapter(activity,Resource.Layout.playerPickerListItem,nameList.ToArray());
                spinner.SetSelection(OwnPlayerIndex);
                spinner.ItemSelected += onPlayerSpinnerSelect;
                adapter.NotifyDataSetChanged();

                var sizeS = (int)DeviceDisplay.MainDisplayInfo.Width >> 2;
                var rStack = activity.FindViewById<ImageView>(Resource.Id.img_reverseStackCard);
                rStack.SetScaleType(ImageView.ScaleType.CenterInside);
                rStack.LayoutParameters.Height = sizeS;
                rStack.LayoutParameters.Width = sizeS;
                rStack.Click += onReverseStackClick;

                var stack = activity.FindViewById<ImageView>(Resource.Id.img_stackCard);
                stack.SetScaleType(ImageView.ScaleType.CenterInside);
                stack.LayoutParameters.Height = sizeS;
                stack.LayoutParameters.Width = sizeS;
                stack.Click += onStackClick;

                var sizeL = (int)(DeviceDisplay.MainDisplayInfo.Width) >> 1;
                var currCardView = activity.FindViewById<ImageView>(Resource.Id.img_currentCard);
                currCardView.SetScaleType(ImageView.ScaleType.CenterInside);
                currCardView.LayoutParameters.Height = sizeL;
                currCardView.LayoutParameters.Width = sizeL;
                currCardView.Click += onCurrentCardClick;

                var gridView = activity.FindViewById<GridView>(Resource.Id.view_cards);
                gridView.NumColumns = fieldWidth;
                cardAdapter = new SkyjoBoardGridAdapter(activity, this);
                gridView.Adapter = cardAdapter;
                gridView.ItemClick += onFieldCardClick;

                setDisplayToPlayer(OwnPlayerIndex);
            });
        }

        /// <summary>
        /// Updates the UI components
        /// </summary>
        private void updateUI()
        {
            activity.RunOnUiThread(() =>
            {
                var rStack = activity.FindViewById<ImageView>(Resource.Id.img_reverseStackCard);
                rStack.SetImageResource(ReverseSkyjoCardStack.GetTopImageId());
                rStack.Invalidate();
                var Stack = activity.FindViewById<ImageView>(Resource.Id.img_stackCard);
                Stack.SetImageResource(SkyjoCardStack.GetTopImageId());
                Stack.Invalidate();
                var currCard = activity.FindViewById<ImageView>(Resource.Id.img_currentCard);
                currCard.SetImageResource(DisplayedField.CurrentCard.GetImageId());
                currCard.Invalidate();
                cardAdapter.NotifyDataSetChanged();
            });
        }

        /// <summary>
        /// Changes the displayed Player
        /// </summary>
        /// <param name="index">The players index</param>
        private void setDisplayToPlayer(int index)
        {
            if (index >= Players.Length || index == CurrentDisplayedPlayerIndex) return;

            CurrentDisplayedPlayerIndex = index;
            System.Diagnostics.Debug.WriteLine("Changing to " + Players[index].Name);

            DisplayedField = Players[CurrentDisplayedPlayerIndex].PlayingField;

            updateUI();
        }

        /// <summary>
        /// Writes the action to the changelog
        /// </summary>
        /// <param name="type">What update happend</param>
        private void updateChangelog(FieldUpdateType type, int playerIndex)
        {
            string updateText = Players[playerIndex].Name + " ";

            switch (type)
            {
                case FieldUpdateType.RevealOnField:
                    updateText += activity.Resources.GetString(Resource.String.game_reveal_on_field);
                    break;

                case FieldUpdateType.CurrentToField:
                    updateText += activity.Resources.GetString(Resource.String.game_current_to_field);
                    break;

                case FieldUpdateType.StackToCurrent:
                    updateText += activity.Resources.GetString(Resource.String.game_stack_to_current);
                    break;

                case FieldUpdateType.ReversedStackToCurrent:
                    updateText += activity.Resources.GetString(Resource.String.game_reversestack_to_current);
                    break;

                case FieldUpdateType.CurrentToReverseStack:
                    updateText += activity.Resources.GetString(Resource.String.game_current_to_reversestack);
                    break;
                case FieldUpdateType.ShuffleStack:
                default:
                    //ignore as it should never trigger here
                    return;
            }

            writeToChangelog(updateText);
        }

        /// <summary>
        /// Writes text to changelog
        /// </summary>
        /// <param name="text">The text to write</param>
        private void writeToChangelog(string text)
        {
            activity.RunOnUiThread(() =>
            {
                var view = activity.FindViewById<TextView>(Resource.Id.textChangelog);
                view.Text = text + "\n" + view.Text;
                view.Invalidate();
            });
        }

        /// <summary>
        /// Displays the results of the game
        /// </summary>
        /// <param name="results">An array of string to display</param>
        private void displayResultsList(string[] results)
        {
            activity.RunOnUiThread(() =>
            {
                activity.SetContentView(Resource.Layout.results_layout);
                var listView = activity.FindViewById<ListView>(Resource.Id.winnerView);
                listView.Adapter = new ArrayAdapter<string>(activity, Resource.Layout.winnerListItem, results);
                listView.Invalidate();
                var btn = activity.FindViewById<Button>(Resource.Id.btnRestartGame);
                if (DependencyClass.Server == null)
                    btn.Visibility = ViewStates.Invisible;
                else
                    btn.Click += onRestartGameButtonClick;
            });
        }

        /// <summary>
        /// Shows a message to the user
        /// </summary>
        /// <param name="message">The message</param>
        public void ShowToast(string message)
        {
            writeToChangelog(message);
            activity.RunOnUiThread(() =>
            {
                Toast.MakeText(activity, message, ToastLength.Short).Show();
            });
        }

        private void updateCurrentPlayerDisplay(int playerIndex)
        {
            activity.RunOnUiThread(() => {
                activity.FindViewById<TextView>(Resource.Id.textCurrentPlayer).Text = string.Format(activity.Resources.GetString(Resource.String.current_player_display), Players[playerIndex].Name);
            });
        }
    }
}