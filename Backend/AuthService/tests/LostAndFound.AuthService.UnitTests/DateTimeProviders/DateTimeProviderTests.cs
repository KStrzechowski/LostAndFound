﻿using FluentAssertions;
using LostAndFound.AuthService.Core.DateTimeProviders;
using System;
using Xunit;

namespace LostAndFound.AuthService.UnitTests.DateTimeProviders
{
    public class DateTimeProviderTests
    {
        private readonly DateTimeProvider _dateTimeProvider;

        public DateTimeProviderTests()
        {
            _dateTimeProvider = new DateTimeProvider();
        }

        [Fact]
        public void UtcNow_ReturnsDateTimeWithUtcTimeZone()
        {
            var utcDateTimeNow = _dateTimeProvider.UtcNow;

            var diffrence = utcDateTimeNow.ToUniversalTime() - utcDateTimeNow;
            diffrence.Should().Be(TimeSpan.Zero);
        }
    }
}
