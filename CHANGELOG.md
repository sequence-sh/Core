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
