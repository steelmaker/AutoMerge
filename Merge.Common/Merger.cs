using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Merge.Common.Services;

namespace Merge.Common
{
    /// <summary>
    /// Merger provides functionality to merge two files to result file based on original file.
    /// In: Original File, First file with changes, Second file with changes.
    /// Out: Result merged file. All result lines contains change state properties.
    /// 
    /// Used Longest Common Subsequence algorithm.
    /// </summary>
    public class Merger
    {
        private readonly FileManager _fileManager = new FileManager();
        private readonly LcsCalculator _lcsCalculator = new LcsCalculator();

        /// <summary>
        /// Gets or sets the original file.
        /// </summary>
        public FileData OriginalFile { get; set; }

        /// <summary>
        /// Gets or sets the first file data.
        /// </summary>
        public FileData FirstFile { get; set; }

        /// <summary>
        /// Gets or sets the second file data.
        /// </summary>
        public FileData SecondFile { get; set; }

        /// <summary>
        /// Gets or sets the result file data.
        /// </summary>
        public FileData ResultFile { get; set; }

        /// <summary>
        /// Merge files.
        /// </summary>
        /// <param name="originalFile">Path of original file.</param>
        /// <param name="firstPath">Path of first file.</param>
        /// <param name="secondPath">Path of second file.</param>
        public void Merge(string originalFile, string firstPath, string secondPath)
        {
            OriginalFile = _fileManager.LoadFile(originalFile);

            SetLineNumbers(OriginalFile);

            FirstFile = _fileManager.LoadFile(firstPath);
            SecondFile = _fileManager.LoadFile(secondPath);

            _lcsCalculator.SetLongestCommonSubsequence(OriginalFile.Lines.ToArray(), FirstFile.Lines.ToArray());
            _lcsCalculator.SetLongestCommonSubsequence(OriginalFile.Lines.ToArray(), SecondFile.Lines.ToArray());

            SetOffset();

            SetOriginalLinesDeleted();

            ResultFile = Compare(FirstFile, SecondFile);
        }

        /// <summary>
        /// Merge files and save result file.
        /// </summary>
        /// <param name="originalFile">Path of original file.</param>
        /// <param name="firstPath">Path of first file.</param>
        /// <param name="secondPath"></param>
        /// <param name="saveResultFilePath">The file path for result file.</param>
        /// <returns>The merged file path.</returns>
        public string Merge(string originalFile, string firstPath, string secondPath, string saveResultFilePath)
        {
            Merge(originalFile, firstPath, secondPath);

            var mergedFilePath = _fileManager.SaveFile(ResultFile, saveResultFilePath);

            return mergedFilePath;
        }

        /// <summary>
        /// Compare two files.
        /// </summary>
        /// <param name="first">The first file.</param>
        /// <param name="second">The second file.</param>
        /// <returns>The result file.</returns>
        public FileData Compare(FileData first, FileData second)
        {
            var result = new FileData
            {
                Lines = new List<Line>()
            };

            var firstCount = first.Lines.Count;
            var secondCount = second.Lines.Count;

            var maxCount = Math.Max(firstCount, secondCount);

            var conflictItem = new Line { Text = "conflict", ChangeState = ChangeState.Conflict };

            for (var i = 0; i < maxCount; i++)
            {
                if (i < firstCount && i < secondCount)
                {
                    var firstItem = first.Lines[i];
                    var secondItem = second.Lines[i];
                    var originalItem = OriginalFile.Lines[i];

                    if (Equals(firstItem, secondItem))
                    {
                        AddEqualsItem(firstItem, secondItem, originalItem, result);
                    }
                    else
                    {
                        if (firstItem.IsEmpty && secondItem.IsEmpty)
                            AddDeleteItem(firstItem, secondItem, result);
                        else
                            AddChangedItem(firstItem, secondItem, originalItem, result, conflictItem);
                    }
                }
                else
                {
                    if (i < firstCount)
                        AddNewItem(first.Lines[i], result);

                    if (i < secondCount)
                        AddNewItem(second.Lines[i], result);
                }
            }

            return result;
        }

        /// <summary>
        /// Set lines offset.
        /// </summary>
        public void SetOffset()
        {
            var originalLines = new List<Line>();
            var firstLines = new List<Line>();
            var secondLines = new List<Line>();

            // sets without changes lines on one level
            SetOnOneLevel(originalLines, firstLines, secondLines);

            AddLinesWithChanges(originalLines, firstLines, secondLines);

            CheckEmptyLines(originalLines, firstLines, secondLines);

            OriginalFile.Lines = originalLines;
            FirstFile.Lines = firstLines;
            SecondFile.Lines = secondLines;
        }

        /// <summary>
        /// Set line numbers.
        /// </summary>
        public void SetLineNumbers(FileData file)
        {
            var i = 0;

            foreach (var item in file.Lines)
                item.OriginalLineNumber = i++;
        }

        /// <summary>
        /// Set original lines is deleted.
        /// </summary>
        public void SetOriginalLinesDeleted()
        {
            for (var i = 0; i < OriginalFile.Lines.Count; i++)
            {
                if (FirstFile.Lines[i].IsEmpty && SecondFile.Lines[i].IsEmpty)
                    OriginalFile.Lines[i].ChangeState = ChangeState.Delete;
            }
        }

        private void AddNewItem(Line item, FileData result)
        {
            item.ChangeState = ChangeState.Add;

            result.Lines.Add(item);
        }

        private void AddChangedItem(Line firstItem, Line secondItem, Line originalItem, FileData result, Line conflictItem)
        {
            var isConflict = firstItem.ChangeState == ChangeState.WithoutChanges &&
                             secondItem.ChangeState == ChangeState.WithoutChanges &&
                             firstItem.OriginalLineNumber != secondItem.OriginalLineNumber;
            if (isConflict)
            {
                result.Lines.Add(conflictItem);
            }
            else if (firstItem.ChangeState == ChangeState.WithoutChanges || firstItem.IsEmpty)
            {

                var isAdded = SetLineState(secondItem, firstItem, originalItem, result, conflictItem);

                if (!isAdded)
                    result.Lines.Add(secondItem);
            }
            else if (secondItem.ChangeState == ChangeState.WithoutChanges || secondItem.IsEmpty)
            {
                var isAddedToResult = SetLineState(firstItem, secondItem, originalItem, result, conflictItem);

                if (!isAddedToResult)
                    result.Lines.Add(firstItem);
            }
            else
            {
                var state = originalItem.IsEmpty ? ChangeState.Add : ChangeState.Modified;

                firstItem.ChangeState =
                    secondItem.ChangeState = state;

                result.Lines.Add(conflictItem);
            }
        }

        private bool SetLineState(Line first, Line second, Line originalItem, FileData result, Line conflictItem)
        {
            if (first.ChangeState != ChangeState.WithoutChanges)
            {
                if (first.IsEmpty)
                {
                    second.ChangeState =
                        first.ChangeState =
                            originalItem.ChangeState = ChangeState.Delete;
                }
                else
                {
                    var isConflict = !originalItem.IsEmpty && second.IsEmpty;

                    if (isConflict)
                    {
                        result.Lines.Add(conflictItem);

                        return true;
                    }

                    var itemIsNew = originalItem.IsEmpty && second.IsEmpty;

                    first.ChangeState = itemIsNew
                        ? ChangeState.Add
                        : first.ChangeState = ChangeState.Modified;
                }
            }
            else if (first.ChangeState == ChangeState.WithoutChanges && second.IsEmpty)
            {
                second.ChangeState = ChangeState.Modified;

                result.Lines.Add(second);

                return true;
            }

            return false;
        }

        private void AddDeleteItem(Line firstItem, Line secondItem, FileData result)
        {
            firstItem.ChangeState =
                secondItem.ChangeState = ChangeState.Delete;

            result.Lines.Add(new Line { ChangeState = ChangeState.Delete });
        }

        private void AddEqualsItem(Line firstItem, Line secondItem, Line originalItem, FileData result)
        {
            var state = ChangeState.WithoutChanges;

            if (firstItem.IsEmpty && secondItem.IsEmpty)
            {
                state = ChangeState.Delete;
            }
            else if (originalItem.IsEmpty)
            {
                state = ChangeState.Add;
            }
            else if (!Equals(originalItem, firstItem))
                state = ChangeState.Modified;

            firstItem.ChangeState =
                secondItem.ChangeState = state;

            result.Lines.Add(firstItem);
        }

        private void AddEmpty(List<Line> lines, int count)
        {
            for (var i = 0; i < count; i++)
                lines.Add(new Line());
        }

        private void CheckEmptyLines(List<Line> originalLines, List<Line> firstLines, List<Line> secondLines)
        {
            if (originalLines.Count != firstLines.Count ||
                originalLines.Count != secondLines.Count)
            {
                Debug.WriteLine("Offset error");
            }

            var emptyLines = new List<int>();

            for (var i = 0; i < originalLines.Count; i++)
            {
                if (originalLines[i].IsEmpty &&
                    firstLines[i].IsEmpty &&
                    secondLines[i].IsEmpty)
                {
                    emptyLines.Add(i);
                }
            }

            foreach (var index in emptyLines.OrderByDescending(x => x))
            {
                originalLines.RemoveAt(index);
                firstLines.RemoveAt(index);
                secondLines.RemoveAt(index);
            }
        }

        private void AddLinesWithChanges(List<Line> originalLines, List<Line> firstLines, List<Line> secondLines)
        {
            var added = AddOtherLines(firstLines, FirstFile);

            AddEmpty(secondLines, added);
            AddEmpty(originalLines, added);

            added = AddOtherLines(secondLines, SecondFile);

            AddEmpty(originalLines, added);
            AddEmpty(firstLines, added);
        }

        private void SetOnOneLevel(List<Line> original, List<Line> first, List<Line> second)
        {
            var firstEmpties = 0;
            var secondEmpties = 0;

            foreach (var item in OriginalFile.Lines)
            {
                if (!item.OriginalLineNumber.HasValue)
                    continue;

                var firstLineIndex = GetItemIndex(FirstFile, item.OriginalLineNumber.Value);
                var secondLineIndex = GetItemIndex(SecondFile, item.OriginalLineNumber.Value);

                var firstLineIndexWithOffset = firstLineIndex > -1 ? firstLineIndex + firstEmpties : firstLineIndex;
                var secondLineIndexWithOffset = secondLineIndex > -1 ? secondLineIndex + secondEmpties : secondLineIndex;

                var offset = Max(item.OriginalLineNumber.Value,
                                 firstLineIndexWithOffset,
                                 secondLineIndexWithOffset) - original.Count;

                if (offset < 0 && firstLineIndex == -1 && secondLineIndex == -1)
                {
                    original.Add(item);
                    first.Add(new Line());
                    firstEmpties++;

                    second.Add(new Line());
                    secondEmpties++;
                }
                else
                {
                    while (offset > -1)
                    {
                        if (offset == 0)
                        {
                            original.Add(item);

                            if (firstLineIndex > -1)
                            {
                                first.Add(FirstFile.Lines[firstLineIndex]);
                            }
                            else
                            {
                                first.Add(new Line());
                                firstEmpties++;
                            }

                            if (secondLineIndex > -1)
                            {
                                second.Add(SecondFile.Lines[secondLineIndex]);
                            }
                            else
                            {
                                second.Add(new Line());
                                secondEmpties++;
                            }
                        }
                        else
                        {
                            original.Add(new Line());
                            first.Add(new Line());
                            second.Add(new Line());

                            firstEmpties++;
                            secondEmpties++;
                        }

                        offset--;
                    }
                }
            }
        }

        private int AddOtherLines(List<Line> lines, FileData fileData)
        {
            var addedLines = 0;

                var next = 0;
                int? lineNumber = null;

            for (var i = 0; i < fileData.Lines.Count; i++)
            {
                var line = fileData.Lines[i];

                if (line.OriginalLineNumber.HasValue)
                {
                    lineNumber = line.OriginalLineNumber.Value;
                    next = 1;

                    continue;
                }

                if (!lineNumber.HasValue)
                {
                    if (next >= lines.Count)
                    {
                        lines.Add(line);
                        addedLines++;
                    }
                    else
                    {
                        lines[next] = line;
                    }
                }
                else
                {
                    var lineIndex = lines.FindIndex(x => x.OriginalLineNumber == lineNumber.Value);

                    var newIndex = lineIndex + next;
                    if (newIndex >= lines.Count)
                    {
                        lines.Add(line);
                        addedLines++;
                    }
                    else
                    {
                        lines[newIndex] = line;
                    }
                }

                next++;
            }

            return addedLines;
        }

        private int Max(int original, int first, int second)
        {
            var max = Math.Max(original, first);

            return Math.Max(max, second);
        }

        private int GetItemIndex(FileData fileData, int i)
        {
            var line = fileData.Lines.FirstOrDefault(l => l.OriginalLineNumber == i);
            if (line != null)
                return fileData.Lines.FindIndex(l => Equals(l, line) && l.OriginalLineNumber == i);

            return -1;
        }
    }
}
