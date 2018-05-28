using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nanny.Main.Utils
{
    public static class EnvironmentManager
    {
        public static bool IsDevelopment()
        {
            if (GetEnvironmentStatus() == EnvironmentStatus.Local)
                return true;
            return false;
        }

        private static EnvironmentStatus GetEnvironmentStatus()
        {
            if (File.Exists("/var/run/secrets/kubernetes.io"))
                return EnvironmentStatus.Kubernetes;

            if (File.Exists("/.dockerenv"))
                return EnvironmentStatus.Docker;

            return EnvironmentStatus.Local;
        }
    }
}
