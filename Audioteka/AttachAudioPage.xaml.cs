using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Audioteka
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class AttachAudioPage : Page
    {
        string accessToken;
        public ObservableCollection<Album> Albums;
        public ObservableCollection<Audio> Songs;
        public Album currAlbum;
        public int count = 10;

        public AttachAudioPage()
        {
            this.InitializeComponent();
            albumsSlitView.OpenPaneLength = Window.Current.Bounds.Width / 2;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                accessToken = e.Parameter.ToString();
            }

            foreach (var item in MainPage.currPost.Attachments.Where(x => x.Type == "audio")) addSongToPanel(item.Audio); // список аудиозаписей поста на панель
            MainPage.currPost.Attachments = new ObservableCollection<Attachment>(MainPage.currPost.Attachments.Where(x => x.Type != "audio")); // очищаем предыдущий список аудиозаписей поста
            count = 10 - MainPage.currPost.Attachments.Count; // количество аудиозаписей, которых можно приложить к посту

            getAlbumList();
        }

        void addSongToPanel(Audio audio)
        {
            Button button = new Button();
            button.Content = audio;
            button.Click += attchSongButton_Click;
            var butList = attachedSongsPanel.Children.ToList().Select(x => x as Button);
            var songsList = butList.Select(x => x.Content as Audio);
            if (!songsList.Select(x => x.Id).Contains(audio.Id))
            {
                attachedSongsPanel.Children.Add(button);
            }
        }

        void getAlbumList()
        {
            int offset = 0;
            string request = "https://api.vk.com/method/audio.getAlbums" +
                $"?owner_id=-{MainPage.currGroup.Id}" +
                $"&offset={offset}" +
                $"&count=100&v=5.50&access_token={accessToken}";
            var response = VkResponse.getResp<Data<Album>>(request);
            albumsListView.ItemsSource = Albums = VkResponse.getResp<Data<Album>>(request).Response.Items;
            offset = Albums.Count;

            while (response.Response.Count != Albums.Count)
            {
                request = "https://api.vk.com/method/audio.getAlbums" +
                    $"?owner_id=-{MainPage.currGroup.Id}" + 
                    $"&offset={offset}" +
                    $"&count=100&v=5.50&access_token={accessToken}";
                foreach (var item in VkResponse.getResp<Data<Album>>(request).Response.Items) Albums.Add(item);
                offset = Albums.Count;
            }
        }

        void getSongList()
        {
            string request = "https://api.vk.com/method/audio.get" + 
                $"?owner_id=-{MainPage.currGroup.Id}" +
                $"&album_id={currAlbum.Id}" +
                $"&v=5.50&access_token={accessToken}";
            var response = VkResponse.getResp<Data<Audio>>(request);
            foreach (var item in response.Response.Items) item.Time = new TimeSpan(0, 0, item.Duration);
            songsListView.ItemsSource = Songs = response.Response.Items;
        }

        private void albumsSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // оставляем в списке альбомов только те, которые соответствуют результатам поиска
            var term = albumsSuggestBox.Text.ToLower();
            var results = Albums.Select(x => x.Title).Where(i => i.ToLower().Contains(term)).ToList();
            albumsListView.ItemsSource = Albums.Where(x => results.Contains(x.Title));
        }

        private void songsSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // оставляем в списке песен только те, которые соответствуют результатам поиска
            var term = songsSuggestBox.Text.ToLower();
            var results = Songs.Select(x => x.Title).Where(i => i.ToLower().Contains(term)).ToList();
            songsListView.ItemsSource = Songs.Where(x => results.Contains(x.Title));
        }

        private void albumsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView list = (ListView)sender;
            if ((Album)list.SelectedItem != null)
                currAlbum = (Album)list.SelectedItem; // сохраняем  объект выбранного альбома
            else
                return;

            getSongList();
            albumsSlitView.IsPaneOpen = true; // открываем панель справа
        }

        private void songsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (attachedSongsPanel.Children.Count != count)
            {
                var list = sender as ListView;
                var audio = e.ClickedItem as Audio;
                addSongToPanel(audio);
            }
        }

        private void attchSongButton_Click(object sender, RoutedEventArgs e)
        {
            attachedSongsPanel.Children.Remove((Button)sender);
        }

        private async void attchButton_Click(object sender, RoutedEventArgs e)
        {
            // получаем список аудиозаписей для прикрепления
            var butList = attachedSongsPanel.Children.ToList().Select(x => x as Button);
            var songsList = butList.Select(x => x.Content as Audio);

            MainPage.currPost.Attachments = new ObservableCollection<Attachment>(songsList.Select(x => new Attachment { Type = "audio", Audio = x }).Concat(MainPage.currPost.Attachments)); // добавляем свой список аудиозаписей

            string request = "https://api.vk.com/method/wall.edit" + 
                $"?owner_id=-{MainPage.currGroup.Id}" +
                $"&post_id={MainPage.currPost.Id}" +
                $"&message={System.Net.WebUtility.UrlEncode(MainPage.currPost.Text)}" +
                $"&attachments={VkResponse.getAttachString(MainPage.currPost.Attachments)}" +
                $"&publish_date={MainPage.currPost.Date}" +
                $"&v=5.50&access_token={accessToken}";
            var result = VkResponse.getResp<Result>(request);

            if (result.Response == 1)
            {
                MessageDialog dialog = new MessageDialog("Success attached");
                await dialog.ShowAsync();
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Fail attached");
                await dialog.ShowAsync();
            }

            Frame.Navigate(typeof(MainPage), accessToken);
        }
    }
}
