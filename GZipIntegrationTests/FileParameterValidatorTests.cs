using GZipConsoleApp.Validators;
using Xunit;

namespace GZipIntegrationTests
{
    public class FileParameterValidatorTests
    {
        [Fact]
        public void Validate_ShouldCheckCorrectPath()
        {
            var fileName = "asdzxc";

            var fileParameterValidator = new FileParameterValidator();
            var result = fileParameterValidator.Validate(fileName);

            Assert.False(result.isValid);
            Assert.NotEmpty(result.errorMessage);
        }
    }
}