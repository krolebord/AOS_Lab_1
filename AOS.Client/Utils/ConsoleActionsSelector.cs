using System.Collections.Generic;
using System.Linq;

namespace AOS.Client.Utils
{
    public class ItemSelector<T>
    {
        public IReadOnlyList<T> Items { get; }

        public int SelectedIndex { get; private set; }

        public ItemSelector(IEnumerable<T> actions, int initialIndex = 0)
        {
            SelectedIndex = initialIndex;
            Items = actions.ToList();
        }

        public T GetSelected()
        {
            return Items[SelectedIndex];
        }

        public bool IsSelected(T item)
        {
            return item!.Equals(GetSelected());
        }

        public void SelectPrevious()
        {
            SelectedIndex--;

            if (SelectedIndex < 0)
            {
                SelectedIndex = Items.Count - 1;
            }
        }

        public void SelectNext()
        {
            SelectedIndex++;

            if (SelectedIndex >= Items.Count)
            {
                SelectedIndex = 0;
            }
        }
    }
}
