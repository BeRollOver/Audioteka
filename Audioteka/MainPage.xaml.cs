﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<Group> Groups;
        public ObservableCollection<Post> Posts;
        public static Group currGroup;
        public static Post currPost;

        public MainPage()
        {
            this.InitializeComponent();
            groupsSplitView.OpenPaneLength = Window.Current.Bounds.Width / 2;
            if(accessToken==null) OAuthVk();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                accessToken = e.Parameter.ToString();
            }
            try
            {
                getGroupList();
            }
            catch (Exception)
            {
                OAuthVk();
            }
        }

            async void OAuthVk()
        {
            try
            {
                Uri requestUri = new Uri("https://oauth.vk.com/authorize?client_id=4919033&scope=audio,wall,groups&redirect_uri=http://oauth.vk.com/blank.html&display=touch&response_type=token");
                Uri callbackUri = new Uri("http://oauth.vk.com/blank.html");

                WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, requestUri, callbackUri);

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
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageDialog dialogError = new MessageDialog("Ошибка входа");
                await dialogError.ShowAsync();
                throw;
            }
        }

        void getGroupList()
        {
            string request = $"https://api.vk.com/method/groups.get?user_id=130602270&filter=admin,moder,editor&extended=1&v=5.50&access_token={accessToken}";
            groupListView.ItemsSource = Groups = VkResponse.getResp<Data<Group>>(request).Response.Items;
        }

        void getPostList()
        {
            string request = $"https://api.vk.com/method/wall.get?owner_id=-{currGroup.Id}&filter=postponed&v=5.50&access_token={accessToken}";
            var response = VkResponse.getResp<Data<Post>>(request);
            foreach (var item in response.Response.Items) item.Time = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(item.Date).ToLocalTime(); // переводим время публикации из unixtime в DateTime, наверно это может делать и newton, но не знаю как
            postsListView.ItemsSource = Posts = response.Response.Items;
        }

        private void groupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // сохраняем объект выбранной группы
            var list = sender as ListView;
            if (list.SelectedItem is Group)
                currGroup = list.SelectedItem as Group;
            else
                return;

            getPostList();
            groupsSplitView.IsPaneOpen = true; // открываем панель справа
        }

        private void postsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            currPost = e.ClickedItem as Post; // сохраняем объект выбранного поста
            Frame.Navigate(typeof(AttachAudioPage), accessToken); // вызываем окно выбора аудиозаписей
        }
    }
}
