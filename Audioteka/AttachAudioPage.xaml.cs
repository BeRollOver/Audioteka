using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using VkData;
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
using Windows.Web.Http;
using Windows.Storage.Streams;
using Windows.Web;
using System.Net;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Audioteka
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class AttachAudioPage : Page
    {
        public ObservableCollection<Group> Groups;
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
            string request = $"https://api.vk.com/method/groups.get?user_id={App.auth.Id}&extended=1&v=5.50&access_token={App.auth.AccessToken}";
            groupListView.ItemsSource = Groups = VkRequest.GetResponse<VkDataResponse<Group>>(request).GetAwaiter().GetResult().GetItems as ObservableCollection<Group>;
            
            foreach (var item in MainPage.currPost.Attachments.Where(x => x.Type == "audio")) addSongToPanel(item.Audio); // список аудиозаписей поста на панель
            MainPage.currPost.Attachments = new ObservableCollection<Attachment>(MainPage.currPost.Attachments.Where(x => x.Type != "audio")); // очищаем предыдущий список аудиозаписей поста
            count = 10 - MainPage.currPost.Attachments.Count; // количество аудиозаписей, которых можно приложить к посту

            getAlbumList(MainPage.currGroup.Id); // загружаем список альбомов той группы, пост которой редактируется
        }

        void addSongToPanel(Audio audio)
        {
            // получаем список прикрепленных песен
            var attachedSongsList = attachedSongsPanel.Children.ToList().Select(x => x as Button).Select(x => x.Content as Audio);

            // если песня с таким же id уже в списке прикрепленных песен, то не добавляем её на панель
            if (!attachedSongsList.Select(x => x.Id).Contains(audio.Id))
            {
                // кнопки представляют собой прикрепляемые аудиозаписи
                // создаём новую кнопку
                Button button = new Button();
                button.Content = audio;
                button.Click += attchSongButton_Click;
                attachedSongsPanel.Children.Add(button);
            }
        }

        void getAlbumList(long id)
        {
            int offset = 0;
            string request = "https://api.vk.com/method/audio.getAlbums" +
                $"?owner_id=-{id}" +
                $"&offset={offset}" +
                $"&count=100&v=5.50&access_token={App.auth.AccessToken}";
            var response = VkRequest.GetResponse<VkDataResponse<Album>>(request).GetAwaiter().GetResult();
            albumsListView.ItemsSource = Albums = response.GetItems as ObservableCollection<Album>;
            offset = Albums.Count;

            while (response.Count != Albums.Count)
            {
                request = "https://api.vk.com/method/audio.getAlbums" +
                    $"?owner_id=-{id}" + 
                    $"&offset={offset}" +
                    $"&count=100&v=5.50&access_token={App.auth.AccessToken}";
                foreach (var item in VkRequest.GetResponse<VkDataResponse<Album>>(request).GetAwaiter().GetResult().GetItems) Albums.Add(item);
                offset = Albums.Count;
            }
        }

        void getSongList()
        {
            string request = "https://api.vk.com/method/audio.get" + 
                $"?owner_id={currAlbum.OwnerId}" +
                $"&album_id={currAlbum.Id}" +
                $"&v=5.50&access_token={App.auth.AccessToken}";
            var response = VkRequest.GetResponse<VkDataResponse<Audio>>(request).GetAwaiter().GetResult();
            songsListView.ItemsSource = Songs = response.GetItems as ObservableCollection<Audio>;
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
                $"&message={Uri.EscapeDataString(MainPage.currPost.Text)}" +
                $"&attachments={VkRequest.GetAttachesString(MainPage.currPost.Attachments)}" +
                $"&publish_date={MainPage.currPost.GetDate}" +
                $"&v=5.50&access_token={App.auth.AccessToken}";
            var result = VkRequest.GetResponse<VkResultResponse>(request).GetAwaiter().GetResult();
            if (result.Res == 1)
            {
                MessageDialog dialog = new MessageDialog("Success attached");
                await dialog.ShowAsync();
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Fail attached");
                await dialog.ShowAsync();
            }

            Frame.GoBack();
        }

        private void groupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            var group = list.SelectedItem as Group;
            getAlbumList(group.Id);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as Button;
            var song = item.DataContext as Audio;
            media.Source = new Uri(song.Url);
            media.Play();
        }
    }
}
