namespace NestorHub.Sentinels.Api
{
    public class SentinelHost
    {
        private string _name;
        private PackageHost _packageHost;

        public void Run(string name, string packageName)
        {
            _name = name;
            _packageHost = new PackageHost(packageName);
        }

        internal string GetName()
        {
            return _name;
        }

        internal string GetPackageName()
        {
            return _packageHost.Name;
        }
    }

    internal class PackageHost
    {
        public string Name { get; }

        public PackageHost(string name)
        {
            Name = name;
        }
    }
}
