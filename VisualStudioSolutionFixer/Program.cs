// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018-2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionFixer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using VisualStudioSolutionFixer.Properties;

    class Program
    {
        /// <summary>
        /// Utility to fix Visual Studio Solution Files (SLN), scans the
        /// given directory for solution files, and ensures that the referenced
        /// projects exist at the specified location. If it does not it will
        /// attempt to scan that same directory for any CSPROJ/SYNPROJ/VBPROJ
        /// and will attempt to update the solution file.
        /// 
        /// This program will throw an InvalidOperationException if the project
        /// guid referenced in the Solution File cannot be found.
        /// </summary>
        /// <param name="args">See <see cref="ShowUsage"/></param>
        static void Main(string[] args)
        {
            int errorCode = 0;

            if (args.Any())
            {
                string command = args.First().ToLowerInvariant();

                if (command.Equals("-?") || command.Equals("/?") || command.Equals("-help") || command.Equals("/help"))
                {
                    errorCode = ShowUsage();
                }
                else if (command.Equals("validatedirectory"))
                {
                    if (args.Length < 2)
                    {
                        Console.WriteLine(StringResources.NotEnoughDirectoryArguments);
                        errorCode = 1;
                    }
                    else
                    {
                        // The second argument is a directory
                        string directoryArgument = args[1];

                        if (Directory.Exists(directoryArgument))
                        {
                            errorCode = PrintToConsole(directoryArgument, false);
                        }
                        else
                        {
                            string error = string.Format(StringResources.InvalidDirectoryArgument, directoryArgument);
                            errorCode = 9009;
                        }
                    }
                }
                else
                {
                    if (Directory.Exists(command))
                    {
                        string targetPath = command;
                        PrintToConsole(targetPath, true);
                        errorCode = 0;
                    }
                    else
                    {
                        string error = string.Format(StringResources.InvalidDirectoryArgument, command);
                        Console.WriteLine(error);
                        errorCode = 1;
                    }
                }
            }
            else
            {
                // This was a bad command
                errorCode = ShowUsage();
            }

            Environment.Exit(errorCode);
        }

        private static int ShowUsage()
        {
            Console.WriteLine(StringResources.HelpTextMessage);
            return 21;
        }

        static int PrintToConsole(string targetDirectory, bool fixProjects)
        {
            int brokenSolutionCount = 0;

            IEnumerable<(string SolutionFile, IReadOnlyDictionary<string, string> MissingProjects)> invalidSolutions = SolutionFixer.Execute(targetDirectory, fixProjects);

            foreach ((string SolutionFile, IReadOnlyDictionary<string, string> MissingProjects) invalidSolutionResult in invalidSolutions)
            {
                brokenSolutionCount++;
                Console.WriteLine($"{invalidSolutionResult.SolutionFile}:");

                foreach (KeyValuePair<string, string> invalidProject in invalidSolutionResult.MissingProjects)
                {
                    Console.WriteLine($"{invalidProject.Value}\t{invalidProject.Key}");
                }

                Console.WriteLine();
            }

            return brokenSolutionCount;
        }

    }
}
