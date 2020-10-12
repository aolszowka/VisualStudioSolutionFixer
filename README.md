# VisualStudioSolutionFixer
Utility program to Validate or Visual Studio Fix Solution Files (SLN) Referenced Projects.

## When To Use This Tool
This tool is intended to be used after you have done a massive source folder reorganization. The problem becomes now all of the relative paths to your Project Files are invalid in your Solution Files.

Previous to this tool you had to open each solution file and remove/re-add the project in question. This is tedious and error prone.

This tool will:

* Scan the given directory for Solution Files (SLN)
* Validate each Referenced Project in the Solution File Exists

If one of these files does not exist at the specified location the tool will:

* Scan the same given directory for all CSPROJ/SYNPROJ/VBPROJ Projects
    * In addition you can provide additional directories to scan as one or more "lookupDirectory" the directory being fixed is always included
* Load those into a lookup dictionary by their ProjectGuid
    * This will throw an exception if the ProjectGuid is duplicated. To identify the issue see https://github.com/aolszowka/MsBuildDuplicateProjectGuid and to fix the issue see https://github.com/aolszowka/MsBuildSetProjectGuid
* It will then update the Solution File with the correct relative path.

Because the tooling operates on the ProjectGuid the name of the project can change completely so long as the project is found within the specified directory.

**NOTE** This tool will only fix Solution Files; you most likely need to fix all of the Project Files as well. For that see the sister tool https://github.com/aolszowka/MsBuildProjectReferenceFixer

## Usage
```
Usage:
VisualStudioSolutionFixer.exe [validateDirectory] directoryToOperateOn [lookupDirectory]+

Scans given directory for Solution Files (*.sln); correcting any invalid
references to projects within them using a lookup directory.

Invalid Command/Arguments. Valid commands are:

directory [lookupDirectory]+
    [MODIFIES] Spins through the specified directory and all subdirectories for
    Solution Files (SLN) updates all solution files. Prints the solution files
    that were fixed to the Console. ALWAYS Returns 0.

validatedirectory directory [lookupDirectory]+
    [READS] Spins through the specified directory and all subdirectories for
    Solution Files (SLN) prints to the console invalid Solutions. Returns the
    number of invalid solution files.

In all instances:
* You can specify multiple directories to be used as "lookup directories"; the
  given directory is always used as a lookup directory and you can specify as
  many additional lookup directories as you want.
* Any project that is invalid is written to the console.
```

## Hacking
The most likely change you will want to make is changing the supported project files. In theory this tool should support any MSBuild Project Format that utilizes a ProjectGuid.

See SolutionFixer.GetProjectsInDirectory(string) for the place to modify this.

Every attempt has been made to make this tool as fast as possible such that we should just be I/O Bound.

Pull requests and bug reports are welcomed so long as they are MIT Licensed.

## License
This tool is MIT Licensed.

## Third Party Licenses
This project uses other open source contributions see [LICENSES.md](LICENSES.md) for a comprehensive listing.
