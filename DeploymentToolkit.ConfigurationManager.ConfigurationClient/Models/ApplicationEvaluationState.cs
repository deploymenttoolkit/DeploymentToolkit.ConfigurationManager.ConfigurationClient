namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public enum ApplicationEvaluationState : uint
    {
        Unknown,
        Enforced,
        NotRequired,
        ApplicationForEnforcement,
        EnforcementFailed,
        Evaluating,
        DownloadingContent,
        WaitingforDependenciesDownload,
        WaitingforServiceWindow,
        WaitingforReboot,
        WaitingToEnforce,
        EnforcingDependencies,
        Enforcing,
        SoftRebootPending,
        HardRebootPending,
        PendingUpdate,
        EvaluationFailed,
        WaitingUserReconnect,
        WaitingforUserLogoff,
        WaitingforUserLogon,
        InProgressWaitingRetry,
        WaitingforPresModeOff,
        AdvanceDownloadingContent,
        AdvanceDependenciesDownload,
        DownloadFailed,
        AdvanceDownloadFailed,
        DownloadSuccess,
        PostEnforceEvaluation
    }

}
