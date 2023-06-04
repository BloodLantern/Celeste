using System;
using System.Collections;

namespace Monocle
{
    public class StateMachine : Component
    {
        private int state;
        private readonly Action[] begins;
        private readonly Func<int>[] updates;
        private readonly Action[] ends;
        private readonly Func<IEnumerator>[] coroutines;
        private readonly Coroutine currentCoroutine;
        public bool ChangedStates;
        public bool Log;
        public bool Locked;

        public int PreviousState { get; private set; }

        public StateMachine(int maxStates = 10)
            : base(true, false)
        {
            PreviousState = state = -1;
            begins = new Action[maxStates];
            updates = new Func<int>[maxStates];
            ends = new Action[maxStates];
            coroutines = new Func<IEnumerator>[maxStates];
            currentCoroutine = new Coroutine();
            currentCoroutine.RemoveOnComplete = false;
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            if (Entity.Scene == null || state != -1)
                return;
            State = 0;
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            if (state != -1)
                return;
            State = 0;
        }

        public int State
        {
            get => state;
            set
            {
                if (Locked || state == value)
                    return;
                if (Log)
                    Calc.Log("Enter State " + value + " (leaving " + state + ")");
                ChangedStates = true;
                PreviousState = state;
                state = value;
                if (PreviousState != -1 && ends[PreviousState] != null)
                {
                    if (Log)
                        Calc.Log("Calling End " + PreviousState);
                    ends[PreviousState]();
                }
                if (begins[state] != null)
                {
                    if (Log)
                        Calc.Log("Calling Begin " + state);
                    begins[state]();
                }
                if (coroutines[state] != null)
                {
                    if (Log)
                        Calc.Log("Starting Coroutine " + state);
                    currentCoroutine.Replace(coroutines[state]());
                }
                else
                    currentCoroutine.Cancel();
            }
        }

        public void ForceState(int toState)
        {
            if (state != toState)
            {
                State = toState;
            }
            else
            {
                if (Log)
                    Calc.Log("Enter State " + toState + " (leaving " + state + ")");
                ChangedStates = true;
                PreviousState = state;
                state = toState;
                if (PreviousState != -1 && ends[PreviousState] != null)
                {
                    if (Log)
                        Calc.Log("Calling End " + state);
                    ends[PreviousState]();
                }
                if (begins[state] != null)
                {
                    if (Log)
                        Calc.Log("Calling Begin " + state);
                    begins[state]();
                }
                if (coroutines[state] != null)
                {
                    if (Log)
                        Calc.Log("Starting Coroutine " + state);
                    currentCoroutine.Replace(coroutines[state]());
                }
                else
                    currentCoroutine.Cancel();
            }
        }

        public void SetCallbacks(
            int state,
            Func<int> onUpdate,
            Func<IEnumerator> coroutine = null,
            Action begin = null,
            Action end = null)
        {
            updates[state] = onUpdate;
            begins[state] = begin;
            ends[state] = end;
            coroutines[state] = coroutine;
        }

        public void ReflectState(Entity from, int index, string name)
        {
            updates[index] = (Func<int>) Calc.GetMethod<Func<int>>(from, name + "Update");
            begins[index] = (Action) Calc.GetMethod<Action>(from, name + "Begin");
            ends[index] = (Action) Calc.GetMethod<Action>(from, name + "End");
            coroutines[index] = (Func<IEnumerator>) Calc.GetMethod<Func<IEnumerator>>(from, name + "Coroutine");
        }

        public override void Update()
        {
            ChangedStates = false;
            if (updates[state] != null)
                State = updates[state]();
            if (!currentCoroutine.Active)
                return;
            currentCoroutine.Update();
            if (ChangedStates || !Log || !currentCoroutine.Finished)
                return;
            Calc.Log("Finished Coroutine " + state);
        }

        public static implicit operator int(StateMachine s) => s.state;

        public void LogAllStates()
        {
            for (int index = 0; index < updates.Length; ++index)
                LogState(index);
        }

        public void LogState(int index) => Calc.Log("State " + index + ": " + (updates[index] != null ? "U" : (object)"") + (begins[index] != null ? "B" : (object)"") + (ends[index] != null ? "E" : (object)"") + (coroutines[index] != null ? "C" : (object)""));
    }
}
