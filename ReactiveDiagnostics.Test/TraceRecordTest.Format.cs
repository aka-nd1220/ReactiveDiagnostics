using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit;

namespace ReactiveDiagnostics
{
    public class TraceRecordTest
    {
        public static IEnumerable<object[]> TestArgs()
        {
            static object[] core<T>(TraceRecord<T> record, string format, string expected)
                => new object[] { record, format, expected };

            {
                var record = new TraceRecord<int>("traceKey", 123, 456, "", "", 1, TimeSpan.FromMilliseconds(5500), TimeSpan.FromMilliseconds(1500), 789);
                yield return core(record, "", "Observable trace traceKey-123-456: thread=1, total duration=00:00:05.5000000, lap duration=00:00:01.5000000, value=789");
                yield return core(record, "%K", "traceKey");
                yield return core(record, "%S", "123");
                yield return core(record, "%I", "456");
                yield return core(record, "%P", "1");
                yield return core(record, "%T", "00:00:05.5000000");
                yield return core(record, "%L", "00:00:01.5000000");
                yield return core(record, "%V", "789");
                yield return core(record, "%K %S", "traceKey 123");
                yield return core(record, "%K-%S", "traceKey-123");
                yield return core(record, "%K,%S", "traceKey,123");
                yield return core(record, "%K$%S", "traceKey$123");
                yield return core(record, "%K!%S", "traceKey!123");
                yield return core(record, "%K*%S", "traceKey*123");
                yield return core(record, "%K+%S", "traceKey+123");
                yield return core(record, "%K& %S", "traceKey 123");
                yield return core(record, "%K&-%S", "traceKey-123");
                yield return core(record, "%K&,%S", "traceKey,123");
                yield return core(record, "%K&$%S", "traceKey$123");
                yield return core(record, "%K&!%S", "traceKey!123");
                yield return core(record, "%K&*%S", "traceKey*123");
                yield return core(record, "%K&+%S", "traceKey+123");
                yield return core(record, "%K&%%S", "traceKey%123");
                yield return core(record, "%4S" , " 123");
                yield return core(record, "%-4S", "123 ");
                yield return core(record, "%S(0000)", "0123");
                yield return core(record, @"%T(d\Dhh\:mm\:ss\.fff)", "0D00:00:05.500");
                yield return core(record, @"&%%T(d\Dhh\:mm\:ss\.fff)&%", "%0D00:00:05.500%");
                yield return core(record, @"%L(s\.fff)/%T(s\.fff)", "1.500/5.500");
                yield return core(record, @"Observable trace %k-0x%s(X04)-0x%4i(X04): thread=%p, total duration=%t(s\.f)sec, lap duration=%l(s\.f)sec, value=%4v",
                                           "Observable trace traceKey-0x007B-0x01C8: thread=1, total duration=5.5sec, lap duration=1.5sec, value= 789");
            }
            yield break;
        }

        [Theory]
        [MemberData(nameof(TestArgs))]
        public void TestFormat(ITraceRecord record, string format, string expected)
        {
            Assert.Equal(expected, string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", record));
        }
    }
}
