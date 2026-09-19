using System;

namespace CodexUsageTrayLite.Models
{
    internal sealed class UsageSnapshot
    {
        public UserLevel userLevel { get; set; }
        public bool? fiveHourLimitApplies { get; set; }
        public int? fiveHourRemainingPercent { get; set; }
        public int? weeklyRemainingPercent { get; set; }
        public DateTime? fiveHourResetAt { get; set; }
        public DateTime? weeklyResetAt { get; set; }
        public int? availableUsageResetCount { get; set; }
        public DateTime lastSuccessfulFetchAt { get; set; }

        internal bool IsFiveHourLimitApplicable
        {
            get { return fiveHourLimitApplies ?? fiveHourRemainingPercent.HasValue; }
        }

        internal bool? KnownFiveHourLimitApplicability
        {
            get
            {
                if (fiveHourLimitApplies.HasValue) return fiveHourLimitApplies.Value;
                return fiveHourRemainingPercent.HasValue ? (bool?)true : null;
            }
        }

        internal bool fiveHourLimitAbsenceInferred { get; set; }

        public UsageSnapshot Clone()
        {
            return (UsageSnapshot)MemberwiseClone();
        }
    }
}
