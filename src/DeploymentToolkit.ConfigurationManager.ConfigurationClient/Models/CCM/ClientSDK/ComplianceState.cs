namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;

public enum ComplianceState : uint
{
    ciNotPresent,
    ciPresent,
    ciPresenceUnknown, // (also used for not applicable)
    ciEvaluationError,
    ciNotEvaluated,
    ciNotUpdated,
    ciNotConfigured
}
