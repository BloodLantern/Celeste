namespace Monocle
{
    public abstract class VirtualInput
    {
        public VirtualInput() => MInput.VirtualInputs.Add(this);

        public void Deregister() => MInput.VirtualInputs.Remove(this);

        public abstract void Update();

        public enum OverlapBehaviors
        {
            CancelOut,
            TakeOlder,
            TakeNewer,
        }

        public enum ThresholdModes
        {
            LargerThan,
            LessThan,
            EqualTo,
        }
    }
}
