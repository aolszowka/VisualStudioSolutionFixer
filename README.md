# VisualStudioSolutionFixer
Utility program to Validate or Visual Studio Fix Solution Files (SLN)

## When To Use This Tool
This tool is intended to be used after you have done a massive source folder reorganization. The problem becomes now all of the relative paths to your Project Files are invalid in your Solution Files.

Previous to this tool you had to open each solution file and remove/re-add the project in question. This is tedious and error prone.

This tool will:

* Scan the given directory for Solution Files (SLN)
* Validate each Referenced Project in the Solution File Exists

If one of these files does not exist at the specified location the tool will:

* Scan the same given directory for all CSPROJ/SYNPROJ/VBPROJ Projects
* Load those into a lookup dictionary by their ProjectGuid
    * This will throw an exception if the ProjectGuid is duplicated. To identify the issue see https://github.com/aolszowka/MsBuildDuplicateProjectGuid and to fix the issue see https://github.com/aolszowka/MsBuildSetProjectGuid
* It will then update the Solution File with the correct relative path.

Because the tooling operates on the ProjectGuid the name of the project can change completely so long as the project is found within the specified directory.

## Usage
```
Usage: VisualStudioSolutionFixer.exe [validateDirectory] directoryToOperateOn

Scans given directory for Solution Files (*.sln); Correcting their Project References.
Invalid Command/Arguments. Valid commands are:

[directory]                   - [MODIFIES] Spins through the specified directory
                                and all subdirectories for Solution Files (SLN)
                                updates all solution files. Prints the solution
                                files that were fixed to the Console. ALWAYS Returns 0.
validatedirectory [directory] - [READS] Spins through the specified directory
                                and all subdirectories for Solution Files (SLN)
                                prints to the console invalid Solutions. Returns the
                                number of invalid solution files.
```
## License
This tool is MIT Licensed.

## Hacking
The most likely change you will want to make is changing the supported project files. In theory this tool should support any MSBuild Project Format that utilizes a ProjectGuid.

See SolutionFixer.GetProjectsInDirectory(string) for the place to modify this.

Every attempt has been made to make this tool as fast as possible such that we should just be I/O Bound.

Pull requests and bug reports are welcomed.