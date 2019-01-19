using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using System.Diagnostics;

namespace Bison_Highlighter
{
    internal sealed class BisonTokenizer
    {

        internal static class Classes
        {
            internal readonly static short WhiteSpace = 0;
            internal readonly static short Keyword = 1;
            internal readonly static short MultiLineComment = 2;
            internal readonly static short Comment = 3;
            internal readonly static short NumberLiteral = 4;
            internal readonly static short StringLiteral = 5;
            internal readonly static short ExcludedCode = 6;
            internal readonly static short BisonToken = 7;
            internal readonly static short BlockName = 12;
            internal readonly static short Other = -1;
            internal readonly static short C = -2;
            internal readonly static short BisonDefinitions = -3;
            internal readonly static short Indentation = -4;
            internal readonly static short GrammarRules = -5;
            internal readonly static short Epilogue = -6;
        }
        internal BisonTokenizer(IStandardClassificationService classifications) => Classifications = classifications;
        internal IStandardClassificationService Classifications { get; }
        internal static List<string> BisonTokens = new List<string>();
        internal static List<string> CDefinitions = new List<string>();

        internal Token Scan(string text, int startIndex, int length, ref Languages language, ref Cases ecase, List<int[]> innerSections, int startTokenId = -1, int startState = 0)
        {
            int index = startIndex;
            Token token = new Token();
            token.StartIndex = index;
            token.TokenId = startTokenId;
            token.State = startState;
            token.Length = length - index;
            if (index >= text.Length)
                return token;


            if (index + 1 < length && text[index] == '/' && text[index + 1] == '/' && language != Languages.Other && language != Languages.Bison)
            {
                token.TokenId = Classes.Comment;
                return token;
            }

            if (((index + 1 < length && text[index] == '/' && text[index + 1] == '*') || token.State == (int)Cases.MultiLineComment) && language != Languages.Other && language != Languages.Bison)
            {
                //if (index + 1 < length && text[index] == '/' && text[index + 1] == '*')
                {
                    index += 2;
                    token.State = (int)Cases.MultiLineComment;
                    token.TokenId = Classes.MultiLineComment;
                }

                while (index < length)
                {
                    index = AdvanceWhile(text, ++index, chr => chr != '*');
                    if (index + 1 < length && text[index + 1] == '/')
                    {
                        token.State = (int)Cases.NoCase;
                        token.Length = index + 2 - startIndex;
                        token.TokenId = Classes.MultiLineComment;
                        return token;
                    }
                }
                return token;
            }
            
            int start = index;
            if (text[index] == '\"')
            {
                index = AdvanceWhile(text, ++index, chr => chr != '\"');
                token.TokenId = Classes.StringLiteral;
                token.Length = index - start + (text.IndexOf('\"', index) != -1 ? 1 : 0);
                return token;
            }
            if (text[index] == '\'') ///
            {
                index = AdvanceWhile(text, ++index, chr => chr != '\''); ///
                token.TokenId = Classes.StringLiteral; ///
                token.Length = index - start + (text.IndexOf('\'', index) != -1 ? 1 : 0); ///
                return token; ///
            }

            index = start;
            if (((index + 1 < length && text[index] == '%' && text[index + 1] == '{' && index == 0) || token.State == (int)Cases.C) && language == Languages.BisonDefinitions)
            {
                if ((index + 1 < length && text[index] == '%' && text[index + 1] == '{'))
                {
                    index += 2;
                    token.State = (int)Cases.C;
                    token.TokenId = Classes.C;
                }

                while (index < length)
                {
                    index = AdvanceWhile(text, index, chr => chr != '%');
                    if (index + 1 < length && text[index + 1] == '}' && (index - 1 > 0 && text[index - 1] == '\n') && !IsBetween(index, innerSections))
                    {
                        index += 2;
                        token.StartIndex = start;
                        token.State = (int)Cases.NoCase;
                        token.TokenId = Classes.C;
                        token.Length = index - start;
                        return token;
                    }
                    index++;
                    if (index >= length)
                    {
                        index += 2;
                        token.StartIndex = start;
                        token.TokenId = Classes.C;
                        return token;
                    }
                }
            }

            if (language == Languages.NoLanguage)
            {
                token.State = (int)Cases.BisonTokens;
                while (index <= length)
                {
                    index = AdvanceWhile(text, index, chr => chr != '%');
                    if (index + 1 < length && text[index + 1] == '%' && !IsBetween(index, innerSections) && index - 1 > 0 && text[index - 1] == '\n')
                    {
                        index += 2;
                        token.StartIndex = start;
                        token.State = (int)Cases.NoCase;
                        token.TokenId = Classes.BisonDefinitions;
                        token.Length = index - start;
                        return token;
                    }
                    if (index >= length)
                    {
                        token.StartIndex = start;
                        token.State = (int)Cases.NoCase;
                        token.TokenId = Classes.BisonDefinitions;
                        token.Length = index - start;
                        return token;
                    }
                    index++;
                }
            }

            index = start;
            if ((index < length && text[index] == '\t' || token.State == (int)Cases.Indentation) && (language == Languages.Bison || language == Languages.BisonDefinitions))
            {
                if (text[index] == '\t')
                {
                    index++;
                    ///token.TokenId = Classes.Indentation;
                    ///language = Languages.C;
                }
                while (index < length)
                {
                    index = AdvanceWhile(text, index, chr => chr == '\t');
                    if (text[index] == ':' || text[index] == '|') ///
                    { ///
                        index++;
                        index = AdvanceWhile(text, index, chr => chr == '\t');
                        token.StartIndex = start; ///
                        token.State = (int)Cases.NoCase; ///
                        token.TokenId = Classes.Other; ///
                        token.Length = index - start; ///
                    } ///
                    else
                    {
                        language = Languages.C; ///
                        token.StartIndex = start;
                        token.State = (int)Cases.NoCase;
                        token.TokenId = Classes.Other;
                        token.Length = index - start;
                    }
                    return token;
                }
            }

            ///if ((index < length && text[index] == '\t' || token.State == (int)Cases.CIndent) && language == Languages.BisonDefinitions)
            ///{
            ///    if (text[index] == '\t')
            ///    {
            ///        index++;
            ///        token.TokenId = Classes.CIndent;
            ///        language = Languages.Other;
            ///    }
            ///    while (index < length)
            ///    {
            ///        index = AdvanceWhile(text, index, chr => chr == '\t');
            ///        token.StartIndex = start;
            ///        token.State = (int)Cases.NoCase;
            ///        token.TokenId = Classes.Other;
            ///        token.Length = index - start;
            ///        return token;
            ///    }
            ///}

            index = start;
            if (((index + 1 < length && text[index] == '%' && text[index + 1] == '%' && index == 0) || token.State == (int)Cases.GrammarRules) && language == Languages.BisonDefinitions)
            {
                index += 2;
                token.State = (int)Cases.GrammarRules;
                token.TokenId = Classes.GrammarRules;

                while (index < length)
                {
                    index = AdvanceWhile(text, index, chr => chr != '%');
                    if (index + 1 < length && text[index + 1] == '%' && text[index - 1] == '\n' && !IsBetween(index, innerSections))
                    {
                        //index += 2;
                        token.StartIndex = start;
                        token.TokenId = Classes.GrammarRules;
                        token.State = (int)Cases.NoCase;
                        token.Length = index - start;
                        return token;
                    }
                    index++;
                    if (index >= length)
                    {
                        token.StartIndex = start;
                        return token;
                    }
                }
            }

            if (((index + 1 < length && text[index] == '%' && text[index + 1] == '%' && index == 0) || token.State == (int)Cases.Epilogue) && language == Languages.Bison)
            {
                index += 2;
                token.State = (int)Cases.Epilogue;
                token.TokenId = Classes.Epilogue;
                token.StartIndex = index;
                return token;
            }

            index = start;
            if (language == Languages.Bison)
                index = AdvanceWhile(text, index, chr => chr == ' ');
            else
                index = AdvanceWhile(text, index, chr => Char.IsWhiteSpace(chr));

            if (index > start)
            {
                token.TokenId = Classes.WhiteSpace;
                token.Length = index - start;
                return token;
            }

            ///if (language == Languages.BisonTokens)
            if (ecase == Cases.BisonToken && language == Languages.BisonDefinitions)
            {
                ///if (start == 0 || (index - 1 > 0 && text[index - 1] == '\n'))
                {
                    index = start;
                    if (text[index] == '_' || Char.IsLetter(text[index]))
                    {
                        index++;
                        index = AdvanceWhileDefinition(text, index);
                        if ((index < text.Length && Char.IsWhiteSpace(text[index])) || index == length)
                        {
                            var s = new string(text.ToCharArray(), start, index - start);
                            if (!BisonTokens.Contains(s))
                                BisonTokens.Add(s);
                            token.Length = index - start;
                            token.TokenId = Classes.BisonToken;
                            ecase = Cases.NoCase;
                            return token;
                        }
                    }

                }
                index = start;
            }
            if (language == Languages.Bison)
            {
                if (start == 0 || (index - 1 > 0 && text[index - 1] == '\n'))
                {
                    index = start;
                    if (text[index] == '_' || Char.IsLetter(text[index]))
                    {
                        index++;
                        index = AdvanceWhileDefinition(text, index);
                        if ((index < text.Length && (Char.IsWhiteSpace(text[index])) || text[index] == '\n' || text[index] == '\t') || index == length)
                        {
                            token.Length = index - start;
                            token.TokenId = Classes.BlockName;
                            return token;
                        }
                    }

                }
                index = start;
            }

            if (language == Languages.C || language == Languages.CEpilogue)
            {
                ///if (text[index] == '\"')
                ///{
                ///    index = AdvanceWhile(text, ++index, chr => chr != '\"');
                ///    token.TokenId = Classes.StringLiteral;
                ///    token.Length = index - start + (text.IndexOf('\"', index) != -1 ? 1 : 0);
                ///    return token;
                ///}
                ///if (text[index] == '\'') ///
                ///{
                ///    index = AdvanceWhile(text, ++index, chr => chr != '\''); ///
                ///    token.TokenId = Classes.StringLiteral; ///
                ///    token.Length = index - start + (text.IndexOf('\'', index) != -1 ? 1 : 0); ///
                ///    return token; ///
                ///}

                if (ecase == Cases.CDirectives)
                {
                    if (text[index] == '<')
                    {
                        index = AdvanceWhile(text, index, chr => chr != '>');
                        token.TokenId = Classes.StringLiteral;
                        token.Length = index - token.StartIndex + (text.IndexOf('>', start) != -1 ? 1 : 0);
                        ecase = Cases.NoCase;
                        return token;
                    }
                }
                if (ecase == Cases.CMacros)
                {
                    index = start;
                    if (text[index] == '_' || Char.IsLetter(text[index]))
                    {
                        index++;
                        index = AdvanceWhileDefinition(text, index);
                        if ((index <= text.Length /*&& Char.IsWhiteSpace(text[index])) || index == length*/))
                        {
                            var s = new string(text.ToCharArray(), start, index - start);
                            if (!CDefinitions.Contains(s))
                                CDefinitions.Add(s);
                            token.Length = index - start;
                            token.TokenId = Classes.BisonToken;
                            ecase = Cases.NoCase;
                            return token;
                        }
                    }
                    //index = AdvanceWhile(text, index, chr => !Char.IsWhiteSpace(chr));
                    //index = 
                    //token.TokenId = Classes.BisonToken;
                    //token.Length = index - start;
                    //ecase = Cases.NoCase;
                    //return token;
                }

                ///string[] test = { @"#include\s", @"#pragma\s", @"#define\s", @"#if\s", @"#endif\s", @"#undef\s", @"#ifdef\s", @"#ifndef\s", @"#else\s", @"#elif\s", @"#error\s" };
                IReadOnlyList<string> test = BisonKeywords.AllDirectivesC;
                foreach (var s in test)
                {
                    int i = -1;
                    foreach (Match match in new Regex(s).Matches(text))
                    {
                        if (match.Index == index)
                        {
                            i = index;
                        }
                    }
                    if (i == index)
                    {
                        token.TokenId = Classes.ExcludedCode;
                        ///token.Length = s.Length - 1;
                        token.Length = s.Length;
                        switch (s)
                        {
                            case "#include":
                                ecase = Cases.CDirectives;
                                return token;
                            case "#define":
                                ecase = Cases.CMacros;
                                return token;
                            case "#if":
                                ecase = Cases.CMacros;
                                return token;
                            default:
                                ecase = Cases.CMacros; ///
                                return token;
                        }
                    }
                }

                foreach (var definition in CDefinitions)
                {
                    foreach (Match match in new Regex($"{definition}[^A-Za-z0-9]").Matches(text))
                    {
                        if (match.Index == index)
                        {
                            token.TokenId = Classes.BisonToken;
                            token.Length = definition.Length;
                            ecase = Cases.NoCase;
                            return token;
                        }
                    }
                }
            }

            if (language == Languages.Bison)
            {
                ///if (ecase == Cases.TokenUsed)
                {
                    foreach (var definition in BisonTokens)
                    {
                        foreach (Match match in new Regex($"{definition}").Matches(text))
                        {
                            if (match.Index == index)
                            {
                                token.TokenId = Classes.BisonToken;
                                token.Length = definition.Length;
                                ecase = Cases.NoCase;
                                return token;
                            }
                        }
                    }
                }
                ///if (text[index] == '{')
                ///{
                ///    ecase = Cases.TokenUsed;
                ///}
            }

            if (language == Languages.BisonDefinitions)
            {
                if (ecase == Cases.BisonToken)
                {
                    token.TokenId = Classes.Other;
                    token.Length = length - index;
                    return token;
                }

                ///string[] test = { @"%option\s" };
                IReadOnlyList<string> test = BisonKeywords.AllBison;
                foreach (var s in test)
                {
                    int i = -1;
                    foreach (Match match in new Regex(s).Matches(text))
                    {
                        if (match.Index == 0)
                        {
                            i = index;
                        }
                    }
                    if (i == index)
                    {
                        ///token.Length = s.Length - 1;
                        token.Length = s.Length;
                        token.TokenId = Classes.ExcludedCode;
                        ecase = Cases.BisonToken;
                        ///switch (s)
                        ///{
                        ///    case @"%option\s":
                        ///        return token;
                        ///    default:
                        ///        return token;
                        ///}
                        return token; ///
                    }
                }
            }

            start = index;
            if (Char.IsDigit(text[index]))
            {
                index = AdvanceWhile(text, index, chr => Char.IsDigit(chr));
            }
            else if (Char.IsLetter(text[index]))
            {
                index = AdvanceWhile(text, index, chr => Char.IsLetter(chr));
            }
            else
            {
                index++;
            }
            string word = text.Substring(start, index - start);
            if (IsDecimalInteger(word))
            {
                token.TokenId = Classes.NumberLiteral;
                token.Length = index - start;
                return token;
            }
            else
            {
                if (language == Languages.C || language == Languages.CEpilogue)
                {
                    token.TokenId = BisonKeywords.CContains(word) ? Classes.Keyword : Classes.Other;
                }
                else if (language == Languages.Bison || language == Languages.Other)
                {
                    ///if (Regex.IsMatch(word, "^[A-Za-z]+$"))
                    ///{
                    ///    token.TokenId = Classes.BlockName;
                    ///}
                    ///else
                    ///{
                        ///token.TokenId = BisonKeywords.BisonContains(word) ? Classes.RegexSpecialCharacters : Classes.Other;
                        token.TokenId = Classes.Other;
                    ///}
                }
                else
                {
                    token.TokenId = BisonKeywords.BisonContains(word) ? Classes.Keyword : Classes.Other;
                }
                token.Length = index - start;
            }
            return token;
        }

        internal static bool IsDecimalInteger(string word)
        {
            foreach (var chr in word)
            {
                if (chr < '0' || chr > '9')
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsBetween(int value, int x1, int x2)
        {
            if (value >= x1 && value <= x2)
            {
                return true;
            }
            return false;
        }
        private bool IsBetween(int value, Tuple<int, int> range)
        {
            if (value >= range.Item1 && value <= range.Item2)
            {
                return true;
            }
            return false;
        }

        private bool IsBetween(int value, List<int[]> innerSections)
        {
            foreach (var range in innerSections)
                if (value >= range[0] && value <= range[1])
                {
                    return true;
                }

            return false;
        }

        internal static int AdvanceWhile(string text, int index, Func<char, bool> predicate)
        {
            for (int length = text.Length; index < length && predicate(text[index]); index++) ;
            return index;
        }

        private int AdvanceWhileDefinition(string text, int index)
        {
            for (int length = text.Length; index < length; index++)
            {
                if (!(Char.IsLetterOrDigit(text[index]) || text[index] == '_'))
                {
                    break;
                }
            }
            return index;
        }
    }
}
