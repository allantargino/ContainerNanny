using Nanny.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Common.Interfaces
{
    public interface IScalingRule
    {
        JobScalingResult GetJobScalingResult(long messageCount, int currentRunningJobs);
    }
}
