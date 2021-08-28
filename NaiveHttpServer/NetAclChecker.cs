using System;
using System.ComponentModel;
using System.Diagnostics;

namespace NaiveHttpServer
{
    public static class NetAclChecker
    {
        public static ILogger? Logger { get; set; }

        public static void AddAddress(string address)
        {
            AddAddress(address, Environment.UserDomainName, Environment.UserName);
        }

        public static void AddAddress(string address, string domain, string user)
        {
            string args = $@"http add urlacl url={address}, user={domain}\{user}";

            try
            {
                ProcessStartInfo processStartInfo = new("netsh", args)
                {
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                };

                var process = Process.Start(processStartInfo);
                process?.WaitForExit();
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == 1223)
                {
                    Logger?.Info("User canceled the operation by rejected the UAC.");
                }
                else
                {
                    Logger?.Warning($"Failed to 'netsh http add urlacl {address}' with an {nameof(Win32Exception)}.", e);
                }
            }
            catch (Exception e)
            {
                Logger?.Warning($"Failed to 'netsh http add urlacl {address}'.", e);
            }
        }
    }
}
