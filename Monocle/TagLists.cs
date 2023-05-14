// Decompiled with JetBrains decompiler
// Type: Monocle.TagLists
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Collections.Generic;

namespace Monocle
{
    public class TagLists
    {
        private readonly List<Entity>[] lists;
        private readonly bool[] unsorted;
        private bool areAnyUnsorted;

        internal TagLists()
        {
            lists = new List<Entity>[BitTag.TotalTags];
            unsorted = new bool[BitTag.TotalTags];
            for (int index = 0; index < lists.Length; ++index)
            {
                lists[index] = new List<Entity>();
            }
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
            {
                return;
            }

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
                {
                    _ = lists[index].Remove(entity);
                }
            }
        }
    }
}
