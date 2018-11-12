// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionFixer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

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
                        string error = string.Format("You must provide a directory as a second argument to use validatedirectory");
                        Console.WriteLine(error);
                        errorCode = 1;
                    }
                    else
                    {
                        // The second argument is a directory
                        string directoryArgument = args[1];

                        if (Directory.Exists(directoryArgument))
                        {
                            errorCode = PrintToConsole(args[1], false);
                        }
                        else
                        {
                            string error = string.Format("The provided directory `{0}` is invalid.", directoryArgument);
                            errorCode = 9009;
                        }
                    }
                }
                else
                {
                    if (Directory.Exists(command))
                    {
                        string targetPath = command;
                        PrintToConsole(args[1], true);
                        errorCode = 0;
                    }
                    else
                    {
                        string error = string.Format("The specified path `{0}` is not valid.", command);
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
            StringBuilder message = new StringBuilder();
            message.AppendLine("Scans given directory for Solution Files (*.sln); Correcting their Project References.");
            message.AppendLine("Invalid Command/Arguments. Valid commands are:");
            message.AppendLine();
            message.AppendLine("[directory]                   - [MODIFIES] Spins through the specified directory\n" +
                               "                                and all subdirectories for Solution Files (SLN)\n" +
                               "                                updates all solution files. Prints the solution\n" +
                               "                                files that were fixed to the Console. ALWAYS Returns 0.");
            message.AppendLine("validateDirectory [directory] - [READS] Spins through the specified directory\n" +
                               "                                and all subdirectories for Solution Files (SLN)\n" +
                               "                                prints to the console invalid Solutions. Returns the\n" +
                               "                                number of invalid solution files.");
            Console.WriteLine(message);
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
