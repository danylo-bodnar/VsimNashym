using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace api.DTOs
{
    public class NearbyUserDto
    {
        public long TelegramId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? ProfilePhotoFileId { get; set; }
        public LocationPoint Location { get; set; } = null!;
    }

    public class LocationPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}