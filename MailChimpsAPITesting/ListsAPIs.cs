using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MailChimpsAPITesting
{
    
    [TestClass]
    public class ListTest
    {
        string endPoint = "https://us17.api.mailchimp.com/3.0/";
        string key = "d13d89c0d2f1074e165f06302450314a-us17";


        [TestMethod]
        public void ShowListInfo()
        {
            var result = "";
            using (var webClient = new WebClient())
            {
                webClient.BaseAddress = endPoint;
                try
                {
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(key));
                    webClient.Headers[HttpRequestHeader.Authorization] = "Basic " +key;
                    result = webClient.DownloadString("lists");
                    dynamic response = JObject.Parse(result);

                    Assert.AreEqual(0, (int)response.total_items);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + result);
                }
            }
        }

        [TestMethod]
        public void CreateAndUpdateNewList()
        {
            string json = String.Empty;
            dynamic payload = new JObject();

            using (var webClient = new WebClient())
            {
                webClient.BaseAddress = endPoint;
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                string readJson = String.Empty;

                try
                {
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(key));

                    webClient.Headers[HttpRequestHeader.Authorization] = "Basic " + key;


                    string start = System.IO.File.ReadAllText((@"../../../payloads/CreateNewList.json"));

                    IDictionary<string, string> map = new Dictionary<string, string>()
                    {
                        { "List_Name", "This is new list names" },
                        { "company_name", "CodeAndQA LLC" },
                        { "f_email_add", "aditya@codeandqa.com" }
                    };

                    var regex = new Regex(String.Join("|", map.Keys));
                    readJson = regex.Replace(start, m => map[m.Value]);


                    var result = webClient.UploadString("lists", "POST", readJson);
                    dynamic response = JObject.Parse(result);

                    Assert.AreEqual("This is new list names", (string)response.name);
                    var idOfNewList = (string)response.id;


                    result = webClient.UploadString("lists/" + idOfNewList , "PATCH", "{\"name\": \"New and Updated List\"}");
                    response = JObject.Parse(result);

                    Assert.AreEqual("New and Updated List", (string)response.name);


                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

        }

        [TestMethod]
        public void AddAndDeleteNewList()
        {
            string json = String.Empty;
            dynamic payload = new JObject();

            using (var webClient = new WebClient())
            {
                webClient.BaseAddress = endPoint;
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                string readJson = String.Empty;
                try
                {
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(key));

                    webClient.Headers[HttpRequestHeader.Authorization] = "Basic " + key;

                    readJson = System.IO.File.ReadAllText((@"../../../payloads/CreateNewList.json"));
                    IDictionary<string, string> map = new Dictionary<string, string>()
                    {
                        { "List_Name", "This is new list names" },
                        { "company_name", "CodeAndQA LLC" },
                        { "f_email_add", "aditya@codeandqa.com" }
                    };

                    var regex = new Regex(String.Join("|", map.Keys));
                    readJson = regex.Replace(readJson, m => map[m.Value]);

                    var result = webClient.UploadString("lists", "POST", readJson);
                    dynamic response = JObject.Parse(result);

                    var idOfNewList = (string)response.id;
                    result = webClient.UploadString("lists/"+idOfNewList, "DELETE", "");
                    Assert.AreEqual("", result);

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

        }


        [TestCleanup]
        public void CleanUpLists()
        {
            var result = "";
            using (var webClient = new WebClient())
            {
                webClient.BaseAddress = endPoint;
                try
                {
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(key));
                    webClient.Headers[HttpRequestHeader.Authorization] = "Basic " + key;
                    result = webClient.DownloadString("lists");
                    dynamic response = JObject.Parse(result);
                    var listOfIds = new List<string>();
                    foreach (var item in response.lists)
                    {
                        result = webClient.UploadString("lists/" + (string)item.id, "DELETE", "");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
