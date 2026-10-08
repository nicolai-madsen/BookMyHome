using BookMyHome.Domain.Aggregates.Accommodations;
using BookMyHome.Shared.DomainDtos;

namespace BookMyHome.Api.Mapping
{
    public static class AccommodationMappingExtensions
    {
        public static AccommodationDto ToDto(this Accommodation a) => new(
            a.Id,
            a.HostId,
            a.Address.StreetName,
            a.Address.StreetNumber,
            a.Address.City,
            a.Address.ZipCode,
            a.Address.Country,
            a.AvailablePeriod.StartDate,
            a.AvailablePeriod.EndDate,
            a.PricePerDay);
    }
}