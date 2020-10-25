using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ReactiveDiagnostics
{
    partial class TraceRecord<T>
    {
        public const char EscapePrefix = '&';

        private static readonly Regex _FormatMatcher
               = new Regex(@"%(?<alignment>\d*)(?<key>[KSIPTLVksitlv])(?:\((?<expr>(?:\\.|[^\)])*)\))?");

        private static string ReplaceMatch<TElement>(Match match, IFormatProvider formatProvider, TElement value)
        {
            var alignment = match.Groups["alignment"].Value;
            var expr = match.Groups["expr"].Value;
            var fmt = "{0";
            if(!string.IsNullOrEmpty(alignment))
                fmt += "," + alignment;
            if(!string.IsNullOrEmpty(expr))
                fmt += ":" + fmt;
            fmt += "}";
            return string.Format(formatProvider, fmt, value);
        }

        /// <inheritdoc />
        public override string ToString()
            => ToString(string.Empty, CultureInfo.CurrentCulture);

        /// <inheritdoc />
        /// <remarks>
        /// Prease see <see cref="ToString(string, IFormatProvider)"/> to reference formatting options.
        /// </remarks>
        public string ToString(string format)
            => ToString(format, CultureInfo.CurrentCulture);

        /// <summary>
        /// Converts the logged trace record int this instance to its equivalent string representation
        /// using the specified culture-specific format information.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>For this method, following style format is available:</para>
        /// <para><c>%[&lt;alignment&gt;]&lt;content-key&gt;[(&lt;format-string&gt;)]</c></para>
        /// <para>Each content-key is case insensitive.</para>
        /// <para>Ampersand (U+0026) is escape sequence and the next one character is export as it is.</para>
        /// <para>This format is equivalent to <c>{&lt;content-key&gt;[,&lt;alignment&gt;][:&lt;format-string&gt;]}</c> under .Net format string.</para>
        /// <para>Following content keys are available'</para>
        /// <list type="bullet">
        ///     <item>
        ///         <term>K</term>
        ///         <description>[<see cref="String"/>] Operator key.</description>
        ///     </item>
        ///     <item>
        ///         <term>S</term>
        ///         <description>[<see cref="Int64"/>] Subscription index.</description>
        ///     </item>
        ///     <item>
        ///         <term>I</term>
        ///         <description>[<see cref="Int64"/>] Value index.</description>
        ///     </item>
        ///     <item>
        ///         <term>P</term>
        ///         <description>[<see cref="Int32"/>] Managed thread ID.</description>
        ///     </item>
        ///     <item>
        ///         <term>T</term>
        ///         <description>[<see cref="TimeSpan"/>] Total duration.</description>
        ///     </item>
        ///     <item>
        ///         <term>L</term>
        ///         <description>[<see cref="TimeSpan"/>] Lap duration.</description>
        ///     </item>
        ///     <item>
        ///         <term>V</term>
        ///         <description>[<typeparamref name="T"/>] Value</description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <para>Followings are examples of format and its result.</para>
        /// <list type="bullet">
        ///     <item>
        ///         <term><c>%K-%04S(X)-%04I(X)</c></term>
        ///         <description><c>sometrace-0002-00C1</c></description>
        ///     </item>
        ///     <item>
        ///         <term><c>&%%T(d\Dhh\:mm\:ss\.fff)&%</c></term>
        ///         <description><c>%0D00:00:05.500%</c></description>
        ///     </item>
        ///     <item>
        ///         <term><c>%L(s\.fff)/%T(s\.fff)</c></term>
        ///         <description><c>0.342/5.586</c></description>
        ///     </item>
        /// </list>
        /// </example>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            const int raw = 0;
            const int beginReplacement = 1;
            const int elementSpecified = 2;
            const int formatting = 3;

            formatProvider ??= CultureInfo.CurrentCulture;
            if(string.IsNullOrEmpty(format))
            {
                return string.Format(
                    formatProvider,
                    "Observable trace {0}-{1}-{2}: thread={3}, total duration={4}, lap duration={5}, value={6}",
                    OperatorKey, SubscriptionIndex, ValueIndex,
                    ManagedThreadId, TotalDuration, LapDuration, Value);
            }

            var result = new StringBuilder();
            var escaped = false;
            var state = raw;
            var elemKey = default(char);
            var alignment = default(string);
            var elemFmtStr = default(string);
            var builder = new StringBuilderSlim();

            void popCurrentFormat()
            {
                if(elemKey == '\0')
                    return;

                var fmt = "{0";
                if(!string.IsNullOrEmpty(alignment)) fmt += "," + alignment;
                if(!string.IsNullOrEmpty(elemFmtStr)) fmt += ":" + elemFmtStr;
                fmt += "}";
                var retval = char.ToUpper(elemKey) switch
                {
                    'K' => string.Format(formatProvider, fmt, OperatorKey),
                    'S' => string.Format(formatProvider, fmt, SubscriptionIndex),
                    'I' => string.Format(formatProvider, fmt, ValueIndex),
                    'P' => string.Format(formatProvider, fmt, ManagedThreadId),
                    'T' => string.Format(formatProvider, fmt, TotalDuration),
                    'L' => string.Format(formatProvider, fmt, LapDuration),
                    'V' => string.Format(formatProvider, fmt, Value),
                    _ => throw new InvalidOperationException(),
                };
                elemKey = default;
                alignment = default;
                elemFmtStr = default;
                result.Append(retval);
            }

            foreach(var c in format)
            {
                if(escaped)
                {
                    builder.Add(c);
                    escaped = false;
                    continue;
                }
                switch(state)
                {
                case raw:
                    switch(c)
                    {
                    case EscapePrefix:
                        escaped = true;
                        continue;
                    case '%':
                        result.Append(builder.ToString());
                        builder.Clear();
                        state = beginReplacement;
                        continue;
                    default:
                        builder.Add(c);
                        continue;
                    }
                case beginReplacement:
                    switch(c)
                    {
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
                        builder.Add(c);
                        continue;
                    case '-':
                        if(builder.Length == 0) goto case '0';
                        else goto default;
                    case 'K':
                    case 'S':
                    case 'I':
                    case 'P':
                    case 'T':
                    case 'L':
                    case 'V':
                    case 'k':
                    case 's':
                    case 'i':
                    case 'p':
                    case 't':
                    case 'l':
                    case 'v':
                        alignment = builder.ToString();
                        builder.Clear();
                        elemKey = c;
                        state = elementSpecified;
                        continue;
                    default:
                        throw new FormatException();
                    }
                case elementSpecified:
                    switch(c)
                    {
                    case EscapePrefix:
                        popCurrentFormat();
                        escaped = true;
                        state = raw;
                        continue;
                    case '(':
                        state = formatting;
                        continue;
                    default:
                        state = raw;
                        popCurrentFormat();
                        builder.Add(c);
                        continue;
                    }
                case formatting:
                    switch(c)
                    {
                    case EscapePrefix:
                        escaped = true;
                        continue;
                    case ')':
                        elemFmtStr = builder.ToString();
                        builder.Clear();
                        state = raw;
                        popCurrentFormat();
                        continue;
                    default:
                        builder.Add(c);
                        continue;
                    }
                default:
                    throw new InvalidOperationException();
                }
            }
            switch(state)
            {
            case raw:
                result.Append(builder.ToString());
                builder.Clear();
                break;
            case elementSpecified:
                popCurrentFormat();
                break;
            case beginReplacement:
            case formatting:
            default:
                throw new FormatException();
            }
            return result.ToString();
        }

    }
}
