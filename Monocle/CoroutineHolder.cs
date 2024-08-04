using System.Collections;
using System.Collections.Generic;

namespace Monocle
{
    public class CoroutineHolder : Component
    {
        private List<CoroutineData> coroutineList;
        private HashSet<CoroutineData> toRemove;
        private int nextID;
        private bool isRunning;

        public CoroutineHolder()
            : base(true, false)
        {
            coroutineList = new List<CoroutineData>();
            toRemove = new HashSet<CoroutineData>();
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
                        coroutineList[index].Data.Push(enumerator.Current as IEnumerator);
                }
                else
                {
                    coroutineList[index].Data.Pop();
                    if (coroutineList[index].Data.Count == 0)
                        toRemove.Add(coroutineList[index]);
                }
            }
            isRunning = false;
            if (toRemove.Count <= 0)
                return;
            foreach (CoroutineData coroutineData in toRemove)
                coroutineList.Remove(coroutineData);
            toRemove.Clear();
        }

        public void EndCoroutine(int id)
        {
            foreach (CoroutineData coroutine in coroutineList)
            {
                if (coroutine.ID == id)
                {
                    if (isRunning)
                    {
                        toRemove.Add(coroutine);
                        break;
                    }
                    coroutineList.Remove(coroutine);
                    break;
                }
            }
        }

        public int StartCoroutine(IEnumerator functionCall)
        {
            CoroutineData coroutineData = new CoroutineData(nextID++, functionCall);
            coroutineList.Add(coroutineData);
            return coroutineData.ID;
        }

        public static IEnumerator WaitForFrames(int frames)
        {
            for (int i = 0; i < frames; ++i)
                yield return 0;
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
