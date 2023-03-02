namespace Domain.Models;

public class Location
{
    public Location()
    {
    }

    public Location(string lng, string lat, string nearestPoint)
    {
        Lng = lng;
        Lat = lat;
        NearestPoint = nearestPoint;
    }

    public string NearestPoint { get; set; }
    public string Lng { get; set; }
    public string Lat { get; set; }
}