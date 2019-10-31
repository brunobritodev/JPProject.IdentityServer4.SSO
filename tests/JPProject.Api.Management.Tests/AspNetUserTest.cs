using System;
using System.Collections.Generic;
using System.Security.Claims;
using Bogus;
using JPProject.Domain.Core.Interfaces;

namespace JPProject.Api.Management.Tests
{
    public class AspNetUserTest : ISystemUser
    {
        private readonly Faker _faker;

        public AspNetUserTest()
        {
            _faker = new Bogus.Faker();
        }
        public string Username { get; } = "TestUser";
        public bool IsAuthenticated() => true;

        public string UserId { get; } = Guid.NewGuid().ToString();
        public IEnumerable<Claim> GetClaimsIdentity() => new List<Claim>();

        public string GetRemoteIpAddress() => _faker.Internet.Ip();

        public string GetLocalIpAddress() => _faker.Internet.Ip();
    }
}