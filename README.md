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
Usage: VisualStudioSolutionFixer C:\DirectoryWithSolutions [-ld=C:\lookupDir]
                                 [-ld=C:\lookupDir2] [-validate]

Scans given directory for Solution Files (*.sln); verifying that each of the
referenced projects exists.

Optionally correcting any invalid references to projects them using a lookup
directory. You can provide multiple lookup directories to this tool using any of
the directory lookup syntaxes.

Invalid Project References are written to the console.

Arguments:

               <>            The directory to scan for Visual Studio Solutions
      --lookupdirectory, --ld=VALUE
                             One or more directories to use to find projects,
                               these directories are searched for projects to
                               correct the paths in the Visual Studio Solution
                               Files if the path is invalid. This argument is
                               optional when run in validation mode.
      --validate             Indicates if this tool should only be run in
                               validation mode
  -?, -h, --help             Show this message and exit
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
