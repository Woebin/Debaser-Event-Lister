using Debaser.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Debaser.Controllers
{
    public class EventsController : Controller
    {

        // 
        // GET: /Events/
        public async System.Threading.Tasks.Task<IActionResult> IndexAsync(string medis, string strand, string humlegarden, DateTime datFrom, DateTime datTo)
        {
            string from = datFrom.ToString("yyyyMMdd");
            string to = datTo.ToString("yyyyMMdd");
            string venues = ParseVenues(medis, strand, humlegarden);
            string url = $"http://www.debaser.se/debaser/api/?version=2&method=getevents&venue={venues}&from={from}&to={to}&format=json";

            string result = "";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(url);
                result = await response.Content.ReadAsStringAsync();
            }
            dynamic eventsJson = JsonConvert.DeserializeObject(result);

            var events = new List<Event>();
            if (eventsJson != null)
            {
                foreach (dynamic d in eventsJson)
                {
                    Event e = new Event
                    {
                        EventId = d["EventId"],
                        EventName = d["Event"],
                        EventDate = d["EventDate"],
                        SubHead = d["SubHead"],
                        Venue = d["Venue"],
                        Description = d["Description"]
                    };
                    FormatEventAttributes(e);
                    events.Add(e);
                }
            }
            ViewData["Result"] = events;

            return View();
        }

        private static string ParseVenues(string medis, string strand, string humlegarden)
        {
            string venues = "";
            if (!string.IsNullOrEmpty(medis))
            {
                venues += "medis";
            }
            if (!string.IsNullOrEmpty(strand))
            {
                if (!string.IsNullOrEmpty(venues))
                {
                    venues += ',';
                }
                venues += "strand";
            }
            if (!string.IsNullOrEmpty(humlegarden))
            {
                if (!string.IsNullOrEmpty(venues))
                {
                    venues += ',';
                }
                venues += "humlegarden";
            }
            // Default to listing all venues if none are selected.
            return string.IsNullOrEmpty(venues) ? "medis,strand,humlegarden" : venues;
        }

        private static void FormatEventAttributes(Event e)
        {
            e.EventName = e.EventName.Replace("&amp;", "&");
            e.SubHead = e.SubHead.Replace("&amp;", "&");
            e.Description = e.Description.Replace("&amp;", "&");
            e.Description = e.Description.Replace("&nbsp;", "\u00A0");
        }
    }
}