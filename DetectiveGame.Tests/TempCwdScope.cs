using System;
using System.IO;

namespace DetectiveGame.Tests
{
    public sealed class TempCwdScope : IDisposable
    {
        private readonly string _oldCwd;

        public string Root { get; }

        public TempCwdScope()
        {
            _oldCwd = Directory.GetCurrentDirectory();
            Root = Path.Combine(Path.GetTempPath(), "DetectiveGameTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            Directory.SetCurrentDirectory(Root);
        }

        public void Dispose()
        {
            try { Directory.SetCurrentDirectory(_oldCwd); } catch { }
            try
            {
                if (Directory.Exists(Root))
                    Directory.Delete(Root, recursive: true);
            }
            catch { }
        }
    }
}