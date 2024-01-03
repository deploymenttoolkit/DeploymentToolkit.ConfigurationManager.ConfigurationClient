namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;

public enum SoftwareUpdateEvaluationState : uint
{
    ciJobStateNone,
    ciJobStateAvailable,
    ciJobStateSubmitted,
    ciJobStateDetecting,
    ciJobStatePreDownload,
    ciJobStateDownloading,
    ciJobStateWaitInstall,
    ciJobStateInstalling,
    ciJobStatePendingSoftReboot,
    ciJobStatePendingHardReboot,
    ciJobStateWaitReboot,
    ciJobStateVerifying,
    ciJobStateInstallComplete,
    ciJobStateError,
    ciJobStateWaitServiceWindow,
    ciJobStateWaitUserLogon,
    ciJobStateWaitUserLogoff,
    ciJobStateWaitJobUserLogon,
    ciJobStateWaitUserReconnect,
    ciJobStatePendingUserLogoff,
    ciJobStatePendingUpdate,
    ciJobStateWaitingRetry,
    ciJobStateWaitPresModeOff,
    ciJobStateWaitForOrchestration,
}