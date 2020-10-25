using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ReactiveDiagnostics
{
    public class StringBuilderSlimTest
    {
        [Fact]
        public void Test1()
        {
            using var builder = new StringBuilderSlim();
            Assert.Equal("", builder.ToString());
            builder.Add('a'); Assert.Equal("a", builder.ToString());
            builder.Add('b'); Assert.Equal("ab", builder.ToString());
            builder.Add('c'); Assert.Equal("abc", builder.ToString());
            builder.Add('d'); Assert.Equal("abcd", builder.ToString());
            builder.Add('e'); Assert.Equal("abcde", builder.ToString());
            builder.Add('f'); Assert.Equal("abcdef", builder.ToString());
            builder.Add('g'); Assert.Equal("abcdefg", builder.ToString());
            builder.Add('h'); Assert.Equal("abcdefgh", builder.ToString());
            builder.Add('i'); Assert.Equal("abcdefghi", builder.ToString());
            builder.Add('j'); Assert.Equal("abcdefghij", builder.ToString());
            builder.Add('k'); Assert.Equal("abcdefghijk", builder.ToString());
            builder.Add('l'); Assert.Equal("abcdefghijkl", builder.ToString());
            builder.Add('m'); Assert.Equal("abcdefghijklm", builder.ToString());
            builder.Add('n'); Assert.Equal("abcdefghijklmn", builder.ToString());
            builder.Add('o'); Assert.Equal("abcdefghijklmno", builder.ToString());
            builder.Add('p'); Assert.Equal("abcdefghijklmnop", builder.ToString());
            builder.Add('q'); Assert.Equal("abcdefghijklmnopq", builder.ToString());
            builder.Add('r'); Assert.Equal("abcdefghijklmnopqr", builder.ToString());
            builder.Add('s'); Assert.Equal("abcdefghijklmnopqrs", builder.ToString());
            builder.Add('t'); Assert.Equal("abcdefghijklmnopqrst", builder.ToString());
            builder.Add('u'); Assert.Equal("abcdefghijklmnopqrstu", builder.ToString());
            builder.Add('v'); Assert.Equal("abcdefghijklmnopqrstuv", builder.ToString());
            builder.Add('w'); Assert.Equal("abcdefghijklmnopqrstuvw", builder.ToString());
            builder.Add('x'); Assert.Equal("abcdefghijklmnopqrstuvwx", builder.ToString());
            builder.Add('y'); Assert.Equal("abcdefghijklmnopqrstuvwxy", builder.ToString());
            builder.Add('z'); Assert.Equal("abcdefghijklmnopqrstuvwxyz", builder.ToString());
        }

        [Fact]
        public void Test2()
        {
            using var builder = new StringBuilderSlim();
            builder.Add("abcd"); Assert.Equal("abcd", builder.ToString());
            builder.Add("efgh"); Assert.Equal("abcdefgh", builder.ToString());
            builder.Add("ijkl"); Assert.Equal("abcdefghijkl", builder.ToString());
            builder.Add("mnop"); Assert.Equal("abcdefghijklmnop", builder.ToString());
            builder.Add("qrst"); Assert.Equal("abcdefghijklmnopqrst", builder.ToString());
            builder.Add("uvwx"); Assert.Equal("abcdefghijklmnopqrstuvwx", builder.ToString());
        }

        [Fact]
        public void Test3()
        {
            using var builder = new StringBuilderSlim();
            for(var i = 0; i < 256; ++i)
            {
                builder.Clear();
                builder.Add("abcdefghijklmnopqrstuvwxyz"); Assert.Equal("abcdefghijklmnopqrstuvwxyz", builder.ToString());
            }
        }
    }
}
