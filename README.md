# Sequence® Core SDK

[Sequence®](https://gitlab.com/reductech/sequence) is a collection of libraries
that automates cross-application e-discovery and forensic workflows.

It consists of the Core SDK which allows developers to
build Steps for various application and to configure workflows
called Sequences using the Sequence Configuration Language (SCL).

`Core` is:

- An interpreter for the `Sequence Configuration Language`
- A collection of application-independent Steps that:
  - Can be used to import/export data and structure workflows
  - Manipulate strings, e.g. Append, Concatenate, ChangeCase
  - Enforce data standards and convert between various formats through the use of Schemas
  - Create and manipulate entities (structured objects that represent data)
  - Control flow, e.g. If, ForEach, While
- An SDK that allows developers to create Connectors for applications

Connectors are collections of Steps that are for use with
a particular application. A Step is a unit of work in an application
such as creating a case, ingesting data, searching or exporting data.

# Try SCL and Core

Using the Sequence console application: https://gitlab.com/reductech/sequence/console

# Releases

Can be downloaded from the [Releases page](https://gitlab.com/reductech/sequence/core/-/releases).

# NuGet Packages

Are available in the [Reductech Nuget feed](https://gitlab.com/reductech/nuget/-/packages).
