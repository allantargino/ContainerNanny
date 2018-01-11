using Nanny.Common.Interfaces;
using Nanny.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Main.Core.Rules
{
    class IncrementRule : IScalableRule
    {
        public TimeSpan NextCheck { get; }

        private long lastMessageCount;
        private int jobCountFactor;

        public IncrementRule(TimeSpan nextCheck)
        {
            NextCheck = nextCheck;

            lastMessageCount = 0;
            jobCountFactor = 1;
        }
        
        public JobScalingResult GetJobScalingResult(long messageCount, int currentRunningJobs)
        {
            if (messageCount > 0)
            {
                if (lastMessageCount != 0)
                {
                    Console.WriteLine($" LastCount: {lastMessageCount} // Count: {messageCount}");
                    if (messageCount > lastMessageCount) //Messages increasing
                        jobCountFactor++; //Apply increment rule
                    else
                        jobCountFactor = 1; //Or just reset it
                }
                lastMessageCount = messageCount;
                return new JobScalingResult(NextCheck, jobCountFactor);
            }
            else
                return new JobScalingResult(NextCheck);
        }
    }
}