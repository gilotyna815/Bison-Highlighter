using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bison_Highlighter
{
    internal static class BisonKeywords
    {
        private static readonly List<string> keywordsC = new List<string>
        {
            "auto", "break", "case", "char", "const", "continue", "default", "do", "double", "else", "enum", "extern", "float", "for", "goto", "if", "int",
            "long", "register", "return", "short", "signed", "sizeof", "static", "struct", "switch", "typedef", "union", "unsigned", "void", "volatile", "while"
        };
        private static readonly HashSet<string> keywordSetC = new HashSet<string>(keywordsC, StringComparer.OrdinalIgnoreCase);
        internal static IReadOnlyList<string> AllC { get; } = new ReadOnlyCollection<string>(keywordsC);
        internal static bool CContains(string word) => keywordSetC.Contains(word);

        private static readonly List<string> directivesC = new List<string>
        {
            "#include", "#pragma", "#define", "#if", "#endif", "#undef", "#ifdef", "#ifndef", "#else", "#elif", "#error"
        };
        private static readonly HashSet<string> directiveSetC = new HashSet<string>(directivesC, StringComparer.OrdinalIgnoreCase);
        internal static IReadOnlyList<string> AllDirectivesC { get; } = new ReadOnlyCollection<string>(directivesC);
        internal static bool DirectivesContains(string word) => directiveSetC.Contains(word);

        private static readonly List<string> keywordsBison = new List<string>
        {
            // Known issue causes a keyword to not be highlighted properly if it first matches with a shorter keyword, %token-table would with %token first.
            // To workaround it include the longer keyword before the shorter one as in the case of %token-table and %token.
            "$accept", "%code", "%debug", "%define", "%defines", "%destructor", "%dprec", "%empty", "$end", "error",
            "%error-verbose", "%file-prefix", "%glr-parser", "%initial-action", "%language", "%left", "%lex-param", "%merge", "%name-prefix",
            "%no-lines", "%nonassoc", "%output", "%param", "%parse-param", "%precedence", "%prec", "%pure-parser", "%require", "%right",
            "%skeleton", "%start", "%token-table", "%token", "%type", "$undefined", "%union"
        };
        private static readonly HashSet<string> keywordSetBison = new HashSet<string>(keywordsBison, StringComparer.OrdinalIgnoreCase);
        internal static IReadOnlyList<string> AllBison { get; } = new ReadOnlyCollection<string>(keywordsBison);
        internal static bool BisonContains(string word) => keywordSetBison.Contains(word);
    }
}
