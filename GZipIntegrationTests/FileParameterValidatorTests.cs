using GZipConsoleApp.Validators;
using Xunit;

namespace GZipIntegrationTests
{
    public class FileParameterValidatorTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("adzxc", null)]
        [InlineData(null, "zxcas")]
        [InlineData("zxcasd", "zxcas")]
        [InlineData(@"Z:\data\tter.gz", @"Z:\data\zxc")]
        public void Validate_ShouldBeFalse(string sourceFilename, string destinationFilename)
        {
            var fileParameterValidator = new FileParameterValidator();
            var (isValid, errorMessage) = fileParameterValidator.Validate(sourceFilename, destinationFilename);

            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }
    }
}