using GZipConsoleApp.Validators;
using Xunit;

namespace GZipUnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void ValidateCommand_ShouldReturnValidationMessage()
        {
            const string command = "asdzxc";

            var commandValidator = new CommandValidator();
            var (isValid, errorMessage) = commandValidator.Validate(command);

            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }

        [Theory]
        [InlineData("compress")]
        [InlineData("decompress")]
        public void ValidateCommand_ValidCommandPassed_ShouldBeValid(string command)
        {
            var commandValidator = new CommandValidator();
            var (isValid, errorMessage) = commandValidator.Validate(command);

            Assert.True(isValid);
            Assert.Empty(errorMessage);
        }
    }
}