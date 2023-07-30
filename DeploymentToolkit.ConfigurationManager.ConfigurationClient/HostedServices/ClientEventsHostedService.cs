using CommunityToolkit.Mvvm.Messaging;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using Microsoft.Extensions.Hosting;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.HostedServices
{
    public class ClientEventsHostedService : IHostedService
    {
        private readonly ClientEventsService _clientEventsService;

        public ClientEventsHostedService(ClientEventsService clientEventsService)
        {
            _clientEventsService = clientEventsService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _clientEventsService.Events.CollectionChanged += Events_CollectionChanged;
            _clientEventsService.InstanceEvents.CollectionChanged += InstanceEvents_CollectionChanged;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            _clientEventsService.Events.CollectionChanged -= Events_CollectionChanged;
            _clientEventsService.InstanceEvents.CollectionChanged -= InstanceEvents_CollectionChanged;
            return Task.CompletedTask;
        }

        private void InstanceEvents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            foreach(InstanceEvent instanceEvent in e.NewItems)
            {
                Debug.WriteLine($"Processing Instance Event: {instanceEvent.ClassName}");

                switch (instanceEvent.ClassName)
                {
                    case "CCM_SoftwareUpdate":
                    {
                        WeakReferenceMessenger.Default.Send(new CCMSoftwareUpdateMessage(instanceEvent));
                    }
                    break;
                }
            }
        }

        private void Events_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            foreach (CcmEvent item in e.NewItems)
            {
                Debug.WriteLine($"Processing Event: {item.ClassName}");

                switch (item.ClassName)
                {
                    /*
                     * Dump Classes:
                     * Get-CimClass -Namespace "ROOT\ccm\Events" | Where-Object { $_.CimClassName -notlike "__*" -and $_.CimClassName -notlike "MSFT_*" } | Select-Object CimClassName
                     * Classes at 
                    CCM_Event
                    CCM_ServiceHost_CertRetrieval_Status
                    CCM_Framework_ClientIdChanged
                    SMS_Event
                    SMS_StatusMessage
                    SMS_Framework_StatusMessage
                    SMS_RemoteClient_ClientIdUpdated
                    SMS_ServiceHost_CertificateUpdateFailure
                    SMS_ServiceHost_CertificateOperationsFailure
                    SMS_ServiceHost_LowMemoryMode_Enter
                    SMS_ServiceHost_CertificateUpdateSuccess
                    SMS_ServiceHost_LowMemoryMode_Recover
                    SMS_Client_StatusMessage
                    SMS_RemoteClient_Reassigned
                    SMS_DCM_StatusMessage
                    SMS_DCM_CIDownloadTimeout
                    SMS_DCM_CIHashMismatch
                    SMS_DCM_CIDownloadFailed
                    SMS_DCM_SdmStatusMessage
                    SMS_DCM_SdmCompliant
                    SMS_DCM_SdmNonCompliant_Warn
                    SMS_DCM_SdmNonCompliant_Error
                    SMS_DCM_ModelViolations
                    SMS_DCM_DiscoveryProcessFailure
                    SMS_DCM_SdmEvaluationFailed
                    SMS_DCM_SdmNonCompliant_Info
                    SMS_DCM_DiscoveryViolations
                    SMS_DCM_SdmDownloadTimeout
                    SMS_DCM_SdmDownloadFailed
                    SMS_DCM_PackageDecompressionFailure
                    SMS_DCM_SdmReportingFailed
                    SMS_DCM_SdmHashMismatch
                    SMS_DCM_DiscoveryLaunchFailure
                    SMS_DCM_PackageInternalFailure
                    SMS_DCM_CIInternalFailure
                    SMS_DCM_DotNetMissing
                    SMS_DCM_CIDecompressionFailure
                    SMS_AppClient_StatusMessage
                    SMS_AppClient_FailureMessage
                    SMS_AppClient_EnforceFailure
                    SMS_AppClient_LaunchFailure
                    SMS_AppClient_EvaluationFailure
                    SMS_AppClient_ContentDownloadFailure
                    SMS_ClientReboot_SuspendBitLocker_Failure
                    DmClientDeployment_Info
                    DmClientDeployment_Info_Verify_Success_Certificate_Enrollment_Not_Required
                    DmClientDeployment_Info_Uninstall_Success
                    DmClientDeployment_Info_Upgrade_Success
                    DmClientDeployment_Info_Verify_Success
                    DmClientDeployment_Info_Newer_Client
                    DmClientDeployment_Info_Install_Success
                    DmClientDeployment_Info_Upgrade_Success_Certificate_Enrollment_Not_Required
                    DmClientDeployment_Info_No_Action
                    DmClientDeployment_Info_Install_Success_Certificate_Enrollment_Not_Required
                    CCM_LocationServices_WebServiceSigningCertificate_VerificationFailure
                    SMS_PolicyAgent_PolicyAuthorizationFailure
                    SMS_RemoteClient_Install_PendingReboot
                    SMS_RemoteClient_Installed
                    DmClientDeployment_Error
                    DmClientDeployment_Error_Verify_Start_Client
                    DmClientDeployment_Error_Bad_Client_Version
                    DmClientDeployment_Error_CliXfer_Dir
                    DMCLIENTDEPLOYMENT_ERROR_PERFORMING_POST_CAB_INSTALL_STEPS
                    DmClientDeployment_Error_Copy
                    DmClientDeployment_Error_Install_Privileged_Certificate
                    DMCLIENTDEPLOYMENT_ERROR_ENFORCING_NEW_CLIENT
                    DmClientDeployment_Error_Invalid_Settings
                    DmClientDeployment_Error_Verify_Cab_Install
                    DmClientDeployment_Error_Cab_Install
                    DmClientDeployment_Error_Bad_Proc
                    DmClientDeployment_Error_No_Status
                    DmClientDeployment_Error_Installer_Dir
                    DMCLIENTDEPLOYMENT_ERROR_ENFORCING_OLD_CLIENT
                    DmClientDeployment_Error_Install_Cab_Install
                    DmClientDeployment_Error_Generic
                    DmClientDeployment_Error_Install_Uninstall_Previous
                    DmClientDeployment_Error_PreCommand
                    DmClientDeployment_Error_Verify_Bad_Settings
                    DmClientDeployment_Error_Client_Uninstall
                    DmClientDeployment_Error_Verify_Restore_Files
                    DmClientDeployment_Error_Install_Bad_Settings
                    DmClientDeployment_Error_Verify_Update_Settings
                    DmClientDeployment_Error_Verify_Uninstall_Previous
                    DmClientDeployment_Error_Unknown_Failure
                    DmClientDeployment_Error_Launch_Client_Setup
                    DmClientDeployment_Error_Install_Update_Settings
                    DmClientDeployment_Error_Install_Start_Client
                    DmClientDeployment_Error_Bad_OS
                    DmClientDeployment_Error_Bad_Settings
                    DmClientDeployment_Error_No_RAPI
                    DmClientDeployment_Error_No_Connection
                    DmClientDeployment_Error_PostCommand
                    SMS_PolicyAgent_PolicyMismatch
                    SMS_RemoteClient_Repaired
                    SMS_PolicyAgent_BitsPolicyDownloadFailed
                    SMS_ClientReboot_SuspendedBitLocker
                    SMS_RemoteClient_SiteSigning_AuthFailure_Revoked
                    SMS_RemoteClient_SiteSigning_AuthFailure_Trust
                    SMS_RemoteClient_Upgraded
                    SMS_RemoteClient_SiteSigning_AuthFailure_Expired
                    SMS_PolicyAgent_CompileFailure
                    DmClientDeployment_Warning
                    DmClientDeployment_Warn_Install_Success_Cert_Failed
                    DmClientDeployment_Warn_Verify_Success_Cert_Failed
                    DmClientDeployment_Warn_Upgrade_Success_Cert_Failed
                    SMS_RemoteClient_AssignmentUnchanged
                    SMS_ClientReboot_ResumeBitLocker_Failure
                    SMS_GenericStatusMessage
                    SMS_GenericStatusMessage_Warning
                    SMS_GenericStatusMessage_Error
                    SMS_GenericStatusMessage_Info
                    SMS_RemoteClient_ClientNotAssigned
                    SMS_ClientReboot_ResumedBitLocker
                    SMS_RemoteClient_Uninstalled
                    SMS_RemoteClient_ManagementPointCertificate_CrossVerificationFailure
                    SMS_Client_ComponentException
                    SMS_UnknownStatusMessage
                    SMS_RemoteClient_DataTransferService_BITS_SecureFailure
                    SMS_SofwareDistribution_Event
                    SoftDistProgramErrorMomEvent
                    SoftDistProgramErrorMIFMomEvent
                    SoftDistProgramExceededTimeMom
                    SoftDistProgramUnexpectedRebootMomEvent
                    SoftDistProgramBadEnvironmentMomEvent
                    SoftDistProgramUnableToExecuteMomEvent
                    SoftDistProgramNotMonitoredMom
                    SoftDistErrorNoContentMom
                    SoftDistErrorInternetClientCannotRunFromNetMom
                    SoftDistAdvertDownloadFailedMomEvent
                    SoftDistAdvertHashMismatchMomEvent
                    SoftDistDownloadAndLocationFailedMomEvent
                    SMS_SoftwareDistributionCacheEvent
                    SoftDistErrorInsufficientCacheMomEvent
                    SoftDistErrorInsufficientCacheSizeMomEvent
                    SoftDistErrorInsufficientCacheSizeEvent
                    SoftDistErrorInsufficientCacheEvent
                    SoftDistWarningInsufficientCacheEvent
                    SoftDistProgramShouldNeverRun
                    SoftDistProgramPrelimSuccessEvent
                    SoftDistProgramWaitingPaused
                    SoftDistWarningProgramBadEnvironmentEvent
                    SoftDistRebootPerformedOutsideOfSWEvent
                    SoftDistAdvertHashMismatchEvent
                    SoftDistProgramHasRunBeforeWithoutSucceeding
                    SoftDistProgramUnableToExecuteEvent
                    SoftDistErrorProgramMayNeverRunEvent
                    SMS_SoftwareDistribution_OfferRejectedPlatform
                    SoftDistProgramErrorEvent
                    SoftDistProgramStartedEvent
                    SoftDistOfferRejectedInvalidPolicyEvent
                    SoftDistProgramBadEnvironmentEvent
                    SoftDistProgramCancelled
                    SoftDistErrorSystemMayNeverRebootEvent
                    SoftDistProgramUnexpectedRebootEvent
                    SoftDistOfferRejectedExpiredEvent
                    SoftDistProgramCompletedSuccessfulMIFEvent
                    SoftDistProgramNotMonitored
                    SoftDistWaitingContentEvent
                    SoftDistOfferRejectedPlatformEvent
                    SoftDistProgramWaitingForUserEnvironment
                    SoftDistWaitingForServiceWindowEvent
                    SoftDistDownloadAndLocationFailedEvent
                    SoftDistWarningProgramUnableToExecuteEvent
                    SoftDistWarningAdvertDownloadFailedEvent
                    SoftDistProgramCompletedSuccessfullyEvent
                    SoftDistProgramCannotRunOnInternet
                    SoftDistProgramHasRunBefore
                    SoftDistErrorNoContent
                    SoftDistProgramWaitingForAnotherProgram
                    SoftDistErrorInternetClientCannotRunFromNet
                    SoftDistNonElevatedInstall
                    SoftDistProgramErrorMIFEvent
                    SoftDistAdvertDownloadFailedEvent
                    SoftDistWaitingContentAvailabilityOnLocalDP
                    SoftDistWarningNoContent
                    SoftDistProgramOfferReceivedEvent
                    SoftDistWarningProgramErrorEvent
                    SoftDistWarningDownloadAndLocationFailedEvent
                    SoftDistProgramExceededTime
                    SoftDistRebootWaitingForServiceWindowEvent
                    SoftDistProgramHasRunBeforeWithoutFailing
                    SMS_SUMAgentEvent
                    SMS_SUMAgentAssignmentEvent
                    SMS_SUMAgentAssignmentError_EnforcementJob
                    SMS_SUMAgentAssignmentError_EnforcementJob_Mom
                    SMS_SUMAgentAssignmentError_UpdatesFailures
                    SMS_SUMAgentAssignmentError_UpdatesFailures_Mom
                    SMS_SUMAgentAssignmentError_NonCompliance
                    SMS_SUMAgentAssignmentError_NonCompliance_Mom
                    SMS_SUMAgentAssignmentError_PostRestartDectect
                    SMS_SUMAgentAssignmentError_PostRestartDectect_Mom
                    SMS_SUMAgentAssignmentError_EnforcementInitiate
                    SMS_SUMAgentAssignmentError_EnforcementInitiate_Mom
                    SMS_SUMAgentAssignmentError_EvaluationInititate
                    SMS_SUMAgentAssignmentError_EvaluationInititate_Mom
                    SMS_SUMAgentAssignmentError_EvaluationJob
                    SMS_SUMAgentAssignmentError_EvaluationJob_Mom
                    SMS_SUMAgentAssignmentError_InvalidPolicy
                    SMS_SUMAgentAssignmentError_InvalidPolicy_Mom
                    SMS_SUMAgentAssignmentError_AdvanceDownloadInitiate
                    SMS_SUMAgentAssignmentError_AdvanceDownloadInitiate_Mom
                    SMS_SUMAgentAssignmentError_AdvanceDownloadJob
                    SMS_SUMAgentAssignmentError_AdvanceDownloadJob_Mom
                    SMS_SUMAgentAssignmentError_NoServiceWindow
                    SMS_SUMAgentAssignmentError_NoServiceWindow_Mom
                    SMS_SUMAgentUpdateEvent
                    SMS_SUMAgentUpdateError_NoScanTool
                    SMS_SUMAgentUpdateError_NoScanTool_Mom
                    SMS_SUMAgentUpdateError_ScanFailed
                    SMS_SUMAgentUpdateError_ScanFailed_Mom
                    SMS_SUMAgentUpdateError_HashMismatch
                    SMS_SUMAgentUpdateError_HashMismatch_Mom
                    SMS_SUMAgentUpdateError_NoContent
                    SMS_SUMAgentUpdateError_NoContent_Mom
                    SMS_SUMAgentUpdateError_InsufficientCache
                    SMS_SUMAgentUpdateError_InsufficientCache_Mom
                    SMS_SUMAgentUpdateError_InsufficientCacheSize
                    SMS_SUMAgentUpdateError_InsufficientCacheSize_Mom
                    SMS_SUMAgentUpdateError_DownloadFailed
                    SMS_SUMAgentUpdateError_DownloadFailed_Mom
                    SMS_SUMAgentUpdateError_InvalidCommandline
                    SMS_SUMAgentUpdateError_InvalidCommandline_Mom
                    SMS_SUMAgentUpdateError_UpdateFailed
                    SMS_SUMAgentUpdateError_UpdateFailed_Mom
                    SMS_SUMAgentUpdateError_TimeExpired
                    SMS_SUMAgentUpdateError_TimeExpired_Mom
                    SMS_SUMAgentUpdateError_CreateProcess
                    SMS_SUMAgentUpdateError_CreateProcess_Mom
                    SMS_SUMAgentUpdateError_InstallerPath
                    SMS_SUMAgentUpdateError_InstallerPath_Mom
                    SMS_SUMAgentUpdateError_ResumeProcess
                    SMS_SUMAgentUpdateError_ResumeProcess_Mom
                    SMS_SUMAgentUpdateError_Internal
                    SMS_SUMAgentUpdateError_Internal_Mom
                    SMS_SUMAgentBundleError_FailedContent
                    SMS_SUMAgentBundleError_FailedContent_Mom
                    SMS_SUMAgentBundleError_FailedExecute
                    SMS_SUMAgentBundleError_FailedExecute_Mom
                    SMS_SUMAgentBundleError_FailedEvaluate
                    SMS_SUMAgentBundleError_FailedEvaluate_Mom
                    SMS_SUMAgentUpdateError_NoServiceWindow
                    SMS_SUMAgentUpdateError_NoServiceWindow_Mom
                    SMS_ImageDeployment_Event
                    ImageDeploymentWarnEvent
                    ImageDeploymentErrorEvent
                    ImageDeploymentInfoEvent
                    SMS_MP_StatusMessage
                    CCM_ContentTranseferManager_ContentInfo_Processing_Failure
                    SoftwareDistributionPackageEvent
                    SoftDistHashMismatchEvent
                    SoftDistDownloadFailedEvent
                    SoftDistContentRemovedEvent
                    MpEvent
                    MPEvent_ServiceHost_CertificateUpdateSuccess
                    MpEvent_Error
                    MpEvent_Warning
                    MPEvent_ServiceHost_CertificateUpdateFailure
                    SMS_PatchMgmt_Event
                    SMS_PatchMgmt_Patch_Event
                    SMS_PatchMgmt_Event_User_Postponed_Install
                    SMS_PatchMgmt_Event_Auto_Postponed_Reboot
                    SMS_PatchMgmt_Event_Scan_Failed
                    SMS_PatchMgmt_Event_Warning_Early_Start
                    SMS_PatchMgmt_Event_Window_Expired_Install
                    SMS_PatchMgmt_Event_Auto_Postponed_Preinst_Reboot
                    SMS_PatchMgmt_Event_Suppressed_Reboot_Svr
                    SMS_PatchMgmt_Event_User_Rescheduled_Preinst_Reboot
                    SMS_PatchMgmt_Event_No_Assignment_Schedule
                    SMS_PatchMgmt_Event_Preinst_Reboot_Required
                    SMS_PatchMgmt_Event_User_Rescheduled_Reboot
                    SMS_PatchMgmt_Event_Install_Summary
                    SMS_PatchMgmt_Event_No_Reboot_Required
                    SMS_PatchMgmt_Event_Auto_Postponed_Install
                    SMS_PatchMgmt_Event_User_Rescheduled_Install
                    SMS_PatchMgmt_Event_User_Postponed_Reboot
                    SMS_PatchMgmt_Event_No_Scan_Path
                    SMS_PatchMgmt_Event_User_Postponed_Preinst_Reboot
                    SMS_PatchMgmt_Event_Reboot_Required
                    SMS_PatchMgmt_Event_Warning_Late_Start
                    SMS_PatchMgmt_Event_Invalid_ChangeWindow_CmdLine
                    SMS_PatchMgmt_Event_Suppressed_Reboot_Ws
                    SMS_PatchMgmt_PerPatch_Event
                    SMS_PatchMgmt_Event_Update_Reboot_Optional
                    SMS_PatchMgmt_Event_Update_Succeeded_Mandatory
                    SMS_PatchMgmt_Event_Update_Cancelled
                    SMS_PatchMgmt_Event_Update_Reboot_Mandatory
                    SMS_PatchMgmt_Event_Low_Disk_Space
                    SMS_PatchMgmt_Event_Update_Succeeded_Optional
                    SMS_PatchMgmt_Event_Update_Time_Expired
                    SMS_PatchMgmt_Event_Update_Failed
                    SMS_SUMScanAgentEvent
                    SMS_SUMScanAgentInfo_ScanStarted
                    SMS_SUMScanAgentWarning_ScanSucceededWithErrors
                    SMS_SUMScanAgentError_UnableToStart
                    SMS_SUMScanAgentError_ScanFailed
                    SMS_SUMScanAgentInfo_SuccessfulScan
                    SMS_SUMScanAgentError_ScanTimeout
                    SMS_SUMScanAgentError_UpdateSourceConfigurationFailed
                    SMS_PowerAgent_StatusMessage
                    CLIMSG_POWERAGENT_ERROR_APPLY_POWERSCHEMA_FAILURE
                    CLIMSG_POWERAGENT_INFO_POWER_POLICY_CONFLICT
                    CLIMSG_POWERAGENT_ERROR_APPLY_POWERSETTING_FAILURE
                    InventoryEvent
                    SoftwareInventoryEvent
                    CLIMSG_SINV_ERROR_COLLECTIONFAILURE
                    CLIMSG_SINV_WARNING_QUERYFAILURE
                    CLIMSG_SINV_ERROR_REPORTFAILURE
                    CLIMSG_SINV_WARNING_RESOURCES
                    CLIMSG_INV_ERROR_REPORTFAILURE
                    CLIMSG_INV_INFO_GENERIC_COLLECTED_SUCCESSFULLY
                    CLIMSG_INV_WARNING_QUERYFAILURE
                    CLIMSG_INV_ERROR_COLLECTIONFAILURE
                    FileCollectionEvent
                    CLIMSG_FILECOLL_WARNING_QUERYFAILURE
                    CLIMSG_FILECOLL_WARNING_RESOURCES
                    CLIMSG_FILECOLL_ERROR_REPORTFAILURE
                    CLIMSG_FILECOLL_WARNING_FILECOL_MAXIMUM
                    CLIMSG_FILECOLL_ERROR_COLLECTIONFAILURE
                    HardwareInventoryEvent
                    CLIMSG_HINV_ERROR_REPORTFAILURE
                    CLIMSG_HINV_WARNING_RESOURCES
                    CLIMSG_HINV_WARNING_QUERYFAILURE
                    CLIMSG_HINV_ERROR_BAD_MIF_SYNTAX
                    CLIMSG_HINV_ERROR_COLLECTIONFAILURE
                    SoftwareDistributionCacheManagerEvent
                    SoftDistCacheLocationModifiedEvent
                    SoftDistCacheSizeModifiedEvent
                    SMS_SoftwareMetering_StatusMessage
                    SWMtr_Status_Send_UsageReport_Failed
                    SWMtr_Status_Create_UsageReport_Failed
                    SWMtr_Status_WMIFailure
                    SWMtr_Status_Received_MeterRule
                    SMS_SourceListUpdate_Event
                    SourceListUpdate_LS_Warning_Event
                    SourceListUpdate_LS_Error_Event
                    SourceListUpdate_Error_Event
                    SourceListUpdate_Warning_Event
                    SourceListUpdate_Success_Event
                    CCM_RemoteClient_Reassigned
                    CCM_Framework_Event
                    CCM_Framework_Error
                    CCM_LocationServices_SiteSigning_AuthFailure_Revoked
                    CCM_LocationServices_SiteSigning_AuthFailure_Expired
                    CCM_LocationServices_SiteSigning_AuthFailure_Trust
                    CCM_LocationServices_ManagementPointCertificate_CrossVerificationFailure
                    CCM_ServiceHost_CertificateUpdateFailure
                    CCM_ServiceHost_CertificateOperationsFailure
                    CCM_Framework_Warning
                    CCM_DataTransferService_BITS_SecureFailure
                    CCM_ServiceHost_LowMemoryMode_Enter
                    CCM_Framework_Info
                    CCM_LocationServices_ProxyChanged
                    CCM_ServiceHost_LowMemoryMode_Recover
                    CCM_ServiceHost_CertificateUpdateSuccess
                    CCM_Framework_ClientIdUpdated
                    CCM_NetworkSettings_Changed
                    CCM_CcmHttp_Status
                    CCM_LocationServices_LocationBaseChange
                    CCM_DDR_REQUIRE_RESEND
                    CCM_PolicyAgent_Event
                    CCM_PolicyAgent_Info
                    CCM_PolicyAgent_AssignmentDisabled
                    CCM_PolicyAgent_SettingsEvaluationComplete
                    CCM_PolicyAgent_PolicyEvaluationComplete
                    CCM_PolicyAgent_AssignmentsRequested
                    CCM_PolicyAgent_AssignmentsReceived
                    CCM_PolicyAgent_PolicyDownloadStarted
                    CCM_PolicyAgent_PolicyDownloadSucceeded
                    CCM_PolicyAgent_PolicyRuleRevoked
                    CCM_PolicyAgent_PolicyRuleApplied
                    CCM_PolicyAgent_EvaluationSucceeded
                    CCM_PolicyAgent_PolicyRevoked
                    CCM_PolicyAgent_PolicyApplied
                    CCM_PolicyAgent_AssignmentEnabled
                    CCM_PolicyAgent_Warning
                    CCM_PolicyAgent_PolicyMismatchWarning
                    CCM_PolicyAgent_Error
                    CCM_PolicyAgent_PolicyCompileFailed
                    CCM_PolicyAgent_EvaluationFailed
                    CCM_PolicyAgent_PolicyDownloadFailed
                    CCM_PolicyAgent_PolicyAuthorizationFailure
                    CCM_PolicyAgent_PolicyRuleApplyFailed
                    CCM_PendingEvent
                    CCM_EventForwarder
                    CIM_Indication
                    CIM_ClassIndication
                    CIM_ClassDeletion
                    CIM_ClassCreation
                    CIM_ClassModification
                    CIM_InstIndication
                    CIM_InstCreation
                    CIM_InstModification
                    CIM_InstDeletion
                    CIM_Error
                    CCM_BitsDownloadMethod_ErrorInfo
                    CCM_FileCopyDownloadMethod_ErrorInfo
                    CCM_HttpDownloadMethod_ErrorInfo
                    CCM_WmiMofActionHandler_ErrorInfo
                    */
                }
            }
        }
    }
}
