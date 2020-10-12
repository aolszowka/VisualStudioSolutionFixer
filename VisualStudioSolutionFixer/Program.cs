// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018-2020. All rights reserved.
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
        /// GUID referenced in the Solution File cannot be found.
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
                        Console.WriteLine(Strings.NotEnoughDirectoryArguments);
                        errorCode = 1;
                    }
                    else
                    {
                        bool allDirectoriesValid = true;

                        // Validate the Remaining Arguments
                        foreach (string directoryArgument in args.Skip(1))
                        {
                            allDirectoriesValid = allDirectoriesValid && IsValidDirectoryArgument(directoryArgument);
                        }

                        if (allDirectoriesValid)
                        {
                            // The Error Code should be the number of projects that would be modified
                            errorCode = PrintToConsole(args.Skip(1), false);
                        }
                        else
                        {
                            errorCode = 9009;
                            Console.WriteLine(Strings.OneOrMoreInvalidDirectories);
                        }
                    }
                }
                else
                {
                    bool allDirectoriesValid = true;

                    // Validate the Remaining Arguments
                    foreach (string directoryArgument in args.Skip(1))
                    {
                        allDirectoriesValid = allDirectoriesValid && IsValidDirectoryArgument(directoryArgument);
                    }

                    if (allDirectoriesValid)
                    {
                        PrintToConsole(args, true);
                        // We always return zero if there was no exception
                        errorCode = 0;
                    }
                    else
                    {
                        errorCode = 9009;
                        Console.WriteLine(Strings.OneOrMoreInvalidDirectories);
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

        private static bool IsValidDirectoryArgument(string directoryArgument)
        {
            bool isValidDirectory = true;

            if (!Directory.Exists(directoryArgument))
            {
                Console.WriteLine(Strings.InvalidDirectoryArgument, directoryArgument);
                isValidDirectory = false;
            }

            return isValidDirectory;
        }

        private static int ShowUsage()
        {
            Console.WriteLine(Strings.HelpTextMessage);
            return 21;
        }

        /// <summary>
        /// Print the result of the Visual Studio Solution Fix to the Console.
        /// </summary>
        /// <param name="directoryArguments">
        /// An IEnumerable of directories to operate on; the first argument
        /// is always the directory to be scanned/modified. Any remaining
        /// arguments are additional directories used for lookups.
        /// </param>
        /// <param name="fixProjects">
        /// Indicates whether or not to fix any invalid solutions.
        /// </param>
        /// <returns>The number of projects that were modified</returns>
        static int PrintToConsole(IEnumerable<string> directoryArguments, bool fixProjects)
        {
            int brokenSolutionCount = 0;
            string targetDirectory = directoryArguments.First();

            IEnumerable<(string SolutionFile, IReadOnlyDictionary<string, string> MissingProjects)> invalidSolutions = SolutionFixer.Execute(targetDirectory, directoryArguments, fixProjects);

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
