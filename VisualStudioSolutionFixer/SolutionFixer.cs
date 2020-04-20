// -----------------------------------------------------------------------
// <copyright file="SolutionFixer.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018-2020. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionFixer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Build.Construction;

    class SolutionFixer
    {
        public static IEnumerable<(string SolutionFile, IReadOnlyDictionary<string, string> MissingProjects)> Execute(string targetDirectory, IEnumerable<string> lookupDirectories, bool fixSolutions)
        {
            IEnumerable<string> solutionsInDirectory = GetSolutionsInDirectory(targetDirectory);
            ConcurrentBag<(string SolutionFile, IReadOnlyDictionary<string, string> MissingProjects)> invalidSolutions = new ConcurrentBag<(string SolutionFile, IReadOnlyDictionary<string, string> MissingProjects)>();

            Parallel.ForEach(solutionsInDirectory, solutionFile =>
            {
                IReadOnlyDictionary<string, string> resultForSolution = EvaluateSolution(solutionFile);

                if (resultForSolution.Any())
                {
                    invalidSolutions.Add((solutionFile, resultForSolution));
                }
            }
            );

            // Only incur this cost if we need to fix broken solutions
            if (fixSolutions && invalidSolutions.Any())
            {
                // THIS PROCESS IS EXPENSIVE; AVOID AT ALL COSTS!
                // At this point if we're going to fix the projects upload up our ProjectGuid Dictionary
                IReadOnlyDictionary<string, string> projectGuidLookup = LoadProjectGuids(lookupDirectories);

                Parallel.ForEach(invalidSolutions, invalidSolution =>
                {
                    FixSolution(invalidSolution.SolutionFile, invalidSolution.MissingProjects, projectGuidLookup);
                }
                );
            }

            return invalidSolutions;
        }

        internal static IReadOnlyDictionary<string, string> EvaluateSolution(string targetSolution)
        {
            SolutionFile solutionFile = null;
            Dictionary<string, string> invalidProjectReferences = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            try
            {
                solutionFile = SolutionFile.Parse(targetSolution);
            }
            catch (Exception ex)
            {
                invalidProjectReferences.Add(targetSolution, $"Failed To Load: {ex.Message}");
            }

            if (solutionFile != null)
            {
                foreach (ProjectInSolution project in solutionFile.ProjectsInOrder)
                {
                    if (project.ProjectType != SolutionProjectType.SolutionFolder)
                    {
                        if (!File.Exists(project.AbsolutePath))
                        {
                            invalidProjectReferences.Add(project.ProjectGuid, project.RelativePath);
                        }
                    }
                }
            }

            return invalidProjectReferences;
        }

        private static void FixSolution(string solutionPath, IReadOnlyDictionary<string, string> invalidProjectReferences, IReadOnlyDictionary<string, string> lookupDictionary)
        {
            string solutionFilePath = PathUtilities.AddTrailingSlash(Path.GetDirectoryName(solutionPath));

            // Load up the target solution into memory
            string solutionContent = File.ReadAllText(solutionPath);

            foreach (KeyValuePair<string, string> invalidProjectReference in invalidProjectReferences)
            {
                string validProjectFullPath;

                // First lookup the Guid of the Invalid Project
                if (!lookupDictionary.TryGetValue(invalidProjectReference.Key, out validProjectFullPath))
                {
                    string expandedOldProjectPath = PathUtilities.ResolveRelativePath(solutionPath, invalidProjectReference.Value);
                    string exception = $"In Solution `{solutionPath}` Project with Guid `{invalidProjectReference.Key}` was not found in lookup dictionary. Previously existed at `{expandedOldProjectPath}`";
                    throw new InvalidOperationException(exception);
                }

                // At this point we have the new Valid Full Path, convert this to a relative path to the solution
                string relativeValidPath = PathUtilities.GetRelativePath(solutionFilePath, validProjectFullPath);

                // Now we need to do the find and replace
                solutionContent = solutionContent.Replace(invalidProjectReference.Value, relativeValidPath);
            }

            Encoding solutionFileEncoding = Encoding.ASCII;

            // See if the original file was UTF-8 BOM
            if (FileUtilities.ContainsUTF8BOM(solutionPath))
            {
                solutionFileEncoding = Encoding.UTF8;
            }

            File.WriteAllText(solutionPath, solutionContent, solutionFileEncoding);
        }

        /// <summary>
        /// Given a set of directories spin for all project files (as defined by
        /// <see cref="GetProjectsInDirectory(string)"/>).
        /// </summary>
        /// <param name="lookupDirectories">The directory to scan.</param>
        /// <returns>
        /// <see cref="IDictionary{TKey, TValue}"/> where the <c>TKey</c> is
        /// the ProjectGuid and the <c>TValue</c> is the path to the project
        /// that contains that Guid.
        /// </returns>
        internal static IReadOnlyDictionary<string, string> LoadProjectGuids(IEnumerable<string> lookupDirectories)
        {
            // These will be file paths which should be case insensitive on Windows.
            IEnumerable<string> projFilesInLookupDirectories =
                lookupDirectories
                .AsParallel()
                .SelectMany(currentLookupDirectory => GetProjectsInDirectory(currentLookupDirectory))
                .Distinct(StringComparer.InvariantCultureIgnoreCase);

            ConcurrentDictionary<string, string> resultDictionary = new ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            Parallel.ForEach(projFilesInLookupDirectories, projFile =>
            {
                string projectGuid = MSBuildUtilities.GetMSBuildProjectGuid(projFile);
                if (!resultDictionary.TryAdd(projectGuid, projFile))
                {
                    string exception = $"Failed to add project `{projFile}` the GUID `{projectGuid}` already existed in project `{resultDictionary[projectGuid]}`";
                    throw new InvalidOperationException(exception);
                }
            }
            );

            return resultDictionary;
        }

        /// <summary>
        /// Gets all Project Files that are understood by this
        /// tool from the given directory and all subdirectories.
        /// </summary>
        /// <param name="targetDirectory">The directory to scan for projects.</param>
        /// <returns>All projects that this tool supports.</returns>
        internal static IEnumerable<string> GetProjectsInDirectory(string targetDirectory)
        {
            HashSet<string> supportedFileExtensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                ".csproj",
                ".fsproj",
                ".sqlproj",
                ".synproj",
                ".vbproj",
            };

            return
                Directory
                .EnumerateFiles(targetDirectory, "*proj", SearchOption.AllDirectories)
                .Where(currentFile => supportedFileExtensions.Contains(Path.GetExtension(currentFile)));
        }

        internal static IEnumerable<string> GetSolutionsInDirectory(string targetDirectory)
        {
            return Directory.EnumerateFiles(targetDirectory, "*.sln", SearchOption.AllDirectories);
        }

    }
}
