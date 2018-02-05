using DependencyInjectionResolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Tools.Navigation.Events;
using UWP.Tools.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWP.Tools.Navigation {
    public class NavigationManager {
        private readonly DependencyInjection _dependencyInjection;

        private static Page _previousPage;
        private static Page _actualPage;
        private static Page _nextPage;

        private object _extraContent;
        private string _pageTitle;

        public NavigationManager(DependencyInjection dependencyInjection) {
            _dependencyInjection = dependencyInjection;
        }


        public void Navigate<T>(string pageTitle) where T : Page => Navigate<T>(pageTitle, null);

        public void Navigate<T>(string pageTitle, object extra) where T : Page {
            var page = default(T);
            try {
                if (GetRootFrame().Content != null && GetRootFrame().Content is Page pg) {
                    _previousPage = pg;
                }
                page = _dependencyInjection.Resolve<T>();
                NavigationEventHub.OnNavigating(_previousPage, new NavigationEventArgs(pageTitle, page, extra));
                _actualPage = page;
                GetRootFrame().Content = page;
                _extraContent = extra;
                _pageTitle = pageTitle;
                page.Loaded += Page_Loaded;
            }catch(Exception e) {
                NavigationEventHub.OnNavigationFailed(page, new NavigationFailedEventArgs(pageTitle, _previousPage, typeof(T), extra, e));
            }
        }

        public static bool CanGoBack() => _previousPage != null;

        public static bool GoBack() {
            var navigated = false;
            if (CanGoBack()) {
                if (GetRootFrame() is Frame mainFrame) {
                    _nextPage = _actualPage;
                    _actualPage = _previousPage;
                    _previousPage = null;
                    mainFrame.Content = _actualPage;
                    navigated = true;
                }
            }
            return navigated;
        }

        private static Frame GetRootFrame() {
            if(Window.Current.Content is Page mainPage && mainPage.Find<Frame>().FirstOrDefault() is Frame rootFrame) {
                return rootFrame;
            }
            return null;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            NavigationEventHub.OnNavigated(_previousPage, new NavigationEventArgs(_pageTitle, _previousPage, _actualPage, _extraContent));
        }
    }
}
