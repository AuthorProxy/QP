﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Quantumart.QP8.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class StatusTypeStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal StatusTypeStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Quantumart.QP8.Resources.StatusTypeStrings", typeof(StatusTypeStrings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add New Status.
        /// </summary>
        public static string AddNewStatusType {
            get {
                return ResourceManager.GetString("AddNewStatusType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Alternate color.
        /// </summary>
        public static string AltColor {
            get {
                return ResourceManager.GetString("AltColor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Alternate color format.
        /// </summary>
        public static string AltColorInvalidFormat {
            get {
                return ResourceManager.GetString("AltColorInvalidFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The length of Alternate color should not exceed {5} characters.
        /// </summary>
        public static string AltColorLengthExceeded {
            get {
                return ResourceManager.GetString("AltColorLengthExceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You should specify Alternate color or clear Color.
        /// </summary>
        public static string AltColorMissed {
            get {
                return ResourceManager.GetString("AltColorMissed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Color.
        /// </summary>
        public static string Color {
            get {
                return ResourceManager.GetString("Color", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Color format.
        /// </summary>
        public static string ColorInvalidFormat {
            get {
                return ResourceManager.GetString("ColorInvalidFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The length of Color should not exceed {5} characters.
        /// </summary>
        public static string ColorLengthExceeded {
            get {
                return ResourceManager.GetString("ColorLengthExceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You should specify Color or clear Alternate color.
        /// </summary>
        public static string ColorMissed {
            get {
                return ResourceManager.GetString("ColorMissed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This status is used by an article and can not be deleted.
        /// </summary>
        public static string StatusArticleUsage {
            get {
                return ResourceManager.GetString("StatusArticleUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This status is Built-In and can not be deleted.
        /// </summary>
        public static string StatusBuiltIn {
            get {
                return ResourceManager.GetString("StatusBuiltIn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Status is not found (Id={0}).
        /// </summary>
        public static string StatusTypeNotFound {
            get {
                return ResourceManager.GetString("StatusTypeNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This status is used by workflow and can not be deleted.
        /// </summary>
        public static string StatusWorkflowUsage {
            get {
                return ResourceManager.GetString("StatusWorkflowUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Weight.
        /// </summary>
        public static string Weight {
            get {
                return ResourceManager.GetString("Weight", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This site already has a status with such weight.
        /// </summary>
        public static string WeightIsInUse {
            get {
                return ResourceManager.GetString("WeightIsInUse", resourceCulture);
            }
        }
    }
}
