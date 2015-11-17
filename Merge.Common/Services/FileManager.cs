using System.IO;
using System.Linq;

namespace Merge.Common.Services
{
    /// <summary>
    /// Represent operation for work with files.
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// Load file data.
        /// </summary>
        /// <param name="path">The full path.</param>
        /// <returns>The file data.</returns>
        public FileData LoadFile(string path)
        {
            var lines = GetLines(path);

            var file = new FileData
            {
                // used Trim because Line with space start and end AND Line without space start and end are EQUALS
                Lines = lines.Select(x => new Line{Text = x, Hash = Sha1Util.GetHashString(x.Trim())}).ToList()
            };

            return file;
        }

        /// <summary>
        /// Save file data.
        /// </summary>
        /// <param name="file">The file data.</param>
        /// <param name="filePath">The full path.</param>
        /// <returns>The full path.</returns>
        public virtual string SaveFile(FileData file, string filePath)
        {
            File.WriteAllLines(filePath, file.Lines.Where(x => !x.IsEmpty).Select(x => x.Text));

            return filePath;
        }

        public virtual string[] GetLines(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);

            return File.ReadAllLines(path);
        }
    }
}
