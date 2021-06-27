using System.Collections.Generic;
using System.Text;
using Coveo.Dal;

namespace fe
{
    [Table("SourceService.Mapping")]

    public class Mapping
    {
        private static List<RulePart> EMPTY_PARTS = new ();
        private static List<MetaData> EMPTY_MATCHES = new ();
        
        public string FieldName;
        [MappingId] public string Id;
        public string Rules;
        [SourceId] public string SourceId;
        public string Type;
        public List<MetaData> MetaDatas;
        public List<RulePart> RuleParts;

        [Pk] [Computed] public string FullPk;

        [Edge_Source_Mapping] public Source Source;

        public void PostLoad()
        {
            FullPk = SourceId + Ctes.SEP_PK + Id;
            if (Rules != null)
                ParseRule(Rules, out RuleParts, out MetaDatas);
            RuleParts ??= EMPTY_PARTS;
            MetaDatas ??= EMPTY_MATCHES;
        }

        public static void ParseRule(string rule, out List<RulePart> ruleParts, out List<MetaData> metaDatas)
        {
            ruleParts = null;
            metaDatas = null;
            var posInRule = 0;
            while (posInRule < rule.Length) {
                var posOpen = rule.IndexOf("%[", posInRule);
                // Np more '%['
                if (posOpen == -1)
                    break;
                if (posOpen != posInRule) {
                    ruleParts ??= new();
                    ruleParts.Add(new RuleLiteral{Literal = rule.Substring(posInRule, posOpen - posInRule), pos1 = posInRule, pos2 = posOpen - posInRule});
                }
                var posName = posOpen + 2;
                var posClose = rule.IndexOf(']', posName);
                // No end ']', or empty meta name ('%[]')
                if (posClose == -1 || posName == posClose)
                    break;
                var posColon = rule.LastIndexOf(':', posClose);
                string name;
                string origin = null;
                if (posColon != -1 && posColon >= posName) {
                    name = rule.Substring(posName, posColon - posName);
                    ++posColon;
                    if (posColon < posClose)
                        origin = rule.Substring(posColon, posClose - posColon);
                } else {
                    name = rule.Substring(posName, posClose - posName);
                }
                posInRule = posClose + 1;

                if (string.IsNullOrEmpty(name))
                    continue;
                metaDatas ??= new();
                var md = new MetaData(name, origin) {pos1 = posOpen, pos2 = posClose + 1};
                metaDatas.Add(md);
                ruleParts ??= new();
                ruleParts.Add(md);
            }
            if (posInRule != rule.Length) {
                ruleParts ??= new();
                ruleParts.Add(new RuleLiteral{Literal = rule.Substring(posInRule), pos1 = posInRule, pos2 = rule.Length - posInRule});
            }
        }

        public static void TestParseRules()
        {
            CompareRules(null, "");
            CompareRules(null, "%[]");
            CompareRules(null, "%[:]");
            CompareRules(new List<MetaData>{new ("a", null)}, "%[a]");
            CompareRules(new List<MetaData>{new ("a", "b")}, "%[a:b]");
            CompareRules(new List<MetaData>{new ("a", null)}, "allo: %[a]");
            CompareRules(new List<MetaData>{new ("a", null)}, "%[a:]");
            CompareRules(new List<MetaData>{new ("a", "b"), new ("c", "d")}, "%[a:b] %[c:d]");
        }

        public static void CompareRules(List<MetaData> expected, string rule)
        {
            ParseRule(rule, out var _, out var actual);
            if (expected == null) {
                if (actual != null) throw new DalException($"CompareRules: Rule '{rule}': expected 'null' but got '{ToString(actual)}'.");
                return;
            }
            if (actual == null) throw new DalException($"CompareRules: Rule '{rule}': expected '{ToString(expected)}' but got 'null'.");
            var nb = expected.Count;
            if (nb != actual.Count) throw new DalException($"CompareRules: Rule '{rule}': expected '{ToString(expected)}' but got '{ToString(actual)}'.");
            for (var i = 0; i < nb; ++i) {
                var exp = expected[i];
                var act = actual[i];
                if (!exp.Same(act)) throw new DalException($"CompareRules: Rule '{rule}': expected '{exp}' but got '{act}'.");
            }
        }

        public static string ToString(List<MetaData> matches)
        {
            if (matches == null)
                return "null";
            var sb = new StringBuilder();
            sb.Append('[');
            var i = 0;
            foreach (var match in matches)
                sb.CommaSpace(i++).Append(match);
            sb.Append(']');
            return sb.ToString();
        }
    }

    public class MappingRule
    {
        public string Rule;
        public List<MetaData> MetaDatas;
    }

    public class RulePart
    {
        public int pos1;
        public int pos2;
    }

    public class RuleLiteral : RulePart
    {
        public string Literal;
    }
    
    public class MetaData : RulePart
    {
        public string Name;
        public string Origin;

        public MetaData(string name, string origin)
        {
            Name = name;
            Origin = origin;
        }

        public override string ToString()
        {
            return Origin == null ? Name : Name + ':' + Origin;
        }

        public bool Same(MetaData that)
        {
            return Name == that.Name && Origin == that.Origin;
        }
    }
}