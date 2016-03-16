using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        string accessToken;
        public static Group currGroup;
        public static Post currPost;

        public MainPage()
        {
            this.InitializeComponent();
            groupsSplitView.OpenPaneLength = Window.Current.Bounds.Width / 2;
            OAuthVk();
        }
        
        async void OAuthVk()
        {
            const string vkUri = "https://oauth.vk.com/authorize?client_id=4919033&scope=9999999&" +
                                    "redirect_uri=http://oauth.vk.com/blank.html&display=touch&response_type=token";
            Uri requestUri = new Uri(vkUri);
            Uri callbackUri = new Uri("http://oauth.vk.com/blank.html");

            WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(
                WebAuthenticationOptions.None, requestUri, callbackUri);

            switch (result.ResponseStatus)
            {
                case WebAuthenticationStatus.UserCancel:
                    break;
                case WebAuthenticationStatus.ErrorHttp:
                    MessageDialog dialogError = new MessageDialog("Не удалось открыть страницу сервиса\n" +
                "Попробуйте войти в приложение позже!", "Ошибка");
                    await dialogError.ShowAsync();
                    break;
                case WebAuthenticationStatus.Success:
                    string responseString = result.ResponseData;
                    char[] separators = { '=', '&' };
                    string[] responseContent = responseString.Split(separators);
                    accessToken = responseContent[1];
                    getGroupList();
                    break;
            }
        }

        void getGroupList()
        {
            string request = "https://api.vk.com/method/groups.get?user_id=130602270&filter=admin,moder,editor&extended=1&v=5.50&access_token=" + accessToken;
            var groups = VkResponse.getResp<Data<Group>>(request);
            groupListView.ItemsSource = groups.Response.Items;
        }

        private void groupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView list = (ListView)sender;
            if ((Group)list.SelectedItem != null)
                currGroup = (Group)list.SelectedItem;  // сохраняем объект выбранной группы
            else
                return;

            // получаем список отложенных записей группы
            string request = "https://api.vk.com/method/wall.get?owner_id=-" +
                 currGroup.Id +
                "&filter=postponed&v=5.50&access_token=" + accessToken;
            var wall = VkResponse.getResp<Data<Post>>(request);
            foreach (var item in wall.Response.Items) item.Time = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(item.Date); // переводим время публикации из unixtime в DateTime, наверно это может делать и newton, но не знаю как
            
            postsListView.ItemsSource = wall.Response.Items; // заполняем список песен справа
            groupsSplitView.IsPaneOpen = true; // открываем панель справа
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // отображаем при нажатии весь текст поста
            ListViewItem childs = (ListViewItem)postsListView.ItemsPanelRoot.Children[postsListView.SelectedIndex];
            Grid ch = (Grid)childs.ContentTemplateRoot;
            TextBlock bl = (TextBlock)ch.Children[2];
            bl.MaxLines = 0;
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        { 
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Attach_Audio_Button_Click(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            currPost = (Post)but.DataContext; // сохраняем объект выбранного поста

            Frame.Navigate(typeof(AttachAudioPage), accessToken); // вызываем окно выбора аудиозаписей
        }
    }
}
