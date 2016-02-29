/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Gamma"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
//            SimpleIoc.Default.Register<ProductionTasksPMViewModel>();
            SimpleIoc.Default.Register<ProductionTasksSGBViewModel>();
            SimpleIoc.Default.Register<ReportListViewModel>();
            SimpleIoc.Default.Register<SourceSpoolsViewModel>();
            SimpleIoc.Default.Register<ManageUsersViewModel>();
            SimpleIoc.Default.Register<ProductionTasksConvertingViewModel>();
        }

        public static MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public static LoginViewModel Login
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LoginViewModel>();
            }
        }

        /*public static ProductionTasksPMViewModel ProductionTasksPM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProductionTasksPMViewModel>();
            }
        }
         * */
        public static ProductionTasksSGBViewModel ProductionTasksSGB
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProductionTasksSGBViewModel>();
            }
        }
        public static ProductionTasksConvertingViewModel ProductionTasksConverting
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProductionTasksConvertingViewModel>();
            }
        }
        public static ProductionTaskBatchViewModel ProductionTask
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProductionTaskBatchViewModel>();
            }
        }

        public static ReportListViewModel ReportList
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ReportListViewModel>();
            }
        }
        public static SourceSpoolsViewModel SourceSpools
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SourceSpoolsViewModel>();
            }
        }
        public static ManageUsersViewModel ManageUsers
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ManageUsersViewModel>();
            }
        }
       public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}