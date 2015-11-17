using System;
using Merge.Common;

namespace Merge
{
    class Program
    {
        static void Main(string[] args)
        {
            const string originalFile = @"c:\origianl.txt";
            const string file1 = @"c:\first.txt";
            const string file2 = @"c:\second.cs";

            var merger = new Merger();

            string fileName;

            try
            {
                fileName = merger.Merge(originalFile, file1, file2, @"c:\result.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                Console.ReadKey();

                return;
            }
            

            Console.WriteLine("Merging complete: {0}", fileName);
            Console.ReadKey();
        }
    }
}
