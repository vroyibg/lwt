namespace Lwt.Test.Services
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;
    using Lwt.Services;
    using Lwt.ViewModels;
    using Moq;
    using Xunit;

    /// <summary>
    /// test text service.
    /// </summary>
    public class TextServiceTest
    {
        private readonly TextService textService;

        private readonly Mock<ITextRepository> textRepository;

        private readonly Mock<ILanguageHelper> languageHelper;

        private readonly Mock<IMapper<TextEditModel, Text>> textEditMapper;
        private readonly Mock<IMapper<Text, TextViewModel>> textViewMapper;
        private readonly Mock<IMapper<Text, TextEditDetailModel>> textEditDetailMapper;

        private readonly Mock<IValidator<Text>> textValidator;

        private readonly Mock<ITermRepository> termRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextServiceTest"/> class.
        /// constructor.
        /// </summary>
        public TextServiceTest()
        {
            this.textEditMapper = new Mock<IMapper<TextEditModel, Text>>();
            this.textViewMapper = new Mock<IMapper<Text, TextViewModel>>();
            this.textEditDetailMapper = new Mock<IMapper<Text, TextEditDetailModel>>();
            this.textRepository = new Mock<ITextRepository>();
            this.textValidator = new Mock<IValidator<Text>>();
            this.languageHelper = new Mock<ILanguageHelper>();
            this.termRepository = new Mock<ITermRepository>();

            this.textService = new TextService(
                this.textRepository.Object,
                this.textEditMapper.Object,
                this.textValidator.Object,
                this.languageHelper.Object,
                this.termRepository.Object,
                this.textViewMapper.Object,
                this.textEditDetailMapper.Object);
        }

        /// <summary>
        /// dependency injection should work.
        /// </summary>
        [Fact]
        public void ShouldGetSolved()
        {
            var helper = new DependencyResolverHelper();
            helper.GetService<ITextService>();
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsyncShouldThrowExceptionIfTextNotValid()
        {
            // arrange
            var text = new Text();
            var validationResult = new Mock<ValidationResult>();
            this.textValidator.Setup(v => v.Validate(text)).Returns(validationResult.Object);

            // add an error
            validationResult.Object.Errors.Add(new ValidationFailure("p", "e"));
            validationResult.Setup(r => r.IsValid).Returns(false);

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => this.textService.CreateAsync(text));
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsyncShouldThrowExceptionIfNotCreator()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text { CreatorId = creatorId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // assert
            await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.DeleteAsync(textId, userId));
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsyncShouldCallRepositoryIfHasPermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text { CreatorId = creatorId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // act
            await this.textService.DeleteAsync(textId, creatorId);

            // assert
            this.textRepository.Verify(r => r.DeleteByIdAsync(text), Times.Once);
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsyncShouldThrowExceptionIfNotHavePermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var editModel = new TextEditModel();
            var text = new Text { CreatorId = creatorId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // assert
            await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.EditAsync(textId, userId, editModel));
        }
    }
}