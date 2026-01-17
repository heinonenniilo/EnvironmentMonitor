using EnvironmentMonitor.Domain.Utils;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace EnvironmentMonitor.UnitTests
{
    [TestFixture]
    public class UriUtilsTests
    {
        [TestCase("https://example.com", new[] { "api", "users" }, "https://example.com/api/users")]
        [TestCase("https://example.com/", new[] { "api", "users" }, "https://example.com/api/users")]
        [TestCase("https://example.com", new[] { "/api", "users" }, "https://example.com/api/users")]
        [TestCase("https://example.com/", new[] { "/api/", "/users/" }, "https://example.com/api/users")]
        [TestCase("https://example.com", new[] { "api/users/123" }, "https://example.com/api/users/123")]
        [TestCase("https://example.com/api", new[] { "users", "123" }, "https://example.com/api/users/123")]
        [TestCase("https://example.com/api/", new[] { "/users", "123" }, "https://example.com/api/users/123")]
        [TestCase("https://example.com:8080", new[] { "api", "users" }, "https://example.com:8080/api/users")]
        [TestCase("https://example.com/base", new[] { "api", "users" }, "https://example.com/base/api/users")]
        [TestCase("http://localhost:5000", new[] { "api", "measurements" }, "http://localhost:5000/api/measurements")]
        public void CombineUrl_WithValidInputs_ReturnsCorrectUrl(string baseUrl, string[] parts, string expected)
        {
            // Act
            var result = UriUtils.CombineUrl(baseUrl, parts);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void CombineUrl_WithNoPartsProvided_ReturnsTrimmedBaseUrl()
        {
            // Arrange
            var baseUrl = "https://example.com/";

            // Act
            var result = UriUtils.CombineUrl(baseUrl);

            // Assert
            Assert.That(result, Is.EqualTo("https://example.com"));
        }

        [Test]
        public void CombineUrl_WithEmptyParts_SkipsEmptyParts()
        {
            // Arrange
            var baseUrl = "https://example.com";
            var parts = new[] { "api", "", "users", "   ", "123" };

            // Act
            var result = UriUtils.CombineUrl(baseUrl, parts);

            // Assert
            Assert.That(result, Is.EqualTo("https://example.com/api/users/123"));
        }

        [Test]
        public void CombineUrl_WithNullBaseUrl_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => UriUtils.CombineUrl(null!, "api", "users"));
            Assert.That(ex!.ParamName, Is.EqualTo("baseUrl"));
        }

        [Test]
        public void CombineUrl_WithEmptyBaseUrl_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => UriUtils.CombineUrl("", "api", "users"));
            Assert.That(ex!.ParamName, Is.EqualTo("baseUrl"));
        }

        [Test]
        public void CombineUrl_WithWhitespaceBaseUrl_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => UriUtils.CombineUrl("   ", "api", "users"));
            Assert.That(ex!.ParamName, Is.EqualTo("baseUrl"));
        }

        [Test]
        public void CombineUrl_WithInvalidBaseUrl_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => UriUtils.CombineUrl("not-a-valid-url", "api", "users"));
            Assert.That(ex!.ParamName, Is.EqualTo("baseUrl"));
            Assert.That(ex.Message, Does.Contain("valid absolute URL"));
        }

        [Test]
        public void CombineUrl_WithRelativeBaseUrl_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => UriUtils.CombineUrl("/api", "users"));
            Assert.That(ex!.ParamName, Is.EqualTo("baseUrl"));
            Assert.That(ex.Message, Does.Contain("valid absolute URL"));
        }

        [TestCase("https://example.com", new[] { "  api  ", "  users  " }, "https://example.com/api/users")]
        [TestCase("https://example.com", new[] { "/api/", "/users/" }, "https://example.com/api/users")]
        [TestCase("https://example.com", new[] { "///api///", "///users///" }, "https://example.com/api/users")]
        public void CombineUrl_WithExtraSpacesAndSlashes_TrimsCorrectly(string baseUrl, string[] parts, string expected)
        {
            // Act
            var result = UriUtils.CombineUrl(baseUrl, parts);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void CombineUrl_WithQueryParameters_PreservesQueryParameters()
        {
            // Arrange
            var baseUrl = "https://example.com";
            var parts = new[] { "api/users?page=1&limit=10" };

            // Act
            var result = UriUtils.CombineUrl(baseUrl, parts);

            // Assert
            Assert.That(result, Is.EqualTo("https://example.com/api/users?page=1&limit=10"));
        }

        [Test]
        public void CombineUrl_WithFragment_PreservesFragment()
        {
            // Arrange
            var baseUrl = "https://example.com";
            var parts = new[] { "docs#section-1" };

            // Act
            var result = UriUtils.CombineUrl(baseUrl, parts);

            // Assert
            Assert.That(result, Is.EqualTo("https://example.com/docs#section-1"));
        }

        [Test]
        public void IsValidHttpUrl_WithValidHttpUrl_ReturnsTrue()
        {
            // Act & Assert
            Assert.That(UriUtils.IsValidHttpUrl("http://example.com"), Is.True);
            Assert.That(UriUtils.IsValidHttpUrl("https://example.com"), Is.True);
        }

        [Test]
        public void IsValidHttpUrl_WithInvalidUrl_ReturnsFalse()
        {
            // Act & Assert
            Assert.That(UriUtils.IsValidHttpUrl("ftp://example.com"), Is.False);
            Assert.That(UriUtils.IsValidHttpUrl("not-a-url"), Is.False);
            Assert.That(UriUtils.IsValidHttpUrl(null), Is.False);
            Assert.That(UriUtils.IsValidHttpUrl(""), Is.False);
        }

        [Test]
        public void IsValidAbsoluteUrl_WithValidUrl_ReturnsTrue()
        {
            // Act & Assert
            Assert.That(UriUtils.IsValidAbsoluteUrl("http://example.com"), Is.True);
            Assert.That(UriUtils.IsValidAbsoluteUrl("https://example.com"), Is.True);
            Assert.That(UriUtils.IsValidAbsoluteUrl("ftp://example.com"), Is.True);
        }

        [Test]
        public void IsValidAbsoluteUrl_WithInvalidUrl_ReturnsFalse()
        {
            // Act & Assert
            Assert.That(UriUtils.IsValidAbsoluteUrl("not-a-url"), Is.False);
            Assert.That(UriUtils.IsValidAbsoluteUrl("/relative/path"), Is.False);
            Assert.That(UriUtils.IsValidAbsoluteUrl(null), Is.False);
            Assert.That(UriUtils.IsValidAbsoluteUrl(""), Is.False);
        }

        [Test]
        public void TryParseUri_WithValidUrl_ReturnsTrue()
        {
            // Act
            var result = UriUtils.TryParseUri("https://example.com", out var uri);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(uri, Is.Not.Null);
            Assert.That(uri!.AbsoluteUri, Is.EqualTo("https://example.com/"));
        }

        [Test]
        public void TryParseUri_WithInvalidUrl_ReturnsFalse()
        {
            // Act
            var result = UriUtils.TryParseUri("not-a-url", out var uri);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(uri, Is.Null);
        }
    }
}
