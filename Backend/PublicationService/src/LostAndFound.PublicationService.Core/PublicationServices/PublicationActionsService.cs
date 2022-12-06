﻿using AutoMapper;
using LostAndFound.PublicationService.Core.DateTimeProviders;
using LostAndFound.PublicationService.Core.PublicationServices.Interfaces;
using LostAndFound.PublicationService.CoreLibrary.Enums;
using LostAndFound.PublicationService.CoreLibrary.Exceptions;
using LostAndFound.PublicationService.CoreLibrary.Internal;
using LostAndFound.PublicationService.CoreLibrary.Requests;
using LostAndFound.PublicationService.CoreLibrary.ResourceParameters;
using LostAndFound.PublicationService.CoreLibrary.Responses;
using LostAndFound.PublicationService.DataAccess.Entities;
using LostAndFound.PublicationService.DataAccess.Entities.PublicationEnums;
using LostAndFound.PublicationService.DataAccess.Repositories.Interfaces;
using LostAndFound.PublicationService.ThirdPartyServices.AzureServices.Interfaces;
using LostAndFound.PublicationService.ThirdPartyServices.Models;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace LostAndFound.PublicationService.Core.PublicationServices
{
    public class PublicationActionsService : IPublicationActionsService
    {
        private readonly IPublicationsRepository _publicationsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        //private readonly IGeocodingService _geocodingService;

        public PublicationActionsService(IPublicationsRepository publicationsRepository, ICategoriesRepository categoriesRepository,
            IDateTimeProvider dateTimeProvider, IMapper mapper, IFileStorageService fileStorageService/*, IGeocodingService geocodingService*/)
        {
            _publicationsRepository = publicationsRepository ?? throw new ArgumentNullException(nameof(publicationsRepository));
            _categoriesRepository = categoriesRepository ?? throw new ArgumentNullException(nameof(categoriesRepository));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            // _geocodingService = geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));
        }

        public async Task<PublicationDetailsResponseDto> CreatePublication(string rawUserId, string username,
            CreatePublicationRequestDto publicationDto, IFormFile subjectPhoto)
        {
            var userId = ParseUserId(rawUserId);

            var category = (await _categoriesRepository
                .FilterByAsync(cat => cat.ExposedId == publicationDto.SubjectCategoryId))
                .FirstOrDefault();
            if (category is null)
            {
                throw new BadRequestException("Category with this id does not exist");
            }

            var publicationEntity = _mapper.Map<Publication>(publicationDto);
            if (publicationEntity == null)
            {
                throw new BadRequestException("The publication data is incorrect.");
            }
            publicationEntity.CreationTime = publicationEntity.LastModificationDate = _dateTimeProvider.UtcNow;
            publicationEntity.Author = new Author()
            {
                Id = userId,
                Username = username,
            };
            publicationEntity.SubjectCategoryName = category.DisplayName;
            // TODO: Add latitude calclulation

            if (subjectPhoto is not null && subjectPhoto.Length > 0)
            {
                FileDto fileDto = CreateFileDto(subjectPhoto);
                publicationEntity.SubjectPhotoUrl = await _fileStorageService.UploadAsync(fileDto);
            }

            await _publicationsRepository.InsertOneAsync(publicationEntity);

            return await GetPublicationDetails(publicationEntity.ExposedId, userId);
        }

        public async Task DeletePublicationPhoto(string rawUserId, Guid publicationId)
        {
            var userId = ParseUserId(rawUserId);
            var publicationEntity = await GetUserPublication(userId, publicationId);

            await DeletePublicationPhotoFromBlob(publicationEntity.SubjectPhotoUrl);

            await _publicationsRepository.UpdatePublicationPhotoUrl(userId, null);
        }

        public async Task<PublicationDetailsResponseDto> UpdatePublicationPhoto(IFormFile photo, string rawUserId, Guid publicationId)
        {
            var userId = ParseUserId(rawUserId);
            var publicationEntity = await GetUserPublication(userId, publicationId);
            FileDto fileDto = CreateFileDto(photo);

            if (publicationEntity.SubjectPhotoUrl is not null)
            {
                await DeletePublicationPhotoFromBlob(publicationEntity.SubjectPhotoUrl);
            }

            var photoUrl = await _fileStorageService.UploadAsync(fileDto);
            await _publicationsRepository.UpdatePublicationPhotoUrl(publicationId, photoUrl);

            return await GetPublicationDetails(publicationId, userId);
        }

        public async Task<PublicationDetailsResponseDto> UpdatePublicationDetails(string rawUserId, Guid publicationId,
            UpdatePublicationDetailsRequestDto publicationDetailsDto)
        {
            var userId = ParseUserId(rawUserId);

            var category = (await _categoriesRepository
                .FilterByAsync(cat => cat.ExposedId == publicationDetailsDto.SubjectCategoryId))
                .FirstOrDefault();
            if (category is null)
            {
                throw new BadRequestException("Category with this id does not exist");
            }

            var publicationEntity = await GetUserPublication(userId, publicationId);
            _mapper.Map(publicationDetailsDto, publicationEntity);
            publicationEntity.SubjectCategoryName = category.DisplayName;

            // TODO: Add latitude calclulation
            await _publicationsRepository.ReplaceOneAsync(publicationEntity);

            return await GetPublicationDetails(publicationId, userId);
        }

        public async Task DeletePublication(string rawUserId, Guid publicationId)
        {
            var userId = ParseUserId(rawUserId);
            var publicationEntity = await GetUserPublication(userId, publicationId);

            await _publicationsRepository.DeleteOneAsync(pub => pub.ExposedId == publicationId);
        }

        public async Task<(IEnumerable<PublicationBaseDataResponseDto>?, PaginationMetadata)> GetPublications(string rawUserId,
            PublicationsResourceParameters resourceParameters)
        {
            var userId = ParseUserId(rawUserId);

            var filterExpression = CreateFilterExpression(resourceParameters, userId);
            var publications = await _publicationsRepository.UseFilterDefinition(filterExpression);

            var publicationsPage = publications.OrderByDescending(pub => pub.IncidentDate)
                .Skip(resourceParameters.PageSize * (resourceParameters.PageNumber - 1))
                .Take(resourceParameters.PageSize)
                .ToList();

            var publicationDtos = Enumerable.Empty<PublicationBaseDataResponseDto>();
            if (publicationsPage != null && publicationsPage.Any())
            {
                publicationDtos = _mapper.Map<IEnumerable<PublicationBaseDataResponseDto>>(publicationsPage);

                if(publicationDtos != null && publicationDtos.Any())
                {
                    foreach (var it in publicationDtos.Zip(publicationsPage, Tuple.Create))
                    {
                        var userVote = it.Item2.Votes.FirstOrDefault(x => x.VoterId == userId);

                        it.Item1.UserVote = userVote is null ?
                            SinglePublicationVote.NoVote : _mapper.Map<SinglePublicationVote>(userVote.Rating);
                    }
                }
            }

            int totalItemCount = publications.Count();
            var paginationMetadata = new PaginationMetadata(totalItemCount, resourceParameters.PageSize, resourceParameters.PageNumber);

            return (publicationDtos, paginationMetadata);
        }

        private FilterDefinition<Publication> CreateFilterExpression(PublicationsResourceParameters resourceParameters, Guid userId)
        {
            var builder = Builders<Publication>.Filter;
            var state = _mapper.Map<State>(resourceParameters.PublicationState);
            var filter = builder.Eq(pub => pub.State, state);

            var type = _mapper.Map<DataAccess.Entities.PublicationEnums.Type>(resourceParameters.PublicationType);
            filter &= builder.Eq(pub => pub.Type, type);

            if (resourceParameters.OnlyUserPublications)
                filter &= builder.Eq(pub => pub.Author.Id, userId);

            if(resourceParameters.SubjectCategoryId != null)
                filter &= builder.Eq(pub => pub.SubjectCategoryId, resourceParameters.SubjectCategoryId);

            if(resourceParameters.FromDate != null)
                filter &= builder.Gte(pub => pub.IncidentDate, resourceParameters.FromDate);

            if (resourceParameters.ToDate != null)
                filter &= builder.Lte(pub => pub.IncidentDate, resourceParameters.ToDate);

            if (!String.IsNullOrEmpty(resourceParameters.SearchQuery))
                filter &= (builder.StringIn(pub => pub.Description, resourceParameters.SearchQuery) |
                    builder.StringIn(pub => pub.Title, resourceParameters.SearchQuery));

            return filter;
        }

        public async Task<PublicationDetailsResponseDto> UpdatePublicationState(string rawUserId,
            Guid publicationId, UpdatePublicationStateRequestDto publicationStateDto)
        {
            var userId = ParseUserId(rawUserId);
            _ = await GetUserPublication(userId, publicationId);

            var state = _mapper.Map<State>(publicationStateDto.PublicationState);
            await _publicationsRepository.UpdatePublicationState(publicationId, state);

            return await GetPublicationDetails(publicationId, userId);
        }

        public async Task<PublicationDetailsResponseDto> UpdatePublicationRating(string rawUserId, Guid publicationId,
            UpdatePublicationRatingRequestDto publicationRatingDto)
        {
            var userId = ParseUserId(rawUserId);
            var publicationEntity = await GetUserPublication(userId, publicationId);

            var userVote = publicationEntity.Votes.FirstOrDefault(x => x.VoterId == userId);
            if (userVote != null)
            {
                if (publicationRatingDto.NewPublicationVote == SinglePublicationVote.NoVote)
                {
                    await _publicationsRepository.DeletePublicationVote(publicationId, userVote);
                }
                else
                {
                    _mapper.Map(publicationRatingDto, userVote);
                    await _publicationsRepository.UpdatePublicationVote(publicationId, userVote);
                }
            }
            else
            {
                var voteEntity = _mapper.Map<Vote>(publicationRatingDto);
                if (voteEntity is null)
                {
                    throw new BadRequestException("The vote data is incorrect.");
                }
                voteEntity.CreationDate = _dateTimeProvider.UtcNow;
                voteEntity.VoterId = userId;

                await _publicationsRepository.InsertNewPublicationVote(publicationId, voteEntity);
            }

            return await GetPublicationDetails(publicationId, userId);
        }

        public async Task<PublicationDetailsResponseDto> GetPublicationDetails(string rawUserId, Guid publicationId)
        {
            var userId = ParseUserId(rawUserId);

            return await GetPublicationDetails(publicationId, userId);
        }

        private async Task<PublicationDetailsResponseDto> GetPublicationDetails(Guid publicationId, Guid userId)
        {
            var publicationEntity = await _publicationsRepository.GetSingleAsync(x => x.ExposedId == publicationId);
            if (publicationEntity == null)
            {
                throw new NotFoundException("Publication not found.");
            }
            var userVote = publicationEntity.Votes.FirstOrDefault(x => x.VoterId == userId);

            var publicationDetailsDto = _mapper.Map<PublicationDetailsResponseDto>(publicationEntity);
            publicationDetailsDto.UserVote = userVote is null ?
                SinglePublicationVote.NoVote : _mapper.Map<SinglePublicationVote>(userVote.Rating);

            return publicationDetailsDto;
        }

        private static FileDto CreateFileDto(IFormFile photo)
        {
            var fileDto = new FileDto()
            {
                Content = photo.OpenReadStream(),
                Name = photo.FileName,
                ContentType = photo.ContentType,
            };
            if (fileDto == null || fileDto.Content.Length == 0)
            {
                throw new BadRequestException("The profile picture is incorrect");
            }

            return fileDto;
        }

        private async Task DeletePublicationPhotoFromBlob(string? photoUrl)
        {
            var blobName = Path.GetFileName(photoUrl);
            if (blobName == null)
            {
                throw new NotFoundException("Publication photo not found.");
            }

            await _fileStorageService.DeleteAsync(blobName);
        }

        private async Task<Publication> GetUserPublication(Guid userId, Guid publicationId)
        {
            var publicationEntity = await _publicationsRepository.GetSingleAsync(x => x.ExposedId == publicationId);
            if (publicationEntity == null)
            {
                throw new NotFoundException("Publication not found.");
            }

            if (publicationEntity.Author.Id != userId)
            {
                throw new UnauthorizedException();
            }

            return publicationEntity;
        }

        private static Guid ParseUserId(string rawUserId)
        {
            if (!Guid.TryParse(rawUserId, out Guid userId))
            {
                throw new UnauthorizedException();
            }

            return userId;
        }
    }
}
