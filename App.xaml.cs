using System.Windows;
using MongoDB.Driver;

namespace MongoCrud
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IMongoDatabase db;
        App()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            db = client.GetDatabase("crud");
        }
    }
}
