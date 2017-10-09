using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YuKu.MathExpression.Tests
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void EmptyInput()
        {
            using (var mathExpression = new StringReader(String.Empty))
            {
                using (var lexer = new Lexer(mathExpression))
                {
                    Assert.IsTrue(lexer.MoveNext());
                    Assert.AreEqual(TokenType.EOF, lexer.Current.Type);
                    Assert.IsFalse(lexer.MoveNext());
                }
            }
        }

        [TestMethod]
        [JsonDataSource(@".\Data\SingleToken.json")]
        public void SingleToken(String input, TokenType token, Boolean text)
        {
            using (var mathExpression = new StringReader(input))
            {
                using (var lexer = new Lexer(mathExpression))
                {
                    Assert.IsTrue(lexer.MoveNext());
                    Assert.AreEqual(token, lexer.Current.Type);
                    if (text)
                    {
                        Assert.AreEqual(input.Trim(), lexer.Current.Text);
                    }
                    Assert.IsTrue(lexer.MoveNext());
                    Assert.AreEqual(TokenType.EOF, lexer.Current.Type);
                    Assert.IsFalse(lexer.MoveNext());
                }
            }
        }
    }
}
