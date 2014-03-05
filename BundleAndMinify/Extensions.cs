using System;
using System.Web;

namespace BundleAndMinify
{
  public static class Extensions
  {
    public static byte[] GetBytes(this string str)
    {
      byte[] bytes = new byte[str.Length * sizeof(char)];
      Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length); // might raise exception in case of str is too long
      return bytes;
    }

    public static string GetString(this byte[] bytes)
    {
      char[] chars = new char[bytes.Length / sizeof(char)];
      Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
      return new string(chars);
    }

    public static string RelativePath(this HttpServerUtility server, string path)
    {
      var app = server.MapPath("~");
      return path.Replace(app, string.Empty).Replace(@"\", "/");
    }
  }
}
