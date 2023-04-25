//ключ приховано
using System;
using Newtonsoft.Json;
using System.Text.Json;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using YouTube_API;
using System.Security.Policy;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace YouTube_API
{
    public class Default
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
    public class High
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
    public class Item
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public Snippet snippet { get; set; }

        [JsonConstructor]
        public Item(Snippet snippet)
        {
            this.kind = "youtube#searchResult";
            this.etag = "etag";
            this.id = "videoId";
            this.snippet = snippet;
        }
    }
    public class Maxres
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
    public class Medium
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
    public class PageInfo
    {
        public int totalResults { get; set; }
        public int resultsPerPage { get; set; }
    }
    public class ResourceId
    {
        public string kind { get; set; }
        public string videoId { get; set; }
        
        [JsonConstructor]
        public ResourceId(string videoId)
        {
            this.videoId = videoId;
        }
    }

    public class Root
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public List<Item> items { get; set; }
        public PageInfo pageInfo { get; set; }

        [JsonConstructor]
        public Root(DateTime publishedAt, string title, string videoId)
        {
            this.kind = "youtube#searchListResponse";
            this.etag = "etag";
            this.nextPageToken = "nextPageToken";
            this.items = new List<Item> { new Item(new Snippet(publishedAt, title, videoId)) };
        }
    }
    public class Snippet
    {
        public DateTime publishedAt { get; set; }
        public string channelId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnails thumbnails { get; set; }
        public string channelTitle { get; set; }
        public string playlistId { get; set; }
        public int position { get; set; }
        public ResourceId resourceId { get; set; }
        public string videoOwnerChannelTitle { get; set; }
        public string videoOwnerChannelId { get; set; }

        [JsonConstructor]

        public Snippet(DateTime publishedAt, string title, string videoId)
        {
            this.resourceId = new ResourceId(videoId);
            this.publishedAt = publishedAt;
            this.title = title;
        }
    }
    public class Standard
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
    public class Thumbnails
    {
        public Default @default { get; set; }
        public Medium medium { get; set; }
        public High high { get; set; }
        public Standard standard { get; set; }
        public Maxres maxres { get; set; }
    }

    public class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string url = "https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=PLSN6qXliOioz5lnckfofNcLJ3CnZJvEJO&key={YOUR_API_KEY}&maxResults=6";
        static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=youtube_data.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return sqlite_conn;
        }
        static void CreateTable(SQLiteConnection conn)
        {

            SQLiteCommand sqlite_cmd;
            string Createsql = "CREATE TABLE IF NOT EXISTS Videos (videoId VARCHAR(50) PRIMARY KEY ON CONFLICT REPLACE, title VARCHAR(250),publishedAt VARCHAR(50))";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
        }
        static void InsertData(SQLiteConnection conn, Root root)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            foreach (var video_data in root.items)
            {
                sqlite_cmd.CommandText = $"INSERT INTO Videos (videoId, title, publishedAt)  VALUES('{video_data.snippet.resourceId.videoId}', '{video_data.snippet.title}', '{video_data.snippet.publishedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}'); ";
                sqlite_cmd.ExecuteNonQuery();
            }        
        }
        public static void SaveToDB(Root root)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            CreateTable(sqlite_conn);
            InsertData(sqlite_conn, root);
        }
        static List<Root> ReadData(SQLiteConnection conn)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Videos";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            List<Root> list = new List<Root>();
            while (sqlite_datareader.Read())
            {
                string videoId = sqlite_datareader.GetString(0);
                string title = sqlite_datareader.GetString(1);
                DateTime publishedAt = DateTime.Parse(sqlite_datareader.GetString(2));
                Root root = new Root(publishedAt, title, videoId);
                list.Add(root);
                Console.WriteLine(publishedAt + " " + title + " " + videoId + " ");
            }
            conn.Close();
            return list;
        }
        public static void GetFromDB()
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            ReadData(sqlite_conn);
        }
        static void GetFromDBAndWriteToFile()
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Videos";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            //List<Root> items = ReadData(sqlite_conn);

            using (StreamWriter file = new StreamWriter("Videos.txt"))
            {
                while (sqlite_datareader.Read())
                {
                    string videoId = sqlite_datareader.GetString(0);
                    string title = sqlite_datareader.GetString(1);
                    DateTime publishedAt = DateTime.Parse(sqlite_datareader.GetString(2));

                    string line = $"{videoId} {title} {publishedAt}";

                    file.WriteLine(line);
                }
            }
            sqlite_conn.Close();
        }
        public static void GetFromDBandSend()
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            List<Root> items = ReadData(sqlite_conn);
            var json = JsonConvert.SerializeObject(items);
            sendVideos(json);
        }
        public static async void getVideos()
        {
            var responseString = await client.GetStringAsync(url);
            Console.WriteLine("Parsing JSON...");
            Root root = JsonConvert.DeserializeObject<Root>(responseString);
            //String response = "";
            //await Task.Run(() =>
            //{
            //    response = client.GetStringAsync(url).Result.ToString();
            //});
            Console.WriteLine(responseString);
            //Root root = JsonSerializer.Deserialize<Root>(response);
            SaveToDB(root);
        }
        public static async void sendVideos(string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
        }
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello, World!1");
            //getVideos();
            //GetFromDB();

            //GetFromDBAndWriteToFile();
            GetFromDBandSend();

            //Console.WriteLine("Hello, World!2");
            Console.ReadKey();
        }
    }
}

