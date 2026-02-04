using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Api.IntegrationTests.Helpers
{
    public class TestDataBuilder
    {
        public static FormFile CreateDummyAvatar(string filename = "avatar.png")
        {
            return new FormFile(
                new MemoryStream(new byte[] { 1, 2, 3 }),
                0, 3,
                "Avatar",
                filename);
        }

        public static FormFile CreateDummyPhoto(string filename = "photo.jpg")
        {
            return new FormFile(
                new MemoryStream(new byte[] { 4, 5, 6 }),
                0, 3,
                "Photo",
                filename);
        }
    }
}