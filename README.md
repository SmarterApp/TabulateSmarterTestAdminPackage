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

For example, Smarter Balanced provided the 2019-20 training and practice test packages
```
TabulateSmarterTestPackage \
  -i 2019-20_Practice_SBAC_TestPackage_08.16.19.zip \
  -i 2019-20_Training_SBAC_TestPackage_08.16.19.zip \
  -ci 2019-20_Practice-Training_SBAC_ContentPackage_081619_ItemReport.csv \
  -cs 2019-20_Practice-Training_SBAC_ContentPackage_081619_StimulusReport.csv \
  -sv -o 2019-20_Practice_Training
```


## Build Notes
Written in C#. Built using Microsoft Visual Studio Express 2013 for Windows Desktop. 
Other editions of Visual Studio should also work; VS 2017 and VS 2019 have been used.
JetBrains Rider works pretty well on a mac.

The project has settings to produce trimmed, single file executables for certain target platforms.
To build for linux (TODO - consider linux-musl-x64 for Alpine linux): 
```
dotnet publish -r linux-x64 -c Release
ls -al TabulateSmarterTestPackage/bin/Release/netcoreapp3.1/linux-x64/publish 
-rwxr-xr-x    1 staff  staff  47808679 Feb  4 15:29 TabulateSmarterTestPackage
``` 
To build for macOS: 
```
dotnet publish -r osx-x64 -c Release
ls -al TabulateSmarterTestPackage/bin/Release/netcoreapp3.1/osx-x64/publish 
-rwxr-xr-x    1 staff  staff  43000712 Feb  4 15:33 TabulateSmarterTestPackage
```
To build framework-dependent (FDD):
```
dotnet publish -c Release
ls -al TabulateSmarterTestPackage/bin/Release/netcoreapp3.1/publish 
-rwxr--r--   1 mark.laffoon  staff  164864 Jan 30 15:55 CsvHelper.dll
-rw-r--r--   1 mark.laffoon  staff     380 Feb  4 14:04 NLog.config
-rwxr--r--   1 mark.laffoon  staff  781824 Nov  4 21:13 NLog.dll
-rw-r--r--   1 mark.laffoon  staff   92160 Feb  7 13:15 ProcessSmarterTestPackage.dll
-rw-r--r--   1 mark.laffoon  staff   27508 Feb  7 13:15 ProcessSmarterTestPackage.pdb
drwxr-xr-x   3 mark.laffoon  staff      96 Feb  7 13:15 Resources
-rw-r--r--   1 mark.laffoon  staff   35328 Feb  7 13:15 SmarterTestPackage.Common.dll
-rw-r--r--   1 mark.laffoon  staff   11772 Feb  7 13:15 SmarterTestPackage.Common.pdb
-rwxr-xr-x   1 mark.laffoon  staff   80868 Feb  7 13:15 TabulateSmarterTestPackage
-rw-r--r--   1 mark.laffoon  staff    3990 Feb  7 13:15 TabulateSmarterTestPackage.deps.json
-rw-r--r--   1 mark.laffoon  staff   74752 Feb  7 13:15 TabulateSmarterTestPackage.dll
-rw-r--r--   1 mark.laffoon  staff   15900 Feb  7 13:15 TabulateSmarterTestPackage.pdb
-rw-r--r--   1 mark.laffoon  staff     146 Feb  7 13:15 TabulateSmarterTestPackage.runtimeconfig.json
-rw-r--r--   1 mark.laffoon  staff   42050 Feb  4 08:48 TestPackageSchema.xsd
-rw-r--r--   1 mark.laffoon  staff   77824 Feb  7 13:15 ValidateSmarterTestPackage.dll
-rw-r--r--   1 mark.laffoon  staff   23072 Feb  7 13:15 ValidateSmarterTestPackage.pdb
```

