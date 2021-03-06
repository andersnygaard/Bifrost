using Bifrost.FluentValidation.Sagas;
using Bifrost.Sagas;
using Machine.Specifications;
using Moq;

namespace Bifrost.FluentValidation.Specs.Sagas.for_ChapterValidationService.given
{
    public class a_chapter_validation_service
    {
        protected static IChapterValidationService chapter_validation_service;
        protected static Mock<IChapterValidatorProvider> chapter_validator_provider_mock;

        Establish context = () =>
        {
            chapter_validator_provider_mock = new Mock<IChapterValidatorProvider>();
            chapter_validation_service = new ChapterValidationService(chapter_validator_provider_mock.Object);
        };
    }
}