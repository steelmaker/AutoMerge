using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using DevExpress.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Merge.Common;
using Merge.Common.Services;
using Microsoft.Win32;

namespace Merge.Desktop.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly Merger _merger;
        private ObservableCollection<ItemViewModel> _items;
        private readonly FileManager _fileManager;
        private string _secondPath;
        private string _firstPath;
        private string _originalPath;
        private bool _isBusy;

        /// <summary>
        /// Load original file command.
        /// </summary>
        public RelayCommand LoadOriginalFileCommand { get; private set; }

        /// <summary>
        /// Load first file command.
        /// </summary>
        public RelayCommand LoadFirstFileCommand { get; private set; }

        /// <summary>
        /// Load second file command.
        /// </summary>
        public RelayCommand LoadSecondFileCommand { get; private set; }

        /// <summary>
        /// Save result file command.
        /// </summary>
        public RelayCommand SaveResultFileCommand { get; private set; }

        /// <summary>
        /// Gets or sets is busy.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        /// <summary>
        /// Full path of original file.
        /// </summary>
        public string OriginalPath
        {
            get { return _originalPath; }
            set { Set(ref _originalPath, value); }
        }

        /// <summary>
        /// Full path of first file.
        /// </summary>
        public string FirstPath
        {
            get { return _firstPath; }
            set { Set(ref _firstPath, value); }
        }

        /// <summary>
        /// Full path of second file.
        /// </summary>
        public string SecondPath
        {
            get { return _secondPath; }
            set { Set(ref _secondPath, value); }
        }

        /// <summary>
        /// Collection of lines.
        /// </summary>
        public ObservableCollection<ItemViewModel> Items
        {
            get { return _items; }
            set
            {
                Set(ref _items, value);
                SaveResultFileCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(Merger merger, FileManager fileManager)
        {
            Guard.ArgumentNotNull(merger, "merger");
            Guard.ArgumentNotNull(fileManager, "fileManager");

            _merger = merger;
            _fileManager = fileManager;

            LoadOriginalFileCommand = new RelayCommand(LoadOriginalFile);
            LoadFirstFileCommand = new RelayCommand(LoadFirstFile);
            LoadSecondFileCommand = new RelayCommand(LoadSecondFile);
            SaveResultFileCommand = new RelayCommand(SaveResultFile, () => Items.Any());

            Items = new ObservableCollection<ItemViewModel>();
        }

        private void SaveResultFile()
        {
            SaveFile();
        }

        private void LoadOriginalFile()
        {
            OriginalPath = GetFile();

            FillData();
        }

        private void LoadFirstFile()
        {
            FirstPath = GetFile();

            FillData();
        }

        private void LoadSecondFile()
        {
            SecondPath = GetFile();

            FillData();
        }

        private void FillData()
        {
            if (string.IsNullOrEmpty(OriginalPath) ||
                string.IsNullOrEmpty(FirstPath) ||
                string.IsNullOrEmpty(SecondPath))
            {
                return;
            }

            IsBusy = true;

            try
            {
                _merger.Merge(OriginalPath, FirstPath, SecondPath);
            }
            catch (Exception ex)
            {
                IsBusy = false;

                Debug.WriteLine(ex.StackTrace);

                MessageBox.Show(string.Format("Error: {0}", ex.Message), "Error");

                return;
            }

            var items = new List<ItemViewModel>();

            var max1 = Math.Max(_merger.OriginalFile.Lines.Count, _merger.ResultFile.Lines.Count);
            var max2 = Math.Max(_merger.FirstFile.Lines.Count, _merger.SecondFile.Lines.Count);

            for (var i = 0; i < Math.Max(max1, max2); i++)
            {
                var item = new ItemViewModel
                {
                    RowNumber = i + 1
                };

                if (i < _merger.ResultFile.Lines.Count)
                {
                    var result = _merger.ResultFile.Lines[i];
                    item.Result = result.Text;
                    item.ResultState = result.ChangeState;
                }

                if (i < _merger.OriginalFile.Lines.Count)
                {
                    var original = _merger.OriginalFile.Lines[i];
                    item.Original = original.Text;
                    item.OriginalState = original.ChangeState;
                }

                if (i < _merger.FirstFile.Lines.Count)
                {
                    var first = _merger.FirstFile.Lines[i];
                    item.First = first.Text;
                    item.FirstState = first.ChangeState;
                }

                if (i < _merger.SecondFile.Lines.Count)
                {
                    var second = _merger.SecondFile.Lines[i];
                    item.Second = second.Text;
                    item.SecondState = second.ChangeState;
                }

                items.Add(item);
            }

            Items = new ObservableCollection<ItemViewModel>(items);

            IsBusy = false;
        }

        private string GetFile()
        {
            var fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileName;
            }

            return null;
        }

        private void SaveFile()
        {
            var fileDialog = new SaveFileDialog();

            if (fileDialog.ShowDialog() == true)
            {
                _fileManager.SaveFile(_merger.ResultFile, fileDialog.FileName);
            }
        }
    }
}