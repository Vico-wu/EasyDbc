using EasyDbc.Contracts;
using EasyDbc.Helpers;
using EasyDbc.Observers;
using EasyDbc.Parsers;
using EasyDbc.Parsers.DbcLineParsers;
using Moq;

namespace EasyDbc.Test
{
    [TestFixture]
    public class CommentLineParserTests
    {
        private MockRepository m_repository;

        [SetUp]
        public void Setup()
        {
            m_repository = new MockRepository(MockBehavior.Strict);
        }

        [TearDown]
        public void Teardown()
        {
            m_repository.VerifyAll();
        }

        private static ILineParser CreateParser()
        {
            return new CommentLineParser(new SilentFailureObserver());
        }

        [Test]
        public void EmptyCommentLineIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(string.Empty, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse("xfsgt_", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void OnlyPrefixIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse("CM_ ", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void OnlyPrefixAndSignalIsIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse("CM_ SG_ ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void IfCanLineIsNotANumberLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse("CM_ SG_ xxx;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ SG_ 75 channelName ""This is a description"";", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsed()
        {
            var multiLineComment = new[]
            {
                "CM_ SG_ 75 channelName \"This is the first line",
                "this is the second line",
                "this is the third line\";"
            };
            var expectedText = Helper.ConcatenateTextComment(multiLineComment, 23);

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", expectedText));
            var commentLineParser = CreateParser();

            var reader = new ArrayBasedLineProvider(multiLineComment);
            Assert.That(commentLineParser.TryParse(multiLineComment[0], dbcBuilderMock.Object, reader), Is.True);
        }

        [Test]
        public void FullLineIsParsedAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ SG_ 75    channelName      ""This is a description""     ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsedAndRobustToWhiteSpace()
        {
            var multiLineComment = new[]
            {
                "CM_ SG_ 75 channelName \"This is the first line",
                "   this is the second line",
                "   this is the third line\";"
            };
            var expectedText = Helper.ConcatenateTextComment(multiLineComment, 23);

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", expectedText));
            var commentLineParser = CreateParser();

            var reader = new ArrayBasedLineProvider(multiLineComment);
            Assert.That(commentLineParser.TryParse(multiLineComment[0], dbcBuilderMock.Object, reader), Is.True);
        }

        [Test]
        public void FullLineIsParsedForMessageAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessageComment(75, "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BO_ 75 ""This is a description""  ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsedForMessageAndRobustToWhiteSpace()
        {
            var multiLineComment = new[]
            {
                "CM_ BO_ 75 \"This is the first line",
                "   this is the second line",
                "   this is the third line\";"
            };
            var expectedText = Helper.ConcatenateTextComment(multiLineComment, 11);

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessageComment(75, expectedText));
            var commentLineParser = CreateParser();

            var reader = new ArrayBasedLineProvider(multiLineComment);
            Assert.That(commentLineParser.TryParse(multiLineComment[0], dbcBuilderMock.Object, reader), Is.True);
        }

        [Test]
        public void IncompleteLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BO_ ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void IncompleteLineWithCanIdAsStringIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BO_ xxx;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineIsParsedForNodeAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("node_name", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BU_ node_name ""This is a description""  ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsedForNodeAndRobustToWhiteSpace()
        {
            var multiLineComment = new[]
            {
                "CM_ BU_ node_name \"This is the first line",
                "   this is the second line",
                "   this is the third line\";"
            };
            var expectedText = Helper.ConcatenateTextComment(multiLineComment, 18);

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("node_name", expectedText));
            var commentLineParser = CreateParser();

            var reader = new ArrayBasedLineProvider(multiLineComment);
            Assert.That(commentLineParser.TryParse(multiLineComment[0], dbcBuilderMock.Object, reader), Is.True);
        }

        [Test]
        public void IncompleteLineForNodeIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BU_ ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void MalformedLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BU_ xxx;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void AnotherMalformedLineIsAcceptedWithoutInteraction()
        {
            // This behaviour is a bit loose. Quotes should be required, here a regex would be more accurate
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("xxx", "no quotes"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BU_ xxx no quotes;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [TestCase("CM_ SG_ 865 \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ BU_ NodeName \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ BO_ 865 \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ EV_ VarName \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ \"Test with incorrect \"syntax\"\";")]
        public void CommentSyntaxErrorIsObserved(string commentLine)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.CommentSyntaxError());

            var commentParser = new CommentLineParser(observerMock.Object);
            commentParser.TryParse(commentLine, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }
    }

    internal class ArrayBasedLineProvider : INextLineProvider
    {
        private readonly IList<string> m_lines;
        private int m_index;

        public ArrayBasedLineProvider(IList<string> lines)
        {
            m_lines = lines;
        }

        public bool TryGetLine(out string line)
        {
            line = null;
            if(++m_index < m_lines.Count)
            {
                line = m_lines[m_index];
                return true;
            }
            return false;
        }
    }
}