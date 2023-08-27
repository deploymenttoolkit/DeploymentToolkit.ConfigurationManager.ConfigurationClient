using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM
{
    public partial class CCM_ClientIdentificationInformation : ObservableObject, IWindowsManagementInstrumentationStaticInstance
    {
        public string Namespace => CCM_Constants.ClientNamespace;
        public string Class => nameof(CCM_ClientIdentificationInformation);

        [ObservableProperty]
        private string _clientId;
        [ObservableProperty]
        private string _hardwareIDFlex;
        [ObservableProperty]
        private string _hardwareID1;
        [ObservableProperty]
        private string _hardwareID2;
        [ObservableProperty]
        private string _hardwareID3;
        [ObservableProperty]
        private string _serializedSigningCertificate;
        [ObservableProperty]
        private string _encodedSigningPublicKey;
        [ObservableProperty]
        private string _serializedEncryptingCertificate;
        [ObservableProperty]
        private string _encodedEncryptingPublicKey;
        [ObservableProperty]
        private string _machineSID;
        [ObservableProperty]
        private string _sMBIOSSerialNumber;
        [ObservableProperty]
        private bool _approved;
        [ObservableProperty]
        private uint _approvalRetry = 0;
        [ObservableProperty]
        private uint _approvalStatus = 0;
        [ObservableProperty]
        private string _reservedString1;
        [ObservableProperty]
        private string _reservedString2;
        [ObservableProperty]
        private string _reservedString3;
        [ObservableProperty]
        private uint _reservedUint1;
        [ObservableProperty]
        private uint _reservedUint2;
    }
}
