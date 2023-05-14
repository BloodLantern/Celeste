// Decompiled with JetBrains decompiler
// Type: Monocle.Coroutine
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Collections;
using System.Collections.Generic;

namespace Monocle
{
    public class Coroutine : Component
    {
        public bool RemoveOnComplete = true;
        public bool UseRawDeltaTime;
        private readonly Stack<IEnumerator> enumerators;
        private float waitTimer;
        private bool ended;

        public bool Finished { get; private set; }

        public Coroutine(IEnumerator functionCall, bool removeOnComplete = true)
            : base(true, false)
        {
            enumerators = new Stack<IEnumerator>();
            enumerators.Push(functionCall);
            RemoveOnComplete = removeOnComplete;
        }

        public Coroutine(bool removeOnComplete = true)
            : base(false, false)
        {
            RemoveOnComplete = removeOnComplete;
            enumerators = new Stack<IEnumerator>();
        }

        public override void Update()
        {
            ended = false;
            if (waitTimer > 0.0)
            {
                waitTimer -= UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime;
            }
            else
            {
                if (enumerators.Count <= 0)
                {
                    return;
                }

                IEnumerator enumerator = enumerators.Peek();
                if (enumerator.MoveNext() && !ended)
                {
                    if (enumerator.Current is int)
                    {
                        waitTimer = (int)enumerator.Current;
                    }

                    if (enumerator.Current is float)
                    {
                        waitTimer = (float)enumerator.Current;
                    }
                    else
                    {
                        if (enumerator.Current is not IEnumerator)
                        {
                            return;
                        }

                        enumerators.Push(enumerator.Current as IEnumerator);
                    }
                }
                else
                {
                    if (ended)
                    {
                        return;
                    }

                    _ = enumerators.Pop();
                    if (enumerators.Count != 0)
                    {
                        return;
                    }

                    Finished = true;
                    Active = false;
                    if (!RemoveOnComplete)
                    {
                        return;
                    }

                    RemoveSelf();
                }
            }
        }

        public void Cancel()
        {
            Active = false;
            Finished = true;
            waitTimer = 0.0f;
            enumerators.Clear();
            ended = true;
        }

        public void Replace(IEnumerator functionCall)
        {
            Active = true;
            Finished = false;
            waitTimer = 0.0f;
            enumerators.Clear();
            enumerators.Push(functionCall);
            ended = true;
        }
    }
}
