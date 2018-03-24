﻿using System;
using System.Net;
using System.Collections.Generic;

using HtmlAgilityPack;
using Newtonsoft.Json;

namespace SimpleBMSTable.TableInfo
{
    //http://bmsnormal2.syuriken.jp/bms_dtmanager.html

    //Stores an instance of a BMS difficulty table
    [JsonObject(MemberSerialization.Fields)]
    public class BMSTable
    {
        private string tableurl;
        private string headerurl;
        private string jsonurl;

        private TableHeader metadata;

        //Given a valid BMS table link, grab all needed links and download the json header
        public BMSTable(string url)
        {
            tableurl = url.Trim();
            
            //Get header url and download header data
            //Code is the same so just use this method to avoid code duplication
            UpdateTable();
        }

        public string TableName
        {
            get { return metadata.name; }
        }

        public string[] LevelOrder
        {
            get { return metadata.level_order; }
        }

        public string Symbol
        {
            get { return metadata.symbol; }
        }

        //Updates the table by getting the header URL and redownloading the header
        public void UpdateTable()
        {
            this.GetHeaderURL();
            this.DownloadHeader();
        }

        //Returns a list of charts from the difficulty table
        //Essentially deserializes the json downloaded from the jsonurl
        public List<TableEntry> GetCharts()
        {
            WebClient client = new WebClient() { Encoding = System.Text.Encoding.UTF8 };
            string json = client.DownloadString(jsonurl);
            return JsonConvert.DeserializeObject<List<TableEntry>>(json);
        }

        //Assumes the tableURL variable has already been set, and assigns the headerURL variable
        private void GetHeaderURL()
        {
            //Main table contains a reference meta tag pointing to a header json
            HtmlWeb htmlclient = new HtmlWeb();
            HtmlDocument table = htmlclient.Load(tableurl);

            string jsonheader = "";
            foreach (HtmlNode node in table.DocumentNode.SelectNodes("//meta"))
            {
                if (node.Attributes["name"] != null && node.Attributes["name"].Value == "bmstable")
                    jsonheader = node.Attributes["content"].Value;
            }

            if (jsonheader == "")
                throw new Exception("Error: table url is invalid");

            headerurl = ConstructURL(tableurl, jsonheader);
        }

        //Downloads the JSON header from the header URL
        //Assumes that headerurl is valid
        private void DownloadHeader()
        {
            WebClient client = new WebClient() { Encoding = System.Text.Encoding.UTF8 };
            string jsonheaderdata = client.DownloadString(headerurl);
            //Optional headers won't be filled if they don't exist
            metadata = JsonConvert.DeserializeObject<TableHeader>(jsonheaderdata);
            jsonurl = ConstructURL(tableurl, metadata.data_url);
        }

        //Some sites may only link the name of the json header/table json, so we must construct the url ourselves
        private string ConstructURL(string url, string newsection)
        {
            if(!newsection.StartsWith("http"))
            {
                //the +1 allows us to include the slash
                return url.Substring(0, url.LastIndexOf('/')+1) + newsection.Trim('/');
            }
            return newsection;
        }
    }
}