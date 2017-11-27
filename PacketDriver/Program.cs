namespace PacketDriver.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                using (var service = new PackageDriver.Services.PackageService())
                {
                    //Wait for package
                    service.ListenPackages();
                }
            }
        }
    }
}
