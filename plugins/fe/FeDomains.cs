using Coveo.Dal;

namespace fe
{
    public class ActivityIdAttribute : StringAttribute { }
    public class AgentIdAttribute : StringAttribute { }
    public class ComponentVersionAttribute : StringAttribute { }
    public class DefaultSubscriptionIdAttribute : StringAttribute { }
    public class Ec2IdAttribute : StringAttribute { }
    public class EmailAttribute : StringAttribute { }
    public class ExtensionIdAttribute : StringAttribute { }
    public class GlobalExtensionIdAttribute : StringAttribute { }
    public class HostAttribute : StringAttribute { }
    public class IndexIdAttribute : StringAttribute { }
    public class InstanceAvailabilityZoneAttribute : StringAttribute { }
    public class InstanceIdAttribute : StringAttribute { }
    public class InstanceTypeAttribute : StringAttribute { } // todo-poco - this should be 'SecurityProvider.SimpleClaims' and friends; revisit when we have a 'From/ToValueString' ?
    public class JobIdAttribute : StringAttribute { } // Should be in jobPlugin, but Task refers to Job that refers to OrgId and such
    public class LicenseTemplateNameAttribute : StringAttribute { }
    public class MaestroVersionAttribute : StringAttribute { }
    public class MappingIdAttribute : StringAttribute { }
    public class OrgClusterIdAttribute : StringAttribute { }
    public class OrgIdAttribute : StringAttribute { }
    public class OrgTemplateNameAttribute : StringAttribute { }
    public class PasswordAttribute : StringAttribute { }
    public class PortAttribute : IntAttribute { }
    public class ProviderIdAttribute : StringAttribute { }
    public class RabbitServerIdAttribute : StringAttribute { }
    public class Schedule_SecurityCacheIdAttribute : StringAttribute { }
    public class Schedule_SourceIdAttribute : StringAttribute { }
    public class SourceIdAttribute : StringAttribute { }
    public class StringListAttribute : StringAttribute { }
    public class StringSetAttribute : StringAttribute { }
    public class SubscriptionIdAttribute : StringAttribute { }
    public class Task_IndexIdAttribute : StringAttribute { }
    public class Task_PlatformIdAttribute : StringAttribute { }
    public class Task_SecurityCacheIdAttribute : StringAttribute { }
    public class Task_SourceIdAttribute : StringAttribute { }
    public class UriAttribute : StringAttribute { }
    public class OperationIdAttribute : StringAttribute { }
    public class TaskIdAttribute : StringAttribute { }
}
