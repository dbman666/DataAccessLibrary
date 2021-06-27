using Amazon;
using Amazon.Runtime;
using Amazon.SimpleSystemsManagement;

namespace Coveo.Dal
{
    public class SsmClient
    {
        private AWSCredentials _AWSCredentials;
        private RegionEndpoint _regionEndpoint;
        private AmazonSimpleSystemsManagementClient _client;

        public AmazonSimpleSystemsManagementClient Client => _client ??= _AWSCredentials == null ? new AmazonSimpleSystemsManagementClient(_regionEndpoint) : new AmazonSimpleSystemsManagementClient(_AWSCredentials, _regionEndpoint);

        public SsmClient(AWSCredentials p_AWSCredentials, RegionEndpoint p_RegionEndpoint)
        {
            _AWSCredentials = p_AWSCredentials;
            _regionEndpoint = p_RegionEndpoint;
        }

        public SsmClient(RegionEndpoint p_RegionEndpoint)
        {
            _regionEndpoint = p_RegionEndpoint;
        }
    }
}