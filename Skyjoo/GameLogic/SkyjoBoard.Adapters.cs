using Android.App;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace Skyjoo.GameLogic
{
    public partial class SkyjoBoard
    {
        public class SkyjoBoardGridAdapter : BaseAdapter
        {
            private Activity mContext;

            public SkyjoBoard board;

            public SkyjoBoardGridAdapter(Activity mainActivity, SkyjoBoard board)
            {
                this.mContext = mainActivity;
                this.board = board;
            }

            override public int Count
            {
                get
                {
                    return board.DisplayedField.FieldCards.Length;
                }
            }

            override public Java.Lang.Object GetItem(int position)
            {
                return board.DisplayedField.FieldCards[position].GetImageId();
            }

            override public long GetItemId(int position)
            {
                return 0;
            }

            override public View GetView(int position, View convertView, ViewGroup parent)
            {
                ImageView imageView = new ImageView(mContext);
                imageView.SetImageResource(board.DisplayedField.FieldCards[position].GetImageId());
                imageView.SetScaleType(ImageView.ScaleType.CenterInside);
                var size = (int)(DeviceDisplay.MainDisplayInfo.Width / 4 - 10);
                imageView.LayoutParameters = new GridView.LayoutParams(size, size);
                return imageView;
            }
        }

        public class SkyjoBoardPlayerAdapter : BaseAdapter
        {
            public Button[] buttons;

            public SkyjoBoardPlayerAdapter(Button[] buttons)
            {
                this.buttons = buttons;
            }

            override public int Count
            {
                get
                {
                    return buttons.Length;
                }
            }

            override public Java.Lang.Object GetItem(int position)
            {
                return buttons[position];
            }

            override public long GetItemId(int position)
            {
                return 0;
            }

            override public View GetView(int position, View convertView, ViewGroup parent)
            {
                return buttons[position];
            }
        }
    }
}