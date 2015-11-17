namespace Merge.Common
{
    /// <summary>
    /// Line change state.
    /// </summary>
    public enum ChangeState
    {
        None,
        WithoutChanges,
        Modified,
        Add,
        Delete,
        Conflict
    }
}