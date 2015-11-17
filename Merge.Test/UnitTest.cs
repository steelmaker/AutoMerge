using System.Diagnostics;
using System.Linq;
using Merge.Common;
using Merge.Common.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Merge.Test
{
    /// <summary>
    /// Unit tests.
    /// </summary>
    [TestClass]
    public class UnitTest
    {
        private Mock<FileManager> _fileManagerMock;
        private Mock<LcsCalculator> _lcsCalculatorMock;

        /// <summary>
        /// Test initialize.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _fileManagerMock = new Mock<FileManager>();
            _lcsCalculatorMock = new Mock<LcsCalculator>();
        }

        /// <summary>
        /// Test Equals method.
        /// </summary>
        [TestMethod]
        public void TestEqualsLine()
        {
            const string firstText = "firstText";
            const string secondText = "secondText";

            var firstWithoutChanges = new Line {Hash = Sha1Util.GetHashString(firstText), Text = firstText, ChangeState = ChangeState.WithoutChanges};
            var firstConflict = new Line {Hash = Sha1Util.GetHashString(firstText), Text = firstText, ChangeState = ChangeState.Conflict};
            var second = new Line {Hash = Sha1Util.GetHashString(secondText), Text = secondText};

            Assert.IsFalse(Equals(firstWithoutChanges, second));
            Assert.IsTrue(Equals(firstWithoutChanges, firstConflict));
        }

        /// <summary>
        /// All lines is equals.
        /// </summary>
        [TestMethod]
        public void FilesWithoutChanges()
        {
            var lines = new[]
            {
                "111", "222", "333", "444", "555"
            };

            Merger merger;

            var result = GetResultFileData(lines, lines, lines, out merger);

            var originalFileItemsCount = merger.OriginalFile.Lines.Count;
            Assert.IsTrue(originalFileItemsCount == result.Lines.Count);
            Assert.IsTrue(originalFileItemsCount == lines.Count());

            for (var i = 0; i < result.Lines.Count; i++)
            {
                Assert.IsTrue(result.Lines[i].ChangeState == ChangeState.WithoutChanges);
                Assert.IsTrue(Equals(merger.OriginalFile.Lines[i], result.Lines[i]));
            }
        }

        /// <summary>
        /// Result file without conflicts.
        /// </summary>
        [TestMethod]
        public void FilesWithConflicts()
        {
            Merger merger;
            var result = GetResultFileData(new[] { "111", "0", "333", "444", "555" },
                                           new[] { "111", "1", "333", "444", "555" },
                                           new[] { "111", "2", "333", "444", "555" }, out merger);

            Assert.IsTrue(result.Lines[1].ChangeState == ChangeState.Conflict);
        }

        /// <summary>
        /// Test changed only first file.
        /// </summary>
        [TestMethod]
        public void UsedFirstChanges()
        {
            Merger merger;
            var result = GetResultFileData(new[] { "111", "0", "333", "444", "555" },
                                           new[] { "111", "1", "333", "444", "555" },
                                           new[] { "111", "0", "333", "444", "555" }, out merger);

            Assert.IsTrue(Equals(result.Lines[1], merger.FirstFile.Lines[1]));
            Assert.IsFalse(Equals(result.Lines[1], merger.SecondFile.Lines[1]));
            Assert.IsFalse(Equals(result.Lines[1], merger.OriginalFile.Lines[1]));
        }

        /// <summary>
        /// Test changed only second file.
        /// </summary>
        [TestMethod]
        public void UsedSecondChanges()
        {
            Merger merger;
            var result = GetResultFileData(new[] { "111", "0", "333", "444", "555" },
                                           new[] { "111", "0", "333", "444", "555" },
                                           new[] { "111", "1", "333", "444", "555" }, out merger);

            Assert.IsFalse(Equals(result.Lines[1], merger.OriginalFile.Lines[1]));
            Assert.IsFalse(Equals(result.Lines[1], merger.FirstFile.Lines[1]));
            Assert.IsTrue(Equals(result.Lines[1], merger.SecondFile.Lines[1]));
        }

        /// <summary>
        /// Test changed first and second file in one line.
        /// </summary>
        [TestMethod]
        public void UsedEqualsFirstAndSecondChanges()
        {
            Merger merger;
            var result = GetResultFileData(new[] { "111", "0", "333", "444", "555" },
                                           new[] { "111", "1", "333", "444", "555" },
                                           new[] { "111", "1", "333", "444", "555" }, out merger);

            Assert.IsFalse(Equals(result.Lines[1], merger.OriginalFile.Lines[1]));
            Assert.IsTrue(Equals(result.Lines[1], merger.FirstFile.Lines[1]));
            Assert.IsTrue(Equals(result.Lines[1], merger.SecondFile.Lines[1]));
        }

        /// <summary>
        /// Test changed in first and second files in other lines.
        /// </summary>
        [TestMethod]
        public void UsedFirstAndSecondChanges()
        {
            Merger merger;
            var result = GetResultFileData(new[] { "111", "0", "333", "444", "555" },
                                           new[] { "111", "0", "111", "444", "555" },
                                           new[] { "111", "0", "333", "222", "555" }, out merger);

            Assert.IsFalse(Equals(result.Lines[2], merger.OriginalFile.Lines[2]));
            Assert.IsFalse(Equals(result.Lines[3], merger.OriginalFile.Lines[3]));

            Assert.IsTrue(Equals(result.Lines[2], merger.FirstFile.Lines[2]));
            Assert.IsTrue(Equals(result.Lines[3], merger.SecondFile.Lines[3]));
        }

        /// <summary>
        /// Test add line to end.
        /// </summary>
        [TestMethod]
        public void UsedFirstAddToEndChanges()
        {
            Merger merger;
            var result = GetResultFileData(new[] { "111", "0", "333", "444" },
                                           new[] { "111", "0", "111", "444", "555" },
                                           new[] { "111", "0", "333", "222" }, out merger);

            Assert.IsTrue(Equals(result.Lines.Last(), merger.FirstFile.Lines.Last()));
            Assert.IsTrue(result.Lines.Last().Text == "555");
        }

        /// <summary>
        /// Test add line to start in first file and add to end in second file.
        /// </summary>
        [TestMethod]
        public void UsedFirstAddToStartAndSecondAddToEndChanges()
        {
            Merger merger;
            var result = GetResultFileData(new[] {        "0", "333", "444" },
                                           new[] { "111", "0", "333", "444" },
                                           new[] {        "0", "333", "444", "555" }, out merger);

            Assert.IsFalse(merger.OriginalFile.Lines.Contains(merger.FirstFile.Lines.First()));
            Assert.IsFalse(merger.OriginalFile.Lines.Contains(merger.SecondFile.Lines.Last()));

            Assert.IsTrue(Equals(result.Lines[0], merger.FirstFile.Lines[0]));
            Assert.IsTrue(Equals(result.Lines.Last(), merger.SecondFile.Lines.Last()));
        }

        /// <summary>
        /// Test change first and delete second line in result conflict.
        /// </summary>
        [TestMethod]
        public void ConflictWhenFirstChangedAndSecondDelete()
        {
            Merger merger;
            var result = GetResultFileData(new[] { "0", "333", "444" },
                                           new[] { "0", "555", "444" },
                                           new[] { "0",  "444" }, out merger);

            Assert.IsTrue(result.Lines[1].ChangeState == ChangeState.Conflict);
        }

        /// <summary>
        /// Test performance.
        /// </summary>
        [TestMethod]
        public void TestPerformance()
        {
            var linesCount = 10000;
            var linesOrigin = new string[linesCount];
            var linesFirst = new string[linesCount];
            var linesSecond = new string[linesCount];

            for (var i = 0; i < linesCount; i++)
            {

                linesOrigin[i] = i.ToString();
                linesFirst[i] = (i%2 == 0 ? i : 0).ToString();
                linesSecond[i] = (i%3 == 0 ? i : 0).ToString();
            }

            var timer = new Stopwatch();

            timer.Start();

            Merger merger;
            GetResultFileData(linesOrigin, linesOrigin, linesOrigin, out merger);

            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < 10000);
        }

        private FileData GetResultFileData(string[] originalData, string[] firstData, string[] secondData, out Merger merger)
        {
            var lcsCalculator = _lcsCalculatorMock.Object;

            merger = new Merger
            {
                OriginalFile = GetFileData(originalData),
                FirstFile = GetFileData(firstData),
                SecondFile = GetFileData(secondData)
            };

            merger.SetLineNumbers(merger.OriginalFile);

            lcsCalculator.SetLongestCommonSubsequence(merger.OriginalFile.Lines.ToArray(), merger.FirstFile.Lines.ToArray());
            lcsCalculator.SetLongestCommonSubsequence(merger.OriginalFile.Lines.ToArray(), merger.SecondFile.Lines.ToArray());

            merger.SetOffset();

            merger.SetOriginalLinesDeleted();

            return merger.Compare(merger.FirstFile, merger.SecondFile);
        }

        private FileData GetFileData(string[] sequence)
        {
            _fileManagerMock.Setup(x => x.GetLines(""))
                            .Returns(sequence);

            return _fileManagerMock.Object.LoadFile("");
        }
    }
}
