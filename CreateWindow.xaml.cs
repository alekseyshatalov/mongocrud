using Microsoft.Win32;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MongoCrud
{
    /// <summary>
    /// Логика взаимодействия для CreateWindow.xaml
    /// </summary>
    public partial class CreateWindow : Window
    {
        private Contact contact;
        private bool isNew = false;
        private MainWindow mainWindow;
        public CreateWindow() => InitializeComponent();

        public CreateWindow(MainWindow mainWindow, Contact contact)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            if(contact == null)
            {
                isNew = true;
                this.contact = new Contact();
                this.contact.Created = new BsonDateTime(DateTime.Now);
            }
            else
            {
                this.contact = contact;
                InitWindow(contact);
            }
        }

        private void InitWindow(Contact contact)
        {
            nameTextBox.Text = contact.Name;
            ageTextBox.Text = contact.Age.ToString();
            if (contact.Phones != null) foreach (string phone in contact.Phones) phonesList.Items.Add(phone);
        }

        private void AddPhoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (contact.Phones == null) contact.Phones = new List<string>();
            contact.Phones.Add(phoneTextBox.Text);
            phonesList.Items.Add(phoneTextBox.Text);
            phoneTextBox.Text = "phone";
        }

        private void SetPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "")
            {
                byte[] bytes = File.ReadAllBytes(openFileDialog.FileName);
                contact.Photo = new BsonBinaryData(bytes);
            }
            else
            {
                contact.Photo = null;
            }
        }

        private void SaveContactButton_Click(object sender, RoutedEventArgs e)
        {
            contact.Name = nameTextBox.Text;
            try { contact.Age = int.Parse(ageTextBox.Text); }
            catch { contact.Age = 0; }

            if (isNew) SaveContact(contact).GetAwaiter();
            else UpdateContact(contact).GetAwaiter();

            mainWindow.LoadContacts();

            Close();
        }

        private async Task SaveContact(Contact contact)
        {
            IMongoCollection<Contact> collection = App.db.GetCollection<Contact>("contacts");
            await collection.InsertOneAsync(contact);
        }

        private async Task UpdateContact(Contact contact)
        {
            IMongoCollection<Contact> collection = App.db.GetCollection<Contact>("contacts");
            var filter = Builders<Contact>.Filter.Eq(s => s.Id, contact.Id);
            var result = await collection.ReplaceOneAsync(filter, contact);
        }
    }
}
