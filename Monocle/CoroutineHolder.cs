// Decompiled with JetBrains decompiler
// Type: Monocle.CoroutineHolder
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Collections;
using System.Collections.Generic;

namespace Monocle
{
    public class CoroutineHolder : Component
    {
        private readonly List<CoroutineHolder.CoroutineData> coroutineList;
        private readonly HashSet<CoroutineHolder.CoroutineData> toRemove;
        private int nextID;
        private bool isRunning;

        public CoroutineHolder()
            : base(true, false)
        {
            coroutineList = new List<CoroutineHolder.CoroutineData>();
            toRemove = new HashSet<CoroutineHolder.CoroutineData>();
        }

        public override void Update()
        {
            isRunning = true;
            for (int index = 0; index < coroutineList.Count; ++index)
            {
                IEnumerator enumerator = coroutineList[index].Data.Peek();
                if (enumerator.MoveNext())
                {
                    if (enumerator.Current is IEnumerator)
                    {
                        coroutineList[index].Data.Push(enumerator.Current as IEnumerator);
                    }
                }
                else
                {
                    _ = coroutineList[index].Data.Pop();
                    if (coroutineList[index].Data.Count == 0)
                    {
                        _ = toRemove.Add(coroutineList[index]);
                    }
                }
            }
            isRunning = false;
            if (toRemove.Count <= 0)
            {
                return;
            }

            foreach (CoroutineHolder.CoroutineData coroutineData in toRemove)
            {
                _ = coroutineList.Remove(coroutineData);
            }

            toRemove.Clear();
        }

        public void EndCoroutine(int id)
        {
            foreach (CoroutineHolder.CoroutineData coroutine in coroutineList)
            {
                if (coroutine.ID == id)
                {
                    if (isRunning)
                    {
                        _ = toRemove.Add(coroutine);
                        break;
                    }
                    _ = coroutineList.Remove(coroutine);
                    break;
                }
            }
        }

        public int StartCoroutine(IEnumerator functionCall)
        {
            CoroutineHolder.CoroutineData coroutineData = new(nextID++, functionCall);
            coroutineList.Add(coroutineData);
            return coroutineData.ID;
        }

        public static IEnumerator WaitForFrames(int frames)
        {
            for (int i = 0; i < frames; ++i)
            {
                yield return 0;
            }
        }

        private class CoroutineData
        {
            public int ID;
            public Stack<IEnumerator> Data;

            public CoroutineData(int id, IEnumerator functionCall)
            {
                ID = id;
                Data = new Stack<IEnumerator>();
                Data.Push(functionCall);
            }
        }
    }
}
