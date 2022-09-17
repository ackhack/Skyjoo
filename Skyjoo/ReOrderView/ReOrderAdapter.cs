using Android.Views;
using System.Collections.ObjectModel;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using static Android.Views.View;
using System.Collections.Generic;

namespace Skyjoo.ReOrderView
{
    public class ReOrderAdapters : RecyclerView.Adapter, ITemTouchHelperAdapter, IOnLongClickListener
    {
        private readonly ObservableCollection<ReOrderListItem> itemList;
        private readonly ServerActivity serverActivity;
        private ReOrderViewHolder _reOrderViewHolder;

        public ReOrderAdapters(ObservableCollection<ReOrderListItem> list, ServerActivity serverActivity)
        {
            itemList = list;
            this.serverActivity = serverActivity;
        }

        public override int ItemCount => itemList.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as ReOrderViewHolder;
            if (viewHolder == null) return;

            _reOrderViewHolder = viewHolder;
            viewHolder.ResourceName.SetOnLongClickListener(this);
            viewHolder.ResourceName.Text = (itemList[position].Name);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(parent.Context);
            var itemView = inflater.Inflate(Resource.Layout.reorderListItem, parent, false);
            return new ReOrderViewHolder(itemView);
        }

        public void OnItemDismiss(int position)
        {
            var item = itemList[position];
            itemList.Remove(item);
            NotifyItemRemoved(position);
        }

        public bool OnItemMove(int fromPosition, int toPosition)
        {
            var tempPlanResource = itemList[fromPosition];
            itemList[fromPosition] = itemList[toPosition];
            itemList[toPosition] = tempPlanResource;

            serverActivity.PlayerList = itemList;
            NotifyItemMoved(fromPosition, toPosition);
            return true;
        }

        public bool OnLongClick(View v)
        {
            serverActivity.OnStartDrag(_reOrderViewHolder);
            return true;
        }
    }

    public class ReOrderViewHolder : RecyclerView.ViewHolder
    {
        public TextView ResourceName;

        public ReOrderViewHolder(View view) : base(view)
        {
            ResourceName = view.FindViewById<TextView>(Resource.Id.recyclerItemView);
        }
    }

    public class ReOrderListItem
    {
        public string Ip;
        public string Name;
        public ReOrderListItem(string ip, string name)
        {
            this.Ip = ip;
            this.Name = name;
        }
    }
}
