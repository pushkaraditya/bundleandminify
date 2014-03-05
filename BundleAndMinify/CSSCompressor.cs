using System.Collections.Generic;
using yui = Yahoo.Yui.Compressor;

namespace BundleAndMinify
{
  public class CSSCompressor : ICompressor
  {
    public CSSCompressor(string folder)
    {
      Cache = cache;
      Extension = "css";
      Folder = folder;
      Tag = "<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />";
    }

    public yui.Compressor GetCompressor()
    {
      return new yui.CssCompressor
      {
        CompressionType = yui.CompressionType.Standard,
        RemoveComments = true
      };
    }

    private static Dictionary<string, string> cache = new Dictionary<string, string>();
    public Dictionary<string, string> Cache { get; private set; }
    public string Extension { get; private set; }
    public string Folder { get; private set; }
    public string Tag { get; private set; }
  }
}