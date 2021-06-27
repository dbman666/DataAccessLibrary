using System.Text;
using System.Text.RegularExpressions;
using Coveo.Dal;

namespace fe
{
    public enum StringType
    {
        Unknown,
        ClusterAgent, // NVirginia-Production-LinuxAgent-8vfjoi6v; contains '-LinuxAgent-'
        ClusterIndexer, // 1105mediainc-s6m4h6a3z45cqdskn4wsjicfc4-Indexer-1-toxfdw663puboxopo2dogpkcqe; 4 dashes, contains '-Indexer-'
        ClusterSecurityCache, // 1105mediainc-s6m4h6a3z45cqdskn4wsjicfc4-Indexer-1-toxfdw663puboxopo2dogpkcqe-SecurityCache; 5 dashes, ends with '-SecurityCache'
        RabbitExchange, //richfortiersqstestsebsvmbci-pawgpmgl-default-Doc; ends with '-Doc'
        RabbitQueue, // richfortiersqstestsebsvmbci-pawgpmgl-default-Dpm; ens with '-Dpm'
        SqsQueue, // richfortiersqstestsebsvmbci-pawgpmgl-default-sqs; ends with '-sqs'
        Organization, // no dashes; often ends with 8 disambiguating chars
        OrgCluster_Extension_Source, // 1105mediainc-s6m4h6a3z45cqdskn4wsjicfc4; 1 dash, ends with 26 base64 chars
            // Source: 7summitsdemohubdqynzibx-wpyit757pmh5dcx4e7yufskkwy; same as OrganizationCluster, but orgid and random base64 may be swapped
        Task_Schedule, // t5dlwm6fh5hb3jkf2n2kk5kpnq; 26 base64 chars
        Component, // Connectors@8.0.2690.12@Windows
        ComponentVersion, // 8.0.2690.12
        Ip, // 4 numbers, but ComponentVersion MUST start with an '8'
        Activity, // b93b740885af4f349880da6682cf7aab; 32 base64 chars
        // Subscription: sadkfv8tu384jgvi31kfmgb34; base64 chars (I have 6, 25, 26, 27 in prod !!)
        Job, // 5515682; all digits 
        Ec2, // i-0d130b83800376dc5; starts with 'i-'
        Host, // NPra-ALijlhw7hs; 4 letters then '-AL' (do we still have 'W' ?) then 8 base64 chars; first letter is 'N', but that might be region-specific; [1..3] are 'dev/sta/prd/hip'
            // may have .xxx.cloud.coveo.com
        Email,
        // Enums are worth it too ? I guess it's easy because we could keep them all in a dictionary (as long as there are no duplicates amongst enums)
        EpochSeconds, // 10 digits
        EpochMillis, // 13 digits
        Uri, // contains '://'
    }

    public static class FeUtil
    {
        static Regex COMPONENT_REGEX = new Regex(@"^[a-zA-Z]+@[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+@[a-zA-Z]+$", RegexOptions.Compiled);
        static Regex FOUR_NUMBERS_REGEX = new Regex(@"^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$", RegexOptions.Compiled);
        static Regex EMAIL_REGEX = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.Compiled);
        static Regex ORGID_REGEX = new Regex(@"^[a-z0-9]+$", RegexOptions.Compiled);

        public static string ToOrgId(string p_Id)
        {
            var pos = p_Id.IndexOf(Ctes.CHAR_DASH);
            return pos == -1 ? p_Id : p_Id.Substring(0, pos);
        }

        public static string ToOrgClusterId(string p_Id)
        {
            int pos = p_Id.IndexOf('-');
            if (pos == -1) return null;
            pos = p_Id.IndexOf('-', pos + 1);
            return pos == -1 ? null : p_Id.Substring(0, pos);
        }
        
        public static string ShortenOrgId(this string p_Id, string p_Replacement)
        {
            // I need 2 digits in the last 8 chars, otherwise I find it too risky.
            var len = p_Id.Length;
            if (len < 9) return p_Id;
            var nbDigits = 0;
            foreach (char c in p_Id.Substring(len - 8))
                if (char.IsDigit(c))
                    ++nbDigits;
            return nbDigits > 1 ? p_Id.Substring(0, len - 8) + p_Replacement : p_Id;
        }

        public static string ShortenSourceName(string p_Name, int p_MaxLenWithEllipsis = -1)
        {
            if (p_Name == null)
                return null;
            p_Name = p_Name
                .Replace("Coveo_master_index", "CMI")
                .Replace("Coveo_web_index", "CWI")
                .Replace("Coveo_pubweb_index", "CPWI")
                .Replace("ac_products_master_index", "acMI")
                .Replace("ac_products_web_index", "acWI");
            if (p_MaxLenWithEllipsis == -1)
                return p_Name;
            if (p_Name.Length > p_MaxLenWithEllipsis)
                p_Name = p_Name.Substring(0, p_MaxLenWithEllipsis) + "...";
            return p_Name;
        }

        public static string ShortenSourceType(string p_Type)
        {
            return p_Type?.Replace("Connector.", "");
        }

        public static string GetShortSourceId(string p_SourceId, string p_OrgId)
        {
            return p_SourceId.StartsWith(p_OrgId) ? "{o}" + p_SourceId.Substring(p_OrgId.Length) : (p_SourceId.EndsWith(p_OrgId) ? p_SourceId.Substring(0, p_SourceId.Length - p_OrgId.Length) + "{o}" : p_SourceId);
        }

        public static string GetShortSecurityProviderType(SecurityProviderType p_Type)
        {
            switch (p_Type) {
            case SecurityProviderType.GOOGLE_DRIVE_DOMAIN_WIDE: return "GDRIVE_DW";
            case SecurityProviderType.ACTIVE_DIRECTORY: return "AD";
            }
            return p_Type.ToString();
        }

        public static string GetSecProvShortName(string p_Name, string p_OrgId, SecurityProviderType p_Type)
        {
            return p_Name
                .Replace("Expanded Sitecore Security Provider for", "ESSPf")
                .Replace("Sitecore Security Provider for", "SSPf")
                .Replace("ClaimsForSharePointOnline-", "CFSPO-")
                .Replace("-SHAREPOINT_ONLINE-", "-SP_O-")
                .Replace(p_Type.ToString(), "{SP}")
                .Replace("-" + p_OrgId, "-{orgid}");
        }

        // Maybe return both an enum and an enum value. So that normal enums can be dealt with here.
        public static StringType DeduceType(string p_Str)
        {
            var len = p_Str.Length;
            var posDash1 = p_Str.IndexOf('-');
            if (posDash1 == -1) {
                if (long.TryParse(p_Str, out _)) {
                    if (len == 10) return StringType.EpochSeconds;
                    if (len == 13) return StringType.EpochMillis;
                    return StringType.Job;
                }
                if (CmfUtil.IsBase64(p_Str)) {
                    if (len == 26) return StringType.Task_Schedule;
                    if (len == 32) return StringType.Activity;
                }
                var posAt = p_Str.IndexOf('@');
                if (posAt != -1 && IsComponent(p_Str)) return StringType.Component;
                var posDot = p_Str.IndexOf('.');
                if (posDot != -1 && IsFourNumbers(p_Str))
                    return p_Str[0] == '8' ? StringType.ComponentVersion : StringType.Ip;
                if (posAt != -1 && posDot != -1 && IsEmail(p_Str)) return StringType.Email;
            } else {
                var posDash2 = p_Str.IndexOf('-', posDash1 + 1);
                if (posDash2 == -1) {
                    if (posDash1 == 1 && p_Str[0] == 'i') return StringType.Ec2;
                    if (posDash1 == 4 && len > 5 && p_Str[5] == 'A') return StringType.Host;
                } else {
                    if (p_Str.EndsWith("-Doc")) return StringType.RabbitExchange;
                    if (p_Str.EndsWith("-Dpm")) return StringType.RabbitQueue;
                    if (p_Str.EndsWith("-sqs")) return StringType.SqsQueue;
                    if (p_Str.EndsWith("-SecurityCache")) return StringType.ClusterSecurityCache;
                    if (p_Str.Contains("-LinuxAgent-")) return StringType.ClusterAgent;
                    if (p_Str.Contains("-Indexer-")) return StringType.ClusterIndexer;
                }
                var len1 = posDash1;
                var len2 = len - posDash1 - 1;
                if (len2 == 8 || len2 == 26 || len1 == 26) return StringType.OrgCluster_Extension_Source;
            }
            if (p_Str.Contains("://")) return StringType.Uri;
            // todo - Also handle enums
            if (IsOrgId(p_Str)) return StringType.Organization;
            return StringType.Unknown;
        }
            
        public static bool IsComponent(string p_Str)
        {
            return COMPONENT_REGEX.IsMatch(p_Str);
        }

        public static bool IsFourNumbers(string p_Str)
        {
            return FOUR_NUMBERS_REGEX.IsMatch(p_Str);
        }

        public static bool IsEmail(string p_Str)
        {
            return EMAIL_REGEX.IsMatch(p_Str);
        }

        public static bool IsOrgId(string p_Str)
        {
            return ORGID_REGEX.IsMatch(p_Str);
        }
    }

    public class Sched
    {
        public int? DayOfTheMonth;
        public string DaysOfWeek;
        public bool? Enabled;
        public int? Hour;
        public int? HourOccurrence;
        public int? Minute;
        public int? MinuteOccurrence;
        public int? MonthOccurrence;
        public OperationType RefreshType;
        public ScheduleFrequencyType FrequencyType;

        public string ToHtml()
        {
            var enabled = Enabled ?? false;
            var sb = new StringBuilder();
            if (!enabled) sb.Append("<strike>");
            //sb.AppendFormat("{0} ", RefreshType);
            switch (FrequencyType) {
            case ScheduleFrequencyType.MINUTELY:
                if (MinuteOccurrence == 1) sb.Append("every minute");
                else sb.AppendFormat("every {0} minutes", MinuteOccurrence);
                break;
            case ScheduleFrequencyType.HOURLY:
                if (HourOccurrence == 1) sb.Append(" every hour");
                else sb.AppendFormat("every {0} hours", HourOccurrence);
                break;
            case ScheduleFrequencyType.DAILY:
                sb.Append("every day");
                break;
            case ScheduleFrequencyType.WEEKLY:
                sb.Append("every ").Append(DaysOfWeekToHml(DaysOfWeek));
                break;
            case ScheduleFrequencyType.MONTHLY:
                if (MonthOccurrence == 1) sb.Append("every month");
                else sb.AppendFormat("every {0} months", MonthOccurrence);
                if (DayOfTheMonth != null)
                    sb.AppendFormat(" on the {0}{1}", DayOfTheMonth, NthSuffix(DayOfTheMonth.Value));
                break;
            }
            if (Hour.HasValue || Minute.HasValue) sb.AppendFormat(" at {0:D2}:{1:D2}", Hour, Minute);
            //sb.AppendFormat(" <small>(next in {0})</small>", GetNextFireTimeAfter(DateTime.Now)); // 1- changes too often, and 2- not sure it's relevant with what stoussaint did
            if (!enabled) sb.Append("</strike>");
            return sb.ToString();
        }

        public static string NthSuffix(int p_I)
        {
            switch (p_I) {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
            }
        }

        public static string DaysOfWeekToHml(string p_Days)
        {
            // Modify ["WEDNESDAY","SATURDAY","SUNDAY"]
            return p_Days.Substring(1, p_Days.Length - 2).ToLower().Replace("\"", "");
        }

        public string GetCronExpression()
        {
            // We could pass a 'secondOffset', but don't. See Java.
// @formatter:off
            switch (FrequencyType) {
            case ScheduleFrequencyType.MINUTELY: return $"0 0/{MinuteOccurrence} * 1/1 * ? *";
            case ScheduleFrequencyType.HOURLY:   return $"0 0 */{HourOccurrence} * * ?";
            case ScheduleFrequencyType.DAILY:    return $"0 {Minute} {Hour} * * ? *";
            case ScheduleFrequencyType.WEEKLY:   return $"0 {Minute} {Hour} ? * {DaysOfWeekToHml(DaysOfWeek)}";
            case ScheduleFrequencyType.MONTHLY:  return $"0 {Minute} {Hour} {DayOfTheMonth} 1/{MonthOccurrence} ? *";
            default:
                return null;
            }
// @formatter:on
        }
    }
}