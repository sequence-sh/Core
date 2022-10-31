﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Reductech.Sequence.Core.Internal.Logging {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class LogMessages_EN {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LogMessages_EN() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Reductech.Sequence.Core.Internal.Logging.LogMessages-EN", typeof(LogMessages_EN).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ConnectorSettings: {settings}.
        /// </summary>
        internal static string ConnectorSettings {
            get {
                return ResourceManager.GetString("ConnectorSettings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Directory Deleted: {Path}.
        /// </summary>
        internal static string DirectoryDeleted {
            get {
                return ResourceManager.GetString("DirectoryDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {StepName} Started with Parameters: {Parameters}.
        /// </summary>
        internal static string EnterStep {
            get {
                return ResourceManager.GetString("EnterStep", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {EnvironmentVariableName}: {EnvironmentVariableValue}.
        /// </summary>
        internal static string EnvironmentVariable {
            get {
                return ResourceManager.GetString("EnvironmentVariable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {StepName} Failed with message: {Message}.
        /// </summary>
        internal static string ExitStepFailure {
            get {
                return ResourceManager.GetString("ExitStepFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {StepName} Completed Successfully with Result: {Result}.
        /// </summary>
        internal static string ExitStepSuccess {
            get {
                return ResourceManager.GetString("ExitStepSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ExternalProcess {process} started with arguments: &apos;{arguments}&apos;.
        /// </summary>
        internal static string ExternalProcessStarted {
            get {
                return ResourceManager.GetString("ExternalProcessStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File Deleted: {Path}.
        /// </summary>
        internal static string FileDeleted {
            get {
                return ResourceManager.GetString("FileDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The id &apos;{Id}&apos; does not exist..
        /// </summary>
        internal static string IdNotPresent {
            get {
                return ResourceManager.GetString("IdNotPresent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Item to Delete did not Exist: {Path}.
        /// </summary>
        internal static string ItemToDeleteDidNotExist {
            get {
                return ResourceManager.GetString("ItemToDeleteDidNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No path was provided. Returning the Current Directory: {CurrentDirectory}.
        /// </summary>
        internal static string NoPathProvided {
            get {
                return ResourceManager.GetString("NoPathProvided", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Path {Path} was not fully qualified. Prepending the Current Directory: {CurrentDirectory}.
        /// </summary>
        internal static string QualifyingPath {
            get {
                return ResourceManager.GetString("QualifyingPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not remove variable {variable} as it was out of scope..
        /// </summary>
        internal static string RemoveVariableOutOfScope {
            get {
                return ResourceManager.GetString("RemoveVariableOutOfScope", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Schema violation: &apos;{message}&apos; at &apos;{location}&apos;..
        /// </summary>
        internal static string SchemaViolation {
            get {
                return ResourceManager.GetString("SchemaViolation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sequence Completed.
        /// </summary>
        internal static string SequenceCompleted {
            get {
                return ResourceManager.GetString("SequenceCompleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sequence Started.
        /// </summary>
        internal static string SequenceStarted {
            get {
                return ResourceManager.GetString("SequenceStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not set variable {variable} as it was out of scope..
        /// </summary>
        internal static string SetVariableOutOfScope {
            get {
                return ResourceManager.GetString("SetVariableOutOfScope", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error Caught in {StepName}: {Message}.
        /// </summary>
        internal static string StepErrorWasCaught {
            get {
                return ResourceManager.GetString("StepErrorWasCaught", resourceCulture);
            }
        }
    }
}
