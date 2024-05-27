using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class Switch : Component
    {
        public bool GroundReset;
        public Action OnActivate;
        public Action OnDeactivate;
        public Action OnFinish;
        public Action OnStartFinished;

        public Switch(bool groundReset)
            : base(true, false)
        {
            GroundReset = groundReset;
        }

        public bool Activated { get; private set; }

        public bool Finished { get; private set; }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            if (!Switch.CheckLevelFlag(SceneAs<Level>()))
                return;
            StartFinished();
        }

        public override void Update()
        {
            base.Update();
            if (!GroundReset || !Activated || Finished)
                return;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || !entity.OnGround())
                return;
            Deactivate();
        }

        public bool Activate()
        {
            if (Finished || Activated)
                return false;
            Activated = true;
            if (OnActivate != null)
                OnActivate();
            return Switch.FinishedCheck(SceneAs<Level>());
        }

        public void Deactivate()
        {
            if (Finished || !Activated)
                return;
            Activated = false;
            if (OnDeactivate == null)
                return;
            OnDeactivate();
        }

        public void Finish()
        {
            Finished = true;
            if (OnFinish == null)
                return;
            OnFinish();
        }

        public void StartFinished()
        {
            if (Finished)
                return;
            Finished = Activated = true;
            if (OnStartFinished == null)
                return;
            OnStartFinished();
        }

        public static bool Check(Scene scene)
        {
            Switch component = scene.Tracker.GetComponent<Switch>();
            return component != null && component.Finished;
        }

        private static bool FinishedCheck(Level level)
        {
            foreach (Switch component in level.Tracker.GetComponents<Switch>())
            {
                if (!component.Activated)
                    return false;
            }
            foreach (Switch component in level.Tracker.GetComponents<Switch>())
                component.Finish();
            return true;
        }

        public static bool CheckLevelFlag(Level level) => level.Session.GetFlag("switches_" + level.Session.Level);

        public static void SetLevelFlag(Level level) => level.Session.SetFlag("switches_" + level.Session.Level);
    }
}
