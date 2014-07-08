using System;

namespace Common
{
    public struct Geoloc
    {
        public double Lat;
        public double Lon;

        public Geoloc(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public override string ToString()
        {
            return "Latitude: " + Lat + " Longitude: " + Lon;
        }

        public string ToQueryString()
        {
            return "+to:" + Lat + "%2B" + Lon;
        }
    }
}
