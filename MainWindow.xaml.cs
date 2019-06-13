using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MongoCrud
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Contact> contacts;

        public MainWindow()
        {
            InitializeComponent();

            contacts = new List<Contact>();
            contactsList.ItemsSource = contacts;
            contactsList.SelectionChanged += ItemChanged;

            LoadContacts();
        }

        private void ItemChanged(object sender, EventArgs e)
        {
            if (contactsList.SelectedIndex != -1)
            {
                if (contacts[contactsList.SelectedIndex].Photo != null)
                {
                    BitmapImage image = new BitmapImage();
                    using (MemoryStream byresStream = new MemoryStream(contacts[contactsList.SelectedIndex].Photo.Bytes))
                    {
                        byresStream.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = null;
                        image.StreamSource = byresStream;
                        image.EndInit();
                    }
                    photoImage.Source = image;
                }
                else
                {
                    photoImage.Source = null;
                }
            }
        }

        public async void LoadContacts()
        {
            this.contacts.Clear();
            IMongoCollection<Contact> collection = App.db.GetCollection<Contact>("contacts");
            BsonDocument filter = new BsonDocument();
            using (IAsyncCursor<Contact> cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var contacts = cursor.Current;
                    foreach (Contact contact in contacts)
                    {
                        this.contacts.Add(contact);
                    }
                }
            }
            contactsList.Items.Refresh();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            CreateWindow createWindow = new CreateWindow(this, null);
            createWindow.ShowDialog();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (contactsList.SelectedIndex != -1)
            {
                CreateWindow createWindow = new CreateWindow(this, contacts[contactsList.SelectedIndex]);
                createWindow.ShowDialog();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (contactsList.SelectedIndex != -1) DeleteContact(contacts[contactsList.SelectedIndex]).GetAwaiter();
        }

        private async Task DeleteContact(Contact contact)
        {
            IMongoCollection<Contact> collection = App.db.GetCollection<Contact>("contacts");
            var filter = Builders<Contact>.Filter.Eq(s => s.Id, contact.Id);
            var result = await collection.DeleteOneAsync(filter);

            LoadContacts();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e) => FindContact().GetAwaiter();

        private async Task FindContact()
        {
            contactsList.SelectedIndex = -1;
            IMongoCollection<Contact> collection = App.db.GetCollection<Contact>("contacts");
            var filter = Builders<Contact>.Filter.Regex(s => s.Name, nameTextBox.Text);
            using (IAsyncCursor<Contact> cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var contacts = cursor.Current;
                    foreach (Contact contact in contacts)
                    {
                        foreach(Contact listContact in this.contacts)
                        {
                            if(listContact.Id == contact.Id)
                            {
                                int index = this.contacts.FindIndex(x => listContact.Equals(x));
                                contactsList.SelectedIndex = index;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void AgeCheckBox_Checked(object sender, RoutedEventArgs e) => AgeSort(true).GetAwaiter();

        private void AgeCheckBox_Unchecked(object sender, RoutedEventArgs e) => AgeSort(false).GetAwaiter();

        private async Task AgeSort(bool sort)
        {
            this.contacts.Clear();
            IMongoCollection<Contact> collection = App.db.GetCollection<Contact>("contacts");
            List<Contact> contacts;
            if (sort) contacts = await collection.Find(new BsonDocument()).Sort("{Age:1}").ToListAsync();
            else contacts = await collection.Find(new BsonDocument()).ToListAsync();
            foreach (Contact contact in contacts) this.contacts.Add(contact);
            contactsList.Items.Refresh();
        }
    }
}
