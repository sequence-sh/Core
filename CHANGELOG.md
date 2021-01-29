## v0.4.0 (2021-01-29)

Major rework of the language and data streaming features so lots of **breaking changes** including:

- Moved from YAML-based configuration to a custom configuration
  language called SCL (Sequence Configuration Language)
  - Using a custom [ANTLR](https://www.antlr.org/) grammar to parse the configuration language
  - The new language is similar to YAML
- Consolidated entity streams, lists and array into a single datatype - `Array<T>`
  - There are no longer separate Steps for arrays and entity streams (e.g. `ForEach`, `IsEmpty`, `Length`)
- Consolidated strings and data streams into a single datatype - `StringStream`
- Update to .NET 5.0
- Reworked logging and exceptions
  - More frequent and more consistent logging
  - Added step scopes
  - Added localization features
  - Added support for appending custom metadata
- Added steps for JSON and IDX
- Documentation has now been moved to https://docs.reductech.io

### New Features

- Allow creation of SCLSettings from IConfiguration object #167
- Create shortcut methods to access nested entity properties #166
- Add 'Features' property to requirements so we can support nuix features #165
- Allow creation of entities from dictionaries #164
- Make settings more dynamic to make it easier to control multiple connectors #163
- GenerateDocumentation links should work #162
- Include more metadata in the results of GenerateDocumentation #161
- Check connector requirements, identifying errors during startup #131
- Make the Documentation generator output entities so we can have documentation spread across multiple files. #129
- Allow GZip decompression during file read and write to make life easier for technicians #159
- Create Steps to convert Entities to and from IDX #155
- Change double formatting when serializing #156
- Allow dangling commas in arrays and entity lists #157
- Create Steps to convert Entities to and from JSON #154
- Use Source Generators to generate unit test code #146
- Allow steps to have arguments without positional parameters #152
- Rename LogSituation_Core to LogSituation and ErrorCode_Core to ErrorCode #151
- Allow Connectors to access Error Exceptions #150
- Allow Connectors to access Error Exceptions #150
- Allow Connectors to Define Custom Error Codes #149
- Systematize log and error output messages so Managers can get detailed information #17
- Rename and merge Array and EntityStream methods #142
- Use the names Sequence Configuration Language and SCL where appropriate #137
- Change Single Line Comments to a single Hash #143
- Add Steps to convert dates to and from strings, so that technicians can format dates #144 #138
- Combine EntityStreams and Arrays #134
- Should Be able to Deserialize DateTimes #141
- Reinstate Serializaion, Deserialization, and DeserializationError tests #120
- Allow Pipelining to make language more concise #119
- Refactor Entities to allow infinite nesting of sequences #135
- Merge String and DataStream to make Datastream usage implicit #127
- Allow Coercion of strings to enums to make using enums easier for technicians #126
- Allow Comments in Sequence files so technicians can comment their code #122
- Allow argument aliases in language to make it easier to write readable code #118
- Allow ordered arguments to make language more concise #117
- Allow nesting of sequences to make complex steps easier to write #121
- Generated Antlr files should be in a namespace and be written to the Core/Antlr folder #124
- Allow antlr build to run on linux #123
- Allow brackets and commas to be omitted in function calls to make the language easier to read and write #115
- Use colons instead of equals in function calls to make language more intuitive #116

### Bug Fixes

- ScopedStateMonad should dispose of scoped state variables when disposed #140
- Streams returned by ToCSV should be moved to the beginning of the stream #139
- StringStreams should dispose of the stream after it is read #136
- Compare functions should be wrapped in brackets when serialized #133
- TestHarness should check ordering or log values #132
- Use of the 'Not' construct prevents mutation testing #125
- Error when creating entities with 'add' in a property name #113 #112

### Maintenance

- Update Antlr nuget packages to 4.9.1 #160
- Add .editorconfig file and standardize formatting #148
- Update to .NET 5.0 #43

## v0.3.0 (2020-11-27)

**Breaking changes** - Step and argument names have changed to make them more
consistent. Step names now follow the convention of _NamespaceAction_, e.g.
`ArrayLength`, `ArraySort`, `EntityMap`, `EntityStreamSort`.

- Added Entities and EntityStreams along with Linq style methods to manipulate them.
- Added Entity Schemas to allow conversion between different
- Step state is now scoped and disposable
- Added Steps to convert `EntityStream` to/from concordance and CSV

### New Features

- Rename some arguments for consistency #109
- Create Steps to read and write concordance so technicians do that easily #108
- Use AsyncEnumerable instead of TPL for EntityStream #106
- Create Linq style methods so Technicians can easily manipulate entities #72
- Create a WriteCSV method so Technicians can write entities to CSV #73
- Use CSVHelper instead of VisualBasic.io to read and write CSV files #94
- RunExternalProcess should use channels rather than StreamReaders to make testing easier #102
- Refactor Unit Test cases so classes inheriting from them have more control over settings #101
- To release control of resources, Make StateMonad IDisposable #100
- RunExternalProcess should take inputStream and Encoding arguments to support Nuix script streaming #99
- Create Schema for Entities so we can enforce and manipulate type and format constraints #95
- Create Data Interchange Entities, so that a Technicians can have a clean way to handle data #71
- Add encoding step, so that technicians can specify text encoding when reading or writing files #82
- To prevent steps going untested, create a unit test to test that all steps are being tested #91
- Allow Serialization and Deserialization of Entity, Stream, and EntityStream #93

### Bug Fixes

- Unit Tests are flaky #97

### Maintenance

- Remove obsolete Script Composition Code #105
- Add Release issue template #98
- Use template ci config, so that it's easier to maintain #96

### Other

- Change references to StateMonad to IStateMonad in Process Run Overloads #107

## v0.2.1 (2020-11-03)

### New Features

- Create a Step test library, so that developers can save time testing connectors #86
- GenerateDocumentation should be a step #75
- Allow short form array declarations #70
- Processes should be asynchronous and take a cancellation token #9

### Bug Fixes

- Yaml scalars throw a does not have the type 'IStep`1' error. #79
- Running yaml causes "Array was a VariableName, not an argument" error #77
- ReadCsv should not require parameters with default values #78
- fail: "The type '<VariableName>' is never set" when setting new array to variable #69
- Type Resolver doesn't work for some generic types #65

### Maintenance

- Remove ConsoleMethods #76

### Other

- Make error messages more verbose, so technicians can debug their yaml more easily #85

## v0.2.0 (2020-10-02)

The Core SDK has been reworked to use a procedural paradigm instead
of injection. See here for more details: https://gitlab.com/groups/reductech/edr/-/epics/6

Processes library has now been renamed to Core.
Chain renamed to sequence, and process to step.

For more details: https://gitlab.com/groups/reductech/-/epics/5

### New Features

- Processes and their factories should be in the same file #56
- Rename 'Process' to 'Step' #52
- Rename 'Processes' to 'Core' #51
- As a technician, I want to compose short forms of functions, to make it easier to write yaml #27
- Enable Composition of processes #36
- Change to the new console, so that a Technicians can have a more familiar interface #44
- Create additional utility processes #38
- As a Technician, I want to see helpful errors in yaml, so that I can diagnose and fix them more easily #28
- Create a "PreviousState" process for process splitting #16
- Create Function for combining Process Configurations #12
- Allow RunExternalProcess to handle errors #41
- Create an object for requirements #14
- Add control flow properties to all processes #11
- Processes should have a requirements property #2

### Bug Fixes

- Deserialization of strings containing apostrophes fails #58
- Allow aliases for Processes and properties when deserializing #1

### Maintenance

- Fix package tags #62
- Update ci script to latest template #61
- Use testing library instead #60
- Rename General to Steps #59
- Integrate new CI/CD script and project properties #55
- Create more tests for processes #48
- Create more tests for documentation #47
- Create more unit tests for serialization #54
- Create unit tests for serialization #49
- Enable mutation testing, so that a we can detect possible bugs #40
- Rename Core.Test to Core.Tests #57
- Merge 'attributes' with 'Attributes' #53
- Correct Mistakes in attribute Names #45
- Fix issue template location #39
- Add issue templates #37
- Create helper processes for unit testing #35
- Create an overload of RunExternalProcess which returns an object #34
- Create Preflight checks for processes #33
- Create Runtime Requirements for processes #32
- Create IRunnableProcessFactory #31
- Improve process settings type safety #30
- Change error type of RunExternalProcess #29
- Create More Test Processes #10
- Move Test processes into the main library #8
- Adjust Processes namespaces to match project hierarchy changes #7

### Documentation

- Prevent documentation generator from putting backticks in process names #64
- Update links in documentation contents #63
- Create a readme so users can see what the project is about and get started #50
- Add Contents section to documentation, so that a technician can find what they need more easily #46
- Create a guide, so that a Developers can build their own connectors. #42

### Other

- Change order of chain arguments #13

## v0.1.0 (2020-03-13)

Initial release of Core designed to work by injecting output of
a previous process into the current one.

### New Features

- Reworked property injection
- Updated enumerations and added foreach test
- Added ProcessSettings to Process
- Replaced Branch and conditions with Conditional Process and Asserts
- Added CreateDirectory

### Bug Fixes

- Sorted out the whole injection situation
- Fixed bug in ProcessInjector

### Documentation

- Add documentation
