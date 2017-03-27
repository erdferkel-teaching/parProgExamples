using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;

namespace HandsOnSharp
{
    static class Pipeline
    {
        static void ProcessFile(string inputPath, string outputPath) {
            var inputLines = new BlockingCollection<string>();
            var processedLines = new BlockingCollection<string>();

            // Stage #1     
            var readLines = Task.Factory.StartNew(() =>     
            {
                try
                {
                    foreach (var line in File.ReadLines(inputPath))
                        inputLines.Add(line);
                }
                finally {
                    inputLines.CompleteAdding();
                }
            });

            // Stage #2     
            var processLines = Task.Factory.StartNew(() =>     
            {
                try
                {
                    foreach (var line in inputLines.GetConsumingEnumerable()                 
                                            .Select(line => Regex.Replace(line, @"\s+", ", ")))
                    {
                        processedLines.Add(line);
                    }
                }
                finally {
                    processedLines.CompleteAdding();
                }
            });

            // Stage #3     
            var writeLines = Task.Factory.StartNew(() =>    
            {
                File.WriteAllLines(outputPath, processedLines.GetConsumingEnumerable());
            }); 

            Task.WaitAll(readLines, processLines, writeLines);
        }
    }
    }
}
