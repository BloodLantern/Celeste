namespace Celeste.Editor
{
    /// <summary>
    /// The type of level. This can be either a <see cref="Level"/> or a <see cref="Filler"/>.
    /// </summary>
    public enum LevelTemplateType
    {
        /// <summary>
        /// A playable level.
        /// </summary>
        Level,
        /// <summary>
        /// Filled with solid blocks.
        /// </summary>
        Filler
    }
}
