namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
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
}
