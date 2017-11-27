using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;

namespace PackageDriver.Services
{
    public class PackageService : IDisposable
    {
        #region App Setting
        private readonly int default_freq;
        private readonly int default_duration;
        #endregion

        #region Private Fields
        private PacketTypes package_type;
        private PacketStates package_state;
        private string prepared_package;
        private string package_body;
        private ConsoleKeyInfo key_pressed;
        #endregion

        #region Init
        public PackageService()
        {
            package_state = PacketStates.WaitStart;
            default_freq = Convert.ToInt32(ConfigurationManager.AppSettings["frequency"]);
            default_duration = Convert.ToInt32(ConfigurationManager.AppSettings["duration"]);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start to listen packages stream
        /// </summary>
        public void ListenPackages()
        {
            WaitPackageHead();
            WaitPackagetType();
            WaitPackageBody();
            WaitPackageEnd();

            ProcessPackage(prepared_package);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Waiting for icoming package
        /// </summary>
        private void WaitPackageHead()
        {
            package_state = PacketStates.WaitStart;
            key_pressed = Console.ReadKey();
            while (key_pressed.Key != ConsoleKey.P)
            {
                key_pressed = Console.ReadKey();
            }
        }
        /// <summary>
        /// When package was detected, start to checking package type
        /// </summary>
        private void WaitPackagetType()
        {
            package_state = PacketStates.WaitType;
            do
            {
                key_pressed = System.Console.ReadKey();
                if (key_pressed.Key == ConsoleKey.T)
                {
                    package_type = PacketTypes.Text;
                }
                if (key_pressed.Key == ConsoleKey.S)
                {
                    package_type = PacketTypes.Sound;
                }
            }
            while
            (key_pressed.Key != ConsoleKey.T && key_pressed.Key != ConsoleKey.S);

            do { key_pressed = System.Console.ReadKey(); } while (key_pressed.KeyChar != ':');
        }


        /// <summary>
        /// When package type was checked, start to parsing package body
        /// </summary>
        private void WaitPackageBody()
        {
            package_state = PacketStates.WaitBody;
            do
            {
                key_pressed = System.Console.ReadKey();
                if (key_pressed.KeyChar >= 32 && key_pressed.KeyChar <= 127 && key_pressed.KeyChar != ':')
                {
                    package_body += key_pressed.KeyChar;
                }

            } while (key_pressed.KeyChar != ':');
        }

        /// <summary>
        /// When package body was parsed, start to waiting package END
        /// </summary>
        private void WaitPackageEnd()
        {
            package_state = PacketStates.WaitEnd;
            do { key_pressed = System.Console.ReadKey(); } while (key_pressed.KeyChar != 'E');

            switch (package_type)
            {
                case PacketTypes.Sound:
                    prepared_package = "PS:" + package_body + ":" + "E";
                    break;
                case PacketTypes.Text:
                    prepared_package = "PT:" + package_body + ":" + "E";
                    break;
            }
        }

        /// <summary>
        /// When package package was completely processed
        /// </summary>
        /// <param name="package">incoming package</param>
        private void ProcessPackage(string package)
        {
            CkeckPackageType(package);

            if (package_type == PacketTypes.Sound)
            {
                package_body = GetPackageBody(package);

                int freq = getFreq(package_body);
                int duration = getDuration(package_body);

                try
                {
                    System.Console.Beep(freq, duration);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else if (package_type == PacketTypes.Text)
            {
                package_body = GetPackageBody(package);
                System.Console.WriteLine("\n" + package_body);
            }
        }
#endregion

        #region Helper Methods
        /// <summary>
        /// Checking package type(TEXT or SOUND)
        /// </summary>
        /// <param name="package">incoming package</param>
        private void CkeckPackageType(string package)
        {
            try
            {

                char type = package[1];
                switch (type)
                {
                    case 'T':
                        package_type = PacketTypes.Text;
                        break;
                    case 'S':
                        package_type = PacketTypes.Sound;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Couldn't prase package type."), ex);
            }
        }

        /// <summary>
        /// Parse package and select package body
        /// </summary>
        /// <param name="package">incoming package</param>
        /// <returns>return package body as string</returns>
        private string GetPackageBody(string package)
        {
            try
            {
                string firstFragment = package.Split(':').First();
                string lastFragment = package.Split(':').Last();

                string output = Regex.Match(package, string.Format("{0}(.*){1}", firstFragment + ":", ":" + lastFragment)).Groups[1].Value;

                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Couldn't parse package body."), ex);
                return "";
            }
        }
        
        /// <summary>
        /// Parse frequency in package body with type: SOUND
        /// </summary>
        /// <param name="body">package body</param>
        /// <returns>freq as int32</returns>
        private int getFreq(string body)
        {
            try
            {
                string get_freq = body.Split(',').First();
                int freq = Convert.ToInt32(get_freq);

                if (freq >= 37 && freq <= 32767)
                    return freq;
                else
                    return default_freq;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nCouldn't parse frequency from package body. Default freq has been set: {0}", default_freq),ex);
                return default_freq;
            }
        }
        /// <summary>
        /// Parse duration in package body with type: SOUND
        /// </summary>
        /// <param name="body">package body</param>
        /// <returns>duration as int32</returns>
        private int getDuration(string body)
        {
            try
            {
                string duration = body.Split(',').Last();
                return Convert.ToInt32(duration);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nCouldn't parse duration from package body. Default duration has been set: {0}", default_duration), ex);
                return default_duration;
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PacketService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
