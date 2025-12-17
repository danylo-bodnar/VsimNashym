
using api.DTOs.Users;

namespace api.Utils
{
    public static class GeoUtils
    {
        private const double EarthRadiusKm = 6371;

        public static double CalculateDistanceKm(
            LocationPointDto a,
            LocationPointDto b)
        {
            double dLat = DegreesToRadians(b.Latitude - a.Latitude);
            double dLon = DegreesToRadians(b.Longitude - a.Longitude);

            double lat1 = DegreesToRadians(a.Latitude);
            double lat2 = DegreesToRadians(b.Latitude);

            double h =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
            return EarthRadiusKm * c;
        }

        private static double DegreesToRadians(double deg)
            => deg * Math.PI / 180;
    }
}
