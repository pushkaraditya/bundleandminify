using System.Collections.Generic;
using yui = Yahoo.Yui.Compressor;


namespace BundleAndMinify
{
  public interface ICompressor
  {
    yui.Compressor GetCompressor();

    Dictionary<string, string> Cache { get; }
    string Extension { get; }
    string Folder { get; }
    string Tag { get; }
  }
}