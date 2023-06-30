namespace Celeste
{
    public class ModeProperties
    {
        public string PoemID;
        public string Path;
        /// <summary>
        /// Total chapter side strawberry count.
        /// </summary>
        public int TotalStrawberries;
        /// <summary>
        /// Strawberry count in the first checkpoint (Start)
        /// </summary>
        public int StartStrawberries;
        public EntityData[,] StrawberriesByCheckpoint;
        public CheckpointData[] Checkpoints;
        public MapData MapData;
        public PlayerInventory Inventory;
        public AudioState AudioState;
        public bool IgnoreLevelAudioLayerData;
    }
}
