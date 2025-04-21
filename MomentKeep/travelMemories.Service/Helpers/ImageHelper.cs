using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace TravelMemories.Service.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Extracts the date a photo was taken from EXIF metadata if available
        /// </summary>
        public static DateTime? GetImageTakenDate(IFormFile imageFile)
        {
            try
            {
                using (var stream = imageFile.OpenReadStream())
                {
                    var image = Image.Load(stream);

                    if (image.Metadata?.ExifProfile != null)
                    {
                        // Try to get the date taken from EXIF data
                        if (image.Metadata.ExifProfile.TryGetValue(ExifTag.DateTimeOriginal, out var dateTimeValue))
                        {
                            if (DateTime.TryParse(dateTimeValue.ToString(), out var dateTaken))
                            {
                                return dateTaken;
                            }
                        }

                        // Try alternative EXIF tags if the primary one is missing
                        if (image.Metadata.ExifProfile.TryGetValue(ExifTag.DateTime, out dateTimeValue))
                        {
                            if (DateTime.TryParse(dateTimeValue.ToString(), out var dateModified))
                            {
                                return dateModified;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail and return null if EXIF data cannot be read
            }

            // If no EXIF data is available, use the current date/time
            return null;
        }

        /// <summary>
        /// Gets the GPS coordinates from image EXIF data if available
        /// </summary>
        public static (double? Latitude, double? Longitude) GetGpsCoordinates(IFormFile imageFile)
        {
            try
            {
                using (var stream = imageFile.OpenReadStream())
                {
                    var image = Image.Load(stream);

                    if (image.Metadata?.ExifProfile != null)
                    {
                        double? latitude = null;
                        double? longitude = null;

                        // Try to extract the GPS coordinates
                        if (TryGetGpsCoordinate(image.Metadata.ExifProfile, ExifTag.GPSLatitude, ExifTag.GPSLatitudeRef, out var lat))
                        {
                            latitude = lat;
                        }

                        if (TryGetGpsCoordinate(image.Metadata.ExifProfile, ExifTag.GPSLongitude, ExifTag.GPSLongitudeRef, out var lon))
                        {
                            longitude = lon;
                        }

                        return (latitude, longitude);
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail and return nulls
            }

            return (null, null);
        }

        private static bool TryGetGpsCoordinate(ExifProfile profile, ExifTag<Rational[]> coordinateTag, ExifTag<string> referenceTag, out double coordinate)
        {
            coordinate = 0;

            if (!profile.TryGetValue(coordinateTag, out IExifValue<Rational[]> coordinateValue) ||
                !profile.TryGetValue(referenceTag, out IExifValue<string> referenceValue))
            {
                return false;
            }

            var parts = coordinateValue.Value.ToString().Split(',');
            if (parts.Length != 3)
            {
                return false;
            }

            var degrees = TryParseRational(parts[0].Trim());
            var minutes = TryParseRational(parts[1].Trim());
            var seconds = TryParseRational(parts[2].Trim());

            if (!degrees.HasValue || !minutes.HasValue || !seconds.HasValue)
            {
                return false;
            }

            // Convert degrees, minutes, seconds to decimal degrees
            coordinate = degrees.Value + minutes.Value / 60 + seconds.Value / 3600;

            // Apply reference direction (N/S or E/W)
            var reference = referenceValue.Value.Trim();
            if (reference == "S" || reference == "W")
            {
                coordinate = -coordinate;
            }

            return true;
        }

        private static double? TryParseRational(string rational)
        {
            if (string.IsNullOrEmpty(rational))
            {
                return null;
            }

            var parts = rational.Split('/');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out var numerator) &&
                double.TryParse(parts[1], out var denominator) &&
                denominator != 0)
            {
                return numerator / denominator;
            }
            else if (double.TryParse(rational, out var number))
            {
                return number;
            }

            return null;
        }
    }
}