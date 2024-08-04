using System.Collections.Generic;

namespace Monocle
{
    public class TagLists
    {
        private List<Entity>[] lists;
        private bool[] unsorted;
        private bool areAnyUnsorted;

        internal TagLists()
        {
            lists = new List<Entity>[BitTag.TotalTags];
            unsorted = new bool[BitTag.TotalTags];
            for (int index = 0; index < lists.Length; ++index)
                lists[index] = new List<Entity>();
        }

        public List<Entity> this[int index] => lists[index];

        internal void MarkUnsorted(int index)
        {
            areAnyUnsorted = true;
            unsorted[index] = true;
        }

        internal void UpdateLists()
        {
            if (!areAnyUnsorted)
                return;
            for (int index = 0; index < lists.Length; ++index)
            {
                if (unsorted[index])
                {
                    lists[index].Sort(EntityList.CompareDepth);
                    unsorted[index] = false;
                }
            }
            areAnyUnsorted = false;
        }

        internal void EntityAdded(Entity entity)
        {
            for (int index = 0; index < BitTag.TotalTags; ++index)
            {
                if (entity.TagCheck(1 << index))
                {
                    this[index].Add(entity);
                    areAnyUnsorted = true;
                    unsorted[index] = true;
                }
            }
        }

        internal void EntityRemoved(Entity entity)
        {
            for (int index = 0; index < BitTag.TotalTags; ++index)
            {
                if (entity.TagCheck(1 << index))
                    lists[index].Remove(entity);
            }
        }
    }
}
