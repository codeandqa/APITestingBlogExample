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
        string key = "fea6a2547s811se37ef219fe4s40d2f7-zk-uk";
        WebClient webClient = null;

        /// <summary>
        /// Setups the web client.
        /// </summary>
        [TestInitialize]
        public void setupWebClient()
        {
            webClient = new WebClient();
            webClient.BaseAddress = endPoint;
            webClient.Headers[HttpRequestHeader.Authorization] = "Basic " + key;
            webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
        }

        /// <summary>
        /// Shows the list info.
        /// </summary>
        [TestMethod]
        public void ShowListInfo()
        {
            var result = "";
            using (webClient)
            {
                try
                {
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
        /// <summary>
        /// Creates the and update new list.
        /// </summary>
        [TestMethod]
        public void CreateAndUpdateNewList()
        {
            string json = String.Empty;
            dynamic payload = new JObject();

            using (webClient)
            {
                string readJson = String.Empty;
                string newListName = Guid.NewGuid().ToString();

                try
                {
                    string start = System.IO.File.ReadAllText((@"../../../payloads/CreateNewList.json"));

                    IDictionary<string, string> map = new Dictionary<string, string>()
                    {
                        { "List_Name", newListName },
                        { "company_name", "CodeAndQA LLC" },
                        { "f_email_add", "aditya@codeandqa.com" }
                    };

                    var regex = new Regex(String.Join("|", map.Keys));
                    readJson = regex.Replace(start, m => map[m.Value]);


                    var result = webClient.UploadString("lists", "POST", readJson);
                    dynamic response = JObject.Parse(result);

                    Assert.AreEqual(newListName, (string)response.name);

                    System.Threading.Thread.Sleep(10000);//Hard wait for review.

                    var idOfNewList = (string)response.id;


                    result = webClient.UploadString("lists/" + idOfNewList , "PATCH", "{\"name\": \"New and Updated List\"}");
                    response = JObject.Parse(result);

                    Assert.AreEqual("New and Updated List", (string)response.name);
                    System.Threading.Thread.Sleep(10000);//Hard wait for review.

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

        }

        /// <summary>
        /// Adds and delete new list.
        /// </summary>
        [TestMethod]
        public void AddAndDeleteNewList()
        {
            string json = String.Empty;
            dynamic payload = new JObject();

            using (webClient)
            {
                string readJson = String.Empty;
                string newListName = Guid.NewGuid().ToString();
                try
                {
                    readJson = System.IO.File.ReadAllText((@"../../../payloads/CreateNewList.json"));
                    IDictionary<string, string> map = new Dictionary<string, string>()
                    {
                        { "List_Name", newListName },
                        { "company_name", "CodeAndQA LLC" },
                        { "f_email_add", "aditya@codeandqa.com" }
                    };

                    var regex = new Regex(String.Join("|", map.Keys));
                    readJson = regex.Replace(readJson, m => map[m.Value]);

                    var result = webClient.UploadString("lists", "POST", readJson);
                    dynamic response = JObject.Parse(result);
                    System.Threading.Thread.Sleep(10000);//Hard wait for review.

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

        /// <summary>
        /// Delete all lists availabe in account.
        /// </summary>
        [TestCleanup]
        public void CleanUpLists()
        {
            var result = "";
            using (webClient)
            {
                try
                {
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
