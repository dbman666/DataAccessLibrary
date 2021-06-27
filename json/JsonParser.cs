using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace Coveo.Dal
{
    public enum JsonToken
    {
        Init,
        NestedLoop,
        Exit,
        ObjectStart,
        ObjectField,
        ObjectStop,
        ListStart,
        ListEntry,
        ListStop,
        ValueString,
        ValueNull,
        ValueBool,
        ValueNumber
    }

    /// <remarks>
    /// The passed string becomes ours. We put nuls ('\0') in it to separate the tokens.
    /// </remarks>
    public abstract unsafe class JsonParserBase
    {
// @formatter:off
        //  0: Take the char as is
        // -1: Char needs to be output as \uhhhh
        // nn: Char needs a backslash; not the original one though; use nn's char value.
        internal static sbyte[] CHARS_FORCING_ESCAPING =
        {
                                                                                             //     00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15
             -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1, 116, 110,  -1,  -1, 114,  -1,  -1,  //  00                            \t \n       \r            // Prefix with backslash
             -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  //  16            
              0,   0,  34,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  //  32     !  "  #  $  %  &  '  (  )  *  +  ,  -  .  /      // Careful: Single-quote is not escaped
              0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  //  48  0  1  2  3  4  5  6  7  8  9  :  ;  <  =  >  ?
              0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  //  64  @  A  B  C  D  E  F  G  H  I  J  K  L  M  N  O
              0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  92,   0,   0,   0,  //  80  P  Q  R  S  T  U  V  W  X  Y  Z  [  \  ]  ^  _      // \ needs backslashing
              0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  //  96  `  a  b  c  d  e  f  g  h  i  j  k  l  m  n  o
              0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  -1,  // 112  p  q  r  s  t  u  v  w  x  y  z  {  |  }  ~
        };
// @formatter:on

        private const char CHAR_0 = '0';
        private const char CHAR_A = 'A';
        private const char CHAR_BACKSLASH = '\\';
        private const char CHAR_CLOSE_BRACE = '}';
        private const char CHAR_CLOSE_SQUARE = ']';
        private const char CHAR_COLON = ':';
        private const char CHAR_COMMA = ',';
        private const char CHAR_DOUBLE_QUOTE = '"';
        private const char CHAR_CR = '\r';
        private const char CHAR_LF = '\n';
        private const char CHAR_LOWER_U = 'u';
        private const char CHAR_NUL = (char)0;
        private const char CHAR_OPEN_BRACE = '{';
        private const char CHAR_OPEN_SQUARE = '[';
        private const char CHAR_SINGLE_QUOTE = '\'';
        private const char CHAR_SLASH = '/';
        private const char CHAR_SPACE = ' ';
        private const char CHAR_TAB = '\t';
        private const char CHAR_DOT = '.';
        private const char CHAR_UPPER_U = 'U';
        private const char CHAR_Z = 'Z';
        private const byte TOLOWER = 'a' - 'A';

        protected static List<object> EMPTY_LIST = new List<object>();

        /// <summary>
        /// Used to keep the tokenizer state since we can't use unsafe stuff in a yield return enumerable !
        /// </summary>
        private int _label;

        /// <summary>
        /// The current line number. Used to output syntax errors.
        /// </summary>
        private JsonLine _curLine;

        /// <summary>
        /// Since we zero the end of buffers, situations like "{abc:123,def:456}" may happen, where we lose the char after the values.
        /// This is used to avoid the issue.
        /// </summary>
        private char _lookAheadToken;

        /// <summary>
        /// Ptr to the current byte.
        /// </summary>
        private char* _pCur;

        /// <summary>
        /// Set by the tokenizer.
        /// </summary>
        private char* _pTokenBegin;

        /// <summary>
        /// The top node.
        /// </summary>
        private object _rootValue;

        protected JsonToken _jsonTokenLabel;
        protected int _jsonTokenLevel;
        protected int _jsonTokenNb; // Use for {} and []
        protected object _jsonTokenValue;
        protected List<string> _unknownFields;

        public bool ThrowOnUnknownFields = true;

        public bool EmptyAsNull; // '{}' and '[]' will be set as null
        public bool RemoveNullEntriesFromLists = false; // That. Plus, if there are no more entries, will use EmptyAsNull to decide what to return.

        protected abstract object ParseTokenStream();

        public object Parse(string p_Json)
        {
            // Contract is that I destroy the string. Maybe eventually do the string copy here ??
            fixed (char* _pBuf = p_Json) {
                char* pBuf = _pBuf;
                return Tokenize(ref pBuf);
            }
        }

        public static string EscapeStringIfNeeded(string p_Src)
        {
            // First pass to determine if any char needs escaping.
            bool needEscaping = false;
            int len = p_Src.Length;
            for (int i = 0; i < len; ++i) {
                char c = p_Src[i];
                if (c > 127 || CHARS_FORCING_ESCAPING[c] != 0) {
                    needEscaping = true;
                    break;
                }
            }
            return needEscaping ? EscapeStringIfNeeded(new StringBuilder(), p_Src, false).ToString() : p_Src;
        }

        public static StringBuilder EscapeStringIfNeeded(StringBuilder sb, string p_Src, bool p_AddQuotes)
        {
            // First pass to determine if any char needs escaping.
            bool needEscaping = false;
            int len = p_Src.Length;
            for (int i = 0; i < len; ++i) {
                char c = p_Src[i];
                if (c > 127 || CHARS_FORCING_ESCAPING[c] != 0) {
                    needEscaping = true;
                    break;
                }
            }

            if (p_AddQuotes)
                sb.Append(CHAR_DOUBLE_QUOTE);

            // No need to escape.
            if (needEscaping) {
                sbyte kind;
                for (int i = 0; i < len; ++i) {
                    char c = p_Src[i];
                    if (c > 127 || (kind = CHARS_FORCING_ESCAPING[c]) == -1) {
                        sb.AppendFormat("\\u{0:X2}{1:X2}", c >> 8, c & 0xFF);
                    } else {
                        if (kind <= 0) {
                            // This catches chars that don't need translation (it includes -2, because the flag is only to know if we quote the string).
                            // No translation needed.
                            sb.Append(c);
                        } else {
                            sb.Append(CHAR_BACKSLASH);
                            sb.Append((char)kind);
                        }
                    }
                }
            } else {
                sb.Append(p_Src);
            }

            if (p_AddQuotes)
                sb.Append(CHAR_DOUBLE_QUOTE);
            return sb;
        }

        /// <summary>
        /// Scans a string up to the non-escaped matching quote, while converting any escaped chars in-place.
        /// _pCur is modified.
        /// </summary>
        private void SkipToEndingQuoteWhilePreparingString(char p_UpTo)
        {
            // Triple-quotes mean we take the string verbatim, up to the closing triple-quotes.
            if (*_pCur == p_UpTo && _pCur[1] == p_UpTo) {
                _pCur += 2;
                _pTokenBegin += 2;
                while (true) {
                    if (*_pCur == p_UpTo && _pCur[1] == p_UpTo && _pCur[2] == p_UpTo) {
                        _pCur[1] = _pCur[2] = CHAR_SPACE;
                        break;
                    }
                    ++_pCur;
                }
            } else {
                char* pDst = _pCur;
                while (*_pCur != p_UpTo) {
                    if (*_pCur != CHAR_BACKSLASH) {
                        *pDst++ = *_pCur++;
                        continue;
                    }
                    *pDst++ = ParseEscaped(ref _pCur);
                    ++_pCur;
                }
                *pDst = CHAR_NUL;
            }
        }

        /// <summary>
        /// Advances the stream ptr up to the next delimiter.
        /// </summary>
        private void SkipToDelim(bool p_AllowColon)
        {
            char c;
            for (c = *_pCur; c != 0; c = *++_pCur) {
                if ((p_AllowColon && c == CHAR_COLON) ||
                    c == CHAR_COMMA ||
                    c == CHAR_CLOSE_BRACE ||
                    c == CHAR_OPEN_BRACE ||
                    c == CHAR_CLOSE_SQUARE ||
                    // IS_WHITE
                    c == CHAR_SPACE || c == CHAR_TAB || c == CHAR_CR || c == CHAR_LF) {
                    break;
                }
            }
        }

        private void SkipWhiteSpace()
        {
            for (char c = *_pCur; c != 0; c = *++_pCur) {
                switch (c) {
                case CHAR_SPACE:
                case CHAR_TAB:
                case CHAR_CR:
                    break;
                case CHAR_LF:
                    NewLine();
                    break;
                case CHAR_SLASH:
                    // If this is a comment, we skip until end-of-line.
                    if (_pCur[1] == CHAR_SLASH) {
                        for (c = *_pCur; c != 0 && c != CHAR_LF; c = *++_pCur) {
                        }
                        NewLine();
                    } else {
                        // Only one slash means it's not a comment.
                        return;
                    }
                    break;
                default:
                    return;
                }
            }
        }

        private void NewLine()
        {
            JsonLine line = new JsonLine(_pCur + 1, _curLine._lineNo + 1);
            _curLine._next = line;
            _curLine._pEnd = _pCur; // Point to the LF
            _curLine = line;
        }

        /// <summary>
        /// The contract is that p_Rhs is a lower-case string. And that p_pLhs is nul-terminated.
        /// </summary>
        private static int CharCompareLower(char* p_pLhs, string p_Rhs)
        {
            int strLen = p_Rhs.Length;
            fixed (char* pRhs = p_Rhs) {
                char* pl = p_pLhs;
                char* pr = pRhs;
                for (int i = 0; *pl != 0 && i < strLen; ++i, ++pl, ++pr) {
                    var l = *pl;
                    if (l >= CHAR_A && l <= CHAR_Z) {
                        l = (char)(l + TOLOWER);
                    }
                    int diff = l - *pr;
                    if (diff != 0) {
                        return diff;
                    }
                }
            }
            return 0;
        }

        private static char* CharIndexOf(char* p_pBegin, char p_CharToFind)
        {
            for (; *p_pBegin != 0; ++p_pBegin) {
                if (*p_pBegin == p_CharToFind) {
                    return p_pBegin;
                }
            }
            return null;
        }

        /// <summary>
        /// Reads an escaped char from the stream.
        /// </summary>
        private static char ParseEscaped(ref char* p_pChar)
        {
            char c;
            ++p_pChar;
// @formatter:off
            switch (*p_pChar) {
            case CHAR_SINGLE_QUOTE:  c = CHAR_SINGLE_QUOTE; break;
            case CHAR_DOUBLE_QUOTE:  c = CHAR_DOUBLE_QUOTE; break;
            case CHAR_BACKSLASH:     c = CHAR_BACKSLASH;    break;
            case CHAR_SLASH:         c = CHAR_SLASH;        break;
            case 'b':                c = '\b';              break;
            case 'f':                c = '\f';              break;
            case 'n':                c = '\n';              break;
            case 'r':                c = '\r';              break;
            case 't':                c = '\t';              break;
            case CHAR_LOWER_U:
            case CHAR_UPPER_U:
                // Python handles surrogate chars when it reads Json: C:\data\_References\Python-2.6\Modules\_json.c.
                // Essentially it reads another \uxxxx if the preceding one is [0xd800, 0xdbff].
                // Since I don't store 32 bits Unicode chars, I don't bother.
                int i = ReadHex(*++p_pChar) << 12;
                i |= ReadHex(*++p_pChar) << 8;
                i |= ReadHex(*++p_pChar) << 4;
                i |= ReadHex(*++p_pChar);
                c = (char)i;
                break;
            default:
                c = *p_pChar;
                break;
            }
            return c;
        }
// @formatter:on

        /// <summary>
        /// Returns the nibble value of the passed hex char.
        /// </summary>
        private static char ReadHex(char c)
        {
            return c switch
            {
                >= '0' and <= '9' => (char)(c - '0'),
                >= 'A' and <= 'F' => (char)(c - 'A' + 10),
                >= 'a' and <= 'f' => (char)(c - 'a' + 10),
                _ => throw new JsonSyntaxErrorException($"Unexpected hex char '{c}'")
            };
        }

        /// <summary>
        /// Try to figure if p_pBuf contains a valid number.
        /// </summary>
        /// <remarks>
        /// Yes we'll parse the string twice (here and in the caller), but this is only called from ReadObject
        /// i.e. when the destination type is 'any', so it shouldn't happen often.
        /// </remarks>
        private static bool TryParseNumber(char* p_pBuf, out object p_Number)
        {
            p_Number = null;
            var c = *p_pBuf;
            var str = new string(p_pBuf);
            // If there's a '.' in the string, we try to parse a 'double'.
            if (CharIndexOf(p_pBuf, CHAR_DOT) == null) {
                if (c == CHAR_0 && (p_pBuf[1] == 'x' || p_pBuf[1] == 'X')
                    ? long.TryParse(str.Substring(2), NumberStyles.HexNumber, null, out long i8) // yuk
                    : long.TryParse(str, out i8)) {
                    p_Number = i8;
                    return true;
                }
                // else try 'decimal' before returning null I guess
            } else {
                if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double r8)) {
                    // Because of decimal separator.
                    p_Number = r8;
                    return true;
                }
            }
            return false;
        }

        private object Tokenize(ref char* p_pBuf)
        {
            _pCur = p_pBuf;
            _label = 0;
            _curLine = new JsonLine(_pCur, 1);
            _jsonTokenLabel = JsonToken.Init;
            _rootValue = ParseTokenStream();
            SkipWhiteSpace();
            if (*_pCur != CHAR_NUL) CmfUtil.Throw("Superfluous chars.");
            p_pBuf = _pCur;
            return _rootValue;
        }

        protected JsonToken NextJsonToken(JsonToken? p_Resume)
        {
// @formatter:off
            switch (p_Resume ?? _jsonTokenLabel) {
            case JsonToken.Init: goto labelInit;
            case JsonToken.NestedLoop: goto labelNestedLoop;
            case JsonToken.Exit: goto labelExit;
            case JsonToken.ObjectStart: goto labelObjectStart;
            case JsonToken.ObjectField: goto labelObjectField;
            case JsonToken.ObjectStop: goto labelObjectStop;
            case JsonToken.ListStart: goto labelListStart;
            case JsonToken.ListEntry: goto labelListEntry;
            case JsonToken.ListStop: goto labelListStop;
            }
            throw new JsonSyntaxErrorException("Unsupported JsonToken case" + _jsonTokenLabel);

        labelInit:
            _jsonTokenLabel = 0;
            _jsonTokenLevel = 0;

        labelNestedLoop:
            var token = NextToken(false);

            if (token == CHAR_OPEN_BRACE) {
                return _jsonTokenLabel = JsonToken.ObjectStart;
            } else if (token == CHAR_OPEN_SQUARE) {
                return _jsonTokenLabel = JsonToken.ListStart;
            } else if (token == CHAR_DOUBLE_QUOTE) {
                // Normal value.
                _jsonTokenValue = new string(_pTokenBegin);
                return JsonToken.ValueString;
            } else if (token == CHAR_SINGLE_QUOTE) {
                char* pBegin = _pTokenBegin;
                switch (*pBegin) {
                    case 'n':
                    case 'N':
                        if (CharCompareLower(pBegin, Ctes.NULL) == 0) {
                            _jsonTokenValue = null;
                            return JsonToken.ValueNull;
                        }
                        break;
                    case 'f':
                    case 'F':
                        if (CharCompareLower(pBegin, Ctes.FALSE) == 0) {
                            _jsonTokenValue = MetaType.FALSE;
                            return JsonToken.ValueBool;
                        }
                        break;
                    case 't':
                    case 'T':
                        if (CharCompareLower(pBegin, Ctes.TRUE) == 0)
                        {
                            _jsonTokenValue = MetaType.TRUE;
                            return JsonToken.ValueBool;
                        }
                        break;
                    case '-':
                    case '+':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (TryParseNumber(_pTokenBegin, out object number)) {
                            _jsonTokenValue = number;
                            return JsonToken.ValueNumber;
                        }
                        break;
                }
                _jsonTokenValue = new string(_pTokenBegin);
                return JsonToken.ValueString;
            } else
                throw new JsonSyntaxErrorException(FormatSyntaxError($"Unexpected token '{token}'"));

#region Object
            labelObjectStart:
            _jsonTokenNb = 0;
            ++_jsonTokenLevel;
        labelObjectLoop:
            var tok1 = LookAheadToken();

            // Done yet ?
            if (tok1 == CHAR_CLOSE_BRACE) {
                // Eat the token.
                NextToken(false);
                goto labelObjectDone;
            }

            // Comma if not first pair.
            if (_jsonTokenNb != 0) {
                tok1 = NextToken(false);
                if (tok1 != CHAR_COMMA) throw new JsonSyntaxErrorException(FormatSyntaxError("Expected ','"));

                // Accept dangling comma.
                if (LookAheadToken() == CHAR_CLOSE_BRACE) {
                    // Eat the token.
                    NextToken(false);
                    goto labelObjectDone;
                }
            }

            // <field> : <value>
            tok1 = NextToken(true);
            char* pKey = _pTokenBegin;
            char firstKeyChar = *pKey;
            // A key can't be a complex object.
            if (!(tok1 == CHAR_DOUBLE_QUOTE || (firstKeyChar != CHAR_OPEN_BRACE && firstKeyChar != CHAR_OPEN_SQUARE)))
                CmfUtil.Throw("JsonParser.TokenizeValue: !(tok == CHAR_DOUBLE_QUOTE || (firstKeyChar != CHAR_OPEN_BRACE && firstKeyChar != CHAR_OPEN_SQUARE))");
            _jsonTokenValue = new string(pKey);
            tok1 = NextToken(false);
            if (tok1 != CHAR_COLON) throw new JsonSyntaxErrorException(FormatSyntaxError("Expected ':'"));

            // Release control so the caller can read a value.
            return _jsonTokenLabel = JsonToken.NestedLoop;
        labelObjectField:
            ++_jsonTokenNb;
            goto labelObjectLoop;
            labelObjectDone:
            return _jsonTokenLabel = JsonToken.ObjectStop;
            labelObjectStop:
            goto labelOutLevel;
#endregion

#region List
            labelListStart:
            _jsonTokenNb = 0;
        labelListLoop:
            var tok2 = LookAheadToken();

            // Done yet ?
            if (tok2 == CHAR_CLOSE_SQUARE) {
                // Eat the token.
                NextToken(false);
                goto labelListDone;
            }

            // Comma if not first entry.
            if (_jsonTokenNb != 0) {
                tok2 = NextToken(false);
                if (tok2 != CHAR_COMMA) throw new JsonSyntaxErrorException(FormatSyntaxError("Expected ','"));

                // Accept dangling comma.
                if (LookAheadToken() == CHAR_CLOSE_SQUARE) {
                    // Eat the token.
                    NextToken(false);
                    goto labelListDone;
                }
            }
            // Release control so the caller can read a value.
            return _jsonTokenLabel = JsonToken.NestedLoop;
        labelListEntry:
            ++_jsonTokenNb;
            ++_jsonTokenLevel;
            goto labelListLoop;
            labelListDone:
            return _jsonTokenLabel = JsonToken.ListStop;
            labelListStop:
            // flow thru
#endregion

            labelOutLevel:
            if (--_jsonTokenLevel != 0)
                goto labelNestedLoop;
        labelExit:
            return _jsonTokenLabel = JsonToken.Exit;
// @formatter:on
        }

        /// <remarks>
        /// I use the same technique that .Net uses when yield return is part of a method.
        /// I had to revert to this because the compiler complained about not being able to generate
        /// an iterator when unsafe stuff is used.
        ///
        /// What complicates this method is that I nul-terminate buffers when I tokenize values. But if
        /// a delimiter immediately follows the value, I would lose it. So I introduce _lookAheadToken for that.
        /// </remarks>
        private char NextToken(bool p_AllowColon)
        {
// @formatter:off
            switch (_label) {
            case 0: goto labelInit;
            case 1: goto labelWhile;
            case 2: goto labelLookAhead;
            case 3: goto labelExit;
            }

        labelInit:
            _lookAheadToken = CHAR_NUL;
            _label = 1;

        labelWhile:
            if (*_pCur == 0) {
                goto labelAfterWhile;
            }
            SkipWhiteSpace();
            switch (*_pCur) {
            case CHAR_OPEN_BRACE:
            case CHAR_CLOSE_BRACE:
            case CHAR_OPEN_SQUARE:
            case CHAR_CLOSE_SQUARE:
            case CHAR_COMMA:
            case CHAR_COLON:
                return *_pCur++;

            case CHAR_DOUBLE_QUOTE:
            case CHAR_SINGLE_QUOTE:
                // Skip over the starting/ending quotes.
                var q = *_pCur;
                _pTokenBegin = ++_pCur;
                SkipToEndingQuoteWhilePreparingString(q);
                // No need to check for a look-ahead token since the quote is the one nul-ed.
                *_pCur++ = CHAR_NUL;
                return CHAR_DOUBLE_QUOTE; // Value was enclosed in quotes.

            default:
                _pTokenBegin = _pCur;
                SkipToDelim(p_AllowColon);
                // Before nul-terminating the buffer, check if *_pCur is actually the next token to use.
                var c = *_pCur;
                if ((p_AllowColon && c == CHAR_COLON) || c == CHAR_COMMA || c == CHAR_CLOSE_BRACE || c == CHAR_CLOSE_SQUARE) {
                    _lookAheadToken = c;
                    _label = 2;
                }
                // I nul-terminate the buffer, but the caller increment _pCur.
                *_pCur++ = CHAR_NUL;
                return CHAR_SINGLE_QUOTE; // Value was not enclosed in quotes.
            }

        labelLookAhead:
            if (_lookAheadToken == 0) CmfUtil.Throw("JsonParser.NextToken: _lookAheadToken == 0");
            _label = 1;
            var lookAhead = _lookAheadToken;
            _lookAheadToken = CHAR_NUL;
            return lookAhead;

        labelAfterWhile:
            _label = 3;

        labelExit:
            return CHAR_NUL;
// @formatter:on
        }

        /// <summary>
        /// Check the next token without advancing _pCur (except for white space).
        /// </summary>
        /// <returns></returns>
        private char LookAheadToken()
        {
            if (_lookAheadToken == CHAR_NUL) {
                SkipWhiteSpace();
                return *_pCur;
            }
            return _lookAheadToken;
        }

        private string FormatSyntaxError(string p_Msg, JsonLine p_Line = null, int p_Offset = 0)
        {
            if (p_Line == null) {
                p_Line = _curLine;
                p_Offset = (int)(_pCur - p_Line._pBegin);
            }
            char* pLineEnd = p_Line.GetLineEnd();
            var sb = new StringBuilder(p_Msg);
            int postLen = (int)(pLineEnd - p_Line._pBegin) - p_Offset;
            sb.AppendFormat(" in line {0}: {1}>>HERE<<{2}",
                p_Line._lineNo,
                new string(p_Line._pBegin, 0, p_Offset).Replace('\0', '\xb0'),
                new string(p_Line._pBegin, p_Offset, postLen < 0 ? 0 : postLen).Replace('\0', '\xb0'));
            return sb.ToString();
        }

        protected void AddUnknownField(Type p_Type, string p_Name)
        {
            var name = p_Type.Name + '.' + p_Name;
            if (_unknownFields == null)
                _unknownFields = new List<string>();
            else {
                if (_unknownFields.Contains(name))
                    return;
            }
            _unknownFields.Add(name);
        }
    }

    public class JsonSyntaxErrorException : Exception
    {
        public JsonSyntaxErrorException(string p_Msg)
            : base(p_Msg)
        {
        }
    }

    /// Small object to mark the descent into {...} objects
    public unsafe class JsonLine
    {
        private const char CHAR_LF = '\n';

        public char* _pBegin;
        public char* _pEnd;
        public JsonLine _next;
        public int _lineNo;

        public JsonLine(char* p_pBegin, int p_LineNo)
        {
            _pBegin = p_pBegin;
            _pEnd = null;
            _lineNo = p_LineNo;
            _next = null;
        }

        public char* GetLineEnd()
        {
            if (_pEnd == null) {
                char c;
                for (_pEnd = _pBegin, c = *_pEnd; c != 0 && c != CHAR_LF; c = *++_pEnd) {
                }
            }
            return _pEnd;
        }
    }

    public class JsonParser : JsonParserBase
    {
        protected Repo _repo;

        public Repo Repo => _repo;

        public JsonParser(Repo p_Repo)
        {
            _repo = p_Repo;
        }

        protected override object ParseTokenStream()
        {
            return ThrowIfUnknownFields(ParseInternal());
        }

        protected object ThrowIfUnknownFields(object p_Obj)
        {
            if (_unknownFields == null)
                return p_Obj;
            if (ThrowOnUnknownFields) {
                var sb = new StringBuilder();
                sb.AppendLine("Unknown fields:");
                foreach (var field in _unknownFields)
                    sb.AppendLine(field);
                throw new JsonSyntaxErrorException(sb.ToString());
            }
            foreach (var field in _unknownFields)
                Console.WriteLine("Skipped unknown field " + field);
            return p_Obj;
        }

        protected object ParseInternal()
        {
            switch (NextJsonToken(null)) {
            case JsonToken.ObjectStart:
            {
                // If no ClrType either, we use 'Row'. That's when we just gobble values.
                Row row = null;
                JsonToken? resume = null;
                while (NextJsonToken(resume) == JsonToken.NestedLoop) {
                    if (row == null) {
                        row = new Row(_repo);
                        resume = JsonToken.ObjectField;
                    }
                    row.Add((string)_jsonTokenValue, ParseInternal());
                }
                return row ?? (EmptyAsNull ? null : _repo.EmptyRow);
            }
            case JsonToken.ListStart:
            {
                IList list = null;
                JsonToken? resume = null;
                while (NextJsonToken(resume) == JsonToken.NestedLoop) {
                    if (list == null) {
                        list = new List<object>();
                        resume = JsonToken.ListEntry;
                    }
                    list.Add(ParseInternal());
                }
                return list ?? (EmptyAsNull ? null : new List<object>()); // For effing Rabbit. Assumes the dst type is a 'Row' :(
            }
            case JsonToken.ValueString:
                return _repo.GetOrAddString((string)_jsonTokenValue);
            case JsonToken.ValueNull:
            case JsonToken.ValueBool:
            case JsonToken.ValueNumber:
                return _jsonTokenValue;
            default:
                throw new JsonSyntaxErrorException("JsonParserRow: Unexpected JsonToken: " + _jsonTokenLabel);
            }
        }
    }

    public class JsonParserPoco<T> : JsonParser, IConvContext
    {
        private Type _typeRoot;
        private object _convContext;

        public object Context
        {
            get => _convContext;
            set => _convContext = value;
        }

        public JsonParserPoco(Repo p_Repo, Type p_Type = null) : base(p_Repo)
        {
            _typeRoot = p_Type ?? typeof(T);
        }

        public JsonParserPoco<T> UsingType(Type p_Type)
        {
            _typeRoot = p_Type;
            return this;
        }

        public T ParseT(string p_Json)
        {
            return (T)Parse(p_Json);
        }

        protected override object ParseTokenStream()
        {
            return ThrowIfUnknownFields(ParseInternal(_typeRoot));
        }

        private object ParseInternal(Type p_Type)
        {
            if (p_Type == typeof(Row)) { // Only for Rabbit I think. Maybe the caller could set EmptyAsNull !!?
                EmptyAsNull = true;
                return ParseInternal();
            }
            switch (NextJsonToken(null)) {
            case JsonToken.ObjectStart:
            {
                // Read maps as lists.
                // List<KeyValue> is for generic values. Like for rabbit arguments.
                // List<KeyValueT<xx>> is for Pocos where we want fully-typed objects.
                if (p_Type.Name == "List`1") {
                    if (p_Type == typeof(List<KeyValue>))
                        return ParseListKeyValue();
                    var genArgs = p_Type.GenericTypeArguments;
                    if (genArgs.Length == 1) {
                        var keyValueTType = genArgs[0];
                        if (keyValueTType.Name == "KeyValueT`1")
                            return ParseListKeyValueT(p_Type, keyValueTType, keyValueTType.GenericTypeArguments[0]);
                    }
                }
                object row = null;
                JsonToken? resume = null;
                var metaType = _repo.FindOrCreateMetaType(p_Type);
                while (NextJsonToken(resume) == JsonToken.NestedLoop) {
                    if (row == null) {
                        row = NewEmpty(metaType);
                        resume = JsonToken.ObjectField;
                    }
                    var fieldName = (string)_jsonTokenValue;
                    var metaField = metaType.FindMetaField(fieldName);
                    if (metaField == null) {
                        var fixedName = FixupFieldName(fieldName);
                        if (fixedName != null)
                            metaField = metaType.FindMetaField(fixedName);
                        if (metaField == null) {
                            AddUnknownField(p_Type, fieldName);
                            ParseSkip();
                            continue;
                        }
                        if (metaField.IsNotUseful) {
                            ParseSkip();
                            continue;
                        }
                    }
                    var val = ParseInternal(metaField.ClrType);
                    if (val != null) {
                        _repo.Set(row, metaField, val);
                    }
                }
                return row ?? (EmptyAsNull ? null : NewEmpty(metaType));
            }
            case JsonToken.ListStart:
            {
                IList list = null;
                Type ofType = null;
                JsonToken? resume = null;
                while (NextJsonToken(resume) == JsonToken.NestedLoop) {
                    if (list == null) {
                        ofType = p_Type.GenericTypeArguments[0];
                        list = (IList)NewEmptyList(p_Type);
                        resume = JsonToken.ListEntry;
                    }
                    list.Add(ParseInternal(ofType));
                }
                if (list == null)
                    return EmptyAsNull ? null : NewEmptyList(p_Type);
                if (RemoveNullEntriesFromLists) {
                    for (var i = list.Count; i-- > 0;)
                        if (list[i] == null)
                            list.RemoveAt(i);
                    if (list.Count == 0)
                        list = null;
                }
                return list ?? (EmptyAsNull ? null : NewEmptyList(p_Type));
            }
            case JsonToken.ValueNull:
                return _jsonTokenValue;
            case JsonToken.ValueString:
            case JsonToken.ValueBool:
            case JsonToken.ValueNumber:
            {
                var srcType = _jsonTokenValue.GetType();
                return srcType == p_Type || p_Type == typeof(object) ? _jsonTokenValue : TypeConverter.FindConvFn(p_Type, srcType)(_jsonTokenValue, p_Type, this);
            }
            default:
                throw new JsonSyntaxErrorException("JsonParserRow: Unexpected JsonToken: " + _jsonTokenLabel);
            }
        }

        private object ParseListKeyValue()
        {
            _repo.FindOrCreateMetaType(typeof(KeyValue));
            List<KeyValue> keyValues = null;
            JsonToken? resume = null;
            while (NextJsonToken(resume) == JsonToken.NestedLoop) {
                if (keyValues == null) {
                    keyValues = new List<KeyValue>();
                    resume = JsonToken.ObjectField;
                }
                var fieldName = _repo.GetOrAddString((string)_jsonTokenValue);
                var val = ParseInternal(typeof(object));
                if (val != null)
                    keyValues.Add(new KeyValue(fieldName, val));
            }
            return keyValues;
        }

        private object ParseListKeyValueT(Type p_ListType, Type p_KeyValueType, Type p_ValueType)
        {
            var metaType = _repo.FindOrCreateMetaType(p_ListType);
            ConstructorInfo keyValueCtorInfo = null;
            IList keyValues = null;
            JsonToken? resume = null;
            while (NextJsonToken(resume) == JsonToken.NestedLoop) {
                if (keyValues == null) {
                    keyValueCtorInfo = p_KeyValueType.GetConstructors()[0];
                    keyValues = (IList)metaType.New();
                    resume = JsonToken.ObjectField;
                }
                var fieldName = _repo.GetOrAddString((string)_jsonTokenValue);
                var val = ParseInternal(p_ValueType);
                if (val != null) {
                    var keyValue = keyValueCtorInfo.Invoke(new [] {fieldName, val});
                    keyValues.Add(keyValue);
                }
            }
            return keyValues;
        }
        
        private void ParseSkip()
        {
            switch (NextJsonToken(null)) {
            case JsonToken.ObjectStart:
            {
                int nb = 0;
                JsonToken? resume = null;
                while (NextJsonToken(resume) == JsonToken.NestedLoop) {
                    if (nb++ == 0)
                        resume = JsonToken.ObjectField;
                    ParseSkip();
                }
                return;
            }
            case JsonToken.ListStart:
            {
                int nb = 0;
                JsonToken? resume = null;
                while (NextJsonToken(resume) == JsonToken.NestedLoop) {
                    if (nb++ == 0)
                        resume = JsonToken.ListEntry;
                    ParseSkip();
                }
                return;
            }
            case JsonToken.ValueString:
            case JsonToken.ValueNull:
            case JsonToken.ValueBool:
            case JsonToken.ValueNumber:
                return;
            default:
                throw new JsonSyntaxErrorException("JsonParserRow: Unexpected JsonToken: " + _jsonTokenLabel);
            }
        }

        private object NewEmpty(MetaType p_MetaType)
        {
            return p_MetaType.New();
        }

        private object NewEmptyList(Type p_Type)
        {
            return Activator.CreateInstance(p_Type);
        }

        private string FixupFieldName(string name)
        {
            if (name[0] == '@')
                return name.Substring(1);
            if (name.Contains('-'))
                return name.Replace('-', '_');
            return null;
        }
    }
}