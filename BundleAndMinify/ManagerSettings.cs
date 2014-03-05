using System.Configuration;

namespace BundleAndMinify
{
  public class ManagerSettings : ConfigurationSection
  {
    [ConfigurationProperty("raiseErrorIfFileDoesNotExists", IsRequired = false, DefaultValue = true)]
    public bool RaiseErrorIfFileDoesNotExists
    {
      get { return (bool)this["raiseErrorIfFileDoesNotExists"]; }
      set { this["raiseErrorIfFileDoesNotExists"] = value; }
    }

    [ConfigurationProperty("isDevMode", IsRequired = false, DefaultValue = false)]
    public bool? IsDevMode
    {
      get { return (bool?)this["isDevMode"]; }
      set { this["isDevMode"] = value; }
    }

    [ConfigurationProperty("addHash", IsRequired = false, DefaultValue = false)]
    public bool AddHash
    {
      get { return (bool)this["addHash"]; }
      set { this["addHash"] = value; }
    }

    [ConfigurationProperty("jsFolder", IsRequired = true, DefaultValue = "~/min/js")]
    public string JSFolder
    {
      get { return this["jsFolder"] as string; }
      set { this["jsFolder"] = value; }
    }

    [ConfigurationProperty("cssFolder", IsRequired = true, DefaultValue = "~/min/css")]
    public string CSSFolder
    {
      get { return this["cssFolder"] as string; }
      set { this["cssFolder"] = value; }
    }

    [ConfigurationProperty("imageFolder", IsRequired = true, DefaultValue = "~/min/images")]
    public string ImageFolder
    {
      get { return this["imageFolder"] as string; }
      set { this["imageFolder"] = value; }
    }

    [ConfigurationProperty("cdn", IsRequired = true)]
    public string CDN
    {
      get { return this["cdn"] as string; }
      set { this["cdn"] = value; }
    }
  }
}
