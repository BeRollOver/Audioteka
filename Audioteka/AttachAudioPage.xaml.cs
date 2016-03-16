using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        List<Album> albumsList;
        Album currAlbum;
        int count = 10;

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

            count = 10 - MainPage.currPost.Attachments.Count; // количество аудиозаписей, которых можно приложить к посту
            
            // получение списка альбомов целиком
            int offset = 0;
            string request = "https://api.vk.com/method/audio.getAlbums?owner_id=-" +
                MainPage.currGroup.Id +
                "&offset=" + offset +
                "&count=100&v=5.50&access_token=" + accessToken;
            var albums = VkResponse.getResp<Data<Album>>(request);
            albumsList = albums.Response.Items;
            offset = albumsList.Count;

            while (albums.Response.Count != albumsList.Count)
            {
                request = "https://api.vk.com/method/audio.getAlbums?owner_id=-" +
                MainPage.currGroup.Id +
                "&offset=" + offset +
                "&count=100&v=5.50&access_token=" + accessToken;
                albumsList.AddRange(VkResponse.getResp<Data<Album>>(request).Response.Items);
                offset = albumsList.Count;
            }

            // передаем список альбомов нужным контейнерам
            albumsListView.ItemsSource = albumsList;
        }

        private void albumsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView list = (ListView)sender;
            if ((Album)list.SelectedItem != null)
                currAlbum = (Album)list.SelectedItem; // сохраняем  объект выбранного альбома
            else
                return;

            // получаем список песен в альбоме
            string request = "https://api.vk.com/method/audio.get?owner_id=-" + MainPage.currGroup.Id +
                "&album_id=" + currAlbum.Id + 
                "&v=5.50&access_token=" + accessToken;
            var songs = VkResponse.getResp<Data<Audio>>(request);
            foreach (var item in songs.Response.Items) item.Time = new TimeSpan(0, 0, item.Duration);
            currAlbum.Songs = songs.Response.Items;

            songsListView.ItemsSource = currAlbum.Songs; // заполняем список песен справа
            albumsSlitView.IsPaneOpen = true; // открываем панель справа
        }

        private void albumsSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // оставляем в списке альбомов только те, которые соответствуют результатам поиска
            var term = albumsSuggestBox.Text.ToLower();
            var results = albumsList.Select(x => x.Title).Where(i => i.ToLower().Contains(term)).ToList();
            albumsListView.ItemsSource = albumsList.Where(x => results.Contains(x.Title));
        }

        private void songsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (attachedSongsPanel.Children.Count != count)
            {
                var list = sender as ListView;
                var audio = e.ClickedItem as Audio;
                Button button = new Button();
                button.Content = audio;
                button.Click += Button_Click;
                var butList = attachedSongsPanel.Children.ToList().Select(x => x as Button);
                var songsList = butList.Select(x => x.Content as Audio);
                if (!songsList.Select(x => x.Id).Contains(audio.Id))
                {
                    attachedSongsPanel.Children.Add(button);
                }
            }
        }

        private void songsSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // оставляем в списке песен только те, которые соответствуют результатам поиска
            var term = songsSuggestBox.Text.ToLower();
            var results = currAlbum.Songs.Select(x => x.Title).Where(i => i.ToLower().Contains(term)).ToList();
            songsListView.ItemsSource = currAlbum.Songs.Where(x => results.Contains(x.Title));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            attachedSongsPanel.Children.Remove((Button)sender);
        }

        private void attchButton_Click(object sender, RoutedEventArgs e)
        {
            var butList = attachedSongsPanel.Children.ToList().Select(x => x as Button);
            var songsList = butList.Select(x => x.Content as Audio);
        }
    }
}
