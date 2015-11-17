namespace Merge.Common
{
    /// <summary>
    /// Represent data of file line.
    /// </summary>
    public class Line
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the hash.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the original line number.
        /// </summary>
        public int? OriginalLineNumber { get; set; }

        /// <summary>
        /// Gets or sets the change state.
        /// </summary>
        public ChangeState ChangeState { get; set; }

        /// <summary>
        /// Gets line is empty.
        /// </summary>
        public bool IsEmpty { get { return string.IsNullOrEmpty(Hash) && string.IsNullOrEmpty(Text); } }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        public override bool Equals(object obj)
        {
            var line = obj as Line;
            if (line == null)
                return false;

            return Equals(Hash, line.Hash);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}