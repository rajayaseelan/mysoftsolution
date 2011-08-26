using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SharedCache.WinServiceCommon")]
[assembly: AssemblyDescription("Shared Cache Client Library - information available at www.sharedcache.com")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Roni Schuetz")]
[assembly: AssemblyProduct("SharedCache.WinServiceCommon")]
[assembly: AssemblyCopyright("Copyright © Roni Schuetz 2005 - 2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("06facc8b-881b-434d-af21-cbc405528248")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("3.0.5.1")]
[assembly: AssemblyFileVersion("3.0.5.1")]

//#region Helper class

///// <summary>
///// Your generated class does not contain the follwoing part.
///// </summary>
//public class AssemblyInfo
//{
//  // Used by Helper Functions to access information from Assembly Attributes

//  private Type myType;
//  /// <summary>
//  /// Initializes a new instance of the <see cref="AssemblyInfo"/> class.
//  /// </summary>
//  public AssemblyInfo()
//  {
//    myType = typeof(SharedCache.WinServiceCommon.CacheUtil);
//  }

//  /// <summary>
//  /// Gets the name of the asm.
//  /// </summary>
//  /// <value>The name of the asm.</value>
//  public string AsmName
//  {
//    get { return myType.Assembly.GetName().Name.ToString(); }
//  }

//  /// <summary>
//  /// Gets the name of the asm FQ.
//  /// </summary>
//  /// <value>The name of the asm FQ.</value>
//  public string AsmFQName
//  {
//    get { return myType.Assembly.GetName().FullName.ToString(); }
//  }

//  /// <summary>
//  /// Gets the code base.
//  /// </summary>
//  /// <value>The code base.</value>
//  public string CodeBase
//  {
//    get { return myType.Assembly.CodeBase; }
//  }

//  /// <summary>
//  /// Gets the copyright.
//  /// </summary>
//  /// <value>The copyright.</value>
//  public string Copyright
//  {
//    get
//    {
//      Type at = typeof(AssemblyCopyrightAttribute);
//      object[] r = myType.Assembly.GetCustomAttributes(at, false);
//      AssemblyCopyrightAttribute ct = (AssemblyCopyrightAttribute)r[0];
//      return ct.Copyright;
//    }
//  }

//  /// <summary>
//  /// Gets the company.
//  /// </summary>
//  /// <value>The company.</value>
//  public string Company
//  {
//    get
//    {
//      Type at = typeof(AssemblyCompanyAttribute);
//      object[] r = myType.Assembly.GetCustomAttributes(at, false);
//      AssemblyCompanyAttribute ct = (AssemblyCompanyAttribute)r[0];
//      return ct.Company;
//    }
//  }


//  /// <summary>
//  /// Gets the description.
//  /// </summary>
//  /// <value>The description.</value>
//  public string Description
//  {
//    get
//    {
//      Type at = typeof(AssemblyDescriptionAttribute);
//      object[] r = myType.Assembly.GetCustomAttributes(at, false);
//      AssemblyDescriptionAttribute da = (AssemblyDescriptionAttribute)r[0];
//      return da.Description;
//    }
//  }


//  /// <summary>
//  /// Gets the product.
//  /// </summary>
//  /// <value>The product.</value>
//  public string Product
//  {
//    get
//    {
//      Type at = typeof(AssemblyProductAttribute);
//      object[] r = myType.Assembly.GetCustomAttributes(at, false);
//      AssemblyProductAttribute pt = (AssemblyProductAttribute)r[0];
//      return pt.Product;
//    }
//  }

//  /// <summary>
//  /// Gets the title.
//  /// </summary>
//  /// <value>The title.</value>
//  public string Title
//  {
//    get
//    {
//      Type at = typeof(AssemblyTitleAttribute);
//      object[] r = myType.Assembly.GetCustomAttributes(at, false);
//      AssemblyTitleAttribute ta = (AssemblyTitleAttribute)r[0];
//      return ta.Title;
//    }
//  }

//  /// <summary>
//  /// Gets the version.
//  /// </summary>
//  /// <value>The version.</value>
//  public string Version
//  {
//    get { return myType.Assembly.GetName().Version.ToString(); }
//  }
//}
//#endregion
