using Nanny.Common.Interfaces;
using Nanny.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Main.Rules
{
    class ConstantFactorRule : IScalingRule
    {
        public TimeSpan NextCheck { get; }
        public int ConstantFactor { get; }

        public ConstantFactorRule(TimeSpan nextCheck, int constantFactor = 1)
        {
            NextCheck = nextCheck;
            ConstantFactor = constantFactor;
        }

        public JobScalingResult GetJobScalingResult(long messageCount, int currentRunningJobs)
        {
            if (messageCount > 0)
                return new JobScalingResult(NextCheck, ConstantFactor);
            return new JobScalingResult(NextCheck);
        }
    }
}
