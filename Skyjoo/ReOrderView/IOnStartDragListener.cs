using AndroidX.RecyclerView.Widget;

namespace Skyjoo.ReOrderView
{
    public interface IOnStartDragListener
    {
        /**
     * Called when a view is requesting a start of a drag.
     *
     * @param viewHolder The holder of the view to drag.
     */
        void OnStartDrag(RecyclerView.ViewHolder viewHolder);
    }
}
