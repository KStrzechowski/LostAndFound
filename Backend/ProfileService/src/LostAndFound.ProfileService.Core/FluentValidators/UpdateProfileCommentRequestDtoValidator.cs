﻿using FluentValidation;
using LostAndFound.ProfileService.CoreLibrary.Requests;

namespace LostAndFound.ProfileService.Core.FluentValidators
{
    public class UpdateProfileCommentRequestDtoValidator : AbstractValidator<UpdateProfileCommentRequestDto>
    {
        public UpdateProfileCommentRequestDtoValidator()
        {
            RuleFor(dto => dto.Content)
                .NotEmpty();

            RuleFor(dto => dto.ProfileRating)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(5);
        }
    }
}
