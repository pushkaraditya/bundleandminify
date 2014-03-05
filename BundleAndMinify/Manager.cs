using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace BundleAndMinify
{
  public class Manager
  {
    #region Singleton

    private Manager() { }

    private static Manager instance = null;
    private static object guard = new object();

    public static Manager Instance
    {
      get
      {
        if (instance == null)
          lock (guard)
            if (instance == null)
            {
              instance = new Manager();
              instance.Initialize();
            }
        return instance;
      }
    }

    private void Initialize()
    {
      var settings = ConfigurationManager.GetSection("minification") as ManagerSettings;
      if (settings == null)
      {
        RaiseErrorIfFileDoesNotExists = true;
#if DEBUG
        IsDevMode = true;
        AddHash = false;
#else
        IsDevMode = false;
        AddHash = true;
#endif
        compressors.Add(BundleType.JS, new JSCompressor("~/min/js"));
        compressors.Add(BundleType.CSS, new CSSCompressor("~/min/js"));
        ImageFolder = "~/min/images";
      }
      else
      {
        CDN = settings.CDN;
        compressors.Add(BundleType.JS, new JSCompressor(settings.JSFolder));
        compressors.Add(BundleType.CSS, new CSSCompressor(settings.CSSFolder));
        ImageFolder = settings.ImageFolder;

        RaiseErrorIfFileDoesNotExists = settings.RaiseErrorIfFileDoesNotExists;
        if (settings.IsDevMode.HasValue)
          IsDevMode = settings.IsDevMode.Value;
        else
        {
#if DEBUG
          IsDevMode = true;
#else
          IsDevMode = false;
#endif
        }
        AddHash = settings.AddHash;
      }
    }

    #endregion

    /// <summary>
    /// Default Value should be true
    /// </summary>
    public bool RaiseErrorIfFileDoesNotExists { get; set; }
    public bool IsDevMode { get; set; }
    public bool AddHash { get; set; }
    public string CDN { get; set; }
    public string ImageFolder { get; set; }

    private Dictionary<BundleType, ICompressor> compressors = new Dictionary<BundleType, ICompressor>();

    public void RegisterJS(HttpServerUtility server, string bundle, params string[] files)
    {
      RegisterBundle(server, bundle, BundleType.JS, files);
    }

    public void RegisterCSS(HttpServerUtility server, string bundle, params string[] files)
    {
      RegisterBundle(server, bundle, BundleType.CSS, files);
    }

    private void RegisterBundle(HttpServerUtility server, string bundle,
      BundleType type,
      params string[] files)
    {
      if (string.IsNullOrWhiteSpace(bundle))
        throw new ArgumentNullException("bundle", "Cannot register an empty bundle");
      var compressor = GetCompressor(type);
      if (compressor.Cache.ContainsKey(bundle))
        throw new ArgumentException("Bundle already registered");
      if (files == null || files.Length == 0)
        throw new ArgumentNullException("files", "No files to register");

      var list = files.ToList();
      if (server != null)
        list = list.ConvertAll(file => server.MapPath(file));

      if (RaiseErrorIfFileDoesNotExists)
      {
        var notFound = list.ToList().FindAll(file => !File.Exists(file));
        if (notFound != null && notFound.Count > 0)
          throw new ArgumentException(string.Format("Not able to find following files: {0}{1}", Environment.NewLine, string.Join(Environment.NewLine, notFound)));
      }

      if (IsDevMode)
        compressor.Cache.Add(bundle, string.Join(Environment.NewLine, files.ToList().ConvertAll(path => string.Format(compressor.Tag, ResolvePath(path)))));
      else
      {
        StringBuilder sb = new StringBuilder();
        var yuiCompressor = compressor.GetCompressor();

        list.ToList().ForEach(file =>
        {
          if (RaiseErrorIfFileDoesNotExists || (!RaiseErrorIfFileDoesNotExists && File.Exists(file)))
            sb.Append(yuiCompressor.Compress(File.ReadAllText(file)));
        });

        if (imageCache.Count > 0)
          imageCache.Keys.ToList().ForEach(key => sb.Replace(key, imageCache[key]));

        var content = sb.ToString();
        content = yuiCompressor.Compress(content); // double compression

        var format = AddHash ? "{0}." + CreateHash(content) + ".{1}" : "{0}.{1}";
        var path = Path.Combine(server.MapPath(compressor.Folder), string.Format(format, bundle, compressor.Extension));
        WriteContent(path, content);

        path = ResolvePath(server.RelativePath(path));
        compressor.Cache.Add(bundle, string.Format(compressor.Tag, path));
      }
    }

    public string GetScript(string bundle)
    {
      return GetBundlePath(bundle, BundleType.JS);
    }

    public string GetCSS(string bundle)
    {
      return GetBundlePath(bundle, BundleType.CSS);
    }

    Dictionary<string, string> imageCache = new Dictionary<string, string>();
    public string GetImagePath(string path)
    {
      if (!imageCache.ContainsKey(path))
        RegisterImage(path);
      return imageCache[path];
    }

    public void RegisterImage(string path)
    {
      if (imageCache.ContainsKey(path))
        throw new ArgumentException("Image is already added", "path");

      var key = path;
      if (IsDevMode)
        path = path.TrimStart('~');
      else
      {
        var server = HttpContext.Current.Server;
        var file = new FileInfo(server.MapPath(path));
        if (!file.Exists && RaiseErrorIfFileDoesNotExists)
          throw new ArgumentException("file does not exists", "path");
        var filename = file.Name;
        if (AddHash)
        {
          var hash = CreateHash(File.ReadAllBytes(file.FullName));
          filename = string.Format("{0}.{1}{2}", filename.Replace(file.Extension, string.Empty), hash, file.Extension);
        }

        var copiedPath = Path.Combine(server.MapPath(ImageFolder), filename);
        file.CopyTo(copiedPath, true);
        if ((File.GetAttributes(copiedPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            File.SetAttributes(copiedPath, File.GetAttributes(copiedPath) & ~FileAttributes.ReadOnly);
        path = ResolvePath(server.RelativePath(copiedPath));
      }

      imageCache.Add(key, path);
    }

    public void RegisterImages(params string[] images)
    {
      if (images == null || images.Length == 0)
        return;
      images.Distinct().ToList()
        .FindAll(image => !imageCache.ContainsKey(image))
        .ForEach(image => RegisterImage(image));
    }

    private string GetBundlePath(string bundle, BundleType type)
    {
      var cache = GetCompressor(type).Cache;
      if (cache.ContainsKey(bundle))
        return cache[bundle];
      else
        throw new Exception("Bundle does not exists");
    }

    private string CreateHash(string content)
    {
      return CreateHash(content.GetBytes());
    }

    private string CreateHash(byte[] bytes)
    {
      using (var algo = System.Security.Cryptography.MD5.Create())
      {
        return BitConverter.ToString(algo.ComputeHash(bytes)).Replace("-", string.Empty).ToLower();
      }
    }

    private string ResolvePath(string path)
    {
      if (IsDevMode)
        return (HttpRuntime.AppDomainAppVirtualPath + path.Replace("~", string.Empty)).Replace("//", "/"); // I am not adding the CDN path only because in Dev mode, developers wanted to work locally
      else
      {
        UriBuilder builder = null;
        if (string.IsNullOrWhiteSpace(CDN))
          builder = new UriBuilder();
        else
          builder = new UriBuilder(CDN);
        builder.Path = (builder.Path + path).Replace("//", "/");
        path = builder.Uri.ToString();
        return path;
      }
    }

    private void WriteContent(string path, string content)
    {
        File.WriteAllText(path, content);
        if ((File.GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
    }

    #region Factory Area

    private ICompressor GetCompressor(BundleType type)
    {
      return compressors[type];
    }

    #endregion
  }
}