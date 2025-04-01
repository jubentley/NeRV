using NeRV;

namespace NeRVTest
{
    public class MultiVersionApiTest
    {
        [Theory]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        public async Task Verify(int netMajor)
        {
            // Arrange
            string url = "https://api.github.com/repos/dotnet/runtime/releases";
            // Act
            var result = await Program.GetNetDetailsAPI(url, netMajor);
            // Assert
            Assert.NotNull(result);
            Assert.StartsWith($"{netMajor}.", result.remoteVersion);
            Assert.False(string.IsNullOrWhiteSpace(result.name));
            Assert.False(string.IsNullOrWhiteSpace(result.body));
            Assert.False(string.IsNullOrWhiteSpace(result.tag_name));
            Assert.True(result.id > 0);
            Assert.False(result.draft);
            Assert.False(result.prerelease);
            Assert.True(result.created_at.Year >= 2010);
            Assert.True(result.published_at > result.created_at);
        }
    }

    public class NetworkFailureTest
    {
        [Fact]
        public async Task URL_Unreachable()
        {
            // Arrange
            Program.TEST_MODE = true;
            string bogusUrl = "https://this-domain-does-not-exist-12345.com/releases";
            int netMajor = 8;

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                Program.GetNetDetailsAPI(bogusUrl, netMajor));

            // Assert
            //Assert.Equal("[NeRV] Network error: Could not reach API.", ex.Message);
            Assert.IsType<HttpRequestException>(ex.InnerException);
        }
    }


    public class InvalidNetVersionTest
    {
        [Fact]
        public async Task NetMajor404()
        {
            // Arrange
            Program.TEST_MODE = true;
            string url = "https://api.github.com/repos/dotnet/runtime/releases";
            int netMajor = 999;

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                Program.GetNetDetailsAPI(url, netMajor));

            // Assert
            Assert.Contains("No matching .NET release found.", ex.ToString());

            // March 2025 - Will only find 6-9 (as release)

            // ex.ToString() strigifys outer + inner exeption, ex.Message displays outer only
        }
    }

}




