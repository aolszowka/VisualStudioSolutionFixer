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

    using NDesk.Options;

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
            string targetDirectory = string.Empty;
            bool validateOnly = false;
            bool showHelp = false;
            List<string> lookupDirectories = new List<string>();

            OptionSet p = new OptionSet()
            {
                { "<>", Strings.TargetDirectoryArgument, v => targetDirectory = v },
                { "lookupdirectory=|ld=", Strings.LookupDirectoryArgumentDescription, v => lookupDirectories.Add(v) },
                { "validate", Strings.ValidateDescription, v => validateOnly = v != null },
                { "?|h|help", Strings.HelpDescription, v => showHelp = v != null },
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine(Strings.ShortUsageMessage);
                Console.WriteLine($"Try `--help` for more information.");
                Environment.Exit(21);
            }

            if (showHelp || string.IsNullOrEmpty(targetDirectory))
            {
                int exitCode = ShowUsage(p);
                Environment.Exit(exitCode);
            }
            else
            {
                // First Ensure that all Directories are Valid
                if (IsValidDirectoryArgument(targetDirectory))
                {
                    bool allDirectoriesValid = true;

                    // Validate the Remaining Arguments
                    foreach (string directoryArgument in lookupDirectories)
                    {
                        allDirectoriesValid = allDirectoriesValid && IsValidDirectoryArgument(directoryArgument);
                    }

                    if (allDirectoriesValid)
                    {
                        bool saveChanges = validateOnly == false;

                        Environment.ExitCode = PrintToConsole(targetDirectory, lookupDirectories, saveChanges);

                        if (saveChanges)
                        {
                            // Always Return Zero
                            Environment.ExitCode = 0;
                        }
                    }
                    else
                    {
                        Environment.ExitCode = -1;
                        Console.WriteLine(Strings.OneOrMoreInvalidDirectories);
                    }
                }
                else
                {
                    Environment.ExitCode = -1;
                    Console.WriteLine(Strings.OneOrMoreInvalidDirectories);
                }
            }
        }

        private static int ShowUsage(OptionSet p)
        {
            Console.WriteLine(Strings.ShortUsageMessage);
            Console.WriteLine();
            Console.WriteLine(Strings.LongDescription);
            Console.WriteLine();
            Console.WriteLine($"               <>            {Strings.TargetDirectoryArgument}");
            p.WriteOptionDescriptions(Console.Out);
            return 21;
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

        /// <summary>
        /// Print the result of the Visual Studio Solution Fix to the Console.
        /// </summary>
        /// <param name="targetDirectory">Directory to scan for Solution Files</param>
        /// <param name="lookupDirectories">Directories to be used for lookups in fix mode</param>
        /// <param name="fixProjects">Indicates whether or not to fix any invalid solutions.</param>
        /// <returns>The number of projects that were modified</returns>
        static int PrintToConsole(string targetDirectory, IEnumerable<string> lookupDirectories, bool fixProjects)
        {
            int brokenSolutionCount = 0;

            IEnumerable<(string SolutionFile, IReadOnlyDictionary<string, string> MissingProjects)> invalidSolutions = SolutionFixer.Execute(targetDirectory, lookupDirectories, fixProjects);

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
