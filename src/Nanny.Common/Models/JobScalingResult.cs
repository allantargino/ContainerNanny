using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Common.Models
{
    public class JobScalingResult
    {
        public TimeSpan NextCheck { get; }
        public int JobCount { get; }

        public JobScalingResult(TimeSpan nextCheck, int jobCount = 0)
        {
            NextCheck = nextCheck;
            JobCount = jobCount;
        }
    }
}
