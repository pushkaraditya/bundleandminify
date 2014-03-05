using System.Collections.Generic;
using yui = Yahoo.Yui.Compressor;

namespace BundleAndMinify
{
  public class JSCompressor : ICompressor
  {
    public JSCompressor(string folder)
    {
      Cache = cache;
      Extension = "js";
      Folder = folder;
      Tag = "<script src=\"{0}\" type=\"text/javascript\"></script>";
    }

    public yui.Compressor GetCompressor()
    {
      return new yui.JavaScriptCompressor
      {
        CompressionType = yui.CompressionType.Standard,
        ObfuscateJavascript = true
      };
    }

    private static Dictionary<string, string> cache = new Dictionary<string, string>();
    public Dictionary<string, string> Cache { get; private set; }
    public string Extension { get; private set; }
    public string Folder { get; private set; }
    public string Tag { get; private set; }
  }
}