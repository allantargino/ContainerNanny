using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Main.Utils
{
    public enum EnvironmentStatus
    {
        Local = 1,
        Docker = 2,
        Kubernetes = 4
    }
}
