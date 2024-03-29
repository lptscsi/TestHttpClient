﻿using System;

namespace TestWorker.Http.Policy
{
    public class PolicyOptions
    {
        public const string PoliciesConfigurationSectionName = "Policies";

        public TimeSpan Timeout { get; set; }

        public CircuitBreakerPolicyOptions HttpCircuitBreaker { get; set; }

        public RetryPolicyOptions HttpRetry { get; set; }
    }
}
