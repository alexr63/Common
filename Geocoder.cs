using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;

    /// <summary>
    /// Resolve addresses into latitude/longitude coordinates
    /// </summary>

namespace Common
{
    public static class Geocoder
    {
        private static string _GoogleMapsKey;
        private static string _WhereIsKey;

        /// <summary>
        /// Google.com Geocoder
        /// Useful in: USA, Canada, France, Spain, Italy and Germany
        /// </summary>
        /// <remarks>
        /// Url request to
        /// http://maps.google.com/maps/geo?q=1600+Amphitheatre+Parkway,+Mountain+View,+CA&output=xml&key=xxxxxxxxxxxxxxxx
        /// and response in the format:
        /// <![CDATA[
        /// <?xml version="1.0" encoding="UTF-8"?>
        /// <kml xmlns="http://earth.google.com/kml/2.0">
        ///     <Response>
        ///         <name>1 Macquarie Street,Chatswood ,NSW,Australia</name>
        /// 	    <Status>
        ///             <code>200</code>
        ///      		<request>geocode</request>
        /// 	    </Status>
        ///         <Placemark>
        /// 	        <address>1 Macquarie St, Chatswood, NSW 2067, Australia</address>
        /// 	        <AddressDetails Accuracy="8" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0">
        ///                 <Country>
        ///                     <CountryNameCode>AU</CountryNameCode>
        ///                     <AdministrativeArea>
        ///                         <AdministrativeAreaName>NSW</AdministrativeAreaName>
        ///                         <Locality>
        ///                             <LocalityName>Chatswood</LocalityName>
        ///                             <Thoroughfare>
        ///                                 <ThoroughfareName>13 Macquarie St</ThoroughfareName>
        ///                             </Thoroughfare>
        ///                             <PostalCode>
        ///                                 <PostalCodeNumber>2067</PostalCodeNumber>
        ///                             </PostalCode>
        ///                         </Locality>
        ///                     </AdministrativeArea>
        ///                 </Country>
        ///             </AddressDetails>
        ///             <Point>
        ///                 <coordinates>151.191666,-33.792181,0</coordinates>
        ///             </Point>
        ///         </Placemark>
        ///     </Response>
        /// </kml>
        /// ]]>
        /// </remarks>
        /// <returns></returns>
        public static Geoloc? LocateGoogle(string query)
        {
            if (string.IsNullOrEmpty(_GoogleMapsKey))
                _GoogleMapsKey = System.Configuration.ConfigurationManager.AppSettings["GoogleMapsKey"];

            string url = "http://maps.google.com/maps/geo?q={0}&output=xml&key=" + _GoogleMapsKey;
                //ABQIAAAAqk5s3ZgDfx6Fror8PkaM3BS7dMl9tWcZKqHtzShLYcoogiPxgBTt2AL3Uh361Po66T0RC10XiuyUnw";
            url = String.Format(url, query);
            XmlNode coords = null;
            try
            {
                string xmlString = GetUrl(url);
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(xmlString);
                XmlNamespaceManager xnm = new XmlNamespaceManager(xd.NameTable);
                //coords = xd.SelectSingleNode("/").ChildNodes[1].ChildNodes[0].ChildNodes[2].LastChild;
                coords = xd.GetElementsByTagName("coordinates")[0];
            }
            catch
            {
            }
            Geoloc? gl = null;
            if (coords != null)
            {
                string[] coordinateArray = coords.InnerText.Split(',');
                if (coordinateArray.Length >= 2)
                {
                    gl = new Geoloc(Convert.ToDouble(coordinateArray[1].ToString()),
                        Convert.ToDouble(coordinateArray[0].ToString()));
                }
            }
            return gl;
        }

        public static string NearestAddressGoogle(string query, out string streetNumber, out string streetName,
            out string locality, out string postalCode)
        {
            streetNumber = String.Empty;
            streetName = String.Empty;
            locality = String.Empty;
            postalCode = String.Empty;
            if (string.IsNullOrEmpty(_GoogleMapsKey))
                _GoogleMapsKey = System.Configuration.ConfigurationManager.AppSettings["GoogleMapsKey"];

            string url = "http://maps.google.com/maps/api/geocode/xml?address={0}&sensor=false";
                // +_GoogleMapsKey; //ABQIAAAAqk5s3ZgDfx6Fror8PkaM3BS7dMl9tWcZKqHtzShLYcoogiPxgBTt2AL3Uh361Po66T0RC10XiuyUnw";
            url = String.Format(url, query);
            XmlNode coords = null;
            try
            {
                string xmlString = GetUrl(url);
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(xmlString);
                XmlNamespaceManager xnm = new XmlNamespaceManager(xd.NameTable);
                //coords = xd.SelectSingleNode("/").ChildNodes[1].ChildNodes[0].ChildNodes[2].LastChild;
                coords = xd.GetElementsByTagName("formatted_address")[0];

                XElement root = XElement.Parse(xmlString);
                var addresses = from e in root.Element("result").Elements("address_component")
                    select e;
                foreach (XElement address in addresses)
                {
                    if (address.Element("type").Value == "street_number")
                    {
                        streetNumber = address.Element("long_name").Value;
                    }
                    if (address.Element("type").Value == "route")
                    {
                        streetName = address.Element("long_name").Value;
                    }
                    if (address.Element("type").Value == "locality")
                    {
                        locality = address.Element("long_name").Value;
                    }
                    if (address.Element("type").Value == "postal_code")
                    {
                        postalCode = address.Element("long_name").Value;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            if (coords != null)
            {
                return coords.InnerText;
            }
            return String.Empty;
        }

        /// <summary>
        /// Keep the same signature as the WhereIs geocode
        /// </summary>
        public static Geoloc? LocateGoogle(string address, string suburb, string state, string postcode)
        {
            return LocateGoogle(address + "," + suburb + "," + state + ",Australia");
        }

        /// <summary>
        /// WhereIs.com Geocoder (Australia only)
        /// </summary>
        /// <remarks>
        /// Url request to
        /// http://api.workshop.whereis.com/geo?number=1&street=martin&type=place&suburb=sydney&state=nsw&ver=1&ref=localhost&key=xxxxxxxxxx
        /// Response format
        /// <![CDATA[
        /// <xml>
        ///     <GeocodeResult>
        ///         <accuracy>94.583336</accuracy>
        ///         <address>
        ///             <coordinates>
        ///                 <longitude>151.214795</longitude>
        ///                 <latitude>-33.881185</latitude>
        ///             </coordinates>
        ///             <street>
        ///                 <name>Crown</name>
        ///                 <type>St</type>
        ///                 <directionalPrefix></directionalPrefix><directionalSuffix></directionalSuffix>
        ///                 <fullName>Crown St</fullName>
        ///             </street>
        ///             <intersectingStreet>null</intersectingStreet>
        ///             <suburb>Surry Hills</suburb>
        ///             <state></state>
        ///             <postcode></postcode>
        ///             <countryCode>AU</countryCode>
        ///         </address>
        ///     </GeocodeResult>
        /// </xml>
        /// ]]>
        /// </remarks>
        /// <returns></returns>
        public static Geoloc? LocateWhereIs(string number, string address, string streettype, string suburb,
            string state, string postcode)
        {
            if (string.IsNullOrEmpty(_WhereIsKey))
                _WhereIsKey = System.Configuration.ConfigurationManager.AppSettings["WhereIsKey"];
            string url =
                "http://api.workshop.whereis.com/geo?number={0}&street={1}&type={2}&suburb={3}&state={4}&ver=1&ref=localhost&key=" +
                _WhereIsKey;
            string[] numberArray = number.Split('-', ' ', '/');
            number = numberArray.Length > 0 ? numberArray[0] : number;
            url = String.Format(url, number, address, streettype, suburb, state);
            XmlNode lon = null, lat = null;
            try
            {
                string xmlString = GetUrl(url);
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(xmlString);
                lon = xd.SelectSingleNode("/xml/GeocodeResult/address/coordinates/longitude");
                lat = xd.SelectSingleNode("/xml/GeocodeResult/address/coordinates/latitude");
            }
            catch
            {
            }
            Geoloc? gl = null;
            if (lon != null && lat != null)
            {
                gl = new Geoloc(Convert.ToDouble(lat.InnerText), Convert.ToDouble(lon.InnerText));
            }
            return gl;
        }

        /// <summary>
        /// Matches LocateGoogle signature
        /// </summary>
        public static Geoloc? LocateWhereIs(string address, string suburb, string state, string postcode)
        {
            return LocateWhereIs("", address, "", suburb, state, postcode);
        }

        /// <summary>
        /// Retrieve a Url via WebClient
        /// </summary>
        /// <param name="url">Url to query (METHOD=GET)</param>
        /// <returns>Result stream (assumed to be Xml)</returns>
        private static string GetUrl(string url)
        {
            string result = string.Empty;
            System.Net.WebClient Client = new WebClient();

            using (Stream strm = Client.OpenRead(url))
            {
                StreamReader sr = new StreamReader(strm);
                result = sr.ReadToEnd();
            }
            return result;
        }
    }
}
