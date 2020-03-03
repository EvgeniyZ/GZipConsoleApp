using GZipConsoleApp.Validators;
using Xunit;

namespace GZipUnitTests
{
    public class CommandParameterValidatorTests
    {
        [Fact]
        public void ValidateCommand_ShouldReturnValidationMessage()
        {
            const string command = "asdzxc";

            var commandParameterValidator = new CommandParameterValidator();
            var (isValid, errorMessage) = commandParameterValidator.Validate(command);

            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }

        [Theory]
        [InlineData("compress")]
        [InlineData("decompress")]
        public void ValidateCommand_ValidCommandPassed_ShouldBeValid(string command)
        {
            var commandParameterValidator = new CommandParameterValidator();
            var (isValid, errorMessage) = commandParameterValidator.Validate(command);

            Assert.True(isValid);
            Assert.Empty(errorMessage);
        }
    }
}