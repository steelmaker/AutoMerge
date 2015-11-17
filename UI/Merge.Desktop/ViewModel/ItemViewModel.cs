using GalaSoft.MvvmLight;
using Merge.Common;

namespace Merge.Desktop.ViewModel
{
    /// <summary>
    /// Represent info about merged item.
    /// </summary>
    public class ItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the 
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Gets or sets the original.
        /// </summary>
        public string Original { get; set; }

        /// <summary>
        /// Gets or sets the first.
        /// </summary>
        public string First { get; set; }

        /// <summary>
        /// Gets or sets the second.
        /// </summary>
        public string Second { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Gets or sets the original state.
        /// </summary>
        public ChangeState OriginalState { get; set; }

        /// <summary>
        /// Gets or sets the first state.
        /// </summary>
        public ChangeState FirstState { get; set; }

        /// <summary>
        /// Gets or sets the second state.
        /// </summary>
        public ChangeState SecondState { get; set; }

        /// <summary>
        /// Gets or sets the result state.
        /// </summary>
        public ChangeState ResultState { get; set; }
    }
}