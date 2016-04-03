using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VkData;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Audioteka
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Group> Groups;
        public ObservableCollection<Post> Posts;
        public static Group currGroup;
        public static Post currPost;

        public MainPage()
        {
            this.InitializeComponent();
            groupsSplitView.OpenPaneLength = Window.Current.Bounds.Width / 2;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                // вбросит исключение, если токен == null
                getGroupList(App.auth);
            }
            catch (Exception)
            {
                // получит токен и установит в качестве продолжения функцию getGroupList
                VKAuth.Auth(getGroupList);
            }
        }
        
        void getGroupList(VKAuth auth)
        {
            // сохраняем полученные токен и id
            if (auth.AccessToken == null) throw new Exception();
            App.auth = auth;

            // загружаем список групп, в которых пользователь админ
            string request = $"https://api.vk.com/method/groups.get?user_id={auth.Id}&filter=admin,moder,editor&extended=1&v=5.50&access_token={App.auth.AccessToken}";
            groupListView.ItemsSource = Groups = VkRequest.GetResponse<VkDataResponse<Group>>(request).GetAwaiter().GetResult().GetItems as ObservableCollection<Group>;
        }

        void getPostList()
        {
            string request = $"https://api.vk.com/method/wall.get?owner_id=-{currGroup.Id}&filter=postponed&v=5.50&access_token={App.auth.AccessToken}";
            var response = VkRequest.GetResponse<VkDataResponse<Post>>(request).GetAwaiter().GetResult();
            postsListView.ItemsSource = Posts = response.GetItems as ObservableCollection<Post>;
        }

        private void groupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // сохраняем объект выбранной группы
            var list = sender as ListView;
            if (list.SelectedItem is Group)
                currGroup = list.SelectedItem as Group;
            else
                return;

            getPostList(); // список постов в панель слева
            groupsSplitView.IsPaneOpen = true; // открываем панель слева
        }

        private void postsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            currPost = e.ClickedItem as Post; // сохраняем объект выбранного поста
            Frame.Navigate(typeof(AttachAudioPage)); // вызываем окно выбора аудиозаписей
        }
    }
}
