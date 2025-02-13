﻿using Adapter.Dapper.AuctionhouseDatabase;
using FluentAssertions;
using System;
using TestConfigurationAccessor;
using Users.Domain.Auth;
using Users.Domain.Repositories;
using Xunit;

namespace Test.Dapper.AuctionhouseDatabase
{
    [Trait("Category", "Integration")]
    public class UserAuthDataRepository_Tests
    {
        private IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private UserAuthenticationData authData = new UserAuthenticationData()
        {
            UserId = Guid.NewGuid(),
            Password = "1234",
            UserName = Guid.NewGuid().ToString().Substring(0, 15),
            Email = "mail@mail.com"
        };

        public UserAuthDataRepository_Tests()
        {
            var repositorySettings = TestConfig.Instance.GetRepositorySettings();
            _userAuthenticationDataRepository = new UserAuthenticationDataRepository(repositorySettings);
        }

        [Fact]
        public void AddUserAuth_adds_auth_data_and_find_by_id_finds_it()
        {
            _userAuthenticationDataRepository.AddUserAuth(authData);

            var found = _userAuthenticationDataRepository.FindUserAuthById(authData.UserId);

            found.Should().NotBeNull();
            found.Should().BeEquivalentTo(authData);
        }

        [Fact]
        public void AddUserAuth_adds_auth_data_and_find_by_username_finds_it()
        {
            _userAuthenticationDataRepository.AddUserAuth(authData);

            var found = _userAuthenticationDataRepository.FindUserAuth(authData.UserName);

            found.Should().NotBeNull();
            found.Should().BeEquivalentTo(authData);
        }

        [Fact]
        public void FindUserAuth_if_auth_data_does_not_exist_returns_null()
        {
            var found = _userAuthenticationDataRepository.FindUserAuth("xxxx");

            found.Should().BeNull();
        }

        [Fact]
        public void FindUserAuthById_if_auth_data_does_not_exist_returns_null()
        {
            var found = _userAuthenticationDataRepository.FindUserAuthById(Guid.NewGuid());

            found.Should().BeNull();
        }
    }
}
