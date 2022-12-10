﻿using FluentValidation.TestHelper;
using LostAndFound.PublicationService.Core.DateTimeProviders;
using LostAndFound.PublicationService.Core.FluentValidators;
using LostAndFound.PublicationService.CoreLibrary.Enums;
using LostAndFound.PublicationService.CoreLibrary.Requests;
using LostAndFound.PublicationService.DataAccess.Repositories.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace LostAndFound.PublicationService.UnitTests.Core.FluentValidators
{
    public class UpdatePublicationDetailsRequestDtoValidatorTests
    {
        private readonly Mock<ICategoriesRepository> _categoriesRepositoryMock;
        private readonly Mock<IDateTimeProvider> _dateTimeProvideMock;
        private readonly DateTime _utcDateNowForTests;

        public UpdatePublicationDetailsRequestDtoValidatorTests()
        {
            _utcDateNowForTests = DateTime.UtcNow;
            _dateTimeProvideMock = new Mock<IDateTimeProvider>();
            _categoriesRepositoryMock = new Mock<ICategoriesRepository>();

            _dateTimeProvideMock.Setup(x => x.UtcNow)
                .Returns(_utcDateNowForTests).Verifiable();
        }

        [Fact]
        public void Validate_DtoWithCategoryThatNotExist_ReturnsFailure()
        {
            var validator = CreateValidator(true);
            var dtoModel = GetValidCreatePublicationRequestDto();

            var result = validator.TestValidate(dtoModel);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public void Validate_DtoWithFutureIncidentDate_ReturnsFailure()
        {
            var validator = CreateValidator(false);
            var dtoModel = GetValidCreatePublicationRequestDto();
            dtoModel.IncidentDate = _utcDateNowForTests.AddDays(1);

            var result = validator.TestValidate(dtoModel);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public void Validate_DtoWithEmptyIncidentDate_ReturnsFailure()
        {
            var validator = CreateValidator(false);
            var dtoModel = GetValidCreatePublicationRequestDto();
            dtoModel.IncidentDate = default;

            var result = validator.TestValidate(dtoModel);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public void Validate_DtoWithEmptyTitle_ReturnsFailure()
        {
            var validator = CreateValidator(false);
            var dtoModel = GetValidCreatePublicationRequestDto();
            dtoModel.Title = String.Empty;

            var result = validator.TestValidate(dtoModel);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public void Validate_DtoWithEmptyDescription_ReturnsFailure()
        {
            var validator = CreateValidator(false);
            var dtoModel = GetValidCreatePublicationRequestDto();
            dtoModel.Description = String.Empty;

            var result = validator.TestValidate(dtoModel);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public void Validate_DtoWithEmptyIncidentAddress_ReturnsFailure()
        {
            var validator = CreateValidator(false);
            var dtoModel = GetValidCreatePublicationRequestDto();
            dtoModel.IncidentAddress = String.Empty;

            var result = validator.TestValidate(dtoModel);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public void Validate_DtoWithEmptySubjectCategory_ReturnsFailure()
        {
            var validator = CreateValidator(false);
            var dtoModel = GetValidCreatePublicationRequestDto();
            dtoModel.SubjectCategoryId = String.Empty;

            var result = validator.TestValidate(dtoModel);

            result.ShouldHaveAnyValidationError();
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(100)]
        public void Validate_DtoWithInvalidPublicationType_ReturnsFailure(int value)
        {
            var validator = CreateValidator(false);
            var dtoModel = GetValidCreatePublicationRequestDto();
            dtoModel.PublicationType = (PublicationType)value;

            var result = validator.TestValidate(dtoModel);

            result.ShouldHaveAnyValidationError();
        }

        [Theory]
        [MemberData(nameof(GetValidRequestDtos))]
        public void Validate_WithValidDto_ReturnsSuccess(UpdatePublicationDetailsRequestDto requestDto)
        {
            var validator = CreateValidator(false);

            var result = validator.TestValidate(requestDto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        public static IEnumerable<object[]> GetValidRequestDtos()
        {
            yield return new object[]
            {
                CreateFromDataCreateRegisterUserRequestDto("notEmpty", "notEmpty", "notEmpty",
                    DateTime.Now.AddDays(-1), "notEmpty", PublicationType.FoundSubject, PublicationState.Open)
            };

            yield return new object[]
            {
                CreateFromDataCreateRegisterUserRequestDto("notEmpty", "notEmpty", "notEmpty",
                    DateTime.Now.AddDays(-1), "notEmpty", PublicationType.LostSubject, PublicationState.Open)
            };

            yield return new object[]
            {
                CreateFromDataCreateRegisterUserRequestDto("notEmpty", "notEmpty", "notEmpty",
                    DateTime.Now.AddDays(-1), "notEmpty", PublicationType.LostSubject, PublicationState.Closed)
            };
        }

        private static UpdatePublicationDetailsRequestDto CreateFromDataCreateRegisterUserRequestDto(string title,
            string description, string address, DateTime date, string category, PublicationType type, PublicationState state)
        {
            return new UpdatePublicationDetailsRequestDto()
            {
                Title = title,
                Description = description,
                IncidentAddress = address,
                IncidentDate = date,
                SubjectCategoryId = category,
                PublicationType = type,
                PublicationState = state,
            };
        }

        private UpdatePublicationDetailsRequestDto GetValidCreatePublicationRequestDto()
        {
            return new UpdatePublicationDetailsRequestDto()
            {
                Title = "notEmpty",
                Description = "notEmpty",
                IncidentAddress = "notEmpty",
                IncidentDate = _utcDateNowForTests.AddDays(-1),
                SubjectCategoryId = "notEmpty",
                PublicationType = PublicationType.FoundSubject,
                PublicationState = PublicationState.Open,
            };
        }

        private UpdatePublicationDetailsRequestDtoValidator CreateValidator(bool doesCategoryExist)
        {
            _categoriesRepositoryMock
                .Setup(repo => repo.DoesCategoryExist(It.IsAny<string>()))
                .Returns<string>(_ => doesCategoryExist);

            return new UpdatePublicationDetailsRequestDtoValidator(
                _dateTimeProvideMock.Object, _categoriesRepositoryMock.Object);
        }
    }
}
