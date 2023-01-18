using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using Google.XR.ARCoreExtensions.Samples.Geospatial;

public class LocationManager : MonoBehaviour
{
    private bool isFirstTime = true;
    private float lastLat, lastLon;
    private float lastDistance;
    public static LocationManager instance;
    public int MEAL = 1, WRESTLER = 2, DOCTOR = 3;
    string API_KEY = "YOUR_MAPS_API_KEY";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public IEnumerator GetDistanceCalculationStarted()
    {
        yield return new WaitForSeconds(2f);

        //Do nothing if location services are not available
        if (Input.location.isEnabledByUser)
        {
            if (isFirstTime)
            {
                lastLat = Input.location.lastData.latitude;
                lastLon = Input.location.lastData.longitude;
                string foodUri = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + lastLat + "," + lastLon + "&radius=500&type=restaurant&key=" + API_KEY;
                string doctorUri = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + lastLat + "," + lastLon + "&radius=500&type=doctor&key=" + API_KEY;
                string gymUri = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + lastLat + "," + lastLon + "&radius=500&type=gym&key=" + API_KEY;
                StartCoroutine(GetRequest(foodUri, MEAL));
                StartCoroutine(GetRequest(doctorUri, DOCTOR));
                StartCoroutine(GetRequest(gymUri, WRESTLER));

                isFirstTime = false;
            }
            float lat;
            float lon;
            while (true)
            {
                lat = Input.location.lastData.latitude;
                lon = Input.location.lastData.longitude;

                // singleText.text = "Depart lat: " + lat + "  lon: " + lon;
                lastDistance = CalculateDistance(lastLat, lat, lastLon, lon);
                Debug.Log("Last " + lastDistance.ToString());
                // Currentstatus.text = lastLat + "  Lon " + lastLon;
                if (lastDistance > 200.0f)
                {
                    GeospatialController.instance.RemoveAllLocations();
                    lastLat = lat;
                    lastLon = lon;
                    lastDistance = 0;
                    Debug.Log("Distance is Greter then 200 meter");
                    string foodUri = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + lat + "," + lon + "&radius=500&type=restaurant&key=" + API_KEY;
                    string doctorUri = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + lat + "," + lon + "&radius=500&type=hospital&key=" + API_KEY;
                    string gymUri = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + lat + "," + lon + "&radius=500&type=gym&key=" + API_KEY;
                    StartCoroutine(GetRequest(foodUri, MEAL));
                    StartCoroutine(GetRequest(doctorUri, DOCTOR));
                    StartCoroutine(GetRequest(gymUri, WRESTLER));
                }
                yield return new WaitForSeconds(2f);
            }
        }
    }

    private float CalculateDistance(float lat_1, float lat_2, float long_1, float long_2)
    {
        int R = 6371;
        var lat_rad_1 = Mathf.Deg2Rad * lat_1;
        var lat_rad_2 = Mathf.Deg2Rad * lat_2;
        var d_lat_rad = Mathf.Deg2Rad * (lat_2 - lat_1);
        var d_long_rad = Mathf.Deg2Rad * (long_2 - long_1);
        var a = Mathf.Pow(Mathf.Sin(d_lat_rad / 2), 2) + (Mathf.Pow(Mathf.Sin(d_long_rad / 2), 2) * Mathf.Cos(lat_rad_1) * Mathf.Cos(lat_rad_2));
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        var total_dist = R * c * 1000; // convert to meters
        return total_dist;
    }

    IEnumerator GetRequest(string uri, int modelType)
    {
        Debug.Log("API call coroutine     " + uri);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            Debug.Log("API RESPONSE SWITCH");

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    Root root = new Root();
                    root = JsonConvert.DeserializeObject<Root>(webRequest.downloadHandler.text);
                    // Debug.Log("Result Catched" + result.name + ":    : " + result);
                    for (int i = 0; i < root.results.Count; i++)
                    {
                        Debug.Log("### names " + root.results[i].name);
                        GeospatialController.instance.AddLocation(root.results[i].geometry.location.lat, root.results[i].geometry.location.lng, modelType);
                    }
                    break;
            }
        }
    }

    [Serializable]
    public class Geometry
    {
        public Location location { get; set; }
        public Viewport viewport { get; set; }
    }

    [Serializable]
    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    [Serializable]
    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    [Serializable]
    public class OpeningHours
    {
        public bool open_now { get; set; }
    }

    [Serializable]
    public class Photo
    {
        public int height { get; set; }
        public List<string> html_attributions { get; set; }
        public string photo_reference { get; set; }
        public int width { get; set; }
    }

    [Serializable]
    public class PlusCode
    {
        public string compound_code { get; set; }
        public string global_code { get; set; }
    }

    [Serializable]
    public class Result
    {
        public string business_status { get; set; }
        public Geometry geometry { get; set; }
        public string icon { get; set; }
        public string icon_background_color { get; set; }
        public string icon_mask_base_uri { get; set; }
        public string name { get; set; }
        public OpeningHours opening_hours { get; set; }
        public List<Photo> photos { get; set; }
        public string place_id { get; set; }
        public PlusCode plus_code { get; set; }
        public double rating { get; set; }
        public string reference { get; set; }
        public string scope { get; set; }
        public List<string> types { get; set; }
        public int user_ratings_total { get; set; }
        public string vicinity { get; set; }
        public bool? permanently_closed { get; set; }
    }

    [Serializable]
    public class Root
    {
        public List<object> html_attributions { get; set; }
        public List<Result> results { get; set; }
        public string status { get; set; }
    }

    [Serializable]
    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    [Serializable]
    public class Viewport
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }
}
