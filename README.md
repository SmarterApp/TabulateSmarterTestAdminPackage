# TabulateSmarterTestAdminPackage
Extracts item and stimulus data from Test Administration and Test Scoring packages in SmarterApp format. The extracted data is tabulated in .csv format for convenient analysis.

## To Use
This is a command-line program for Windows. Execute "TabulateSmarterTestAdminPackage -h" for a help screen and syntax.

The application has the following inputs:
- **-i** The path to a valid file, .zip, or directory. There must be one or more -i arguments **(required)**
- **-o** This text will be prepended to the names of the output reports for items, stimuli, and errors. **(required)**
- **-ci** The path to the .csv ItemReport output of the ContentPackageTabulator. (optional)
- **-cs** The path to the .csv StimulusReport output of the ContentPackageTabulator. (optional)
- **-sv** If this flag is present and both -ci and -cs arguments are provided, the tabulator will write cross tabulation errors for stimuli versions.

*NOTE: Both the -ci and -cs arguments are required for validation and tabulation to occur between the content and test packages

## Build Notes
Written in C#. Built using Microsoft Visual Studio Express 2013 for Windows Desktop. 
Other editions of Visual Studio should also work; VS 2017 and VS 2019 have been used.

The project has settings to produce trimmed, single file executables for certain target platforms.
To build for linux, `dotnet publish -r linux-x64 -c Release`. Similarly to build for macOS
`dotnet publish -r osx-x64 -c Release`.

TODO - consider linux-musl-x64 for Alpine linux if needed 
