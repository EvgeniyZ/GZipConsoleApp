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
        public void Validate_NullParameters_ShouldBeFalse(string sourceFilename, string destinationFilename)
        {
            var fileParameterValidator = new FileParametersValidator();
            var (isValid, errorMessage) = fileParameterValidator.Validate(sourceFilename, destinationFilename);

            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }

        [Theory]
        [InlineData("zxcasd", "zxcas")]
        [InlineData(@"Z:\data\tter.gz", @"Z:\data\zxc")]
        public void Validate_InvalidParameters_ShouldBeFalse(string sourceFilename, string destinationFilename)
        {
            var fileParameterValidator = new FileParametersValidator();
            var (isValid, errorMessage) = fileParameterValidator.Validate(sourceFilename, destinationFilename);

            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }

        //TODO: uncomment - [Theory(Skip = "Integration test heavy rely on a existing disk, path and folders")]
        [Theory]
        [InlineData(@"C:\me.txt", @"C:\zxc")]
        public void Validate_ShouldBeTrue(string sourceFilename, string destinationFilename)
        {
            var fileParameterValidator = new FileParametersValidator();
            var (isValid, errorMessage) = fileParameterValidator.Validate(sourceFilename, destinationFilename);

            Assert.True(isValid);
            Assert.Empty(errorMessage);
        }
    }
}