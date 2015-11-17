/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Merge.Desktop"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using Autofac;
using GalaSoft.MvvmLight.Ioc;
using Merge.Common;
using Merge.Common.Services;
using Microsoft.Practices.ServiceLocation;

namespace Merge.Desktop.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private readonly IContainer _mContainer;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var builder = new ContainerBuilder();

            builder.RegisterType<Merger>().SingleInstance();
            builder.RegisterType<FileManager>().SingleInstance();

            builder.RegisterType<MainViewModel>();

            _mContainer = builder.Build();
        }

        /// <summary>
        /// Main view model.
        /// </summary>
        public MainViewModel Main
        {
            get { return _mContainer.Resolve<MainViewModel>(); }
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}