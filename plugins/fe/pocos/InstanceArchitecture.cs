namespace fe
{
    public enum InstanceArchitecture
    {
        T3_MICRO,
        T3_SMALL,
        T3_MEDIUM,
        T3_LARGE,
        T3_XLARGE,
        T3_2XLARGE,
        T3_NANO_UNLIMITED,
        T3_MICRO_UNLIMITED,
        T3_SMALL_UNLIMITED,
        T3_MEDIUM_UNLIMITED,
        T3_LARGE_UNLIMITED,
        T3_XLARGE_UNLIMITED,
        T3_2XLARGE_UNLIMITED,
        T3A_MICRO,
        T3A_SMALL,
        T3A_MEDIUM,
        T3A_LARGE,
        T3A_XLARGE,
        T3A_2XLARGE,
        T3A_NANO_UNLIMITED,
        T3A_MICRO_UNLIMITED,
        T3A_SMALL_UNLIMITED,
        T3A_MEDIUM_UNLIMITED,
        T3A_LARGE_UNLIMITED,
        T3A_XLARGE_UNLIMITED,
        T3A_2XLARGE_UNLIMITED,
        M3_MEDIUM,
        M5_LARGE,
        M5_XLARGE,
        M5_2XLARGE,
        M5_4XLARGE,
        M5_8XLARGE,
        M5_12XLARGE,
        M5_16XLARGE,
        M5_24XLARGE,
        M5A_LARGE,
        M5A_XLARGE,
        M5A_2XLARGE,
        M5A_4XLARGE,
        M5A_8XLARGE,
        M5A_12XLARGE,
        M5A_16XLARGE,
        M5A_24XLARGE,
        C5_LARGE,
        C5_XLARGE,
        C5_2XLARGE,
        C5_4XLARGE,
        C5_9XLARGE,
        C5_12XLARGE,
        C5_18XLARGE,
        C5_24XLARGE,
        C5A_LARGE,
        C5A_XLARGE,
        C5A_2XLARGE,
        C5A_4XLARGE,
        C5A_8XLARGE,
        C5A_12XLARGE,
        C5A_16XLARGE,
        C5A_24XLARGE,
        R5_LARGE,
        R5_XLARGE,
        R5_2XLARGE,
        R5_4XLARGE,
        R5_8XLARGE,
        R5_12XLARGE,
        R5_16XLARGE,
        R5_24XLARGE,
        R5A_LARGE,
        R5A_XLARGE,
        R5A_2XLARGE,
        R5A_4XLARGE,
        R5A_8XLARGE,
        R5A_12XLARGE,
        R5A_16XLARGE,
        R5A_24XLARGE,

        //@Deprecated C3_LARGE("c3.large"),
        //@Deprecated C3_XLARGE("c3.xlarge"),
        //@Deprecated C3_2XLARGE("c3.2xlarge"),
        //@Deprecated C3_4XLARGE("c3.4xlarge"),
        //@Deprecated C3_8XLARGE("c3.8xlarge"),
        //@Deprecated C4_LARGE("c4.large", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated C4_XLARGE("c4.xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated C4_2XLARGE("c4.2xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated C4_4XLARGE("c4.4xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated C4_8XLARGE("c4.8xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated M3_LARGE("m3.large"),
        //@Deprecated M3_XLARGE("m3.xlarge"),
        //@Deprecated M3_2XLARGE("m3.2xlarge"),
        //@Deprecated M4_LARGE("m4.large", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated M4_XLARGE("m4.xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated M4_2XLARGE("m4.2xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated M4_4XLARGE("m4.4xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated M4_10XLARGE("m4.10xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated R3_LARGE("r3.large"),
        //@Deprecated R3_XLARGE("r3.xlarge"),
        //@Deprecated R3_2XLARGE("r3.2xlarge"),
        //@Deprecated R3_4XLARGE("r3.4xlarge"),
        //@Deprecated R3_8XLARGE("r3.8xlarge"),
        //@Deprecated R4_LARGE("r4.large", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated R4_XLARGE("r4.xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated R4_2XLARGE("r4.2xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated R4_4XLARGE("r4.4xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated R4_8XLARGE("r4.8xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated R4_16XLARGE("r4.16xlarge", new InstanceArchitectureOptions().withEbsOptimized(true)),
        //@Deprecated T2_NANO("t2.nano"),
        //@Deprecated T2_MICRO("t2.micro"),
        T2_SMALL,
        T2_MEDIUM,
        //@Deprecated T2_LARGE("t2.large"),
        //@Deprecated T2_XLARGE("t2.xlarge"),
        //@Deprecated T2_2XLARGE("t2.2xlarge"),
        //@Deprecated T2_NANO_UNLIMITED("t2.nano", new InstanceArchitectureOptions().withUnlimitedCpuCredits()),
        //@Deprecated T2_MICRO_UNLIMITED("t2.micro", new InstanceArchitectureOptions().withUnlimitedCpuCredits()),
        //@Deprecated T2_SMALL_UNLIMITED("t2.small", new InstanceArchitectureOptions().withUnlimitedCpuCredits()),
        //@Deprecated T2_MEDIUM_UNLIMITED("t2.medium", new InstanceArchitectureOptions().withUnlimitedCpuCredits()),
        //@Deprecated T2_LARGE_UNLIMITED("t2.large", new InstanceArchitectureOptions().withUnlimitedCpuCredits()),
        //@Deprecated T2_XLARGE_UNLIMITED("t2.xlarge", new InstanceArchitectureOptions().withUnlimitedCpuCredits()),
        //@Deprecated T2_2XLARGE_UNLIMITED("t2.2xlarge", new InstanceArchitectureOptions().withUnlimitedCpuCredits()),
        //@Deprecated T3_NANO("t3.nano", new InstanceArchitectureOptions().withEbsOptimized(true).withStandardCpuCredits()),
        //@Deprecated T3A_NANO("t3a.nano", new InstanceArchitectureOptions().withEbsOptimized(true).withStandardCpuCredits());
    }
}
